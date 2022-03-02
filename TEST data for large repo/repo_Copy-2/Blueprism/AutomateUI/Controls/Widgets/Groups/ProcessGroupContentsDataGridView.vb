Imports AutomateControls.DataGridViews
Imports BluePrism.AutomateAppCore.Groups

''' <summary>
''' DataGridView which shows the contents of process-backed groups.
''' </summary>
Public Class ProcessGroupContentsDataGridView : Inherits GroupContentsDataGridView

#Region " Member Variables "

    ' The column holding the last modified date of the process/object
    Private colModified As DateColumn

    ' The column holding the username of the last modifier of the process/object
    Private colModifiedBy As DataGridViewTextBoxColumn

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty process-backed datagridview
    ''' </summary>
    Public Sub New()
        colModified = New DateColumn() With {
            .AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            .HeaderText = My.Resources.ProcessGroupContentsDataGridView_Modified,
            .Name = "colModified",
            .ReadOnly = True
        }

        colModifiedBy = New DataGridViewTextBoxColumn() With {
            .AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            .HeaderText = My.Resources.ProcessGroupContentsDataGridView_By,
            .Name = "colModifiedBy",
            .ReadOnly = True
        }

        Columns.AddRange(colModified, colModifiedBy)

    End Sub

#End Region

#Region " Methods "

    ''' <summary>
    ''' Adds a row representing a specified group member into the appropriate part of
    ''' this grid view depending on how sorting is being done.
    ''' </summary>
    ''' <param name="gm">The group member for whom a new row is required.</param>
    ''' <returns>The new row created for the group member, or null if a row was not
    ''' created for some reason - in this case, if it wasn't a process-backed group
    ''' member.</returns>
    Protected Overrides Function AddRow(gm As IGroupMember) As DataGridViewRow
        Return If(TypeOf gm Is ProcessBackedGroupMember, MyBase.AddRow(gm), Nothing)
    End Function

    ''' <summary>
    ''' Updates a data grid view row with data from a group member
    ''' </summary>
    ''' <param name="row">The row to update</param>
    ''' <param name="mem">The group member update the row with</param>
    Protected Overrides Sub UpdateRow(row As DataGridViewRow, mem As IGroupMember)
        MyBase.UpdateRow(row, mem)
        ' We're only interested in process-backed group members
        Dim procMem = TryCast(mem, ProcessBackedGroupMember)
        If procMem Is Nothing Then Return

        ' Update with the process-based information
        row.Cells(colModified.Index).Value = procMem.ModifiedAt
        row.Cells(colModifiedBy.Index).Value = procMem.ModifiedBy
        ' Update tag with new object so that refreshed state is available
        row.Tag = mem
    End Sub

#End Region

End Class
