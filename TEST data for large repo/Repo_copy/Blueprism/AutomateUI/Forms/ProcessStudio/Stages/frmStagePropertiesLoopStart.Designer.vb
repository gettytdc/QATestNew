Partial Class frmStagePropertiesLoopStart
    Inherits frmProperties

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
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesLoopStart))
        Me.Label4 = New System.Windows.Forms.Label()
        Me.cmbCollections = New AutomateControls.StyledComboBox()
        Me.SuspendLayout()
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        '
        'txtDescription
        '
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        '
        'mTitleBar
        '
        resources.ApplyResources(Me.mTitleBar, "mTitleBar")
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'cmbCollections
        '
        resources.ApplyResources(Me.cmbCollections, "cmbCollections")
        Me.cmbCollections.DropDownWidth = 400
        Me.cmbCollections.FormattingEnabled = True
        Me.cmbCollections.Name = "cmbCollections"
        '
        'frmStagePropertiesLoopStart
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.cmbCollections)
        Me.Controls.Add(Me.Label4)
        Me.Name = "frmStagePropertiesLoopStart"
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.Controls.SetChildIndex(Me.Label4, 0)
        Me.Controls.SetChildIndex(Me.cmbCollections, 0)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents cmbCollections As AutomateControls.StyledComboBox

End Class