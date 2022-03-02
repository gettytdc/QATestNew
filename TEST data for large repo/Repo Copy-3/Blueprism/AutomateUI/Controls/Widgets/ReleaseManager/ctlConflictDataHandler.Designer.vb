<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlConflictDataHandler
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
        Me.mContents = New System.Windows.Forms.TableLayoutPanel()
        Me.SuspendLayout()
        '
        'mContents
        '
        Me.mContents.AutoSize = True
        Me.mContents.ColumnCount = 1
        Me.mContents.ColumnStyles.Add(New System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.mContents.Dock = System.Windows.Forms.DockStyle.Top
        Me.mContents.Location = New System.Drawing.Point(0, 0)
        Me.mContents.Name = "mContents"
        Me.mContents.RowCount = 1
        Me.mContents.RowStyles.Add(New System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50.0!))
        Me.mContents.Size = New System.Drawing.Size(455, 0)
        Me.mContents.TabIndex = 0
        '
        'ctlConflictDataHandler
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.mContents)
        Me.Name = "ctlConflictDataHandler"
        Me.Size = New System.Drawing.Size(455, 252)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

    Friend WithEvents mContents As TableLayoutPanel
End Class
