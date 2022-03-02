
Imports System.Runtime.Serialization

''' <summary>
''' Exception fired when an active queue lock failed.
''' This is here mainly to ensure that the localization of the date occurs on the
''' client-side (ie. when <see cref="Exception.Message">Message</see> is retrieved),
''' not on the server-side (ie. when the exception is created).
''' </summary>
<Serializable>
Public Class ActiveQueueLockFailedException : Inherits LockFailedException
    ' The name of the affected queue
    Private mQueueName As String

    ' The name associated with the queue's current lock
    Private mLockName As String

    ' The (UTC) time registered on the lock, or MinValue if no time is registered
    Private mLockTime As Date

    ''' <summary>
    ''' Constructor required to deserialize the exception
    ''' </summary>
    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
        mQueueName = info.GetString(NameOf(mQueueName))
        mLockName = info.GetString(NameOf(mLockName))
        mLockTime = info.GetDateTime(NameOf(mLockTime))
    End Sub

    Public Overrides Sub GetObjectData(info As SerializationInfo, ctx As StreamingContext)
        MyBase.GetObjectData(info, ctx)
        info.AddValue(NameOf(mQueueName), mQueueName)
        info.AddValue(NameOf(mLockName), mLockName)
        info.AddValue(NameOf(mLockTime), mLockTime)
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given args
    ''' </summary>
    ''' <param name="queueName">The name of the queue</param>
    ''' <param name="lockName">The name associated with the lock</param>
    ''' <param name="lockTime">The date/time associated with the lock, or
    ''' <see cref="DateTime.MinValue"/> if there is no such time</param>
    Public Sub New(queueName As String, lockName As String, lockTime As Date)
        mQueueName = queueName
        mLockName = lockName
        mLockTime = lockTime
    End Sub

    ''' <summary>
    ''' Gets the message explaining the details of the exception
    ''' </summary>
    Public Overrides ReadOnly Property Message As String
        Get
            Return String.Format(
                My.Resources.ActiveQueueLockFailedException_TheActiveQueue0IsAlreadyLockedUnderTheName1LastSeenAt2,
                If(mQueueName, "?"),
                If(mLockName, "?"),
                If(mLockTime = Date.MinValue, "?", mLockTime.ToLocalTime().ToString())
                )
        End Get
    End Property

End Class
