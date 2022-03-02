Imports BluePrism.Common.Security

#If UNITTESTS Then

Imports System.Globalization
Imports System.Drawing
Imports System.Drawing.Imaging

Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateProcessCore.clsProcessOperators
Imports NUnit.Framework

''' <summary>
''' A suite of tests for the clsProcessValue operators, as currently defined in the
''' clsProcessOperators class.
''' </summary>
<TestFixture()> _
Public Class OperatorTests

#Region " Utility Methods "

    ''' <summary>
    ''' Shortcut method to call an operator and gather the result
    ''' </summary>
    ''' <param name="op">The operator to call</param>
    ''' <param name="pv1">The left hand value for the operator</param>
    ''' <param name="pv2">The right hand value for the operator</param>
    ''' <returns>The result of the operator on the given values</returns>
    ''' <exception cref="AssertionException">If an error is reported by the operator
    ''' </exception>
    Private Function CallOp(ByVal op As String, _
     ByVal pv1 As clsProcessValue, ByVal pv2 As clsProcessValue) As clsProcessValue
        Dim res As clsProcessValue = Nothing
        Dim errmsg As String = Nothing
        If Not DoOperation(op, pv1, pv2, res, False, errmsg) _
         Then Assert.Fail(errmsg)
        Return res
    End Function

    ''' <summary>
    ''' Tests the given operator, throwing an assert failure if the result of the
    ''' operator on the two operands is different to the expected result
    ''' </summary>
    ''' <param name="op">The operator to call</param>
    ''' <param name="pv1">The left hand operand value</param>
    ''' <param name="pv2">The right hand operand value</param>
    ''' <param name="expected">The expected result</param>
    ''' <exception cref="AssertionException">If the result is something other than
    ''' the <paramref name="expected">expected result</paramref></exception>
    Private Sub TestOp(ByVal op As String, _
     ByVal pv1 As clsProcessValue, ByVal pv2 As clsProcessValue, _
     ByVal expected As clsProcessValue)
        Assert.That(CallOp(op, pv1, pv2), [Is].EqualTo(expected))
    End Sub

    ''' <summary>
    ''' Tests that an operator reports an error when given particular operands
    ''' </summary>
    ''' <param name="op">The operator to call</param>
    ''' <param name="pv1">The left hand operand value</param>
    ''' <param name="pv2">The right hand operand value</param>
    Private Sub TestOpFail(ByVal op As String, _
     ByVal pv1 As clsProcessValue, ByVal pv2 As clsProcessValue)
        Dim errmsg As String = Nothing

        ' It should fail...
        Assert.False(DoOperation(op, pv1, pv2, Nothing, False, errmsg))

        ' And there should be a meaningful message... well, a message at least
        Assert.That(errmsg, [Is].Not.Null.And.Not.Empty)

    End Sub

    ''' <summary>
    ''' Tests the validating of an operation
    ''' </summary>
    ''' <param name="op">The operator to call</param>
    ''' <param name="v1">The left hand operand value</param>
    ''' <param name="v2">The right hand operand value</param>
    ''' <param name="valid">True to expect a response indicating that the operator
    ''' call is valid</param>
    ''' <param name="expectedDataType">The expected data type of the result if the
    ''' operator call is valid. Ignored if not valid.</param>
    Private Sub TestValidate(ByVal op As String, _
     ByVal v1 As clsProcessValue, ByVal v2 As clsProcessValue, _
     ByVal valid As Boolean, ByVal expectedDataType As DataType)

        Dim res As clsProcessValue = Nothing
        Dim errmsg As String = Nothing

        Assert.That(DoOperation(op, v1, v2, res, True, errmsg), [Is].EqualTo(valid))
        ' Don't check the response if the operation is not valid.
        If Not valid Then Return

        ' Datatype should be correct
        Assert.That(res.DataType, [Is].EqualTo(expectedDataType))

        ' The response from a 'justValidate' operator call should be null where
        ' possible
        Assert.That( _
         res.DataType = DataType.text _
         OrElse res.DataType = DataType.password _
         OrElse res.IsNull)

    End Sub

#End Region

#Region " Tests "

    ''' <summary>
    ''' Tests the unary operators - currently just "+" and "-"
    ''' </summary>
    <Test()> _
    Public Sub TestUnaryOperators()

        ' Number
        TestOp("+", Nothing, 0, 0)
        TestOp("+", Nothing, 1, 1)
        TestOp("+", Nothing, -1, -1)
        TestOp("+", Nothing, 3.5D, 3.5D)

        ' Timespan
        TestOp("+", Nothing, TimeSpan.Zero, TimeSpan.Zero)
        TestOp("+", Nothing, New TimeSpan(1, 5, 5), New TimeSpan(1, 5, 5))
        TestOp("+", Nothing, New TimeSpan(2, 1, 5, 5), New TimeSpan(2, 1, 5, 5))
        TestOp("+", Nothing, _
         New TimeSpan(-1, -5, -17, -51), New TimeSpan(-1, -5, -17, -51))

        ' Somewhat less meaningful ones, but currently supported
        ' +text
        TestOp("+", Nothing, "", "")
        TestOp("+", Nothing, "fish", "fish")

        ' +password
        TestOp("+", Nothing, _
         New SafeString("fish"), _
         New SafeString("fish"))

        ' +binary
        TestOp("+", Nothing, New Byte() {1, 2, 3}, New Byte() {1, 2, 3})

        ' +date
        TestOp("+", Nothing, _
         New clsProcessValue(DataType.date, Date.Now), _
         New clsProcessValue(DataType.date, Date.Now))

        ' +datetime
        Dim dt As Date = Date.Now
        TestOp("+", Nothing, _
         New clsProcessValue(DataType.datetime, dt), _
         New clsProcessValue(DataType.datetime, dt))

        ' +time
        TestOp("+", Nothing, _
         New clsProcessValue(DataType.time, dt), _
         New clsProcessValue(DataType.time, dt))

        ' +flag
        TestOp("+", Nothing, True, True)
        TestOp("+", Nothing, False, False)

        ' "-" operator; only valid for numbers and timespans

        TestOp("-", Nothing, 0, 0)
        TestOp("-", Nothing, 1, -1)
        TestOp("-", Nothing, -1, 1)
        TestOp("-", Nothing, 3.5D, -3.5D)
        TestOp("-", Nothing, -2.4D, 2.4D)

        TestOp("-", Nothing, TimeSpan.Zero, TimeSpan.Zero)
        TestOp("-", Nothing, New TimeSpan(1, 5, 5), New TimeSpan(-1, -5, -5))
        TestOp("-", Nothing, _
         New TimeSpan(2, 1, 5, 5), New TimeSpan(2, 1, 5, 5).Negate())
        TestOp("-", Nothing, _
         New TimeSpan(-1, -5, -17, -51), New TimeSpan(1, 5, 17, 51))

    End Sub

    ''' <summary>
    ''' Tests the Add operator ("+")
    ''' </summary>
    <Test()> _
    Public Sub TestAdd()
        ' === Number + Number ===

        ' 0 + 0 = 0
        TestOp("+", 0, 0, 0)
        TestValidate("+", 0, 0, True, DataType.number)

        ' 0 + 1 = 1
        TestOp("+", 0, 1, 1)
        TestOp("+", 1, 0, 1)
        TestValidate("+", 0, 1, True, DataType.number)
        TestValidate("+", 1, 0, True, DataType.number)

        ' 1 + 1 = 2
        TestOp("+", 1, 1, 2)
        TestValidate("+", 1, 1, True, DataType.number)

        ' 1 + -1 = 0
        TestOp("+", 1, -1, 0)
        TestOp("+", -1, 1, 0)
        TestValidate("+", 1, -1, True, DataType.number)
        TestValidate("+", -1, 1, True, DataType.number)

        ' 1.5 + 2.5 = 4
        TestOp("+", 1.5D, 2.5D, 4D)
        TestValidate("+", 1.5D, 2.5D, True, DataType.number)

        ' 1.5 + "2.5" => Error (Can't add a text to a number)
        TestOpFail("+", 1.5D, "2.5")
        TestOpFail("+", "", 1.5D)
        TestValidate("+", 1.5D, "2.5", False, Nothing)
        TestValidate("+", "", 2.5D, False, Nothing)

        ' === TimeSpan + TimeSpan ===

        TestOp("+", _
         New TimeSpan(1, 2, 3), New TimeSpan(4, 5, 6), New TimeSpan(5, 7, 9))
        TestValidate("+", _
         New TimeSpan(1, 2, 3), New TimeSpan(4, 5, 6), True, DataType.timespan)

        ' TimeSpan + Number? Fah!
        TestOpFail("+", New TimeSpan(1, 2, 3), 7)
        TestValidate("+", New TimeSpan(1, 2, 3), 7, False, Nothing)

        ' === Date + TimeSpan ===

        ' 31/07/2014 + 1 day = 01/08/2014
        TestOp("+", _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)), _
         New TimeSpan(1, 0, 0, 0), _
         New clsProcessValue(DataType.date, New Date(2014, 8, 1)))
        TestValidate("+", _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)), _
         New TimeSpan(1, 0, 0, 0), True, DataType.date)

        ' 31/07/2014 + 1d 23h 59m 59s = 01/08/2014
        TestOp("+", _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)), _
         New TimeSpan(1, 23, 59, 59), _
         New clsProcessValue(DataType.date, New Date(2014, 8, 1)))
        TestValidate("+", _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)), _
         New TimeSpan(1, 23, 59, 59), True, DataType.date)

        ' 31/07/2014 + 0d 23h 59m 59s = 01/08/2014
        TestOp("+", _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)), _
         New TimeSpan(23, 59, 59), _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)))
        TestValidate("+", _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)), _
         New TimeSpan(23, 59, 59), True, DataType.date)

        ' Date + Date = error
        TestOpFail("+", _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)), _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)))
        TestValidate("+", _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)), _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)), False, Nothing)

        ' === DateTime + TimeSpan ===

        ' 31/07/2014 00:00:00 + 0d 23h 59m 59s = 31/07/2014 23:59:59
        TestOp("+", _
         New clsProcessValue(DataType.datetime, New Date(2014, 7, 31)), _
         New TimeSpan(23, 59, 59), _
         New clsProcessValue(DataType.datetime, New Date(2014, 7, 31, 23, 59, 59)))
        TestValidate("+", _
         New clsProcessValue(DataType.datetime, New Date(2014, 7, 31)), _
         New TimeSpan(23, 59, 59), True, DataType.datetime)

        ' 31/07/2014 23:59:59 + 0d 0h 0m 1s = 01/08/2014 00:00:00
        TestOp("+", _
         New clsProcessValue(DataType.datetime, New Date(2014, 7, 31, 23, 59, 59)), _
         New TimeSpan(0, 0, 1), _
         New clsProcessValue(DataType.datetime, New Date(2014, 8, 1)))
        TestValidate("+", _
         New clsProcessValue(DataType.datetime, New Date(2014, 7, 31, 23, 59, 59)), _
         New TimeSpan(0, 0, 1), True, DataType.datetime)

        ' 31/07/2014 23:59:59 + 0d 0h 0m 0s = 31/07/2014 23:59:59
        TestOp("+", _
         New clsProcessValue(DataType.datetime, New Date(2014, 7, 31, 23, 59, 59)), _
         TimeSpan.Zero, _
         New clsProcessValue(DataType.datetime, New Date(2014, 7, 31, 23, 59, 59)))
        TestValidate("+", _
         New clsProcessValue(DataType.datetime, New Date(2014, 7, 31, 23, 59, 59)), _
         TimeSpan.Zero, True, DataType.datetime)

        ' === Time + TimeSpan ===

        ' 10:30:00 + 00:50:00 = 11:20:00 (... also, date part is ignored)
        TestOp("+", _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 10, 30, 0)), _
         New TimeSpan(0, 50, 0), _
         New clsProcessValue(DataType.time, New Date(2014, 7, 31, 11, 20, 0)))
        TestValidate("+", _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 10, 30, 0)), _
         New TimeSpan(0, 50, 0), True, DataType.time)

        ' 23:59:59 + 00:00:01 = 00:00:00 (overflow)
        TestOp("+", _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 23, 59, 59)), _
         New TimeSpan(0, 0, 1), _
         New clsProcessValue(DataType.time, New Date()))
        TestValidate("+", _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 23, 59, 59)), _
         New TimeSpan(0, 0, 1), True, DataType.time)

        ' === Binary + Binary ===
        TestOp("+", New Byte() {1, 2, 3}, New Byte() {4, 5, 6}, _
         New Byte() {1, 2, 3, 4, 5, 6})
        TestOp("+", New Byte() {1, 2, 3}, New Byte() {}, _
         New Byte() {1, 2, 3})
        TestValidate("+", _
         New Byte() {1, 2, 3}, _
         New Byte() {4, 5, 6}, True, DataType.binary)
        TestValidate("+", New Byte() {1, 2, 3}, New Byte() {}, True, DataType.binary)

        ' In a bizarre exception to the rule, apparently you can add empty binarys
        TestOp("+", New Byte() {1, 2, 3}, New clsProcessValue(DataType.binary), _
         New Byte() {1, 2, 3})
        TestOp("+", New clsProcessValue(DataType.binary), New Byte() {1, 2, 3}, _
         New Byte() {1, 2, 3})
        TestOp("+", _
         New clsProcessValue(DataType.binary), _
         New clsProcessValue(DataType.binary), _
         New Byte() {})

        ' But an empty value doesn't trigger the validation - in fact the empty
        ' checking is specifically not done when validating... no idea why
        TestValidate("+", _
         New Byte() {1, 2, 3}, _
         New clsProcessValue(DataType.binary), True, DataType.binary)

    End Sub

    ''' <summary>
    ''' Tests the subtraction operator
    ''' </summary>
    <Test()> _
    Public Sub TestSubtract()

        ' === number minus number ===
        TestOp("-", 0D, 0D, 0D)
        TestOp("-", 1D, 0D, 1D)
        TestOp("-", 1D, 1D, 0D)
        TestOp("-", 0D, 1D, -1D)

        ' Mess around with the unit types because why not...
        ' All except integers should be converted to decimals first anyway
        TestOp("-", 0I, 1I, -1I)
        TestOp("-", 1.5D, -2.5D, 4.0F)
        TestOp("-", 1.5D, 2.5D, -1L)
        TestOp("-", -1.5D, -2.5D, 1D)
        TestOp("-", -1.5D, 2.5D, -4)
        TestOp("-", -1.5D, 1.5D, -3D)
        TestOp("-", -1.5D, -1.5D, 0)

        ' Can't subtract a text item
        TestOpFail("-", 1.5, "2.5")

        ' Can't subtract if either side is empty
        TestOpFail("-", New clsProcessValue(DataType.number), 5)
        TestOpFail("-", 5, New clsProcessValue(DataType.number))

        ' === date - timespan ===
        ' 31/07/2014 - 0.00:00:00 = 31/07/2014
        TestOp("-", _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)), _
         TimeSpan.Zero, _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)))

        ' A date goes from midnight so subtracting the smallest amount should
        ' cause the date to go back a day. Our 'smallest amount' is 1 second...
        TestOp("-", _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)), _
         New TimeSpan(TimeSpan.TicksPerSecond), _
         New clsProcessValue(DataType.date, New Date(2014, 7, 30)))

        TestOp("-", _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)), _
         TimeSpan.FromHours(24), _
         New clsProcessValue(DataType.date, New Date(2014, 7, 30)))

        TestOp("-", _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)), _
         TimeSpan.FromHours(-24), _
         New clsProcessValue(DataType.date, New Date(2014, 8, 1)))

        TestOp("-", _
         New clsProcessValue(DataType.date, New Date(2014, 7, 31)), _
         New TimeSpan(31, 0, 0, 0), _
         New clsProcessValue(DataType.date, New Date(2014, 6, 30)))

        ' Can't subtract if either side is empty
        TestOpFail("-", New clsProcessValue(DataType.date), TimeSpan.FromHours(1.5))
        TestOpFail("-", _
         New clsProcessValue(DataType.date, New Date(2013, 2, 27)), _
         New clsProcessValue(DataType.timespan))

        ' === time - timespan ===

        ' 20:15:13 - 0.00:00:00 = 20:15:13
        TestOp("-", _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 20, 15, 13)), _
         New TimeSpan(0), _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 20, 15, 13)))

        ' 20:15:13 - 0.00:15:13 = 20:00:00
        TestOp("-", _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 20, 15, 13)), _
         New TimeSpan(0, 15, 13), _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 20, 0, 0)))

        ' 20:15:13 - 0.20:15:13 = 00:00:00
        TestOp("-", _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 20, 15, 13)), _
         New TimeSpan(20, 15, 13), _
         New clsProcessValue(DataType.time, Date.MinValue))

        ' 20:15:13 - -0.02:00:01 = 22:15:14
        TestOp("-", _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 20, 15, 13)), _
         New TimeSpan(2, 0, 1).Negate(), _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 22, 15, 14)))

        ' 09:59:59 - -0.00:00:01 = 10:00:00
        TestOp("-", _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 9, 59, 59)), _
         TimeSpan.FromSeconds(-1), _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 10, 0, 0)))

        ' 00:00:01 - 0.01:02:34 = 22:57:27 (expected behaviour of overflow)
        TestOp("-", _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 0, 0, 1)), _
         New TimeSpan(1, 2, 34), _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 22, 57, 27)))

        ' 00:00:01 - 17.01:02:34 = 22:57:27 (ensure days are ignored)
        TestOp("-", _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 0, 0, 1)), _
         New TimeSpan(17, 1, 2, 34), _
         New clsProcessValue(DataType.time, New Date(1, 1, 1, 22, 57, 27)))

        ' Can't subtract if either side is empty
        TestOpFail("-", New clsProcessValue(DataType.time), TimeSpan.FromHours(1.5))
        TestOpFail("-", _
         New clsProcessValue(DataType.time, New Date(2013, 2, 27, 18, 7, 41)), _
         New clsProcessValue(DataType.timespan))

        ' === datetime - timespan ===

        ' 12/03/2014 17:31:05 - 0.00:00:00 = 12/03/2014 17:31:05
        TestOp("-", _
         New clsProcessValue(DataType.datetime, New Date(2014, 3, 12, 17, 31, 5)), _
         TimeSpan.Zero, _
         New clsProcessValue(DataType.datetime, New Date(2014, 3, 12, 17, 31, 5)))

        ' 12/03/2014 17:31:05 - 13.05:30:00 = 27/02/2014 12:01:05
        TestOp("-", _
         New clsProcessValue(DataType.datetime, New Date(2014, 3, 12, 17, 31, 5)), _
         New TimeSpan(13, 5, 30, 0), _
         New clsProcessValue(DataType.datetime, New Date(2014, 2, 27, 12, 1, 5)))

        ' 31/12/2014 00:00:00 - -1.00:00:01 = 01/01/2014 00:00:01
        TestOp("-", _
         New clsProcessValue(DataType.datetime, New Date(2014, 12, 31, 0, 0, 0)), _
         New TimeSpan(-1, 0, 0, -1), _
         New clsProcessValue(DataType.datetime, New Date(2015, 1, 1, 0, 0, 1)))

        ' 01/01/2015 00:00:00 - 0.00:00:01 = 31/12/2014 23:59:59
        TestOp("-", _
         New clsProcessValue(DataType.datetime, New Date(2015, 1, 1, 0, 0, 0)), _
         New TimeSpan(0, 0, 0, 1), _
         New clsProcessValue(DataType.datetime, New Date(2014, 12, 31, 23, 59, 59)))

        ' 31/03/2013 09:00:00 - 7:00:00:00 = 24/03/2013 09:00:00 (BST starts 30/03)
        TestOp("-", _
         New clsProcessValue(DataType.datetime, New Date(2014, 3, 31, 9, 0, 0)), _
         New TimeSpan(7, 0, 0, 0), _
         New clsProcessValue(DataType.datetime, New Date(2014, 3, 24, 9, 0, 0)))

        ' Can't subtract if either side is empty
        TestOpFail("-", New clsProcessValue(DataType.datetime), TimeSpan.FromDays(7))
        TestOpFail("-", _
         New clsProcessValue(DataType.datetime, New Date(2013, 2, 27, 18, 7, 41)), _
         New clsProcessValue(DataType.timespan))

        ' === timespan - timespan ===
        ' 2.17:05:42 - 0.00:00:00 = 2.17:05:42
        TestOp("-", _
         New TimeSpan(2, 17, 5, 42), TimeSpan.Zero, _
         New TimeSpan(2, 17, 5, 42))

        ' 2.17:05:42 - 1.12:29:04 = 1.04:36:38
        TestOp("-", _
         New TimeSpan(2, 17, 5, 42), New TimeSpan(1, 12, 29, 4), _
         New TimeSpan(1, 4, 36, 38))

        ' 2.17:05:42 - 7.41:01:57 = -5.23:56:15
        TestOp("-", _
         New TimeSpan(2, 17, 5, 42), New TimeSpan(7, 41, 1, 57), _
         New TimeSpan(5, 23, 56, 15).Negate())

        ' 0.00:00:00 - 7.41:01:57 = -7.41:01:57
        TestOp("-", _
         TimeSpan.Zero, New TimeSpan(7, 41, 1, 57), _
         New TimeSpan(-7, -41, -1, -57))

        ' Can't subtract if either side is empty
        TestOpFail("-", New clsProcessValue(DataType.timespan), TimeSpan.Zero)
        TestOpFail("-", TimeSpan.Zero, New clsProcessValue(DataType.timespan))
        TestOpFail("-", New clsProcessValue(DataType.timespan), _
         New clsProcessValue(DataType.timespan))

        ' === time - time (= timespan) ===
        ' 13:52:17 - 09:00:00 = 0.04:52:17
        TestOp("-", _
         New clsProcessValue(DataType.time, New Date(2014, 3, 6, 13, 52, 17)), _
         New clsProcessValue(DataType.time, New Date(2014, 3, 6, 9, 0, 0)), _
         New TimeSpan(4, 52, 17))

        ' Same but with arbitrary dates in the date passed into the constructor
        TestOp("-", _
         New clsProcessValue(DataType.time, New Date(2014, 3, 6, 13, 52, 17)), _
         New clsProcessValue(DataType.time, New Date(2014, 12, 4, 9, 0, 0)), _
         New TimeSpan(4, 52, 17))

        ' 13:52:17 - 00:00:00 = 0.13:52:17
        TestOp("-", _
         New clsProcessValue(DataType.time, New Date(2014, 3, 6, 13, 52, 17)), _
         New clsProcessValue(DataType.time, New Date(2014, 12, 4, 0, 0, 0)), _
         New TimeSpan(13, 52, 17))

        ' 11:41:02 - 12:30:05 = -0.00:49:03
        TestOp("-", _
         New clsProcessValue(DataType.time, New Date(2014, 3, 6, 11, 41, 2)), _
         New clsProcessValue(DataType.time, New Date(2014, 12, 4, 12, 30, 5)), _
         New TimeSpan(0, -49, -3))

        ' Can't subtract if either side is empty
        TestOpFail("-", _
         New clsProcessValue(DataType.time), _
         New clsProcessValue(DataType.time, Date.MinValue))
        TestOpFail("-", _
         New clsProcessValue(DataType.time, Date.MinValue), _
         New clsProcessValue(DataType.time))

        ' === date - date ===
        ' 06/03/2014 - 01/01/2014 = 64 days
        TestOp("-", _
         New clsProcessValue(DataType.date, New Date(2014, 3, 6, 13, 52, 17)), _
         New clsProcessValue(DataType.date, New Date(2014, 1, 1, 0, 0, 0)), _
         TimeSpan.FromDays(64))

        ' 31/03/2013 - 24/03/2013 = 7:00:00:00 (BST starts 30/03)
        TestOp("-", _
         New clsProcessValue(DataType.date, New Date(2014, 3, 31)), _
         New clsProcessValue(DataType.date, New Date(2014, 3, 24)), _
         New TimeSpan(7, 0, 0, 0))

        ' Can't subtract if either side is empty
        TestOpFail("-", _
         New clsProcessValue(DataType.datetime), _
         New clsProcessValue(DataType.datetime, Date.MinValue))
        TestOpFail("-", _
         New clsProcessValue(DataType.datetime, Date.MinValue), _
         New clsProcessValue(DataType.datetime))

        ' === datetime - datetime ===
        ' 31/03/2013 09:00:00 - 24/03/2013 09:00:00 = 7:00:00:00 (BST starts 30/03)
        TestOp("-", _
         New clsProcessValue(DataType.datetime, New Date(2014, 3, 31, 9, 0, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2014, 3, 24, 9, 0, 0)), _
         New TimeSpan(7, 0, 0, 0))

        ' Can't subtract using differing date/time types
        TestOpFail("-", _
         New clsProcessValue(DataType.time, New Date(2014, 2, 17, 12, 42, 47)), _
         New clsProcessValue(DataType.date, New Date(2014, 5, 1, 4, 51, 39)))
        TestOpFail("-", _
         New clsProcessValue(DataType.time, New Date(2014, 2, 17, 12, 42, 47)), _
         New clsProcessValue(DataType.datetime, New Date(2014, 5, 1, 4, 51, 39)))
        TestOpFail("-", _
         New clsProcessValue(DataType.date, New Date(2014, 2, 17, 12, 42, 47)), _
         New clsProcessValue(DataType.time, New Date(2014, 5, 1, 4, 51, 39)))
        TestOpFail("-", _
         New clsProcessValue(DataType.date, New Date(2014, 2, 17, 12, 42, 47)), _
         New clsProcessValue(DataType.datetime, New Date(2014, 5, 1, 4, 51, 39)))
        TestOpFail("-", _
         New clsProcessValue(DataType.datetime, New Date(2014, 2, 17, 12, 42, 47)), _
         New clsProcessValue(DataType.time, New Date(2014, 5, 1, 4, 51, 39)))
        TestOpFail("-", _
         New clsProcessValue(DataType.datetime, New Date(2014, 2, 17, 12, 42, 47)), _
         New clsProcessValue(DataType.date, New Date(2014, 5, 1, 4, 51, 39)))

    End Sub

    ''' <summary>
    ''' Tests the concatenation operator
    ''' </summary>
    <Test()> _
    Public Sub TestConcat()

        ' text & text
        TestOp("&", "", "", "")
        TestOp("&", "left", "", "left")
        TestOp("&", "", "right", "right")
        TestOp("&", "left", "right", "leftright")
        TestOp("&", " ", " ", "  ")

        ' text & number
        TestOp("&", "left", 1, "left1")
        TestOp("&", 5, "right", "5right")

        Using New CultureBlock(CultureInfo.InvariantCulture)
            TestOp("&", "left", 1.25D, "left1.25")
            TestOp("&", 5.5D, "right", "5.5right")

            ' May be a bit "trying to do too much", this builds up a final result
            ' using the results of 2 other operations (including a '+' call)
            Dim result As clsProcessValue = CallOp("&", 1.5D, " + 2.5 = ")
            Dim result2 As clsProcessValue = CallOp("+", 1.5D, 2.5D)
            Dim result3 As clsProcessValue = CallOp("&", result, result2)
            Assert.That(result3.DataType, [Is].EqualTo(DataType.text))
            Assert.That(CStr(result3), [Is].EqualTo("1.5 + 2.5 = 4.0"))
        End Using

        Using New CultureBlock("en-US")
            TestOp("&", 1.5D, " < 2.5", "1.5 < 2.5")
        End Using

        Using New CultureBlock("fr-FR")
            TestOp("&", "1.5 < ", 2.5D, "1.5 < 2,5")
        End Using

        ' text & flag
        TestOp("&", "Result is: ", True, "Result is: True")
        TestOp("&", "That is ", False, "That is False")

        ' text & password
        TestOp("&", "Don't tell anybody the password - it's ", _
         New SafeString("hunter2"), _
         "Don't tell anybody the password - it's hunter2")

        ' text & time
        ' Note that this currently shows the 'ToShortTimeString()' - ie. it
        ' only displays hours and minutes. It omits any seconds value a time has
        Dim processValueTime = New clsProcessValue(DataType.time, New Date(2014, 1, 1, 17, 46, 12))
        Dim systemFormattedTime = CDate(processValueTime).ToShortTimeString()
        TestOp("&", "At the third stroke, the time sponsored by Accurist will be: ", processValueTime, "At the third stroke, the time sponsored by Accurist will be: " + systemFormattedTime)

        ' text & date
        Dim processValueDateCurrentCulture = New clsProcessValue(DataType.date, New Date(2014, 11, 9, 17, 46, 12))
        TestOp("&", "Date:", processValueDateCurrentCulture, "Date:" + processValueDateCurrentCulture.FormattedValue)

        Using New CultureBlock("en-US")
            Dim processValueDateEnUs = New clsProcessValue(DataType.date, New Date(2014, 11, 9, 17, 46, 12))
            TestOp("&", "Date:", processValueDateEnUs, "Date:" + processValueDateEnUs.FormattedValue)
        End Using

        Using New CultureBlock("ja-JP")
            Dim processValueDateJaJp = New clsProcessValue(DataType.date, New Date(2014, 11, 9, 17, 46, 12))
            TestOp("&", "Date:", processValueDateJaJp, "Date:" + processValueDateJaJp.FormattedValue)
        End Using

        ' text & datetime
        Dim processValueDateTimeLocal = New clsProcessValue(DataType.datetime, New Date(2014, 11, 9, 17, 46, 12))
        TestOp("&", "DateTime:", processValueDateTimeLocal, "DateTime:" + processValueDateTimeLocal.FormattedValue)


        Using New CultureBlock("en-US")
            Dim processValueDateTimeJaJp = New clsProcessValue(DataType.date, New Date(2014, 11, 9, 17, 46, 12))
            TestOp("&", "DateTime:", processValueDateTimeJaJp, "DateTime:" + processValueDateTimeJaJp.FormattedValue)
        End Using

        Using New CultureBlock("ja-JP")
            Dim processValueDateTimeJaJp = New clsProcessValue(DataType.date, New Date(2014, 11, 9, 17, 46, 12))
            TestOp("&", "DateTime:", processValueDateTimeJaJp, "DateTime:" + processValueDateTimeJaJp.FormattedValue)
        End Using

        ' text & <null>
        TestOpFail("&", "Null: ", New clsProcessValue(DataType.number))
        TestOpFail("&", "Null: ", New clsProcessValue(DataType.date))
        TestOpFail("&", "Null: ", New clsProcessValue(DataType.datetime))
        TestOpFail("&", "Null: ", New clsProcessValue(DataType.time))
        TestOpFail("&", "Null: ", New clsProcessValue(DataType.flag))

        ' text & <unsupported>
        Using bmp As New Bitmap(1, 1)
            bmp.SetPixel(0, 0, Color.Red)
            TestOpFail("&", "Image: ", New clsProcessValue(bmp))
        End Using
        TestOpFail("&", "Binary: ", New Byte() {1, 2, 3, 4})
        TestOpFail("&", "Unknown: ", New clsProcessValue())
        TestOpFail("&", "Collection", New clsCollection())

    End Sub

    ''' <summary>
    ''' Tests the multiplication operator.
    ''' Tests:
    ''' - number * number
    ''' - number * timespan
    ''' - timespan * number
    ''' - various fail conditions
    ''' </summary>
    <Test()> _
    Public Sub TestMultiply()

        ' === number * number ===
        TestOp("*", 0, 0, 0)
        TestOp("*", 1, 0, 0)
        TestOp("*", 0, 1, 0)
        TestOp("*", 1, 1, 1)
        TestOp("*", 2, 1, 2)
        TestOp("*", 2, 2, 4)
        TestOp("*", 2, 1.5, 3)
        TestOp("*", 1.5, 1.5, 2.25)
        TestOp("*", -1, 1, -1)
        TestOp("*", 1, -1, -1)
        TestOp("*", -1, -1, 1)
        TestOp("*", 2, -1, -2)
        TestOp("*", 2, 0.5, 1)
        TestOp("*", 2, -0.5, -1)
        TestOp("*", -9, -7, 63)

        ' Can't be empty
        TestOpFail("*", 0, New clsProcessValue(DataType.number))
        TestOpFail("*", New clsProcessValue(DataType.number), 0)
        TestOpFail("*", _
         New clsProcessValue(DataType.number), New clsProcessValue(DataType.number))

        ' === number * timespan (and vice versa) ===
        TestOp("*", 1, TimeSpan.Zero, TimeSpan.Zero)
        TestOp("*", TimeSpan.Zero, 1, TimeSpan.Zero)
        TestOp("*", 1, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
        TestOp("*", 2, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2))
        TestOp("*", 1000, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1000))
        TestOp("*", 1000, TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(-1000))
        TestOp("*", TimeSpan.FromSeconds(1), -500, TimeSpan.FromSeconds(-500))
        TestOp("*", TimeSpan.FromSeconds(-1), 500, TimeSpan.FromSeconds(-500))
        TestOp("*", 24, New TimeSpan(1, 0, 0), New TimeSpan(1, 0, 0, 0))
        TestOp("*", -24, New TimeSpan(1, 0, 0), New TimeSpan(-1, 0, 0, 0))

        ' Can't be empty
        TestOpFail("*", 0, New clsProcessValue(DataType.timespan))
        TestOpFail("*", New clsProcessValue(DataType.timespan), 0)
        TestOpFail("*", _
         New clsProcessValue(DataType.number, "7"), _
         New clsProcessValue(DataType.timespan))

        ' === invalid combinations ===
        TestOpFail("*", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1))
        TestOpFail("*", TimeSpan.FromSeconds(1), True)
        TestOpFail("*", 7, True)
        TestOpFail("*", False, "fish")

    End Sub

    ''' <summary>
    ''' Tests the division operator
    ''' </summary>
    <Test()> _
    Public Sub TestDivide()

        ' === number / number ===
        TestOp("/", 0, 1, 0)
        TestOp("/", 1, 1, 1)
        TestOp("/", 1, 2, 0.5)
        TestOp("/", 1, 10, 0.1)
        TestOp("/", 1, 100, 0.01)
        TestOp("/", 1, 20, 0.05)
        TestOp("/", 1, 8, 0.125)
        TestOp("/", 2, 1, 2)
        TestOp("/", 99, 2, 49.5)

        TestOp("/", -1, 2, -0.5)
        TestOp("/", -2, 2, -1)
        TestOp("/", -5, -1, 5)
        TestOp("/", 5, -1, -5)

        ' Can't divide by zero
        TestOpFail("/", 5, 0)
        TestOpFail("/", 0, 0)
        TestOpFail("/", -1, 0)

        ' Can't be blank
        TestOpFail("/", New clsProcessValue(DataType.number), 5)
        TestOpFail("/", 5, New clsProcessValue(DataType.number))
        TestOpFail("/", _
         New clsProcessValue(DataType.number), New clsProcessValue(DataType.number))

        ' === timespan / number ===
        TestOp("/", TimeSpan.Zero, 1, TimeSpan.Zero)
        TestOp("/", TimeSpan.FromSeconds(30), 30, TimeSpan.FromSeconds(1))
        TestOp("/", TimeSpan.FromDays(1), 24, TimeSpan.FromHours(1))
        TestOp("/", New TimeSpan(1, 10, 30, 0), 2, New TimeSpan(0, 17, 15, 0))
        TestOp("/", New TimeSpan(50, 0, 0, 0), 25, New TimeSpan(2, 0, 0, 0))

        ' Still can't divide by zero
        TestOpFail("/", TimeSpan.Zero, 0)
        TestOpFail("/", TimeSpan.FromSeconds(30), 0)

        ' Still can't be blank
        TestOpFail("/", New clsProcessValue(DataType.timespan), 5)
        TestOpFail("/", _
         TimeSpan.FromSeconds(30), New clsProcessValue(DataType.number))
        TestOpFail("/", _
         New clsProcessValue(DataType.timespan), _
         New clsProcessValue(DataType.number))

        ' Can't divide a timespan by a timespan (though I don't see why not...)
        TestOpFail("/", TimeSpan.FromHours(5), TimeSpan.FromMinutes(30))

    End Sub

    ''' <summary>
    ''' Tests the exponent operator
    ''' </summary>
    <Test()> _
    Public Sub TestExponent()
        TestOp("^", 0, 0, 1) ' this is debatable, but for now it's how it works
        TestOp("^", 1, 0, 1)
        TestOp("^", 0, 1, 0)
        TestOp("^", 1, 1, 1)
        TestOp("^", 2, 0, 1)
        TestOp("^", Integer.MaxValue, 0, 1)
        TestOp("^", Integer.MaxValue, 1, Integer.MaxValue)
        TestOp("^", 1, Integer.MaxValue, 1)
        TestOp("^", 2, 2, 4)
        TestOp("^", 1, -2, 1)       ' 1/(1^2) = 1/1 = 1
        TestOp("^", 2, -2, 0.25)    ' 1/(2^2) = 1/4 = 0.25
        TestOp("^", 4, -2, 0.0625)  ' 1/(4^2) = 1/16 = 0.0625
        TestOp("^", 10, 3, 1000)

        ' No blanks
        TestOpFail("^", New clsProcessValue(DataType.number), 5)
        TestOpFail("^", 5, New clsProcessValue(DataType.number))
        TestOpFail("^", _
         New clsProcessValue(DataType.number), New clsProcessValue(DataType.number))

        ' No other stuff
        TestOpFail("^", 5, "")
        TestOpFail("^", True, 6)
        TestOpFail("^", New clsProcessValue(DataType.image), 6)

        ' (taken from bad expressions test in APCUnitTest)
        ' No -ve'th power of zero
        TestOpFail("^", 0, -1)
        TestOpFail("^", 0, -5)
        ' But +ve'th power is fine
        TestOp("^", 0, 5, 0)
        TestOp("^", 0, 2, 0)

    End Sub

    ''' <summary>
    ''' Tests the equality operator
    ''' </summary>
    <Test()> _
    Public Sub TestEquality()
        ' === number = number ===
        TestOp("=", 0, 0, True)
        TestOp("=", 0, 1, False)
        TestOp("=", 1, 1, True)
        TestOp("=", 1, 0, False)

        ' === flag = flag ===
        TestOp("=", True, True, True)
        TestOp("=", False, True, False)
        TestOp("=", False, False, True)

        ' === text = text ===
        TestOp("=", "7", "7", True)
        TestOp("=", "seven", "seven", True)
        TestOp("=", "start", "startfrom", False)
        TestOp("=", "startfrom", "start", False)
        TestOp("=", "end", "atend", False)
        TestOp("=", "atend", "end", False)

        TestOp("=", "", "", True)
        ' There is no 'null' text type at the moment - only an empty string
        TestOp("=", "", New clsProcessValue(DataType.text), True)
        TestOp("=", New clsProcessValue(DataType.text), "", True)

        ' === password = password ===
        TestOp("=", _
         New SafeString("hunter2"), _
         New SafeString("hunter2"), True)
        TestOp("=", _
         New SafeString("hunter2"), _
         New SafeString("hunter21"), False)
        TestOp("=", _
         New SafeString("hunter21"), _
         New SafeString("hunter2"), False)
        TestOp("=", _
         New SafeString(""), _
         New SafeString(" "), False)
        TestOp("=", _
         New SafeString(" "), _
         New SafeString(""), False)

        ' === datetime = datetime ===

        ' Ignore milliseconds
        TestOp("=", _
         New Date(2004, 1, 16, 16, 25, 9, 234), _
         New Date(2004, 1, 16, 16, 25, 9, 170), True)

        TestOp("=", _
         New clsProcessValue(DataType.datetime, New Date(2001, 9, 24, 0, 0, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2001, 9, 24, 0, 0, 1)), _
         False)

        ' === timespan <> timespan ===
        TestOp("=", TimeSpan.Zero, TimeSpan.Zero, True)
        TestOp("=", TimeSpan.Zero, TimeSpan.FromSeconds(1), False)
        TestOp("=", TimeSpan.FromSeconds(1), TimeSpan.Zero, False)
        TestOp("=", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), True)
        TestOp("=", TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(1), False)

        ' === image = image ===
        Using b1 As New Bitmap(2, 2, PixelFormat.Format24bppRgb)
            b1.SetPixel(0, 0, Color.Red)
            b1.SetPixel(0, 1, Color.Blue)
            b1.SetPixel(1, 0, Color.Green)
            b1.SetPixel(1, 1, Color.White)

            Using b2 As New Bitmap(2, 2, PixelFormat.Format24bppRgb)
                TestOp("=", b1, b2, False)
                b2.SetPixel(0, 0, Color.Red)
                b2.SetPixel(0, 1, Color.Blue)
                b2.SetPixel(1, 0, Color.Green)
                b2.SetPixel(1, 1, Color.White)
                TestOp("=", b1, b2, True)
            End Using

            Using b2 As New Bitmap(2, 2, PixelFormat.Format32bppRgb)
                TestOp("=", b1, b2, False)
                b2.SetPixel(0, 0, Color.Red)
                b2.SetPixel(0, 1, Color.Blue)
                b2.SetPixel(1, 0, Color.Green)
                b2.SetPixel(1, 1, Color.White)
                TestOp("=", b1, b2, True)
            End Using

            Using b2 As New Bitmap(2, 2, PixelFormat.Format32bppArgb)
                TestOp("=", b1, b2, False)
                b2.SetPixel(0, 0, Color.Red)
                b2.SetPixel(0, 1, Color.Blue)
                b2.SetPixel(1, 0, Color.Green)
                b2.SetPixel(1, 1, Color.White)
                TestOp("=", b1, b2, True)
                b2.SetPixel(1, 1, Color.FromArgb(&HFFFFFFFE))
                TestOp("=", b1, b2, False)
            End Using

        End Using

        ' === binary = binary ===
        If True Then
            Dim arr1() As Byte = {1, 2, 3, 4, 5}
            Dim arr2() As Byte = {3, 4, 5, 6, 7}
            Dim arr3() As Byte = {1, 2, 3, 4, 5, 6, 7}
            TestOp("=", arr1, arr2, False)
            TestOp("=", arr1, arr1, True)
            TestOp("=", arr2, arr1, False)
            TestOp("=", arr1, arr3, False)

            Dim arr1_clone() As Byte = DirectCast(arr1.Clone(), Byte())
            TestOp("=", arr1, arr1_clone, True)

            ' Binary procvals work differently to normal procvals - ie. they
            ' keep a reference to the byte array that represents their value.
            ' As such, they are mutable in a way which other procvals aren't
            ' (collections aside)
            Dim pv2 As clsProcessValue = arr2
            Dim pv1_clone As clsProcessValue = arr1_clone
            TestOp("=", pv2, pv1_clone, False)

            For i As Integer = 0 To arr2.Length - 1
                arr1_clone(i) = arr2(i)
            Next
            TestOp("=", pv2, pv1_clone, True)

        End If


        ' === mix and don't match ===
        TestOp("=", 1, True, False)
        TestOp("=", "7", 7, False)
        TestOp("=", _
         "hunter2", New SafeString("hunter2"), False)

    End Sub

    ''' <summary>
    ''' Tests the non-equality operator. Basically just a reverse test of the
    ''' equality operator (actually just cloned and the expected results toggled
    ''' as at 16/06/2014
    ''' </summary>
    <Test()> _
    Public Sub TestNonEquality()

        ' === number <> number ===
        TestOp("<>", 0, 0, False)
        TestOp("<>", 0, 1, True)
        TestOp("<>", 1, 1, False)
        TestOp("<>", 1, 0, True)

        ' === flag <> flag ===
        TestOp("<>", True, True, False)
        TestOp("<>", False, True, True)
        TestOp("<>", False, False, False)

        ' === text <> text ===
        TestOp("<>", "7", "7", False)
        TestOp("<>", "seven", "seven", False)
        TestOp("<>", "start", "startfrom", True)
        TestOp("<>", "startfrom", "start", True)
        TestOp("<>", "end", "atend", True)
        TestOp("<>", "atend", "end", True)

        TestOp("<>", "", "", False)
        ' There is no 'null' text type at the moment - only an empty string
        TestOp("<>", "", New clsProcessValue(DataType.text), False)
        TestOp("<>", New clsProcessValue(DataType.text), "", False)

        ' === password <> password ===
        TestOp("<>", _
         New SafeString("hunter2"), _
         New SafeString("hunter2"), False)
        TestOp("<>", _
         New SafeString("hunter2"), _
         New SafeString("hunter21"), True)
        TestOp("<>", _
         New SafeString("hunter21"), _
         New SafeString("hunter2"), True)
        TestOp("<>", _
         New SafeString(""), _
         New SafeString(" "), True)
        TestOp("<>", _
         New SafeString(" "), _
         New SafeString(""), True)

        ' === datetime = datetime ===

        ' Ignore milliseconds
        TestOp("<>", _
         New Date(2004, 1, 16, 16, 25, 9, 234), _
         New Date(2004, 1, 16, 16, 25, 9, 170), False)

        TestOp("<>", _
         New clsProcessValue(DataType.datetime, New Date(2001, 9, 24, 0, 0, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2001, 9, 24, 0, 0, 1)), _
         True)

        ' === timespan <> timespan ===
        TestOp("<>", TimeSpan.Zero, TimeSpan.Zero, False)
        TestOp("<>", TimeSpan.Zero, TimeSpan.FromSeconds(1), True)
        TestOp("<>", TimeSpan.FromSeconds(1), TimeSpan.Zero, True)
        TestOp("<>", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), False)
        TestOp("<>", TimeSpan.FromSeconds(-1), TimeSpan.FromSeconds(1), True)

        ' === image <> image ===
        Using b1 As New Bitmap(2, 2, PixelFormat.Format24bppRgb)
            b1.SetPixel(0, 0, Color.Red)
            b1.SetPixel(0, 1, Color.Blue)
            b1.SetPixel(1, 0, Color.Green)
            b1.SetPixel(1, 1, Color.White)

            Using b2 As New Bitmap(2, 2, PixelFormat.Format24bppRgb)
                TestOp("<>", b1, b2, True)
                b2.SetPixel(0, 0, Color.Red)
                b2.SetPixel(0, 1, Color.Blue)
                b2.SetPixel(1, 0, Color.Green)
                b2.SetPixel(1, 1, Color.White)
                TestOp("<>", b1, b2, False)
            End Using

            Using b2 As New Bitmap(2, 2, PixelFormat.Format32bppRgb)
                TestOp("<>", b1, b2, True)
                b2.SetPixel(0, 0, Color.Red)
                b2.SetPixel(0, 1, Color.Blue)
                b2.SetPixel(1, 0, Color.Green)
                b2.SetPixel(1, 1, Color.White)
                TestOp("<>", b1, b2, False)
            End Using

            Using b2 As New Bitmap(2, 2, PixelFormat.Format32bppArgb)
                TestOp("<>", b1, b2, True)
                b2.SetPixel(0, 0, Color.Red)
                b2.SetPixel(0, 1, Color.Blue)
                b2.SetPixel(1, 0, Color.Green)
                b2.SetPixel(1, 1, Color.White)
                TestOp("<>", b1, b2, False)
                b2.SetPixel(1, 1, Color.FromArgb(&HFFFFFFFE))
                TestOp("<>", b1, b2, True)
            End Using

        End Using

        ' === binary <> binary ===
        If True Then
            Dim arr1() As Byte = {1, 2, 3, 4, 5}
            Dim arr2() As Byte = {3, 4, 5, 6, 7}
            Dim arr3() As Byte = {1, 2, 3, 4, 5, 6, 7}
            TestOp("<>", arr1, arr2, True)
            TestOp("<>", arr1, arr1, False)
            TestOp("<>", arr2, arr1, True)
            TestOp("<>", arr1, arr3, True)

            Dim arr1_clone() As Byte = DirectCast(arr1.Clone(), Byte())
            TestOp("<>", arr1, arr1_clone, False)

            ' Binary procvals work differently to normal procvals - ie. they
            ' keep a reference to the byte array that represents their value.
            ' As such, they are mutable in a way which other procvals aren't
            ' (collections aside)
            Dim pv2 As clsProcessValue = arr2
            Dim pv1_clone As clsProcessValue = arr1_clone
            TestOp("<>", pv2, pv1_clone, True)

            For i As Integer = 0 To arr2.Length - 1
                arr1_clone(i) = arr2(i)
            Next
            TestOp("<>", pv2, pv1_clone, False)

        End If

        ' === mix and don't match ===
        TestOp("<>", 1, True, True)
        TestOp("<>", "7", 7, True)
        TestOp("<>", _
         "hunter2", New SafeString("hunter2"), True)

    End Sub

    ''' <summary>
    ''' Tests the greater than operator
    ''' </summary>
    <Test()> _
    Public Sub TestGreaterThan()

        ' === number > number ===
        TestOp(">", 0, 0, False)
        TestOp(">", 1, 0, True)
        TestOp(">", 1, 1, False)
        TestOp(">", 0, 1, False)
        TestOp(">", 0, -1, True)
        TestOp(">", -1, -2, True)
        TestOp(">", -1, -1, False)
        TestOp(">", -1, 1, False)
        TestOp(">", 1.000001, 1, True)
        TestOp(">", 1, 1.000001, False)
        TestOp(">", 1, 0.999999, True)
        TestOp(">", -1, -0.999999, False)
        TestOp(">", _
         Decimal.Parse("1,00", CultureInfo.GetCultureInfo("FR-fr")), _
         Decimal.Parse("1.0", CultureInfo.InvariantCulture), False)

        ' No nulls please, we're british
        TestOpFail(">", 1, New clsProcessValue(DataType.number))
        TestOpFail(">", New clsProcessValue(DataType.number), 1)
        TestOpFail(">", _
         New clsProcessValue(DataType.number), New clsProcessValue(DataType.number))

        ' === date > date ===
        TestOp(">", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 20)), True)

        TestOp(">", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), False)

        TestOp(">", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 22)), False)

        TestOp(">", _
         New clsProcessValue(DataType.date, New Date(2000, 12, 31)), _
         New clsProcessValue(DataType.date, New Date(2001, 1, 1)), False)

        TestOp(">", _
         New clsProcessValue(DataType.date, New Date(2001, 1, 1)), _
         New clsProcessValue(DataType.date, New Date(2000, 12, 31)), True)

        ' Date should ignore time
        TestOp(">", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 16, 30, 0)), False)

        TestOp(">", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 23, 59, 59)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 0, 0, 0)), False)

        ' Null don't like it (rockin the kasbah)
        TestOpFail(">", _
         New clsProcessValue(DataType.date, Date.Now), _
         New clsProcessValue(DataType.date))
        TestOpFail(">", _
         New clsProcessValue(DataType.date), _
         New clsProcessValue(DataType.date, Date.Now))
        TestOpFail(">", _
         New clsProcessValue(DataType.date), New clsProcessValue(DataType.date))

        ' === time > time ===
        TestOp(">", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 1)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), True)

        TestOp(">", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), False)

        TestOp(">", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 31, 0)), False)

        ' times should ignore date
        TestOp(">", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 22, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), False)

        TestOp(">", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 20, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2001, 12, 9, 16, 30, 0)), True)

        TestOp(">", _
         New clsProcessValue(DataType.time, New Date(2001, 1, 1, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 12, 31, 17, 30, 0)), False)

        ' Also, should ignore millis
        TestOp(">", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0, 100)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0, 17)), _
         False)

        ' We don't like nulls round ere
        TestOpFail(">", _
         New clsProcessValue(DataType.time, Date.Now), _
         New clsProcessValue(DataType.time))
        TestOpFail(">", _
         New clsProcessValue(DataType.time), _
         New clsProcessValue(DataType.time, Date.Now))
        TestOpFail(">", _
         New clsProcessValue(DataType.time), New clsProcessValue(DataType.time))

        ' === datetime > datetime ===
        TestOp(">", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 1)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         True)

        TestOp(">", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         False)

        TestOp(">", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 31, 0)), _
         False)

        TestOp(">", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 22, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         True)

        TestOp(">", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 20, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2001, 12, 9, 16, 30, 0)), _
         False)

        TestOp(">", _
         New clsProcessValue(DataType.datetime, New Date(2001, 1, 1, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 12, 31, 17, 30, 0)), _
         True)

        TestOp(">", _
         New clsProcessValue(DataType.datetime, New Date(1999, 12, 31, 23, 59, 59)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 1, 1, 0, 0, 0)), _
         False)

        TestOp(">", _
         New clsProcessValue(DataType.datetime, New Date(2000, 1, 1, 0, 0, 0)), _
         New clsProcessValue(DataType.datetime, New Date(1999, 12, 31, 23, 59, 59)), _
         True)

        ' No millis support in datetimes either
        TestOp(">", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0, 100)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0, 17)), _
         False)

        ' Neither which no nulls innit
        TestOpFail(">", _
         New clsProcessValue(DataType.datetime, Date.Now), _
         New clsProcessValue(DataType.datetime))
        TestOpFail(">", _
         New clsProcessValue(DataType.datetime), _
         New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail(">", _
         New clsProcessValue(DataType.datetime), _
         New clsProcessValue(DataType.datetime))

        ' === timespan > timespan ===
        TestOp(">", TimeSpan.Zero, TimeSpan.Zero, False)
        TestOp(">", TimeSpan.Zero, TimeSpan.FromSeconds(-1), True)
        TestOp(">", TimeSpan.FromSeconds(-1), TimeSpan.Zero, False)
        TestOp(">", TimeSpan.FromSeconds(1), TimeSpan.Zero, True)
        TestOp(">", TimeSpan.Zero, TimeSpan.FromSeconds(1), False)
        TestOp(">", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), False)

        ' Ignore millis
        TestOp(">", TimeSpan.Zero, TimeSpan.FromMilliseconds(-1), False)
        TestOp(">", TimeSpan.FromMilliseconds(1), TimeSpan.Zero, False)

        ' Have nothing to do with nulls
        TestOpFail(">", _
         TimeSpan.FromSeconds(30), _
         New clsProcessValue(DataType.timespan))
        TestOpFail(">", _
         New clsProcessValue(DataType.timespan), _
         TimeSpan.FromSeconds(30))
        TestOpFail(">", _
         New clsProcessValue(DataType.timespan), _
         New clsProcessValue(DataType.timespan))

        ' === Things that don't work ===

        ' date/time types must match
        TestOpFail(">", _
         New clsProcessValue(DataType.date, Date.Now), _
         New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail(">", _
         New clsProcessValue(DataType.date, Date.Now), _
         New clsProcessValue(DataType.time, Date.Now))
        TestOpFail(">", _
         New clsProcessValue(DataType.datetime, Date.Now), _
         New clsProcessValue(DataType.time, Date.Now))
        TestOpFail(">", _
         New clsProcessValue(DataType.datetime, Date.Now), _
         New clsProcessValue(DataType.date, Date.Now))
        TestOpFail(">", _
         New clsProcessValue(DataType.time, Date.Now), _
         New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail(">", _
         New clsProcessValue(DataType.time, Date.Now), _
         New clsProcessValue(DataType.date, Date.Now))

        ' numbers and others don't mix
        TestOpFail(">", 7, New clsProcessValue(DataType.date, Date.Now))
        TestOpFail(">", 7, New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail(">", 7, New clsProcessValue(DataType.time, Date.Now))
        TestOpFail(">", 7, TimeSpan.Zero)
        TestOpFail(">", 7, True)
        TestOpFail(">", 7, "7")
        TestOpFail(">", New clsProcessValue(DataType.date, Date.Now), 7)
        TestOpFail(">", New clsProcessValue(DataType.datetime, Date.Now), 7)
        TestOpFail(">", New clsProcessValue(DataType.time, Date.Now), 7)
        TestOpFail(">", TimeSpan.Zero, 7)
        TestOpFail(">", True, 7)
        TestOpFail(">", "7", 7)

        ' text doesn't scan
        TestOpFail(">", "Seven", "Eight")
        TestOpFail(">", "", "Eight")
        TestOpFail(">", "", "")

        ' nor flags
        TestOpFail(">", False, True)
        TestOpFail(">", False, False)
        TestOpFail(">", True, True)
        TestOpFail(">", True, False)

        ' binary wontary
        TestOpFail(">", New Byte() {1, 2, 3}, New Byte() {1, 2, 3})
        TestOpFail(">", New Byte() {1, 2, 3}, New clsProcessValue(DataType.binary))

        ' nor not no images
        Using b1 As New Bitmap(2, 2, PixelFormat.Format24bppRgb)
            b1.SetPixel(0, 0, Color.Red)
            b1.SetPixel(0, 1, Color.Blue)
            b1.SetPixel(1, 0, Color.Green)
            b1.SetPixel(1, 1, Color.White)

            TestOpFail(">", b1, New clsProcessValue(DataType.image))
            TestOpFail(">", b1, "")
            Using b2 As New Bitmap(2, 2, PixelFormat.Format32bppArgb)
                b2.SetPixel(0, 0, Color.Red)
                b2.SetPixel(0, 1, Color.Blue)
                b2.SetPixel(1, 0, Color.Green)
                b2.SetPixel(1, 1, Color.White)
                TestOpFail(">", b1, b2)
            End Using
        End Using

    End Sub

    ''' <summary>
    ''' Tests the "less than" operator. This is largely cribbed from the "greater
    ''' than" tests with some modifications and altered expected results where
    ''' appropriate.
    ''' </summary>
    <Test()> _
    Public Sub TestLessThan()

        ' === number < number ===
        TestOp("<", 0, 0, False)
        TestOp("<", 1, 0, False)
        TestOp("<", 1, 1, False)
        TestOp("<", 0, 1, True)
        TestOp("<", -1, 0, True)
        TestOp("<", 0, -1, False)
        TestOp("<", -2, -1, True)
        TestOp("<", -2, -2, False)
        TestOp("<", -2, -3, False)
        TestOp("<", 0.9, 1, True)
        TestOp("<", -0.9, -1, False)
        TestOp("<", Integer.MinValue, Integer.MaxValue, True)
        TestOp("<", Integer.MaxValue, Integer.MinValue, False)

        ' No nulls please, we're british
        TestOpFail("<", 1, New clsProcessValue(DataType.number))
        TestOpFail("<", New clsProcessValue(DataType.number), 1)
        TestOpFail("<", _
         New clsProcessValue(DataType.number), New clsProcessValue(DataType.number))

        ' === date < date ===
        TestOp("<", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 20)), False)

        TestOp("<", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), False)

        TestOp("<", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 22)), True)

        TestOp("<", _
         New clsProcessValue(DataType.date, New Date(2000, 12, 31)), _
         New clsProcessValue(DataType.date, New Date(2001, 1, 1)), True)

        TestOp("<", _
         New clsProcessValue(DataType.date, New Date(2001, 1, 1)), _
         New clsProcessValue(DataType.date, New Date(2000, 12, 31)), False)

        ' Date should ignore time
        TestOp("<", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 16, 30, 0)), False)

        TestOp("<", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 16, 30, 0)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 17, 30, 0)), False)

        TestOp("<", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 23, 59, 59)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 0, 0, 0)), False)

        ' Null don't like it (rockin the kasbah)
        TestOpFail("<", _
         New clsProcessValue(DataType.date, Date.Now), _
         New clsProcessValue(DataType.date))
        TestOpFail("<", _
         New clsProcessValue(DataType.date), _
         New clsProcessValue(DataType.date, Date.Now))
        TestOpFail("<", _
         New clsProcessValue(DataType.date), New clsProcessValue(DataType.date))

        ' === time < time ===
        TestOp("<", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 1)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), False)

        TestOp("<", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), False)

        TestOp("<", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 1)), True)

        TestOp("<", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 31, 0)), True)

        ' times should ignore date
        TestOp("<", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 22, 17, 30, 0)), False)

        TestOp("<", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 20, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2001, 12, 9, 16, 30, 0)), False)

        TestOp("<", _
         New clsProcessValue(DataType.time, New Date(2001, 5, 20, 16, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 12, 9, 17, 30, 0)), True)

        TestOp("<", _
         New clsProcessValue(DataType.time, New Date(2001, 1, 1, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 12, 31, 17, 30, 0)), False)

        ' Also, should ignore millis
        TestOp("<", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0, 5)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0, 100)), _
         False)

        ' We don't like nulls round ere
        TestOpFail("<", _
         New clsProcessValue(DataType.time, Date.Now), _
         New clsProcessValue(DataType.time))
        TestOpFail("<", _
         New clsProcessValue(DataType.time), _
         New clsProcessValue(DataType.time, Date.Now))
        TestOpFail("<", _
         New clsProcessValue(DataType.time), New clsProcessValue(DataType.time))

        ' === datetime < datetime ===
        TestOp("<", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 1)), _
         True)

        TestOp("<", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         False)

        TestOp("<", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 31, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         False)

        TestOp("<", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 22, 17, 30, 0)), _
         True)

        TestOp("<", _
         New clsProcessValue(DataType.datetime, New Date(2001, 12, 9, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 20, 16, 30, 0)), _
         False)

        TestOp("<", _
         New clsProcessValue(DataType.datetime, New Date(2001, 1, 1, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 12, 31, 17, 30, 0)), _
         False)

        TestOp("<", _
         New clsProcessValue(DataType.datetime, New Date(1999, 12, 31, 23, 59, 59)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 1, 1, 0, 0, 0)), _
         True)

        TestOp("<", _
         New clsProcessValue(DataType.datetime, New Date(2000, 1, 1, 0, 0, 0)), _
         New clsProcessValue(DataType.datetime, New Date(1999, 12, 31, 23, 59, 59)), _
         False)

        ' No millis support in datetimes either
        TestOp("<", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0, 9)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0, 106)), _
         False)

        ' Neither which no nulls innit
        TestOpFail("<", _
         New clsProcessValue(DataType.datetime, Date.Now), _
         New clsProcessValue(DataType.datetime))
        TestOpFail("<", _
         New clsProcessValue(DataType.datetime), _
         New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail("<", _
         New clsProcessValue(DataType.datetime), _
         New clsProcessValue(DataType.datetime))

        ' === timespan < timespan ===
        TestOp("<", TimeSpan.Zero, TimeSpan.Zero, False)
        TestOp("<", TimeSpan.Zero, TimeSpan.FromSeconds(-1), False)
        TestOp("<", TimeSpan.FromSeconds(-1), TimeSpan.Zero, True)
        TestOp("<", TimeSpan.FromSeconds(1), TimeSpan.Zero, False)
        TestOp("<", TimeSpan.Zero, TimeSpan.FromSeconds(1), True)
        TestOp("<", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), False)

        ' Ignore millis
        TestOp("<", TimeSpan.Zero, TimeSpan.FromMilliseconds(-1), False)
        TestOp("<", TimeSpan.FromMilliseconds(1), TimeSpan.Zero, False)

        ' Have nothing to do with nulls
        TestOpFail("<", _
         TimeSpan.FromSeconds(30), _
         New clsProcessValue(DataType.timespan))
        TestOpFail("<", _
         New clsProcessValue(DataType.timespan), _
         TimeSpan.FromSeconds(30))
        TestOpFail("<", _
         New clsProcessValue(DataType.timespan), _
         New clsProcessValue(DataType.timespan))

        ' === Things that don't work ===

        ' date/time types must match
        TestOpFail("<", _
         New clsProcessValue(DataType.date, Date.Now), _
         New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail("<", _
         New clsProcessValue(DataType.date, Date.Now), _
         New clsProcessValue(DataType.time, Date.Now))
        TestOpFail("<", _
         New clsProcessValue(DataType.datetime, Date.Now), _
         New clsProcessValue(DataType.time, Date.Now))
        TestOpFail("<", _
         New clsProcessValue(DataType.datetime, Date.Now), _
         New clsProcessValue(DataType.date, Date.Now))
        TestOpFail("<", _
         New clsProcessValue(DataType.time, Date.Now), _
         New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail("<", _
         New clsProcessValue(DataType.time, Date.Now), _
         New clsProcessValue(DataType.date, Date.Now))

        ' numbers and others don't mix
        TestOpFail("<", 7, New clsProcessValue(DataType.date, Date.Now))
        TestOpFail("<", 7, New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail("<", 7, New clsProcessValue(DataType.time, Date.Now))
        TestOpFail("<", 7, TimeSpan.Zero)
        TestOpFail("<", 7, True)
        TestOpFail("<", 7, "7")
        TestOpFail("<", New clsProcessValue(DataType.date, Date.Now), 7)
        TestOpFail("<", New clsProcessValue(DataType.datetime, Date.Now), 7)
        TestOpFail("<", New clsProcessValue(DataType.time, Date.Now), 7)
        TestOpFail("<", TimeSpan.Zero, 7)
        TestOpFail("<", True, 7)
        TestOpFail("<", "7", 7)

        ' text doesn't scan
        TestOpFail("<", "Seven", "Eight")
        TestOpFail("<", "", "Eight")
        TestOpFail("<", "", "")

        ' nor flags
        TestOpFail("<", False, True)
        TestOpFail("<", False, False)
        TestOpFail("<", True, True)
        TestOpFail("<", True, False)

        ' binary wontary
        TestOpFail("<", New Byte() {1, 2, 3}, New Byte() {1, 2, 3})
        TestOpFail("<", New Byte() {1, 2, 3}, New clsProcessValue(DataType.binary))

        ' nor not no images
        Using b1 As New Bitmap(2, 2, PixelFormat.Format24bppRgb)
            b1.SetPixel(0, 0, Color.Red)
            b1.SetPixel(0, 1, Color.Blue)
            b1.SetPixel(1, 0, Color.Green)
            b1.SetPixel(1, 1, Color.White)

            TestOpFail("<", b1, New clsProcessValue(DataType.image))
            TestOpFail("<", b1, "")
            Using b2 As New Bitmap(2, 2, PixelFormat.Format32bppArgb)
                b2.SetPixel(0, 0, Color.Red)
                b2.SetPixel(0, 1, Color.Blue)
                b2.SetPixel(1, 0, Color.Green)
                b2.SetPixel(1, 1, Color.White)
                TestOpFail("<", b1, b2)
            End Using
        End Using

    End Sub

    ''' <summary>
    ''' Tests the "greater than or equal" operator. Again, largely cribbed and
    ''' altered from previous test methods.
    ''' </summary>
    <Test()> _
    Public Sub TestGreaterOrEqual()
        TestOp(">=", 0, 0, True)
        TestOp(">=", 0, 1, False)
        TestOp(">=", 1, 0, True)
        TestOp(">=", 1, 1, True)
        TestOp(">=", -1, 0, False)
        TestOp(">=", -1, -1, True)
        TestOp(">=", 0, -1, True)
        TestOp(">=", -2, -1, False)
        TestOp(">=", -1, -2, True)
        TestOp(">=", 0.1, 0, True)
        TestOp(">=", -0.1, 0, False)
        TestOp(">=", 0.9999, 1, False)
        TestOp(">=", 1, 1.00001, False)

        ' No nulls please, we're british
        TestOpFail(">=", 1, New clsProcessValue(DataType.number))
        TestOpFail(">=", New clsProcessValue(DataType.number), 1)
        TestOpFail(">=", _
         New clsProcessValue(DataType.number), New clsProcessValue(DataType.number))

        ' === date >= date ===
        TestOp(">=", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 20)), True)

        TestOp(">=", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), True)

        TestOp(">=", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 22)), False)

        TestOp(">=", _
         New clsProcessValue(DataType.date, New Date(2000, 12, 31)), _
         New clsProcessValue(DataType.date, New Date(2001, 1, 1)), False)

        TestOp(">=", _
         New clsProcessValue(DataType.date, New Date(2001, 1, 1)), _
         New clsProcessValue(DataType.date, New Date(2000, 12, 31)), True)

        ' Date should ignore time
        TestOp(">=", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 16, 30, 0)), True)

        TestOp(">=", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 23, 59, 59)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 0, 0, 0)), True)

        TestOp(">=", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 0, 0, 0)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 23, 59, 59)), True)

        ' Null don't like it (rockin the kasbah)
        TestOpFail(">=", _
         New clsProcessValue(DataType.date, Date.Now), _
         New clsProcessValue(DataType.date))
        TestOpFail(">=", _
         New clsProcessValue(DataType.date), _
         New clsProcessValue(DataType.date, Date.Now))
        TestOpFail(">=", _
         New clsProcessValue(DataType.date), New clsProcessValue(DataType.date))

        ' === time >= time ===
        TestOp(">=", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 1)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), True)

        TestOp(">=", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), True)

        TestOp(">=", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 31, 0)), False)

        ' times should ignore date
        TestOp(">=", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 22, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), True)

        TestOp(">=", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 20, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2001, 12, 9, 16, 30, 0)), True)

        TestOp(">=", _
         New clsProcessValue(DataType.time, New Date(2001, 1, 1, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 12, 31, 17, 31, 0)), False)

        ' Also, should ignore millis
        TestOp(">=", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0, 9)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0, 12)), _
         True)

        ' We don't like nulls round ere
        TestOpFail(">=", _
         New clsProcessValue(DataType.time, Date.Now), _
         New clsProcessValue(DataType.time))
        TestOpFail(">=", _
         New clsProcessValue(DataType.time), _
         New clsProcessValue(DataType.time, Date.Now))
        TestOpFail(">=", _
         New clsProcessValue(DataType.time), New clsProcessValue(DataType.time))

        ' === datetime >= datetime ===
        TestOp(">=", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 1)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         True)

        TestOp(">=", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         True)

        TestOp(">=", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 31, 0)), _
         False)

        TestOp(">=", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 22, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         True)

        TestOp(">=", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 20, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2001, 12, 9, 16, 30, 0)), _
         False)

        TestOp(">=", _
         New clsProcessValue(DataType.datetime, New Date(2001, 1, 1, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 12, 31, 17, 30, 0)), _
         True)

        TestOp(">=", _
         New clsProcessValue(DataType.datetime, New Date(1999, 12, 31, 23, 59, 59)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 1, 1, 0, 0, 0)), _
         False)

        TestOp(">=", _
         New clsProcessValue(DataType.datetime, New Date(2000, 1, 1, 0, 0, 0)), _
         New clsProcessValue(DataType.datetime, New Date(1999, 12, 31, 23, 59, 59)), _
         True)

        ' No millis support in datetimes either
        TestOp(">=", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0, 105)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0, 219)), _
         True)

        ' Neither which no nulls innit
        TestOpFail(">=", _
         New clsProcessValue(DataType.datetime, Date.Now), _
         New clsProcessValue(DataType.datetime))
        TestOpFail(">=", _
         New clsProcessValue(DataType.datetime), _
         New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail(">=", _
         New clsProcessValue(DataType.datetime), _
         New clsProcessValue(DataType.datetime))

        ' === timespan >= timespan ===
        TestOp(">=", TimeSpan.Zero, TimeSpan.Zero, True)
        TestOp(">=", TimeSpan.Zero, TimeSpan.FromSeconds(-1), True)
        TestOp(">=", TimeSpan.FromSeconds(-1), TimeSpan.Zero, False)
        TestOp(">=", TimeSpan.FromSeconds(1), TimeSpan.Zero, True)
        TestOp(">=", TimeSpan.Zero, TimeSpan.FromSeconds(1), False)
        TestOp(">=", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), True)

        ' Ignore millis
        TestOp(">=", TimeSpan.Zero, TimeSpan.FromMilliseconds(-1), True)
        TestOp(">=", TimeSpan.FromMilliseconds(1), TimeSpan.Zero, True)
        TestOp(">=", _
         New TimeSpan(0, 2, 4, 10, 500), New TimeSpan(0, 2, 4, 10, 950), True)

        ' Have nothing to do with nulls
        TestOpFail(">=", _
         TimeSpan.FromSeconds(30), _
         New clsProcessValue(DataType.timespan))
        TestOpFail(">=", _
         New clsProcessValue(DataType.timespan), _
         TimeSpan.FromSeconds(30))
        TestOpFail(">=", _
         New clsProcessValue(DataType.timespan), _
         New clsProcessValue(DataType.timespan))

        ' === Things that don't work ===

        ' date/time types must match
        TestOpFail(">=", _
         New clsProcessValue(DataType.date, Date.Now), _
         New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail(">=", _
         New clsProcessValue(DataType.date, Date.Now), _
         New clsProcessValue(DataType.time, Date.Now))
        TestOpFail(">=", _
         New clsProcessValue(DataType.datetime, Date.Now), _
         New clsProcessValue(DataType.time, Date.Now))
        TestOpFail(">=", _
         New clsProcessValue(DataType.datetime, Date.Now), _
         New clsProcessValue(DataType.date, Date.Now))
        TestOpFail(">=", _
         New clsProcessValue(DataType.time, Date.Now), _
         New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail(">=", _
         New clsProcessValue(DataType.time, Date.Now), _
         New clsProcessValue(DataType.date, Date.Now))

        ' numbers and others don't mix
        TestOpFail(">=", 7, New clsProcessValue(DataType.date, Date.Now))
        TestOpFail(">=", 7, New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail(">=", 7, New clsProcessValue(DataType.time, Date.Now))
        TestOpFail(">=", 7, TimeSpan.Zero)
        TestOpFail(">=", 7, True)
        TestOpFail(">=", 7, "7")
        TestOpFail(">=", New clsProcessValue(DataType.date, Date.Now), 7)
        TestOpFail(">=", New clsProcessValue(DataType.datetime, Date.Now), 7)
        TestOpFail(">=", New clsProcessValue(DataType.time, Date.Now), 7)
        TestOpFail(">=", TimeSpan.Zero, 7)
        TestOpFail(">=", True, 7)
        TestOpFail(">=", "7", 7)

        ' text doesn't scan
        TestOpFail(">=", "Seven", "Eight")
        TestOpFail(">=", "", "Eight")
        TestOpFail(">=", "", "")

        ' nor flags
        TestOpFail(">=", False, True)
        TestOpFail(">=", False, False)
        TestOpFail(">=", True, True)
        TestOpFail(">=", True, False)

        ' binary wontary
        TestOpFail(">=", New Byte() {1, 2, 3}, New Byte() {1, 2, 3})
        TestOpFail(">=", New Byte() {1, 2, 3}, New clsProcessValue(DataType.binary))

        ' nor not no images
        Using b1 As New Bitmap(2, 2, PixelFormat.Format24bppRgb)
            b1.SetPixel(0, 0, Color.Red)
            b1.SetPixel(0, 1, Color.Blue)
            b1.SetPixel(1, 0, Color.Green)
            b1.SetPixel(1, 1, Color.White)

            TestOpFail(">=", b1, New clsProcessValue(DataType.image))
            TestOpFail(">=", b1, "")
            Using b2 As New Bitmap(2, 2, PixelFormat.Format32bppArgb)
                b2.SetPixel(0, 0, Color.Red)
                b2.SetPixel(0, 1, Color.Blue)
                b2.SetPixel(1, 0, Color.Green)
                b2.SetPixel(1, 1, Color.White)
                TestOpFail(">=", b1, b2)
            End Using
        End Using

    End Sub

    ''' <summary>
    ''' Tests the "less than or equal" operator. Again, largely cribbed and altered
    ''' from previous test methods.
    ''' </summary>
    <Test()> _
    Public Sub TestLessOrEqual()
        TestOp("<=", 0, 0, True)
        TestOp("<=", 0, 1, True)
        TestOp("<=", 1, 0, False)
        TestOp("<=", 1, 1, True)
        TestOp("<=", -1, 0, True)
        TestOp("<=", -1, -1, True)
        TestOp("<=", 0, -1, False)
        TestOp("<=", -2, -1, True)
        TestOp("<=", -1, -2, False)
        TestOp("<=", 0.1, 0, False)
        TestOp("<=", -0.1, 0, True)
        TestOp("<=", 0.9999, 1, True)
        TestOp("<=", 1, 1.00001, True)

        ' No nulls please, we're british
        TestOpFail("<=", 1, New clsProcessValue(DataType.number))
        TestOpFail("<=", New clsProcessValue(DataType.number), 1)
        TestOpFail("<=", _
         New clsProcessValue(DataType.number), New clsProcessValue(DataType.number))

        ' === date <= date ===
        TestOp("<=", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 20)), False)

        TestOp("<=", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), True)

        TestOp("<=", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 22)), True)

        TestOp("<=", _
         New clsProcessValue(DataType.date, New Date(2000, 12, 31)), _
         New clsProcessValue(DataType.date, New Date(2001, 1, 1)), True)

        TestOp("<=", _
         New clsProcessValue(DataType.date, New Date(2001, 1, 1)), _
         New clsProcessValue(DataType.date, New Date(2000, 12, 31)), False)

        ' Date should ignore time
        TestOp("<=", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 16, 30, 0)), True)

        TestOp("<=", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 23, 59, 59)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 0, 0, 0)), True)

        TestOp("<=", _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 0, 0, 0)), _
         New clsProcessValue(DataType.date, New Date(2000, 5, 21, 23, 59, 59)), True)

        ' Null don't like it (rockin the kasbah)
        TestOpFail("<=", _
         New clsProcessValue(DataType.date, Date.Now), _
         New clsProcessValue(DataType.date))
        TestOpFail("<=", _
         New clsProcessValue(DataType.date), _
         New clsProcessValue(DataType.date, Date.Now))
        TestOpFail("<=", _
         New clsProcessValue(DataType.date), New clsProcessValue(DataType.date))

        ' === time <= time ===
        TestOp("<=", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 1)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), False)

        TestOp("<=", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), True)

        TestOp("<=", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 31, 0)), True)

        ' times should ignore date
        TestOp("<=", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 22, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0)), True)

        TestOp("<=", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 20, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2001, 12, 9, 16, 30, 0)), False)

        TestOp("<=", _
         New clsProcessValue(DataType.time, New Date(2001, 1, 1, 17, 30, 0)), _
         New clsProcessValue(DataType.time, New Date(2000, 12, 31, 17, 31, 0)), True)

        ' Also, should ignore millis
        TestOp("<=", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0, 9)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0, 12)), _
         True)

        TestOp("<=", _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0, 101)), _
         New clsProcessValue(DataType.time, New Date(2000, 5, 21, 17, 30, 0, 88)), _
         True)

        ' We don't like nulls round ere
        TestOpFail("<=", _
         New clsProcessValue(DataType.time, Date.Now), _
         New clsProcessValue(DataType.time))
        TestOpFail("<=", _
         New clsProcessValue(DataType.time), _
         New clsProcessValue(DataType.time, Date.Now))
        TestOpFail("<=", _
         New clsProcessValue(DataType.time), New clsProcessValue(DataType.time))

        ' === datetime <= datetime ===
        TestOp("<=", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 1)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         False)

        TestOp("<=", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         True)

        TestOp("<=", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 31, 0)), _
         True)

        TestOp("<=", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 22, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0)), _
         False)

        TestOp("<=", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 20, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2001, 12, 9, 16, 30, 0)), _
         True)

        TestOp("<=", _
         New clsProcessValue(DataType.datetime, New Date(2001, 1, 1, 17, 30, 0)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 12, 31, 17, 30, 0)), _
         False)

        TestOp("<=", _
         New clsProcessValue(DataType.datetime, New Date(1999, 12, 31, 23, 59, 59)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 1, 1, 0, 0, 0)), _
         True)

        TestOp("<=", _
         New clsProcessValue(DataType.datetime, New Date(2000, 1, 1, 0, 0, 0)), _
         New clsProcessValue(DataType.datetime, New Date(1999, 12, 31, 23, 59, 59)), _
         False)

        ' No millis support in datetimes either
        TestOp("<=", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0, 105)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0, 219)), _
         True)

        TestOp("<=", _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0, 999)), _
         New clsProcessValue(DataType.datetime, New Date(2000, 5, 21, 17, 30, 0, 1)), _
         True)

        ' Neither which no nulls innit
        TestOpFail("<=", _
         New clsProcessValue(DataType.datetime, Date.Now), _
         New clsProcessValue(DataType.datetime))
        TestOpFail("<=", _
         New clsProcessValue(DataType.datetime), _
         New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail("<=", _
         New clsProcessValue(DataType.datetime), _
         New clsProcessValue(DataType.datetime))

        ' === timespan <= timespan ===
        TestOp("<=", TimeSpan.Zero, TimeSpan.Zero, True)
        TestOp("<=", TimeSpan.Zero, TimeSpan.FromSeconds(-1), False)
        TestOp("<=", TimeSpan.FromSeconds(-1), TimeSpan.Zero, True)
        TestOp("<=", TimeSpan.FromSeconds(1), TimeSpan.Zero, False)
        TestOp("<=", TimeSpan.Zero, TimeSpan.FromSeconds(1), True)
        TestOp("<=", TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), True)

        ' Ignore millis
        TestOp("<=", TimeSpan.Zero, TimeSpan.FromMilliseconds(-1), True)
        TestOp("<=", TimeSpan.FromMilliseconds(1), TimeSpan.Zero, True)
        TestOp("<=", _
         New TimeSpan(0, 2, 4, 10, 500), New TimeSpan(0, 2, 4, 10, 950), True)
        TestOp("<=", _
         New TimeSpan(0, 2, 4, 10, 950), New TimeSpan(0, 2, 4, 10, 500), True)

        ' Have nothing to do with nulls
        TestOpFail("<=", _
         TimeSpan.FromSeconds(30), _
         New clsProcessValue(DataType.timespan))
        TestOpFail("<=", _
         New clsProcessValue(DataType.timespan), _
         TimeSpan.FromSeconds(30))
        TestOpFail("<=", _
         New clsProcessValue(DataType.timespan), _
         New clsProcessValue(DataType.timespan))

        ' === Things that don't work ===

        ' date/time types must match
        TestOpFail("<=", _
         New clsProcessValue(DataType.date, Date.Now), _
         New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail("<=", _
         New clsProcessValue(DataType.date, Date.Now), _
         New clsProcessValue(DataType.time, Date.Now))
        TestOpFail("<=", _
         New clsProcessValue(DataType.datetime, Date.Now), _
         New clsProcessValue(DataType.time, Date.Now))
        TestOpFail("<=", _
         New clsProcessValue(DataType.datetime, Date.Now), _
         New clsProcessValue(DataType.date, Date.Now))
        TestOpFail("<=", _
         New clsProcessValue(DataType.time, Date.Now), _
         New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail("<=", _
         New clsProcessValue(DataType.time, Date.Now), _
         New clsProcessValue(DataType.date, Date.Now))

        ' numbers and others don't mix
        TestOpFail("<=", 7, New clsProcessValue(DataType.date, Date.Now))
        TestOpFail("<=", 7, New clsProcessValue(DataType.datetime, Date.Now))
        TestOpFail("<=", 7, New clsProcessValue(DataType.time, Date.Now))
        TestOpFail("<=", 7, TimeSpan.Zero)
        TestOpFail("<=", 7, True)
        TestOpFail("<=", 7, "7")
        TestOpFail("<=", New clsProcessValue(DataType.date, Date.Now), 7)
        TestOpFail("<=", New clsProcessValue(DataType.datetime, Date.Now), 7)
        TestOpFail("<=", New clsProcessValue(DataType.time, Date.Now), 7)
        TestOpFail("<=", TimeSpan.Zero, 7)
        TestOpFail("<=", True, 7)
        TestOpFail("<=", "7", 7)

        ' text doesn't scan
        TestOpFail("<=", "Seven", "Eight")
        TestOpFail("<=", "", "Eight")
        TestOpFail("<=", "", "")

        ' nor flags
        TestOpFail("<=", False, True)
        TestOpFail("<=", False, False)
        TestOpFail("<=", True, True)
        TestOpFail("<=", True, False)

        ' binary wontary
        TestOpFail("<=", New Byte() {1, 2, 3}, New Byte() {1, 2, 3})
        TestOpFail("<=", New Byte() {1, 2, 3}, New clsProcessValue(DataType.binary))

        ' nor not no images
        Using b1 As New Bitmap(2, 2, PixelFormat.Format24bppRgb)
            b1.SetPixel(0, 0, Color.Red)
            b1.SetPixel(0, 1, Color.Blue)
            b1.SetPixel(1, 0, Color.Green)
            b1.SetPixel(1, 1, Color.White)

            TestOpFail("<=", b1, New clsProcessValue(DataType.image))
            TestOpFail("<=", b1, "")
            Using b2 As New Bitmap(2, 2, PixelFormat.Format32bppArgb)
                b2.SetPixel(0, 0, Color.Red)
                b2.SetPixel(0, 1, Color.Blue)
                b2.SetPixel(1, 0, Color.Green)
                b2.SetPixel(1, 1, Color.White)
                TestOpFail("<=", b1, b2)
            End Using
        End Using

    End Sub

    ''' <summary>
    ''' Tests the AND operator
    ''' </summary>
    <Test()> _
    Public Sub TestAnd()
        TestOp("and", True, True, True)
        TestOp("and", True, False, False)
        TestOp("and", False, True, False)
        TestOp("and", False, False, False)

        TestOp("AND", True, True, True)
        TestOp("AND", True, False, False)
        TestOp("AND", False, True, False)
        TestOp("AND", False, False, False)

        ' Null procvals cannot be AND'ed
        TestOpFail("and", True, New clsProcessValue(DataType.flag))
        TestOpFail("and", False, New clsProcessValue(DataType.flag))
        TestOpFail("AND", True, New clsProcessValue(DataType.flag))
        TestOpFail("AND", False, New clsProcessValue(DataType.flag))

        ' Numbers? Strings? It's a world gone topsy turvy
        TestOpFail("and", True, 1)
        TestOpFail("and", 1, 1)
        TestOpFail("and", 1, True)
        TestOpFail("and", "True", "True")
        TestOpFail("and", True, "False")
        TestOpFail("and", "True", 1)

        ' Currently we do not support mixed case (again... not sure why not)
        TestOpFail("And", True, True)
        TestOpFail("AnD", True, True)
        TestOpFail("aNd", True, True)
        TestOpFail("ANd", True, True)

    End Sub

    ''' <summary>
    ''' Tests the OR operator
    ''' </summary>
    <Test()> _
    Public Sub TestOr()
        TestOp("or", True, True, True)
        TestOp("or", True, False, True)
        TestOp("or", False, True, True)
        TestOp("or", False, False, False)

        TestOp("OR", True, True, True)
        TestOp("OR", True, False, True)
        TestOp("OR", False, True, True)
        TestOp("OR", False, False, False)

        ' Null procvals cannot be OR'ed
        TestOpFail("or", True, New clsProcessValue(DataType.flag))
        TestOpFail("or", False, New clsProcessValue(DataType.flag))
        TestOpFail("OR", True, New clsProcessValue(DataType.flag))
        TestOpFail("OR", False, New clsProcessValue(DataType.flag))

        ' Numbers? Strings? It's a world gone topsy turvy
        TestOpFail("or", True, 1)
        TestOpFail("or", 1, 1)
        TestOpFail("or", 1, True)
        TestOpFail("or", "True", "True")
        TestOpFail("or", True, "False")
        TestOpFail("or", "True", 1)

        ' Currently we do not support mixed case (again... not sure why not)
        TestOpFail("Or", True, True)
        TestOpFail("oR", True, True)
        TestOpFail("Or", False, True)

    End Sub
#End Region

End Class

#End If