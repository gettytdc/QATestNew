Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping

Public Interface IUserMappingErrorLogger
    Sub LogErrors(path As String, errors As List(Of UserMappingResult))
End Interface
