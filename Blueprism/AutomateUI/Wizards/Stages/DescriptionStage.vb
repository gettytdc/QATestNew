''' <summary>
''' A stage, ie. a single part of a wizard, represented by a control which sits in
''' the wizard.
''' </summary>
Public Class DescriptionStage : Inherits WizardStage

    ''' <summary>
    ''' The ID of this stage
    ''' </summary>
    Public Const StageId As String = "description-stage"

    ' The description currently held in this stage.
    Private mDescription As String

    ' The prompt to request the description - ie. the label on the stage.
    Private mPrompt As String

    ''' <summary>
    ''' Creates a new description stage.
    ''' </summary>
    Public Sub New(ByVal thing As String)
        Me.New(My.Resources.DescriptionStage_Description, String.Format(My.Resources.DescriptionStage_PleaseEnterADescriptionForThe0, thing))
    End Sub

    ''' <summary>
    ''' Creates a new description stage with the given title and subtitle
    ''' </summary>
    ''' <param name="title">The title for this stage, displayed in the wizard.
    ''' </param>
    ''' <param name="subtitle">The subtitle for this stage, displayed in the wizard
    ''' below the title.</param>
    Public Sub New(ByVal title As String, ByVal subtitle As String)
        MyBase.New(title, subtitle)
    End Sub

    ''' <summary>
    ''' The unique ID for this stage.
    ''' </summary>
    Public Overrides ReadOnly Property Id() As String
        Get
            If StageId Is Nothing Then Throw New NotImplementedException(My.Resources.DescriptionStage_StageIDNotSet)
            Return StageId
        End Get
    End Property

    ''' <summary>
    ''' Creates the control which displays or modifies the contents of this stage
    ''' </summary>
    ''' <returns>The control used to display and modify this stage's contents.
    ''' </returns>
    Protected Overrides Function CreateControl() As ctlWizardStageControl
        Dim d As New ctlDescriber()
        d.Text = mDescription
        If mPrompt IsNot Nothing Then d.Prompt = mPrompt
        Return d
    End Function

    ''' <summary>
    ''' The description modelled in this stage.
    ''' </summary>
    Public Property Description() As String
        Get
            If mControl IsNot Nothing Then mDescription = mControl.Text
            Return mDescription
        End Get
        Set(ByVal value As String)
            mDescription = value
            If mControl IsNot Nothing Then mControl.Text = value
        End Set
    End Property

    ''' <summary>
    ''' The backing control as a Describer control
    ''' </summary>
    Private ReadOnly Property Describer() As ctlDescriber
        Get
            Return DirectCast(mControl, ctlDescriber)
        End Get
    End Property


    ''' <summary>
    ''' The prompt to display to the user asking them to enter a description. This
    ''' is typically a label in the stage proper.
    ''' </summary>
    Public Property Prompt() As String
        Get
            If Describer IsNot Nothing Then mPrompt = Describer.Prompt
            Return mPrompt
        End Get
        Set(ByVal value As String)
            mPrompt = value
            If Describer IsNot Nothing Then Describer.Prompt = value
        End Set
    End Property

    ''' <summary>
    ''' Handles this stage being committed by saving the name to the member variable
    ''' so that the control can be disposed of.
    ''' </summary>
    Protected Overrides Sub OnCommitted(ByVal e As StageCommittedEventArgs)
        mDescription = mControl.Text
        MyBase.OnCommitted(e)
    End Sub

End Class
