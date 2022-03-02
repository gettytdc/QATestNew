<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlWorkQueueManagement
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlWorkQueueManagement))
        Me.mSplitter = New AutomateControls.SplitContainers.HighlightingSplitContainer()
        Me.mWorkQueueList = New AutomateUI.ctlWorkQueueList()
        Me.mWorkQueueContents = New AutomateUI.ctlWorkQueueContents()
        CType(Me.mSplitter, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.mSplitter.Panel1.SuspendLayout()
        Me.mSplitter.Panel2.SuspendLayout()
        Me.mSplitter.SuspendLayout()
        Me.SuspendLayout()
        '
        'mSplitter
        '
        Me.mSplitter.Cursor = System.Windows.Forms.Cursors.Default
        Me.mSplitter.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        resources.ApplyResources(Me.mSplitter, "mSplitter")
        Me.mSplitter.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.mSplitter.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.mSplitter.GripVisible = False
        Me.mSplitter.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.mSplitter.MouseLeaveColor = System.Drawing.Color.White
        Me.mSplitter.Name = "mSplitter"
        '
        'mSplitter.Panel1
        '
        Me.mSplitter.Panel1.Controls.Add(Me.mWorkQueueList)
        '
        'mSplitter.Panel2
        '
        Me.mSplitter.Panel2.Controls.Add(Me.mWorkQueueContents)
        Me.mSplitter.TabStop = False
        Me.mSplitter.TextColor = System.Drawing.Color.Black
        '
        'mWorkQueueList
        '
        resources.ApplyResources(Me.mWorkQueueList, "mWorkQueueList")
        Me.mWorkQueueList.Name = "mWorkQueueList"
        Me.mWorkQueueList.SelectedQueue = Nothing
        '
        'mWorkQueueContents
        '
        Me.mWorkQueueContents.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.mWorkQueueContents.Cursor = System.Windows.Forms.Cursors.Default
        resources.ApplyResources(Me.mWorkQueueContents, "mWorkQueueContents")
        Me.mWorkQueueContents.Name = "mWorkQueueContents"
        '
        'ctlWorkQueueManagement
        '
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.BackColor = System.Drawing.SystemColors.ControlLightLight
        Me.Controls.Add(Me.mSplitter)
        Me.Name = "ctlWorkQueueManagement"
        resources.ApplyResources(Me, "$this")
        Me.mSplitter.Panel1.ResumeLayout(False)
        Me.mSplitter.Panel2.ResumeLayout(False)
        CType(Me.mSplitter, System.ComponentModel.ISupportInitialize).EndInit()
        Me.mSplitter.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub
    Public WithEvents mWorkQueueList As AutomateUI.ctlWorkQueueList
    Friend WithEvents mWorkQueueContents As AutomateUI.ctlWorkQueueContents
    Friend WithEvents mSplitter As AutomateControls.SplitContainers.HighlightingSplitContainer
End Class
