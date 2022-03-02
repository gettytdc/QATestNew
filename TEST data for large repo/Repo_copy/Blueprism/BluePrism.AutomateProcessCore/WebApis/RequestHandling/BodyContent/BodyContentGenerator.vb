Namespace WebApis.RequestHandling.BodyContent

    Public MustInherit Class BodyContentGenerator _
        (Of TRequestBodyContent As {IBodyContent, Class})
        Implements IBodyContentGenerator

        ''' <inheritdoc/>
        Public Function CanHandle(requestBodyContent As IBodyContent) As Boolean _
            Implements IBodyContentGenerator.CanHandle

            Return TryCast(requestBodyContent, TRequestBodyContent) IsNot Nothing
        End Function

        ''' <inheritdoc />
        Public Function GetBodyContent(context As ActionContext) As IBodyContentResult _
            Implements IBodyContentGenerator.GetBodyContent

            Dim content = CastActionBodyContent(context)
            Return GetBodyContent(context, content)
        End Function


        Public MustOverride Function GetBodyContent(context As ActionContext,
                                                    content As TRequestBodyContent) _
                                                    As IBodyContentResult

        Private Function CastActionBodyContent(context As ActionContext) As TRequestBodyContent
            Dim content = context.Action.Request.BodyContent
            Dim castContent = TryCast(content, TRequestBodyContent)
            If castContent Is Nothing Then
                Throw New ArgumentException(String.Format(My.Resources.Resources.BodyContentGenerator_UnexpectedContentTypeExpected0ButWas1, GetType(TRequestBodyContent).Name, content.GetType().Name))
            End If
            Return castContent
        End Function

    End Class

End Namespace
