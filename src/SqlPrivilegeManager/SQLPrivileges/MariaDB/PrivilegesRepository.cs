using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlPrivilegeManager.MariaDB
{
    public class PrivilegesRepository : IPrivilegesRepository
    {
        private readonly ISqlExecutor _sqlExecutor;
        private readonly IShowGrantsRowParser _showGrantsRowParser;

        public PrivilegesRepository(ISqlExecutor sqlExecutor, IShowGrantsRowParser showGrantsRowParser)
        {
            _sqlExecutor = sqlExecutor ?? throw new ArgumentNullException(nameof(sqlExecutor));
            _showGrantsRowParser = showGrantsRowParser ?? throw new ArgumentNullException(nameof(showGrantsRowParser));
        }

        public IEnumerable<IGrantedPrivilege> GetExistingPrivileges(IGrantee grantee)
        {
            if(grantee is null)
            {
                throw new ArgumentNullException(nameof(grantee));
            }

            IEnumerable<string> grantsQueries = new List<string>();
            string sql = $"SHOW GRANTS FOR {GetGranteeQueryPart(grantee)};";

            grantsQueries = _sqlExecutor.ExecuteAndGetResult(sql);

            List<IGrantedPrivilege> results = new List<IGrantedPrivilege>();
            if (!grantsQueries.Any())
            {
                return results;
            }

            foreach (var grantsQuery in grantsQueries)
            {
                var parseResults = _showGrantsRowParser.ParseOneRow(grantsQuery);
                results.AddRange(parseResults);
            }

            return results;
        }

        public IEnumerable<string> GetAllGlobalPrivilegeNames()
        {
            return PrivilegeFunctions.GetAllGlobalPrivileges();
        }

        public IEnumerable<string> GetAllDatabasePrivilegeNames()
        {
            return PrivilegeFunctions.GetAllDatabasePrivileges();
        }

        public IEnumerable<string> GetAllTablePrivilegeNames()
        {
            return PrivilegeFunctions.GetAllTablePrivileges();
        }

        private string GetGranteeQueryPart(IGrantee grantee)
        {
            if(grantee.IsRole)
            {
                return $"`{grantee.Name}`";
            }
            else
            {
                return $"`{grantee.Name}`@`{grantee.Host}`";
            }
        }
    }
}
