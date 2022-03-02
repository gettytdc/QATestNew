Imports BluePrism.AutomateAppCore.Auth

Namespace Commands
    Public Interface ICommand
        ReadOnly Property CommandAuthenticationRequired As CommandAuthenticationMode
        ReadOnly Property Help As String
        ReadOnly Property Name As String
        ReadOnly Property RequiresControlResourcePermission As Boolean
        ReadOnly Property ValidRunStates As IEnumerable(Of ResourcePcRunState)
        Function Execute(command As String) As (output as String, clientsToRemove As IReadOnlyCollection(Of IListenerClient))
        Function CheckPermissions(authUser As IUser, resourceId As Guid) As (errorMessage As String, allowed As Boolean)
    End Interface
End Namespace
