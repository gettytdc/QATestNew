Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping

Public Interface IUserMappingFileReader
    Function Read(path As String) As List(Of UserMappingRecord)
End Interface
