Imports System.Runtime.Serialization

Namespace Sessions

    <Serializable, DataContract(Name:="ssd", [Namespace]:="bp")>
    Public Class StartSessionData

        <DataMember(Name:="r", EmitDefaultValue:=False)>
        Private ReadOnly mResourceId As Guid

        <DataMember(Name:="p", EmitDefaultValue:=False)>
        Private ReadOnly mProcessId As Guid

        <DataMember(Name:="s", EmitDefaultValue:=False)>
        Private ReadOnly mSessionId As Guid

        <DataMember(Name:="a", EmitDefaultValue:=False)>
        Private mAuthorizationToken As clsAuthToken

        <DataMember(Name:="i", EmitDefaultValue:=False)>
        Private ReadOnly mInputParamsXML As String

        <DataMember(Name:="t", EmitDefaultValue:=False)>
        Private ReadOnly mTag As Object

        <DataMember(Name:="schId", EmitDefaultValue:=False)>
        Private ReadOnly mScheduledSessionId As Integer


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

        Public ReadOnly Property InputParametersXML As String
            Get
                Return mInputParamsXML
            End Get
        End Property

        Public ReadOnly Property SessionId As Guid
            Get
                Return mSessionId
            End Get
        End Property

        Public ReadOnly Property Tag As Object
            Get
                Return mTag
            End Get
        End Property

        Public ReadOnly Property ScheduledSessionId As Integer
            Get
                Return mScheduledSessionId
            End Get
        End Property

        Public Sub New(resourceId As Guid, processId As Guid, sessionId As Guid, inputParametersXML As String, tag As Object, authToken As clsAuthToken, scheduledSessionId As Integer)
            mResourceId = resourceId
            mProcessId = processId
            mAuthorizationToken = authToken
            mSessionId = sessionId
            mInputParamsXML = inputParametersXML
            mTag = tag
            mScheduledSessionId = scheduledSessionId
        End Sub
    End Class
End Namespace
