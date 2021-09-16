
Imports BluePrism.Core.Resources

Namespace Resources
    Public Interface IUserAuthResourceConnectionManager
        Inherits IResourceConnectionManager
        Function GetAllResourceConnectionStatistics() As IDictionary(Of Guid, ResourceConnectionStatistic)
        Sub SendGetSessionVariablesAsUser(token As clsAuthToken, resourceId As Guid, sessionID As Guid)
    End Interface


End Namespace
