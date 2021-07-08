using System;
using System.Linq;

namespace SqlPrivilegeManager.MariaDB
{
    public class TablePrivilege : IPrivilege
    {
        private readonly string _privilege;

        public TablePrivilege(TablePrivilegeType privilege, string database, string table)
        {
            PrivilegeName = privilege.GetPrivilegeName();
            Database = database;
            Table = table;
        }

        public TablePrivilege(string privilege, string database, string table)
        {
            var tablePrivileges = PrivilegeFunctions.GetAllTablePrivileges();
            if (!tablePrivileges.Contains(privilege))
            {
                throw new SqlPrivilegeException($"Privilege {privilege} is not one of {nameof(TablePrivilegeType)}");
            }
            PrivilegeName = privilege;
            Database = database;
            Table = table;
        }

        public string PrivilegeName { get; }

        public PrivilegeType PrivilegeType => PrivilegeType.Table;

        public string Database { get; }

        public string Table { get; }
    }
}
