namespace SqlPrivilegeManager.MariaDB
{
    public enum DatabasePrivilegeType
    {
        [Privilege(PrivilegeName = "CREATE")]
        Create,

        [Privilege(PrivilegeName = "CREATE TEMPORARY TABLES")]
        CreateTemporaryTables,

        [Privilege(PrivilegeName = "ALTER")]
        Alter,

        [Privilege(PrivilegeName = "DROP")]
        Drop,

        [Privilege(PrivilegeName = "CREATE ROUTINE")]
        CreateRoutine,

        [Privilege(PrivilegeName = "ALTER ROUTINE")]
        AlterRoutine,

        [Privilege(PrivilegeName = "EXECUTE")]
        Execute,

        [Privilege(PrivilegeName = "EVENT")]
        Event,

        [Privilege(PrivilegeName = "LOCK TABLES")]
        LockTables,

        [Privilege(PrivilegeName = "CREATE VIEW")]
        CreateView,

        [Privilege(PrivilegeName = "SHOW VIEW")]
        ShowView,

        [Privilege(PrivilegeName = "TRIGGER")]
        Trigger,

        [Privilege(PrivilegeName = "INDEX")]
        Index,

        [Privilege(PrivilegeName = "REFERENCES")]
        References,

        [Privilege(PrivilegeName = "SELECT")]
        Select,

        [Privilege(PrivilegeName = "INSERT")]
        Insert,

        [Privilege(PrivilegeName = "UPDATE")]
        Update,

        [Privilege(PrivilegeName = "DELETE")]
        Delete,

        [Privilege(PrivilegeName = "DELETE HISTORY")]
        DeleteHistory,

        [Privilege(PrivilegeName = "GRANT OPTION")]
        GrantOption
    }
}
