''' Project  : BPCoreLib
''' Class    : clsEnum
''' 
''' <summary>
''' Really useful class that makes parsing of enums much easier
''' 
''' example usage:
''' 
''' Dim wd as WeekDay = clsEnum(Of WeekDay).Parse("Monday",False)
''' 
''' or
''' 
''' Dim wd as WeekDay
''' If clsEnum(Of WeekDay).TryParse("September", wd) Then
'''   Console.Write("Valid")
''' Else
'''   Console.Write("Invalid")
''' End If
''' 
''' Another method uses the non-generic clsEnum class (see the bottom of this
''' file). It infers the type from the output result parameter resulting in
''' cleaner calling code.
''' 
''' Dim wd as WeekDay
''' If clsEnum.TryParse("Fish", wd) Then
'''   Console.WriteLine("Valid")
''' Else
'''   Console.WriteLine("Invalid")
''' End If
''' 
''' </summary>
''' <typeparam name="T">The type of Enum you want to parse.</typeparam>
Public Class clsEnum(Of T)

    ''' <summary>
    ''' A dictionary for case sensitive lookups
    ''' </summary>
    Private Shared mCaseSensMap As Dictionary(Of String, T)

    ''' <summary>
    ''' A dictionary for case in-sensitive lookups
    ''' </summary>
    Private Shared mCaseInsensMap As Dictionary(Of String, T)

    ''' <summary>
    ''' Shared method constructor builds up some dictionaries for quick lookups
    ''' </summary>
    Shared Sub New()
        mCaseInsensMap = New Dictionary(Of String, T)(StringComparer.OrdinalIgnoreCase)
        mCaseSensMap = New Dictionary(Of String, T)
        For Each enumName As String In [Enum].GetNames(GetType(T))
            Dim value As T = DirectCast([Enum].Parse(GetType(T), enumName), T)
            mCaseSensMap.Add(enumName, value)
            mCaseInsensMap.Add(enumName, value)
        Next
    End Sub

    ''' <summary>
    ''' Parses the given enum value - the value must exactly (including case) match
    ''' an enum value of the underlying enum type.
    ''' </summary>
    ''' <param name="value">The string value to parse</param>
    ''' <returns>The enum value corresponding to the given string.</returns>
    ''' <exception cref="ArgumentNullException">If the given value was null.
    ''' </exception>
    ''' <exception cref="ArgumentException">If the given value was an empty string or
    ''' a name which did not correspond to a name on the enum.</exception>
    Public Shared Function Parse(ByVal value As String) As T
        Return Parse(value, False)
    End Function

    ''' <summary>
    ''' Parses the given enum value, ignoring case as specified.
    ''' </summary>
    ''' <param name="value">The string value to parse</param>
    ''' <param name="ignorecase">Whether to ignore the case.</param>
    ''' <returns>The enum value corresponding to the specified name</returns>
    ''' <exception cref="ArgumentNullException">If the given value was null.
    ''' </exception>
    ''' <exception cref="ArgumentException">If the given value was an empty string or
    ''' a name which did not correspond to a name on the enum.</exception>
    ''' <remarks>This just uses the underlying <see cref="[Enum].Parse"/> method -
    ''' see that for any further implementation details</remarks>
    Public Shared Function Parse( _
     ByVal value As String, ByVal ignorecase As Boolean) As T
        Return CType([Enum].Parse(GetType(T), value, ignorecase), T)
    End Function

    ''' <summary>
    ''' Attempts to parse the given value into an enum case sensitively; returns a
    ''' flag indicating success.
    ''' </summary>
    ''' <param name="value">The value to parse</param>
    ''' <param name="result">When this method returns, the value associated with the
    ''' specified string, if the key is found; otherwise, the default value for the
    ''' type of the value parameter. This parameter is passed uninitialized.</param>
    ''' <returns>True if the given string was successfully parsed, false if it did
    ''' not match any of the values defined in the enum represented by this class.
    ''' </returns>
    Public Shared Function TryParse(ByVal value As String, ByRef result As T) As Boolean
        Return TryParse(value, False, result)
    End Function

    ''' <summary>
    ''' Attempts to parse the given enum value.
    ''' </summary>
    ''' <param name="value">The string value to parse</param>
    ''' <param name="ignorecase">Whether to ignore the case.</param>
    ''' <param name="result">When this method returns, the value associated with the
    ''' specified string, if the key is found; otherwise, the default value for the
    ''' type of the value parameter. This parameter is passed uninitialized.</param>
    ''' <returns>True If successful, False if not</returns>
    Public Shared Function TryParse( _
     ByVal value As String, ByVal ignorecase As Boolean, ByRef result As T) As Boolean
        If value Is Nothing Then
            result = Nothing ' effectively like C#'s "result = default(TEnum);"
            Return False
        End If
        Dim map As IDictionary(Of String, T)
        If ignorecase Then map = mCaseInsensMap Else map = mCaseSensMap
        Return map.TryGetValue(value, result)
    End Function

End Class

''' <summary>
''' An equivalent of clsEnum(Of T) which has the generic specification on the
''' TryParse method rather than the class which makes for cleaner calling code.
''' 
''' There's no benefit on doing the same on the Parse() method itself, since
''' the type must still be specified and it just adds unnecessary complexity and
''' makes it slower. Thus, only the TryParse method has been implemented here.
''' </summary>
Public Class clsEnum

    ''' <summary>
    ''' Parses the given string value into the required enum, returning the specified
    ''' value if the parsing fails.
    ''' </summary>
    ''' <typeparam name="TEnum">The type of enum to be parsed into</typeparam>
    ''' <param name="str">The string to parse</param>
    ''' <param name="valIfFails">The value to return if the parsing fails</param>
    ''' <returns>The enum value attained by parsing the given string</returns>
    ''' <exception cref="ArgumentException">If the type being parsed is not an
    ''' enumeration type.</exception>
    ''' <remarks>The string parsing is done case sensitively in this method - in such
    ''' a way that, when parsing "right" into a <see cref="Direction"/> enum, it
    ''' would <em>not</em> match <see cref="Direction.Right"/></remarks>
    Public Shared Function Parse(Of TEnum)(ByVal str As String, _
     ByVal valIfFails As TEnum) As TEnum
        Return Parse(str, False, valIfFails)
    End Function

    ''' <summary>
    ''' Parses the given string value into the required enum, returning the specified
    ''' value if the parsing fails.
    ''' </summary>
    ''' <typeparam name="TEnum">The type of enum to be parsed into</typeparam>
    ''' <param name="str">The string to parse</param>
    ''' <param name="ignoreCase">True to ignore case when parsing the value, False to
    ''' require that the case matches the enum values when parsing.</param>
    ''' <param name="valIfFails">The value to return if the parsing fails</param>
    ''' <returns>The enum value attained by parsing the given string</returns>
    ''' <exception cref="ArgumentException">If the type being parsed is not an
    ''' enumeration type.</exception>
    Public Shared Function Parse(Of TEnum)(ByVal str As String, _
     ByVal ignoreCase As Boolean, ByVal valIfFails As TEnum) As TEnum
        Dim val As TEnum = Nothing
        If TryParse(str, ignoreCase, val) Then Return val Else Return valIfFails
    End Function

    ''' <summary>
    ''' Attempts to parse the given value into an enum, return a flag indicating
    ''' success.
    ''' </summary>
    ''' <param name="str">The string to parse</param>
    ''' <param name="result">When this method returns, the value associated with the
    ''' specified string, if the key is found; otherwise, the default value for the
    ''' type of the value parameter. This parameter is passed uninitialized.</param>
    ''' <returns>True if the given string was successfully parsed, false if it did
    ''' not match any of the values defined in the enum represented by this class.
    ''' </returns>
    ''' <typeparam name="TEnum">The type of enum being parsed.</typeparam>
    ''' <exception cref="ArgumentException">If the type being parsed is not an
    ''' enumeration type.</exception>
    ''' <remarks>The string parsing is done case sensitively in this method - in such
    ''' a way that, when parsing "right" into a <see cref="Direction"/> enum, it
    ''' would <em>not</em> match <see cref="Direction.Right"/></remarks>
    Public Shared Function TryParse(Of TEnum)( _
     ByVal str As String, ByRef result As TEnum) As Boolean
        Return TryParse(str, False, result)
    End Function

    ''' <summary>
    ''' Attempts to parse the given enum value.
    ''' </summary>
    ''' <param name="str">The string value to parse</param>
    ''' <param name="ignorecase">Whether to ignore the case.</param>
    ''' <param name="result">When this method returns, the value associated with the
    ''' specified string, if the key is found; otherwise, the default value for the
    ''' type of the value parameter. This parameter is passed uninitialized.</param>
    ''' <returns>True If successful, False if not</returns>
    ''' <typeparam name="TEnum">The type of enum being parsed.</typeparam>
    ''' <exception cref="ArgumentException">If the type being parsed is not an
    ''' enumeration type.</exception>
    Public Shared Function TryParse(Of TEnum)(ByVal str As String, _
     ByVal ignoreCase As Boolean, ByRef result As TEnum) As Boolean

        If Not GetType(TEnum).IsEnum Then Throw New ArgumentException(
         My.Resources.clsEnum_YouCanOnlyParseAnEnumeratedTypeWithThisMethod)

        Return clsEnum(Of TEnum).TryParse(Str, ignoreCase, result)

    End Function
    ''' <summary>
    ''' Attempts to retrieve the localised text for the given enum value.
    ''' </summary>
    ''' <param name="type">The enum value to localise</param>
    ''' <returns>The localized enum string</returns>
    ''' <typeparam name="TEnum">The type of enum being localized.</typeparam>
    ''' <exception cref="ArgumentException">If the type being localized is not an
    ''' enumeration type.</exception>
    Public Shared Function GetLocalizedFriendlyName(Of TEnum)(ByVal type As TEnum) As String

        If Not GetType(TEnum).IsEnum Then Throw New ArgumentException(
         My.Resources.clsEnum_YouCanOnlyLocaliseAnEnumeratedTypeWithThisMethod)

        Dim result As String = Nothing
        Dim ttype = GetType(TEnum).ToString
        If ttype IsNot Nothing Then
            ttype = ttype.Substring(If(ttype.LastIndexOf(".") > 0, ttype.LastIndexOf(".") + 1, 0))
            result = My.Resources.ResourceManager.GetString($"{ttype}_{type}")
        End If

        Return If(result <> Nothing, result, type.ToString())

    End Function

End Class