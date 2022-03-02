Imports AutomateControls
Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Class to encapsulate a save form with a user prompt
''' </summary>
Friend Class frmSavePrompt
    Implements IEnvironmentColourManager

    ''' <summary>
    ''' The response returned from a save prompt
    ''' </summary>
    Public Class Response
        ' The dialog result returned from the dialog
        Private mResult As DialogResult
        ' The text entered by the user
        Private mText As String

        ''' <summary>
        ''' Creates a new response based on the given result and text.
        ''' </summary>
        ''' <param name="res">The dialog result returned by the dialog</param>
        ''' <param name="txt">The user entered text in the dialog</param>
        Public Sub New(ByVal res As DialogResult, ByVal txt As String)
            mResult = res
            mText = txt
        End Sub
        ''' <summary>
        ''' The result returned from the dialog on its closure.
        ''' </summary>
        Public ReadOnly Property Result() As DialogResult
            Get
                Return mResult
            End Get
        End Property
        ''' <summary>
        ''' The text entered by the user into the save prompt dialog
        ''' </summary>
        Public ReadOnly Property Text() As String
            Get
                Return mText
            End Get
        End Property
    End Class

    ' The button in the bottom left (often a 'Discard Changes' type button)
    Private WithEvents mLeftButton As AutomateControls.Buttons.StandardStyledButton
    ' The collection of right buttons, in left to right order
    Private mRightButtons As ICollection(Of AutomateControls.Buttons.StandardStyledButton)
    ' The current config of this form
    Private mConfig As ISavePromptConfig

    ''' <summary>
    ''' Empty constructor - only here for the forms designer.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new Save Prompt form based on the given configuration.
    ''' </summary>
    ''' <param name="cfg">The configuration from which this form's UI should be
    ''' configured.</param>
    Public Sub New(ByVal cfg As ISavePromptConfig)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Config = cfg

    End Sub

    ''' <summary>
    ''' The configuration of this form.
    ''' </summary>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)> _
    Public Property Config() As ISavePromptConfig
        Get
            Return mConfig
        End Get
        Set(ByVal cfg As ISavePromptConfig)
            mConfig = cfg
            If cfg IsNot Nothing Then
                Text = cfg.WindowTitle
                mHeading.Title = cfg.Heading
                lblUserPrompt.Text = cfg.Prompt
                LeftButton = cfg.LeftButton
                RightButtons = cfg.RightButtons
            End If
        End Set
    End Property

    ''' <summary>
    ''' The lower left button on this form.
    ''' </summary>
    Private Property LeftButton() As AutomateControls.Buttons.StandardStyledButton
        Get
            Return mLeftButton
        End Get
        Set(ByVal btn As AutomateControls.Buttons.StandardStyledButton)
            panTable.SuspendLayout()
            Try
                If mLeftButton IsNot Nothing Then panTable.Controls.Remove(mLeftButton)
                If btn IsNot Nothing Then
                    btn.Margin = New Padding(0)
                    panTable.Controls.Add(btn)
                    panTable.SetColumn(btn, 0)
                End If
                mLeftButton = btn
            Finally
                panTable.ResumeLayout()
            End Try
        End Set
    End Property

    ''' <summary>
    ''' The collection of right buttons on this form, in left to right order.
    ''' </summary>
    Private Property RightButtons() As ICollection(Of AutomateControls.Buttons.StandardStyledButton)
        Get
            If mRightButtons Is Nothing Then Return GetEmpty.ICollection(Of AutomateControls.Buttons.StandardStyledButton)()
            Return GetReadOnly.ICollection(Of AutomateControls.Buttons.StandardStyledButton)(mRightButtons)
        End Get
        Set(ByVal value As ICollection(Of AutomateControls.Buttons.StandardStyledButton))
            panFlow.SuspendLayout()
            panFlow.BackColor = Color.Transparent
            Try
                If mRightButtons Is Nothing Then
                    mRightButtons = New List(Of AutomateControls.Buttons.StandardStyledButton)
                Else
                    For Each btn As Button In mRightButtons
                        panFlow.Controls.Remove(btn)
                        RemoveHandler btn.Click, AddressOf HandleButtonClicked
                    Next
                    mRightButtons.Clear()
                End If
                For Each btn As AutomateControls.Buttons.StandardStyledButton In New clsReverseEnumerable(Of AutomateControls.Buttons.StandardStyledButton)(value)
                    btn.Margin = New Padding(6, 0, 0, 0)
                    mRightButtons.Add(btn)
                    panFlow.Controls.Add(btn)
                    AddHandler btn.Click, AddressOf HandleButtonClicked
                Next
            Finally
                panFlow.ResumeLayout()
            End Try
        End Set
    End Property

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
    Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return mHeading.BackColor
        End Get
        Set(value As Color)
            mHeading.BackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the environment-specific back colour in use in this environment.
    ''' Only set to the database-held values after login.
    ''' </summary>
    ''' <remarks>Note that this only affects the UI owned directly by this form - ie.
    ''' setting the colour here will not update the database</remarks>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return mHeading.TitleColor
        End Get
        Set(value As Color)
            mHeading.TitleColor = value
        End Set
    End Property

    ''' <summary>
    ''' Handles any of the buttons registered on this form being clicked.
    ''' </summary>
    ''' <remarks>Note that although this is only compile-time registered as handling
    ''' the left button click, it handles each of the right button clicks too - this
    ''' method is added as a handler when they are processed by the RightButtons
    ''' property.</remarks>
    Private Sub HandleButtonClicked(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mLeftButton.Click
        If mConfig Is Nothing Then Return

        Dim res As DialogResult = _
         mConfig.GetResponse(Me, DirectCast(sender, Button), txtSummary.Text)
        If res <> DialogResult.None Then Me.DialogResult = res : Close()
    End Sub

    ''' <summary>
    ''' Shows the prompt form specified by the given config and returns the user's
    ''' response after the dialog has been dismissed.
    ''' </summary>
    ''' <param name="cfg">The configuration to use for the save prompt form.</param>
    ''' <returns>The response from the user, encapsulating the dialog result and the
    ''' text entered by the user.</returns>
    Public Shared Function ShowPrompt(parentApp As frmApplication, ByVal cfg As ISavePromptConfig) As Response
        Using f As New frmSavePrompt(cfg)
            f.SetEnvironmentColoursFromAncestor(parentApp)
            f.ShowInTaskbar = False
            Dim res As DialogResult = f.ShowDialog()
            Return New Response(res, f.txtSummary.Text)
        End Using
    End Function

End Class

