using System.Collections.Generic;

namespace SqlPrivilegeManager
{
    public interface IPrivilegesRepository
    {
        IEnumerable<IGrantedPrivilege> GetExistingPrivileges(IGrantee grantee);
        IEnumerable<string> GetAllGlobalPrivilegeNames();
        IEnumerable<string> GetAllDatabasePrivilegeNames();
        IEnumerable<string> GetAllTablePrivilegeNames();
    }
}
