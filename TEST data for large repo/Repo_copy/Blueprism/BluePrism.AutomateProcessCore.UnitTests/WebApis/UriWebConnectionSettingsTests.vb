#If UNITTESTS Then

Imports FluentAssertions
Imports NUnit.Framework

Public Class UriWebConnectionSettingsTests

    Const googleUri = "http://www.google.com"

    <Test>
    Public Sub DefaultConstructor_AppliesDefaultSettings()
        Dim classUnderTest = New UriWebConnectionSettings("http://www.google.com")

        classUnderTest.BaseUri.Should().Be(New Uri("http://www.google.com"))
        classUnderTest.ConnectionLimit.Should().Be(2)
        classUnderTest.ConnectionLeaseTimeout.Should().BeNull()
        classUnderTest.MaxIdleTime.Should().Be(5)
    End Sub

    <Test>
    Public Sub ExplicitConstructor_AppliesSettings()
        Dim classUnderTest = New UriWebConnectionSettings("http://www.google.com", 2, 2, 5)

        classUnderTest.BaseUri.Should().Be(New Uri("http://www.google.com"))
        classUnderTest.ConnectionLimit.Should().Be(2)
        classUnderTest.ConnectionLeaseTimeout.Should().Be(2)
        classUnderTest.MaxIdleTime.Should().Be(5)
    End Sub

    <TestCase("https://www.google.com", 100, 2, 329)>
    <TestCase("https://www.google.com/", 100, 2, 329)>
    <TestCase("https://localhost", 100, 2, 329)>
    <TestCase("https://localhost:9000", 100, 2, 329)>
    Public Sub NewConnectionSettings_ValidURI_Passes(uri As String, connLimit As Integer, connTimeout As Integer?, maxIdleTime As Integer)
        Dim classUnderTest = New UriWebConnectionSettings(uri, connLimit, connTimeout, maxIdleTime)

        classUnderTest.Should().NotBeNull()
    End Sub

    <Test>
    Public Sub NewConnectionSettings_ConnectionTimeoutIsNull_Passes()
        Dim classUnderTest = New UriWebConnectionSettings("http://www.google.com", 2, Nothing, 5)

        classUnderTest.Should().NotBeNull()
    End Sub

    <Test>
    Public Sub NewConnectionSettings_IncompleteURI_ShouldThrowException()
        Dim action = New Action(Function() New UriWebConnectionSettings("www.google.com", 2, 2, 5))

        action.ShouldThrow(Of ArgumentException)
    End Sub

    <Test>
    Public Sub NewConnectionSettings_NoURI_ShouldThrowException()
        Dim action = New Action(Function() New UriWebConnectionSettings("", 2, 2, 5))

        action.ShouldThrow(Of ArgumentException)
    End Sub

    <Test>
    Public Sub NewConnectionSettings_InvalidURI_ShouldThrowException()
        Dim action = New Action(Function() New UriWebConnectionSettings("http://www.google.com/whatever", 2, 2, 5))

        action.ShouldThrow(Of ArgumentException)
    End Sub

    <Test>
    Public Sub NewConnectionSettings_ConnectionLimitBelowMinimumValue_ShouldThrowException()
        Dim action = New Action(Function() New UriWebConnectionSettings(googleUri, 0, 2, 5))

        action.ShouldThrow(Of ArgumentException)
    End Sub

    <Test>
    Public Sub NewConnectionSettings_ConnectionTimeoutBelowMinimumValue_ShouldThrowException()
        Dim action = New Action(Function() New UriWebConnectionSettings(googleUri, 2, 0, 5))

        action.ShouldThrow(Of ArgumentException)
    End Sub

    <Test>
    Public Sub NewConnectionSettings_MaxIdleTimeBelowMinimumValue_ShouldThrowException()
        Dim action = New Action(Function() New UriWebConnectionSettings(googleUri, 2, 2, 0))

        action.ShouldThrow(Of ArgumentException)
    End Sub

    <Test>
    Public Sub TestEquals_DifferentConnectionLimit_ReturnsFalse()
        Dim settings1 = New UriWebConnectionSettings(googleUri, 2, 2, 5)
        Dim settings2 = New UriWebConnectionSettings(googleUri, 7, 2, 5)

        Assert.IsFalse(settings1.Equals(settings2))
    End Sub

    <Test>
    Public Sub TestEquals_DifferentConnectionTimeout_ReturnsFalse()
        Dim settings1 = New UriWebConnectionSettings(googleUri, 2, 7, 5)
        Dim settings2 = New UriWebConnectionSettings(googleUri, 2, 2, 5)

        Assert.IsFalse(settings1.Equals(settings2))
    End Sub

    <Test>
    Public Sub TestEquals_EmptyConnectionTimeout_DoesNotThrow()
        Dim settings1 = New UriWebConnectionSettings(googleUri, 2, Nothing, 5)
        Dim settings2 = New UriWebConnectionSettings(googleUri, 2, Nothing, 5)

        Assert.DoesNotThrow(Sub() settings1.Equals(settings2))
    End Sub

    <Test>
    Public Sub TestEquals_DifferentMaxIdleTime_ReturnsFalse
        Dim settings1 = New UriWebConnectionSettings(googleUri, 2, 2, 5)
        Dim settings2 = New UriWebConnectionSettings(googleUri, 2, 2, 6)

        Assert.IsFalse(settings1.Equals(settings2))
    End Sub

    <Test>
    Public Sub TestEquals_DifferentUri_ReturnsFalse

        Dim settings1 = New UriWebConnectionSettings("http://notGoogle.co.uk", 2, 2, 5)
        Dim settings2 = New UriWebConnectionSettings(googleUri, 2, 2, 5)

        Assert.IsFalse(settings1.Equals(settings2))
    End Sub

    <Test>
    Public Sub TestEquals_SameSettings_ReturnsTrue

        Dim settings1 = New UriWebConnectionSettings(googleUri, 2, 2, 5)
        Dim settings2 = New UriWebConnectionSettings(googleUri, 2, 2, 5)

        Assert.IsTrue(settings1.Equals(settings2))
    End Sub


End Class

#End If