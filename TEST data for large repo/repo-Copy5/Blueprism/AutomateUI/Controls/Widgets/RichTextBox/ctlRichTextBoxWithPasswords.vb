

''' Project  : Automate
''' Class    : RichTextBoxWithPasswords
''' 
''' <summary>
''' Extends the richtextbox class by adding support for password characters
''' analogous to ordinary textboxes.
''' 
''' </summary>
Public Class ctlRichTextBoxWithPasswords
    Inherits ctlRichTextBox

    ''' <summary>
    ''' Private member to store public property PasswordChar
    ''' </summary>
    Private mPasswordChar As Char
    ''' <summary>
    ''' The password character.
    ''' </summary>
    ''' <value>The value</value>
    Public Property PasswordChar() As Char
        Get
            Return mPasswordChar
        End Get
        Set(ByVal Value As Char)
            If Not Me.mPasswordChar = Value Then
                Me.mPasswordChar = Value
            End If
        End Set

    End Property

    ''' <summary>
    ''' Boolean to show whether we are changing the text and therefore whether
    ''' we should ignore the textchanged event.
    ''' </summary>
    Private mbChanging As Boolean

    ''' <summary>
    ''' Event handler for text changed. Replaces text with masked characters if
    ''' needed.
    ''' </summary>
    ''' <param name="sender">sender</param>
    ''' <param name="e">e</param>
    Protected Sub TextHasBeenChanged(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.TextChanged
        If (Not Me.mbChanging) AndAlso (Not Me.mPasswordChar = Nothing) Then
            Dim s As String = ""
            For i As Integer = 1 To MyBase.Text.Length
                s &= Me.mPasswordChar
            Next

            Me.Tag = MyBase.Text
            Me.mbChanging = True
            Dim iSelectionStart As Integer = Me.SelectionStart
            If iSelectionStart < 0 Then iSelectionStart = 0
            MyBase.Text = s
            Me.SelectionStart = iSelectionStart
            Me.mbChanging = False
        End If
    End Sub

    ''' <summary>
    ''' Overrides Text() property in base class. Changes result in character
    ''' masking in needs be.
    ''' </summary>
    ''' <value>Value</value>
    Public Overloads Overrides Property Text() As String
        Get
            Return Me.Text(False)
        End Get
        Set(ByVal Value As String)
            Me.Text(False) = Value
        End Set
    End Property

    ''' <summary>
    ''' Overloads Text() property. Allows for masking of characters to be suppressed.
    ''' This is useful if you have already masked the characters yourself.
    ''' </summary>
    ''' <param name="SuppressMasking">Set to true to turn off character masking.
    ''' </param>
    ''' <value>Value</value>
    Public Overloads Property Text(ByVal SuppressMasking As Boolean) As String
        Get

            If (Not Me.mPasswordChar = Nothing) Then
                If Not Me.Tag Is Nothing Then
                    Return Me.Tag.ToString
                Else
                    Return ""
                End If
            Else
                Return MyBase.Text
            End If
        End Get
        Set(ByVal Value As String)
            Me.mbChanging = SuppressMasking
            MyBase.Text = Value
            Me.mbChanging = False
        End Set
    End Property

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.ResumeLayout(False)
        Me.ImeMode = ImeMode.Off

    End Sub
End Class
