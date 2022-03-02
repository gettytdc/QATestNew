#If UNITTESTS

Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports BluePrism.Common.Security
Imports FluentAssertions
Imports NUnit.Framework

Namespace WebApis.RequestHandling

    Public Class BasicAuthenticationHelperTests

        <TestCase("UTF-8")>
        <TestCase("ASCII")>
        Public Sub GetAuthorizeHeaderValue_WithEncoding_ReturnsValidHeader(encodingName As String)

            Dim encoding = System.Text.Encoding.GetEncoding(encodingName)
            Dim userName = "joebloggs"
            Dim password = "password".AsSecureString()

            Dim basicAuthValue =
                    BasicAuthenticationHelper.GetAuthorizationHeaderValue(userName, password, encoding)

            Dim headerValuePrefix = basicAuthValue.Substring(0, 6)
            headerValuePrefix.Should.Be("Basic ")

            Dim base64Credentials = basicAuthValue.Substring(6, basicAuthValue.Length - 6)
            Dim decodedCredentials = encoding.GetString(Convert.FromBase64String(base64Credentials))
            decodedCredentials.Should.Be("joebloggs:password")

        End Sub

    End Class
End Namespace

#End If
