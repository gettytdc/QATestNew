<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class WorkQueueItemAttemptViewer
    Inherits frmForm

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
        Me.titleBar = New AutomateControls.TitleBar()
        Me.tableLayout = New System.Windows.Forms.TableLayoutPanel()
        Me.tableLayout.SuspendLayout()
        Me.SuspendLayout()
        '
        'titleBar
        '
        Me.titleBar.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.titleBar.Font = New System.Drawing.Font("Segoe UI", 12.0!)
        Me.titleBar.Location = New System.Drawing.Point(3, 3)
        Me.titleBar.MaximumSize = New System.Drawing.Size(0, 50)
        Me.titleBar.MinimumSize = New System.Drawing.Size(0, 50)
        Me.titleBar.Name = "titleBar"
        Me.titleBar.Size = New System.Drawing.Size(801, 50)
        Me.titleBar.TabIndex = 44
        Me.titleBar.TabStop = False
        Me.titleBar.WrapTitle = False
        '
        'tableLayout
        '
        Me.tableLayout.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tableLayout.ColumnCount = 1
        Me.tableLayout.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tableLayout.Controls.Add(Me.titleBar, 0, 0)
        Me.tableLayout.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.FixedSize
        Me.tableLayout.Location = New System.Drawing.Point(-4, -2)
        Me.tableLayout.Margin = New System.Windows.Forms.Padding(0)
        Me.tableLayout.Name = "tableLayout"
        Me.tableLayout.RowCount = 2
        Me.tableLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50.0!))
        Me.tableLayout.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100.0!))
        Me.tableLayout.Size = New System.Drawing.Size(807, 452)
        Me.tableLayout.TabIndex = 45
        '
        'WorkQueueItemAttemptViewer
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.ClientSize = New System.Drawing.Size(800, 450)
        Me.Controls.Add(Me.tableLayout)
        Me.Name = "WorkQueueItemAttemptViewer"
        Me.Text = "frmAttemptViewer"
        Me.tableLayout.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

    Friend WithEvents titleBar As AutomateControls.TitleBar
    Friend WithEvents tableLayout As TableLayoutPanel
End Class
