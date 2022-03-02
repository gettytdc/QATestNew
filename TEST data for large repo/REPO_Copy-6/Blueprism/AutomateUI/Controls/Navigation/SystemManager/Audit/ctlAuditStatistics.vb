Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports System.Linq

Public Class ctlAuditStatistics
    Implements IChild
    Implements IPermission

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        Dim statsTable As DataTable = gSv.GetStatistics()
        If statsTable IsNot Nothing Then
            For Each dc As DataColumn In statsTable.Columns
                dc.ColumnName = LocaleTools.LTools.GetC(dc.ColumnName, "misc")
            Next
        End If
        dgStats.DataSource = statsTable

        ' Width needs to be calculated dynamically so that the header test fits the column AND the column is resizable
        dgStats.Columns.Cast(Of DataGridViewColumn).ToList().ForEach(
            Sub(c)
                c.Width = c.GetPreferredWidth(DataGridViewAutoSizeColumnMode.ColumnHeader, True) + 20
            End Sub)
        dgStats.Refresh()
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public ReadOnly Property RequiredPermissions() As System.Collections.Generic.ICollection(Of BluePrism.AutomateAppCore.Auth.Permission) Implements BluePrism.AutomateAppCore.Auth.IPermission.RequiredPermissions
        Get
            Return Permission.ByName("Audit - Statistics")
        End Get
    End Property
End Class
