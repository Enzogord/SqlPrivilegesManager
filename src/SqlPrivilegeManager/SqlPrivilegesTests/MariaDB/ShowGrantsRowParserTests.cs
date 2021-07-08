using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace SqlPrivilegeManager.MariaDB.Tests
{
    [TestFixture()]
    public class ShowGrantsRowParserTests
    {
        private static object[] ParseOneRowCases =
        {
            new object[] { "Just USAGE privilege", 
                "GRANT USAGE ON *.* TO `Test`@`%` IDENTIFIED BY PASSWORD 'SOME_PASS_HASH'", 
                "Test", "%", "*", "*", 
                new string[] { "USAGE" } },

            new object[] { "Check extra spaces", 
                "GRANT     SELECT      ON      *.*      TO      `Test`@`%`      IDENTIFIED BY PASSWORD 'SOME_PASS_HASH'            WITH GRANT OPTION            ", 
                "Test", "%", "*", "*",
                new string[] { "SELECT", "GRANT OPTION" } },
            
            new object[] { "Check all privileges with grant option", 
                "GRANT ALL PRIVILEGES ON *.* TO `Test`@`%` IDENTIFIED BY PASSWORD 'SOME_PASS_HASH' WITH GRANT OPTION", 
                "Test", "%", "*", "*",
                new string[] { "GRANT OPTION", "SELECT", "INSERT", "UPDATE", "DELETE", "CREATE", "DROP", "RELOAD", "SHUTDOWN", "PROCESS", "FILE", "REFERENCES", "INDEX", "ALTER", "SHOW DATABASES", "SUPER", "CREATE TEMPORARY TABLES", "LOCK TABLES", "EXECUTE", "REPLICATION SLAVE", "BINLOG MONITOR", "CREATE VIEW", "SHOW VIEW", "CREATE ROUTINE", 
                    "ALTER ROUTINE", "CREATE USER", "EVENT", "TRIGGER", "CREATE TABLESPACE", "DELETE HISTORY", "SET USER", "FEDERATED ADMIN", "CONNECTION ADMIN", "READ_ONLY ADMIN", "REPLICATION SLAVE ADMIN", "REPLICATION MASTER ADMIN", "BINLOG ADMIN", "BINLOG REPLAY", "SLAVE MONITOR" } },
            
            new object[] { "Check all privileges without grant option", 
                "GRANT ALL PRIVILEGES ON *.* TO `Test`@`%` IDENTIFIED BY PASSWORD 'SOME_PASS_HASH'", 
                "Test", "%", "*", "*",
                new string[] { "SELECT", "INSERT", "UPDATE", "DELETE", "CREATE", "DROP", "RELOAD", "SHUTDOWN", "PROCESS", "FILE", "REFERENCES", "INDEX", "ALTER", "SHOW DATABASES", "SUPER", "CREATE TEMPORARY TABLES", "LOCK TABLES", "EXECUTE", "REPLICATION SLAVE", "BINLOG MONITOR", "CREATE VIEW", "SHOW VIEW", "CREATE ROUTINE",
                    "ALTER ROUTINE", "CREATE USER", "EVENT", "TRIGGER", "CREATE TABLESPACE", "DELETE HISTORY", "SET USER", "FEDERATED ADMIN", "CONNECTION ADMIN", "READ_ONLY ADMIN", "REPLICATION SLAVE ADMIN", "REPLICATION MASTER ADMIN", "BINLOG ADMIN", "BINLOG REPLAY", "SLAVE MONITOR" } },
            
            new object[] { "Check all privileges without SELECT", 
                "GRANT INSERT, UPDATE, DELETE, CREATE, DROP, RELOAD, SHUTDOWN, PROCESS, FILE, REFERENCES, INDEX, ALTER, SHOW DATABASES, SUPER, CREATE TEMPORARY TABLES, LOCK TABLES, EXECUTE, REPLICATION SLAVE, BINLOG MONITOR, CREATE VIEW, SHOW VIEW, CREATE ROUTINE, ALTER ROUTINE, CREATE USER, EVENT, TRIGGER, CREATE TABLESPACE, DELETE HISTORY, SET USER, FEDERATED ADMIN, CONNECTION ADMIN, READ_ONLY ADMIN, REPLICATION SLAVE ADMIN, REPLICATION MASTER ADMIN, BINLOG ADMIN, BINLOG REPLAY, SLAVE MONITOR ON *.* TO `Test`@`%` IDENTIFIED BY PASSWORD 'SOME_PASS_HASH'", 
                "Test", "%", "*", "*",
                new string[] { "INSERT", "UPDATE", "DELETE", "CREATE", "DROP", "RELOAD", "SHUTDOWN", "PROCESS", "FILE", "REFERENCES", "INDEX", "ALTER", "SHOW DATABASES", "SUPER", "CREATE TEMPORARY TABLES", "LOCK TABLES", "EXECUTE", "REPLICATION SLAVE", "BINLOG MONITOR", "CREATE VIEW", "SHOW VIEW", "CREATE ROUTINE",
                    "ALTER ROUTINE", "CREATE USER", "EVENT", "TRIGGER", "CREATE TABLESPACE", "DELETE HISTORY", "SET USER", "FEDERATED ADMIN", "CONNECTION ADMIN", "READ_ONLY ADMIN", "REPLICATION SLAVE ADMIN", "REPLICATION MASTER ADMIN", "BINLOG ADMIN", "BINLOG REPLAY", "SLAVE MONITOR" } },
            
            new object[] { "Check parsing block \"TO\" with ` braces", 
                "GRANT SELECT ON *.* TO `Test`@`%` some_incorrect_symbols", 
                "Test", "%", "*", "*",
                new string[] { "SELECT" } },
            
            new object[] { "Check parsing block \"TO\" with ' braces", 
                "GRANT SELECT ON *.* TO 'Test'@'%' some_incorrect_symbols", 
                "Test", "%", "*", "*",
                new string[] { "SELECT" } },

            new object[] { "Check parsing block \"TO\" with ` braces and without end space",
                "GRANT SELECT ON *.* TO `Test`@`%`some_incorrect_symbols",
                "Test", "%", "*", "*",
                new string[] { "SELECT" } },

            new object[] { "Check parsing block \"TO\" with ' braces and without end space",
                "GRANT SELECT ON *.* TO 'Test'@'%'some_incorrect_symbols",
                "Test", "%", "*", "*",
                new string[] { "SELECT" } },
            
            new object[] { "Check parsing GRANT OPTION with spaces", 
                "GRANT SELECT ON *.* TO 'Test'@'%' some_incorrect_symbols WITH GRANT OPTION some_incorrect_symbols", 
                "Test", "%", "*", "*",
                new string[] { "SELECT", "GRANT OPTION" } },
            
            new object[] { "Check parsing GRANT OPTION without spaces", 
                "GRANT SELECT ON *.* TO 'Test'@'%' some_incorrect_symbolsWITH GRANT OPTIONsome_incorrect_symbols", 
                "Test", "%", "*", "*",
                new string[] { "SELECT", "GRANT OPTION" } },
            
            new object[] { "Check parsing with concrete database without table", 
                "GRANT SELECT ON `test_database`.* TO 'Test'@'%'", 
                "Test", "%", "test_database", "*",
                new string[] { "SELECT" } },

            new object[] { "Check parsing with concrete database and table",
                "GRANT SELECT ON `test_database`.`test_table` TO 'Test'@'%'",
                "Test", "%", "test_database", "test_table",
                new string[] { "SELECT" } }
        };

        [TestCaseSource(nameof(ParseOneRowCases))]
        public void ParseOneRowTest(string description, string inputRow, string expectedToUser, string expectedToHost, string expectedOnDatabase, string expectedOnTable, IEnumerable<string> expectedGrants) 
        {
            //arrange
            ShowGrantsRowParser parser = new ShowGrantsRowParser();

            //act
            var results = parser.ParseOneRow(inputRow);

            //assert
            Assert.Multiple(() =>
            {
                var users = results.Select(x => x.Grantee.Name);
                var hosts = results.Select(x => x.Grantee.Host);
                var databases = results.Select(x => x.Privilege.Database);
                var tables = results.Select(x => x.Privilege.Table);
                var privileges = results.Select(x => x.Privilege.PrivilegeName).ToList();

                Assert.That(users, Is.All.EqualTo(expectedToUser), $"{description}. User: ");
                Assert.That(hosts, Is.All.EqualTo(expectedToHost), $"{description}. Host: ");
                Assert.That(databases, Is.All.EqualTo(expectedOnDatabase), $"{description}. Database: ");
                Assert.That(tables, Is.All.EqualTo(expectedOnTable), $"{description}. Table: ");

                foreach (var grant in expectedGrants)
                {
                    Assert.Contains(grant, privileges, $"{description}. Check that expected grant contains in parsed privileges:");
                }
            });
        }

        private static object[] IncorrectParseOneRowCases =
        {
            new object[] { null },
            new object[] { "" },
            new object[] { "safhasfhasfhasfh" },
            new object[] { "GRANTSELECT ON *.* TO `Test`@`%`" },
            new object[] { "GRANT SELECTON *.* TO `Test`@`%`" },
            new object[] { "GRANT SELECT ON*.* TO `Test`@`%`" },
            new object[] { "GRANT SELECT *.* TO `Test`@`%`" },
            new object[] { "GRANT SELECT ON *.*TO `Test`@`%`" },
            new object[] { "GRANT SELECT ON *.* TO`Test`@`%`" },
            new object[] { "GRANT SELECT ON *.* `Test`@`%`" },
            new object[] { "GRANT SELECT ON *.* TO " },
            new object[] { "GRANT SELECT ON *.* TO Test@%" },
            new object[] { "GRANT SELECT ON *.* TO `Test`@`%" },
            new object[] { "GRANT SELECT ON test_database.test_table TO `Test`@`%`" },
            new object[] { "GRANT SELECT ON .test_table TO `Test`@`%`" },
            new object[] { "GRANT SELECT ON `test_database`. TO `Test`@`%`" },
            new object[] { "GRANT SELECT ON `test_database`.`test_table` TO @`%`" },
            new object[] { "GRANT SELECT ON `test_database`.`test_table` TO `Test`@" },
            new object[] { "GRANT SELECT ON `test_database`.`test_table` TO `Test``%`" }
        };

        [TestCaseSource(nameof(IncorrectParseOneRowCases))]
        public void ParseOneRowTest_with_incorrect_input_grants_string(string inputRow)
        {
            //arrange
            ShowGrantsRowParser parser = new ShowGrantsRowParser();

            //act
            TestDelegate act = () => parser.ParseOneRow(inputRow);

            //assert
            Assert.Throws(typeof(SqlPrivilegeException), act);
        }
    }
}