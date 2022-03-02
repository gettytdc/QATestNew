Public Class frmSpyInfo
    Inherits System.Windows.Forms.Form

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'Form overrides dispose to clean up the component list.
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
    Friend WithEvents lblSelection As System.Windows.Forms.Label
    Friend WithEvents txtSelection As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents txtBounds As System.Windows.Forms.TextBox
    Friend WithEvents txtParent As System.Windows.Forms.TextBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmSpyInfo))
        Me.lblSelection = New System.Windows.Forms.Label()
        Me.txtSelection = New System.Windows.Forms.TextBox()
        Me.txtBounds = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.txtParent = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'lblSelection
        '
        resources.ApplyResources(Me.lblSelection, "lblSelection")
        Me.lblSelection.Name = "lblSelection"
        '
        'txtSelection
        '
        resources.ApplyResources(Me.txtSelection, "txtSelection")
        Me.txtSelection.Name = "txtSelection"
        '
        'txtBounds
        '
        resources.ApplyResources(Me.txtBounds, "txtBounds")
        Me.txtBounds.Name = "txtBounds"
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'txtParent
        '
        resources.ApplyResources(Me.txtParent, "txtParent")
        Me.txtParent.Name = "txtParent"
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'frmSpyInfo
        '
        resources.ApplyResources(Me, "$this")
        Me.BackColor = System.Drawing.Color.White
        Me.Controls.Add(Me.txtParent)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.txtBounds)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.txtSelection)
        Me.Controls.Add(Me.lblSelection)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "frmSpyInfo"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

End Class
