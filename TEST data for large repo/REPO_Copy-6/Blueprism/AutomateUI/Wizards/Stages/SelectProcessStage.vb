Imports BluePrism.AutomateAppCore.Groups

Public Class SelectProcessStage : Inherits WizardStage

#Region " Constants "

    ''' <summary>
    ''' The ID of this stage
    ''' </summary>
    Public Const StageId As String = "selectprocess.stage"

#End Region

#Region " Members "

    Private mSelectedMember As ProcessBackedGroupMember

    Private mProcType As ProcessType
#End Region

#Region " Constructors "

    Public Sub New()
        Me.New(Nothing, Nothing)
    End Sub

    Public Sub New(ByVal title As String, proctype As ProcessType)
        Me.New(title, Nothing, proctype)
    End Sub

    Public Sub New(ByVal title As String, ByVal subtitle As String, procType As ProcessType)
        MyBase.New(title, subtitle)
        mProcType = procType
    End Sub

#End Region

#Region " Properties "

    Public Overrides ReadOnly Property Id As String
        Get
            Return StageId
        End Get
    End Property

    Public ReadOnly Property SelectProcessControl As ctlProcessChooser
        Get
            Return DirectCast(mControl, ctlProcessChooser)
        End Get
    End Property

    Public ReadOnly Property SelectedMember As ProcessBackedGroupMember
        Get
            Return mSelectedMember
        End Get
    End Property

#End Region

#Region " Methods "

    Protected Overrides Function CreateControl() As ctlWizardStageControl
        Return New ctlProcessChooser(mProcType)
    End Function

    Protected Overrides Sub OnCommitting(e As StageCommittingEventArgs)
        mSelectedMember = SelectProcessControl.SelectedMember
        MyBase.OnCommitting(e)
    End Sub
#End Region

End Class
