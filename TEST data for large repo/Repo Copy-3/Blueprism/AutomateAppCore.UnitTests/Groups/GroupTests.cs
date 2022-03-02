#if UNITTESTS
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.Groups;
using BluePrism.AutomateAppCore;
using BluePrism.BPCoreLib.Collections;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System;
using BluePrism.Core.Resources;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting.TestSupport;

namespace AutomateAppCore.UnitTests.Groups
{
    [TestFixture]
    public class GroupTests
    {
        private GroupTree ProcessesTree { get; set; }
        private TreeRoot RootGroup { get; set; }
        
        [SetUp]
        public void Setup()
        {
            ProcessesTree = new GroupTree(GroupTreeType.Processes);
            RootGroup = new TreeRoot(ProcessesTree);
            var mockGroupStore = new Mock<IGroupStore>();
            mockGroupStore.Setup(gs => gs.Update(It.IsAny<IGroup>()));
            ProcessesTree.Store = mockGroupStore.Object;
        }

        private void SetupMockServer(IServer serverMock)
        {
            var serverManagerMock = new Mock<ServerManager>();
            serverManagerMock.SetupGet(m => m.Server).Returns(serverMock);
            var serverFactoryMock = new Mock<BluePrism.AutomateAppCore.ClientServerConnection.IServerFactory>();
            serverFactoryMock.SetupGet(m => m.ServerManager).Returns(serverManagerMock.Object);
            ReflectionHelper.SetPrivateField(typeof(app), "ServerFactory", null, serverFactoryMock.Object);
        }

        [Test]
        public void GivenGroupNameHasBeenAdded_WhenNoGroupExistsInSameTreeType_ThenGroupShouldBeAdded()
        {
            //Arrange
            var group = new Group();
            RootGroup.Add(group);

            var groupName = "test group 1";
            group.CreateGroup(groupName);
            var testGroupName = "test group 2";

            //Act
            var testGroup = group.CreateGroup(testGroupName);

            //Assert 
            testGroup.Should().NotBeNull();
        }

        [Test]
        public void GivenGroupNameHasBeenAdded_WhenGroupExistsInSameTreeTypeAsRootName_ThenExceptionShouldBeThrown()
        {
            //Arrange
            var group = new Group();
            RootGroup.Add(group);
            var testGroupName = "Processes";

            //Act
            Action act = () => group.CreateGroup(testGroupName);

            //Assert 
            act.ShouldThrow<AlreadyExistsException>();
        }

        [Test]
        public void GivenGroupNameHasBeenAdded_WhenGroupExistsInSameTreeType_ThenExceptionShouldBeThrown()
        {
            //Arrange
            var groupName = "test group 1";
            var group = new Group { Name = groupName };
            RootGroup.Add(group);

            var testGroup = new Group { Name = "test group 2" };
            RootGroup.Add(testGroup);

            //Act
            Action act = () => RootGroup.CreateGroup(groupName);

            //Assert 
            act.ShouldThrow<AlreadyExistsException>();
        }

        [Test]
        public void GivenGroupNameHasBeenRenamed_WhenGroupDoesNotExistInSameTreeType_ThenGroupShouldBeRenamed()
        {
            //Arrange
            var group = new Group();
            RootGroup.Add(group);

            var testGroupName = "test group 1";
            var groupName = "test group 2";
            var testRenameForGroup = "TestGroup5";
            group.CreateGroup(testGroupName);
            group.CreateGroup(groupName);

            //Act
            Action act = () => group.UpdateGroupName(testRenameForGroup);

            //Assert
            act.ShouldNotThrow<AlreadyExistsException>();
        }


        [Test]
        public void GivenGroupNameHasBeenRenamed_WhenGroupExistsInSameTreeType_ThenExceptionShouldBeThrown()
        {
            //Arrange
            var groupName = "test group 1";
            var group = new Group { Name = groupName };
            RootGroup.Add(group);

            var testGroup = new Group { Name = "test group 2" };
            RootGroup.Add(testGroup);

            //Act
            Action act = () => testGroup.UpdateGroupName(groupName);

            //Assert
            act.ShouldThrow<AlreadyExistsException>();
        }

        [Test]
        public void GivenGroupNameHasBeenRenamed_WhenGroupExistsInSameTreeTypeAsRootName_ThenExceptionShouldBeThrown()
        {
            //Arrange
            var testGroupName = "Processes";
            var group = new Group { Name = "test group 1" };
            RootGroup.Add(group);

            var testGroup = new Group { Name = "test group 2" };
            RootGroup.Add(testGroup);

            //Act
            Action act = () => testGroup.UpdateGroupName(testGroupName);

            //Assert
            act.ShouldThrow<AlreadyExistsException>();
        }

        //Converted from AutomateAppCore/_UnitTests/GroupTests
        [Test]
        public void Group_CopyMember_CopyGroupWithSubgroup()
        {

            // Set up source and target groups, and group member to copy.

            // - rootGroup
            // | - sourceGroup
            // |      - groupToCopy
            // |          - subGroupToCopy
            // |
            // | - targetGroup

            // Id of the group to copy..
            var groupToCopyId = Guid.NewGuid();

            // Id of the subgroup to copy.
            var subGroupToCopyId = Guid.NewGuid();

            var tree = new GroupTree(GroupTreeType.Processes);
            var rootGroup = new TreeRoot(tree);
            var sourceGroup = new Group()
            {
                Name = "Source Group"
            };
            var targetGroup = new Group()
            {
                Name = "Target Group"
            };

            rootGroup.Add(sourceGroup);
            rootGroup.Add(targetGroup);

            var groupToCopy = new Group()
            {
                Name = "Group to copy",
                Id = groupToCopyId
            };
            var subGroupToCopy = new Group()
            {
                Name = "Subgroup to copy",
                Id = subGroupToCopyId
            };
            groupToCopy.Add(subGroupToCopy);

            // The id the copy of the groups will have.
            var cloneGroupToCopyId = Guid.NewGuid();
            var cloneSubgroupToCopyId = Guid.NewGuid();

            var cloneGroupToCopy = new Group()
            {
                Name = groupToCopy.Name,
                Id = cloneGroupToCopyId
            };
            var cloneSubGroupToCopy = new Group()
            {
                Name = subGroupToCopy.Name,
                Id = cloneSubgroupToCopyId
            };
            cloneGroupToCopy.Add(cloneSubGroupToCopy);

            var iServer = new Mock<IServer>();
            iServer.Setup(x => x.MoveGroupEntry(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<GroupMember>(),
                It.IsAny<bool>()
                )).Returns(cloneGroupToCopy);
            iServer.Setup(x => x.GetEffectiveGroupPermissions(
                It.IsAny<Guid>()
                )).Returns(new GroupPermissions(PermissionState.UnRestricted));
            iServer.Setup(x => x.GetEffectiveMemberPermissions(
                It.IsAny<IGroupMember>()
                )).Returns(new MemberPermissions(null));

            SetupMockServer(iServer.Object);

            var groupStore = new DatabaseGroupStore(iServer.Object);

            sourceGroup.Add(groupToCopy);
            tree.Store = groupStore;

            // Copy group from source group to target group.
            sourceGroup.CopyMember(groupToCopy, targetGroup);

            // Check that the source folder still contains the copied group
            Assert.IsTrue(sourceGroup.Any(x => x.Name == "Group to copy" && x.IdAsGuid() == groupToCopyId));
            Assert.IsTrue(sourceGroup.ContainsInSubtree(x => x.Name == "Subgroup to copy" && x.IdAsGuid() == subGroupToCopyId));

            // Check that the target folder contains a copy of the groups (with the new IDs)
            Assert.IsTrue(targetGroup.Any(x => x.Name == "Group to copy" && x.IdAsGuid() == cloneGroupToCopyId));
            Assert.IsTrue(targetGroup.ContainsInSubtree(x => x.Name == "Subgroup to copy" && x.IdAsGuid() == cloneSubgroupToCopyId));
        }

        [Test]
        public void Group_ResourceGroupMember_CloneOrphaned()
        {
            var poolControllerId = Guid.NewGuid();
            var poolMemberId = Guid.NewGuid();
            var tree = new GroupTree(GroupTreeType.Resources);
            var rootGroup = tree.Root;
            var poolController = new ResourceGroupMember()
            {
                Name = "Controller1",
                Id = poolControllerId,
                Attributes = ResourceAttribute.Pool
            };
            var poolMember = new ResourceGroupMember()
            {
                Name = "Resource1",
                Id = poolMemberId
            };
            rootGroup.Add(poolController);
            rootGroup.Add(poolMember);
            poolController.SetPoolMembers(new List<Guid>()
                { poolMemberId});

            var copy = poolController.CloneOrphaned() as ResourceGroupMember;

            Assert.That(copy?.Name, Is.EqualTo(poolController.Name));
            Assert.AreNotSame(copy?.PoolMembers.First(), poolMember);
        }

        /// <summary>
        /// Deleting a group which contains no items should succeed.
        /// </summary>
        [Test]
        public void Group_Delete_DeleteEmptyTopLevelGroup_Delete_DeleteGroupWithoutManageAccessRights_Exception_Group_Success()
        {
            var mockServer = new Mock<IServer>(MockBehavior.Strict);
            mockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            Permission.Init(mockServer.Object);

            var tree = new GroupTree(GroupTreeType.Processes);
            var rootGroup = new TreeRoot(tree);
            var group = new Group();
            rootGroup.Add(group);

            group.Permissions = SetupMockMemberPermissions(tree);
            rootGroup.Add(group);

            group.Delete();

            Assert.IsFalse(rootGroup.Contains(group));
        }

        /// <summary>
        /// Test that deleting a group which contains items throws an exception.
        /// </summary>
        [Test]
        public void Group_Delete_DeleteTopLevelGroupWithItems_Exception()
        {
            var tree = new GroupTree(GroupTreeType.Processes);
            var rootGroup = new TreeRoot(tree);

            var group = new Group
            {
                new Group()
            };
            rootGroup.Add(group);

            Assert.Throws<BluePrismException>(() => group.Delete());
            Assert.IsTrue(rootGroup.Contains(group));
        }

        /// <summary>
        /// Test deleting a group which contains hidden items throws an exception.
        /// </summary>
        [Test]
        public void Group_Delete_DeleteTopLevelGroupWithHiddenItems_Exception()
        {
            var tree = new GroupTree(GroupTreeType.Processes);
            var rootGroup = new TreeRoot(tree);

            var group = new Group
            {
                ContainsHiddenMembers = true
            };

            rootGroup.Add(group);

            Assert.Throws<BluePrismException>(() => group.Delete());
            Assert.IsTrue(rootGroup.Contains(group));
        }

        /// <summary>
        /// Tests that the items contained in a deleted group are moved to the deleted group's parent.
        /// </summary>
        [Test]
        public void Group_Delete_DeleteGroupWithSubgroup_SubgroupMovesToOwner()
        {
            var tree = new GroupTree(GroupTreeType.Processes);
            var rootGroup = new TreeRoot(tree);

            var parentGroup = new Group()
            {
                Name = "Parent"
            };
            var group = new Group()
            {
                Name = "Group"
            };
            group.Permissions = new MockMemberPermissions
            {
                State = PermissionState.UnRestricted
            };

            var subgroup = new Group
            {
                Name = "Subgroup",
                Permissions = new MockMemberPermissions
                {
                    State = PermissionState.UnRestricted
                }
            };

            rootGroup.Add(parentGroup);
            parentGroup.Add(group);
            group.Add(subgroup);

            group.Delete();

            Assert.IsTrue(parentGroup.Contains(subgroup));
            Assert.IsFalse(parentGroup.Contains(group));
        }

        /// <summary>
        /// Test that deleting a group with manage access rights succeeds.
        /// </summary>
        [Test]
        public void Group_Delete_DeleteGroupWithManageAccessRights_Success()
        {
            var mockServer = new Mock<IServer>(MockBehavior.Strict);
            mockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            Permission.Init(mockServer.Object);
            var mockGroupStore = new Mock<IGroupStore>();
            mockGroupStore.Setup(x => x.GetGroup(It.IsAny<Guid>())).Returns(new Group()
            {
                Name = "Processes"
            });

            var tree = new GroupTree(GroupTreeType.Processes)
            {
                Store = mockGroupStore.Object
            };
            var rootGroup = new TreeRoot(tree)
            {
                Name = "Root"
            };
            var parentGroup = new Group()
            {
                Name = "Parent"
            };
            var group = new Group()
            {
                Name = "Group"
            };
            rootGroup.Add(parentGroup);
            parentGroup.Add(group);

            group.Permissions = SetupMockMemberPermissions(tree);
            group.Delete();

            Assert.IsFalse(parentGroup.Contains(group));
        }

        /// <summary>
        /// Test that deleting a group the user does not have manage access rights permission on fails.
        /// </summary>
        [Test]
        public void Group_Delete_DeleteGroupWithoutManageAccessRights_Exception()
        {
            var mockServer = new Mock<IServer>(MockBehavior.Strict);
            mockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            Permission.Init(mockServer.Object);
            var mockGroupStore = new Mock<IGroupStore>();
            mockGroupStore.Setup(x => x.GetGroup(It.IsAny<Guid>())).Returns(new Group()
            {
                Name = "Processes"
            });

            var tree = new GroupTree(GroupTreeType.Processes)
            {
                Store = mockGroupStore.Object
            };
            var rootGroup = new TreeRoot(tree)
            {
                Name = "Root"
            };
            var parentGroup = new Group()
            {
                Name = "Parent"
            };
            var group = new Group()
            {
                Name = "Group"
            };
            rootGroup.Add(parentGroup);
            parentGroup.Add(group);

            group.Permissions = new MockMemberPermissions
            {
                State = PermissionState.Restricted
            };

            Assert.Throws<BluePrismException>(() => group.Delete());
            Assert.IsTrue(parentGroup.Contains(group));
        }
        
        private static PermissionData CreatePermissionDataObject()
        {
            var perms = new Dictionary<int, Permission>
            {
                { 83, Permission.CreatePermission(83, Permission.ProcessStudio.ManageProcessAccessRights) },
                { 67, Permission.CreatePermission(67, Permission.ProcessStudio.EditProcessGroups) }
            };

            var groups = new Dictionary<int, PermissionGroup>
            {
                {4, new PermissionGroup(4, "Process Studio")}
            };
            var returnValue = new PermissionData(perms, groups);
            return returnValue;
        }

        /// <summary>
        /// Test that deleting a group with a subgroup the user does not have manage access rights permission on fails.
        /// </summary>
        [Test]
        public void Group_Delete_DeleteGroupWithoutManageAccessRightsOnSubgroup_Exception()
        {
            var mockServer = new Mock<IServer>(MockBehavior.Strict);
            mockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            Permission.Init(mockServer.Object);

            var mockGroupStore = new Mock<IGroupStore>();
            mockGroupStore.Setup(x => x.GetGroup(It.IsAny<Guid>())).Returns(new Group()
            {
                Name = "Processes"
            });

            var tree = new GroupTree(GroupTreeType.Processes)
            {
                Store = mockGroupStore.Object
            };
            var rootGroup = new TreeRoot(tree)
            {
                Name = "Root"
            };
            var parentGroup = new Group()
            {
                Name = "Parent"
            };

            var group = new Group()
            {
                Name = "Group"
            };
            group.Permissions = new MockMemberPermissions
            {
                State = PermissionState.UnRestricted
            };

            var subgroup = new Group()
            {
                Name = "Subgroup"
            };
            rootGroup.Add(parentGroup);
            parentGroup.Add(group);
            group.Add(subgroup);

            subgroup.Permissions = SetupMockMemberPermissions(tree, PermissionState.Restricted);

            Assert.Throws<BluePrismException>(() => group.Delete());
            Assert.IsTrue(parentGroup.Contains(group));
            Assert.IsFalse(parentGroup.Contains(subgroup));
        }

        /// <summary>
        /// Test that deleting a group with a subgroup the user does have manage access rights permission on succeeds.
        /// </summary>
        [Test]
        public void Group_Delete_DeleteGroupWithManageAccessRightsOnSubgroup_Success()
        {
            var mockServer = new Mock<IServer>(MockBehavior.Strict);
            mockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            Permission.Init(mockServer.Object);
            var mockGroupStore = new Mock<IGroupStore>();
            mockGroupStore.Setup(x => x.GetGroup(It.IsAny<Guid>())).Returns(new Group()
            {
                Name = "Processes"
            });

            var tree = new GroupTree(GroupTreeType.Processes)
            {
                Store = mockGroupStore.Object
            };

            var rootGroup = new TreeRoot(tree)
            {
                Name = "Root"
            };
            var parentGroup = new Group()
            {
                Name = "Parent"
            };
            var group = new Group()
            {
                Name = "Group"
            };
            rootGroup.Add(parentGroup);
            parentGroup.Add(group);

            group.Permissions = SetupMockMemberPermissions(tree);

            var subgroup = new Group()
            {
                Name = "Subgroup"
            };
            group.Add(subgroup);

            subgroup.Permissions = SetupMockMemberPermissions(tree, PermissionState.Restricted);

            group.Delete();

            Assert.IsTrue(parentGroup.Contains(subgroup));
            Assert.IsFalse(parentGroup.Contains(group));
        }

        [Test]
        public void Group_Delete_DeleteGroupContainingItems_ItemsMoveIntoParentGroup()
        {
            var mockServer = new Mock<IServer>(MockBehavior.Strict);
            mockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            Permission.Init(mockServer.Object);

            var mockGroupStore = new Mock<IGroupStore>();
            mockGroupStore.Setup(x => x.GetGroup(It.IsAny<Guid>())).Returns(new Group()
            {
                Name = "Processes"
            });

            var tree = new GroupTree(GroupTreeType.Processes)
            {
                Store = mockGroupStore.Object
            };

            var rootGroup = new TreeRoot(tree)
            {
                Name = "Root"
            };

            var parentGroup = new Group()
            {
                Name = "Parent"
            };
            var group = new Group()
            {
                Name = "Group"
            };
            rootGroup.Add(parentGroup);
            parentGroup.Add(group);

            group.Permissions = SetupMockMemberPermissions(tree);

            var p = new ProcessGroupMember();
            group.Add(p);

            group.Delete();
            Assert.IsTrue(parentGroup.Contains(p));
            Assert.IsFalse(parentGroup.Contains(group));
        }

        [Test]
        public void Group_UpdateName_NoDuplicates_IsSuccessful()
        {
            var mockServer = new Mock<IServer>(MockBehavior.Strict);
            mockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            Permission.Init(mockServer.Object);
            var mockGroupStore = new Mock<IGroupStore>();
            mockGroupStore.Setup(x => x.GetGroup(It.IsAny<Guid>())).Returns(new Group()
            {
                Name = "Processes"
            });

            var tree = new GroupTree(GroupTreeType.Processes);
            tree.Store = mockGroupStore.Object;
            var rootGroup = new TreeRoot(tree) { Name = "Root" };
            var parentGroup = new Group() { Name = "Parent" };
            var group = new Group() { Name = "Group" };
            rootGroup.Add(parentGroup);
            parentGroup.Add(group);

            group.Permissions = new MockMemberPermissions();
            group.Permissions.State = PermissionState.UnRestricted;

            mockGroupStore.Setup(x => x.GetGroup(It.IsAny<Guid>())).Returns(new Group()
            {
                Name = "GroupA"
            });
            group.UpdateGroupName("GroupA");

            Assert.AreEqual("GroupA", group.Name);
        }

        [Test]
        public void Group_UpdateName_HasDuplicate_Fails()
        {
            var mockServer = new Mock<IServer>(MockBehavior.Strict);
            mockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            Permission.Init(mockServer.Object);
            var mockGroupStore = new Mock<IGroupStore>();
            mockGroupStore.Setup(x => x.GetGroup(It.IsAny<Guid>())).Returns(new Group()
            {
                Name = "Processes"
            });

            var tree = new GroupTree(GroupTreeType.Processes)
            {
                Store = mockGroupStore.Object
            };
            var rootGroup = new TreeRoot(tree)
            {
                Name = "Root"
            };
            var parentGroup = new Group()
            {
                Name = "Parent"
            };
            var groupA = new Group()
            {
                Name = "GroupA"
            };
            var groupB = new Group()
            {
                Name = "GroupB"
            };
            rootGroup.Add(parentGroup);
            parentGroup.Add(groupA);
            parentGroup.Add(groupB);

            groupA.Permissions = new MockMemberPermissions
            {
                State = PermissionState.UnRestricted
            };

            groupB.Permissions = new MockMemberPermissions
            {
                State = PermissionState.UnRestricted
            };

            Assert.Throws<AlreadyExistsException>(() => groupA.UpdateGroupName("GroupB"));
            Assert.AreEqual("GroupA", groupA.Name);
        }

        [Test]
        public void Group_UpdateName_HasDuplicateInOtherGroup_IsSuccessful()
        {
            var mockServer = new Mock<IServer>(MockBehavior.Strict);
            mockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            Permission.Init(mockServer.Object);
            var mockGroupStore = new Mock<IGroupStore>();
            mockGroupStore.Setup(x => x.GetGroup(It.IsAny<Guid>())).Returns(new Group()
            {
                Name = "Processes"
            });

            var tree = new GroupTree(GroupTreeType.Processes)
            {
                Store = mockGroupStore.Object
            };
            var rootGroup = new TreeRoot(tree)
            {
                Name = "Root"
            };
            var parentGroup = new Group()
            {
                Name = "Parent"
            };
            var otherParentGroup = new Group()
            {
                Name = "OtherParent"
            };
            var groupA = new Group()
            {
                Name = "GroupA"
            };
            var groupB = new Group()
            {
                Name = "GroupB"
            };
            rootGroup.Add(parentGroup);
            rootGroup.Add(otherParentGroup);
            parentGroup.Add(groupA);
            otherParentGroup.Add(groupB);

            groupA.Permissions = new MockMemberPermissions
            {
                State = PermissionState.UnRestricted
            };

            groupB.Permissions = new MockMemberPermissions
            {
                State = PermissionState.UnRestricted
            };

            mockGroupStore.Setup(x => x.GetGroup(It.IsAny<Guid>())).Returns(new Group()
            {
                Name = "GroupB"
            });

            groupA.UpdateGroupName("GroupB");
            Assert.AreEqual("GroupB", groupA.Name);
        }

        /// <summary>
        /// Helper function to reduce code duplication
        /// </summary>
        /// <param name="tree">group </param>
        /// <param name="permissionState"></param>
        /// <returns></returns>
        private static MockMemberPermissions SetupMockMemberPermissions(IGroupTree tree, PermissionState permissionState = PermissionState.UnRestricted)
        {
            var mock = new MockMemberPermissions
            {
                State = permissionState
            };
            mock.AddPermissions(new List<Permission>()
            {
                tree.TreeType.GetTreeDefinition().AccessRightsPermission,
                tree.TreeType.GetTreeDefinition().EditPermission
            });
            return mock;
        }

        private class MockMemberPermissions : MemberPermissions
        {
            private List<int> _perms = new List<int>();

            public MockMemberPermissions() : base(null)
            {
            }

            public void AddPermissions(IEnumerable<Permission> perms)
            {
                if (_perms == null)
                    _perms = new List<int>();
                _perms.AddRange(perms.Select(x => x.Id));
            }

            public override bool HasPermission(IUser u, ICollection<Permission> perms)
            {
                return CollectionUtil.ContainsAny(_perms, perms.Select((p => p.Id)));
            }
        }
    }
}
#endif
