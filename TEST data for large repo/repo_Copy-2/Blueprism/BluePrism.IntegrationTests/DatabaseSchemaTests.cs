#if UNITTESTS

namespace BluePrism.IntegrationTests
{
    using System;
    using System.Data;
    using AutomateAppCore;
    using BluePrism.Common.Security;
    using Utilities.Functional;
    using NUnit.Framework;
    using UnitTesting;


    /// <summary>
    /// Contains tests for checking database create and upgrade scripts work when the user 
    /// creating the database does not have access to the dbo schema.
    /// </summary>
    /// <seealso cref="BluePrism.IntegrationTests.DatabaseTestBase" />
    [TestFixture]
    public class DatabaseSchemaTests : DatabaseTestBase
    {
        private readonly string _otherSqlSchema = $"testschema_{Guid.NewGuid().ToString().Replace("-", "")}";
        private readonly string _loginName = $"testuser_{Guid.NewGuid().ToString().Replace("-", "")}";
        private const string _password = "p4$$w0Rd";
        private const string _allSchemaPermissions = "ALTER, CONTROL, CREATE SEQUENCE, DELETE, EXECUTE, INSERT, REFERENCES, SELECT, TAKE OWNERSHIP, UPDATE, VIEW CHANGE TRACKING, VIEW DEFINITION";

        public override void Setup()
        {
            base.Setup();

            LegacyUnitTestHelper.SetupDependencyResolver();
                    
        }

        [Test]
        public void CreateAndAnnotateDatabase_LoggedInUserCannotAccessDboSchema_ShouldThrowNoExceptions()
        {
            CreateDatabase(0);
            CreateNewSchema();
            CreateServerLoginWithFullServerControl();
            CreateDbUserForLoginWithNewSchemaAsDefault();
            MoveDbObjectsFromDboToNewSchema();
            DenyDbUserAllPermissionsOnDboSchema();
            ReRunAllCreateAndUpgradeScriptsAsDbUser();
        }

        private void CreateNewSchema() => ExecuteQuery($"create schema {_otherSqlSchema};", _ => { }, DatabaseName);
        
        /// <remarks>
        /// Note that the login cannot be given the sysadmin role, otherwise the default schema 
        /// will be ignored for the associated user and will revert to the dbo schema
        /// </remarks>
        private void CreateServerLoginWithFullServerControl() => ExecuteQuery($"create login {_loginName} with password = '{_password}'; grant control server to {_loginName};", _ => { }, "master");

        private void CreateDbUserForLoginWithNewSchemaAsDefault() => ExecuteQuery($"create user {_loginName} with default_schema = {_otherSqlSchema};", _ => { }, DatabaseName);

        private void DenyDbUserAllPermissionsOnDboSchema() => ExecuteQuery($"deny {_allSchemaPermissions} on schema::dbo to {_loginName};", _ => { }, DatabaseName);

        private void ReRunAllCreateAndUpgradeScriptsAsDbUser()
        {

            var connectionForOtherUser = new clsDBConnectionSetting(
                        DatabaseName,
                        @"(LocalDB)\MSSQLLocalDB",
                        DatabaseName,
                        _loginName,
                        new SafeString(_password),
                        false);

            var installer = CreateInstaller(connectionForOtherUser);

            installer.CreateDatabase(
                null,
                false,
                false,
                0);

            installer.AnnotateDatabase();
        }

        private void MoveDbObjectsFromDboToNewSchema() =>
           ExecuteQuery(
               $"[sp_MoveDBObjectsFromDboToNewSchema]",
               command => command
                   .Tee((c)=> c.CommandType = CommandType.StoredProcedure)
                   .Tee(AddParameter("@newschema", _otherSqlSchema)));


        public override void TearDown()
        {
            base.TearDown();
            
            try { ExecuteQuery($"drop login {_loginName};", _ => { }, databaseName: "master"); }
            catch { /* ignored */ }

        }
    }
}

#endif