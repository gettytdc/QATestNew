Imports System.Runtime.Serialization

<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class ScheduledSession
    <DataMember(Name:="p", EmitDefaultValue:=False)>
    Public Property ProcessName As String
    <DataMember(Name:="r", EmitDefaultValue:=False)>
    Public Property ResourceName As String
End Class
