<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class frmTerminal
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmTerminal))
        Me.btnLaunch = New System.Windows.Forms.Button()
        Me.cmbTerminalType = New System.Windows.Forms.ComboBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.txtSessionFile = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.txtSessionID = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.lvFields = New System.Windows.Forms.ListView()
        Me.ColumnHeader1 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader2 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader3 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader4 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader5 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.ColumnHeader6 = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.Label5 = New System.Windows.Forms.Label()
        Me.btnGetText = New System.Windows.Forms.Button()
        Me.btnSetText = New System.Windows.Forms.Button()
        Me.txtFieldText = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.btnSpyNewField = New System.Windows.Forms.Button()
        Me.btnDelete = New System.Windows.Forms.Button()
        Me.btnBrowse = New System.Windows.Forms.Button()
        Me.txtSessionDLLName = New System.Windows.Forms.TextBox()
        Me.txtSessionDLLEntryPoint = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'btnLaunch
        '
        resources.ApplyResources(Me.btnLaunch, "btnLaunch")
        Me.btnLaunch.Name = "btnLaunch"
        Me.btnLaunch.UseVisualStyleBackColor = True
        '
        'cmbTerminalType
        '
        resources.ApplyResources(Me.cmbTerminalType, "cmbTerminalType")
        Me.cmbTerminalType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbTerminalType.FormattingEnabled = True
        Me.cmbTerminalType.Name = "cmbTerminalType"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'txtSessionFile
        '
        resources.ApplyResources(Me.txtSessionFile, "txtSessionFile")
        Me.txtSessionFile.Name = "txtSessionFile"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'txtSessionID
        '
        resources.ApplyResources(Me.txtSessionID, "txtSessionID")
        Me.txtSessionID.Name = "txtSessionID"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'lvFields
        '
        resources.ApplyResources(Me.lvFields, "lvFields")
        Me.lvFields.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.ColumnHeader1, Me.ColumnHeader2, Me.ColumnHeader3, Me.ColumnHeader4, Me.ColumnHeader5, Me.ColumnHeader6})
        Me.lvFields.FullRowSelect = True
        Me.lvFields.HideSelection = False
        Me.lvFields.MultiSelect = False
        Me.lvFields.Name = "lvFields"
        Me.lvFields.UseCompatibleStateImageBehavior = False
        Me.lvFields.View = System.Windows.Forms.View.Details
        '
        'ColumnHeader1
        '
        resources.ApplyResources(Me.ColumnHeader1, "ColumnHeader1")
        '
        'ColumnHeader2
        '
        resources.ApplyResources(Me.ColumnHeader2, "ColumnHeader2")
        '
        'ColumnHeader3
        '
        resources.ApplyResources(Me.ColumnHeader3, "ColumnHeader3")
        '
        'ColumnHeader4
        '
        resources.ApplyResources(Me.ColumnHeader4, "ColumnHeader4")
        '
        'ColumnHeader5
        '
        resources.ApplyResources(Me.ColumnHeader5, "ColumnHeader5")
        '
        'ColumnHeader6
        '
        resources.ApplyResources(Me.ColumnHeader6, "ColumnHeader6")
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.Name = "Label5"
        '
        'btnGetText
        '
        resources.ApplyResources(Me.btnGetText, "btnGetText")
        Me.btnGetText.Name = "btnGetText"
        Me.btnGetText.UseVisualStyleBackColor = True
        '
        'btnSetText
        '
        resources.ApplyResources(Me.btnSetText, "btnSetText")
        Me.btnSetText.Name = "btnSetText"
        Me.btnSetText.UseVisualStyleBackColor = True
        '
        'txtFieldText
        '
        resources.ApplyResources(Me.txtFieldText, "txtFieldText")
        Me.txtFieldText.Name = "txtFieldText"
        '
        'Label6
        '
        resources.ApplyResources(Me.Label6, "Label6")
        Me.Label6.Name = "Label6"
        '
        'btnSpyNewField
        '
        resources.ApplyResources(Me.btnSpyNewField, "btnSpyNewField")
        Me.btnSpyNewField.Name = "btnSpyNewField"
        Me.btnSpyNewField.UseVisualStyleBackColor = True
        '
        'btnDelete
        '
        resources.ApplyResources(Me.btnDelete, "btnDelete")
        Me.btnDelete.Name = "btnDelete"
        Me.btnDelete.UseVisualStyleBackColor = True
        '
        'btnBrowse
        '
        resources.ApplyResources(Me.btnBrowse, "btnBrowse")
        Me.btnBrowse.Name = "btnBrowse"
        Me.btnBrowse.UseVisualStyleBackColor = True
        '
        'txtSessionDLLName
        '
        resources.ApplyResources(Me.txtSessionDLLName, "txtSessionDLLName")
        Me.txtSessionDLLName.Name = "txtSessionDLLName"
        '
        'txtSessionDLLEntryPoint
        '
        resources.ApplyResources(Me.txtSessionDLLEntryPoint, "txtSessionDLLEntryPoint")
        Me.txtSessionDLLEntryPoint.Name = "txtSessionDLLEntryPoint"
        '
        'Label7
        '
        resources.ApplyResources(Me.Label7, "Label7")
        Me.Label7.Name = "Label7"
        '
        'Label8
        '
        resources.ApplyResources(Me.Label8, "Label8")
        Me.Label8.Name = "Label8"
        '
        'frmTerminal
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.txtSessionDLLEntryPoint)
        Me.Controls.Add(Me.txtSessionDLLName)
        Me.Controls.Add(Me.btnBrowse)
        Me.Controls.Add(Me.btnDelete)
        Me.Controls.Add(Me.btnSpyNewField)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.txtFieldText)
        Me.Controls.Add(Me.btnSetText)
        Me.Controls.Add(Me.btnGetText)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.lvFields)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.txtSessionID)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.txtSessionFile)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.cmbTerminalType)
        Me.Controls.Add(Me.btnLaunch)
        Me.Name = "frmTerminal"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents btnLaunch As System.Windows.Forms.Button
    Friend WithEvents cmbTerminalType As System.Windows.Forms.ComboBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtSessionFile As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents txtSessionID As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents lvFields As System.Windows.Forms.ListView
    Friend WithEvents ColumnHeader1 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader2 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader3 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader4 As System.Windows.Forms.ColumnHeader
    Friend WithEvents ColumnHeader5 As System.Windows.Forms.ColumnHeader
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents btnGetText As System.Windows.Forms.Button
    Friend WithEvents btnSetText As System.Windows.Forms.Button
    Friend WithEvents txtFieldText As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents btnSpyNewField As System.Windows.Forms.Button
    Friend WithEvents btnDelete As System.Windows.Forms.Button
    Friend WithEvents btnBrowse As System.Windows.Forms.Button
    Friend WithEvents ColumnHeader6 As System.Windows.Forms.ColumnHeader
    Friend WithEvents txtSessionDLLName As System.Windows.Forms.TextBox
    Friend WithEvents txtSessionDLLEntryPoint As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents Label8 As System.Windows.Forms.Label
End Class
