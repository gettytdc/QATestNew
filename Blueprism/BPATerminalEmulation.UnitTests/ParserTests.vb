#If UNITTESTS Then
Imports System.Collections.Generic
Imports System.Text
Imports BluePrism.BPCoreLib
Imports NUnit.Framework
Imports BluePrism.TerminalEmulation

<TestFixture>
Public Class ParserTests


    <Test>
    <TestCase(Nothing)>
    <TestCase("")>
    Public Sub TestParseEmptySequence(s As String)

        Dim keys = Terminal.ParseKeySequence(s)

        Dim list As New List(Of KeyCode)(keys)

        Assert.That(list.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub TestParseKeySequence()
        Dim s = "abc{enter}def"

        Dim keys = Terminal.ParseKeySequence(s)

        Dim list As New List(Of KeyCode)(keys)

        Assert.That(list(0).Character, Iz.EqualTo("a"c))
        Assert.That(list(1).Character, Iz.EqualTo("b"c))
        Assert.That(list(2).Character, Iz.EqualTo("c"c))
        Assert.That(list(3).Code, Iz.EqualTo(KeyCodeMappings.Enter))
        Assert.That(list(4).Character, Iz.EqualTo("d"c))
        Assert.That(list(5).Character, Iz.EqualTo("e"c))
        Assert.That(list(6).Character, Iz.EqualTo("f"c))
    End Sub

    <Test>
    Public Sub TestParseKeyDoubleSequence()
        Dim s = "abc{eNter}def{uppershift}{CursorDown}"

        Dim keys = Terminal.ParseKeySequence(s)

        Dim list As New List(Of KeyCode)(keys)

        Assert.That(list(0).Character, Iz.EqualTo("a"c))
        Assert.That(list(1).Character, Iz.EqualTo("b"c))
        Assert.That(list(2).Character, Iz.EqualTo("c"c))
        Assert.That(list(3).Code, Iz.EqualTo(KeyCodeMappings.Enter))
        Assert.That(list(4).Character, Iz.EqualTo("d"c))
        Assert.That(list(5).Character, Iz.EqualTo("e"c))
        Assert.That(list(6).Character, Iz.EqualTo("f"c))
        Assert.That(list(7).Code, Iz.EqualTo(KeyCodeMappings.UpperShift))
        Assert.That(list(8).Code, Iz.EqualTo(KeyCodeMappings.CursorDown))
    End Sub


    <Test>
    <TestCase("{}")>
    <TestCase("{}z")>
    <TestCase("abc}enter")>
    <TestCase("abc{enter")>
    <TestCase("abc{{invalid}}")>
    <TestCase("abc{invalid}")>
    <TestCase("abc{{{{{")>
    <TestCase("abc}}}}}")>
    <TestCase("{enter}}}}}")>
    <TestCase("{{{{{enter}")>
    <TestCase("{{enter}test{}}")>
    <TestCase("{{}test{enter}}")>
    Public Sub TestParseKeySequenceInvalid(s As String)

        Assert.Throws(Of InvalidFormatException)(
            Sub() Terminal.ParseKeySequence(s))

    End Sub

    <Test>
    <TestCase("+")>
    <TestCase("^")>
    <TestCase("%")>
    <TestCase("~")>
    <TestCase("(")>
    <TestCase(")")>
    <TestCase("[")>
    <TestCase("]")>
    <TestCase("^{enter}")>
    Public Sub TestParseKeySequenceNotImplemented(s As String)

        Assert.Throws(Of NotImplementedException)(
            Sub() Terminal.ParseKeySequence(s))

    End Sub

    <Test>
    <TestCase("{{}", "{")>
    <TestCase("{}}", "}")>
    <TestCase("{{}{enter}", "{")>
    <TestCase("{}}{enter}", "}")>
    <TestCase("{+}", "+")>
    <TestCase("{^}", "^")>
    <TestCase("{%}", "%")>
    <TestCase("{~}", "~")>
    <TestCase("{(}", "(")>
    <TestCase("{)}", ")")>
    <TestCase("{[}", "[")>
    <TestCase("{]}", "]")>
    Public Sub TestParseKeySequenceEscape(s As String, r As String)

        Dim keys = Terminal.ParseKeySequence(s)

        Dim list As New List(Of KeyCode)(keys)

        Assert.That(list(0).Character, Iz.EqualTo(CChar(r)))
    End Sub

    <Test>
    Public Sub TestParseKeySequenceOpenCloseBrace()
        Dim s As String = "{{}test{}}"

        Dim keys = Terminal.ParseKeySequence(s)

        Dim list As New List(Of KeyCode)(keys)

        Assert.That(list(0).Character, Iz.EqualTo("{"c))
        Assert.That(list(1).Character, Iz.EqualTo("t"c))
        Assert.That(list(2).Character, Iz.EqualTo("e"c))
        Assert.That(list(3).Character, Iz.EqualTo("s"c))
        Assert.That(list(4).Character, Iz.EqualTo("t"c))
        Assert.That(list(5).Character, Iz.EqualTo("}"c))
    End Sub

    'Well this is not a test but considering its probably not often
    'going to be used there doesn't seem to be much point in integrating
    'it into the release code
    Public Sub GenerateDocumentation()
        Dim mappings As New SortedDictionary(Of String, KeyCodeMappings)
        For Each k As KeyCodeMappings In [Enum].GetValues(GetType(KeyCodeMappings))
            mappings.Add(k.ToString(), k)
        Next

        Dim sb As New StringBuilder()
        sb.AppendLine("<table>")
        sb.AppendLine("<tr><th>Key</th><th>Code</th></tr>")
        For Each k As KeyCodeMappings In mappings.Values
            Dim attr = KeyCode.GetAttribute(Of KeyCodeInfo)(k)
            sb.Append("<tr><td>{")
            sb.Append(k.ToString())
            sb.Append("}</td><td>")
            sb.Append(attr.Description)
            sb.AppendLine("</td></tr>")
        Next
        sb.Append("</table>")

        Console.WriteLine(sb.ToString())
    End Sub

End Class
#End If
