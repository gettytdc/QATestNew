<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlBreadcrumbTrail
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
        Dim LinkLabel1 As System.Windows.Forms.LinkLabel
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlBreadcrumbTrail))
        Dim LinkLabel2 As System.Windows.Forms.LinkLabel
        Dim LinkLabel3 As System.Windows.Forms.LinkLabel
        Dim LinkLabel4 As System.Windows.Forms.LinkLabel
        Dim Label1 As System.Windows.Forms.Label
        Me.flowPanel = New System.Windows.Forms.FlowLayoutPanel()
        LinkLabel1 = New System.Windows.Forms.LinkLabel()
        LinkLabel2 = New System.Windows.Forms.LinkLabel()
        LinkLabel3 = New System.Windows.Forms.LinkLabel()
        LinkLabel4 = New System.Windows.Forms.LinkLabel()
        Label1 = New System.Windows.Forms.Label()
        Me.flowPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'LinkLabel1
        '
        resources.ApplyResources(LinkLabel1, "LinkLabel1")
        LinkLabel1.Name = "LinkLabel1"
        LinkLabel1.TabStop = True
        '
        'LinkLabel2
        '
        resources.ApplyResources(LinkLabel2, "LinkLabel2")
        LinkLabel2.Name = "LinkLabel2"
        LinkLabel2.TabStop = True
        '
        'LinkLabel3
        '
        resources.ApplyResources(LinkLabel3, "LinkLabel3")
        LinkLabel3.Name = "LinkLabel3"
        LinkLabel3.TabStop = True
        '
        'LinkLabel4
        '
        resources.ApplyResources(LinkLabel4, "LinkLabel4")
        LinkLabel4.Name = "LinkLabel4"
        LinkLabel4.TabStop = True
        '
        'Label1
        '
        resources.ApplyResources(Label1, "Label1")
        Label1.Name = "Label1"
        '
        'flowPanel
        '
        Me.flowPanel.Controls.Add(LinkLabel1)
        Me.flowPanel.Controls.Add(LinkLabel2)
        Me.flowPanel.Controls.Add(LinkLabel3)
        Me.flowPanel.Controls.Add(LinkLabel4)
        Me.flowPanel.Controls.Add(Label1)
        resources.ApplyResources(Me.flowPanel, "flowPanel")
        Me.flowPanel.Name = "flowPanel"
        '
        'ctlBreadcrumbTrail
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.flowPanel)
        Me.Name = "ctlBreadcrumbTrail"
        Me.flowPanel.ResumeLayout(False)
        Me.flowPanel.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Private WithEvents flowPanel As System.Windows.Forms.FlowLayoutPanel

End Class
