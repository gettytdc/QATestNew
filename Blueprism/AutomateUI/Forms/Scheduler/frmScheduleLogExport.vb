Imports BluePrism.AutomateAppCore
Imports System.IO
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

Friend Class frmScheduleLogExport

    Private mInst As IScheduleInstance

    Public Sub New()
        MyBase.New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        DefaultFileName = My.Resources.BPAScheduleLog

        'Until we implement HTML export of schedule logs this radio
        'needs to be invisible
        rdbHTML.Visible = False
    End Sub

    Public Sub Populate(ByVal inst As IScheduleInstance)
        mInst = inst
    End Sub

    Protected Overrides Sub ExportLog()
        ResetProgressBar(100)

        Using oStreamWriter As New StreamWriter(txtFile.Text)
            Dim type As ScheduleLogOutputType

            Select Case miFileType
                Case FileType.csv : type = ScheduleLogOutputType.CSV
                Case FileType.txt : type = ScheduleLogOutputType.Readable
                Case Else
                    Throw New InvalidArgumentException(My.Resources.UnableToExportLogUnknownFileType)
            End Select

            Dim scheduleWriter As New ScheduleLogWriter(oStreamWriter, type)
            scheduleWriter.OutputScheduleLog(TryCast(mInst, HistoricalScheduleLog))

            'Yeah this is lame, but do we really need to know how far along we are?
            'If we do then FIXME
            ProgressBar1.Value = ProgressBar1.Maximum
        End Using
    End Sub
End Class
