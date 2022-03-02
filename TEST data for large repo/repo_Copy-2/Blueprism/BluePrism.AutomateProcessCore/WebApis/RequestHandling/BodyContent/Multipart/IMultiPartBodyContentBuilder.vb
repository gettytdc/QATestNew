Namespace WebApis.RequestHandling.BodyContent.Multipart

    Public Interface IMultiPartBodyContentBuilder

        ''' <summary>
        ''' Build a multipart/form-data body content from the supplied collection
        ''' of <see cref="MultiPartFileBodySection"/>
        ''' </summary>
        ''' <param name="sections">Collection of body sections to build the 
        ''' body content</param>
        ''' <param name="generateBoundary">Function used to generate a new random boundary Id</param>
        Function Build(sections As IEnumerable(Of MultiPartFileBodySection),
                       generateBoundary As Func(Of String)) As RawDataBodyContentResult

    End Interface

End Namespace
