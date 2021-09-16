Imports BluePrism.AutomateAppCore.Utility
Imports AutomateUI.My.Resources
Imports BluePrism.AutomateAppCore

''' <summary>
''' UI wizard for guiding the user through the steps when
''' unlocking a process (and possibly opening an autosaved
''' version). Use as follows:
''' 
''' Launch the wizard, providing a user ID.
''' 
''' Check the dialogresult. A result of OK indicates
''' that the user has chosen to do something. A result
''' of cancel indicates that the user has cancelled the
''' wizard without choosing a course of action.
''' 
''' Check the 
''' </summary>
Friend Class frmAutosavePrompt
    Inherits frmWizard

    Public Sub New()
        MyBase.New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()
    End Sub

    ''' <summary>
    ''' Indicates whether the process of interest is locked.
    ''' </summary>
    Private mbProcessIsLocked As Boolean

    ''' <summary>
    ''' The ID of the process.
    ''' </summary>
    Private mgProcessID As Guid

    Public Sub New(ByVal ProcessID As Guid, ByVal WizardType As WizardType)
        MyBase.New(WizardType)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        'Set here for the convenience of the layout used in the designer
        Dim sz As New Size(600, 400)
        MyBase.Size = sz
        MyBase.MinimumSize = sz
        MyBase.SetMaxSteps(1)
        Me.rdoOpenUnsavedVersion.Checked = True

        Me.mgProcessID = ProcessID
        Dim lockUsername As String = Nothing
        Dim lockMachineName As String = Nothing
        Me.mbProcessIsLocked = gSv.ProcessIsLocked(mgProcessID, lockUsername, lockMachineName)
        Select Case WizardType
            Case WizardType.BusinessObject 
                Label1.Text = frmAutoSavePrompt_ObjectRemoveTheLockNow
                Label2.Text = frmautoSavePrompt_ObjectAnotherUserIsEditing
                Label3.Text = String.Format(frmAutoSavePrompt_SelectedObjectLockedForEditingByUSer0On1, lockUsername, lockMachineName)
                rdoUnlockProcess.Text = frmAutoSavePrompt_UnlockTheBusinessObjectAndstartEditingIt
                rdoViewProcess.Text = frmautoSavePrompt_LeaveTheLockInPlaceAndViewItheBusinessObjectInstead
                lblIgnoreUnsavedHints.Text = frmAutoSavePrompt_ChosingThisOptionWillNotDiscardTheAutosavedWorkUntilYouNextSaveTheBusinessObject
                lblInitialMessage.Text = frmAutoSavePrompt_ObjectUnsavedWork
                Text = frmAutosavePrompt_ObjectTitle
            Case WizardType.Process
                Label3.Text = String.Format(frmAutosavePrompt_SelectedProcessLockedForEditingByUser0On1, lockUsername, lockMachineName)
        End Select
        Me.objBluebar.Title = frmAutosavePrompt_RecoverUnsavedWorkFollowingASystemErrorSuchAsAPowerFailure
    End Sub

    Protected Overrides Sub UpdatePage()
        Select Case MyBase.GetStep
            Case 0
                'Which initial page to show? Unlocking or Process recovery?
                If Me.mbProcessIsLocked Then

                    Me.objBluebar.Title = My.Resources.frmAutosavePrompt_ProcessBusinessObjectIsLocked

                    MyBase.ShowPage(pnlUnlock)
                    Me.rdoUnlockProcess.Checked = True
                Else
                    'No need to unlock process - move on to process recovery
                    If gSv.AutoSaveBackupSessionExistsForProcess(mgProcessID) Then
                        MyBase.ShowPage(pnlRecover)
                        MyBase.SetStep(1)
                    Else
                        'No unlocking and no process recovery? Wizard should not have been shown
                        Me.mChosenOutcome = OutComes.EditProcess
                        Me.DialogResult = System.Windows.Forms.DialogResult.OK
                    End If
                End If

            Case 1
                Me.objBluebar.Title = My.Resources.frmAutosavePrompt_RecoverUnsavedWorkFollowingASystemErrorSuchAsAPowerFailure

                'Take action following unlocking stage
                Select Case True
                    Case Me.rdoUnlockProcess.Checked
                        Me.mbUnlockRequested = True
                        If gSv.AutoSaveBackupSessionExistsForProcess(mgProcessID) Then
                            MyBase.ShowPage(pnlRecover)
                        Else
                            Me.mChosenOutcome = OutComes.EditProcess
                            Me.DialogResult = System.Windows.Forms.DialogResult.OK
                        End If
                    Case Me.rdoViewProcess.Checked
                        Me.mChosenOutcome = OutComes.ViewProcess
                        Me.DialogResult = System.Windows.Forms.DialogResult.OK
                    Case Else
                        MyBase.Rollback(My.Resources.frmAutosavePrompt_PleaseChooseOneOfTheAvailableOptions)
                        Exit Sub
                End Select

            Case 2
                Me.objBluebar.Title = My.Resources.frmAutosavePrompt_RecoverUnsavedWorkFollowingASystemErrorSuchAsAPowerFailure

                'Take action following process recovery stage
                Select Case True
                    Case Me.rdoCompare.Checked
                        Me.mChosenOutcome = OutComes.CompareAutosaveVersions
                        Me.DialogResult = System.Windows.Forms.DialogResult.OK
                    Case Me.rdoOpenUnsavedVersion.Checked
                        Me.mChosenOutcome = OutComes.EditAutosaveVersion
                        Me.DialogResult = System.Windows.Forms.DialogResult.OK
                    Case Me.rdoOpenOriginalVersion.Checked
                        Me.mChosenOutcome = OutComes.EditOriginalVersion
                        Me.DialogResult = System.Windows.Forms.DialogResult.OK
                    Case False
                        MyBase.Rollback(My.Resources.frmAutosavePrompt_PleaseChooseOneOfTheAvailableOptions)
                        Exit Sub
                End Select
        End Select
    End Sub

    Public Overrides Function GetHelpFile() As String
        Select Case True
            Case MyBase.CurrentPage Is Me.pnlUnlock
                Return "frmSystemUnlock.htm"
            Case MyBase.CurrentPage Is Me.pnlRecover
                Return "helpAutosave.htm"
            Case Else
                Return ""
        End Select
    End Function

    ''' <summary>
    ''' Opens the help file whether online or offline.
    ''' </summary>
    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    ''' <summary>
    ''' Outcomes of the wizard - what the user wants to do.
    ''' </summary>
    Public Enum OutComes
        ''' <summary>
        ''' Default value - the user has not chosen a specific
        ''' action, (or the wizard is not yet complete).
        ''' </summary>
        None
        ''' <summary>
        ''' The user wants to edit it the process (used in conjunction
        ''' with unlocking).
        ''' </summary>
        EditProcess
        ''' <summary>
        ''' The user has chosen not to to view the process in readonly mode.
        ''' </summary>
        ViewProcess
        ''' <summary>
        ''' The user wishes to compare the original
        ''' vs autosaved versions.
        ''' </summary>
        CompareAutosaveVersions
        ''' <summary>
        ''' The user wants to edit the autosaved
        ''' version.
        ''' </summary>
        EditAutosaveVersion
        ''' <summary>
        ''' An autosaved version of the process exists. The user wants to edit the original
        ''' version of the process.
        ''' </summary>
        EditOriginalVersion
    End Enum

    ''' <summary>
    ''' Private member to store public property ChosenOutcome
    ''' </summary>
    Private mChosenOutcome As OutComes
    ''' <summary>
    ''' The desired action, as chosen by the user. Valid after
    ''' a dialogresult of OK.
    ''' </summary>
    Public ReadOnly Property ChosenOutcome() As OutComes
        Get
            Return mChosenOutcome
        End Get
    End Property

    ''' <summary>
    ''' Private member to store public property UnlockRequested.
    ''' </summary>
    Private mbUnlockRequested As Boolean = False
    ''' <summary>
    ''' Determines whether unlocking of the process is necessary.
    ''' </summary>
    ''' <remarks>The unlocking is deferred until after the wizard is
    ''' complete, in case the user cancels.</remarks>
    Public ReadOnly Property UnlockRequested() As Boolean
        Get
            Return mbUnlockRequested
        End Get
    End Property

End Class