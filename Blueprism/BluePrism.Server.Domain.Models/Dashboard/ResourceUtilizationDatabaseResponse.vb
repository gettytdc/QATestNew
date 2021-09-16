Imports System.Runtime.Serialization

Namespace Dashboard
    <Serializable>
    <DataContract([Namespace]:="bp")>
    Public Class ResourceUtilizationDatabaseResponse
        <IgnoreDataMember>
        Public ResourceId As Guid
        <IgnoreDataMember>
        Public DigitalWorkerName As String
        <IgnoreDataMember>
        Public UtilizationDate As DateTime
        <IgnoreDataMember>
        Public Usage As Integer
    End Class
End NameSpace
