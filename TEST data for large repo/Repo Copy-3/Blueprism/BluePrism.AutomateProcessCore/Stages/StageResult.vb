Imports LocaleTools

Namespace Stages

    ''' <summary>
    ''' Class used to store the results of executing a stage.
    ''' </summary>
    Public Class StageResult

        ''' <summary>
        ''' Static value indicating an 'OK' result - ie. a successful result.
        ''' </summary>
        Public Shared ReadOnly OK As New StageResult(True, Nothing, Nothing)

        ''' <summary>
        ''' Creates a result object representing an internal error from the specified
        ''' error message
        ''' </summary>
        ''' <param name="msg">The error message</param>
        ''' <returns>A Result object wrapping the internal error.</returns>
        Public Shared Function InternalError(ByVal msg As String) As StageResult
            Return New StageResult(False, "Internal", msg)
        End Function

        ''' <summary>
        ''' Creates a result object representing an internal error from the specified
        ''' error message and arguments
        ''' </summary>
        ''' <param name="msg">The error message with format placeholders</param>
        ''' <param name="args">The arguments detailing the error.</param>
        ''' <returns>A Result object wrapping the internal error.</returns>
        Public Shared Function InternalError(
         ByVal msg As String, ByVal ParamArray args() As Object) As StageResult
            Return New StageResult(False, "Internal", String.Format(msg, args))
        End Function

        ''' <summary>
        ''' Creates a result object representing an internal error from the specified
        ''' exception.
        ''' </summary>
        ''' <param name="ex">The exception from which the error should be drawn.
        ''' </param>
        ''' <returns>A Result object representing an internal exception.</returns>
        Public Shared Function InternalError(ByVal ex As Exception) As StageResult
            Return New StageResult(False, "Internal", ex.Message)
        End Function

        ''' <summary>
        ''' Constructor
        ''' </summary>
        ''' <param name="success">True if the execution succeeded, False otherwise.</param>
        ''' <param name="exType">The exception type - relevant only if the execution
        ''' failed.</param>
        ''' <param name="exDetail">The exception detail - relevant only if the
        ''' execution failed.</param>
        Public Sub New(ByVal success As Boolean, Optional ByVal exType As String = Nothing, Optional ByVal exDetail As String = Nothing)
            mSuccess = success
            mExceptionType = exType
            mExceptionDetail = exDetail
        End Sub

        ''' <summary>
        ''' True if the execution succeeded, False otherwise.
        ''' </summary>
        Public ReadOnly Property Success() As Boolean
            Get
                Return mSuccess
            End Get
        End Property
        Private mSuccess As Boolean

        ''' <summary>
        ''' The exception type - relevant only if the execution failed.
        ''' </summary>
        Public ReadOnly Property ExceptionType() As String
            Get
                Return mExceptionType
            End Get
        End Property
        Private mExceptionType As String

        ''' <summary>
        ''' The exception detail - relevant only if the execution failed.
        ''' </summary>
        Public ReadOnly Property ExceptionDetail() As String
            Get
                Return mExceptionDetail
            End Get
        End Property
        Private mExceptionDetail As String

        ''' <summary>
        ''' Get an error string which contains both the type and detail that are
        ''' encapsulated within this instance. If the instance doesn't represent
        ''' an error condition, an empty string is returned.
        ''' </summary>
        ''' <returns>The text.</returns>
        Public Function GetText() As String
            If mSuccess Then Return ""

            If String.IsNullOrWhiteSpace(mExceptionType) Then
                mExceptionType = New Exception().GetType().Name
            End If
            Return mExceptionType & My.Resources.Resources.Result_Colon & mExceptionDetail
        End Function
    End Class

End Namespace