Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth

''' Project  : Automate
''' Class    : clsAutomateProcessDebugHook
''' 
''' <summary>
''' Provides the debug hook implementation required by APC for the main Automate
''' application.
''' </summary>
Public Class clsAutomateProcessDebugHook
    Inherits clsProcessDebugHook

    ''' <summary>
    ''' This property is queried by APC to determine whether to use the ProcessDebugHook
    ''' method or not.
    ''' </summary>
    ''' <returns>True if ProcessDebugHook should be called when relevant</returns>
    Public Overrides ReadOnly Property UseDebugHook() As Boolean
        Get
            'We won't process these is there is no main form - we're not an interactive
            'instance...
            If Not IsInteractive Then Return False

            'We also should be logged in.
            If Not User.LoggedIn Then Return False

            'If we passed all the previous tests, we should be ok...
            Return True
        End Get
    End Property

    ''' <summary>
    ''' This is called during execution when a subprocess (only a Process, not a
    ''' Business Object) is about to be prepared for execution. It gives the
    ''' application the opportunity to provide a clsProcess object for the subprocess
    ''' instead of letting APC load and create one.
    ''' If a process is provided, it must be reset to its starting state.
    ''' </summary>
    ''' <param name="procid">The ID of the process about to be created.</param>
    ''' <returns>A clsProcess object that APC should use for execution, or Nothing
    ''' if one is not available.</returns>
    Public Overrides Function GetDebugSubprocess(ByVal procid As Guid) As clsProcess

        'Try and find an already-open Process Studio form that contains the requested
        'process...
        Dim procform As frmProcess = frmProcess.GetInstance(procid)
        If procform IsNot Nothing Then
            Return procform.mProcessViewer.Process
        End If
        Return Nothing

    End Function

    ''' <summary>
    ''' This is called during execution when a sub-process (either a real 'Process'
    ''' or a Business Object of course) is about to be executed.
    ''' </summary>
    ''' <param name="proc">The process to be executed.</param>
    ''' <param name="gProcessID">The local process ID of this process</param>
    ''' <param name="parentProcess">The parent process.</param>
    ''' <param name="lastmod">The date/time the process was loaded.</param>
    ''' <param name="sErr">In the event of failure, contains an error description
    ''' </param>
    ''' <returns>True if successful, False otherwise</returns>
    Public Overrides Function ProcessDebugHook(ByVal proc As clsProcess, ByVal gProcessID As Guid, ByVal parentProcess As clsProcess, ByVal lastmod As DateTime, ByRef sErr As String) As Boolean

        Dim procform As frmProcess

        'First, see if there is already a form displaying this process instance. If
        'so, we must have handed it to APC via GetDebugSubprocess.
        procform = frmProcess.GetInstance(proc)
        If procform IsNot Nothing Then

            'There is an existing form. It might need to be reset...
            proc.RunAction(ProcessRunAction.Reset)

        Else

            'There is no existing form, so we will need to create one in an
            'appropriate mode...

            Dim mode As ProcessViewMode
            Dim bCanEdit As Boolean
            Dim bCanView As Boolean

            Dim lockUserName As String = Nothing
            Dim lockMachineName As String = Nothing
            Dim isLocked As Boolean = gSv.ProcessIsLocked(gProcessID, lockUserName, lockMachineName)
            Dim lastModifiedInDB As DateTime = gSv.GetProcessLastModified(gProcessID)

            Dim modified As Boolean = lastmod <> lastModifiedInDB

            Dim isBusinessObjectProcessType = (proc.ProcessType = DiagramType.Object)

            Dim impliedEditObject = If(isBusinessObjectProcessType, Permission.ObjectStudio.ImpliedEditBusinessObject,
                                                                    Permission.ProcessStudio.ImpliedEditProcess)

            Dim impliedView = If(isBusinessObjectProcessType, Permission.ObjectStudio.ImpliedViewBusinessObject,
                                                              Permission.ProcessStudio.ImpliedViewProcess)
            
            Dim currentProcessGroupPermissions = gSv.GetEffectiveGroupPermissionsForProcess(gProcessID)
            bCanEdit = (Not isLocked) AndAlso (Not modified) AndAlso currentProcessGroupPermissions.HasPermission(User.Current, impliedEditObject)
            bCanView = currentProcessGroupPermissions.HasPermission(User.Current, impliedView)

            If bCanEdit Then
                If isBusinessObjectProcessType Then
                    mode = ProcessViewMode.EditObject
                Else
                    mode = ProcessViewMode.EditProcess
                End If
            ElseIf bCanView Then
                If isBusinessObjectProcessType Then
                    mode = ProcessViewMode.AdHocTestObject
                Else
                    mode = ProcessViewMode.AdHocTestProcess
                End If
            Else
                sErr = My.Resources.NotAuthorisedToDebug
                Return False
            End If

            'Create the child form in the appropriate mode
            procform = New frmProcess(mode, proc, gProcessID)
            procform.mProcessViewer.OpenedAsDebugSubProcess = True

            'Find reference to parent form, calling this subprocess
            Dim parentform As frmProcess = frmProcess.GetInstance(parentProcess)
            If parentform Is Nothing Then
                sErr = My.Resources.FailedToFindReferenceToParentForm
                Return False
            End If

            'Make the child form pop up on top of the parent one, in the same position
            If Not parentform Is Nothing Then
                procform.StartPosition = FormStartPosition.Manual
                procform.Location = parentform.Location
                procform.Size = parentform.Size
            End If

            gMainForm.StartForm(procform)
        End If

        procform.Activate()

        'Finally, load the parent app model Id and focus the current debug stage within the window.
        'Since ctlProcessViewer.Startup is only performed on the load event (why?)
        'we have to wait until after the form is loaded before we can safely
        'do this
        procform.mProcessViewer.GetParentApplicationModelId()
        procform.mProcessViewer.ShowStage(proc.GetStage(proc.RunStageID), False)

        Return True

    End Function

    ''' <summary>
    ''' Called when debugging has returned from a child process.
    ''' </summary>
    ''' <param name="proc">The child process that was executed.</param>
    ''' <param name="parentProc">The parent process now being returned to.</param>
    ''' <returns>True if the application has taken (or retained) ownership of the
    ''' child process object. It can only do this for a Process. Not, currently, for
    ''' a Business Object. False if it thinks APC owns it.</returns>
    Public Overrides Function ProcessDebugReturn(ByVal proc As clsProcess, ByVal parentProc As clsProcess) As Boolean

        Dim owned As Boolean = False

        Dim procform As frmProcess = frmProcess.GetInstance(proc)
        If procform IsNot Nothing Then
            If procform.mProcessViewer.OpenedAsDebugSubProcess Then
                While True
                    If procform.mProcessViewer.PromptToSave() Then
                        procform.Close()
                        Exit While
                    Else
                        If proc.ProcessType = DiagramType.Process Then
                            owned = True
                            Exit While
                        End If
                    End If
                    UserMessage.Show(My.Resources.YouMustEitherSaveOrDiscardTheChangesWhenYouHaveModifiedABusinessObjectThatYouSt)
                End While
            End If
        End If

        'Re-focus the parent...
        procform = frmProcess.GetInstance(parentProc)
        If procform IsNot Nothing Then procform.Activate()

        Return owned
    End Function

End Class
