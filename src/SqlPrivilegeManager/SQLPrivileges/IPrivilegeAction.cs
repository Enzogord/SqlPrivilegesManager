namespace SqlPrivilegeManager
{
    public interface IPrivilegeAction
    {
        PrivilegeOperation Operation { get; }
        IPrivilege Privilege { get; }
    }
}