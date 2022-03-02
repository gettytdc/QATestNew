#If UNITTESTS Then

Imports NUnit.Framework
Imports FluentAssertions
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
''' <summary>
''' Tests for scope the support for JSON path parsing with Newtonsoft
''' </summary>
Public Class JsonPathTests
    Private mSut As JToken

    <SetUp>
    Public Sub SetUp()
        mSut = GetTestJson()
    End Sub

    <TestCase("$.Name", "Bob")>
    <TestCase("Name", "Bob")>
    <TestCase("$['Name']", "Bob")>
    <TestCase("$.Location.Country", "Spain")>
    <TestCase("$.Location.Country", "Spain")>
    <TestCase("$['Location']['Country']", "Spain")>
    <TestCase("$['Pets'][1]['Name']", "goat")>
    <TestCase("$.MainPets[*].Name", "Elephant")>
    <TestCase("$.Numbers[1:2]", "2")>
    <TestCase("Pets[?(@.Name == 'donkey')].Name", "donkey")>
    <TestCase("$.Numbers[?(@ == 7)]", "7")>
    <TestCase("$.Numbers[?(@ > 6)]", "7")>
    Public Sub CheckTokenFinder_ValidPath_FindsValue(path As String, expectedOutput As String)
        Dim token = mSut.SelectToken(path)
        Dim deserialised = token.ToObject(Of String)
        deserialised.Should().Be(expectedOutput)
    End Sub

    <TestCase("$..*")>
    <TestCase("..Name")>
    <TestCase("$.Pets[*].Name")>
    Public Sub CheckTokenFinder_MultipleMatchingTokens_ThrowsException(path As String)
        Dim attemptParse As Action = Sub() mSut.SelectToken(path)
        attemptParse.ShouldThrow(Of JsonException)
    End Sub

    Protected Class TestData
        Property Name As String = "Bob"
        Property Pets As IEnumerable(Of Pet) = {New Pet("dog"), New Pet("goat"), New Pet("donkey"), New Pet("llama")}
        Property Numbers As IEnumerable(Of Integer) = {3, 2, 5, 4, 7, 5, 2, 6, 1, 1}
        Property Location As Country = New Country()
        Property MainPets As IEnumerable(Of Pet) = {New Pet("Elephant")}
    End Class

    Protected Class Pet
        Property Name As String
        Property Age As Integer

        Sub New(petName As String)
            Name = petName
            Age = petName.Length
        End Sub
    End Class

    Protected Class Country
        Property Country As String = "Spain"
    End Class

    Shared Function GetTestJson() As JToken
        Return JToken.Parse(JsonConvert.SerializeObject(New TestData()))
    End Function
End Class

#End If
