Imports System.Globalization
Imports AutomateControls
Imports AutomateUI.SystemManager
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Common.Security
Imports BluePrism.Images
Imports CustomControls
Imports LocaleTools
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.BPCoreLib.DependencyInjection
Imports AutomateControls.Textboxes
Imports BluePrism.Server.Domain.Models

''' Project  : Automate
''' Class    : frmCredentials
''' 
''' <summary>
''' Form to allow editing or creating of credentials.
''' </summary>
Friend Class frmCredential
    Implements IEnvironmentColourManager


    Private mAccessRightsTab As TabPage
    Private mIsPasswordCredentialType As Boolean

    ''' <summary>
    ''' Form constructor
    ''' </summary>
    ''' <param name="credential">The <see cref="clsCredential"/> object to display
    ''' if editing an existing instance. Can be nothing if using form to create
    ''' a new instance.</param>
    Public Sub New(credential As clsCredential)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        mCredential = If(credential, New clsCredential)

        lstProcesses.SmallImageList = ImageLists.Auth_16x16
        lstResources.SmallImageList = ImageLists.Auth_16x16
        lstRoles.SmallImageList = ImageLists.Auth_16x16

        ' Add any initialization after the InitializeComponent() call.

        mUseCultureCal = Options.Instance.UseCultureCalendar

        If (mUseCultureCal) Then
            ' Culture aware calendar control doesn't support internal checkbox so we use two controls
            cultureDpExpiryCheckBox.Location = New Point(dpExpiryDate.Location.X, dpExpiryDate.Location.Y + CInt((cultureDpExpiryDate.Height - cultureDpExpiryCheckBox.Height) / 2))
            cultureDpExpiryDate.Location = New Point(dpExpiryDate.Location.X + cultureDpExpiryCheckBox.Width + 4, dpExpiryDate.Location.Y)
            cultureDpExpiryDate.Width = (dpExpiryDate.Width - cultureDpExpiryCheckBox.Width) - 4
            cultureDpExpiryDate.Culture = New CultureInfo(CultureInfo.CurrentCulture.Name)
            dpExpiryDate.Visible = False
        Else
            cultureDpExpiryDate.Visible = False
            cultureDpExpiryCheckBox.Visible = False
        End If

        dpExpiryDate.Checked = False
        cultureDpExpiryCheckBox.Checked = False
        ToggleExpiryDate()

        PopulateProcesses()
        PopulateResources()
        PopulateRoles()
        Populate()

        mAccessRightsTab = tcMain.TabPages(tcMain.TabPages.IndexOfKey("tpRights"))
    End Sub

    Private mIsLoaded As Boolean

    Private Sub OnLoaded() Handles Me.Shown
        mIsLoaded = True
        mUserNameControlOriginalY = txtUserName.Location.Y
        mUserNameLabelOriginalY = lblUsername.Location.Y
        mPasswordControlOriginalY = pwdPassword.Location.Y
        mPasswordLabelOriginalY = lblPassword.Location.Y
        mPasswordConfirmControlOriginalY = pwdPasswordConfirm.Location.Y
        mPasswordConfirmLabelOriginalY = lblPasswordConfirm.Location.Y

        cmbType_SelectedValueChanged(Me, New EventArgs())

        'Add Handlers to catch changes to the form
        AddDirtyFormHandler(Me)
    End Sub


    ''' <summary>
    ''' Holds a credential
    ''' </summary>
    Private mCredential As New clsCredential

    Private ReadOnly mCredentialProperties As New List(Of CredentialProperty)

    Private mIsDeleting As Boolean = False

    Private mIsCancelling As Boolean = False

    Private mUserNameLabelOriginalY As Integer
    Private mUserNameControlOriginalY As Integer
    Private mPasswordLabelOriginalY As Integer
    Private mPasswordControlOriginalY As Integer
    Private mPasswordConfirmLabelOriginalY As Integer
    Private mPasswordConfirmControlOriginalY As Integer

    ''' <summary>
    ''' Flag to determine if the password for the credential has been changed.
    ''' </summary>
    Private mPasswordchanged As Boolean = False

    ' Whether to use the third party culture aware calendar as windows form version cannot have locale set
    Private mUseCultureCal As Boolean

    ''' <summary>
    ''' Provides access to the credential, which is loaded to the forms components
    ''' automatically.
    ''' </summary>
    Public ReadOnly Property Credential() As clsCredential
        Get
            Return mCredential
        End Get
    End Property

    ''' <summary>
    ''' Saves the credential from the forms components
    ''' </summary>
    Public Sub Commit()
        mCredential.Name = txtName.Text
        mCredential.Description = txtDescription.Text
        mCredential.Type = CType(cmbType.SelectedValue, CredentialType)
        mCredential.Username = txtUserName.Text

        If mPasswordchanged Then
            mCredential.Password = pwdPassword.SecurePassword
        End If
        If (mUseCultureCal) Then
            mCredential.ExpiryDate = If(cultureDpExpiryCheckBox.Checked, cultureDpExpiryDate.Value.Date, DateTime.MinValue)
        Else
            mCredential.ExpiryDate = If(dpExpiryDate.Checked, dpExpiryDate.Value.Date, DateTime.MinValue)
        End If

        mCredential.IsInvalid = cbInvalid.Checked


        If allProcesses.Checked OrElse Not mCredential.Type.IsAccessRightsVisible Then
            mCredential.ProcessIDs.Clear()
            mCredential.ProcessIDs.Add(Guid.Empty)
        Else
            ' Add/remove processes from the list as required. Note this doesn't
            ' clear the list first because there may be processes assigned to this
            ' credential that the user can't see through their access rights.
            For Each item As ListViewItem In lstProcesses.Items
                Dim id = CType(item.Tag, Guid)
                If item.Checked Then
                    If Not mCredential.ProcessIDs.Contains(id) Then _
                        mCredential.ProcessIDs.Add(id)
                Else
                    If mCredential.ProcessIDs.Contains(id) Then _
                        mCredential.ProcessIDs.Remove(id)
                End If
            Next
        End If

        If allResources.Checked OrElse Not mCredential.Type.IsAccessRightsVisible Then
            mCredential.ResourceIDs.Clear()
            mCredential.ResourceIDs.Add(Guid.Empty)
        Else
            ' Add/remove resources from the list as required. Note this doesn't
            ' clear the list first because there may be resources assigned to this
            ' credential that the user can't see through their access rights.
            For Each item As ListViewItem In lstResources.Items
                Dim id = CType(item.Tag, Guid)
                If item.Checked Then
                    If Not mCredential.ResourceIDs.Contains(id) Then _
                        mCredential.ResourceIDs.Add(id)
                Else
                    If mCredential.ResourceIDs.Contains(id) Then _
                        mCredential.ResourceIDs.Remove(id)
                End If
            Next
        End If

        mCredential.Roles.Clear()
        If allRoles.Checked OrElse Not mCredential.Type.IsAccessRightsVisible Then
            mCredential.Roles.Add(Nothing)
        Else
            For Each item As ListViewItem In lstRoles.Items
                If item.Checked Then mCredential.Roles.Add(CType(item.Tag, Role))
            Next
        End If

        mCredentialProperties.Clear()

        Dim credentialsOnGrid = dgvProperties.Rows.OfType(Of DataGridViewRow).
                                Where(Function(r) Not r.IsNewRow).
                                Select(AddressOf GetCredentialPropertyFromRow).
                                ToList()

        If IsNewCredential() Then
            mCredential.Properties.Clear()
            credentialsOnGrid.
                ForEach(Sub(p) mCredential.Properties.Add(p.NewName, p.Password))
            Return
        End If

        mCredentialProperties.InsertRange(0, credentialsOnGrid)

        '' All of the original properties that haven't yet been deleted.
        Dim remainingOriginalPropertyNames = mCredentialProperties.
                             Where(Function(p) p.OldName IsNot Nothing).
                             Select(Function(p) p.OldName)

        '' The original names not within the old names now.
        Dim deletedProperties = mCredential.Properties.Keys.
                            Except(remainingOriginalPropertyNames).
                            Select(AddressOf GetDeletedCredentialProperty)

        mCredentialProperties.InsertRange(0, deletedProperties)

    End Sub

    ''' <summary>
    ''' Creates a credential property from a string, setting it's deleted property to True.
    ''' </summary>
    ''' <param name="deletedName">The old original name of the property. </param>
    ''' <returns>A CredentialProperty. </returns>
    Private Function GetDeletedCredentialProperty(deletedName As String) As CredentialProperty

        Dim credProp = New CredentialProperty()
        credProp.OldName = deletedName
        credProp.IsDeleted = True
        Return credProp

    End Function

    ''' <summary>
    ''' Creates a credential property from a DataGridViewRow.
    ''' </summary>
    ''' <param name="row">The row containing data for the property. </param>
    ''' <returns>A Credential Property.</returns>
    Private Function GetCredentialPropertyFromRow(row As DataGridViewRow) As CredentialProperty

        Dim credProp = New CredentialProperty()

        credProp.OldName = CType(row.Tag, String)
        credProp.NewName = CStr(row.Cells(propertyName.Index).Value)

        Dim pwdCell = DirectCast(row.Cells(propertyValue.Index), PasswordCell)
        If pwdCell.HasPasswordChanged Then
            credProp.Password = pwdCell.NewPassword
        End If

        Return credProp

    End Function

    ''' <summary>
    ''' Populates the forms components from the credential
    ''' </summary>
    Public Sub Populate()

        txtName.Text = mCredential.Name
        txtDescription.Text = mCredential.Description
        txtUserName.Text = mCredential.Username
        pwdPassword.UsePlaceholder = True
        pwdPasswordConfirm.UsePlaceholder = True

        Dim items = CredentialType.GetAll().
            Select(Function(ct) New ComboBoxItem(ct.LocalisedTitle, ct)).
            ToList()
        cmbType.DataSource = items
        cmbType.SelectedValue = mCredential.Type

        If mCredential.ExpiryDate <> DateTime.MinValue Then
            dpExpiryDate.Checked = True
            cultureDpExpiryCheckBox.Checked = True
            dpExpiryDate.Value = mCredential.ExpiryDate
            cultureDpExpiryDate.Value = mCredential.ExpiryDate
        Else
            dpExpiryDate.Checked = False
            cultureDpExpiryCheckBox.Checked = False
        End If
        ToggleExpiryDate()
        cbInvalid.Checked = mCredential.IsInvalid

        mbDisallowCheck = True

        If mCredential.IsForAllProcesses Then
            allProcesses.Checked = True
            SetAllChecked(lstProcesses, allProcesses)
        Else
            For Each it As ListViewItem In lstProcesses.Items
                it.Checked = mCredential.ProcessIDs.Contains(CType(it.Tag, Guid))
            Next
        End If

        If mCredential.IsForAllResources Then
            allResources.Checked = True
            SetAllChecked(lstResources, allResources)
        Else
            For Each it As ListViewItem In lstResources.Items
                it.Checked = mCredential.ResourceIDs.Contains(CType(it.Tag, Guid))
            Next
        End If

        If mCredential.IsForAllRoles Then
            allRoles.Checked = True
            SetAllChecked(lstRoles, allRoles)
        Else
            For Each it As ListViewItem In lstRoles.Items
                it.Checked = mCredential.Roles.Contains(CType(it.Tag, Role))
            Next
        End If

        mbDisallowCheck = False

        Dim defaultSafeString = New SafeString("*")

        dgvProperties.Rows.Clear()
        For Each prop As KeyValuePair(Of String, SafeString) In
                                    mCredential.Properties.OrderBy(Function(x) x.Key)

            Dim gridViewRow = New DataGridViewRow()

            gridViewRow.CreateCells(dgvProperties,
                                    Nothing,
                                    prop.Key,
                                    defaultSafeString)

            gridViewRow.Tag = prop.Key

            dgvProperties.Rows.Add(gridViewRow)
        Next
    End Sub

    ''' <summary>
    ''' Event handler for the ok button.
    ''' </summary>
    Private Sub btnOK_Click(sender As Object, e As EventArgs) Handles btnOK.Click

        Dim credentialNameValid = IsCredentialNameValid()
        If Not credentialNameValid.IsValid Then
            UserMessage.Err(credentialNameValid.InvalidMessage)
            Return
        End If

        Dim gridInValidState = IsGridInValidState()
        If Not gridInValidState.IsValid Then
            UserMessage.Err(gridInValidState.InvalidMessage)
            Return
        End If

        If mIsPasswordCredentialType Then
            If mPasswordchanged AndAlso pwdPassword.SecurePassword <> pwdPasswordConfirm.SecurePassword Then
                UserMessage.Show(My.Resources.frmCredential_PasswordsDoNotMatch)
                Return
            End If
        End If

        Dim oldName As String = mCredential.Name
        Try
            If mFormDirty Then
                Commit()
                If IsNewCredential() Then
                    gSv.CreateCredential(Credential)
                Else
                    gSv.UpdateCredential(Credential, oldName, mCredentialProperties, mPasswordchanged)
                End If
                DialogResult = DialogResult.OK
            End If
            Close()

        Catch duplicate As NameAlreadyExistsException
            UserMessage.Show(duplicate.Message)

        Catch ex As Exception
            UserMessage.Show(String.Format(frmCredential_Resources.Err_Template_Failed_To_Save_Credential, ex.Message))

        End Try


    End Sub

    ''' <summary>
    ''' Event handler for the close button
    ''' </summary>
    Private Sub btnCancel_Click(sender As Object, e As EventArgs) Handles btnCancel.Click

        DialogResult = DialogResult.Cancel
        Close()
    End Sub


    Private Sub HandlePasswordChanged(sender As Object, e As EventArgs) Handles pwdPassword.TextChanged

        mPasswordchanged = pwdPassword.SecurePassword.Length > 0
    End Sub


    ''' <summary>
    ''' Reference to the listitem that represents all processes
    ''' </summary>
    Private allProcesses As ListViewItem

    ''' <summary>
    ''' Populates the processes listview from the database.
    ''' </summary>
    Private Sub PopulateProcesses()
        Dim groupStore = DependencyResolver.Resolve(Of IGroupStore)()
        Dim tree = groupStore.GetTree(GroupTreeType.Processes, Nothing, Nothing, False, False, False)

        lstProcesses.BeginUpdate()
        allProcesses = lstProcesses.Items.Add(My.Resources.AllProcesses)
        allProcesses.Tag = Guid.Empty
        allProcesses.ImageKey = ImageLists.Keys.Auth.Process

        For Each member As ProcessGroupMember In tree.Root.FlattenedContents(Of BluePrism.BPCoreLib.Collections.clsSortedSet(Of IGroupMember))(False)
            Dim it As ListViewItem = lstProcesses.Items.Add(member.Name)
            it.SubItems.Add(member.Description)
            it.Tag = member.IdAsGuid
            it.ImageKey = ImageLists.Keys.Auth.Process
        Next
        lstProcesses.EndUpdate()
    End Sub

    ''' <summary>
    ''' Reference to the listitem that represents all resources
    ''' </summary>
    Private allResources As ListViewItem

    ''' <summary>
    ''' Populates the resources listview from the database.
    ''' </summary>
    Private Sub PopulateResources()
        Dim groupStore = DependencyResolver.Resolve(Of IGroupStore)()
        Dim tree = groupStore.GetTree(GroupTreeType.Resources,
                        ResourceGroupMember.ViewableResource, Nothing, False, True, False)

        lstProcesses.BeginUpdate()
        allResources = lstResources.Items.Add(My.Resources.AllResources)
        allResources.Tag = Guid.Empty
        allResources.ImageKey = ImageLists.Keys.Auth.Robot

        ' include all resources and pools, including retired resources if they are explicitly assigned to credential
        ' if they were made active again, then they would be effectively assigned.
        Dim resourcePredicate = New Func(Of IGroupMember, Boolean)(Function(g)
                                                                       If g.IsPool Then Return True
                                                                       If g.IsGroup Then Return False
                                                                       Dim resource = TryCast(g, ResourceGroupMember)
                                                                       Return resource IsNot Nothing AndAlso (Not resource.HasAttribute(BluePrism.Core.Resources.ResourceAttribute.Retired) OrElse mCredential.ResourceIDs.Contains(resource.IdAsGuid))
                                                                   End Function)


        Dim resources = tree.Root.Search(resourcePredicate).
            GroupBy(Function(x) x.Id).
            Select(Function(y) y.First).
            ToList()

        For Each member As IGroupMember In resources
            Dim resourceName = member.Name
            Dim resourceId = member.IdAsGuid

            Dim item As ListViewItem = lstResources.Items.Add(resourceName)
            item.Tag = resourceId
            item.ImageKey = ImageLists.Keys.Auth.Robot
        Next
        lstProcesses.EndUpdate()
    End Sub

    ''' <summary>
    ''' Reference to the listitem that represents all roles
    ''' </summary>
    Private allRoles As ListViewItem

    ''' <summary>
    ''' Populates the roles listview from the database. 
    ''' </summary>
    Private Sub PopulateRoles()

        lstRoles.BeginUpdate()
        Try
            allRoles = lstRoles.Items.Add(My.Resources.AllRoles)
            allRoles.ImageKey = ImageLists.Keys.Auth.Users
            For Each r As Role In SystemRoleSet.Current
                With lstRoles.Items.Add(LTools.GetC(r.Name, "roleperms", "role"))
                    .Tag = r
                    .ImageKey = ImageLists.Keys.Auth.Users
                End With
            Next
        Finally
            lstRoles.EndUpdate()
        End Try

    End Sub

    ''' <summary>
    ''' Handles the process listview item checked event
    ''' </summary>
    Private Sub lstProcesses_ItemChecked(sender As System.Object,
                     e As ItemCheckedEventArgs) Handles lstProcesses.ItemChecked
        If Not mbDisallowCheck Then
            Dim gItem As Guid = CType(e.Item.Tag, Guid)
            If gItem.Equals(Guid.Empty) Then
                mbDisallowCheck = True
                SetAllChecked(lstProcesses, e.Item)
                mbDisallowCheck = False
            Else
                If allProcesses.Checked Then
                    e.Item.Checked = True
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handles the resources listview item checked event
    ''' </summary>
    Private Sub lstResources_ItemChecked(sender As Object,
             e As System.Windows.Forms.ItemCheckedEventArgs) Handles lstResources.ItemChecked

        If Not mbDisallowCheck Then
            Dim gItem As Guid = CType(e.Item.Tag, Guid)
            If gItem.Equals(Guid.Empty) Then
                mbDisallowCheck = True
                SetAllChecked(lstResources, e.Item)
                mbDisallowCheck = False
            Else
                If allResources.Checked Then
                    e.Item.Checked = True
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' Handles the roles listview item checked event
    ''' </summary>
    Private Sub lstRoles_ItemChecked(
      sender As Object, e As ItemCheckedEventArgs) Handles lstRoles.ItemChecked

        If Not mbDisallowCheck Then
            Dim r As Role = DirectCast(e.Item.Tag, Role)
            If r Is Nothing Then
                mbDisallowCheck = True
                SetAllChecked(lstRoles, e.Item)
                mbDisallowCheck = False
            Else
                If allRoles.Checked Then e.Item.Checked = True
            End If
        End If
    End Sub

    ''' <summary>
    ''' Boolean indicating whether to supress check event handling code.
    ''' </summary>
    Private mbDisallowCheck As Boolean

    ''' <summary>
    ''' Sets all the listview items checkstate to that of the item passed, and greys
    ''' out listitems as neccecery.
    ''' </summary>
    ''' <param name="lv">The listview for which to check all items</param>
    ''' <param name="it">The item for which to copy the checkstate from</param>
    Private Sub SetAllChecked(lv As ListView, it As ListViewItem)
        For Each item As ListViewItem In lv.Items
            If item Is Nothing Then Continue For
            If it Is item Then Continue For

            item.Checked = it.Checked
            If item.Checked Then
                item.ForeColor = Color.Gray
                If lv Is lstProcesses Then
                    item.ImageKey = ImageLists.Keys.Auth.Process_Disabled
                ElseIf lv Is lstResources Then
                    item.ImageKey = ImageLists.Keys.Auth.Robot_Disabled
                Else
                    item.ImageKey = ImageLists.Keys.Auth.Users_Disabled
                End If
            Else
                item.ForeColor = Color.Black
                If lv Is lstProcesses Then
                    item.ImageKey = ImageLists.Keys.Auth.Process
                ElseIf lv Is lstResources Then
                    item.ImageKey = ImageLists.Keys.Auth.Robot
                Else
                    item.ImageKey = ImageLists.Keys.Auth.Users
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' Clear the expiry date if de-selected
    ''' </summary>
    Private Sub ToggleExpiryDate()
        If dpExpiryDate.Checked Or cultureDpExpiryCheckBox.Checked Then
            dpExpiryDate.CustomFormat = My.Resources.DateTimeFormat_DddDMMMMYyyy
            cultureDpExpiryDate.Enabled = True
        Else
            dpExpiryDate.CustomFormat = " "
            cultureDpExpiryDate.Enabled = False
        End If
    End Sub

    ''' <summary>
    ''' Event handler for the txtName textbox
    ''' </summary>
    Private Sub txtName_TextChanged(sender As System.Object,
                             e As System.EventArgs) Handles txtName.TextChanged
        If txtName.Text <> String.Empty Then
            btnOK.Enabled = True
        Else
            btnOK.Enabled = False
        End If
    End Sub

    ''' <summary>
    ''' Event handler for expiry datepicker
    ''' </summary>
    Private Sub dpExpiryDate_ValueChanged(sender As System.Object,
                     e As System.EventArgs) Handles dpExpiryDate.ValueChanged, cultureDpExpiryDate.ValueChanged, cultureDpExpiryCheckBox.CheckedChanged
        ToggleExpiryDate()
    End Sub

    ''' <summary>
    ''' Gets the help file name.
    ''' </summary>
    Public Overrides Function GetHelpFile() As String
        Return "frmCredentials.htm"
    End Function

    ''' <summary>
    ''' Opens the help file whether online or offline.
    ''' </summary>
    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub

    Private Sub dgvProperties_MouseLeave(sender As Object, e As EventArgs) Handles dgvProperties.MouseLeave
        mIsDeleting = False
    End Sub

    ''' <summary>
    ''' Event fired in validating user input into the editable cells
    ''' </summary>
    Private Sub dgvProperties_CellValidating(sender As Object,
                                             e As DataGridViewCellValidatingEventArgs) _
                                             Handles dgvProperties.CellValidating

        If mIsCancelling Then Return

        Dim changingPropertyName = e.ColumnIndex = propertyName.Index AndAlso
                                   dgvProperties.IsCurrentCellDirty()

        If changingPropertyName Then

            Dim validProperty = IsValidPropertyName(Convert.ToString(e.FormattedValue), e.RowIndex)
            If Not validProperty.IsValid Then
                e.Cancel = True
                UserMessage.Err(validProperty.InvalidMessage)
                Return
            End If
        End If

        e.Cancel = False
    End Sub

    Private Function IsGridInValidState() As (IsValid As Boolean, InvalidMessage As String)
        For Each row As DataGridViewRow In dgvProperties.Rows
            If row.IsNewRow Then Continue For

            Dim nameOfProperty = CStr(row.Cells(propertyName.Index).Value)
            Dim result = IsValidPropertyName(nameOfProperty, row.Index)
            If Not result.IsValid Then
                Return result
            End If
        Next

        Return (True, Nothing)
    End Function


    Private Function IsCredentialNameValid() As (IsValid As Boolean, InvalidMessage As String)

        Dim isValid = True
        Dim message = ""

        Dim credentialType = CType(cmbType.SelectedValue, CredentialType)

        If credentialType.Name = "DataGatewayCredentials" Then
            Dim name = txtName.Text
            isValid = name.All(Function(x) Char.IsLetterOrDigit(x) OrElse Char.IsWhiteSpace(x))

            If Not isValid Then
                message = My.Resources.CredentialNameContainsInvalidChars
            End If
        End If

        Return (isValid, message)
    End Function


    ''' <summary>
    ''' Method that checks whether a property name is valid and can be used on the
    ''' grid.
    ''' </summary>
    ''' <param name="propertyNameToCheck">The property name to validate against. </param>
    ''' <returns>A value tuple indicating whether the property name is valid and
    ''' a message if it isn't. </returns>
    Private Function IsValidPropertyName(propertyNameToCheck As String, currentRowIndex As Integer) _
                                     As (IsValid As Boolean, InvalidMessage As String)

        If propertyNameToCheck = "" Then
            Return (False, frmCredential_Resources.Err_Cannot_Have_Empty_Property_Names)
        End If

        Dim propertyNameTaken = GetRowsToCheckForDuplicates(currentRowIndex).
                                 Any(Function(r) String.Equals(propertyNameToCheck,
                                                               Convert.ToString(r.Cells(
                                                               propertyName.Index).Value),
                                                               StringComparison.OrdinalIgnoreCase))

        If propertyNameTaken Then
            Return (False, frmCredential_Resources.Err_Cannot_Have_Duplicate_Property_Names)
        End If

        Return (True, Nothing)

    End Function

    ''' <summary>
    ''' Indicates whether the credential being used within the form
    ''' is new or is being updated.
    ''' </summary>
    ''' <returns>A true or false value. </returns>
    Private Function IsNewCredential() As Boolean
        Return mCredential.ID = Guid.Empty
    End Function

    Private Function GetRowsToCheckForDuplicates(currentRowIndex As Integer) As IEnumerable(Of DataGridViewRow)
        Return dgvProperties.Rows.Cast(Of DataGridViewRow).Where(Function(r) Not r.Index = currentRowIndex)
    End Function

    Private Sub dgvProperties_CellMouseUp(sender As System.Object,
                                          e As DataGridViewCellMouseEventArgs) Handles dgvProperties.CellMouseUp

        If e.Button = System.Windows.Forms.MouseButtons.Left AndAlso e.ColumnIndex = Action.Index _
         AndAlso e.RowIndex >= 0 AndAlso e.RowIndex <> dgvProperties.Rows.Count - 1 Then
            dgvProperties.Rows.RemoveAt(e.RowIndex)
            mFormDirty = True
        End If
    End Sub

    Private Sub dgvProperties_CellFormatting(sender As System.Object,
                    e As System.Windows.Forms.DataGridViewCellFormattingEventArgs) Handles dgvProperties.CellFormatting

        If e.ColumnIndex = 0 Then
            If e.RowIndex = dgvProperties.Rows.Count - 1 Then
                e.Value = ToolImages.New_16x16
                dgvProperties.Rows(e.RowIndex).Cells(e.ColumnIndex).ToolTipText =
                    frmCredential_Resources.Enter_New_Property
            Else
                e.Value = ToolImages.Delete_Red_16x16
                dgvProperties.Rows(e.RowIndex).Cells(e.ColumnIndex).ToolTipText =
                    frmCredential_Resources.Remove_Property
            End If
        End If
    End Sub

    Private Sub dgvProperties_CellMouseEnter(sender As System.Object,
                            e As System.Windows.Forms.DataGridViewCellEventArgs) Handles dgvProperties.CellMouseEnter

        If e.ColumnIndex = 0 AndAlso e.RowIndex >= 0 _
         AndAlso e.RowIndex <> dgvProperties.Rows.Count - 1 Then
            dgvProperties.Cursor = Cursors.Hand
            mIsDeleting = True
        Else
            dgvProperties.Cursor = Cursors.Arrow
            mIsDeleting = False
        End If
    End Sub

    'Mark the form as Not Dirty
    Private mFormDirty As Boolean = False
    Private Sub AddDirtyFormHandler(control As Control)
        For Each c As Control In control.Controls
            Select Case c.GetType
                Case GetType(TextBox), GetType(StyledTextBox)
                    AddHandler c.TextChanged, AddressOf SetFormDirty
                Case GetType(CheckBox)
                    AddHandler TryCast(c, CheckBox).CheckedChanged, AddressOf SetFormDirty
                Case GetType(DatePicker)
                    AddHandler TryCast(c, DatePicker).Enter, AddressOf SetFormDirty
                Case GetType(DateTimePicker)
                    AddHandler TryCast(c, DateTimePicker).ValueChanged, AddressOf SetFormDirty
                Case GetType(DataGridView)
                    AddHandler TryCast(c, DataGridView).CellEndEdit, AddressOf SetFormDirtyForDataGridView
                Case GetType(ListView)
                    AddHandler TryCast(c, ListView).ItemChecked, AddressOf SetFormDirty
                    AddHandler TryCast(c, ListView).AfterLabelEdit, AddressOf SetFormDirty
                Case GetType(ComboBox)
                    AddHandler TryCast(c, ComboBox).SelectedValueChanged, AddressOf SetFormDirty
                Case GetType(AutomateUI.ctlAutomateSecurePassword)
                    AddHandler TryCast(c, ctlAutomateSecurePassword).TextChanged, AddressOf SetFormDirty
                Case GetType(TabPage), GetType(TabControl), GetType(TableLayoutPanel)
                    AddDirtyFormHandler(c)
            End Select
        Next

    End Sub

    Private Sub SetFormDirty(sender As Object, e As EventArgs)

        If DirectCast(sender, Control).Focused Then
            mFormDirty = True
        End If

    End Sub

    Private Sub SetFormDirtyForDataGridView(sender As Object, e As EventArgs)
        If DirectCast(sender, DataGridView).CurrentCell.GetType = GetType(PasswordCell) Then
            If DirectCast(DirectCast(sender, DataGridView).CurrentCell, PasswordCell).HasPasswordChanged Then
                mFormDirty = True
            End If
        End If

        If DirectCast(sender, DataGridView).IsCurrentRowDirty Or DirectCast(sender, DataGridView).IsCurrentCellDirty Then
            mFormDirty = True
        End If

    End Sub


#Region "IEnvironmentColourManager implementation"

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return objTitleBar.BackColor
        End Get
        Set(value As Color)
            objTitleBar.BackColor = value
            lblType.BackColor = value
        End Set
    End Property

    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return objTitleBar.TitleColor
        End Get
        Set(value As Color)
            objTitleBar.TitleColor = value
            objTitleBar.SubtitleColor = value
            lblType.ForeColor = value
        End Set
    End Property



    Private Sub btnCancel_MouseEnter(sender As Object, e As EventArgs) Handles btnCancel.MouseEnter
        mIsCancelling = True
    End Sub

    Private Sub btnCancel_MouseLeave(sender As Object, e As EventArgs) Handles btnCancel.MouseLeave
        mIsCancelling = False
    End Sub

    Private Sub btnCancel_Enter(sender As Object, e As EventArgs) Handles btnCancel.Enter
        mIsCancelling = True
    End Sub

    Private Sub btnCancel_Leave(sender As Object, e As EventArgs) Handles btnCancel.Leave
        mIsCancelling = False
    End Sub

    Private Sub cmbType_SelectedValueChanged(sender As Object, e As EventArgs) Handles cmbType.SelectedValueChanged
        If Not mIsLoaded Then Return

        Dim selectedType = If(CType(cmbType.SelectedValue, CredentialType), CredentialType.General)

        mIsPasswordCredentialType = HasCredentialTypeGotAPassword(selectedType)

        If mIsPasswordCredentialType Then
            ShowConfirmPassword(True)
        Else
            ShowConfirmPassword(False)
        End If

        Dim showUsername = selectedType.IsUsernameVisible
        txtUserName.Visible = showUsername
        lblUsername.Visible = showUsername

        If Not showUsername Then
            lblPassword.Location = New Point(lblPassword.Location.X, mUserNameLabelOriginalY)
            pwdPassword.Location = New Point(pwdPassword.Location.X, mUserNameControlOriginalY)

            lblPasswordConfirm.Location = New Point(lblPasswordConfirm.Location.X, mPasswordLabelOriginalY)
            pwdPasswordConfirm.Location = New Point(pwdPasswordConfirm.Location.X, mPasswordControlOriginalY)
        Else
            lblPassword.Location = New Point(lblPassword.Location.X, mPasswordLabelOriginalY)
            pwdPassword.Location = New Point(pwdPassword.Location.X, mPasswordControlOriginalY)

            lblPasswordConfirm.Location = New Point(lblPasswordConfirm.Location.X, mPasswordConfirmLabelOriginalY)
            pwdPasswordConfirm.Location = New Point(pwdPasswordConfirm.Location.X, mPasswordConfirmControlOriginalY)
        End If

        Dim showAccessRightsTab = selectedType.IsAccessRightsVisible
        If Not showAccessRightsTab Then
            tcMain.SelectedIndex = tcMain.TabPages.IndexOfKey("tpCredentials")
        End If

        If showAccessRightsTab AndAlso Not tcMain.TabPages.Contains(mAccessRightsTab) Then
            tcMain.TabPages.Add(mAccessRightsTab)
        End If

        If Not showAccessRightsTab AndAlso tcMain.TabPages.Contains(mAccessRightsTab) Then
            tcMain.TabPages.Remove(mAccessRightsTab)
        End If

        lblUsername.Text = selectedType.LocalisedUsernamePropertyTitle
        lblPassword.Text = selectedType.LocalisedPasswordPropertyTitle
        lblCredentialTypeDescription.Text = selectedType.LocalisedDescription
    End Sub

    Private Sub ShowConfirmPassword(isVisible As Boolean)
        pwdPasswordConfirm.Visible = isVisible
        lblPasswordConfirm.Visible = isVisible
    End Sub

    Private Function HasCredentialTypeGotAPassword(credentialType As CredentialType) As Boolean
        If credentialType Is CredentialType.General OrElse credentialType Is CredentialType.BasicAuthentication OrElse credentialType Is CredentialType.DataGatewayCredentials Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub lbLinkHelp_LinkClicked(sender As Object, e As LinkLabelLinkClickedEventArgs) Handles lbLinkHelp.LinkClicked
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub
#End Region

End Class
