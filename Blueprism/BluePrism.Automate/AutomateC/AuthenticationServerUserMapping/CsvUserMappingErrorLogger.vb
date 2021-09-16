Imports System.Globalization
Imports System.Linq
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.AuthenticationServerUserMapping
Imports CsvHelper
Imports CsvHelper.Configuration

Namespace AuthenticationServerUserMapping

    Public Class CsvUserMappingErrorLogger
        Implements IUserMappingErrorLogger

        Private ReadOnly mStreamWriterFactory As IStreamWriterFactory

        Public Sub New(streamWriterFactory As IStreamWriterFactory)
            mStreamWriterFactory = streamWriterFactory
        End Sub

        Public Sub LogErrors(path As String, errors As List(Of UserMappingResult)) Implements IUserMappingErrorLogger.LogErrors
            Dim csvConfiguration = New CsvConfiguration(CultureInfo.InvariantCulture, hasHeaderRecord:=False)
            Using writer = mStreamWriterFactory.Create(path), csv = New CsvWriter(writer, csvConfiguration)
                Dim csvRows = errors.Select(Function(mappingError) UserMappingErrorCsvRecord.MapFrom(mappingError))
                csv.WriteRecords(csvRows)
            End Using
        End Sub
    End Class
End NameSpace
