using MySql.Data.MySqlClient;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace SqlPrivilegeManager.MariaDB.Tests
{
    [TestFixture()]
    public class PrivilegesControllerTests
    {
        private static MySqlException mysqlExceptionStub;

        static PrivilegesControllerTests()
        {
            ReceiveMysqlExceptionStub();
        }

        //Get MysqlException object for tests, because ctor is internal 
        //In static method because, executon during around 2 seconds 
        private static void ReceiveMysqlExceptionStub()
        {
            try
            {
                string a = null;
                string b = null;
                MySqlHelper.ExecuteScalar(a, b);
            }
            catch (Exception ex)
            {
                mysqlExceptionStub = ex as MySqlException;
            }
        }

        #region Constructor test

        [Test]
        public void ConstructorTest_When_sqlExecutor_is_null_throw_ArgumentNullException()
        {
            //arrange
            ISqlExecutor sqlExecutorMock = null;
            IPrivilegeQueryBuilder privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            IPrivilegesRepository privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();

            //act
            TestDelegate act = () => new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //assert
            Assert.Throws(typeof(ArgumentNullException), act);
        }

        [Test]
        public void ConstructorTest_When_privilegeQueryBuilder_is_null_throw_ArgumentNullException()
        {
            //arrange
            ISqlExecutor sqlExecutorMock = Substitute.For<ISqlExecutor>();
            IPrivilegeQueryBuilder privilegeQueryBuilderMock = null;
            IPrivilegesRepository privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();

            //act
            TestDelegate act = () => new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //assert
            Assert.Throws(typeof(ArgumentNullException), act);
        }

        [Test]
        public void ConstructorTest_When_privilegeRepository_is_null_throw_ArgumentNullException()
        {
            //arrange
            ISqlExecutor sqlExecutorMock = Substitute.For<ISqlExecutor>();
            IPrivilegeQueryBuilder privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            IPrivilegesRepository privilegeRepositoryMock = null;

            //act
            TestDelegate act = () => new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //assert
            Assert.Throws(typeof(ArgumentNullException), act);
        }

        #endregion Constructor test


        #region ApplyPrivilege(granteeMock) method

        [Test]
        public void ApplyPrivilegeTest_with_user_and_host_arguments_for_global_privilege_returns_good_result()
        {
            //arrange
            var privilege = "SELECT";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Global;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetGlobalPrivilegeQuery(operation, privilege, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            privilegeRepositoryMock.GetAllGlobalPrivilegeNames().Returns(new string[] { privilege });

            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            var result = privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);

            //assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Grantee.Name, Is.EqualTo(granteeMock.Name), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Name)}");
                Assert.That(result.Grantee.Host, Is.EqualTo(granteeMock.Host), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Host)}");
                Assert.That(result.ErrorMessage, Is.Empty, $"Свойство: {nameof(result.ErrorMessage)}");
                Assert.That(result.Privilege.Database, Is.Empty, $"Свойство: {nameof(result.Privilege.Database)}");
                Assert.That(result.Privilege.Table, Is.Empty, $"Свойство: {nameof(result.Privilege.Table)}");
                Assert.That(result.Privilege.PrivilegeName, Is.EqualTo(privilege), $"Свойство: {nameof(result.Privilege.PrivilegeName)}");
                Assert.That(result.Privilege.PrivilegeType, Is.EqualTo(privilegeType), $"Свойство: {nameof(result.Privilege.PrivilegeType)}");
                Assert.That(result.Operation, Is.EqualTo(operation), $"Свойство: {nameof(result.Operation)}");
                Assert.That(result.Result, Is.True, $"Свойство: {nameof(result.Result)}");
            });
        }

        [Test]
        public void ApplyPrivilegeTest_with_user_and_host_arguments_for_database_privilege_returns_good_result()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Database;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetDatabasePrivilegeQuery(operation, privilege, database, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            privilegeRepositoryMock.GetAllDatabasePrivilegeNames().Returns(new string[] { privilege });

            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            var result = privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);

            //assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Grantee.Name, Is.EqualTo(granteeMock.Name), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Name)}");
                Assert.That(result.Grantee.Host, Is.EqualTo(granteeMock.Host), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Host)}");
                Assert.That(result.ErrorMessage, Is.Empty, $"Свойство: {nameof(result.ErrorMessage)}");
                Assert.That(result.Privilege.Database, Is.EqualTo(database), $"Свойство: {nameof(result.Privilege.Database)}");
                Assert.That(result.Privilege.Table, Is.Empty, $"Свойство: {nameof(result.Privilege.Table)}");
                Assert.That(result.Privilege.PrivilegeName, Is.EqualTo(privilege), $"Свойство: {nameof(result.Privilege.PrivilegeName)}");
                Assert.That(result.Privilege.PrivilegeType, Is.EqualTo(privilegeType), $"Свойство: {nameof(result.Privilege.PrivilegeType)}");
                Assert.That(result.Operation, Is.EqualTo(operation), $"Свойство: {nameof(result.Operation)}");
                Assert.That(result.Result, Is.True, $"Свойство: {nameof(result.Result)}");
            });
        }

        [Test]
        public void ApplyPrivilegeTest_with_user_and_host_arguments_for_table_privilege_returns_good_result()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";
            var table = "test_table";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Table;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            privilegeRepositoryMock.GetAllTablePrivilegeNames().Returns(new string[] { privilege });

            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            var result = privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);

            //assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Grantee.Name, Is.EqualTo(granteeMock.Name), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Name)}");
                Assert.That(result.Grantee.Host, Is.EqualTo(granteeMock.Host), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Host)}");
                Assert.That(result.ErrorMessage, Is.Empty, $"Свойство: {nameof(result.ErrorMessage)}");
                Assert.That(result.Privilege.Database, Is.EqualTo(database), $"Свойство: {nameof(result.Privilege.Database)}");
                Assert.That(result.Privilege.Table, Is.EqualTo(table), $"Свойство: {nameof(result.Privilege.Table)}");
                Assert.That(result.Privilege.PrivilegeName, Is.EqualTo(privilege), $"Свойство: {nameof(result.Privilege.PrivilegeName)}");
                Assert.That(result.Privilege.PrivilegeType, Is.EqualTo(privilegeType), $"Свойство: {nameof(result.Privilege.PrivilegeType)}");
                Assert.That(result.Operation, Is.EqualTo(operation), $"Свойство: {nameof(result.Operation)}");
                Assert.That(result.Result, Is.True, $"Свойство: {nameof(result.Result)}");
            });
        }

        [Test]
        public void ApplyPrivilegeTest_with_user_and_host_arguments_dont_throw_mysqlException()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";
            var table = "test_table";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Table;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            privilegeRepositoryMock.GetAllTablePrivilegeNames().Returns(new string[] { privilege });

            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            var result = privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);

            //assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Grantee.Name, Is.EqualTo(granteeMock.Name), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Name)}");
                Assert.That(result.Grantee.Host, Is.EqualTo(granteeMock.Host), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Host)}");
                Assert.That(result.ErrorMessage, Is.EqualTo(mysqlExceptionStub.Message), $"Свойство: {nameof(result.ErrorMessage)}");
                Assert.That(result.Privilege.Database, Is.EqualTo(database), $"Свойство: {nameof(result.Privilege.Database)}");
                Assert.That(result.Privilege.Table, Is.EqualTo(table), $"Свойство: {nameof(result.Privilege.Table)}");
                Assert.That(result.Privilege.PrivilegeName, Is.EqualTo(privilege), $"Свойство: {nameof(result.Privilege.PrivilegeName)}");
                Assert.That(result.Privilege.PrivilegeType, Is.EqualTo(privilegeType), $"Свойство: {nameof(result.Privilege.PrivilegeType)}");
                Assert.That(result.Operation, Is.EqualTo(operation), $"Свойство: {nameof(result.Operation)}");
                Assert.That(result.Result, Is.False, $"Свойство: {nameof(result.Result)}");
            });
        }

        [Test]
        public void ApplyPrivilegeTest_with_user_and_host_arguments_when_user_is_null_must_throw_ArgumentException()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";
            var table = "test_table";

            IGrantee granteeMock = null;

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Table;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentNullException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_user_and_host_arguments_when_privilegeAction_is_null_must_throw_ArgumentNullException()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";
            var table = "test_table";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Table;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            IPrivilegeAction privilegeActionMock = null;

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(ArgumentNullException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_user_and_host_arguments_when_privilegeType_unknown_must_throw_NotSupportedException()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";
            var table = "test_table";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = (PrivilegeType)3;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(NotSupportedException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_user_and_host_arguments_when_global_privilege_unknown_must_throw_SqlPrivilegeException()
        {
            //arrange
            var privilege = "SOME_UNKNOWN_PRIVILEGE";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Global;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetGlobalPrivilegeQuery(operation, privilege, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(SqlPrivilegeException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_user_and_host_arguments_when_database_privilege_unknown_must_throw_SqlPrivilegeException()
        {
            //arrange
            var privilege = "SOME_UNKNOWN_PRIVILEGE";
            var database = "test_database";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Database;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetDatabasePrivilegeQuery(operation, privilege, database, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(SqlPrivilegeException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_user_and_host_arguments_when_table_privilege_unknown_must_throw_SqlPrivilegeException()
        {
            //arrange
            var privilege = "SOME_UNKNOWN_PRIVILEGE";
            var database = "test_database";
            var table = "test_table";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Table;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(SqlPrivilegeException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_user_and_host_arguments_when_privilege_in_privilegeAction_is_null_throw_ArgumentException()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";
            var table = "test_table";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;

            IPrivilege privilegeMock = null;

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_user_and_host_arguments_when_privilegeName_in_privilege_is_null_throw_ArgumentException()
        {
            //arrange
            string privilege = null;
            var database = "test_database";
            var table = "test_table";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Table;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_user_and_host_arguments_when_database_in_privilege_is_null_and_database_privilege_type_throw_ArgumentException()
        {
            //arrange
            var privilege = "SELECT";
            string database = null;

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Database;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetDatabasePrivilegeQuery(operation, privilege, database, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_user_and_host_arguments_when_table_in_privilege_is_null_and_table_privilege_type_throw_ArgumentException()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";
            string table = null;

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Table;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        #endregion ApplyPrivilege(granteeMock) method


        #region ApplyPrivilege(databaseUser) method

        [Test]
        public void ApplyPrivilegeTest_with_databaseUser_arguments_for_global_privilege_returns_good_result()
        {
            //arrange
            var privilege = "SELECT";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Global;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetGlobalPrivilegeQuery(operation, privilege, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            privilegeRepositoryMock.GetAllGlobalPrivilegeNames().Returns(new string[] { privilege });

            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            var result = privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);

            //assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Grantee.Name, Is.EqualTo(granteeMock.Name), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Name)}");
                Assert.That(result.Grantee.Host, Is.EqualTo(granteeMock.Host), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Host)}");
                Assert.That(result.ErrorMessage, Is.Empty, $"Свойство: {nameof(result.ErrorMessage)}");
                Assert.That(result.Privilege.Database, Is.Empty, $"Свойство: {nameof(result.Privilege.Database)}");
                Assert.That(result.Privilege.Table, Is.Empty, $"Свойство: {nameof(result.Privilege.Table)}");
                Assert.That(result.Privilege.PrivilegeName, Is.EqualTo(privilege), $"Свойство: {nameof(result.Privilege.PrivilegeName)}");
                Assert.That(result.Privilege.PrivilegeType, Is.EqualTo(privilegeType), $"Свойство: {nameof(result.Privilege.PrivilegeType)}");
                Assert.That(result.Operation, Is.EqualTo(operation), $"Свойство: {nameof(result.Operation)}");
                Assert.That(result.Result, Is.True, $"Свойство: {nameof(result.Result)}");
            });
        }

        [Test]
        public void ApplyPrivilegeTest_with_databaseUser_arguments_for_database_privilege_returns_good_result()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Database;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetDatabasePrivilegeQuery(operation, privilege, database, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            privilegeRepositoryMock.GetAllDatabasePrivilegeNames().Returns(new string[] { privilege });

            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            var result = privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);

            //assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Grantee.Name, Is.EqualTo(granteeMock.Name), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Name)}");
                Assert.That(result.Grantee.Host, Is.EqualTo(granteeMock.Host), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Host)}");
                Assert.That(result.ErrorMessage, Is.Empty, $"Свойство: {nameof(result.ErrorMessage)}");
                Assert.That(result.Privilege.Database, Is.EqualTo(database), $"Свойство: {nameof(result.Privilege.Database)}");
                Assert.That(result.Privilege.Table, Is.Empty, $"Свойство: {nameof(result.Privilege.Table)}");
                Assert.That(result.Privilege.PrivilegeName, Is.EqualTo(privilege), $"Свойство: {nameof(result.Privilege.PrivilegeName)}");
                Assert.That(result.Privilege.PrivilegeType, Is.EqualTo(privilegeType), $"Свойство: {nameof(result.Privilege.PrivilegeType)}");
                Assert.That(result.Operation, Is.EqualTo(operation), $"Свойство: {nameof(result.Operation)}");
                Assert.That(result.Result, Is.True, $"Свойство: {nameof(result.Result)}");
            });
        }

        [Test]
        public void ApplyPrivilegeTest_with_databaseUser_arguments_for_table_privilege_returns_good_result()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";
            var table = "test_table";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Table;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            privilegeRepositoryMock.GetAllTablePrivilegeNames().Returns(new string[] { privilege });

            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            var result = privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);

            //assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Grantee.Name, Is.EqualTo(granteeMock.Name), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Name)}");
                Assert.That(result.Grantee.Host, Is.EqualTo(granteeMock.Host), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Host)}");
                Assert.That(result.ErrorMessage, Is.Empty, $"Свойство: {nameof(result.ErrorMessage)}");
                Assert.That(result.Privilege.Database, Is.EqualTo(database), $"Свойство: {nameof(result.Privilege.Database)}");
                Assert.That(result.Privilege.Table, Is.EqualTo(table), $"Свойство: {nameof(result.Privilege.Table)}");
                Assert.That(result.Privilege.PrivilegeName, Is.EqualTo(privilege), $"Свойство: {nameof(result.Privilege.PrivilegeName)}");
                Assert.That(result.Privilege.PrivilegeType, Is.EqualTo(privilegeType), $"Свойство: {nameof(result.Privilege.PrivilegeType)}");
                Assert.That(result.Operation, Is.EqualTo(operation), $"Свойство: {nameof(result.Operation)}");
                Assert.That(result.Result, Is.True, $"Свойство: {nameof(result.Result)}");
            });
        }

        [Test]
        public void ApplyPrivilegeTest_with_databaseUser_arguments_dont_throw_mysqlException()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";
            var table = "test_table";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Table;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            privilegeRepositoryMock.GetAllTablePrivilegeNames().Returns(new string[] { privilege });


            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            var result = privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);

            //assert
            Assert.Multiple(() =>
            {
                Assert.That(result.Grantee.Name, Is.EqualTo(granteeMock.Name), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Name)}");
                Assert.That(result.Grantee.Host, Is.EqualTo(granteeMock.Host), $"Свойство: {nameof(result.Grantee)}.{nameof(result.Grantee.Host)}");
                Assert.That(result.ErrorMessage, Is.EqualTo(mysqlExceptionStub.Message), $"Свойство: {nameof(result.ErrorMessage)}");
                Assert.That(result.Privilege.Database, Is.EqualTo(database), $"Свойство: {nameof(result.Privilege.Database)}");
                Assert.That(result.Privilege.Table, Is.EqualTo(table), $"Свойство: {nameof(result.Privilege.Table)}");
                Assert.That(result.Privilege.PrivilegeName, Is.EqualTo(privilege), $"Свойство: {nameof(result.Privilege.PrivilegeName)}");
                Assert.That(result.Privilege.PrivilegeType, Is.EqualTo(privilegeType), $"Свойство: {nameof(result.Privilege.PrivilegeType)}");
                Assert.That(result.Operation, Is.EqualTo(operation), $"Свойство: {nameof(result.Operation)}");
                Assert.That(result.Result, Is.False, $"Свойство: {nameof(result.Result)}");
            });
        }

        [Test]
        public void ApplyPrivilegeTest_with_databaseUser_arguments_when_user_is_null_must_throw_ArgumentException()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";
            var table = "test_table";

            IGrantee granteeMock = null;

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Table;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);

            //assert
            Assert.Throws(typeof(ArgumentNullException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_databaseUser_arguments_when_privilegeAction_is_null_must_throw_ArgumentNullException()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";
            var table = "test_table";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Table;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            IPrivilegeAction privilegeActionMock = null;

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(ArgumentNullException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_databaseUser_arguments_when_privilegeType_unknown_must_throw_NotSupportedException()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";
            var table = "test_table";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = (PrivilegeType)3;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(NotSupportedException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_databaseUser_arguments_when_global_privilege_unknown_must_throw_SqlPrivilegeException()
        {
            //arrange
            var privilege = "SOME_UNKNOWN_PRIVILEGE";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Global;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetGlobalPrivilegeQuery(operation, privilege, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(SqlPrivilegeException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_databaseUser_arguments_when_database_privilege_unknown_must_throw_SqlPrivilegeException()
        {
            //arrange
            var privilege = "SOME_UNKNOWN_PRIVILEGE";
            var database = "test_database";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Database;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetDatabasePrivilegeQuery(operation, privilege, database, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(SqlPrivilegeException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_databaseUser_arguments_when_table_privilege_unknown_must_throw_SqlPrivilegeException()
        {
            //arrange
            var privilege = "SOME_UNKNOWN_PRIVILEGE";
            var database = "test_database";
            var table = "test_table";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Table;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(SqlPrivilegeException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_databaseUser_arguments_when_privilege_in_privilegeAction_is_null_throw_ArgumentException()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";
            var table = "test_table";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;

            IPrivilege privilegeMock = null;

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_databaseUser_arguments_when_privilegeName_in_privilege_is_null_throw_ArgumentException()
        {
            //arrange
            string privilege = null;
            var database = "test_database";
            var table = "test_table";

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Table;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_databaseUser_arguments_when_database_in_privilege_is_null_and_database_privilege_type_throw_ArgumentException()
        {
            //arrange
            var privilege = "SELECT";
            string database = null;

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Database;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetDatabasePrivilegeQuery(operation, privilege, database, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        [Test]
        public void ApplyPrivilegeTest_with_databaseUser_arguments_when_table_in_privilege_is_null_and_table_privilege_type_throw_ArgumentException()
        {
            //arrange
            var privilege = "SELECT";
            var database = "test_database";
            string table = null;

            var granteeMock = Substitute.For<IGrantee>();
            granteeMock.Name.Returns("test_user");
            granteeMock.Host.Returns("test_host");
            granteeMock.IsRole.Returns(false);

            var query = "Test query";
            var operation = PrivilegeOperation.Grant;
            var privilegeType = PrivilegeType.Table;

            var privilegeMock = Substitute.For<IPrivilege>();
            privilegeMock.PrivilegeName.Returns(privilege);
            privilegeMock.Database.Returns(database);
            privilegeMock.Table.Returns(table);
            privilegeMock.PrivilegeType.Returns(privilegeType);

            var privilegeActionMock = Substitute.For<IPrivilegeAction>();
            privilegeActionMock.Operation.Returns(operation);
            privilegeActionMock.Privilege.Returns(privilegeMock);

            var sqlExecutorMock = Substitute.For<ISqlExecutor>();
            sqlExecutorMock.When(x => x.Execute(query)).Throw(mysqlExceptionStub);
            sqlExecutorMock.When(x => x.Execute("")).Throw(new Exception("Query is empty"));

            var privilegeQueryBuilderMock = Substitute.For<IPrivilegeQueryBuilder>();
            privilegeQueryBuilderMock.GetTablePrivilegeQuery(operation, privilege, database, table, granteeMock).Returns(query);

            var privilegeRepositoryMock = Substitute.For<IPrivilegesRepository>();
            var userPrivilegeInfosMock = Substitute.For<IEnumerable<IGrantedPrivilege>>();
            privilegeRepositoryMock.GetExistingPrivileges(granteeMock).Returns(userPrivilegeInfosMock);

            var privilegeController = new PrivilegesController(sqlExecutorMock, privilegeQueryBuilderMock, privilegeRepositoryMock);

            //act
            TestDelegate act = () => privilegeController.ApplyPrivilege(privilegeActionMock, granteeMock);


            //assert
            Assert.Throws(typeof(ArgumentException), act);
        }

        #endregion ApplyPrivilege(databaseUser) method

    }
}