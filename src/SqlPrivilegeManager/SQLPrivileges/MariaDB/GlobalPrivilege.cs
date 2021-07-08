using System;
using System.Linq;

namespace SqlPrivilegeManager.MariaDB
{
    public class GlobalPrivilege : IPrivilege
    {
        private readonly string _privilege;

        public GlobalPrivilege(GlobalPrivilegeType privilege)
        {
            PrivilegeName = privilege.GetPrivilegeName();
        }

        public GlobalPrivilege(string privilege)
        {
            var globalPrivileges = PrivilegeFunctions.GetAllGlobalPrivileges();
            if (!globalPrivileges.Contains(privilege))
            {
                throw new SqlPrivilegeException($"Privilege {privilege} is not one of {nameof(GlobalPrivilegeType)}");
            }
            PrivilegeName = privilege;
        }

        public string PrivilegeName { get; }

        public PrivilegeType PrivilegeType => PrivilegeType.Global;

        public string Database => "*";

        public string Table => "*";
    }
}
