Imports System.Globalization
Imports CsvHelper

Public Class CsvUserTemplateFileWriter : Implements ICsvUserTemplateFileWriter

    Private ReadOnly mStreamWriterFactory As IStreamWriterFactory

    Public Sub New(streamReaderFactory As IStreamWriterFactory)
        mStreamWriterFactory = streamReaderFactory
    End Sub

    Public Sub Write(template As CsvUserTemplate) Implements ICsvUserTemplateFileWriter.Write
        Using streamWriter = mStreamWriterFactory.Create(template.Path),
              csvWriter = New CsvWriter(streamWriter,
                                        New Configuration.CsvConfiguration(CultureInfo.InvariantCulture, hasHeaderRecord:=False))
            For Each field In template.HeadingFields
                csvWriter.WriteField(field)
            Next
            csvWriter.NextRecord
            csvWriter.WriteRecords(template.UserMapping)
        End Using
    End Sub
End Class
