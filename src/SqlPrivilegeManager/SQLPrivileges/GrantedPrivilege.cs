using System;

namespace SqlPrivilegeManager
{
    public class GrantedPrivilege : IGrantedPrivilege
    {
        public IGrantee Grantee { get; }
        public IPrivilege Privilege { get; }

        public GrantedPrivilege(IGrantee grantee, IPrivilege privilege)
        {
            Grantee = grantee ?? throw new ArgumentNullException(nameof(grantee));
            Privilege = privilege ?? throw new ArgumentNullException(nameof(privilege));
        }
    }
}