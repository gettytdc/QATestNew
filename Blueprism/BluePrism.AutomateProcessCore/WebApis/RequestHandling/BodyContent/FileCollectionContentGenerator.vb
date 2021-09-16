Imports System.IO
Imports System.Linq
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent.Multipart

Namespace WebApis.RequestHandling.BodyContent

    Public Class FileCollectionContentGenerator
        Inherits BodyContentGenerator(Of FileCollectionBodyContent)

        Private mMultipartBodyContentBuilder As IMultiPartBodyContentBuilder

        Sub New(builder As IMultiPartBodyContentBuilder)
            mMultipartBodyContentBuilder = builder
        End Sub

        Public Overrides Function GetBodyContent(context As ActionContext,
                                                 content As FileCollectionBodyContent) _
            As IBodyContentResult

            Dim files As clsProcessValue = Nothing

            If Not context.Parameters.TryGetValue(content.FileInputParameterName, files) Then _
                Throw New ArgumentException($"Could not find the '{content.FileInputParameterName}' input parameter", NameOf(context))

            If Not files.HasCollectionData Then Return New EmptyBodyContentResult()

            Dim bodyCollection = New List(Of MultiPartFileBodySection)

            For Each file In files.Collection.Rows

                If Not content.CollectionRowContainsRequiredField(file) Then _
                    Throw New ArgumentException(
                        String.Format(My.Resources.Resources.FileCollectionContentGenerator_TheCollectionUsedInThe0InputParameterDoesNotContainTheFollowingRequiredFields1,
                                        content.FileInputParameterName,
                                        String.Join(My.Resources.Resources.FileCollectionContentGenerator_Comma, content.
                                                            InputSchemaFields.
                                                            RequiredFields.
                                                            Select(Function(x) $"{x.Name} ({x.DataType.ToString()})"))))

                Dim contentBytes = CType(file.GetField("File"), Byte())
                Dim contentType = CType(file.GetField("Content Type"), String)
                Dim fieldName = CType(file.GetField("Field Name"), String)
                Dim fileName = CType(file.GetField("File Name"), String)

                bodyCollection.Add(New MultiPartFileBodySection(fieldName,
                    Path.GetFileName(fileName), contentBytes, contentType))
            Next

            Return mMultipartBodyContentBuilder.Build(bodyCollection, Function() $"Boundary{Guid.NewGuid()}")

        End Function

    End Class

End Namespace
