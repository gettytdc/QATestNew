Imports DataPipelineOutputConfigUISettings = BluePrism.DataPipeline.UI.DataPipelineOutputConfigUISettings

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class ctlFileOutputOptions
    Inherits UserControl

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
        Me.lblPath = New System.Windows.Forms.Label()
        Me.txtPath = New AutomateControls.Textboxes.StyledTextBox()
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.SuspendLayout()
        '
        'lblPath
        '
        Me.lblPath.AutoSize = True
        Me.lblPath.Location = New System.Drawing.Point(7, 8)
        Me.lblPath.Name = "lblPath"
        Me.lblPath.Size = New System.Drawing.Size(30, 13)
        Me.lblPath.TabIndex = 1
        Me.lblPath.Text = My.Resources.ctlFileOutputOptions_Path
        '
        'txtPath
        '
        Me.txtPath.Location = New System.Drawing.Point(10, 29)
        Me.txtPath.Name = "txtPath"
        Me.txtPath.Size = New System.Drawing.Size(480, 22)
        Me.txtPath.TabIndex = 2
        '
        'TableLayoutPanel1
        '
        Me.TableLayoutPanel1.Location = New System.Drawing.Point(0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        Me.TableLayoutPanel1.Size = New System.Drawing.Size(200, 100)
        Me.TableLayoutPanel1.TabIndex = 0
        '
        'ctlFileOutputOptions
        '
        Me.Font = DataPipelineOutputConfigUISettings.StandardFont
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.txtPath)
        Me.Controls.Add(Me.lblPath)
        Me.Name = "ctlFileOutputOptions"
        Me.Size = New System.Drawing.Size(733, 62)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblPath As Label
    Friend WithEvents txtPath As AutomateControls.Textboxes.StyledTextBox
    Friend WithEvents TableLayoutPanel1 As TableLayoutPanel
End Class
