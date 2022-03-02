Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping
Imports CsvHelper.Configuration.Attributes

Public Class UserMappingCsvRecord
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

    Public Function MapTo() As UserMappingRecord
        Return New UserMappingRecord(BluePrismUsername, AuthenticationServerUserId, FirstName, LastName, Email)
    End Function

End Class
