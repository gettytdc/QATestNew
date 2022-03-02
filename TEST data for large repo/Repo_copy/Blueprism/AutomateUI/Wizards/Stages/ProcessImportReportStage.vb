Imports AutomateUI
Imports BluePrism.AutomateAppCore

Public Class ProcessImportReportStage : Inherits WizardStage

    Public Sub New()
        MyBase.New(My.Resources.ctlImportProcessReport_ImportReport,"")
    End Sub

    ''' <summary>
    ''' The ID of this stage
    ''' </summary>
    Public Const StageId As String = "Import.Report"

    Public ImportFiles As List(Of ImportFile)

    Public Overrides ReadOnly Property Id As String
        Get
            If StageId Is Nothing Then Throw New NotImplementedException(My.Resources.ImportConflictStage_StageIDNotSet)
            Return StageId
        End Get
    End Property

    Protected Overrides Function CreateControl() As ctlWizardStageControl
        Dim importProcessReport As New ctlImportProcessReport()
        importProcessReport.ImportFiles = ImportFiles
        Return importProcessReport
    End Function
End Class
