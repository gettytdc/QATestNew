

''' <summary>
''' Minor variation on the Log Export form which allows the tables to be populated
''' directly.
''' </summary>
Friend Class frmAuditLogExport

    ''' <summary>
    ''' Populates the Log Export Wizard with the specified settings.
    ''' </summary>
    ''' <param name="tables">The log pages</param>
    ''' <param name="title">The form title, with appropriate format placeholders
    ''' </param>
    ''' <param name="args">The arguments for the title</param>
    Public Sub Populate(ByRef tables As DataTable(), _
     ByVal title As String, ByVal ParamArray args() As Object)
        maLogDataTables = tables
        msTitle = String.Format(title, args)
    End Sub

End Class
