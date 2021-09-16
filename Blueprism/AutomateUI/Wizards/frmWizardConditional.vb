''' <summary>
''' Enhances the basic wizard to allow for sequences which
''' require each step to condition on the information given in
''' the last step.
''' </summary>
Friend Class frmWizardConditional
    Inherits frmWizard

    ''' <summary>
    ''' A stack containing all pages viewed, including the current one.
    ''' Hence, this stack should always contain at least one member,
    ''' following the first invocation of ShowPage.
    ''' </summary>
    Private mBackwardPageStack As New Generic.Stack(Of WizardStep)

    ''' <summary>
    ''' The forward stack of pages. This is only ever populated when
    ''' the user uses the back button. 
    ''' </summary>
    ''' <remarks>If after moving forwards, the
    ''' forward stack is no longer relevant (because the information in
    ''' the current step indicates that a different path should be taken),
    ''' then the forward stack is cleared.</remarks>
    Private mForwardPageStack As New Generic.Stack(Of WizardStep)


    Protected Overrides Sub BackPage()
        If Me.mBackwardPageStack.Count > 1 Then
            Me.mForwardPageStack.Push(Me.mBackwardPageStack.Pop)
            Me.ShowStep(Me.mBackwardPageStack.Pop) 'Pop it here because it gets pushed back on
        End If

        Me.UpdateNavigation()
    End Sub

    Protected Overrides Sub NextPage()
        'The backward stack contains the current page
        Dim CurrentPage As WizardStep = mBackwardPageStack.Peek

        'Postprocess the current page
        Dim sErr As String = Nothing
        If Not CurrentPage.RunPostProcessStep.Invoke(CurrentPage.Page, sErr) Then
            UserMessage.Show(sErr)
            Exit Sub
        End If

        sErr = Nothing
        Dim NextPage As WizardStep = CurrentPage.GetNextStep(sErr)
        If NextPage IsNot Nothing Then
            'See if the page we are moving to coincides with the next
            'page on the forward stack. If not then clear the stack,
            'should it be non-empty
            If Me.mForwardPageStack.Count > 0 Then
                If NextPage Is mForwardPageStack.Peek Then
                    mForwardPageStack.Pop()
                Else
                    mForwardPageStack.Clear()
                End If
            End If

            Me.ShowStep(NextPage)
        Else
            'If sErr is non blank then an error occurred, otherwise
            'we are at the last step in the wizard
            If Not String.IsNullOrEmpty(sErr) Then
                sErr = My.Resources.frmWizardConditional_InternalErrorCouldNotResolveNextStep
                UserMessage.Show(sErr)
            Else
                Me.Close()
            End If
        End If
    End Sub

    Protected Overrides Sub UpdateNavigation()
        Me.btnBack.Enabled = Me.mBackwardPageStack.Count > 1 'Current page resides on backward stack, so 1 here (not zero)
        Me.btnNext.Enabled = True
    End Sub

    Protected Overrides Sub ShowPage(ByVal objPage As System.Windows.Forms.Panel)
        Throw New InvalidOperationException(My.Resources.frmWizardConditional_DoNotCallShowPageButUseShowStepInstead)
    End Sub

    ''' <summary>
    ''' Shows the supplied step as the current step of the wizard.
    ''' </summary>
    ''' <param name="NewStep">The new step to be shown.</param>
    Protected Sub ShowStep(ByVal NewStep As WizardStep)
        MyBase.ShowPage(NewStep.Page)
        Me.mBackwardPageStack.Push(NewStep) 'The backward stack always includes the current page
        Me.UpdateNavigation()

        'we do the pre-processing after the control has been added,
        'because this is important for some control (eg when selecting textboxes).
        'It comes after updatenavigation, because some pages might want to control
        'the availability of the back/forward buttons
        If NewStep.RunPreProcessStep IsNot Nothing Then
            Dim sErr As String = Nothing
            If Not NewStep.RunPreProcessStep.Invoke(NewStep.Page, sErr) Then
                UserMessage.Show(sErr)
            End If
        End If
    End Sub


    Public Overrides Function GetHelpFile() As String
        Dim CurrentStep As WizardStep = Me.mBackwardPageStack.Peek
        If CurrentStep IsNot Nothing Then
            Return CurrentStep.HelpReference
        Else
            Return String.Empty
        End If
    End Function

    ''' <summary>
    ''' Represents a step in the wizard, complete with links
    ''' to the next possible step, based on the information in
    ''' the current step.
    ''' </summary>
    Protected Class WizardStep
        ''' <summary>
        ''' The panel to be displayed during this wizard step.
        ''' </summary>
        ''' <remarks>Must be suitable to supply as a parameter to
        ''' <see cref="ShowPage">ShowPage</see>.</remarks>
        Public ReadOnly Page As Panel

        ''' <summary>
        ''' Reference to the function which will decide the next step,
        ''' based on the information supplied by the user in the current step.
        ''' </summary>
        ''' <remarks>Must not be null.</remarks>
        Private CalculateOutcome As OutComeCalculator

        ''' <summary>
        ''' Signature required for functions determining the outcome of this step.
        ''' </summary>
        ''' <param name="P">The parameter of the current step, from which the next
        ''' step must be calculated.</param>
        ''' <returns>Returns the index of the next step, as found in the
        ''' member <see cref="ForwardSteps">ForwardSteps</see>; or -1 if this is
        ''' to be the last step; or -2 if there is an error (in which case the
        ''' sErr parameter will carry back more information).</returns>
        ''' <param name="sErr">Carries back a message in the event of an error.
        ''' Relevant only when the return value is -2.</param>
        ''' <remarks></remarks>
        Public Delegate Function OutComeCalculator(ByVal P As Panel, ByRef sErr As String) As Integer

        ''' <summary>
        ''' Reference to the function which will do post-processing on this step,
        ''' where required.
        ''' </summary>
        ''' <remarks>This member may be a null reference if no processing is
        ''' required.</remarks>
        Public RunPostProcessStep As ProcessingStep


        ''' <summary>
        ''' Reference to the function which will do pre-processing on this step,
        ''' where required.
        ''' </summary>
        ''' <remarks>This member may be a null reference if no processing is
        ''' required.</remarks>
        Public RunPreProcessStep As ProcessingStep

        ''' <summary>
        ''' Signature definition for functions which do pre- or post-processing on
        ''' a step in the wizard.
        ''' </summary>
        ''' <param name="P">The panel to which this processing relates.</param>
        ''' <param name="sErr">Carries back a message in the event of an error.</param>
        ''' <returns>Returns true on success, false otherwise.</returns>
        Public Delegate Function ProcessingStep(ByVal P As Panel, ByRef sErr As String) As Boolean

        ''' <summary>
        ''' Calculates the next step of the wizard, based on the information
        ''' in the current step.
        ''' </summary>
        ''' <returns>Returns a reference to the next step to be displayed,
        ''' or a null reference if either this is to be the last step, or
        ''' if progress can not continue due to an error. To distinguish
        ''' between these two latter possibilities, check for a message
        ''' in the sErr parameter.</returns>
        Public Function GetNextStep(ByRef sErr As String) As WizardStep
            Dim Index As Integer = CalculateOutcome.Invoke(Me.Page, sErr)
            Select Case Index
                Case -2, -1
                    Return Nothing
                Case Else
                    Return Me.ForwardSteps(Index)
            End Select
        End Function

        ''' <summary>
        ''' The possible steps available from here. Which step we move to
        ''' depends on the choices made in this step. The outcome of this conditioning
        ''' is determined by the invocation of the member
        ''' <see cref="CalculateOutcome">CalculateOutcome</see>.
        ''' </summary>
        ''' <remarks>This member is never null, but may be empty.
        ''' When empty, this step automatically becomes the last one.</remarks>
        Public ForwardSteps As New Generic.List(Of WizardStep)

        ''' <summary>
        ''' A reference to a help file, in the html help.
        ''' </summary>
        Public ReadOnly HelpReference As String

        Public Sub New(ByVal P As Panel, ByVal Calculator As OutComeCalculator, ByVal PreProcessor As ProcessingStep, ByVal PostProcessor As ProcessingStep, Optional ByVal HelpReference As String = "")
            Me.Page = P
            Me.CalculateOutcome = Calculator
            Me.RunPostProcessStep = PostProcessor
            Me.RunPreProcessStep = PreProcessor
            Me.HelpReference = HelpReference
        End Sub
    End Class

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmWizardConditional))
        Me.SuspendLayout()
        '
        'frmWizardConditional
        '
        resources.ApplyResources(Me, My.Resources.frmWizardConditional_This)
        Me.Name = "frmWizardConditional"
        Me.ResumeLayout(False)

    End Sub
End Class
