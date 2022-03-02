#If UNITTESTS Then

Imports BluePrism.AutomateProcessCore.WebApis
Imports NUnit.Framework
Imports FluentAssertions
Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.WebApis.Authentication

Namespace WebApis

    <TestFixture, Category("Web APIs")>
    Public Class WebApiConfigurationTests

        <Test>
        Public Sub Constructor_WithNonUniqueSharedParameterNames_ShouldThrow()

            Dim builder As New WebApiConfigurationBuilder
            builder.WithParameters({
                New ActionParameter("Name 1", "", DataType.text, True, ""),
                New ActionParameter("Name 1", "", DataType.text, True, ""),
                New ActionParameter("Name 2", "", DataType.text, True, ""),
                New ActionParameter("Name 2", "", DataType.text, True, ""),
                New ActionParameter("Name 3", "", DataType.text, True, "")
            })
            AssertConstructorThrows(Of ArgumentException)(builder, "Common parameters contain non-unique names: Name 1, Name 2")

        End Sub

        <Test>
        Public Sub Constructor_WithNonUniqueActionNames_ShouldThrow()

            Dim builder As New WebApiConfigurationBuilder
            builder.
                WithAction("Action1", HttpMethod.Get, "/action1").
                WithAction("Action1", HttpMethod.Get, "/action1").
                WithAction("Action2", HttpMethod.Get, "/action2").
                WithAction("Action2", HttpMethod.Get, "/action2").
                WithAction("Action3", HttpMethod.Get, "/action3")

            AssertConstructorThrows(Of ArgumentException)(builder, "Actions contain non-unique names: Action1, Action2")

        End Sub

        <Test>
        Public Sub Constructor_ActionsWithNonUniqueParameters_ShouldThrow()

            Dim builder As New WebApiConfigurationBuilder
            Dim parameters = {
                New ActionParameter("Parameter 1", "", DataType.text, True, ""),
                New ActionParameter("Parameter 1", "", DataType.text, True, ""),
                New ActionParameter("Parameter 2", "", DataType.text, True, ""),
                New ActionParameter("Parameter 2", "", DataType.text, True, ""),
                New ActionParameter("Parameter 3", "", DataType.text, True, "")
            }
            builder.
                WithAction("Action1", HttpMethod.Get, "/action1", parameters:=parameters).
                WithAction("Action2", HttpMethod.Get, "/action1", parameters:=parameters)

            AssertConstructorThrows(Of ArgumentException)(builder, "Actions contain parameters with non-unique names: Action1 (Parameter 1, Parameter 2), Action2 (Parameter 1, Parameter 2)")

        End Sub

        <Test>
        Public Sub Constructor_ActionsWithParametersDuplicatingSharedParameters_ShouldThrow()

            Dim builder As New WebApiConfigurationBuilder
            Dim sharedParameters = {
                New ActionParameter("Parameter 1", "", DataType.text, True, ""),
                New ActionParameter("Parameter 2", "", DataType.text, True, ""),
                New ActionParameter("Parameter 3", "", DataType.text, True, "")
            }
            Dim actionParameters = {
                New ActionParameter("Parameter 1", "", DataType.text, True, ""),
                New ActionParameter("Parameter 2", "", DataType.text, True, ""),
                New ActionParameter("Parameter 4", "", DataType.text, True, "")
            }
            builder.
                WithParameters(sharedParameters).
                WithAction("Action1", HttpMethod.Get, "/action1", parameters:=actionParameters).
                WithAction("Action2", HttpMethod.Get, "/action1", parameters:=actionParameters)

            AssertConstructorThrows(Of ArgumentException)(builder, "Actions contain parameters with non-unique names: Action1 (Parameter 1, Parameter 2), Action2 (Parameter 1, Parameter 2)")

        End Sub

        Private Function AssertConstructorThrows(Of TException As Exception)(builder As WebApiConfigurationBuilder,
                                                                             Optional message As String = Nothing) As TException

            Try
                builder.Build()
            Catch ex As Exception
                ex.Should.BeOfType(Of TException)()

                If message IsNot Nothing Then
                    ex.Message.Should.StartWith(message)
                End If
                Return CType(ex, TException)
            End Try

            Assert.Fail($"Expected {GetType(TException).Name} not thrown")
            Return Nothing

        End Function



        <Test>
        Public Sub Constructor_BasicAuthenticationWithParameterDuplicatingActionParameters_ShouldThrow()

            Dim builder As New WebApiConfigurationBuilder
            Dim sharedParameters = {
                                       New ActionParameter("Parameter 1", "", DataType.text, True, ""),
                                       New ActionParameter("Parameter 2", "", DataType.text, True, "")
                                   }
            Dim actionParameters = {
                                       New ActionParameter("Parameter 3", "", DataType.text, True, ""),
                                       New ActionParameter("Parameter 4", "", DataType.text, True, "")
                                   }
            Dim authcredential = New AuthenticationCredential("Test Credential", True, "Parameter 3")

            builder.
                WithParameters(sharedParameters).
                WithAction("Action1", HttpMethod.Get, "/action1", parameters:=actionParameters).
                WithCommonAuthentication(New BasicAuthentication(authcredential, True))

            AssertConstructorThrows(Of ArgumentException)(builder,
                                                           "Authentication parameters contain non-unique names: Parameter 3
Parameter name: commonAuthentication")
        End Sub

        <Test>
        Public Sub Constructor_BasicAuthentication_UniqueNameValidationShouldAllowDifferentActionsToHaveSameParameterNames()

            Dim builder As New WebApiConfigurationBuilder
            Dim sharedParameters = {
                                       New ActionParameter("Parameter 1", "", DataType.text, True, ""),
                                       New ActionParameter("Parameter 2", "", DataType.text, True, "")
                                   }
            Dim action1Parameters = {
                                        New ActionParameter("Parameter 3", "", DataType.text, True, ""),
                                        New ActionParameter("Parameter 4", "", DataType.text, True, "")
                                    }
            Dim action2Parameters = {
                                        New ActionParameter("Parameter 3", "", DataType.text, True, ""),
                                        New ActionParameter("Parameter 4", "", DataType.text, True, "")
                                    }
            Dim authcredential = New AuthenticationCredential("Test Credential", True, "Credential Parameter")

            builder.
                WithParameters(sharedParameters).
                WithAction("Action1", HttpMethod.Get, "/action1", parameters:=action1Parameters).
                WithAction("Action2", HttpMethod.Get, "/action2", parameters:=action2Parameters).
                WithCommonAuthentication(New BasicAuthentication(authcredential, True))


            Dim d = builder.Build()
            d.Should.NotBeNull()


        End Sub
    End Class

End Namespace

#End If