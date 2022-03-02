Imports System.Globalization
Imports System.Linq
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping
Imports CsvHelper

Public Class CsvUserMappingFileReader : Implements IUserMappingFileReader

    Private ReadOnly mStreamReaderFactory As IStreamReaderFactory

    Public Sub New(streamReaderFactory As IStreamReaderFactory)
        mStreamReaderFactory = streamReaderFactory
    End Sub

    Public Function Read(path As String) As List(Of UserMappingRecord) Implements IUserMappingFileReader.Read
        Using streamReader = mStreamReaderFactory.Create(path), csvReader = New CsvReader(streamReader, CultureInfo.InvariantCulture)
            Return csvReader.
                        GetRecords(Of UserMappingCsvRecord).
                        Select(Function(csvRow) csvRow.MapTo()).
                        ToList()
        End Using
    End Function
End Class
