Imports System.Runtime.Serialization

Namespace WorkQueues

    <Serializable, DataContract(Namespace:="bp")>
    Public Class ActiveQueueTargetSessionCount
        <DataMember>
        Private mQueueId As Integer
        <DataMember>
        Private mTargetSessionCount As Integer

        Public Property QueueId As Integer
            Get
                Return mQueueId
            End Get
            Set(value As Integer)
                mQueueId = value
            End Set
        End Property

        Public Property TargetSessionCount As Integer
            Get
                Return mTargetSessionCount
            End Get
            Set(value As Integer)
                mTargetSessionCount = value
            End Set
        End Property
    End Class

End Namespace
