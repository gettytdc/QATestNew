Imports System.Text
Imports System.IO
Imports System.Runtime.Serialization
Imports System.Linq

Namespace WebApis.RequestHandling.BodyContent.Multipart

    ''' <summary>
    ''' Class that builds the content of a multipart/form-data HTTP request
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class MultiPartBodyContentBuilder
        Implements IMultiPartBodyContentBuilder

        ''' <summary>
        ''' Prefix defined in the multipart/form-data specification
        ''' </summary>
        Private Const BoundaryPrefix = "--"

        Private Function GetBoundary(boundaryId As String) As String
            Return vbCrLf & BoundaryPrefix & boundaryId & vbCrLf
        End Function

        Private Function GetTrailer(boundaryId As String) As String
            Return vbCrLf & BoundaryPrefix & boundaryId & BoundaryPrefix & vbCrLf
        End Function

        ''' <inheritdoc/>
        Public Function Build(sections As IEnumerable(Of MultiPartFileBodySection),
                              generateBoundary As Func(Of String)) As RawDataBodyContentResult _
            Implements IMultiPartBodyContentBuilder.Build

            Dim boundaryId = GetUniqueBoundaryId(sections, generateBoundary)

            Using stream As New MemoryStream()

                For Each section In sections
                    Dim boundaryBytes = Encoding.UTF8.GetBytes(GetBoundary(boundaryId))
                    stream.Write(boundaryBytes, 0, boundaryBytes.Length)

                    Dim headerBytes = Encoding.UTF8.GetBytes(section.Header())
                    stream.Write(headerBytes, 0, headerBytes.Length)

                    stream.Write(section.Content, 0, section.Content.Length)
                Next

                Dim trailerBytes = Encoding.UTF8.GetBytes(GetTrailer(boundaryId))
                stream.Write(trailerBytes, 0, trailerBytes.Length)

                Dim contentTypeHeaders = GetContentTypeHeaders(boundaryId)

                Return New RawDataBodyContentResult(stream.ToArray(), contentTypeHeaders)

            End Using

        End Function

        Private Function GetUniqueBoundaryId(sections As IEnumerable(Of MultiPartFileBodySection),
                                             generateBoundary As Func(Of String)) As String

            Dim boundaryId As String
            Do
                boundaryId = generateBoundary.Invoke()
                If sections.Any(Function(x) SectionContainsBoundary(x, boundaryId)) Then Continue Do
                Exit Do
            Loop

            Return boundaryId

        End Function

        Private Function SectionContainsBoundary(section As MultiPartFileBodySection,
                                                 boundaryId As String) As Boolean
            Return section.Header.Contains(boundaryId) OrElse
                Encoding.UTF8.GetString(section.Content).Contains(boundaryId)
        End Function

        Public Shared Iterator Function GetContentTypeHeaders(boundaryId As String) As IEnumerable(Of HttpHeader)
            Yield New HttpHeader("Content-Type", $"multipart/form-data; boundary={ boundaryId.ToString() }")
        End Function

    End Class

End Namespace
