#If UNITTESTS Then

Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling
Imports FluentAssertions
Imports NUnit.Framework

Namespace WebApis.RequestHandling

    Public Class EmptyAuthenticationHandlerTests

        <Test>
        Public Sub GetCredentialParameters_ShouldBeEmpty()
            Dim handler As New EmptyAuthenticationHandler()
            Dim configuration = New WebApiConfigurationBuilder().
                WithCommonAuthentication(New EmptyAuthentication()).
                WithAction("Action1", HttpMethod.Get, "/api/action1").
                Build()

            Dim context As New ActionContext(
                Guid.NewGuid(), configuration, "Action1", New Dictionary(Of String, clsProcessValue),
                New clsSession(Guid.NewGuid, 105, New WebConnectionSettings(5, 5, 5, New List(Of UriWebConnectionSettings)())))
            handler.GetCredentialParameters(context).Should().BeEmpty()
        End Sub
    End Class
End Namespace

#End If
