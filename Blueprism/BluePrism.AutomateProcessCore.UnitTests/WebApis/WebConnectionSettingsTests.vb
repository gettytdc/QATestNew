#If UNITTESTS Then

Imports System.Linq
Imports FluentAssertions
Imports NUnit.Framework

Public Class WebConnectionSettingsTests
    Private Const googleUri As String = "http://www.google.com"
    Private Const bbcUri As String = "https://www.bbc.co.uk"

    <Test>
    Public Sub ExplicitConstructor_AppliesSettings()
        Dim uriSettings = {New UriWebConnectionSettings(googleUri, 2, 2, 5)}.ToList()
        Dim classUnderTest = New WebConnectionSettings(2, 5, 2, uriSettings)

        classUnderTest.ConnectionLimit.Should().Be(2)
        classUnderTest.MaxIdleTime.Should().Be(5)
        classUnderTest.UriSpecificSettings.Should().BeEquivalentTo(uriSettings)
    End Sub


    <TestCase(0, 1, 5, googleUri, bbcUri)>
    <TestCase(2, 0, 1, googleUri, bbcUri)>
    <TestCase(2, 1, 5, googleUri, googleUri)>
    Public Sub Construct_FromInvalidFormat_Fails(connLimit As Integer,
                                                maxIdleTime As Integer,
                                                connLeaseTimeout As Integer?,
                                                firstUriSetting As String,
                                                secondUriSetting As String)
        Dim makeSettings = Function(uri As String) New UriWebConnectionSettings(uri, 2, 2, 5)

        Dim action = New Action(Function() New WebConnectionSettings(connLimit,
                                                                     maxIdleTime,
                                                                     connLeaseTimeout,
                                                                     {makeSettings(firstUriSetting), makeSettings(secondUriSetting)}))
        action.ShouldThrow(Of ArgumentException)
    End Sub


    <Test>
    Public Sub TestEquals_DifferentConnectionLimit_ReturnsFalse
        Dim settings1 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5)})
        Dim settings2 = New WebConnectionSettings(5, 5, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5)})

        Assert.IsFalse(settings1.Equals(settings2))
    End Sub

    <Test>
    Public Sub TestEquals_DifferentMaxIdleTime_ReturnsFalse
        Dim settings1 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5)})
        Dim settings2 = New WebConnectionSettings(2, 5, 7, {New UriWebConnectionSettings(googleUri, 2, 2, 5)})

        Assert.IsFalse(settings1.Equals(settings2))
    End Sub

    <Test>
    Public Sub TestEquals_DifferentConnectionLeaseTimeout_ReturnsFalse
        Dim settings1 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5)})
        Dim settings2 = New WebConnectionSettings(2, 9, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5)})

        Assert.IsFalse(settings1.Equals(settings2))
    End Sub

    <Test>
    Public Sub TestEquals_DifferentUriRow_ReturnsFalse
        Dim settings1 = New WebConnectionSettings(2, 5, 3, {New UriWebConnectionSettings(googleUri, 2, 2, 5)})
        Dim settings2 = New WebConnectionSettings(2, 5, 3, {New UriWebConnectionSettings(bbcUri, 3, 4, 5)})

        Assert.IsFalse(settings1.Equals(settings2))
    End Sub

    <Test>
    Public Sub TestEquals_DifferentAmountOfUriRows_ReturnsFalse
        Dim settings1 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5)})
        Dim settings2 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(bbcUri, 3, 4, 5),
                                                         New UriWebConnectionSettings(googleUri, 2, 2, 5)})

        Assert.IsFalse(settings1.Equals(settings2))
    End Sub

    <Test>
    Public Sub TestEquals_SameSettings_ReturnsTrue
        Dim settings1 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5)})
        Dim settings2 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5)})
        Assert.IsTrue(settings1.Equals(settings2))
        Assert.IsTrue(settings2.Equals(settings1))
    End Sub

    <Test>
    Public Sub TestEquals_EmptyConnectionTimeout_DoesNotThrow
        Dim settings1 = New WebConnectionSettings(2, 5, Nothing, {New UriWebConnectionSettings(googleUri, 2, Nothing, 5)})
        Dim settings2 = New WebConnectionSettings(2, 5, Nothing, {New UriWebConnectionSettings(googleUri, 2, Nothing, 5)})
        Assert.DoesNotThrow(Sub() settings1.Equals(settings2))
    End Sub

    <Test>
    Public Sub TestEquals_EmptyConnectionTimeoutInUriSettings_DoesNotThrow
        Dim settings1 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, Nothing, 5)})
        Dim settings2 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, Nothing, 5)})
        Assert.DoesNotThrow(Sub() settings1.Equals(settings2))
    End Sub

    <Test>
    Public Sub TestEquals_SameSettings_NoUriRows_ReturnsTrue

        Dim settings1 = New WebConnectionSettings(2, 5, 2, New List(Of UriWebConnectionSettings))
        Dim settings2 = New WebConnectionSettings(2, 5, 2, New List(Of UriWebConnectionSettings))

        Assert.IsTrue(settings1.Equals(settings2))
    End Sub

    <Test>
    Public Sub TestEquals_SameSettings_TransposedUriRows_ReturnsTrue
        Dim settings1 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5),
                                                  New UriWebConnectionSettings(bbcUri, 3, 4, 7)})
        Dim settings2 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(bbcUri, 3, 4, 7),
                                                  New UriWebConnectionSettings(googleUri, 2, 2, 5)})
        Assert.IsTrue(settings1.Equals(settings2))
        Assert.IsTrue(settings2.Equals(settings1))
    End Sub


    <Test>
    Public Sub Test_GetExistingUriSettings_DifferentHost_ReturnsNothing
        Dim settings1 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5),
                                                         New UriWebConnectionSettings(bbcUri, 3, 4, 7)})

        Dim testUri = New Uri("http://www.blueprism.com")
        Dim existingUriSettings = settings1.GetExistingUriSettings(testUri)
        Assert.IsNull(existingUriSettings)
    End Sub

    <Test>
    Public Sub Test_GetExistingUriSettings_DifferentAudience_ReturnsNothing
        Dim settings1 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5),
                                                         New UriWebConnectionSettings(bbcUri, 3, 4, 7)})

        Dim testUri = New Uri($"{googleUri}:9999")
        Dim existingUriSettings = settings1.GetExistingUriSettings(testUri)
        Assert.IsNull(existingUriSettings)
    End Sub

    <Test>
    Public Sub Test_GetExistingUriSettings_DifferentScheme_ReturnsNothing
        Dim settings1 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5),
                                                         New UriWebConnectionSettings(bbcUri, 3, 4, 7)})

        Dim testUri = New Uri($"https://www.google.com")
        Dim existingUriSettings = settings1.GetExistingUriSettings(testUri)
        Assert.IsNull(existingUriSettings)
    End Sub

    <Test>
    Public Sub Test_GetExistingUriSettings_ExcludingWWW_ReturnsNothing
        Dim settings1 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5),
                                                         New UriWebConnectionSettings(bbcUri, 3, 4, 7)})

        Dim testUri = New Uri("http://google.com")
        Dim existingUriSettings = settings1.GetExistingUriSettings(testUri)
        Assert.IsNull(existingUriSettings)
    End Sub


    <Test>
    Public Sub Test_GetExistingUriSettings_sameSchemeAndHost_ReturnsSettings
        Dim settings1 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5),
                                                         New UriWebConnectionSettings(bbcUri, 3, 4, 7)})

        Dim testUri = New Uri($"{googleUri}/stuff/12/")
        Dim existingUriSettings = settings1.GetExistingUriSettings(testUri)
        Assert.IsTrue(existingUriSettings.Equals(settings1.UriSpecificSettings(0)))
    End Sub

    <Test>
    Public Sub Test_GetExistingUriSettings_sameUri_ReturnsSettings
        Dim settings1 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5),
                                                         New UriWebConnectionSettings(bbcUri, 3, 4, 7)})
        Dim testUri = New Uri(googleUri)
        Dim existingUriSettings = settings1.GetExistingUriSettings(testUri)
        Assert.IsTrue(existingUriSettings.Equals(settings1.UriSpecificSettings(0)))
    End Sub

    <Test>
    Public Sub Test_GetExistingUriSettings_sameUriTrailingSlash_ReturnsSettings
        Dim settings1 = New WebConnectionSettings(2, 5, 2, {New UriWebConnectionSettings(googleUri, 2, 2, 5),
                                                         New UriWebConnectionSettings(bbcUri, 3, 4, 7)})
        Dim testUri = New Uri(googleUri & "/")
        Dim existingUriSettings = settings1.GetExistingUriSettings(testUri)
        Assert.IsTrue(existingUriSettings.Equals(settings1.UriSpecificSettings(0)))
    End Sub



End Class

#End If