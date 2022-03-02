Imports BluePrism.AutomateProcessCore.Stages

''' <summary>
''' Control which holds the name of the stage in which to store the result of an
''' expression
''' </summary>
''' <remarks>
''' FIXME: A lot of this (especially event handling) functionality is identical
''' to ctlExpressionEdit - a common base class might be useful here.
''' </remarks>
Public Class ctlStoreInEdit : Inherits UserControl

#Region " Class Scope Declarations "

    ''' <summary>
    ''' Event raised when a data item should be created
    ''' with the given name.
    ''' </summary>
    ''' <param name="DataItemName">The name of the data item
    ''' to be created, minus any square brackets.</param>
    Public Event AutoCreateRequested(ByVal DataItemName As String)

#End Region

#Region " Member Variables "

    ' The default name to use when auto-create is clicked
    Private mAutoCreateDefaultName As String

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Constructor
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        btnAutoCreate.Enabled = False
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The forecolor of this control. Setting this sets the nested control at the
    ''' same time.
    ''' </summary>
    Public Overrides Property ForeColor() As Color
        Get
            Return MyBase.ForeColor
        End Get
        Set(ByVal value As Color)
            MyBase.ForeColor = value
            txtStoreInValue.ForeColor = value
        End Set
    End Property

    ''' <summary>
    ''' The back color of this control. Setting this sets the nested control at the
    ''' same time.
    ''' </summary>
    Public Overrides Property BackColor() As System.Drawing.Color
        Get
            Return MyBase.BackColor
        End Get
        Set(ByVal value As System.Drawing.Color)
            MyBase.BackColor = value
            txtStoreInValue.BackColor = value
        End Set
    End Property

    ''' <summary>
    ''' Return the txtStorinValue.text as the text of this control
    ''' </summary>
    Public Overrides Property Text() As String
        Get
            Return txtStoreInValue.Text
        End Get
        Set(ByVal value As String)
            txtStoreInValue.Text = value
        End Set
    End Property

    ''' <summary>
    ''' Return the txtStorinValue.PasswordChar as the PasswordChar of this control
    ''' </summary>
    Property PasswordChar() As Char
        Get
            Return txtStoreInValue.PasswordChar
        End Get
        Set(ByVal value As Char)
            txtStoreInValue.PasswordChar = value
        End Set
    End Property

    ''' <summary>
    ''' The default name to be given to an auto-created data item, if no text is
    ''' entered into this field.
    ''' </summary>
    ''' <remarks>A data item is auto-created when the user clicks the button on this
    ''' control. The text in the field is used as the data item name. If the text is
    ''' blank then this value is used instead.</remarks>
    Public Property AutoCreateDefault() As String
        Get
            Return mAutoCreateDefaultName
        End Get
        Set(ByVal value As String)
            mAutoCreateDefaultName = value
            UpdateButton()
        End Set
    End Property

    ''' <summary>
    ''' Gets whether this control has text focus - ie. has the focus in a text
    ''' control operated within this control
    ''' </summary>
    Public ReadOnly Property HasTextFocus As Boolean
        Get
            Return txtStoreInValue.Focused
        End Get
    End Property

#End Region

#Region " Overriding Event Handlers "

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
    Protected Overrides Sub OnDragEnter(ByVal e As DragEventArgs)
        MyBase.OnDragEnter(e)
        Dim item As ctlEditableListItem = GetEditableListItemAncestor()

        If (e.Data.GetDataPresent(GetType(TreeNode))) Then
            e.Effect = DragDropEffects.Move
        Else
            e.Effect = DragDropEffects.None
        End If

        If item IsNot Nothing Then
            item.IsHighlighted = e.Effect = DragDropEffects.Move
            item.HighlightInnerColour = AutomateControls.ColourScheme.Default.ListViewDataStoreInDragDropHighlightInner
            item.HighlightOuterColour = AutomateControls.ColourScheme.Default.ListViewDataStoreInDragDropHighlightOuter
            item.Invalidate()
        End If
    End Sub

    ''' <summary>
    ''' Handles an item being dragged away from this control
    ''' </summary>
    Protected Overrides Sub OnDragLeave(ByVal e As System.EventArgs)
        MyBase.OnDragLeave(e)

        Dim item As ctlEditableListItem = GetEditableListItemAncestor()
        If item IsNot Nothing Then
            item.IsHighlighted = False
            item.Invalidate()
        End If
    End Sub

    ''' <summary>
    ''' Override the enabled changed event to ensure the background is set to the
    ''' disabled color
    ''' </summary>
    Protected Overrides Sub OnEnabledChanged(ByVal e As EventArgs)
        MyBase.OnEnabledChanged(e)
        txtStoreInValue.Enabled = Me.Enabled
        If Not Me.Enabled Then
            Me.BackColor = SystemColors.Control
        Else
            Me.BackColor = SystemColors.Window
        End If
    End Sub

#End Region

#Region " Child Event Handlers "

    ''' <summary>
    ''' Treat child controls being entered as this control being entered
    ''' </summary>
    Private Sub HandleChildControlEntered(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnAutoCreate.Enter, txtStoreInValue.Enter
        OnEnter(e)
    End Sub

    ''' <summary>
    ''' Treat child controls' mousedown this control's mousedown
    ''' </summary>
    Private Sub HandleChildControlMouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) _
     Handles btnAutoCreate.MouseDown, txtStoreInValue.MouseDown
        OnEnter(EventArgs.Empty)
    End Sub

    ''' <summary>
    ''' Treat child controls firing preview key down as this control previewing key
    ''' down.
    ''' </summary>
    Private Sub HandleChildControlPreviewKeyDown( _
     ByVal sender As Object, ByVal e As PreviewKeyDownEventArgs) _
     Handles btnAutoCreate.PreviewKeyDown, txtStoreInValue.PreviewKeyDown
        OnPreviewKeyDown(e)
    End Sub


    ''' <summary>
    ''' Treat child controls firing keydown as this control firing key down
    ''' </summary>
    Private Sub HandleChildControlKeyDown( _
     ByVal sender As Object, ByVal e As KeyEventArgs) _
     Handles btnAutoCreate.KeyDown, txtStoreInValue.KeyDown
        OnKeyDown(e)
    End Sub

    ''' <summary>
    ''' Chains LostFocus events from the store in text box to this control
    ''' </summary>
    Private Sub HandleStoreInLostFocus(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtStoreInValue.LostFocus
        OnLostFocus(e)
    End Sub


    ''' <summary>
    ''' Apply a dragrop effect if the a node were dragged over this control
    ''' </summary>
    Private Sub HandleStoreInDragEnter(ByVal sender As Object, ByVal e As DragEventArgs) _
     Handles txtStoreInValue.DragEnter
        OnDragEnter(e)
    End Sub

    ''' <summary>
    ''' Chains DragDrop events from the store in text box to this control
    ''' </summary>
    Private Sub HandleStoreInDragDrop(ByVal sender As Object, ByVal e As DragEventArgs) _
     Handles txtStoreInValue.DragDrop
        OnDragDrop(e)
    End Sub

    ''' <summary>
    ''' Chains DragLeave events from the store in text box to this control
    ''' </summary>
    Private Sub HandleStoreInDragLeave(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtStoreInValue.DragLeave
        OnDragLeave(e)
    End Sub

    ''' <summary>
    ''' When the StoreinValue is clicked raise this controls event instead
    ''' </summary>
    Private Sub HandleStoreInValueClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtStoreInValue.Click
        OnClick(e)
    End Sub

    ''' <summary>
    ''' Handles the auto-create button being clicked.
    ''' </summary>
    Private Sub HandleAutoCreateClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnAutoCreate.Click
        'Use the current text as the data item name, or
        'revert to the default if none
        Dim DataItemName As String = Me.Text
        If String.IsNullOrEmpty(DataItemName) Then
            DataItemName = Me.mAutoCreateDefaultName
            Me.txtStoreInValue.Text = DataItemName
        End If

        'Disallow blank data item names
        If String.IsNullOrEmpty(DataItemName) Then
            UserMessage.Show(My.Resources.ctlStoreInEdit_YouMustTypeANameForTheDataItemToBeAutoCreated)
            Exit Sub
        End If

        'Check for illegal characters in chosen name
        Dim sErr As String = Nothing
        If Not clsDataStage.IsValidDataName(DataItemName, sErr) Then
            UserMessage.Show(String.Format(My.Resources.ctlStoreInEdit_CanNotCreateDataStageWithTheChosenName0, sErr))
            Exit Sub
        End If

        Try
            RaiseEvent AutoCreateRequested(DataItemName)
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlStoreInEdit_UnexpectedError0, ex.Message))
        End Try
    End Sub

    ''' <summary>
    ''' Handles the text changing in the Store In text box.
    ''' </summary>
    Private Sub HandleStoreInTextChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtStoreInValue.TextChanged
        UpdateButton()
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Selects all the text in this store in edit control
    ''' </summary>
    Public Sub SelectAll()
        txtStoreInValue.SelectAll()
    End Sub

    ''' <summary>
    ''' Gets the owning editablelistitem object, if it exists.
    ''' </summary>
    ''' <returns>The first ancestor which is an editable list item,
    ''' if one exists. Returns nothing otherwise.</returns>
    Private Function GetEditableListItemAncestor() As ctlEditableListItem
        Return clsUserInterfaceUtils.GetAncestor(Of ctlEditableListItem)(Me)
    End Function

    ''' <summary>
    ''' Enables or disables auto-create button, depending on whether the store-in
    ''' text field contains a name
    ''' </summary>
    Private Sub UpdateButton()
        'Can always autocreate unless the field text is empty, and there is no default set
        btnAutoCreate.Enabled = (mAutoCreateDefaultName <> "" OrElse txtStoreInValue.Text <> "")
    End Sub

#End Region

End Class
