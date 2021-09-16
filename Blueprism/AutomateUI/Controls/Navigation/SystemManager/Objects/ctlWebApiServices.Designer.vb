<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlWebApiServices
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlWebApiServices))
        Me.llFindReferences = New AutomateControls.BulletedLinkLabel()
        Me.llAdd = New AutomateControls.BulletedLinkLabel()
        Me.llDelete = New AutomateControls.BulletedLinkLabel()
        Me.llEdit = New AutomateControls.BulletedLinkLabel()
        Me.dataGridViewWebApi = New AutomateControls.DataGridViews.RowBasedDataGridView()
        CType(Me.dataGridViewWebApi, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'llFindReferences
        '
        resources.ApplyResources(Me.llFindReferences, "llFindReferences")
        Me.llFindReferences.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llFindReferences.Name = "llFindReferences"
        Me.llFindReferences.TabStop = True
        Me.llFindReferences.UseCompatibleTextRendering = True
        '
        'llAdd
        '
        resources.ApplyResources(Me.llAdd, "llAdd")
        Me.llAdd.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llAdd.Name = "llAdd"
        Me.llAdd.TabStop = True
        Me.llAdd.UseCompatibleTextRendering = True
        '
        'llDelete
        '
        resources.ApplyResources(Me.llDelete, "llDelete")
        Me.llDelete.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llDelete.Name = "llDelete"
        Me.llDelete.TabStop = True
        Me.llDelete.UseCompatibleTextRendering = True
        '
        'llEdit
        '
        resources.ApplyResources(Me.llEdit, "llEdit")
        Me.llEdit.LinkColor = System.Drawing.Color.FromArgb(CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer), CType(CType(64, Byte), Integer))
        Me.llEdit.Name = "llEdit"
        Me.llEdit.TabStop = True
        Me.llEdit.UseCompatibleTextRendering = True
        '
        'dataGridViewWebApi
        '
        resources.ApplyResources(Me.dataGridViewWebApi, "dataGridViewWebApi")
        Me.dataGridViewWebApi.Name = "dataGridViewWebApi"
        Me.dataGridViewWebApi.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize
        '
        'ctlWebApiServices
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.dataGridViewWebApi)
        Me.Controls.Add(Me.llFindReferences)
        Me.Controls.Add(Me.llAdd)
        Me.Controls.Add(Me.llDelete)
        Me.Controls.Add(Me.llEdit)
        Me.Name = "ctlWebApiServices"
        resources.ApplyResources(Me, "$this")
        CType(Me.dataGridViewWebApi, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents llFindReferences As AutomateControls.BulletedLinkLabel
    Friend WithEvents llAdd As AutomateControls.BulletedLinkLabel
    Friend WithEvents llDelete As AutomateControls.BulletedLinkLabel
    Friend WithEvents llEdit As AutomateControls.BulletedLinkLabel
    Friend WithEvents dataGridViewWebApi As AutomateControls.DataGridViews.RowBasedDataGridView
End Class
