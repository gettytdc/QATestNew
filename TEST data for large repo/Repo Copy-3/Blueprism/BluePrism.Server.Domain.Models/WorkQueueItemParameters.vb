Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models.Pagination
Imports Func
Imports BluePrism.Server.Domain.Models.DataFilters

<Serializable()>
<DataContract([Namespace]:="bp")>
Public Class WorkQueueItemParameters
    <DataMember(Name:="s", EmitDefaultValue:=False)>
    Public Property SortBy As WorkQueueItemSortByProperty
    <IgnoreDataMember>
    Public Property KeyValue As DataFilter(Of String)
    <IgnoreDataMember>
    Public Property Status As DataFilter(Of String)
    <IgnoreDataMember>
    Public Property ExceptionReason As DataFilter(Of String)
    <IgnoreDataMember>
    Public Property WorkTime As DataFilter(Of Integer)
    <IgnoreDataMember>
    Public Property Attempt As DataFilter(Of Integer)
    <IgnoreDataMember>
    Public Property Priority As DataFilter(Of Integer)
    <IgnoreDataMember>
    Public Property LoadedDate As DataFilter(Of DateTimeOffset)
    <IgnoreDataMember>
    Public Property LastUpdated As DataFilter(Of DateTimeOffset)
    <IgnoreDataMember>
    Public Property DeferredDate As DataFilter(Of DateTimeOffset)
    <IgnoreDataMember>
    Public Property LockedDate As DataFilter(Of DateTimeOffset)
    <IgnoreDataMember>
    Public Property CompletedDate As DataFilter(Of DateTimeOffset)
    <IgnoreDataMember>
    Public Property ExceptionedDate As DataFilter(Of DateTimeOffset)
    <IgnoreDataMember>
    Public Property ItemsPerPage As Integer
    <IgnoreDataMember>
    Public Property PagingToken As [Option](Of WorkQueueItemPagingToken)
End Class
