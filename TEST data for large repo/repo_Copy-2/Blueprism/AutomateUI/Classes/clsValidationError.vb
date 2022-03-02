
''' <summary>
''' Class to represent a single validation error.
''' </summary>
Public Class clsValidationError

    ' The error message
    Private _message As String

    ' The affected control, if there is one
    Private _control As Control

    ''' <summary>
    ''' Creates a new validation error with the given message and which
    ''' affects the specified control
    ''' </summary>
    ''' <param name="ctrl">The control which is affected by this validation
    ''' error.</param>
    ''' <param name="formatMsg">The message for this validation error with any
    ''' formatting required.</param>
    ''' <param name="args">The arguments for the formatted message.</param>
    Public Sub New( _
     ByVal ctrl As Control, ByVal formatMsg As String, ByVal ParamArray args() As Object)
        _message = String.Format(formatMsg, args)
        _control = ctrl
    End Sub

    ''' <summary>
    ''' Creates a new validation error with the given message and no control
    ''' </summary>
    ''' <param name="formatMsg">The message for this validation error with any
    ''' formatting required.</param>
    ''' <param name="args">The arguments for the formatted message.</param>
    Public Sub New(ByVal formatMsg As String, ByVal ParamArray args() As Object)
        Me.New(Nothing, formatMsg, args)
    End Sub

    ''' <summary>
    ''' The error message indicating what caused the validation error.
    ''' </summary>
    Public ReadOnly Property Message() As String
        Get
            Return _message
        End Get
    End Property

    ''' <summary>
    ''' The control whose data caused the validation error.
    ''' </summary>
    Public ReadOnly Property Control() As Control
        Get
            Return _control
        End Get
    End Property

End Class
