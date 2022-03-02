#If UNITTESTS Then
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.UnitTesting.TestSupport
Imports FluentAssertions
Imports Moq
Imports NUnit.Framework

Namespace ReleaseManager.Conflicts

    <TestFixture>
    Public Class WebApiComponentTests

        Protected Property ServerMock As Mock(Of IServer)

        <SetUp>
        Public Overridable Sub SetUp()
            SetupServerMock()
        End Sub

        Private Sub SetupServerMock()
            ServerMock = New Mock(Of IServer)()

            Dim serverManagerMock = New Mock(Of ServerManager)()
            serverManagerMock.SetupGet(Function(m) m.Server).Returns(ServerMock.Object)

            Dim serverFactoryMock = New Mock(Of ClientServerConnection.IServerFactory)()
            serverFactoryMock.SetupGet(Function(m) m.ServerManager).Returns(serverManagerMock.Object)

            ReflectionHelper.SetPrivateField(GetType(app), "ServerFactory", Nothing, serverFactoryMock.Object)
        End Sub

        <Test>
        Public Sub FindsCorrectConflict_NoExistingWebApiWithName_NotSkillImport()
            Dim existingName = "web api 1"
            ServerMock.Setup(Function(s) s.GetWebApiId(existingName)).Returns(Of Guid)(Nothing)
            Dim owner = New clsRelease(Mock.Of(Of IDataProvider))
            Dim sut = New WebApiComponent(owner, Guid.NewGuid(), existingName)

            sut.Conflicts.Should().BeEmpty()
        End Sub

        <Test>
        Public Sub FindsCorrectConflict_NoExistingWebApiWithName_SkillImport()
            Dim existingName = "web api 1"
            ServerMock.Setup(Function(s) s.GetWebApiId(existingName)).Returns(Of Guid)(Nothing)
            Dim owner = New clsRelease(Mock.Of(Of IDataProvider))
            owner.Members.Add(New SkillComponent(owner, Mock.Of(Of IDataProvider)))
            Dim sut = New WebApiComponent(owner, Guid.NewGuid(), existingName)

            sut.Conflicts.Should().BeEmpty()
        End Sub

        <Test>
        Public Sub FindsCorrectConflict_ExistingNonSkillWebApiWithSameNameAndDifferentId_NotSkillImport()
            Dim existingName = "web api 1"
            ServerMock.Setup(Function(s) s.GetWebApiId(existingName)).Returns(Guid.NewGuid())
            Dim owner = New clsRelease(Mock.Of(Of IDataProvider))
            Dim sut = New WebApiComponent(owner, Guid.NewGuid(), existingName)
            Dim expectedConflicts = GetSingleton.ICollection(New ConflictDefinition(
                "WebApiNameClash",
                "A web api with the same name already exists in the database",
                "Please choose one of the following ways to resolve this conflict",
                New ConflictOption(ConflictOption.UserChoice.Overwrite,
                                   "Overwrite the existing web api with the incoming web api"),
                New ConflictOption(ConflictOption.UserChoice.Rename,
                                   "Choose a new name for the incoming web api",
                                   New ConflictDataHandler("NewName",
                                                           Nothing,
                                                           New ConflictArgument("Web API Name", "", "Web API Name"))),
                New ConflictOption(ConflictOption.UserChoice.Skip,
                                   "Don't import this web api")
                ) With {
                                                                .DefaultInteractiveResolution = ConflictOption.UserChoice.Overwrite,
                                                                .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Overwrite
                                                                }).Select(Function(c) New Conflict(sut, c))

            sut.Conflicts.ShouldBeEquivalentTo(expectedConflicts)
        End Sub

        <Test>
        Public Sub FindsCorrectConflict_ExistingNonSkillWebApiWithSameNameAndDifferentId_SkillImport()
            Dim existingName = "web api 1"
            ServerMock.Setup(Function(s) s.GetWebApiId(existingName)).Returns(Guid.NewGuid())
            Dim owner = New clsRelease(Mock.Of(Of IDataProvider))
            owner.Members.Add(New SkillComponent(owner, Mock.Of(Of IDataProvider)))
            Dim sut = New WebApiComponent(owner, Guid.NewGuid(), existingName)
            Dim expectedConflicts = GetSingleton.ICollection(New ConflictDefinition(
                "WebApiNameClashForSkill",
                "A web api with the same name already exists in the database",
                "The existing web api will need to be renamed before this skill can be imported", Nothing
                ) With {
                                                                .DefaultInteractiveResolution = ConflictOption.UserChoice.Fail,
                                                                .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Fail
                                                                }).Select(Function(c) New Conflict(sut, c))

            sut.Conflicts.ShouldBeEquivalentTo(expectedConflicts)
        End Sub

        <Test>
        Public Sub FindsCorrectConflict_ExistingSkillWebApiWithSameNameAndSameId_SkillImport()
            Dim existingName = "web api 1"
            Dim existingId = Guid.NewGuid()
            ServerMock.Setup(Function(s) s.GetWebApiId(existingName)).Returns(existingId)
            ServerMock.Setup(Function(s) s.GetSkillVersionsWithWebApi(existingId)).Returns({Guid.NewGuid()})
            Dim owner = New clsRelease(Mock.Of(Of IDataProvider))
            owner.Members.Add(New SkillComponent(owner, Mock.Of(Of IDataProvider)))
            Dim sut = New WebApiComponent(owner, existingId, existingName)

            sut.Conflicts.Should().BeEmpty()
        End Sub

        <Test>
        Public Sub FindsCorrectConflict_ExistingSkillWebApiWithSameNameAndSameId_NotSkillImport()
            Dim existingName = "web api 1"
            Dim existingId = Guid.NewGuid()
            ServerMock.Setup(Function(s) s.GetWebApiId(existingName)).Returns(existingId)
            ServerMock.Setup(Function(s) s.GetSkillVersionsWithWebApi(existingId)).Returns({Guid.NewGuid()})
            Dim owner = New clsRelease(Mock.Of(Of IDataProvider))
            Dim sut = New WebApiComponent(owner, existingId, existingName)

            Dim expectedConflicts = GetSingleton.ICollection(New ConflictDefinition(
                "WebApiNameClashForExistingSkill",
                "There is already a web api in the database with the same name that is used by a skill",
                "", New ConflictOption(ConflictOption.UserChoice.Rename,
                                       "Choose a new name for the incoming web api",
                                       New ConflictDataHandler("NewName",
                                                               Nothing,
                                                               New ConflictArgument("Web API Name", "")))
                ) With {
                                                                .DefaultInteractiveResolution = ConflictOption.UserChoice.Rename,
                                                                .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Fail
                                                                }).Select(Function(c) New Conflict(sut, c))

            sut.Conflicts.ShouldBeEquivalentTo(expectedConflicts)
        End Sub

        <Test>
        Public Sub FindsCorrectConflict_ExistingSkillWebApiWithSameNameAndDifferentId_SkillImport()
            Dim existingName = "web api 1"
            Dim existingId = Guid.NewGuid()
            ServerMock.Setup(Function(s) s.GetWebApiId(existingName)).Returns(existingId)
            Dim owner = New clsRelease(Mock.Of(Of IDataProvider))
            owner.Members.Add(New SkillComponent(owner, Mock.Of(Of IDataProvider)))
            Dim sut = New WebApiComponent(owner, Guid.NewGuid(), existingName)
            Dim expectedConflicts = GetSingleton.ICollection(New ConflictDefinition(
                "WebApiNameClashForSkill",
                "A web api with the same name already exists in the database",
                "The existing web api will need to be renamed before this skill can be imported", Nothing
                ) With {
                                                                .DefaultInteractiveResolution = ConflictOption.UserChoice.Fail,
                                                                .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Fail
                                                                }).Select(Function(c) New Conflict(sut, c))

            sut.Conflicts.ShouldBeEquivalentTo(expectedConflicts)
        End Sub

        <Test>
        Public Sub FindsCorrectConflict_ExistingSkillWebApiWithSameNameAndDifferentId_NotSkillImport()
            Dim existingName = "web api 1"
            Dim existingId = Guid.NewGuid()
            ServerMock.Setup(Function(s) s.GetWebApiId(existingName)).Returns(existingId)
            ServerMock.Setup(Function(s) s.GetSkillVersionsWithWebApi(existingId)).Returns({Guid.NewGuid()})
            Dim owner = New clsRelease(Mock.Of(Of IDataProvider))
            Dim sut = New WebApiComponent(owner, Guid.NewGuid(), existingName)
            Dim expectedConflicts = GetSingleton.ICollection(New ConflictDefinition(
                "WebApiNameClashForExistingSkill",
                "There is already a web api in the database with the same name that is used by a skill",
                "", New ConflictOption(ConflictOption.UserChoice.Rename,
                                       "Choose a new name for the incoming web api",
                                       New ConflictDataHandler("NewName",
                                                               Nothing,
                                                               New ConflictArgument("Web API Name", "")))
                ) With {
                                                                .DefaultInteractiveResolution = ConflictOption.UserChoice.Rename,
                                                                .DefaultNonInteractiveResolution = ConflictOption.UserChoice.Fail
                                                                }).Select(Function(c) New Conflict(sut, c))

            sut.Conflicts.ShouldBeEquivalentTo(expectedConflicts)
        End Sub
    End Class

End Namespace
#End If
