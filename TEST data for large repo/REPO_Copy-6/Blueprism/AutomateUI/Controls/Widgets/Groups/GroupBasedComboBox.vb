Imports System.Xml
Imports AutomateControls
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Core.Xml

Public MustInherit Class GroupBasedComboBox : Inherits StyledComboBox

#Region " Class-scope Declarations "

    ''' <summary>
    ''' The default increment to add for each indented group member
    ''' </summary>
    Protected Const DefaultIndent As Integer = 16

#End Region

#Region " Member Variables "

    ' The type of tree to retrieve
    Private mTreeType As GroupTreeType

    ' The tree that this combo box is displaying
    Private mTree As IGroupTree

    ' The filter to apply to the combo box
    Private mFilter As Predicate(Of IGroupMember)

    ' The group filter to apply to the combo box
    Private mGroupFilter As Predicate(Of IGroupMember)

#End Region

#Region " Auto-Properties "

    ''' <summary>
    ''' Gets or sets whether empty groups should be displayed in this combo box.
    ''' </summary>
    <Category("Appearance"), DefaultValue(False), Description(
        "Whether to show empty groups in this combo box or not")>
    Public Property ShowEmptyGroups As Boolean

    ''' <summary>
    ''' Gets or sets the text to display to indicate 'nothing selected' for this
    ''' combobox. When <see cref="NoSelectionAllowed"/> is set, the top entry in the
    ''' combo will be a ComboBoxItem with this text and no tag set in it.
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(""),
     Description("The text to display when nothing is selected")>
    Public Property NoSelectionText As String = ""

    ''' <summary>
    ''' Gets or sets whether a 'nothing selected' item should be included in this
    ''' combo box. If true, this will cause an entry to be included at the top of the
    ''' combo box to represent 'nothing selected'. <seealso cref="NoSelectionText"/>
    ''' </summary>
    <Browsable(True), Category("Behavior"), DefaultValue(False),
     Description("Whether to allow no members to be selected in this combo box")>
    Public Property NoSelectionAllowed As Boolean


#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty group combo box
    ''' </summary>
    Public Sub New()
        DropDownStyle = ComboBoxStyle.DropDownList
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Shadow of ComboBox.DropDownStyle to only allow a style value of
    ''' <see cref="ComboBoxStyle.DropDownList"/>, and to hide this property from
    ''' editors and designers.
    ''' </summary>
    <Browsable(False), EditorBrowsable(EditorBrowsableState.Never),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Shadows Property DropDownStyle As ComboBoxStyle
        Get
            Return MyBase.DropDownStyle
        End Get
        Set(value As ComboBoxStyle)
            If value <> ComboBoxStyle.DropDownList Then Throw New ArgumentException(
                "GroupMemberComboBox only supports DropDownList styles")
            MyBase.DropDownStyle = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the filter to apply to the group tree backing this combo box.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property Filter As Predicate(Of IGroupMember)
        Get
            If mFilter Is Nothing Then Return Function(m) True
            Return mFilter
        End Get
        Set(value As Predicate(Of IGroupMember))
            mFilter = value
            If IsHandleCreated Then RefreshData(False)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the group filter to apply to the group tree backing this combo box.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property GroupFilter As Predicate(Of IGroupMember)
        Get
            If mGroupFilter Is Nothing Then Return Function(m) True
            Return mGroupFilter
        End Get
        Set(value As Predicate(Of IGroupMember))
            mGroupFilter = value
            If IsHandleCreated Then RefreshData(False)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the tree type for this combo, retrieving the data from the
    ''' current store as required.
    ''' </summary>
    <Browsable(True), Category("Data"), DefaultValue(GroupTreeType.None),
     Description("The type of tree to display in this combo box")>
    Public Property TreeType As GroupTreeType
        Get
            Return mTreeType
        End Get
        Set(value As GroupTreeType)
            If value = mTreeType Then Return
            mTreeType = value
            If IsHandleCreated Then RefreshData(True)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the selected member its ID in this combo box
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedId As Object
        Get
            Dim mem As IGroupMember = SelectedMember
            If mem Is Nothing Then Return Nothing Else Return mem.Id
        End Get
        Set(value As Object)
            For Each item As ComboBoxItem In Items
                Dim mem As IGroupMember = TryCast(item.Tag, IGroupMember)
                If mem Is Nothing AndAlso value Is Nothing Then
                    SelectedItem = item
                    Return

                ElseIf mem IsNot Nothing AndAlso Object.Equals(mem.Id, value) Then
                    SelectedItem = item
                    Return

                End If
            Next
            ' If we didn't find it, set the selected value to nothing
            SelectedItem = Nothing
        End Set
    End Property


    Public ReadOnly Property NoSelectionSelected As Boolean
        Get
            Return NoSelectionAllowed AndAlso DirectCast(Items(0), ComboBoxItem).Checked
        End Get
    End Property


    ''' <summary>
    ''' Gets or sets the selected member its (Guid) ID in this combo box. This will
    ''' return <see cref="Guid.Empty"/> if there is no selected group member.
    ''' </summary>
    ''' <exception cref="InvalidCastException">If the ID of the selected member is
    ''' not a Guid.</exception>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedIdAsGuid As Guid
        Get
            Dim id As Object = SelectedId
            Return If(id Is Nothing, Guid.Empty, DirectCast(SelectedId, Guid))
        End Get
        Set(value As Guid)
            SelectedId = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the selected group member in this combo box.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public Property SelectedMember As IGroupMember
        Get
            Dim item As ComboBoxItem = SelectedComboBoxItem
            If item Is Nothing Then Return Nothing
            Return DirectCast(item.Tag, IGroupMember)
        End Get
        Set(value As IGroupMember)
            SelectedComboBoxItem = FindComboBoxItemByTag(value)
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Called when the handle for this control is created.
    ''' </summary>
    Protected Overrides Sub CreateHandle()
        MyBase.CreateHandle()
        RefreshData(False)
    End Sub

    ''' <summary>
    ''' Refreshes the data in this combo box from the store
    ''' </summary>
    Protected Overridable Sub RefreshData(reloadFromStore As Boolean)
        Try
            mTree = GetGroupStore().GetTree(TreeType, Me.Filter,
                                            Me.GroupFilter, reloadFromStore, True, False)
        Catch ex As Exception
            UserMessage.Err(
                ex, My.Resources.GroupBasedComboBox_AnErrorOccurredWhileLoadingThe0Tree, TreeType)
            Return

        End Try

        ' Save the selected ID, if there is one
        Dim selected As Object = SelectedId
        BeginUpdate()
        Try
            Items.Clear()
            If NoSelectionAllowed Then Items.Add(New ComboBoxItem(NoSelectionText) _
                                        With {.Checked = True, .Checkable = True})
            If mTree IsNot Nothing Then
                ' We don't show the root for the drop down list, so just add entries
                ' for the roots members (and their subtrees)
                For Each mem As IGroupMember In mTree.Root : AddEntry(mem, 0) : Next
            End If

        Finally
            EndUpdate()
        End Try
        SelectedId = selected

    End Sub

    ''' <summary>
    ''' If the NoSelection item is being used and has been picked - alter the other items
    ''' accordingly.
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overrides Sub OnSelectionChangeCommitted(e As EventArgs)
        If Checkable AndAlso SelectedIndex >= 0 Then
            Dim selected = DirectCast(Items(SelectedIndex), ComboBoxItem)
            If NoSelectionAllowed AndAlso selected.Checkable Then
                Dim noSelection = DirectCast(Items(0), ComboBoxItem)
                If SelectedIndex = 0 Then ' NoSelection has been selected
                    For Each y As ComboBoxItem In Items.Cast(Of ComboBoxItem)
                        If y IsNot noSelection AndAlso y.Checkable Then
                            y.Checked = Not noSelection.Checked
                        End If
                    Next
                Else
                    ' something else has been selected
                    DirectCast(Items(0), ComboBoxItem).Checked = False
                    noSelection.Checked = False
                End If
                Invalidate()
            End If
        End If
        MyBase.OnSelectionChangeCommitted(e)
    End Sub

    ''' <summary>
    ''' Update the caption if noselection is being used with mutiselect
    ''' </summary>
    ''' <returns></returns>
    Protected Overrides Function GetCaptionText() As String
        Dim currentItemText = CType(SelectedItem, ComboBoxItem).Text

        If Not Checkable Then
            Return currentItemText
        End If

        If NoSelectionAllowed AndAlso DirectCast(Items(0), ComboBoxItem).Checked Then
            Return NoSelectionText
        End If

        Dim selected = Items.Cast(Of ComboBoxItem).Where(Function(x) x.Checkable AndAlso x.Checked).Select(Function(y) y.Text)
        Dim count = selected.Distinct().Count()
        If count = 1 Then
            Return selected.First()
        Else
            Return String.Format(My.Resources.GroupBasedComboBox_0ItemsSelected, count)
        End If

    End Function

    ''' <summary>
    ''' Adds an entry representing the given group member
    ''' </summary>
    ''' <param name="mem">The member to add an entry for</param>
    ''' <param name="indent">The indent at which to add the entry</param>
    Protected MustOverride Sub AddEntry(mem As IGroupMember, indent As Integer)


    Const _AllCheckedAttribute As String = "AllSelected"
    Const _HeaderKey As String = "GroupComboBox"
    Const _ItemHeaderKey As String = "SelectedItems"
    Const _ItemKey As String = "SelectedItem"

    ''' <summary>
    ''' Gets/Sets an XML representation of the state of the selected items.
    ''' </summary>
    ''' <returns></returns>
    Public Property SelectedItemsXML As String
        Get

            Dim doc As New XmlDocument()
            Dim outer As XmlElement = DirectCast(doc.AppendChild(doc.CreateElement(_HeaderKey)), XmlElement)

            Dim _all As ComboBoxItem = Nothing

            ' is there an all option
            If NoSelectionAllowed AndAlso Items IsNot Nothing AndAlso Items.Count > 0 Then
                _all = DirectCast(Items(0), ComboBoxItem)
                outer.SetAttribute(_AllCheckedAttribute, XmlConvert.ToString(_all.Checked))
            End If

            ' Only write the filters if 'all' isn't selected.
            If _all Is Nothing OrElse Not _all.Checked Then
                Dim list As XmlElement = doc.CreateElement(_ItemHeaderKey)
                GetCheckedItems().ForEach(Sub(y) list.AppendChild(doc.CreateElement(_ItemKey)).InnerText = y)
                outer.AppendChild(list)
            End If
            Return doc.OuterXml
        End Get
        Set(value As String)

            Dim _all As ComboBoxItem = Nothing

            ' is there an all option
            If NoSelectionAllowed AndAlso Items IsNot Nothing AndAlso Items.Count > 0 Then
                _all = CType(Items(0), ComboBoxItem)
            End If

            'deal with legacy value of sinlge item
            If Not value.StartsWith("<" + _HeaderKey) Then
                MyBase.ClearCheckedItems()
                If _all IsNot Nothing AndAlso value = NoSelectionText Then
                    _all.Checked = True
                Else
                    CheckItem(value)
                End If
            Else
                ' Otherwise it's the newer xml format
                Dim xd As New ReadableXmlDocument(value)
                Dim e As XmlElement = xd.DocumentElement

                If _all IsNot Nothing AndAlso e.Attributes IsNot Nothing AndAlso e.Attributes(_AllCheckedAttribute) IsNot Nothing Then
                    _all.Checked = XmlConvert.ToBoolean(e.Attributes(_AllCheckedAttribute).Value)
                    SetAllCheckableItems(_all.Checked)
                End If

                'Only set other filters if 'all' is not set.
                If _all Is Nothing OrElse Not _all.Checked Then
                    MyBase.ClearCheckedItems()
                    For Each node As XmlNode In e.SelectNodes("//" + _ItemKey)
                        Try
                            CheckItem(node.InnerText)
                            ' Ignore it if it no longer exists.
                        Catch generatedExceptionName As KeyNotFoundException
                        End Try
                    Next
                End If
            End If

            ' force the control to refresh
            Me.Invalidate()
        End Set
    End Property

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        '
        'GroupBasedComboBox
        '
        Me.ResumeLayout(False)

    End Sub

    Public Sub RefreshFromStore()
        RefreshData(True)
    End Sub

    Private Sub GroupBasedComboBox_KeyDown(sender As Object, e As KeyEventArgs) Handles MyBase.KeyDown
        If (e.KeyCode = Keys.Enter) Then
            Dim comboBox As GroupMemberComboBox
            Dim selectedItem As ComboBoxItem

            comboBox = CType(sender, GroupMemberComboBox)
            selectedItem = CType(comboBox.SelectedItem, ComboBoxItem)

            If (Not selectedItem.Selectable) Then
                e.Handled = True
            End If
        End If
    End Sub

    ''' <summary>
    ''' Gets a list of all items with a status of selected
    ''' </summary>
    ''' <returns>A list of strings representing GroupBasedComboBox items that have been selected.</returns>
    Public Function GetCheckedItems() As List(Of String)

        Return Items.Cast(Of ComboBoxItem).Where(Function(x) x.Checked).
            Select(Function(x) If(TryCast(x.Tag, IGroupMember)?.Name, x.Text)).
            ToList()

    End Function

#End Region

End Class
