Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : ctlExpressionEdit
''' 
''' <summary>
''' A simple wrapper control that adds a button to a ctlExpressionRichTextBox.
''' This button when pressed opens up a new instance of the ExpressionViewer 
''' control
''' </summary>
''' <remarks>
''' FIXME: A lot of this (especially event handling) functionality is identical
''' to ctlStoreInEdit - a common base class might be useful here.
''' </remarks>
Public Class ctlExpressionEdit : Inherits UserControl

#Region " Events "

    ''' <summary>
    ''' Event raised when the <see cref="TextBoxPadding"/> property of this control
    ''' is changed.
    ''' </summary>
    Public Event TextBoxPaddingChanged As EventHandler

#End Region

#Region " Member Variables "

    ''' <summary>
    ''' Stores the full text of the expression. We can't store this in the textbox
    ''' control, because the textbox control is Single-Line. Any multiline expression
    ''' entered into the pop-up form would otherwise be truncated. For this reason we
    ''' also disable the in-line edit box when the expression is multiline.
    ''' </summary>
    Private mText As String

    ' The padding to apply around the textbox.
    Private mTextBoxPadding As Padding

    ' The process viewer within which the full expression editor can be launched
    Private mProcessViewer As ctlProcessViewer

    ' The stage that this control relates to, to be used as a scope stage when
    ' opening the expression editor form
    Private mProcessStage As clsProcessStage

    ''' <summary>
    ''' Holds a value indicating whether this Expression edit field represents a 
    ''' decision or not.
    ''' </summary>
    Private mbDecision As Boolean = False

    ''' <summary>
    ''' Indicates when it is that we are causing a change to the text property (in 
    ''' which case no need to updated the richtextbox again). When not set, the 
    ''' change must come from outside, meaning we need to pass this change on to the
    ''' richtextbox.
    ''' </summary>
    Private mbMeChangingText As Boolean

    ' Whether the border is applied to this control or not
    Private mbBorder As Boolean

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new expression editor control
    ''' </summary>
    Public Sub New()
        InitializeComponent()
        mTextBoxPadding = DefaultTextBoxPadding
        mTooltip.SetToolTip(
            btnExpression, My.Resources.ctlExpressionEdit_ClickThisButtonToOpenFullExpressionEditor)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Use (3,1,3,1) as default padding for this control
    ''' </summary>
    Protected Overridable ReadOnly Property DefaultTextBoxPadding As Padding
        Get
            Return New Padding(3, 4, 3, 1)
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the padding which separates the textbox from the rest of the
    ''' expression editor control
    ''' </summary>
    <Browsable(True), Category("Layout"), Description(
        "The padding to apply around the textbox within the expression editor")>
    Public Property TextBoxPadding As Padding
        Get
            Return mTextBoxPadding
        End Get
        Set(value As Padding)
            If value <> mTextBoxPadding Then
                mTextBoxPadding = value
                OnTextBoxPaddingChanged(EventArgs.Empty)
            End If
        End Set
    End Property

    ''' <summary>
    ''' A process viewer used to launch stage properties.
    ''' </summary>
    ''' <remarks>May be null, but if null then no stage properties can be viewed.</remarks>
    <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Friend Property ProcessViewer() As ctlProcessViewer
        Get
            Return mProcessViewer
        End Get
        Set(ByVal value As ctlProcessViewer)
            mProcessViewer = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the underlying text value from the Expression 
    ''' </summary>
    Public Overrides Property Text() As String
        Get
            Return mText
        End Get
        Set(ByVal value As String)
            mText = value
            Me.UpdateInlineExpressionField()
        End Set
    End Property

    ''' <summary>
    ''' Sets the forecolor of the underlying control, as well  as the forcolor of 
    ''' this control
    ''' </summary>
    Public Overrides Property ForeColor() As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(ByVal value As Color)
            MyBase.ForeColor = value
            txtExpression.ForeColor = value
        End Set
    End Property

    ''' <summary>
    ''' Sets the underlying backcolor of the control, as well as the backcolor of 
    ''' this control
    ''' </summary>
    Public Overrides Property BackColor() As System.Drawing.Color
        Get
            Return MyBase.BackColor
        End Get
        Set(ByVal value As System.Drawing.Color)
            MyBase.BackColor = value
            txtExpression.BackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Sets a value indicating weather the nested expression control should have a 
    ''' border.
    ''' </summary>
    Public Property Border() As Boolean
        Get
            Return mbBorder
        End Get
        Set(ByVal value As Boolean)
            mbBorder = value
            If value Then
                txtExpression.BorderStyle = BorderStyle.FixedSingle
                txtExpression.Dock = DockStyle.Fill
            Else
                txtExpression.BorderStyle = BorderStyle.None
            End If
            LayoutTextBox()
        End Set
    End Property

    ''' <summary>
    ''' Sets the underlying passwordchar of the expression
    ''' </summary>
    Public Property PasswordChar() As Char
        Get
            Return txtExpression.PasswordChar
        End Get
        Set(ByVal value As Char)
            txtExpression.PasswordChar = value
        End Set
    End Property

    ''' <summary>
    ''' Sets the underlying hilighting enabled property of the control
    ''' </summary>
    Public Property HighlightingEnabled() As Boolean
        Get
            Return txtExpression.HighlightingEnabled
        End Get
        Set(ByVal value As Boolean)
            txtExpression.HighlightingEnabled = value
        End Set
    End Property

    ''' <summary>
    ''' The stage being worked with as the scope stage.
    ''' This value will be passed to the expression
    ''' building popup form as the scope stage for the
    ''' embedded data item treeview.
    ''' </summary>
    Public Property Stage() As clsProcessStage
        Get
            Return Me.mProcessStage
        End Get
        Set(ByVal value As clsProcessStage)
            Me.mProcessStage = value
        End Set
    End Property

    ''' <summary>
    ''' Provides access to a value indicating whether this field represents a 
    ''' decision or not.
    ''' </summary>
    Public Property IsDecision() As Boolean
        Get
            Return mbDecision
        End Get
        Set(ByVal value As Boolean)
            mbDecision = value
        End Set
    End Property

    ''' <summary>
    ''' Gets whether this control has text focus - ie. has the focus in a text
    ''' control operated within this control
    ''' </summary>
    Public ReadOnly Property HasTextFocus As Boolean
        Get
            Return txtExpression.Focused
        End Get
    End Property

#End Region

#Region " OnXxx-style Event Handlers "

    ''' <summary>
    ''' Ensures that the textbox is laid out correctly when this control is loaded
    ''' </summary>
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        LayoutTextBox()
    End Sub

    ''' <summary>
    ''' Override the enabled changed event to ensure the background is set to the 
    ''' disabled color
    ''' </summary>
    Protected Overrides Sub OnEnabledChanged(ByVal e As EventArgs)
        MyBase.OnEnabledChanged(e)
        If Not Me.Enabled Then
            Me.BackColor = SystemColors.Control
        Else
            Me.BackColor = Color.White
        End If
    End Sub

    ''' <summary>
    ''' Handles the PreviewKeyDown event occurring in this object.
    ''' </summary>
    Protected Overrides Sub OnPreviewKeyDown(ByVal e As PreviewKeyDownEventArgs)
        MyBase.OnPreviewKeyDown(e)
        If e.KeyCode = Keys.Up OrElse e.KeyCode = Keys.Down Then e.IsInputKey = True
    End Sub

    ''' <summary>
    ''' Handles an item being dragged into this control
    ''' </summary>
    ''' <param name="e">The event args detailing the drag event</param>
    Protected Overrides Sub OnDragEnter(ByVal e As DragEventArgs)
        MyBase.OnDragEnter(e)

        If (e.Data.GetDataPresent(GetType(TreeNode))) Then
            e.Effect = DragDropEffects.Move
        Else
            e.Effect = DragDropEffects.None
        End If

        Dim item As ctlEditableListItem = GetEditableListItemAncestor()
        If item IsNot Nothing Then
            item.IsHighlighted = e.Effect = DragDropEffects.Move
            item.HighlightInnerColour = AutomateControls.ColourScheme.Default.ListViewExpressionDataDragDropHighlightInner
            item.HighlightOuterColour = AutomateControls.ColourScheme.Default.ListViewExpressionDataDragDropHighlightOuter
            item.Invalidate()
        End If
    End Sub

    ''' <summary>
    ''' Handles an item being dragged away from this control
    ''' </summary>
    Protected Overrides Sub OnDragLeave(ByVal e As EventArgs)
        Dim item As ctlEditableListItem = GetEditableListItemAncestor()
        If item IsNot Nothing Then
            item.IsHighlighted = False
            item.Invalidate()
        End If
    End Sub

    ''' <summary>
    ''' Raises the <see cref="TextBoxPaddingChanged"/> event
    ''' </summary>
    Protected Overridable Sub OnTextBoxPaddingChanged(e As EventArgs)
        LayoutTextBox()
        RaiseEvent TextBoxPaddingChanged(Me, e)
    End Sub

#End Region

#Region " Child Control event handlers "

    ''' <summary>
    ''' Treat child controls being entered as this control being entered
    ''' </summary>
    Private Sub HandleChildControlEntered(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnExpression.Enter, txtExpression.Enter
        OnEnter(e)
    End Sub

    ''' <summary>
    ''' Treat child controls' mousedown this control's mousedown
    ''' </summary>
    Private Sub HandleChildControlMouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) _
     Handles btnExpression.MouseDown, txtExpression.MouseDown
        OnEnter(EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' Treat child controls firing preview key down as this control previewing key
    ''' down.
    ''' </summary>
    Private Sub HandleChildControlPreviewKeyDown(
     ByVal sender As Object, ByVal e As PreviewKeyDownEventArgs) _
     Handles btnExpression.PreviewKeyDown, txtExpression.PreviewKeyDown
        OnPreviewKeyDown(e)
    End Sub

    ''' <summary>
    ''' Treat child controls firing keydown as this control firing key down
    ''' </summary>
    Private Sub HandleChildControlKeyDown(
     ByVal sender As Object, ByVal e As KeyEventArgs) _
     Handles btnExpression.KeyDown, txtExpression.KeyDown
        OnKeyDown(e)
    End Sub

    ''' <summary>
    ''' Shows an expression edit fired off the expression button.
    ''' </summary>
    Private Sub HandleButtonClick(ByVal sender As Object, ByVal e As EventArgs) Handles btnExpression.Click
        Using f As New frmExpressionChooser()
            f.SetEnvironmentColoursFromAncestor(Me)
            Debug.Assert(Me.Stage IsNot Nothing, "Cannot show expression form without stage being set")
            f.Stage = Me.Stage
            f.Expression = mText
            If mbDecision Then
                f.mExpressionBuilder.Validator = AddressOf f.mExpressionBuilder.IsValidDecision
                f.mExpressionBuilder.Tester = AddressOf f.mExpressionBuilder.TestDecision
            Else
                f.mExpressionBuilder.Validator = AddressOf f.mExpressionBuilder.IsValidExpression
                f.mExpressionBuilder.Tester = AddressOf f.mExpressionBuilder.TestExpression
            End If
            f.mExpressionBuilder.ProcessViewer = mProcessViewer
            f.mExpressionBuilder.StoreInVisible = False
            f.ShowInTaskbar = False

            If f.ShowDialog = System.Windows.Forms.DialogResult.OK Then
                mText = f.Expression
                UpdateInlineExpressionField()
                OnLostFocus(EventArgs.Empty)
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Apply a dragdrop effect if a node is dragged over this control
    ''' </summary>
    Private Sub HandleExpressionDragEnter(ByVal sender As Object, ByVal e As DragEventArgs) Handles txtExpression.DragEnter
        OnDragEnter(e)
    End Sub

    ''' <summary>
    ''' When the Expression recieves dragdrop event make this control raise its 
    ''' dragdrop event instead
    ''' </summary>
    Private Sub HandleExpressionDragDrop(ByVal sender As Object, ByVal e As DragEventArgs) Handles txtExpression.DragDrop
        OnDragDrop(e)
    End Sub

    ''' <summary>
    ''' When the Expression is clicked raise this controls event instead
    ''' </summary>
    Private Sub HandleExpressionClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles txtExpression.Click
        OnClick(e)
    End Sub

    ''' <summary>
    ''' When the Expression looses focus make this control raise its loose focus 
    ''' event instead
    ''' </summary>
    Private Sub HandleExpressionLostFocus(ByVal sender As Object, ByVal e As EventArgs) Handles txtExpression.LostFocus
        OnLostFocus(e)
    End Sub


    ''' <summary>
    ''' Event handler for the Expressions text changed event
    ''' </summary>
    Private Sub HandleExpressionTextChanged(ByVal sender As Object, ByVal e As EventArgs) Handles txtExpression.TextChanged
        Try
            Me.mbMeChangingText = True
            If Not Me.txtExpression.ReadOnly Then
                mText = Me.txtExpression.Text
            End If
            Me.UpdateInlineExpressionField()
        Finally
            Me.mbMeChangingText = False
        End Try
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Checks if the <see cref="TextBoxPadding"/> property should be serialized.
    ''' </summary>
    ''' <returns>True if the current <see cref="TextBoxPadding"/> value is not equal
    ''' to <see cref="DefaultTextBoxPadding"/></returns>
    Protected Overridable Function ShouldSerializeTextBoxPadding() As Boolean
        Return TextBoxPadding <> DefaultTextBoxPadding
    End Function

    ''' <summary>
    ''' Resets the <see cref="TextBoxPadding"/> property on this control
    ''' </summary>
    Private Sub ResetTextBoxPadding()
        TextBoxPadding = DefaultTextBoxPadding
    End Sub

    ''' <summary>
    ''' Selects all the text in the rich text box held by this expression editor.
    ''' </summary>
    Public Sub SelectAll()
        txtExpression.SelectAll()
    End Sub

    ''' <summary>
    ''' Performs updates to the inline expression field, according to the current 
    ''' value of the Expression. Eg multiline expressions cause the edit field to 
    ''' become readonly with a special message.
    ''' </summary>
    Private Sub UpdateInlineExpressionField()
        mTooltip.SetToolTip(txtExpression, My.Resources.ctlExpressionEdit_Expression & vbCrLf & Me.Text)

        'If Text contains carriage return then we can't
        'edit it properly in the inline edit area - popup
        'form must be used instead
        Dim Index As Integer = -1
        If mText IsNot Nothing Then
            Index = mText.IndexOf(vbLf)
        End If
        Dim WasReadonly As Boolean = txtExpression.ReadOnly
        txtExpression.ReadOnly = (Index > -1)

        RemoveHandler txtExpression.TextChanged, AddressOf Me.HandleExpressionTextChanged
        If txtExpression.ReadOnly Then
            txtExpression.Text = My.Resources.MultilineExpressionClickButtonToEdit
        Else
            If WasReadonly OrElse txtExpression.Text <> mText Then
                txtExpression.Text = mText
            End If
        End If
        AddHandler txtExpression.TextChanged, AddressOf Me.HandleExpressionTextChanged
    End Sub

    ''' <summary>
    ''' Colours the text of the underlying expression
    ''' </summary>
    Public Sub ColourText()
        RemoveHandler txtExpression.TextChanged, AddressOf Me.HandleExpressionTextChanged
        txtExpression.ColourText()
        AddHandler txtExpression.TextChanged, AddressOf Me.HandleExpressionTextChanged
    End Sub

    ''' <summary>
    ''' Sets the expression controls size and position based on the controls margin 
    ''' property.
    ''' </summary>
    Private Sub LayoutTextBox()
        Dim p = TextBoxPadding
        With txtExpression
            .Anchor = AnchorStyles.None
            .Dock = DockStyle.None
            .Location = New Point(p.Left, p.Top)
            .Size = New Size(
                Width - btnExpression.Width - p.Left - p.Right,
                Height - p.Top - p.Bottom)
            .Anchor = AnchorStyles.Bottom Or
                AnchorStyles.Left Or
                AnchorStyles.Right Or
                AnchorStyles.Top
        End With
    End Sub

    ''' <summary>
    ''' Gets the owning editablelistitem object, if it exists.
    ''' </summary>
    ''' <returns>The first ancestor which is a editable list item, if one exists. 
    ''' Returns nothing otherwise.</returns>
    Private Function GetEditableListItemAncestor() As ctlEditableListItem
        Return clsUserInterfaceUtils.GetAncestor(Of ctlEditableListItem)(Me)
    End Function

#End Region

End Class
