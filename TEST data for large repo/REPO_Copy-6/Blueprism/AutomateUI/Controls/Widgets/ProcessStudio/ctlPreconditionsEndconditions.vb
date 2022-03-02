
Imports BluePrism.AutomateAppCore

''' Project  : Automate
''' Class    : ctlPreconditionsEndconditions
''' 
''' <summary>
''' A control to display preconditions and postconditions.
''' </summary>
Public Class ctlPreconditionsEndconditions
    Inherits System.Windows.Forms.UserControl

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'UserControl overrides dispose to clean up the component list.
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
    Friend WithEvents lPrecond As System.Windows.Forms.Label
    Friend WithEvents lEndpoint As System.Windows.Forms.Label
    Friend WithEvents lstEndPoint As AutomateUI.clsEditableListBox
    Private WithEvents mSplitPanel As AutomateControls.FlickerFreeSplitContainer
    Friend WithEvents lstPreconditions As AutomateUI.clsEditableListBox
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlPreconditionsEndconditions))
        Me.lPrecond = New System.Windows.Forms.Label()
        Me.lEndpoint = New System.Windows.Forms.Label()
        Me.lstEndPoint = New AutomateUI.clsEditableListBox()
        Me.lstPreconditions = New AutomateUI.clsEditableListBox()
        Me.mSplitPanel = New AutomateControls.FlickerFreeSplitContainer()
        CType(Me.mSplitPanel, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.mSplitPanel.Panel1.SuspendLayout()
        Me.mSplitPanel.Panel2.SuspendLayout()
        Me.mSplitPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'lPrecond
        '
        resources.ApplyResources(Me.lPrecond, "lPrecond")
        Me.lPrecond.Name = "lPrecond"
        '
        'lEndpoint
        '
        resources.ApplyResources(Me.lEndpoint, "lEndpoint")
        Me.lEndpoint.Name = "lEndpoint"
        '
        'lstEndPoint
        '
        resources.ApplyResources(Me.lstEndPoint, "lstEndPoint")
        Me.lstEndPoint.ForeColor = System.Drawing.Color.Green
        Me.lstEndPoint.Name = "lstEndPoint"
        Me.lstEndPoint.ReadOnly = False
        '
        'lstPreconditions
        '
        resources.ApplyResources(Me.lstPreconditions, "lstPreconditions")
        Me.lstPreconditions.ForeColor = System.Drawing.Color.Green
        Me.lstPreconditions.Name = "lstPreconditions"
        Me.lstPreconditions.ReadOnly = False
        '
        'mSplitPanel
        '
        resources.ApplyResources(Me.mSplitPanel, "mSplitPanel")
        Me.mSplitPanel.Name = "mSplitPanel"
        '
        'mSplitPanel.Panel1
        '
        Me.mSplitPanel.Panel1.Controls.Add(Me.lPrecond)
        Me.mSplitPanel.Panel1.Controls.Add(Me.lstPreconditions)
        '
        'mSplitPanel.Panel2
        '
        Me.mSplitPanel.Panel2.Controls.Add(Me.lEndpoint)
        Me.mSplitPanel.Panel2.Controls.Add(Me.lstEndPoint)
        '
        'ctlPreconditionsEndconditions
        '
        Me.Controls.Add(Me.mSplitPanel)
        Me.Name = "ctlPreconditionsEndconditions"
        resources.ApplyResources(Me, "$this")
        Me.mSplitPanel.Panel1.ResumeLayout(False)
        Me.mSplitPanel.Panel2.ResumeLayout(False)
        CType(Me.mSplitPanel, System.ComponentModel.ISupportInitialize).EndInit()
        Me.mSplitPanel.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the readonly s
    ''' </summary>
    ''' <value></value>
    Public Property [ReadOnly]() As Boolean
        Get
            Return lstPreconditions.ReadOnly AndAlso lstEndPoint.ReadOnly
        End Get
        Set(ByVal value As Boolean)
            lstPreconditions.ReadOnly = value
            lstEndPoint.ReadOnly = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a collection of postconditions in this control.
    ''' </summary>
    ''' <remarks>Note that this collection is 'disconnected' - ie. the collection
    ''' itself is not retained by this control, only its contents. Equally, when
    ''' getting the preconditions, the collection created by this control is not
    ''' retained, so it can be modified safely by other classes without affecting
    ''' this control.
    ''' </remarks>
    Public Property PostConditions() As ICollection(Of String)
        Get
            Return GetConditions(lstEndPoint)
        End Get
        Set(ByVal value As ICollection(Of String))
            SetConditions(lstEndPoint, value)
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets a collection of preconditions in this control.
    ''' </summary>
    ''' <remarks>Note that this collection is 'disconnected' - ie. the collection
    ''' itself is not retained by this control, only its contents. Equally, when
    ''' getting the preconditions, the collection created by this control is not
    ''' retained, so it can be modified safely by other classes without affecting
    ''' this control.
    ''' </remarks>
    Public Property PreConditions() As ICollection(Of String)
        Get
            Return GetConditions(lstPreconditions)
        End Get
        Set(ByVal value As ICollection(Of String))
            SetConditions(lstPreconditions, value)
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Gets the conditions from a listbox, ignoring any empty entries
    ''' </summary>
    ''' <param name="box">The box from which to draw the conditions.</param>
    ''' <returns>A collection of conditions found in the listbox.</returns>
    Private Function GetConditions(ByVal box As ListBox) As ICollection(Of String)
        Dim lst As New List(Of String)
        For Each s As String In box.Items
            If s <> "" Then lst.Add(s)
        Next
        Return lst
    End Function

    ''' <summary>
    ''' Sets conditions into a given list box to values in a collection, ignoring
    ''' any empty values in the collection.
    ''' </summary>
    ''' <param name="box">The list box in which to set the conditions</param>
    ''' <param name="conds">The conditions to set in the list box</param>
    Private Sub SetConditions(
     ByVal box As ListBox, ByVal conds As ICollection(Of String))
        box.Items.Clear()
        box.BeginUpdate()
        Try
            For Each cond As String In conds
                If cond <> "" Then
                    box.Items.Add(cond)
                    box.HorizontalScrollbar = clsUtility.MeasureString(box.Font, cond) > box.Width
                End If
            Next
        Finally
            box.EndUpdate()
        End Try
    End Sub

#End Region

End Class
