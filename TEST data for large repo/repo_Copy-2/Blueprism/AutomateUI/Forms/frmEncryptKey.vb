
Imports BluePrism.AutomateAppCore.app
Imports BluePrism.AutomateAppCore
Imports AutomateControls
Imports BluePrism.BPCoreLib
Imports BluePrism.Common.Security
Imports BluePrism.Server.Domain.Models
Imports LocaleTools

Public Class frmEncryptKey
    Implements IEnvironmentColourManager

    ''' <summary>
    ''' Represents the encryption scheme
    ''' </summary>
    Public Property Encrypter() As clsEncryptionScheme
        Get
            Return mEncrypter
        End Get
        Set(ByVal value As clsEncryptionScheme)
            mEncrypter = value
            mOriginalLocation = mEncrypter.KeyLocation
            PopulateUI()
        End Set
    End Property
    Private mEncrypter As clsEncryptionScheme
    Private mOriginalLocation As EncryptionKeyLocation
    Private mLastSelectedDefaultAlgorithm As Integer

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        'Setup algorithm combo
        For Each alg As EncryptionAlgorithm In clsEncryptionScheme.GetOrderedAlgorithms()
            Dim cmbItem As New ComboBoxItem(alg.GetFriendlyName(True), alg)
            If Not clsFIPSCompliance.CheckForFIPSCompliance(alg)
                cmbItem.Text += My.Resources.NotFIPSCompliant 
                cmbItem.Selectable = False
                cmbItem.Enabled = False
            End If
            cmbAlgorithm.Items.Add(cmbItem)
        Next

        cmbAlgorithm.SelectedIndex = 0
        mLastSelectedDefaultAlgorithm = 0

        'Default to hide keys
        txtKey.UseSystemPasswordChar = True

        'Default to App Server
        rbAppServer.Checked = True
    End Sub

    Private Sub PopulateUI()
        If mEncrypter.Name = clsEncryptionScheme.DefaultEncryptionSchemeName Then
            txtName.Text = mEncrypter.Name
        Else
            txtName.Text = LTools.GetC(mEncrypter.Name, "misc", "crypto")
        End If

        If mEncrypter.IsAvailable Then
            chkAvailable.Checked = True
            'If scheme used in config then it can't be made unavailable
            chkAvailable.Enabled = Not gSv.EncryptionSchemeInUse(mEncrypter.ID)
        End If
        If mEncrypter.KeyLocation = EncryptionKeyLocation.Server Then
            rbAppServer.Checked = True
        Else
            rbDatabase.Checked = True
            'Get the algorithm name from the server as we don't have the key 
            'here to verify if it's valid, so would only ever return <unresolvedKey>
            cmbAlgorithm.SelectedIndex =
             cmbAlgorithm.FindString(gSv.GetAlgorithmName(mEncrypter))

            'Crossing the boundary here to where revealing the encryption key
            'is a "UI feature" (no other way to put one in or get one out).
            Dim plainKey As New StringBuilder()
            Using pinned = mEncrypter.Key.Pin()
                For Each c As Char In pinned.Chars
                    plainKey.Append(c)
                Next
            End Using
            txtKey.Text = plainKey.ToString()

            'Key cannot be changed once created
            txtKey.ReadOnly = True
            cmbAlgorithm.Enabled = False
            llGenerateKey.Enabled = False
        End If
    End Sub

    Private Sub HandleLocationChange(ByVal sender As Object, ByVal e As EventArgs) _
     Handles rbAppServer.CheckedChanged, rbDatabase.CheckedChanged
        If rbAppServer.Checked Then
            lblMessage.Visible = True
            lblMethod.Visible = False
            cmbAlgorithm.Visible = False
            lblKey.Visible = False
            txtKey.Visible = False
            chkShowKey.Visible = False
            llGenerateKey.Visible = False
            lblRetiredScheme.Visible = False
        Else
            lblMessage.Visible = False
            lblMethod.Visible = True
            cmbAlgorithm.Visible = True
            lblKey.Visible = True
            txtKey.Visible = True
            chkShowKey.Visible = True
            llGenerateKey.Visible = True
        End If
    End Sub

    ''' <summary>
    ''' Show a warning if a retired scheme is selected.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub cmbAlgorithm_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cmbAlgorithm.SelectedIndexChanged
        If (cmbAlgorithm.SelectedItem IsNot Nothing) Then
            Dim item As ComboBoxItem = TryCast(cmbAlgorithm.SelectedItem, ComboBoxItem)

            If item IsNot Nothing AndAlso item.Tag IsNot Nothing Then
                Dim alg = CType(item.Tag, EncryptionAlgorithm)
                lblRetiredScheme.Visible = alg.IsRetired()
            End If

            mLastSelectedDefaultAlgorithm = cmbAlgorithm.SelectedIndex
        End If
    End Sub

    Private Sub HandleToggleShowKey(ByVal sender As Object, ByVal e As EventArgs) Handles chkShowKey.CheckedChanged
        txtKey.UseSystemPasswordChar = Not chkShowKey.Checked
    End Sub

    Private Sub HandleGenerateKey(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) Handles llGenerateKey.LinkClicked
        Dim scheme As New clsEncryptionScheme(txtName.Text)
        Dim item As ComboBoxItem = CType(cmbAlgorithm.SelectedItem, ComboBoxItem)
        scheme.Algorithm = CType(item.Tag, EncryptionAlgorithm)
        scheme.GenerateKey()

        'Crossing the boundary here to where revealing the encryption key
        'is a "UI feature" (no other way to put one in or get one out).
        Dim plainKey As New StringBuilder()
        Using pinned = scheme.Key.Pin()
            For Each c As Char In pinned.Chars
                plainKey.Append(c)
            Next
        End Using
        txtKey.Text = plainKey.ToString()

    End Sub

    Private Sub HandleOK(ByVal sender As Object, ByVal e As EventArgs) Handles btnOK.Click
        If mEncrypter Is Nothing Then mEncrypter = New clsEncryptionScheme()
        mEncrypter.Name = txtName.Text
        mEncrypter.IsAvailable = chkAvailable.Checked
        If rbAppServer.Checked Then
            mEncrypter.KeyLocation = EncryptionKeyLocation.Server
            mEncrypter.Key = Nothing
            mEncrypter.Algorithm = Nothing
        Else
            mEncrypter.KeyLocation = EncryptionKeyLocation.Database
            mEncrypter.Key = New SafeString(txtKey.Text)
            Dim item As ComboBoxItem = CType(cmbAlgorithm.SelectedItem, ComboBoxItem)
            mEncrypter.Algorithm = CType(item.Tag, EncryptionAlgorithm)
        End If

        If mEncrypter.Name = String.Empty Then
            UserMessage.Show(My.Resources.PleaseEnterANameForThisEncryptionScheme)
            Return
        ElseIf mEncrypter.KeyLocation = EncryptionKeyLocation.Database _
         AndAlso Not mEncrypter.HasValidKey Then
            UserMessage.Show(My.Resources.KeyNotValidForSelectedEncryptionMethod)
            Return
        ElseIf mEncrypter.InDatabase AndAlso
         mOriginalLocation = EncryptionKeyLocation.Database AndAlso
         mEncrypter.KeyLocation = EncryptionKeyLocation.Server Then
            If UserMessage.TwoButtonsWithCustomText(My.Resources.ChangingTheKeyLocationFromDatabaseToApplicationServerWillResultInTheKeyBeingRem, My.Resources.Proceed, My.Resources.Cancel) <> MsgBoxResult.Yes Then Return
        End If

        Try
            gSv.StoreEncryptionScheme(mEncrypter)
        Catch exists As NameAlreadyExistsException
            UserMessage.Show(String.Format(
               My.Resources.AnEncryptionSchemeWithTheName0AlreadyExists, mEncrypter.Name))
            Return
        Catch ex As Exception
            UserMessage.Show(My.Resources.FailedToStoreEncryptionScheme, ex)
        End Try

        DialogResult = DialogResult.OK
        Close()
    End Sub

    Private Sub HandleCancel(ByVal sender As Object, ByVal e As EventArgs) Handles btnCancel.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    Private Sub CmbAlgorithm_LostFocus Handles cmbAlgorithm.LostFocus
        If cmbAlgorithm.SelectedItem.ToString.Contains(My.Resources.NotFIPSCompliant)
            cmbAlgorithm.SelectedIndex = mLastSelectedDefaultAlgorithm
        End If
    End Sub


#Region "IEnvironmentColourManager implementation"

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return tBar.BackColor
        End Get
        Set(value As Color)
            tBar.BackColor = value
        End Set
    End Property

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return tBar.TitleColor
        End Get
        Set(value As Color)
            tBar.TitleColor = value
        End Set
    End Property

#End Region

End Class
