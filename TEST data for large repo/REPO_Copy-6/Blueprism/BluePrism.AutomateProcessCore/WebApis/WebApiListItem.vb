Imports System.Runtime.Serialization

Namespace WebApis

    <Serializable, DataContract([Namespace]:="bp")>
    Public Class WebApiListItem

        <DataMember>
        Public Property Id As Guid

        <DataMember>
        Public Property Enabled As Boolean

        <DataMember>
        Public Property Name As String

        <DataMember>
        Public Property LastUpdated As DateTime

        <DataMember>
        Public Property NumberOfActions As Integer

        <DataMember>
        Public Property AssociatedSkills As IList(Of String)

    End Class

End Namespace