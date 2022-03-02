Imports BluePrism.AutomateAppCore

Public Class LogLevelStage : Inherits WizardStage

#Region "Constants"
    Public Const StageId As String = "log.level"
#End Region

#Region "Private Members"
    Private _processComponents As IEnumerable(Of ProcessComponent)
#End Region

#Region "Constructor(s)"
    Public Sub New()
        MyBase.New(My.Resources.LogLevelStage_LogLevelOverview, My.Resources.LogLevelStage_CheckTheLogLevelsOnTheImportedRelease)
    End Sub
#End Region

#Region "Public Properties"
    Public Overrides ReadOnly Property Id() As String
        Get
            If StageId Is Nothing Then Throw New NotImplementedException(My.Resources.LogLevelStage_StageIDNotSet)
            Return StageId
        End Get
    End Property

    Public Property ProcessComponents() As IEnumerable(Of ProcessComponent)
        Get
            Return _processComponents
        End Get
        Set(ByVal value As IEnumerable(Of ProcessComponent))
            _processComponents = value
            If LogLevelControl IsNot Nothing Then LogLevelControl.ProcessComponents = value
        End Set
    End Property
#End Region

#Region "Private Properties"
    Private ReadOnly Property LogLevelControl() As ctlLogLevels
        Get
            Return DirectCast(mControl, ctlLogLevels)
        End Get
    End Property
#End Region

#Region "Protected Methods"
    Protected Overrides Function CreateControl() As ctlWizardStageControl
        Dim cs As New ctlLogLevels()
        cs.ProcessComponents = _processComponents
        Return cs
    End Function
#End Region

End Class
