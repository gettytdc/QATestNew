Imports System.Linq

Namespace WebApis.TemplateProcessing

    ''' <summary>
    ''' Replaces parameter placeholders in a string with parameter values. Parameter 
    ''' placeholders consist of the name of the parameter within the specified delimiter 
    ''' characters,  e.g. [Parameter 1]. Parameter names can contain spaces, but there 
    ''' should be no spaces between the delimiters and the parameter names. Placeholders
    ''' containing unrecognised parameters will be replaced with empty values.
    ''' </summary>
    Public Module ParameterInterpolator

        ''' <summary>
        ''' Replaces parameter placeholders in the input string with parameter values
        ''' </summary>
        ''' <param name="template">A string containing parameter placeholders</param>
        ''' <param name="parameters">A collection of parameter values</param>
        ''' <returns>A string containing the template content with parameter values 
        ''' inserted into placeholders</returns>
        Public Function ProcessTemplate(template As String,
                                      parameters As Dictionary(Of String, clsProcessValue)) As String

            Dim tokeniser As New ContentElementLexer("[", "]")
            Dim tokens = tokeniser.Tokenise(template).ToArray()
            Return String.Concat(tokens.Select(Function(t) GetElementContent(t, parameters)))

        End Function

        Private Function GetElementContent(element As ContentElement,
                                         parameters As Dictionary(Of String, clsProcessValue)) As String

            ' Only parameters need special treatment - keeping it simple
            Dim content = GetStaticContent(element)
            If content Is Nothing Then
                content = GetParameterContent(element, parameters)
            End If
            If content Is Nothing Then
                Throw New ArgumentException(String.Format(My.Resources.Resources.ParameterInterpolator_UnexpectedElementType0, element.GetType), NameOf(element))
            End If
            Return content

        End Function

        Private Function GetStaticContent(element As ContentElement) As String

            Dim staticElement = TryCast(element, StaticElement)
            Return staticElement?.Text

        End Function

        Private Function GetParameterContent(element As ContentElement,
                                          parameters As Dictionary(Of String, clsProcessValue)) As String

            Dim parameterElement = TryCast(element, ParameterElement)
            If parameterElement IsNot Nothing Then
                Dim parameterValue As clsProcessValue = Nothing
                parameters.TryGetValue(parameterElement.Name, parameterValue)
                If parameterValue Is Nothing Then Return ""

                Return ParameterValueFormatter.FormatValue(parameterValue)

            Else
                Return Nothing
            End If

        End Function

    End Module
End Namespace