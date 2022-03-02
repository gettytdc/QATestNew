Imports System.Runtime.Serialization

Namespace Sessions
    <Serializable, DataContract(Name:="csd", [Namespace]:="bp")>
    Public Class CreateSessionData

        <DataMember(Name:="r", EmitDefaultValue:=False)>
        Private ReadOnly mResourceId As Guid

        <DataMember(Name:="p", EmitDefaultValue:=False)>
        Private ReadOnly mProcessId As Guid

        <DataMember(Name:="q", EmitDefaultValue:=False)>
        Private ReadOnly mQueueIdentifier As Integer

        <DataMember(Name:="t", EmitDefaultValue:=False)>
        Private ReadOnly mTag As Object

        Private mAuthorizationToken As clsAuthToken

        Public ReadOnly Property ResourceId As Guid
            Get
                Return mResourceId
            End Get
        End Property

        Public ReadOnly Property ProcessId As Guid
            Get
                Return mProcessId
            End Get
        End Property

        Public ReadOnly Property QueueIdentifier As Integer

            Get
                Return mQueueIdentifier
            End Get
        End Property

        Public ReadOnly Property Tag As Object
            Get
                Return mTag
            End Get
        End Property

        Public Property AuthorizationToken As clsAuthToken
            Get
                Return mAuthorizationToken
            End Get
            Set(value As clsAuthToken)
                mAuthorizationToken = value
            End Set
        End Property


        Public Property ScheduledSessionId As Integer

        Public Property SessionId As Guid



        Public Sub New(resourceId As Guid, processId As Guid, queueIdentifier As Integer, tag As Object, authToken As clsAuthToken)
            mResourceId = resourceId
            mProcessId = processId
            mQueueIdentifier = queueIdentifier
            mTag = tag
            mAuthorizationToken = authToken
        End Sub
    End Class
End Namespace

