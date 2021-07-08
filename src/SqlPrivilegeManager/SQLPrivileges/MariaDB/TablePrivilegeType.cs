namespace SqlPrivilegeManager.MariaDB
{
    public enum TablePrivilegeType
    {
        [Privilege(PrivilegeName = "CREATE")]
        Create,

        [Privilege(PrivilegeName = "ALTER")]
        Alter,

        [Privilege(PrivilegeName = "DROP")]
        Drop,

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
