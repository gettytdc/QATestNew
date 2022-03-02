Imports AutomateControls
Imports BluePrism.AMI
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.CharMatching.UI
Imports clsElement = BluePrism.AMI.clsAMI.clsElement

Friend Class frmIntegrationAssistant : Implements IEnvironmentColourManager

#Region " Class Scope Definitions "

    ''' <summary>
    ''' Utility class to wrap the application name in an application parameter
    ''' </summary>
    Private Class AppNameParameter : Inherits clsApplicationParameter
        Public Sub New(ByVal name As String)
            Me.Name = "Application Name"
            Me.ParameterType = ParameterTypes.String
            Me.HelpText = My.Resources.ApplicationName
            Me.AcceptNullValue = False
            Me.Value = name
        End Sub
    End Class

    Private SpyButtonText As String = My.Resources.Identify

#End Region

#Region " Published Events "

    ''' <summary>
    ''' Event raised when the user commits the changes they have
    ''' made, by clicking the OK or Apply button.
    ''' </summary>
    ''' <param name="NewAppDef">The latest version of the application
    ''' definition.</param>
    ''' <remarks></remarks>
    Public Event ApplicationDefinitonUpdated(ByVal NewAppDef As clsApplicationDefinition, ByVal Parent As KeyValuePair(Of Guid, String))

    ''' <summary>
    ''' Event raised when spying begins.
    ''' </summary>
    Public Event BeginSpy()

    ''' <summary>
    ''' Event raised when spying ends.
    ''' </summary>
    Public Event EndSpy()

    Public Event FindReferences(dep As clsProcessDependency)

#End Region

#Region " Constructors / Destructors "

    ''' <summary>
    ''' Creates an empty integration assistant form, not connected to AMI
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, Nothing, New KeyValuePair(Of Guid, String)(Guid.Empty, Nothing))
    End Sub

    Public Sub New(ami As clsAMI, businessObject As clsProcess, parent As KeyValuePair(Of Guid, String))
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Check if user can edit this application model.
        ' If this is a shared model, we shouldn't be able to edit this if we don't have
        ' edit permission on the parent object.

        Dim id = If(parent.Value IsNot Nothing, parent.Key, businessObject.Id)
        Dim perms = gSv.GetEffectiveMemberPermissionsForProcess(id)
        mHasPermissionToEdit =
            perms.HasPermission(User.Current, Permission.ObjectStudio.ImpliedEditBusinessObject)

        mBoldMenuFont = mnuOpenScreenshot.Font
        mRegularMenuFont = mnuNewScreenshot.Font

        ' Add any initialization after the InitializeComponent() call.
        btnApply.Text = My.Resources.Apply
        btnApply.Enabled = False
        btnOK.Text = My.Resources.OK
        btnOK.Enabled = mHasPermissionToEdit
        btnCancel.Text = My.Resources.frmIntegrationAssistant_Cancel
        mTitleBar.Title = My.Resources.DefineTheElementsOfAnApplicationThatWillBeUsedByObjectStudio


        mObjectName = businessObject.Name
        mParent = parent
        If mParent.Value IsNot Nothing Then
            LockParent(mParent.Key, mParent.Value)
        End If

        'Update spy button text
        mAMI = ami
        PopulateDataTypesComboBox()
        Dim applicationLaunched = mAMI.ApplicationIsLaunched
        UpdateLaunchState(applicationLaunched)
        UpdateUserInterface()

        'Stops jiggery pokery with labels having their height set to zero
        'following the automatic call to frmForm.SetFont.
        'See bug 3139
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.None

        SetTitle()
    End Sub


#End Region

#Region " Member Variables "

    ''' Member reference to the AMI. Used for calling spy methods, etc.
    Private WithEvents mAMI As clsAMI

    ' The name of the object being edited
    Private mObjectName As String

    ' The parent object (ID and name) hostin the shared model
    Private mParent As New KeyValuePair(Of Guid, String)(Guid.Empty, Nothing)

    ' The object ID locked by this modeller form
    Private mLockedObject As Guid = Guid.Empty

    ' The bold font to use for this form
    Private mBoldMenuFont As Font

    ' The regular font to use for this form
    Private mRegularMenuFont As Font

    ''' The application member currently being edited, if any.
    Private mCurrentApplicationMember As clsApplicationMember

    ''' <summary>
    ''' The application definition being edited in this form.
    ''' Beware that this definition is not cloned when this form loads its data from
    ''' it, whereas the root element is - thus, elements found by the app definition
    ''' will <em>not</em> match those found in the root element.
    ''' I currently don't know why this is, but it's the way it's always been so I
    ''' left it alone after getting bitten by it myself.
    ''' </summary>
    Private mApplicationDefinition As clsApplicationDefinition

    ' The clone of the root element of the current app defn, modelled in the
    ' application explorer control
    Private mRootElement As clsApplicationElement

    ' State flag indicating if the GUI is currently being updated from the model
    Private mUpdatingUI As Boolean

    ' A cached version of the application model last built for the navigator
    Private mLastModelTree As ICollection(Of clsElement)

    ' Flag indicating that the label position layout needs performing, a layout
    ' is triggered by changing to the app info panel
    Private mLabelPosnLayoutScheduled As Boolean

    ' The window state when this form was hidden for a spy operation
    Private mSavedWindowState As FormWindowState

    'Flag indicating that the user has permission to edit this application model.
    Private mHasPermissionToEdit As Boolean = False

    Private const BrowserTimeout As String = "30"
#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the environment-specific back colour in use in this environment.
    ''' Only set to the database-held values after login.
    ''' </summary>
    ''' <remarks>Note that this only affects the UI owned directly by this form - ie.
    ''' setting the colour here will not update the database</remarks>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentBackColor As Color _
     Implements IEnvironmentColourManager.EnvironmentBackColor
        Get
            Return mTitleBar.BackColor
        End Get
        Set(value As Color)
            mTitleBar.BackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the environment-specific back colour in use in this environment.
    ''' Only set to the database-held values after login.
    ''' </summary>
    ''' <remarks>Note that this only affects the UI owned directly by this form - ie.
    ''' setting the colour here will not update the database</remarks>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property EnvironmentForeColor As Color _
     Implements IEnvironmentColourManager.EnvironmentForeColor
        Get
            Return mTitleBar.ForeColor
        End Get
        Set(value As Color)
            mTitleBar.ForeColor = value
        End Set
    End Property

    ''' <summary>
    ''' The application member currently set in this integration assistant form. This
    ''' is the one which is currently being displayed in the detail pane. Null if no
    ''' member is currently being displayed.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property CurrentMember() As clsApplicationMember
        Get
            Return mCurrentApplicationMember
        End Get
    End Property

    ''' <summary>
    ''' The application element currently set in this integration assistant form.
    ''' This is the one which is currently being displayed in the detail pane. Null
    ''' if no member is currently being displayed, or if the member currently being
    ''' displayed is not an application element.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property CurrentElement() As clsApplicationElement
        Get
            Return TryCast(mCurrentApplicationMember, clsApplicationElement)
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether the ApplicationInfo panel is currently visible or not
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property ApplicationInfoVisible() As Boolean
        Get
            Return (panSwitch.SelectedTab Is tabAppInfo)
        End Get
    End Property

    ''' <summary>
    ''' Gets the application name currently set in the model held in this form
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Private ReadOnly Property AppDefnName() As String
        Get
            If mApplicationDefinition Is Nothing Then Return Nothing
            If mApplicationDefinition.RootApplicationElement Is Nothing Then _
             Return Nothing
            Return mApplicationDefinition.RootApplicationElement.Name
        End Get
    End Property

    ''' <summary>
    ''' Determines whether the application has been launched.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property ApplicationLaunched As Boolean
        Get
            Return mAMI.ApplicationIsLaunched
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the application definition to use in this form
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property ApplicationDefinition() As clsApplicationDefinition
        Get
            Return mApplicationDefinition
        End Get
        Set(ByVal defn As clsApplicationDefinition)
            mApplicationDefinition = defn
            mRootElement =
             DirectCast(defn.RootApplicationElement.Clone(), clsApplicationElement)
            UpdateLaunchState(mAMI.ApplicationIsLaunched)
            UpdateApplicationInfo()

            'Add a default node to the application definiton, if there are no children
            Dim initMember As clsApplicationMember = Nothing
            If Not mRootElement.HasChildren Then
                initMember = New clsApplicationElementGroup(My.Resources.Element & "1")
                mRootElement.AddMember(initMember)
            End If

            appExplorer.LoadApplicationDefinition(defn, mRootElement)
            ' The app explorer automatically selects a node - usually the last selected
            ' app member, or the root node if there is no last selected member.
            ' We only want to override that choice if we just added an initial app member.
            If initMember IsNot Nothing Then appExplorer.SelectedMember = initMember

        End Set
    End Property

    ''' <summary>
    ''' Returns True if the application model cannot be edited. This is only where a
    ''' shared model is in use and we were unable to get a lock on the parent object.
    ''' </summary>
    Public ReadOnly Property [ReadOnly]() As Boolean
        Get
            Return mParent.Value IsNot Nothing _
             AndAlso mLockedObject = Guid.Empty _
            OrElse Not mHasPermissionToEdit
        End Get
    End Property

    ''' <summary>
    ''' Gets the currently selected element type in this form
    ''' </summary>
    ''' <value></value>
    Private ReadOnly Property SelectedElementType As clsElementTypeInfo
        Get
            Return DirectCast(cmbElemType.SelectedItem, clsElementTypeInfo)
        End Get
    End Property

    ''' <summary>
    ''' Gets the currently selected data type in this form
    ''' </summary>
    ''' <value></value>
    Private ReadOnly Property SelectedDataType As DataType
        Get
            Return DirectCast(cmbDataType.SelectedValue, DataType)
        End Get
    End Property

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' Removes the event handler from events raised by the internal
    ''' AMI reference.
    ''' </summary>
    Public Sub HaltApplicationMonitoring()
        If mAMI IsNot Nothing Then _
         RemoveHandler mAMI.ApplicationStatusChanged, AddressOf HandleAppClosed

    End Sub

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Updates the UI to match the current launch state
    ''' </summary>
    Private Sub UpdateLaunchState(applicationLaunched As Boolean)
        If applicationLaunched Then
            btnIdentify.Text = SpyButtonText
            btnIdentify.ContextMenuStrip = ctxMenuIdentify
        Else
            btnIdentify.Text = mAMI.GetLaunchCommandUIString(Me.mApplicationDefinition?.ApplicationInfo)
            btnIdentify.ContextMenuStrip = Nothing
        End If
        UpdateButtons(applicationLaunched)
    End Sub


    ''' <summary>
    ''' Populates the data type combobox with available automate data types.
    ''' </summary>
    Private Sub PopulateDataTypesComboBox()
        cmbDataType.DataSource = New List(Of clsDataTypeInfo)(
         clsProcessDataTypes.GetPublicScalars())
    End Sub

    ''' <summary>
    ''' Populates the combobox of possible element types.
    ''' </summary>
    Private Sub PopulateElementTypesComboBox(ByVal el As clsApplicationElement)
        cmbElemType.DataSource = CollectionUtil.MergeList(
         GetSingleton.ICollection(el.BaseType), el.BaseType.AlternateTypes)
    End Sub

    ''' <summary>
    ''' Updates the Enabled property on the text of the buttons, according
    ''' to which application member is selected in the tree, etc.
    ''' </summary>
    Private Sub UpdateButtons(applicationLaunched As Boolean)
        Dim memberSelected As Boolean = (CurrentMember IsNot Nothing)
        Dim elemSelected As Boolean = (CurrentElement IsNot Nothing)

        If applicationLaunched Then
            btnIdentify.Enabled = mHasPermissionToEdit
        Else
            btnIdentify.Enabled = memberSelected
        End If
        panAppInfo.LaunchEnabled = Not applicationLaunched

        miAppNavigator.Enabled = applicationLaunched AndAlso memberSelected
        miUIAutomationNavigator.Enabled = applicationLaunched AndAlso memberSelected
        btnShowElement.Enabled = applicationLaunched AndAlso elemSelected
        btnClearAttributes.Enabled = elemSelected
        UpdateRegionsButton()

    End Sub


    ''' <summary>
    ''' Updates the regions button with the current state in this form.
    ''' </summary>
    Private Sub UpdateRegionsButton()
        ' If the current element is a region / hosting element,
        ' enable the button and its menu items
        Dim cont As clsRegionContainer = Nothing
        Dim reg As clsApplicationRegion = Nothing
        ExtractContainerAndRegion(CurrentElement, cont, reg)
        If cont Is Nothing Then
            btnRegions.Visible = False

        Else
            ' Enable opening existing screenshot if there is something there to open
            Dim ss As clsProcessValue = cont.GetValue(clsAMI.ScreenshotIdentifierId)
            mnuOpenScreenshot.Enabled =
             (ss IsNot Nothing AndAlso Not ss.IsNull)

            ' You can always spy a new screenshot on a region container if
            ' there's an application to spy it from
            mnuNewScreenshot.Enabled = mAMI.ApplicationIsLaunched()

            ' Highlight the default menu item with bold font. Ensure other menu
            ' items are not bolded, so we can see the difference.
            If mnuOpenScreenshot.Enabled Then
                mnuOpenScreenshot.Font = mBoldMenuFont
                mnuNewScreenshot.Font = mRegularMenuFont
            ElseIf mnuNewScreenshot.Enabled Then
                mnuOpenScreenshot.Font = mRegularMenuFont
                mnuNewScreenshot.Font = mBoldMenuFont
            Else ' ie. both disabled
                mnuOpenScreenshot.Enabled = False
                mnuNewScreenshot.Enabled = False
            End If

            ' If any of the menu items are enabled 
            btnRegions.Visible =
             (mnuNewScreenshot.Enabled OrElse mnuOpenScreenshot.Enabled)
        End If
    End Sub

    ''' <summary>
    ''' Determines if the application explorer's selected element is the root element
    ''' of the embedded app model.
    ''' </summary>
    ''' <returns>Returns true if the app explorer has a node selected which
    ''' represents the root element of the current application model; false if a
    ''' different (or no) node is selected</returns>
    Private Function IsRootElementSelected() As Boolean
        Return (mRootElement IsNot Nothing _
         AndAlso appExplorer.SelectedElement Is mRootElement)
    End Function

    ''' <summary>
    ''' Populates application element editor with current application member, updates
    ''' enabled property on buttons, populates comboboxes, edit fields etc with
    ''' current application member.
    ''' </summary>
    Private Sub UpdateUserInterface()
        UpdateUserInterface(True)
    End Sub

    ''' <summary>
    ''' Populates application element editor with current application member, updates
    ''' enabled property on buttons, populates comboboxes, edit fields etc with
    ''' current application member.
    ''' </summary>
    ''' <param name="moveFocus">True to transfer the focus to the name textbox of the
    ''' detail panel; False to leave focus as it is.</param>
    Private Sub UpdateUserInterface(ByVal moveFocus As Boolean)
        mUpdatingUI = True
        'If root node of treeview is selected then disable everything - not editable
        If IsRootElementSelected() Then
            btnIdentify.Enabled = False
            elemEditor.Enabled = False
            cmbElemType.Enabled = False
            cmbDataType.Enabled = False
            txtElementName.Enabled = False
            Exit Sub
        Else
            'Enable everything by default and then selectively disable
            btnIdentify.Enabled = True
            elemEditor.Enabled = True
            cmbElemType.Enabled = True
            cmbDataType.Enabled = True
            txtElementName.Enabled = True
        End If


        If Not mCurrentApplicationMember Is Nothing Then
            elemEditor.Populate(Me.mCurrentApplicationMember)

            'Populate details outside of element editor
            txtElementName.Text = mCurrentApplicationMember.Name
            If TypeOf mCurrentApplicationMember Is clsApplicationElement Then
                Dim elem As clsApplicationElement = CurrentElement
                Dim hasAttrs As Boolean =
                 (elem.Attributes IsNot Nothing AndAlso elem.Attributes.Count > 0)

                cmbElemType.Enabled = hasAttrs
                PopulateElementTypesComboBox(elem)
                cmbElemType.SelectedItem = elem.Type

                cmbDataType.SelectedValue = elem.DataType

                elemEditor.Enabled = hasAttrs
                txtDescription.Enabled = True
                txtNotes.Enabled = True
                txtDescription.Text = elem.Description
                txtNotes.Text = elem.Narrative
            Else
                cmbDataType.SelectedItem = Nothing
                cmbDataType.Enabled = False

                cmbElemType.DataSource = New clsElementTypeInfo() {}
                cmbElemType.Enabled = False

                elemEditor.Enabled = False
                txtNotes.Text = ""
                txtDescription.Text = ""
                txtNotes.Enabled = False
                txtDescription.Enabled = False
            End If

        End If

        'disable/enable UI features
        UpdateButtons(mAMI.ApplicationIsLaunched)
        If moveFocus Then
            txtElementName.Select()
            clsUserInterfaceUtils.SetFocusDelayed(txtElementName)
        End If

        'everything is read-only if unable to lock shared model
        If Me.ReadOnly Then
            Me.txtElementName.Enabled = False
            Me.txtDescription.Enabled = False
            Me.cmbElemType.Enabled = False
            Me.cmbDataType.Enabled = False
            Me.txtNotes.Enabled = False
            Me.elemEditor.Readonly = True
            Me.btnClearAttributes.Enabled = False
            Me.btnRegions.Enabled = False
        Else
            Me.elemEditor.Readonly = False
            Me.btnClearAttributes.Enabled = True
            Me.btnRegions.Enabled = True
        End If
        appExplorer.ReadOnly = Me.ReadOnly

        mUpdatingUI = False
    End Sub

#End Region

#Region " Button Handlers "

    ''' <summary>
    ''' Handles Apply being pressed, applying the changes made to the model
    ''' </summary>
    Private Sub HandleApply(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnApply.Click
        ApplyChanges()
    End Sub

    ''' <summary>
    ''' Handles OK being pressed; it applies the changes to the model and closes
    ''' the form with a dialog result of <see cref="DialogResult.OK"/>
    ''' </summary>
    Private Sub HandleOK(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnOK.Click
        ' OK button clicked. Apply changes and close form
        If ApplyChanges() Then
            DialogResult = DialogResult.OK
            Close()
        End If
    End Sub

    ''' <summary>
    ''' Cancels any changes made to the app model and closes the form
    ''' </summary>
    Private Sub HandleCancel(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnCancel.Click
        DialogResult = DialogResult.Cancel
        Close()
    End Sub

    ''' <summary>
    ''' Applies the changes in the current UI state to the underlying application
    ''' definition.
    ''' </summary>
    Private Function ApplyChanges() As Boolean
        SaveApplicationInfo()
        mApplicationDefinition.RootApplicationElement = mRootElement
        If mParent.Value IsNot Nothing AndAlso btnApply.Enabled Then
            If Not UpdateParent() Then Return False
        End If
        RaiseEvent ApplicationDefinitonUpdated(mApplicationDefinition.Clone(), mParent)

        btnApply.Enabled = False
        Return True
    End Function

#End Region

#Region " Event Handlers "

    Private Sub HandleMemberChanged(ByVal member As clsApplicationMember) _
     Handles appExplorer.MemberAdded, appExplorer.MemberDeleted
        btnApply.Enabled = mHasPermissionToEdit
    End Sub

    Private Sub HandleTabChanged(ByVal sender As Object, ByVal e As TabControlEventArgs) _
     Handles panSwitch.Deselected
        mLabelPosnLayoutScheduled = (e.TabPage Is tabAppInfo)
    End Sub

    ''' <summary>
    ''' Handles an application member being selected in the application explorer.
    ''' </summary>
    ''' <param name="e">The args detailing the event.</param>
    Private Sub HandleMemberSelected(
     ByVal sender As Object, ByVal e As ApplicationMemberEventArgs) _
     Handles appExplorer.MemberSelected
        mCurrentApplicationMember = e.Member

        'Show application info panel if root node is selected
        If IsRootElementSelected() Then
            If Not panAppInfo.HasParams Then UpdateApplicationInfo()
            panSwitch.SelectedTab = tabAppInfo
            panAppInfo.ReadOnly = Me.ReadOnly

        Else
            If ApplicationInfoVisible Then
                SaveApplicationInfo()
                panSwitch.SelectedTab = tabElemInfo

            End If
            ' Update the user interface - don't transfer the focus if the member
            ' selection is as a result of a filter being applied
            UpdateUserInterface(Not e.IsResultOfFilter)

        End If

        mAMI.CancelSpy(Nothing)
    End Sub

    Private Sub HandleFindReferences(el As clsApplicationElement) _
        Handles appExplorer.FindReferences
        RaiseEvent FindReferences(New clsProcessElementDependency(CStr(IIf(mParent.Value IsNot Nothing, mParent.Value, mObjectName)), el.ID))
    End Sub

    ''' <summary>
    ''' Saves the info on the application info page.
    ''' </summary>
    Private Sub SaveApplicationInfo()
        If Not ApplicationInfoVisible Then Return

        Dim oldAppType As clsApplicationTypeInfo =
         mApplicationDefinition.ApplicationInfo.Clone()

        panAppInfo.SaveToModel()

        ' Apply the name
        If mRootElement.Name <> panAppInfo.AppName Then
            ' Set in the original app defn
            mApplicationDefinition.RootApplicationElement.Name =
             panAppInfo.AppName

            ' And in the clone in use in the app explorer
            mRootElement.Name = panAppInfo.AppName

            ' Finally in the app explorer itself.
            appExplorer.UpdateName(mRootElement)

        End If

        'Check that since the update, the application
        'is still fundamentally the same. If not then we must
        'relaunch it . . .
        If Not mAMI.ApplicationsAreEqual(
         oldAppType, mApplicationDefinition.ApplicationInfo) Then
            UpdateLaunchState(False)
        End If
    End Sub

    ''' <summary>
    ''' Refreshes the application info page to match the new app model
    ''' </summary>
    Private Sub UpdateApplicationInfo()
        panAppInfo.Parameters =
         CollectionUtil.MergeList(
          New clsApplicationParameter() {New AppNameParameter(AppDefnName)},
          mApplicationDefinition.ApplicationInfo.Parameters
         )
        panAppInfo.LaunchButtonText =
         mAMI.GetLaunchCommandUIString(Me.mApplicationDefinition?.ApplicationInfo)
    End Sub

    ''' <summary>
    ''' Handler for changes raised by controls in the Application info page.
    ''' </summary>
    Private Sub ApplicationInfoControlChanged(
     ByVal sender As Object, ByVal e As EventArgs) _
     Handles panAppInfo.InfoChanged
        If Not mUpdatingUI Then btnApply.Enabled = mHasPermissionToEdit
    End Sub

#Region "Button Click Events"

    ''' <summary>
    ''' Handler for the click event of the button to re-run
    ''' the application info wizard.
    ''' </summary>
    Private Sub HandleReRunAppInfoWizardButtonPress(
     ByVal sender As Object, ByVal e As EventArgs) Handles panAppInfo.WizardClick
        Using f As New frmApplicationDefinitionCreate(mObjectName, mParent)

            ' Set the app defn to prepopulate the wizard
            f.PrototypeApplicationDefinition = mApplicationDefinition
            f.PrototypeName = mRootElement.Name
            f.SetEnvironmentColours(Me)
            f.ShowInTaskbar = False

            ' Show the wizard - if the user cancelled out, return without changes
            If f.ShowDialog() <> DialogResult.OK Then Return

            ' Exit if there is now no model
            If f.ParentObject.Value Is Nothing AndAlso f.ApplicationDefinition.ApplicationInfo Is Nothing Then
                RaiseEvent ApplicationDefinitonUpdated(New clsApplicationDefinition(), Nothing)
                DialogResult = DialogResult.OK
                Close()
                Return
            End If

            ' Otherwise, update our app defn memvar
            If f.ParentObject.Value Is Nothing Then
                ' We don't have a shared model
                mApplicationDefinition = f.ApplicationDefinition
            Else
                'Load XML and extract shared model
                Dim sXML As String = gSv.GetProcessXML(f.ParentObject.Key)
                ApplicationDefinition = clsProcess.FromXml(Options.Instance.GetExternalObjectsInfo(), sXML, False).ApplicationDefinition
            End If
            'Handle changes of parent
            If Not f.ParentObject.Equals(mParent) Then
                'Unlock former parent (if required)
                UnlockParent()
                If f.ParentObject.Value IsNot Nothing Then
                    'Lock new parent
                    LockParent(f.ParentObject.Key, f.ParentObject.Value)
                    btnApply.Enabled = False
                End If
            End If
            mParent = f.ParentObject

            ' Update the root element with the new name
            mRootElement.Name = mApplicationDefinition.RootApplicationElement.Name
            appExplorer.UpdateName(mRootElement)

            ' update the launch button
            UpdateLaunchState(mAMI.ApplicationIsLaunched)
            ' and re-populate the app info panel
            UpdateApplicationInfo()
            panAppInfo.ReadOnly = Me.ReadOnly
            appExplorer.ReadOnly = Me.ReadOnly
            SetTitle()

        End Using
    End Sub

    ''' <summary>
    ''' Performs preparatory operations ahead of a spy
    ''' </summary>
    Private Sub OnBeginSpy()
        HideIntegrationAssistant()
        btnClearAttributes.Enabled = False
        btnShowElement.Enabled = False
        btnIdentify.Enabled = False

        RaiseEvent BeginSpy()
    End Sub

    Private Sub OnEndSpy()
        RaiseEvent EndSpy()
        UpdateButtons(mAMI.ApplicationIsLaunched)
        tcElementDetails.SelectTab(Me.tpAttributes)
        RestoreIntegrationAssistant()

    End Sub

    Private Sub btnDiagnostics_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles panAppInfo.DiagnosticsClick
        Try
            Using f As New frmDiagnostics(mAMI, mApplicationDefinition)
                f.ShowInTaskbar = False
                f.ShowDialog(Me)
            End Using

        Catch ex As Exception
            UserMessage.Show(My.Resources.AnErrorOccurredWithinTheDiagnosticsForm, ex)

        End Try
    End Sub

    ''' <summary>
    ''' Checks with the user to ensure that they are aware that they will be
    ''' overwriting existing information in the operation they are requesting.
    ''' </summary>
    ''' <returns>True if the user has OK'ed the warning; False if they closed the
    ''' prompt via any other method.</returns>
    Private Function PassesOverwriteElementWarning() As Boolean

        Dim currEl As clsApplicationElement = CurrentElement
        ' If there is no element selected, or it's currently empty, that effectively
        ' passes the warning.
        If currEl Is Nothing OrElse currEl.Attributes.Count = 0 Then Return True

        Dim res As MsgBoxResult = UserMessage.YesNo(
         My.Resources.TheCurrentlySelectedElementInTheTreeAlreadyContainsInformationFromAPreviousSpyO)

        Return (res = MsgBoxResult.Yes)

    End Function

    Private Sub HandleSpyOrLaunchClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles panAppInfo.LaunchClick, btnIdentify.Click, miSpyElement.Click
        Dim regContainer As clsRegionContainer = Nothing
        Try
            btnIdentify.Enabled = False

            Dim applicationLaunched = mAMI.ApplicationIsLaunched
            'If application is not yet launched then launch it
            If Not applicationLaunched Then
                SaveApplicationInfo()
                Dim Err As clsAMIMessage = Nothing
                If Not mAMI.SetTargetApplication(Me.mApplicationDefinition.ApplicationInfo, Err) Then
                    UserMessage.Show(String.Format(My.Resources.CommunicationWithApplicationManagerFailed0, Err.Message), Err.HelpTopic)
                    Exit Sub
                End If
                Err = Nothing
                Dim myArgs As Dictionary(Of String,String) = Nothing
                If mAMI.GetTargetAppInfo().ID = clsApplicationTypeInfo.BrowserLaunchId OrElse mAMI.GetTargetAppInfo().ID = clsApplicationTypeInfo.BrowserAttachId Then
                    myArgs = New Dictionary(Of String, String) From {
                        {"BrowserLaunchTimeout", BrowserTimeout}
                    }
                End If
                applicationLaunched = mAMI.LaunchApplication(Err, myArgs)
                
                If Not applicationLaunched Then
                    ShowMessage(Err.HelpTopic,
                        My.Resources.ErrorFailedToLaunchApplication0, Err.Message)
                End If
                UpdateLaunchState(applicationLaunched)
                Exit Sub
            End If

            ' Ensure that the user is happy with the current element being
            ' overwritten by the spy operation
            If Not PassesOverwriteElementWarning() Then Exit Sub

            'We spy to collect details for the currently selected element.
            Dim typeInfo As clsElementTypeInfo = Nothing
            Dim ids As List(Of clsIdentifierInfo) = Nothing

            'Hide Automate windows before spying - makes
            'easier for user to access target app.
            OnBeginSpy()

            ' If the spy didn't work (or user cancelled it), there's nothing for
            ' us to do, so exit now.
            If Not mAMI.Spy(typeInfo, ids) Then Return

            ' Create a new element and replace the current member with it.
            Dim elem As clsApplicationElement = UpdateCurrentMember(typeInfo, ids)

            ' Ensure we have the region container if a) an element was updated
            ' and b) it was a region container element
            regContainer = TryCast(elem, clsRegionContainer)

        Catch ex As Exception
            UserMessage.Show(My.Resources.ThereWasAnErrorDuringTheSpyingOperation, ex)

        Finally
            btnIdentify.Enabled = True
            OnEndSpy()

        End Try

        ' If we're dealing with a Win32Region, open the region editor with which
        ' the hosted regions can be specified
        If regContainer IsNot Nothing Then CaptureRegions(regContainer)

    End Sub

    ''' <summary>
    ''' Updates the current member selected in this form with the element data given.
    ''' </summary>
    ''' <param name="typeInfo">The type information of the new element</param>
    ''' <param name="idInfos">The collection of identifiers which identify the
    ''' element to replace the current member with.</param>
    Private Function UpdateCurrentMember(ByVal typeInfo As clsElementTypeInfo,
     ByVal idInfos As ICollection(Of clsIdentifierInfo)) As clsApplicationElement

        ' Not sure if this can ever happen, but it was tested for in
        ' HandleSpyOrLaunchClick, so we should check for it here
        If idInfos Is Nothing OrElse typeInfo Is Nothing Then Return Nothing

        Dim regionEditorImage As Bitmap = Nothing

        Dim currMember As clsApplicationMember = appExplorer.SelectedMember
        If currMember Is Nothing Then
            UserMessage.Show(
             My.Resources.NoTreenodeIsSelectedCannotApplySpyInformationToAnElement)
            Return Nothing
        End If

        ' Gets the container and region associated with the current element
        Dim cont As clsRegionContainer = Nothing
        Dim reg As clsApplicationRegion = Nothing
        ExtractContainerAndRegion(currMember, cont, reg)

        ' If the current element is a region within a region container then
        ' we want to remove the region from its region container, as we are
        ' now respying the element.
        If cont IsNot Nothing AndAlso reg IsNot Nothing Then
            cont.Regions.Remove(reg)
            ' If the current element was the relative parent of another region
            ' within that region container then we also need to clear the
            ' relative parent value for that region.
            For Each r In cont.Regions.Where(Function(x) CType(x.GetValue("RelativeParentID"), Guid) = reg.ID)
                Dim attr = r.GetAttribute("RelativeParentID")
                attr.Value = Guid.Empty
            Next
        End If

        ' Create the element - either a region container or an app element
        Dim newEl As clsApplicationElement = Nothing

        ' If there's a screenshot in the identifiers, we create a region container
        For Each id As clsIdentifierInfo In idInfos
            If id.ID = clsAMI.ScreenshotIdentifierId Then
                newEl = CreateNewRegionContainer(currMember, typeInfo, idInfos)
                Exit For
            End If
        Next

        ' If we didn't find a screenshot, it must be a standard app element
        If newEl Is Nothing Then _
            newEl = CreateNewElement(currMember, typeInfo, idInfos)

        'Update treenode's reference to newly spied element
        appExplorer.ReplaceMember(currMember, newEl)
        mCurrentApplicationMember = newEl
        UpdateUserInterface()

        ' The model has changed, so allow the user to apply the change
        btnApply.Enabled = mHasPermissionToEdit

        Return newEl

    End Function

    ''' <summary>
    ''' Captures the regions for the given region container by popping up the region
    ''' mapper and allowing the user to specify regions on it.
    ''' </summary>
    ''' <param name="cont">The container on which to capture the regions. This must
    ''' have a valid screenshot to display on which the regions can be captured.
    ''' </param>
    Private Sub CaptureRegions(ByVal cont As clsRegionContainer)

        Using screenshot As Bitmap = cont.ScreenshotImage

            ' Get the regions from the user
            Dim regs As ICollection(Of SpyRegion) =
             frmRegionEditor.GetRegions(Me, screenshot, cont.Regions)

            ' Null return implies that 'Cancel' was clicked. Just return, leaving
            ' the regions on the container as is
            If regs Is Nothing Then Return

            ' Clear down the old regions from the container
            ClearRegions(cont)

            ' Go through each region and add an element representing it into the
            ' application model.
            Dim children As New HashSet(Of clsApplicationMember)
            Dim processed As New HashSet(Of SpyRegion)

            For Each spyReg In regs
                If Not processed.Contains(spyReg) Then _
                    clsRegionConvert.ConvertToAppRegion(spyReg, cont, screenshot, children, processed)
            Next

            If children.Count > 0 Then _
             appExplorer.AddChildren(cont, children)

        End Using

    End Sub

    ''' <summary>
    ''' Clears the regions from the given container, also clearing them from the
    ''' application explorer at the same time.
    ''' </summary>
    ''' <param name="cont">The container whose regions should be cleared</param>
    Private Sub ClearRegions(ByVal cont As clsRegionContainer)

        ' We have the ok... get to it.
        ' We need to delete the regions from the model too, so get to that
        For Each childReg As clsApplicationRegion In cont.Regions
            ' If we find the currently selected region in those that we are clearing
            ' auto-select the container in its place
            If childReg Is mCurrentApplicationMember Then _
             appExplorer.SelectedMember = cont
            Try
                appExplorer.DeleteMember(childReg)
            Catch
                'The Member was not found, its parent was deleted first when clearing
            End Try
        Next

        ' They are all gone, so clear the regions from the container
        cont.Regions.Clear()

    End Sub

    ''' <summary>
    ''' Hides the application modeller window by minimizing it
    ''' </summary>
    Private Sub HideIntegrationAssistant()
        mSavedWindowState = WindowState
        WindowState = FormWindowState.Minimized
    End Sub

    ''' <summary>
    ''' Restores the application modeller window by un-minimizing it
    ''' </summary>
    Private Sub RestoreIntegrationAssistant()
        WindowState = mSavedWindowState
        Activate()
        'Bug3845 Restore scrollbar on attributes properly
        elemEditor.RestoreScroll()
    End Sub

    ''' <summary>
    ''' Handles the 'Highlight' button being clicked.
    ''' </summary>
    Private Sub btnShowElement_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnShowElement.Click

        Dim el As clsApplicationElement = appExplorer.SelectedElement
        If el IsNot Nothing Then
            Try
                'Make sure not in the way of target application
                HideIntegrationAssistant()

                ' Get all of the identifiers
                Dim idents As New List(Of clsIdentifierInfo)(
                 el.GetSupplementaryIdentifiers())
                idents.AddRange(el.ActiveIdentifiers)

                'Do the highlighting
                Dim amiMsg As clsAMIMessage = Nothing
                Dim success = mAMI.HighlightWindow(
                    el.Type, idents, New Dictionary(Of String, String), amiMsg)

                RestoreIntegrationAssistant()
                If Not success Then ShowMessage(
                    amiMsg.HelpTopic, My.Resources.x0HighlightingResults1,
                    amiMsg.FriendlyMessageType, amiMsg.Message)

            Catch ex As Exception
                RestoreIntegrationAssistant()
                UserMessage.Err(
                    ex, My.Resources.InternalErrorUnexpectedError0, ex.Message)

            End Try
        End If

    End Sub
#End Region

    ''' <summary>
    ''' Shows a message window.
    ''' </summary>
    ''' <param name="topic">The help topic to show to the user. -1 will not set a
    ''' help topic.</param>
    ''' <param name="msg">The message to give.</param>
    Private Sub ShowMessage(topic As Integer, msg As String)
        If topic > 0 Then msg &= vbCrLf & vbCrLf & String.Format(My.Resources.SeeHelpTopic0, topic)
        UserMessage.Show(msg)
    End Sub

    ''' <summary>
    ''' Shows a message window.
    ''' </summary>
    ''' <param name="topic">The help topic to show to the user. -1 will not set a
    ''' help topic.</param>
    ''' <param name="formatMsg">The message to give with format placeholders.</param>
    ''' <param name="args">The args to insert into the format message.</param>
    Private Sub ShowMessage(
     topic As Integer, formatMsg As String, ParamArray args() As Object)
        ShowMessage(topic, String.Format(formatMsg, args))
    End Sub

#Region "Changed Value Events on UI Controls - we update corresponding application member"

    ''' <summary>
    ''' Handles the text changing in the element's name - this just stops and starts
    ''' a timer which ensures that the element name is committed after the user has
    ''' stopped typing for short period.
    ''' </summary>
    Private Sub HandleElementNameTextChange(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtElementName.TextChanged
        timerElementName.Stop()
        timerElementName.Start()
    End Sub

    ''' <summary>
    ''' Handles the committing of the value of the name text box, ensuring that the
    ''' model backing the interface is updated, as well as other representations of
    ''' the name (ie. the application explorer treeview).
    ''' </summary>
    Private Sub HandleElementNameCommit(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtElementName.Validated, timerElementName.Tick
        timerElementName.Stop()
        ' Don't be messing around with the names if we're currently populating this
        ' form, or if the app info page is currently being displayed, or if the name
        ' hasn't changed.
        Dim emptyLabel As String = RelativeSpyRegionTypeConverter.EmptyLabel
        If mUpdatingUI OrElse ApplicationInfoVisible OrElse
         mCurrentApplicationMember.Name = txtElementName.Text OrElse
         String.Equals(txtElementName.Text, emptyLabel,
                       StringComparison.InvariantCultureIgnoreCase) _
         Then Return

        mCurrentApplicationMember.Name = txtElementName.Text
        appExplorer.UpdateName(mCurrentApplicationMember)

        btnApply.Enabled = mHasPermissionToEdit

    End Sub

    ''' <summary>
    ''' Handles the type or datatype of the current element changing from the UI.
    ''' This ensures that the backing model is updated to match the interface
    ''' </summary>
    Private Sub HandleElemTypeChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles cmbElemType.SelectedIndexChanged, cmbDataType.SelectedIndexChanged

        ' If currently populating, exit this event handler immediately
        If mUpdatingUI Then Return

        ' Get the currently selected element
        Dim el As clsApplicationElement = CurrentElement
        If el Is Nothing Then Return

        ' And the combo box which changed
        Dim cmb As ComboBox = TryCast(sender, ComboBox)

        Dim et As clsElementTypeInfo = SelectedElementType
        Dim dt As DataType = SelectedDataType

        If cmb Is cmbElemType Then
            ' If the user has changed the data type, leave it overridden to the
            ' value that they set, otherwise use the default in the new elem type
            Dim changed As Boolean = (dt <> el.DefaultDataType)
            el.Type = SelectedElementType
            If Not changed AndAlso dt <> el.DefaultDataType _
             AndAlso DataTypeAvailable(el.DefaultDataType) Then
                cmbDataType.SelectedValue = el.DefaultDataType
            End If
        ElseIf cmb Is cmbDataType Then
            el.DataType = DirectCast(cmbDataType.SelectedValue, DataType)
        End If

        btnApply.Enabled = mHasPermissionToEdit

    End Sub

    ''' <summary>
    ''' Indicates whether or not the passed DataType is available for selection in
    ''' the Data Type combobox.
    ''' </summary>
    ''' <param name="dt">The DataType to check for</param>
    ''' <returns>True if the passed datatype exists in the combobox, otherwise
    ''' False</returns>
    Private Function DataTypeAvailable(dt As DataType) As Boolean
        For Each obj In cmbDataType.Items
            Dim dti = TryCast(obj, clsDataTypeInfo)
            If dti IsNot Nothing AndAlso dti.Value = dt Then Return True
        Next
        Return False
    End Function

#End Region

#End Region

    ''' <summary>
    ''' Handle form closing
    ''' </summary>
    Protected Overrides Sub OnFormClosing(ByVal e As FormClosingEventArgs)
        'Unlock any parent object on closing form
        mAMI.CancelSpy(Nothing)
        UnlockParent()
    End Sub

    Private Sub HandleAppClosed(
        appInfo As clsApplicationTypeInfo,
        newStatus As clsAMI.ApplicationStatus) Handles mAMI.ApplicationStatusChanged

        ' If this doesn't apply to us, or we're pre/past caring, ignore it.
        If IsDisposed OrElse Not IsHandleCreated OrElse
         Not mAMI.ApplicationsAreEqual(appInfo, mApplicationDefinition.ApplicationInfo) _
         Then Return

        Dim applicationLaunched = If(newStatus = clsAMI.ApplicationStatus.Launched, True, False)
        Try
            'This invocation can fail if the application is shut down
            'and yet (not IsDisposed) and IsHandleCreated are still both true.
            '
            'See Control.WaitForWaitHandle():
            'If Application.ThreadContext.FromId(this.CreateThreadId)
            'returns null then this invocation results in a null reference
            'exception. See bug 2853.
            Invoke(New Action(Of Boolean)(AddressOf UpdateLaunchState), applicationLaunched)
        Catch
            'Do nothing
        End Try

    End Sub


    Private Sub HandleAttributeChanged() Handles elemEditor.AttributeChanged
        If Not mUpdatingUI Then btnApply.Enabled = mHasPermissionToEdit
    End Sub

    Private Sub HandleVisibleChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles Me.VisibleChanged
        If Not Visible Then mAMI.CancelSpy(Nothing)
    End Sub

    Private Sub HandleClearAttrs(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnClearAttributes.Click
        If UserMessage.YesNo(
         My.Resources.AreYouSureYouWishToClearTheAttributesForThisElement) =
         MsgBoxResult.Yes Then

            Dim currMember As clsApplicationMember = appExplorer.SelectedMember
            If currMember Is Nothing Then
                UserMessage.Show(My.Resources.NoElementSeemsToBeSelectedFromTheTreePleaseFirstSelectAnElement)
                Return
            End If

            Dim newMember As _
             New clsApplicationElementGroup(mCurrentApplicationMember.Name)

            ' No need for recursion since the added children
            ' will already have the required structure
            For Each child In currMember.ChildMembers
                newMember.AddMember(child)
            Next

            appExplorer.ReplaceMember(currMember, newMember)
            mCurrentApplicationMember = newMember
            UpdateUserInterface()
            btnApply.Enabled = mHasPermissionToEdit
        End If
    End Sub

    Private Sub PaintBottomStrip(ByVal Sender As Object, ByVal e As PaintEventArgs) Handles pnlBottomStrip.Paint
        GraphicsUtil.Draw3DLine(e.Graphics,
         New Point(0, 1), ListDirection.LeftToRight, pnlBottomStrip.Width)
    End Sub

    Private Sub txtDescription_TextChanged(
     ByVal sender As Object, ByVal e As EventArgs) Handles txtDescription.TextChanged
        If mUpdatingUI Then Return
        Dim el As clsApplicationElement = Me.CurrentElement
        If el Is Nothing Then Return
        el.Description = txtDescription.Text
        btnApply.Enabled = mHasPermissionToEdit
    End Sub

    Private Sub txtNotes_TextChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtNotes.TextChanged
        If mUpdatingUI Then Return
        Dim el As clsApplicationElement = Me.CurrentElement
        If el Is Nothing Then Return
        el.Narrative = txtNotes.Text
        btnApply.Enabled = mHasPermissionToEdit
    End Sub

    Private Sub SplitterMoved(ByVal sender As Object, ByVal e As SplitterEventArgs) _
     Handles splitMain.SplitterMoved
        'http://social.msdn.microsoft.com/forums/en-US/winforms/thread/37e8066b-2502-4ffa-bf29-cc32c0199629/
        DirectCast(sender, SplitContainer).Invalidate(True)
    End Sub

    ''' <summary>
    ''' Handles the regions button being clicked. If an existing screenshot exists,
    ''' this opens it. Otherwise, it creates a new screenshot.
    ''' </summary>
    Private Sub btnRegions_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnRegions.Click
        If mnuOpenScreenshot.Enabled Then
            HandleOpenScreenshotClick(sender, e)
        ElseIf mnuNewScreenshot.Enabled Then
            HandleNewScreenshotClick(sender, e)
        Else
            Dim msg As String = My.Resources.ThereIsNoScreenshotAvailableAndTheApplicationIsNotAvailableToGetAScreenshotFrom
            MessageBox.Show(msg, My.Resources.ScreenshotUnavailable,
             MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
    End Sub

    ''' <summary>
    ''' Gets the container and region associated with the given member if it is a
    ''' region container or a region itself.
    ''' </summary>
    ''' <param name="mem">The member to extract container and region from</param>
    ''' <param name="cont">The container of the region in the relationship - if
    ''' <paramref name="mem"/> is a container, this will contain that. If it is a
    ''' region, it will contain the container of that region. If it is neither, this
    ''' will be set to null.</param>
    ''' <param name="reg">The region described in the relationship, if there is one.
    ''' If <paramref name="mem"/> is a container, this will be set to null. If it
    ''' is a region, this will contain a reference to it. If it is neither, this
    ''' will be set to null.</param>
    Private Sub ExtractContainerAndRegion(ByVal mem As clsApplicationMember,
     ByRef cont As clsRegionContainer, ByRef reg As clsApplicationRegion)
        cont = TryCast(mem, clsRegionContainer)
        reg = TryCast(mem, clsApplicationRegion)
        If cont Is Nothing AndAlso reg IsNot Nothing Then cont = reg.Container
    End Sub

    ''' <summary>
    ''' Handles a new screenshot being requested for a region container
    ''' </summary>
    Private Sub HandleNewScreenshotClick(
     ByVal sender As Object, ByVal e As EventArgs) Handles mnuNewScreenshot.Click

        Dim cont As clsRegionContainer = Nothing
        Dim reg As clsApplicationRegion = Nothing
        ExtractContainerAndRegion(CurrentMember, cont, reg)

        If cont Is Nothing Then _
         MessageBox.Show(My.Resources.CouldNotFindRegionContainer) : Return

        Dim bmp As Bitmap = Nothing
        OnBeginSpy()
        Try
            bmp = mAMI.SpyBitmap()
        Catch ex As Exception
            UserMessage.Show(
             My.Resources.ErrorAttemptingToGetAScreenshotOfTheElement & ex.Message)
        Finally
            OnEndSpy()
        End Try

        ' If we don't have an image at this point we can't really do anything
        If bmp Is Nothing Then Return

        ' Update the screenshot and capture the regions on it
        cont.ScreenshotImage = bmp
        CaptureRegions(cont)

        UpdateRegionsButton()

    End Sub

    ''' <summary>
    ''' Handles the 'open current screenshot' menu item on the split button.
    ''' </summary>
    Private Sub HandleOpenScreenshotClick(
     ByVal sender As Object, ByVal e As EventArgs) _
     Handles mnuOpenScreenshot.Click

        Dim cont As clsRegionContainer = Nothing
        Dim reg As clsApplicationRegion = Nothing
        ExtractContainerAndRegion(CurrentElement, cont, reg)

        If cont Is Nothing Then _
         MessageBox.Show(My.Resources.CouldNotFindRegionContainer) : Return

        Try
            ' Check that there's a screenshot image there
            If Not cont.HasScreenshot Then

                Dim applicationLaunched = mAMI.ApplicationIsLaunched
                ' If the app is launched we can redirect to get a new screenshot
                If applicationLaunched Then
                    Dim resp As DialogResult = MessageBox.Show(
                     My.Resources.TheRegionContainerHasNoScreenshotDoYouWantToSpyANewScreenshot, My.Resources.MissingScreenshot,
                     MessageBoxButtons.OKCancel, MessageBoxIcon.Question)

                    ' If they want to spy a new screenshot, send them to the relevant
                    ' handler, otherwise, ensure the regions button is up to date
                    If resp = DialogResult.OK _
                     Then HandleNewScreenshotClick(sender, e) _
                     Else UpdateRegionsButton()

                    ' Either way, there's nothing more we can do here.
                    Return

                End If

                ' otherwise, no screenshot image, but app is not launched. Just error
                MessageBox.Show(My.Resources.TheRegionContainerHasNoScreenshot,
                 My.Resources.MissingScreenshot, MessageBoxButtons.OK, MessageBoxIcon.Error)

                UpdateRegionsButton()
                Return

            End If

            CaptureRegions(cont)

        Catch ex As Exception
            UserMessage.Show(
             My.Resources.ErrorOccurredWhileRetrievingScreenshotRegions, ex)

        End Try

        UpdateRegionsButton()

    End Sub

    ''' <summary>
    ''' Handles the 'clear regions' menu item on the split button
    ''' </summary>
    Private Sub HandleClearRegions(ByVal sender As Object, ByVal e As EventArgs)

        Dim cont As clsRegionContainer = Nothing
        Dim reg As clsApplicationRegion = Nothing
        ExtractContainerAndRegion(CurrentElement, cont, reg)
        If cont Is Nothing Then
            MessageBox.Show(My.Resources.CouldNotFindRegionContainer)
            Return
        End If

        Dim msg As String =
         My.Resources.AreYouSureYouWantToClearAllRegionsOnThe0Element

        ' Prefix with a warning about other regions, if the user has selected
        ' a region to clear the regions from
        If reg IsNot Nothing Then msg = My.Resources.ThisWillClearAllRegionsDefinedOnThe0ElementNotJustTheSelectedOne & vbCrLf & msg

        ' Format in the container name to the message
        msg = String.Format(msg, cont.Name)

        Dim confirm As DialogResult = MessageBox.Show(msg, My.Resources.ClearRegions,
         MessageBoxButtons.OKCancel, MessageBoxIcon.Question)

        If confirm <> DialogResult.OK Then Return

        ClearRegions(cont)
        UpdateRegionsButton()

    End Sub

    ''' <summary>
    ''' Initialises a newly created application element with data from the member
    ''' it is replacing and the type and identifier info gleaned from an 'Identify'
    ''' operation (from the spy tool or the app navigator)
    ''' </summary>
    ''' <param name="currMem">The current member which is to be replaced with the
    ''' new element.</param>
    ''' <param name="newEl">The newly created element which should be initialised by
    ''' this method.</param>
    ''' <param name="typeInfo">The typeinfo from the element which has been
    ''' identified.</param>
    ''' <param name="idInfos">The collection of identifiers which identify the
    ''' element which is being initialised.</param>
    ''' <returns>The given new element, after it has been initialised.</returns>
    Private Function CreateInto(ByVal currMem As clsApplicationMember,
     ByVal newEl As clsApplicationElement, ByVal typeInfo As clsElementTypeInfo,
     ByVal idInfos As ICollection(Of clsIdentifierInfo)) As clsApplicationElement

        ' Set all the current data into the new element.
        If currMem IsNot Nothing Then newEl.ReplaceMember(currMem)

        ' Then pick up the new spied data and set that into the new element
        newEl.Type = typeInfo
        newEl.BaseType = typeInfo
        Dim dt As DataType = DataType.unknown
        clsEnum.TryParse(typeInfo.DefaultDataType, dt)
        newEl.DataType = dt

        newEl.AllIdentifiers = idInfos

        Return newEl

    End Function

    ''' <summary>
    ''' Creates a new element into the given application element using the given
    ''' element type and identifiers.
    ''' </summary>
    ''' <param name="currMem">The current element which is to be used as a base for
    ''' the new element.</param>
    ''' <param name="typeInfo">The element type to create</param>
    ''' <param name="idInfos">The identifiers which make up the new element</param>
    ''' <returns>The newly created application element</returns>
    Private Function CreateNewElement(
     ByVal currMem As clsApplicationMember, ByVal typeInfo As clsElementTypeInfo,
     ByVal idInfos As ICollection(Of clsIdentifierInfo)) As clsApplicationElement
        Return CreateInto(currMem,
         New clsApplicationElement(CurrentMember.Name), typeInfo, idInfos)
    End Function

    ''' <summary>
    ''' Creates a new element into the given application element using the given
    ''' element type and identifiers.
    ''' </summary>
    ''' <param name="currMem">The current element which is to be used as a base for
    ''' the new element.</param>
    ''' <param name="typeInfo">The element type to create</param>
    ''' <param name="idInfos">The identifiers which make up the new element</param>
    ''' <returns>The newly created application element</returns>
    Private Function CreateNewRegionContainer(
     ByVal currMem As clsApplicationMember, ByVal typeInfo As clsElementTypeInfo,
     ByVal idInfos As ICollection(Of clsIdentifierInfo)) As clsApplicationElement
        Return CreateInto(currMem,
         New clsRegionContainer(CurrentMember.Name), typeInfo, idInfos)
    End Function

    ''' <summary>
    ''' Handles the UI Automation Navigator button being clicked, spawning the
    ''' <see cref="frmUIAutomationNavigator">UI Automation Navigator</see> form
    ''' </summary>
    Private Sub HandleUIAutomationNavigatorClick(
     sender As Object, e As EventArgs) Handles miUIAutomationNavigator.Click
        ' Ensure that the user is happy with the current element being
        ' overwritten by the navigator operation
        If Not PassesOverwriteElementWarning() Then Return

        Using nav As New frmUIAutomationNavigator(mAMI)
            AddHandler nav.ElementChosen, AddressOf HandleNavigatorElementChosen
            Hide()
            Try
                nav.ShowDialog()
            Finally
                Show()
            End Try
        End Using

    End Sub


    ''' <summary>
    ''' Handles the Application Navigator button being clicked, spawning the
    ''' <see cref="frmApplicationTreeNavigator">Application Tree Navigator</see> form
    ''' </summary>
    Private Sub HandleAppNavigatorClick(
     sender As Object, e As EventArgs) Handles miAppNavigator.Click

        ' Ensure that the user is happy with the current element being
        ' overwritten by the navigator operation
        If Not PassesOverwriteElementWarning() Then Exit Sub

        Using nav As New frmApplicationTreeNavigator(
         mAMI, mApplicationDefinition.ApplicationInfo.ID, mLastModelTree)
            AddHandler nav.ElementChosen, AddressOf HandleNavigatorElementChosen
            AddHandler nav.RegionEditorRequest,
             AddressOf HandleNavigatorRegionEditorRequest
            Hide()
            Try
                nav.ShowDialog()
            Finally
                Show()
            End Try
            mLastModelTree = nav.ElementTree
        End Using

    End Sub

    ''' <summary>
    ''' Handles an element being chosen in the application navigator
    ''' </summary>
    Private Sub HandleNavigatorElementChosen(
     ByVal sender As Object, ByVal e As ElementChosenEventArgs)
        ' Only support one for now - probably this will add them as children
        ' of the currently selected element when multiselect is enabled
        With e.FirstElement
            UpdateCurrentMember(.ElementType, .Identifiers.Values)
        End With
    End Sub

    ''' <summary>
    ''' Handles an element being chosen in the application navigator and the request
    ''' to open that element in the region editor
    ''' </summary>
    Private Sub HandleNavigatorRegionEditorRequest(
     ByVal sender As Object, ByVal e As RegionEditorRequestEventArgs)
        ' Only support one for now - probably this will add them as children
        ' of the currently selected element when multiselect is enabled
        Dim el As clsElement = e.FirstElement
        Try
            Dim pic As clsPixRect = mAMI.GetSnapshot(
             el.ElementType, el.Identifiers.Values, Nothing)
            Dim bmp As Bitmap = pic.ToBitmap()

            Dim newEl As clsApplicationElement = CreateNewRegionContainer(
             CurrentElement, el.ElementType, el.Identifiers.Values)

            ' We need to add the screenshot one ourselves, since it won't be
            ' there when coming from the navigator.
            newEl.Attributes.Add(New clsApplicationAttribute(
             clsAMI.ScreenshotIdentifierId,
             New clsProcessValue(DataType.image, pic.ToString()), False, True))

            appExplorer.ReplaceMember(CurrentMember, newEl)
            mCurrentApplicationMember = newEl
            UpdateUserInterface()

            ' We need to put this into the message queue to deal with since the
            ' navigator form is still open (and modal), and it needs to get out
            ' of the way before we open the region editor.
            BeginInvoke(
             New Action(Of clsRegionContainer)(AddressOf CaptureRegions),
             DirectCast(mCurrentApplicationMember, clsRegionContainer))

        Catch ex As Exception
            UserMessage.Err(ex, String.Format(My.Resources.AnErrorOccurredWhileGettingTheSnapshotExMessage, ex.Message))
            Return
        End Try


    End Sub

    ''' <summary>
    ''' Set window title to include parent object name (if appropriate)
    ''' </summary>
    Private Sub SetTitle()
        Text = ApplicationProperties.ApplicationModellerName
        If Me.ReadOnly Then Text &= My.Resources.xReadOnly
        If mParent.Value IsNot Nothing Then
            lblModelOwner.Text = String.Format(My.Resources.ApplicationModelBelongsToParentObject0, mParent.Value)
            lblModelOwner.BackColor = Color.Red
            lblModelOwner.ForeColor = Color.White
        Else
            lblModelOwner.Text = My.Resources.ApplicationModelBelongsToThisObject
            lblModelOwner.BackColor = Color.Transparent
            lblModelOwner.ForeColor = SystemColors.ControlText
        End If
    End Sub

    ''' <summary>
    ''' Attempts to lock the parent object containing the shared model
    ''' </summary>
    ''' <param name="id">The object ID of the parent</param>
    ''' <param name="name">The parent object name</param>
    ''' <returns></returns>
    Private Function LockParent(ByVal id As Guid, ByVal name As String) As Boolean
        
        Dim lockUsername As String = Nothing
        Dim lockMachineName As String = Nothing

        'Check parent is not already locked
        If gSv.ProcessIsLocked(id, lockUsername, lockMachineName) Then
            UserMessage.OK(My.Resources.TheSharedApplicationModelCannotBeModifiedOwningObject0HasBeenLockedForEditingBy, name, lockUsername)
            Return False
        End If

        'Attempt to lock it
        Try
            gSv.LockProcess(id)
        Catch ex As Exception
            UserMessage.Show(My.Resources.ErrorLockingParentModel & ex.Message)
            Return False
        End Try

        'If successfully locked save the object id
        mLockedObject = id
        Return True
    End Function

    ''' <summary>
    ''' Updates the parent object with changes to the shared model
    ''' </summary>
    Private Function UpdateParent() As Boolean
        Dim sErr As String = Nothing
        Dim parentXML As String = Nothing, newXML As String = Nothing
        Dim modDate As Date

        'Get XML for parent object
        Try
            parentXML = gSv.GetProcessXML(mParent.Key)
        Catch ex As Exception
            parentXML = ""
        End Try

        'Load process and update application definition
        Dim parentObject As clsProcess = clsProcess.FromXml(Options.Instance.GetExternalObjectsInfo(), parentXML, False)
        parentObject.ApplicationDefinition = mApplicationDefinition
        newXML = parentObject.GenerateXML()

        'Update the parent (ensuring we still have a lock on it)
        Dim lockUsername As String = Nothing
        Dim lockMachineName As String = Nothing
        If gSv.ProcessIsLocked(mParent.Key, lockUsername, lockMachineName) AndAlso lockUsername = Auth.User.CurrentName Then
            Try
                gSv.EditProcess(mParent.Key, parentObject.Name, parentObject.Version, parentObject.Description, newXML, Nothing, My.Resources.SharedApplicationModelAmendedViaObject & mObjectName,
                    modDate, parentObject.GetDependencies(False))
                parentObject.Dispose()
                Return True
            Catch ex As Exception
                sErr = ex.Message
            End Try
        Else
            sErr = My.Resources.YourLockOnTheParentObjectHasBeenRemovedByAnotherUser
        End If

        parentObject.Dispose()

        'If we get here then something went wrong
        UserMessage.Show(String.Format(My.Resources.FailedToUpdateParentApplicationModelSErr, sErr))
        Return False
    End Function

    ''' <summary>
    ''' Unlocks the parent object
    ''' </summary>
    Private Sub UnlockParent()
        If mLockedObject = Guid.Empty Then Return

        'Unlock parent object
        Dim sErr As String = Nothing
        gSv.UnlockProcess(mLockedObject)
        mLockedObject = Guid.Empty
    End Sub

    Public Overrides Function GetHelpFile() As String
        Return "frmIntegrationAssistant.htm"
    End Function

    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub
End Class
