Partial Friend Class ctlParameterListRow



    'UserControl overrides dispose to clean up the component list.
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
    Friend WithEvents txtName As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents txtDescription As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents mobjExpressionEdit As ctlExpressionEdit
    Friend WithEvents cmbDatatype As clsDataTypesComboBox
    Friend WithEvents txtDatatype As AutomateControls.Textboxes.StyledTextBox
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlParameterListRow))
        Me.txtName = New AutomateControls.Textboxes.StyledTextBox()
        Me.txtDescription = New AutomateControls.Textboxes.StyledTextBox()
        Me.cmbDatatype = New AutomateUI.clsDataTypesComboBox()
        Me.mobjExpressionEdit = New AutomateUI.ctlExpressionEdit()
        Me.txtDatatype = New AutomateControls.Textboxes.StyledTextBox()
        Me.mobjStoreInEdit = New AutomateUI.ctlStoreInEdit()
        Me.SuspendLayout()
        '
        'txtName
        '
        Me.txtName.BackColor = System.Drawing.Color.White
        Me.txtName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        resources.ApplyResources(Me.txtName, "txtName")
        Me.txtName.Name = "txtName"
        Me.txtName.ReadOnly = True
        '
        'txtDescription
        '
        Me.txtDescription.BackColor = System.Drawing.Color.White
        Me.txtDescription.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        Me.txtDescription.Name = "txtDescription"
        '
        'cmbDatatype
        '
        Me.cmbDatatype.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbDatatype.ChosenDataType = BluePrism.AutomateProcessCore.DataType.unknown
        Me.cmbDatatype.ChosenValue = BluePrism.AutomateProcessCore.DataType.unknown
        Me.cmbDatatype.DisabledItemColour = System.Drawing.Color.LightGray
        resources.ApplyResources(Me.cmbDatatype, "cmbDatatype")
        Me.cmbDatatype.DropDownBackColor = System.Drawing.SystemColors.ControlLightLight
        Me.cmbDatatype.Name = "cmbDatatype"
        '
        'mobjExpressionEdit
        '
        Me.mobjExpressionEdit.AllowDrop = True
        resources.ApplyResources(Me.mobjExpressionEdit, "mobjExpressionEdit")
        Me.mobjExpressionEdit.Border = False
        Me.mobjExpressionEdit.HighlightingEnabled = True
        Me.mobjExpressionEdit.IsDecision = False
        Me.mobjExpressionEdit.Name = "mobjExpressionEdit"
        Me.mobjExpressionEdit.PasswordChar = ChrW(0)
        Me.mobjExpressionEdit.Stage = Nothing
        '
        'txtDatatype
        '
        Me.txtDatatype.BackColor = System.Drawing.Color.White
        Me.txtDatatype.BorderStyle = System.Windows.Forms.BorderStyle.None
        resources.ApplyResources(Me.txtDatatype, "txtDatatype")
        Me.txtDatatype.Name = "txtDatatype"
        Me.txtDatatype.ReadOnly = True
        '
        'mobjStoreInEdit
        '
        Me.mobjStoreInEdit.AllowDrop = True
        Me.mobjStoreInEdit.AutoCreateDefault = Nothing
        Me.mobjStoreInEdit.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me.mobjStoreInEdit, "mobjStoreInEdit")
        Me.mobjStoreInEdit.Name = "mobjStoreInEdit"
        Me.mobjStoreInEdit.PasswordChar = ChrW(0)
        '
        'ctlParameterListRow
        '
        Me.BackColor = System.Drawing.Color.White
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents mobjStoreInEdit As AutomateUI.ctlStoreInEdit


End Class
