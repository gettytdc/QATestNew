<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class ctlWorkQueueList
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlWorkQueueList))
        Me.lblQueues = New System.Windows.Forms.Label()
        Me.lstQueues = New AutomateControls.FlickerFreeListView()
        Me.colName = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colStatus = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colWorked = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colPending = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colReferred = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colTotal = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colAverageWorktime = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.colTotalWorktime = CType(New System.Windows.Forms.ColumnHeader(), System.Windows.Forms.ColumnHeader)
        Me.TableLayoutPanel1 = New System.Windows.Forms.TableLayoutPanel()
        Me.TableLayoutPanel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblQueues
        '
        resources.ApplyResources(Me.lblQueues, "lblQueues")
        Me.lblQueues.ForeColor = System.Drawing.Color.Black
        Me.lblQueues.Name = "lblQueues"
        '
        'lstQueues
        '
        Me.lstQueues.Columns.AddRange(New System.Windows.Forms.ColumnHeader() {Me.colName, Me.colStatus, Me.colWorked, Me.colPending, Me.colReferred, Me.colTotal, Me.colAverageWorktime, Me.colTotalWorktime})
        resources.ApplyResources(Me.lstQueues, "lstQueues")
        Me.lstQueues.FullRowSelect = True
        Me.lstQueues.GridLines = True
        Me.lstQueues.HideSelection = False
        Me.lstQueues.Name = "lstQueues"
        Me.lstQueues.UseCompatibleStateImageBehavior = False
        Me.lstQueues.View = System.Windows.Forms.View.Details
        '
        'colName
        '
        resources.ApplyResources(Me.colName, "colName")
        '
        'colStatus
        '
        resources.ApplyResources(Me.colStatus, "colStatus")
        '
        'colWorked
        '
        resources.ApplyResources(Me.colWorked, "colWorked")
        '
        'colPending
        '
        resources.ApplyResources(Me.colPending, "colPending")
        '
        'colReferred
        '
        resources.ApplyResources(Me.colReferred, "colReferred")
        '
        'colTotal
        '
        resources.ApplyResources(Me.colTotal, "colTotal")
        '
        'colAverageWorktime
        '
        resources.ApplyResources(Me.colAverageWorktime, "colAverageWorktime")
        '
        'colTotalWorktime
        '
        resources.ApplyResources(Me.colTotalWorktime, "colTotalWorktime")
        '
        'TableLayoutPanel1
        '
        resources.ApplyResources(Me.TableLayoutPanel1, "TableLayoutPanel1")
        Me.TableLayoutPanel1.Controls.Add(Me.lstQueues, 0, 1)
        Me.TableLayoutPanel1.Controls.Add(Me.lblQueues, 0, 0)
        Me.TableLayoutPanel1.Name = "TableLayoutPanel1"
        '
        'ctlWorkQueueList
        '
        resources.ApplyResources(Me, "$this")
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
        Me.Controls.Add(Me.TableLayoutPanel1)
        Me.Name = "ctlWorkQueueList"
        Me.TableLayoutPanel1.ResumeLayout(False)
        Me.TableLayoutPanel1.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents lblQueues As System.Windows.Forms.Label
    Friend WithEvents TableLayoutPanel1 As System.Windows.Forms.TableLayoutPanel
    Private WithEvents lstQueues As AutomateControls.FlickerFreeListView
    Private WithEvents colName As System.Windows.Forms.ColumnHeader
    Private WithEvents colStatus As System.Windows.Forms.ColumnHeader
    Private WithEvents colWorked As System.Windows.Forms.ColumnHeader
    Private WithEvents colPending As System.Windows.Forms.ColumnHeader
    Private WithEvents colReferred As System.Windows.Forms.ColumnHeader
    Private WithEvents colTotal As System.Windows.Forms.ColumnHeader
    Private WithEvents colAverageWorktime As System.Windows.Forms.ColumnHeader
    Private WithEvents colTotalWorktime As System.Windows.Forms.ColumnHeader

End Class
