Imports System.Runtime.Serialization

Namespace BackgroundJobs

    ''' <summary>
    ''' Contains details of progress of a background job running on the server
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class BackgroundJobData

        ''' <summary>
        ''' A BackgroundJobData instance representing an unknown job
        ''' </summary>
        Public Shared ReadOnly Unknown As New BackgroundJobData(BackgroundJobStatus.Unknown, 0, "", DateTime.MinValue)

        <DataMember>
        Private ReadOnly mStatus As BackgroundJobStatus

        <DataMember>
        Private ReadOnly mPercentComplete As Integer

        <DataMember>
        Private ReadOnly mDescription As String

        <DataMember>
        Private ReadOnly mDate As Date

        <DataMember>
        Private ReadOnly mError As BackgroundJobError

        <DataMember>
        Private ReadOnly mResultData As Object

        ''' <summary>
        ''' Creates a new instance of BackgroundJobData
        ''' </summary>
        ''' <param name="status"></param>
        ''' <param name="percentComplete"></param>
        ''' <param name="description"></param>
        ''' <param name="[date]"></param>
        ''' <param name="error"></param>
        ''' <param name="resultData"></param>
        Public Sub New(status As BackgroundJobStatus, percentComplete As Integer, description As String,
                [date] As DateTime, Optional [error] As BackgroundJobError = Nothing, Optional resultData As Object = Nothing)
            mStatus = status
            mPercentComplete = percentComplete
            mDescription = description
            mDate = [date]
            mError = [error]
            mResultData = resultData
        End Sub

        ''' <summary>
        ''' Current status of the job
        ''' </summary>
        Public ReadOnly Property Status as BackgroundJobStatus
            Get
                return mStatus
            End Get
        End Property

        ''' <summary>
        ''' A value between 1 and 100 indicating progress
        ''' </summary>
        Public ReadOnly Property PercentComplete as Integer
            Get
                return mPercentComplete
            End Get
        End Property

        ''' <summary>
        ''' A description of the current activity on the job
        ''' </summary>
        Public ReadOnly Property Description as String
            Get
                return mDescription
            End Get
        End Property

        ''' <summary>
        ''' The UTC date on which this information was updated
        ''' </summary>
        Public ReadOnly Property [Date] as Date
            Get
                Return mDate
            End Get
        End Property

        ''' <summary>
        ''' An error that occured that caused the background job to fail. This may be
        ''' available on jobs with a Failed status.
        ''' </summary>
        Public ReadOnly Property [Error] as BackgroundJobError
            Get
                Return mError
            End Get
        End Property

        ''' <summary>
        ''' Indicates whether the job has completed with either success or failure
        ''' </summary>
        Public ReadOnly Property IsComplete As Boolean
            Get
                Return Status = BackgroundJobStatus.Success _
                    OrElse Status = BackgroundJobStatus.Failure
            End Get
        End Property

        ''' <summary>
        ''' Arbitrary data that is made available during running or completion of the job.
        ''' This is suitable for providing further details about the result of the job, such
        ''' as the id of any objects created.
        ''' </summary>
        Public ReadOnly Property ResultData as Object
            Get
                return mResultData
            End Get
        End Property
    End Class
End NameSpace
