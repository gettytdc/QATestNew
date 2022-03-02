Imports System.IO
Imports BluePrism.AutomateAppCore
Imports BluePrism.BPCoreLib.Extensions
Imports BluePrism.Server.Domain.Models

Friend Class frmLogExport
    Inherits frmWizard

#Region " Windows Form Designer generated code "


    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents pnlFileFormat As System.Windows.Forms.Panel
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents pnlChooseFileLocation As System.Windows.Forms.Panel
    Friend WithEvents btnBrowse As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtFile As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents ProgressBar1 As System.Windows.Forms.ProgressBar
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents lblExportMessages As System.Windows.Forms.Label
    Friend WithEvents rdbHTML As AutomateControls.StyledRadioButton
    Friend WithEvents rdbText As AutomateControls.StyledRadioButton
    Friend WithEvents rdbCSV As AutomateControls.StyledRadioButton
    Friend WithEvents pnlExportProgress As System.Windows.Forms.Panel
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmLogExport))
        Me.pnlFileFormat = New System.Windows.Forms.Panel()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.pnlChooseFileLocation = New System.Windows.Forms.Panel()
        Me.btnBrowse = New AutomateControls.Buttons.StandardStyledButton()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtFile = New AutomateControls.Textboxes.StyledTextBox()
        Me.pnlExportProgress = New System.Windows.Forms.Panel()
        Me.lblExportMessages = New System.Windows.Forms.Label()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.ProgressBar1 = New System.Windows.Forms.ProgressBar()
        Me.rdbCSV = New AutomateControls.StyledRadioButton()
        Me.rdbText = New AutomateControls.StyledRadioButton()
        Me.rdbHTML = New AutomateControls.StyledRadioButton()
        Me.pnlFileFormat.SuspendLayout()
        Me.pnlChooseFileLocation.SuspendLayout()
        Me.pnlExportProgress.SuspendLayout()
        Me.SuspendLayout()
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        '
        'btnNext
        '
        resources.ApplyResources(Me.btnNext, "btnNext")
        '
        'btnBack
        '
        resources.ApplyResources(Me.btnBack, "btnBack")
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        '
        'pnlFileFormat
        '
        Me.pnlFileFormat.Controls.Add(Me.rdbHTML)
        Me.pnlFileFormat.Controls.Add(Me.rdbText)
        Me.pnlFileFormat.Controls.Add(Me.rdbCSV)
        Me.pnlFileFormat.Controls.Add(Me.Label1)
        resources.ApplyResources(Me.pnlFileFormat, "pnlFileFormat")
        Me.pnlFileFormat.Name = "pnlFileFormat"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'pnlChooseFileLocation
        '
        Me.pnlChooseFileLocation.Controls.Add(Me.btnBrowse)
        Me.pnlChooseFileLocation.Controls.Add(Me.Label2)
        Me.pnlChooseFileLocation.Controls.Add(Me.txtFile)
        resources.ApplyResources(Me.pnlChooseFileLocation, "pnlChooseFileLocation")
        Me.pnlChooseFileLocation.Name = "pnlChooseFileLocation"
        '
        'btnBrowse
        '
        resources.ApplyResources(Me.btnBrowse, "btnBrowse")
        Me.btnBrowse.Name = "btnBrowse"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'txtFile
        '
        resources.ApplyResources(Me.txtFile, "txtFile")
        Me.txtFile.Name = "txtFile"
        '
        'pnlExportProgress
        '
        Me.pnlExportProgress.Controls.Add(Me.lblExportMessages)
        Me.pnlExportProgress.Controls.Add(Me.Label3)
        Me.pnlExportProgress.Controls.Add(Me.ProgressBar1)
        resources.ApplyResources(Me.pnlExportProgress, "pnlExportProgress")
        Me.pnlExportProgress.Name = "pnlExportProgress"
        '
        'lblExportMessages
        '
        resources.ApplyResources(Me.lblExportMessages, "lblExportMessages")
        Me.lblExportMessages.Name = "lblExportMessages"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'ProgressBar1
        '
        resources.ApplyResources(Me.ProgressBar1, "ProgressBar1")
        Me.ProgressBar1.Name = "ProgressBar1"
        '
        'rdbCSV
        '
        resources.ApplyResources(Me.rdbCSV, "rdbCSV")
        Me.rdbCSV.Name = "rdbCSV"
        Me.rdbCSV.TabStop = True
        Me.rdbCSV.UseVisualStyleBackColor = True
        '
        'rdbText
        '
        resources.ApplyResources(Me.rdbText, "rdbText")
        Me.rdbText.Name = "rdbText"
        Me.rdbText.TabStop = True
        Me.rdbText.UseVisualStyleBackColor = True
        '
        'rdbHTML
        '
        resources.ApplyResources(Me.rdbHTML, "rdbHTML")
        Me.rdbHTML.Name = "rdbHTML"
        Me.rdbHTML.TabStop = True
        Me.rdbHTML.UseVisualStyleBackColor = True
        '
        'frmLogExport
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.pnlExportProgress)
        Me.Controls.Add(Me.pnlChooseFileLocation)
        Me.Controls.Add(Me.pnlFileFormat)
        Me.Name = "frmLogExport"
        Me.Controls.SetChildIndex(Me.pnlFileFormat, 0)
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.Controls.SetChildIndex(Me.pnlChooseFileLocation, 0)
        Me.Controls.SetChildIndex(Me.pnlExportProgress, 0)
        Me.pnlFileFormat.ResumeLayout(False)
        Me.pnlFileFormat.PerformLayout()
        Me.pnlChooseFileLocation.ResumeLayout(False)
        Me.pnlChooseFileLocation.PerformLayout()
        Me.pnlExportProgress.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region "Members"

    ''' <summary>
    ''' The log pages.
    ''' </summary>
    ''' <remarks></remarks>
    Protected maLogDataTables As DataTable()

    ''' <summary>
    ''' The blue bar title.
    ''' </summary>
    ''' <remarks></remarks>
    Protected msTitle As String

    ''' <summary>
    ''' The default export file name.
    ''' </summary>
    ''' <remarks></remarks>
    Public DefaultFileName As String

    ''' <summary>
    ''' The log format to use when exporting.
    ''' </summary>
    ''' <remarks></remarks>
    Protected miFileType As FileType

    ''' <summary>
    ''' The log formats to use when exporting.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Enum FileType
        csv
        html
        txt
    End Enum

#End Region

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        DefaultFileName = "BPALog"

        Me.Text = My.Resources.frmLogExport_LogExportWizard

        MyBase.SetMaxSteps(2)

    End Sub

    ''' <summary>
    ''' Moves the wizard to the next page.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overrides Sub UpdatePage()

        Select Case MyBase.GetStep
            Case 0
                ShowPage(Me.pnlFileFormat)
                Me.rdbCSV.Checked = True

            Case 1
                Select Case True
                    Case Me.rdbText.Checked
                        miFileType = FileType.txt
                    Case rdbCSV.Checked
                        miFileType = FileType.csv
                    Case rdbHTML.Checked
                        miFileType = FileType.html
                    Case Else
                        UserMessage.Show(My.Resources.frmLogExport_PleaseChooseOneOfTheAvailableFileFormats)
                        MyBase.Rollback()
                        Exit Sub
                End Select

                ShowPage(Me.pnlChooseFileLocation)
                Me.txtFile.Text = Nothing

            Case 2

                'validate filename
                Dim sErr As String = Nothing
                If Not FileNameIsValid(sErr) Then
                    MyBase.Rollback(sErr)
                    Exit Sub
                End If

                'check directory exists
                Dim iSlashIndex As Integer = Me.txtFile.Text.LastIndexOf("\")
                Dim sDirectory As String = Me.txtFile.Text.Mid(1, iSlashIndex)

                If iSlashIndex > -1 Then
                    If Not Directory.Exists(sDirectory) Then
                        Try
                            If UserMessage.YesNo(My.Resources.frmLogExport_ThisFolderDoesNotExistDoYouWantToCreateItNow) = MsgBoxResult.Yes Then
                                Directory.CreateDirectory(sDirectory)
                                If Not Directory.Exists(sDirectory) Then
                                    MyBase.Rollback(My.Resources.frmLogExport_AnErrorHasOccuredAndTheFolderCouldNotBeCreated2)
                                    Exit Sub
                                End If
                            Else
                                MyBase.Rollback()
                                Exit Sub
                            End If
                        Catch ex As Exception
                            MyBase.Rollback(My.Resources.frmLogExport_AnErrorHasOccuredAndTheFolderCouldNotBeCreated & ex.Message)
                            Exit Sub
                        End Try
                    End If
                End If

                Try
                    Me.ShowPage(Me.pnlExportProgress)
                    Me.lblExportMessages.Visible = True
                    Me.btnNext.Enabled = False
                    DoExport()
                Catch ex As Exception
                    UserMessage.Show(String.Format(My.Resources.frmLogExport_ErrorExportingLog0PleaseTryAgainOrClickCancelToQuit, ex.Message))
                    MyBase.Rollback()
                Finally
                    Me.btnNext.Enabled = True
                End Try

            Case 3

                'quit wizard
                Me.DialogResult = System.Windows.Forms.DialogResult.OK
                Me.Close()

        End Select
    End Sub

    ''' <summary>
    ''' Performs the export operation, adding a little expository text to the
    ''' interface before it does so
    ''' </summary>
    Private Sub DoExport()
        lblExportMessages.Text = My.Resources.frmLogExport_CreatingFile

        If Not gSv.AuditRecordSysConfigEvent(SysConfEventCode.ExportAuditLog, msTitle) Then
            Throw New BluePrismException(My.Resources.frmLogExport_FailedToCreateAuditEntry)
        End If

        ExportLog()
        lblExportMessages.Text = ""
    End Sub

    ''' <summary>
    ''' Writes log data out to a file.
    ''' </summary>
    Protected Overridable Sub ExportLog()

        Using sw = New StreamWriter(txtFile.Text)
            Select Case miFileType
                Case FileType.csv, FileType.txt
                    WriteCSV(maLogDataTables, sw)
                Case FileType.html
                    WriteHTML(maLogDataTables, sw)
                Case Else
                    Throw New InvalidArgumentException(My.Resources.frmLogExport_UnableToExportLogUnknownFileType)
            End Select
        End Using

    End Sub

    ''' <summary>
    ''' Validates the filename in txtFile.
    ''' </summary>
    ''' <param name="sErr">A string to carry a message in the event
    ''' that the file is not valid.</param>
    ''' <returns>True if filename is valid.</returns>
    Private Function FileNameIsValid(ByRef sErr As String) As Boolean

        'check for blank file names
        If Me.txtFile.Text.Trim.Length = 0 Then
            sErr = My.Resources.frmLogExport_TheFileNameThatYouEnteredIsBlank
            Return False
        End If

        'check file extension
        Dim iDotIndex As Integer = Me.txtFile.Text.LastIndexOf("."c)
        If iDotIndex = -1 Then
            sErr = String.Format(My.Resources.frmLogExport_The0FileExtensionIsMissing, miFileType.ToString)
            Return False
        Else
            'user has supplied a file extension - let's see if it matches reqd type
            If Me.txtFile.Text.Mid(iDotIndex + 1, Me.txtFile.Text.Length) <> "." & miFileType.ToString Then
                sErr = String.Format(My.Resources.frmLogExport_TheFileExtensionMustBe0, miFileType.ToString)
                Return False
            End If
        End If

        sErr = ""
        Return True
    End Function

    ''' <summary>
    ''' Writes the data into an HTML document.
    ''' </summary>
    ''' <param name="aDataTables">The log data datatables</param>
    ''' <param name="output">A streamwriter to write the output into</param>
    Protected Sub WriteHTML(ByVal aDataTables As DataTable(), ByVal output As StreamWriter)

        output.WriteLine("<?xml version=""1.0"" encoding=""UTF-8""?>")
        output.WriteLine("<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Strict//EN""")
        output.WriteLine("""http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd"">")
        Dim lang As String = System.Threading.Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName
        output.WriteLine("<HTML xmlns=""http://www.w3.org/1999/xhtml"" xml:lang=""" & lang & """ lang=""" & lang & """>")
        output.WriteLine("<HEAD>")
        output.WriteLine("<TITLE>" & msTitle & "</TITLE>")

        output.WriteLine("<STYLE>")
        output.WriteLine("BODY{font-family: helvetica;}")
        output.WriteLine("TABLE.AutomateTable {width: 100%;border: 1;empty-cells: show;border-collapse: collapse;font-size: 8pt;}")
        output.WriteLine("TD, TH {border: 1px solid #9fa4c1;padding-left: 3px;padding-right: 1px;padding-top: 1px;padding-bottom: 1px;vertical-align: top;}")
        output.WriteLine("TH {font-weight: bold; color: white;background-color: #9fa4c1;}")
        If lang.Equals("ja") Or lang.Equals("zh") Then
            output.WriteLine("TH {word-break: keep-all;}")
        End If
        output.WriteLine("</STYLE>")

        output.WriteLine("</HEAD>")
        WriteHTMLBody(aDataTables, output)
        output.WriteLine("</HTML>")

    End Sub

    ''' <summary>
    ''' Writes the data into an HTML BODY statement.
    ''' </summary>
    ''' <param name="aDataTables">The log data</param>
    ''' <param name="output">A StreamWriter to write the data into</param>
    Protected Overridable Sub WriteHTMLBody(ByVal aDataTables As DataTable(), ByVal output As StreamWriter)

        If aDataTables Is Nothing OrElse aDataTables.Length = 0 Then
            Return
        End If

        ResetProgressBar(aDataTables.Length)

        output.WriteLine("<BODY>")
        output.WriteLine("<TABLE class='AutomateTable'>")

        Dim dt As DataTable = Nothing

        For t As Integer = 0 To aDataTables.Length - 1
            dt = aDataTables(t)

            If t = 0 Then
                'Start a TABLE with a column header row.
                output.WriteLine("<TR>")
                For c As Integer = 0 To dt.Columns.Count - 1
                    output.Write("<TH>")
                    output.Write(dt.Columns(c).ColumnName)
                    output.Write("</TH>")
                Next
                output.WriteLine("</TR>")
            End If

            'Add a TR for every data row.
            For Each r As DataRow In dt.Rows
                output.WriteLine("<TR>")
                For c As Integer = 0 To dt.Columns.Count - 1
                    'For all othe columns, add a TD.
                    output.Write("<TD>")
                    If r(c) Is Nothing OrElse TypeOf r(c) Is DBNull OrElse r(c).ToString = "" Then
                        output.Write("&nbsp;")
                    Else
                        output.Write(r(c).ToString)
                    End If
                    output.Write("</TD>")
                Next
            Next

            IncrementProgressBar()
        Next

        output.WriteLine("</TABLE>")
        output.WriteLine("</BODY>")


    End Sub

    ''' <summary>
    ''' Writes the data into a CSV file.
    ''' </summary>
    ''' <param name="aDataTables">The log data datatables</param>
    ''' <param name="output">A Streamwriter to write the data into</param>
    Protected Overridable Sub WriteCSV(ByVal aDataTables() As System.Data.DataTable, ByVal output As StreamWriter)

        Const sSeparator As String = ","

        If aDataTables Is Nothing OrElse aDataTables.Length = 0 Then
            Return
        End If

        ResetProgressBar(aDataTables.Length)

        Dim dt As DataTable = Nothing

        For t As Integer = 0 To aDataTables.Length - 1
            dt = aDataTables(t)

            'Make a header row
            If t = 0 Then
                For c As Integer = 0 To dt.Columns.Count - 1
                    output.Write("""" & dt.Columns(c).ColumnName & """" & sSeparator)
                Next
                output.WriteLine()
            End If

            For Each r As DataRow In dt.Rows
                For c As Integer = 0 To dt.Columns.Count - 1
                    If r(c) Is Nothing OrElse TypeOf r(c) Is DBNull OrElse r(c).ToString = "" Then
                        output.Write(sSeparator)
                    Else
                        output.Write(r(c).ToString.Replace("""", """""") & sSeparator)
                    End If
                Next
                output.WriteLine()
            Next

            IncrementProgressBar()
        Next

    End Sub

    ''' <summary>
    ''' Increments the progress bar value by 1 if safe to do so.
    ''' </summary>
    Protected Sub IncrementProgressBar()
        If ProgressBar1.Value < ProgressBar1.Maximum Then
            ProgressBar1.Value += 1
            ProgressBar1.Invalidate()
        End If
    End Sub

    ''' <summary>
    ''' Resets the progress bar.
    ''' </summary>
    ''' <param name="iMaximum"></param>
    ''' <remarks></remarks>
    Protected Sub ResetProgressBar(ByVal iMaximum As Integer)
        ProgressBar1.Minimum = 0
        ProgressBar1.Maximum = iMaximum
        ProgressBar1.Value = ProgressBar1.Minimum
        ProgressBar1.Invalidate()
    End Sub

    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowse.Click

        Dim oSaveFileDialog As New SaveFileDialog
        Dim sFilter As String

        Select Case miFileType
            Case FileType.csv
                sFilter = My.Resources.frmLogExport_CSVFiles
            Case FileType.html
                sFilter = My.Resources.frmLogExport_HTMLFiles
            Case FileType.txt
                sFilter = My.Resources.frmLogExport_TextFiles
            Case Else
                sFilter = My.Resources.frmLogExport_AllFiles
        End Select
        oSaveFileDialog.Filter = String.Format(My.Resources.frmLogExport_01, sFilter, miFileType.ToString) & "|" & "*" & miFileType.ToString

        oSaveFileDialog.AddExtension = True
        oSaveFileDialog.DefaultExt = miFileType.ToString
        oSaveFileDialog.CreatePrompt = False
        oSaveFileDialog.FileName = DefaultFileName & "." & miFileType.ToString
        If Directory.Exists(Me.txtFile.Text) Then
            oSaveFileDialog.InitialDirectory = Me.txtFile.Text
        Else
            oSaveFileDialog.InitialDirectory = clsFileSystem.GetExportedLogsPath
        End If
        oSaveFileDialog.OverwritePrompt = True
        oSaveFileDialog.ShowHelp = False
        oSaveFileDialog.Title = My.Resources.frmLogExport_ChooseAFileToExportLogTo
        oSaveFileDialog.ValidateNames = True

        If oSaveFileDialog.ShowDialog = System.Windows.Forms.DialogResult.OK Then
            Me.txtFile.Text = oSaveFileDialog.FileName
        End If

    End Sub

    ''' <summary>
    ''' Returns help file for this form.
    ''' </summary>
    ''' <returns>The file name of the html help file for this form.</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmLogExport.htm"
    End Function
End Class
