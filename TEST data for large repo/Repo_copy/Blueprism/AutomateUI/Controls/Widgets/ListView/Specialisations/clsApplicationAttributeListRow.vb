Imports BluePrism.AutomateProcessCore
Imports BluePrism.AMI
Imports AutomateControls.ComboBoxes
Imports BluePrism.ApplicationManager.AMI

Imports ComparisonTypes = BluePrism.AMI.clsAMI.ComparisonTypes

Friend Class clsApplicationAttributeListRow
    Inherits clsListRow

#Region " Published Events "

    ''' <summary>
    ''' Raised when an attribute is edited.
    ''' </summary>
    Public Event AttributeChanged()

#End Region

#Region " Member Variables "

    Private mReadOnly As Boolean

    Private mAttr As clsApplicationAttribute

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new listrow representing an application attribute owned ultimately
    ''' by the given listview.
    ''' </summary>
    ''' <param name="lv">The listview owning this listrow</param>
    Public Sub New(ByVal lv As ctlListView)
        MyBase.New(lv)
        SetNullValues()
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Provides access to the Application Attribute of this row.
    ''' </summary>
    Public Property ApplicationAttribute() As clsApplicationAttribute
        Get
            Return mAttr
        End Get
        Set(ByVal value As clsApplicationAttribute)
            mAttr = value
            mReadOnly = (value Is Nothing OrElse value.IsSystem)
            SetDisplayValues()
        End Set
    End Property

    ''' <summary>
    ''' Sets this listrow to be readonly. Note that this doesn't have an effect on
    ''' the currently editing row - ie. if the editable controls have already been
    ''' created by this listrow, it has no reference to them to be able to modify
    ''' their state.
    ''' </summary>
    ''' <remarks>This can be changed by setting the attribute in this row - system
    ''' attributes are, by default, read only.</remarks>
    Public Property [ReadOnly]() As Boolean
        Get
            Return mReadOnly
        End Get
        Set(ByVal value As Boolean)
            If value <> mReadOnly Then
                mReadOnly = value
            End If
        End Set
    End Property

#End Region

#Region " Methods "

    Public Sub SetDisplayValues()
        If mAttr IsNot Nothing Then
            Dim ident As clsIdentifierInfo = clsAMI.GetIdentifierInfo(mAttr.Name)
            Items.Clear()

            ' ID Name
            Dim item As clsListItem
            If ident.Type = IdentifierType.Parent _
             Then item = AddItem(String.Format(LocaleTools.Properties.GlobalResources.Parent0, ident.Name)) _
             Else item = AddItem(ident.Name)

            ' There's not a lot we can do if we can't inherit the font from
            ' the owning listview. This will wait on rendering the font until
            ' the row is selected.
            If mReadOnly AndAlso item.Font IsNot Nothing Then _
             item.Font = New Font(item.Font, FontStyle.Italic)

            ' In Use
            AddItem(New clsProcessValue(mAttr.InUse))

            ' Comparison Type
            If mAttr.Dynamic _
             Then AddItem(My.Resources.clsApplicationAttributeListRow_Dynamic) _
             Else AddItem(clsAMI.GetComparisonTypeFriendlyName(mAttr.ComparisonType))

            ' Value
            AddItem(mAttr.Value)

        Else
            SetNullValues()

        End If
    End Sub

    Private Sub SetNullValues()
        Items.Clear()
        AddItem("") ' Attr name
        AddItem(New clsProcessValue(DataType.flag)) ' In Use
        AddItem("") ' Comparison Type
        AddItem("") ' Value
    End Sub

    Public Overrides Function CreateEditRow() As ctlEditableListRow

        'create readonly name edit area
        Dim nameLbl As New Label()
        Dim ident As clsIdentifierInfo = clsAMI.GetIdentifierInfo(mAttr.Name)
        If ident.Type = IdentifierType.Parent Then
            nameLbl.Text = String.Format(LocaleTools.Properties.GlobalResources.Parent0, ident.Name)
        Else
            nameLbl.Text = ident.Name
        End If
        nameLbl.Font = Items(0).Font
        nameLbl.BackColor = SystemColors.ControlLightLight

        'create 'in use' checkbox
        Dim inUseCb As New CheckBox()
        inUseCb.Checked = mAttr.InUse
        inUseCb.AutoCheck = Not mReadOnly
        AddHandler inUseCb.CheckedChanged, AddressOf HandleInUseChanged

        'create 'match type' dropdown
        Dim compCombo As New MonoComboBox()
        compCombo.Enabled = Not mReadOnly
        compCombo.DropDownStyle = ComboBoxStyle.DropDownList
        compCombo.DropDownWidth = 150

        'Add each comparison type available to the chosen wait condition
        For Each tp As ComparisonTypes In
         clsAMI.GetAllowedComparisonTypes(mAttr.Value.DataType.ToString())
            compCombo.Items.Add(New MonoComboBoxItem(
             clsAMI.GetComparisonTypeFriendlyName(tp), tp))
        Next

        Dim dynamicItem As New MonoComboBoxItem(My.Resources.clsApplicationAttributeListRow_Dynamic)
        compCombo.Items.Add(dynamicItem)

        ' Select the current value in the combo, according to the dynamic flag or
        ' inital value parameter
        If mAttr.Dynamic Then
            compCombo.SelectedItem = dynamicItem
        Else
            For Each item As MonoComboBoxItem In compCombo.Items
                If CType(item.Tag, ComparisonTypes) = mAttr.ComparisonType Then
                    compCombo.SelectedItem = item
                    Exit For
                End If
            Next
        End If
        AddHandler compCombo.SelectedIndexChanged, AddressOf HandleMatchTypeChanged

        'create 'value' edit area
        Dim val As clsProcessValue = mAttr.Value
        Dim valueCtl As Control = clsProcessValueControl.GetControl(val.DataType)
        valueCtl.Text = val.FormattedValue
        AddHandler valueCtl.TextChanged, AddressOf HandleValueChanged
        'Add Handlers for controls that do not implement TextChanged
        If valueCtl.GetType = GetType(AutomateUI.ctlProcessText) Then
            AddHandler DirectCast(valueCtl, AutomateUI.ctlProcessText).Changed, AddressOf HandleValueChanged
        End If
        If valueCtl.GetType = GetType(AutomateUI.ctlProcessFlag) Then
            AddHandler DirectCast(valueCtl, AutomateUI.ctlProcessFlag).Changed, AddressOf HandleValueChanged
        End If

        valueCtl.Enabled = Not mAttr.Dynamic
            valueCtl.BackColor = SystemColors.ControlLightLight
        Dim iValue As IProcessValue = TryCast(valueCtl, IProcessValue)
        If iValue IsNot Nothing Then
            iValue.ReadOnly = mReadOnly
        End If

        'populate the new row
        Dim row As New ctlEditableListRow()
        With row.Items
            .Add(New ctlEditableListItem(nameLbl))
            .Add(New ctlEditableListItem(inUseCb))
            .Add(New ctlEditableListItem(compCombo))
            .Add(New ctlEditableListItem(valueCtl))
        End With
        row.Tag = mAttr

        Return row
    End Function

    Public Overrides Sub EndEdit(ByVal EditRow As ctlEditableListRow)
        Me.mAttr.InUse = CType(EditRow.Items(1).NestedControl, CheckBox).Checked
        Dim c As MonoComboBox = CType(EditRow.Items(2).NestedControl, MonoComboBox)

        Me.SetAttributeMatchType(Me.mAttr, c)

        Dim processValueControl As IProcessValue = CType(EditRow.Items(3).NestedControl, IProcessValue)
        Me.mAttr.Value = processValueControl.Value

        MyBase.EndEdit(EditRow)
    End Sub

    ''' <summary>
    ''' Updates the corresponding attribute after the in use
    ''' checkbox changes value.
    ''' </summary>
    ''' <param name="sender">The Checkbox raising the checkChanged
    ''' event.</param>
    ''' <param name="e">The eventargs.</param>
    ''' <remarks></remarks>
    Private Sub HandleInUseChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim c As CheckBox = CType(sender, CheckBox)
        Dim ParentRow As ctlEditableListRow = CType(c.Parent, ctlEditableListItem).ParentRow
        Dim A As clsApplicationAttribute = CType(ParentRow.Tag, clsApplicationAttribute)

        A.InUse = c.Checked
        RaiseEvent AttributeChanged()
    End Sub

    ''' <summary>
    ''' Updates the corresponding attribute after the in use
    ''' checkbox changes value.
    ''' </summary>
    ''' <param name="sender">The Checkbox raising the checkChanged
    ''' event.</param>
    ''' <param name="e">The eventargs.</param>
    Private Sub HandleMatchTypeChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim c As MonoComboBox = CType(sender, MonoComboBox)
        Dim ParentRow As ctlEditableListRow = CType(c.Parent, ctlEditableListItem).ParentRow
        Dim attribute As clsApplicationAttribute = CType(ParentRow.Tag, clsApplicationAttribute)
        Dim ValueEditItem As ctlEditableListItem = ParentRow.Items(3)
        Dim ValueEditArea As Control = CType(ValueEditItem.NestedControl, Control)

        'Update object model
        SetAttributeMatchType(attribute, c)

        'Enable/disable according as dynamic
        ValueEditArea.Enabled = Not attribute.Dynamic
        Dim OriginalBackColour As Color = ValueEditArea.BackColor
        ValueEditArea.BackColor = OriginalBackColour

        RaiseEvent AttributeChanged()
    End Sub

    Private Sub SetAttributeMatchType(
     ByVal a As clsApplicationAttribute, ByVal c As MonoComboBox)
        If c.SelectedText <> "" Then
            If c.SelectedText = My.Resources.clsApplicationAttributeListRow_Dynamic Then
                a.Dynamic = True
            Else
                a.Dynamic = False
                a.ComparisonType = CType(c.SelectedItem.Tag, clsAMI.ComparisonTypes)
            End If
        End If
    End Sub


    ''' <summary>
    ''' Updates the corresponding attribute after the 'value'
    ''' textbox text is changed.
    ''' </summary>
    ''' <param name="sender">The ctlProcessText control
    ''' raising the event.</param>
    ''' <param name="e">The eventargs.</param>
    ''' <remarks></remarks>
    Private Sub HandleValueChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim processValueControl As IProcessValue = CType(sender, IProcessValue)
        Dim valueControl As Control = CType(sender, Control)
        Dim parentRow As ctlEditableListRow = CType(valueControl.Parent, ctlEditableListItem).ParentRow
        Dim attribute As clsApplicationAttribute = CType(parentRow.Tag, clsApplicationAttribute)

        attribute.Value = processValueControl.Value

        RaiseEvent AttributeChanged()
    End Sub

    Public Overrides Sub Draw(ByVal G As Graphics, ByVal Top As Integer)
        'Iterate through the columns, drawing each one for this row
        Dim Left As Integer = 0
        For ColumnIndex As Integer = 0 To Me.Owner.Columns.Count - 1
            Dim ItemWidth As Integer = Me.Owner.Columns(ColumnIndex).Width

            'We draw the checkboxes ourselves, use default implementation for other columns
            Select Case ColumnIndex
                Case 1
                    Dim CHState As CheckState = CheckState.Unchecked
                    Dim ItemValue As clsProcessValue = Me.Items(ColumnIndex).Value
                    If ItemValue IsNot Nothing AndAlso CBool(ItemValue) Then
                        CHState = CheckState.Checked
                    End If
                    Dim chBounds As Rectangle = GetCheckboxBoundsForColumn(ColumnIndex)
                    chBounds.Offset(0, Top)
                    AutomateControls.ControlPaintStyle.DrawCheckbox(G, chBounds, CHState)
                Case Else
                    'The font colour we use depends on whether dynamic
                    If ColumnIndex = 3 Then
                        If Me.ApplicationAttribute.Dynamic Then
                            Me.Items(3).TextBrush = clsListItem.DisabledTextBrush
                        Else
                            Me.Items(3).TextBrush = clsListItem.NormalTextBrush
                        End If
                    End If

                    Dim BoundingRectangle As New Rectangle(Left, Top, ItemWidth, ctlListView.DefaultRowHeight)
                    Me.Items(ColumnIndex).Draw(G, BoundingRectangle)
            End Select

            Left += ItemWidth
        Next

    End Sub

    ''' <summary>
    ''' Gets the bounds of the specified checkbox, in local row coordinates.
    ''' </summary>
    ''' <param name="ColumnIndex">The index of the column in which the checkbox
    ''' resides.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetCheckboxBoundsForColumn(ByVal ColumnIndex As Integer) As Rectangle
        If Me.Items.Count >= 2 Then
            Dim ItemLeft As Integer
            For i As Integer = 0 To ColumnIndex - 1
                ItemLeft += Me.Owner.Columns(i).Width
            Next
            Dim ItemWidth As Integer = Me.Owner.Columns(ColumnIndex).Width
            Dim CheckBoxSize As New Size(13, 13)
            Dim Location As New Point(ItemLeft + (ItemWidth - CheckBoxSize.Width) \ 2, (ctlListView.DefaultRowHeight - CheckBoxSize.Height) \ 2)
            Return New Rectangle(Location, CheckBoxSize)
        Else
            Return Rectangle.Empty
        End If
    End Function

    Public Overrides Sub OnMouseUp(ByVal locn As Point)
        If Not mReadOnly Then
            ' Toggle the checkbox immediately if a mouse click was detected on it.
            Const CheckBoxIndex As Integer = 1

            Dim bounds As Rectangle = GetCheckboxBoundsForColumn(CheckBoxIndex)
            If bounds.Contains(locn) AndAlso bounds.Contains(mMouseDownPoint) Then
                Dim val As clsProcessValue = Me.Items(CheckBoxIndex).Value
                If val IsNot Nothing Then
                    Items(CheckBoxIndex).Value = Not CBool(val)
                End If
            End If
        End If

        MyBase.OnMouseUp(locn)
    End Sub

#End Region


End Class
