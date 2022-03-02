Imports System.Collections.ObjectModel
Imports System.Drawing

Imports BluePrism.BPCoreLib.Collections

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsProcessDataTypes
''' 
''' <summary>
''' A class with shared members only, providing information about
''' the available data types.
''' </summary>
Public Class clsProcessDataTypes

    ''' <summary>
    ''' The date/time data types within Blue Prism
    ''' </summary>
    Public Shared ReadOnly DateTimeTypes As ICollection(Of DataType) = _
     GetReadOnly.ICollectionFrom(DataType.date, DataType.datetime, DataType.time)

    ''' <summary>
    ''' Map containing information about all known data types.
    ''' </summary>
    Private Shared mAllTypes As IDictionary(Of String, clsDataTypeInfo) = CreateTypeInfos()



    ''' <summary>
    ''' Rebuild the TypeInfo in case of a locale change
    ''' </summary>
    Public Shared Sub RebuildTypeInfo()

        mAllTypes = CreateTypeInfos()

    End Sub

    ''' <summary>
    ''' Creates the data type info dictionary used throughough this class.
    ''' </summary>
    ''' <returns>The dictionary of DataTypeInfo objects mapped against their
    ''' names. Once returned from this method, the dictionary cannot be modified.
    ''' Attempting to do so will cause an exception to be thrown.
    ''' </returns>
    Private Shared Function CreateTypeInfos() As IDictionary(Of String, clsDataTypeInfo)
        Dim map As New clsOrderedDictionary(Of String, clsDataTypeInfo)

        map("collection") = New clsDataTypeInfo(
         DataType.collection, clsDataTypeInfo.GetLocalizedFriendlyName(DataType.collection), clsDataTypeInfo.GetLocalizedFriendlyName(DataType.collection, True), True, False, String.Format(
         My.Resources.Resources.clsProcessDataTypes_CollectionsAreUsedToStoreManyPiecesOfInformationAtOnce00ForExampleThisMayBeALis, vbCrLf)
        )

        map("date") = New clsDataTypeInfo(
         DataType.date, clsDataTypeInfo.GetLocalizedFriendlyName(DataType.date), clsDataTypeInfo.GetLocalizedFriendlyName(DataType.date, True), String.Format(
         My.Resources.Resources.clsProcessDataTypes_DateItemsAreUsedToRecordADateInTheYearValuesMayRangeFrom0To122TheyAreSimilarToD,
         Date.MinValue.ToShortDateString(), Date.MaxValue.ToShortDateString(), vbCrLf)
        )

        map("datetime") = New clsDataTypeInfo(
         DataType.datetime, clsDataTypeInfo.GetLocalizedFriendlyName(DataType.datetime), clsDataTypeInfo.GetLocalizedFriendlyName(DataType.datetime, True), String.Format(
         My.Resources.Resources.clsProcessDataTypes_DateTimeItemsAreUsedToStoreDateAndTimeDataValuesMayRangeFrom0To122TheyAreSimila,
         Date.MinValue, Date.MaxValue, vbCrLf)
        )

        map("flag") = New clsDataTypeInfo(
         DataType.flag, clsDataTypeInfo.GetLocalizedFriendlyName(DataType.flag), clsDataTypeInfo.GetLocalizedFriendlyName(DataType.flag, True), String.Format(
         My.Resources.Resources.clsProcessDataTypes_AFlagItemCanStoreOneOfTwoValuesTrueOrFalse00TypicallyAFlagItemMayBeUsedToRecord, vbCrLf)
        )

        map("number") = New clsDataTypeInfo(
         DataType.number, clsDataTypeInfo.GetLocalizedFriendlyName(DataType.number), clsDataTypeInfo.GetLocalizedFriendlyName(DataType.number, True), String.Format(
         My.Resources.Resources.clsProcessDataTypes_NumberItemsAreUsedToStoreNumericValues00ForExampleThisMayBeAnAccountBalanceOrTh, vbCrLf)
        )

        map("password") = New clsDataTypeInfo(
         DataType.password, clsDataTypeInfo.GetLocalizedFriendlyName(DataType.password), clsDataTypeInfo.GetLocalizedFriendlyName(DataType.password, True), String.Format(
         My.Resources.Resources.clsProcessDataTypes_PasswordItemsAreUsedToStoreSensitiveData00AnyInformationTypedIntoAPasswordField,
         vbCrLf)
        )

        map("text") = New clsDataTypeInfo(
         DataType.text, clsDataTypeInfo.GetLocalizedFriendlyName(DataType.text), clsDataTypeInfo.GetLocalizedFriendlyName(DataType.text, True), String.Format(
         My.Resources.Resources.clsProcessDataTypes_TextItemsAreUsedToStoreAlphanumericData00ThisInformationMayBeNamesAddressesTele, vbCrLf)
        )

        map("time") = New clsDataTypeInfo(
         DataType.time, clsDataTypeInfo.GetLocalizedFriendlyName(DataType.time), clsDataTypeInfo.GetLocalizedFriendlyName(DataType.time, True), String.Format(
         My.Resources.Resources.clsProcessDataTypes_TimeItemsAreUsedToStoreTimeDataValuesCanRangeFrom0To1TheyAreSimilarToDateTimeIt,
         Date.MinValue.ToLongTimeString(), Date.MaxValue.ToLongTimeString(), vbCrLf)
        )

        map("timespan") = New clsDataTypeInfo(
         DataType.timespan, clsDataTypeInfo.GetLocalizedFriendlyName(DataType.timespan), clsDataTypeInfo.GetLocalizedFriendlyName(DataType.timespan, True), String.Format(
         My.Resources.Resources.clsProcessDataTypes_TimeSpanItemsAreUsedToStoreLengthsOfTimeInDaysHoursMinutesAndSeconds00ForExampl, vbCrLf)
        )

        map("image") = New clsDataTypeInfo(
         DataType.image, clsDataTypeInfo.GetLocalizedFriendlyName(DataType.image), clsDataTypeInfo.GetLocalizedFriendlyName(DataType.image, True), True, False,
         My.Resources.Resources.clsProcessDataTypes_ImageItemsAreUsedToStoreImages)

        map("binary") = New clsDataTypeInfo(
         DataType.binary, clsDataTypeInfo.GetLocalizedFriendlyName(DataType.binary), clsDataTypeInfo.GetLocalizedFriendlyName(DataType.binary, True), True, False,
         My.Resources.Resources.clsProcessDataTypes_BinaryItemsAreUsedToStoreBinaryData)

        map("unknown") = New clsDataTypeInfo(
         DataType.unknown, clsDataTypeInfo.GetLocalizedFriendlyName(DataType.unknown), clsDataTypeInfo.GetLocalizedFriendlyName(DataType.unknown, True), False, False,
         My.Resources.Resources.clsProcessDataTypes_UnknownIsTheNameGivenToTheTypeOfADataItemWhenNoTypeHasYetBeenSetForTheItem
        )

        Return GetReadOnly.IDictionary(map)

    End Function

    ''' <summary>
    ''' Maps Blue Prism data types onto their framework equivalents.
    ''' </summary>
    Private Shared ReadOnly sDataTypeMap As IDictionary(Of Type, DataType) = InitDataTypeMap()

    ''' <summary>
    ''' Initialises the data type map, mapping BP data types onto system types.
    ''' </summary>
    ''' <returns>A read-only map containing BP types keyed against the equivalent
    ''' system types.</returns>
    Private Shared Function InitDataTypeMap() As IDictionary(Of Type, DataType)

        'All .Net Built in types should be accounted for here 
        'see http://msdn.microsoft.com/en-us/library/ya5y69ds.aspx
        'Note char and char arrays are considered here as text.
        Dim map As New Dictionary(Of Type, DataType)

        ' Flags...
        map(GetType(Boolean)) = DataType.flag

        ' Numbers...
        map(GetType(Decimal)) = DataType.number
        map(GetType(Single)) = DataType.number
        map(GetType(Double)) = DataType.number
        map(GetType(Byte)) = DataType.number
        map(GetType(SByte)) = DataType.number
        map(GetType(Int16)) = DataType.number
        map(GetType(UInt16)) = DataType.number
        map(GetType(Int32)) = DataType.number
        map(GetType(UInt32)) = DataType.number
        map(GetType(Int64)) = DataType.number
        map(GetType(UInt64)) = DataType.number

        ' Date/time
        map(GetType(Date)) = DataType.datetime
        map(GetType(TimeSpan)) = DataType.timespan

        ' Text
        map(GetType(String)) = DataType.text
        map(GetType(Char)) = DataType.text
        map(GetType(Char())) = DataType.text

        ' Bitmap
        map(GetType(Bitmap)) = DataType.image

        ' Binary
        map(GetType(Byte())) = DataType.binary

        ' Collection
        map(GetType(DataTable)) = DataType.collection

        Return GetReadOnly.IDictionary(map)

    End Function

    ' A map of each convertible datatype against all the datatypes that values of
    ' that type can theoretically be converted into, notwithstanding state/formatting
    ' constraints and not including itself
    Private Shared sValidConversionMap As _
     IDictionary(Of DataType, ICollection(Of DataType)) = InitConversionMap()

    ''' <summary>
    ''' Generates the valid conversions from the appropriate datatypes. The returned
    ''' map does not map datatype values against themselves - eg. the collection
    ''' against DataType.text does not include DataType.text. Also, any datatype
    ''' which is not represented as a key in this collection cannot be converted into
    ''' any other data type.
    ''' </summary>
    ''' <returns>A map, keyed on datatype, containing the other datatypes to which
    ''' values of the key type can be converted.</returns>
    Private Shared Function InitConversionMap() _
     As IDictionary(Of DataType, ICollection(Of DataType))
        Dim map As New Dictionary(Of DataType, ICollection(Of DataType))

        map(DataType.text) = CompileTypes(DataType.datetime, DataType.date,
         DataType.time, DataType.timespan, DataType.password, DataType.number,
         DataType.flag)

        map(DataType.password) = CompileTypes(DataType.text)

        map(DataType.number) = CompileTypes(DataType.text)

        map(DataType.date) = CompileTypes(DataType.text, DataType.datetime)

        map(DataType.time) = CompileTypes(DataType.text)

        map(DataType.datetime) = CompileTypes(DataType.text, DataType.date, DataType.time)

        map(DataType.timespan) = CompileTypes(DataType.text)

        map(DataType.flag) = CompileTypes(DataType.text)

        Return GetReadOnly.IDictionary(map)

    End Function

    ''' <summary>
    ''' Compiles a paramarray of datatypes into a readonly set.
    ''' </summary>
    ''' <param name="types"></param>
    ''' <returns>The given array of datatypes wrapped into a read-only set.</returns>
    Private Shared Function CompileTypes(ByVal ParamArray types() As DataType) _
     As ICollection(Of DataType)
        Return GetReadOnly.ISet(New clsSet(Of DataType)(types))
    End Function

    ''' <summary>
    ''' Get the friendly name for the given data type
    ''' </summary>
    ''' <param name="sName">The internal data type name</param>
    ''' <param name="bPlural">True to return plural form</param>
    ''' <param name="bLocalize">True to return localized form</param>
    ''' <returns>The friendly name, or "ERROR" if an invalid type
    ''' was passed in.</returns>
    Public Shared Function GetFriendlyName(ByVal sName As String, Optional ByVal bPlural As Boolean = False, Optional ByVal bLocalize As Boolean = True) As String
        Dim dt As clsDataTypeInfo = Nothing
        If mAllTypes.TryGetValue(sName.ToLower(), dt) Then
            If (bLocalize) Then
                Return clsDataTypeInfo.GetLocalizedFriendlyName(dt.Value, bPlural)
            Else
                Return clsDataTypeInfo.GetNeutralFriendlyName(dt.Value, bPlural)
            End If
        End If
        Return "ERROR"
    End Function

    ''' <summary>
    ''' Get the friendly name for the given data type
    ''' </summary>
    ''' <param name="dtDataType">The internal data type name</param>
    ''' <param name="bPlural">True to return plural form</param>
    ''' <param name="bLocalize">True to return localized form</param>
    ''' <returns>The friendly name, or "ERROR" if an invalid type
    ''' was passed in.</returns>
    Public Shared Function GetFriendlyName(ByVal dtDataType As DataType, Optional ByVal bPlural As Boolean = False, Optional ByVal bLocalize As Boolean = True) As String
        Return GetFriendlyName(dtDataType.ToString(), bPlural, bLocalize)
    End Function

    ''' <summary>
    ''' Get the internal name of a data type, given the friendly name.
    ''' </summary>
    ''' <param name="sFriendlyName">A data type friendly name</param>
    ''' <returns>The internal name of the data type, or "ERROR" if
    ''' no match was found.</returns>
    Public Shared Function GetName(ByVal sFriendlyName As String) As String
        For Each dt As clsDataTypeInfo In mAllTypes.Values
            If dt.FriendlyName = sFriendlyName Then Return dt.Name
        Next
        Return "ERROR"
    End Function

    ''' <summary>
    ''' Gets the data type info object representing the given data type.
    ''' </summary>
    ''' <param name="dt">The data type for which the info is required.</param>
    ''' <returns>The data type info for the given type</returns>
    Public Shared Function GetInfo(ByVal dt As DataType) As clsDataTypeInfo
        For Each dti As clsDataTypeInfo In mAllTypes.Values
            If dti.Value = dt Then Return dti
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Get a collection of all the possible data types. The collection cannot be
    ''' modified.
    ''' </summary>
    ''' <returns>A reference to a list of clsDataTypeInfo instances.</returns>
    Public Shared Function GetAll() As ICollection(Of clsDataTypeInfo)
        Return mAllTypes.Values
    End Function

    ''' <summary>
    ''' Gets the public scalar data types - effectively, all public data types
    ''' except for 'collection'
    ''' </summary>
    ''' <returns>A collection of the public scalar data types held in this class.
    ''' </returns>
    Public Shared Function GetPublicScalars() As ICollection(Of clsDataTypeInfo)
        Return CollectionUtil.Filter(GetAll(), AddressOf IsPublicScalar)
    End Function

    ''' <summary>
    ''' Predicate to determine if the given data type info represents a public
    ''' scalar data type or not
    ''' </summary>
    ''' <param name="dt">The data type info to test</param>
    ''' <returns>True if the given object is a non-null info object
    ''' representing a public scalar data type.</returns>
    Private Shared Function IsPublicScalar(ByVal dt As clsDataTypeInfo) As Boolean
        Return (dt IsNot Nothing _
         AndAlso dt.IsPublic AndAlso dt.Value <> DataType.collection)
    End Function

    ''' <summary>
    ''' Tests if it is possible to case from one data type into another, disregarding
    ''' any formatting requirements or state constraints.
    ''' </summary>
    ''' <param name="fromType">The type casting from</param>
    ''' <param name="toType">The type casting to</param>
    ''' <returns>True if it is at all possible to cast from the 'from' type to the
    ''' 'to' type.</returns>
    Friend Shared Function CanCast(
     ByVal fromType As DataType, ByVal toType As DataType) As Boolean
        If fromType = toType Then Return True
        Dim types As ICollection(Of DataType) = Nothing

        Return sValidConversionMap.TryGetValue(fromType, types) _
         AndAlso types.Contains(toType)
    End Function

    ''' <summary>
    ''' Names of possible data types.
    ''' </summary>
    Public Shared ReadOnly DataTypeNames As ICollection(Of String) =
     New ReadOnlyCollection(Of String)(New String() {
      "collection",
      "date",
      "datetime",
      "flag",
      "number",
      "password",
      "text",
      "time",
      "timespan",
      "image",
      "binary",
      "unknown"
     })

    ''' <summary>
    ''' Parses the given data type name. Note that the parse is case-insensitive.
    ''' </summary>
    ''' <param name="name">The name of the required datatype</param>
    ''' <returns>The DataType corresponding to the given name.</returns>
    ''' <remarks>This is ostensibly the same as DataTypeId(), which used to use
    ''' [Enum].Parse() - this is fine, but we have them all in a dictionary now,
    ''' so we might as well use it - it's much faster.</remarks>
    ''' <exception cref="InvalidDataTypeException">If no datatype with the given name
    ''' was found.</exception>
    Public Shared Function Parse(ByVal name As String) As DataType
        Dim dt As DataType = Nothing
        If Not TryParse(name, dt) Then Throw New InvalidDataTypeException(
         My.Resources.Resources.clsProcessDataTypes_NoDatatypeFoundWithTheName0, name)
        Return dt
    End Function

    ''' <summary>
    ''' Attempts to parse the given datatype name. Note that the parse is case-
    ''' insensitive.
    ''' </summary>
    ''' <param name="name">The name of the required datatype</param>
    ''' <param name="dt">On success, the datatype parsed from the given name
    ''' </param>
    ''' <returns>True if the name was parsed into a datatype successfully, false
    ''' if the name was not recognised as being a datatype</returns>
    Public Shared Function TryParse(ByVal name As String, ByRef dt As DataType) As Boolean
        Dim dti As clsDataTypeInfo = Nothing
        If mAllTypes.TryGetValue(name.ToLower(), dti) Then dt = dti.Value : Return True
        Return False
    End Function

    ''' <summary>
    ''' Get the ID of a datatype, given its name.
    ''' </summary>
    ''' <param name="name">The name of the data type. The only valid
    ''' values for this are those returned in the DataTypeNames()
    ''' property.</param>
    ''' <returns>The internal ID of the data type.</returns>
    ''' <remarks>
    ''' Throws an ApplicationException if an invalid data type name is given.
    ''' </remarks>
    Public Shared Function DataTypeId(ByVal name As String) As DataType
        Return Parse(name)
    End Function

    ''' <summary>
    ''' Gets a friendly tip describing the nature and usage of a datatype.
    ''' </summary>
    ''' <param name="dtDataType">The datatype of interest.</param>
    ''' <returns>Returns a string describing in simple terms what the datatype
    ''' is used for. If the datatype represents an undefined value, an empty
    ''' string is returned.</returns>
    Public Shared Function GetFriendlyDescription(ByVal dtDataType As DataType) As String
        Dim dt As clsDataTypeInfo = Nothing
        If mAllTypes.TryGetValue(dtDataType.ToString(), dt) Then Return dt.FriendlyDescription
        Return ""
    End Function

    ''' <summary>
    ''' Gets the corresponding .NET framework data type from the
    ''' Automate data type.
    ''' </summary>
    ''' <param name="dt">The data type to translate.</param>
    ''' <returns>Returns a type corresponding to the supplied Automate data type.
    ''' This may be nothing, since a  correspondence does not exist.</returns>
    ''' <remarks>See also GetFrameworkIncompatibleDataTypes</remarks>
    Public Shared Function GetFrameworkEquivalentFromDataType(ByVal dt As DataType) As Type
        Select Case dt
            Case DataType.collection
                Return GetType(DataTable)
            Case DataType.date
                Return GetType(System.DateTime)
            Case DataType.datetime
                Return GetType(DateTime)
            Case DataType.flag
                Return GetType(Boolean)
            Case DataType.number
                Return GetType(Decimal)
            Case DataType.text, DataType.password
                Return GetType(String)
            Case DataType.time
                Return GetType(System.DateTime)
            Case DataType.timespan
                Return GetType(TimeSpan)
            Case DataType.image
                Return GetType(Bitmap)
            Case DataType.binary
                Return GetType(Byte())
            Case DataType.unknown
                Return Nothing
            Case Else
                Throw New InvalidOperationException(My.Resources.Resources.clsProcessDataTypes_UnrecognisedDataType)
        End Select
    End Function

    ''' <summary>
    ''' Gets the datatype equivalent for the given system type.
    ''' </summary>
    ''' <param name="tp">The type for which the Blue Prism equivalent is required
    ''' </param>
    ''' <returns>The data type corresponding to the given system type.</returns>
    Public Shared Function GetDataTypeFromFrameworkEquivalent(ByVal tp As Type) As DataType
        Dim dt As DataType = DataType.unknown
        sDataTypeMap.TryGetValue(tp, dt)
        Return dt
    End Function

    ''' <summary>
    ''' Returns those data types which cannot be mapped 
    ''' to a .NET reference type.
    ''' </summary>
    ''' <returns>Returns all such data types, ORed together.</returns>
    ''' <remarks>See also GetFrameworkEquivalentFromDataType</remarks>
    Public Shared Function GetFrameworkIncompatibleDataTypes() As DataType
        Return DataType.unknown
    End Function

    ''' <summary>
    ''' Determines whether the supplied data type can be used as a statistic.
    ''' </summary>
    ''' <param name="dt">The data type of interest.</param>
    ''' <returns>Returns true if the data type can be used as a statistic;
    ''' false otherwise.</returns>
    Public Shared Function IsStatisticCompatible(ByVal dt As DataType) As Boolean
        Dim MatchingInfo As clsDataTypeInfo = Nothing
        For Each dti As clsDataTypeInfo In GetAll()
            If dti.Name = dt.ToString Then
                MatchingInfo = dti
            End If
        Next

        If MatchingInfo IsNot Nothing Then
            If MatchingInfo.IsPublic Then
                If MatchingInfo.IsStatisticCompatible Then Return True
            End If
        End If

        Return False
    End Function
End Class
