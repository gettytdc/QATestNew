''' <summary>
''' A stage, ie. a single part of a wizard, represented by a control which sits in
''' the wizard.
''' </summary>
Public Class MessageStage : Inherits WizardStage

#Region " Constants "

    ''' <summary>
    ''' The ID of this stage
    ''' </summary>
    Public Const StageId As String = "message.stage"

#End Region

#Region " Member Variables "

    ' The text being displayed in the message
    Private mText As String

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new message stage with no message, and the default title
    ''' ("Message") and no subtitle.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new message stage with the given message.
    ''' By default, the title of the stage will be "Message"; there will be no
    ''' subtitle.
    ''' </summary>
    ''' <param name="msg">The message to display.</param>
    Public Sub New(ByVal msg As String)
        Me.New(My.Resources.MessageStage_Message, Nothing, msg)
    End Sub

    ''' <summary>
    ''' Creates a new message stage with the given parameters.
    ''' </summary>
    ''' <param name="title">The title of the stage to use.</param>
    ''' <param name="subtitle">The subtitle of the stage to use - null if no subtitle
    ''' is required.</param>
    ''' <param name="msg">The message to display</param>
    Public Sub New(ByVal title As String, ByVal subtitle As String, ByVal msg As String)
        MyBase.New(title, subtitle)
        mText = msg
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The unique ID for this stage.
    ''' </summary>
    Public Overrides ReadOnly Property Id() As String
        Get
            If StageId Is Nothing Then Throw New NotImplementedException(My.Resources.MessageStage_StageIDNotSet)
            Return StageId
        End Get
    End Property

    ''' <summary>
    ''' The text of the message
    ''' </summary>
    Public Property Text() As String
        Get
            Return mText
        End Get
        Set(ByVal value As String)
            mText = value
            If mControl IsNot Nothing Then mControl.Text = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Creates the control which displays or modifies the contents of this stage
    ''' </summary>
    ''' <returns>The control used to display and modify this stage's contents.
    ''' </returns>
    Protected Overrides Function CreateControl() As ctlWizardStageControl
        Dim msg As New ctlMessage()
        If mText <> Nothing Then msg.Text = mText
        Return msg
    End Function

#End Region

End Class
