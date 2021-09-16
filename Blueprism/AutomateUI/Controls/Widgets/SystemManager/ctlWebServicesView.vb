Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore
Imports ObjectEventCode = BluePrism.AutomateAppCore.ObjectEventCode

''' Project  : Automate
''' Class    : ctlBusinessObjectsView
''' 
''' <summary>
''' A control to display and manage Business Objects.
''' </summary>
Public Class ctlWebServicesView
    Inherits UserControl

    Public WebServices As ICollection(Of clsWebServiceDetails)

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        If LicenseManager.UsageMode = LicenseUsageMode.Runtime Then UpdateList()

    End Sub

#Region " Windows Form Designer generated code "

    'UserControl overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    Friend WithEvents dataGridViewBusinessObjects As AutomateControls.DataGridViews.RowBasedDataGridView

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlWebServicesView))
        Me.dataGridViewBusinessObjects = New AutomateControls.DataGridViews.RowBasedDataGridView()
        CType(Me.dataGridViewBusinessObjects, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'dataGridViewBusinessObjects
        '
        resources.ApplyResources(Me.dataGridViewBusinessObjects, "dataGridViewBusinessObjects")
        Me.dataGridViewBusinessObjects.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        Me.dataGridViewBusinessObjects.Name = "dataGridViewBusinessObjects"
        '
        'ctlWebServicesView
        '
        Me.BackColor = System.Drawing.Color.White
        Me.Controls.Add(Me.dataGridViewBusinessObjects)
        Me.Name = "ctlWebServicesView"
        CType(Me.dataGridViewBusinessObjects, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

#End Region

    ''' <summary>
    ''' Update the contents of the list to reflect the current registrations in the
    ''' database.
    ''' </summary>
    Public Sub UpdateList()
        Dim dataView = New DataView
        Dim dataTable = New DataTable("Web Services")
        dataTable.Columns.Add(New DataColumn("Enabled", GetType(Boolean)))
        dataTable.Columns.Add(New DataColumn("Name", GetType(String)))
        dataTable.Columns.Add(New DataColumn("URL", GetType(String)))
        dataTable.Columns.Add(New DataColumn("Timeout", GetType(String)))

        Try
            WebServices = gSv.GetWebServiceDefinitions()
        Catch
            Exit Sub
        End Try

        For Each webService In WebServices
            Dim row = dataTable.NewRow()
            row("Enabled") = webService.Enabled
            row("Name") = webService.FriendlyName
            row("URL") = webService.URL
            row("Timeout") = webService.Timeout.ToString

            dataTable.Rows.Add(row)
        Next

        dataView.Table = dataTable
        dataGridViewBusinessObjects.DataSource = dataView
        SetupDataTable()
    End Sub


    ''' <summary>
    ''' The Delete Object menu item event handler.
    ''' </summary>
    Public Sub DeleteSelectedObjects()
        Dim remove As New List(Of Guid)
        Dim names As New List(Of String)
        For Each row As DataGridViewRow In dataGridViewBusinessObjects.SelectedRows
            remove.Add(WebServices.FirstOrDefault(Function(x) x.FriendlyName = row.Cells("Name").Value.ToString).Id)
            names.Add(row.Cells("Name").Value.ToString)
        Next

        If remove.Count > 0 Then
            Try
                gSv.DeleteWebservices(remove)
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlWebServicesView_FailedToRemoveWebServices0, ex.Message))
            End Try
        End If

    End Sub

    Private Sub SetupDataTable()
        dataGridViewBusinessObjects.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dataGridViewBusinessObjects.Columns(0).FillWeight = 60
        dataGridViewBusinessObjects.Columns(1).FillWeight = 200
        dataGridViewBusinessObjects.Columns(2).FillWeight = 200
        dataGridViewBusinessObjects.Columns(3).FillWeight = 100

        dataGridViewBusinessObjects.Columns(0).HeaderText = My.Resources.ctlWebServicesView_Enabled
        dataGridViewBusinessObjects.Columns(1).HeaderText = My.Resources.ctlWebServicesView_Name
        dataGridViewBusinessObjects.Columns(2).HeaderText = My.Resources.ctlWebServicesView_URL
        dataGridViewBusinessObjects.Columns(3).HeaderText = My.Resources.ctlWebServicesView_Timeout

        dataGridViewBusinessObjects.Columns(0).ReadOnly = False
        dataGridViewBusinessObjects.Columns(1).ReadOnly = True
        dataGridViewBusinessObjects.Columns(2).ReadOnly = True
        dataGridViewBusinessObjects.Columns(3).ReadOnly = True
    End Sub

    Private Sub dataGridViewBusinessObjects_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dataGridViewBusinessObjects.CellValueChanged
        If (e.ColumnIndex = dataGridViewBusinessObjects.Columns("Enabled").Index And e.RowIndex <> -1) Then
            Dim row As DataGridViewRow = dataGridViewBusinessObjects.Rows(e.RowIndex)
            Dim id = WebServices.FirstOrDefault(Function(x) x.FriendlyName = row.Cells("Name").Value.ToString).Id
            Dim checked = Boolean.Parse(row.Cells("Enabled").Value.ToString)
            Try
                gSv.UpdateWebServiceEnabled(id, checked)
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlWebServicesView_FailedToUpdateTheWebServicesEnabledState0, ex.Message))
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Event handler that is required for checkbox ticked event handlers to trigger, see this for more information: 
    ''' https://stackoverflow.com/questions/11843488/how-to-detect-datagridview-checkbox-event-change
    ''' </summary>
    Private Sub dataGridViewBusinessObjects_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles dataGridViewBusinessObjects.CellContentClick
        dataGridViewBusinessObjects.CommitEdit(DataGridViewDataErrorContexts.Commit)
    End Sub

    Private Sub dataGridViewBusinessObjects_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles dataGridViewBusinessObjects.DoubleClick

        Select Case dataGridViewBusinessObjects.SelectedRows.Count
            Case 0
                UserMessage.Show(SystemManagerObjects_Resources.ctlWebServicesViewYouMustSelectOneToUpdate)
            Case 1
                Using objWebConfig As New frmWebServices
                    Dim id = WebServices.FirstOrDefault(Function(x) x.FriendlyName =
                dataGridViewBusinessObjects.SelectedRow.Cells("Name").Value.ToString).Id
                    objWebConfig.Setup(id)
                    objWebConfig.ShowInTaskbar = False
                    objWebConfig.ShowDialog()

                    If objWebConfig.DialogResult = DialogResult.OK Then
                        Dim details As clsWebServiceDetails = objWebConfig.ServiceDetails
                        Try
                            gSv.SaveWebServiceDefinition(details)
                        Catch ex As Exception
                            UserMessage.Err(ex,
                                SystemManagerObjects_Resources.ctlWebServicesViewFailedToUpdateWebService,
                                ex.Message)
                        End Try
                    End If
                End Using
                UpdateList()
            Case Else
                UserMessage.Show(SystemManagerObjects_Resources.ctlWebServicesViewCanOnlyConfigureOneAtATime)
        End Select

    End Sub
End Class
