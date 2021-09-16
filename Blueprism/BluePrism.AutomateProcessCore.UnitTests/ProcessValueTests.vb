Imports BluePrism.Core

#If UNITTESTS Then

Imports System.Linq
Imports System.Xml

Imports BluePrism.BPCoreLib
Imports BluePrism.Common.Security
Imports NUnit.Framework

<TestFixture()>
Public Class ProcessValueTests

    ''' <summary>
    ''' Probably not the best place for it - I just wanted to make sure I hadn't
    ''' broken the way it was formatting in the clsWSDLProcess.AutomateValueToXSD()
    ''' method by using XmlConvert rather than operating directly on the
    ''' EncodedeValue
    ''' </summary>
    <Test()>
    Public Sub TestXmlConversion()
        Dim pv As clsProcessValue = 2.5D
        Assert.That(XmlConvert.ToString(CDec(pv)), [Is].EqualTo("2.5"))
        pv = -2.5D
        Assert.That(XmlConvert.ToString(CDec(pv)), [Is].EqualTo("-2.5"))

        pv = True
        Assert.That(XmlConvert.ToString(CBool(pv)), [Is].EqualTo("true"))
        pv = False
        Assert.That(XmlConvert.ToString(CBool(pv)), [Is].EqualTo("false"))

    End Sub

    ''' <summary>
    ''' Various tests which test the current handling of null values in the process
    ''' value class.
    ''' </summary>
    <Test()>
    Public Sub TestNull()
        Dim pv As clsProcessValue

        ' === Binary ===
        pv = New clsProcessValue(DataType.binary, "")
        Assert.True(pv.IsNull)
        Assert.That(CType(pv, Byte()), [Is].Null)

        pv = CType(Nothing, Byte())
        Assert.True(pv.IsNull)
        Assert.That(CType(pv, Byte()), [Is].Null)

        pv = New Byte() {}
        Assert.False(pv.IsNull)
        Assert.That(CType(pv, Byte()), [Is].Not.Null)
        Assert.That(CType(pv, Byte()), [Is].EqualTo(New Byte() {}))

        pv = New clsProcessValue(New Byte() {})
        Assert.False(pv.IsNull)
        Assert.That(CType(pv, Byte()), [Is].Not.Null)
        Assert.That(CType(pv, Byte()), [Is].EqualTo(New Byte() {}))

        ' === Number ===
        pv = New clsProcessValue(DataType.number, "")
        Assert.True(pv.IsNull)
        Assert.That(CInt(pv), [Is].EqualTo(0))

        pv = New clsProcessValue(DataType.number, "0")
        Assert.False(pv.IsNull)
        Assert.That(CInt(pv), [Is].EqualTo(0))

        pv = 0
        Assert.False(pv.IsNull)
        Assert.That(CInt(pv), [Is].EqualTo(0))

        ' === Flag ===
        pv = New clsProcessValue(DataType.flag, "")
        Assert.True(pv.IsNull)
        Assert.That(CBool(pv), [Is].False)

        pv = New clsProcessValue(DataType.flag, "False")
        Assert.False(pv.IsNull)
        Assert.That(CBool(pv), [Is].False)

        pv = False
        Assert.False(pv.IsNull)
        Assert.That(CBool(pv), [Is].False)

        ' === Date ===
        pv = New clsProcessValue(DataType.date, "")
        Assert.True(pv.IsNull)
        Assert.That(pv.GetDateValue(), [Is].EqualTo(Date.MinValue))

        pv = New clsProcessValue(DataType.date, Date.MinValue)
        Assert.True(pv.IsNull)
        Assert.That(pv.GetDateValue(), [Is].EqualTo(Date.MinValue))

        ' === Time ===
        pv = New clsProcessValue(DataType.time, "")
        Assert.True(pv.IsNull)
        Assert.That(CDate(pv).TimeOfDay, [Is].EqualTo(TimeSpan.Zero))

        ' Odd one this - unlike date and datetime, Date.MinValue just means
        ' "midnight" to a time ProcessValue, which can't be treated as null
        pv = New clsProcessValue(DataType.time, Date.MinValue)
        Assert.False(pv.IsNull)
        Assert.That(CDate(pv).TimeOfDay, [Is].EqualTo(TimeSpan.Zero))

        pv = New clsProcessValue(DataType.time, "00:00:00")
        Assert.False(pv.IsNull)
        Assert.That(CDate(pv).TimeOfDay, [Is].EqualTo(TimeSpan.Zero))

        ' === DateTime ===
        pv = New clsProcessValue(DataType.datetime, "")
        Assert.True(pv.IsNull)
        Assert.That(pv.GetValueAsUTCDateTime(), [Is].EqualTo(Date.MinValue))
        Assert.That(pv.GetValueAsLocalDateTime(), [Is].EqualTo(Date.MinValue))

        pv = New clsProcessValue(DataType.datetime, Date.MinValue)
        Assert.True(pv.IsNull)
        Assert.That(pv.GetValueAsUTCDateTime(), [Is].EqualTo(Date.MinValue))
        Assert.That(pv.GetValueAsLocalDateTime(), [Is].EqualTo(Date.MinValue))

        pv = Date.MinValue
        Assert.True(pv.IsNull)
        Assert.That(pv.GetValueAsUTCDateTime(), [Is].EqualTo(Date.MinValue))
        Assert.That(pv.GetValueAsLocalDateTime(), [Is].EqualTo(Date.MinValue))
        Assert.That(CDate(pv), [Is].EqualTo(Date.MinValue))

        ' === Timespan ===
        pv = New clsProcessValue(DataType.timespan, "")
        Assert.True(pv.IsNull)
        Assert.That(CType(pv, TimeSpan), [Is].EqualTo(TimeSpan.Zero))

        pv = New clsProcessValue(TimeSpan.Zero)
        Assert.False(pv.IsNull)
        Assert.That(CType(pv, TimeSpan), [Is].EqualTo(TimeSpan.Zero))

        pv = TimeSpan.Zero
        Assert.False(pv.IsNull)
        Assert.That(CType(pv, TimeSpan), [Is].EqualTo(TimeSpan.Zero))

    End Sub

    ''' <summary>
    ''' Tests the casting from a process value into various .NET data types.
    ''' </summary>
    <Test()>
    Public Sub TestCasting()

        ' With a GUID value (ie. a string)
        Dim id As Guid = Guid.NewGuid()
        Dim pv As clsProcessValue = New clsProcessValue(id)
        Assert.That(CType(pv, Guid), [Is].EqualTo(id))
        Assert.That(CStr(pv), [Is].EqualTo(id.ToString()))

        Try
            Dim int As Integer = CInt(pv)
            Assert.Fail("Successfully converted a GUID process value into an int")
        Catch ice As InvalidCastException
        End Try

        Try
            Dim dec As Decimal = CDec(pv)
            Assert.Fail(
             "Successfully converted a GUID process value into an decimal")
        Catch ice As InvalidCastException
        End Try

        Try
            Dim flag As Boolean = CBool(pv)
            Assert.Fail(
             "Successfully converted a GUID process value into a boolean")
        Catch ice As InvalidCastException
        End Try

        ' With a FLAG value
        pv = New clsProcessValue(False)
        Assert.That(pv.DataType, [Is].EqualTo(DataType.flag))
        Assert.That(pv.EncodedValue, [Is].EqualTo("False"))
        Assert.That(CBool(pv), [Is].False)
        Assert.That(CStr(pv), [Is].EqualTo("False"))

        Try
            Dim int As Integer = CInt(pv)
            Assert.Fail("Successfully converted a flag process value into an int")
        Catch ice As InvalidCastException
        End Try

        Try
            Dim dec As Decimal = CDec(pv)
            Assert.Fail(
             "Successfully converted a flag process value into an decimal")
        Catch ice As InvalidCastException
        End Try

        pv = New clsProcessValue(DataType.flag, "True")
        Assert.That(pv.DataType, [Is].EqualTo(DataType.flag))
        Assert.That(pv.EncodedValue, [Is].EqualTo("True"))
        Assert.That(CBool(pv), [Is].True)
        Assert.That(CStr(pv), [Is].EqualTo("True"))

        Try
            Dim int As Integer = CInt(pv)
            Assert.Fail("Successfully converted a flag process value into an int")
        Catch ice As InvalidCastException
        End Try

        Try
            Dim dec As Decimal = CDec(pv)
            Assert.Fail(
             "Successfully converted a flag process value into an decimal")
        Catch ice As InvalidCastException
        End Try

        ' Get the date now, and discard any time component below a second - that's
        ' as far as process value goes, so that's all we need
        Dim dtNow As Date = Date.UtcNow
        dtNow = dtNow.AddTicks(-dtNow.Ticks Mod TimeSpan.TicksPerSecond)

        Dim dtHeldUtc As Date = dtNow
        Dim dtHeldLocal As Date = dtHeldUtc.ToLocalTime()

        pv = dtNow
        Assert.That(pv.DataType, [Is].EqualTo(DataType.datetime))
        Assert.That(pv.EncodedValue,
         [Is].EqualTo(dtNow.ToString(clsProcessValue.InternalDateTimeFormat)))
        Dim dt As Date = CDate(pv)
        Assert.That(dt, [Is].EqualTo(dtHeldUtc),
         "Dates not equal - found {0}, kind: {1}; Expected {2}, kind: {3}",
         dt, dt.Kind, dtHeldUtc, dtHeldUtc.Kind)

        Assert.That(pv.GetValueAsLocalDateTime(), [Is].EqualTo(dtHeldLocal))
    End Sub

    ''' <summary>
    ''' Tests the formatting of date/datetime/time process values
    ''' </summary>
    <Test()>
    Public Sub TestFormatDate()

        Dim dtNow As Date = Date.Now

        Dim val As New clsProcessValue(DataType.date, dtNow)
        Dim dt As Date = CDate(val)
        Assert.That(dt, [Is].EqualTo(dtNow.Date))
        Assert.That(
         val.FormatDate("'MakeDate('d','M','yyyy')'"),
         [Is].EqualTo(String.Format("MakeDate({0},{1},{2})", dt.Day, dt.Month, dt.Year)))
        Assert.That(val.FormatDate("HH:mm:ss"), [Is].EqualTo("00:00:00"))

        val = New clsProcessValue(DataType.datetime, dtNow)
        Assert.That(val.FormatDate("u"), [Is].EqualTo(dtNow.ToString("u")))
        Assert.That(val.FormatDate("HH:mm:ss"), [Is].EqualTo(dtNow.ToString("HH:mm:ss")))

        val = New clsProcessValue(DataType.time, dtNow)
        Assert.That(val.FormatDate("dd/MM/yyyy"), [Is].EqualTo("01/01/0001"))
        Assert.That(val.FormatDate("HH:mm:ss"), [Is].EqualTo(dtNow.ToString("HH:mm:ss")))

    End Sub

    ''' <summary>
    ''' Tests the input and output from numeric ProcessValues in various cultures.
    ''' </summary>
    <Test()>
    Public Sub TestCultureNumbers()

        ' With a numeric value
        Dim pv As New clsProcessValue(1.5)
        Assert.That(pv.DataType, [Is].EqualTo(DataType.number))
        Assert.That(CDec(pv), [Is].EqualTo(1.5D))

        Assert.That(pv.EncodedValue, [Is].EqualTo("1.5"))
        Assert.That(pv.FormattedValue, [Is].EqualTo("1.5"))

        Using New CultureBlock("cs-CZ")

            Assert.That(pv.EncodedValue, [Is].EqualTo("1.5"))
            Assert.That(pv.FormattedValue, [Is].EqualTo("1,5"))

            pv = New clsProcessValue(DataType.number, "1.5")

            Assert.That(pv.DataType, [Is].EqualTo(DataType.number))
            Assert.That(CDec(pv), [Is].EqualTo(1.5D))
            Assert.That(pv.EncodedValue, [Is].EqualTo("1.5"))
            Assert.That(pv.FormattedValue, [Is].EqualTo("1,5"))

        End Using

    End Sub

    ''' <summary>
    ''' Test the basic password handling for process value tests
    ''' </summary>
    <Test, TestCaseSource(GetType(TestUtil), "PasswordTests")>
    Sub TestPasswords(input As String)
        Dim ss As New SafeString(input)

        Dim pv As New clsProcessValue(ss)
        Assert.That(CStr(pv), Iz.EqualTo(input))
        Assert.That(pv.DataType, Iz.EqualTo(DataType.password))

        pv = ss
        Assert.That(CStr(pv), Iz.EqualTo(input))

    End Sub

    ''' <summary>
    ''' Tests that the cloning of a password process value works correctly.
    ''' </summary>
    <Test, TestCaseSource(GetType(TestUtil), "PasswordTests")>
    Public Sub TestPasswordClone(input As String)
        Dim ss As New SafeString(input)
        Dim pv As New clsProcessValue(ss)

        Dim clone = pv.Clone()
        Assert.That(CStr(clone), Iz.EqualTo(input))
        Assert.That(clone.DataType, Iz.EqualTo(DataType.password))
        Assert.That(clone, Iz.EqualTo(pv))
        Assert.That(clone.EncodedValue, Iz.EqualTo(pv.EncodedValue))

    End Sub

    ''' <summary>
    ''' Tests that the older mechanism for creating password values works
    ''' transparently with values from the new version
    ''' </summary>
    <Test, TestCaseSource(GetType(TestUtil), "PasswordTests")>
    Public Sub TestPasswordOldSchool(input As String)
        Dim ss As New SafeString(input)

        Dim pv As New clsProcessValue(ss)

        ' We want to check that the old way works, but there's a Debug assertion in
        ' there that we want to keep, so disable the UI handling of the assertion
        ' failure before we test the old-school method.
        Dim list() As TraceListener = Debug.Listeners.Cast(Of TraceListener).ToArray()
        Debug.Listeners.Clear()
        Try

            Dim oldStyle As New clsProcessValue(DataType.password, input)
            Assert.That(oldStyle.DataType, Iz.EqualTo(DataType.password))
            Assert.That(CStr(oldStyle), Iz.EqualTo(input))
            Assert.That(oldStyle.EncodedValue, Iz.EqualTo(pv.EncodedValue))
            Assert.That(oldStyle, Iz.EqualTo(pv))
        Finally
            Debug.Listeners.AddRange(list)
        End Try

    End Sub

    ''' <summary>
    ''' Tests the XML in and XML out of the process value to ensure that the value is
    ''' correctly retained after a round trip
    ''' </summary>
    <Test, TestCaseSource(GetType(TestUtil), "PasswordTests")>
    Public Sub TestPasswordXmlHandling(input As String)
        Dim ss As New SafeString(input)

        Dim pv As New clsProcessValue(ss)
        Dim doc As New XmlDocument()
        Dim xml = pv.ToXML(doc)

        Dim pvPost = clsProcessValue.FromXML(xml)

        Assert.That(pvPost, Iz.EqualTo(pv))
        Assert.That(pvPost.DataType, Iz.EqualTo(DataType.password))
        Assert.That(CStr(pvPost), Iz.EqualTo(input))
        Assert.That(pvPost.EncodedValue, Iz.EqualTo(pv.EncodedValue))
        Assert.That(pvPost, Iz.EqualTo(pv))

    End Sub

    ''' <summary>
    ''' Tests the casting of password
    ''' </summary>
    <Test, TestCaseSource(GetType(TestUtil), "PasswordTests")>
    Sub TestPasswordCasting(input As String)
        Dim ss As New SafeString(input)

        Dim pv As New clsProcessValue(ss)

        Assert.That(pv.CanCastInto(DataType.text), Iz.True)

        Dim pvText = pv.CastInto(DataType.text)
        Assert.That(pvText.DataType, Iz.EqualTo(DataType.text))
        Assert.That(CStr(pvText), Iz.EqualTo(CStr(pv)))
        Assert.That(CStr(pvText), Iz.EqualTo(input))
        Assert.That(pvText.EncodedValue, Iz.EqualTo(input))

        Assert.That(pvText.CanCastInto(DataType.password), Iz.True)

        Dim pvPassword = pvText.CastInto(DataType.password)
        Assert.That(pvPassword.DataType, Iz.EqualTo(DataType.password))
        Assert.That(CStr(pvPassword), Iz.EqualTo(CStr(pvText)))
        Assert.That(CStr(pvPassword), Iz.EqualTo(input))
        Assert.That(pvPassword.EncodedValue, Iz.Not.EqualTo(input))

        Dim ssCast As SafeString = CType(pv, SafeString)
        Dim pvCast = CType(ssCast, clsProcessValue)
        Assert.That(pvCast.DataType, Iz.EqualTo(DataType.password))
        Assert.That(CStr(pvCast), Iz.EqualTo(CStr(pv)))
        Assert.That(CStr(pvCast), Iz.EqualTo(input))
        Assert.That(pvCast.EncodedValue, Iz.Not.EqualTo(input))

    End Sub

    ''' <summary>
    ''' Test that the encode and decode round trip treats the data correctly
    ''' </summary>
    <Test, TestCaseSource(GetType(TestUtil), "PasswordTests")>
    Sub TestPasswordEncodeDecode(input As String)
        Dim ss As New SafeString(input)

        Dim pv As New clsProcessValue(ss)
        Dim encType As String = pv.EncodedType
        Dim encVal As String = pv.EncodedValue

        Dim pvCoded = clsProcessValue.Decode(encType, encVal)

        Assert.That(pvCoded.DataType, Iz.EqualTo(DataType.password))
        Assert.That(CStr(pvCoded), Iz.EqualTo(input))
        Assert.That(pvCoded.EncodedValue, Iz.EqualTo(pv.EncodedValue))
        Assert.That(pvCoded, Iz.EqualTo(pv))

    End Sub

    ''' <summary>
    ''' Test that equals works correctly for process values with passwords of equal
    ''' value.
    ''' </summary>
    <Test, TestCaseSource(GetType(TestUtil), "PasswordTests")>
    Sub TestPasswordEquals(input As String)
        Dim ss As New SafeString(input)

        Dim pv As New clsProcessValue(ss)
        Dim pvClone = pv.Clone()

        Assert.That(pvClone, Iz.EqualTo(pv))
        Assert.That(pvClone.IsValueMatch(pv), Iz.True)

        Dim pvDecodeEncode = clsProcessValue.Decode(pv.EncodedType, pv.EncodedValue)
        Assert.That(pvDecodeEncode, Iz.EqualTo(pv))
        Assert.That(pvDecodeEncode.IsValueMatch(pv), Iz.True)

        ' We want to check that the old way works, but there's a Debug assertion in
        ' there that we want to keep, so disable the UI handling of the assertion
        ' failure before we test the old-school method.
        Dim list() As TraceListener = Debug.Listeners.Cast(Of TraceListener).ToArray()
        Debug.Listeners.Clear()
        Try

            Dim pvOldSchool As New clsProcessValue(DataType.password, input)
            Assert.That(pvOldSchool, Iz.EqualTo(pv))
            Assert.That(pvOldSchool.IsValueMatch(pv), Iz.True)
        Finally
            Debug.Listeners.AddRange(list)
        End Try

        Dim pvNotEqual As New clsProcessValue(New SafeString("Goodbye World"))
        Assert.That(pvNotEqual, Iz.Not.EqualTo(pv))
        Assert.That(pvNotEqual.IsValueMatch(pv), Iz.False)

    End Sub

    ''' <summary>
    ''' Tests the input and output from date ProcessValues in various cultures.
    ''' </summary>
    <Test, Ignore("Not implemented")>
    Public Sub TestCultureDates()

    End Sub

    ''' <summary>
    ''' Tests the input and output from date ProcessValues in various cultures.
    ''' </summary>
    <Test, Ignore("Not implemented")>
    Public Sub TestCultureDateTimes()

    End Sub

End Class

#End If
