Imports AutomateUI
Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib

Public Class ctlSecurityCredentials
    Implements IPermission, IHelp, IStubbornChild

    Private mSorter As clsListViewSorter

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        PopulateCredentials(Nothing)

        Me.Enabled = Licensing.License.CanUse(LicenseUse.Credentials)
    End Sub

    Private Sub llCredAdd_LinkClicked(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles llCredAdd.LinkClicked
        Try
            CreateCredential()
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlSecurityCredentials_UnexpectedError0, ex.Message), ex)
        End Try
    End Sub

    Private Sub lstCredentials_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles lstCredentials.DoubleClick
        Try
            EditCredential()
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlSecurityCredentials_UnexpectedError0, ex.Message), ex)
        End Try
    End Sub

    Private Sub llCredEdit_LinkClicked(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles llCredEdit.LinkClicked
        Try
            EditCredential()
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlSecurityCredentials_UnexpectedError0, ex.Message), ex)
        End Try
    End Sub

    Private Sub llCredDelete_LinkClicked(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles llCredDelete.LinkClicked
        'Ensure at least one credential is selected
        If lstCredentials.SelectedItems.Count = 0 Then Return

        'Build dependency list to check references for
        Dim deps As New clsProcessDependencyList()
        For Each it As ListViewItem In lstCredentials.SelectedItems
            Dim cred As clsCredential = TryCast(it.Tag, clsCredential)
            If cred IsNot Nothing Then
                deps.Add(New clsProcessCredentialsDependency(cred.Name))
            End If
        Next
        'Return if user cancelled deletion
        If Not mParent.ConfirmDeletion(deps) Then Return

        'If ok to proceed then delete the credentials
        Dim creds As New List(Of clsCredential)
        For Each it As ListViewItem In lstCredentials.SelectedItems
            Dim cred As clsCredential = TryCast(it.Tag, clsCredential)
            If deps.Has(New clsProcessCredentialsDependency(cred.Name)) Then
                creds.Add(cred)
            End If
        Next

        Try
            gSv.DeleteCredentials(creds)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlSecurityCredentials_FailedToDeleteCredentials0, ex.Message), ex)
            Exit Sub
        End Try

        'Re-draw the listview
        PopulateCredentials(Nothing)
    End Sub

    Private Sub llFindReferences_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) _
        Handles llFindReferences.LinkClicked

        If lstCredentials.SelectedItems.Count <> 1 Then
            UserMessage.Show(My.Resources.ctlSecurityCredentials_PleaseSelectASingleCredential)
        Else
            Dim cred As clsCredential = TryCast(lstCredentials.SelectedItems(0).Tag, clsCredential)
            If cred Is Nothing Then Return

            mParent.FindReferences(New clsProcessCredentialsDependency(cred.Name))
        End If
    End Sub

    Private Sub CreateCredential()
        Dim f As New frmCredential(Nothing)
        f.SetEnvironmentColours(mParent)
        f.ShowDialog()

        If f.DialogResult = DialogResult.OK Then
            PopulateCredentials(f.Credential)
        End If
    End Sub

    Private Sub EditCredential()
        If lstCredentials.SelectedItems.Count >= 1 Then
            Dim item As ListViewItem = lstCredentials.SelectedItems(0)
            Dim credential = TryCast(item.Tag, clsCredential)

            Debug.Assert(credential IsNot Nothing)

            Try
                credential = gSv.GetCredentialForUI(credential.ID)
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.ctlSecurityCredentials_FailedToGetCredential0, ex.Message), ex)
                Exit Sub
            End Try

            Dim form As New frmCredential(credential)
            form.SetEnvironmentColours(mParent)

            If form.ShowDialog() = DialogResult.OK Then
                PopulateCredentials(Nothing)
            End If
        Else
            UserMessage.Show(My.Resources.ctlSecurityCredentials_PleaseFirstSelectACredentialToEdit)
        End If
    End Sub

    Private Sub PopulateCredentials(ByVal NewCredential As clsCredential)
        Dim PreviousSelection As New List(Of Guid)

        If NewCredential Is Nothing Then
            For Each item As ListViewItem In lstCredentials.SelectedItems
                Dim cred As clsCredential = TryCast(item.Tag, clsCredential)
                If Not cred Is Nothing Then
                    PreviousSelection.Add(cred.ID)
                End If
            Next
        End If


        Dim creds As ICollection(Of clsCredential) = Nothing
        Try
            creds = gSv.GetAllCredentialsInfo()
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlSecurityCredentials_FailedToPopulateCredentialsList0, ex.Message), ex)
            Exit Sub
        End Try

        lstCredentials.Items.Clear()

        For Each cred As clsCredential In creds

            Dim it As New ListViewItem(cred.Name)
            it.SubItems.Add(cred.Description)
            If cred.ExpiryDate <> DateTime.MinValue Then
                it.SubItems.Add(cred.ExpiryDate.ToShortDateString)
            Else
                it.SubItems.Add("")
            End If
            it.SubItems.Add(cred.GetLocalisedFriendlyName)
            it.SubItems.Add(cred.Type.LocalisedTitle)
            it.Tag = cred
            lstCredentials.Items.Add(it)
            If Not NewCredential Is Nothing Then
                If NewCredential.ID.Equals(cred.ID) Then
                    it.Selected = True
                End If
            Else
                If PreviousSelection.Contains(cred.ID) Then
                    it.Selected = True
                    it.Focused = True
                End If
            End If
        Next

        If mSorter Is Nothing Then
            mSorter = New clsListViewSorter(lstCredentials)
            mSorter.ColumnDataTypes = New Type() {
              GetType(String), GetType(String), GetType(DateTime), GetType(String)}
            mSorter.SortColumn = 0
            mSorter.Order = SortOrder.Ascending
        End If
        lstCredentials.ListViewItemSorter = mSorter

        If lstCredentials.Items.Count > 0 AndAlso lstCredentials.SelectedItems.Count = 0 Then
            lstCredentials.TopItem.Selected = True
        End If

        lstCredentials.Select()
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
            Return Permission.ByName(Permission.SystemManager.Security.Credentials)
        End Get
    End Property

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpCredentials.htm"
    End Function

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Return True
    End Function
End Class
