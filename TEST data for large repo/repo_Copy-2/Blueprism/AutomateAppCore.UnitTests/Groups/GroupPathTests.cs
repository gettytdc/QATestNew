using BluePrism.AutomateAppCore.Groups;
using BluePrism.BPCoreLib.Data;
using BluePrism.Server.Domain.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AutomateAppCore.UnitTests.Groups
{
    /// <summary>
    /// Tests various parts of the groups management
    /// </summary>
    [TestFixture]
    public class GroupPathTests
    {
        private GroupTree _tree;
        private Group _gp1, _gp2, _gp3, _gp1_1, _gp2_1, _gp2_2, _gp2_1_1;
        private ProcessGroupMember _p1, _p2, _p3, _p3_clone, _p_gp2_2;
        private ICollection<Group> _allGroups;
        private ICollection<ProcessGroupMember> _allProcessMembers;

        /// <summary>
        /// Builds the tree used in the group tests in this fixture
        /// </summary>
        [SetUp]
        public void BuildTreeAndMembers()
        {
            _tree = new GroupTree(GroupTreeType.Processes);
            _gp1 = new Group() { Id = Guid.NewGuid(), Name = "Group 1" };
            _gp2 = new Group() { Id = Guid.NewGuid(), Name = "Group 2" };
            _gp3 = new Group() { Id = Guid.NewGuid(), Name = "Group 3" };
            _gp1_1 = new Group() { Id = Guid.NewGuid(), Name = "Group 1.1" };
            _gp2_1 = new Group() { Id = Guid.NewGuid(), Name = "Group 2.1" };
            _gp2_2 = new Group() { Id = Guid.NewGuid(), Name = "Group 2.2" };
            _gp2_1_1 = new Group() { Id = Guid.NewGuid(), Name = "Group 2.1.1" };
            _p1 = new ProcessGroupMember(new DictionaryDataProvider(new Dictionary<string, object>() { { "id", Guid.NewGuid() }, { "name", "Process 1" }, { "description", "The first process" }, { "createddate", DateTime.Now }, { "createdby", "stuart" }, { "lastmodifieddate", DateTime.MinValue }, { "lastmodifiedby", null } }));
            _p2 = new ProcessGroupMember(new DictionaryDataProvider(new Dictionary<string, object>() { { "id", Guid.NewGuid() }, { "name", "Process 2" }, { "description", "The second process" }, { "createddate", DateTime.Now }, { "createdby", "stuart" }, { "lastmodifieddate", DateTime.MinValue }, { "lastmodifiedby", null } }));
            _p3 = new ProcessGroupMember(new DictionaryDataProvider(new Dictionary<string, object>() { { "id", Guid.NewGuid() }, { "name", "Process 3" }, { "description", "The third process" }, { "createddate", DateTime.Now }, { "createdby", "stuart" }, { "lastmodifieddate", DateTime.MinValue }, { "lastmodifiedby", null } }));
            _p3_clone = (ProcessGroupMember)_p3.CloneOrphaned();
            _p_gp2_2 = new ProcessGroupMember(new DictionaryDataProvider(new Dictionary<string, object>() { { "id", Guid.NewGuid() }, { "name", "Group 2.2" }, { "description", "The process with Group 2.2's name" }, { "createddate", DateTime.Now }, { "createdby", "stuart" }, { "lastmodifieddate", DateTime.MinValue }, { "lastmodifiedby", null } }));
            _allGroups = new List<Group>() { _gp1, _gp2, _gp3, _gp1_1, _gp2_1, _gp2_2, _gp2_1_1 };
            _allProcessMembers = new List<ProcessGroupMember>() { _p1, _p2, _p3, _p3_clone, _p_gp2_2 };
            {
                var withBlock = _tree.Root;
                withBlock.Add(_gp1);
                withBlock.Add(_gp2);
                withBlock.Add(_gp3);
                withBlock.Add(_p1);
            }

            {
                var withBlock1 = _gp1;
                withBlock1.Add(_gp1_1);
            }

            {
                var withBlock2 = _gp1_1;
                withBlock2.Add(_p2);
            }

            {
                var withBlock3 = _gp2;
                withBlock3.Add(_gp2_1);
                withBlock3.Add(_gp2_2);
                withBlock3.Add(_p_gp2_2);
            }

            {
                var withBlock4 = _gp2_1;
                withBlock4.Add(_gp2_1_1);
                withBlock4.Add(_p3);
            }

            {
                var withBlock5 = _gp2_1_1;
                withBlock5.Add(_p3_clone);
            }

            // So we have the structure:
            // [Processes]
            // + Group:   Group 1
            // | + Group:   Group 1.1
            // |   - Process: Process 2
            // |
            // + Group:   Group 2
            // | + Group:   Group 2.1
            // | | + Group:   Group 2.1.1
            // | | | - Process: Process 3
            // | | |
            // | | - Process: Process 3
            // | |
            // | + Group:   Group 2.2
            // |
            // | - Process: Group 2.2
            // |
            // + Group:   Group 3
            // |
            // - Process: Process 1

        }

        /// <summary>
        /// Destroys the variables used in this test fixture
        /// </summary>
        [TearDown]
        public void DestroyTree()
        {
            _tree = null;
            _gp1 = null;
            _gp2 = null;
            _gp3 = null;
            _gp1_1 = null;
            _gp2_1 = null;
            _gp2_2 = null;
            _gp2_1_1 = null;
            _p1 = null;
            _p2 = null;
            _p3 = null;
            _p3_clone = null;
            _p_gp2_2 = null;
            _allGroups.Clear();
            _allGroups = null;
            _allProcessMembers.Clear();
            _allProcessMembers = null;
        }

        [Test(Description = "Some path tests with blank paths / path elements")]
        public void TestBlanks()
        {
            IGroup r = _tree.Root;
            // Root: "/" or "" should return the group that was called on
            Assert.That(r.GetMemberAtPath("/"), Is.EqualTo(r));
            Assert.That(r.GetMemberAtPath(""), Is.EqualTo(r));
            Assert.That(r.GetMemberAtPath(new string[] { }), Is.EqualTo(r));
            Assert.That(r.GetMemberAtPath(new[] { "" }), Is.EqualTo(r));
            Assert.That(r.GetMemberAtPath(new[] { "", "" }), Is.EqualTo(r));
            Assert.That(r.GetMemberAtPath("/Group 1/"), Is.EqualTo(_gp1));
            // Blanks are not allowed after element 2
            Assert.That(() => r.GetMemberAtPath(new[] { "", "", "" }), Throws.InstanceOf<NoSuchElementException>());
            // Blanks are not allowed if first element is a name
            Assert.That(() => r.GetMemberAtPath(new[] { "Group 1//Group 1.1" }), Throws.InstanceOf<NoSuchElementException>());
        }

        /// <summary>
        /// Tests the round trip for a path - ensuring that the path of a member
        /// can be used to retrieve that member back from the tree.
        /// </summary>
        [Test(Description = "Tests the retrieval of group members from a tree by their path")]
        public void TestPathRoundTrip()
        {
            var paths = new Dictionary<IGroupMember, string>(new GroupMember.OwnedMemberComparer());
            foreach (var g in _allGroups)
                paths[g] = g.Path();
            foreach (var p in _allProcessMembers)
                paths[p] = p.Path();
            foreach (var pair in paths)
                Assert.That(_tree.Root.GetMemberAtPath(pair.Key.MemberType, pair.Value), Is.EqualTo(pair.Key));
        }

        [Test]
        public void TestPreferItemOverGroup()
        {
            Assert.That(_tree.Root.GetMemberAtPath("Group 2/Group 2.2").MemberType, Is.EqualTo(GroupMemberType.Process));
        }

        [Test]
        public void TestReadPathFromNonRootGroup()
        {
            Assert.That(_gp2.GetMemberAtPath("Group 2.1"), Is.EqualTo(_gp2_1));
            Assert.That(_gp2.GetMemberAtPath("/Group 2.1/Process 3"), Is.EqualTo(_p3));
            Assert.That(_gp2.GetMemberAtPath("Group 2.1/Group 2.1.1/Process 3"), Is.EqualTo(_p3_clone));

            // Check the cloned process, taking into account ownership
            var comp = new GroupMember.OwnedMemberComparer();
            Assert.That(comp.Equals(_p3, _gp2.GetMemberAtPath("Group 2.1/Process 3")), Is.True);
            Assert.That(comp.Equals(_p3_clone, _gp2.GetMemberAtPath("Group 2.1/Group 2.1.1/Process 3")), Is.True);

            // Double check that _p3 <> _p3_clone in what is returned from the method
            Assert.That(comp.Equals(_p3, _gp2.GetMemberAtPath("Group 2.1/Group 2.1.1/Process 3")), Is.False);
        }
    }
}
