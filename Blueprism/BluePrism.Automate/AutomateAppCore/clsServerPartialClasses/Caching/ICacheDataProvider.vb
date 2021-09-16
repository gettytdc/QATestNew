Imports BluePrism.Data

Namespace clsServerPartialClasses.Caching
    Public Interface ICacheDataProvider
        Function GetAllGroupPermissions(connection As IDatabaseConnection) As IReadOnlyDictionary(Of String, IGroupPermissions)
        Function GetProcessGroups(connection As IDatabaseConnection) As IReadOnlyDictionary(Of Guid, List(Of Guid))
        Function GetResourceGroups(connection As IDatabaseConnection) As IReadOnlyDictionary(Of Guid, List(Of Guid))
        Function IsMIReportingEnabled(connection As IDatabaseConnection) As Boolean
    End Interface

End Namespace