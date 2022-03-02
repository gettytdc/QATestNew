''' Project  : AutomateAppCore
''' Class    : OperationCompletedEventArgs
''' <summary>
''' Event args class for an operation being completed.
''' </summary>
Public Class OperationCompletedEventArgs : Inherits EventArgs

    ' Flag indicating success. False implies either Failed or Cancelled.
    Private mSuccess As Boolean

    ' Flag indicating the operation was cancelled
    Private mCancelled As Boolean

    ' The exception raised - non-null indicates a failed operation
    Private mError As Exception

    ''' <summary>
    ''' Creates a new event args object with either sucess or cancelled status.
    ''' </summary>
    ''' <param name="cancelled">True to indicate the event that this object describes
    ''' is a cancelled operation; False to indicate that it was a sucessful operation.
    ''' </param>
    Public Sub New(ByVal cancelled As Boolean)
        mSuccess = Not cancelled
        mCancelled = cancelled
    End Sub

    ''' <summary>
    ''' Creates a new event args object with a failed status. The exception indicates
    ''' the manner of the failure.
    ''' </summary>
    ''' <param name="ex">The exception which caused the failure of the operation.
    ''' </param>
    ''' <exception cref="ArgumentNullException">If the given exception was null - 
    ''' the exception is used internally to decide the status of the operation, so
    ''' to indicate failure a non-null exception is required.</exception>
    Public Sub New(ByVal ex As Exception)
        If ex Is Nothing Then Throw New ArgumentNullException(NameOf(ex))
        mSuccess = False
        mError = ex
    End Sub

    ''' <summary>
    ''' Flag indicating the successful completion of an operation. This will be false
    ''' if the operation failed or was cancelled.
    ''' </summary>
    Public ReadOnly Property Success() As Boolean
        Get
            Return mSuccess
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating the cancellation of an operation. This will be false if the
    ''' operation completed successfully, or failed.
    ''' </summary>
    Public ReadOnly Property Cancelled() As Boolean
        Get
            Return mCancelled
        End Get
    End Property

    ''' <summary>
    ''' Exception describing the failure reason for the operation. This will be null
    ''' if the operation completed successfully or was cancelled.
    ''' </summary>
    Public ReadOnly Property [Error]() As Exception
        Get
            Return mError
        End Get
    End Property
End Class

