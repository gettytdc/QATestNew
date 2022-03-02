using BluePrism.AutomateAppCore.Auth;
using NUnit.Framework;
using System.Collections.Generic;

namespace AutomateAppCore.UnitTests.Roles
{
    [TestFixture]
    public class RoleSetTests
    {
        [Test(Description = "Tests that RoleSet.Equals() correctly checks for equality")]
        public void TestRoleSetEquals()
        {
            var roleMap = new Dictionary<string, Role>
            {
                { "Add User", new Role("Add User") },
                { "Retire User", new Role("Retire User") },
                { "Edit User", new Role("Edit User") },
                { "Reset Password", new Role("Reset Password") },
                { "Edit Role Permissions", new Role("Edit Role Permissions") },
                { "Edit Role AD Group", new Role("Edit Role AD Group") }
            };
            var rs1 = new RoleSet();
            var rs2 = new RoleSet();
            Assert.True(rs1.Equals(rs2));
            Assert.False(rs1.Equals(null));
            rs1.Add(roleMap["Add User"]);
            rs2.Add(roleMap["Add User"]);
            Assert.True(rs1.Equals(rs2));
            Assert.True(rs2.Equals(rs1));
            rs1.Add(roleMap["Retire User"]);
            Assert.False(rs1.Equals(rs2));
            Assert.False(rs2.Equals(rs1));
            var retireUserClonedRole = roleMap["Retire User"].CloneRole();
            retireUserClonedRole.Name = "Delete User";
            rs2.Add(retireUserClonedRole);
            Assert.False(rs1.Equals(rs2));
            Assert.False(rs2.Equals(rs1));
        }

        [Test(Description = "Tests the change report in a roleset to ensure that the changes are " + "accurately reported")]
        public void TestChangeReport()
        {
            var roleMap = new Dictionary<string, Role>
            {
                { "Add User", new Role("Add User") },
                { "Retire User", new Role("Retire User") },
                { "Edit User", new Role("Edit User") },
                { "Reset Password", new Role("Reset Password") },
                { "Edit Role Permissions", new Role("Edit Role Permissions") },
                { "Edit Role AD Group", new Role("Edit Role AD Group") }
            };
            var rs1 = new RoleSet();
            var rs2 = new RoleSet();
            rs1.Add(roleMap["Add User"]);
            rs2.Add(roleMap["Add User"]);
            rs1.Add(roleMap["Retire User"]);
            var report = rs2.GetChangeReport(rs1);
            Assert.That(report, Is.EqualTo("Added: 'Retire User'"));

            // Getting the change report shouldn't alter either role set
            Assert.False(rs1.Equals(rs2));
            Assert.False(rs2.Equals(rs1));

            // Run it again to be certain - should have the same results
            report = rs2.GetChangeReport(rs1);
            Assert.That(report, Is.EqualTo("Added: 'Retire User'"));

            // Clone the role so we have a role with the same ID, but change the name
            var retireUserClonedRole = roleMap["Retire User"].CloneRole();
            retireUserClonedRole.Name = "Delete User";
            rs2.Add(retireUserClonedRole);
            report = rs2.GetChangeReport(rs1);
            Assert.That(report, Is.EqualTo("Modified: 'Delete User':[Name:'Delete User'->'Retire User']"));
            rs2.Remove("Add User");
            report = rs2.GetChangeReport(rs1);
            Assert.That(report, Contains.Substring("Added: 'Add User'"));
            Assert.That(report, Contains.Substring("Modified: 'Delete User':[Name:'Delete User'->'Retire User']"));

            // And the converse...
            report = rs1.GetChangeReport(rs2);
            Assert.That(report, Contains.Substring("Deleted: 'Add User'"));
            Assert.That(report, Contains.Substring("Modified: 'Retire User':[Name:'Retire User'->'Delete User']"));
        }
    }
}
