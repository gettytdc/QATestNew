

''' <summary>
''' Wizard stage which accepts a name
''' </summary>
Public Class NameStage : Inherits WizardStage

    ''' <summary>
    ''' The ID of this stage.
    ''' </summary>
    Public Const StageId As String = "name-stage"

    ' The name held by this stage
    Private mName As String

    ''' <summary>
    ''' Creates a new name stage for naming the specified thing
    ''' </summary>
    ''' <param name="thing">The thing that is being named. The subtitle will be set
    ''' as "Please choose a name for the {0}" where {0} represents the 'thing'.
    ''' </param>
    Public Sub New(ByVal thing As String)
        MyBase.New(My.Resources.NameStage_Name, String.Format(My.Resources.NameStage_PleaseEnterANameForThe0, thing))
    End Sub

    ''' <summary>
    ''' Creates the control which represents this stage.
    ''' </summary>
    ''' <returns>The control for this stage.</returns>
    Protected Overrides Function CreateControl() As ctlWizardStageControl
        Dim nameCtl As New ctlNamer()
        nameCtl.Text = mName
        Return nameCtl
    End Function

    ''' <summary>
    ''' The ID of this stage
    ''' </summary>
    Public Overrides ReadOnly Property Id() As String
        Get
            Return StageId
        End Get
    End Property

    ''' <summary>
    ''' The name represented by this stage.
    ''' </summary>
    Public Property Name() As String
        Get
            If mControl IsNot Nothing Then mName = mControl.Text
            Return mName.Trim()
        End Get
        Set(ByVal value As String)
            mName = value
            If mControl IsNot Nothing Then mControl.Text = value
        End Set
    End Property

    ''' <summary>
    ''' Handles the committing of this stage, ensuring that a name has been entered.
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overrides Sub OnCommitting(ByVal e As StageCommittingEventArgs)
        If Me.Name.Length = 0 Then
            UserMessage.Show(My.Resources.NameStage_YouMustProvideAName)
            e.Cancel = True
        Else
            MyBase.OnCommitting(e)
        End If
    End Sub

    ''' <summary>
    ''' Handles this stage being committed by saving the name to the member variable
    ''' so that the control can be disposed of.
    ''' </summary>
    Protected Overrides Sub OnCommitted(ByVal e As StageCommittedEventArgs)
        mName = mControl.Text
        MyBase.OnCommitted(e)
    End Sub

End Class

