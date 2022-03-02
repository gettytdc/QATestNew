Imports System.IO
Imports BluePrism.AutomateAppCore
Imports BluePrism.Images

Public Class ctlImportProcessReport : Inherits ctlWizardStageControl
    #Region "Private Classes"
    Private Class ProcessImportReport
        Public Sub New(compName As String, imp As Boolean, conf As String, res As String, impAs As String)
            ComponentName = compName
            Imported = imp
            Conflict = conf
            Resolution = res
            ImportedAs = impAs
        End Sub

        Public ComponentName As String
        Public Imported As Boolean
        Public Conflict As String
        Public Resolution As String
        Public ImportedAs As String
    End Class

#End Region

    Private ReadOnly mProcessImportReport As List(Of ProcessImportReport) = New List(Of ProcessImportReport)

    Private mImportFiles As List(Of ImportFile)

    Public Property ImportFiles As List(Of ImportFile)
        Get
            Return mImportFiles
        End Get
        Set(value As List(Of ImportFile))
            mImportFiles = value
            If value Is Nothing OrElse Not value.Any Then Return

            mContents.AutoGenerateColumns = False

            mImportFiles.ForEach(
                Sub(importFile)
                    If importFile.CanImport Then
                        If importFile.Conflicts.Any Then
                            importFile.Conflicts.ForEach(
                        Sub(conflict)
                            mProcessImportReport.Add(New ProcessImportReport(importFile.BluePrismName, importFile.CanImport, conflict.Definition.Text, conflict.Resolution.ConflictOption.Text,
                                                                             If(importFile.BluePrismName <> conflict.Component.Name, $"{My.Resources.ctlImportProcessReport_ImportedAs} - {conflict.Component.Name}", "")))
                        End Sub)
                        Else
                            mProcessImportReport.Add(New ProcessImportReport(importFile.BluePrismName, importFile.CanImport, "", "", ""))
                        End If
                    Else
                        If importFile.UserHasPermission Then
                            mProcessImportReport.Add(New ProcessImportReport(importFile.BluePrismName, importFile.CanImport, String.Join(" | ", importFile.Errors), My.Resources.NA, My.Resources.ctlImportProcessReport_NotImported))
                        Else
                            mProcessImportReport.Add(New ProcessImportReport(importFile.BluePrismName,
                                                                             importFile.CanImport,
                                                                             String.Join(" | ", importFile.Errors),
                                                                             My.Resources.NA, 
                                                                             String.Format(My.Resources.ctlImportProcessReport_PermissionRequired0, 
                                                                                           If(importFile.ProcessType = PackageComponentType.BusinessObject, 
                                                                                              My.Resources.ctlImportProcessReport_ImportBusinesssObject, 
                                                                                              My.Resources.ctlImportProcessReport_ImportProcess))))
                        End If
                        
                    End If
                End Sub)

            mContents.DataSource = New BindingSource(mProcessImportReport, Nothing)
        End Set
    End Property

    #Region "Event Handlers"
    Private Sub MContents_CellFormatting(sender As Object, e As DataGridViewCellFormattingEventArgs) Handles mContents.CellFormatting

        Dim data = CType(mContents.Rows(e.RowIndex).DataBoundItem, ProcessImportReport)

        mContents.Rows(e.RowIndex).Cells("ComponentColumn").Value = data.ComponentName
        mContents.Rows(e.RowIndex).Cells("ComponentColumn").ToolTipText  = data.ComponentName

        mContents.Rows(e.RowIndex).Cells("IssueColumn").Value = data.Conflict
        mContents.Rows(e.RowIndex).Cells("IssueColumn").ToolTipText = data.Conflict

        mContents.Rows(e.RowIndex).Cells("ResolutionColumn").Value = data.Resolution
        mContents.Rows(e.RowIndex).Cells("ResolutionColumn").ToolTipText = data.Resolution

        mContents.Rows(e.RowIndex).Cells("ImportedAsColumn").Value =data.ImportedAs
        mContents.Rows(e.RowIndex).Cells("ImportedAsColumn").ToolTipText = data.ImportedAs
    End Sub

    Private Sub MContents_DataBindingComplete(sender As Object, e As DataGridViewBindingCompleteEventArgs) Handles mContents.DataBindingComplete
        ' Images are updated on DataBindingComplete because when they are updated on CellFormatting, they cause the image column to keep formatting itself
        For Each row As DataGridViewRow In mContents.Rows
            Dim data = CType(row.DataBoundItem, ProcessImportReport)
            row.Cells("ImageColumn").Value = if(data.Imported,  ToolImages.Tick_16x16, ToolImages.Cross_16x16)
        Next
    End Sub
    Private Sub BtnExport_Click(sender As Object, e As EventArgs) Handles btnExport.Click
        Dim saveAsDialog = New SaveFileDialog
        saveAsDialog.Filter = "CSV Files (*.csv*)|*.csv"
        saveAsDialog.FileName = My.Resources.ctlImportProcessReport_ImportReport 
        If saveAsDialog.ShowDialog = DialogResult.OK Then
            Try
Using sw As New StreamWriter(saveAsDialog.FileName)
                dim sb = new StringBuilder
                sb.AppendLine($"{My.Resources.ctlImportProcessReport_ComponentFileName}, {My.Resources.ctlImportProcessReport_Imported}, {My.Resources.ctlImportProcessReport_Issues}, {My.Resources.ctlImportProcessReport_Resolution}, {My.Resources.ctlImportProcessReport_ImportedAs}")
                mProcessImportReport.ForEach(Sub(x) sb.AppendLine($"""{x.ComponentName}"", {(If(x.Imported,My.Resources.Yes,My.Resources.No))},""{x.Conflict}"",""{x.Resolution}"",""{x.ImportedAs}"" "))
                sw.Write(sb)
            End Using
            Catch ex As IoException
                UserMessage.Show(string.Format(My.Resources.ctlImportProcessReport_CannotWriteToFile0CheckPermissions, saveAsDialog.FileName))
            End Try
            
        End If 
    End Sub
    #End Region
End Class
