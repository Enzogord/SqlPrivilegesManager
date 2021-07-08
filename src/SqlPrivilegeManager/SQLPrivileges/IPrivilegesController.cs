using System.Collections.Generic;

namespace SqlPrivilegeManager
{
    public interface IPrivilegesController
    {
        PrivilegeApplyResult ApplyPrivilege(IPrivilegeAction privilegeAction, IGrantee grantee);
        IEnumerable<PrivilegeApplyResult> ApplyPrivileges(IEnumerable<IPrivilegeAction> privilegeActions, IGrantee grantee, bool discardOnFail = true);
    }
}
