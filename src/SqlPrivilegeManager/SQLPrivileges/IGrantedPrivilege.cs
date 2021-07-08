namespace SqlPrivilegeManager
{
    public interface IGrantedPrivilege
    {
        IGrantee Grantee { get; }
        IPrivilege Privilege { get; }
    }
}


