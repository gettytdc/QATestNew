#If UNITTESTS

Imports System.Drawing
Imports BluePrism.AutomateProcessCore.WebApis.TemplateProcessing
Imports BluePrism.Core.Utility
Imports BluePrism.UnitTesting.TestSupport
Imports FluentAssertions
Imports NUnit.Framework

Namespace WebApis.RequestHandling

    <TestFixture, Category("Web APIs")>
    Public Class ParameterInterpolatorTests

        <TestCase("/customers/[Customer ID]", "Customer ID")>
        <TestCase("/customers/[CustomerID]", "CustomerID")>
        <TestCase("/customers/[A]", "A")>
        <TestCase("/customers/[A B]", "A B")>
        <TestCase("/customers/[A.B]", "A.B")>
        Public Sub ProcessTemplate_WithValidTokenAndMatchingParameter_ShouldInsertValue(template As String, parameterName As String)
            Const expected = "/customers/abcd-1234"
            TestWithSingleParameter(template, parameterName, "abcd-1234", expected)
        End Sub

        <Test>
        Public Sub ProcessTemplate_WithUnrecognisedParameter_ShouldInsertEmptyValue()
            Const template = "/customers/[CustomerID]"
            Const expected = "/customers/"
            TestWithSingleParameter(template, "Other Parameter", "abcd-1234", expected)
        End Sub

        <TestCase("/customers/[ Customer ID]")>
        <TestCase("/customers/[Customer ID  ]")>
        <TestCase("/customers/[   Customer ID   ]")>
        <TestCase("/customers/[ A ]")>
        <TestCase("/customers/[A ]")>
        <TestCase("/customers/[ A]")>
        <TestCase("/customers/[ A.B ]")>
        <TestCase("/customers/[ A")>
        <TestCase("/customers/[A")>
        <TestCase("/customers/[")>
        Public Sub ProcessTemplate_WithInvalidParameterFormat_ShouldKeepOriginalContent(template As String)

            Dim parameters = New Dictionary(Of String, clsProcessValue)

            Dim result = ParameterInterpolator.ProcessTemplate(template, parameters)

            result.Should.Be(template)

        End Sub

        <Test>
        Public Sub ProcessTemplate_WithMultipleTokensAndParameters_ShouldInsertValues()

            Dim template = "/api/v[Version]/customers/[Customer ID]"
            Dim parameters = New Dictionary(Of String, clsProcessValue) From
                {{"Version", "1.0"}, {"Customer ID", New clsProcessValue("abcd-1234")}}

            Dim result = ParameterInterpolator.ProcessTemplate(template, parameters)

            result.Should.Be("/api/v1.0/customers/abcd-1234")

        End Sub

        <Test>
        Public Sub ProcessTemplate_WithEscapedTokenAndMatchingParameter_ShouldDisplayEscapedDelimiter()
            Const template = "/customers/[[Customer ID]"
            Const expected = "/customers/[Customer ID]"
            TestWithSingleParameter(template, "Customer ID", "abcd-1234", expected)
        End Sub

        <Test>
        Public Sub ProcessTemplate_WithMultipleEscapedTokensAndMatchingParameter_ShouldDisplayEscapedDelimiters()
            Const template = "/customers/[[[[[[Customer ID]"
            Const expected = "/customers/[[[Customer ID]"
            TestWithSingleParameter(template, "Customer ID", "abcd-1234", expected)
        End Sub

        <Test>
        Public Sub ProcessTemplate_WithEscapedTokensThenValidParameter_ShouldCombineElements()
            Const template = "/customers/[[[[[[[Customer ID]"
            Const expected = "/customers/[[[abcd-1234"
            TestWithSingleParameter(template, "Customer ID", "abcd-1234", expected)
        End Sub

        <TestCaseSource("GetValuesUsingEncodedValue")>
        Public Sub ProcessTemplate_WithValuesUsingEncodedValue_InsertsEncodedValue(value As clsProcessValue)
            Dim template = "Value:[Param 1]"
            TestWithSingleParameter(template, "Param 1", value, $"Value:{value.EncodedValue}")
        End Sub

        <Test>
        Public Sub ProcessTemplate_WithPasswordValue_InsertsPlainText()
            Dim template = "Value:[Param 1]"
            Const password = "password123"
            TestWithSingleParameter(template, "Param 1", New clsProcessValue(password.ToSecureString()), $"Value:{password}")
        End Sub

        <Test>
        Public Sub ProcessTemplate_WithImageValue_InsertsBase64EncodedBitmap()

            Dim bitmap = New TestBitmapGenerator().
                    WithColour("R"c, Color.Red).
                    WithColour("W"c, Color.White).
                    WithPixels("RWWWWWWR").Create()

            Dim value = New clsProcessValue(bitmap)

            Dim template = "Value:[Param 1]"
            TestWithSingleParameter(template, "Param 1", value, $"Value:Qk1OAAAAAAAAADYAAAAoAAAACAAAAAEAAAABABgAAAAAAAAAAADEDgAAxA4AAAAAAAAAAAAAAAD/////////////////////////AAD/")
        End Sub

        ' Test helper method to make tests more concise
        Private Sub TestWithSingleParameter(template As String,
                                            name As String,
                                            value As String,
                                            expectedResult As String)
            TestWithSingleParameter(template, name, New clsProcessValue(value), expectedResult)
        End Sub

        ' Test helper method to make tests more concise
        Private Sub TestWithSingleParameter(template As String,
                                            name As String,
                                            value As clsProcessValue,
                                            expectedResult As String)
            Dim parameters = New Dictionary(Of String, clsProcessValue) From {{name, value}}

            Dim result = ParameterInterpolator.ProcessTemplate(template, parameters)

            result.Should.Be(expectedResult)
        End Sub

        Protected Shared Iterator Function GetValuesUsingEncodedValue() As IEnumerable(Of clsProcessValue)
            Yield New clsProcessValue("Some string")
            Yield New clsProcessValue(3)
            Yield New clsProcessValue(True)
            Yield New clsProcessValue(New TimeSpan(1, 2, 3))
            Yield New clsProcessValue(DataType.datetime, DateTime.UtcNow)
            Yield New clsProcessValue(DataType.date, DateTime.UtcNow)
        End Function

    End Class


End Namespace

#End If
