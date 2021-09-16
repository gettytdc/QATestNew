Imports System.Linq
Imports System.Net
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports BluePrism.AutomateProcessCore.WebApis.TemplateProcessing

Namespace WebApis.RequestHandling

    ''' <summary>
    ''' Class used to build a <see cref="HttpWebRequest"/> that can then be used
    ''' to call a particular Web API action. This builder will ensure the Uri, path 
    ''' headers, authentication and message body are all populated with the correct
    ''' parameter values
    ''' </summary>
    Public Class HttpRequestBuilder
        Implements IHttpRequestBuilder, IDiagnosticEmitter

        Public Event Diags(message As String) Implements IDiagnosticEmitter.Diags
        Private Sub RaiseDiags(ByVal msg As String)
            RaiseEvent Diags(msg)
        End Sub

        ''' <inheritdoc />
        Public Function Build(context As ActionContext,
                              authenticationHandler As IAuthenticationHandler,
                              bodyContentGenerator As IBodyContentGenerator) _
            As HttpRequestData Implements IHttpRequestBuilder.Build

            ExtendParameters(context, authenticationHandler)
            Dim uri = GetRequestUri(context)
            Dim request = Create(uri)
            SetSimpleProperties(request, context)
            Dim contentResult = bodyContentGenerator.GetBodyContent(context)

            AddHeaders(request, context, contentResult)
            AddAuthentication(request, context, authenticationHandler)
            ApplyConnectionSettings(request, context)

            If Not context.Action.DisableSendingOfRequest Then
                contentResult.Write(request)
            End If
            Return New HttpRequestData(request, contentResult.Content)

        End Function

        ''' <summary>
        ''' Creates a new HttpWebRequest with the specified URL
        ''' </summary>
        ''' <param name="uri">The uri with which to create the request</param>
        ''' <returns>The request</returns>
        ''' <remarks>Can be overriden to allow underlying request to be mocked during tests</remarks>
        Protected Overridable Function Create(uri As Uri) As HttpWebRequest
            Return WebRequest.CreateHttp(uri)
        End Function

        ''' <summary>
        ''' Sets simple properties of the request
        ''' </summary>
        ''' <param name="request">The request</param>
        ''' <param name="context">Context containing information about the action</param>
        Private Sub SetSimpleProperties(request As HttpWebRequest, context As ActionContext)
            request.Method = context.Action.Request.HttpMethod.Method()
        End Sub

        ''' <summary>
        ''' Gets the URI to use for the request, ensuring any placeholders are 
        ''' replaced by their equivalent parameter values
        ''' </summary>
        ''' <returns>The URI to use to perform the request</returns>
        Private Function GetRequestUri(context As ActionContext) As Uri

            Dim uriString = CombineBaseUri(context)
            uriString = ReplaceParameterTokens(uriString, context)

            Dim validatedUri As Uri = Nothing
            Try
                ' validate the uri now the parameters are in place
                validatedUri = New Uri(uriString)
            Catch ex As Exception
                RaiseDiags($"Error creating base url: {ex.Message}")
                Throw
            End Try
            Return validatedUri
        End Function

        ''' <summary>
        ''' Combines the Base URI of the Web API and the action path, returning the 
        ''' result in string form.
        ''' </summary>
        ''' <param name="context">Context containing information about the action</param>
        ''' <returns>The base URI and action path combined into a single URI in 
        ''' string form</returns>
        Private Function CombineBaseUri(context As ActionContext) As String
            Dim baseUri = context.Configuration.BaseUrl
            Dim path = context.Action.Request.UrlPath
            Return baseUri.TrimEnd("/"c) & "/" & path.TrimStart("/"c)
        End Function

        ''' <summary>
        ''' Adds the appropriate headers to the Http Web Request, ensuring any 
        ''' placeholders are replaced by their equivalent parameter values.
        ''' </summary>
        ''' <param name="request">The request on which the headers are to be set
        ''' </param>
        ''' <param name="context">Context containing information about the action</param>
        ''' <param name="contentResult">The generated body content</param>
        Private Sub AddHeaders(request As HttpWebRequest,
                               context As ActionContext,
                               contentResult As IBodyContentResult)

            Dim headers = contentResult.Headers.
                            Concat(context.Configuration.CommonRequestHeaders).
                            Concat(context.Action.Request.Headers).
                            ToList()

            For Each header As HttpHeader In headers
                Dim replacedValue = ReplaceParameterTokens(header.Value, context)
                Try
                    If WebHeaderCollection.IsRestricted(header.Name) Then
                        SetRestrictedHeader(request, header.Name, replacedValue)
                    Else
                        request.Headers.Add(header.Name, replacedValue)
                    End If
                Catch ex As Exception
                    Throw New ArgumentException(My.Resources.Resources.httpRequestBuilder_TheSpecifiedHeaderNameHasInvalidCharacters, header.Name, ex)
                End Try
            Next

        End Sub

        ''' <summary>
        ''' Sets the value of the specified restricted header into the given request.
        ''' </summary>
        ''' <param name="request">The request into which the header is to be set.
        ''' </param>
        ''' <param name="name">The name of the 'restricted' header to set.</param>
        ''' <param name="value">The value of the restricted header to set.</param>
        Private Sub SetRestrictedHeader(
         request As HttpWebRequest, name As String, value As String)
            Select Case name.ToLowerInvariant()
                Case "accept" : request.Accept = value
                Case "connection" : request.Connection = value
                Case "content-length" : request.ContentLength = CInt(value)
                Case "content-type" : request.ContentType = value
                Case "date" : request.Date = CDate(value)
                Case "expect" : request.Expect = value
                Case "host" : request.Host = value
                Case "if-modified-since" : request.IfModifiedSince = CDate(value)
                Case "referer" : request.Referer = value
                Case "transfer-encoding" : request.TransferEncoding = value
                Case "user-agent" : request.UserAgent = value

                Case Else
                    ' In real terms, this is 'Range' and 'Proxy-Connection'
                    ' "Range" requires adding ranges using the object model. Not
                    '    at all sure how to address that in the first instance.
                    ' "Proxy-Connection" takes an IWebProxy; it may be trivial to
                    '    turn a string into one of those but I haven't checked that
                    '    out yet.
                    Dim message = String.Format(WebApiResources.RestrictedHeaderNotSupportedErrorTemplate, name)
                    Throw New NotSupportedException(message)

            End Select

        End Sub

        Friend Sub ApplyConnectionSettings(request As HttpWebRequest, context As ActionContext)

            request.Timeout = context.Configuration.ConfigurationSettings.HttpRequestConnectionTimeout * 1000

            ' check if this already contains a uri specific one for this request
            Dim webConnectionSettings = CType(context.SessionWebConnectionSettings, WebConnectionBaseSettings)
            Dim existingUriSettings = context.SessionWebConnectionSettings.GetExistingUriSettings(request.RequestUri)

            ApplySettingsToServicePoint(If(existingUriSettings, webConnectionSettings),
                                        request.RequestUri)


        End Sub

        Private Sub ApplySettingsToServicePoint(settings As WebConnectionBaseSettings,
                                                requestUri As Uri)

            Dim sp = ServicePointManager.FindServicePoint(requestUri)
            With sp
                .ConnectionLeaseTimeout = If(settings.connectionLeaseTimeout * 1000, -1) 'This is the default if no timeout is specified
                .MaxIdleTime = settings.maxIdleTime * 1000
                .ConnectionLimit = settings.connectionLimit
            End With

        End Sub

        ''' <summary>
        ''' Add the authentication used by the Web API to the Http Web Request
        ''' </summary>
        ''' <param name="request">The request to add the authenticaiton to</param>
        ''' <param name="context">Context containing information about the action</param>
        ''' <param name="authenticationHandler">Handler used to handle the request's authentication</param>
        Private Sub AddAuthentication(request As HttpWebRequest,
                                      context As ActionContext,
                                      authenticationHandler As IAuthenticationHandler)
            authenticationHandler.Handle(request, context)
        End Sub

        ''' <summary>
        ''' Replaces the parameter placeholder tokens in the given string with the
        ''' appropriate parameters values.
        ''' </summary>
        ''' <param name="text">The text in which the placeholder tokens are to be
        ''' replaced.</param>
        ''' <param name="context">Context containing information about the action</param>
        ''' <returns>The given text with any parameter placeholder replaced with the
        ''' corresponding parameter values</returns>
        Private Function ReplaceParameterTokens(text As String,
                                                context As ActionContext) As String
            Return ProcessTemplate(text, context.Parameters)
        End Function

        Private Sub ExtendParameters(context As ActionContext, authenticationHandler As IAuthenticationHandler)
            authenticationHandler.GetCredentialParameters(context).ToList().
                                    ForEach(Sub(k) context.Parameters.Add(k.Key, k.Value))
        End Sub

    End Class

End Namespace