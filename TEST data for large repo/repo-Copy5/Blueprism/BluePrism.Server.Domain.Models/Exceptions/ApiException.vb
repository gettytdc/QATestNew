<Serializable()>
Public Class ApiException
        Inherits BluePrismException
        ''' <summary>
        ''' Creates a new exception with no message
        ''' </summary>
        Public Sub New()
            MyBase.New()
        End Sub

        ''' <summary>
        ''' Creates a new exception with the given message.
        ''' </summary>
        ''' <param name="msg">The message.</param>
        Public Sub New(ByVal msg As String)
            MyBase.New(msg)
        End Sub

        ''' <summary>
        ''' Creates a new exception with the given formatted message.
        ''' </summary>
        ''' <param name="formattedMsg">The message, with format placeholders, as defined
        ''' in the <see cref="String.Format"/> method.</param>
        ''' <param name="args">The arguments for the formatted message.</param>
        Public Sub New(ByVal formattedMsg As String, ByVal ParamArray args() As Object)
            MyBase.New(formattedMsg, args)
        End Sub
End Class