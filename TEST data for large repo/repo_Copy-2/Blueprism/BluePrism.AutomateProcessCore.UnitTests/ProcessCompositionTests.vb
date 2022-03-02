Imports BluePrism.Core.Expressions
Imports BluePrism.CharMatching
Imports BluePrism.AutomateProcessCore.ProcessLoading
Imports BluePrism.ApplicationManager.AMI
#If UNITTESTS Then

Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AMI

Imports NUnit.Framework

Public Class ProcessCompositionTests

    Private gTestObjectID As Guid = Guid.NewGuid
    Private gTestChildObjectID As Guid = Guid.NewGuid
    Private gTestParentObjectID As Guid = Guid.NewGuid

    Private Class clsTestProcessLoader
        Implements IProcessLoader

        Public ProcXML As New Dictionary(Of Guid, String)

        Public ReadOnly Property AvailableFontNames As ICollection(Of String) Implements IFontStore.AvailableFontNames
            Get
                Throw New NotImplementedException()
            End Get
        End Property

        Public Property CacheBehaviour As CacheRefreshBehaviour Implements IProcessLoader.CacheBehaviour
            Get
                Return CacheRefreshBehaviour.CheckForUpdatesEveryTime
            End Get
            Set(value As CacheRefreshBehaviour)
            End Set
        End Property

        Public Sub SaveFont(font As BPFont) Implements IFontStore.SaveFont
            Throw New NotImplementedException()
        End Sub

        Public Sub SaveFontOcrPlus(name As String, data As String) Implements IFontStore.SaveFontOcrPlus
            Throw New NotImplementedException()
        End Sub

        Public Function GetEnvVars() As Dictionary(Of String, clsArgument) Implements IProcessLoader.GetEnvVars
            Return New Dictionary(Of String, clsArgument)
        End Function

        Public Function GetEnvVars(refreshFromServer As Boolean) As Dictionary(Of String, clsArgument) Implements IProcessLoader.GetEnvVars
            Return New Dictionary(Of String, clsArgument)
        End Function

        Public Function GetEnvVarSingle(name As String, updateCache As Boolean) As clsArgument Implements IProcessLoader.GetEnvVarSingle
            Return New clsArgument("GetEnvVarSingle", New clsProcessValue(DataType.text, "GetEnvVarSingle_txt"))
        End Function


        Public Function GetFont(name As String) As BPFont Implements IFontStore.GetFont
            Throw New NotImplementedException()
        End Function

        Public Function GetFontOcrPlus(name As String) As String Implements IFontStore.GetFontOcrPlus
            Throw New NotImplementedException()
        End Function

        Public Function DeleteFont(name As String) As Boolean Implements IFontStore.DeleteFont
            Throw New NotImplementedException()
        End Function

        Public Function GetAMIInfo() As clsGlobalInfo Implements IProcessLoader.GetAMIInfo
            Return New clsGlobalInfo
        End Function

        ''' <summary>
        ''' Get the process XML from the data source for a given process.
        ''' </summary>
        ''' <param name="gProcessID">The ID of the process</param>
        ''' <param name="sXML">On success, the process XML</param>
        ''' <param name="loadedDate">The date/time the process was loaded.</param>
        ''' <param name="sErr">On failure, an error message</param>
        ''' <returns>True if successful, otherwise false</returns>
        Public Function GetProcessXML(ByVal gProcessID As Guid, ByRef sXML As String, ByRef loadedDate As Date, ByRef sErr As String) As Boolean Implements IProcessLoader.GetProcessXML
            loadedDate = Date.Now
            If ProcXML.ContainsKey(gProcessID) Then
                sXML = ProcXML(gProcessID)
                Return True
            End If
            sErr = "Process not found"
            Return False

        End Function

        Public Function GetEffectiveRunMode(processId As Guid) As BusinessObjectRunMode Implements IProcessLoader.GetEffectiveRunMode
            Return BusinessObjectRunMode.Exclusive
        End Function

        ''' <summary>
        ''' Gets the attributes of the specified process from the database.
        ''' </summary>
        ''' <param name="gProcessID">ID of the process.</param>
        ''' <param name="Attributes">Attributes of the process.</param>
        ''' <param name="sErr">Error message when return value is false.</param>
        ''' <returns>Returns true unless error occurs; otherwise false.</returns>
        Public Function GetProcessAtrributes(ByVal gProcessID As Guid, ByRef Attributes As ProcessAttributes, ByRef sErr As String) As Boolean Implements IProcessLoader.GetProcessAtrributes
            Attributes = ProcessAttributes.Published
            Return True
        End Function

    End Class
    Private mProcessLoader As New clsTestProcessLoader()

    Private Class TestObjectLoader : Implements IObjectLoader
        Public Function CreateAll(p As clsProcess, s As clsSession) As IEnumerable(Of clsInternalBusinessObject) Implements IObjectLoader.CreateAll
            Return New List(Of clsInternalBusinessObject)
        End Function
    End Class
    Private mObjectLoader As New TestObjectLoader()

    Private Sub LinkStages(ByVal objFrom As clsProcessStage, ByVal objTo As clsProcessStage, ByVal objProcess As clsProcess)
        Dim sErr As String = Nothing
        If Not objProcess.CreateLink(CType(objFrom, clsLinkableStage), objTo, sErr) Then
            Assert.That(False, "Failed to link stage " & objFrom.GetName() & " to " & objTo.GetName() & " : " & sErr)
        End If
    End Sub


    ''' <summary>
    ''' Programtically build a test business object.
    ''' </summary>
    ''' <param name="name">Name of this object.</param>
    ''' <param name="modelFrom">Name of parent object to depend on for the
    ''' Application Model. If Nothing, the object will have its own model.</param>
    ''' <param name="modelFromObj">If modelFrom is specified, this must be the
    ''' corresponding Process.</param>
    ''' <returns>The created process.</returns>
    Private Function CreateTestBusinessObject(ByVal name As String, ByVal modelFrom As String, ByVal modelFromObj As clsProcess) As clsProcess

        Dim stage1 As clsProcessStage, stage2 As clsProcessStage

        'Create the process object...
        Dim proc As New clsProcess(Nothing, DiagramType.Object, True)
        proc.Name = name

        'Tell it that it's a business object, not a normal process...
        proc.GetMainPage.Published = True

        Dim calcmainwindow As clsApplicationElement
        Dim button1 As clsApplicationElement
        Dim button1_7 As clsApplicationElement
        If modelFrom IsNot Nothing Then
            proc.ParentObject = modelFrom
            calcmainwindow = CType(modelFromObj.ApplicationDefinition.FindMemberByName("Main Window"), clsApplicationElement)
            button1 = CType(modelFromObj.ApplicationDefinition.FindMemberByName("1 Button"), clsApplicationElement)
            button1_7 = CType(modelFromObj.ApplicationDefinition.FindMemberByName("1 Button Win7"), clsApplicationElement)
        Else
            'Create an application definition...
            proc.ApplicationDefinition = New clsApplicationDefinition()
            Dim ati As clsApplicationTypeInfo
            Dim ami As New clsAMI(New clsGlobalInfo)

            'Find and set the "Win32" application type...
            Dim lati = ami.GetApplicationTypes()
            For Each ati In lati
                If ati.ID = "Win32" Then
                    proc.ApplicationDefinition.ApplicationInfo = ati
                    Exit For
                End If
            Next
            Assert.That(proc.ApplicationDefinition.ApplicationInfo, [Is].Not.Null)

            For Each ati In proc.ApplicationDefinition.ApplicationInfo.SubTypes
                If ati.ID = "Win32Launch" Then
                    proc.ApplicationDefinition.ApplicationInfo = ati
                    Exit For
                End If
            Next
            For Each p As clsApplicationParameter In proc.ApplicationDefinition.ApplicationInfo.Parameters
                Select Case p.Name
                    Case "Path"
                        p.Value = "C:\Windows\System32\calc.exe"
                    Case "CommandLineParams"
                        p.Value = ""
                    Case "NonInvasive"
                        p.Value = "True"
                End Select
            Next
            proc.ApplicationDefinition.RootApplicationElement = New clsApplicationElement("Calculator")
            proc.ApplicationDefinition.RootApplicationElement.Type = clsAMI.GetElementTypeInfo("Application")

            'Add some elements to the application definition...
            button1 = New clsApplicationElement("1 Button")
            button1.Type = clsAMI.GetElementTypeInfo("Button")
            button1.Attributes.Add(New clsApplicationAttribute("WindowText", New clsProcessValue(DataType.text, "1"), True))
            Dim grp1 As New clsApplicationElement("calc buttons")
            grp1.AddMember(button1)
            button1_7 = New clsApplicationElement("1 Button Win7")
            button1_7.Type = clsAMI.GetElementTypeInfo("Button")
            button1.Attributes.Add(New clsApplicationAttribute("ClassName", New clsProcessValue(DataType.text, "Button"), True))
            button1_7.Attributes.Add(New clsApplicationAttribute("X", New clsProcessValue(0), True))
            button1_7.Attributes.Add(New clsApplicationAttribute("Y", New clsProcessValue(128), True))
            grp1.AddMember(button1_7)
            proc.ApplicationDefinition.RootApplicationElement.AddMember(grp1)
            calcmainwindow = New clsApplicationElement("Main Window")
            calcmainwindow.Type = clsAMI.GetElementTypeInfo("Window")
            calcmainwindow.Attributes.Add(New clsApplicationAttribute("WindowText", New clsProcessValue(DataType.text, "Calculator"), True))
            proc.ApplicationDefinition.RootApplicationElement.AddMember(calcmainwindow)
        End If

        'Link start and end on the init (i.e. main) page...
        stage1 = proc.GetStageByTypeAndSubSheet(StageTypes.Start, Guid.Empty)
        stage2 = proc.GetStageByTypeAndSubSheet(StageTypes.End, Guid.Empty)
        LinkStages(stage1, stage2, proc)

        'Add the cleanup page...
        Dim sheet As clsProcessSubSheet = proc.AddSubSheet("Clean Up")
        sheet.Published = True
        sheet.SheetType = SubsheetType.CleanUp

        'Link start and end on the cleanup page...
        stage1 = proc.GetStageByTypeAndSubSheet(StageTypes.Start, sheet.ID)
        stage2 = proc.GetStageByTypeAndSubSheet(StageTypes.End, sheet.ID)
        LinkStages(stage1, stage2, proc)

        Dim step1 As clsNavigateStep
        Dim ns As clsNavigateStage

        'Add Launch action, only if this is the parent or a standalone version...
        If modelFrom Is Nothing Then

            sheet = proc.AddSubSheet("Launch")
            sheet.Published = True
            sheet.SheetType = SubsheetType.Normal

            'Create a navigate stage on the action page...
            stage1 = proc.AddStage(StageTypes.Navigate, "Start App")
            stage1.SetSubSheetID(sheet.ID)
            stage1.SetDisplayX(15)
            stage1.SetDisplayY(-75)
            ns = CType(stage1, clsNavigateStage)
            step1 = New clsNavigateStep(ns)
            step1.Action = clsAMI.GetActionTypeInfo("Launch")
            step1.ElementId = proc.ApplicationDefinition.RootApplicationElement.ID
            ns.Steps.Add(step1)

            'Link start and 'Start App'....
            stage2 = proc.GetStageByTypeAndSubSheet(StageTypes.Start, sheet.ID)
            LinkStages(stage2, stage1, proc)
            'Link 'Start App' and end...
            stage2 = proc.GetStageByTypeAndSubSheet(StageTypes.End, sheet.ID)
            LinkStages(stage1, stage2, proc)

        End If

        'Add an action...
        sheet = proc.AddSubSheet("Test Action")
        sheet.Published = True
        sheet.SheetType = SubsheetType.Normal

        'Create a wait start stage...
        Dim gWaitGroup As Guid = Guid.NewGuid()
        Dim objWaitStartStage As clsWaitStartStage
        objWaitStartStage = CType(proc.AddStage(StageTypes.WaitStart, "Wait 1"), clsWaitStartStage)
        objWaitStartStage.SetDisplayX(15)
        objWaitStartStage.SetDisplayY(-45)
        objWaitStartStage.SetSubSheetID(sheet.ID)
        objWaitStartStage.SetGroupID(gWaitGroup)

        'Add an option...
        Dim ch As New clsWaitChoice(objWaitStartStage)
        ch.Name = "Calc Ready"
        ch.ElementID = calcmainwindow.ID
        For Each ct As clsConditionTypeInfo In clsAMI.GetAllowedConditions(clsAMI.GetElementTypeInfo("Window"), Nothing)
            If ct.ID = "CheckExists" Then
                ch.Condition = ct
                Exit For
            End If
        Next
        ch.ExpectedReply = "True"
        objWaitStartStage.Choices.Add(ch)
        objWaitStartStage.Timeout = "20"

        'And a corresponding wait end stage...
        Dim objWaitEndStage As clsWaitEndStage
        objWaitEndStage = CType(proc.AddStage(StageTypes.WaitEnd, "Wait end 1"), clsWaitEndStage)
        objWaitEndStage.SetDisplayX(15)
        objWaitEndStage.SetDisplayY(15)
        objWaitEndStage.SetSubSheetID(sheet.ID)
        objWaitEndStage.SetGroupID(gWaitGroup)
        stage2 = proc.GetStageByTypeAndSubSheet(StageTypes.End, sheet.ID)
        objWaitEndStage.OnSuccess = stage2.GetStageID

        'Decision for Win7 or not
        Dim win7Stage As clsProcessStage
        win7Stage = proc.AddStage(StageTypes.Decision, "Windows 7?")
        win7Stage.SetSubSheetID(sheet.ID)
        win7Stage.SetDisplayX(100)
        win7Stage.SetDisplayY(0)
        CType(win7Stage, clsDecisionStage).Expression =
         BPExpression.FromNormalised("GetOSVersionMajor()>=6 AND GetOSVersionMinor()>=1")

        'Create a button-pressing stage
        Dim objPressStage As clsProcessStage
        objPressStage = proc.AddStage(StageTypes.Navigate, "Press Button")
        objPressStage.SetSubSheetID(sheet.ID)
        objPressStage.SetDisplayX(120)
        objPressStage.SetDisplayY(0)
        ns = CType(objPressStage, clsNavigateStage)
        step1 = New clsNavigateStep(ns)
        step1.Action = clsAMI.GetActionTypeInfo("Press")
        step1.ElementId = button1.ID
        ns.Steps.Add(step1)

        'Create a button-pressing stage for Windows 7
        Dim objPressStage7 As clsProcessStage
        objPressStage7 = proc.AddStage(StageTypes.Navigate, "Press Button (win7)")
        objPressStage7.SetSubSheetID(sheet.ID)
        objPressStage7.SetDisplayX(115)
        objPressStage7.SetDisplayY(0)
        ns = CType(objPressStage7, clsNavigateStage)
        step1 = New clsNavigateStep(ns)
        step1.Action = clsAMI.GetActionTypeInfo("Press")
        step1.ElementId = button1_7.ID
        ns.Steps.Add(step1)

        'Create a close stage
        Dim objCloseStage As clsProcessStage
        objCloseStage = proc.AddStage(StageTypes.Navigate, "Close Window")
        objCloseStage.SetSubSheetID(sheet.ID)
        objCloseStage.SetDisplayX(120)
        objCloseStage.SetDisplayY(60)
        ns = CType(objCloseStage, clsNavigateStage)
        step1 = New clsNavigateStep(ns)
        step1.Action = clsAMI.GetActionTypeInfo("CloseWindow")
        step1.ElementId = calcmainwindow.ID
        ns.Steps.Add(step1)

        'Link start and 'Wait'...
        stage2 = proc.GetStageByTypeAndSubSheet(StageTypes.Start, sheet.ID)
        LinkStages(stage2, objWaitStartStage, proc)
        'Link successful wait to 'Windows 7?'....
        ch.LinkTo = win7Stage.GetStageID()
        'Link decision to correct button press...
        CType(win7Stage, clsDecisionStage).OnTrue = objPressStage7.GetStageID()
        CType(win7Stage, clsDecisionStage).OnFalse = objPressStage.GetStageID()
        'Link 'Press Button' and 'Close'...
        LinkStages(objPressStage7, objCloseStage, proc)
        LinkStages(objPressStage, objCloseStage, proc)
        'Link 'Close' and end...
        stage2 = proc.GetStageByTypeAndSubSheet(StageTypes.End, sheet.ID)
        LinkStages(objCloseStage, stage2, proc)

        'Add an input and output parameter to the action page...
        stage1 = proc.GetStageByTypeAndSubSheet(StageTypes.Start, sheet.ID)
        stage1.AddParameter(ParamDirection.In, DataType.text, "Text To Enter", "", MapType.Stage, "Txt")
        'The output just mirrors the input...
        stage1 = proc.GetStageByTypeAndSubSheet(StageTypes.End, sheet.ID)
        stage1.AddParameter(ParamDirection.Out, DataType.text, "Text Entered", "", MapType.Stage, "Txt")

        'Create data item to store input parameter, which will also be passed back
        'out as the output parameter, just to test the parameter passing...
        stage1 = proc.AddStage(StageTypes.Data, "Txt")
        stage1.SetSubSheetID(sheet.ID)
        stage1.SetDisplayX(0)
        stage1.SetDisplayY(120)
        CType(stage1, clsDataStage).IsPrivate = True
        CType(stage1, clsDataStage).SetDataType(DataType.text)
        CType(stage1, clsDataStage).SetInitialValue(New clsProcessValue(DataType.text, ""))

        Return proc
    End Function

    ''' <summary>
    ''' A simple mock up of IGroupObjectDetails.
    ''' </summary>
    Private Class clsMockGroupObjectDetails : Implements IGroupObjectDetails
        ReadOnly Property Children As IList(Of IObjectDetails) = New List(Of IObjectDetails) Implements IGroupObjectDetails.Children
        Public Property FriendlyName As String Implements IObjectDetails.FriendlyName
    End Class

    ''' <summary>
    ''' Programatically build a process which makes use of the Business Object(s)
    ''' created in CreateTestBusinessObject()
    ''' </summary>
    ''' <param name="procName">The name for the process.</param>
    ''' <param name="objectName">The name of the Business Object to use.</param>
    ''' <param name="parentObjectName">The name of the parent Business Object to use,
    ''' or Nothing if there isn't one. If there is, the parent object contains the
    ''' Launch stage.</param>
    ''' <returns>The created process, or Nothing if a failure occurred</returns>
    Private Function CreateTestObjectProcess(ByVal procName As String, ByVal objectName As String, ByVal parentObjectName As String) As clsProcess

        Dim objEndStage As clsProcessStage
        Dim objStartStage As clsProcessStage
        Dim objAction0Stage As clsProcessStage
        Dim objAction1Stage As clsProcessStage
        Dim sngX As Single, sngY As Single

        Dim ei As New clsMockGroupObjectDetails
        Dim vbo As clsVBODetails
        If parentObjectName IsNot Nothing Then
            vbo = New clsVBODetails()
            vbo.ID = gTestChildObjectID
            vbo.FriendlyName = objectName
            ei.Children.Add(vbo)
            vbo = New clsVBODetails()
            vbo.ID = gTestParentObjectID
            vbo.FriendlyName = parentObjectName
            ei.Children.Add(vbo)
        Else
            vbo = New clsVBODetails()
            vbo.ID = gTestObjectID
            vbo.FriendlyName = objectName
            ei.Children.Add(vbo)
        End If

        Dim proc As New clsProcess(ei, DiagramType.Process, True)
        proc.Name = procName

        'Get the start stage...
        objStartStage = proc.GetStage(proc.GetStartStage())
        sngX = objStartStage.GetDisplayX
        sngY = objStartStage.GetDisplayY

        'Action stage #0...
        'This calls the Launch action, which will be on the parent object if
        'there is one.
        objAction0Stage = proc.AddStage(StageTypes.Action, "action0")
        sngY += 60
        objAction0Stage.SetDisplayX(sngX)
        objAction0Stage.SetDisplayY(sngY)

        Dim launchobj As String
        If parentObjectName Is Nothing Then
            launchobj = objectName
        Else
            launchobj = parentObjectName
        End If
        CType(objAction0Stage, Stages.clsActionStage).SetResource(launchobj, "Launch")

        'Action stage #1...
        objAction1Stage = proc.AddStage(StageTypes.Action, "action1")
        sngY += 60
        objAction1Stage.SetDisplayX(sngX)
        objAction1Stage.SetDisplayY(sngY)

        CType(objAction1Stage, Stages.clsActionStage).SetResource(objectName, "Test Action")
        objAction1Stage.AddParameter(ParamDirection.In, DataType.text, "Text To Enter", "", MapType.Expr, """12345""")
        objAction1Stage.AddParameter(ParamDirection.Out, DataType.text, "Text Entered", "", MapType.Stage, "data_outparam")

        'Create an end stage...
        objEndStage = proc.AddStage(StageTypes.End, "end1")
        sngY += 60
        objEndStage.SetDisplayX(sngX)
        objEndStage.SetDisplayY(sngY)
        'Link the stages so we have a valid process...
        LinkStages(objStartStage, objAction0Stage, proc)
        LinkStages(objAction0Stage, objAction1Stage, proc)
        LinkStages(objAction1Stage, objEndStage, proc)

        'Create data item to store output parameter from test action...
        Dim objStage1 As clsProcessStage
        objStage1 = proc.AddStage(StageTypes.Data, "data_outparam")
        objStage1.SetDisplayX(120)
        objStage1.SetDisplayY(120)
        CType(objStage1, clsDataStage).IsPrivate = True
        CType(objStage1, clsDataStage).SetDataType(DataType.text)
        CType(objStage1, clsDataStage).SetInitialValue(New clsProcessValue(DataType.text, ""))

        Return proc

    End Function

    ''' <summary>
    ''' Runs the supplied process from start to end, expecting is to succeed.
    ''' </summary>
    ''' <param name="objProcess">The process to run.</param>
    Private Sub RunProcess(ByVal objProcess As clsProcess)

        Dim res As StageResult

        res = objProcess.RunAction(ProcessRunAction.Go)
        Assert.That(res.Success, "Failed to start running process - " & res.GetText())

        Do While objProcess.RunState = ProcessRunState.Running
            res = objProcess.RunAction(ProcessRunAction.RunNextStep)
            Assert.That(res.Success, "RunStep reported an error - " & res.GetText())
        Loop

        Assert.That(objProcess.RunState, [Is].EqualTo(ProcessRunState.Completed))
    End Sub

    <Test()> _
    Public Sub TestDependencies()

        clsAPC.ProcessLoader = mProcessLoader
        clsAPC.ObjectLoader = mObjectLoader

        'Create objects and process
        Dim parentObject As clsProcess = CreateTestBusinessObject("Test Business Object Parent", Nothing, Nothing)
        mProcessLoader.ProcXML(gTestParentObjectID) = parentObject.GenerateXML()
        Dim childObject As clsProcess = CreateTestBusinessObject("Test Business Object Child", "Test Business Object Parent", parentObject)
        mProcessLoader.ProcXML(gTestChildObjectID) = childObject.GenerateXML()
        Dim testProc As clsProcess = CreateTestObjectProcess("Test Process PC", "Test Business Object Child", "Test Business Object Parent")

        Dim deps As clsProcessDependencyList = testProc.GetDependencies(False)
        Assert.That(deps.Dependencies.Count, [Is].EqualTo(4))

        childObject.Dispose()
        parentObject.Dispose()
        testProc.Dispose()

    End Sub

End Class

#End If
