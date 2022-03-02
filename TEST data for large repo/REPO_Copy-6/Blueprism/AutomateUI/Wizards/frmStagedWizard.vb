''' <summary>
''' Wizard form which supports stages which can be separated from the wizard proper
''' and used on multiple wizards.
''' </summary>
Friend Class frmStagedWizard
    Inherits frmWizard

    ' Flag indicating if this wizard has 'completed' or not.
    Private mCompleted As Boolean

    ' The list of stages used by this wizard
    Private mStages As IList(Of WizardStage)

    ' The map of stages against their names
    Private mStageLookup As IDictionary(Of String, WizardStage)

    ' The currently displaying stage, if any
    Private WithEvents mCurrentStage As WizardStage

    ' The next stage to move to after the next button is pressed.
    Private mQueuedStage As WizardStage

    ' The stack of stages navigated to in this wizard, to enable proper back button
    ' handling.
    Private mStageStack As Stack(Of WizardStage)

    ''' <summary>
    ''' Empty constructor, largely here for the designer.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, New WizardStage() {})
    End Sub

    ''' <summary>
    ''' Creates a new wizard with the given title. The title is set as the
    ''' text property of this form.
    ''' </summary>
    ''' <param name="wizardTitle">The title of the wizard to display in the form
    ''' title bar. If this is empty the title "Wizard" is used.</param>
    ''' <param name="stages">The stages used in this wizard</param>
    Protected Sub New(ByVal wizardTitle As String, ByVal stages As IList(Of WizardStage))
        mStageStack = New Stack(Of WizardStage)
        mStages = stages
        mStageLookup = New Dictionary(Of String, WizardStage)
        For Each stg As WizardStage In stages
            mStageLookup(stg.Id) = stg
        Next

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        ' Allow resize... the restriction is wholly fascist
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable

        ' Default, kinda rubbish, title if one isn't given
        If String.IsNullOrEmpty(wizardTitle) Then wizardTitle = My.Resources.frmStagedWizard_Wizard
        Text = wizardTitle
        SetMaxSteps(stages.Count - 1)

    End Sub

    ''' <summary>
    ''' Flag indicating if this wizard has 'completed' or not. If it has, then
    ''' the dialog result is set to <see cref="DialogResult.OK"/> even if the
    ''' user clicks the close button or presses Alt+F4.
    ''' </summary>
    Public Property Completed() As Boolean
        Get
            Return mCompleted
        End Get
        Protected Set(ByVal value As Boolean)
            mCompleted = value
        End Set
    End Property

    ''' <summary>
    ''' Handles the closing of this wizard. This checks whether the wizard has
    ''' completed or not - if it has, it ensures that the dialog result is set to
    ''' be 'OK' indicating that the work has been done.
    ''' </summary>
    Protected Overrides Sub OnClosing(ByVal e As CancelEventArgs)
        If Me.Completed Then Me.DialogResult = DialogResult.OK
        MyBase.OnClosing(e)
    End Sub

    ''' <summary>
    ''' Gets the stage corresponding to the given name held in this wizard.
    ''' </summary>
    ''' <param name="name">The name of the stage required.</param>
    ''' <returns>The stage corresponding to the given name, or null if no such stage
    ''' was found in this wizard.</returns>
    Friend Function GetStage(ByVal name As String) As WizardStage
        Dim stg As WizardStage = Nothing
        mStageLookup.TryGetValue(name, stg)
        Return stg
    End Function

    ''' <summary>
    ''' Gets the stage corresponding to the given index.
    ''' </summary>
    ''' <param name="index">The index of the required stage.</param>
    ''' <returns>The stage corresponding to the given index, or null if the index
    ''' was too large for the stages represented in this wizard.</returns>
    Friend Function GetStage(ByVal index As Integer) As WizardStage
        If index >= mStages.Count Then Return Nothing
        Return mStages(index)
    End Function

    ''' <summary>
    ''' Sets the current control on the wizard to the specified control
    ''' </summary>
    ''' <param name="ctl">The control to fill this wizard with.</param>
    Protected Sub SetControl(ByVal ctl As ctlWizardStageControl)
        Dim curr As Control = Nothing
        If panContents.Controls.Count > 0 Then curr = panContents.Controls(0)
        If Object.ReferenceEquals(ctl, curr) Then Return
        panContents.Controls.Clear()
        ctl.Dock = DockStyle.Fill
        panContents.Controls.Add(ctl)
        ctl.GetFirstFocus()
    End Sub

    ''' <summary>
    ''' Sets the current stage to the given one, storing the current stage into the
    ''' history of this wizard.
    ''' </summary>
    ''' <param name="stg">The stage to set in this wizard</param>
    Protected Sub SetStage(ByVal stg As WizardStage)
        SetStage(stg, True)
    End Sub

    ''' <summary>
    ''' Sets the current stage to the given one, either storing or omitting the
    ''' current stage from the history as required.
    ''' </summary>
    ''' <param name="stg">The stage to set in this wizard</param>
    ''' <param name="storeCurrentInHistory">True to store the current stage in the
    ''' history, such that the back button would revert to the current stage. This
    ''' would typically need to be true when progressing through the wizard via the
    ''' Next button, and False when returning through the history via the Back button
    ''' </param>
    Protected Overridable Sub SetStage(ByVal stg As WizardStage, ByVal storeCurrentInHistory As Boolean)
        If mCurrentStage Is stg Then Return
        Me.Title = stg.Title
        SetControl(stg.Control)
        If mCurrentStage IsNot Nothing Then mCurrentStage.Control = Nothing
        ' Store the current stage into the history of the wizard if appropriate.
        If storeCurrentInHistory _
         AndAlso mCurrentStage IsNot Nothing AndAlso mCurrentStage.CanSaveToHistory Then
            If mStageStack.Contains(stg) Then
                ' If we already have this stage in our history, rewind to it.
                Do
                    mStageStack.Pop()
                Loop While mStageStack.Contains(stg)
            Else
                ' Otherwise, store the current stage and move to the specified stage
                mStageStack.Push(mCurrentStage)
            End If
            'mStageStack.Push(mCurrentStage)
        End If
        mCurrentStage = stg
        miStep = mStages.IndexOf(stg)
        ' It would be nice to just call UpdateNavigation() for this, but it calls
        ' UpdatePage(), which calls this method, so we can't
        btnBack.Enabled = (mStageStack.Count > 0)
        ' Next and cancel are always enabled on each stage - they can be overridden
        ' if necessary by overriding the OnInitStage() event handler method
        btnNext.Enabled = True
        btnCancel.Enabled = True
        UpdateNextButtonText()

        OnInitStage(stg)
    End Sub

    ''' <summary>
    ''' Commits the current stage of the wizard and, if successful, moves onto the
    ''' next stage.
    ''' </summary>
    Protected Overrides Sub NextPage()
        If CommitStage(mCurrentStage) Then
            If CurrentStep = miMaxSteps Then
                Me.DialogResult = DialogResult.OK
                Me.Close()

            ElseIf mQueuedStage IsNot Nothing Then
                ' Get it and clear it immediately
                Dim stg As WizardStage = mQueuedStage
                mQueuedStage = Nothing

                ' Take the saved stage and move to it
                CurrentStep = mStages.IndexOf(stg)

            Else
                Dim nextStep As Integer = miStep
                Dim args As WizardSteppingEventArgs
                Do
                    nextStep += 1
                    args = New WizardSteppingEventArgs(Me, nextStep)
                    OnSteppingNext(args)

                Loop While args.Skip AndAlso nextStep < miMaxSteps

                miStep = nextStep
                UpdateNextButtonText()
                UpdatePage()

            End If
        End If
    End Sub

    ''' <summary>
    ''' Event handler for stepping onto the next stage.
    ''' Subclasses can indicate that stages should be skipped over by setting the
    ''' <see cref="WizardSteppingEventArgs.Skip"/> property of the supplied args
    ''' to <c>True</c>
    ''' </summary>
    ''' <param name="e">The args detailing the stage which is being stepped to.
    ''' If, after this method returns, the <c>Skip</c> property of the args is set
    ''' to true, the stage will be skipped and the subsequent stage will be stepped
    ''' to instead. This remains true up to the last stage, which is always shown,
    ''' regardless of the resultant Skip value.
    ''' </param>
    Protected Overridable Sub OnSteppingNext(ByVal e As WizardSteppingEventArgs)
    End Sub

    ''' <summary>
    ''' Goes back one page.
    ''' </summary>
    Protected Overrides Sub BackPage()
        mQueuedStage = Nothing
        If mStageStack.Count > 0 Then
            SetStage(mStageStack.Pop(), False)
        Else
            MyBase.BackPage()
        End If
    End Sub

    ''' <summary>
    ''' Commits the current stage of the wizard and, if successful, moves onto the
    ''' specified stage.
    ''' </summary>
    ''' <param name="stg">The stage to go to</param>
    Protected Sub CommitAndGotoStage(ByVal stg As WizardStage)
        If CommitStage(mCurrentStage) Then
            CurrentStep = mStages.IndexOf(stg)
        End If
    End Sub

    ''' <summary>
    ''' Gets or sets the next stage to move to when the next button is pressed.
    ''' This is cleared if the back button is pressed.
    ''' </summary>
    Protected Property QueuedStage() As WizardStage
        Get
            Return mQueuedStage
        End Get
        Set(ByVal value As WizardStage)
            mQueuedStage = value
        End Set
    End Property

    ''' <summary>
    ''' Updates the page, setting the stage to the current step.
    ''' </summary>
    Protected Overrides Sub UpdatePage()
        SetStage(mStages(CurrentStep))
    End Sub

    ''' <summary>
    ''' Handles the stage being activated - effectively this emulates a next button
    ''' click to go through the validation / committal and progression of the stage
    ''' </summary>
    Private Sub HandleStageActivated(ByVal sender As WizardStage, ByVal e As ActivationEventArgs) _
     Handles mCurrentStage.Activated
        btnNext.PerformClick()
    End Sub

    ''' <summary>
    ''' Handles the title or subtitle being changed on the current stage.
    ''' </summary>
    Private Sub HandleTitlesChanged(ByVal sender As WizardStage, ByVal e As EventArgs) _
     Handles mCurrentStage.TitlesChanged
        Me.Title = sender.Title
    End Sub

    ''' <summary>
    ''' Initialises the given stage. This ensures that the Back and Cancel buttons
    ''' are disabled if the current step is the last step, then tells the stage to
    ''' initialise itself.
    ''' </summary>
    ''' <param name="stg">The stage to initialise</param>
    Protected Overridable Sub OnInitStage(ByVal stg As WizardStage)
        ' Disable the back / cancel buttons on the last stage
        If CurrentStep = miFirstStep Then
            btnBack.Enabled = False

        ElseIf CurrentStep = miMaxSteps Then
            btnBack.Enabled = False
            btnCancel.Enabled = False

        Else
            btnBack.Enabled = True
            btnCancel.Enabled = True

        End If
        stg.Init(Me)
    End Sub

    ''' <summary>
    ''' Attempts to commit the given stage.
    ''' </summary>
    ''' <param name="stg">The stage to commit</param>
    ''' <returns>True if the stage committed successfully, False if it failed to
    ''' commit for some reason (validation error or such like)</returns>
    Protected Overridable Function CommitStage(ByVal stg As WizardStage) As Boolean
        If stg Is Nothing Then Return False
        Return stg.TryCommit()
    End Function

    ''' <summary>
    ''' Disposes of this wizard, ensuring that all contained stages are disposed of
    ''' too.
    ''' </summary>
    ''' <param name="disposing">True to indicate that this is an explicit dispose
    ''' call. False to indicate that disposes is being called as part of object
    ''' finalization.</param>
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        ' Required by Windows Form Designer
        If disposing Then
            If components IsNot Nothing Then components.Dispose()
            If mStages IsNot Nothing Then
                For Each stg As WizardStage In mStages
                    stg.Dispose()
                Next
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

End Class



