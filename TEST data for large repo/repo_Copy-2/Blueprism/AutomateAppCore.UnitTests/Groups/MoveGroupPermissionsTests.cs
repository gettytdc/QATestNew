#if UNITTESTS
using BluePrism.AutomateAppCore;
using BluePrism.AutomateAppCore.Auth;
using BluePrism.AutomateAppCore.Groups;
using BluePrism.BPCoreLib.Data;
using BluePrism.Server.Domain.Models;
using BluePrism.UnitTesting.TestSupport;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace AutomateAppCore.UnitTests.Groups
{
    [TestFixture]
    public class MoveGroupPermissionsTests
    {
        private Guid mSourceId;
        private Mock<IServer> mMockServer;
        private Mock<IGroup> mTargetGroup;
        private Mock<IGroup> mSourceGroup;
        private MessageHelper mSg;
        private Guid mTargetId;
        private IUser mOckUser;

        [SetUp]
        public void Init()
        {
            mSourceId = Guid.NewGuid();
            mTargetId = Guid.NewGuid();
            mSg = new MessageHelper(true); // Press yes on confirm dialog.
            mSourceGroup = new Mock<IGroup>();
            mTargetGroup = new Mock<IGroup>();
            mMockServer = new Mock<IServer>(MockBehavior.Strict);
            mOckUser = CreateDefaultUserObject();
        }

        private void SetupMockServer(IServer serverMock)
        {
            var serverManagerMock = new Mock<ServerManager>();
            serverManagerMock.SetupGet(m => m.Server).Returns(serverMock);
            var serverFactoryMock = new Mock<BluePrism.AutomateAppCore.ClientServerConnection.IServerFactory>();
            serverFactoryMock.SetupGet(m => m.ServerManager).Returns(serverManagerMock.Object);
            ReflectionHelper.SetPrivateField(typeof(app), "ServerFactory", null, serverFactoryMock.Object);
        }

        /// <summary>
        /// Test that an unrestricted group can be moved under another unrestricted
        /// group without a warning confirmation message being displayed.
        /// </summary>
        [Test]
        public void TestValidateMoveSimpleGroup()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted);
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted);
            _ = mMockServer.SetupSequence(x => x.GetEffectiveGroupPermissions(It.IsAny<Guid>())).Returns(sourcePermissions).Returns(newParentPermissions);
            _ = mMockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            _ = mSourceGroup.Setup(x => x.Tree).Returns(new GroupTree(GroupTreeType.Processes));
            _ = mSourceGroup.Setup(x => x.IsGroup).Returns(true);
            SetupMockServer(mMockServer.Object);

            Permission.Init(mMockServer.Object);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(mSg.WasCalled);
        }

        /// <summary>
        /// Test that moving an restricted group under a parent that is unrestricted is ok.
        /// </summary>
        [Test]
        public void TestValidateMoveDirectlyRestrictedToUnrestricted()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted);
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted);
            _ = mMockServer.SetupSequence(x => x.GetEffectiveGroupPermissions(It.IsAny<Guid>())).Returns(sourcePermissions).Returns(newParentPermissions);
            _ = mMockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            SetupMockServer(mMockServer.Object);

            var myMock = new MyGroupMember();
            _ = mSourceGroup.Setup(x => x.Tree).Returns(new GroupTree(GroupTreeType.Processes));
            _ = mSourceGroup.Setup(x => x.RawMember).Returns(myMock);
            _ = mSourceGroup.Setup(x => x.IsGroup).Returns(true);
            Permission.Init(mMockServer.Object);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Message_Inherit_Perms, mSg.MessageID);
        }

        [Serializable()]
        [DebuggerDisplay("{MemberType}: {Name}")]
        [DataContract(Namespace = "bp", IsReference = true)]
        private class MyGroupMember : GroupMember
        {
            public MyGroupMember() : base("Test")
            {
            }

            protected MyGroupMember(IDataProvider prov) : base(prov)
            {
            }

            internal MyGroupMember(string memberName) : base(memberName)
            {
            }

            internal MyGroupMember(object memberId, string memberName) : base(memberId, memberName)
            {
            }

            public override string ImageKey
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override string LinkTableName
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override GroupMemberType MemberType { get; } = GroupMemberType.Group;
        }

        /// <summary>
        /// Test that moving an unrestricted group under a parent that is restricted raises a warning -
        /// which we are going to say 'yes' to this time.
        /// </summary>
        [Test]
        public void TestValidateMoveDirectlyRestrictedToRestrictedTrueOnConfirm()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Message_Inherit_Perms, mSg.MessageID);
        }


        /// <summary>
        /// Test that moving an unrestricted group under a parent that is restricted raises a warning -
        /// which we are going to say 'no' to this time.
        /// </summary>
        [Test]
        public void TestValidateMoveDirectlyRestrictedToRestrictedFalseOnConfirm()
        {
            // Arrange
            mSg = new MessageHelper(false); // Press no on confirm dialog.
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Message_Inherit_Perms, mSg.MessageID);
        }



        /// <summary>
        /// Test that moving an unrestricted group under a parent that is restricted by inheritance
        /// raises a warning - which we are going to say 'yes' to this time.
        /// </summary>
        [Test]
        public void TestValidateMoveDirectlyRestrictedToInhRestricted()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Message_Inherit_Perms, mSg.MessageID);
        }

        /// <summary>
        /// Test that moving a restricted group under a parent that is restricted raises a warning -
        /// </summary>
        [Test]
        public void TestValidateMoveDirectlyRestrictedToRestricted()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Message_Overwrite_Parent, mSg.MessageID);
        }

        /// <summary>
        /// Test that moving a restricted group under a parent that is restricted ny inheritance
        /// raises a warning -
        /// </summary>
        [Test]
        public void TestValidateMoveDirectlyRestrictedToRestrictedByInh()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Message_Overwrite_Parent, mSg.MessageID);
        }

        /// <summary>
        /// Test that moving a restricted by inheritance group under a parent that
        /// is unrestricted raises a warning
        /// </summary>
        [Test]
        public void TestValidateMoveRestrictedByInheritanceToUnrestricted()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Message_Lose_Perms, mSg.MessageID);
        }

        /// <summary>
        /// Test that moving a restricted by inheritance group under another restricted
        /// by inheritance group with the same ancestor doesn't shwo a warning.
        /// </summary>
        [Test]
        public void TestValidateMoveRestrictedByInheritanceToRestrictedBySameAncestor()
        {
            // Arrange
            var ancestor = Guid.NewGuid();
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance) { InheritedAncestorID = ancestor };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance) { InheritedAncestorID = ancestor };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(mSg.WasCalled);
        }


        /// <summary>
        /// Test that moving a restricted by inheritance group under another restricted
        /// by inheritance group with the same ancestor doesn't shwo a warning.
        /// </summary>
        [Test]
        public void TestValidateMoveRestrictedByInheritanceToRestrictedByInhSameAncestor()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance) { InheritedAncestorID = mTargetId };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = mTargetId };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(mSg.WasCalled);
        }

        /// <summary>
        /// Test that moving a restricted by inheritance group under another restricted
        /// group with a different ancestor shows a warning.
        /// </summary>
        [Test]
        public void TestValidateMoveRestrictedByInheritanceToRestrictedSameAncestor()
        {
            // Arrange
            var ancestorId = Guid.NewGuid();
            var ancestorId2 = Guid.NewGuid();
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance) { InheritedAncestorID = ancestorId };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance) { InheritedAncestorID = ancestorId2 };
            var ancestorGroup = new Group() { Name = "newGroup" };
            _ = mMockServer.Setup(x => x.GetGroup(ancestorId2)).Returns(ancestorGroup);
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Message_Overwrite_Ancestor, mSg.MessageID);
        }

        /// <summary>
        /// Test that moving a restricted by inheritance group under another restricted
        /// by inheritance group with a different ancestor doesn't show a warning as the
        /// ancestors are the same.
        /// </summary>
        [Test]
        public void TestValidateMoveRestrictedByInheritanceToRestrictedByInhSameAncestorUpTree()
        {
            // Arrange
            var ancestorId = Guid.NewGuid();
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance) { InheritedAncestorID = ancestorId };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance) { InheritedAncestorID = ancestorId };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(mSg.WasCalled);
        }


        /// <summary>
        /// Test that when moving a process from an unrestricted folder to a restricted folder displays the correct warning.
        /// </summary>
        [Test]
        public void TestMoveProcessFromUnrestrictedToRestrictedDisplaysWarning()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted);
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted);
            var movingMember = new Mock<IGroupMember>();
            _ = movingMember.Setup(x => x.Id).Returns(memberId);
            _ = movingMember.Setup(x => x.IsGroup).Returns(false);
            _ = movingMember.Setup(x => x.Tree).Returns(new GroupTree(GroupTreeType.Processes));
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(movingMember.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(mSg.MessageID, GroupPermissionLogic.MessageID.Warn_Message_Member_Inherit_Perms);
        }

        /// <summary>
        /// Test that when moving a process from an unrestricted folder to folder restricted by inheritance displays the correct warning.
        /// </summary>
        [Test]
        public void TestMoveProcessFromUnrestrictedToRestrictedByInheritanceDisplaysWarning()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted);
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance);
            var movingMember = new Mock<IGroupMember>();
            _ = mSourceGroup.Setup(x => x.Id).Returns(mSourceId);
            _ = movingMember.Setup(x => x.Id).Returns(memberId);
            _ = movingMember.Setup(x => x.IsGroup).Returns(false);
            _ = movingMember.Setup(x => x.Tree).Returns(new GroupTree(GroupTreeType.Processes));
            _ = mTargetGroup.Setup(x => x.Id).Returns(mTargetId);
            _ = mMockServer.Setup(x => x.GetEffectiveGroupPermissions(mSourceId)).Returns(sourcePermissions);
            _ = mMockServer.Setup(x => x.GetEffectiveGroupPermissions(mTargetId)).Returns(newParentPermissions);
            _ = mMockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            SetupMockServer(mMockServer.Object);
            Permission.Init(mMockServer.Object);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(movingMember.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(mSg.MessageID, GroupPermissionLogic.MessageID.Warn_Message_Member_Inherit_Perms);
        }


        /// <summary>
        /// Test that when moving a process from n restricted folder to an unrestricted folder displays the correct warning.
        /// </summary>
        [Test]
        public void TestMoveProcessFromRestrictedToUnRestrictedDisplaysWarning()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted);
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted);
            var movingMember = new Mock<IGroupMember>();
            _ = mSourceGroup.Setup(x => x.Id).Returns(mSourceId);
            _ = movingMember.Setup(x => x.Id).Returns(memberId);
            _ = movingMember.Setup(x => x.IsGroup).Returns(false);
            _ = movingMember.Setup(x => x.Tree).Returns(new GroupTree(GroupTreeType.Processes));
            _ = mTargetGroup.Setup(x => x.Id).Returns(mTargetId);
            _ = mMockServer.Setup(x => x.GetEffectiveGroupPermissions(mSourceId)).Returns(sourcePermissions);
            _ = mMockServer.Setup(x => x.GetEffectiveGroupPermissions(mTargetId)).Returns(newParentPermissions);
            _ = mMockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            SetupMockServer(mMockServer.Object);
            Permission.Init(mMockServer.Object);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(movingMember.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(mSg.MessageID, GroupPermissionLogic.MessageID.Warn_Message_Member_Lose_Perms);
        }

        /// <summary>
        /// Test that when moving a process from a group with inherited restrictions to an unrestricted group displays the correct warning.
        /// </summary>
        [Test]
        public void TestMoveProcessFromRestrictedByInheritanceToUnRestrictedDisplaysWarning()
        {
            // Arrange
            var memberId = Guid.NewGuid();
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance);
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted);
            var movingMember = new Mock<IGroupMember>();
            _ = mSourceGroup.Setup(x => x.Id).Returns(mSourceId);
            _ = movingMember.Setup(x => x.Id).Returns(memberId);
            _ = movingMember.Setup(x => x.IsGroup).Returns(false);
            _ = movingMember.Setup(x => x.Tree).Returns(new GroupTree(GroupTreeType.Processes));
            _ = mTargetGroup.Setup(x => x.Id).Returns(mTargetId);
            _ = mMockServer.Setup(x => x.GetEffectiveGroupPermissions(mSourceId)).Returns(sourcePermissions);
            _ = mMockServer.Setup(x => x.GetEffectiveGroupPermissions(mTargetId)).Returns(newParentPermissions);
            _ = mMockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            SetupMockServer(mMockServer.Object);
            Permission.Init(mMockServer.Object);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(movingMember.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(mSg.MessageID, GroupPermissionLogic.MessageID.Warn_Message_Member_Lose_Perms);
        }

        /// <summary>
        /// Test that copying a group to a restricted folder shows and appropriate warning.
        /// </summary>
        [Test]
        public void TestCopyGroupToRestrictedParent()
        {
            // Arrange
            var ancestorId = Guid.NewGuid();
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.Empty };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = ancestorId };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Copy_Unrestricted_to_Restricted, mSg.MessageID);
        }


        /// <summary>
        /// Test that copying a gorup to an unrestricted parent shows and appropriate message.
        /// </summary>
        [Test]
        public void TestCopyGroupToUnRestrictedParent()
        {
            // Arrange
            var ancestorId = Guid.NewGuid();
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = ancestorId };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.Empty };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Copy_Restricted_to_Unrestricted, mSg.MessageID);
        }

        /// <summary>
        /// Test that when we copy a group to an unrestricted parent from an unrestricted parent no message is shown
        /// </summary>
        [Test]
        public void TestCopyGroupToUnRestrictedParentFromUnres()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.Empty };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.Empty };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(mSg.WasCalled);
        }

        /// <summary>
        /// Test that when we copy a restricted folder under another restricted folder with a different ancestor, a message is shown
        /// </summary>
        [Test]
        public void TestCopyGroupToRestrictedParentFromDiffRes()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Copy_Restricted_Diff_Ancestor, mSg.MessageID);
        }

        /// <summary>
        /// Test that if we copy a folder to another place under the same tree no message is shown.
        /// Not convinced this is possible in the UI.
        /// </summary>
        [Test]
        public void TestCopyGroupSameParentSameState()
        {
            // Arrange
            var ancestorId = Guid.NewGuid();
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance) { InheritedAncestorID = ancestorId };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance) { InheritedAncestorID = ancestorId };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(mSg.WasCalled);
        }


        /// <summary>
        /// Test that if we copy a restricted folder to another restricted place under the same tree
        /// a specific message is shown
        /// </summary>
        [Test]
        public void TestCopyGroupRestrictedToRestricted()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Copy_Restricted_Diff_Ancestor, mSg.MessageID);
        }


        /// <summary>
        /// Test that if we copy a folder to another place under the same tree no message is shown.
        /// Not convinced this is possible in the UI.
        /// </summary>
        [Test]
        public void TestCopyGroupResInhToUnrestricted()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Copy_Restricted_to_Unrestricted, mSg.MessageID);
        }


        /// <summary>
        /// Test that a user can move an unrestricted group into an unrestricted group when user has the 'Edit Groups' permission.
        /// </summary>
        [Test]
        public void TestCopyGroupUnrestrictedToUnrestrictedUserGotEditGroupsPermissions()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(mSg.WasCalled);
        }


        /// <summary>
        /// Test that a user without the 'Edit Groups' permissions can not move a
        /// </summary>
        [Test]
        public void TestCopyGroupUnrestrictedToUnrestrictedUserNotGotEditGroupsPermission()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var mockUser = new Mock<IUser>(MockBehavior.Strict);
            _ = mockUser.Setup(x => x.HasPermission(Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups))).Returns(false);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mockUser.Object);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Inform_Insufficient_Permissions_Move, mSg.MessageID);
        }

        /// <summary>
        /// Test that a user with the 'Edit Process Groups' and 'Manage Access Rights' permissions on the restricted group can move an unrestricted group into it
        /// </summary>
        [Test]
        public void TestCopyGroupUnrestrictedToRestrictedUserGotEditAndManageGroupsPermission()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Copy_Unrestricted_to_Restricted, mSg.MessageID);
        }

        /// <summary>
        /// Test that a user without the 'Manage Access Rights' permissions on the restricted group can not move an unrestricted group into it
        /// </summary>
        [Test]
        public void TestCopyGroupUnrestrictedToRestrictedUserNotGotManageGroupsPermission()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Inform_Insufficient_Permissions_Restricted_Target, mSg.MessageID);
        }

        /// <summary>
        /// Test that a user without the 'Edit Process Group' permissions on the restricted group can not move an unrestricted group into it
        /// </summary>
        [Test]
        public void TestCopyGroupUnrestrictedToRestrictedUserNotGotEditGroupsPermission()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Inform_Insufficient_Permissions_Restricted_Target, mSg.MessageID);
        }


        /// <summary>
        /// Test that a user with the 'Edit Process Group'and 'Manage Access Rights' permissions on the restricted group can move it into an unrestricted group
        /// </summary>
        [Test]
        public void TestCopyGroupRestrictedToUnrestrictedUserGotEditAndManageGroupsPermission()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights),
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Copy_Restricted_to_Unrestricted, mSg.MessageID);
        }

        /// <summary>
        /// Test that a user without the 'Edit Process Group' permissions on the restricted group can not move it into an unrestricted group
        /// </summary>
        [Test]
        public void TestCopyGroupRestrictedToUnrestrictedUserNotGotEditGroupsPermission()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Inform_Insufficient_Permissions_Restricted_Source, mSg.MessageID);
        }

        /// <summary>
        /// Test that a user without the 'Manage Group' permissions on the restricted group can not move it into an unrestricted group
        /// </summary>
        [Test]
        public void TestCopyGroupRestrictedToUnrestrictedUserNotGotManageGroupsPermission()
        {
            // Arrang
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.UnRestricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Inform_Insufficient_Permissions_Restricted_Source, mSg.MessageID);
        }

        /// <summary>
        /// Test that a user without the 'Manage Group' permissions on the source restricted group can not move it into an restricted group
        /// </summary>
        [Test]
        public void TestCopyGroupRestrictedToRestrictedUserNotGotManageGroupsPermissionOnSourceGroup()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var sourceglp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups)
            };
            sourcePermissions.Add(sourceglp);
            var newParentglp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            newParentPermissions.Add(newParentglp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Inform_Insufficient_Permissions_Restricted_Source, mSg.MessageID);
        }

        /// <summary>
        /// Test that a user without the 'Edit Group' permissions on the source restricted group can not move it into an restricted group
        /// </summary>
        [Test]
        public void TestCopyGroupRestrictedToRestrictedUserNotGotEditGroupsPermissionOnSourceGroup()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var sourceglp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(sourceglp);
            var newParentglp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            newParentPermissions.Add(newParentglp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Inform_Insufficient_Permissions_Restricted_Source, mSg.MessageID);
        }

        /// <summary>
        /// Test that a user with the 'Edit Groups' and 'Manage Access Rights' permissions on the source and target restricted groups can move it into an restricted group
        /// </summary>
        [Test]
        public void TestCopyGroupRestrictedToRestrictedUserGotEditAndManageGroupsPermission()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var sourceglp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(sourceglp);
            var newParentglp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            newParentPermissions.Add(newParentglp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Warn_Copy_Restricted_Diff_Ancestor, mSg.MessageID);
        }

        /// <summary>
        /// Test that a user without the 'Edit Groups' permissions on the target restricted group can not move a restricted group into it
        /// </summary>
        [Test]
        public void TestCopyGroupRestrictedToRestrictedUserNotGotEditGroupsPermissionOnTarget()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var sourceglp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(sourceglp);
            var newParentglp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            newParentPermissions.Add(newParentglp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Inform_Insufficient_Permissions_Restricted_Target, mSg.MessageID);
        }


        /// <summary>
        /// Test that a user without the 'Manage Access rights' permissions on the target restricted group can not move a restricted group into it
        /// </summary>
        [Test]
        public void TestCopyGroupRestrictedToRestrictedUserNotGotManageGroupsPermissionOnTarget()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = Guid.NewGuid() };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var sourceglp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups),
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(sourceglp);
            var newParentglp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups)
            };
            newParentPermissions.Add(newParentglp);
            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Inform_Insufficient_Permissions_Restricted_Target, mSg.MessageID);
        }


        /// <summary>
        /// Test that when copying a process into a subfolder with inherited permissions at the same node as the
        /// process, no message is displayed.
        /// </summary>
        [Test]
        public void TestMoveProcessToInheritedRestrictedGroupAtSameSubLevel()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = mSourceId };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance) { InheritedAncestorID = mSourceId };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.EditProcessGroups)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(mSg.WasCalled);

            // same but with copy = true
            result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, true, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(mSg.WasCalled);
        }

        /// <summary>
        /// Test that when copying a process into a subfolder with inherited permissions at the same node as the
        /// process, no message is displayed.
        /// </summary>
        [Test]
        public void TestMoveProcessToInheritedRestrictedGroupWithoutEditGroupsPermissionAtSameSubLevel()
        {
            // Arrange
            var sourcePermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.Restricted) { InheritedAncestorID = mSourceId };
            var newParentPermissions = new GroupPermissions(Guid.NewGuid(), PermissionState.RestrictedByInheritance) { InheritedAncestorID = mSourceId };
            SetupMocks(sourcePermissions, newParentPermissions, true);
            var glp = new GroupLevelPermissions(1)
            {
                Permission.GetPermission(Permission.ProcessStudio.ManageProcessAccessRights)
            };
            sourcePermissions.Add(glp);
            newParentPermissions.Add(glp);

            // sut
            var sut = new GroupPermissionLogic();

            // Act
            var result = sut.ValidateMoveMember(mSourceGroup.Object, mSourceGroup.Object, mTargetGroup.Object, false, mSg.ShowMessage, mOckUser);

            // Assert
            Assert.IsFalse(result);
            Assert.IsTrue(mSg.WasCalled);
            Assert.AreEqual(GroupPermissionLogic.MessageID.Inform_Insufficient_Permissions_Same_Target_And_Source, mSg.MessageID);
        }

        private void SetupMocks(IGroupPermissions sourcePermissions, IGroupPermissions newParentPermissions, bool isGroup)
        {
            _ = mSourceGroup.Setup(x => x.Id).Returns(mSourceId);
            _ = mSourceGroup.Setup(x => x.IsGroup).Returns(isGroup);
            _ = mTargetGroup.Setup(x => x.Id).Returns(mTargetId);
            _ = mMockServer.Setup(x => x.GetEffectiveGroupPermissions(mSourceId)).Returns(sourcePermissions);
            _ = mMockServer.Setup(x => x.GetEffectiveGroupPermissions(mTargetId)).Returns(newParentPermissions);
            _ = mMockServer.Setup(x => x.GetPermissionData()).Returns(CreatePermissionDataObject());
            _ = mSourceGroup.Setup(x => x.Tree).Returns(new GroupTree(GroupTreeType.Processes));
            SetupMockServer(mMockServer.Object);
            Permission.Init(mMockServer.Object);
        }

        /// <summary>
        /// This class mimics a message box, but allows the unit test to
        /// retrieve the ID of the message
        /// </summary>
        private class MessageHelper
        {
            public bool Response { get; set; } = false;
            public bool WasCalled { get; set; } = false;
            public GroupPermissionLogic.MessageID MessageID { get; set; }

            public MessageHelper(bool response)
            {
                Response = response;
            }

            public bool ShowMessage(GroupPermissionLogic.MessageID id, bool informOnly, params string[] args)
            {
                WasCalled = true;
                MessageID = id;
                return Response;
            }
        }

        private PermissionData CreatePermissionDataObject()
        {
            var perms = new Dictionary<int, Permission>
            {
                { 83, Permission.CreatePermission(83, Permission.ProcessStudio.ManageProcessAccessRights) },
                { 73, Permission.CreatePermission(73, Permission.ProcessStudio.EditProcessGroups) }
            };
            var groups = new Dictionary<int, PermissionGroup>
            {
                { 4, new PermissionGroup(4, "Process Studio") }
            };
            var retval = new PermissionData(perms, groups);
            return retval;
        }

        private IUser CreateDefaultUserObject()
        {
            var mockUser = new Mock<IUser>(MockBehavior.Strict);
            _ = mockUser.Setup(x => x.HasPermission(It.IsAny<Permission>())).Returns(true);
            _ = mockUser.Setup(x => x.HasPermission(It.IsAny<string>())).Returns(true);
            _ = mockUser.Setup(x => x.HasPermission(It.IsAny<ICollection<Permission>>())).Returns(true);
            _ = mockUser.Setup(x => x.IsSystemAdmin).Returns(false);
            _ = mockUser.SetupGet(x => x.AuthType).Returns(AuthMode.Native);
            var userRoleSet = new RoleSet();
            var dataProvider = new DictionaryDataProvider(new Hashtable() { { "id", 1 }, { "name", "System Administrators" }, { "ssogroup", "Test AD Group" } });
            _ = userRoleSet.Add(new Role(dataProvider));
            _ = mockUser.Setup(x => x.Roles).Returns(userRoleSet);
            return mockUser.Object;
        }
    }
}
#endif
