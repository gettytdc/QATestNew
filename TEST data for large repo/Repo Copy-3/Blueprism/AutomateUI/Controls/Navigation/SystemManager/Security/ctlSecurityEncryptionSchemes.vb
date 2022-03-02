Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib
Imports AutomateControls
Imports LocaleTools
Imports AutomateControls.Forms


Public Class ctlSecurityEncryptionSchemes
    Implements IChild, IPermission, IHelp, IStubbornChild

    Private mSorter As clsListViewSorter
    Private mLastSelectedDefaultScheme As Integer

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        PopulateSchemes()

        Me.Enabled = Licensing.License.CanUse(LicenseUse.Credentials) AndAlso
            User.Current.HasPermission(Permission.SystemManager.Security.ManageEncryptionSchemes)
    End Sub

    Private Sub HandlesAddScheme(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles llSchemeAdd.LinkClicked
        Dim f As New frmEncryptKey()
        f.EnvironmentBackColor = ParentAppForm.EnvironmentBackColor
        f.EnvironmentForeColor = ParentAppForm.EnvironmentForeColor
        If f.ShowDialog() = DialogResult.OK Then
            PopulateSchemes()
        End If
    End Sub

    Private Sub HandleSchemeDoubleClick(ByVal sender As Object, ByVal e As DataGridViewCellEventArgs) Handles dgvKeys.CellDoubleClick
        If e.RowIndex < 0 Then Return
        Dim row As DataGridViewRow = dgvKeys.Rows(e.RowIndex)
        EditScheme(CType(row.Tag, clsEncryptionScheme))
    End Sub

    Private Sub HandlesEditScheme(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles llSchemeEdit.LinkClicked
        Dim row As DataGridViewRow = dgvKeys.SelectedRows(0)
        EditScheme(CType(row.Tag, clsEncryptionScheme))
    End Sub

    Private Sub HandlesDeleteScheme(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles llSchemeDelete.LinkClicked
        Dim row As DataGridViewRow = dgvKeys.SelectedRows(0)
        Dim schemeNoKey As clsEncryptionScheme = CType(row.Tag, clsEncryptionScheme)

        Dim msg As String = String.Format(
         My.Resources.ctlSecurityEncryptionSchemes_AreYouSureYouWantToDeleteEncryptionScheme0, schemeNoKey.Name)
        If UserMessage.OkCancel(msg) <> MsgBoxResult.Ok Then Return

        Try
            gSv.DeleteEncryptionScheme(schemeNoKey)
            PopulateSchemes()
        Catch ioe As InvalidOperationException
            UserMessage.Show(ioe.Message)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlSecurityEncryptionSchemes_FailedToDeleteEncryptionScheme0, ex.Message), ex)
        End Try
    End Sub

    Private Sub HandleCredentialEncChange(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbDefaultEncrypter.SelectionChangeCommitted
        Dim item As ComboBoxItem = DirectCast(cmbDefaultEncrypter.SelectedItem, ComboBoxItem)
        mLastSelectedDefaultScheme = cmbDefaultEncrypter.SelectedIndex
        gSv.SetDefaultEncrypter(CInt(item.Tag), item.Text)
    End Sub

    Private Sub PopulateSchemes()
        dgvKeys.Rows.Clear()
        cmbDefaultEncrypter.Items.Clear()
        For Each schemeNoKey As clsEncryptionScheme In gSv.GetEncryptionSchemesExcludingKey()
            Dim i As Integer = dgvKeys.Rows.Add(schemeNoKey.Name)
            'Get the algorithm name from the server as we don't have the key here to verify if it's valid, so would only ever return <unresolvedKey>
            Dim algorithmName As String = gSv.GetAlgorithmName(schemeNoKey)
            If Not schemeNoKey.FIPSCompliant Then
                algorithmName += My.Resources.NotFIPSCompliant
            End If

            dgvKeys.Rows(i).Cells("colMethod").Value = algorithmName
            dgvKeys.Rows(i).Cells("colLocation").Value = clsEncryptionScheme.GetLocalizedFriendlyName(schemeNoKey.KeyLocation.ToString())
            dgvKeys.Rows(i).Cells("colStatus").Value = IIf(schemeNoKey.IsAvailable, My.Resources.ctlSecurityEncryptionSchemes_Available, My.Resources.ctlSecurityEncryptionSchemes_Unavailable)
            dgvKeys.Rows(i).Tag = schemeNoKey

            If schemeNoKey.IsAvailable Then
                Dim encryptionSchemeName = schemeNoKey.Name
                If encryptionSchemeName <> clsEncryptionScheme.DefaultEncryptionSchemeName Then
                    encryptionSchemeName = LTools.GetC(schemeNoKey.Name, "misc", "crypto")
                End If

                Dim scheme = New ComboBoxItem(String.Format(My.Resources.ctlSecurityEncryptionSchemes_AlgorName0LocationName1, encryptionSchemeName, algorithmName),
                                              schemeNoKey.ID) With {
                    .Selectable = schemeNoKey.FIPSCompliant,
                    .Enabled = schemeNoKey.FIPSCompliant
                                              }
                cmbDefaultEncrypter.Items.Add(scheme)
            End If

        Next
        Dim defaultEncryptionSchemeIndex = gSv.GetDefaultEncrypter()
        cmbDefaultEncrypter.SelectedItem = cmbDefaultEncrypter.FindComboBoxItemByTag(defaultEncryptionSchemeIndex)
        mLastSelectedDefaultScheme = defaultEncryptionSchemeIndex

    End Sub

    Private Sub EditScheme(ByVal schemeNoKey As clsEncryptionScheme)
        Dim f As New frmEncryptKey()
        f.EnvironmentBackColor = ParentAppForm.EnvironmentBackColor
        f.EnvironmentForeColor = ParentAppForm.EnvironmentForeColor
        ' schemes in the grid have had their key missed off for security.
        ' refetch the scheme object if it is stored in the db and the user can see it.
        If schemeNoKey.KeyLocation = EncryptionKeyLocation.Database Then
            f.Encrypter = gSv.GetEncryptionSchemeByName(schemeNoKey.Name)
        Else
            f.Encrypter = schemeNoKey
        End If

        If f.ShowDialog() = DialogResult.OK Then
            PopulateSchemes()
        End If
    End Sub

    Private Sub CmbDefaultEncrypter_LostFocus Handles cmbDefaultEncrypter.LostFocus
        If cmbDefaultEncrypter.SelectedItem.ToString.Contains(My.Resources.NotFIPSCompliant) Then
            Dim comboBoxItem As ComboBoxItem = cmbDefaultEncrypter.FindComboBoxItemByTag(mLastSelectedDefaultScheme)
            If comboBoxItem Is Nothing Then

                Dim found = cmbDefaultEncrypter.Items.Cast(Of ComboBoxItem).FirstOrDefault(Function(c) Not c.Text.Contains(My.Resources.NotFIPSCompliant))
                If found IsNot Nothing Then
                    cmbDefaultEncrypter.SelectedItem = found
                End If
                'do we need to handle the edge case that nothing is in the list?
            Else
                cmbDefaultEncrypter.SelectedItem = comboBoxItem
            End If

        End If
    End Sub

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        If User.LoggedIn
            dim userManageEncryptionSchemes = User.Current.EffectivePermissions.Any(Function(p) p.Name = Permission.SystemManager.Security.ManageEncryptionSchemes)

            If Not userManageEncryptionSchemes
                Return True
            End If
        End If
        'If the user has clicked to close the parent form do not show the popup form, allowing the application to close
        If cmbDefaultEncrypter.SelectedItem.ToString.Contains(My.Resources.NotFIPSCompliant) AndAlso User.LoggedIn AndAlso Not mParent.ApplicationFormClosing Then
            Dim popup = New PopupForm(My.Resources.FIPSPolicyEnabled, My.Resources.DefaultEncryptionScheme_NonFIPSSchemeSelected, My.Resources.btnOk)
            AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
            popup.ShowDialog()
            Return False
        End If
        Return True
    End Function

    Private Sub HandleOnBtnOKClick(sender As Object, e As EventArgs)
        Dim popup = CType(sender, PopupForm)
        RemoveHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOKClick
        popup.Close()
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
            Return Permission.ByName(Permission.SystemManager.Security.ViewEncryptionSchemes)
        End Get
    End Property

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "helpEncryption.htm"
    End Function

End Class
