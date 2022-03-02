#If UNITTESTS Then

Imports BluePrism.AutomateProcessCore.WebApis
Imports NUnit.Framework
Imports FluentAssertions
Imports BluePrism.Common.Security
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Data.Linq
Imports System.Text
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
''' <summary>
''' Tests for CustomOutputParameter
''' </summary>
Public Class CustomOutputParametersTests
    Protected Class TestData
        Property CompletedAt As DateTime = New DateTime(2011, 1, 1, 10, 0, 0)
        Property TimeTaken As TimeSpan = New TimeSpan(10, 0, 0)
        Property Tries As Decimal = 5
        Property Name As String = "Bob"
        Property Pets As IEnumerable(Of Pet) = {New Pet("dog"), New Pet("cat"), New Pet("donkey"), New Pet("llama")}
        Property Location As Country = New Country()
        Property IsTheBest As Boolean = True
        ReadOnly Property Image As Byte()
            Get
                Return GetImageAsBytes()
            End Get
        End Property
        ReadOnly Property Binary As Binary
            Get
                Dim bytes = Encoding.Unicode.GetBytes("Hello World")
                Return New Binary(bytes)
            End Get
        End Property
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

   <TestCase("$. Location.Country")>
   <TestCase("$ .Location.Country")>
   <TestCase("$.Location. Country")>
    Public Sub GetFromJson_InvalidJsonPath_ShouldThrowJsonException(path As string)
        Dim sut = CreateOutputParam(DataType.Text, "TestParam", path)

        Dim getResponse As Action = Function() sut.GetFromResponse(GetTestJson())
        getResponse.ShouldThrow(Of JsonException)
    End Sub

    Public Sub GetFromJson_PathWithoutRoot_DoesNotThrowJsonException()
        Dim sut = CreateOutputParam(DataType.Text, "TestParam", "response")

        Dim getResponse As Action = Function() sut.GetFromResponse(GetTestJson())
        getResponse.ShouldNotThrow(Of JsonException)
    End Sub

    <Test>
    Public Sub GetFromJson_EmptyJsonPath_ThrowsException()
        Dim sut = CreateOutputParam(DataType.Text, "TestParam", "")

        Dim getResponse As Action = Function() sut.GetFromResponse(GetTestJson())
        getResponse.ShouldThrow(Of Exception)
    End Sub

    <Test>
    Public Sub GetFromJson_UnknownParameterDataType_ThrowsException()
        Dim sut = CreateOutputParam(DataType.Unknown, "TestParam", "$.Name")

        Dim getResponse As Action = Function() sut.GetFromResponse(GetTestJson())
        getResponse.ShouldThrow(Of Exception)
    End Sub

    
    <TestCase(DataType.text)>
    <TestCase(DataType.binary)>
    <TestCase(DataType.collection)>
    <TestCase(DataType.date)>
    <TestCase(DataType.datetime)>
    <TestCase(DataType.flag)>
    <TestCase(DataType.image)>
    <TestCase(DataType.number)>
    <TestCase(DataType.password)>
    <TestCase(DataType.time)>
    <TestCase(DataType.timespan)>
   Public Sub GetFromJson_NoSuchPath_ReturnsEmptyParam(paramType As DataType)
        Dim sut = CreateOutputParam(paramType, "TestParam", "$.NotAValidPath")
        Dim result = sut.GetFromResponse(GetTestJson())
        
       result.ShouldBeEquivalentTo(New clsProcessValue(paramType))
    End Sub

   
    <Test>
    Public Sub GetFromJson_Simple_GetCorrectValue()
        Dim sut = CreateOutputParam(DataType.text, "TestParam", "$.test")

        Dim result = sut.GetFromResponse(JToken.Parse("{'test':'frank'}"))
        result.ShouldBeEquivalentTo(New clsProcessValue("frank"))
    End Sub

    <Test>
    Public Sub GetFromJson_TextDataType_GetCorrectValue()
        Dim sut = CreateOutputParam(DataType.text, "TestParam", "$.Location.Country")

        Dim result = sut.GetFromResponse(GetTestJson())
        result.ShouldBeEquivalentTo(New clsProcessValue("Spain"))
    End Sub

    <Test>
    Public Sub GetFromJson_CollectionDataType_GetCorrectValue()
        Dim sut = CreateOutputParam(DataType.collection, "TestParam", "$.Pets")

        Dim result = sut.GetFromResponse(GetTestJson())
        result.EncodedValue.Should().Be(GetExpectedPets())
    End Sub

    Private Function GetExpectedPets() As String
        Dim result = New StringBuilder()
        result.Append("<collection>")
        result.Append("<row>")
        result.Append("<field name=""Name"" type=""text"" value=""dog"" />")
        result.Append("<field name=""Age"" type=""number"" value=""3"" />")
        result.Append("</row>")
        result.Append("<row>")
        result.Append("<field name=""Name"" type=""text"" value=""cat"" />")
        result.Append("<field name=""Age"" type=""number"" value=""3"" />")
        result.Append("</row>")
        result.Append("<row>")
        result.Append("<field name=""Name"" type=""text"" value=""donkey"" />")
        result.Append("<field name=""Age"" type=""number"" value=""6"" />")
        result.Append("</row>")
        result.Append("<row>")
        result.Append("<field name=""Name"" type=""text"" value=""llama"" />")
        result.Append("<field name=""Age"" type=""number"" value=""5"" />")
        result.Append("</row>")
        result.Append("</collection>")

        Return result.ToString()
    End Function

    <Test>
    Public Sub GetFromJson_DateDataType_GetCorrectValue()
        Dim sut = CreateOutputParam(DataType.date, "TestParam", "$.CompletedAt")

        Dim result = sut.GetFromResponse(GetTestJson())
        result.ShouldBeEquivalentTo(New clsProcessValue(DataType.date, New Date(2011, 1, 1)))
    End Sub

    <Test>
    Public Sub GetFromJson_DateTimeDataType_GetCorrectValue()
        Dim sut = CreateOutputParam(DataType.datetime, "TestParam", "$.CompletedAt")

        Dim result = sut.GetFromResponse(GetTestJson())
        result.ShouldBeEquivalentTo(New clsProcessValue(DataType.datetime, New DateTime(2011, 1, 1, 10, 0, 0)))
    End Sub

    <Test>
    Public Sub GetFromJson_FlagDataType_GetCorrectValue()
        Dim sut = CreateOutputParam(DataType.flag, "TestParam", "$.IsTheBest")

        Dim result = sut.GetFromResponse(GetTestJson())
        result.ShouldBeEquivalentTo(New clsProcessValue(True))
    End Sub

    <Test>
    Public Sub GetFromJson_NumberDataType_GetCorrectValue()
        Dim sut = CreateOutputParam(DataType.number, "TestParam", "$.Tries")

        Dim result = sut.GetFromResponse(GetTestJson())
        result.ShouldBeEquivalentTo(New clsProcessValue(5))
    End Sub

    <Test>
    Public Sub GetFromJson_PasswordDataType_GetCorrectValue()
        Dim sut = CreateOutputParam(DataType.password, "TestParam", "$.Name")

        Dim result = sut.GetFromResponse(GetTestJson())
        result.ShouldBeEquivalentTo(New clsProcessValue(New SafeString("Bob")))
    End Sub

    <Test>
    Public Sub GetFromJson_TimeDataType_GetCorrectValue()
        Dim sut = CreateOutputParam(DataType.time, "TestParam", "$.CompletedAt")

        Dim result = sut.GetFromResponse(GetTestJson())
        result.ShouldBeEquivalentTo(New clsProcessValue(DataType.time, "10:00:00"))
    End Sub

    <Test>
    Public Sub GetFromJson_TimeSpanDataType_GetCorrectValue()
        Dim sut = CreateOutputParam(DataType.timespan, "TestParam", "$.TimeTaken")

        Dim result = sut.GetFromResponse(GetTestJson())
        result.ShouldBeEquivalentTo(New clsProcessValue(New TimeSpan(10, 0, 0)))
    End Sub

    <Test>
    Public Sub GetFromJson_ImageDataType_GetCorrectValue()
        Dim sut = CreateOutputParam(DataType.image, "TestParam", "$.Image")

        Dim result = sut.GetFromResponse(GetTestJson())
        result.ShouldBeEquivalentTo(New clsProcessValue(GetImage()))
    End Sub

    <Test>
    Public Sub GetFromJson_BinaryDataType_GetCorrectValue()
        Dim sut = CreateOutputParam(DataType.binary, "TestParam", "$.Binary")

        Dim result = sut.GetFromResponse(GetTestJson())
        result.ShouldBeEquivalentTo(New clsProcessValue(Encoding.Unicode.GetBytes("Hello World")))
    End Sub

    Private Function CreateOutputParam(type As DataType, name As String, path As String) As JsonPathOutputParameter
        Return New JsonPathOutputParameter(name, "Description", path, type)
    End Function

    Private Function GetStreamReader(content As String) As StreamReader
        Using stream = New MemoryStream()
            Dim bytes = Encoding.Default.GetBytes(content)
            stream.Write(bytes, 0, bytes.Length)
            Return New StreamReader(stream)
        End Using
    End Function

    Private Shared Function GetImage() As Bitmap
        Dim image As New Bitmap(2, 2, PixelFormat.Format32bppArgb)
        image.SetPixel(0, 0, Color.Red)
        image.SetPixel(0, 1, Color.Blue)
        image.SetPixel(1, 0, Color.Green)
        image.SetPixel(1, 1, Color.White)
        Return image
    End Function


    Private Shared Function GetImageAsBytes() As Byte()
        Using stream = New MemoryStream()
            Using image = GetImage()
                image.Save(stream, ImageFormat.Bmp)
                Return stream.ToArray()
            End Using
        End Using
    End Function
End Class

#End If
