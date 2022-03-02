Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.BPCoreLib

''' Project  : Automate
''' Class    : frmPromptToSave
''' 
''' <summary>
''' A form prompting the user to save a process. Also ridiculously doubles as
''' a form for prompting the user for a reason for deleting a process. And, as
''' if that wasn't ridiculous enough, a form for setting an exception message
''' for work queue items.
''' </summary>
Friend Class frmPromptToSave : Inherits frmForm
    Implements IEnvironmentColourManager

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.BlueBar.Title = My.Resources.frmPromptToSave_SaveYourChangesWithAnAccompanyingComment

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents btnSave As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnCancel As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnNoSave As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents txtSummary As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents lblSummary As System.Windows.Forms.Label
    Friend WithEvents lblUserPrompt As System.Windows.Forms.Label
    Friend WithEvents btnValidate As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents BlueBar As AutomateControls.TitleBar
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPromptToSave))
        Me.txtSummary = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnSave = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnCancel = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.lblSummary = New System.Windows.Forms.Label()
        Me.btnNoSave = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.lblUserPrompt = New System.Windows.Forms.Label()
        Me.BlueBar = New AutomateControls.TitleBar()
        Me.btnValidate = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.SuspendLayout()
        '
        'txtSummary
        '
        Me.txtSummary.AcceptsReturn = True
        resources.ApplyResources(Me.txtSummary, "txtSummary")
        Me.txtSummary.Name = "txtSummary"
        '
        'btnSave
        '
        resources.ApplyResources(Me.btnSave, "btnSave")
        Me.btnSave.Image = Global.AutomateUI.My.Resources.ToolImages.Save_16x16
        Me.btnSave.Name = "btnSave"
        Me.btnSave.UseVisualStyleBackColor = False
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        Me.btnCancel.Name = "btnCancel"
        Me.btnCancel.UseVisualStyleBackColor = False
        '
        'lblSummary
        '
        resources.ApplyResources(Me.lblSummary, "lblSummary")
        Me.lblSummary.Name = "lblSummary"
        '
        'btnNoSave
        '
        resources.ApplyResources(Me.btnNoSave, "btnNoSave")
        Me.btnNoSave.Name = "btnNoSave"
        Me.btnNoSave.UseVisualStyleBackColor = False
        '
        'lblUserPrompt
        '
        resources.ApplyResources(Me.lblUserPrompt, "lblUserPrompt")
        Me.lblUserPrompt.Name = "lblUserPrompt"
        '
        'BlueBar
        '
        resources.ApplyResources(Me.BlueBar, "BlueBar")
        Me.BlueBar.Name = "BlueBar"
        Me.BlueBar.TabStop = False
        '
        'btnValidate
        '
        resources.ApplyResources(Me.btnValidate, "btnValidate")
        Me.btnValidate.Image = Global.AutomateUI.My.Resources.ToolImages.Notebook_Tick_16x16
        Me.btnValidate.Name = "btnValidate"
        Me.btnValidate.UseVisualStyleBackColor = False
        '
        'frmPromptToSave
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.btnValidate)
        Me.Controls.Add(Me.BlueBar)
        Me.Controls.Add(Me.lblUserPrompt)
        Me.Controls.Add(Me.btnNoSave)
        Me.Controls.Add(Me.btnCancel)
        Me.Controls.Add(Me.btnSave)
        Me.Controls.Add(Me.txtSummary)
        Me.Controls.Add(Me.lblSummary)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D
        Me.Name = "frmPromptToSave"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    ''' <summary>
    ''' The result of the user's interaction with the form. Possible values
    ''' are cancel, yes or no.
    ''' </summary>
    Public Result As MsgBoxResult = MsgBoxResult.Cancel


    ''' <summary>
    ''' Specifies whether the user should be forced to submit an edit
    ''' summary.
    ''' </summary>
    Public mbSummaryCompulsory As Boolean

    ''' <summary>
    ''' The message that shall be displayed to the user by default.
    ''' </summary>
    ''' <value></value>
    Public Shared ReadOnly Property DefaultMessage() As String
        Get
            Return My.Resources.frmPromptToSave_YouHaveNotYetSavedYourChangesToThisProcessWouldYouLikeToSaveYourChangesBeforeEx
        End Get
    End Property

    Private Sub btnSave_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSave.Click
        If mbSummaryCompulsory AndAlso txtSummary.Text.Trim().Length = 0 Then
            UserMessage.Show(Me, String.Format(
             My.Resources.frmPromptToSave_Your0AdministratorHasRequestedThatAllChangesToProcessesBeAccompaniedByAnEditSum, ApplicationProperties.ApplicationName))
        Else
            Result = MsgBoxResult.Yes
            Close()
        End If
    End Sub

    ''' <summary>
    ''' Handles this form being closed - this ensures that the owner form is
    ''' activated when this form closes - it was previously bringing up another
    ''' window instead (see bug 5770 pt 3).
    ''' Possibly related to .net issue: http://support.microsoft.com/kb/905719
    ''' </summary>
    Protected Overrides Sub OnClosed(ByVal e As System.EventArgs)
        MyBase.OnClosed(e)
        Dim f As Form = Me.Owner
        If f IsNot Nothing Then f.Activate()
    End Sub

    Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCancel.Click
        Result = MsgBoxResult.Cancel
        Me.Close()
    End Sub

    Private Sub btnNoSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNoSave.Click
        Result = MsgBoxResult.No
        Me.Close()
    End Sub

    Private Sub btnProcessValidation_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnValidate.Click
        Dim p As frmProcess = TryCast(Me.Owner, frmProcess)
        If p IsNot Nothing Then p.ShowProcessValidation()
        Me.Close()
    End Sub

    Private Sub frmPromptToSave_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        txtSummary.Select()
    End Sub

    ''' <summary>
    ''' Creates an instance of the form, using the supplied UI information.
    ''' </summary>
    ''' <param name="ShowSummary">Boolean value determining whether or not the
    ''' summary field should be displayed.</param>
    ''' <param name="ShowBlueBar">A flag to display the blue header on the form</param>
    ''' <param name="SummaryIsCompulsory">Boolean value determining whether the user
    ''' is permitted to enter a blank summary.</param>
    ''' <param name="ShowNoSaveButton">Boolean value determining whether or not the
    ''' "don't save" button should be displayed.</param>
    ''' <param name="sMessage">A message to relace the default message presented to
    ''' the user.</param>
    ''' <param name="sbtnYesText">A string to replace the default text on the yes
    ''' button.</param>
    ''' <param name="sbtnNoText">A string to replace the default text on the no
    ''' button.</param>
    ''' <param name="sbtnCancelText">A string to replace the default text on the
    ''' cancel button.</param>
    ''' <returns>Returns an instance of frmPromptToSave, using the supplied UI
    ''' parameters.</returns>
    Private Shared Function CreateForm(ByVal ShowSummary As Boolean, ByVal ShowBlueBar As Boolean, ByVal SummaryIsCompulsory As Boolean, ByVal ShowNoSaveButton As Boolean, ByVal sFormTitle As String, ByVal sBlueBarTitle As String, ByVal sMessage As String, ByVal SummaryBoxLabel As String, ByVal sbtnYesText As String, ByVal sbtnNoText As String, ByVal sbtnCancelText As String, Optional ByVal bUseBusinessObjects As Boolean = False) As frmPromptToSave

        Dim f As New frmPromptToSave()
        f.Result = MsgBoxResult.Cancel

        f.Text = sFormTitle
        f.CenterToScreen()

        If Not sMessage = "" Then f.lblUserPrompt.Text = sMessage
        If Not sbtnYesText = "" Then f.btnSave.Text = sbtnYesText
        If Not sbtnNoText = "" Then f.btnNoSave.Text = sbtnNoText
        If Not sbtnCancelText = "" Then f.btnCancel.Text = sbtnCancelText
        If Not String.IsNullOrEmpty(SummaryBoxLabel) Then f.lblSummary.Text = SummaryBoxLabel

        If Not ShowBlueBar Then
            f.Controls.Remove(f.BlueBar)
            f.Height -= f.BlueBar.Height
            For Each c As Control In f.Controls
                If Not TypeOf c Is Button Then c.Top -= f.BlueBar.Height
            Next
        Else
            If Not sBlueBarTitle = "" Then f.BlueBar.Title = sBlueBarTitle
        End If

        If Not ShowSummary Then
            f.txtSummary.Hide()
            f.lblSummary.Hide()
            f.Height -= f.txtSummary.Top + f.txtSummary.Height - f.lblSummary.Top
        End If

        If Not ShowNoSaveButton Then
            f.btnNoSave.Hide()
        End If

        f.mbSummaryCompulsory = SummaryIsCompulsory

        If bUseBusinessObjects Then
            f.btnCancel.Text = My.Resources.ReturnToObjectStudio
            f.lblUserPrompt.Text = f.lblUserPrompt.Text.Replace("process", "business object")
            f.BlueBar.Title = f.BlueBar.Title.Replace(My.Resources.frmPromptToSave_Process1, My.Resources.frmPromptToSave_BusinessObject1)
        Else
            f.btnCancel.Text = My.Resources.ReturnToProcessStudio
            f.lblUserPrompt.Text = f.lblUserPrompt.Text.Replace("business object", "process")
            f.BlueBar.Title = f.BlueBar.Title.Replace(My.Resources.frmPromptToSave_BusinessObject1, My.Resources.frmPromptToSave_Process1)
        End If

        'adjust layout to suit size of contents
        With f
            'set label height
            .lblSummary.MaximumSize = New Size(.txtSummary.Width, Integer.MaxValue)
            Dim layoutsize As SizeF = New SizeF(.lblUserPrompt.Width, 5000.0)
            Dim g As Graphics = Graphics.FromHwnd(.lblUserPrompt.Handle)
            Dim StringSize As SizeF = g.MeasureString(.lblUserPrompt.Text, .lblUserPrompt.Font, layoutsize)
            g.Dispose()
            .lblUserPrompt.Height = CInt(Math.Ceiling(StringSize.Height))

            'now adjust everything else to suit
            .lblSummary.Top = .lblUserPrompt.Bottom + 8
            Dim Offset As Integer = .txtSummary.Top - .lblSummary.Bottom
            f.txtSummary.Top -= Offset
            .txtSummary.Height += Offset
            .lblSummary.BringToFront()
        End With

        Return f
    End Function

    ''' <summary>
    ''' Show the form and wait for a response.
    ''' </summary>
    ''' <param name="ShowSummary">Boolean value determining whether or not the
    ''' summary field should be displayed.</param>
    ''' <param name="ShowBlueBar">A flag to display the blue header on the form</param>
    ''' <param name="SummaryIsCompulsory">Boolean value determining whether the user
    ''' is permitted to enter a blank summary.</param>
    ''' <param name="ShowNoSaveButton">Boolean value determining wether or not the
    ''' "don't save" button should be displayed.</param>
    ''' <param name="Summary">A string in which the summary input by the user will
    ''' be stored.</param>
    ''' <param name="sMessage">A message to relace the default message presented to
    ''' the user.</param>
    ''' <param name="sbtnYesText">A string to replace the default text on the yes
    ''' button.</param>
    ''' <param name="sbtnNoText">A string to replace the default text on the no
    ''' button.</param>
    ''' <param name="sbtnCancelText">A string to replace the default text on the
    ''' cancel button.</param>
    ''' <returns>Returns a MsgBoxResult indicating which option was selected,
    ''' either Yes, No or Cancel.</returns>
    Private Shared Function ShowForm(ByVal owner As Control, ByVal ShowSummary As Boolean, ByVal ShowBlueBar As Boolean, ByVal SummaryIsCompulsory As Boolean, ByVal ShowNoSaveButton As Boolean, ByRef Summary As String, ByVal sFormTitle As String, ByVal sBlueBarTitle As String, ByVal sMessage As String, ByVal SummaryBoxLabel As String, ByVal sbtnYesText As String, ByVal sbtnNoText As String, ByVal sbtnCancelText As String, Optional ByVal bUseBusinessObjects As Boolean = False) As MsgBoxResult
        Dim f As frmPromptToSave = CreateForm(ShowSummary, ShowBlueBar, SummaryIsCompulsory, ShowNoSaveButton, sFormTitle, sBlueBarTitle, sMessage, SummaryBoxLabel, sbtnYesText, sbtnNoText, sbtnCancelText, bUseBusinessObjects)
        f.SetEnvironmentColoursFromAncestor(owner)
        Try
            f.BringToFront()
            f.ShowInTaskbar = False
            f.ShowDialog(owner)
        Finally
            f.Dispose()
        End Try

        Summary = f.txtSummary.Text
        Return f.Result
    End Function

    ''' <summary>
    ''' Show the form and wait for a response.
    ''' </summary>
    ''' <param name="ShowSummary">Boolean value determining whether or not the
    ''' summary field should be displayed.</param>
    ''' <param name="ShowBlueBar">A flag to display the blue header on the form</param>
    ''' <param name="SummaryIsCompulsory">Boolean value determining whether the user
    ''' is permitted to enter a blank summary.</param>
    ''' <param name="ShowNoSaveButton">Boolean value determining wether or not the
    ''' "don't save" button should be displayed.</param>
    ''' <param name="Summary">A string in which the summary input by the user will
    ''' be stored.</param>
    ''' <param name="sFormTitle">The form title</param>
    ''' <param name="sMessage">A message to relace the default message presented to
    ''' the user.</param>
    ''' <returns>Returns a MsgBoxResult indicating which option was selected,
    ''' either Yes, No or Cancel.</returns>
    Public Shared Function ShowForm(ByVal owner As Control, ByVal ShowSummary As Boolean, ByVal ShowBlueBar As Boolean, ByVal SummaryIsCompulsory As Boolean, ByVal ShowNoSaveButton As Boolean, ByRef Summary As String, ByVal sFormTitle As String, ByVal sMessage As String, Optional ByVal bUseBusinessObjects As Boolean = False) As MsgBoxResult
        Return ShowForm(owner, ShowSummary, ShowBlueBar, SummaryIsCompulsory, ShowNoSaveButton, Summary, sFormTitle, "", sMessage, "", "", "", "", bUseBusinessObjects)
    End Function

    Public Property EnvironmentBackColor As Color Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return BlueBar.BackColor
        End Get
        Set(value As Color)
            BlueBar.BackColor = value
        End Set
    End Property

    Public Property EnvironmentForeColor As Color Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return BlueBar.TitleColor
        End Get
        Set(value As Color)
            BlueBar.TitleColor = value
        End Set
    End Property
End Class
