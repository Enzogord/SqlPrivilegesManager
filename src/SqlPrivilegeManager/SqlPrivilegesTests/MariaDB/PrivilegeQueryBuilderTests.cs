using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlPrivilegeManager.MariaDB.Tests
{
    [TestFixture()]
    public class PrivilegeQueryBuilderTests
    {
        #region Global privilege

        private static object[] GoodGlobalPrivilegeCases =
        {
            new object[] { PrivilegeOperation.Grant, "SELECT", "test_user", "test_host",  new string[] {
                "GRANT SELECT ON *.* TO 'test_user'@'test_host';",
                "GRANT SELECT ON *.* TO `test_user`@`test_host`;"
            } },
            new object[] { PrivilegeOperation.Grant, "GRANT OPTION", "test_user", "test_host",  new string[] {
                "GRANT GRANT OPTION ON *.* TO 'test_user'@'test_host';",
                "GRANT GRANT OPTION ON *.* TO `test_user`@`test_host`;"
            } },
            new object[] { PrivilegeOperation.Revoke, "GRANT OPTION", "test_user", "test_host",  new string[] {
                "REVOKE GRANT OPTION ON *.* FROM 'test_user'@'test_host';",
                "REVOKE GRANT OPTION ON *.* FROM `test_user`@`test_host`;"
            } },
            new object[] { PrivilegeOperation.Revoke, "SELECT", "test_user", "test_host",  new string[] {
                "REVOKE SELECT ON *.* FROM 'test_user'@'test_host';",
                "REVOKE SELECT ON *.* FROM `test_user`@`test_host`;"
            } }
        };

        [TestCaseSource(nameof(GoodGlobalPrivilegeCases))]
        public void GetGlobalPrivilegeQueryTest(PrivilegeOperation operation, string privilege, string user, string host, IEnumerable<string> acceptableResults)
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns(user);
            granteeMock.Host.Returns(host);
            granteeMock.IsRole.Returns(false);

            //act
            var query = queryBuilder.GetGlobalPrivilegeQuery(operation, privilege, granteeMock);
            //assert
            Assert.That(query, Is.AnyOf(acceptableResults.ToArray()));
        }

        [Test]
        public void Throw_ArgumentException_In_GetGlobalPrivilegeQuery_if_privilege_null()
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            PrivilegeOperation operation = PrivilegeOperation.Grant;
            string privilege = null;
            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            //act
            TestDelegate act = () => queryBuilder.GetGlobalPrivilegeQuery(operation, privilege, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void Throw_ArgumentException_In_GetGlobalPrivilegeQuery_if_privilege_whitespace()
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            PrivilegeOperation operation = PrivilegeOperation.Grant;
            string privilege = " ";
            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            //act
            TestDelegate act = () => queryBuilder.GetGlobalPrivilegeQuery(operation, privilege, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void Throw_ArgumentException_In_GetGlobalPrivilegeQuery_if_grantee_null()
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            PrivilegeOperation operation = PrivilegeOperation.Grant;
            string privilege = "SELECT";
            IGrantee grantee = null;

            //act
            TestDelegate act = () => queryBuilder.GetGlobalPrivilegeQuery(operation, privilege, grantee);

            //assert
            Assert.Throws(typeof(ArgumentNullException), act);
        }

        #endregion

        #region Database privileges

        private static object[] DatabasePrivilegeCases =
        {
            new object[] { PrivilegeOperation.Grant, "SELECT", "test_database", "test_user", "test_host",  new string[] {
                "GRANT SELECT ON `test_database`.* TO 'test_user'@'test_host';",
                "GRANT SELECT ON `test_database`.* TO `test_user`@`test_host`;"
            } },
            new object[] { PrivilegeOperation.Grant, "GRANT OPTION", "test_database", "test_user", "test_host",  new string[] {
                "GRANT GRANT OPTION ON `test_database`.* TO 'test_user'@'test_host';",
                "GRANT GRANT OPTION ON `test_database`.* TO `test_user`@`test_host`;"
            } },
            new object[] { PrivilegeOperation.Revoke, "GRANT OPTION", "test_database", "test_user", "test_host",  new string[] {
                "REVOKE GRANT OPTION ON `test_database`.* FROM 'test_user'@'test_host';",
                "REVOKE GRANT OPTION ON `test_database`.* FROM `test_user`@`test_host`;"
            } },
            new object[] { PrivilegeOperation.Revoke, "SELECT", "test_database", "test_user", "test_host",  new string[] {
                "REVOKE SELECT ON `test_database`.* FROM 'test_user'@'test_host';",
                "REVOKE SELECT ON `test_database`.* FROM `test_user`@`test_host`;"
            } }
        };

        [TestCaseSource(nameof(DatabasePrivilegeCases))]
        public void GetDatabasePrivilegeQueryTest(PrivilegeOperation operation, string privilege, string database, string user, string host, IEnumerable<string> acceptableResults)
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns(user);
            granteeMock.Host.Returns(host);
            granteeMock.IsRole.Returns(false);

            //act
            var query = queryBuilder.GetDatabasePrivilegeQuery(operation, privilege, database, granteeMock);

            //assert
            Assert.That(query, Is.AnyOf(acceptableResults.ToArray()));
        }

        [Test]
        public void Throw_ArgumentException_In_GetDatabasePrivilegeQuery_if_privilege_null()
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            PrivilegeOperation operation = PrivilegeOperation.Grant;
            string privilege = null;
            string database = "test_database";
            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            //act
            TestDelegate act = () => queryBuilder.GetDatabasePrivilegeQuery(operation, privilege, database, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void Throw_ArgumentException_In_GetDatabasePrivilegeQuery_if_privilege_whitespace()
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            PrivilegeOperation operation = PrivilegeOperation.Grant;
            string privilege = " ";
            string database = "test_database";
            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            //act
            TestDelegate act = () => queryBuilder.GetDatabasePrivilegeQuery(operation, privilege, database, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void Throw_ArgumentException_In_GetDatabasePrivilegeQuery_if_database_null()
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            PrivilegeOperation operation = PrivilegeOperation.Grant;
            string privilege = "SELECT";
            string database = null;
            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            //act
            TestDelegate act = () => queryBuilder.GetDatabasePrivilegeQuery(operation, privilege, database, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void Throw_ArgumentException_In_GetDatabasePrivilegeQuery_if_database_whitespace()
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            PrivilegeOperation operation = PrivilegeOperation.Grant;
            string privilege = "SELECT";
            string database = " ";
            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            //act
            TestDelegate act = () => queryBuilder.GetDatabasePrivilegeQuery(operation, privilege, database, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void Throw_ArgumentException_In_GetDatabasePrivilegeQuery_if_user_null()
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            PrivilegeOperation operation = PrivilegeOperation.Grant;
            string privilege = "SELECT";
            string database = "test_database";
            IGrantee granteeMock = null;

            //act
            TestDelegate act = () => queryBuilder.GetDatabasePrivilegeQuery(operation, privilege, database, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentNullException), act);
        }

        #endregion

        #region Table privileges

        private static object[] TablePrivilegeCases =
       {
            new object[] { PrivilegeOperation.Grant, "SELECT", "test_database", "test_table", "test_user", "test_host",  new string[] {
                "GRANT SELECT ON `test_database`.`test_table` TO 'test_user'@'test_host';",
                "GRANT SELECT ON `test_database`.`test_table` TO `test_user`@`test_host`;"
            } },
            new object[] { PrivilegeOperation.Grant, "GRANT OPTION", "test_database", "test_table", "test_user", "test_host",  new string[] {
                "GRANT GRANT OPTION ON `test_database`.`test_table` TO 'test_user'@'test_host';",
                "GRANT GRANT OPTION ON `test_database`.`test_table` TO `test_user`@`test_host`;"
            } },
            new object[] { PrivilegeOperation.Revoke, "GRANT OPTION", "test_database", "test_table", "test_user", "test_host",  new string[] {
                "REVOKE GRANT OPTION ON `test_database`.`test_table` FROM 'test_user'@'test_host';",
                "REVOKE GRANT OPTION ON `test_database`.`test_table` FROM `test_user`@`test_host`;"
            } },
            new object[] { PrivilegeOperation.Revoke, "SELECT", "test_database", "test_table", "test_user", "test_host",  new string[] {
                "REVOKE SELECT ON `test_database`.`test_table` FROM 'test_user'@'test_host';",
                "REVOKE SELECT ON `test_database`.`test_table` FROM `test_user`@`test_host`;"
            } }
        };

        [TestCaseSource(nameof(TablePrivilegeCases))]
        public void GetTablePrivilegeQueryTest(PrivilegeOperation operation, string privilege, string database, string table, string user, string host, IEnumerable<string> acceptableResults)
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns(user);
            granteeMock.Host.Returns(host);
            granteeMock.IsRole.Returns(false);

            //act
            var query = queryBuilder.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock);

            //assert
            Assert.That(query, Is.AnyOf(acceptableResults.ToArray()));
        }


        [Test]
        public void Throw_ArgumentException_In_GetTablePrivilegeQuery_if_privilege_null()
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            PrivilegeOperation operation = PrivilegeOperation.Grant;
            string privilege = null;
            string database = "test_database";
            string table = "test_table";
            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            //act
            TestDelegate act = () => queryBuilder.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void Throw_ArgumentException_In_GetTablePrivilegeQuery_if_privilege_whitespace()
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            PrivilegeOperation operation = PrivilegeOperation.Grant;
            string privilege = " ";
            string database = "test_database";
            string table = "test_table";
            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            //act
            TestDelegate act = () => queryBuilder.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void Throw_ArgumentException_In_GetTablePrivilegeQuery_if_database_null()
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            PrivilegeOperation operation = PrivilegeOperation.Grant;
            string privilege = "SELECT";
            string database = null;
            string table = "test_table";
            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            //act
            TestDelegate act = () => queryBuilder.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void Throw_ArgumentException_In_GetTablePrivilegeQuery_if_database_whitespace()
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            PrivilegeOperation operation = PrivilegeOperation.Grant;
            string privilege = "SELECT";
            string database = " ";
            string table = "test_table";
            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            //act
            TestDelegate act = () => queryBuilder.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void Throw_ArgumentException_In_GetTablePrivilegeQuery_if_table_null()
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            PrivilegeOperation operation = PrivilegeOperation.Grant;
            string privilege = "SELECT";
            string database = "test_database";
            string table = null;
            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            //act
            TestDelegate act = () => queryBuilder.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void Throw_ArgumentException_In_GetTablePrivilegeQuery_if_table_whitespace()
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            PrivilegeOperation operation = PrivilegeOperation.Grant;
            string privilege = "SELECT";
            string database = "test_database";
            string table = " ";
            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            //act
            TestDelegate act = () => queryBuilder.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void Throw_ArgumentException_In_GetTablePrivilegeQuery_if_user_null()
        {
            //arange
            var queryBuilder = new PrivilegeQueryBuilder();
            PrivilegeOperation operation = PrivilegeOperation.Grant;
            string privilege = "SELECT";
            string database = "test_database";
            string table = "test_table";
            IGrantee granteeMock = null;

            //act
            TestDelegate act = () => queryBuilder.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentNullException), act);
        }

        #endregion
    }
}