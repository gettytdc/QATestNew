Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib

Public Class ctlAuditAlerts
    Implements IChild
    Implements IPermission

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        RepopulateAlerts()

        Me.Enabled = Licensing.License.CanUse(LicenseUse.ProcessAlerts)
    End Sub

    Private Sub ctlAuditAlerts_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize
        lstMachines.Columns(0).Width = lstMachines.Width
    End Sub

    ''' <summary>
    ''' Handles the 'Delete' button being clicked.
    ''' </summary>
    Private Sub btnDelete_Click( _
     ByVal sender As Object, ByVal e As EventArgs) Handles btnDelete.Click
        DeleteSelectedAlertsMachines()
    End Sub

    ''' <summary>
    ''' Handles the refresh button being clicked by repopulating the alerts.
    ''' </summary>
    Private Sub btnRefresh_Click( _
     ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRefresh.Click
        RepopulateAlerts()
    End Sub

    ''' <summary>
    ''' Repopulates the UI list with the latest data
    ''' </summary>
    Private Sub RepopulateAlerts()
        Me.lstMachines.Items.Clear()
        Try
            Dim Machines = gSv.GetAlertsMachines()
            For Each s As String In Machines
                Me.lstMachines.Items.Add(s)
            Next
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlAuditAlerts_Error0, ex.Message), ex)
        End Try
    End Sub

    ''' <summary>
    ''' Deletes any selected alerts machines
    ''' </summary>
    Private Sub DeleteSelectedAlertsMachines()
        If lstMachines.SelectedItems.Count > 0 Then
            Dim Machines As New List(Of String)
            For Each item As ListViewItem In lstMachines.SelectedItems
                Machines.Add(item.Text)
            Next

            Try
                gSv.DeleteAlertsMachines(Machines)
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlAuditAlerts_OperationFailed0, ex.Message), ex)
            End Try

            RepopulateAlerts()
        Else
            UserMessage.Show(My.Resources.ctlAuditAlerts_PleaseFirstSelectSomeRowsToDelete)
        End If
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
            Return Permission.ByName("Audit - Alerts")
        End Get
    End Property
End Class
