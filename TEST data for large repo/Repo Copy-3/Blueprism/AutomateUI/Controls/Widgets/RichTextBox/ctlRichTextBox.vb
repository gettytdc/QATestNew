''' Project  : Automate
''' Class    : AutomateUI.ctlRichTextBox
''' 
''' <summary>
''' Class that adds a few features to the standard .NET rich text box.
''' </summary>
Public Class ctlRichTextBox
    Inherits RichTextBox

    ''' <summary>
    ''' Used to render the win2k style border.
    ''' </summary>
    Private mobjRender As VisualStyles.VisualStyleRenderer

    ''' <summary>
    ''' This event is raised whenever a non client paint event occours.
    ''' </summary>
    ''' <remarks>This is not currently used and is provided for convenience in future
    ''' implementations.</remarks>
    Public Event NonClientPaint As EventHandler

    ''' <summary>
    ''' This is the width of the rendered border which is set to a constant 3 pixels.
    ''' </summary>
    Private Const BorderWidth As Integer = 2

    ''' <summary>
    ''' True to accept the enter key being pressed; false to delegate it to the
    ''' form handling - ie. its default button.
    ''' </summary>
    Private mAcceptsReturn As Boolean

    ''' <summary>
    ''' Constructor
    ''' </summary>
    Public Sub New()
        MyBase.New()

        ' Default accepts return to true
        mAcceptsReturn = True
        Me.ImeMode = ImeMode.On

        'initialise the context menu
        Me.mnuContext = New ContextMenu
        Me.mnuContext.MenuItems.Add(New MenuItem(My.Resources.ctlRichTextBox_CUt, AddressOf OnCut, Shortcut.CtrlX))
        Me.mnuContext.MenuItems.Add(New MenuItem(My.Resources.ctlRichTextBox_Copy, AddressOf OnCopy, Shortcut.CtrlC))
        Me.mnuContext.MenuItems.Add(New MenuItem(My.Resources.ctlRichTextBox_Paste, AddressOf OnPaste, Shortcut.CtrlV))
        Me.mnuContext.MenuItems.Add(New MenuItem(My.Resources.ctlRichTextBox_Delete, AddressOf OnDelete, Shortcut.Del))
        Me.mnuContext.MenuItems.Add(My.Resources.ctlRichTextBox_MenuSeparator)
        Me.mnuContext.MenuItems.Add(New MenuItem(My.Resources.ctlRichTextBox_SelectAll, AddressOf OnSelectAll, Shortcut.CtrlA))
    End Sub


    ''' <summary>
    ''' Gets or sets a value indicating whether pressing ENTER in a multiline
    ''' Rich Text Box control creates a new line of text in the control or
    ''' activates the default button for the form
    ''' </summary>
    <Browsable(True), _
     EditorBrowsable(EditorBrowsableState.Always), _
     DesignerSerializationVisibility(DesignerSerializationVisibility.Content), _
     DefaultValue(True), Category("Behavior"), _
     Description("Indicates if return characters are accepted as input for multi-line edit controls.")> _
    Public Property AcceptsReturn() As Boolean
        Get
            Return mAcceptsReturn
        End Get
        Set(ByVal value As Boolean)
            mAcceptsReturn = value
        End Set
    End Property

    ''' <summary>
    ''' We handle non-client painting here is this native typesafe method
    ''' </summary>
    ''' <param name="e"></param>
    Protected Overridable Sub OnNonClientPaint(ByVal e As PaintEventArgs)
        If mobjRender Is Nothing Then
            mobjRender = New VisualStyles.VisualStyleRenderer(VisualStyles.VisualStyleElement.TextBox.TextEdit.Normal)
        End If
        mobjRender.DrawBackground(e.Graphics, e.ClipRectangle)
        RaiseEvent NonClientPaint(Me, e)
    End Sub

#Region "Members"

    ''' <summary>
    ''' The context menu displayed on this richtextbox.
    ''' </summary>
    Private mnuContext As ContextMenu

#End Region

#Region "Event handlers"

    ''' <summary>
    ''' Handles the mouse up event of the rich text box.
    ''' </summary>
    ''' <param name="sender">The sender of the event</param>
    ''' <param name="e">The event arguments</param>
    Private Sub RichTextBox_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles MyBase.MouseUp
        If e.Button = System.Windows.Forms.MouseButtons.Right Then

            'disable paste if there is nothing on the clipboard
            mnuContext.MenuItems(2).Enabled = System.Windows.Forms.Clipboard.GetDataObject.GetDataPresent(GetType(String))

            'cut, delete and copy only enabled if we have some text selected
            Me.mnuContext.MenuItems(0).Enabled = Me.SelectionLength > 0
            Me.mnuContext.MenuItems(1).Enabled = Me.mnuContext.MenuItems(0).Enabled
            Me.mnuContext.MenuItems(3).Enabled = Me.mnuContext.MenuItems(0).Enabled

            'disable cut, delete and paste if the box is readonly
            If Me.ReadOnly Then
                Me.mnuContext.MenuItems(2).Enabled = False
                Me.mnuContext.MenuItems(0).Enabled = False
                Me.mnuContext.MenuItems(3).Enabled = False
            End If

            mnuContext.Show(Me, New Point(e.X, e.Y))
        End If
    End Sub

    ''' <summary>
    ''' Determines whether the specified key is an input key or a special key that
    ''' requires preprocessing.
    ''' This enables the text box to accept return keys
    ''' </summary>
    ''' <param name="keyData">One of the keys values</param>
    ''' <returns>true if the specified key is an input key; otherwise, false
    ''' </returns>
    Protected Overrides Function IsInputKey(ByVal keyData As Keys) As Boolean

        If Me.Multiline AndAlso (keyData And Keys.Alt) = Keys.None Then
            If (keyData And Keys.KeyCode) = Keys.Return Then Return mAcceptsReturn
        End If
        Return MyBase.IsInputKey(keyData)

    End Function

    ''' <summary>
    ''' Event handler for cutting of text.
    ''' </summary>
    ''' <param name="sender">.</param>
    ''' <param name="e">.</param>
    Private Sub OnCut(ByVal sender As Object, ByVal e As EventArgs)
        MyBase.Cut()
    End Sub

    ''' <summary>
    ''' Event handler for copying of text.
    ''' </summary>
    ''' <param name="sender">.</param>
    ''' <param name="e">.</param>
    Private Sub OnCopy(ByVal sender As Object, ByVal e As EventArgs)
        MyBase.Copy()
    End Sub

    ''' <summary>
    ''' Event handler for pasting of text.
    ''' </summary>
    ''' <param name="sender">.</param>
    ''' <param name="e">.</param>
    Private Sub OnPaste(ByVal sender As Object, ByVal e As EventArgs)
        MyBase.Paste()
    End Sub


    ''' <summary>
    ''' Event handler for selecting of text.
    ''' </summary>
    ''' <param name="sender">.</param>
    ''' <param name="e">.</param>
    Private Sub OnSelectAll(ByVal sender As Object, ByVal e As EventArgs)
        MyBase.SelectAll()
    End Sub

    ''' <summary>
    ''' Event handler for deleting of text.
    ''' </summary>
    ''' <param name="sender">.</param>
    ''' <param name="e">.</param>
    Private Sub OnDelete(ByVal sender As Object, ByVal e As EventArgs)
        MyBase.SelectedText = String.Empty
    End Sub

#End Region

#Region "Non-Client paint"

    ''' <summary>
    ''' We override WndProc to process the WM_NCPAINT window message.
    ''' </summary>
    ''' <param name="m"></param>
    Protected Overrides Sub WndProc(ByRef m As System.Windows.Forms.Message)
        Const WM_NCPAINT As Integer = &H85
        MyBase.WndProc(m)

        Select Case m.Msg
            Case WM_NCPAINT
                If Application.RenderWithVisualStyles AndAlso Not BorderStyle = System.Windows.Forms.BorderStyle.None Then
                    Dim hDC As IntPtr = GetWindowDC(m.HWnd)
                    ExcludeClipRect(hDC, BorderWidth, BorderWidth, Me.Width - BorderWidth, Me.Height - BorderWidth)
                    Dim bounds As New Rectangle(0, 0, Me.Width, Me.Height)
                    Using args As New PaintEventArgs(Graphics.FromHdc(hDC), bounds)
                        Me.OnNonClientPaint(args)
                    End Using
                End If
        End Select
    End Sub

    <System.Runtime.InteropServices.DllImport("user32.dll")> _
    Private Shared Function GetWindowDC(ByVal hWnd As IntPtr) As IntPtr
    End Function

    <System.Runtime.InteropServices.DllImport("gdi32.dll")> _
    Private Shared Function ExcludeClipRect(ByVal hdc As IntPtr, ByVal nLeftRect As Integer, ByVal nTopRect As Integer, ByVal nRightRect As Integer, ByVal nBottomRect As Integer) As Integer
    End Function
#End Region

    ''' <summary>
    ''' Appends text to the textbox in a given color. 
    ''' (this function will affect selected text)
    ''' </summary>
    ''' <param name="text">The text to append</param>
    ''' <param name="color">The color to set the text to</param>
    Public Overloads Sub AppendText(ByVal text As String, ByVal color As Color)
        Dim start As Integer = TextLength
        AppendText(text)
        Dim finish As Integer = TextLength

        Me.Select(start, finish - start)
        SelectionColor = color
        SelectionLength = 0
    End Sub
End Class
