Imports System.Security.Cryptography.X509Certificates
Imports BluePrism.Common.Security

Public Class ctlChooseWebServiceCredentials

    Private Enum StoreLocationDropdownItems
        CurrentUser = 1
        LocalMachine = 2
        File = 3
    End Enum

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        Dim delta As Integer = lblStoreLocation.Right - cmbStoreLocation.Left
        delta = lblStoreName.Right - cmbStoreName.Left
        cmbStoreName.Left += delta
        cmbStoreName.Width -= delta
        delta = lblFindType.Right - cmbFindType.Left
        cmbFindType.Left += delta
        cmbFindType.Width -= delta

        For Each storeName As StoreName In [Enum].GetValues(GetType(StoreName))
            cmbStoreName.Items.Add(storeName)
        Next

        For Each storeLocation As StoreLocationDropdownItems In [Enum].GetValues(GetType(StoreLocationDropdownItems))
            Dim resource = My.Resources.ResourceManager.GetString($"ctlChooseWebServiceCredentials_WebServiceStoreLocation_{storeLocation}")
            cmbStoreLocation.Items.Add(resource)
        Next

        For Each findType As X509FindType In [Enum].GetValues(GetType(X509FindType))
            cmbFindType.Items.Add(findType)
        Next
    End Sub
    ''' <summary>
    ''' Event handler for the needs username and password checkbox
    ''' </summary>
    Private Sub chkNeedsUsernameAndPassword_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkNeedsUsernameAndPassword.CheckedChanged
        CheckSettings()
    End Sub


    Private Sub txtUsername_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtUsername.TextChanged
        CheckSettings()
    End Sub


    Private Sub txtPassword_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtPassword.TextChanged
        CheckSettings()
    End Sub

    Private Sub chkNeedsClientSSL_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles chkNeedsClientSSL.CheckedChanged
        CheckSettings()
    End Sub

    Private Sub cmbStoreLocation_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbStoreLocation.SelectedIndexChanged
        CheckSettings()
    End Sub

    Private Sub txtSearchLocation_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles txtSearchLocation.TextChanged
        CheckSettings()
    End Sub

    Private Sub cmbStoreName_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbStoreName.SelectedIndexChanged
        CheckSettings()
    End Sub

    Private Sub cmbFindType_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbFindType.SelectedIndexChanged
        CheckSettings()
    End Sub

    Private Sub CheckSettings()
        Dim usepassword As Boolean = chkNeedsUsernameAndPassword.Checked
        txtUsername.Enabled = usepassword
        txtPassword.Enabled = usepassword
        lblUsername.Enabled = usepassword
        lblPassword.Enabled = usepassword

        Dim usessl As Boolean = chkNeedsClientSSL.Checked
        If Not usessl Then
            cmbStoreLocation.Text = ""
            cmbStoreName.Text = ""
            cmbFindType.Text = ""
            txtSearchLocation.Text = ""
            txtCertPassword.SecurePassword = New SafeString
        End If
        lblStoreLocation.Enabled = usessl
        cmbStoreLocation.Enabled = usessl

        lblSearchLocation.Enabled = usessl
        txtSearchLocation.Enabled = usessl
        txtCertPassword.Enabled = usessl
        lblCertPassword.Enabled = usessl
        lblStoreName.Enabled = usessl
        cmbStoreName.Enabled = usessl
        lblFindType.Enabled = usessl
        cmbFindType.Enabled = usessl
        btnChange.Enabled = False

        Dim storeLocation As StoreLocationDropdownItems

        For Each item As StoreLocationDropdownItems In System.Enum.GetValues(GetType(StoreLocationDropdownItems))
            Dim stringValue = My.Resources.ResourceManager.GetString($"ctlChooseWebServiceCredentials_WebServiceStoreLocation_{item}")
            If stringValue Is cmbStoreLocation.SelectedItem Then
                storeLocation = item
                Exit For
            End If
        Next

        Select Case storeLocation
            Case StoreLocationDropdownItems.CurrentUser, StoreLocationDropdownItems.LocalMachine
                btnBrowse.Enabled = False
                lblSearchLocation.Text = My.Resources.ctlChooseWebServiceCredentials_FindCriteria
            Case StoreLocationDropdownItems.File
                lblStoreName.Enabled = False
                cmbStoreName.Enabled = False
                lblFindType.Enabled = False
                cmbFindType.Enabled = False
                btnBrowse.Enabled = True
                lblSearchLocation.Text = My.Resources.ctlChooseWebServiceCredentials_FileLocation
        End Select

        Dim storeName = CType(cmbStoreName.SelectedItem, StoreName)
        Dim findType = CType(cmbFindType.SelectedItem, X509FindType)

        Dim topSectionOk As Boolean = Not usepassword OrElse Not String.IsNullOrEmpty(txtUsername.Text) AndAlso txtPassword.SecurePassword.Length > 0
        Dim bottomSectionOk As Boolean = Not usessl OrElse (storeLocation = StoreLocationDropdownItems.File AndAlso Not String.IsNullOrEmpty(txtSearchLocation.Text)) OrElse (Not String.IsNullOrEmpty(txtSearchLocation.Text) AndAlso storeName > 0 AndAlso cmbFindType.SelectedItem IsNot Nothing)

        NavigateNext = topSectionOk AndAlso bottomSectionOk

        UpdateNavigate()
    End Sub

    Public Function GetCertificate() As X509Certificate2

        If Not chkNeedsClientSSL.Checked Then Return Nothing

        Dim storeLocationItem = CType([Enum].Parse(GetType(StoreLocationDropdownItems),
                                                    cmbStoreLocation.SelectedItem.ToString), StoreLocationDropdownItems)
        Select Case storeLocationItem
            Case StoreLocationDropdownItems.CurrentUser
                Return OpenFromStore(StoreLocation.CurrentUser)
            Case StoreLocationDropdownItems.LocalMachine
                Return OpenFromStore(StoreLocation.LocalMachine)
            Case StoreLocationDropdownItems.File
                Return New X509Certificate2(txtSearchLocation.Text, txtCertPassword.SecurePassword)
        End Select

        Return Nothing
    End Function

    Private Function OpenFromStore(storeLocation As StoreLocation) As X509Certificate2
        Dim storeName = CType(cmbStoreName.SelectedItem, StoreName)
        Dim findType = CType(cmbFindType.SelectedItem, X509FindType)

        Using store As New X509Store(storeName, storeLocation)
            store.Open(OpenFlags.ReadOnly)
            Dim certs As X509Certificate2Collection = store.Certificates.Find(findType, txtSearchLocation.Text, True)
            If certs.Count = 1 Then
                Return certs.Item(0)
            ElseIf certs.Count > 1 Then
                Throw New InvalidOperationException(My.Resources.ctlChooseWebServiceCredentials_MultipleCertificatesWereFoundMatchingTheCriteria)
            Else
                certs = store.Certificates.Find(findType, txtSearchLocation.Text, False)
                If certs.Count > 0 Then
                    Throw New InvalidOperationException(My.Resources.ctlChooseWebServiceCredentials_NoValidCertificateFoundMatchingThatCriteria)
                End If
                Throw New InvalidOperationException(My.Resources.ctlChooseWebServiceCredentials_NoCertificateFoundMatchingThatCriteria)
            End If
        End Using
    End Function

    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowse.Click
        If dlgCertificate.ShowDialog = DialogResult.OK Then
            txtSearchLocation.Text = dlgCertificate.FileName
        End If
    End Sub

    Public Property HasCertificate() As Boolean
        Get
            Return mHasCertificate
        End Get
        Set(ByVal value As Boolean)
            mHasCertificate = value
            CheckSettings()
            btnChange.Enabled = value
            lblCertPassword.Enabled = Not value
            txtCertPassword.Enabled = Not value
            lblSearchLocation.Enabled = Not value
            txtSearchLocation.Enabled = Not value
            lblStoreLocation.Enabled = Not value
            cmbStoreLocation.Enabled = Not value
            lblStoreName.Enabled = Not value
            cmbStoreName.Enabled = Not value
            lblFindType.Enabled = Not value
            cmbFindType.Enabled = Not value
        End Set
    End Property
    Private mHasCertificate As Boolean

    Private Sub btnChange_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnChange.Click
        HasCertificate = False
    End Sub
End Class
