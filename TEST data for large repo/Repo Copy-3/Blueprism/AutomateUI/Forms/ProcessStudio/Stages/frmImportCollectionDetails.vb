Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages

''' Project  : Automate
''' 
''' <summary>
''' A one-page wizard for importing a collection definition from the parameters
''' of a selected business object action into the current stage.
''' </summary>
Friend Class frmImportCollectionDetails
    Inherits frmWizard

#Region " Inner ComboBox classes "
    ' These classes allow the actual data to be stored in the combo box, so 
    ' we can access them easier than constantly navigating the process object model.

    ''' <summary>
    ''' Base class for a combo box item - only defines the name property
    ''' </summary>
    Private MustInherit Class ComboBoxItem
        Public MustOverride ReadOnly Property Name() As String
    End Class

    ''' <summary>
    ''' Combo Box Item representing a business object action.
    ''' </summary>
    Private Class ActionItem : Inherits ComboBoxItem
        ' The action represented by this combo box item.
        Private mAction As clsBusinessObjectAction

        ''' <summary>
        ''' Creates a new action combo box item representing the given action.
        ''' </summary>
        ''' <param name="action">The action that this combo box item should
        ''' represent.</param>
        Public Sub New(ByVal action As clsBusinessObjectAction)
            mAction = action
        End Sub

        ''' <summary>
        ''' The name of the action represented by this combo box item
        ''' </summary>
        Public Overrides ReadOnly Property Name() As String
            Get
                Return mAction.FriendlyName()
            End Get
        End Property

        ''' <summary>
        ''' The action represented by this combo box item.
        ''' </summary>
        Public ReadOnly Property Value() As clsBusinessObjectAction
            Get
                Return mAction
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Combo box item representing a business object action parameter
    ''' </summary>
    Private Class ParamItem : Inherits ComboBoxItem
        ' The parameter represented by this item.
        Private mParam As clsProcessParameter

        ''' <summary>
        ''' Creates a new parameter combo box item.
        ''' </summary>
        ''' <param name="param">The parameter that this combo box item should
        ''' represent.</param>
        Public Sub New(ByVal param As clsProcessParameter)
            mParam = param
        End Sub

        ''' <summary>
        ''' The name of the parameter represented by this combo box item
        ''' </summary>
        Public Overrides ReadOnly Property Name() As String
            Get
                Return String.Format(My.Resources.ParamItem_ParamName0InputOutput1, mParam.FriendlyName,
                 IIf(mParam.Direction = ParamDirection.Out, My.Resources.ParamItem_Output, My.Resources.ParamItem_Input))
            End Get
        End Property

        ''' <summary>
        ''' The parameter represented by this combo box item
        ''' </summary>
        Public ReadOnly Property Value() As clsProcessParameter
            Get
                Return mParam
            End Get
        End Property
    End Class

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new import collection details dialog, not yet associated
    ''' with any collection definition manager.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new import collection details dialog backed by the specified
    ''' collection definition manager.
    ''' </summary>
    ''' <param name="mgr">The manager to which the selected fields should be
    ''' imported.</param>
    Public Sub New(ByVal mgr As ICollectionDefinitionManager)
        Me.New(mgr, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new import collection details dialog backed by the specified
    ''' collection definition manager and hosted within the given stage.
    ''' </summary>
    ''' <param name="mgr">The manager to which the selected fields should be
    ''' imported.</param>
    ''' <param name="stg">The stage which is hosting this collection, if
    ''' appropriate - this will be used to retrieve the business objects that
    ''' the owning process can see, otherwise all available business objects 
    ''' without any process constraints will be used.
    ''' </param>
    Public Sub New(
     ByVal mgr As ICollectionDefinitionManager, ByVal stg As clsCollectionStage)
        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        mManager = mgr
        mStage = stg
        Me.objBluebar.Title = My.Resources.frmImportCollectionDetails_ChooseTheParameterFromABusinessObjectAction
        Me.Title = My.Resources.frmImportCollectionDetails_ChooseTheParameterFromABusinessObjectAction

    End Sub


#End Region

#Region " Windows Form Designer generated code "

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Private WithEvents cmbParam As System.Windows.Forms.ComboBox
    Private WithEvents cmbAction As System.Windows.Forms.ComboBox
    Friend WithEvents cmbObject As AutomateControls.StyledComboBox
    Private WithEvents treePreview As System.Windows.Forms.TreeView
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmImportCollectionDetails))
        Dim Label5 As System.Windows.Forms.Label
        Dim Label4 As System.Windows.Forms.Label
        Dim Label3 As System.Windows.Forms.Label
        Dim Label1 As System.Windows.Forms.Label
        Me.cmbParam = New System.Windows.Forms.ComboBox()
        Me.cmbAction = New System.Windows.Forms.ComboBox()
        Me.treePreview = New System.Windows.Forms.TreeView()
        Me.cmbObject = New AutomateControls.StyledComboBox()
        Label5 = New System.Windows.Forms.Label()
        Label4 = New System.Windows.Forms.Label()
        Label3 = New System.Windows.Forms.Label()
        Label1 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'btnCancel
        '
        resources.ApplyResources(Me.btnCancel, "btnCancel")
        '
        'btnNext
        '
        resources.ApplyResources(Me.btnNext, "btnNext")
        '
        'btnBack
        '
        Me.btnBack.BackColor = System.Drawing.SystemColors.Control
        Me.btnBack.ForeColor = System.Drawing.SystemColors.ControlText
        resources.ApplyResources(Me.btnBack, "btnBack")
        '
        'objBluebar
        '
        resources.ApplyResources(Me.objBluebar, "objBluebar")
        '
        'Label5
        '
        resources.ApplyResources(Label5, "Label5")
        Label5.Name = "Label5"
        '
        'Label4
        '
        resources.ApplyResources(Label4, "Label4")
        Label4.Name = "Label4"
        '
        'Label3
        '
        resources.ApplyResources(Label3, "Label3")
        Label3.Name = "Label3"
        '
        'Label1
        '
        resources.ApplyResources(Label1, "Label1")
        Label1.Name = "Label1"
        '
        'cmbParam
        '
        resources.ApplyResources(Me.cmbParam, "cmbParam")
        Me.cmbParam.DisplayMember = "Name"
        Me.cmbParam.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbParam.Name = "cmbParam"
        Me.cmbParam.ValueMember = "Value"
        '
        'cmbAction
        '
        resources.ApplyResources(Me.cmbAction, "cmbAction")
        Me.cmbAction.DisplayMember = "Name"
        Me.cmbAction.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbAction.Name = "cmbAction"
        Me.cmbAction.ValueMember = "Action"
        '
        'treePreview
        '
        resources.ApplyResources(Me.treePreview, "treePreview")
        Me.treePreview.Name = "treePreview"
        '
        'cmbObject
        '
        resources.ApplyResources(Me.cmbObject, "cmbObject")
        Me.cmbObject.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbObject.Checkable = False
        Me.cmbObject.DisabledItemColour = System.Drawing.Color.LightGray
        Me.cmbObject.DisplayMember = "FriendlyName"
        Me.cmbObject.DropDownBackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbObject.FormattingEnabled = True
        Me.cmbObject.Name = "cmbObject"
        '
        'frmImportCollectionDetails
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.cmbObject)
        Me.Controls.Add(Label1)
        Me.Controls.Add(Me.treePreview)
        Me.Controls.Add(Me.cmbParam)
        Me.Controls.Add(Label5)
        Me.Controls.Add(Me.cmbAction)
        Me.Controls.Add(Label4)
        Me.Controls.Add(Label3)
        Me.MaximizeBox = True
        Me.Name = "frmImportCollectionDetails"
        Me.Title = "Choose the parameter from a business object action"
        Me.Controls.SetChildIndex(Me.objBluebar, 0)
        Me.Controls.SetChildIndex(Me.btnBack, 0)
        Me.Controls.SetChildIndex(Me.btnNext, 0)
        Me.Controls.SetChildIndex(Me.btnCancel, 0)
        Me.Controls.SetChildIndex(Label3, 0)
        Me.Controls.SetChildIndex(Label4, 0)
        Me.Controls.SetChildIndex(Me.cmbAction, 0)
        Me.Controls.SetChildIndex(Label5, 0)
        Me.Controls.SetChildIndex(Me.cmbParam, 0)
        Me.Controls.SetChildIndex(Me.treePreview, 0)
        Me.Controls.SetChildIndex(Label1, 0)
        Me.Controls.SetChildIndex(Me.cmbObject, 0)
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region " Selected Item properties "

    ''' <summary>
    ''' The currently selected business object, or null if none is currently
    ''' selected
    ''' </summary>
    Private ReadOnly Property SelectedBusinessObject() As clsBusinessObject
        Get
            Dim objItem = cmbObject.SelectedComboBoxItem
            If objItem Is Nothing Then Return Nothing
            Return TryCast(objItem.Tag, clsBusinessObject)
        End Get
    End Property

    ''' <summary>
    ''' The currently selected business object action, or null if none is currently
    ''' selected.
    ''' </summary>
    Private ReadOnly Property SelectedAction() As clsBusinessObjectAction
        Get
            Dim item As ActionItem = TryCast(cmbAction.SelectedItem, ActionItem)
            If item Is Nothing Then Return Nothing Else Return item.Value
        End Get
    End Property

    ''' <summary>
    ''' The currently selected parameter in this form, or null if none is currently
    ''' selected.
    ''' </summary>
    Private ReadOnly Property SelectedParameter() As clsProcessParameter
        Get
            Dim item As ParamItem = TryCast(cmbParam.SelectedItem, ParamItem)
            If item Is Nothing Then Return Nothing Else Return item.Value
        End Get
    End Property

#End Region

#Region " Member variables & Properties "

    ''' <summary>
    ''' Constant to hold the string "None" - used in the combo boxes to allow
    ''' 'no selection'.
    ''' </summary>
    Private csNone As String = My.Resources.None

    ''' <summary>
    ''' The collection stage that this form is importing fields into.
    ''' Note that this is only modified if the user clicks the OK button, so,
    ''' assuming that this is the desired effect, no cloning of the data is
    ''' necessary.
    ''' </summary>
    Public Property Manager() As ICollectionDefinitionManager
        Get
            Return mManager
        End Get
        Set(ByVal Value As ICollectionDefinitionManager)
            mManager = Value
        End Set
    End Property
    Private mManager As ICollectionDefinitionManager

    ''' <summary>
    ''' The stage which is currently being manipulated - this is used to load
    ''' the business objects. If it is not set, the business objects are loaded
    ''' each time the form is opened - otherwise, the objects can be cached
    ''' against the parent process and, more importantly, represent those
    ''' viewable by the process - including any changes that have been made in
    ''' the current debugging session.
    ''' </summary>
    Public Property Stage() As clsCollectionStage
        Get
            Return mStage
        End Get
        Set(ByVal value As clsCollectionStage)
            mStage = value
        End Set
    End Property
    Private mStage As clsCollectionStage

#End Region

#Region " Event handlers "

    ''' <summary>
    ''' Handler for the form load event - ensures that the business objects combo
    ''' box is populated with all the business objects found in the system.
    ''' </summary>
    Private Sub HandleFormLoaded(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.Load
        Me.SetMaxSteps(0)
        Me.UpdateObjectsList()
    End Sub

    ''' <summary>
    ''' Handles a business object being selected - ensures that the actions combo
    ''' box is updated with the actions from the selected object.
    ''' </summary>
    Private Sub HandleObjectSelected(ByVal sender As Object, ByVal e As EventArgs) _
     Handles cmbObject.SelectedIndexChanged
        UpdateActionList()
    End Sub

    ''' <summary>
    ''' Handles a business object action being selected - ensures that the params
    ''' combo box is updated with parameters from the selected action.
    ''' </summary>
    Private Sub HandleActionSelected(ByVal sender As Object, ByVal e As EventArgs) _
     Handles cmbAction.SelectedIndexChanged
        UpdateParamsList()
    End Sub

    ''' <summary>
    ''' Handles an item being selected in the Output combobox - this updates
    ''' the treeview with a preview of the collection to be imported.
    ''' </summary>
    ''' <param name="sender">The combo box which has had an item selected.
    ''' </param>
    ''' <param name="e">The args defining the event.</param>
    Private Sub HandleOutputSelected(ByVal sender As Object, ByVal e As EventArgs) _
     Handles cmbParam.SelectedIndexChanged
        Dim param As clsProcessParameter = Me.SelectedParameter
        If param Is Nothing OrElse param.CollectionInfo Is Nothing Then
            treePreview.Nodes.Clear()
            treePreview.Tag = Nothing

        ElseIf param IsNot treePreview.Tag Then
            treePreview.BeginUpdate()
            Try
                treePreview.Nodes.Clear()
                PopulateTree(treePreview.Nodes.Add(param.FriendlyName).Nodes, param.CollectionInfo)
                treePreview.Tag = param
                treePreview.ExpandAll()

            Finally
                treePreview.EndUpdate()

            End Try
        End If

    End Sub

#End Region

#Region " Control Update methods "

    ''' <summary>
    ''' Updates the business objects list with a list of business objects in the
    ''' current environment.
    ''' </summary>
    Private Sub UpdateObjectsList()

        cmbObject.Items.Clear()
        mNoneComboBoxItem = New AutomateControls.ComboBoxItem(My.Resources.None, True)
        cmbObject.Items.Add(mNoneComboBoxItem)

        ' If we don't have a stage with which we can load the business objects,
        ' load them directly... otherwise use the stage's parent process to
        ' retrieve the business objects.
        If mStage Is Nothing Then
            Using bos As New clsGroupBusinessObject(Options.Instance.GetExternalObjectsInfo())
                For Each obr In bos.Children
                    DescendChildren(obr, 0)
                Next
            End Using
        Else
            For Each obr In mStage.Process.GetBusinessObjects().Children
                DescendChildren(obr, 0)
            Next
        End If

        cmbObject.SelectFirst()
    End Sub

    Private mNoneComboBoxItem As AutomateControls.ComboBoxItem

    Private Sub DescendChildren(obj As clsBusinessObject, indent As Integer)
        Const increment = 16

        If Not obj.Valid Then Return

        Dim name As String = obj.FriendlyName

        Dim group = TryCast(obj, clsGroupBusinessObject)
        If group IsNot Nothing Then
            Dim item As New AutomateControls.ComboBoxItem(name)
            item.Indent = indent
            item.Style = FontStyle.Bold
            cmbObject.Items.Add(item)
            For Each childObj In group.Children
                DescendChildren(childObj, indent + increment)
            Next
        Else
            ' Add the objects to the combobox which have an action with a well-defined
            ' Collection (input or output)
            Dim actionProcessed As Boolean = False
            For Each action As clsBusinessObjectAction In obj.GetActions()
                For Each param As clsProcessParameter In action.GetParameters()
                    If param.HasDefinedCollection() Then
                        Dim item As New AutomateControls.ComboBoxItem(name, obj)
                        item.Indent = indent
                        cmbObject.Items.Add(item)
                        actionProcessed = True
                        Exit For
                    End If
                Next
                If actionProcessed Then Exit For
            Next action
        End If

    End Sub
    ''' <summary>
    ''' Updates the combo box containing business object actions with relevant
    ''' actions from the currently selected business object.
    ''' </summary>
    Private Sub UpdateActionList()

        cmbAction.Items.Clear()

        Dim businessObject As clsBusinessObject = SelectedBusinessObject
        If businessObject Is Nothing Then
            cmbAction.Enabled = False
            cmbParam.Enabled = False

        Else
            cmbAction.Enabled = True
            cmbAction.Sorted = True

            'only use the business object's actions which have defined collection parameters
            If businessObject IsNot Nothing Then
                For Each action As clsBusinessObjectAction In businessObject.GetActions()
                    For Each param As clsProcessParameter In action.GetParameters()
                        If param.HasDefinedCollection() Then
                            ' the action has at least one well-defined collection param - 
                            ' add this action and move on to next action
                            cmbAction.Items.Add(New ActionItem(action))
                            Exit For
                        End If
                    Next
                Next
            End If

            'now that we have added the actions in alphabetical order
            'add the word "None" at the top of the list
            cmbAction.Sorted = False
            cmbAction.Items.Insert(0, csNone)
            cmbAction.SelectedIndex = 0

        End If
    End Sub

    ''' <summary>
    ''' Updates the combo box containing parameters with the relevant parameters
    ''' from the currently selected business object action.
    ''' </summary>
    Private Sub UpdateParamsList()
        Dim action As clsBusinessObjectAction = SelectedAction
        If action Is Nothing Then
            cmbParam.Enabled = False

        Else
            cmbParam.Enabled = True
            cmbParam.Items.Clear()
            cmbParam.Sorted = True

            'only use collection data type output parameters
            'add the parameters
            For Each param As clsProcessParameter In action.GetParameters()
                If param.HasDefinedCollection() Then
                    ' The combo box is defined to pick out the text from the 'Name'
                    ' property in the held items, so we can store a wrapper around the
                    ' actual object.
                    cmbParam.Items.Add(New ParamItem(param))
                End If
            Next

            'now that we have added the outputs in alphabetical order,
            'add the word "None"
            cmbParam.Sorted = False
            cmbParam.Items.Insert(0, csNone)
            cmbParam.SelectedIndex = 0

        End If
    End Sub

    ''' <summary>
    ''' Recursively populates the given treenode collection with information from
    ''' the given collection info object.
    ''' The node text is written out in the format "{name} ({datatype})" and it
    ''' has no input functionality - it's documentary only.
    ''' </summary>
    ''' <param name="nodes">The node collection to which the nodes representing 
    ''' field definitions in the collection info should be added</param>
    ''' <param name="info">The collection info from which to draw the field info.
    ''' </param>
    Private Sub PopulateTree(ByVal nodes As TreeNodeCollection, ByVal info As clsCollectionInfo)
        If info Is Nothing OrElse info.Count = 0 Then Return
        ' Write out the collection info into the given node collection.
        For Each fld As clsCollectionFieldInfo In info
            Dim n As TreeNode = nodes.Add(String.Format(My.Resources.frmImportCollectionDetails_FieldName0FieldType1, fld.DisplayName, clsProcessDataTypes.GetFriendlyName(fld.DataType)))
            If fld.HasChildren() Then PopulateTree(n.Nodes, fld.Children)
        Next
    End Sub

#End Region

#Region " Wizard methods "

    ''' <summary>
    ''' Updates the page - there is only one page in this wizard (?!), so this
    ''' updates the collection definition on the associated collection stage.
    ''' </summary>
    Protected Overrides Sub UpdatePage()
        If MyBase.GetStep() > 0 Then
            Dim param As clsProcessParameter = Me.SelectedParameter
            If param IsNot Nothing AndAlso param.HasDefinedCollection() Then
                ' Go through each field in turn - if the manager doesn't have a 
                ' param with the specified name, add it - otherwise append an
                ' incremental number to make it unique and add that.
                For Each fld As clsCollectionFieldInfo In param.CollectionInfo
                    Dim name As String = fld.Name
                    Dim displayName = fld.DisplayName
                    Dim i As Integer = 0
                    ' Make sure we do not already have a field with the same name
                    ' And if we do, come up with a name suffixed with an incrementing number
                    While mManager.ContainsField(name)
                        i += 1
                        name = String.Format(My.Resources.frmImportCollectionDetails_FieldName0Count1, fld.Name, CStr(i))
                    End While
                    ' If we had to come up with a new name, clone the field and
                    ' set the new name in it, so we can add the field directly
                    ' (and ensure all its data is picked up)
                    If i > 0 Then
                        fld = DirectCast(fld.Clone(), clsCollectionFieldInfo)
                        fld.Name = name
                        fld.DisplayName = displayName


                    End If
                    mManager.AddField(fld)
                Next

            End If

            Me.DialogResult = System.Windows.Forms.DialogResult.OK
            Me.Close()
        End If
    End Sub


    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesCollection.htm"
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
