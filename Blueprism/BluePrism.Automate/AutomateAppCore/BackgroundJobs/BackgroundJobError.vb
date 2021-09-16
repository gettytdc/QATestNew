Imports System.Runtime.Serialization

Namespace BackgroundJobs

    ''' <summary>
    ''' Contains details of exception thrown when running a background job
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class BackgroundJobError

        <DataMember>
        Private ReadOnly mMessage As String

        <DataMember>
        Private ReadOnly mDetails As String

        ''' <summary>
        ''' Creates a new BackgroundJobError
        ''' </summary>
        ''' <param name="message">A summary of the error</param>
        ''' <param name="details">Full details of the error</param>
        Public Sub New(message As String, details As String)
            mMessage = message
            mDetails = details
        End Sub

        ''' <summary>
        ''' Creates a new BackgroundJobError
        ''' </summary>
        ''' <param name="exception">The exception on which the details wll be based</param>
        Public Sub New(exception As Exception)
            If exception Is Nothing Then
                Throw New ArgumentNullException(NameOf(exception))
            End If
            mMessage = exception.Message
            mDetails = exception.ToString
        End Sub

        ''' <summary>
        ''' A summary of the error from the Message property of the original exception
        ''' </summary>
        Public ReadOnly Property Message as String
            Get
                return mMessage
            End Get
        End Property

        ''' <summary>
        ''' Full details of the error from the ToString function of the original exception
        ''' </summary>
        Public ReadOnly Property Details as String
            Get
                return mDetails
            End Get
        End Property

    End Class
End NameSpace