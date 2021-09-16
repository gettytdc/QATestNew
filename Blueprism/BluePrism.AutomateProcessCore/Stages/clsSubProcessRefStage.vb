Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore.Processes

Namespace Stages

    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsSubProcessRefStage
    ''' 
    ''' <summary>
    ''' The sub process reference stage holds a reference to another process.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsSubProcessRefStage
        Inherits clsLinkableStage

        ''' <summary>
        ''' Specifies which external process or subsheet the stage refers to
        ''' </summary>
        <DataMember>
        Private mReferenceId As Guid

        ''' <summary>
        ''' The ID of the target that this stage refers to.
        ''' Since clsSubSheetRefStage extends this, it's left as an exercise for the
        ''' caller to determine whether this GUID refers to a process or a subpage,
        ''' which is obviously how object oriented programming is designed to work.
        ''' </summary>
        Public Property ReferenceId() As Guid
            Get
                Return mReferenceId
            End Get
            Set(ByVal value As Guid)
                mReferenceId = value
            End Set
        End Property

        ''' <summary>
        ''' Creates a new instance of the clsSubProcessRefStage class and sets its
        ''' parent
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
            mReferenceId = Guid.Empty
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of an action stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsSubProcessRefStage(mParent)
        End Function

        ''' <summary>
        ''' Creates a deep copy of the Sub Process reference stage.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function Clone() As clsProcessStage
            Dim copy As clsSubProcessRefStage = CType(MyBase.Clone, clsSubProcessRefStage)
            copy.ReferenceId = Me.ReferenceId
            Return copy
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Process
            End Get
        End Property

        ''' <summary>
        ''' Returns items referred to by this stage, so externally defined things
        ''' (such as Processes) and when required things defined within the process
        ''' (e.g. page references).
        ''' </summary>
        ''' <param name="inclInternal">Indicates internal references required</param>
        ''' <returns>List of dependencies</returns>
        Public Overrides Function GetDependencies(inclInternal As Boolean) As List(Of clsProcessDependency)
            Dim deps As List(Of clsProcessDependency) = MyBase.GetDependencies(inclInternal)

            If StageType = StageTypes.Process AndAlso ReferenceId <> Guid.Empty Then
                deps.Add(New clsProcessIDDependency(ReferenceId))
            End If

            If inclInternal Then
                If StageType = StageTypes.SubSheet AndAlso ReferenceId <> Guid.Empty Then
                    deps.Add(New clsProcessPageDependency(ReferenceId))
                End If
            End If

            Return deps
        End Function

        Public ReadOnly Property UseDebugHook() As Boolean
            Get
                'Decide whether or not to use the debug hook, which is if it's available
                'AND if we want to use it...
                Dim bUseDebugHook As Boolean
                bUseDebugHook = clsAPC.ProcessDebugHook IsNot Nothing AndAlso clsAPC.ProcessDebugHook.UseDebugHook AndAlso mParent.RunState <> ProcessRunState.SteppingOver AndAlso mParent.RunState <> ProcessRunState.Running
                Return bUseDebugHook
            End Get
        End Property

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            Dim sErr As String = Nothing
            Dim res As StageResult
            Dim subproc As clsProcess = Nothing

            'Check we have a process loader, otherwise we can't do this action.
            If clsAPC.ProcessLoader Is Nothing Then
                Return New StageResult(False, "Internal", My.Resources.Resources.clsSubProcessRefStage_CanTRunAnExternalProcessNoProcessLoaderAvailable)
            End If

            Dim bUseDebugHook As Boolean = UseDebugHook

            If mParent.mChildWaitProcess Is Nothing Then

                'Here we create and set up our external process, and then put the
                'process into ChildWait mode.

                'Check for missing subprocess reference
                If mReferenceId = Guid.Empty Then
                    Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsSubProcessRefStage_TheProcessStage0DoesNotReferenceAValidSubprocess, GetName()))
                End If

                'check that subprocess has not been retired
                Dim attr As ProcessAttributes
                If Not clsAPC.ProcessLoader.GetProcessAtrributes(mReferenceId, attr, sErr) Then
                    Return New StageResult(False, "Internal", sErr)
                Else
                    If (attr And ProcessAttributes.Retired) <> 0 Then
                        Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsSubProcessRefStage_TheProcessStage0ReferencesASubprocessWhichHasBeenRetired, GetName()))
                    End If
                End If

                Dim bProcExternal As Boolean = False
                Try

                    Dim loadedDate As DateTime

                    subproc = GetCalleeProcess(bUseDebugHook, loadedDate, bProcExternal, sErr)
                    If subproc Is Nothing Then
                        Return New StageResult(False, "Internal", sErr)
                    End If

                    'Give it our logging engines...
                    subproc.Logger.Union(logger)

                    'Get the inputsXML for the external process
                    Dim inputs As clsArgumentList = Nothing
                    If Not mParent.ReadDataItemsFromParameters(subproc.GetStage(subproc.GetStartStage()).GetParameters, Me, inputs, sErr, True, False) Then
                        Return New StageResult(False, "Internal", sErr)
                    End If
                    subproc.SetInputParams(inputs)

                    ProcessRefPrologue(logger, inputs)

                    'If possible, hand the process over to the debug hook which will take
                    'responsibility for running it (it won't block while it does so though)
                    If bUseDebugHook Then
                        If Not clsAPC.ProcessDebugHook.ProcessDebugHook(subproc, ReferenceId, mParent, loadedDate, sErr) Then
                            Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsSubProcessRefStage_DebugHookFailed0, sErr))
                        End If
                    End If

                    'Go into 'ChildWait' mode by storing a reference to this process.
                    mParent.mChildWaitProcess = subproc
                    mParent.mChildWaitProcessOwned = Not bProcExternal
                    subproc = Nothing   'See comment below

                Finally
                    'This dispose is for the case where we failed to build and hand
                    'over the process, which is why we set objProc to Nothing when
                    'we successfully do so above.
                    If Not bProcExternal And subproc IsNot Nothing Then
                        subproc.Dispose()
                    End If
                End Try

            End If

            If Not mParent.mChildWaitProcess Is Nothing Then
                'If we're in ChildWait mode, we're waiting for a result from our
                'child process, which we created in a previous call to this method,
                'or even just now!

                subproc = mParent.mChildWaitProcess

                'If we didn't hand the process off elsewhere (e.g. the Automate debugger)
                'then we need to make it run...
                If Not bUseDebugHook Then
                    'Run (or continue running) the external process...
                    '(we are only going to run one step's worth every time it comes
                    'in here, to keep the client responsive to requests to stop etc)
                    If subproc.RunState = ProcessRunState.Off Then
                        res = subproc.RunAction(ProcessRunAction.Go)
                        If Not res.Success Then
                            Return res
                        End If
                    End If
                    If subproc.RunState <> ProcessRunState.Running Then
                        Return New StageResult(False, "Internal", My.Resources.Resources.clsSubProcessRefStage_ExternalProcessDidNotStart)
                    End If
                    res = subproc.RunAction(ProcessRunAction.RunNextStep)
                    If Not res.Success Then
                        mParent.mChildWaitProcess.Dispose()
                        mParent.mChildWaitProcess = Nothing
                        Return res
                    End If
                End If

                Select Case subproc.RunState
                    Case ProcessRunState.Completed, ProcessRunState.Aborted, ProcessRunState.Failed
                        'It's finished, one way or another...

                        If subproc.RunState = ProcessRunState.Completed Then
                            'Deal with the outputs...
                            Dim outputs As clsArgumentList
                            outputs = subproc.GetOutputs()

                            ProcessRefEpilogue(logger, outputs)

                            'Store outputs in the appropriate places...
                            If Not outputs Is Nothing Then
                                If Not mParent.StoreParametersInDataItems(Me, outputs, sErr) Then
                                    Return New StageResult(False, "Internal", sErr)
                                End If
                            End If

                            'Move on to the next stage...
                            If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then
                                Return New StageResult(False, "Internal", sErr)
                            End If

                        End If

                        If bUseDebugHook Then
                            If clsAPC.ProcessDebugHook.ProcessDebugReturn(subproc, mParent) Then
                                mParent.mChildWaitProcessOwned = False
                            End If
                        End If

                        'We're finished, so dispose of the process
                        If mParent.mChildWaitProcessOwned Then mParent.mChildWaitProcess.Dispose()
                        mParent.mChildWaitProcess = Nothing

                        If subproc.RunState = ProcessRunState.Completed Then Return New StageResult(True)
                        Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsSubProcessRefStage_ExternalProcess0, subproc.RunState.ToString()))

                    Case ProcessRunState.Running
                        'Deliberately doing nothing here

                    Case ProcessRunState.Off
                        'Deliberately doing nothing here too. This state occurs when
                        'the debugger has opened up, but the user has not stepped into
                        'the process yet. It would be tidier all round if it automatically
                        'did that for them, I will make that a...
                        'TODO: see above

                    Case Else
                        Debug.Assert(False, My.Resources.Resources.clsSubProcessRefStage_UnexpectedRunModeDuringSubprocessExecution)

                End Select

            End If

            Return New StageResult(True)

        End Function

        Private Sub ProcessRefPrologue(logger As CompoundLoggingEngine, ByVal inputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.ProcessRefProLogue(info, Me, inputs)
        End Sub

        Private Sub ProcessRefEpilogue(logger As CompoundLoggingEngine, ByVal outputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.ProcessRefEpiLogue(info, Me, outputs)
        End Sub

        Private Function GetCalleeProcess(ByVal bUseDebugHook As Boolean, ByRef loadedDate As DateTime, ByRef bProcExternal As Boolean, ByRef sErr As String) As clsProcess
            Dim objProc As clsProcess = Nothing
            If bUseDebugHook Then
                objProc = clsAPC.ProcessDebugHook.GetDebugSubprocess(mReferenceId)
            End If
            If Not objProc Is Nothing Then

                bProcExternal = True

            Else
                'Create the process...
                Dim sXML As String = Nothing
                If Not clsAPC.ProcessLoader.GetProcessXML(mReferenceId, sXML, loadedDate, sErr) Then
                    Return Nothing
                End If

                objProc = clsProcess.FromXML(mParent.ExternalObjectsInfo, sXML, mParent.Editable, sErr)
                If objProc Is Nothing Then
                    sErr = String.Format(My.Resources.Resources.clsSubProcessRefStage_ErrorLoadingExternalProcess0, sErr)
                    Return Nothing
                End If

                objProc.ParentProcess = mParent

            End If

            Return objProc
        End Function

        Public Overrides Sub ToXml(ByVal ParentDocument As System.Xml.XmlDocument, ByVal StageElement As System.Xml.XmlElement, ByVal bSelectionOnly As Boolean)
            MyBase.ToXml(ParentDocument, StageElement, bSelectionOnly)
            Dim e2 As Xml.XmlElement
            Dim objProcessRef As clsSubProcessRefStage = CType(Me, clsSubProcessRefStage)
            e2 = ParentDocument.CreateElement("processid")
            e2.AppendChild(ParentDocument.CreateTextNode(objProcessRef.ReferenceId.ToString()))
            StageElement.AppendChild(e2)
        End Sub

        Public Overrides Sub FromXML(ByVal e2 As System.Xml.XmlElement)
            MyBase.FromXML(e2)
            For Each e3 As Xml.XmlElement In e2.ChildNodes
                Select Case e3.Name
                    Case "processid"
                        ReferenceId = New Guid(e3.InnerText)
                End Select
            Next
        End Sub

        Public Overrides Function CheckForErrors(ByVal bAttemptRepair As Boolean, ByVal SkipObjects As Boolean) As ValidationErrorList
            Dim errors As ValidationErrorList = MyBase.CheckForErrors(bAttemptRepair, SkipObjects)

            'Avoid all these checks for a SubSheetRefStage, which inherits from this
            'class.
            If Not TypeOf (Me) Is clsSubSheetRefStage Then
                Dim gID As Guid = ReferenceId
                If gID.Equals(Guid.Empty) Then
                    'forgot to reference subprocess
                    errors.Add(New ValidateProcessResult(Me, "frmStagePropertiesProcess.htm", 121))
                Else
                    If Not clsAPC.ProcessLoader Is Nothing Then
                        Dim A As ProcessAttributes
                        Dim sErr As String = Nothing

                        Try
                            If clsAPC.ProcessLoader.GetProcessAtrributes(gID, A, sErr) Then
                                Dim ProcessIsRetired As Boolean = (A And ProcessAttributes.Retired) > 0
                                If ProcessIsRetired Then
                                    'referenced subprocess has been retired
                                    errors.Add(New ValidateProcessResult(Me, "helpSystemManagerProcesses.htm", 122))
                                End If
                            End If
                        Catch
                            'referenced subprocess no longer exists
                            errors.Add(New ValidateProcessResult(Me, 134))
                            Return errors
                        End Try

                        Dim loadedDate As DateTime
                        Dim bProcExternal As Boolean
                        Dim objProc As clsProcess = GetCalleeProcess(UseDebugHook, loadedDate, bProcExternal, sErr)
                        If objProc Is Nothing Then
                            errors.Add(New ValidateProcessResult(Me, 134))
                        Else
                            Dim parameters As List(Of clsProcessParameter) = objProc.GetStage(objProc.GetStartStage()).GetParameters
                            For Each mapped As clsProcessParameter In parameters
                                If GetParameter(mapped.Name, mapped.Direction) Is Nothing Then
                                    If bAttemptRepair Then
                                        AddParameter(mapped.Direction, mapped.GetDataType, mapped.Name, mapped.Narrative, MapType.None, Nothing, Nothing, mapped.FriendlyName)
                                    Else
                                        errors.Add(New RepairableValidateProcessResult(Me, 65, mapped.Name))
                                    End If
                                End If
                            Next
                        End If
                    End If
                End If

            End If

            Return errors
        End Function

    End Class

End Namespace
