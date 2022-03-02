Imports BluePrism.BPCoreLib.Collections

''' Project  : Automate
''' Class    : ctlListItem
''' 
''' <summary>
''' Class that represents a list item.
''' </summary>
Friend Class ctlEditableListItem : Inherits UserControl

#Region " Class scope definitions "

    ''' <summary>
    ''' Event raised when the user selects the nested control inside a list item.
    ''' </summary>
    ''' <param name="sender">The list item containing the selected control.</param>
    Public Event Selected(ByVal sender As ctlEditableListItem)

    ''' <summary>
    ''' Event raised when the user presses a key in the nested control inside this
    ''' item.
    ''' </summary>
    ''' <param name="sender">The list item containing the affected control.</param>
    Public Event ListItemKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs)

    ''' <summary>
    ''' The margin between the edge of this control and the edge of the nested
    ''' control
    ''' </summary>
    Private Shared ReadOnly InnerMargin As New Padding(2)

    ''' <summary>
    ''' The types of control for which we ignore keydown events - allowing the
    ''' controls themselves to deal with them.
    ''' </summary>
    Private Shared IgnoreKeyDownTypes As ICollection(Of Type) = _
     GetReadOnly.ICollection(New clsSet(Of Type)( _
       GetType(ComboBox), _
       GetType(AutomateControls.ComboBoxes.MonoComboBox) _
     ))

    ''' <summary>
    ''' Checks if keydown events on the given control should be delegated out to
    ''' the list containing this item or not.
    ''' </summary>
    ''' <param name="sender">The control to check to see if keydown events should be
    ''' handled.</param>
    ''' <returns>true if the keydown event should be handled for this nested control.
    ''' false otherwise. </returns>
    Private Shared Function ShouldHandleKeyDownFor(ByVal sender As Object) As Boolean
        For Each tp As Type In IgnoreKeyDownTypes
            If tp.IsAssignableFrom(sender.GetType()) Then Return False
        Next
        Return True
    End Function

#End Region

#Region " Member variables "

    ' The owning row of this list item
    Private mParentRow As ctlEditableListRow

    ' The list item is essentially just a container control that holds the real control.
    Private WithEvents mControl As Control

    ' When true, the item will be drawn with a colourful border.
    Private mHighlighted As Boolean

    ' The outer colour to use when highlighting
    Private mHighlightOuterColour As Color

    ' The inner colour to use when highlighting
    Private mHighlightInnerColour As Color

    ' Private member to store public property UnHighlightedForeColor()
    Private mNormalForeColour As Color


#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates an empty editable list item
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates an editable list item wrapping the given control
    ''' </summary>
    ''' <param name="ctl">The control to embed in this list item</param>
    Public Sub New(ByVal ctl As Control)
        Me.New(Nothing, ctl)
    End Sub

    ''' <summary>
    ''' Creates an editable list item using the given parent row, and wrapping the
    ''' given control
    ''' </summary>
    ''' <param name="ctl">The control to embed inside this item. The list item will
    ''' not be valid until a control is set, either through this constructor or
    ''' by using the <see cref="NestedControl"/> property.</param>
    ''' <param name="parentRow">The row owning this item. Null indicates no parent
    ''' row.</param>
    Public Sub New(ByVal parentRow As ctlEditableListRow, ByVal ctl As Control)

        mParentRow = parentRow

        'If the control is a label align it correctly and add padding.
        Dim lb As Label = TryCast(ctl, Label)
        If lb IsNot Nothing Then
            lb.TextAlign = ContentAlignment.MiddleLeft
            lb.Padding = New Padding(2, 0, 0, 0)
        End If

        Dim ex As ctlExpressionEdit = TryCast(ctl, ctlExpressionEdit)
        If ex IsNot Nothing Then ex.Border = False

        Me.NestedControl = ctl
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Indicates whether this item should be drawn with a colourful border or
    ''' not
    ''' </summary>
    Public Property IsHighlighted() As Boolean
        Get
            Return mHighlighted
        End Get
        Set(ByVal value As Boolean)
            mHighlighted = value
        End Set
    End Property

    ''' <summary>
    ''' The outer color to use when highlighting this item.
    ''' </summary>
    Public Property HighlightOuterColour() As Color
        Get
            Return mHighlightOuterColour
        End Get
        Set(ByVal value As Color)
            mHighlightOuterColour = value
        End Set
    End Property

    ''' <summary>
    ''' The inner color to use when highlighting this item.
    ''' </summary>
    Public Property HighlightInnerColour() As Color
        Get
            Return mHighlightInnerColour
        End Get
        Set(ByVal value As Color)
            mHighlightInnerColour = value
        End Set
    End Property

    ''' <summary>
    ''' The row which owns this item.
    ''' </summary>
    Public Property ParentRow() As ctlEditableListRow
        Get
            Return mParentRow
        End Get
        Set(ByVal value As ctlEditableListRow)
            mParentRow = value
        End Set
    End Property


    ''' <summary>
    ''' The control displayed within this list item.
    ''' </summary>
    Public Property NestedControl() As Control
        Get
            Return mControl
        End Get
        Set(ByVal value As Control)
            If value IsNot Nothing Then
                If mControl IsNot Nothing Then Controls.Remove(mControl)

                mControl = value
                Controls.Add(mControl)
                mControl.TabStop = True
                OnResize(Nothing)

                If TypeOf mControl Is ctlExpressionEdit Then
                    CType(mControl, ctlExpressionEdit).HighlightingEnabled = True
                    Invalidate()
                End If
            End If
        End Set
    End Property

    ''' <summary>
    ''' Used to make the selected items background a different color
    ''' </summary>
    ''' <value></value>
    Public WriteOnly Property IsSelected() As Boolean
        Set(ByVal value As Boolean)
            If value Then Highlight() Else UnHighlight()
        End Set
    End Property

#End Region

#Region " Event handler overrides "

    ''' <summary>
    ''' Handles this item being entered by ensuring that it is selected
    ''' </summary>
    ''' <seealso cref="OnSelected"/>
    Protected Overrides Sub OnEnter(ByVal e As EventArgs)
        MyBase.OnEnter(e)
        OnSelected(EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' Handles a mousedown event on this item by ensuring that it is selected
    ''' </summary>
    ''' <seealso cref="OnSelected"/>
    Protected Overrides Sub OnMouseDown(ByVal e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        OnSelected(EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' Handles a keydown event on this item by passing on the message to the
    ''' <see cref="ListItemKeyDown"/> event
    ''' </summary>
    Protected Overrides Sub OnKeyDown(ByVal e As KeyEventArgs)
        MyBase.OnKeyDown(e)
        OnListItemKeyDown(e)
    End Sub


    ''' <summary>
    ''' Handles the resize event to ensure all the elements in the listitem are the
    ''' correct size.
    ''' </summary>
    Protected Overrides Sub OnResize(ByVal e As EventArgs)

        MyBase.OnResize(e)

        If mControl Is Nothing Then Return

        Select Case True

            Case (TypeOf mControl Is IProcessValue), _
             (TypeOf mControl Is ctlExpressionRichTextBox)
                mControl.Top = InnerMargin.Top
                mControl.Left = InnerMargin.Left
                mControl.Width = Me.Width - InnerMargin.Horizontal
                mControl.Height = Me.Height - InnerMargin.Vertical

                'Corner edge hack - different according to whether border exists or not
            Case ((TypeOf mControl Is TextBox) AndAlso CType(mControl, TextBox).BorderStyle = BorderStyle.None)
                mControl.Top = 3
                mControl.Left = 3
                mControl.Width = Me.Width - 4

            Case (TypeOf mControl Is CheckBox)
                mControl.Width = 14
                mControl.Top = (Me.Height - mControl.Height) \ 2
                mControl.Left = (Me.Width - mControl.Width) \ 2

            Case Else
                mControl.Width = Me.Width - 1
                mControl.Height = Me.Height - 1
                mControl.Top = (Me.Height - mControl.Height) \ 2
                mControl.Left = (Me.Width - mControl.Width) \ 2
        End Select

    End Sub

    ''' <summary>
    ''' Handles the paint event for this control
    ''' </summary>
    Protected Overrides Sub OnPaint(ByVal e As PaintEventArgs)
        MyBase.OnPaint(e)
        Dim g As Graphics = e.Graphics
        If mParentRow.IsSelected Then g.Clear(mParentRow.HighlightedBackColour)

        If Me.IsHighlighted Then
            Static HighlightPen As New Pen(Color.White)
            Dim rect As Rectangle = New Rectangle(Point.Empty, Me.ClientSize)

            rect.Inflate(-1, -1)
            HighlightPen.Color = mHighlightOuterColour
            g.DrawRectangle(HighlightPen, rect)

            rect.Inflate(-1, -1)
            HighlightPen.Color = mHighlightInnerColour
            g.DrawRectangle(HighlightPen, rect)
        End If
    End Sub

    ''' <summary>
    ''' Handles the forecolor changing in this control by propogating the change
    ''' to the nested control if there is one.
    ''' </summary>
    Protected Overrides Sub OnForeColorChanged(ByVal e As EventArgs)
        MyBase.OnForeColorChanged(e)
        If mControl IsNot Nothing Then mControl.ForeColor = Me.ForeColor
    End Sub

#End Region

#Region " Nested control event handlers "

    ''' <summary>
    ''' Handles the nested control being entered, ensuring that this item is
    ''' selected.
    ''' </summary>
    Private Sub HandleControlEntered(ByVal sender As Object, ByVal e As EventArgs) _
     Handles mControl.Enter
        OnSelected(EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' Handles a keydown event for the nested control, passing on the message to
    ''' the <see cref="ListItemKeyDown"/> event if KeyDown events are handled for
    ''' the type of source control.
    ''' </summary>
    Private Sub HandleControlKeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) _
     Handles mControl.KeyDown
        If ShouldHandleKeyDownFor(sender) Then OnListItemKeyDown(e)
    End Sub

    ''' <summary>
    ''' Handles the preview keydown event for the nested control. This ensures that
    ''' up and down keys are marked as input keys, unless we should not be
    ''' <see cref="ShouldHandleKeyDownFor">handling keydown events</see> for the
    ''' sending control.
    ''' </summary>
    Private Sub HandleControlPreviewKeyDown( _
     ByVal sender As Object, ByVal e As PreviewKeyDownEventArgs) Handles mControl.PreviewKeyDown
        Dim key As Keys = e.KeyCode
        If ShouldHandleKeyDownFor(sender) AndAlso (key = Keys.Up OrElse key = Keys.Down) Then
            e.IsInputKey = True
        End If
    End Sub

#End Region

#Region " EditableListItem-fired event methods "

    ''' <summary>
    ''' Handles the selected event on the nested control.
    ''' </summary>
    ''' <remarks>This event is passed up the chain to the parent row etc,
    ''' so that the listview can highlight selected rows.</remarks>
    Protected Overridable Sub OnSelected(ByVal e As EventArgs)
        If mParentRow.HasOwner Then RaiseEvent Selected(Me)
    End Sub

    ''' <summary>
    ''' Used to control key events - so that we can cursor up and down through the
    ''' list
    ''' </summary>
    Protected Overridable Sub OnListItemKeyDown(ByVal e As KeyEventArgs)
        RaiseEvent ListItemKeyDown(Me, e)
        If e.KeyCode = Keys.Up OrElse e.KeyCode = Keys.Down Then e.Handled = True
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Gets the preferred size for this item, taking into account the nested control
    ''' if one is embedded within this item.
    ''' </summary>
    ''' <param name="proposedSize"></param>
    ''' <returns></returns>
    Public Overrides Function GetPreferredSize(ByVal proposedSize As Size) As Size
        If mControl Is Nothing Then Return Size.Empty
        Return mControl.GetPreferredSize(proposedSize) + InnerMargin.Size
    End Function

    ''' <summary>
    ''' Highlights the item  by changing its backcolor
    ''' </summary>
    Private Sub Highlight()
        mNormalForeColour = NestedControl.ForeColor

        NestedControl.BackColor = mParentRow.HighlightedBackColour
        NestedControl.ForeColor = mParentRow.HighlightedForeColor

        'If automate controls combo box then deal with back colour behaviour
        Dim cmb As AutomateControls.ComboBoxes.MonoComboBox = _
         TryCast(NestedControl, AutomateControls.ComboBoxes.MonoComboBox)

        If cmb IsNot Nothing Then
            cmb.BackColor = mParentRow.HighlightedBackColour
            cmb.DropDownBackColor = SystemColors.ControlLightLight
        End If

        'This is a bit of a bespoke hack to invert the colour of the text 
        'in the expression richtextbox against the new backcolour
        Dim ex As ctlExpressionEdit = TryCast(NestedControl, ctlExpressionEdit)
        If Not ex Is Nothing Then
            ex.HighlightingEnabled = False
            ex.ForeColor = mParentRow.HighlightedForeColor
        End If
        Invalidate()
    End Sub

    ''' <summary>
    ''' Unhighlights the item  by changing its backcolor back to defaults.
    ''' </summary>
    Private Sub UnHighlight()
        'Reset colors to how they were before we highlighted the row
        NestedControl.BackColor = SystemColors.ControlLightLight
        If Me.mNormalForeColour.Equals(Color.Empty) Then
            NestedControl.ForeColor = SystemColors.ControlText
        Else
            NestedControl.ForeColor = mNormalForeColour
        End If

        'This is a bit of a bespoke hack to make the expression rich text box behave 
        Dim ex As ctlExpressionEdit = TryCast(NestedControl, ctlExpressionEdit)
        If Not ex Is Nothing Then
            ex.HighlightingEnabled = True
        End If
        Invalidate()
    End Sub

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlEditableListItem))
        Me.SuspendLayout()
        '
        'ctlEditableListItem
        '
        resources.ApplyResources(Me, "$this")
        Me.Name = "ctlEditableListItem"
        Me.ResumeLayout(False)

    End Sub

#End Region

End Class
