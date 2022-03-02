
Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models.DataFilters
Imports BluePrism.Server.Domain.Models.Pagination
Imports Func

<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class ProcessSessionParameters
    <DataMember(Name:="s", EmitDefaultValue:=False)>
    Public Property SortBy As ProcessSessionSortByProperty
    <DataMember(Name:="i", EmitDefaultValue:=False)>
    Public Property ItemsPerPage As Integer

    <IgnoreDataMember>
    Public Property ProcessName As DataFilter(Of String)
    <IgnoreDataMember>
    Public Property SessionNumber As DataFilter(Of String)
    <IgnoreDataMember>
    Public Property ResourceName As DataFilter(Of String)
    <IgnoreDataMember>
    Public Property User As DataFilter(Of String)
    <IgnoreDataMember>
    Public Property Status As DataFilter(Of SessionStatus)
    <IgnoreDataMember>
    Public Property StartTime As DataFilter(Of DateTimeOffset)
    <IgnoreDataMember>
    Public Property EndTime As DataFilter(Of DateTimeOffset)
    <IgnoreDataMember>
    Public Property LatestStage As DataFilter(Of String)
    <IgnoreDataMember>
    Public Property StageStarted As DataFilter(Of DateTimeOffset)
    <IgnoreDataMember>
    Public Property PagingToken As [Option](Of SessionsPagingToken)
End Class
