Imports System.Linq
Imports System.Net
Imports BluePrism.AutomateProcessCore.WebApis.CustomCode
Imports BluePrism.Core.Utility
Imports BluePrism.Utilities.Functional
Imports Newtonsoft.Json.Linq

Namespace WebApis.RequestHandling

    ''' <summary>
    ''' Maps values in the response to output parameters when executing a Web 
    ''' API action
    ''' </summary>
    Public Class OutputParameterMapper

        Private ReadOnly mBuilder As ICustomCodeBuilder

        Sub New(builder As ICustomCodeBuilder)
            mBuilder = builder
        End Sub

        ''' <summary>
        ''' Creates output arguments based on the response to an HTTP request executed 
        ''' for a Web API action. If the response within the <see cref="HttpRequestData"/>
        ''' object is nothing, then parameters with empty values are returned.
        ''' </summary>
        ''' <param name="responseData">Contains the response data</param>
        ''' <param name="includeRequestData">Controls whether the Request Content 
        ''' parameter, which includes full details of the request is included</param>
        ''' <returns>A collection containing the output arguments</returns>
        Public Function CreateParameters(responseData As HttpResponseData,
                                         includeRequestData As Boolean,
                                         context As ActionContext) As clsArgumentList
            Dim arguments = CreateArguments(responseData, includeRequestData, context)
            Return New clsArgumentList(arguments)

        End Function

        Private Function CreateArguments(responseData As HttpResponseData,
                                           includeRequestData As Boolean,
                                           context As ActionContext
                                           ) As IEnumerable(Of clsArgument)
            Dim responseBody = If(responseData.Response IsNot Nothing,
                                  responseData.Response.GetResponseBodyAsString(),
                                  Nothing)
            Dim builtInArguments = CreateBuiltInArguments(responseData, responseBody, includeRequestData)
            Dim jsonPathArguments = CreateJsonPathArguments(responseBody, context)
            Dim customCodeArguments = CreateCustomCodeArguments(responseBody, context)
            Return builtInArguments.Concat(jsonPathArguments).Concat(customCodeArguments)
        End Function

        Private Iterator Function CreateBuiltInArguments(responseData As HttpResponseData,
                                                                    responseBody As String,
                                                                    includeRequestData As Boolean) _
            As IEnumerable(Of clsArgument)
            ' Note that response will be null if request not made
            Yield New clsArgument(OutputParameters.ResponseHeaders, GetHeaderTableFromHeaders(responseData.Response?.Headers))
            Yield New clsArgument(OutputParameters.StatusCode, If(CType(responseData.Response?.StatusCode, Nullable(Of Int32))?.ToString(), ""))
            Yield New clsArgument(OutputParameters.ResponseContent, If(responseBody, ""))

            If includeRequestData Then _
                Yield New clsArgument(OutputParameters.RequestData, GetRequestDataParameterValue(responseData.RequestData))
        End Function

        Private Function CreateJsonPathArguments(responseBody As String,
                                                   context As ActionContext) _
                                                   As IEnumerable(Of clsArgument)

            Dim jsonParameters = context.
                                    Action.
                                    OutputParameterConfiguration.
                                    Parameters.
                                    OfType(Of JsonPathOutputParameter)

            If Not jsonParameters.Any() Then Return Enumerable.Empty(Of clsArgument)

            Dim responseAsJson As JToken
            If responseBody Is Nothing Then
                responseAsJson = Nothing
            Else
                Try
                    responseAsJson = JToken.Parse(responseBody)
                Catch ex As Exception
                    Throw New InvalidOperationException(WebApiResources.ErrorParsingJson)
                End Try
                If responseAsJson Is Nothing Then _
                    Throw New InvalidOperationException(WebApiResources.ErrorParsingJson)
            End If

            Return jsonParameters.
                        Select(Function(p)
                                   Return New clsArgument(p.Name, p.GetFromResponse(responseAsJson))
                               End Function)

        End Function

        Private Function CreateCustomCodeArguments(responseBody As String,
                                                     context As ActionContext) _
                                                   As IEnumerable(Of clsArgument)

            Return CustomCodeMethodType.OutputParameters.Invoke(mBuilder, context, responseBody)

        End Function
        
        Private Function GetRequestDataParameterValue(requestData As HttpRequestData) As clsProcessValue
            Dim formattedData = RequestDataFormatter.Format(requestData)
            Return New clsProcessValue(formattedData)
        End Function


        ''' <summary>
        ''' A method used to convert a Web Header Collection to a Data Table so it 
        ''' can appear as a collection in Process Studio.
        ''' </summary>
        ''' <param name="headers">The headers from a HttpResponseMessage</param>
        ''' <returns>A DataTable that appears as a collection in Process Studio. </returns>
        Private Shared Function GetHeaderTableFromHeaders(headers As WebHeaderCollection) As DataTable
            Dim table = New DataTable()
            table.Columns.Add()
            table.Columns.Add()

            headers?.
                AllKeys().
                ForEach(Function(key) table.Rows.Add(key, headers(key))).
                Evaluate()

            Return table
        End Function

    End Class

End Namespace