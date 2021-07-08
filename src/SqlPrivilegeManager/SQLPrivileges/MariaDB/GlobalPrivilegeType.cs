namespace SqlPrivilegeManager.MariaDB
{
    public enum GlobalPrivilegeType
    {
        [Privilege(PrivilegeName = "USAGE")]
        Usage,

        [Privilege(PrivilegeName = "BINLOG ADMIN")]
        BinlogAdmin,

        [Privilege(PrivilegeName = "BINLOG MONITOR")]
        BinlogMonitor,

        [Privilege(PrivilegeName = "REPLICATION CLIENT")]
        ReplicationClient,

        [Privilege(PrivilegeName = "BINLOG REPLAY")]
        BinlogReplay,

        [Privilege(PrivilegeName = "SLAVE MONITOR")]
        SlaveMonitor,

        [Privilege(PrivilegeName = "REPLICA MONITOR")]
        ReplicaMonitor,

        [Privilege(PrivilegeName = "REPLICATION SLAVE")]
        ReplicationSlave,

        [Privilege(PrivilegeName = "REPLICATION REPLICA")]
        ReplicationReplica,

        [Privilege(PrivilegeName = "REPLICATION MASTER ADMIN")]
        ReplicationMasterAdmin,

        [Privilege(PrivilegeName = "REPLICATION SLAVE ADMIN")]
        ReplicationSlaveAdmin,

        [Privilege(PrivilegeName = "CONNECTION ADMIN")]
        ConnectionAdmin,

        [Privilege(PrivilegeName = "CREATE USER")]
        CreateUser,

        [Privilege(PrivilegeName = "FEDERATED ADMIN")]
        FederatedAdmin,

        [Privilege(PrivilegeName = "FILE")]
        File,

        [Privilege(PrivilegeName = "PROCESS")]
        Process,

        [Privilege(PrivilegeName = "READ_ONLY ADMIN")]
        ReadOnlyAdmin,

        [Privilege(PrivilegeName = "RELOAD")]
        Reload,

        [Privilege(PrivilegeName = "SET USER")]
        SetUser,

        [Privilege(PrivilegeName = "SHOW DATABASES")]
        ShowDatabases,

        [Privilege(PrivilegeName = "SHUTDOWN")]
        Shutdown,

        [Privilege(PrivilegeName = "SUPER")]
        Super,

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
        GrantOption,


        /// <summary>
        /// Not implemented in MariaDB, just inherited from Mysql
        /// </summary>
        [Privilege(PrivilegeName = "CREATE TABLESPACE")]
        CreateTablespace
    }
}
