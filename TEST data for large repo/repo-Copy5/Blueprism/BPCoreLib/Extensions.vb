Imports System.Reflection
Imports System.Text.RegularExpressions
Imports System.Runtime.CompilerServices
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models

Public Module Extensions

#Region " Private Helper Elements "

    ''' <summary>
    ''' State enumeration used in the <see cref="SplitQuoted"/>() method
    ''' </summary>
    Private Enum QuotedSplitState
        None = 0
        InEscape = 1
        QuotesFoundWhileEscaped = 2
    End Enum

    ''' <summary>
    ''' A regular expression which describes a SecurityIdentifier
    ''' </summary>
    ''' <remarks>This regex was lifted (then slightly modified) from:
    '''  http://stackoverflow.com/a/18828822/430967
    ''' </remarks>
    Private mSidRegex As New Regex("^S-\d-(\d+-){0,14}\d+$", RegexOptions.None, DefaultRegexTimeout)

#End Region

#Region " Enums "

    ''' <summary>
    ''' Checks if an enumeration has any of a set of specified flags set
    ''' </summary>
    ''' <param name="value">The value to test</param>
    ''' <param name="flags">The flags to check for in the value.</param>
    ''' <returns>True if the value has any of the specified flags set within it, or
    ''' if <paramref name="flags"/> is zero.</returns>
    ''' <exception cref="InvalidArgumentException">If the two enums are not of
    ''' equivalent types.</exception>
    <Extension>
    Public Function HasAnyFlag(value As [Enum], flags As [Enum]) As Boolean
        If Not value.GetType().IsEquivalentTo(flags.GetType()) Then
            Throw New InvalidArgumentException(My.Resources.Extensions_EnumTypesDoNotMatch0Vs1,
             value.GetType(), flags.GetType())
        End If

        ' Compatibility with the HasFlag() implementation which always returns
        ' true if the passed flag value is zero
        Dim flagVal As Long = Convert.ToInt64(flags)
        If flagVal = 0L Then Return True

        ' Int64 should cover all enum types without any trouble
        Return (Convert.ToInt64(value) And flagVal) <> 0
    End Function

    ''' <summary>
    ''' Sets a flag on a value and returns the result.
    ''' </summary>
    ''' <typeparam name="T">The enum type to use; although the compile-time checking
    ''' allows for other structures, this will only work for enums.</typeparam>
    ''' <param name="value">The value to set the flag on</param>
    ''' <param name="flag">The flag to set on the value</param>
    ''' <returns>The result after the flag has been set on the value.</returns>
    ''' <exception cref="BadCastException">If <paramref name="value"/> or
    ''' <paramref name="flag"/> were not enum values.</exception>
    <Extension, CLSCompliant(False)>
    Public Function SetFlags(Of T As {Structure, IConvertible})(
     value As T, flag As T) As T
        ' Runtime checking is not as good as compile-time checking, but it's better
        ' than nothing at all
        If Not TypeOf value Is [Enum] Then Throw New BadCastException(
            My.Resources.Extensions_SetFlagCanOnlyBeCalledOnEnumValues)

        Select Case value.GetTypeCode()
            Case TypeCode.Int16
                Dim result = Convert.ToInt16(value) Or Convert.ToInt16(flag)
                Return DirectCast(CObj(result), T)

            Case TypeCode.Int32
                Dim result = Convert.ToInt32(value) Or Convert.ToInt32(flag)
                Return DirectCast(CObj(result), T)

            Case TypeCode.Int64
                Dim result = Convert.ToInt64(value) Or Convert.ToInt64(flag)
                Return DirectCast(CObj(result), T)

            Case Else
                Throw New InvalidTypeException(
                    My.Resources.Extensions_EnumWithTypecodeOf0IsNotSupportedInSetFlags,
                    value.GetTypeCode())
        End Select
    End Function

    ''' <summary>
    ''' Sets a flag on a value and returns the result.
    ''' </summary>
    ''' <typeparam name="T">The enum type to use; although the compile-time checking
    ''' allows for any structure, this will only work for enums.</typeparam>
    ''' <param name="value">The value to set the flag on</param>
    ''' <param name="flag">The flag to set on the value</param>
    ''' <returns>The result after the flag has been set on the value.</returns>
    ''' <exception cref="BadCastException">If <paramref name="value"/> or
    ''' <paramref name="flag"/> were not enum values.</exception>
    <Extension, CLSCompliant(False)>
    Public Function ClearFlags(Of T As {Structure, IConvertible})(
     value As T, flag As T) As T
        ' Runtime checking is not as good as compile-time checking, but it's better
        ' than nothing at all
        If Not TypeOf value Is [Enum] Then Throw New BadCastException(
            My.Resources.Extensions_ClearFlagCanOnlyBeCalledOnEnumValues)

        Select Case value.GetTypeCode()
            Case TypeCode.Int16
                Dim result = Convert.ToInt16(value) And Not Convert.ToInt16(flag)
                Return DirectCast(CObj(result), T)

            Case TypeCode.Int32
                Dim result = Convert.ToInt32(value) And Not Convert.ToInt32(flag)
                Return DirectCast(CObj(result), T)

            Case TypeCode.Int64
                Dim result = Convert.ToInt64(value) And Not Convert.ToInt64(flag)
                Return DirectCast(CObj(result), T)

            Case Else
                Throw New InvalidTypeException(
                    My.Resources.Extensions_EnumWithTypecodeOf0IsNotSupportedInClearFlags,
                    value.GetTypeCode())
        End Select
    End Function

    ''' <summary>
    ''' Get the friendly name for a specified enum. The friendly name is set using 
    ''' the <see cref="FriendlyNameAttribute"/>, and if the enum is not assigned the
    ''' attribute then this function returns Nothing.
    ''' </summary>
    ''' <param name="e">The enum to get the friend name for</param>
    ''' <returns>The friendly name of the enum</returns>
    <Extension>
    Public Function GetFriendlyName(e As [Enum]) As String
        Return GetFriendlyName(e, False)
    End Function

    ''' <summary>
    ''' Get the friendly name for a specified enum. The friendly name is set using 
    ''' the <see cref="FriendlyNameAttribute"/>, and if the enum is not assigned the
    ''' attribute then this function returns Nothing.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <param name="includeRetirementInfo">If true, then include information about 
    ''' whether the enum is retired in the friendly name</param>
    ''' <returns>The friendly name of the enum</returns>
    <Extension>
    Public Function GetFriendlyName(e As [Enum], includeRetirementInfo As Boolean) As String
        Dim attr As FriendlyNameAttribute =
            BPUtil.GetAttributeValue(Of FriendlyNameAttribute)(e)
        If attr Is Nothing Then Return Nothing

        Dim friendlyName = attr.Name

        If includeRetirementInfo AndAlso HasRetiredAttribute(e) Then _
            friendlyName &= My.Resources.Retired

        Return friendlyName

    End Function

    Private Function HasRetiredAttribute(e As [Enum]) As Boolean
        Dim attr As RetiredAttribute =
            BPUtil.GetAttributeValue(Of RetiredAttribute)(e)
        Return attr IsNot Nothing
    End Function

#End Region

#Region " Integers "

    ''' <summary>
    ''' Enforces the bounding of a value to a given range.
    ''' </summary>
    ''' <param name="value">The value to bound</param>
    ''' <param name="min">The minimum allowed value</param>
    ''' <param name="max">The maximum allowed value</param>
    ''' <returns>The value clipped to the minimum or maximum if it fell outside the
    ''' bounds defined by them.</returns>
    ''' <exception cref="InvalidArgumentException">If <paramref name="min"/> is of a
    ''' greater value than <paramref name="max"/></exception>
    <Extension>
    Public Function Bound(value As Integer, min As Integer, max As Integer) As Integer
        If min > max Then Throw New InvalidArgumentException(
            My.Resources.Extensions_Min0CannotExceedMax1InBoundsCall, min, max)
        If value < min Then value = min
        If value > max Then value = max
        Return value
    End Function

#End Region

#Region " Strings "

    ''' <summary>
    ''' Splits a string on a specified character, allowing for the quoting of
    ''' elements to allow elements with the split character as part of their value.
    ''' </summary>
    ''' <param name="str">The string to split</param>
    ''' <param name="splitChar">The character to split the string on</param>
    ''' <returns>A collection of strings, split from the source string on the split
    ''' character, allowing for quoted elements within which the split character acts
    ''' as a normal character rather than a separator</returns>
    ''' <remarks>
    ''' <para>
    ''' To escape the split character within an element, the element must be encased
    ''' in quotes, eg.
    ''' <c>These\Are\Separated\"This '\' is part of the value"\Back\To\Separated</c>
    ''' with "\" as the split character would generate the elements:
    ''' <c>["These", "Are", "Separated", "This '\' is part of the value", "Back",
    ''' "To", "Separated"]</c>
    ''' </para>
    ''' <para>
    ''' To escape quote characters within a quoted element, double the quotes char,
    ''' as in a VB string, eg. the string:
    ''' <c>This|"is a |pipe| ""example"""|for you</c>
    ''' would split, with "|" as the split char, into (using single quotes for the
    ''' strings for clarity:
    ''' <c>['This', 'is a |pipe| "example"', 'for you']</c>
    ''' </para>
    ''' </remarks>
    ''' <exception cref="InvalidFormatException">If an escaped element contains an
    ''' unescaped quotes character, or if a quotes character at the start of an
    ''' element is left unterminated.</exception>
    <Extension>
    Public Function SplitQuoted(str As String, splitChar As Char) _
     As ICollection(Of String)
        Return SplitQuoted(str, splitChar, StringSplitOptions.None)
    End Function

    ''' <summary>
    ''' Splits a string on a specified character, allowing for the quoting of
    ''' elements to allow elements with the split character as part of their value.
    ''' </summary>
    ''' <param name="str">The string to split</param>
    ''' <param name="splitChar">The character to split the string on</param>
    ''' <param name="options">The split options; effectively, whether to return empty
    ''' entries or to discard them if discovered.</param>
    ''' <returns>A collection of strings, split from the source string on the split
    ''' character, allowing for quoted elements within which the split character acts
    ''' as a normal character rather than a separator</returns>
    ''' <remarks>
    ''' <para>
    ''' To escape the split character within an element, the element must be encased
    ''' in quotes, eg.
    ''' <c>These\Are\Separated\"This '\' is part of the value"\Back\To\Separated</c>
    ''' with "\" as the split character would generate the elements:
    ''' <c>["These", "Are", "Separated", "This '\' is part of the value", "Back",
    ''' "To", "Separated"]</c>
    ''' </para>
    ''' <para>
    ''' To escape quote characters within a quoted element, double the quotes char,
    ''' as in a VB string, eg. the string:
    ''' <c>This|"is a |pipe| ""example"""|for you</c>
    ''' would split, with "|" as the split char, into (using single quotes for the
    ''' strings for clarity:
    ''' <c>['This', 'is a |pipe| "example"', 'for you']</c>
    ''' </para>
    ''' </remarks>
    ''' <exception cref="InvalidFormatException">If an escaped element contains an
    ''' unescaped quotes character, or if a quotes character at the start of an
    ''' element is left unterminated.</exception>
    <Extension>
    Public Function SplitQuoted(
     str As String, splitChar As Char, options As StringSplitOptions) _
     As ICollection(Of String)
        ' Whether to skip (ie. not include) empty elements in the split list
        Dim skipEmpty = options.HasFlag(StringSplitOptions.RemoveEmptyEntries)
        ' The list of strings created from the splitting
        Dim els As New List(Of String)
        ' The current element, appended to as the loop goes on
        Dim currEl As New StringBuilder()
        ' The state of the parsing
        Dim state As QuotedSplitState = QuotedSplitState.None
        ' The index at which the current escaped value started
        Dim escapeInd As Integer = -1

        ' Loop through each char, altering the state as appropriate; build up the
        ' 'current element' buffer, committing it to the list when the element is
        ' complete.
        For i As Integer = 0 To str.Length
            Dim c As Char = If(i < str.Length, str(i), Char.MinValue)
            Select Case state
                Case QuotedSplitState.QuotesFoundWhileEscaped
                    If c = """"c Then
                        currEl.Append(""""c)
                        state = QuotedSplitState.InEscape

                    ElseIf c = splitChar OrElse i = str.Length Then
                        If Not skipEmpty OrElse currEl.Length > 0 _
                         Then els.Add(currEl.ToString())
                        currEl.Length = 0
                        state = QuotedSplitState.None
                        escapeInd = -1

                    Else
                        Throw New InvalidFormatException(
                            My.Resources.Extensions_UnescapedQuoteFoundAtIndex01213,
                            i - 1, vbCrLf, str, New String("-"c, i - 1))
                    End If

                Case QuotedSplitState.InEscape
                    If c = """"c Then
                        state = QuotedSplitState.QuotesFoundWhileEscaped
                    ElseIf i = str.Length Then
                        Throw New InvalidFormatException(
                            My.Resources.Extensions_UnterminatedQuotedValueFoundAtIndex01213,
                            escapeInd, vbCrLf, str, New String("-"c, escapeInd))
                    Else
                        currEl.Append(c)
                    End If

                Case Else
                    If c = splitChar OrElse i = str.Length Then
                        If Not skipEmpty OrElse currEl.Length > 0 _
                         Then els.Add(currEl.ToString())
                        currEl.Length = 0
                        state = QuotedSplitState.None
                        escapeInd = -1

                    ElseIf c = """"c AndAlso currEl.Length = 0 Then
                        state = QuotedSplitState.InEscape
                        escapeInd = i

                    Else
                        currEl.Append(c)

                    End If
            End Select

        Next
        Return els
    End Function

    ''' <summary>
    ''' Checks if <paramref name="this"/> contains the string specified in
    ''' <paramref name="toCheck"/>, using the culture and case sensitivity settings
    ''' provided in <paramref name="comp"/>
    ''' </summary>
    ''' <param name="this">The string to search within for any of the provided values
    ''' </param>
    ''' <param name="toCheck">The string to search the string for</param>
    ''' <param name="comp">The string comparison parameters to use when testing to
    ''' see if the string contains the check value.</param>
    ''' <returns>True if any of the given values were present in the string; False
    ''' otherwise.</returns>
    <Extension>
    Public Function Contains(
     this As String, toCheck As String, comp As StringComparison) As Boolean
        Return this.IndexOf(toCheck, comp) >= 0
    End Function

    ''' <summary>
    ''' Checks if <paramref name="this"/> contains any of the given
    ''' <paramref name="values"/>, using the current culture and retaining case
    ''' sensitivity
    ''' </summary>
    ''' <param name="this">The string to search within for any of the provided values
    ''' </param>
    ''' <param name="values">The values to search the string for</param>
    ''' <returns>True if any of the given values were present in the string; False
    ''' otherwise.</returns>
    <Extension>
    Public Function ContainsAny(
     this As String, ParamArray values() As String) As Boolean
        Return this.ContainsAny(StringComparison.CurrentCulture, values)
    End Function

    ''' <summary>
    ''' Checks if <paramref name="this"/> contains any of the given
    ''' <paramref name="values"/>, using the culture and case sensitivity settings
    ''' provided in <paramref name="comp"/>
    ''' </summary>
    ''' <param name="this">The string to search within for any of the provided values
    ''' </param>
    ''' <param name="values">The values to search the string for</param>
    ''' <returns>True if any of the given values were present in the string; False
    ''' otherwise.</returns>
    <Extension>
    Public Function ContainsAny(
     this As String, comp As StringComparison, ParamArray values() As String) _
     As Boolean
        Return values.Any(Function(v) this.Contains(v))
    End Function

    ''' <summary>
    ''' Checks if this string contains the format expected of a SecurityIdentifier.
    ''' Note that this does not check that the security identifier is valid; it only
    ''' checks to see if the format matches that expected of a SecurityIdentifier.
    ''' </summary>
    ''' <param name="this">The string to test to see if it is formatted as a SID.
    ''' </param>
    ''' <returns>True if the format of the string matches a SecurityIdentifier; false
    ''' if the string is null or does not match a SID format.</returns>
    ''' <remarks>This regex was lifted (then slightly modified) from:
    '''  http://stackoverflow.com/a/18828822/430967
    ''' </remarks>
    <Extension>
    Public Function IsSid(this As String) As Boolean
        Return (this IsNot Nothing AndAlso mSidRegex.IsMatch(this))
    End Function

    ''' <summary>
    ''' Truncates a string to the given length.
    ''' </summary>
    ''' <param name="value">The string to truncate</param>
    ''' <param name="maxLength">The maximum length of the string (including the suffix)</param>
    ''' <param name="suffix">A suffix to append to the end of the string</param>
    <Extension>
    Public Function Truncate(value As String, maxLength As Integer, suffix As String) As String
        If String.IsNullOrEmpty(value) Then Return value
        maxLength -= If(suffix Is Nothing, 0, suffix.Length)
        Return If(value.Length <= maxLength, value, value.Substring(0, maxLength) & suffix)
    End Function

    ''' <summary>
    ''' Truncates a string to the given length.
    ''' </summary>
    ''' <param name="value">The string to truncate</param>
    ''' <param name="maxLength">The maximum length of the string</param>
    <Extension>
    Public Function Truncate(value As String, maxLength As Integer) As String
        Return Truncate(value, maxLength, String.Empty)
    End Function

    ''' <summary>
    ''' Removes any non-ASCII characters from the given string.
    ''' </summary>
    ''' <param name="value">The string to check</param>
    <Extension>
    Public Function StripNonASCII(value As String) As String
        Return Regex.Replace(value, "[^\u0000-\u007F]+", String.Empty)
    End Function

    <Extension>
    Public Function Capitalize(data As String) As String
        If data.Length = 0 Then Return data
        If data.Length = 1 Then Return data.ToUpper
        Return $"{Char.ToUpper(data(0))}{data.Substring(1)}"
    End Function

    ''' <summary>
    ''' Returns a string containing a specified number of characters from the left of a string
    ''' equivalent to Microsoft.VisualBasic.Left
    ''' </summary>
    ''' <returns>string</returns>
    <Extension>
    Public Function Left(str As String, ByVal length As Integer) As String
        If str Is Nothing Then Return String.Empty
        If str.Length < length Then Return str

        Return str.Substring(0, length)
    End Function
    ''' <summary>
    ''' Returns a string that contains all of the characters starting from a  specified number of characters from the left of a string
    ''' equivalent to Microsoft.VisualBasic.Mid
    ''' </summary>
    ''' <returns>string</returns>
    <Extension>
    Public Function Mid(str As String, start As Integer, length As Integer) As String
        If str Is Nothing Then Return String.Empty

        If str.Length < start Then Return String.Empty
        If str.Length - start < length Then Return str.Substring(start - 1)

        Return str.Substring(start - 1, length)
    End Function
    ''' <summary>
    ''' Returns a string that contains all of the characters starting from a  specified number of characters from the left of a string
    ''' equivalent to Microsoft.VisualBasic.Mid
    ''' </summary>
    ''' <returns>string</returns>
    <Extension>
    Public Function Mid(str As String, start As Integer) As String
        If str Is Nothing Then Return str
        If str.Length < start Then Return String.Empty

        Return str.Substring(start - 1)
    End Function

    ''' <summary>
    ''' Returns a string that contains all of the characters starting from a  specified number of characters from the left of a string
    ''' equivalent to Microsoft.VisualBasic.Mid
    ''' </summary>
    ''' <returns>string</returns>
    <Extension>
    Public Function Right(str As String, length As Integer) As String
        If str Is Nothing Then Return String.Empty
        If length >= str.Length Then Return str

        Return str.Substring(str.Length - length)
    End Function

#End Region

#Region " General "

    ''' <summary>
    ''' Gets all concrete subclasses of the specified type in 'this' assembly.
    ''' </summary>
    ''' <typeparam name="TBase">The type for which all concrete subclasses is
    ''' required.</typeparam>
    ''' <param name="this">The assembly in which to look for subclass types.</param>
    ''' <returns>An enumerable over the non-abstract subclass types of the specified
    ''' type with the specified assembly.</returns>
    <Extension>
    Public Function GetConcreteSubclasses(Of TBase)(this As Assembly) _
     As IEnumerable(Of Type)
        Return this.GetConcreteSubclasses(GetType(TBase))
    End Function

    ''' <summary>
    ''' Gets all concrete subclasses of the specified type in 'this' assembly.
    ''' </summary>
    ''' <param name="this">The assembly in which to look for subclass types.</param>
    ''' <param name="tp">The type for which all concrete subclasses is
    ''' required.</param>
    ''' <returns>An enumerable over the non-abstract subclass types of the specified
    ''' type with the specified assembly.</returns>
    <Extension>
    Public Function GetConcreteSubclasses(this As Assembly, tp As Type) _
     As IEnumerable(Of Type)
        Return this.GetTypes().Where(
         Function(t) Not t.IsAbstract AndAlso t.IsSubclassOf(tp)
        )
    End Function

    ''' <summary>
    ''' Gets all concrete classes which implement the interface described by the
    ''' specified type in 'this' assembly.
    ''' </summary>
    ''' <typeparam name="TInterface">The interface type for which all implementing
    ''' concrete classes is required.</typeparam>
    ''' <param name="this">The assembly in which to look for implementing types.
    ''' </param>
    ''' <returns>An enumerable over the non-abstract implementing types of the
    ''' specified interface type with the specified assembly.</returns>
    <Extension>
    Public Function GetConcreteImplementations(Of TInterface)(this As Assembly) _
     As IEnumerable(Of Type)
        Return this.GetConcreteImplementations(GetType(TInterface))
    End Function

    ''' <summary>
    ''' Gets all concrete classes which implement the interface described by the
    ''' specified type in 'this' assembly.
    ''' </summary>
    ''' <param name="this">The assembly in which to look for implementing types.
    ''' </param>
    ''' <param name="tp">The interface type for which all implementing
    ''' concrete classes is required.</param>
    ''' <returns>An enumerable over the non-abstract implementing types of the
    ''' specified interface type with the specified assembly.</returns>
    <Extension>
    Public Function GetConcreteImplementations(this As Assembly, tp As Type) _
     As IEnumerable(Of Type)
        Return this.GetTypes().Where(
         Function(t) Not t.IsAbstract AndAlso tp.IsAssignableFrom(t)
        )
    End Function

    ''' <summary>
    ''' Gets all the concrete derived types of the given type in 'this' assembly.
    ''' </summary>
    ''' <param name="this">The assembly in which to look for derived types.
    ''' </param>
    ''' <typeparam name="TBase">The type to derive from - this can be an interface or a
    ''' class type; all concrete derived types will be returned.</typeparam>
    ''' <returns>An enumerable over the concrete derived types of the specified
    ''' type in the specified assembly.</returns>
    <Extension>
    Public Function GetConcreteDerivedTypes(Of TBase)(this As Assembly) _
     As IEnumerable(Of Type)
        Return this.GetConcreteDerivedTypes(GetType(TBase))
    End Function

    ''' <summary>
    ''' Gets all the concrete derived types of the given type in 'this' assembly.
    ''' </summary>
    ''' <param name="this">The assembly in which to look for derived types.
    ''' </param>
    ''' <param name="tp">The type to derive from - this can be an interface or a
    ''' class type; all concrete derived types will be returned.</param>
    ''' <returns>An enumerable over the concrete derived types of the specified
    ''' type in the specified assembly.</returns>
    <Extension>
    Public Function GetConcreteDerivedTypes(this As Assembly, tp As Type) _
     As IEnumerable(Of Type)
        If tp.IsInterface Then
            Return this.GetConcreteImplementations(tp)
        Else
            Return this.GetConcreteSubclasses(tp)
        End If
    End Function

    ''' <summary>
    ''' Gets all the derived interfaces of a base interface type
    ''' </summary>
    ''' <typeparam name="TBase">The base interface type for which all subtypes are
    ''' required.</typeparam>
    ''' <param name="this">The assembly from which to draw the types</param>
    ''' <returns>An enumerable over all the interface types found in this
    ''' assembly which extend the given base interface type</returns>
    <Extension>
    Public Function GetSubInterfaces(Of TBase)(this As Assembly) _
     As IEnumerable(Of Type)
        Return this.GetSubInterfaces(GetType(TBase))
    End Function

    ''' <summary>
    ''' Gets all the derived interfaces of a base interface type
    ''' </summary>
    ''' <param name="this">The assembly from which to draw the types</param>
    ''' <param name="baseType">The base interface type for which all subtypes are
    ''' required.</param>
    ''' <returns>An enumerable over all the interface types found in this
    ''' assembly which extend the given base interface type</returns>
    <Extension>
    Public Function GetSubInterfaces(this As Assembly, baseType As Type) _
     As IEnumerable(Of Type)
        ' Not an interface - then it can't have any subinterfaces
        If Not baseType.IsInterface Then Return GetEmpty.IEnumerable(Of Type)

        ' Get all the types which are:
        ' * Interfaces
        ' * Except the type we want the subinterfaces of
        ' * That can be assigned to that type
        Return this.GetTypes().
            Where(Function(t) t.IsInterface).
            Where(Function(t) t IsNot baseType).
            Where(Function(t) baseType.IsAssignableFrom(t))

    End Function

    ''' <summary>
    ''' Gets all referenced types found in a given type. This will only return a
    ''' single instance of each type found, even if it is encountered multiple times
    ''' within the type.
    ''' </summary>
    ''' <param name="tp">The type from which to get the referenced types.</param>
    ''' <returns>An enumerable of the types referenced in the specified type.
    ''' </returns>
    <Extension>
    Public Function GetAllReferencedTypes(tp As Type) As IEnumerable(Of Type)
        Dim methods As IEnumerable(Of Type) =
            tp.GetMethods().SelectMany(Function(m) m.GetReferencedTypes()).Distinct()

        Dim properties As IEnumerable(Of Type) =
            tp.GetProperties().Select(Function(n) n.GetType).Distinct()

        Return properties.Union(methods).Distinct()

    End Function

    ''' <summary>
    ''' Gets all types referenced in the given method, including those specified in
    ''' generics and arrays.
    ''' </summary>
    ''' <param name="this">The method whose types should be enumerated</param>
    ''' <returns>An enumerable over the types referenced in the given method
    ''' </returns>
    <Extension>
    Public Function GetReferencedTypes(this As MethodInfo) As IEnumerable(Of Type)
        Dim types As New HashSet(Of Type)

        For Each p As ParameterInfo In this.GetParameters()
            Dim pt = p.ParameterType
            If pt.IsByRef Then pt = pt.GetElementType()
            types.Add(pt)
            types.UnionWith(UnwrapTypes(pt))
        Next

        If this.ReturnType IsNot GetType(Void) Then
            types.Add(this.ReturnType)
            types.UnionWith(UnwrapTypes(this.ReturnType))
        End If

        Return types
    End Function

    ''' <summary>
    ''' Unwraps all generic and array types from the given type, returning all of
    ''' them in an enumerable. Note that the given type itself is not returned.
    ''' </summary>
    ''' <param name="tp">The type to unwrap its referenced types from</param>
    ''' <returns>An enumerable of types picked out from the given type, unwrapping
    ''' any array or generic references within it. Note that the given type is not
    ''' included in the returned enumerable</returns>
    Private Function UnwrapTypes(tp As Type) As IEnumerable(Of Type)
        Return UnwrapTypes(tp, New HashSet(Of Type))
    End Function

    ''' <summary>
    ''' Unwraps all generic and array types from the given type into a provided set and
    ''' returns it. Note that the given type itself is not added to the set by this
    ''' method.
    ''' </summary>
    ''' <param name="tp">The type to unwrap its referenced types from</param>
    ''' <param name="types">The set into which the types should be unwrapped.</param>
    ''' <returns>An enumerable of types picked out from the given type, unwrapping
    ''' any array or generic references within it. Note that the given type is not
    ''' added into the returned enumerable</returns>
    Private Function UnwrapTypes(tp As Type, types As ISet(Of Type)) As IEnumerable(Of Type)

        If tp.IsArray Then
            tp = tp.GetElementType()
            If types.Add(tp) Then UnwrapTypes(tp, types)
        End If

        If tp.IsGenericType Then
            For Each gt As Type In tp.GetGenericArguments()
                If types.Add(gt) Then UnwrapTypes(gt, types)
            Next
        End If

        Return types

    End Function

    ''' <summary>
    ''' Slightly odd function to return the object the method is called on. This is
    ''' purely to allow for code which performs some operations within a
    ''' With.. End With block and then returns the object operated on, all without
    ''' having to assign it to a variable, eg.
    ''' <c>
    ''' With New Foo()
    '''   .Bar = "Foobar"
    '''   .Add(New Thing())
    '''   .DoSomethingElse()
    '''   Return .It()
    ''' End With
    ''' </c>
    ''' </summary>
    ''' <typeparam name="T">The type of object passed in</typeparam>
    ''' <param name="this">The object to return</param>
    ''' <returns>The object passed in, unmolested.</returns>
    <Extension>
    Public Function It(Of T)(this As T) As T
        Return this
    End Function

    ''' <summary>
    ''' Simple extension to convert a datatable to a csv formatted string
    ''' </summary>
    ''' <param name="this">The datatable to convert to a csv</param>
    ''' <param name="includeHeaders">
    ''' A flag to determine if column headers should be included in the output.
    ''' </param>
    ''' <returns>
    ''' A csv formatted string of the datatable provided, with or without
    ''' headers.
    ''' </returns>

    <Extension>
    Public Function DataTableToCsv(this As DataTable, includeHeaders As Boolean) As String

        Dim builder As StringBuilder = New StringBuilder()

        If includeHeaders Then
            Dim columnHeaders = this.Columns.Cast(Of DataColumn)() _
                                   .Select(Function(x) x.ColumnName).ToArray()

            builder.AppendLine(String.Join(",", columnHeaders))
        End If

        For Each row As DataRow In this.Rows()
            Dim rowValues As String() = Array.ConvertAll(row.ItemArray(), Function(x) If(x?.ToString().ToEscapedCsvString(), String.Empty))
            builder.AppendLine(String.Join(",", rowValues))
        Next

        Return builder.ToString()

    End Function

    ''' <summary>
    ''' Simple extension to escape a string to a csv safe string
    ''' </summary>
    ''' <param name="this">The string to to make safe</param>
    ''' <returns>
    ''' A safe csv formatted string
    ''' </returns>
    <Extension>
    Public Function ToEscapedCsvString(this As String) As String
        Dim mustQuote As Boolean = this.IndexOf(",", StringComparison.Ordinal) >= 0 OrElse
                                   this.IndexOf("""", StringComparison.Ordinal) >= 0 OrElse
                                   this.Contains("\n", StringComparison.Ordinal) OrElse
                                   this.Contains("\r", StringComparison.Ordinal)

        If mustQuote Then
            Dim sb = New StringBuilder()
            sb.Append("""")
            For Each nextChar In this
                sb.Append(nextChar)
                If nextChar = """"c Then sb.Append("""")
            Next
            sb.Append("""")
            Return sb.ToString()

        End If
        Return this
    End Function

    ''' <summary>
    ''' Simple extension to return the midnight time of a date, works 
    ''' out the midnight time in relation to UTC when the datetime is in
    ''' a different timezone.
    ''' </summary>
    ''' <param name="this">The datetime that will be used.</param>
    ''' <returns>
    ''' A date that represents the midnight time of the date passed in.
    ''' </returns>
    <Extension>
    Public Function ToMidnight(this As Date) As Date
        Select Case this.Kind
            Case DateTimeKind.Local
                Return this.Date
            Case Else
                Return this.ToLocalTime().Date.ToUniversalTime()
        End Select
    End Function

#End Region

End Module
