#If UNITTESTS Then

Imports System.Drawing
Imports System.Net.Http
Imports System.Xml.Linq
Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports BluePrism.Core.Utility
Imports BluePrism.Server.Domain.Models
Imports BluePrism.UnitTesting.TestSupport
Imports FluentAssertions
Imports NUnit.Framework

Namespace WebApis

    ''' <summary>
    ''' Class that tests the Xml Serialization and deserialization of Web API classes
    ''' </summary>
    <TestFixture, Category("Web APIs")>
    Public Class WebApiXmlSerializationTests

#Region " Xml Round Trip Tests "
        <Test>
        Public Sub WebApiConfigurationXmlRoundTrip_ShouldReturnSameObject()
            Dim configuration = GetFullyPopulatedWebApiConfiguration()
            Dim configurationXml = configuration.ToXml()

            Dim roundTrip = WebApiConfiguration.FromXml(configuration.ToXml())

            roundTrip.ShouldBeEquivalentTo(configuration)

        End Sub

        <Test>
        Public Sub EmptyAuthenticationXmlRoundTrip_ShouldReturnSameObject()
            Dim testAuthentication = New EmptyAuthentication()
            Dim testAuthenticationXml = testAuthentication.ToXElement().ToString()

            Dim roundTrip = AuthenticationDeserializer.Deserialize(testAuthenticationXml)

            roundTrip.ShouldBeEquivalentTo(testAuthentication)

        End Sub

        <Test>
        Public Sub BasicAuthenticationXmlRoundTrip_ShouldReturnSameObject()
            Dim credential = New AuthenticationCredential("My Credential", True, "Credential")
            Dim testAuthentication = New BasicAuthentication(credential, True)
            Dim testAuthenticationXml = testAuthentication.ToXElement().ToString()

            Dim roundTrip = AuthenticationDeserializer.Deserialize(testAuthenticationXml)

            roundTrip.ShouldBeEquivalentTo(testAuthentication)

        End Sub

        <Test>
        Public Sub BearerAuthenticationXmlRoundTrip_ShouldReturnSameObject()
            Dim credential = New AuthenticationCredential("My Credential", True, "Credential")
            Dim testAuthentication = New BearerTokenAuthentication(credential)
            Dim testAuthenticationXml = testAuthentication.ToXElement().ToString()

            Dim roundTrip = AuthenticationDeserializer.Deserialize(testAuthenticationXml)

            roundTrip.ShouldBeEquivalentTo(testAuthentication)

        End Sub

        <Test>
        Public Sub OAuth2ClientCredentialsAuthenticationXmlRoundTrip_ShouldReturnSameObject()
            Dim credential = New AuthenticationCredential("My Credential", True, "Credential")
            Dim testAuthentication = New OAuth2ClientCredentialsAuthentication(credential, "api", New Uri("https://www.authserver.com"))
            Dim testAuthenticationXml = testAuthentication.ToXElement().ToString()

            Dim roundTrip = AuthenticationDeserializer.Deserialize(testAuthenticationXml)

            roundTrip.ShouldBeEquivalentTo(testAuthentication)

        End Sub

        <Test>
        Public Sub OAuth2JwtAuthenticationXmlRoundTrip_ShouldReturnSameObject()
            Dim credential = New AuthenticationCredential("My Credential", True, "Credential")
            Dim jwt = New JwtConfiguration("http://www.api.com/token", "api", "testuser@api.com", 60, credential)
            Dim testAuthentication = New OAuth2JwtBearerTokenAuthentication(jwt, New Uri("https://www.authserver.com"))
            Dim testAuthenticationXml = testAuthentication.ToXElement().ToString()

            Dim roundTrip = AuthenticationDeserializer.Deserialize(testAuthenticationXml)

            roundTrip.ShouldBeEquivalentTo(testAuthentication)

        End Sub

        <Test>
        Public Sub CustomAuthenticationXmlRoundTrip_ShouldReturnSameObject()
            Dim credential = New AuthenticationCredential("My Credential", True, "Credential")
            Dim testAuthentication = New CustomAuthentication(credential)
            Dim testAuthenticationXml = testAuthentication.ToXElement().ToString()

            Dim roundTrip = AuthenticationDeserializer.Deserialize(testAuthenticationXml)

            roundTrip.ShouldBeEquivalentTo(testAuthentication)

        End Sub

        <Test>
        Public Sub NoBodyContentXmlRoundTrip_ShouldReturnSameObject()
            Dim testContent = New NoBodyContent()
            Dim testContentXml = testContent.ToXElement().ToString()

            Dim roundTrip = BodyContentDeserializer.Deserialize(testContentXml)

            roundTrip.ShouldBeEquivalentTo(testContent)

        End Sub

        <Test>
        Public Sub TemplateBodyContentXmlRoundTrip_ShouldReturnSameObject()
            Dim testContent = New TemplateBodyContent("Some text")
            Dim testContentXml = testContent.ToXElement().ToString()

            Dim roundTrip = BodyContentDeserializer.Deserialize(testContentXml)

            roundTrip.ShouldBeEquivalentTo(testContent)

        End Sub

        <Test>
        Public Sub SingleFileBodyContentXmlRoundTrip_ShouldReturnSameObject()
            Dim testContent = New SingleFileBodyContent("Input 1")
            Dim testContentXml = testContent.ToXElement().ToString()

            Dim roundTrip = BodyContentDeserializer.Deserialize(testContentXml)

            roundTrip.ShouldBeEquivalentTo(testContent)

        End Sub

        <Test>
        Public Sub FileCollectionBodyContentXmlRoundTrip_ShouldReturnSameObject()
            Dim testContent = New FileCollectionBodyContent("Input 1")
            Dim testContentXml = testContent.ToXElement().ToString()

            Dim roundTrip = BodyContentDeserializer.Deserialize(testContentXml)

            roundTrip.ShouldBeEquivalentTo(testContent)

        End Sub

        <Test>
        Public Sub WebApiConfigurationXElementRoundTrip_ShouldReturnSameObject()
            Dim configuration = GetFullyPopulatedWebApiConfiguration()

            Dim roundTrip = WebApiConfiguration.FromXElement(configuration.ToXElement())

            roundTrip.ShouldBeEquivalentTo(configuration)

        End Sub

        <Test>
        Public Sub TemplateBodyContentXElementRoundTrip_TemplateContainingCarriageReturn_ShouldPreserveText()
            Dim template = $"{{{vbCrLf}""param1"": 200{vbCrLf}}}"
            Dim testContent = New TemplateBodyContent(template)
            Dim testContentXml = testContent.ToXElement().ToString()

            Dim roundTrip = BodyContentDeserializer.Deserialize(testContentXml)
            roundTrip.ShouldBeEquivalentTo(testContent)
            DirectCast(roundTrip, TemplateBodyContent).Template.Should.Be(template)
        End Sub

        <Test>
        Public Sub TemplateBodyContentXElementRoundTrip_TemplateContainingXMLCharacters_ShouldPreserveText()
            Dim template = $"{{<param1: 200>}}"
            Dim testContent = New TemplateBodyContent(template)
            Dim testContentXml = testContent.ToXElement().ToString()

            Dim roundTrip = BodyContentDeserializer.Deserialize(testContentXml)
            roundTrip.ShouldBeEquivalentTo(testContent)
            DirectCast(roundTrip, TemplateBodyContent).Template.Should.Be(template)
        End Sub

        <Test>
        Public Sub ResponseOutputParameter_JsonPath_Roundtrip_ShouldReturnSameObject()
            Dim jsonPathParam = New JsonPathOutputParameter("Test", "Description", "$.TestPath", DataType.text)

            Dim jsonParamXml = jsonPathParam.ToXElement()
            Dim roundtrip = OutputParameterDeserializer.Deserialize(jsonParamXml)

            roundtrip.ShouldBeEquivalentTo(jsonPathParam)
        End Sub

        <Test>
        Public Sub ResponseOutputParameter_CustomCodePath_Roundtrip_ShouldReturnSameObject()
            Dim customCodeParam = New CustomCodeOutputParameter("Test 1", "Description", DataType.date)

            Dim jsonParamXml = customCodeParam.ToXElement()
            Dim roundtrip = OutputParameterDeserializer.Deserialize(jsonParamXml)

            roundtrip.ShouldBeEquivalentTo(customCodeParam)
        End Sub

#End Region

#Region " XElement Round Trip Tests "
        <Test>
        Public Sub WebApiConfigurationFromXElement_InvalidElementName_ShouldThrowException()

            Dim invalidXml As New XElement("invalid",
                                  New XAttribute("baseurl", "https://www.google.co.uk"),
                                    New XElement("actions", {}),
                                    New XElement("commonparameters", {}),
                                    New XElement("commonheaders", {})
                                )

            Assert.That(Sub() WebApiConfiguration.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiConfigurationFromXElement_NoBaseUrl_ShouldThrowException()

            Dim invalidXml As New XElement("configuration",
                                  New XElement("actions", {}),
                                    New XElement("commonparameters", {}),
                                    New XElement("commonheaders", {})
                                )

            Assert.That(Sub() WebApiConfiguration.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiConfigurationFromXElement_NoActions_ShouldThrowException()

            Dim invalidXml As New XElement("configuration",
                                  New XAttribute("baseurl", "https://www.google.co.uk"),
                                    New XElement("commonparameters", {}),
                                    New XElement("commonheaders", {})
                                )

            Assert.That(Sub() WebApiConfiguration.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiConfigurationFromXElement_NoCommonParameters_ShouldThrowException()

            Dim invalidXml As New XElement("configuration",
                                  New XAttribute("baseurl", "https://www.google.co.uk"),
                                    New XElement("actions", {}),
                                    New XElement("commonheaders", {})
                                )

            Assert.That(Sub() WebApiConfiguration.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiConfigurationFromXElement_NoCommonHeaders_ShouldThrowException()

            Dim invalidXml As New XElement("configuration",
                                  New XAttribute("baseurl", "https://www.google.co.uk"),
                                    New XElement("actions", {}),
                                    New XElement("commonparameters", {}),
                                  New XElement("commonheaders", {})
                                    )

            Assert.That(Sub() WebApiConfiguration.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiConfigurationFromXElement_NoCommonAuthentication_ShouldThrowException()

            Dim invalidXml As New XElement("configuration",
                                  New XAttribute("baseurl", "https://www.google.co.uk"),
                                  New XElement("actions", {}),
                                  New XElement("commonparameters", {})
                                  )

            Assert.That(Sub() WebApiConfiguration.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiConfigurationXElementRoundTrip_BaseUrlIncludesEncodedSpace_ShouldReturnSameObject()

            Dim configuration = New WebApiConfigurationBuilder().
                                        WithBaseUrl("http://example.com/some%20path").
                                        WithCommonAuthentication(New EmptyAuthentication).
                                        Build

            Dim roundTrip = WebApiConfiguration.FromXElement(configuration.ToXElement())

            roundTrip.BaseUrl.Should.Be(configuration.BaseUrl)

        End Sub

        <Test>
        Public Sub ActionParameterXElementRoundTrip_ShouldReturnSameObject()
            Dim parameter = New ActionParameter(
                "param 1", "Some description", DataType.text, True, "Some value")

            Dim roundTrip = ActionParameter.FromXElement(parameter.ToXElement())
            roundTrip.ShouldBeEquivalentTo(parameter)
        End Sub

        <Test>
        Public Sub ActionParameterFromXElement_InvalidElementName_ShouldThrowException()

            Dim invalidXml As New XElement("invalid",
                                New XAttribute("name", "param 1"),
                                New XAttribute("expose", True),
                                    New XElement("initialvalue", New XCData("Some default")),
                                    New XElement("description", New XCData("Some data"))
                                )

            Assert.That(Sub() ActionParameter.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub ActionParameterFromXElement_NoName_ShouldThrowException()

            Dim invalidXml As New XElement("actionparameter",
                                New XAttribute("expose", True),
                                    New XElement("initialvalue", New XCData("Some default")),
                                    New XElement("description", New XCData("Some data")))

            Assert.That(Sub() ActionParameter.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub ActionParameterFromXElement_NoExpose_ShouldThrowException()

            Dim invalidXml As New XElement("actionparameter",
                                New XAttribute("name", "param 1"),
                                    New XElement("initialvalue", New XCData("Some default")),
                                    New XElement("description", New XCData("Some data"))
                                )

            Assert.That(Sub() ActionParameter.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub ActionParameterFromXElement_NoInitialValue_ShouldThrowException()

            Dim invalidXml As New XElement("actionparameter",
                                New XAttribute("name", "param 1"),
                                New XAttribute("expose", True),
                                    New XElement("description", New XCData("Some data")))

            Assert.That(Sub() ActionParameter.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))

        End Sub

        <Test>
        Public Sub ActionParameterFromXElement_NoDescription_ShouldThrowException()

            Dim invalidXml As New XElement("actionparameter",
                                New XAttribute("name", "param 1"),
                                New XAttribute("expose", True),
                                    New XElement("initialvalue", New XCData("Some default")))

            Assert.That(Sub() ActionParameter.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))

        End Sub

        <Test, TestCaseSource("GetWhiteSpaceStrings")>
        Public Sub ActionParameterXElementRoundTrip_DescriptionHasWhiteSpace_ShouldReturnSameObject(description As String)
            Dim parameter = New ActionParameter(
                "Name 1", description, DataType.text, False, String.Empty)

            Dim roundTrip = ActionParameter.FromXElement(parameter.ToXElement())
            roundTrip.Description.Should.Be(description)

        End Sub

        <Test, TestCaseSource("GetProcessValues")>
        Public Sub ActionParameterXElementRoundTrip_InitialValue_ShouldReturnSameObject(initialValue As clsProcessValue)
            Dim parameter = New ActionParameter(
                "Name 1", "Use this parameter to do some stuff", initialValue.DataType, False,
                initialValue)
            Dim roundTrip = ActionParameter.FromXElement(parameter.ToXElement())
            Assert.That(roundTrip.InitialValue, Iz.EqualTo(initialValue))

        End Sub

        <Test>
        Public Sub WebApiRequestXElementRoundTrip_ShouldReturnSameObject()
            Dim request As New WebApiRequest(
                HttpMethod.Put,
                "/action1",
                {
                 New HttpHeader("accept", "text/html"),
                 New HttpHeader("Accept-Charset", "utf-8")
                },
                New TemplateBodyContent("{""param1"": 200}"))

            Dim roundTrip = WebApiRequest.FromXElement(request.ToXElement())
            roundTrip.ShouldBeEquivalentTo(request)
        End Sub

        <Test>
        Public Sub WebApiRequestXElementRoundTrip_NonStandardHttpMethod_ShouldReturnSameObject()
            Dim request As New WebApiRequest(
                New HttpMethod("NonStandardMethod"),
                "/action1",
                {
                    New HttpHeader("accept", "text/html"),
                    New HttpHeader("Accept-Charset", "utf-8")
                },
                New TemplateBodyContent("{""param1"": 200}"))

            Dim roundTrip = WebApiRequest.FromXElement(request.ToXElement())
            roundTrip.ShouldBeEquivalentTo(request)
        End Sub

        <Test>
        Public Sub WebApiRequestXElementRoundTrip_InvalidElementName_ShouldThrowException()
            Dim invalidXml = New XElement("invalid",
                                New XAttribute("httpmethod", HttpMethod.Options()),
                                New XAttribute("urlpath", "/action1/{param1}"),
                                New XAttribute("bodytype", WebApiRequestBodyType.Template),
                                    New XElement("headers", {}),
                                    New XElement("bodytemplate", New XCData("some text"))
                                )
            Assert.That(Sub() HttpHeader.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiRequestXElementRoundTrip_NoHttpMethod_ShouldThrowException()

            Dim invalidXml As New XElement("request",
                                New XAttribute("urlpath", "/action1/{param1}"),
                                New XAttribute("bodytype", WebApiRequestBodyType.Template),
                                    New XElement("headers", {}),
                                    New XElement("bodytemplate", New XCData("some text"))
                                )

            Assert.That(Sub() WebApiRequest.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiRequestXElementRoundTrip_NoUrlPath_ShouldThrowException()

            Dim invalidXml As New XElement("request",
                                New XAttribute("httpmethod", HttpMethod.Options()),
                                New XAttribute("bodytype", WebApiRequestBodyType.Template),
                                    New XElement("headers", {}),
                                    New XElement("bodytemplate", New XCData("some text"))
                                )

            Assert.That(Sub() WebApiRequest.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiRequestXElementRoundTrip_NoBodyType_ShouldThrowException()

            Dim invalidXml As New XElement("request",
                                New XAttribute("httpmethod", HttpMethod.Options()),
                                New XAttribute("urlpath", "/action1/{param1}"),
                                    New XElement("headers", {}),
                                    New XElement("bodytemplate", New XCData("some text"))
                                )

            Assert.That(Sub() WebApiRequest.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiRequestXElementRoundTrip_NoHeaders_ShouldThrowException()

            Dim invalidXml As New XElement("request",
                                New XAttribute("httpmethod", HttpMethod.Options()),
                                New XAttribute("urlpath", "/action1/{param1}"),
                                New XAttribute("bodytype", WebApiRequestBodyType.Template),
                                    New XElement("bodytemplate", New XCData("some text"))
                                )

            Assert.That(Sub() WebApiRequest.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiRequestXElementRoundTrip_NoBodyTemplate_ShouldThrowException()

            Dim invalidXml As New XElement("request",
                                New XAttribute("httpmethod", HttpMethod.Options()),
                                New XAttribute("urlpath", "/action1/{param1}"),
                                New XAttribute("bodytype", WebApiRequestBodyType.Template),
                                    New XElement("headers", {})
                                )

            Assert.That(Sub() WebApiRequest.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test, TestCaseSource("GetWhiteSpaceStrings")>
        Public Sub WebApiRequestXElementRoundTrip_BodyTemplateHasWhitespace_ShouldReturnSameObject(bodyTemplate As String)
            Dim request = New WebApiRequest(HttpMethod.Post,
                                            "/action1/",
                                            {},
                                            New TemplateBodyContent(bodyTemplate))


            Dim roundTrip = WebApiRequest.FromXElement(request.ToXElement())
            Dim expected = New TemplateBodyContent(bodyTemplate)
            roundTrip.BodyContent.ShouldBeEquivalentTo(New TemplateBodyContent(bodyTemplate))


        End Sub

        <Test>
        Public Sub WebApiActionXElementRoundTrip_ShouldReturnSameObject()
            Dim header = New WebApiAction(
                "Action 1",
                "This is a description",
                True,
                True,
                True,
                New WebApiRequest(
                    HttpMethod.Post,
                    "/action/{param1}",
                    {New HttpHeader("accept", "text/html")},
                    New TemplateBodyContent("Body template goes here")),
                {
                    New ActionParameter(
                        "param1",
                        "Description of param1",
                        DataType.text,
                        True,
                        "default value 1"),
                    New ActionParameter(
                        "param2",
                        "Description of param2",
                        DataType.text,
                        True,
                        "default value 2")
                },
                New OutputParameterConfiguration({New JsonPathOutputParameter("Test", "Description", "$.TestPath", DataType.text),
                                                    New CustomCodeOutputParameter("Test 1", "Description", DataType.date)}, "come code"))

            Dim roundTrip = WebApiAction.FromXElement(header.ToXElement())
            roundTrip.ShouldBeEquivalentTo(header)
        End Sub

        <Test>
        Public Sub WebApiActionXElementRoundTrip_InvalidElementName_ShouldThrowException()
            Dim invalidXml = New XElement("invalid",
                                New XAttribute("name", "action1"),
                                New XElement("description", ""),
                                New XAttribute("enabled", False),
                                New XElement("parameters", {}),
                                GetSampleValidRequestXML()
                               )
            Assert.That(Sub() WebApiAction.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiActionXElementRoundTrip_NoName_ShouldThrowException()

            Dim invalidXml = New XElement("action",
                                New XAttribute("enabled", False),
                                New XElement("description", ""),
                                New XElement("parameters", {}),
                                GetSampleValidRequestXML()
                               )

            Assert.That(Sub() WebApiAction.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiActionXElementRoundTrip_NoDescription_ShouldThrowException()

            Dim invalidXml = New XElement("action",
                                New XAttribute("name", "action1"),
                                New XAttribute("enabled", False),
                                New XElement("parameters", {}),
                                GetSampleValidRequestXML()
                               )

            Assert.That(Sub() WebApiAction.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiActionXElementRoundTrip_NoEnabled_ShouldThrowException()

            Dim invalidXml = New XElement("action",
                                New XAttribute("name", "action1"),
                                New XAttribute("description", ""),
                                New XElement("parameters", {}),
                                GetSampleValidRequestXML()
                               )

            Assert.That(Sub() WebApiAction.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiActionXElementRoundTrip_NoParameters_ShouldThrowException()

            Dim invalidXml = New XElement("action",
                                New XAttribute("name", "action1"),
                                New XAttribute("enabled", False),
                                New XElement("description", ""),
                                GetSampleValidRequestXML()
                               )

            Assert.That(Sub() WebApiAction.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub WebApiActionXElementRoundTrip_NoRequest_ShouldThrowException()

            Dim invalidXml = New XElement("action",
                                New XAttribute("name", "action1"),
                                New XAttribute("enabled", False),
                                New XElement("description", ""),
                                New XElement("parameters", {})
                               )

            Assert.That(Sub() WebApiAction.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test, TestCaseSource("GetWhiteSpaceStrings")>
        Public Sub WebApiActionXElementRoundTrip_DescriptionHasWhiteSpace_ShouldReturnSameObject(actionDescription As String)
            Dim action = New WebApiAction(
                "Action1",
                actionDescription,
                True,
                True,
                True,
                New WebApiRequest(
                    HttpMethod.Post, "https://mywebapipath.com/", {},
                    New TemplateBodyContent(String.Empty)),
                {}, New OutputParameterConfiguration({}, "")
            )

            Dim roundTrip = WebApiAction.FromXElement(action.ToXElement())
            roundTrip.Description.Should.Be(actionDescription)

        End Sub

        <Test>
        Public Sub HttpHeaderXElementRoundTrip_ShouldReturnSameObject()
            Dim header = New HttpHeader("accept", "text/html")
            Dim roundTrip = HttpHeader.FromXElement(header.ToXElement())
            roundTrip.ShouldBeEquivalentTo(header)
        End Sub

        <Test>
        Public Sub HttpHeaderXElementRoundTrip_InvalidElementName_ShouldThrowException()
            Dim invalidXml = <invalid name="accept" value="text/html"/>
            Assert.That(Sub() HttpHeader.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub HttpHeaderXElementRoundTrip_NoName_ShouldThrowException()
            Dim invalidXml = <httpheader value="xxx"/>
            Assert.That(Sub() HttpHeader.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub HttpHeaderXElementRoundTrip_NoValue_ShouldThrowException()
            Dim invalidXml = <httpheader name="Accept"/>
            Assert.That(Sub() HttpHeader.FromXElement(invalidXml),
                   Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub EmptyAuthenticationXElementRoundTrip_ShouldReturnSameObject()
            Dim auth = New EmptyAuthentication
            Dim roundTrip = EmptyAuthentication.FromXElement(auth.ToXElement())
            roundTrip.ShouldBeEquivalentTo(auth)
        End Sub

        <Test>
        Public Sub EmptyAuthenticationXElementRoundTrip_InvalidElementName_ShouldReturnSameObject()
            Dim invalidXml = <invalid/>
            Assert.That(Sub() EmptyAuthentication.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub EmptyAuthenticationXElementRoundTrip_NoType_ShouldThrowException()
            Dim invalidXml = <authentication/>
            Assert.That(Sub() EmptyAuthentication.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub BasicAuthenticationXElementRoundTrip_ShouldReturnSameObject()
            Dim auth = New BasicAuthentication(
                New AuthenticationCredential("testCredential", True, "testParamName"),
                True)
            Dim roundTrip = BasicAuthentication.FromXElement(auth.ToXElement())
            roundTrip.ShouldBeEquivalentTo(auth)
        End Sub

        <Test>
        Public Sub BasicAuthenticationXElementRoundTrip_InvalidElementName_ShouldThrowException()
            Dim invalidXml = <invalid
                                 type=<%= 1 %>
                                 preemptive=<%= True %>
                                 <%= New AuthenticationCredential(
                                        "testCredential", True, "testParamName").ToXElement %>
                             />
            Assert.That(Sub() BasicAuthentication.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub BasicAuthenticationXElementRoundTrip_NoPreEmptive_ShouldThrowException()
            Dim x = <authentication
                        type=<%= 1 %>
                        <%= New AuthenticationCredential(
                            "testCredential", True, "testParamName").ToXElement %>
                    />
            Assert.That(Sub() BasicAuthentication.FromXElement(x),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub BasicAuthenticationXElementRoundTrip_NoCredential_ShouldThrowException()
            Dim invalidXml = <authentication
                                 type=<%= 1 %>
                                 preemptive=<%= True %>
                             />
            Assert.That(Sub() BasicAuthentication.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub BasicAuthenticationXElementRoundTrip_NoType_ShouldThrowException()
            Dim invalidXml = <authentication
                                 preemptive=<%= True %>
                                 <%= New AuthenticationCredential(
                                    "testCredential", True, "testParamName").ToXElement %>
                             />
            Assert.That(Sub() BasicAuthentication.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub BearerTokenAuthenticationXElementRoundTrip_ShouldReturnSameObject()
            Dim auth = New BearerTokenAuthentication(
                New AuthenticationCredential("testCredential", True, "testParamName"))
            Dim roundTrip = BearerTokenAuthentication.FromXElement(auth.ToXElement())
            roundTrip.ShouldBeEquivalentTo(auth)
        End Sub

        <Test>
        Public Sub BearerTokenAuthenticationXElementRoundTrip_InvalidElementName_ShouldThrowException()
            Dim invalidXml = <invalid
                                 type=<%= 2 %>
                                 <%= New AuthenticationCredential(
                            "testCredential", True, "testParamName").ToXElement %>
                             />
            Assert.That(Sub() BearerTokenAuthentication.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub


        <Test>
        Public Sub BearerTokenAuthenticationXElementRoundTrip_NoCredential_ShouldThrowException()
            Dim invalidXml = <authentication
                                 type=<%= 2 %>
                             />
            Assert.That(Sub() BearerTokenAuthentication.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub BearerTokenAuthenticationXElementRoundTrip_NoType_ShouldThrowException()
            Dim invalidXml = <authentication
                                 <%= New AuthenticationCredential(
                            "testCredential", True, "testParamName").ToXElement %>
                             />
            Assert.That(Sub() BearerTokenAuthentication.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub AuthenticationCredentialXElementRoundTrip_ShouldReturnSameObject()
            Dim cred = New AuthenticationCredential("testCredential", True, "testParamName")
            Dim roundTrip = AuthenticationCredential.FromXElement(cred.ToXElement())
            roundTrip.ShouldBeEquivalentTo(cred)
        End Sub

        <Test>
        Public Sub AuthenticationCredentialXElementRoundTrip_InvalidElementName_ShouldThrowException()
            Dim invalidXml = <invalid
                                 credentialname=<%= "Test Credential" %>
                                 exposetoprocess=<%= True %>
                                 inputparametername=<%= "Test Parameter Name" %>
                             />
            Assert.That(Sub() AuthenticationCredential.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub AuthenticationCredentialXElementRoundTrip_NoName_ShouldThrowException()
            Dim invalidXml = <credential
                                 exposetoprocess=<%= True %>
                                 inputparametername=<%= "Test Parameter Name" %>
                             />
            Assert.That(Sub() AuthenticationCredential.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub AuthenticationCredentialXElementRoundTrip_NoExposeToProcess_ShouldThrowException()
            Dim invalidXml = <credential
                                 credentialname=<%= "Test Credential" %>
                                 inputparametername=<%= "Test Parameter Name" %>
                             />
            Assert.That(Sub() AuthenticationCredential.FromXElement(invalidXml),
                 Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub AuthenticationCredentialXElementRoundTrip_NoInputParameterName_ShouldThrowException()
            Dim invalidXml = <credential
                                 credentialname=<%= "Test Credential" %>
                                 exposetoprocess=<%= True %>
                             />
            Assert.That(Sub() AuthenticationCredential.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub OAuth2ClientCredentialsXElementRoundTrip_ShouldReturnSameObject()
            Dim auth = New OAuth2ClientCredentialsAuthentication(
                New AuthenticationCredential("testCredential", True, "testParamName"),
                                "test this scope",
                                New Uri("http://testThisAuthServer.com"))
            Dim roundTrip = OAuth2ClientCredentialsAuthentication.FromXElement(auth.ToXElement())
            roundTrip.ShouldBeEquivalentTo(auth)
        End Sub

        <Test>
        Public Sub OAuth2ClientCredentialsXElementRoundTrip_InvalidElementName_ShouldThrowException()
            Dim invalidXml = <invalid
                                 type=<%= 4 %>
                                 scope=<%= "test Scope" %>
                                 authorizationserver=<%= New Uri("http://testAuthServer.com") %>>
                                 <%= New AuthenticationCredential(
                            "testCredential", True, "testParamName").ToXElement %>
                             </invalid>

            Assert.That(Sub() OAuth2ClientCredentialsAuthentication.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub OAuth2ClientCredentialsXElementRoundTrip_NoType_ShouldThrowException()
            Dim invalidXml = <authentication
                                 scope=<%= "test Scope" %>
                                 authorizationserver=<%= New Uri("http://testAuthServer.com") %>
                                 <%= New AuthenticationCredential(
                            "testCredential", True, "testParamName").ToXElement %>>
                             </authentication>
            Assert.That(Sub() OAuth2ClientCredentialsAuthentication.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub OAuth2ClientCredentialsXElementRoundTrip_NoCredential_ShouldThrowException()
            Dim invalidXml = <authentication
                                 type=<%= 4 %>
                                 scope=<%= "test Scope" %>
                                 authorizationserver=<%= New Uri("http://testAuthServer.com") %>>
                             </authentication>
            Assert.That(Sub() OAuth2ClientCredentialsAuthentication.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub OAuth2ClientCredentialsXElementRoundTrip_NoScope_ShouldThrowException()
            Dim invalidXml = <authentication
                                 type=<%= 4 %>
                                 authorizationserver=<%= New Uri("http://testAuthServer.com") %>
                                 <%= New AuthenticationCredential(
                            "testCredential", True, "testParamName").ToXElement %>>
                             </authentication>
            Assert.That(Sub() OAuth2ClientCredentialsAuthentication.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub OAuth2ClientCredentialsXElementRoundTrip_NoAuthorizationServer_ShouldThrowException()
            Dim invalidXml = <authentication
                                 type=<%= 4 %>
                                 scope=<%= "test Scope" %>
                                 <%= New AuthenticationCredential(
                            "testCredential", True, "testParamName").ToXElement %>>
                             </authentication>
            Assert.That(Sub() OAuth2ClientCredentialsAuthentication.FromXElement(invalidXml),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub OAuth2JwtBearerTokenXElementRoundTrip_ShouldReturnSameObject()
            Dim jwtConfig = New JwtConfiguration(
                "http://testServer.com",
                "api/scope",
                "dave.smith@example.com",
                120,
                New AuthenticationCredential("testCredential", True, "testParamName"))

            Dim auth = New OAuth2JwtBearerTokenAuthentication(jwtConfig,
                                                              New Uri("http://testServer.com"))

            Dim roundTrip = OAuth2JwtBearerTokenAuthentication.FromXElement(auth.ToXElement())
            roundTrip.ShouldBeEquivalentTo(auth)

        End Sub

        <Test>
        Public Sub OAuth2JwtBearerTokenXElementRoundTrip_InvalidElementName_ShouldThrowException()
            Dim jwtConfig = New JwtConfiguration(
                "http://testServer.com",
                "api/scope",
                "dave.smith@example.com",
                120,
                New AuthenticationCredential("testCredential", True, "testParamName"))

            Dim auth = <thisIsNotValid
                           type=<%= 5 %>
                           authorizationserver=<%= New Uri("http://testAuthServer.com") %>>
                           <%= jwtConfig.ToXElement %>
                       </thisIsNotValid>
            Assert.That(Sub() OAuth2JwtBearerTokenAuthentication.FromXElement(auth),
                                                Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub OAuth2JwtBearerTokenXElementRoundTrip_NoType_ShouldThrowException()
            Dim jwtConfig = New JwtConfiguration(
                "http://testServer.com",
                "api/scope",
                "dave.smith@example.com",
                120,
                New AuthenticationCredential("testCredential", True, "testParamName"))

            Dim auth = <authentication
                           authorizationserver=<%= New Uri("http://testAuthServer.com") %>>
                           <%= jwtConfig.ToXElement %>
                       </authentication>
            Assert.That(Sub() OAuth2JwtBearerTokenAuthentication.FromXElement(auth),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub


        <Test>
        Public Sub OAuth2JwtBearerTokenXElementRoundTrip_NoAuthorizationServer_ShouldThrowException()
            Dim jwtConfig = New JwtConfiguration(
                "http://testServer.com",
                "api/scope",
                "dave.smith@example.com",
                120,
                New AuthenticationCredential("testCredential", True, "testParamName"))

            Dim auth = <authentication
                           type=<%= 5 %>>
                           <%= jwtConfig.ToXElement %>
                       </authentication>

            Assert.That(Sub() OAuth2JwtBearerTokenAuthentication.FromXElement(auth),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub OAuth2JwtBearerTokenXElementRoundTrip_NoJwtConfiguration_ShouldThrowException()

            Dim auth = <authentication
                           type=<%= 5 %>
                           authorizationserver=<%= New Uri("http://testAuthServer.com") %>>
                       </authentication>

            Assert.That(Sub() OAuth2JwtBearerTokenAuthentication.FromXElement(auth),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub CustomAuthenticationXElementRoundTrip_ShouldReturnSameObject()
            Dim testCredential = New AuthenticationCredential("test", False, "test")
            Dim customAuth = New CustomAuthentication(testCredential)

            Dim roundTrip = CustomAuthentication.FromXElement(customAuth.ToXElement())
            roundTrip.ShouldBeEquivalentTo(customAuth)

        End Sub

        <Test>
        Public Sub JwtConfigurationXElementRoundTrip_ShouldReturnSameObject()

            Dim config = New JwtConfiguration(
                "http://testServer.com",
                "api/scope",
                "dave.smith@example.com",
                120,
                New AuthenticationCredential("testCredential", True, "testParamName"))
            Dim roundTrip = JwtConfiguration.FromXElement(config.ToXElement())
            roundTrip.ShouldBeEquivalentTo(config)

        End Sub

        <Test>
        Public Sub JwtConfigurationXElementRoundTrip_InvalidElementName_ShouldThrowException()
            Dim jwtConfig = <justWrong
                                algorithm=<%= "RS256" %>
                                audience=<%= New Uri("http://testServer.com") %>
                                scope=<%= "api/scope" %>
                                subject=<%= "dave.smith@example.com" %>
                                jwtexpiry=<%= 120 %>
                                <%= New AuthenticationCredential(
                                    "testCredential", True, "testParamName").ToXElement %>>
                            </justWrong>
            Assert.That(Sub() JwtConfiguration.FromXElement(jwtConfig),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub JwtConfigurationXElementRoundTrip_NoAlgorithm_ShouldThrowException()
            Dim jwtConfig = <jwtconfiguration
                                audience=<%= New Uri("http://testServer.com") %>
                                scope=<%= "api/scope" %>
                                subject=<%= "dave.smith@example.com" %>
                                jwtexpiry=<%= 120 %>
                                <%= New AuthenticationCredential(
                                    "testCredential", True, "testParamName").ToXElement %>>
                            </jwtconfiguration>

            Assert.DoesNotThrow(Sub() JwtConfiguration.FromXElement(jwtConfig))
            Assert.That(JwtConfiguration.FromXElement(jwtConfig).Algorithm, Iz.EqualTo("RS256"))
        End Sub

        <Test>
        Public Sub JwtConfigurationXElementRoundTrip_NoAudience_ShouldThrowException()
            Dim jwtConfig = <jwtconfiguration
                                algorithm=<%= "RS256" %>
                                scope=<%= "api/scope" %>
                                subject=<%= "dave.smith@example.com" %>
                                jwtexpiry=<%= 120 %>
                                <%= New AuthenticationCredential(
                                    "testCredential", True, "testParamName").ToXElement %>>
                            </jwtconfiguration>
            Assert.That(Sub() JwtConfiguration.FromXElement(jwtConfig),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub JwtConfigurationXElementRoundTrip_NoScope_ShouldThrowException()
            Dim jwtConfig = <jwtconfiguration
                                algorithm=<%= "RS256" %>
                                audience=<%= New Uri("http://testServer.com") %>
                                subject=<%= "dave.smith@example.com" %>
                                jwtexpiry=<%= 120 %>
                                <%= New AuthenticationCredential(
                                    "testCredential", True, "testParamName").ToXElement %>>
                            </jwtconfiguration>
            Assert.That(Sub() JwtConfiguration.FromXElement(jwtConfig),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub JwtConfigurationXElementRoundTrip_NoSubject_ShouldThrowException()
            Dim jwtConfig = <jwtconfiguration
                                algorithm=<%= "RS256" %>
                                audience=<%= New Uri("http://testServer.com") %>
                                scope=<%= "api/scope" %>
                                jwtexpiry=<%= 120 %>
                                <%= New AuthenticationCredential(
                                    "testCredential", True, "testParamName").ToXElement %>>
                            </jwtconfiguration>
            Assert.That(Sub() JwtConfiguration.FromXElement(jwtConfig),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub JwtConfigurationXElementRoundTrip_JwtExpiry_ShouldThrowException()
            Dim jwtConfig = <jwtconfiguration
                                algorithm=<%= "RS256" %>
                                audience=<%= New Uri("http://testServer.com") %>
                                scope=<%= "api/scope" %>
                                subject=<%= "dave.smith@example.com" %>
                                <%= New AuthenticationCredential(
                                    "testCredential", True, "testParamName").ToXElement %>>
                            </jwtconfiguration>
            Assert.That(Sub() JwtConfiguration.FromXElement(jwtConfig),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub JwtConfigurationXElementRoundTrip_NoCredential_ShouldThrowException()
            Dim jwtConfig = <jwtconfiguration
                                algorithm=<%= "RS256" %>
                                audience=<%= New Uri("http://testServer.com") %>
                                scope=<%= "api/scope" %>
                                subject=<%= "dave.smith@example.com" %>
                                jwtexpiry=<%= 120 %>>
                            </jwtconfiguration>
            Assert.That(Sub() JwtConfiguration.FromXElement(jwtConfig),
                        Throws.TypeOf(Of MissingXmlObjectException))
        End Sub

        <Test>
        Public Sub NoBodyContentXElementRoundTrip_ShouldReturnSameObject()
            Dim testContent = New NoBodyContent()

            Dim roundTrip = NoBodyContent.FromXElement(testContent.ToXElement())
            roundTrip.ShouldBeEquivalentTo(testContent)
        End Sub


        <Test>
        Public Sub NoBodyContentXElementRoundTrip_InvalidElementName_ShouldThrowException()
            Dim testContent = <invalid type="0"></invalid>
            Assert.Throws(Of MissingXmlObjectException)(Sub() NoBodyContent.
                                                                    FromXElement(testContent))
        End Sub

        <Test>
        Public Sub NoBodyContentXElementRoundTrip_MissingType_ShouldThrowException()
            Dim testContent = <bodycontent></bodycontent>
            Assert.Throws(Of MissingXmlObjectException)(Sub() NoBodyContent.
                                                                    FromXElement(testContent))
        End Sub

        <Test>
        Public Sub TemplateBodyContentXElementRoundTrip_ShouldReturnSameObject()
            Dim testContent = New TemplateBodyContent("{""param1"": 200}")

            Dim roundTrip = TemplateBodyContent.FromXElement(testContent.ToXElement())
            roundTrip.ShouldBeEquivalentTo(testContent)
        End Sub

        <Test>
        Public Sub TemplateBodyContentXElementRoundtrip_InvalidElement_ShouldThrowException()
            Dim testContent = <invalid type="1">
                                  <template><![CDATA[{"param1": 200}]]></template>
                              </invalid>

            Assert.Throws(Of MissingXmlObjectException)(Sub() TemplateBodyContent.
                                                                    FromXElement(testContent))
        End Sub

        <Test>
        Public Sub TemplateBodyContentXElementRoundTrip_MissingType_ShouldThrowException()
            Dim testContent = <bodycontent>
                                  <template><![CDATA[{"param1": 200}]]></template>
                              </bodycontent>

            Assert.Throws(Of MissingXmlObjectException)(Sub() TemplateBodyContent.
                                                                    FromXElement(testContent))
        End Sub

        <Test>
        Public Sub TemplateBodyContentXElementRoundTrip_MissingTemplate_ShouldThrowException()
            Dim testContent =
                    <bodycontent type="1">
                    </bodycontent>

            Assert.Throws(Of MissingXmlObjectException)(Sub() TemplateBodyContent.
                                                                    FromXElement(testContent))
        End Sub

        <TestCaseSource("GetWhiteSpaceStrings")>
        Public Sub TemplateBodyContentXElementRoundTrip_TemplateHasWhiteSpace_ShouldReturnSameObject(template As String)
            Dim testContent = New TemplateBodyContent(template)

            Dim roundTrip = TemplateBodyContent.FromXElement(testContent.ToXElement())
            roundTrip.ShouldBeEquivalentTo(testContent)

        End Sub

        <Test>
        Public Sub TemplateBodyContenXElementtRoundTrip_TemplateContainsXml_ShouldReturnSameObject()
            Dim testContent = New TemplateBodyContent("<sometemplate someattribute=""somevalue"">some text</sometemplate>")

            Dim roundTrip = TemplateBodyContent.FromXElement(testContent.ToXElement())
            roundTrip.ShouldBeEquivalentTo(testContent)

        End Sub

        <Test>
        Public Sub SingleFileBodyContentXElementRoundTrip_ShouldReturnSameObject()
            Dim testContent = New SingleFileBodyContent("Single file test parameter name")

            Dim roundTrip = SingleFileBodyContent.FromXElement(testContent.ToXElement())
            roundTrip.ShouldBeEquivalentTo(testContent)
        End Sub

        <Test>
        Public Sub SingleFileBodyContentXElementRoundTrip_InvalidElement_ShouldThrowException()
            Dim testContent =
                    <invalid type="2"
                        fileinputparametername="Single file test parameter name"/>
            Assert.Throws(Of MissingXmlObjectException)(Sub() SingleFileBodyContent.
                                                                    FromXElement(testContent))
        End Sub

        <Test>
        Public Sub SingleFileBodyContentXElementRoundTrip_MissingType_ShouldThrowException()
            Dim testContent =
                    <bodycontent
                        fileinputparametername="Single file test parameter name"/>
            Assert.Throws(Of MissingXmlObjectException)(Sub() SingleFileBodyContent.
                                                                    FromXElement(testContent))
        End Sub

        <Test>
        Public Sub SingleFileBodyContentXElementRoundTrip_MissingFileInputParameterName_ShouldThrowException()
            Dim testContent =
                    <bodycontent
                        type="2"/>

            Assert.Throws(Of MissingXmlObjectException)(Sub() SingleFileBodyContent.
                                                                    FromXElement(testContent))
        End Sub

        <Test>
        Public Sub FileCollectionBodyContentXElementRoundTrip_ShouldReturnSameObject()
            Dim testContent = New FileCollectionBodyContent("File collection test parameter name")
            Dim roundTrip = FileCollectionBodyContent.FromXElement(testContent.ToXElement())
            roundTrip.ShouldBeEquivalentTo(testContent)
        End Sub

        <Test>
        Public Sub FileCollectionBodyContentXElementRoundTrip_InvalidElement_ShouldThrowException()
            Dim testContent =
                    <invalid type="3"
                        fileinputparametername="Single file test parameter name"/>
            Assert.Throws(Of MissingXmlObjectException)(Sub() FileCollectionBodyContent.
                                                                FromXElement(testContent))
        End Sub

        <Test>
        Public Sub FileCollectionBodyContentXElementRoundTrip_MissingType_ShouldThrowException()
            Dim testContent =
                    <bodycontent
                        fileinputparametername="Single file test parameter name"
                    />
            Assert.Throws(Of MissingXmlObjectException)(Sub() FileCollectionBodyContent.
                                                                    FromXElement(testContent))
        End Sub

        <Test>
        Public Sub FileCollectionBodyContentXElementRoundTrip_MissingFileInputParameterName_ShouldThrowException()
            Dim testContent =
                    <bodycontent
                        type="3"/>

            Assert.Throws(Of MissingXmlObjectException)(Sub() FileCollectionBodyContent.
                                                                    FromXElement(testContent))
        End Sub

        <Test>
        Public Sub CustomCodeBodyContentXElementRoundTrip_ShouldReturnSameObject()
            Dim testContent = New CustomCodeBodyContent("Content = """";")

            Dim roundTrip = CustomCodeBodyContent.FromXElement(testContent.ToXElement())
            roundTrip.ShouldBeEquivalentTo(testContent)
        End Sub

        <Test>
        Public Sub CustomCodeBodyContentXElementRoundtrip_InvalidElement_ShouldThrowException()
            Dim testContent = <invalid type="1">
                                  <code><![CDATA[Content = """";]]></code>
                              </invalid>

            Assert.Throws(Of MissingXmlObjectException)(Sub() CustomCodeBodyContent.
                                                                    FromXElement(testContent))
        End Sub

        <Test>
        Public Sub CustomCodeBodyContentXElementRoundTrip_MissingType_ShouldThrowException()
            Dim testContent = <bodycontent>
                                  <code><![CDATA[{Content = """";]]></code>
                              </bodycontent>

            Assert.Throws(Of MissingXmlObjectException)(Sub() CustomCodeBodyContent.
                                                                    FromXElement(testContent))
        End Sub

        <Test>
        Public Sub CustomCodeBodyContentXElementRoundTrip_MissingCode_ShouldThrowException()
            Dim testContent =
                    <bodycontent type="4">
                    </bodycontent>

            Assert.Throws(Of MissingXmlObjectException)(Sub() CustomCodeBodyContent.
                                                                    FromXElement(testContent))
        End Sub

        <TestCaseSource("GetWhiteSpaceStrings")>
        Public Sub CustomCodeBodyContentXElementRoundTrip_CustomCodeHasWhiteSpace_ShouldReturnSameObject(template As String)
            Dim testContent = New CustomCodeBodyContent(template)

            Dim roundTrip = CustomCodeBodyContent.FromXElement(testContent.ToXElement())
            roundTrip.ShouldBeEquivalentTo(testContent)

        End Sub

        <Test>
        Public Sub OutputParameterConfigurationXElementRoundTrip_NoOutputParameters_ShouldReturnSameObject()

            Dim configuration = New OutputParameterConfiguration({}, "")

            Dim roundTrip = OutputParameterConfiguration.FromXElement(configuration.ToXElement())
            roundTrip.ShouldBeEquivalentTo(configuration)
        End Sub

        <TestCaseSource("GetWhiteSpaceStrings")>
        Public Sub OutputParameterConfigurationXElementRoundTrip_WhiteSpaceInCode_ShouldReturnSameObject(code As String)

            Dim configuration = New OutputParameterConfiguration({}, code)

            Dim roundTrip = OutputParameterConfiguration.FromXElement(configuration.ToXElement())
            roundTrip.ShouldBeEquivalentTo(configuration)
        End Sub

        <TestCaseSource("GetSpecialCharacterStrings")>
        Public Sub OutputParameterConfigurationXElementRoundTrip_SpecialCharactersInCode_ShouldReturnSameObject(code As String)

            Dim configuration = New OutputParameterConfiguration({}, code)

            Dim roundTrip = OutputParameterConfiguration.FromXElement(configuration.ToXElement())
            roundTrip.ShouldBeEquivalentTo(configuration)
        End Sub

        <Test>
        Public Sub OutputParameterConfigurationXElementRoundTrip_InvalidElementName_ShouldThrowException()
            Dim configuration = <invalid>
                                    <code><![CDATA[Some text]]></code>
                                    <parameters></parameters>
                                </invalid>

            Assert.Throws(Of MissingXmlObjectException)(Sub() OutputParameterConfiguration.
                                                                    FromXElement(configuration))
        End Sub

        <Test>
        Public Sub OutputParameterConfigurationXElementRoundTrip_MissingCode_ShouldThrowException()
            Dim configuration = <outputparameters>
                                    <parameters></parameters>
                                </outputparameters>

            Assert.Throws(Of MissingXmlObjectException)(Sub() OutputParameterConfiguration.
                                                                    FromXElement(configuration))
        End Sub

        <Test>
        Public Sub OutputParameterConfigurationXElementRoundTrip_MissingParameters_ShouldThrowException()
            Dim configuration = <outputparameters>
                                    <code><![CDATA[Some text]]></code>
                                </outputparameters>

            Assert.Throws(Of MissingXmlObjectException)(Sub() OutputParameterConfiguration.
                                                                    FromXElement(configuration))
        End Sub
        <Test>
        Public Sub ConfigurationSettings_XElementRoundTrip_ShouldReturnSameObject()
            Dim testConfiguration = New WebApiConfigurationSettings(1, 2)

            Dim roundTrip = WebApiConfigurationSettings.FromXElement(testConfiguration.ToXElement())
            roundTrip.ShouldBeEquivalentTo(testConfiguration)
        End Sub

        <Test>
        Public Sub ConfigurationSettings_XElementRoundTrip_MissingRequestTimeout_ThrowsExceptions()
            Dim testConfiguration = <configurationsettings
                                        authserverrequesttimeout=<%= 2 %>/>

            Assert.Throws(Of MissingXmlObjectException)(Sub() WebApiConfigurationSettings.FromXElement(testConfiguration))
        End Sub

        <Test>
        Public Sub ConfigurationSettings_XElementRoundTrip_MissingAuthServerRequestTimeout_ThrowsExceptions()
            Dim testConfiguration = <configurationsettings
                                        requesttimeout=<%= 1 %>/>

            Assert.Throws(Of MissingXmlObjectException)(Sub() WebApiConfigurationSettings.FromXElement(testConfiguration))
        End Sub

        <Test>
        Public Sub JsonPathParameterXElementRoundTrip_ShouldReturnSameObject()
            Dim parameter As New JsonPathOutputParameter("Some name", "Description", "$", DataType.flag)
            Dim roundTrip = JsonPathOutputParameter.FromXElement(parameter.ToXElement())
            roundTrip.ShouldBeEquivalentTo(parameter)
        End Sub

        <Test>
        Public Sub JsonPathParameterXElementRoundTrip_InvalidElement_ShouldThrowException()
            Dim customOutputParameter = <invalid
                                            type="1"
                                            name="Some name"
                                            path="$"
                                            datatype="1">
                                        </invalid>
            Assert.Throws(Of MissingXmlObjectException)(Sub() JsonPathOutputParameter.
                                                                    FromXElement(customOutputParameter))
        End Sub

        <Test>
        Public Sub JsonPathParameterXElementRoundTrip_MissingName_ShouldThrowException()
            Dim customOutputParameter = <customoutputparameter
                                            type="1"
                                            path="$"
                                            datatype="1">
                                        </customoutputparameter>
            Assert.Throws(Of MissingXmlObjectException)(Sub() JsonPathOutputParameter.
                                                                    FromXElement(customOutputParameter))
        End Sub

        <Test>
        Public Sub JsonPathParameterXElementRoundTrip_MissingPath_ShouldThrowException()
            Dim customOutputParameter = <customoutputparameter
                                            type="1"
                                            name="Some name"
                                            datatype="1">
                                        </customoutputparameter>
            Assert.Throws(Of MissingXmlObjectException)(Sub() JsonPathOutputParameter.
                                                                    FromXElement(customOutputParameter))
        End Sub

        <Test>
        Public Sub JsonPathParameterXElementRoundTrip_MisssingDataType_ShouldThrowException()
            Dim customOutputParameter = <customoutputparameter
                                            type="1"
                                            path="$"
                                            name="Some name">
                                        </customoutputparameter>
            Assert.Throws(Of MissingXmlObjectException)(Sub() JsonPathOutputParameter.
                                                                    FromXElement(customOutputParameter))
        End Sub

        <Test>
        Public Sub JsonPathParameterXElementRoundTrip_InvalidDataType_ShouldThrowException()
            Dim customOutputParameter = <customoutputparameter
                                            type="1"
                                            name="Some name"
                                            path="$"
                                            datatype="invalid">
                                        </customoutputparameter>
            Assert.Throws(Of InvalidArgumentException)(Sub() JsonPathOutputParameter.
                                                                    FromXElement(customOutputParameter))
        End Sub

        <Test>
        Public Sub CustomCodeParameterXElementRoundTrip_ShouldReturnSameObject()
            Dim parameter As New CustomCodeOutputParameter("Some name", "Description", DataType.flag)
            Dim roundTrip = CustomCodeOutputParameter.FromXElement(parameter.ToXElement())
            roundTrip.ShouldBeEquivalentTo(parameter)
        End Sub

        <Test>
        Public Sub CustomCodeParameterXElementRoundTrip_InvalidElement_ShouldThrowException()
            Dim customOutputParameter = <invalid
                                            type="1"
                                            name="Some name"
                                            datatype="1">
                                        </invalid>
            Assert.Throws(Of MissingXmlObjectException)(Sub() CustomCodeOutputParameter.
                                                                    FromXElement(customOutputParameter))
        End Sub

        <Test>
        Public Sub CustomCodeParameterXElementRoundTrip_MissingName_ShouldThrowException()
            Dim customOutputParameter = <customoutputparameter
                                            type="1"
                                            datatype="1">
                                        </customoutputparameter>
            Assert.Throws(Of MissingXmlObjectException)(Sub() CustomCodeOutputParameter.
                                                                    FromXElement(customOutputParameter))
        End Sub

        <Test>
        Public Sub CustomCodeParameterXElementRoundTrip_MisssingDataType_ShouldThrowException()
            Dim customOutputParameter = <customoutputparameter
                                            type="1"
                                            name="Some name">
                                        </customoutputparameter>
            Assert.Throws(Of MissingXmlObjectException)(Sub() CustomCodeOutputParameter.
                                                                    FromXElement(customOutputParameter))
        End Sub

        <Test>
        Public Sub CustomCodeParameterXElementRoundTrip_InvalidDataType_ShouldThrowException()
            Dim customOutputParameter = <customoutputparameter
                                            type="1"
                                            name="Some name"
                                            datatype="invalid">
                                        </customoutputparameter>
            Assert.Throws(Of InvalidArgumentException)(Sub() CustomCodeOutputParameter.
                                                                    FromXElement(customOutputParameter))
        End Sub




#End Region

#Region " Helper Methods "

        Protected Shared Iterator Function GetWhiteSpaceStrings() As IEnumerable(Of String)
            Yield "Some " & vbCrLf & "Text"
            Yield "Some " & vbTab & "Text"
            Yield "Some " & vbCr & "Text"
            Yield " "
            Yield "Some  Text"
            Yield "Some Text "
            Yield " Some Text"
            Yield vbTab & " "
            Yield " " & vbTab
        End Function

        Protected Shared Iterator Function GetSpecialCharacterStrings() As IEnumerable(Of String)
            Yield "#^&_[]()=@*^%$'¬`;:><,+-"
            Yield "ÄäÖöÜüß"
            Yield "\0\r\n\t\r\n"
            Yield "🙃 ⊕ ⊖ ⊗ ⊝ ば ぶ ぺ 〄 ° ઊ હ ஞ ଲ ⊙ ﹏ 🕴"
        End Function

        Protected Shared Iterator Function GetProcessValues() As IEnumerable(Of clsProcessValue)
            Yield New clsProcessValue("Some string")
            Yield New clsProcessValue(3)
            Yield New clsProcessValue(True)
            Yield New clsProcessValue("Some password".ToSecureString())

            Dim bitmap = New TestBitmapGenerator().
                    WithColour("R"c, Color.Red).
                    WithColour("W"c, Color.White).
                    WithPixels("RWWWWWWR").Create()
            Yield New clsProcessValue(bitmap)

            Dim converter = New ImageConverter()
            Dim imageBytes = CType(converter.ConvertTo(bitmap, GetType(Byte())), Byte())
            Yield New clsProcessValue(imageBytes)

            Yield New clsProcessValue(New TimeSpan(1, 2, 3))
            Yield New clsProcessValue(DataType.datetime, DateTime.UtcNow)
            Yield New clsProcessValue(DataType.date, DateTime.UtcNow)
        End Function

        Private Function GetSampleValidRequestXML() As XElement
            Return _
                <request
                    httpmethod=<%= HttpMethod.Options %>
                    urlpath="/action1/{param1}"
                    bodytype=<%= WebApiRequestBodyType.Template %>>
                    <headers/>
                    <bodytemplate><![CDATA[some text]]></bodytemplate>
                </request>
        End Function

        Private Function GetFullyPopulatedWebApiConfiguration() As WebApiConfiguration
            Return New WebApiConfigurationBuilder().
                WithBaseUrl("https://www.myapi.org/").
                WithAction("Action1",
                    HttpMethod.Get,
                    "/action1",
                    "This is the description",
                    False,
                    False,
                    False,
                    New TemplateBodyContent("{parameter 1}"),
                    {
                        New HttpHeader("username", "password"),
                        New HttpHeader("special-header", "value 3")
                    },
                    {
                        New ActionParameter(
                            "Parameter 1", "This parameter does Nothing",
                            DataType.text, False, String.Empty),
                        New ActionParameter(
                            "Parameter 2", String.Empty,
                            DataType.text, True, "562789238")
                    },
                           New OutputParameterConfiguration({New JsonPathOutputParameter("Test", "Description", "$.TestPath", DataType.text),
                                                             New CustomCodeOutputParameter("Test 1", "Description", DataType.date)}, "some code")
                ).
                WithHeader("custom-header-1", "value 1").
                WithHeader("custom-header-2", "value 2").
                WithParameters({
                    New ActionParameter(
                        "Common Parameter 1", "This parameter does Nothing",
                        DataType.text, False, String.Empty),
                    New ActionParameter(
                        "Common Parameter 2", String.Empty,
                        DataType.text, True, "456")}).
                WithCommonAuthentication(
                    New BasicAuthentication(
                        New AuthenticationCredential("testcred", True, "testParamName"),
                        True)).
                WithCommonCode(
                    New CodeProperties(
                        "This is some code hahahaha",
                        CodeLanguage.CSharp,
                        {"namespace1", "namespace2"},
                        {"reference1", "reference2"})).
                WithConfigurationSettings(
                    New WebApiConfigurationSettings(1, 2)).
            Build
        End Function



#End Region


    End Class
End Namespace

#End If
