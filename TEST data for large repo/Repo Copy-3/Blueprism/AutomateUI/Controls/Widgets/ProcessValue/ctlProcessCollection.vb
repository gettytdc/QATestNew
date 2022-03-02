

Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' 
''' <summary>
''' This control lets the user edit a automate collection
''' </summary>
Friend Class ctlProcessCollection
    Inherits UserControl
    Implements IActivatableProcessValue

    ''' <summary>
    ''' Event fired when the collection backed by this control is changed by
    ''' this control.
    ''' </summary>
    Public Event Changed As EventHandler Implements IProcessValue.Changed

    ''' <summary>
    ''' Event fired when this collection control is 'activated'. In this
    ''' context, that means that the collection's link label has been clicked.
    ''' This control cannot do anything about it in and of itself, so it just
    ''' indicates activation and delegates what to do about it to any interested
    ''' parties.
    ''' Typically, this event would be subscribed to by the containing form
    ''' which would handle collection activation in whichever way it deems
    ''' appropriate.
    ''' </summary>
    ''' <param name="sender">The control which has been activated.</param>
    ''' <param name="e">The arguments defining the event.</param>
    Public Event Activated(ByVal sender As IActivatableProcessValue, ByVal e As EventArgs) _
     Implements IActivatableProcessValue.Activated

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.SuspendLayout()

        Me.Height = DefaultHeight
        Me.Width = DefaultWidth

        Me.ResumeLayout(True)
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
    Private WithEvents lnkCollection As System.Windows.Forms.LinkLabel
    Private WithEvents btnClear As AutomateControls.Buttons.StandardStyledButton

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlProcessCollection))
        Me.lnkCollection = New System.Windows.Forms.LinkLabel()
        Me.btnClear = New AutomateControls.Buttons.StandardStyledButton()
        Me.SuspendLayout()
        '
        'lnkCollection
        '
        resources.ApplyResources(Me.lnkCollection, "lnkCollection")
        Me.lnkCollection.Name = "lnkCollection"
        Me.lnkCollection.TabStop = True
        '
        'btnClear
        '
        resources.ApplyResources(Me.btnClear, "btnClear")
        Me.btnClear.Name = "btnClear"
        Me.btnClear.UseVisualStyleBackColor = True
        '
        'ctlProcessCollection
        '
        Me.Controls.Add(Me.btnClear)
        Me.Controls.Add(Me.lnkCollection)
        Me.Name = "ctlProcessCollection"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    ''' <summary>
    ''' The default width applied to this control.
    ''' </summary>
    Private Const DefaultWidth As Integer = 200

    ''' <summary>
    ''' The default height applied to this
    ''' control.
    ''' </summary>
    Private Const DefaultHeight As Integer = 24

    ''' <summary>
    ''' The value backing this control
    ''' </summary>
    Private mValue As clsProcessValue

    ''' <summary>
    ''' Gets the preferred size of this control, which is the larger of the
    ''' proposed size or the default <see cref="DefaultHeight">height</see> and 
    ''' <see cref="DefaultWidth">width</see> of this control.
    ''' </summary>
    ''' <param name="proposedSize">The proposed size for the control.</param>
    ''' <returns>The preferred size of this control.</returns>
    Public Overrides Function GetPreferredSize(ByVal proposedSize As Size) As Size
        Return New Size(Math.Max(DefaultWidth, proposedSize.Width), DefaultHeight)
    End Function

    ''' <summary>
    ''' Gets or sets the underlying process value that this collection control is
    ''' modelling.
    ''' </summary>
    ''' <value></value>
    Public Property Value() As clsProcessValue Implements IProcessValue.Value
        Get
            Return mValue
        End Get
        Set(ByVal value As clsProcessValue)
            mValue = value
            UpdateLabel()
        End Set
    End Property

    ''' <summary>
    ''' Selects this control. This simply focuses on the link providing access to
    ''' the underlying collection.
    ''' </summary>
    Public Sub SelectControl() Implements IProcessValue.SelectControl
        lnkCollection.Focus()
    End Sub

    ''' <summary>
    ''' Updates the link label to match the state of the collection.
    ''' </summary>
    Private Sub UpdateLabel()
        lnkCollection.Text = clsCollection.GetInfoLabel(Value.Collection)
    End Sub

    ''' <summary>
    ''' Handles the clear button being clicked within this control by clearing the
    ''' underlying collection.
    ''' </summary>
    Private Sub HandleClearClicked(ByVal sender As Object, ByVal e As EventArgs) Handles btnClear.Click
        mValue.Collection = Nothing
        UpdateLabel()
        RaiseEvent Changed(Me, EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' Handles the link being clicked - this just chains the event by raising an
    ''' <see cref="Activated"/> event itself. It doesn't have the context required
    ''' to do anything more with the collection.
    ''' </summary>
    Private Sub HandleLinkClicked(ByVal sender As Object, ByVal e As LinkLabelLinkClickedEventArgs) _
     Handles lnkCollection.LinkClicked
        RaiseEvent Activated(Me, EventArgs.Empty)
    End Sub

    Public Property [ReadOnly]() As Boolean Implements IProcessValue.ReadOnly
        Get
            Return True
        End Get
        Set(ByVal value As Boolean)
            'Do Nothing
        End Set
    End Property

    ''' <summary>
    ''' Commits the changes made in this control
    ''' </summary>
    Public Sub Commit() Implements IProcessValue.Commit
        ' This control is readonly, so there's no work to do here
    End Sub

End Class
