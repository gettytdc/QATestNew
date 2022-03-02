#If UNITTESTS Then

Imports BluePrism.AutomateProcessCore.WebApis
Imports NUnit.Framework
Imports FluentAssertions

Namespace WebApis

    <TestFixture, Category("Web APIs")>
    Public Class WebApiConfigurationSettingsTests

        <Test>
        Public Sub DefaultContructor_InitialisedWithExpectedDefaults()
            Dim classUnderTest = New WebApiConfigurationSettings()

            classUnderTest.HttpRequestConnectionTimeout.Should().Be(10)
            classUnderTest.AuthServerRequestConnectionTimeout.Should().Be(10)
        End Sub

        <Test>
        Public Sub Contructor_InitialisedWithExpectedValues()
            Dim classUnderTest = New WebApiConfigurationSettings(4, 3)

            classUnderTest.HttpRequestConnectionTimeout.Should().Be(4)
            classUnderTest.AuthServerRequestConnectionTimeout.Should().Be(3)
        End Sub
    End Class

End Namespace

#End If