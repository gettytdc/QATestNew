Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore.clsServer
Imports BluePrism.AutomateAppCore.Auth

Public Class ctlWebServices : Implements IChild, IPermission, IHelp

    Private Sub llUpdate_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles llUpdate.LinkClicked
        Select Case Me.objWebServicesView.dataGridViewBusinessObjects.SelectedRows.Count
            Case 0
                UserMessage.Show(My.Resources.ctlWebServices_YouMustFirstSelectAtLeastOneWebServiceToUpdate)
            Case 1
                Using objWebConfig As New frmWebServices
                    objWebConfig.SetEnvironmentColours(mParent)
                    Dim id = objWebServicesView.WebServices.FirstOrDefault(Function(x) x.FriendlyName =
                        objWebServicesView.dataGridViewBusinessObjects.SelectedRow.Cells("Name").Value.ToString).Id
                    objWebConfig.Setup(id)
                    objWebConfig.ShowDialog()

                    If objWebConfig.DialogResult = DialogResult.OK Then
                        'save the new config to database table
                        Dim details As clsWebServiceDetails = objWebConfig.ServiceDetails
                        Try
                            gSv.SaveWebServiceDefinition(details)
                        Catch ex As Exception
                            UserMessage.Show(String.Format(My.Resources.ctlWebServices_FailedToAddTheWebServiceToTheDatabase0, ex.Message))
                        End Try

                        objWebServicesView.UpdateList()
                    End If
                End Using
            Case Else
                UserMessage.Show(My.Resources.ctlWebServices_YouCanOnlyConfigureOneWebServiceAtATime)
        End Select

    End Sub

    Private Sub llDelete_LinkClicked(ByVal sender As Object,
     ByVal e As LinkLabelLinkClickedEventArgs) Handles llDelete.LinkClicked
        Dim servicesToDelete As DataGridViewSelectedRowCollection = objWebServicesView.dataGridViewBusinessObjects.SelectedRows

        'Ensure at least one web service is selected
        If servicesToDelete.Count = 0 Then
            UserMessage.Show(My.Resources.ctlWebServices_PleaseFirstSelectOneOrMoreWebServicesToDelete)
            Return
        End If

        'Build dependency list to check references for
        Dim deps As New clsProcessDependencyList()
        For Each row As DataGridViewRow In servicesToDelete
            Dim wsName = row.Cells("Name").Value.ToString
            deps.Add(New clsProcessWebServiceDependency(wsName))
        Next
        'Return if user cancelled deletion
        If Not mParent.ConfirmDeletion(deps) Then Return

        'De-select any web services that are not to be deleted
        For Each row As DataGridViewRow In objWebServicesView.dataGridViewBusinessObjects.SelectedRows
            Dim wsName = row.Cells("Name").Value.ToString
            If Not deps.Has(New clsProcessWebServiceDependency(wsName)) Then
                row.Selected = False
            End If
        Next

        'If ok to proceed then delete the selected web services
        objWebServicesView.DeleteSelectedObjects()
        objWebServicesView.UpdateList()

    End Sub

    Private Sub llAdd_LinkClicked(ByVal sender As Object,
     ByVal e As LinkLabelLinkClickedEventArgs) Handles llAdd.LinkClicked
        Try
            Using objWebServiceAdd As New frmWebServices()
                objWebServiceAdd.SetEnvironmentColours(mParent)
                objWebServiceAdd.Setup(Guid.Empty)
                objWebServiceAdd.ShowDialog()

                If objWebServiceAdd.DialogResult = DialogResult.OK Then
                    'save the new config to database table
                    Dim details As clsWebServiceDetails = objWebServiceAdd.ServiceDetails
                    details.Id = Guid.NewGuid()
                    Try
                        gSv.SaveWebServiceDefinition(details)
                    Catch ex As Exception
                        UserMessage.Show(String.Format(My.Resources.ctlWebServices_FailedToUpdateTheWebService0, ex.Message))
                    End Try

                    objWebServicesView.UpdateList()
                End If
            End Using
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlWebServices_UnexpectedError0YourNewWebServiceMayNotHaveBeenAddedSuccessfully, ex.Message))
        End Try
    End Sub

    Private Sub llFindReferences_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) _
        Handles llFindReferences.LinkClicked

        If objWebServicesView.dataGridViewBusinessObjects.SelectedRows.Count <> 1 Then
            UserMessage.Show(My.Resources.ctlWebServices_PleaseSelectASingleWebService)
        Else
            Dim wsName = objWebServicesView.dataGridViewBusinessObjects.SelectedRow.Cells("Name").Value.ToString
            mParent.FindReferences(New clsProcessWebServiceDependency(wsName))
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

    Public ReadOnly Property RequiredPermissions() As ICollection(Of Permission) _
     Implements IPermission.RequiredPermissions
        Get
            Return Permission.ByName(Permission.SystemManager.BusinessObjects.WebServicesSoap)
        End Get
    End Property

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "frmWebServices.htm"
    End Function
End Class
