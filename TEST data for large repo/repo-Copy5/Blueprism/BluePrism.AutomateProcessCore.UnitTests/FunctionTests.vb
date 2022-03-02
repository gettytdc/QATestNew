Imports BluePrism.Common.Security

#If UNITTESTS Then

Imports System.Globalization
Imports System.Drawing
Imports System.Text

Imports NUnit.Framework

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports Signature = BluePrism.AutomateProcessCore.clsFunction.Signature

''' <summary>
''' Tests the APC functions
''' </summary>
<TestFixture()> _
Public Class FunctionTests

#Region " Stub Function "

    Private Class StubFunction : Inherits clsFunction

        Public Overrides ReadOnly Property DataType() As DataType
            Get
                Return DataType.unknown
            End Get
        End Property

        Public Overrides ReadOnly Property DefaultSignature() As clsFunctionParm()
            Get
                Return New clsFunctionParm() { _
                 New clsFunctionParm("First", "First Parameter", DataType.number)}
            End Get
        End Property

        Public Overrides ReadOnly Property GroupName() As String
            Get
                Return "Groupless"
            End Get
        End Property

        Public Overrides ReadOnly Property HelpText() As String
            Get
                Return "You get no help from me"
            End Get
        End Property

        Protected Overrides Function InnerEvaluate( _
         ByVal parms As IList(Of clsProcessValue), ByVal proc As clsProcess) _
         As clsProcessValue
            If CInt(parms(0)) = 1 Then Throw New NotImplementedException()
            Throw New clsFunctionException("parms[0] != 1")
        End Function

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Stub"
            End Get
        End Property

        Public Overrides ReadOnly Property ShortDesc() As String
            Get
                Return "St"
            End Get
        End Property

        ''' <summary>
        ''' Tests the given arguments against a set of signatures.
        ''' </summary>
        ''' <param name="args">The arguments to test</param>
        ''' <param name="sigs">The signatures to test against</param>
        ''' <param name="expected">The expected set of values which have been matched
        ''' against the most appropriate signature and cast into the appropriate
        ''' data types.</param>
        Public Sub TestArgs( _
         ByVal args As IList(Of clsProcessValue), _
         ByVal sigs As IList(Of Signature), _
         ByVal expected As IList(Of clsProcessValue))
            ' We pass in the params; after the call, the list is replaced with the
            ' arguments cast into their required type
            Dim castArgs As IList(Of clsProcessValue) = args
            EnsureParams(castArgs, CollectionUtil.ToArray(sigs))
            Assert.That(castArgs, [Is].EqualTo(expected))
        End Sub

        ''' <summary>
        ''' Tests that attempting to provide an invalid set of arguments which don't
        ''' match any of the signatures will fail as appropriate.
        ''' </summary>
        ''' <param name="expectedText">The text which is expected to appear within
        ''' the error message.</param>
        ''' <param name="args">The arguments to provide as input arguments for the
        ''' signatures</param>
        ''' <param name="sigs">The signatures to test against</param>
        Public Sub TestArgsFail( _
         ByVal expectedText As String, _
         ByVal args As IList(Of clsProcessValue), _
         ByVal sigs As IList(Of Signature))
            ' We pass in the params; after the call, the list is replaced with the
            ' arguments cast into their required type
            Dim castArgs As IList(Of clsProcessValue) = args
            Try
                EnsureParams(castArgs, CollectionUtil.ToArray(sigs))
                Assert.Fail("TestArgsFail didn't fail. Got cast args of: {0}", _
                 CollectionUtil.Join(castArgs, ","))
            Catch ex As Exception
                Assert.That(ex.Message, Does.Contain(expectedText))
            End Try
        End Sub

    End Class

#End Region

#Region " Function Map "

    ' The map of functions to their function names, created in the Functions property
    Private mFunctions As IDictionary(Of String, clsFunction)

    ''' <summary>
    ''' Dictionary containing all of the APC Functions mapped against their names.
    ''' </summary>
    Private ReadOnly Property Functions() As IDictionary(Of String, clsFunction)
        Get
            If mFunctions Is Nothing Then
                Dim allFns As New Dictionary(Of String, clsFunction)
                For Each fn As clsFunction In clsFunctions.NUnit_GetApcFunctions()
                    allFns.Add(fn.Name, fn)
                Next
                mFunctions = GetReadOnly.IDictionary(allFns)
            End If
            Return mFunctions
        End Get
    End Property

    ''' <summary>
    ''' Gets the function referenced by the given name and performs a basic test
    ''' on it, including checking its: <list>
    ''' <item>Name;</item>
    ''' <item>DataType;</item>
    ''' <item>GroupName;</item>
    ''' </list>
    ''' It will also check that the <see cref="clsFunction.RecoveryOnly"/> and
    ''' <see cref="clsFunction.TextAppendFunction"/> properties of the function are
    ''' set to False.
    ''' </summary>
    ''' <param name="fnName">The name of the function required</param>
    ''' <param name="dtype">The expected data type of the function</param>
    ''' <param name="gpName">The expected group name of the function</param>
    ''' <returns>The function associated with the requested name, after passing the
    ''' basic tests.</returns>
    ''' <exception cref="AssertionException">If any of the basic tests of the
    ''' function fail.</exception>
    Private Function GetFn(ByVal fnName As String, _
     ByVal dtype As DataType, ByVal gpName As String) As clsFunction
        Return GetFn(fnName, dtype, gpName, False, False)
    End Function

    ''' <summary>
    ''' Gets the function referenced by the given name and performs a basic test
    ''' on it, including checking its: <list>
    ''' <item>Name;</item>
    ''' <item>DataType;</item>
    ''' <item>GroupName;</item>
    ''' <item>RecoverOnly state;</item>
    ''' <item>TextAppend state;</item>
    ''' </list>
    ''' </summary>
    ''' <param name="fnName">The name of the function required</param>
    ''' <param name="dtype">The expected data type of the function</param>
    ''' <param name="gpName">The expected group name of the function</param>
    ''' <param name="isRecovery">Whether the <see cref="clsFunction.RecoveryOnly"/>
    ''' flag should be set in the function.</param>
    ''' <param name="isAppend">Whether the <see cref="clsFunction.TextAppendFunction"/>
    ''' flag should be set in the function.</param>
    ''' <returns>The function associated with the requested name, after passing the
    ''' basic tests.</returns>
    ''' <exception cref="AssertionException">If any of the basic tests of the
    ''' function fail.</exception>
    Private Function GetFn(ByVal fnName As String, _
     ByVal dtype As DataType, ByVal gpName As String, _
     ByVal isRecovery As Boolean, ByVal isAppend As Boolean) As clsFunction
        Dim fn As clsFunction = Functions(fnName)
        Assert.That(fn, [Is].Not.Null)
        Assert.That(fn.DataType, [Is].EqualTo(dtype))
        Assert.That(fn.GroupName, [Is].EqualTo(gpName))
        Assert.That(fn.Name, [Is].EqualTo(fnName))
        Assert.That(fn.RecoveryOnly, [Is].EqualTo(isRecovery))
        Assert.That(fn.TextAppendFunction, [Is].EqualTo(isAppend))
        Return fn
    End Function

#End Region

#Region " Utility Methods "

#Region " TestFn Overloads "

    ''' <summary>
    ''' Tests that a function returns the expected result
    ''' </summary>
    ''' <param name="fn">The function to test</param>
    ''' <param name="expected">The expected result, null to just return the value
    ''' from the function without testing it first</param>
    ''' <returns>The result of the function call</returns>
    Private Function TestFn( _
     ByVal fn As clsFunction, ByVal expected As clsProcessValue) _
     As clsProcessValue
        Return TestFn(fn, GetEmpty.IList(Of clsProcessValue), expected)
    End Function

    ''' <summary>
    ''' Tests that a function returns the expected result with given arguments
    ''' </summary>
    ''' <param name="fn">The function to test</param>
    ''' <param name="pv">The only argument to pass to the function</param>
    ''' <param name="expected">The expected result, null to just return the value
    ''' from the function without testing it first</param>
    ''' <returns>The result of the function call</returns>
    Private Function TestFn(ByVal fn As clsFunction, _
     ByVal pv As clsProcessValue, ByVal expected As clsProcessValue) _
     As clsProcessValue
        Return TestFn(fn, GetSingleton.IList(pv), expected)
    End Function

    ''' <summary>
    ''' Tests that a function returns the expected result with given arguments
    ''' </summary>
    ''' <param name="fn">The function to test</param>
    ''' <param name="pv1">The first argument to pass to the function</param>
    ''' <param name="pv2">The second argument to pass to the function</param>
    ''' <param name="expected">The expected result, null to just return the value
    ''' from the function without testing it first</param>
    ''' <returns>The result of the function call</returns>
    Private Function TestFn(ByVal fn As clsFunction, _
     ByVal pv1 As clsProcessValue, ByVal pv2 As clsProcessValue, _
     ByVal expected As clsProcessValue) _
     As clsProcessValue
        Return TestFn(fn, GetReadOnly.IListFrom(pv1, pv2), expected)
    End Function

    ''' <summary>
    ''' Tests that a function returns the expected result with given arguments
    ''' </summary>
    ''' <param name="fn">The function to test</param>
    ''' <param name="pv1">The first argument to pass to the function</param>
    ''' <param name="pv2">The second argument to pass to the function</param>
    ''' <param name="pv3">The third argument to pass to the function</param>
    ''' <param name="expected">The expected result, null to just return the value
    ''' from the function without testing it first</param>
    ''' <returns>The result of the function call</returns>
    Private Function TestFn(ByVal fn As clsFunction, _
     ByVal pv1 As clsProcessValue, ByVal pv2 As clsProcessValue, _
     ByVal pv3 As clsProcessValue, ByVal expected As clsProcessValue) _
     As clsProcessValue
        Return TestFn(fn, GetReadOnly.IListFrom(pv1, pv2, pv3), expected)
    End Function

    ''' <summary>
    ''' Tests that a function returns the expected result with given arguments
    ''' </summary>
    ''' <param name="fn">The function to test</param>
    ''' <param name="values">The arguments and expected result for the test. The last
    ''' value in the array is taken as the expected result, and all prior values are
    ''' taken as the arguments for the function (in the order presented).</param>
    ''' <returns>The result of the function call</returns>
    ''' <exception cref="ArgumentException">If no values were provided</exception>
    Private Function TestFn( _
     ByVal fn As clsFunction, ByVal ParamArray values() As clsProcessValue) _
     As clsProcessValue
        If values.Length = 0 Then Throw New ArgumentException( _
         "At least one argument must be provided (return value) to test a function")
        Dim expected As clsProcessValue = values(values.Length - 1)
        Dim args As New List(Of clsProcessValue)
        For i As Integer = 0 To values.Length - 2
            args.Add(values(i))
        Next

        Return TestFn(fn, args, expected)
    End Function

    ''' <summary>
    ''' Tests that a function returns the expected result with given arguments
    ''' </summary>
    ''' <param name="fn">The function to test</param>
    ''' <param name="args">The arguments to pass to the function</param>
    ''' <param name="expected">The expected result, null to just return the value
    ''' from the function without testing it first</param>
    ''' <returns>The result of the function call</returns>
    Private Function TestFn(ByVal fn As clsFunction, _
     ByVal args As IList(Of clsProcessValue), ByVal expected As clsProcessValue) _
     As clsProcessValue
        Dim result As clsProcessValue = fn.Evaluate(args, Nothing)
        Assert.That(result, [Is].Not.Null)
        If expected IsNot Nothing Then Assert.That(result, [Is].EqualTo(expected))
        Return result
    End Function

#End Region

#Region " TestFnFail Overloads "

    ''' <summary>
    ''' Tests that a function fails with a FunctionException
    ''' </summary>
    ''' <param name="fn">The function to test</param>
    ''' <param name="pv">The only argument to pass to the function</param>
    Private Sub TestFnFail( _
     ByVal fn As clsFunction, ByVal pv As clsProcessValue)
        TestFnFail(fn, GetSingleton.IList(pv))
    End Sub

    ''' <summary>
    ''' Tests that a function fails with a FunctionException
    ''' </summary>
    ''' <param name="fn">The function to test</param>
    ''' <param name="pv1">The first argument to pass to the function</param>
    ''' <param name="pv2">The second argument to pass to the function</param>
    Private Sub TestFnFail(ByVal fn As clsFunction, _
     ByVal pv1 As clsProcessValue, ByVal pv2 As clsProcessValue)
        TestFnFail(fn, GetReadOnly.IListFrom(pv1, pv2))
    End Sub

    ''' <summary>
    ''' Tests that a function fails with a FunctionException
    ''' </summary>
    ''' <param name="fn">The function to test</param>
    ''' <param name="pv1">The first argument to pass to the function</param>
    ''' <param name="pv2">The second argument to pass to the function</param>
    ''' <param name="pv3">The third argument to pass to the function</param>
    Private Sub TestFnFail(ByVal fn As clsFunction, _
     ByVal pv1 As clsProcessValue, ByVal pv2 As clsProcessValue, _
     ByVal pv3 As clsProcessValue)
        TestFnFail(fn, GetReadOnly.IListFrom(pv1, pv2, pv3))
    End Sub

    ''' <summary>
    ''' Tests that a function fails with a FunctionException
    ''' </summary>
    ''' <param name="fn">The function to test</param>
    ''' <param name="args">The list of arguments to pass to the function</param>
    Private Sub TestFnFail( _
     ByVal fn As clsFunction, ByVal args As IList(Of clsProcessValue))
        Try
            fn.Evaluate(args, Nothing)
            Assert.Fail("This function call should have thrown an exception")

        Catch fe As clsFunctionException
            ' We expected a FunctionException, so... success!

        Catch ae As AssertionException
            ' just means that the above assertion failure kicked in
            Throw

        Catch ex As Exception
            ' This means that an 'unhandled' exception was thrown
            Assert.Fail( _
             "Should have got a FunctionException, but received this instead: {0}", _
             ex)
        End Try
    End Sub

#End Region

    ''' <summary>
    ''' Generates an IList of clsProcessValue objects from the given param array
    ''' </summary>
    ''' <param name="args">The values to compound into a list</param>
    ''' <returns>A list containing the given arguments</returns>
    Private Function Values(ByVal ParamArray args() As clsProcessValue) _
     As IList(Of clsProcessValue)
        Return GetReadOnly.IList(args)
    End Function

    ''' <summary>
    ''' Generates an IList of DataType objects from the given param array
    ''' </summary>
    ''' <param name="args">The types to compound into a list</param>
    ''' <returns>A list containing the given arguments</returns>
    Private Function Types(ByVal ParamArray args() As DataType) As Signature
        Return New Signature(args)
    End Function

    ''' <summary>
    ''' Generates an IList of signatures from the given lists of data types
    ''' </summary>
    ''' <param name="args">The signatures to compound into a list</param>
    ''' <returns>A list containing the given arguments</returns>
    Private Function Sigs(ByVal ParamArray args() As Signature) As IList(Of Signature)
        Return New List(Of Signature)(args)
    End Function

#End Region

#Region " Tests for function-related elements "

    ''' <summary>
    ''' Tests the EnsureParams method in the clsFunction class
    ''' </summary>
    <Test()> _
    Public Sub TestEnsureParams()
        Dim stub As New StubFunction()

        ' 1 => ((number)) => 1
        stub.TestArgs(Values(1), Sigs(Types(DataType.number)), Values(1))

        ' "1" => ((number)) => 1
        stub.TestArgs(Values("1"), Sigs(Types(DataType.number)), Values(1))

        ' If multiple signatures can apply, the most compatible one should be
        ' chosen over one where a cast is required...

        ' "1" => ((number)(text)) => "1"
        stub.TestArgs(Values("1"), _
         Sigs(Types(DataType.number), Types(DataType.text)), Values("1"))

        ' "1" => ((text)(number)) => "1"
        stub.TestArgs(Values("1"), _
         Sigs(Types(DataType.text), Types(DataType.number)), Values("1"))

        ' If multiple signatures with equal compatibility to the args are found,
        ' the first one in the list of signatures should be applied.

        ' "1", 1 => ((number,number),(text,text)) => 1,1
        stub.TestArgs(Values("1", 1), Sigs( _
          Types(DataType.number, DataType.number), _
          Types(DataType.text, DataType.text)), _
         Values(1, 1) _
        )

        ' "1", 1 => ((text,text),(number,number)) => "1", "1"
        stub.TestArgs(Values("1", 1), Sigs( _
          Types(DataType.text, DataType.text), _
          Types(DataType.number, DataType.number)), _
         Values("1", "1") _
        )

        ' "one" => ((number)) => <fail>
        stub.TestArgsFail("function requires 1 argument of type: (number)", _
         Values("one"), Sigs(Types(DataType.number)))

        ' True => ((number)) => <fail>
        stub.TestArgsFail("function requires 1 argument of type: (number)", _
         Values(True), Sigs(Types(DataType.number)))

        ' "yesterday" => ((date)) => <fail>
        stub.TestArgsFail("function requires 1 argument of type: (date)", _
         Values("yesterday"), Sigs(Types(DataType.date)))

    End Sub

    ''' <summary>
    ''' Tests the signature, primarily the <see cref="Signature.WillMatch"/> method
    ''' </summary>
    <Test()> _
    Public Sub TestSignature()
        Dim percent As Double
        Dim sig As Signature

        ' === Empty signature ===
        sig = New Signature()
        Assert.That(sig.ToString(), [Is].EqualTo("()"))

        ' Empty list of values - will match - 100% compat
        Assert.True( _
         sig.WillMatch(GetEmpty.ICollection(Of clsProcessValue), percent))
        Assert.That(percent, [Is].EqualTo(100.0R))

        ' Single numeric value - will not match - 0% compat
        Assert.False( _
         sig.WillMatch(GetSingleton.ICollection(Of clsProcessValue)(1), percent))
        Assert.That(percent, [Is].EqualTo(0.0R))

        ' Single text value - will not match - 0% compat
        Assert.False( _
         sig.WillMatch(GetSingleton.ICollection(Of clsProcessValue)(""), percent))
        Assert.That(percent, [Is].EqualTo(0.0R))

        ' 3 args - (text,number,datetime) - will not match - 0% compat
        Assert.False( _
         sig.WillMatch(New clsProcessValue() {"1", 2, Date.Now}, percent))
        Assert.That(percent, [Is].EqualTo(0.0R))

        ' Empty array of values - will match - 100% compat
        Assert.True( _
         sig.WillMatch(New clsProcessValue() {}, percent))
        Assert.That(percent, [Is].EqualTo(100.0R))

        ' === Single text param signature ===
        sig = New Signature(DataType.text)
        Assert.That(sig.ToString(), [Is].EqualTo("(text)"))

        ' Empty args - no match - 0%
        Assert.False(sig.WillMatch(GetEmpty.ICollection(Of clsProcessValue), percent))
        Assert.That(percent, [Is].EqualTo(0.0R))

        ' Single text arg - will match - 100% compat
        Assert.True( _
         sig.WillMatch(GetSingleton.ICollection(Of clsProcessValue)("1"), percent))
        Assert.That(percent, [Is].EqualTo(100.0R))

        ' Single numeric arg - will match (by conversion) - 0% compat
        Assert.True( _
         sig.WillMatch(GetSingleton.ICollection(Of clsProcessValue)(1), percent))
        Assert.That(percent, [Is].EqualTo(0.0R))

        ' Single binary arg - will not match - 0% compat
        Assert.False(sig.WillMatch( _
         GetSingleton.ICollection(Of clsProcessValue)(New Byte() {1, 2, 3}), percent))
        Assert.That(percent, [Is].EqualTo(0.0R))

        ' Single flag arg - will match - 0% compat
        Assert.True(sig.WillMatch( _
         GetSingleton.ICollection(Of clsProcessValue)(True), percent))
        Assert.That(percent, [Is].EqualTo(0.0R))

        ' === Single numeric param signature ===
        sig = New Signature(DataType.number)
        Assert.That(sig.ToString(), [Is].EqualTo("(number)"))

        ' Single numeric arg - will match (by conversion) - 100% compat
        Assert.True( _
         sig.WillMatch(GetSingleton.ICollection(Of clsProcessValue)(1), percent))
        Assert.That(percent, [Is].EqualTo(100.0R))

        ' Single text arg containing number - will match (by conversion) - 0% compat
        Assert.True( _
         sig.WillMatch(GetSingleton.ICollection(Of clsProcessValue)("1"), percent))
        Assert.That(percent, [Is].EqualTo(0.0R))

        ' Single text arg containing non-num - will not match - 0% compat
        Assert.False( _
         sig.WillMatch(GetSingleton.ICollection(Of clsProcessValue)("one"), percent))
        Assert.That(percent, [Is].EqualTo(0.0R))

        ' Single flag arg - will not match - 0% compat
        Assert.False( _
         sig.WillMatch(GetSingleton.ICollection(Of clsProcessValue)(True), percent))
        Assert.That(percent, [Is].EqualTo(0.0R))

        ' === Two numeric params ===
        sig = New Signature(DataType.number, DataType.number)
        Assert.That(sig.ToString(), [Is].EqualTo("(number,number)"))

        ' Empty args - no match - 0%
        Assert.False(sig.WillMatch(GetEmpty.ICollection(Of clsProcessValue), percent))
        Assert.That(percent, [Is].EqualTo(0.0R))

        ' Single numeric arg - no match - 0%
        Assert.False( _
         sig.WillMatch(GetSingleton.ICollection(Of clsProcessValue)(1), percent))
        Assert.That(percent, [Is].EqualTo(0.0R))

        ' Two numeric args - match - 100%
        Assert.True(sig.WillMatch(New clsProcessValue() {1, 2}, percent))
        Assert.That(percent, [Is].EqualTo(100.0R))

        ' Three numeric args - no match - 0%
        Assert.False(sig.WillMatch(New clsProcessValue() {1, 2, 3}, percent))
        Assert.That(percent, [Is].EqualTo(0.0R))

        ' Two numeric args, one flag - no match - 0%
        Assert.False(sig.WillMatch(New clsProcessValue() {1, 2, True}, percent))
        Assert.That(percent, [Is].EqualTo(0.0R))

        ' One numeric, one text (containing number) - match - 50%
        Assert.True(sig.WillMatch(New clsProcessValue() {1, "2"}, percent))
        Assert.That(percent, [Is].EqualTo(50.0R))

        ' One text (containing number), one numeric - match - 50%
        Assert.True(sig.WillMatch(New clsProcessValue() {"1", 2}, percent))
        Assert.That(percent, [Is].EqualTo(50.0R))

        ' Two text (containing number) - match - 0%
        Assert.True(sig.WillMatch(New clsProcessValue() {"1", "2"}, percent))
        Assert.That(percent, [Is].EqualTo(0.0R))

    End Sub

#End Region

#Region " Actual function tests "

    ''' <summary>
    ''' Tests the Round() function
    ''' </summary>
    <Test()> _
    Public Sub TestRound()
        Dim fnRound As clsFunction = GetFn("Round", DataType.number, "Number")

        ' === Round(x) ===
        TestFn(fnRound, 1D, 1D)
        TestFn(fnRound, 0D, 0D)
        TestFn(fnRound, -1D, -1D)
        TestFn(fnRound, 1.9D, 2D)
        TestFn(fnRound, 1.1D, 1D)
        TestFn(fnRound, 0.1D, 0D)
        TestFn(fnRound, 0.9D, 1D)
        TestFn(fnRound, -0.1D, 0D)
        TestFn(fnRound, -0.9D, -1D)

        ' The default rounding rounds midpoints to the nearest even number
        TestFn(fnRound, 0.5D, 0D)
        TestFn(fnRound, 1.5D, 2D)
        TestFn(fnRound, 2.5D, 2D)
        TestFn(fnRound, -1.5D, -2D)
        TestFn(fnRound, -0.5D, 0D)
        TestFn(fnRound, -2.5D, -2D)
        TestFn(fnRound, -2.50001D, -3D)

        ' === Round(x,y) ===
        TestFn(fnRound, 0.55D, 1, 0.6D)
        TestFn(fnRound, 0.55D, 0, 1)
        TestFn(fnRound, 1, 5, 1)
        TestFn(fnRound, 0, 12, 0)
        TestFn(fnRound, 17.5D, 1, 17.5D)

        ' Round() uses banker's rounding. Make sure that holds
        TestFn(fnRound, 2.55D, 1, 2.6D)
        TestFn(fnRound, 2.65D, 1, 2.6D)
        TestFn(fnRound, 2.75D, 1, 2.8D)
        TestFn(fnRound, 2.85D, 1, 2.8D)

    End Sub

    ''' <summary>
    ''' Tests the RndUp function
    ''' </summary>
    <Test()> _
    Public Sub TestRndUp()
        Dim fnRndUp As clsFunction = GetFn("RndUp", DataType.number, "Number")

        ' === RndUp(x) ===
        TestFn(fnRndUp, 0, 0)
        TestFn(fnRndUp, 1, 1)
        TestFn(fnRndUp, 0.5, 1)
        TestFn(fnRndUp, 1.1, 2)
        TestFn(fnRndUp, 1.5, 2)
        TestFn(fnRndUp, 1.9, 2)
        TestFn(fnRndUp, 0.1, 1)
        TestFn(fnRndUp, 0.999, 1)
        TestFn(fnRndUp, -1, -1)

        ' er...
        ' These appear to be wrong to me... surely a roundup from -0.9 should be 0
        ' Regardless, that's the way they've been since commit a811dfe99e (and
        ' presumably before, but that's when the unit tests were added to
        ' APCUnitTest)
        TestFn(fnRndUp, -0.9, -1)
        TestFn(fnRndUp, -1.1, -2)

        ' === RndUp(x,y) ===
        TestFn(fnRndUp, 0, 1, 0)
        TestFn(fnRndUp, 1, 1, 1)
        TestFn(fnRndUp, 0, 0, 0)
        TestFn(fnRndUp, 1, 0, 1)
        TestFn(fnRndUp, -1, 1, -1)
        TestFn(fnRndUp, 0.5D, 1, 0.5D)
        TestFn(fnRndUp, 1.5D, 1, 1.5D)
        TestFn(fnRndUp, -1.5D, 1, -1.5D)
        TestFn(fnRndUp, 1.51D, 1, 1.6D)
        TestFn(fnRndUp, 1.59D, 1, 1.6D)
        TestFn(fnRndUp, 0.9999D, 1, 1)
        TestFn(fnRndUp, 0.123456789D, 5, 0.12346D)

        ' Still hmmming
        TestFn(fnRndUp, -1.505D, 2, -1.51D)
        TestFn(fnRndUp, -0.12345D, 2, -0.13D)
        TestFn(fnRndUp, -0.123456789D, 5, -0.12346D)
        TestFn(fnRndUp, -0.123456789D, 0, -1)

    End Sub

    ''' <summary>
    ''' Tests the RndDn function
    ''' </summary>
    <Test()> _
    Public Sub TestRndDn()
        Dim fnRndDn As clsFunction = GetFn("RndDn", DataType.number, "Number")

        ' === RndDn(x) ===
        TestFn(fnRndDn, 0, 0)
        TestFn(fnRndDn, 1, 1)
        TestFn(fnRndDn, 0.5, 0)
        TestFn(fnRndDn, 1.1, 1)
        TestFn(fnRndDn, 1.5, 1)
        TestFn(fnRndDn, 1.9, 1)
        TestFn(fnRndDn, 0.1, 0)
        TestFn(fnRndDn, 0.999, 0)
        TestFn(fnRndDn, -1, -1)

        ' er... same as the equivalent RndUp functions; these seem to be the
        ' wrong way around for me; nevertheless it's been that way for aeons
        ' so we should ensure that it stays that way
        TestFn(fnRndDn, -0.9, 0)
        TestFn(fnRndDn, -1.1, -1)

        ' === RndDn(x,y) ===
        TestFn(fnRndDn, 0, 1, 0)
        TestFn(fnRndDn, 1, 1, 1)
        TestFn(fnRndDn, 0, 0, 0)
        TestFn(fnRndDn, 1, 0, 1)
        TestFn(fnRndDn, -1, 1, -1)
        TestFn(fnRndDn, 0.5D, 1, 0.5D)
        TestFn(fnRndDn, 1.5D, 1, 1.5D)
        TestFn(fnRndDn, -1.5D, 1, -1.5D)
        TestFn(fnRndDn, 1.51D, 1, 1.5D)
        TestFn(fnRndDn, 1.59D, 1, 1.5D)
        TestFn(fnRndDn, 0.9999D, 1, 0.9D)
        TestFn(fnRndDn, 0.123456789D, 5, 0.12345D)

        ' The weird negative handling again
        TestFn(fnRndDn, -1.505D, 2, -1.5D)
        TestFn(fnRndDn, -0.12345D, 2, -0.12D)
        TestFn(fnRndDn, -0.123456789D, 5, -0.12345D)
        TestFn(fnRndDn, -0.123456789D, 0, 0)

    End Sub

    ''' <summary>
    ''' Tests the DecPad function
    ''' </summary>
    <Test()> _
    Public Sub TestDecPad()
        Dim fn As clsFunction = GetFn("DecPad", DataType.text, "Number")
        TestFn(fn, 0, 0, "0")
        TestFn(fn, 1, 0, "1")
        TestFn(fn, 0, 1, "0.0")
        TestFn(fn, 1, 1, "1.0")
        TestFn(fn, 0.5D, 1, "0.5")
        TestFn(fn, 0.5D, 2, "0.50")
        TestFn(fn, 0.5D, 5, "0.50000")
        TestFn(fn, 0, 2, "0.00")
        TestFn(fn, 3, 2, "3.00")
        TestFn(fn, 1.2345, 10, "1.2345000000")
        TestFn(fn, -1.5D, 1, "-1.5")
        TestFn(fn, 2.46D, 1000, "2.46" & New String("0"c, 998))
        TestFn(fn, 2.46D, -2, "2")
        TestFn(fn, -5.2D, 4, "-5.2000")

        ' DecPad rounds if there's more decimal places than are required by the
        ' 'places' argument.
        TestFn(fn, 0.5D, 0, "0")
        TestFn(fn, 1.5D, 0, "2")
        TestFn(fn, 1.23456789D, 1, "1.2")
        TestFn(fn, 1.23456789D, 2, "1.23")
        TestFn(fn, 1.23456789D, 3, "1.235")
        TestFn(fn, 1.23456789D, 4, "1.2346")
        TestFn(fn, 1.23456789D, 5, "1.23457")
        TestFn(fn, 1.23456789D, 6, "1.234568")
        TestFn(fn, 1.23456789D, 7, "1.2345679")
        TestFn(fn, 1.23456789D, 8, "1.23456789")
        TestFn(fn, 1.23456789D, 9, "1.234567890")

        ' DecPad() should round the same way as the Round() function, which is
        ' currently banker's rounding - ie. "round half to even" - bug 8210
        TestFn(fn, 2.55D, 1, "2.6")
        TestFn(fn, 2.65D, 1, "2.6")
        TestFn(fn, 2.75D, 1, "2.8")
        TestFn(fn, 2.85D, 1, "2.8")

        ' Ensure that the rounding DecPad() works the same way as Round() -
        ' also bug 8210
        Dim fnRound As clsFunction = Functions("Round")

        For Each d As Decimal In New Decimal() _
         {0.5D, 1.5D, 2.5D, 3.5D, 4.5D, -0.5D, -1.5D, -2.5D, -3.5D}

            Dim rndOut As clsProcessValue, padOut As clsProcessValue
            Dim params() As clsProcessValue = {d, 0}
            rndOut = fn.Evaluate(params, Nothing).CastInto(DataType.number)
            padOut = fn.Evaluate(params, Nothing).CastInto(DataType.number)

            Assert.That(rndOut, [Is].EqualTo(padOut), _
             "Round({0}, 0) = {1}; DecPad({2}, 0) = {3}", _
             d, CDec(rndOut), d, CDec(padOut))
        Next

        ' The output should be coincident with the culture in place
        Using New CultureBlock("fr-FR")
            TestFn(fn, 0, 0, "0")
            TestFn(fn, 1, 0, "1")
            TestFn(fn, 0, 1, "0,0")
            TestFn(fn, 1, 1, "1,0")
            TestFn(fn, 0.5D, 1, "0,5")
            TestFn(fn, 0.5D, 2, "0,50")
            TestFn(fn, 0.5D, 5, "0,50000")
            TestFn(fn, 0, 2, "0,00")
            TestFn(fn, 3, 2, "3,00")
            TestFn(fn, 1.2345, 10, "1,2345000000")
            TestFn(fn, -1.5D, 1, "-1,5")
        End Using

    End Sub

    ''' <summary>
    ''' Tests the Sqrt() function
    ''' </summary>
    <Test()> _
    Public Sub TestSqrt()
        Dim fn As clsFunction = GetFn("Sqrt", DataType.number, "Number")
        TestFn(fn, 0, 0)
        TestFn(fn, 1, 1)
        TestFn(fn, 4, 2)
        TestFn(fn, 9, 3)

        Try
            fn.Evaluate(New clsProcessValue() {-1}, Nothing)
            Assert.Fail("Sqrt(-1) should give an error")
        Catch ' Good. It should fail
        End Try

    End Sub

    <Test()> _
    Public Sub TestLog()
        Dim fn As clsFunction = GetFn("Log", DataType.number, "Number")

        TestFn(fn, 1, 10, 0)
        TestFn(fn, 1, 27, 0)
        TestFn(fn, 10, 10, 1)
        TestFn(fn, 16, 256, 0.5D)
        TestFn(fn, 100, 10, 2)
        TestFn(fn, 9, 3, 2)
        TestFn(fn, 6 ^ 2, 6, 2)
        TestFn(fn, 81, 7, 2.25830013621432D)
        TestFn(fn, 1, 0.5D, 0)
        TestFn(fn, 16, 0.5D, -4)

        Dim result As clsProcessValue = TestFn(fn, 5, 10, Nothing)
        Assert.That(Math.Round(CDec(result), 5), [Is].EqualTo(0.69897D))

        TestFnFail(fn, 0, 1)
        TestFnFail(fn, 1, 0)
        TestFnFail(fn, 0, 10)
        TestFnFail(fn, 10, 0)
        TestFnFail(fn, -1, 10)
        TestFnFail(fn, 10, -1)
        ' Taken from 'bad expression list' tests in APCUnitTest
        TestFnFail(fn, 16, -2)
        TestFnFail(fn, -1000, 10)
        TestFnFail(fn, -1000, -10)
    End Sub

    <Test()> _
    Public Sub TestLeft()
        Dim fn As clsFunction = GetFn("Left", DataType.text, "Text")

        TestFn(fn, "Leftie", 4, "Left")
        TestFn(fn, "Lef", 4, "Lef")
        TestFn(fn, 205, 2, "20")
        TestFn(fn, False, 1, "F")
        TestFn(fn, "", 0, "")
        TestFn(fn, "", 1, "")
        TestFn(fn, "", 5, "")
        TestFn(fn, "What is this I don't even?", 0, "")

        ' It's silly, but it works, so...
        TestFn(fn, "12345", 2.5, "12")

        TestFnFail(fn, "This is not good", -1)
        TestFnFail(fn, "", -10)

    End Sub

    <Test()> _
    Public Sub TestRight()
        Dim fn As clsFunction = GetFn("Right", DataType.text, "Text")

        TestFn(fn, "AllRight", 5, "Right")
        TestFn(fn, "", 29.5, "")
        TestFn(fn, 12345, 3, "345")
        TestFn(fn, True, 1, "e")
        TestFn(fn, "This is a crock", 0, "")

        TestFnFail(fn, "Wassup?", -28)

    End Sub

    <Test()> _
    Public Sub TestMid()
        Dim fn As clsFunction = GetFn("Mid", DataType.text, "Text")

        TestFn(fn, "OneTwoThree", 4, 3, "Two")
        TestFn(fn, "OneTwoThree", 4, 0, "")
        TestFn(fn, "OneTwoThree", 1, 3, "One")
        TestFn(fn, "OneTwoThree", 7, 5, "Three")

        ' If the length or the string index is beyond the end of the text item,
        ' an empty text value is returned (according to the VB Mid() rules)
        TestFn(fn, "OneTwoThree", 7, 10, "Three")
        TestFn(fn, "OneTwoThree", 17, 4, "")

        ' An index of < 1 or a negative length, however, result in an exception
        TestFnFail(fn, "OneTwoThree", 0, 5)
        TestFnFail(fn, "OneTwoThree", -4, 5)
        TestFnFail(fn, "OneTwoThree", 5, -2)

    End Sub

    <Test()> _
    Public Sub TestChr()
        Dim fn As clsFunction = GetFn("Chr", DataType.text, "Text")

        TestFn(fn, 32, " ")
        TestFn(fn, 65, "A")
        TestFn(fn, 0, vbNullChar)
        TestFn(fn, 97, "a")
        TestFn(fn, "97", "a")
        TestFn(fn, 126, "~")

        TestFnFail(fn, True)
        TestFnFail(fn, 28872)
        TestFnFail(fn, -5)

    End Sub

    <Test()> _
    Public Sub TestNewLine()
        Dim fn As clsFunction = GetFn("NewLine", DataType.text, "Text", False, True)
        TestFn(fn, vbCrLf)
        ' Um... not really sure what else I can test here...
    End Sub

    <Test()> _
    Public Sub TestInStr()
        Dim fn As clsFunction = GetFn("InStr", DataType.number, "Text")
        TestFn(fn, "Automate", "toma", 3)
        TestFn(fn, "Automate", "tomato", 0)
        TestFn(fn, "", "wat is this i dont even", 0)
        TestFn(fn, "abcdefg", "ABCDEFG", 0)
        TestFn(fn, "abcdefg", "abcdefg", 1)
        TestFn(fn, "abcdefg", "abcdefgh", 0)
        TestFn(fn, "Starts Middles Ends", "Starts", 1)
        TestFn(fn, "Starts Middles Ends", "Ends", 16)
        TestFn(fn, "Starts Middles Ends", "Middles", 8)
        TestFn(fn, "Starts Middles Ends", " ", 7)

    End Sub

    <Test()> _
    Public Sub TestLen()
        Dim fn As clsFunction = GetFn("Len", DataType.number, "Text")
        TestFn(fn, "Automate", 8)
        TestFn(fn, "", 0)
        TestFn(fn, "-1", 2)

        TestFn(fn, New Byte() {1, 2, 3}, 3)
        TestFn(fn, New Byte() {}, 0)
        TestFn(fn, New clsProcessValue(DataType.binary), 0)

    End Sub

    <Test()> _
    Public Sub TestBytes()
        Dim fn As clsFunction = GetFn("Bytes", DataType.number, "Data")

    End Sub

    <Test()> _
    Public Sub TestTrimStart()
        Dim fn As clsFunction = GetFn("TrimStart", DataType.text, "Text")

        TestFn(fn, "  automate  ", "automate  ")
        TestFn(fn, _
         vbTab & "  endings  " & vbTab & vbCrLf, "endings  " & vbTab & vbCrLf)
        TestFn(fn, "  endings  " & vbCrLf & vbTab, "endings  " & vbCrLf & vbTab)
        TestFn(fn, "", "")
        TestFn(fn, "fish", "fish")

    End Sub

    <Test()> _
    Public Sub TestTrimEnd()
        Dim fn As clsFunction = GetFn("TrimEnd", DataType.text, "Text")

        TestFn(fn, "  automate  ", "  automate")
        TestFn(fn, vbTab & "  endings  " & vbTab & vbCrLf, vbTab & "  endings")
        TestFn(fn, "  endings  " & vbCrLf & vbTab, "  endings")
        TestFn(fn, "", "")
        TestFn(fn, "fish", "fish")
    End Sub

    <Test()> _
    Public Sub TestTrim()
        Dim fn As clsFunction = GetFn("Trim", DataType.text, "Text")

        TestFn(fn, "  automate  ", "automate")
        TestFn(fn, vbTab & "  endings  " & vbTab & vbCrLf, "endings")
        TestFn(fn, "  endings  " & vbCrLf & vbTab, "endings")
        TestFn(fn, "", "")
        TestFn(fn, "fish", "fish")
    End Sub

    <Test()> _
    Public Sub TestStartsWith()
        Dim fn As clsFunction = GetFn("StartsWith", DataType.flag, "Text")

        TestFn(fn, "automate", "", True)
        TestFn(fn, "automate", "a", True)
        TestFn(fn, "automate", "au", True)
        TestFn(fn, "automate", "aut", True)
        TestFn(fn, "automate", "auto", True)
        TestFn(fn, "automate", "autom", True)
        TestFn(fn, "automate", "automa", True)
        TestFn(fn, "automate", "automat", True)
        TestFn(fn, "automate", "automate", True)
        TestFn(fn, "automate", "automater", False)
        TestFn(fn, "automate", "automate", True)
        TestFn(fn, "automate", "utomate", False)
        TestFn(fn, "automate", "tomate", False)
        TestFn(fn, "automate", "omate", False)
        TestFn(fn, "automate", "mate", False)
        TestFn(fn, "automate", "ate", False)
        TestFn(fn, "automate", "te", False)
        TestFn(fn, "automate", "e", False)
        TestFn(fn, "automate", "", True)

        TestFn(fn, "UPPER", "u", False)
        TestFn(fn, "UPPER", "U", True)
        TestFn(fn, "UPPER", "r", False)
        TestFn(fn, "UPPER", "R", False)

        TestFn(fn, "", "empty", False)

    End Sub

    <Test()> _
    Public Sub TestEndsWith()
        Dim fn As clsFunction = GetFn("EndsWith", DataType.flag, "Text")

        TestFn(fn, "automate", "", True)
        TestFn(fn, "automate", "a", False)
        TestFn(fn, "automate", "au", False)
        TestFn(fn, "automate", "aut", False)
        TestFn(fn, "automate", "auto", False)
        TestFn(fn, "automate", "autom", False)
        TestFn(fn, "automate", "automa", False)
        TestFn(fn, "automate", "automat", False)
        TestFn(fn, "automate", "automate", True)
        TestFn(fn, "automate", "automater", False)
        TestFn(fn, "automate", "automate", True)
        TestFn(fn, "automate", "utomate", True)
        TestFn(fn, "automate", "tomate", True)
        TestFn(fn, "automate", "omate", True)
        TestFn(fn, "automate", "mate", True)
        TestFn(fn, "automate", "ate", True)
        TestFn(fn, "automate", "te", True)
        TestFn(fn, "automate", "e", True)
        TestFn(fn, "automate", "", True)

        TestFn(fn, "UPPER", "u", False)
        TestFn(fn, "UPPER", "U", False)
        TestFn(fn, "UPPER", "r", False)
        TestFn(fn, "UPPER", "R", True)

        TestFn(fn, "", "empty", False)
    End Sub

    <Test()> _
    Public Sub TestUpper()
        Dim fn As clsFunction = GetFn("Upper", DataType.text, "Text")
        TestFn(fn, "autoMATE", "AUTOMATE")
        TestFn(fn, "AUTOMATE", "AUTOMATE")
        TestFn(fn, "automate", "AUTOMATE")
        TestFn(fn, "", "")
        TestFn(fn, "12345", "12345")
        TestFn(fn, "a b c d e f g", "A B C D E F G")

        ' Fun fact - if the argument can be cast into a text item, it will be
        TestFn(fn, 12, "12")
        TestFn(fn, True, "TRUE")
        TestFn(fn, New SafeString("hunter2"), "HUNTER2")

        ' Some things, however, cannot be converted into a text item
        TestFnFail(fn, New Byte() {1, 2, 4})
        Using bmp As New Bitmap(2, 2)
            TestFnFail(fn, bmp)
        End Using

    End Sub

    <Test()> _
    Public Sub TestReplace()
        Dim fn As clsFunction = GetFn("Replace", DataType.text, "Text")
        TestFn(fn, "", "automate", "some-competitor", "")
        TestFn(fn, "automate", "te", "to", "automato")
        TestFn(fn, "automate", "t", "r", "auromare")
        TestFn(fn, "automate", "z", "x", "automate")
        TestFn(fn, "aaaaa", "aa", "bb", "bbbba")
        TestFn(fn, "     ", "   ", "-", "-  ")
        TestFn(fn, "automate", "automater", "filetmignon", "automate")

    End Sub

    <Test()> _
    Public Sub TestLower()
        Dim fn As clsFunction = GetFn("Lower", DataType.text, "Text")

    End Sub

    <Test()> _
    Public Sub TestToday()
        Dim fn As clsFunction = GetFn("Today", DataType.date, "Date")

    End Sub

    <Test()> _
    Public Sub TestLocalTime()
        Dim fn As clsFunction = GetFn("LocalTime", DataType.time, "Date")

    End Sub

    <Test()> _
    Public Sub TestUTCTime()
        Dim fn As clsFunction = GetFn("UTCTime", DataType.time, "Date")

    End Sub

    <Test()> _
    Public Sub TestNow()
        Dim fn As clsFunction = GetFn("Now", DataType.datetime, "Date")

    End Sub

    <Test()> _
    Public Sub TestDateAdd()
        Dim fn As clsFunction = GetFn("DateAdd", DataType.date, "Date")

    End Sub

    <Test()> _
    Public Sub TestDateDiff()
        Dim fn As clsFunction = GetFn("DateDiff", DataType.number, "Date")

    End Sub

    <Test()> _
    Public Sub TestMakeDate()
        Dim fn As clsFunction = GetFn("MakeDate", DataType.date, "Date")
        TestFn(fn, 4, 5, 2006, _
         New clsProcessValue(DataType.date, New Date(2006, 5, 4)))
        TestFn(fn, 29, 2, 2012, _
         New clsProcessValue(DataType.date, New Date(2012, 2, 29)))
        TestFn(fn, 29, 2, 2000, _
         New clsProcessValue(DataType.date, New Date(2000, 2, 29)))

        ' These test the arbitrary "2029 rule" mis-feature of the MakeDate()
        ' function, which is supported for backwards compatibility.

        TestFn(fn, 15, 10, 99, _
         New clsProcessValue(DataType.date, New Date(1999, 10, 15)))
        TestFn(fn, 15, 10, 0, _
         New clsProcessValue(DataType.date, New Date(2000, 10, 15)))
        TestFn(fn, 15, 10, 29, _
         New clsProcessValue(DataType.date, New Date(2029, 10, 15)))
        TestFn(fn, 15, 10, 30, _
         New clsProcessValue(DataType.date, New Date(1930, 10, 15)))

        TestFnFail(fn, 7, 31, 2006) ' US stylee - nope
        TestFnFail(fn, 31, 6, 2006) ' beyond June's allotted day count
        TestFnFail(fn, 29, 2, 2014) ' Not a leap year, not divisible by 4
        TestFnFail(fn, 29, 2, 1900) ' Also not a leap year, though divisible by 4

    End Sub

    <Test()> _
    Public Sub TestMakeDateTime()
        Dim fn As clsFunction = GetFn("MakeDateTime", DataType.datetime, "Date")

        TestFn(fn, 4, 5, 2006, 13, 10, 5, False, _
         New clsProcessValue(DataType.datetime, New Date(2006, 5, 4, 13, 10, 5)))

        TestFn(fn, 4, 5, 2006, 13, 10, 5, True, _
         New clsProcessValue(DataType.datetime, New Date(2006, 5, 4, 13, 10, 5).ToUniversalTime()))

    End Sub

    <Test()> _
    Public Sub TestMakeTime()
        Dim fn As clsFunction = GetFn("MakeTime", DataType.time, "Date")

        TestFn(fn, 10, 45, 15, _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 10, 45, 15)))
        TestFn(fn, 23, 15, 1, _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 23, 15, 1)))

    End Sub

    <Test()> _
    Public Sub TestMakeTimeSpan()
        Dim fn As clsFunction = GetFn("MakeTimeSpan", DataType.timespan, "Date")

        TestFn(fn, 10, 11, 4, 59, 323, New TimeSpan(10, 11, 4, 59))
        TestFn(fn, -10, 11, -4, -59, 323, New TimeSpan(9, 13, 4, 59).Negate())
        TestFn(fn, 10, 11, 4, 59, New TimeSpan(10, 11, 4, 59))
        TestFn(fn, -10, 11, -4, -59, New TimeSpan(9, 13, 4, 59).Negate())

    End Sub

    <Test()> _
    Public Sub TestAddMonths()
        Dim fn As clsFunction = GetFn("AddMonths", DataType.date, "Date")

    End Sub

    <Test()> _
    Public Sub TestAddDays()
        Dim fn As clsFunction = GetFn("AddDays", DataType.date, "Date")

    End Sub

    <Test()> _
    Public Sub TestFormatDate()
        Dim fn As clsFunction = GetFn("FormatDate", DataType.text, "Date")

    End Sub

    <Test()> _
    Public Sub TestFormatDateTime()
        Dim fn As clsFunction = GetFn("FormatDateTime", DataType.text, "Date")

    End Sub

    <Test()> _
    Public Sub TestFormatUTCDateTime()
        Dim fn As clsFunction = GetFn("FormatUTCDateTime", DataType.text, "Date")

    End Sub

    <Test()> _
    Public Sub TestLoadBinaryFile()
        Dim fn As clsFunction = GetFn("LoadBinaryFile", DataType.binary, "File")

    End Sub

    <Test()> _
    Public Sub TestLoadTextFile()
        Dim fn As clsFunction = GetFn("LoadTextFile", DataType.text, "File")

    End Sub

    <Test()> _
    Public Sub TestIsNumber()
        Dim fn As clsFunction = GetFn("IsNumber", DataType.flag, "Logic")

        TestFn(fn, "0", True)
        TestFn(fn, "1", True)
        TestFn(fn, "0212", True)
        TestFn(fn, "43.4355", True)
        ' Overflow
        TestFn(fn, "38247892387472834823894283482374434234234234234", False)
        TestFn(fn, "", False)
        TestFn(fn, "x", False)
        TestFn(fn, "-", False)
        TestFn(fn, "nonsense", False)
        TestFn(fn, "-1", True)
        TestFn(fn, "0.0.344", False)

        ' Test culture-specific stuff
        TestFn(fn, "1,5", True) ' Treats it as a thousand-separator - ie. "1,5" = 15
        TestFn(fn, "1.5", True)

        TestFn(fn, "1 000", False)
        TestFn(fn, "1 000,500", False)
        TestFn(fn, "1'234'567", False)
        TestFn(fn, "1'234'567.5", False)
        TestFn(fn, "1.234.567", False)
        TestFn(fn, "1.234.567,5", False)

        Using New CultureBlock("fr-FR") ' French, spaces for digit gps
            TestFn(fn, "1,5", True)
            TestFn(fn, "1 000", True)
            TestFn(fn, "1 000,500", True)
            TestFn(fn, "1.5", True) ' Fall back to the default if local doesn't work
        End Using

        Using New CultureBlock("de-CH") ' Swiss/German - single quotes for digit gps
            ' The thousands separator in this culture changes between Windows versions 
            ' so insert the correct one for the current environment
            Dim sep = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator
            TestFn(fn, $"1{sep}234{sep}567", True)
            TestFn(fn, $"1{sep}234{sep}567.5", True)

        End Using

        Using New CultureBlock("is-IS") ' Icelandic - dots for digit groups
            TestFn(fn, "1.234.567", True)
            TestFn(fn, "1.234.567,5", True)
        End Using

    End Sub

    <Test()> _
    Public Sub TestIsDate()
        Dim fn As clsFunction = GetFn("IsDate", DataType.flag, "Logic")

        ' Some nice straightforward dates in enGB or universal format
        TestFn(fn, "12/05/2010", True)
        TestFn(fn, "2010-12-30", True)

        ' And in the internal format, largely for backward compatibility
        TestFn(fn, "2005/01/24", True)

        ' Empty String? Not a date, say I
        TestFn(fn, "", False)

        ' We'll have no silly American date styles round here
        TestFn(fn, "06/17/2012", False)

        ' ... unless, er, you know, you're American
        Using New CultureBlock("en-US")
            TestFn(fn, "06/17/2012", True)
        End Using

        ' 2-digit dates are dates too
        TestFn(fn, "31/12/99", True)
        TestFn(fn, "12/31/99", False)

        Using New CultureBlock("en-US")
            TestFn(fn, "12/31/99", True)
        End Using

        ' Let's try some dots. The parsing is actually pretty smart at allowing
        ' for different date separator symbols
        TestFn(fn, "7.5.2012", True)

        Using New CultureBlock("cs-CZ")
            TestFn(fn, "24.7.2014", True)
            ' As above, so below:
            TestFn(fn, "7/05/2012", True)
        End Using

        ' And test some nonsense
        TestFn(fn, "nonsense", False)
        TestFn(fn, 7, False)
        TestFn(fn, True, False)
        TestFn(fn, New Byte() {1, 2, 3}, False)
        TestFn(fn, TimeSpan.FromDays(1.423), False)

        ' We can convert from a date into a date; quite simply actually
        TestFn(fn, _
         New clsProcessValue(DataType.date, New Date(2004, 1, 17, 12, 30, 0)), True)

        ' and a datetime
        TestFn(fn, _
         New clsProcessValue(DataType.datetime, New Date(2004, 1, 17, 12, 30, 0)), True)

        '  but not a time
        TestFn(fn, _
         New clsProcessValue(DataType.time, New Date(2004, 1, 17, 12, 30, 0)), False)


    End Sub

    <Test()> _
    Public Sub TestIsTime()
        Dim fn As clsFunction = GetFn("IsTime", DataType.flag, "Logic")

        TestFn(fn, "14:00", True)
        TestFn(fn, "14:00:15", True)

        ' This apparently works!
        TestFn(fn, "2AM", True)

        TestFn(fn, "", False)
        TestFn(fn, 7, False)
        TestFn(fn, New Byte() {1, 2, 3}, False)
        TestFn(fn, "nonsense", False)

    End Sub

    <Test()> _
    Public Sub TestIsDateTime()
        Dim fn As clsFunction = GetFn("IsDateTime", DataType.flag, "Logic")

        TestFn(fn, "2006-12-21 14:01:00Z", True)
        TestFn(fn, "21/12/2006 12:30:00Z", True)
        TestFn(fn, "21/5/1997 01:30:00Z", True)

        TestFn(fn, "", False)
        TestFn(fn, "nonsense", False)

    End Sub

    <Test()> _
    Public Sub TestIsTimeSpan()
        Dim fn As clsFunction = GetFn("IsTimeSpan", DataType.flag, "Logic")

        TestFn(fn, "", False)
        TestFn(fn, "nonsense", False)

    End Sub

    <Test()> _
    Public Sub TestIsFlag()
        Dim fn As clsFunction = GetFn("IsFlag", DataType.flag, "Logic")

        ' "Happy tests"
        TestFn(fn, "True", True)
        TestFn(fn, "False", True)

        ' Case is ignored
        TestFn(fn, "true", True)
        TestFn(fn, "false", True)

        ' Whitespace is trimmed in the check
        TestFn(fn, " TRUE", True)
        TestFn(fn, " fAlSE  ", True)

        ' But not internal whitespace
        TestFn(fn, "T r u e", False)

        ' Of course flags are flags too
        TestFn(fn, True, True)
        TestFn(fn, False, True)

        ' Number strings are not welcome here
        TestFn(fn, "1", False)
        TestFn(fn, "0", False)

        ' Nor empty strings
        TestFn(fn, "", False)

        ' And this is just obviously not a flag. It even says so
        TestFn(fn, "This is not a flag", False)

        ' Only text and flag values need apply
        TestFn(fn, 1, False)
        TestFn(fn, Encoding.ASCII.GetBytes("True"), False)
        TestFn(fn, New clsProcessValue(), False)
        TestFn(fn, TimeSpan.Zero, False)
        TestFn(fn, New clsProcessValue(DataType.date, New Date(2001, 12, 6)), False)

    End Sub

    <Test()> _
    Public Sub TestExceptionType()
        Dim fn As clsFunction = _
         GetFn("ExceptionType", DataType.text, "Exceptions", True, False)

    End Sub

    <Test()> _
    Public Sub TestExceptionDetail()
        Dim fn As clsFunction = _
         GetFn("ExceptionDetail", DataType.text, "Exceptions", True, False)

    End Sub

    <Test()> _
    Public Sub TestExceptionStage()
        Dim fn As clsFunction = _
         GetFn("ExceptionStage", DataType.text, "Exceptions", True, False)

    End Sub

    <Test()> _
    Public Sub TestToNumber()
        Dim fn As clsFunction = GetFn("ToNumber", DataType.number, "Conversion")

        TestFn(fn, "0", 0)
        TestFn(fn, "1", 1)
        TestFn(fn, "10", 10)
        TestFn(fn, "0212", 212)
        TestFn(fn, "43.4355", 43.4355D)
        TestFn(fn, "123456789", 123456789D)
        TestFn(fn, "0.14324", 0.14324D)

        TestFnFail(fn, "38247892387472834823894283482374434234234234234", False)
        TestFnFail(fn, "x")
        TestFnFail(fn, "-")
        TestFn(fn, "-1", -1)
        TestFnFail(fn, "0.0.344")
        TestFnFail(fn, "1'234'567.5")

        ' Test culture-specific stuff
        TestFn(fn, "1,5", 15) ' Treats it as a thousand-separator - ie. "1,5" = 15
        TestFn(fn, "1.5", 1.5D)

        Using New CultureBlock("fr-FR")
            TestFn(fn, "1,5", 1.5D)
            TestFn(fn, "1.5", 1.5D)
        End Using

        Using New CultureBlock("de-CH")
            ' The thousands separator in this culture changes between Windows versions 
            ' so insert the correct one for the current environment
            Dim sep = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator
            TestFn(fn, $"1{sep}234{sep}567.5", 1234567.5D)
        End Using

    End Sub

    <Test()> _
    Public Sub TestToTime()
        Dim fn As clsFunction = GetFn("ToTime", DataType.time, "Conversion")

        TestFn(fn, "2AM", _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 2, 0, 0)))

        TestFn(fn, "12:34:45", _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 12, 34, 45)))

        TestFn(fn, "12:34", _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 12, 34, 0)))

        TestFnFail(fn, "")
        TestFnFail(fn, "7.12:34:45")
        TestFnFail(fn, 81)

    End Sub

    <Test()> _
    Public Sub TestToDateTime()
        Dim fn As clsFunction = GetFn("ToDateTime", DataType.datetime, "Conversion")

        TestFn(fn, "10/12/2004", _
         New clsProcessValue(DataType.datetime, New Date(2004, 12, 10, 0, 0, 0)))
        TestFn(fn, "2004-12-10", _
         New clsProcessValue(DataType.datetime, New Date(2004, 12, 10, 0, 0, 0)))
        TestFn(fn, "10/12/04", _
         New clsProcessValue(DataType.datetime, New Date(2004, 12, 10, 0, 0, 0)))
        TestFn(fn, "7/5/99", _
         New clsProcessValue(DataType.datetime, New Date(1999, 5, 7, 0, 0, 0)))
        TestFn(fn, "7/5/04", _
         New clsProcessValue(DataType.datetime, New Date(2004, 5, 7, 0, 0, 0)))
        TestFn(fn, "10.12.2004", _
         New clsProcessValue(DataType.datetime, New Date(2004, 12, 10, 0, 0, 0)))

        TestFn(fn, "10/12/2004 07:13", _
         New clsProcessValue(DataType.datetime, New Date(2004, 12, 10, 7, 13, 0)))

        TestFn(fn, "10/12/2004 07:13:00", _
         New clsProcessValue(DataType.datetime, New Date(2004, 12, 10, 7, 13, 0)))

        TestFn(fn, "10/12/2004 07:13:51", _
         New clsProcessValue(DataType.datetime, New Date(2004, 12, 10, 7, 13, 51)))

        TestFn(fn, "10/12/2004 2:30 PM", _
         New clsProcessValue(DataType.datetime, New Date(2004, 12, 10, 14, 30, 0)))

        TestFn(fn, "10/12/2004 9AM", _
         New clsProcessValue(DataType.datetime, New Date(2004, 12, 10, 9, 0, 0)))

        TestFn(fn, "2004-12-10 01:02:03", _
         New clsProcessValue(DataType.datetime, New Date(2004, 12, 10, 1, 2, 3)))

        TestFn(fn, "2004-12-10 5:30 PM", _
         New clsProcessValue(DataType.datetime, New Date(2004, 12, 10, 17, 30, 0)))

        TestFn(fn, "2004-12-10 5PM", _
         New clsProcessValue(DataType.datetime, New Date(2004, 12, 10, 17, 0, 0)))

        Using New CultureBlock("cs-CZ")
            TestFn(fn, "10.12.2004", _
             New clsProcessValue(DataType.datetime, New Date(2004, 12, 10, 0, 0, 0)))
            TestFn(fn, "10/12/2004", _
             New clsProcessValue(DataType.datetime, New Date(2004, 12, 10, 0, 0, 0)))
            TestFn(fn, "2004-12-10", _
             New clsProcessValue(DataType.datetime, New Date(2004, 12, 10, 0, 0, 0)))
        End Using

        Using New CultureBlock("ja-JP")

            TestFn(fn, "2001/05/21", _
             New clsProcessValue(DataType.datetime, New Date(2001, 5, 21)))

            TestFn(fn, "2001-05-21", _
             New clsProcessValue(DataType.datetime, New Date(2001, 5, 21)))

            TestFnFail(fn, "21/05/2001")

        End Using

        ' datetime items should be equivalent after the fact
        TestFn(fn, New clsProcessValue(DataType.datetime, New Date(1999, 10, 15)), _
         New clsProcessValue(DataType.datetime, New Date(1999, 10, 15)))

        TestFn(fn, _
         New clsProcessValue(DataType.datetime, New Date(1999, 10, 15, 7, 12, 5)), _
         New clsProcessValue(DataType.datetime, New Date(1999, 10, 15, 7, 12, 5)))

        ' Including null datetime values
        TestFn(fn, New clsProcessValue(DataType.datetime), _
         New clsProcessValue(DataType.datetime))

        ' We can convert date items too
        TestFn(fn, New clsProcessValue(DataType.date, New Date(1999, 10, 15)), _
         New clsProcessValue(DataType.datetime, New Date(1999, 10, 15)))

        TestFn(fn, New clsProcessValue(DataType.date, New Date(2050, 7, 12)), _
         New clsProcessValue(DataType.datetime, New Date(2050, 7, 12)))

        ' But not time items, or anything else really
        TestFnFail(fn, _
         New clsProcessValue(DataType.time, New Date(2006, 12, 5, 17, 30, 0)))
        TestFnFail(fn, 5)
        TestFnFail(fn, True)
        TestFnFail(fn, New Byte() {1, 2, 3})
        TestFnFail(fn, TimeSpan.FromDays(1543.5))

        ' Including strings which do not represent dates
        TestFnFail(fn, "")
        TestFnFail(fn, "This is not a date")
        TestFnFail(fn, "Monday")
        TestFnFail(fn, "Tomorrow")


    End Sub

    ''' <summary>
    ''' Tests the ToDate() function
    ''' </summary>
    <Test()> _
    Public Sub TestToDate()
        ' First test the function's basics
        Dim fn As clsFunction = _
         GetFn("ToDate", DataType.date, "Conversion")

        ' === Test ToDate(text) ===

        ' UK Style
        Dim pv As clsProcessValue = "21/05/2001"
        Assert.That(pv.DataType, [Is].EqualTo(DataType.text))

        TestFn(fn, "21/05/2001", _
         New clsProcessValue(DataType.date, New Date(2001, 5, 21)))

        ' Universal Style
        TestFn(fn, "2011-06-12", _
         New clsProcessValue(DataType.date, New Date(2011, 6, 12)))


        ' Japanese Style
        Using New CultureBlock("ja-JP")

            TestFn(fn, "2001/05/21", _
             New clsProcessValue(DataType.date, New Date(2001, 5, 21)))

            TestFnFail(fn, "21/05/2001")

        End Using

        ' Full "u" Style (with time included)
        TestFn(fn, "2011-06-12 12:01:24Z", _
         New clsProcessValue(DataType.date, New Date(2011, 6, 12)))

        ' === Test ToDate(datetime) ===

        ' 2001-07-21 12:13:52 (Local - BST is in effect on this date)
        ' ergo should be 2001-07-21 11:13:52 (UTC)
        Dim localDate As New Date(2001, 7, 21, 12, 13, 52, DateTimeKind.Local)
        Dim utcDate As Date = localDate.ToUniversalTime()

        pv = New clsProcessValue(DataType.datetime, localDate, True)
        Assert.That(CDate(pv), [Is].EqualTo(utcDate))

        TestFn(fn, pv, New clsProcessValue(DataType.date, utcDate.Date))

        ' === Other ToDate calls should error ===

        ' pv = 5 ' ie. DataType.number
        TestFnFail(fn, 5)
        ' DataType.flag
        TestFnFail(fn, True)
        ' An empty string
        TestFnFail(fn, "")
        ' An invalid date within a string
        TestFnFail(fn, "This is not a fish")

    End Sub

    <Test()> _
    Public Sub TestToDays()
        Dim fn As clsFunction = GetFn("ToDays", DataType.number, "Conversion")
        TestFn(fn, New TimeSpan(1, 0, 0, 0), 1)
        TestFn(fn, New TimeSpan(1, 23, 59, 59), 1)
        TestFn(fn, TimeSpan.FromDays(0.9D), 0)
        TestFn(fn, TimeSpan.Zero, 0)
        TestFn(fn, TimeSpan.FromHours(25), 1)
        TestFn(fn, TimeSpan.FromHours(99).Negate(), -4)

        ' If it can be converted into a timespan, it will be
        TestFn(fn, "1.10:04:17", 1)
        TestFn(fn, "-1.10:04:17", -1)
        TestFn(fn, "10:04:17", 0)


        TestFnFail(fn, New Date(2005, 1, 12))
        TestFnFail(fn, New Byte() {1, 2, 3})
        TestFnFail(fn, False)

    End Sub

    <Test()> _
    Public Sub TestToHours()
        Dim fn As clsFunction = GetFn("ToHours", DataType.number, "Conversion")

        TestFn(fn, New TimeSpan(1, 0, 0, 0), 24)
        TestFn(fn, New TimeSpan(1, 23, 59, 59), 47)
        TestFn(fn, TimeSpan.FromDays(0.9D), 21)
        TestFn(fn, TimeSpan.Zero, 0)
        TestFn(fn, TimeSpan.FromHours(25), 25)
        TestFn(fn, TimeSpan.FromHours(99).Negate(), -99)

        ' If it can be converted into a timespan, it will be
        TestFn(fn, "1.10:04:17", 34)
        TestFn(fn, "-1.10:04:17", -34)
        TestFn(fn, "10:04:17", 10)

        TestFnFail(fn, New Date(2005, 1, 12))
        TestFnFail(fn, New Byte() {1, 2, 3})
        TestFnFail(fn, False)

    End Sub

    <Test()> _
    Public Sub TestToMinutes()
        Dim fn As clsFunction = GetFn("ToMinutes", DataType.number, "Conversion")

        TestFn(fn, New TimeSpan(1, 0, 0, 0), 24 * 60)
        TestFn(fn, New TimeSpan(1, 23, 59, 59), 2879)
        TestFn(fn, TimeSpan.FromDays(0.9D), 1296)
        TestFn(fn, TimeSpan.Zero, 0)
        TestFn(fn, TimeSpan.FromHours(25), 25 * 60)
        TestFn(fn, TimeSpan.FromHours(99).Negate(), -99 * 60)

        ' If it can be converted into a timespan, it will be
        TestFn(fn, "1.10:04:17", 2044)
        TestFn(fn, "-1.10:04:17", -2044)
        TestFn(fn, "10:04:17", 604)

        TestFnFail(fn, New Date(2005, 1, 12))
        TestFnFail(fn, New Byte() {1, 2, 3})
        TestFnFail(fn, False)

    End Sub

    <Test()> _
    Public Sub TestToSeconds()
        Dim fn As clsFunction = GetFn("ToSeconds", DataType.number, "Conversion")

        TestFn(fn, New TimeSpan(1, 0, 0, 0), 24 * 60 * 60)
        TestFn(fn, New TimeSpan(1, 23, 59, 59), 172799)
        TestFn(fn, TimeSpan.FromDays(0.9D), 77760)
        TestFn(fn, TimeSpan.Zero, 0)
        TestFn(fn, TimeSpan.FromHours(25), 25 * 60 * 60)
        TestFn(fn, TimeSpan.FromHours(99).Negate(), -99 * 60 * 60)

        ' If it can be converted into a timespan, it will be
        TestFn(fn, "1.10:04:17", 122657)
        TestFn(fn, "-1.10:04:17", -122657)
        TestFn(fn, "10:04:17", 36257)

        TestFnFail(fn, New Date(2005, 1, 12))
        TestFnFail(fn, New Byte() {1, 2, 3})
        TestFnFail(fn, False)

    End Sub

    <Test()> _
    Public Sub TestGetOSVersionMajor()
        Dim fn As clsFunction = GetFn("GetOSVersionMajor", DataType.number, "Environment")

    End Sub

    <Test()> _
    Public Sub TestGetOSVersionMinor()
        Dim fn As clsFunction = GetFn("GetOSVersionMinor", DataType.number, "Environment")

    End Sub

    <Test()> _
    Public Sub TestGetIEVersionMajor()
        Dim fn As clsFunction = GetFn("GetIEVersionMajor", DataType.number, "Environment")

    End Sub

    <Test()> _
    Public Sub TestGetOSVersion()
        Dim fn As clsFunction = GetFn("GetOSVersion", DataType.text, "Environment")

    End Sub

#End Region

End Class

#End If

