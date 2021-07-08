namespace SqlPrivilegeManager
{
    public interface IPrivilegeQueryBuilder
    {
        string GetGlobalPrivilegeQuery(PrivilegeOperation operation, string privilege, IGrantee grantee);
        string GetDatabasePrivilegeQuery(PrivilegeOperation operation, string privilege, string database, IGrantee grantee);
        string GetTablePrivilegeQuery(PrivilegeOperation operation, string privilege, string database, string table, IGrantee grantee);
    }
}
