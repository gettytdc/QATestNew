Imports BluePrism.Core.Expressions
Imports BluePrism.ApplicationManager.AMI
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AMI
Imports ComparisonType = BluePrism.AMI.clsAMI.ComparisonTypes
Imports AutomateControls.ComboBoxes

''' <summary>
''' Editable list row which represents an application element parameter
''' </summary>
Friend Class ctlApplicationElementParameterListRow : Inherits ctlEditableListRow

#Region " Member Variables "

    ' The field used for displaying the name.
    Private mNameField As Label

    ' The field used for displaying the data type.
    Private mDataTypeField As Label

    ' The field used for displaying the comparison type setting.
    Private mComparisonTypeField As MonoComboBox

    ' The field used for displaying the expression.
    Private mExpressionField As ctlExpressionEdit

    ' The name of the parameter
    Private mParamName As String

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new app element parameter listrow
    ''' </summary>
    Public Sub New()
        mNameField = New Label
        mNameField.BackColor = SystemColors.ControlLightLight

        mDataTypeField = New Label
        mDataTypeField.BackColor = SystemColors.ControlLightLight

        mComparisonTypeField = New AutomateControls.ComboBoxes.MonoComboBox
        mComparisonTypeField.DropDownStyle = ComboBoxStyle.DropDownList

        mExpressionField = New ctlExpressionEdit
        mExpressionField.Border = False
        mExpressionField.BackColor = SystemColors.ControlLightLight

        Me.Items.Add(New ctlEditableListItem(mNameField))
        Me.Items.Add(New ctlEditableListItem(mDataTypeField))
        Me.Items.Add(New ctlEditableListItem(mComparisonTypeField))
        Me.Items.Add(New ctlEditableListItem(mExpressionField))
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the latest element parameter as represented in this UI,
    ''' taking into account all edits from the user.
    ''' </summary>
    Public ReadOnly Property ElementParameter() As clsApplicationElementParameter
        Get
            Dim param As New clsApplicationElementParameter()
            param.Name = mParamName
            param.DataType = CType(mDataTypeField.Tag, DataType)
            param.ComparisonType = _
             CType(mComparisonTypeField.SelectedItem.Tag, ComparisonType)
            param.Expression = BPExpression.FromLocalised(mExpressionField.Text)

            Return param
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Populates the comparison types drop down with the 
    ''' types available to the supplied data type.
    ''' </summary>
    ''' <param name="dtype">The data type for which the available
    ''' comparison types should be shown.</param>
    ''' <param name="initVal">The initial value to be displayed
    ''' in the control. This will be ignored if it is not valid for the
    ''' specified data type.</param>
    Private Sub PopulateComparisonTypesDropDown( _
     ByVal dtype As DataType, _
     ByVal initVal As clsAMI.ComparisonTypes)
        Me.mComparisonTypeField.Items.Clear()

        'Add each comparison type available to the chosen wait condition
        For Each tp As ComparisonType In _
         clsAMI.GetAllowedComparisonTypes(dtype.ToString())
            mComparisonTypeField.Items.Add(New MonoComboBoxItem( _
             clsAMI.GetComparisonTypeFriendlyName(tp), tp))
        Next

        'Select the current value in the combo, according to the init value parameter
        For Each item As MonoComboBoxItem In mComparisonTypeField.Items
            If CType(item.Tag, clsAMI.ComparisonTypes) = initVal Then
                mComparisonTypeField.SelectedItem = item
                Exit For
            End If
        Next
    End Sub

    ''' <summary>
    ''' Populates the row with the details of the supplied 
    ''' Element Parameter.
    ''' </summary>
    ''' <param name="param">The Element parameter whose details are to be populated.
    ''' </param>
    ''' <param name="stg">The stage owning this parameter. Used as the scope stage.
    ''' </param>
    Public Sub Populate( _
     ByVal param As clsApplicationElementParameter, ByVal stg As clsProcessStage)
        Dim ident As clsIdentifierInfo = clsAMI.GetIdentifierInfo(param.Name)
        If ident.Type = IdentifierType.Parent Then
            mNameField.Text = String.Format(My.Resources.ctlApplicationElementParameterListRow_Parent0, ident.Name)
        Else
            mNameField.Text = ident.Name
        End If
        mParamName = param.Name
        mDataTypeField.Text = clsProcessDataTypes.GetFriendlyName(param.DataType)
        mDataTypeField.Tag = param.DataType
        mDataTypeField.ForeColor = _
         DataItemColour.GetDataItemColor(param.DataType)
        PopulateComparisonTypesDropDown(param.DataType, param.ComparisonType)

        mExpressionField.Text = param.Expression.LocalForm
        mExpressionField.Stage = stg
    End Sub

#End Region

End Class
