Friend Class FileOrDatabaseStage : Inherits WizardStage

#Region " Enums "

    Public Enum ProcessLocationType
        File
        Database
    End Enum

#End Region

#Region " Constants "

    ''' <summary>
    ''' The ID of this stage
    ''' </summary>
    Public Const StageId As String = "fileordatabase.stage"

#End Region

#Region " Members "

    Private mProcessLocation As ProcessLocationType

#End Region

#Region " Constructors "

    Public Sub New()
        Me.New(Nothing, Nothing)
    End Sub

    Public Sub New(ByVal title As String)
        MyBase.New(title, Nothing)
    End Sub

    Public Sub New(ByVal title As String, ByVal subtitle As String)
        MyBase.New(title, subtitle)
    End Sub

#End Region

#Region " Properties "

    Public Overrides ReadOnly Property Id As String
        Get
            If StageId Is Nothing Then Throw New NotImplementedException(My.Resources.FileOrDatabaseStage_StageIDNotSet)
            Return StageId
        End Get
    End Property

    Private ReadOnly Property FileOrDatabaseControl() As ctlFileOrDatabaseChooser
        Get
            Return DirectCast(mControl, ctlFileOrDatabaseChooser)
        End Get
    End Property

    Public ReadOnly Property ProcessLocation As ProcessLocationType
        Get
            Return mProcessLocation
        End Get
    End Property

#End Region

#Region " Methods "

    Protected Overrides Function CreateControl() As ctlWizardStageControl
        Return New ctlFileOrDatabaseChooser()
    End Function

    Protected Overrides Sub OnCommitting(e As StageCommittingEventArgs)
        mProcessLocation = FileOrDatabaseControl.ProcessLocation
        MyBase.OnCommitting(e)
    End Sub

#End Region

End Class
