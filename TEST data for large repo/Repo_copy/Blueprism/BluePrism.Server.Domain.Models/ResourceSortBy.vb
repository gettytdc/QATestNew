Imports BluePrism.Server.Domain.Models.Attributes

Public Enum ResourceSortBy
    <ColumnNameSortBy(ColumnName:="r.name", SortDirection:="asc")>
    NameAscending = 0
    <ColumnNameSortBy(ColumnName:="r.name", SortDirection:="desc")>
    NameDescending = 1
    <ColumnNameSortBy(ColumnName:="poolRes.name", SortDirection:="asc")>
    PoolNameAscending = 2
    <ColumnNameSortBy(ColumnName:="poolRes.name", SortDirection:="desc")>
    PoolNameDescending = 3
    <ColumnNameSortBy(ColumnName:="g.name", SortDirection:="asc")>
    GroupNameAscending = 4
    <ColumnNameSortBy(ColumnName:="g.name", SortDirection:="desc")>
    GroupNameDescending = 5
    <ColumnNameSortBy(ColumnName:="(r.processesrunning - r.actionsrunning)", SortDirection:="asc")>
    PendingCountAscending = 6
    <ColumnNameSortBy(ColumnName:="(r.processesrunning - r.actionsrunning)", SortDirection:="desc")>
    PendingCountDescending = 7
    <ColumnNameSortBy(ColumnName:="r.actionsrunning", SortDirection:="asc")>
    ActiveCountAscending = 8
    <ColumnNameSortBy(ColumnName:="r.actionsrunning", SortDirection:="desc")>
    ActiveCountDescending = 9
    <ColumnNameSortBy(ColumnName:="s.displayStatusOrder", SortDirection:="asc")>
    DisplayStatusAscending = 10
    <ColumnNameSortBy(ColumnName:="s.displayStatusOrder", SortDirection:="desc")>
    DisplayStatusDescending = 11
End Enum
