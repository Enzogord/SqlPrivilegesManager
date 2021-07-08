using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlPrivilegeManager.MariaDB
{
    public class PrivilegesController : IPrivilegesController
    {
		private readonly IEnumerable<string> _globalPrivileges;
		private readonly IEnumerable<string> _databasePrivileges;
		private readonly IEnumerable<string> _tablePrivileges;

        private readonly ISqlExecutor _sqlExecutor;
        private readonly IPrivilegeQueryBuilder _privilegeQueryBuilder;
		private readonly IPrivilegesRepository _privilegesRepository;

		public PrivilegesController(ISqlExecutor sqlExecutor, IPrivilegeQueryBuilder privilegeQueryBuilder, IPrivilegesRepository privilegesRepository)
        {
            _sqlExecutor = sqlExecutor ?? throw new ArgumentNullException(nameof(sqlExecutor));
            _privilegeQueryBuilder = privilegeQueryBuilder ?? throw new ArgumentNullException(nameof(privilegeQueryBuilder));
			_privilegesRepository = privilegesRepository ?? throw new ArgumentNullException(nameof(privilegesRepository));
			_globalPrivileges = privilegesRepository.GetAllGlobalPrivilegeNames();
			_databasePrivileges = privilegesRepository.GetAllDatabasePrivilegeNames();
			_tablePrivileges = privilegesRepository.GetAllTablePrivilegeNames();
		}

		#region IPrivilegesController implementation


		public PrivilegeApplyResult ApplyPrivilege(IPrivilegeAction privilegeAction, IGrantee grantee)
		{
			try
			{
				ExecuteApplyingPrivilege(privilegeAction, grantee);
				return new PrivilegeApplyResult(grantee, privilegeAction.Privilege, privilegeAction.Operation, true);
			}
			catch(MySqlException ex)
			{
				return new PrivilegeApplyResult(grantee, privilegeAction.Privilege, privilegeAction.Operation, false, ex.Message);
			}
		}

		public IEnumerable<PrivilegeApplyResult> ApplyPrivileges(IEnumerable<IPrivilegeAction> privilegeActions, IGrantee grantee, bool discardOnFail = true)
		{
			if(privilegeActions is null)
			{
				throw new ArgumentNullException(nameof(privilegeActions));
			}

            if(grantee is null)
            {
                throw new ArgumentNullException(nameof(grantee));
            }

            List<PrivilegeApplyResult> results = new List<PrivilegeApplyResult>();

			if(!privilegeActions.Any())
			{
				return results;
			}

			var exitingUserPrivileges = _privilegesRepository.GetExistingPrivileges(grantee);
			List<string> discardQueries = new List<string>();

			foreach(var privilegeAction in privilegeActions)
			{
				try
				{
					ExecuteApplyingPrivilege(privilegeAction, grantee);
					results.Add(new PrivilegeApplyResult(grantee, privilegeAction.Privilege, privilegeAction.Operation, true));

					if(discardOnFail)
					{
						var dicardQuery = CreateDiscardQuery(privilegeAction, grantee, exitingUserPrivileges);
						discardQueries.Add(dicardQuery);
					}
				}
				catch(Exception ex)
				{
					var discardSucceed = TryDiscardChanges(discardQueries, out Exception discardQueriesException);
					if(discardOnFail && !discardSucceed)
					{
						throw new AggregateException(discardQueriesException, ex);
					}
                    if(ex is MySqlException)
                    {
                        if(discardOnFail)
                        {
							foreach(var item in results)
							{
								item.Result = false;
								item.ErrorMessage = "Discarding changes when an exception is thrown when a another privilege is applied";
							}
						}
						
						results.Add(new PrivilegeApplyResult(grantee, privilegeAction.Privilege, privilegeAction.Operation, false, ex.Message));
						return results;
					}
				}
			}

			return results;
		}

		#endregion IPrivilegesController implementation

		private void ExecuteApplyingPrivilege(IPrivilegeAction privilegeAction, IGrantee grantee)
		{
			if(privilegeAction is null)
			{
				throw new ArgumentNullException(nameof(privilegeAction));
			}

            if(grantee is null)
            {
                throw new ArgumentNullException(nameof(grantee));
            }

			ValidatePrivilegeActionFields(privilegeAction);
			ValidatePrivilegeFields(privilegeAction.Privilege);

			string query = GetPrivilegeQuery(privilegeAction.Privilege, privilegeAction.Operation, grantee);

			_sqlExecutor.Execute(query);
		}

		private string GetPrivilegeQuery(IPrivilege privilege, PrivilegeOperation operation, IGrantee grantee)
		{
			string query;
			switch(privilege.PrivilegeType)
			{
				case PrivilegeType.Global:
					ValidateGlobalPrivilege(privilege.PrivilegeName);
					query = _privilegeQueryBuilder.GetGlobalPrivilegeQuery(operation, privilege.PrivilegeName, grantee);
					break;
				case PrivilegeType.Database:
					ValidateDatabasePrivilege(privilege.PrivilegeName);
					query = _privilegeQueryBuilder.GetDatabasePrivilegeQuery(operation, privilege.PrivilegeName, privilege.Database, grantee);
					break;
				case PrivilegeType.Table:
					ValidateTablePrivilege(privilege.PrivilegeName);
					query = _privilegeQueryBuilder.GetTablePrivilegeQuery(operation, privilege.PrivilegeName, privilege.Database, privilege.Table, grantee);
					break;
				default:
					throw new NotSupportedException($"Type {privilege.PrivilegeType} not supported");
			}

			return query;
		}

		private void ValidatePrivilegeFields(IPrivilege privilege)
		{
			if(string.IsNullOrWhiteSpace(privilege.PrivilegeName))
			{
				throw new ArgumentException($"Property {nameof(privilege.PrivilegeName)} in {nameof(privilege)} argument cannot be null or whitespace.");
			}

			if(privilege.PrivilegeType == PrivilegeType.Database && string.IsNullOrWhiteSpace(privilege.Database))
			{
				throw new ArgumentException($"Property {nameof(privilege.Database)} in {nameof(privilege)} argument (with {PrivilegeType.Database} privilege type) cannot be null or whitespace.");
			}

			if(privilege.PrivilegeType == PrivilegeType.Table && string.IsNullOrWhiteSpace(privilege.Table))
			{
				throw new ArgumentException($"Property {nameof(privilege.Table)} in {nameof(privilege)} argument (with {PrivilegeType.Table} privilege type) cannot be null or whitespace.");
			}
		}

		private void ValidatePrivilegeActionFields(IPrivilegeAction privilegeAction)
		{
			if (privilegeAction.Privilege == null)
			{
				throw new ArgumentException($"Property {nameof(privilegeAction.Privilege)} in {nameof(privilegeAction)} argument cannot be null");
			}
		}

		private void ValidateGlobalPrivilege(string privilege)
		{
			if(!_globalPrivileges.Contains(privilege))
			{
				throw new SqlPrivilegeException($"Global privilege \"{privilege}\" not supported in MariaDB ({Constants.SupportedMariaDBVersion})");
			}
		}

		private void ValidateDatabasePrivilege(string privilege)
		{
			if(!_databasePrivileges.Contains(privilege))
			{
				throw new SqlPrivilegeException($"Database privilege \"{privilege}\" not supported in MariaDB ({Constants.SupportedMariaDBVersion})");
			}
		}

		private void ValidateTablePrivilege(string privilege)
		{
			if(!_tablePrivileges.Contains(privilege))
			{
				throw new SqlPrivilegeException($"Table privilege \"{privilege}\" not supported in MariaDB ({Constants.SupportedMariaDBVersion})");
			}
		}

		private string CreateDiscardQuery(IPrivilegeAction privilegeAction, IGrantee grantee, IEnumerable<IGrantedPrivilege> exitingUserPrivileges)
		{
			var privilege = privilegeAction.Privilege;

			var hasPrivilege = exitingUserPrivileges
								.Where(x => x.Grantee.Name == grantee.Name)
								.Where(x => x.Grantee.Host == grantee.Host)
								.Where(x => x.Grantee.IsRole == grantee.IsRole)
								.Where(x => x.Privilege.PrivilegeName == privilege.PrivilegeName)
								.Where(x => x.Privilege.PrivilegeType == privilege.PrivilegeType)
								.Where(x => x.Privilege.Database == privilege.Database)
								.Where(x => x.Privilege.Table == privilege.Table)
								.Any();

			if(privilegeAction.Operation == PrivilegeOperation.Grant && !hasPrivilege)
			{
				return GetPrivilegeQuery(privilege, PrivilegeOperation.Revoke, grantee);
			}

			if(privilegeAction.Operation == PrivilegeOperation.Revoke && hasPrivilege)
			{
				return GetPrivilegeQuery(privilege, PrivilegeOperation.Grant, grantee);
			}

			throw new SqlPrivilegeException($"Cannot create discard query");
		}

		private bool TryDiscardChanges(IEnumerable<string> discardQueries, out Exception exception)
		{
			exception = null;
			Exception lastException = null;
			var attempts = 3;

			bool result = true;
			foreach(var discardQuery in discardQueries)
			{
				bool currentQuerySucceed = false;
				while(attempts > 0)
				{
					try
					{
						_sqlExecutor.Execute(discardQuery);
						currentQuerySucceed = true;
						break;
					}
					catch(Exception ex)
					{
						currentQuerySucceed |= false;
						lastException = ex;
						attempts--;
					}
				}
				result &= currentQuerySucceed;
			}
			if(!result)
			{
				exception = lastException;

			}
			return result;
		}
	}
}
