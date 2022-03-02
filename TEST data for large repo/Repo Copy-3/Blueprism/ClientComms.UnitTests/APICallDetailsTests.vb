#If UNITTESTS Then

Imports NUnit.Framework

Namespace _UnitTests

    <TestFixture>
    Public Class APICallDetailsTests
        <TestCase("\\", "\")>
        <TestCase("\c", ",")>
        <TestCase("\e", "=")>
        <TestCase("\r", vbCr)>
        <TestCase("\n", vbLf)>
        <TestCase("\r\n", vbCrLf)>
        <TestCase("C:\\Users\\charles", "C:\Users\charles")>
        <TestCase("""ok""", """ok""")>
        <TestCase("\\", "\")>
        <TestCase("\\c", "\c")>
        <TestCase("\\\c", "\,")>
        <TestCase("\c\e", ",=")>
        <TestCase("\\\\\e\\c\c\\\\\e\e\r\n\c\\c", "\\=\c,\\==" & vbCrLf & ",\c")>
        Public Sub TestUnescapeEscaped(data As String, expected As String)
            Dim c = clsAPICallDetails.Parse(String.Format("Test(txt=""{0}"")", data))

            Assert.That(c.Parameters("txt"), Iz.EqualTo(expected))
        End Sub
    End Class

End Namespace

#End If
