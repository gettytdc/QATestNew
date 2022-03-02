#If UNITTESTS Then

Imports System.Text.RegularExpressions
Imports System.Globalization
Imports System.Reflection
Imports System.IO

Imports NUnit.Framework

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections

Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.AutomateProcessCore.Stages

Imports BluePrism.Server.Domain.Models

''' <summary>
''' Tests the expression evaluation inside clsProcess
''' </summary>
<TestFixture()> _
Public Class ExpressionTests

    ''' <summary>
    ''' Class to encapsulate an expression test loaded from the expressions text file
    ''' </summary>
    Private Class ExpressionTest

        ' The expression being evaluated
        Private mExpr As String

        ' The expected value of the expression
        Private mExpectedResponse As clsProcessValue

        ''' <summary>
        ''' Creates a new expression test
        ''' </summary>
        ''' <param name="expr">The expression to test</param>
        ''' <param name="expectedEncodedValue">The encoded value using
        ''' clsProcessValue rules of the expected result of the expression evaluation
        ''' </param>
        ''' <param name="dtype">The type of the expected result</param>
        Public Sub New(ByVal expr As String, _
         ByVal expectedEncodedValue As String, ByVal dtype As String)
            Me.New(expr, clsProcessValue.Decode(dtype, expectedEncodedValue))
        End Sub

        ''' <summary>
        ''' Creates a new expression test
        ''' </summary>
        ''' <param name="expr">The expression to test</param>
        ''' <param name="expected">The expected result of the evaluation</param>
        Public Sub New(ByVal expr As String, ByVal expected As clsProcessValue)
            mExpr = expr
            mExpectedResponse = expected
        End Sub

        ''' <summary>
        ''' The expression to be evaluated
        ''' </summary>
        Public ReadOnly Property Expression() As String
            Get
                Return mExpr
            End Get
        End Property

        ''' <summary>
        ''' The expected result of the expression after evaluation
        ''' </summary>
        Public ReadOnly Property Expected() As clsProcessValue
            Get
                Return mExpectedResponse
            End Get
        End Property

    End Class

    ''' <summary>
    ''' A group of tests which are run within a specific culture.
    ''' </summary>
    Private Class CultureTestGroup

        ' The culture that the tests in this group should be run within
        Private mCulture As CultureInfo

        ' The tests which are to be run in the specified culture
        Private mTests As ICollection(Of ExpressionTest)

        ''' <summary>
        ''' Creates a new culture test group for a particular culture
        ''' </summary>
        ''' <param name="culture">The culture to set within this group which should
        ''' be used to run these tests.</param>
        Public Sub New(ByVal culture As CultureInfo)
            mCulture = culture
            mTests = New List(Of ExpressionTest)
        End Sub

        ''' <summary>
        ''' The culture in which the tests in this group should be run
        ''' </summary>
        Public ReadOnly Property Culture() As CultureInfo
            Get
                Return mCulture
            End Get
        End Property

        ''' <summary>
        ''' The tests to run from this group, within this group's specified culture
        ''' </summary>
        Public ReadOnly Property Tests() As ICollection(Of ExpressionTest)
            Get
                Return mTests
            End Get
        End Property

    End Class

    ' The format string for the filename embedded into the assembly
    Private Shared ReadOnly FilenameFmt As String = _
     GetType(ExpressionTests).Namespace & ".{0}"

    ' The text file containing the 'positive' tests
    Private Const GoodTestsFilename As String = "exprtests-good.txt"

    ' The text file containing bad expressions which should fail to parse
    Private Const BadTestsFilename As String = "exprtests-bad.txt"

    ' The text file containing localised tests for different cultures
    Private Const LocalTestsFilename As String = "exprtests-local.txt"

    ' Regex which defines a declaration which sets/unsets a culture for some tests
    Private Shared ReadOnly CultureTestRegex As New Regex("^<(/?)([-a-zA-Z]{5})>$")

    ' Regex to match a test in the 'good tests' file
    Private Shared ReadOnly ExprTestRegex As New Regex("^(.+?)\t(.+?)\t([a-z]+)$")

    ' A collection of expression tests with their expected results
    Private mTests As ICollection(Of ExpressionTest)

    ' A collection of expression tests for specific locales
    Private mLocalTests As ICollection(Of CultureTestGroup)

    ' A collection bad expressions which are expected to fail to evaluate
    Private mBadExpressions As ICollection(Of String)

    ''' <summary>
    ''' Gets the stream from the embedded resource in this assembly.
    ''' </summary>
    ''' <param name="filename">The filename for which a stream is required.</param>
    ''' <returns>A stream wrapping the content of the required file</returns>
    Private Shared Function GetStream(ByVal filename As String) As Stream
        Dim asm As Assembly = Assembly.GetExecutingAssembly()
        Return asm.GetManifestResourceStream(String.Format(FilenameFmt, filename))
    End Function

    ''' <summary>
    ''' Loads the expressions from the embedded text files into the expression
    ''' collections within this object.
    ''' </summary>
    <OneTimeSetUp()> _
    Public Sub LoadExpressions()
        Dim tests As New List(Of ExpressionTest)

        Using reader As New StreamReader(GetStream(GoodTestsFilename))
            While Not reader.EndOfStream
                Dim line As String = reader.ReadLine().Trim()
                If line = "" OrElse line.StartsWith("#") Then Continue While
                Dim m As Match = ExprTestRegex.Match(line)
                If Not m.Success Then Continue While
                Try
                    tests.Add(New ExpressionTest( _
                     m.Groups(1).Value, m.Groups(2).Value, m.Groups(3).Value))
                Catch
                End Try
            End While
        End Using
        mTests = GetReadOnly.ICollection(tests)

        Dim badTests As New List(Of String)
        Using reader As New StreamReader(GetStream(BadTestsFilename))
            While Not reader.EndOfStream
                Dim line As String = reader.ReadLine().Trim()
                If line = "" OrElse line.StartsWith("#") Then Continue While
                badTests.Add(line)
            End While
        End Using
        mBadExpressions = GetReadOnly.ICollection(badTests)

        Dim locTests As New List(Of CultureTestGroup)
        Using reader As New StreamReader(GetStream(LocalTestsFilename))
            Dim gp As CultureTestGroup = Nothing
            Dim lineNo As Integer = 0
            While Not reader.EndOfStream
                lineNo += 1
                Dim line As String = reader.ReadLine().Trim()
                ' Comments or empty lines are skipped
                If line = "" OrElse line.StartsWith("#") Then Continue While

                ' See if it's a culture setting/unsetting line
                Dim m As Match = CultureTestRegex.Match(line)
                If m.Success Then
                    Dim closing As Boolean = (m.Groups(1).Value = "/")
                    Dim cult As String = m.Groups(2).Value
                    If closing Then
                        If gp Is Nothing Then Throw New InvalidStateException( _
                         "{0}: No culture group; tried to close '{1}'", lineNo, cult)

                        If gp.Culture.Name <> cult Then Throw New InvalidStateException( _
                         "{0}: Tried to close '{1}'; Current culture is '{2}'", _
                         lineNo, cult, gp.Culture.Name)

                        If gp.Tests.Count > 0 Then locTests.Add(gp)
                        gp = Nothing

                    Else
                        ' We allow a culture to be opened with another group active
                        ' only if that group is the current culture (implying that
                        ' the group was not set explicitly, rather a test was added
                        ' outside any group, ie. in the current culture).
                        If gp IsNot Nothing Then
                            If gp.Culture Is CultureInfo.CurrentCulture Then
                                If gp.Tests.Count > 0 Then locTests.Add(gp)
                                gp = Nothing
                            Else
                                Throw New InvalidStateException( _
                                 "{0}: Tried to open culture '{1}' while culture '{2}' was open", _
                                 lineNo, cult, gp.Culture.Name)
                            End If
                        End If

                        gp = New CultureTestGroup(CultureInfo.GetCultureInfo(cult))

                    End If

                    Continue While ' Finished with this - onto the next line
                End If

                ' See if it's an expression line
                m = ExprTestRegex.Match(line)

                ' If it doesn't fit, we just ignore it
                If Not m.Success Then Continue While

                ' Otherwise parse the expression test into the appropriate class
                Try
                    If gp Is Nothing Then
                        gp = New CultureTestGroup(CultureInfo.CurrentCulture)
                    End If
                    gp.Tests.Add(New ExpressionTest( _
                     m.Groups(1).Value, m.Groups(2).Value, m.Groups(3).Value))


                Catch ex As Exception
                    Throw New InvalidStateException( _
                     "{0}: Error loading expression test. Message: {1}; Line: {2}", _
                     lineNo, ex.Message, line)

                End Try

            End While
            If gp IsNot Nothing AndAlso gp.Tests.Count > 0 Then locTests.Add(gp)
            mLocalTests = GetReadOnly.ICollection(locTests)

        End Using

        Debug.Print( _
         "ExpressionTests: Got tests: {0} good, {1} bad and {2} ugly (er, local)", _
         tests.Count, badTests.Count, locTests.Count)

    End Sub

    ''' <summary>
    ''' Just clears the expressions from the collections built up in the fixture
    ''' setup.
    ''' </summary>
    <OneTimeTearDown()> _
    Public Sub ClearExpressions()
        mTests = Nothing
        mBadExpressions = Nothing
    End Sub

    ' A process, yesterday
    Private mProcess As clsProcess

    ''' <summary>
    ''' Sets up each test, building up a process to a known specification
    ''' </summary>
    <SetUp()> _
    Public Sub InitTest()
        Dim proc As _
         New clsProcess(Nothing, DiagramType.Process, True)

        'Dim sheet As clsProcessSubSheet = proc.StartStage.SubSheet

        For Each dt As DataType In [Enum].GetValues(GetType(DataType))
            If dt <> DataType.unknown AndAlso dt <> DataType.collection Then
                proc.AddDataStage( _
                 "Sample" & clsProcessDataTypes.GetFriendlyName(dt), dt)
            End If
        Next

        With CType(proc.AddDataStage("SampleCollection", DataType.collection), _
         clsCollectionStage)

            .AddField("ID", DataType.number)
            .AddField("Name", DataType.text)
            .AddField("Reviews", DataType.collection)
            With .GetFieldDefinition("Reviews").Children
                .AddField("Review Date", DataType.date)
                .AddField("Score", DataType.number)
                .AddField("Reviewer ID", DataType.number)
                .AddField("Notes", DataType.text)
            End With
            .AddField("Audits", DataType.collection)
            With .GetFieldDefinition("Audits").Children
                .AddField("Completed", DataType.datetime)
                .AddField("Success?", DataType.flag)
                .AddField("Notes", DataType.text)
            End With

        End With

        mProcess = proc

    End Sub

    ''' <summary>
    ''' Runes the given tests, ensuring that the expected outcome is correct.
    ''' Any errors will be prefixed with an optional message
    ''' </summary>
    ''' <param name="tests">The expression tests to run</param>
    ''' <param name="prefix">A prefix for any assertion failure messages which occur
    ''' while running these tests</param>
    Private Sub RunTests( _
     ByVal tests As ICollection(Of ExpressionTest), ByVal prefix As String)
        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory)

        For Each test As ExpressionTest In tests
            Assert.That(test.Expression, [Is].Not.Empty)
            Dim result As clsProcessValue = Nothing
            Dim info As clsExpressionInfo = Nothing
            Dim sErr As String = Nothing

            Assert.True(clsExpression.EvaluateExpression( _
             test.Expression, result, mProcess.StartStage, False, info, sErr), _
             "{0}Expression: {1} ; Error while evaluating: {2}", _
             prefix, test.Expression, sErr)

            Assert.That(result, [Is].EqualTo(test.Expected), _
             "{0}Expression: {1}", prefix, test.Expression)

            Assert.True(clsExpression.EvaluateExpression( _
             test.Expression, result, mProcess.StartStage, True, info, sErr), _
             "{0}Expression: {1} ; Error while validating: {2}", _
             prefix, test.Expression, sErr)

            Assert.That(result.DataType, [Is].EqualTo(test.Expected.DataType), _
             "{0}Expression: {1}", prefix, test.Expression)
        Next
    End Sub

    ''' <summary>
    ''' Runes the given tests, ensuring that the expected outcome is correct.
    ''' Any errors will be prefixed with an optional message
    ''' </summary>
    ''' <param name="tests">The expression tests to run</param>
    ''' <param name="prefix">A prefix for any assertion failure messages which occur
    ''' while running these tests</param>
    Private Sub RunLocalTests( _
     ByVal tests As ICollection(Of ExpressionTest), ByVal prefix As String)
        For Each test As ExpressionTest In tests
            Dim normalExpr As String = clsExpression.LocalToNormal(test.Expression)
            Assert.That(normalExpr, [Is].Not.Empty)
            Dim result As clsProcessValue = Nothing
            Dim info As clsExpressionInfo = Nothing
            Dim sErr As String = Nothing

            Assert.True(clsExpression.EvaluateExpression( _
             normalExpr, result, mProcess.StartStage, False, info, sErr), _
             "{0}Expression: {1} ; Error while evaluating: {2}", _
             prefix, test.Expression, sErr)

            Assert.That(result, [Is].EqualTo(test.Expected), _
             "{0}Expression: {1}", prefix, test.Expression)

            Assert.True(clsExpression.EvaluateExpression( _
             normalExpr, result, mProcess.StartStage, True, info, sErr), _
             "{0}Expression: {1} ; Error while validating: {2}", _
             prefix, test.Expression, sErr)

            Assert.That(result.DataType, [Is].EqualTo(test.Expected.DataType), _
             "{0}Expression: {1}", prefix, test.Expression)
        Next
    End Sub

    ''' <summary>
    ''' Tests that all of the expressions defined in the 'exprtests-good.txt' file
    ''' evaluate without errors and result in the expected result.
    ''' </summary>
    <Test()> _
    Public Sub TestGoodExpressions()
        RunTests(mTests, Nothing)
    End Sub

    ''' <summary>
    ''' Tests that the bad expressions defined in the exprtests-bad.txt fail to
    ''' evaluate correctly.
    ''' </summary>
    <Test()> _
    Public Sub TestBadExpressions()
        For Each expr As String In mBadExpressions
            Dim result As clsProcessValue = Nothing
            Dim info As clsExpressionInfo = Nothing
            Dim sErr As String = Nothing

            Assert.False(clsExpression.EvaluateExpression( _
             expr, result, mProcess.StartStage, False, info, sErr), _
             "Expression: {0} ; Received result: {1}", expr, result)
        Next
    End Sub

    ''' <summary>
    ''' Tests that expressions which are evaluated in a locale other than the
    ''' standard locale work correctly.
    ''' </summary>
    <Test()> _
    Public Sub TestLocalExpressions()
        For Each gp As CultureTestGroup In mLocalTests
            Using New CultureBlock(gp.Culture)
                RunLocalTests(gp.Tests, "'" & gp.Culture.Name & "': ")
            End Using
        Next
    End Sub

End Class

#End If
