Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Server.Domain.Models.Pagination
Imports Func

<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class ResourceParameters
    <DataMember(Name:="s", EmitDefaultValue:=False)>
    Public Property SortBy As ResourceSortBy

    <IgnoreDataMember>
    Public Property Name As DataFilter(Of String)
    <IgnoreDataMember>
    Public Property GroupName As DataFilter(Of String)
    <IgnoreDataMember>
    Public Property PoolName As DataFilter(Of String)
    <IgnoreDataMember>
    Public Property ActiveSessionCount As DataFilter(Of Integer)
    <IgnoreDataMember>
    Public Property PendingSessionCount As DataFilter(Of Integer)
    <IgnoreDataMember>
    Public Property DisplayStatus As DataFilter(Of ResourceDisplayStatus)
    <IgnoreDataMember>
    Public Property ItemsPerPage As Integer
    <IgnoreDataMember>
    Public Property PagingToken As [Option](Of ResourcePagingToken)

    Public Overrides Function Equals(obj As Object) As Boolean
        Return TypeOf obj Is ResourceParameters AndAlso Equals(CType(obj, ResourceParameters))
    End Function

    Protected Overloads Function Equals(other As ResourceParameters) As Boolean
        Return _
            other.SortBy = SortBy AndAlso
            other.ItemsPerPage = ItemsPerPage AndAlso
            other.PagingToken.Equals(PagingToken)
    End Function
End Class
