#If UNITTESTS Then

Imports System.Linq

Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.Core
Imports BluePrism.Common.Security
Imports BluePrism.UnitTesting.TestSupport
Imports BluePrism.Utilities.Functional
Imports Moq

Imports NUnit.Framework

<TestFixture()>
Public Class ProcessTests

    ''' <summary>
    ''' Tests the zoom handling in a process, ensuring that the right thing is done
    ''' when an old XML schema is loaded, and when a more modern XML schema is loaded
    ''' </summary>
    <Test()>
    Public Sub TestZoom()

        ' All the following tests assume that the scale factor is 1.5
        Assert.That(clsProcess.ScaleFactor, [Is].EqualTo(1.25!),
         "Zoom tests rely on a scale factor of 1.25; not {0}", clsProcess.ScaleFactor)

        ' Build a pre-v5 process from the following XML, testing that the importing
        ' of pre-v5 processes handles the zoom alterations properly.
        Dim xmlPreV5 As String =
            <process name="test" version="1" schema-version="5.0" narrative="">
                <view>
                    <camerax>100</camerax>
                    <cameray>100</cameray>
                    <zoom>1</zoom>
                </view>
                <preconditions/>
                <endpoint narrative=""/>
                <subsheet subsheetid="84e691e1-8ca6-48f0-b633-219224fffd14" type="Normal" published="False">
                    <name>Prime</name>
                    <view>
                        <camerax>56</camerax>
                        <cameray>189</cameray>
                        <zoom>1.2</zoom>
                    </view>
                </subsheet>
                <subsheet subsheetid="716265eb-6d11-4bb8-98f9-6879257da569" type="Normal" published="False">
                    <name>Secondus</name>
                    <view>
                        <camerax>-3</camerax>
                        <cameray>276</cameray>
                        <zoom>1</zoom>
                    </view>
                </subsheet>
                <stage stageid="0e074986-6c3e-4591-baf0-3e3c09086b53" name="Data" type="SubSheetInfo">
                    <subsheetid>84e691e1-8ca6-48f0-b633-219224fffd14</subsheetid>
                    <loginhibit/>
                    <narrative>
                    </narrative>
                    <displayx>-195</displayx>
                    <displayy>-105</displayy>
                    <displaywidth>150</displaywidth>
                    <displayheight>90</displayheight>
                    <font family="Tahoma" size="10" style="Regular" color="000000"/>
                </stage>
                <stage stageid="ed555e48-4c12-47a3-bae8-7c41f00453fa" name="Start" type="Start">
                    <subsheetid>84e691e1-8ca6-48f0-b633-219224fffd14</subsheetid>
                    <loginhibit/>
                    <narrative>
                    </narrative>
                    <displayx>15</displayx>
                    <displayy>-105</displayy>
                    <displaywidth>60</displaywidth>
                    <displayheight>30</displayheight>
                    <font family="Tahoma" size="10" style="Regular" color="000000"/>
                    <onsuccess>bafaecce-e48d-495d-aadd-c092f5ffae7a</onsuccess>
                </stage>
                <stage stageid="bafaecce-e48d-495d-aadd-c092f5ffae7a" name="End" type="End">
                    <subsheetid>84e691e1-8ca6-48f0-b633-219224fffd14</subsheetid>
                    <loginhibit/>
                    <narrative>
                    </narrative>
                    <displayx>105</displayx>
                    <displayy>-105</displayy>
                    <displaywidth>60</displaywidth>
                    <displayheight>30</displayheight>
                    <font family="Tahoma" size="10" style="Regular" color="000000"/>
                </stage>
                <stage stageid="c5ffc15b-17cc-4418-96b2-59e961f5f8e3" name="New TO Request" type="SubSheetInfo">
                    <subsheetid>716265eb-6d11-4bb8-98f9-6879257da569</subsheetid>
                    <loginhibit/>
                    <narrative>
                    </narrative>
                    <displayx>-195</displayx>
                    <displayy>-30</displayy>
                    <displaywidth>150</displaywidth>
                    <displayheight>90</displayheight>
                    <font family="Tahoma" size="10" style="Regular" color="000000"/>
                </stage>
                <stage stageid="e191afc9-5230-4f04-a678-d10b30b6f63c" name="Start" type="Start">
                    <subsheetid>716265eb-6d11-4bb8-98f9-6879257da569</subsheetid>
                    <loginhibit/>
                    <narrative>
                    </narrative>
                    <displayx>15</displayx>
                    <displayy>-15</displayy>
                    <displaywidth>60</displaywidth>
                    <displayheight>30</displayheight>
                    <font family="Tahoma" size="10" style="Regular" color="000000"/>
                    <onsuccess>f56640a7-427e-4fe5-a0fb-52ee214c53cc</onsuccess>
                </stage>
                <stage stageid="8a3bf63b-85ce-400c-9ede-b2187489a075" name="End" type="End">
                    <subsheetid>716265eb-6d11-4bb8-98f9-6879257da569</subsheetid>
                    <loginhibit/>
                    <narrative>
                    </narrative>
                    <displayx>15</displayx>
                    <displayy>570</displayy>
                    <displaywidth>60</displaywidth>
                    <displayheight>30</displayheight>
                    <font family="Tahoma" size="10" style="Regular" color="000000"/>
                </stage>
            </process>.ToString()

        ' Load in the pre-v5 XML
        Dim proc = clsProcess.FromXml(Nothing, xmlPreV5, True)

        ' Have a look at the sheets
        Dim sheets = proc.SubSheets

        ' ----- "Zeroeth" sheet - the main page -----
        proc.SetActiveSubSheet(sheets(0).ID)

        ' The root (ie. main page), had a zoom value of 1 - should have been scaled
        Assert.That(sheets(0).Zoom, [Is].EqualTo(1.25!))
        ' The percentage should look identical though - ie. 100%
        Assert.That(sheets(0).ZoomPercent, [Is].EqualTo(100))

        ' Double check via the process object itself
        Assert.That(proc.Zoom, [Is].EqualTo(1.25!))
        Assert.That(proc.ZoomPercent, [Is].EqualTo(100))

        ' ---- "First" sheet - "Prime" -----
        proc.SetActiveSubSheet(sheets(1).ID)

        ' The first had a (non-default) zoom value of 1.2 - should be left as 1.2
        Assert.That(sheets(1).Zoom, [Is].EqualTo(1.2!))
        ' The zoom percent should not now be 120%, however = should be 1.2 / 1.25 = 96%
        Assert.That(sheets(1).ZoomPercent, [Is].EqualTo(96))

        ' And via the process
        Assert.That(proc.Zoom, [Is].EqualTo(1.2!))
        Assert.That(proc.ZoomPercent, [Is].EqualTo(96))

        ' ----- "Second" sheet - "Secondus" -----
        proc.SetActiveSubSheet(sheets(2).ID)

        ' The second had a default of 1 - should be scaled
        Assert.That(sheets(2).Zoom, [Is].EqualTo(1.25!))
        ' The percentage should look identical though - ie. 100%
        Assert.That(sheets(2).ZoomPercent, [Is].EqualTo(100))

        Assert.That(proc.Zoom, [Is].EqualTo(1.25!))
        Assert.That(proc.ZoomPercent, [Is].EqualTo(100))

        ' Now test that a new process is unchanged
        ' Set the zoom in the second sheet
        sheets(2).Zoom = 1

        ' Check that the value took
        Assert.That(sheets(2).Zoom, [Is].EqualTo(1.0!))
        Assert.That(sheets(2).ZoomPercent, [Is].EqualTo(80))
        Assert.That(proc.Zoom, [Is].EqualTo(1.0!))
        Assert.That(proc.ZoomPercent, [Is].EqualTo(80))

        ' =============================================================

        ' Save and reload the process
        Dim xmlOutput = proc.GenerateXML(True)
        proc = clsProcess.FromXml(Nothing, xmlOutput, True)
        sheets = proc.SubSheets

        ' Now, go through all the same tests (for sheets 0 and 1)
        ' They should all retain the same values they had after the process was
        ' loaded originally

        ' ----- "Zeroeth" sheet - the main page -----
        proc.SetActiveSubSheet(sheets(0).ID)
        Assert.That(sheets(0).Zoom, [Is].EqualTo(1.25!))
        Assert.That(sheets(0).ZoomPercent, [Is].EqualTo(100))
        Assert.That(proc.Zoom, [Is].EqualTo(1.25!))
        Assert.That(proc.ZoomPercent, [Is].EqualTo(100))

        ' ---- "First" sheet - "Prime" -----
        proc.SetActiveSubSheet(sheets(1).ID)
        Assert.That(sheets(1).Zoom, [Is].EqualTo(1.2!))
        Assert.That(sheets(1).ZoomPercent, [Is].EqualTo(96))
        Assert.That(proc.Zoom, [Is].EqualTo(1.2!))
        Assert.That(proc.ZoomPercent, [Is].EqualTo(96))

        ' ----- "Second" sheet - "Secondus" -----
        ' This one differs - it should be zoom = 1; zoompercent = 80
        proc.SetActiveSubSheet(sheets(2).ID)

        ' The second had a default of 1 - should be scaled
        Assert.That(sheets(2).Zoom, [Is].EqualTo(1.0!))
        Assert.That(sheets(2).ZoomPercent, [Is].EqualTo(80))
        Assert.That(proc.Zoom, [Is].EqualTo(1.0!))
        Assert.That(proc.ZoomPercent, [Is].EqualTo(80))

    End Sub

    <Test, TestCaseSource(GetType(TestUtil), "PasswordTests")>
    Sub TestGarbleUngarble(input As String)

        Dim ss As New SafeString(input)
        Dim garbled As String = clsProcess.GarblePassword(ss)

        Assert.That(garbled, Iz.Not.Null)

        ' empty strings result in empty strings and long may they do so
        If input = "" Then
            Assert.That(garbled, Iz.EqualTo(""))

        Else ' Non-empty strings shouldn't look like themselves
            Assert.That(garbled, Iz.Not.EqualTo(input))

        End If

        Dim ungarbled As SafeString = clsProcess.UngarblePassword(garbled)
        Assert.That(ungarbled, Iz.Not.Null)

        Dim ungarbledPlaintext = ungarbled.AsString()

        Assert.That(ungarbledPlaintext, Iz.EqualTo(input))

    End Sub

    Private Function GetBusinessObjects(name As String) As clsBusinessObject
        If name = "webservice" Then
            Return New clsWebService()
        ElseIf name = "COM" Then
            Return New clsCOMBusinessObject("", "")
        Else
            Return New clsVBO(New clsVBODetails(), Nothing, Nothing)
        End If
    End Function

    <Test>
    Public Sub GetVisualBusinessObjectsUsedNamesReturnsExpectedValues()

        Dim actionStages As IEnumerable(Of clsProcessStage) = Enumerable.Range(1, 5).
                                        Concat(Enumerable.Range(3, 7)).
                                        Select(Function(x) New clsActionStage(Nothing) With {.Name = $"Test {x}"}).
                                        Concat({New clsActionStage(Nothing) With {.Name = "webservice"}}).
                                        Concat({New clsActionStage(Nothing) With {.Name = "COM"}}).
                                        ForEach(Sub(x) x.SetResource(x.Name, String.Empty))

        Dim schema = New ProcessSchema(False, "1.0", DiagramType.Object)
        schema.Stages.AddRange(actionStages)
        Dim softwareUnderTest = New Mock(Of clsProcess)(MockBehavior.Loose, Nothing, DiagramType.Process, False)
        softwareUnderTest.Setup(Function(x) x.GetBusinessObjectRef(It.IsAny(Of String))).Returns(Of String)(AddressOf GetBusinessObjects)
        softwareUnderTest.CallBase = True

        Dim expectedResult = Enumerable.Range(1, 9).
                              Select(Function(x) $"Test {x}").
                              ToList()

        ReflectionHelper.SetPrivateField("mSchema", softwareUnderTest.Object, schema)

        Dim result = softwareUnderTest.Object.GetVisualBusinessObjectsUsedNames()

        Assert.AreEqual(expectedResult, result)
    End Sub

    <Test>
    Public Sub GetBusinessObjectsUsedReturnsExpectedValues()

        Dim expectedResult =
                    Enumerable.Range(1, 5).
                    Select(Function(x) Guid.NewGuid()).
                    ToList()


        Dim groupObjectDetailsMock = New Mock(Of IGroupObjectDetails)()
        groupObjectDetailsMock.
                SetupGet(Function(m) m.Children).
                Returns(New List(Of IObjectDetails)())

        Dim objectTree = New clsGroupBusinessObject(groupObjectDetailsMock.Object) With
            {
                .Children =
                {
                    New clsGroupBusinessObject(groupObjectDetailsMock.Object) With
                    {
                        .Children =
                            expectedResult.
                            Select(Function(x) _
                                New clsVBO(New clsVBODetails() With {.ID = x}, Nothing, Nothing)).
                            Select(Function(x) DirectCast(x, clsBusinessObject)).
                            ToList()
                    }
                }
            }

        Dim classUnderTest = New clsProcess(Nothing, DiagramType.Process, False)

        Dim actionStages =
                    Enumerable.Range(1, 5).
                    Concat(Enumerable.Range(3, 7)).
                    Select(Function(x) New clsActionStage(Nothing) With {.Id = Guid.NewGuid()}).
                    ToList()

        Dim schema = New ProcessSchema(False, "1.0", DiagramType.Object)
        schema.Stages.AddRange(actionStages)

        ReflectionHelper.SetPrivateField("mSchema", classUnderTest, schema)
        ReflectionHelper.SetPrivateField(Of clsProcess)(
                "GroupBusinessObjectFactory",
                Nothing,
                DirectCast(
                    Function(x1 As IGroupObjectDetails, x2 As clsProcess, x3 As clsSession, x4 As Boolean, x5 As List(Of String)) objectTree,
                    Func(Of IGroupObjectDetails, clsProcess, clsSession, Boolean, List(Of String), clsGroupBusinessObject)))

        Dim result = classUnderTest.GetBusinessObjectsUsedIds(False)

        Assert.AreEqual(expectedResult, result)

    End Sub

    <Test>
    Public Sub GetProcessesUsedReturnsExpectedValues()

        Dim classUnderTest = New clsProcess(Nothing, DiagramType.Process, False)

        Dim actionStages =
                    Enumerable.Range(1, 5).
                    Concat(Enumerable.Range(3, 7)).
                    Select(Function(x) New clsSubProcessRefStage(Nothing) With {.ReferenceId = Guid.NewGuid()}).
                    ToList()

        Dim expectedResult =
                    actionStages.
                    Select(Function(x) x.ReferenceId).
                    ToList()

        Dim schema = New ProcessSchema(False, "1.0", DiagramType.Object)
        schema.Stages.AddRange(actionStages)

        ReflectionHelper.SetPrivateField("mSchema", classUnderTest, schema)

        Dim result = classUnderTest.GetProcessesUsed()

        Assert.AreEqual(expectedResult, result)

    End Sub

    <Test>
    Public Sub GeneratedXml_CollectionPassedByReference_HasCorrectAttributeValue()
        Dim sut = New clsProcess(Nothing, DiagramType.Process, False)
        sut.PassNestedCollectionsByReference = True

        Dim xml = sut.GenerateXML(Guid.Empty)

        Assert.IsTrue(xml.Contains("byrefcollection=""true"""))
    End Sub

    <Test>
    Public Sub GeneratedXml_CollectionNotPassedByReference_HasCorrectAttributeValue()
        Dim sut = New clsProcess(Nothing, DiagramType.Process, False)
        sut.PassNestedCollectionsByReference = False

        Dim xml = sut.GenerateXML(Guid.Empty)

        Assert.IsTrue(xml.Contains("byrefcollection=""false"""))
    End Sub

    <Test>
    Public Sub ParsingFromXML_NoPassByRefAttribute_DoesPassByReference()
        Dim xml = "<process></process>"

        Dim process = clsProcess.FromXml(Nothing, xml, False)

        Assert.IsTrue(process.PassNestedCollectionsByReference)
    End Sub

    <Test>
    Public Sub ParsingFromXML_CollectionPassedByReference_DoesPassByReference()
        Dim xml = "<process byrefcollection=""true""></process>"

        Dim process = clsProcess.FromXml(Nothing, xml, False)

        Assert.IsTrue(process.PassNestedCollectionsByReference)
    End Sub

    <Test>
    Public Sub ParsingFromXML_CollectionNotPassedByReference_DoesPassByReference()
        Dim xml = "<process byrefcollection=""false""></process>"

        Dim process = clsProcess.FromXml(Nothing, xml, False)

        Assert.IsFalse(process.PassNestedCollectionsByReference)
    End Sub
End Class

#End If