Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore.WebApis.Authentication

Namespace Controls.Widgets.SystemManager.WebApi.Authentication

    ''' <summary>
    ''' User control that is used to configure a credential used as part of a Web API
    ''' authentication method
    ''' </summary>
    Public Class AuthenticationCredentialPanel

        ''' <summary>
        ''' Occurs when the credential data is changed on the control
        ''' </summary>
        Public Event CredentialChanged As CredentialChangedEventHandler

        Private mCredential As AuthenticationCredential

        Private mCredentialItems As IEnumerable(Of AutomateControls.ComboBoxItem)

        Public Sub New()

            ' This call is required by the designer.
            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

        End Sub

        ''' <summary>
        ''' The underlying credential data that the control is editing
        ''' </summary>
        <Browsable(False),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public Property Credential As AuthenticationCredential
            Get
                Return mCredential
            End Get
            Set(value As AuthenticationCredential)
                mCredential = value

                chkExposeToProcess.Checked = value.ExposeToProcess
                UpdateParameterNameEnabled()

                txtParameterName.Text = value.InputParameterName

                cmbCredentialName.SelectedItem = FindCredentialInComboBox()
            End Set
        End Property

        Private Function FindCredentialInComboBox() As AutomateControls.ComboBoxItem
            Return cmbCredentialName.Items.OfType(Of AutomateControls.ComboBoxItem).
                FirstOrDefault(Function(i) i.Text = mCredential?.CredentialName)
        End Function

        ''' <summary>
        ''' The list of credentials that are used to populate the credential styled 
        ''' combo box.
        ''' </summary>
        <Browsable(False),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public WriteOnly Property Credentials As IEnumerable(Of clsCredential)
            Set(value As IEnumerable(Of clsCredential))
                mCredentialItems = value.
                                    GroupBy(Function(cred) cred.Type).
                                    SelectMany(AddressOf ConvertGroupToComboItems)

                cmbCredentialName.Items.AddRange(mCredentialItems.ToArray())
                cmbCredentialName.SelectedItem = FindCredentialInComboBox()
            End Set
        End Property

        ''' <summary>
        ''' Method to convert a grouping of clsCredential keyed on credential type into an
        ''' Enumerable of AutomateControls.ComboBoxItem.
        ''' </summary>
        ''' <param name="credentialGroup">The group of clsCredential instances. </param>
        ''' <returns>An IEnumerable of AutomateControls.ComboBoxItem. </returns>
        Private Iterator Function ConvertGroupToComboItems(credentialGroup As IGrouping(Of CredentialType, clsCredential)) _
            As IEnumerable(Of AutomateControls.ComboBoxItem)

            Dim groupHeader = New AutomateControls.ComboBoxItem(credentialGroup.Key.LocalisedTitle)
            groupHeader.Style = FontStyle.Bold
            groupHeader.Selectable = False

            Yield groupHeader

            For Each cred In credentialGroup
                Yield New AutomateControls.ComboBoxItem(cred.Name) With {.Indent = 25}
            Next

        End Function

        ''' <summary>
        ''' Gets a string representing the default Credential Parameter Name, 
        ''' according to they type of authentication being used
        ''' </summary>
        Public Property DefaultParameterName As String

        ''' <summary>
        ''' Handles the expose to process check box value changing, updates the 
        ''' underlying data, populates the parameter name text box with a default 
        ''' value if set checked and disables/clears it if set to be unchecked
        ''' </summary>
        Private Sub HandleExposeToProcessCheckedChanged() Handles chkExposeToProcess.CheckedChanged

            Dim hasDataChanged = mCredential.ExposeToProcess <> chkExposeToProcess.Checked

            If hasDataChanged Then
                UpdateParameterName()
                UpdateParameterNameEnabled()
                UpdateCredentialData()
            End If
        End Sub

        ''' <summary>
        ''' Handles the parameter name field changing and updates the underlying data
        ''' </summary>
        Private Sub HandleParameterNameTextChanged() Handles txtParameterName.TextChanged
            If txtParameterName.Text.Contains(".") Then
                UserMessage.Err(WebApi_Resources.ErrorInvalidParameterName)
                Return
            End If

            Dim hasDataChanged = mCredential.InputParameterName <> txtParameterName.Text
            If hasDataChanged Then UpdateCredentialData()
        End Sub

        ''' <summary>
        ''' Handles the credential name changing and updates the underlying data
        ''' </summary>
        Private Sub HandleCredentialNameChanged() Handles cmbCredentialName.SelectedIndexChanged
            Dim hasDataChanged = mCredential.CredentialName <> If(cmbCredentialName.SelectedItem?.ToString(), "")
            If hasDataChanged Then UpdateCredentialData()
        End Sub



        ''' <summary>
        ''' Updates the underlying credential data
        ''' </summary>
        Public Sub UpdateCredentialData()
            mCredential = New AuthenticationCredential(If(cmbCredentialName.SelectedItem?.ToString(), ""),
                                                       chkExposeToProcess.Checked,
                                                       txtParameterName.Text)

            OnCredentialChanged(New CredentialChangedEventArgs(mCredential))
        End Sub

        ''' <summary>
        ''' Raises the CredentialChanged event
        ''' </summary>
        Private Sub OnCredentialChanged(e As CredentialChangedEventArgs)
            RaiseEvent CredentialChanged(Me, e)
        End Sub

        ''' <summary>
        ''' Sets the enabled property of the parameter name control based on whether
        ''' Expose to Process is checked or unchecked
        ''' </summary>
        Private Sub UpdateParameterNameEnabled()
            txtParameterName.Enabled = chkExposeToProcess.Checked
        End Sub

        ''' <summary>
        ''' Updates the text in the parameter name text box, clearing it or 
        ''' populating it with the default based on whether Expose to Process is 
        ''' checked or unchecked
        ''' </summary>
        Private Sub UpdateParameterName()
            If chkExposeToProcess.Checked Then
                txtParameterName.Text = DefaultParameterName
            Else
                txtParameterName.Text = ""
            End If
        End Sub

        Private Sub HandleCredentialNameChanged(sender As Object, e As EventArgs) Handles cmbCredentialName.SelectedIndexChanged

        End Sub
    End Class
End Namespace