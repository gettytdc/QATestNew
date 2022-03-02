<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class WebApiCommonCodePanel
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(WebApiCommonCodePanel))
        Me.TabControl1 = New System.Windows.Forms.TabControl()
        Me.TabPage1 = New System.Windows.Forms.TabPage()
        Me.TableLayoutPanel2 = New System.Windows.Forms.TableLayoutPanel()
        Me.btnCheckCode = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.ctlEditor = New AutomateUI.ctlCodeEditor()
        Me.TabPage2 = New System.Windows.Forms.TabPage()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.lstDlls = New AutomateUI.ctlListView()
        Me.lLanguage = New System.Windows.Forms.Label()
        Me.btnBrowse = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.cmbLanguage = New System.Windows.Forms.ComboBox()
        Me.btnAddReference = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnRemoveNamespace = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.lstNamespaces = New AutomateUI.ctlListView()
        Me.btnAddNamespace = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnRemoveRef = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.TabControl1.SuspendLayout
        Me.TabPage1.SuspendLayout
        Me.TableLayoutPanel2.SuspendLayout
        Me.TabPage2.SuspendLayout
        Me.Panel1.SuspendLayout
        Me.SuspendLayout
        '
        'TabControl1
        '
        Me.TabControl1.Controls.Add(Me.TabPage1)
        Me.TabControl1.Controls.Add(Me.TabPage2)
        resources.ApplyResources(Me.TabControl1, "TabControl1")
        Me.TabControl1.Name = "TabControl1"
        Me.TabControl1.SelectedIndex = 0
        '
        'TabPage1
        '
        Me.TabPage1.Controls.Add(Me.TableLayoutPanel2)
        resources.ApplyResources(Me.TabPage1, "TabPage1")
        Me.TabPage1.Name = "TabPage1"
        Me.TabPage1.UseVisualStyleBackColor = true
        '
        'TableLayoutPanel2
        '
        resources.ApplyResources(Me.TableLayoutPanel2, "TableLayoutPanel2")
        Me.TableLayoutPanel2.Controls.Add(Me.btnCheckCode, 0, 1)
        Me.TableLayoutPanel2.Controls.Add(Me.ctlEditor, 0, 0)
        Me.TableLayoutPanel2.Name = "TableLayoutPanel2"
        '
        'btnCheckCode
        '
        resources.ApplyResources(Me.btnCheckCode, "btnCheckCode")
        Me.btnCheckCode.BackColor = System.Drawing.Color.White
        Me.btnCheckCode.Name = "btnCheckCode"
        Me.btnCheckCode.UseVisualStyleBackColor = false
        '
        'ctlEditor
        '
        Me.ctlEditor.BackgroundColour = System.Drawing.SystemColors.Window
        Me.ctlEditor.Code = ""
        resources.ApplyResources(Me.ctlEditor, "ctlEditor")
        Me.ctlEditor.Name = "ctlEditor"
        Me.ctlEditor.ReadOnly = false
        '
        'TabPage2
        '
        Me.TabPage2.Controls.Add(Me.Panel1)
        resources.ApplyResources(Me.TabPage2, "TabPage2")
        Me.TabPage2.Name = "TabPage2"
        Me.TabPage2.UseVisualStyleBackColor = true
        '
        'Panel1
        '
        Me.Panel1.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.Panel1.Controls.Add(Me.lstDlls)
        Me.Panel1.Controls.Add(Me.lLanguage)
        Me.Panel1.Controls.Add(Me.btnBrowse)
        Me.Panel1.Controls.Add(Me.cmbLanguage)
        Me.Panel1.Controls.Add(Me.btnAddReference)
        Me.Panel1.Controls.Add(Me.btnRemoveNamespace)
        Me.Panel1.Controls.Add(Me.lstNamespaces)
        Me.Panel1.Controls.Add(Me.btnAddNamespace)
        Me.Panel1.Controls.Add(Me.btnRemoveRef)
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Name = "Panel1"
        '
        'lstDlls
        '
        Me.lstDlls.AllowDrop = true
        resources.ApplyResources(Me.lstDlls, "lstDlls")
        Me.lstDlls.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.lstDlls.CurrentEditableRow = Nothing
        Me.lstDlls.FillColumn = Nothing
        Me.lstDlls.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lstDlls.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.lstDlls.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182,Byte),Integer), CType(CType(202,Byte),Integer), CType(CType(234,Byte),Integer))
        Me.lstDlls.HighlightedRowOutline = System.Drawing.SystemColors.Highlight
        Me.lstDlls.LastColumnAutoSize = true
        Me.lstDlls.MinimumColumnWidth = 200
        Me.lstDlls.Name = "lstDlls"
        Me.lstDlls.Readonly = false
        Me.lstDlls.RowHeight = 26
        Me.lstDlls.Rows.Capacity = 0
        Me.lstDlls.Sortable = false
        '
        'lLanguage
        '
        resources.ApplyResources(Me.lLanguage, "lLanguage")
        Me.lLanguage.Name = "lLanguage"
        '
        'btnBrowse
        '
        resources.ApplyResources(Me.btnBrowse, "btnBrowse")
        Me.btnBrowse.BackColor = System.Drawing.Color.White
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.UseVisualStyleBackColor = false
        '
        'cmbLanguage
        '
        resources.ApplyResources(Me.cmbLanguage, "cmbLanguage")
        Me.cmbLanguage.DisplayMember = "FriendlyName"
        Me.cmbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbLanguage.Name = "cmbLanguage"
        '
        'btnAddReference
        '
        resources.ApplyResources(Me.btnAddReference, "btnAddReference")
        Me.btnAddReference.BackColor = System.Drawing.Color.White
        Me.btnAddReference.Name = "btnAddReference"
        Me.btnAddReference.UseVisualStyleBackColor = false
        '
        'btnRemoveNamespace
        '
        resources.ApplyResources(Me.btnRemoveNamespace, "btnRemoveNamespace")
        Me.btnRemoveNamespace.BackColor = System.Drawing.Color.White
        Me.btnRemoveNamespace.Name = "btnRemoveNamespace"
        Me.btnRemoveNamespace.UseVisualStyleBackColor = false
        '
        'lstNamespaces
        '
        Me.lstNamespaces.AllowDrop = true
        resources.ApplyResources(Me.lstNamespaces, "lstNamespaces")
        Me.lstNamespaces.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.lstNamespaces.CurrentEditableRow = Nothing
        Me.lstNamespaces.FillColumn = Nothing
        Me.lstNamespaces.FrameBorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.lstNamespaces.HighlightedForeColor = System.Drawing.SystemColors.HighlightText
        Me.lstNamespaces.HighlightedRowBackColour = System.Drawing.Color.FromArgb(CType(CType(182,Byte),Integer), CType(CType(202,Byte),Integer), CType(CType(234,Byte),Integer))
        Me.lstNamespaces.HighlightedRowOutline = System.Drawing.SystemColors.Highlight
        Me.lstNamespaces.LastColumnAutoSize = true
        Me.lstNamespaces.MinimumColumnWidth = 200
        Me.lstNamespaces.Name = "lstNamespaces"
        Me.lstNamespaces.Readonly = false
        Me.lstNamespaces.RowHeight = 26
        Me.lstNamespaces.Rows.Capacity = 0
        Me.lstNamespaces.Sortable = false
        '
        'btnAddNamespace
        '
        resources.ApplyResources(Me.btnAddNamespace, "btnAddNamespace")
        Me.btnAddNamespace.BackColor = System.Drawing.Color.White
        Me.btnAddNamespace.Name = "btnAddNamespace"
        Me.btnAddNamespace.UseVisualStyleBackColor = false
        '
        'btnRemoveRef
        '
        resources.ApplyResources(Me.btnRemoveRef, "btnRemoveRef")
        Me.btnRemoveRef.BackColor = System.Drawing.Color.White
        Me.btnRemoveRef.Name = "btnRemoveRef"
        Me.btnRemoveRef.UseVisualStyleBackColor = false
        '
        'WebApiCommonCodePanel
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.TabControl1)
        Me.Name = "WebApiCommonCodePanel"
        resources.ApplyResources(Me, "$this")
        Me.TabControl1.ResumeLayout(false)
        Me.TabPage1.ResumeLayout(false)
        Me.TableLayoutPanel2.ResumeLayout(false)
        Me.TableLayoutPanel2.PerformLayout
        Me.TabPage2.ResumeLayout(false)
        Me.Panel1.ResumeLayout(false)
        Me.Panel1.PerformLayout
        Me.ResumeLayout(false)

End Sub

    Friend WithEvents TabControl1 As TabControl
    Friend WithEvents TabPage1 As TabPage
    Friend WithEvents TableLayoutPanel2 As TableLayoutPanel
    Friend WithEvents btnCheckCode As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents ctlEditor As ctlCodeEditor
    Friend WithEvents TabPage2 As TabPage
    Friend WithEvents Panel1 As Panel
    Friend WithEvents lstDlls As ctlListView
    Friend WithEvents lLanguage As Label
    Friend WithEvents btnBrowse As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents cmbLanguage As ComboBox
    Friend WithEvents btnAddReference As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnRemoveNamespace As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents lstNamespaces As ctlListView
    Friend WithEvents btnAddNamespace As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnRemoveRef As AutomateControls.Buttons.StandardStyledButton
End Class
