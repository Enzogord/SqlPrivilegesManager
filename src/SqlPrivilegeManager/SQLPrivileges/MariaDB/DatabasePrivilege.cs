using System;
using System.Linq;

namespace SqlPrivilegeManager.MariaDB
{
    public class DatabasePrivilege : IPrivilege
    {
        private readonly string _privilege;

        public DatabasePrivilege(DatabasePrivilegeType privilege, string database)
        {
            PrivilegeName = privilege.GetPrivilegeName();
            Database = database;
        }

        public DatabasePrivilege(string privilege, string database)
        {
            var databasePrivileges = PrivilegeFunctions.GetAllDatabasePrivileges();
            if (!databasePrivileges.Contains(privilege))
            {
                throw new SqlPrivilegeException($"Privelege {privilege} is not one of {nameof(DatabasePrivilegeType)}");
            }
            PrivilegeName = privilege;
            Database = database;
        }

        public string PrivilegeName { get; }

        public PrivilegeType PrivilegeType => PrivilegeType.Database;

        public string Database { get; }

        public string Table => "*";
    }
}
