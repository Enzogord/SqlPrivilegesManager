namespace SqlPrivilegeManager
{
    public class PrivilegeApplyResult
    {
        public IGrantee Grantee { get; }
        public IPrivilege Privilege { get; }
        public PrivilegeOperation Operation { get; }
        public bool Result { get; internal set; }
        public string ErrorMessage { get; internal set; }

        public PrivilegeApplyResult(IGrantee grantee, IPrivilege privilege, PrivilegeOperation operation, bool result, string errorMessage = "")
        {
            Grantee = grantee;
            Privilege = privilege;
            Operation = operation;
            Result = result;
            ErrorMessage = errorMessage;
        }
    }
}
