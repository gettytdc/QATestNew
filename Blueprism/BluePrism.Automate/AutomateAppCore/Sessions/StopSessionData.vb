Imports System.Runtime.Serialization

Namespace Sessions
    <Serializable, DataContract(Name:="psd", [Namespace]:="bp")>
    Public Class StopSessionData

        <DataMember(Name:="r", EmitDefaultValue:=False)>
        Private ReadOnly mResourceId As Guid

        <DataMember(Name:="s", EmitDefaultValue:=False)>
        Private ReadOnly mSessionId As Guid

        <DataMember(Name:="schId", EmitDefaultValue:=False)>
        Private ReadOnly mScheduleSessionId As Integer
        

        Public ReadOnly Property ResourceId As Guid
            Get
                Return mResourceId
            End Get
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

        Public Sub New(resourceId As Guid, sessionId As Guid, scheduleSessionId As Integer)
            mResourceId = resourceId
            mSessionId = sessionId
            mScheduleSessionId = scheduleSessionId
        End Sub
    End Class
End Namespace
