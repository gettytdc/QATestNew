Imports AutomateControls.Forms
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Utility
Imports System.IO
Imports System.Reflection


''' <summary>
''' Properties form for business object info. Basically same as process info but with
''' extra tab page for global code.
''' </summary>
Friend Class frmStagePropertiesObjectInfo

#Region " Member Vars / Constants "

    ' A clone of the original process stage
    Private mOriginal As clsProcessStage
    Private mDirectoryWarningDisplayed As Boolean = False

    ' The friendly names for the supported code stage / global code languages
    Private Class FriendlyNames
        Public VB As String = "Visual Basic" 'My.Resources.FriendlyNames_VisualBasic
        Public CSharp As String = "C#" 'My.Resources.FriendlyNames_C
        Public JSharp As String = "Visual J#" 'My.Resources.FriendlyNames_VisualJ
    End Class
    Dim myFriendlyNames As FriendlyNames = New FriendlyNames()

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new object properties info form for the process with the given ID
    ''' </summary>
    ''' <param name="gProcessID">The process ID for which a properties form is
    ''' required.</param>
    Public Sub New(ByVal gProcessID As Guid)
        MyBase.New(gProcessID)

        Me.InitializeComponent()

        cmbLanguage.Items.Add(myFriendlyNames.VB)
        cmbLanguage.Items.Add(myFriendlyNames.CSharp)
        cmbLanguage.Items.Add(myFriendlyNames.JSharp)

    End Sub

#End Region

#Region " Event Handling Methods "

    ''' <summary>
    ''' Handles the 'Add DLL Reference' button being pressed.
    ''' </summary>
    Private Sub HandleAddDllRef(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnAddReference.Click
        AddNewRow(lstDlls)
    End Sub

    ''' <summary>
    ''' Handles the 'Remove DLL Reference' button being pressed.
    ''' </summary>
    Private Sub HandleRemoveDllRef(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnRemoveRef.Click
        RemoveCurrentRow(lstDlls)
    End Sub

    ''' <summary>
    ''' Handles the Browse button for DLL references being pressed
    ''' </summary>
    Private Sub HandleBrowseDll(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnBrowse.Click

        ' Might as well check that we have somewhere to put a DLL ref before we start
        Dim currRow As clsListRow = lstDlls.CurrentEditableRow
        If currRow Is Nothing Then
            UserMessage.Show(
             My.Resources.frmStagePropertiesObjectInfo_UnableToPopulateFieldWithSelectionThereIsNoActiveRowPleaseSelectARowByPlacingTh)
            Return
        End If

        ' Capture the DLL that we want to add a reference to
        Using fo As New OpenFileDialog()
            With fo
                .AddExtension = True
                .CheckFileExists = True
                .CheckPathExists = True
                .DereferenceLinks = True
                .Filter = My.Resources.frmStagePropertiesObjectInfo_DynamicLinkLibrariesDllDll
                .Multiselect = False
                .InitialDirectory = Environment.SystemDirectory &
                 "\..\Microsoft.NET\Framework\v" & Environment.Version.ToString(3)
            End With

            If fo.ShowDialog() <> DialogResult.OK Then Return

            If Not mDirectoryWarningDisplayed AndAlso Not IsRecommendedDirectory(Path.GetDirectoryName(fo.FileName)) Then
                Dim popup = New PopupForm(My.Resources.frmStagePropertiesObjectInfo_Warning,
                                          My.Resources.frmStagePropertiesObjectInfo_DllLocationWarningMessage,
                                          My.Resources.frmStagePropertiesObjectInfo_Okay)
                AddHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOkClick
                popup.ShowDialog()
                mDirectoryWarningDisplayed = True
            End If

            Dim ref As String = fo.FileName
            If ref = "" Then Return

            currRow.Items(0).Value = New clsProcessValue(ref)
            lstDlls.EditRowControl().Items(0).NestedControl.Text = ref

        End Using
    End Sub

    Private Shared Sub HandleOnBtnOkClick(sender As Object, e As EventArgs)
        Dim popup = CType(sender, PopupForm)
        RemoveHandler popup.OnBtnOKClick, AddressOf HandleOnBtnOkClick
        popup.Close()
    End Sub

    ''' <summary>
    ''' Handles the 'Add Namespace' button being pressed
    ''' </summary>
    Private Sub HandleAddNamespace(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnAddNamespace.Click
        AddNewRow(lstNamespaces)
    End Sub

    ''' <summary>
    ''' Handles the 'Remove Namespace' button being pressed
    ''' </summary>
    Private Sub HandleRemoveNamespace(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnRemoveNamespace.Click
        RemoveCurrentRow(lstNamespaces)
    End Sub

    ''' <summary>
    ''' Handles the 'Check Code' button being pressed
    ''' </summary>
    Private Sub btnCheckCode_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnCheckCode.Click

        Dim proc As clsProcess = mProcessInfoStage.Process

        If proc.CompilerRunner Is Nothing Then
            UserMessage.Show(My.Resources.frmStagePropertiesObjectInfo_UnableToCheckCode)
        Else
            UpdateStage()
            'This goes against the way properties forms usaully work.
            'Normally the process is updated after the properties form
            'closes with DialogResult.OK. In this case the process is 
            'updated now so that the code fragment can be tested. The
            'FormClosing event handler will undo any changes if the OK
            'button has not been pressed.
            proc.SetStage(mProcessInfoStage.Id, mProcessInfoStage)
            Try
                Using f As New frmValidateResults(
                 proc, mProcessInfoStage, ValidateProcessResult.SourceTypes.Code)
                    f.Owner = Me
                    f.StartPosition = FormStartPosition.CenterParent
                    f.ShowInTaskbar = False
                    f.ShowDialog()
                End Using

            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.frmStagePropertiesObjectInfo_UnexpectedError0, ex.Message), ex)

            End Try
        End If
    End Sub

    ''' <summary>
    ''' Handles the selected row being changed on one of the listviews.
    ''' </summary>
    Private Sub HandleSelectedRowChanged(
     ByVal sender As Object, ByVal e As ListRowChangedEventArgs) _
     Handles lstDlls.EditableRowChanged, lstNamespaces.EditableRowChanged
        UpdateButtons()
    End Sub

    ''' <summary>
    ''' Handles the form closing. This ensures that the process info stage is reset
    ''' to what it was when this form started if OK was not pressed.
    ''' </summary>
    Protected Overrides Sub OnFormClosing(ByVal e As FormClosingEventArgs)
        MyBase.OnFormClosing(e)
        If Not e.Cancel AndAlso DialogResult <> DialogResult.OK Then
            'Put stage back as it was.
            mProcessInfoStage.Process.SetStage(
             mProcessInfoStage.GetStageID(), mOriginal)
        End If
    End Sub

    ''' <summary>
    ''' Handles the Shared Object checkbox changing.
    ''' </summary>
    Private Sub HandleSharedObjectChange(sender As Object, e As EventArgs) _
     Handles cbSharedObject.Click
        If cbSharedObject.Checked AndAlso gSv.IsReferenced(
         New clsProcessParentDependency(mProcessInfoStage.Process.Name)) Then
            UserMessage.Show(My.Resources.frmStagePropertiesObjectInfo_ThisObjectMustBeDefinedAsShareableBecauseOtherObjectsReferenceItSApplicationMod)
            Exit Sub
        End If
        cbSharedObject.Checked = Not cbSharedObject.Checked
    End Sub
#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Populates the list of external references using the stage object.
    ''' </summary>
    Private Sub PopulateListViews()
        'Set up headers
        lstDlls.Columns.Add(My.Resources.frmStagePropertiesObjectInfo_ExternalReferences)
        lstDlls.LastColumnAutoSize = True

        lstNamespaces.Columns.Add(My.Resources.frmStagePropertiesObjectInfo_NamespaceImports)
        lstNamespaces.LastColumnAutoSize = True

        'For each reference populate listview control with entry
        For Each s As String In mProcessInfoStage.AssemblyReferences
            AddSingleLineListRow(lstDlls, s)
        Next
        'If no references exist, add a row by default
        If lstDlls.Rows.Count = 0 Then AddSingleLineListRow(lstDlls, "")

        'For each import populate listview control with entry
        For Each s As String In mProcessInfoStage.NamespaceImports
            AddSingleLineListRow(lstNamespaces, s)
        Next
        'If no namespaces exist, add a row by default
        If lstNamespaces.Rows.Count = 0 Then AddSingleLineListRow(lstNamespaces, "")

        lstDlls.UpdateView()
        lstNamespaces.UpdateView()
        UpdateButtons()
    End Sub

    ''' <summary>
    ''' Populates the form using data from the stage
    ''' object.
    ''' </summary>
    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()

        PopulateListViews()
        mEditor.Populate(mProcessInfoStage.CodeText, mProcessInfoStage.Language)
        If Not String.IsNullOrEmpty(mProcessInfoStage.Language) Then
            Dim lingo As String
            Select Case mProcessInfoStage.Language
                Case "csharp" : lingo = myFriendlyNames.CSharp
                Case "vjsharp" : lingo = myFriendlyNames.JSharp
                Case Else : lingo = myFriendlyNames.VB
            End Select
            cmbLanguage.Text = lingo

        ElseIf cmbLanguage.Items.Count > 0 Then
            cmbLanguage.Text = myFriendlyNames.VB

        End If
        PopulateRunModes()

        If mProcessInfoStage.Process.ParentObject Is Nothing Then
            cbSharedObject.Checked = mProcessInfoStage.Process.IsShared
            cbSharedObject.Text = My.Resources.frmStagePropertiesObjectInfo_Shareable
            lblSharedObject.Text = My.Resources.frmStagePropertiesObjectInfo_ShareableObjectsCanExposeTheirApplicationModelToOtherObjectsModelSharingAtRunti
        Else
            cbSharedObject.Checked = True
            cbSharedObject.Enabled = False
            cbSharedObject.Text = My.Resources.frmStagePropertiesObjectInfo_Shared
            lblSharedObject.Text = My.Resources.frmStagePropertiesObjectInfo_ThisObjectIsSharedAtRuntimeASingleInstanceOfTheObjectAndApplicationModelIsCreat
        End If

        mOriginal = mProcessInfoStage.Clone()
    End Sub

    ''' <summary>
    ''' Hides publication checkbox etc.
    ''' </summary>
    Public Overrides Sub PrepareForObjectStudio()
        MyBase.PrepareForObjectStudio()
        chbxPublish.Visible = False
    End Sub

    ''' <summary>
    ''' Populates the run modes combo box with the available options.
    ''' </summary>
    ''' <remarks>Member stage data should already be populated, so that the correct
    ''' option can be selected in the user interface.</remarks>
    Private Sub PopulateRunModes()
        Select Case mProcessStage.Process.ObjectRunMode
            Case BusinessObjectRunMode.Foreground : rdoForeground.Checked = True
            Case BusinessObjectRunMode.Background : rdoBackground.Checked = True
            Case Else : rdoExclusive.Checked = True
        End Select
    End Sub

    ''' <summary>
    ''' Makes buttons enabled/disabled according to the status
    ''' of the application.
    ''' </summary>
    Private Sub UpdateButtons()
        btnAddReference.Enabled = IsEditable
        btnRemoveRef.Enabled = (lstDlls.CurrentEditableRow IsNot Nothing)
        btnRemoveNamespace.Enabled = (lstNamespaces.CurrentEditableRow IsNot Nothing)
        btnBrowse.Enabled = btnRemoveRef.Enabled
    End Sub

    Private Shared Function IsRecommendedDirectory(directory As String) As Boolean
        Try
            If String.Equals(directory, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), StringComparison.CurrentCultureIgnoreCase) Then
                Return True
            End If

            Dim validPaths = Environment.GetEnvironmentVariable("path").Split(";"c)
            Return validPaths.Any(Function(x) String.Equals(x, directory, StringComparison.CurrentCultureIgnoreCase))
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Applies the changes to the stage object that the form represents, calling the
    ''' base implementation first.
    ''' </summary>
    ''' <returns>Returns true on success.</returns>
    Protected Overrides Function ApplyChanges() As Boolean
        If MyBase.ApplyChanges() Then
            UpdateStage()
            Dim proc As clsProcess = mProcessInfoStage.Process
            If rdoForeground.Checked Then
                proc.ObjectRunMode = BusinessObjectRunMode.Foreground
            ElseIf rdoBackground.Checked Then
                proc.ObjectRunMode = BusinessObjectRunMode.Background
            Else
                proc.ObjectRunMode = BusinessObjectRunMode.Exclusive
            End If
            If cbSharedObject.Enabled Then _
             proc.IsShared = cbSharedObject.Checked
            Return True
        Else
            Return False
        End If

    End Function

    ''' <summary>
    ''' Read the changes from the appropriate user interface controls into
    ''' the embedded ProcessInfoStage
    ''' </summary>
    Private Sub UpdateStage()
        'Add external references
        mProcessInfoStage.AssemblyReferences.Clear()
        For Each r As clsListRow In lstDlls.Rows
            Dim val As String = CStr(r.Items(0).Value).Trim()
            If val <> "" Then mProcessInfoStage.AssemblyReferences.Add(val)
        Next

        'Add namespaces
        mProcessInfoStage.NamespaceImports.Clear()
        For Each r As clsListRow In lstNamespaces.Rows
            Dim val As String = CStr(r.Items(0).Value).Trim()
            If val <> "" Then mProcessInfoStage.NamespaceImports.Add(val)
        Next

        'Add code
        mProcessInfoStage.CodeText = mEditor.Code

        Dim lingo As String
        Select Case cmbLanguage.Text
            Case myFriendlyNames.CSharp : lingo = "csharp"
            Case myFriendlyNames.JSharp : lingo = "vjsharp"
            Case Else : lingo = "visualbasic"
        End Select
        mProcessInfoStage.Language = lingo

    End Sub

    ''' <summary>
    ''' Adds a single line text value into a listrow, which it then adds into the
    ''' given listview.
    ''' </summary>
    ''' <param name="lv">The ListView to add the row into</param>
    ''' <param name="val">The value to set in the item within the row</param>
    ''' <returns>The listrow created and added into the listview</returns>
    Private Function AddSingleLineListRow(
     ByVal lv As ctlListView, ByVal val As String) As clsListRow
        Dim row As New clsListRow(lv)
        row.AddItem(val, False)
        lv.Rows.Add(row)
        Return row
    End Function

    ''' <summary>
    ''' Removes the current editable row from the given listview.
    ''' </summary>
    ''' <param name="lv">The listview whose current editable row should be removed
    ''' </param>
    Private Sub RemoveCurrentRow(ByVal lv As ctlListView)
        lv.Rows.Remove(lv.CurrentEditableRow)
        lv.UpdateView()
        UpdateButtons()
    End Sub

    ''' <summary>
    ''' Adds a new empty row to the given listview and focuses on it
    ''' </summary>
    ''' <param name="lv">The listview to which the new row should be added</param>
    Private Sub AddNewRow(ByVal lv As ctlListView)
        lv.CurrentEditableRow = AddSingleLineListRow(lv, "")
        lv.FocusEditColumn(0)
        lv.UpdateView()
        UpdateButtons()
    End Sub

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesObjectInfo.htm"
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

#End Region

End Class
