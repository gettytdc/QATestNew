Imports System.Runtime.Serialization

<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class ScheduledTask
    <DataMember(Name:="id", EmitDefaultValue:=False)>
    Public Property Id As Integer
    <DataMember(Name:="n", EmitDefaultValue:=False)>
    Public Property Name As String
    <DataMember(Name:="d", EmitDefaultValue:=False)>
    Public Property Description As String
    <DataMember(Name:="f", EmitDefaultValue:=False)>
    Public Property FailFastOnError As Boolean
    <DataMember(Name:="d", EmitDefaultValue:=False)>
    Public Property DelayAfterEnd As Integer
    <DataMember(Name:="osid", EmitDefaultValue:=False)>
    Public Property OnSuccessTaskId As Integer
    <DataMember(Name:="osn", EmitDefaultValue:=False)>
    Public Property OnSuccessTaskName As String
    <DataMember(Name:="ofid", EmitDefaultValue:=False)>
    Public Property OnFailureTaskId As Integer
    <DataMember(Name:="ofn", EmitDefaultValue:=False)>
    Public Property OnFailureTaskName As String
End Class

