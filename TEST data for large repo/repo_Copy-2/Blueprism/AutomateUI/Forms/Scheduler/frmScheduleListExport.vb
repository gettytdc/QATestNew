Imports BluePrism.AutomateAppCore
Imports System.IO
Imports BluePrism.BPCoreLib

Friend Class frmScheduleListExport

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ExportList As ScheduleList

    Public Sub New()
        MyBase.New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.

        DefaultFileName = My.Resources.BPAScheduleLog

        'Until we implement HTML export of schedule lists this radio
        'needs to be invisible
        rdbHTML.Visible = False
    End Sub

    Protected Overrides Sub ExportLog()
        If ExportList Is Nothing Then Throw New ArgumentNullException(
            My.Resources.ThereIsNoScheduleListAvailableToExport)

        ResetProgressBar(100)

        Using sw As New StreamWriter(txtFile.Text)
            Dim type As ScheduleLogOutputType

            Select Case miFileType
                Case FileType.csv : type = ScheduleLogOutputType.CSV
                Case FileType.txt : type = ScheduleLogOutputType.Readable
                Case Else
                    Throw New InvalidOperationException(My.Resources.UnableToExportLogUnknownFileType)
            End Select

            Dim writer As New ScheduleLogWriter(sw, type)
            AddHandler writer.ProgressUpdate,
                Sub(percent) ProgressBar1.Value = CInt(percent)

            writer.OutputScheduleList(ExportList)

        End Using

    End Sub
End Class
