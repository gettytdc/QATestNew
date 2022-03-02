Imports System.Runtime.CompilerServices

Public Module ToolstipMenuItemExtension
    <Extension>
    Public Function DisableMenuItemAndSetTooltip(this As ToolStripMenuItem, errorMessage As String) As ToolStripMenuItem
        this.ToolTipText = errorMessage
        this.Enabled = False
        Return this
    End Function
End Module
