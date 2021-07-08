namespace SqlPrivilegeManager
{
    public interface IPrivilege
    {
        string PrivilegeName { get; }
        PrivilegeType PrivilegeType { get; }
        string Database { get; }
        string Table { get; }
    }
}
