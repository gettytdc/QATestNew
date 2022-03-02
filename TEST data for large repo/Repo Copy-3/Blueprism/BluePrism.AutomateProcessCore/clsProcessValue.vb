
Imports System.Drawing
Imports System.Globalization
Imports System.Runtime.Serialization
Imports System.Text.RegularExpressions
Imports System.Xml
Imports BluePrism.AutomateProcessCore.clsProcessDataTypes
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Common.Security
Imports BluePrism.Core
Imports BluePrism.Server.Domain.Models

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsProcessValue
'''
''' <summary>
''' Represents a Value - i.e. an instance of a Data Type with a
''' value attached.
''' </summary>
<Serializable()>
<KnownType(GetType(SafeString))>
<KnownType(GetType(clsCollection))>
<DebuggerDisplay("ProcessValue = {DebuggerDisplay}")> _
Public Class clsProcessValue
    Implements IComparable(Of clsProcessValue), ISerializable

#Region " Class-scope Declarations "

    ' The format used internally to represent a DataType.date value
    Friend Const InternalDateFormat As String = "yyyy'/'MM'/'dd"

    ' The format used internally to represent a DataType.time value
    Friend Const InternalTimeFormat As String = "HH':'mm':'ss"

    ' The format used internally to represent a DataType.datetime value
    Public Const InternalDateTimeFormat As String = "u"

    ' The list of date formats used by this class
    Private Shared ReadOnly Formats As IList(Of String) = GetReadOnly.IListFrom( _
     InternalDateFormat, InternalTimeFormat, InternalDateTimeFormat)

    ' Regex which describes a valid timespan format as held internally by this class
    Private Shared ReadOnly TimespanRx As New Regex("^[+\-]?\d+\.\d\d:\d\d:\d\d$")

    ' Regex which describes a valid base64-encoded string
    Private Shared ReadOnly Base64Rx As New Regex("^[a-zA-Z0-9\+/]*={0,3}$")

    ''' <summary>
    ''' Gets the possible encoded values for the given date using the internal
    ''' encodings of this class for dates and times.
    ''' Note that this will return the 'encoded values' of the given date, so much
    ''' as a representation of those encoded values which can be used in a search
    ''' of values encoded into some text somewhere (typically XML). Specifically,
    ''' the search using these encodes should check for the existence of these
    ''' encodes within a larger string rather than checking for precise equality.
    ''' (eg. using "...where x like '%' + term + '%'" rather than "where x = term")
    ''' </summary>
    ''' <param name="dt">The date for which the encoded alternatives are required.
    ''' </param>
    ''' <returns>The alternative encodings of the given date using the encoding rules
    ''' of this class.</returns>
    ''' <remarks>If the given date has no date component (ie. the date component of
    ''' the DateTime is 01/01/0001), this will assume it represents a 'time'.
    ''' Otherwise, a date or datetime is assumed and both encoding variations will
    ''' be returned. The granularity of the datetime encoding returned is determined
    ''' by the incoming date - if it has a 'seconds' component, that will be encoded
    ''' into the returned string, otherwise only hours and minutes are represented
    ''' in the emitted encode.</remarks>
    Public Shared Function GetDateEncodes(ByVal dt As Date) As ICollection(Of String)
        ' If there is no date component, that's easy - we just return the date
        ' encoded to a time process value.
        If dt.Date = Date.MinValue Then _
         Return GetSingleton.ICollection(dt.ToString(InternalTimeFormat))

        ' If no time is specified, we actually want to search for the date only,
        ' even within a datetime value.
        If dt.TimeOfDay = TimeSpan.Zero Then
            ' We use the date component of the internal datetime format only as well
            ' as our internal date format
            Return GetReadOnly.ICollectionFrom( _
             dt.ToString("yyyy'-'MM'-'dd"), _
             dt.ToString(InternalDateFormat))

        End If

        ' Otherwise a time is specified as well;
        ' If seconds are specified, we provide the string to search for which
        ' includes a seconds component, otherwise we limit ourselves to hours
        ' and minutes.
        If dt.Second > 0 Then
            Return GetReadOnly.ICollectionFrom( _
             dt.ToString("yyyy'-'MM'-'dd HH':'mm':'ss"), _
             dt.ToString(InternalDateFormat))
        Else
            Return GetReadOnly.ICollectionFrom( _
             dt.ToString("yyyy'-'MM'-'dd HH':'mm"), _
             dt.ToString(InternalDateFormat))
        End If

    End Function

    ''' <summary>
    ''' Gets the number encoded using the encoded rules in this class.
    ''' </summary>
    ''' <param name="num">The number for which the internal encoding is required.
    ''' </param>
    ''' <returns>The encoded number using the encoding rules in this class.</returns>
    Public Shared Function GetNumberEncode(ByVal num As Decimal) As String
        Return num.ToString(InternalCulture.Instance)
    End Function

    ''' <summary>
    ''' Validate the given <em>encoded</em> value against the specified datatype
    ''' </summary>
    ''' <param name="val">The value to validate</param>
    ''' <returns>True if the given value is valid, False otherwise</returns>
    ''' <remarks>This is for validating the internal format of a string value; a
    ''' failure in validation here does not mean we cannot interpret the given value
    ''' and get a meaningful value from it; it just means that the internal value
    ''' is not correct.</remarks>
    Public Shared Function ValidateInternal( _
     ByVal type As DataType, ByVal val As String) As Boolean
        Try
            Select Case type
                Case DataType.date, DataType.datetime
                    ' Null dates/datetimes are ok...
                    If val = "" Then Return True

                    Dim rx As Regex
                    If type = DataType.date _
                     Then rx = New Regex("^\d{4}[-/]\d{2}[-/]\d{2}$") _
                     Else rx = New Regex("^\d{4}[-/]\d{2}[-/]\d{2} \d{2}:\d{2}:\d{2}Z?$")

                    ' The Parse() will throw an exception if the year/month/day
                    ' combination is not valid.
                    If rx.IsMatch(val) Then Date.Parse(val) : Return True

                    ' If we get here, it wasn't a match to the regex, thus invalid
                    Return False

                Case DataType.flag
                    ' Null, True or False are the only valid values
                    Return (val = "" OrElse val = "True" OrElse val = "False")

                Case DataType.number
                    ' Null is allowed, or a parse using the invariant culture
                    Return (val = "" OrElse Decimal.TryParse(val, _
                     NumberStyles.Number, InternalCulture.Instance, Nothing))

                Case DataType.password, DataType.text
                    'Anything goes for a password or text

                Case DataType.time
                    'Null times are ok...
                    If val = "" Then Return True
                    For Each c As Char In val
                        'No characters other than these are valid in a time...
                        If Not Char.IsDigit(c) AndAlso c <> ":"c Then Return False
                    Next
                    '** TEMP ** Further validation can be done on the
                    '           actual format here...

                Case DataType.timespan
                    'Null timespans are ok...
                    If val = "" Then Return True

                    For Each c As Char In val
                        'No characters other than these are valid in a time...
                        If Not Char.IsDigit(c) _
                         AndAlso c <> ":"c AndAlso c <> "."c AndAlso c <> "-"c _
                         Then Return False
                    Next

                Case DataType.unknown
                    'If it's unknown, how can it have any value?...
                    'Unknown is really a dummy datatype.
                    Return (val = "")

                Case DataType.collection
                    'clsProcessValue does not hold collections (yet?) (TODO: er...)
                    Return False

                Case DataType.image
                    Return (val = "" OrElse clsPixRect.ValidateText(val))

                Case DataType.binary
                    Return True

                Case Else
                    'Certainly shouldn't get here - either someone did some
                    'naughty type-casting or a new DataType was added
                    'without the relevant validation code being added here.
                    Return False

            End Select
            Return True
        Catch
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Decodes the given datatype and value strings into a process value object.
    ''' </summary>
    ''' <param name="dtype">The datatype required</param>
    ''' <param name="encVal">The value, encoded using process value rules.
    ''' </param>
    ''' <returns>A process value instance of the parsed type and value.</returns>
    ''' <exception cref="InvalidDataTypeException">If the supplied
    ''' <paramref name="dtype">data type</paramref> could not be parsed into a valid
    ''' data type.</exception>
    ''' <exception cref="InvalidFormatException">If the format of the encoded value
    ''' was invalid and could not be properly decoded into the required type.
    ''' </exception>
    Public Shared Function Decode( _
     ByVal dtype As String, ByVal encVal As String) As clsProcessValue
        Return Decode(clsProcessDataTypes.Parse(dtype), encVal)
    End Function

    ''' <summary>
    ''' Special handling for potentially invalid decodes.
    ''' </summary>
    ''' <param name="dtypeStr">The type of value represented in the encoded values.
    ''' </param>
    ''' <param name="encStr">The encoded value, possibly in a format which is not
    ''' strictly supported by this class.</param>
    ''' <param name="strict">True to enforce the required encoded value in this
    ''' process value; False to allow invalid values to be treated as date/times,
    ''' even if this class can no longer convert it into a meaningful date/time.
    ''' </param>
    ''' <returns>A ProcessValue instance which represents the given encoded values.
    ''' </returns>
    Friend Shared Function Decode( _
     ByVal dtypeStr As String, ByVal encStr As String, ByVal strict As Boolean) _
     As clsProcessValue
        ' Treat as normal for 'strict' decodes
        If strict Then Return Decode(dtypeStr, encStr)
        Dim dtype As DataType = Parse(dtypeStr)
        If DateTimeTypes.Contains(dtype) Then
            ' See if we can pick up a regionally formatted datetime from the
            ' string given...
            ' If successful, we can output it in the correct format and allow
            ' the value to persist without the 'invalid' flag...
            ' Most consistent way to do this is to try and cast it from a text value.
            ' That attempts all valid formats (local and fixed) to convert it.
            Dim dateVal As clsProcessValue = Nothing
            If New clsProcessValue(encStr).TryCastInto(dtype, dateVal) Then _
             Return dateVal
        End If
        Return New clsProcessValue(dtype, encStr, True)
    End Function

    ''' <summary>
    ''' Decodes the given datatype and value into a process value object.
    ''' </summary>
    ''' <param name="dtype">The datatype required</param>
    ''' <param name="encVal">The value, encoded using process value rules.
    ''' </param>
    ''' <returns>A process value instance of the parsed type and value.</returns>
    ''' <exception cref="InvalidFormatException">If the format of the encoded value
    ''' was invalid and could not be properly decoded into the required type.
    ''' </exception>
    Public Shared Function Decode( _
     ByVal dtype As DataType, ByVal encVal As String) As clsProcessValue

        ' Sometimes the encoded value does not contain the "<collection>" root
        ' element, so check for that first.
        ' If it's empty or has a root element, the constructors deal with that
        ' so just drop through to use the standard constructor
        If dtype = DataType.collection AndAlso encVal <> "" _
         AndAlso Not encVal.StartsWith("<collection>") Then
            Try
                Return clsCollection.ParseWithoutRoot(encVal)
            Catch
                ' Any errors and we try with the main constructor rather than
                ' the parsing without the root element
            End Try
        End If
        Return New clsProcessValue(dtype, encVal)
    End Function

#End Region

#Region " Member Variables "

    ' The data type of this value, defaults to unknown
    Private mDataType As DataType

    ' The password for this value; only meaningful for password process values
    Private mPassword As SafeString

    ' The current value, in Automate's text-encoded format. See the documentation
    ' for GetEncodedValue() for full details.
    Private mValue As String

    ' Holds the current value when the value is of datatype binary.
    Private mBinaryValue As Byte()

    ' Holds the current value when the value is of datatype collection
    Private mCollection As clsCollection

    ' The description of this process value, if it has one
    Private mDescription As String = Nothing

    ' When this flag is set, the value is treated as text. This is for backward
    ' compatibility with bad business objects only, read documentation for method
    ' TreatAsText().
    Private mbTreatAsText As Boolean

#End Region

#Region " Serialization Handling "

    ''' <summary>
    ''' Writes the data held in this object to the serialization info given.
    ''' </summary>
    ''' <param name="info">The info to which the data should be written</param>
    ''' <param name="ctx">The context for the serialization</param>
    Sub GetObjectData(info As SerializationInfo, ctx As StreamingContext) _
     Implements ISerializable.GetObjectData

        Dim contextString = CStr(If(ctx.Context, ""))
        If contextString = "datagateway" Then
            If mValue IsNot Nothing Then info.AddValue("Value", mValue)
            If mBinaryValue IsNot Nothing Then info.AddValue("Binary Value", mBinaryValue)
            If mCollection IsNot Nothing Then info.AddValue("Collection", mCollection)
            If mPassword IsNot Nothing Then info.AddValue("Password", mPassword)
        Else
            info.AddValue("dt", mDataType)
            info.AddValue("val", mValue)
            info.AddValue("pwd", CType(Password, SafeString))
            info.AddValue("bin", mBinaryValue)
            info.AddValue("coll", mCollection)
            info.AddValue("desc", mDescription)
            info.AddValue("tat", mbTreatAsText)
        End If


    End Sub

    ''' <summary>
    ''' Creates the object and populates it with the data from the given
    ''' serialization info
    ''' </summary>
    ''' <param name="info">The info containing the data to be used to populate the
    ''' new process value object</param>
    ''' <param name="ctx">The context for the serialization</param>
    Protected Sub New(info As SerializationInfo, ctx As StreamingContext)
        Dim enu = info.GetEnumerator()
        While enu.MoveNext()
            Dim e = enu.Current
            Select Case e.Name
                Case "dt" : mDataType = CType(e.Value, DataType)
                Case "val" : mValue = CStr(e.Value)
                Case "desc" : mDescription = CStr(e.Value)
                Case "pwd" : Password = CType(e.Value, SafeString)
                Case "bin" : mBinaryValue = CType(e.Value, Byte())
                Case "coll" : mCollection = CType(e.Value, clsCollection)
                Case "tat" : mbTreatAsText = CBool(e.Value)

            End Select
        End While
    End Sub

#End Region

#Region " Constructors "

    ' Note that the serialization constructor is in the 'Serialization Handling'
    ' region rather than this one

    ''' <summary>
    ''' Default constructor. The instance will be in an invalid state
    ''' until a Data Type and Value have been set, in that order.
    ''' </summary>
    Public Sub New()
        Me.New(DataType.unknown, "")
    End Sub

    ''' <summary>
    ''' Creates a new process value representing a GUID. This will be a text data
    ''' item containing the GUID string value, or an empty string if Guid.Empty is
    ''' given.
    ''' </summary>
    ''' <param name="id">The ID for which a process value is required.</param>
    Public Sub New(ByVal id As Guid)
        Me.New(DataType.text, CStr(IIf(id = Guid.Empty, "", id.ToString())))
    End Sub

    ''' <summary>
    ''' Creates a new image process value using the given bitmap.
    ''' </summary>
    ''' <param name="img">The image to hold in the new process value.</param>
    Public Sub New(ByVal img As Bitmap)
        Me.New(DataType.image, clsPixRect.BitmapToString(img))
    End Sub

    ''' <summary>
    ''' Constructor to create a process value of type collection populated from a
    ''' DataTable.
    ''' </summary>
    ''' <param name="tab">A DataTable whose rows and columns will be
    ''' directly converted to a collection.</param>
    Public Sub New(ByVal tab As DataTable)
        Me.New(New clsCollection(tab))
    End Sub

    ''' <summary>
    ''' Creates a new process value of type 'Text' and sets its value to the given
    ''' string.
    ''' </summary>
    ''' <param name="txt">The value of the text data item</param>
    Public Sub New(ByVal txt As String)
        Me.New(DataType.text, txt)
    End Sub

    ''' <summary>
    ''' Creates a new process value of type
    ''' <see cref="AutomateProcessCore.DataType.password">password</see> and sets its value to that of
    ''' the given secure string.
    ''' </summary>
    ''' <param name="ss">The safe string to extract the value from in this process
    ''' value. Note that a copy of the safe string is made - the actual instance
    ''' passed in can be safely modified without altering the value of the resultant
    ''' process value object.</param>
    Public Sub New(ss As SafeString)
        mDataType = DataType.password
        Password = If(ss Is Nothing, New SafeString(), ss.Copy())
    End Sub

    ''' <summary>
    ''' Creates a new process value of type 'Flag' and sets its value to the flag
    ''' representation of the specified boolean value.
    ''' </summary>
    ''' <param name="flagValue">True to indicate that the value of the flag should
    ''' be 'True'; False to indicate that it should be 'False'.</param>
    Public Sub New(ByVal flagValue As Boolean)
        Me.New(DataType.flag, CStr(IIf(flagValue, "True", "False")))
    End Sub

    ''' <summary>
    ''' Creates a new timespan process value based on the given system TimeSpan value
    ''' </summary>
    ''' <param name="value">The timespan representing the value that should be
    ''' held in this process value object.</param>
    Public Sub New(ByVal value As TimeSpan)
        Me.New(DataType.timespan, GetEncodedTextForTimeSpan(value))
    End Sub

    ''' <summary>
    ''' Creates a new numeric process value with the given value.
    ''' </summary>
    ''' <param name="num">The value to set in the resultant process value object.
    ''' </param>
    Public Sub New(ByVal num As Integer)
        Me.New(CDec(num))
    End Sub

    ''' <summary>
    ''' Creates a new numeric process value with the given value.
    ''' </summary>
    ''' <param name="num">The value to set in the resultant process value object.
    ''' </param>
    Public Sub New(ByVal num As Double)
        Me.New(CDec(num))
    End Sub

    ''' <summary>
    ''' Creates a new numeric process value with the given value.
    ''' </summary>
    ''' <param name="num">The value to set in the resultant process value object.
    ''' </param>
    Public Sub New(ByVal num As Decimal)
        Me.New(DataType.number, num.ToString(InternalCulture.Instance))
    End Sub

    ''' <summary>
    ''' Constructor for binary data.
    ''' </summary>
    ''' <param name="value">The binary data, as a byte array.</param>
    Public Sub New(ByVal value As Byte())
        Me.New(DataType.binary)
        mBinaryValue = value
    End Sub

    ''' <summary>
    ''' Constructor to create a process value of type collection populated from a
    ''' clsCollection. The clsCollection is NOT copied.
    ''' </summary>
    ''' <param name="col">The clsCollection to use as the value</param>
    Public Sub New(ByVal col As clsCollection)
        Me.New(DataType.collection)
        mCollection = col
    End Sub

    ''' <summary>
    ''' Constructor that creates a Null value, with a particular data type.
    ''' </summary>
    Public Sub New(ByVal dt As DataType)
        mDataType = dt
        Select Case dt
            Case DataType.binary, DataType.collection, DataType.image
                ' Leave mValue as null for binary / collection types
            Case Else
                mValue = ""
        End Select
    End Sub

    ''' <summary>
    ''' Creates a new process value from the data in a data provider.
    ''' </summary>
    ''' <param name="prov">The provider of the data for this object. It should
    ''' contain the following data:<list>
    ''' <item>"datatype" : The data type of the required value - one of the types
    ''' defined in <see cref="clsProcessDataTypes"/></item>
    ''' <item>"value" : The encoded string value of the data</item></list>
    ''' If the provider does not contain a datatype, then a datatype of
    ''' <see cref="AutomateProcessCore.DataType.unknown"/> is used.
    ''' </param>
    ''' <exception cref="InvalidDataTypeException">If the given data type was not
    ''' recognised.</exception>
    Public Sub New(ByVal prov As IDataProvider)
        Me.New( _
         Parse(prov.GetValue("datatype", "unknown")), prov.GetString("value"))
    End Sub

    ''' <summary>
    ''' Constructor that takes a data type and a value as a DateTime. The data type
    ''' must be one of the three date/time related types.
    ''' </summary>
    ''' <param name="dtDataType">The data type</param>
    ''' <param name="dValue">The value</param>
    ''' <param name="bValueIsLocal">If set to True, the value given is treated as
    ''' being local time - otherwise it is assumed to be universal. This is relevant
    ''' only for a DataType.datetime data type - the other two use the value in the
    ''' DateTime directly, regardless of this parameter.</param>
    Public Sub New(ByVal dtDataType As DataType, _
     ByVal dValue As DateTime, Optional ByVal bValueIsLocal As Boolean = False)
        Me.New(dtDataType, GetEncodedTextForDate(dtDataType, dValue, bValueIsLocal))
    End Sub

    ''' <summary>
    ''' Constructor that takes Data Type and Value.
    ''' </summary>
    ''' <param name="dtype">The data type of this instance.
    ''' </param>
    ''' <param name="val">The initial value for the instance, in Automate's
    ''' text-encoded format. For full details, see the documentation for
    ''' GetEncodedValue()</param>
    ''' <exception cref="InvalidFormatException">If the given string did not pass a
    ''' validation check for the specified data type</exception>
    Public Sub New(ByVal dtype As DataType, ByVal val As String)
        Me.New(dtype, val, False)
    End Sub

    ''' <summary>
    ''' Constructor that takes Data Type and Value.
    ''' </summary>
    ''' <param name="dtype">The data type of this instance.
    ''' </param>
    ''' <param name="val">The initial value for the instance, in Automate's
    ''' text-encoded format. For full details, see the documentation for
    ''' <see cref="EncodedValue"/></param>
    ''' <param name="allowInvalid">If this optional parameter is True invalid data
    ''' (e.g. a mis-formatted date) will be allowed to be stored. In addition, it is
    ''' used during validation to signify that the value is invalid and only the data
    ''' type is relevant. It is flagged as 'TreatAsText' from that point on, in
    ''' order to allow previous behaviour of live processes relying on bad Business
    ''' Object outputs to be maintained. No new code should use this flag, and no
    ''' new Business Objects/Processes should exploit the backwards-compatibility
    ''' option.</param>
    ''' <exception cref="InvalidFormatException">If the given string did not pass a
    ''' validation check for the specified data type. This exception will not be
    ''' thrown for some data types if the <paramref name="allowInvalid"/> parameter
    ''' is set to true, eg. dates, flags, numbers.</exception>
    Public Sub New( _
     ByVal dtype As DataType, ByVal val As String, ByVal allowInvalid As Boolean)

        ' Store the data type first...
        Me.New(dtype)

        ' Special case handling for timespans for the invalid values which were
        ' being written at some point - see bug 6378
        If dtype = DataType.timespan AndAlso _
         val <> "" AndAlso Not TimespanRx.IsMatch(val) Then
            Try
                val = GetEncodedTextForTimeSpan(TimeSpan.Parse(val))
            Catch fe As FormatException
                If Not allowInvalid Then Throw New InvalidFormatException(
                 My.Resources.Resources.clsProcessValue_CouldNotConvertTheString0IntoATimespanValue1,
                 val, fe.Message)
                ' If we're allowing invalid values at this point, we just have to
                ' accept it as it is.
            End Try

        End If

        'Special case handling for numbers, which may have been written in a locale
        'specific way in v4.x and earlier.
        If dtype = AutomateProcessCore.DataType.number AndAlso val.Contains(",") Then
            val = val.Replace(",", ".")
        End If

        If dtype = DataType.collection Then
            ' Treat an empty value string as a null collection, otherwise
            ' load the collection from the XML in the value.
            If val <> "" Then
                mCollection = New clsCollection()
                mCollection.Parse(val)
            End If

        ElseIf dtype = DataType.binary Then
            Try
                If val = "" _
                 Then mBinaryValue = Nothing _
                 Else mBinaryValue = Convert.FromBase64String(val)
            Catch fe As FormatException
                Throw New InvalidFormatException(
                 My.Resources.Resources.clsProcessValue_CouldNotConvertTheGivenBase64StringIntoABinaryValue0,
                 fe.Message)
            End Try

        ElseIf dtype = DataType.password Then
            Try
                If val = "" Then
                    ' If we don't have a value, we still need a secure string
                    Password = New SafeString()
                Else
                    ' If there is a value present, it might be an encoded SafeString,
                    ' so try that first.
                    Password = SafeString.Decode(val)
                End If

            Catch fe As FormatException
                ' This is not ideal - really we should be stopping code which uses
                ' plaintext strings to create password values, but at the moment, I
                ' just want to support passwords properly within this class - I can
                ' worry about how to better call this later
                Debug.Fail(
                    My.Resources.Resources.clsProcessValue_SomethingIsCallingNewClsProcessValuePassingAPasswordInAsPlaintext)
                Password = New SafeString(val)

            End Try


        Else

            ' A null in here can break things quite spectacularly (for text items).
            ' Treat it as empty string, since we (partially) handle the concept
            ' of null at a higher level than reference level.
            If val Is Nothing Then val = ""

            ' Auto-normalise any image data so that we're always dealing with
            ' the same format of image
            Try
                If dtype = DataType.image AndAlso val <> "" Then _
                 val = clsPixRect.NormaliseString(val)
            Catch
                Throw New InvalidFormatException(
                 My.Resources.Resources.clsProcessValue_InvalidValue0ForANew1,
                 IIf(val.Length > 100, val.Left(100) & "...", val), dtype)
            End Try

            'See if the data is valid, before we decide what to do...
            If Not ValidateInternal(val) Then
                ' If we're not allowing invalid values (?), raise an error
                If Not allowInvalid Then Throw New InvalidFormatException(
                 My.Resources.Resources.clsProcessValue_InvalidValue0ForANew1,
                 IIf(val.Length > 100, val.Left(100) & "...", val), dtype)

                ' Otherwise, a known bad business object must have given us this
                ' data, so we will allow it in and make sure we handle
                ' it as Automate used to.
                mbTreatAsText = True
            End If

            mValue = val
        End If

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the active password in this value. Note that this will only ever
    ''' be null if the <see cref="DataType"/> is something other than
    ''' <see cref="AutomateProcessCore.DataType.password">password</see>. If it is otherwise, somehow set
    ''' to null, this instantiates a new password value before returning it.
    ''' </summary>
    Private Property Password As SafeString
        Get
            If mDataType <> DataType.password Then Return Nothing
            If mPassword Is Nothing Then mPassword = New SafeString()
            Return mPassword
        End Get
        Set(value As SafeString)
            mPassword = value
        End Set
    End Property

    ''' <summary>
    ''' The current type of this value, encoded into a string.
    ''' This value is guaranteed to work correctly using the static
    ''' "clsProcessValue.Decode" methods.
    ''' </summary>
    Public ReadOnly Property EncodedType() As String
        Get
            Return mDataType.ToString()
        End Get
    End Property

    ''' <summary>
    ''' The current encoded value of this instance. This should only be used to pass
    ''' data around safely - the format of the output of this property is subject to
    ''' change so it should not be relied upon to be in a particular format.
    '''
    ''' Basically, don't parse or substring this property externally to get at
    ''' individual components of a particular datatype encoding - use the appropriate
    ''' conversion operator or write a method in this class to do it so that all of
    ''' the knowledge about the makeup of the internal format remains in this one
    ''' class.
    ''' </summary>
    Public ReadOnly Property EncodedValue() As String
        Get
            If mDataType = DataType.collection Then
                If mCollection Is Nothing _
                 Then Return "" _
                 Else Return mCollection.GenerateXML()

            ElseIf mDataType = DataType.binary Then
                ' BASE64Encode(Nothing) is ""
                If mBinaryValue Is Nothing Then Return ""
                Return Convert.ToBase64String(mBinaryValue)

            ElseIf mDataType = DataType.password Then
                If Password Is Nothing Then Return ""
                Return Password.Encoded

            Else
                Return mValue

            End If
        End Get
    End Property

    ''' <summary>
    ''' Get the current value for this instance in a form appropriate for logging.
    ''' In most cases, this is the same as <see cref="EncodedValue"/>, but for
    ''' example, for passwords, we mask the characters
    ''' </summary>
    Public ReadOnly Property LoggableValue() As String
        Get
            Select Case mDataType
                Case DataType.collection
                    If mCollection Is Nothing Then Return "<null collection/>"
                    Return mCollection.GenerateXML(True)
                Case DataType.image, DataType.password
                    Return FormattedValue
                Case DataType.binary
                    If mBinaryValue Is Nothing Then Return ""
                    Return Convert.ToBase64String(mBinaryValue)
                Case Else
                    Return mValue
            End Select
        End Get
    End Property

    ''' <summary>
    ''' Formats the value for display to the user. When the datatype is collection,
    ''' the number of rows will be returned, ready for display (eg "9 rows").
    '''
    ''' The result returned is in user-format and should never be processed or passed
    ''' to another component. Doing so can lose and/or corrupt data.
    ''' It is for Display!
    ''' </summary>
    Public ReadOnly Property FormattedValue() As String
        Get
            'Backward-compatibility for Business Objects that don't return
            'values according to the specification - we can't attempt to
            'format such a value for display, so we just return the exact
            'text the Business Object gave us in the first place.
            If mbTreatAsText Then Return mValue

            Select Case mDataType

                Case DataType.date, DataType.datetime, DataType.time

                    'A null date is displayed as an empty string...
                    If mValue = "" Then Return ""

                    'Convert our internal storage to a working format.
                    'Note that we do not need the functionality of
                    'Date.Parse here, as we are working with our internal
                    'format. We shouldn't be using it.
                    Dim dt As DateTime = CDate(Me)
                    Select Case mDataType ' date,datetime or time
                        Case DataType.date : Return dt.ToString("d")
                        Case DataType.datetime : Return dt.ToString("G")
                        Case Else : Return dt.ToString("T")
                    End Select

                Case DataType.number
                    If mValue = "" Then Return ""
                    ' We want to output the localized version for this output, so
                    ' parse the internal representation using the invariant culture
                    ' and then output using the currently set culture
                    Return CDec(Me).ToString()

                Case DataType.timespan
                    '** TEMP **:
                    'We could display Timespans better than the default...
                    '(But don't forget about NULL's)
                    Return mValue

                Case DataType.password
                    If Password.Length = 0 Then Return ""

                    'Mask password characters
                    Return New String(BPUtil.PasswordChar, Password.Length)

                Case DataType.collection
                    Return clsCollection.GetInfoLabel(Me.Collection)

                Case DataType.image
                    If mValue = "" Then Return My.Resources.Resources.clsProcessValue_Empty
                    'If there is an image, return something like "320x240"...
                    Dim sz As Size = clsPixRect.ParseDimensions(mValue)
                    Return sz.Width & "x" & sz.Height

                Case DataType.binary
                    If mBinaryValue Is Nothing Then Return My.Resources.Resources.clsProcessValue_Empty
                    Return String.Format(My.Resources.Resources.clsProcessValue_0Bytes, mBinaryValue.Length)

                Case Else
                    Return mValue
            End Select
        End Get
    End Property

    ''' <summary>
    ''' Formats the date/time according to the given format. This overload ensures
    ''' that the current date is not set within a <see cref="AutomateProcessCore.DataType.time">time</see>
    ''' value, which it will be if the style is omitted.
    ''' </summary>
    ''' <param name="fmt">The format required for the data held by this process
    ''' value. This follows the standard .NET formatting rules, accepted by the
    ''' "DateTime.ToString" method amongst others.</param>
    ''' <returns>The date/time held in this value formatted as required or an empty
    ''' string if this value is <see cref="IsNull">null</see>, ie. empty.</returns>
    ''' <exception cref="InvalidDataTypeException">If the data type is anything other
    ''' than <see cref="AutomateProcessCore.DataType.[date]"/>, <see cref="AutomateProcessCore.DataType.datetime"/> or
    ''' <see cref="AutomateProcessCore.DataType.time"/></exception>
    ''' <exception cref="FormatException">If the format given was invalid - ie. a
    ''' single character which does not match one of the standard format codes, or
    ''' an invalid custom format pattern</exception>
    Public Function FormatDate(ByVal fmt As String) As String
        Return FormatDate(fmt, DateTimeStyles.NoCurrentDateDefault)
    End Function

    ''' <summary>
    ''' Formats the date/time according to the given format.
    ''' </summary>
    ''' <param name="fmt">The format required for the data held by this process
    ''' value. This follows the standard .NET formatting rules, accepted by the
    ''' "DateTime.ToString" method amongst others.</param>
    ''' <param name="styles">The styles to apply when parsing the date held in this
    ''' process value.</param>
    ''' <returns>The date/time held in this value formatted as required or an empty
    ''' string if this value is <see cref="IsNull">null</see>, ie. empty.</returns>
    ''' <exception cref="InvalidDataTypeException">If the data type is anything other
    ''' than <see cref="AutomateProcessCore.DataType.[date]"/>, <see cref="AutomateProcessCore.DataType.datetime"/> or
    ''' <see cref="AutomateProcessCore.DataType.time"/></exception>
    ''' <exception cref="FormatException">If the format given was invalid - ie. a
    ''' single character which does not match one of the standard format codes, or
    ''' an invalid custom format pattern</exception>
    Friend Function FormatDate(
     ByVal fmt As String, ByVal styles As DateTimeStyles) As String

        If IsNull Then Return ""

        ' Get the format we need to use to parse the value
        Dim internalFormat As String
        Select Case mDataType
            Case DataType.date : internalFormat = InternalDateFormat
            Case DataType.datetime : internalFormat = InternalDateTimeFormat
            Case DataType.time : internalFormat = InternalTimeFormat
            Case Else
                ' We can format a date if we don't have a date (/time/datetime)
                Throw New InvalidDataTypeException(
                 My.Resources.Resources.clsProcessValue_CannotFormatADateOnA0Value, mDataType)
        End Select

        Return Date.ParseExact(mValue, internalFormat, Nothing, styles).ToString(fmt)

    End Function

    ''' <summary>
    ''' The display text for the debugger for an instance of this class
    ''' </summary>
    Private ReadOnly Property DebuggerDisplay() As String
        Get
            Return DataTypeName & ": " & FormattedValue
        End Get
    End Property

    ''' <summary>
    ''' The data type of this instance. This is an internal data type
    ''' name, as defined in DataType
    ''' </summary>
    Public Property DataType() As DataType
        Get
            Return mDataType
        End Get
        Set(ByVal value As DataType)
            mDataType = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the friendly name of the data type set within this value.
    ''' </summary>
    Public ReadOnly Property DataTypeName() As String
        Get
            Return GetFriendlyName(Me.DataType)
        End Get
    End Property

    ''' <summary>
    ''' A description of the value, or it's purpose. This is optional, and used
    ''' in certain cases to carry around a useful description - for example, when
    ''' the value is that of a session variable, which is being passed elsewhere.
    ''' Frequently though, this will just be Nothing, indicating no description is
    ''' available.
    ''' </summary>
    Public Property Description() As String
        Get
            Return mDescription
        End Get
        Set(ByVal value As String)
            mDescription = value
        End Set
    End Property

    ''' <summary>
    ''' True if the value is a null value, False otherwise. Note that
    ''' only some data types have a concept of null. Number is an
    ''' example of one that does, and Text is one that doesn't!
    ''' </summary>
    Public ReadOnly Property IsNull() As Boolean
        Get
            Select Case mDataType
                Case DataType.unknown : Return True
                Case DataType.binary : Return (mBinaryValue Is Nothing)
                Case DataType.collection : Return (mCollection Is Nothing)
                Case DataType.number, DataType.flag, DataType.date,
                 DataType.datetime, DataType.time, DataType.timespan, DataType.image
                    Return (mValue = "")

            End Select
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Checks if this value is valid, or if it's allowed itself to contain invalid
    ''' data
    ''' </summary>
    Public ReadOnly Property IsValid() As Boolean
        Get
            Return ValidateInternal(mValue)
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets the collection backing this process value.
    ''' This will have little effect unless the data type of this value object is
    ''' <see cref="AutomateProcessCore.DataType.collection" />
    ''' </summary>
    Public Property Collection() As clsCollection
        Get
            Return mCollection
        End Get
        Set(ByVal value As clsCollection)
            mCollection = value
        End Set
    End Property

    ''' <summary>
    ''' Gets whether this process value contains any collection data - ie. that it
    ''' contains a collection with any rows in it.
    ''' </summary>
    Public ReadOnly Property HasCollectionData As Boolean
        Get
            Return mCollection IsNot Nothing AndAlso mCollection.Count > 0
        End Get
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Gets the encoded text which represents the given date in the specified
    ''' data type, converting it from local if necessary.
    ''' </summary>
    ''' <param name="dt">The data type to be converted into</param>
    ''' <param name="dval">The date value to encode</param>
    ''' <param name="isLocal">True to indicate that the date represents a local
    ''' date/time and needs to be converted into universal. If the given date has
    ''' a <see cref="DateTime.Kind"/> value of <see cref="DateTimeKind.Local"/>, it
    ''' will not be altered, regardless of this flag.</param>
    ''' <returns>The encoded text which represents the given date/time in the
    ''' required datatype.</returns>
    Private Shared Function GetEncodedTextForDate(ByVal dt As DataType,
     ByVal dval As Date, ByVal isLocal As Boolean) As String
        Select Case dt
            Case DataType.datetime
                ' Use MinValue as a 'null' date
                If dval = Date.MinValue Then Return ""
                If isLocal Then dval = dval.ToUniversalTime()
                Return dval.ToString("u")

            Case DataType.date
                ' Only use the date component of the date
                dval = dval.Date
                ' Use MinValue to detect a null date
                If dval = Date.MinValue Then Return ""
                Return dval.ToString(InternalDateFormat)

            Case DataType.time
                ' We can't easily detect a null value with time since midnight
                ' is a perfectly reasonable value to use (as opposed to 01/01/0001)
                ' so we just have to use the time as it comes in
                Return dval.ToString(InternalTimeFormat)

        End Select
        Throw New BluePrismException(My.Resources.Resources.clsProcessValue_InvalidDataTypeForDateTime0, dt)

    End Function

    ''' <summary>
    ''' Gets the internal encoded text for the given timespan
    ''' </summary>
    ''' <param name="ts">The timespan for which the text is required</param>
    ''' <returns>The encoded text representing the given timespan</returns>
    Private Shared Function GetEncodedTextForTimeSpan(ByVal ts As TimeSpan) As String
        ' This is very close to the standart TimeSpan.ToString() except that we
        ' always use the days value (even if zero) and we don't handle any
        ' time component below a second
        Return String.Format("{0}{1}.{2:D2}:{3:D2}:{4:D2}",
         IIf(ts.Ticks < 0L, "-", ""),
         Math.Abs(ts.Days),
         Math.Abs(ts.Hours),
         Math.Abs(ts.Minutes),
         Math.Abs(ts.Seconds))
    End Function

    ''' <summary>
    ''' Safely parses text originating from outside of Automate as a Process
    ''' Value object.
    ''' </summary>
    ''' <param name="dtype">The expected data type.</param>
    ''' <param name="valStr">The string value to be parsed.</param>
    ''' <returns>Returns the resulting Process Value object.</returns>
    ''' <exception cref="InvalidValueException">If the given value is not valid.
    ''' </exception>
    ''' <remarks>Throws exception if the value is not valid.</remarks>
    Public Shared Function FromUIText(
     ByVal dtype As DataType, ByVal valStr As String) As clsProcessValue
        Select Case dtype
            Case DataType.unknown, DataType.password, DataType.collection
                Throw New InvalidValueException(
                 My.Resources.Resources.clsProcessValue_DataTypeOf0IsNotValidForAValueOriginatingFromAUserInterface, GetFriendlyName(dtype))

            Case DataType.text
                Return valStr

            Case DataType.datetime, DataType.date, DataType.time
                Dim resultDate As DateTime
                If DateTime.TryParse(valStr, resultDate) Then
                    Return New clsProcessValue(dtype, resultDate)
                Else
                    Throw New InvalidValueException(
                     My.Resources.Resources.clsProcessValue_InvalidValueForADateDatetime0, valStr)
                End If

            Case DataType.flag
                Try
                    Return CBool(valStr)
                Catch
                    Throw New InvalidValueException(
                     My.Resources.Resources.clsProcessValue_InvalidValueForAFlag0, valStr)
                End Try

            Case DataType.number
                Try
                    Return CDec(valStr)
                Catch ex As Exception
                    Throw New InvalidValueException(
                     My.Resources.Resources.clsProcessValue_InvalidValueForANumber0, valStr)
                End Try

            Case DataType.timespan
                'We assume that this is in one format only: the unique valid format
                Return New clsProcessValue(dtype, valStr)

            Case Else
                Throw New InvalidValueException(
                 My.Resources.Resources.clsProcessValue_UnexpectedDataType0, GetFriendlyName(dtype))
        End Select
    End Function

    ''' <summary>
    ''' Validate the given <em>encoded</em> value against the currently set datatype
    ''' </summary>
    ''' <param name="val">The value to validate</param>
    ''' <returns>True if the given value is valid, False otherwise</returns>
    ''' <remarks>This is for validating the internal format of a string value; a
    ''' failure in validation here does not mean we cannot interpret the given value
    ''' and get a meaningful value from it; it just means that the internal value
    ''' is not correct.</remarks>
    Private Function ValidateInternal(ByVal val As String) As Boolean
        Return ValidateInternal(mDataType, val)
    End Function

    ''' <summary>
    ''' Tests if this process value matches the given string.
    ''' This test may differ depending on the type of process value held.
    ''' </summary>
    ''' <param name="text">The text to match</param>
    ''' <param name="partialMatch">Whether a partial match counts as a match or not
    ''' </param>
    ''' <param name="caseSens">Whether case difference inhibits a match or not.
    ''' </param>
    ''' <returns>True if the given constrained text is a match for this process value
    ''' </returns>
    ''' <remarks>Values of type 'binary', 'image' and 'unknown' cannot be matched for
    ''' (reasonably) obvious reasons. Values of type 'collection' are also not
    ''' matched at the moment - they may be in future, but the processing is not
    ''' trivial and it isn't clear what should match in such a case (field names?
    ''' just values?). Also, right now, it's not needed. Finally, null values or
    ''' empty text values never match.</remarks>
    Public Function Matches(
     ByVal text As String, ByVal partialMatch As Boolean, ByVal caseSens As Boolean) As Boolean

        ' Quickest check first - we can't match on binary, image or unknown, so
        ' don't even try
        Select Case mDataType
            Case DataType.binary, DataType.collection, DataType.image, DataType.unknown
                Return False
        End Select

        ' Only encode the value once...
        Dim val As String = EncodedValue

        ' Null never matches, neither does 'empty'
        If IsNull OrElse val = "" Then Return False

        ' Finally, just check the encoded text for the search string.
        Return BPUtil.IsMatch(val, text, partialMatch, caseSens)

    End Function

    ''' <summary>
    ''' Creates a deep clone of this process value object.
    ''' </summary>
    ''' <returns>A deep copy of this object.</returns>
    Public Function Clone() As clsProcessValue
        Dim copy As clsProcessValue = DirectCast(MemberwiseClone(), clsProcessValue)
        If mBinaryValue IsNot Nothing Then _
         copy.mBinaryValue = CType(mBinaryValue.Clone(), Byte())
        If mCollection IsNot Nothing Then copy.mCollection = mCollection.Clone()
        If Password IsNot Nothing Then copy.Password = Password.Copy()
        Return copy
    End Function

    ''' <summary>
    ''' Gets a hash of this process value.
    ''' </summary>
    ''' <returns>An integer hash derived from this process value's type and data.
    ''' </returns>
    Public Overrides Function GetHashCode() As Integer
        Dim num As Integer = (CInt(mDataType) << 16) Xor EncodedValue.GetHashCode()
        ' FIXME: ???
    End Function

    ''' <summary>
    ''' Determines if this value is equal to another by comparing datatype and
    ''' <see cref="EncodedValue">encoded value</see>.
    ''' </summary>
    ''' <param name="o">Object to compare.</param>
    ''' <returns>True if equal, false otherwise.</returns>
    ''' <remarks>This should work correctly for all data types now.</remarks>
    Public Overrides Function Equals(ByVal o As Object) As Boolean
        Dim val As clsProcessValue = TryCast(o, clsProcessValue)

        ' Null is never equal
        If val Is Nothing Then Return False

        ' If the datatypes don't match they are not equal
        If mDataType <> val.DataType Then Return False

        ' If one value is null, the other one must be in order to be equal
        If IsNull Then Return val.IsNull

        ' Otherwise, we must check the underlying value
        Select Case mDataType
            Case DataType.collection
                Return clsCollection.Equals(mCollection, val.mCollection)
            Case DataType.binary
                Return CollectionUtil.AreEqual(mBinaryValue, val.mBinaryValue)
            Case DataType.number
                ' For some reason, the process operator
                ' (clsProcessOperators.DoOp_Equality) always cast into an actual
                ' number first - I guess we could hold trailing zeroes or something,
                ' but we should be controlling that. Anyway, left here for compat.
                Return CDec(Me) = CDec(val)
            Case Else
                Return EncodedValue = val.EncodedValue
        End Select
    End Function

    ''' <summary>
    ''' Checks if this value is a value match compared to the given object. Note that
    ''' this can return true for process values of different data types if their
    ''' internal values happen to hold the same value. As such, it should be used
    ''' with care.
    ''' </summary>
    ''' <param name="pv">The process value to test against</param>
    ''' <returns>True if the given process value is not null and holds the same value
    ''' as this process value.</returns>
    Public Function IsValueMatch(ByVal pv As clsProcessValue) As Boolean
        If pv Is Nothing Then Return False
        Return (EncodedValue = pv.EncodedValue)
    End Function

    ''' <summary>
    ''' Gets a string representation of this process value.
    ''' </summary>
    ''' <returns>This process value's string representation</returns>
    Public Overrides Function ToString() As String
        Return String.Format("{0} ({1})", EncodedValue, EncodedType)
    End Function

    ''' <summary>
    ''' Serialises this process value object to an XML element, ready to be written
    ''' as a string.
    ''' </summary>
    ''' <param name="x">The parent XML document</param>
    ''' <returns>Returns an XML element, representing this value object.</returns>
    Public Function ToXML(ByVal x As XmlDocument) As XmlElement
        Dim pvElem As XmlElement = x.CreateElement("ProcessValue")

        If mDataType = DataType.collection Then

            'add data type as sub element - TODO: why? Why not an attribute as below?
            BPUtil.AppendTextElement(pvElem, "datatype", mDataType.ToString())

            'add data value, encrypting it where appropriate
            Dim valueElem As XmlElement = x.CreateElement("value")
            Dim sVal As String = EncodedValue
            If sVal <> "" Then
                If mDataType = DataType.text Then
                    Dim a As XmlAttribute = x.CreateAttribute(
                     "xml", "space", "http://www.w3.org/XML/1998/namespace")
                    a.Value = "preserve"
                    valueElem.Attributes.Append(a)
                End If
                valueElem.AppendChild(x.CreateTextNode(sVal))
            End If
            pvElem.AppendChild(valueElem)

        Else
            pvElem.SetAttribute("datatype", mDataType.ToString())
            ' Special case for passwords
            If mDataType = DataType.password Then
                pvElem.SetAttribute("valueenc", clsProcess.GarblePassword(Password))
            Else
                pvElem.SetAttribute("value", EncodedValue)
            End If

        End If

        Return pvElem
    End Function


    ''' <summary>
    ''' Generates a process value object from an XML element.
    ''' </summary>
    ''' <param name="e">The element from which to generate
    ''' a process value. Root element must have name "ProcessValue"</param>
    ''' <returns>Returns a process value object if the xml element
    ''' is successfully parsed. Nothing otherwise.</returns>
    Public Shared Function FromXML(ByVal e As XmlElement) As clsProcessValue
        If e.Name <> "ProcessValue" Then Return Nothing

        Dim v As New clsProcessValue()

        If e.HasAttribute("datatype") Then
            v.DataType = clsProcessDataTypes.Parse(e.GetAttribute("datatype"))

            ' Passwords are now held in a subelement, but we need to support the old
            ' style passwords too, so check for them (that's the "valueenc")
            If e.HasAttribute("value") OrElse e.HasAttribute("valueenc") Then
                'More efficient format - the type and value are in attributes...

                If e.HasAttribute("valueenc") Then
                    v.Password =
                        clsProcess.UngarblePassword(e.GetAttribute("valueenc"))
                Else
                    If v.DataType = DataType.binary Then
                        v.mBinaryValue = Convert.FromBase64String(e.GetAttribute("value"))
                    Else
                        If v.DataType = DataType.text Then
                            'bg-3604 - the null char may be present in older exports (now fixed), we have to filter if now being imported
                            Dim value = e.GetAttribute("value")
                            Const pattern As String = "\x00\s*"
                            v.mValue = Regex.Replace(value, pattern, "")
                        Else
                            v.mValue = e.GetAttribute("value")
                        End If
                    End If
                End If
            Else
                ' Get the password from the element within the process value elem
                ' which holds the data parseable by SafeString
                For Each el As XmlElement In e.ChildNodes
                    If el.Name <> SafeString.XmlElementName Then _
                     Continue For
                    ' Parse and implicit cast into the SafeString we need
                    v.Password = SafeString.FromXml(el)
                Next

            End If
        Else
            'Old, less efficient format - the type and value are in child elements. We
            'do not generate this format any more, except for collections, but they still
            'exist for all types.
            For Each e2 As XmlElement In e.ChildNodes
                Select Case e2.Name
                    Case "datatype"
                        v.DataType = clsProcessDataTypes.Parse(e2.InnerText)
                    Case "valueenc"
                        v.Password = clsProcess.UngarblePassword(e2.InnerText)
                    Case "value"
                        v.mValue = e2.InnerText
                End Select
            Next
        End If

        Return v

    End Function

    ''' <summary>
    ''' Compares two value objects.
    ''' </summary>
    ''' <param name="other">The ProcessValue instance to which this
    ''' instance should be compared.</param>
    ''' <returns>Returns -1 if this instance is 'greater than'
    ''' the supplied instance; 0 if equal; +1 if 'less than'.</returns>
    ''' <remarks>See the iComparable interface.</remarks>
    Public Function CompareTo(ByVal other As clsProcessValue) As Integer _
     Implements IComparable(Of clsProcessValue).CompareTo

        ' Reference null is lower then any reference non-null
        If other Is Nothing Then Return 1

        ' Group by data type first
        Dim dtComp As Integer = DataType.CompareTo(other.DataType)
        If dtComp <> 0 Then Return dtComp

        ' Data types are the same; test non-null > null
        If other.IsNull AndAlso IsNull Then Return 0
        If other.IsNull Then Return 1
        If IsNull Then Return -1

        ' Otherwise, check their values against each other
        Select Case mDataType
            Case DataType.datetime, DataType.date, DataType.time
                Return Date.Parse(mValue).CompareTo(Date.Parse(other.mValue))

            Case DataType.number
                Return Decimal.Parse(mValue).CompareTo(Decimal.Parse(other.mValue))

            Case DataType.flag
                Return Boolean.Parse(mValue).CompareTo(Boolean.Parse(other.mValue))

            Case DataType.collection, DataType.password
                'Make no comparison
                Return 0

            Case DataType.timespan
                Return TimeSpan.Parse(mValue).CompareTo(TimeSpan.Parse(other.mValue))

            Case Else
                'Treat as text
                Return mValue.CompareTo(other.mValue)

        End Select

    End Function

    ''' <summary>
    ''' Tries to parse the value in this process value into a decimal, returning
    ''' success or otherwise as appropriate.
    ''' </summary>
    ''' <param name="dec">The decimal to which the parsed value should be saved.
    ''' </param>
    ''' <returns>True if the value held in this object was successfully parsed into
    ''' a decimal; False otherwise.</returns>
    Private Function TryParseDecimal(ByRef dec As Decimal) As Boolean
        If mValue Is Nothing Then dec = 0 : Return False
        Return Decimal.TryParse(mValue, dec) OrElse
         Decimal.TryParse(mValue, NumberStyles.Number, InternalCulture.Instance, dec)
    End Function

    ''' <summary>
    ''' Tries to parse the value in this process value into a date, returning success
    ''' or otherwise as appropriate.
    ''' </summary>
    ''' <param name="dt">The date variable to which the parsed date should be saved.
    ''' </param>
    ''' <returns>True if the value held in this object was succesfully parsed into a
    ''' date; False otherwise.</returns>
    Private Function TryParseDate(ByRef dt As Date) As Boolean
        If mValue Is Nothing Then dt = Date.MinValue : Return False
        If Date.TryParse(mValue, dt) Then Return True
        Return Date.TryParseExact(mValue,
         CollectionUtil.ToArray(Formats), InternalCulture.Instance, Nothing, dt)
    End Function

    ''' <summary>
    ''' Tries to cast this process value into the required type, returning true or
    ''' false on success
    ''' </summary>
    ''' <param name="tp">The type to cast this value into</param>
    ''' <param name="val">The reference param to store the result of the cast into.
    ''' </param>
    ''' <returns>True on success; False otherwise</returns>
    Public Function TryCastInto(ByVal tp As DataType, ByRef val As clsProcessValue) _
     As Boolean
        Return TryCastInto(tp, val, Nothing)
    End Function

    ''' <summary>
    ''' Tries to cast this process value into the required type, returning true or
    ''' false on success
    ''' </summary>
    ''' <param name="tp">The type to cast this value into</param>
    ''' <param name="val">The reference param to store the result of the cast into.
    ''' </param>
    ''' <param name="sErr">The message indicating any problems while attempting to
    ''' cast the data</param>
    ''' <returns>True on success; False otherwise</returns>
    Public Function TryCastInto(ByVal tp As DataType,
     ByRef val As clsProcessValue, ByRef sErr As String) As Boolean
        sErr = Nothing
        Try
            val = CastInto(tp)
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Casts this process value into a specific data type.
    ''' </summary>
    ''' <param name="tp">The type that this value should be cast into</param>
    ''' <returns>A process value of the required type cast from this value.</returns>
    ''' <exception cref="BadCastException">If this value could not be cast into the
    ''' required data type.</exception>
    Public Function CastInto(ByVal tp As DataType) As clsProcessValue
        ' If the types are simply incompatible, just quit now
        If Not CanCastInto(tp) Then
            ' Slightly different message if we're attempting to convert to unknown
            If tp = DataType.unknown Then Throw New BadCastException(
             My.Resources.Resources.clsProcessValue_CannotCastToUnknownType)

            Throw New BadCastException(My.Resources.Resources.clsProcessValue_NoConversionAvailableFrom0To1,
             GetFriendlyName(mDataType), GetFriendlyName(tp))
        End If

        ' If we're validating then we've already tested if the types are compatible,
        ' return success here and now
        ' If validateValue Then Return New clsProcessValue(tp)

        'If no conversion is necessary, then don't bother trying....
        If mDataType = tp Then Return Me

        Select Case mDataType
            Case DataType.text
                ' 'text' can be converted to lots of things
                Select Case tp
                    Case DataType.datetime, DataType.date, DataType.time
                        Dim dt As Date
                        If TryParseDate(dt) Then Return New clsProcessValue(tp, dt)

                        Throw New BadCastException(
                         My.Resources.Resources.clsProcessValue_TextValue0CannotBeInterpretedAsA1,
                         mValue, GetFriendlyName(tp))

                    Case DataType.password
                        Return New clsProcessValue(New SafeString(mValue))

                    Case DataType.number
                        Dim dec As Decimal
                        If TryParseDecimal(dec) Then Return dec

                        Throw New BadCastException(
                         My.Resources.Resources.clsProcessValue_UnableToCastValue0ToANumberDataItem, mValue)

                    Case DataType.flag
                        'Casting from an empty string should produce null
                        '(indeterminate) flag value. See bug 3036.
                        If mValue = "" Then Return New clsProcessValue(DataType.flag)
                        Dim flag As Boolean
                        If Boolean.TryParse(mValue, flag) Then Return flag

                        Throw New BadCastException(
                         My.Resources.Resources.clsProcessValue_UnableToParseValue0AsFlag, mValue)

                    Case DataType.timespan
                        ' Cast from empty string produces null-ish timespan, I
                        ' assume - as for flags above.
                        If mValue = "" Then Return New clsProcessValue(DataType.timespan)

                        Dim span As TimeSpan
                        If TimeSpan.TryParse(mValue, span) Then Return span

                        Throw New BadCastException(
                         My.Resources.Resources.clsProcessValue_UnableToParseValue0AsTimespan, mValue)

                End Select

            Case DataType.date
                ' The only valid 'toType's are text and datetime
                If tp = DataType.text Then
                    Return CDate(Me).ToShortDateString()
                ElseIf tp = DataType.datetime Then
                    ' Create a datetime with the same date, and set time to midnight
                    ' to ensure that the time info is ignored
                    Return New clsProcessValue(DataType.datetime, CDate(Me))
                End If

            Case DataType.time
                ' The only valid 'toType' is text
                If tp <> DataType.text Then Exit Select

                Return CDate(Me).ToShortTimeString()

            Case DataType.datetime
                ' The only valid 'toType's are text, date and time
                If tp = DataType.text Then
                    Return CDate(Me).ToString()
                ElseIf tp = DataType.date Then
                    Return New clsProcessValue(DataType.date, CDate(Me))
                ElseIf tp = DataType.time Then
                    Return New clsProcessValue(DataType.time, CDate(Me))
                End If

            Case DataType.password, DataType.number, DataType.timespan, DataType.flag
                ' The only valid 'toType' is text
                If tp = DataType.text Then Return CStr(Me)

        End Select

        Throw New BadCastException(
         My.Resources.Resources.clsProcessValue_InvalidCastOperationArgumentsFrom0To1Value2,
         GetFriendlyName(mDataType), GetFriendlyName(tp), CStr(Me))
        Return False

    End Function

    ''' <summary>
    ''' Tests if this process value can be cast into a specific data type, in theory.
    ''' Note that this doesn't test the value to see if it can be converted, only
    ''' that the data type is potentially convertible into the required type.
    ''' </summary>
    ''' <param name="tp">The required type to cast this value into</param>
    ''' <returns>True if this value can be cast into the given type, regardless of
    ''' its value; False otherwise.</returns>
    Public Function CanCastInto(ByVal tp As DataType) As Boolean
        Return clsProcessDataTypes.CanCast(mDataType, tp)
    End Function

#End Region

#Region " Helpers for translating to and from .NET data types "

    ''' <summary>
    ''' Gets the underlying value as a Date. This is only valid for process values
    ''' with a datatype of <see cref="AutomateProcessCore.DataType.date"/>. Calling for any other data
    ''' type will cause an exception to be thrown. Note that this includes a value
    ''' with a datatype of <see cref="AutomateProcessCore.DataType.datetime"/> - The datetime-specific
    ''' methods <see cref="GetValueAsLocalDateTime"/> and <see
    ''' cref="GetValueAsUTCDateTime"/> should be used instead for such values.
    ''' </summary>
    ''' <returns>The current value in this object as a Date, or Date.MinValue if
    ''' this value is currently 'null'</returns>
    ''' <exception cref="InvalidTypeException">If the datatype of this value is
    ''' anything other than 'date'.</exception>
    Public Function GetDateValue() As Date
        If mDataType <> DataType.date Then Throw New InvalidTypeException(
         My.Resources.Resources.clsProcessValue_TheDatatype0CannotBeConvertedIntoADate, mDataType)
        If mValue = "" Then Return Nothing ' effectively Date.MinValue
        Return Date.ParseExact(mValue, InternalDateFormat, Nothing)
    End Function

    ''' <summary>
    ''' Get the underlying value as a DateTime in UTC. This is only valid when the
    ''' datatype is a 'datetime'. Calling for any other
    ''' datatype will throw an exception.
    ''' </summary>
    ''' <returns>A DataTime containing the underlying value, or Nothing (ie.
    ''' <see cref="DateTime.MinValue"/>) if the underlying value is Null.</returns>
    ''' <exception cref="InvalidTypeException">If the data type of this value is
    ''' something other than <see cref="AutomateProcessCore.DataType.datetime"/></exception>
    ''' <remarks>This method assumes that the datetime value set in this object is in
    ''' UTC unless the <see cref="mbTreatAsText">Allow Invalid</see> flag was set
    ''' when the object was created. In that case, the assumption is that it is a
    ''' local datetime and it is converted accordingly</remarks>
    Public Function GetValueAsUTCDateTime() As DateTime
        If mDataType <> DataType.datetime Then Throw New InvalidTypeException(
         My.Resources.Resources.clsProcessValue_CannotGetValueAsUTCDateTimeWhenTypeIs0, mDataType)

        ' The conversion does exactly what this method used to do with the addition
        ' of allowing conversion of dates and times as well - we've already removed
        ' that extension in the above check, so we might as well just use the
        ' conversion operator to handle the rest
        Return CDate(Me)
    End Function

    ''' <summary>
    ''' Get the underlying value as a DateTime in Local Time. This is only valid when
    ''' the datatype is a 'datetime'. Calling for any other datatype will throw an
    ''' exception.
    ''' </summary>
    ''' <returns>A DataTime containing the underlying value, or Nothing (ie.
    ''' <see cref="DateTime.MinValue"/>) if the underlying value is Null.</returns>
    ''' <exception cref="InvalidTypeException">If the data type of this value is
    ''' something other than <see cref="AutomateProcessCore.DataType.datetime"/></exception>
    Public Function GetValueAsLocalDateTime() As DateTime
        If mDataType <> DataType.datetime Then Throw New InvalidTypeException(
         My.Resources.Resources.clsProcessValue_CannotGetValueAsLocalDateTimeWhenTypeIs0, mDataType)

        If mValue = "" Then Return Nothing
        Dim styles As DateTimeStyles = Nothing

        'For a 'treatastext' value, the developer probably passed a local time...
        If mbTreatAsText Then styles = DateTimeStyles.AssumeLocal
        Return DateTime.Parse(mValue, Nothing, styles)
    End Function

#End Region

#Region " Conversion Operators "

#Region " ProcessValue -> .NET types "

    ''' <summary>
    ''' Casts this process value into a string. This will attempt to cast any
    ''' </summary>
    ''' <param name="pv">The process value to cast</param>
    ''' <returns>A string representing the given process value.</returns>
    ''' <exception cref="InvalidCastException">If the given value did not have a
    ''' datatype capable of converting into a string - this means any of :- <list>
    ''' <item><see cref="AutomateProcessCore.DataType.binary"/></item>
    ''' <item><see cref="AutomateProcessCore.DataType.collection"/></item>
    ''' <item><see cref="AutomateProcessCore.DataType.image"/></item>
    ''' <item><see cref="AutomateProcessCore.DataType.unknown"/></item></list></exception>
    ''' <remarks>Note that if casting a date-related process value into a string, the
    ''' UTC date representation is what is returned in the string. To do otherwise,
    ''' the date specific retrieval methods should be used instead.</remarks>
    Public Shared Narrowing Operator CType(ByVal pv As clsProcessValue) As String
        If pv Is Nothing Then Return Nothing
        ' So... what datatypes *shouldn't* be able to be cast into a string?
        Select Case pv.DataType
            Case DataType.date : Return CDate(pv).ToString("d")
            Case DataType.datetime : Return CDate(pv).ToString("G")
            Case DataType.number : Return CDec(pv).ToString()

            Case DataType.flag,
             DataType.text,
             DataType.timespan,
             DataType.time
                Return pv.EncodedValue

            Case DataType.password
                Debug.Print(
                    "Need to fix this - " &
                    "make it less easy to get a string from a secure string")
                Return pv.Password.AsString()

            Case Else
                Throw New BadCastException(
                 My.Resources.Resources.clsProcessValue_CannotCastA0ValueIntoAString, pv.DataType)
        End Select
    End Operator


    ''' <summary>
    ''' Casts this process value into a date. Note that a null reference or a
    ''' process value with a null value is cast into an 'empty' date value - ie.
    ''' <see cref="DateTime.MinValue"/>.
    ''' </summary>
    ''' <param name="pv">The process value to cast</param>
    ''' <returns>The date value of the process value</returns>
    ''' <exception cref="InvalidCastException">If the given value did not have a
    ''' datatype of <see cref="AutomateProcessCore.DataType.datetime"/> or <see cref="AutomateProcessCore.DataType.date"/>
    ''' </exception>
    ''' <remarks>Note that the date/time returned is a UTC datetime. If the date
    ''' represents a local date/time the <see cref="GetValueAsLocalDateTime"/>
    ''' method should be used instead.</remarks>
    Public Shared Narrowing Operator CType(ByVal pv As clsProcessValue) As Date
        If pv Is Nothing Then Return Nothing

        If Not DateTimeTypes.Contains(pv.DataType) Then Throw New BadCastException(
         My.Resources.Resources.clsProcessValue_CannotConvertA0ValueIntoADate, pv.DataType)

        If pv.IsNull Then Return Nothing

        Dim styles As DateTimeStyles = DateTimeStyles.AdjustToUniversal

        'For a 'treatastext' value, the developer probably passed
        'a local time, so we have to convert it...
        If pv.mbTreatAsText _
         Then styles = styles Or DateTimeStyles.AssumeLocal _
         Else styles = styles Or DateTimeStyles.AssumeUniversal

        ' Get the appropriate format for the datatype and use that to parse our value
        Dim fmt As String
        Select Case pv.DataType
            Case DataType.date : fmt = InternalDateFormat
            Case DataType.datetime : fmt = InternalDateTimeFormat
            Case DataType.time : fmt = InternalTimeFormat
            Case Else
                Throw New BadCastException(
                My.Resources.Resources.clsProcessValue_CannotCastA0ValueIntoADate, pv.DataType)
        End Select
        Return DateTime.ParseExact(pv.mValue, fmt, Nothing, styles)

    End Operator

    ''' <summary>
    ''' Casts a process value into a timespan. A null reference or a process value
    ''' with a null value is cast into a 'zero' Timespan.
    ''' </summary>
    ''' <param name="pv">The value to cast into timespan</param>
    ''' <returns>A TimeSpan representing the same value as the process value.
    ''' </returns>
    Public Shared Narrowing Operator CType(ByVal pv As clsProcessValue) As TimeSpan
        If pv Is Nothing Then Return TimeSpan.Zero
        If pv.DataType <> DataType.timespan Then Throw New BadCastException(
         My.Resources.Resources.clsProcessValue_CannotCastA0ValueIntoATimespan, pv.DataType)

        If pv.EncodedValue = "" Then Return TimeSpan.Zero
        Return TimeSpan.Parse(pv.EncodedValue)
    End Operator

    ''' <summary>
    ''' Casts this process value into an integer. Note that a null reference or a
    ''' process value with a null value is cast into the integer value of 0.
    ''' </summary>
    ''' <param name="pv">The process value to cast</param>
    ''' <returns>The integer value of the process value</returns>
    ''' <exception cref="InvalidCastException">If the given value did not have a
    ''' datatype of <see cref="AutomateProcessCore.DataType.number"/></exception>
    ''' <remarks>This will lose the fractional part of the process value, if there is
    ''' one and will round to the nearest whole number, or the nearest even number if
    ''' the fractional part is exactly 0.5 - this is sometimes called "banker's
    ''' rounding".</remarks>
    Public Shared Narrowing Operator CType(ByVal pv As clsProcessValue) As Integer
        Try
            Return CInt(CDec(pv))
        Catch ex As BadCastException ' Just change the msg - "decimal" -> "integer"
            Throw New BadCastException(
               My.Resources.Resources.clsProcessValue_CannotCastA0ValueIntoAnInteger, pv.DataType)
        End Try
    End Operator

    ''' <summary>
    ''' Casts this process value into a decimal. Note that a null reference or a
    ''' process value with a null value is cast into the decimal value of 0.
    ''' </summary>
    ''' <param name="pv">The process value to cast</param>
    ''' <returns>The decimal value of the process value</returns>
    ''' <exception cref="InvalidCastException">If the given value did not have a
    ''' datatype of <see cref="AutomateProcessCore.DataType.number"/></exception>
    Public Shared Narrowing Operator CType(ByVal pv As clsProcessValue) As Decimal
        If pv Is Nothing Then Return 0
        If pv.DataType <> DataType.number Then Throw New BadCastException(
         My.Resources.Resources.clsProcessValue_CannotCastA0ValueIntoADecimal, pv.DataType)
        If pv.EncodedValue = "" _
         Then Return 0 _
         Else Return Decimal.Parse(pv.EncodedValue, InternalCulture.Instance)
    End Operator

    ''' <summary>
    ''' Casts this process value into a boolean. Note that a null reference or a
    ''' process value with a null value is cast into the boolean value of False.
    ''' </summary>
    ''' <param name="pv">The process value to cast</param>
    ''' <returns>The boolean value of the process value or False if the value is
    ''' empty.</returns>
    ''' <exception cref="InvalidCastException">If the given value did not have a
    ''' datatype of <see cref="AutomateProcessCore.DataType.flag"/></exception>
    Public Shared Narrowing Operator CType(ByVal pv As clsProcessValue) As Boolean
        If pv Is Nothing Then Return False
        If pv.DataType <> DataType.flag Then Throw New BadCastException(
         My.Resources.Resources.clsProcessValue_CannotCastA0ValueIntoABoolean, pv.DataType)
        Return (pv.EncodedValue <> "" AndAlso Boolean.Parse(pv.EncodedValue))
    End Operator

    ''' <summary>
    ''' Casts this process value into a Guid. Note that a null reference or a
    ''' process value with a null value is cast into the value of
    ''' <see cref="Guid.Empty"/>.
    ''' </summary>
    ''' <param name="pv">The process value to cast</param>
    ''' <returns>The Guid value of the process value</returns>
    ''' <exception cref="InvalidCastException">If the given value did not have a
    ''' datatype of <see cref="AutomateProcessCore.DataType.flag"/></exception>
    Public Shared Narrowing Operator CType(ByVal pv As clsProcessValue) As Guid
        If pv Is Nothing Then Return Guid.Empty
        If pv.DataType <> DataType.text Then Throw New BadCastException(
         My.Resources.Resources.clsProcessValue_CannotCastA0ValueIntoAGuid, pv.DataType)
        If pv.EncodedValue = "" Then Return Guid.Empty
        Return New Guid(pv.EncodedValue)
    End Operator

    ''' <summary>
    ''' Casts the process value into a bitmap. A null object or a process value with
    ''' a null value will always return null.
    ''' </summary>
    ''' <param name="pv">The process value to convert into a bitmap</param>
    ''' <returns>The bitmap represented by the given process value, or null if the
    ''' given value was null or its contained value was null.</returns>
    ''' <exception cref="InvalidCastException">If the given value's datatype was
    ''' anything other than <see cref="AutomateProcessCore.DataType.image"/></exception>
    Public Shared Narrowing Operator CType(ByVal pv As clsProcessValue) As Bitmap
        If pv Is Nothing Then Return Nothing
        If pv.DataType <> DataType.image Then Throw New BadCastException(
         My.Resources.Resources.clsProcessValue_CannotCastA0ValueIntoAnImage, pv.DataType)
        If pv.EncodedValue = "" Then Return Nothing
        Return clsPixRect.ParseBitmap(pv.EncodedValue)
    End Operator

    ''' <summary>
    ''' Casts the process value into a byte array. A null object or a process value
    ''' with a null value will always return null.
    ''' </summary>
    ''' <param name="pv">The value to cast into a byte array</param>
    ''' <returns>A byte array from the given value or null if the value was null.
    ''' </returns>
    ''' <remarks>
    ''' Note that this array is the one that is held in this process value, meaning
    ''' that alterations made to the data within this array will be reflected in
    ''' the given value object.
    ''' </remarks>
    ''' <exception cref="InvalidCastException">If the given value's datatype was
    ''' anything other than <see cref="AutomateProcessCore.DataType.binary"/></exception>
    Public Shared Narrowing Operator CType(ByVal pv As clsProcessValue) As Byte()
        If pv Is Nothing Then Return Nothing
        If pv.DataType <> DataType.binary Then Throw New BadCastException(
         My.Resources.Resources.clsProcessValue_CannotCastA0ValueIntoAByteArray, pv.DataType)
        Return pv.mBinaryValue
    End Operator

    ''' <summary>
    ''' Casts the process value into a secure string. This ensures that the data in
    ''' this process value does not enter managed memory in the transition from its
    ''' internal form into a secure string.
    ''' </summary>
    ''' <param name="pv">The value to cast into a secure string</param>
    ''' <returns>A secure string containing the password content of the given value.
    ''' </returns>
    ''' <exception cref="InvalidCastException">If the given value's datatype was
    ''' anything other than <see cref="AutomateProcessCore.DataType.password"/></exception>
    Public Shared Narrowing Operator CType(ByVal pv As clsProcessValue) As SafeString
        If pv Is Nothing Then Return Nothing
        If pv.DataType <> DataType.password Then Throw New BadCastException(
         My.Resources.Resources.clsProcessValue_CannotCastA0ValueIntoASecureString, pv.DataType)
        Return If(pv.Password Is Nothing, Nothing, pv.Password.Copy())
    End Operator

#End Region

#Region " .NET types -> ProcessValue "

    ''' <summary>
    ''' Converts the given string into a <see cref="AutomateProcessCore.DataType.text">text</see>
    ''' process value
    ''' </summary>
    ''' <param name="str">The value to convert into a process value</param>
    ''' <returns>The process value representing the given value.</returns>
    Public Shared Widening Operator CType(ByVal str As String) As clsProcessValue
        Return New clsProcessValue(DataType.text, str)
    End Operator

    ''' <summary>
    ''' Converts the given date into a <see cref="AutomateProcessCore.DataType.datetime">datetime</see>
    ''' process value
    ''' </summary>
    ''' <param name="dt">The value to convert into a process value</param>
    ''' <returns>The process value representing the given value.</returns>
    ''' <remarks>Note that the date is converted to UTC when stored in the process
    ''' value. It will assume that the date is already a UTC date unless the
    ''' <see cref="DateTime.Kind"/> on the passed in value is set to
    ''' <see cref="DateTimeKind.Local"/></remarks>
    Public Shared Widening Operator CType(ByVal dt As Date) As clsProcessValue
        ' Treat 'MinValue' as a null datetime
        If dt = Date.MinValue Then Return New clsProcessValue(DataType.datetime, "")
        Return New clsProcessValue( _
         DataType.datetime, dt, dt.Kind = DateTimeKind.Local)
    End Operator

    ''' <summary>
    ''' Converts the given integer into a <see cref="AutomateProcessCore.DataType.number">number</see>
    ''' process value
    ''' </summary>
    ''' <param name="num">The value to convert into a process value</param>
    ''' <returns>The process value representing the given value.</returns>
    Public Shared Widening Operator CType(ByVal num As Integer) As clsProcessValue
        Return New clsProcessValue(num)
    End Operator

    ''' <summary>
    ''' Converts the given decimal into a <see cref="AutomateProcessCore.DataType.number">number</see>
    ''' process value
    ''' </summary>
    ''' <param name="num">The value to convert into a process value</param>
    ''' <returns>The process value representing the given value.</returns>
    Public Shared Widening Operator CType(ByVal num As Decimal) As clsProcessValue
        Return New clsProcessValue(num)
    End Operator

    ''' <summary>
    ''' Converts the given double into a <see cref="AutomateProcessCore.DataType.number">number</see>
    ''' process value
    ''' </summary>
    ''' <param name="num">The value to convert into a process value</param>
    ''' <returns>The process value representing the given value.</returns>
    Public Shared Widening Operator CType(ByVal num As Double) As clsProcessValue
        Return New clsProcessValue(num)
    End Operator

    ''' <summary>
    ''' Converts the given boolean into a <see cref="AutomateProcessCore.DataType.flag">flag</see>
    ''' process value
    ''' </summary>
    ''' <param name="flag">The value to convert into a process value</param>
    ''' <returns>The process value representing the given value.</returns>
    Public Shared Widening Operator CType(ByVal flag As Boolean) As clsProcessValue
        Return New clsProcessValue(flag)
    End Operator

    ''' <summary>
    ''' Converts the given timespan into a
    ''' <see cref="AutomateProcessCore.DataType.timespan">timespan</see> process value
    ''' </summary>
    ''' <param name="span">The value to convert into a process value</param>
    ''' <returns>The process value representing the given value.</returns>
    Public Shared Widening Operator CType(ByVal span As TimeSpan) As clsProcessValue
        Return New clsProcessValue(span)
    End Operator

    ''' <summary>
    ''' Converts the given Guid into a <see cref="AutomateProcessCore.DataType.text">text</see>
    ''' process value
    ''' </summary>
    ''' <param name="id">The value to convert into a process value</param>
    ''' <returns>The process value representing the given value.</returns>
    Public Shared Widening Operator CType(ByVal id As Guid) As clsProcessValue
        Return New clsProcessValue(id)
    End Operator

    ''' <summary>
    ''' Converts the given bitmap into a <see cref="AutomateProcessCore.DataType.image">image</see>
    ''' process value
    ''' </summary>
    ''' <param name="img">The value to convert into a process value</param>
    ''' <returns>The process value representing the given value.</returns>
    Public Shared Widening Operator CType(ByVal img As Bitmap) As clsProcessValue
        Return New clsProcessValue(img)
    End Operator

    ''' <summary>
    ''' Converts the given array into a <see cref="AutomateProcessCore.DataType.binary">binary</see>
    ''' process value
    ''' </summary>
    ''' <param name="arr">The value to convert into a process value</param>
    ''' <returns>The process value representing the given value.</returns>
    Public Shared Widening Operator CType(ByVal arr() As Byte) As clsProcessValue
        Return New clsProcessValue(arr)
    End Operator

    ''' <summary>
    ''' Converts the given collection into a
    ''' <see cref="AutomateProcessCore.DataType.collection">collection</see> process value
    ''' </summary>
    ''' <param name="coll">The value to convert into a process value</param>
    ''' <returns>The process value representing the given value.</returns>
    Public Shared Widening Operator CType(ByVal coll As clsCollection) _
     As clsProcessValue
        Return New clsProcessValue(coll)
    End Operator

    ''' <summary>
    ''' Converts the given data table into a
    ''' <see cref="AutomateProcessCore.DataType.collection">collection</see> process value
    ''' </summary>
    ''' <param name="data">The value to convert into a process value</param>
    ''' <returns>The process value representing the given value.</returns>
    Public Shared Widening Operator CType(ByVal data As DataTable) As clsProcessValue
        Return New clsProcessValue(data)
    End Operator

    ''' <summary>
    ''' Casts the given secure string into a
    ''' <see cref="AutomateProcessCore.DataType.password">password</see> process value.
    ''' </summary>
    ''' <param name="ss">The value to convert into a process value</param>
    ''' <returns>The process value representing the given value.</returns>
    Public Shared Widening Operator CType(ByVal ss As SafeString) As clsProcessValue
        Return New clsProcessValue(ss)
    End Operator


#End Region

#End Region

End Class
