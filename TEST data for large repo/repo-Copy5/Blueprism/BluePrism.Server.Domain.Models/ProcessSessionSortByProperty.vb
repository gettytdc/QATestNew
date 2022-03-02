Imports BluePrism.Server.Domain.Models.Attributes

Public Enum ProcessSessionSortByProperty
   <ColumnNameSortBy(ColumnName:="sessionnumber", SortDirection:="asc") >
    SessionNumberAsc = 0
    <ColumnNameSortBy(ColumnName:="sessionnumber", SortDirection:="desc") >
    SessionNumberDesc = 1
    <ColumnNameSortBy(ColumnName:="processname", SortDirection:="asc") >
    ProcessNameAsc = 2
    <ColumnNameSortBy(ColumnName:="processname", SortDirection:="desc") >
    ProcessNameDesc = 3
    <ColumnNameSortBy(ColumnName:="starterresourcename", SortDirection:="asc") >
    ResourceNameAsc = 4
    <ColumnNameSortBy(ColumnName:="starterresourcename", SortDirection:="desc") >
    ResourceNameDesc = 5
    <ColumnNameSortBy(ColumnName:="starterusername", SortDirection:="asc") >
    UserAsc = 6
    <ColumnNameSortBy(ColumnName:="starterusername", SortDirection:="desc") >
    UserDesc = 7
    <ColumnNameSortBy(ColumnName:="statusid", SortDirection:="asc") >
    StatusAsc = 8
    <ColumnNameSortBy(ColumnName:="statusid", SortDirection:="desc") >
    StatusDesc = 9
    <ColumnNameSortBy(ColumnName:="exceptiontype", SortDirection:="asc") >
    ExceptionTypeAsc = 10
    <ColumnNameSortBy(ColumnName:="exceptiontype", SortDirection:="desc") >
    ExceptionTypeDesc = 11
    <ColumnNameSortBy(ColumnName:="dateadd(SECOND, -starttimezoneoffset, startdatetime)", SortDirection:="asc") >
    StartTimeAsc = 12
    <ColumnNameSortBy(ColumnName:="dateadd(SECOND, -starttimezoneoffset, startdatetime)", SortDirection:="desc") >
    StartTimeDesc = 13
    <ColumnNameSortBy(ColumnName:="dateadd(SECOND, -endtimezoneoffset, enddatetime)", SortDirection:="asc") >
    EndTimeAsc = 14
    <ColumnNameSortBy(ColumnName:="dateadd(SECOND, -endtimezoneoffset, enddatetime)", SortDirection:="desc") >
    EndTimeDesc = 15
    <ColumnNameSortBy(ColumnName:="laststage", SortDirection:="asc") >
    LatestStageAsc = 16
    <ColumnNameSortBy(ColumnName:="laststage", SortDirection:="desc") >
    LatestStageDesc = 17
    <ColumnNameSortBy(ColumnName:="dateadd(SECOND, -lastupdatedtimezoneoffset, lastupdated)", SortDirection:="asc") >
    StageStartedAsc = 18
    <ColumnNameSortBy(ColumnName:="dateadd(SECOND, -lastupdatedtimezoneoffset, lastupdated)", SortDirection:="desc") >
    StageStartedDesc = 19
End Enum
