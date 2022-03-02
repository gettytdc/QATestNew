Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore.Auth
Imports AutomateUI
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.BPCoreLib
Imports System.Globalization
Imports AutomateControls
Imports AutomateControls.Forms
Imports BluePrism.Server.Domain.Models

Public Class ctlWebApiServices : Implements IStubbornChild, IPermission, IHelp

    Private Enum ServiceAction
        Update
        Delete
        Add
        Find
    End Enum

    Private mObjSorter As clsListViewSorter
    Private mParent As frmApplication

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()
        UpdateDataTable()
    End Sub

    ''' <summary>
    ''' Checks that the given name is valid for a web api with the given ID. This
    ''' will ensure that no other web api exists with the same name (ignoring case).
    ''' </summary>
    ''' <param name="id">The ID of the API to check the name on behalf of. This
    ''' ensures that a name is not rejected because the API is the one being edited.
    ''' </param>
    ''' <param name="name">The name to test for validity, especially ensuring that no
    ''' API with an ID other than <paramref name="id"/> exists with the same name.
    ''' </param>
    ''' <returns>True if the name is valid and is not a duplicate.</returns>
    Private Function CheckValidName(id As Guid, name As String) As Boolean
        If name = "" Then Return False

        Return dataGridViewWebApi.Rows.OfType(Of DataGridViewRow).
            All(Function(row) Not name.Equals(GetWebApiName(row), StringComparison.CurrentCultureIgnoreCase))
    End Function

    Private Function GetWebApiName(row As DataGridViewRow) As String
        Return row.Cells("Name").Value.ToString()
    End Function

    Private Sub llAdd_LinkClicked(
     sender As Object, e As LinkLabelLinkClickedEventArgs) Handles llAdd.LinkClicked
        Try
            Using createForm = CreateWebApiForm(ServiceAction.Add)
                createForm.SetEnvironmentColours(mParent)
                Dim result = createForm.ShowDialog()
                If result = DialogResult.OK Then
                    Dim service = createForm.Service
                    Try
                        gSv.SaveWebApi(service)
                        UpdateDataTable()
                    Catch ex As Exception
                        UserMessage.Err(ex,
                          SystemManagerObjects_Resources.ctlWebApiFailedToAddApiToDatabase,
                          ex.Message)

                    End Try
                End If
            End Using

        Catch ex As Exception
            UserMessage.Err(ex,
             SystemManagerObjects_Resources.ctlWebApiUnexpectedErrorAdded_Template,
             ex.Message)
        End Try
    End Sub

    Private Sub llEdit_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles llEdit.LinkClicked
        ValidateEditWarningShouldBeDisplayed()
    End Sub

    Private Sub ValidateEditWarningShouldBeDisplayed()
        Dim nameOfSkillLinkedToWebApi = dataGridViewWebApi.SelectedRow.Cells("Skills").Value?.ToString()

        If String.IsNullOrWhiteSpace(nameOfSkillLinkedToWebApi) Then
            UpdateSelectedWebAPIService(displayWarning:=False)
        Else
            Dim message = String.Format(SystemManagerObjects_Resources.ctlWebApiServices_WebApiIsLinkedToSkill, nameOfSkillLinkedToWebApi)
            Dim popUp = New YesNoPopupForm(My.Resources.frmAdvSearch_WebAPIServices, message)
            popUp.ShowDialog()
            Select Case popUp.DialogResult
                Case DialogResult.Yes
                    UpdateSelectedWebAPIService(displayWarning:=True)
                Case DialogResult.No
                    Return
                Case Else
                    Return
            End Select
        End If
    End Sub

    Private Sub dataGridViewWebApi_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles dataGridViewWebApi.DoubleClick
        ValidateEditWarningShouldBeDisplayed()
    End Sub

    Private Sub UpdateSelectedWebAPIService(displayWarning As Boolean)

        Select Case dataGridViewWebApi.SelectedRows.Count
            Case 0
                UserMessage.Show(SystemManagerObjects_Resources.ctlWebApiYouMustSelectOneApiToUpdate)
            Case 1
                Using updateForm = CreateWebApiForm(ServiceAction.Update)
                    updateForm.SetEnvironmentColours(mParent)
                    updateForm.SetWarningLabelLinkedToSkillVisibility(displayWarning)
                    Dim result = updateForm.ShowDialog()
                    If result = DialogResult.OK Then
                        Try
                            Dim service As WebApi = updateForm.Service
                            gSv.SaveWebApi(service)
                            UpdateDataTable()
                        Catch ex As Exception
                            UserMessage.Err(ex,
                         SystemManagerObjects_Resources.ctlWebApiFailedToUpdateWebApi,
                         ex.Message)
                        End Try
                    End If
                End Using
            Case Else
                UserMessage.Show(SystemManagerObjects_Resources.ctlWebApiCanOnlyConfigureOneAtATime)
        End Select

    End Sub

    Private Sub llDelete_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles llDelete.LinkClicked

        Dim apisToDelete = dataGridViewWebApi.SelectedRows

        If dataGridViewWebApi.SelectedRows.Count = 0 Then
            UserMessage.Show(SystemManagerObjects_Resources.ctlWebApiPleaseSelectOneOrMoreToDelete)
            Return
        End If

        If ValidateSelectionNotLinkedToSkill() Then
            'Build dependency list to check references for
            Dim dependancyList As New clsProcessDependencyList()
            For Each api As DataGridViewRow In apisToDelete
                dependancyList.Add(New clsProcessWebApiDependency(GetWebApiName(api)))
            Next
            'Return if user cancelled deletion
            If Not mParent.ConfirmDeletion(dependancyList) Then Return

            'If ok to proceed then delete the selected web APIs
            For Each row In dataGridViewWebApi.SelectedRows.Cast(Of DataGridViewRow).Where(Function(x) x.Selected)
                Dim name = GetWebApiName(row)
                Dim webApiId = gSv.GetWebApiId(name)

                Try
                    gSv.DeleteWebApi(webApiId, name)
                Catch ex As Exception
                    UserMessage.Err(SystemManagerObjects_Resources.ctlWebApiFailedToRemoveWebApi, ex.Message)
                End Try
            Next
        End If

        UpdateDataTable()
    End Sub

    Private Function ValidateSelectionNotLinkedToSkill() As Boolean
        Dim linkedWebApis As New StringBuilder()

        Try
            For Each api As DataGridViewRow In dataGridViewWebApi.SelectedRows
                Dim webApiName = GetWebApiName(api)
                Dim template = "{0} - Version:{1}"
                Dim linkedSkills = gSv.GetSkillsWithVersionLinkedToWebApi(webApiName, template)

                If linkedSkills?.Any() Then
                    If linkedWebApis.Length = 0 Then linkedWebApis.Append(Environment.NewLine)
                    linkedWebApis.AppendLine(webApiName)

                    For Each skillVersion In linkedSkills.ToList()
                        linkedWebApis.AppendLine($"{vbTab}{skillVersion}")
                    Next
                End If
            Next

            If linkedWebApis.Length = 0 Then
                Return True
            Else
                UserMessage.Show(String.Format(
                    SystemManagerObjects_Resources.ctlWebServicesViewServicesCouldNotBeDeleted, linkedWebApis.ToString()))
                Return False
            End If
        Catch ex As Exception
            UserMessage.Err($"Unable to validate Web API: {ex.Message}", ex)
        End Try
    End Function

    Private Sub llFindReferences_LinkClicked(
     sender As Object, e As LinkLabelLinkClickedEventArgs) _
     Handles llFindReferences.LinkClicked

        If dataGridViewWebApi.SelectedRows.Count <> 1 Then
            UserMessage.Show(SystemManagerObjects_Resources.ctlWebApiPleaseSelectASingleWebApi)
            Return
        End If

        Dim selectedWebApiName = GetWebApiName(dataGridViewWebApi.SelectedRow)
        mParent.FindReferences(New clsProcessWebApiDependency(selectedWebApiName))
    End Sub

    Private Sub dataGridViewWebApi_CellValueChanged(sender As Object, e As DataGridViewCellEventArgs) Handles dataGridViewWebApi.CellValueChanged

        If (e.ColumnIndex = dataGridViewWebApi.Columns("Enabled").Index And e.RowIndex <> -1) Then
            Dim row As DataGridViewRow = dataGridViewWebApi.Rows(e.RowIndex)
            Dim checked = Boolean.Parse(row.Cells("Enabled").Value.ToString())
            Dim webApiId = gSv.GetWebApiId(GetWebApiName(row))

            Try
                gSv.UpdateWebApiEnabled(webApiId, checked)
            Catch ex As Exception
                UserMessage.Err(SystemManagerObjects_Resources.ctlWebApiFailedToUpdateEnabledState, ex.Message)
            End Try 
        End If

    End Sub

    ''' <summary>
    ''' Event handler that is required for checkbox ticked event handlers to trigger
    ''' </summary>
    Private Sub dataGridViewWebApi_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) _
        Handles dataGridViewWebApi.CellContentClick
        dataGridViewWebApi.CommitEdit(DataGridViewDataErrorContexts.Commit)
    End Sub


    ''' <summary>
    ''' Creates the Web API form which can deal with the given service action.
    ''' </summary>
    ''' <param name="action">The action to determine the state of the WebApi form
    ''' which should be created. This should either be
    ''' <see cref="ServiceAction.Add"/> or <see cref="ServiceAction.Update"/> to
    ''' create or edit a Web API, respectively.</param>
    ''' <returns>The WebApiForm which can deal with the given action.</returns>
    ''' <exception cref="InvalidArgumentException">If the given action was not one of
    ''' <see cref="ServiceAction.Add"/> or <see cref="ServiceAction.Update"/>.
    ''' </exception>
    Private Function CreateWebApiForm(action As ServiceAction) As WebApiForm
        If action = ServiceAction.Delete OrElse action = ServiceAction.Find Then
            Throw New InvalidArgumentException(
                SystemManagerObjects_Resources.ctlWebApiInvalidActionForForm, action)
        End If

        Dim service As WebApi = Nothing
        If action = ServiceAction.Update Then
            If dataGridViewWebApi.SelectedRow Is Nothing Then
                UserMessage.Err(
                 SystemManagerObjects_Resources.ctlWebApiPleaseSelectASingleWebApi)
                Return Nothing
            End If

            Dim webApiName = GetWebApiName(dataGridViewWebApi.SelectedRow)
            Dim selectedWebApiId = gSv.GetWebApiId(webApiName)
            service = gSv.GetWebApi(selectedWebApiId)
        End If

        Return New WebApiForm() With {
            .IsUniqueNameDelegate = AddressOf CheckValidName,
            .Service = service
        }
    End Function


    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(
                Permission.SystemManager.BusinessObjects.WebServicesWebApi)
        End Get
    End Property

    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "Web API/HTML/web-api.htm"
    End Function

    Private Sub UpdateDataTable()
        'Add any initialization after the InitializeComponent() call
        If Not LicenseManager.UsageMode = LicenseUsageMode.Runtime Then Return

        Dim dataView = New DataView
        Dim dataTable = New DataTable("Web Services")
        dataTable.Columns.Add(New DataColumn("Enabled", GetType(Boolean)))
        dataTable.Columns.Add(New DataColumn("Name", GetType(String)))
        dataTable.Columns.Add(New DataColumn("Number of Actions", GetType(String)))
        dataTable.Columns.Add(New DataColumn("Last Updated", GetType(String)))
        dataTable.Columns.Add(New DataColumn("Skills", GetType(String)))

        Dim webApis = Enumerable.Empty(Of WebApiListItem)
        Try
            webApis = gSv.GetWebApiListItems()
        Catch ex As Exception
            UserMessage.Err(ex,
                SystemManagerObjects_Resources.ctlWebApiFailedToRetrieveList,
                ex.Message)
        End Try

        For Each webService In webApis
            Dim row = dataTable.NewRow()
            row("Enabled") = webService.Enabled
            row("Name") = webService.Name
            row("Number of Actions") = webService.NumberOfActions.ToString()
            row("Last Updated") = webService.LastUpdated.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern)
            row("Skills") = String.Join(", ", webService.AssociatedSkills)
            dataTable.Rows.Add(row)
        Next

        dataView.Table = dataTable
        dataGridViewWebApi.DataSource = dataView
        SetupDataTable()
        EnableLinks()
    End Sub

    Private Sub SetupDataTable()
        dataGridViewWebApi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        dataGridViewWebApi.Columns(0).FillWeight = 60
        dataGridViewWebApi.Columns(1).FillWeight = 200
        dataGridViewWebApi.Columns(2).FillWeight = 100
        dataGridViewWebApi.Columns(3).FillWeight = 100
        dataGridViewWebApi.Columns(4).FillWeight = 100

        dataGridViewWebApi.Columns(0).ReadOnly = False
        dataGridViewWebApi.Columns(1).ReadOnly = True
        dataGridViewWebApi.Columns(2).ReadOnly = True
        dataGridViewWebApi.Columns(3).ReadOnly = True
        dataGridViewWebApi.Columns(4).ReadOnly = True

        dataGridViewWebApi.Columns(0).HeaderText = My.Resources.ctlWebApiServices_Enabled
        dataGridViewWebApi.Columns(1).HeaderText = My.Resources.ctlWebApiServices_Name
        dataGridViewWebApi.Columns(2).HeaderText = My.Resources.ctlWebApiServices_NumberOfActions
        dataGridViewWebApi.Columns(3).HeaderText = My.Resources.ctlWebApiServices_LastUpdated
        dataGridViewWebApi.Columns(4).HeaderText = My.Resources.ctlWebApiServices_Skills


        dataGridViewWebApi.Sort(dataGridViewWebApi.Columns(0), ListSortDirection.Ascending)
    End Sub
    Private Sub EnableLinks()
        Dim enableLinks = dataGridViewWebApi.Rows.Count > 0
        llDelete.Enabled = enableLinks
        llEdit.Enabled = enableLinks
        llFindReferences.Enabled = enableLinks
    End Sub

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Return True
    End Function

End Class

