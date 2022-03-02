#If UNITTESTS Then
Imports System.Drawing
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.ClientServerResources.Core.Enums
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.Core.Resources
Imports NUnit.Framework

Namespace Resources

    ''' <summary>
    ''' A suite of tests that check the logic in the clsResourceMachine class.
    ''' </summary>
    <TestFixture>
    Public Class ResourceMachineTests
        Private mSut As ResourceMachine

        <SetUp>
        Public Sub SetUp()
            mSut = New ResourceMachine(
                ResourceConnectionState.Connecting,
                "Machine",
                Guid.NewGuid(),
                ResourceAttribute.None)
        End Sub

        <Test>
        Public Sub Constructor_StaticRepresentationOfConnectionState_InitialisesCorrectly()
            Dim id = Guid.NewGuid()
            Dim machine = New ResourceMachine(
                ResourceConnectionState.Connected,
                "Machine",
                id,
                ResourceAttribute.Local)

            Assert.IsNotNull(machine)
            Assert.AreEqual("Machine", machine.Name)
            Assert.AreEqual(id, machine.Id)
            Assert.AreEqual(0, machine.ChildResourceCount)
            Assert.IsNull(machine.ChildResourceNames)
            Assert.IsNull(machine.ChildResources)
            Assert.AreEqual(ResourceMachine.ResourceDBStatus.Unknown, machine.DBStatus)
            Assert.AreEqual(ResourceAttribute.Local, machine.Attributes)
            Assert.AreEqual(ResourceConnectionState.Connected, machine.ConnectionState)
            Assert.False(machine.IsController)
            Assert.AreEqual(0, machine.ProcessesPending)
            Assert.AreEqual(0, machine.ProcessesRunning)
            Assert.AreEqual(String.Empty, machine.LastError)
            Assert.False(machine.IsInPool)
            Assert.IsNull(machine.Info)
            Assert.AreEqual(ResourceStatus.Working, machine.DisplayStatus)
            Assert.AreEqual(Color.Empty, machine.InfoColour)
        End Sub

        <Test>
        <TestCase(ResourceConnectionState.Connected, True)>
        <TestCase(ResourceConnectionState.Connecting, False)>
        <TestCase(ResourceConnectionState.Error, False)>
        <TestCase(ResourceConnectionState.InUse, False)>
        <TestCase(ResourceConnectionState.Offline, False)>
        <TestCase(ResourceConnectionState.Unavailable, False)>
        Public Sub IsConnected_IsCorrect(state As ResourceConnectionState,
                                         isConnected As Boolean)
            Dim machine = New ResourceMachine(
                state,
                "Machine",
                Guid.NewGuid(),
                ResourceAttribute.Local)

            Assert.AreEqual(isConnected, machine.IsConnected)
        End Sub

        <Test>
        Public Sub SettingDBStatus_ValueChange_SetsValue()
            Dim hasChanged = False
            AddHandler mSut.DbStatusChanged, Sub()
                                                 hasChanged = True
                                             End Sub

            mSut.DBStatus = ResourceMachine.ResourceDBStatus.Pending

            Assert.AreEqual(ResourceMachine.ResourceDBStatus.Pending, mSut.DBStatus)
        End Sub

        <Test>
        Public Sub SettingDBStatus_ValueChange_RaisesChangedEvent()
            Dim hasChanged = False
            AddHandler mSut.DbStatusChanged, Sub()
                                                 hasChanged = True
                                             End Sub

            mSut.DBStatus = ResourceMachine.ResourceDBStatus.Pending

            Assert.IsTrue(hasChanged)
        End Sub

        <Test>
        Public Sub SettingDBStatus_NoValueChange_DoesNotRaiseChangedEvent()
            Dim hasChanged = False
            AddHandler mSut.DbStatusChanged, Sub()
                                                 hasChanged = True
                                             End Sub

            mSut.DBStatus = ResourceMachine.ResourceDBStatus.Unknown

            Assert.IsFalse(hasChanged)
        End Sub

        <Test>
        Public Sub HasAttribute_DoesHaveAttribute_IsTrue()
            mSut.Attributes = ResourceAttribute.Debug

            Assert.IsTrue(mSut.HasAttribute(ResourceAttribute.Debug))
        End Sub

        <Test>
        Public Sub HasAttribute_DoesNotHaveAttribute_IsFalse()
            mSut.Attributes = ResourceAttribute.Debug

            Assert.IsFalse(mSut.HasAttribute(ResourceAttribute.LoginAgent))
        End Sub

        <Test>
        Public Sub HasAnyAttribute_DoesHaveAttribute_IsTrue()
            mSut.Attributes = ResourceAttribute.Debug

            Assert.IsTrue(mSut.HasAnyAttribute(ResourceAttribute.Debug _
                                               Or ResourceAttribute.Retired))
        End Sub

        <Test>
        Public Sub HasAnyAttribute_DoesNotHaveAttribute_IsFalse()
            mSut.Attributes = ResourceAttribute.Debug

            Assert.IsFalse(mSut.HasAnyAttribute(ResourceAttribute.LoginAgent _
                                                Or ResourceAttribute.Retired))
        End Sub

        <Test>
        Public Sub SettingAttributes_NoConnectionAndChangeValue_ValueChangedNotRaised()
            Dim hasChanged = False
            AddHandler mSut.AttributesChanged, Sub()
                                                   hasChanged = True
                                               End Sub

            mSut.Attributes = ResourceAttribute.Debug

            Assert.AreEqual(ResourceAttribute.Debug, mSut.Attributes)
            Assert.IsFalse(hasChanged)
        End Sub

        <Test>
        Public Sub SettingAttributes_NoConnectionAndNoChangeOfValue_ValueChangedNotRaised()
            Dim hasChanged = False
            AddHandler mSut.AttributesChanged, Sub()
                                                   hasChanged = True
                                               End Sub

            mSut.Attributes = ResourceAttribute.None

            Assert.AreEqual(ResourceAttribute.None, mSut.Attributes)
            Assert.IsFalse(hasChanged)
        End Sub

        <Test>
        <TestCase(ResourceAttribute.Local, True)>
        <TestCase(ResourceAttribute.Local Or ResourceAttribute.Debug, True)>
        <TestCase(ResourceAttribute.Debug, False)>
        <TestCase(ResourceAttribute.LoginAgent, False)>
        <TestCase(ResourceAttribute.None, False)>
        <TestCase(ResourceAttribute.Pool, False)>
        <TestCase(ResourceAttribute.Private, False)>
        <TestCase(ResourceAttribute.Retired, False)>
        Public Sub IsLocal_IsCorrect(attributes As ResourceAttribute,
                                     isLocal As Boolean)
            mSut.Attributes = attributes

            Assert.AreEqual(isLocal, mSut.Local)
        End Sub

        <Test>
        Public Sub Sessions_NoConnection_EmptyDictionary()
            Assert.AreEqual(GetEmpty.IDictionary(Of Guid, RunnerStatus)(),
                            mSut.GetSessions())
        End Sub

        <Test>
        Public Sub HasPoolMember_DoesNotHave_IsFalse()
            Assert.IsFalse(mSut.HasPoolMember("Pool"))
        End Sub

        <Test>
        Public Sub HasPoolMember_DoesHave_IsTrue()
            mSut.ChildResources = New List(Of IResourceMachine)
            mSut.ChildResources.Add(New ResourceMachine(
                ResourceConnectionState.Connected,
                "Pool",
                Guid.NewGuid(),
                ResourceAttribute.Local))
            Assert.IsTrue(mSut.HasPoolMember("Pool"))
        End Sub

        <Test>
        Public Sub GetPortFromName_EmptyString_IsZero()
            Assert.That(ResourceMachine.GetPortFromName(String.Empty), Iz.EqualTo(0))
        End Sub

        <Test>
        Public Sub GetPortFromName_Null_IsZero()
            Assert.That(ResourceMachine.GetPortFromName(Nothing), Iz.EqualTo(0))
        End Sub

        <Test>
        Public Sub GetPortFromName_NoPortSpecified_IsDefaultPort()
            Assert.That(ResourceMachine.GetPortFromName("Path"), Iz.EqualTo(8181))
        End Sub

        <Test>
        Public Sub GetPortFromName_HasPortSpecified_Port()
            Assert.That(ResourceMachine.GetPortFromName("Path:12121"), Iz.EqualTo(12121))
        End Sub
    End Class

End Namespace
#End If
