using System;

namespace SqlPrivilegeManager
{
    public sealed class PrivilegeAction : IPrivilegeAction
    {
        public IPrivilege Privilege { get; }
        public PrivilegeOperation Operation { get; }

        public PrivilegeAction(IPrivilege privilege, PrivilegeOperation operation)
        {
            Privilege = privilege ?? throw new ArgumentNullException(nameof(privilege));
            Operation = operation;
        }
    }
}
