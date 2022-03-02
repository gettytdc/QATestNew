Imports AutomateC.My.Resources
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping
Imports CsvHelper.Configuration.Attributes
Public Class UserMappingErrorCsvRecord
    <Index(0)>
    Public Property BluePrismUsername As String

    <Index(1)>
    Public Property AuthenticationServerUserId As Guid?

    <Index(2)>
    Public Property FirstName As String

    <Index(3)>
    Public Property LastName As String

    <Index(4)>
    Public Property Email As String

    <Index(5)>
    Public Property ErrorDescription As String

    Public Shared Function MapFrom(userMappingResult As UserMappingResult) As UserMappingErrorCsvRecord
        Return New UserMappingErrorCsvRecord() With {
            .BluePrismUsername = userMappingResult.Record.BluePrismUsername,
            .AuthenticationServerUserId = userMappingResult.Record.AuthenticationServerUserId,
            .FirstName = userMappingResult.Record.FirstName,
            .LastName = userMappingResult.Record.LastName,
            .Email = userMappingResult.Record.Email,
            .ErrorDescription = userMappingResult.ResultCode.ToLocalizedDescription()
        }
    End Function

End Class
