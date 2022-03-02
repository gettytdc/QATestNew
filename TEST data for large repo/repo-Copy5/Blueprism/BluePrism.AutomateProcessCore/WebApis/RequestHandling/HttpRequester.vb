Imports System.Linq
Imports System.Net
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports System.Text
Imports BluePrism.Core.Utility

Namespace WebApis.RequestHandling
    ''' <summary>
    ''' Co-ordinates the building and sending of an HTTP Request to a Web API action
    ''' </summary>
    Public Class HttpRequester
        Implements IHttpRequester, IDiagnosticEmitter

        Private ReadOnly mAuthenticationHandlers As IEnumerable(Of IAuthenticationHandler)
        Private ReadOnly mBodyContentHandlers As IEnumerable(Of IBodyContentGenerator)
        Private ReadOnly mRequestBuilder As IHttpRequestBuilder

        Public Event Diags(message As String) Implements IDiagnosticEmitter.Diags

        Private Sub RaiseDiags(ByVal msg As String)
            RaiseEvent Diags(msg)
        End Sub

        Sub New(authenticationHandlers As IEnumerable(Of IAuthenticationHandler),
                bodyContentHandlers As IEnumerable(Of IBodyContentGenerator),
                requestBuilder As IHttpRequestBuilder)
            mAuthenticationHandlers = authenticationHandlers
            mBodyContentHandlers = bodyContentHandlers
            mRequestBuilder = requestBuilder
        End Sub

        ''' <inheritdoc/>
        Public Function GetResponse(context As ActionContext) As HttpResponseData _
            Implements IHttpRequester.GetResponse

            Dim authenticationHandler =
                GetAuthenticationHandler(context.Configuration.CommonAuthentication)

            Dim bodyContentGenerator = GetBodyContentGenerator(context.Action.Request.BodyContent)

            Dim requestData = mRequestBuilder.Build(context, authenticationHandler, bodyContentGenerator)
            LogRequestDataDiagnostics(requestData)
            LogAppliedConnnectionSettings(requestData.Request)

            If context.Action.DisableSendingOfRequest Then
                LogResponseDataDiagnostics("No Response - Request Sending is Disabled.")
                Return New HttpResponseData(requestData, Nothing)
            End If

            TryCloseRequestStream(requestData.Request)

            Try
                Return GetResponse(requestData, context, authenticationHandler, bodyContentGenerator)
            Catch ex As WebException
                Throw New WebRequestException(ex)
            End Try
        End Function

        ''' <summary>
        ''' If the request is not a GET request, the requestStream used for writing the 
        ''' request body must be closed before calling GetResponse on the request
        ''' </summary>
        Private Sub TryCloseRequestStream(request As HttpWebRequest)
            If request.Method <> "GET" Then
                request.GetRequestStream()?.Dispose()
            End If
        End Sub

        Private Function GetResponse(requestData As HttpRequestData,
                                     context As ActionContext,
                                     authenticationHandler As IAuthenticationHandler,
                                     bodyContentGenerator As IBodyContentGenerator) As HttpResponseData
            Try
                Dim response = requestData.Request.GetResponse()
                Dim httpResponse = DirectCast(response, HttpWebResponse)
                LogResponseDataDiagnostics(FormatResponseData(httpResponse))
                Return New HttpResponseData(requestData, httpResponse)
            Catch ex As WebException When ShouldRetry(ex, 0, authenticationHandler)
                Return RetryOnException(1, context, authenticationHandler, bodyContentGenerator)
            End Try

        End Function


        Private Function RetryOnException(retryAttempt As Integer,
                                          context As ActionContext,
                                          authenticationHandler As IAuthenticationHandler,
                                          bodyContentHandler As IBodyContentGenerator) _
                                          As HttpResponseData

            authenticationHandler.BeforeRetry(context)
            Dim requestData = mRequestBuilder.Build(context, authenticationHandler, bodyContentHandler)
            LogRequestDataDiagnostics(requestData)
            LogAppliedConnnectionSettings(requestData.Request)
            Try
                Dim response = requestData.Request.GetResponse()
                Dim httpResponse = DirectCast(response, HttpWebResponse)
                LogResponseDataDiagnostics(FormatResponseData(httpResponse))
                Return New HttpResponseData(requestData, httpResponse)
            Catch ex As WebException When ShouldRetry(ex, retryAttempt, authenticationHandler)
                Return RetryOnException(retryAttempt + 1, context, authenticationHandler, bodyContentHandler)
            End Try

        End Function

        Private Function ShouldRetry(exception As WebException,
                                     retryAttempt As Integer,
                                     handler As IAuthenticationHandler) As Boolean

            Return exception.Is401WebException AndAlso
                retryAttempt < handler.RetryAttemptsOnUnauthorizedException

        End Function

        Private Function GetAuthenticationHandler(authentication As IAuthentication) As IAuthenticationHandler

            Dim handler = mAuthenticationHandlers.FirstOrDefault(Function(h) h.CanHandle(authentication))
            If handler Is Nothing Then _
                Throw New NotImplementedException(
                    $"Handler not found for authentication type {authentication.Type}")

            Return handler

        End Function

        Private Function GetBodyContentGenerator(bodyContent As IBodyContent) As IBodyContentGenerator

            Dim handler = mBodyContentHandlers.FirstOrDefault(Function(h) h.CanHandle(bodyContent))
            If handler Is Nothing Then _
                Throw New NotImplementedException(
                    $"Handler not found for body content type {bodyContent.Type}")

            Return handler

        End Function

        Private Sub LogRequestDataDiagnostics(requestData As HttpRequestData)
            Dim message = New StringBuilder()
            message.AppendLine("HTTP Request Data:-")
            message.AppendLine(RequestDataFormatter.Format(requestData))
            RaiseDiags(message.ToString())
        End Sub

        Private Sub LogAppliedConnnectionSettings(request As HttpWebRequest)
            Dim message = New StringBuilder()
            message.AppendLine($"Max Idle Time (ms):- {request.ServicePoint.MaxIdleTime}")
            message.AppendLine($"Connection Limit:- {request.ServicePoint.ConnectionLimit}")
            message.AppendLine($"Connection Lease Timeout (ms):- {request.ServicePoint.ConnectionLeaseTimeout}")
            RaiseDiags(message.ToString())
        End Sub

        Private Sub LogResponseDataDiagnostics(responseData As String)
            Dim message = New StringBuilder()
            message.AppendLine("HTTP Response Data:-")
            message.AppendLine(responseData)
            RaiseDiags(message.ToString())
        End Sub

        Private Function FormatResponseData(response As HttpWebResponse) As String

            If response Is Nothing Then Return String.Empty

            Dim result = New StringBuilder()
            result.AppendLine($"Content-Type: {response.ContentType}")
            result.AppendLine($"Headers: {response.Headers}")
            result.AppendLine($"Status Code: {response.StatusCode}")
            Return result.ToString()
        End Function

    End Class

End Namespace
