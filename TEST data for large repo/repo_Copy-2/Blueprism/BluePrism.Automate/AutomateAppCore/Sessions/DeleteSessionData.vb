Imports System.Runtime.Serialization

Namespace Sessions

    <Serializable, DataContract(Name:="dsd", [Namespace]:="bp")>
    Public Class DeleteSessionData

        <DataMember(Name:="r", EmitDefaultValue:=False)>
        Private ReadOnly mResourceId As Guid

        <DataMember(Name:="p", EmitDefaultValue:=False)>
        Private ReadOnly mProcessId As Guid

        <DataMember(Name:="s", EmitDefaultValue:=False)>
        Private ReadOnly mSessionId As Guid

        <DataMember(Name:="a", EmitDefaultValue:=False)>
        Private mAuthorizationToken As clsAuthToken

        <DataMember(Name:="schId", EmitDefaultValue:=False)>
        Private ReadOnly mScheduleSessionId As Integer

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

        Public Property AuthorizationToken As clsAuthToken
            Get
                Return mAuthorizationToken
            End Get
            Set(value As clsAuthToken)
                mAuthorizationToken = value
            End Set
        End Property

        Public ReadOnly Property SessionId As Guid
            Get
                Return mSessionId
            End Get
        End Property

        Public ReadOnly Property ScheduleSessionId As Integer
            Get
                Return mScheduleSessionId
            End Get
        End Property

        Public Sub New(resourceId As Guid, processId As Guid, sessionId As Guid, authToken As clsAuthToken, scheduleSessionId As Integer)
            mResourceId = resourceId
            mProcessId = processId
            mAuthorizationToken = authToken
            mSessionId = sessionId
            mScheduleSessionId = scheduleSessionId
        End Sub
    End Class
End Namespace
