Imports System.Collections.Concurrent
Imports System.Drawing
Imports System.Drawing.Text
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Management
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Text.RegularExpressions
Imports System.Xml
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Server.Domain.Models
Imports Microsoft.Win32

''' <summary>
''' It's a utility class, borne from the 4th time I found myself wanting
''' an IfNull() method, and resenting having to rewrite one every time.
''' </summary>
Public Class BPUtil

#Region " Static Members / Constants "

    ''' <summary>
    ''' The range of invalid XML characters, according to the XML 1.0 spec
    ''' </summary>
    Public Shared InvalidXmlCharRegex As New Regex(
     "[^\x09\x0A\x0D\x20-\uD7FF\uE000-\uFFFD\u10000-u10FFFF]")

    ' Lazily populated flag indicating if the host OS is 64 bit or not
    Private Shared m_64BitOS As Nullable(Of Boolean)

    ''' <summary>
    ''' The UTC equivalent of <see cref="Date.MinValue"/>
    ''' </summary>
    Public Shared ReadOnly DateMinValueUtc As Date =
     Date.SpecifyKind(Date.MinValue, DateTimeKind.Utc)

    ''' <summary>
    ''' The UTC equivalent of <see cref="Date.MaxValue"/>
    ''' </summary>
    Public Shared ReadOnly DateMaxValueUtc As Date =
     Date.SpecifyKind(Date.MinValue, DateTimeKind.Utc)

    ''' <summary>
    ''' Cache of types against their fully qualified classnames
    ''' (well, fully qualified except for the Assembly anyway)
    ''' </summary>
    Private Shared mExceptionTypeCache As New ConcurrentDictionary(Of String, Type)

#End Region

#Region " Constructor Hider "

    ''' <summary>
    ''' Static class - no instances allowed
    ''' </summary>
    Private Sub New()
    End Sub

#End Region

#Region " Operating system version "
    ''' <summary>
    ''' Returns the operating system version, and will also work with all windows
    ''' versions including Windows 8.1 and above.
    ''' </summary>
    ''' <returns>The operating system version</returns>
    Public Shared Function GetOSVersion() As Version

        Dim ver = Environment.OSVersion.Version
        'On Windows 8.1 and above the GetVersion function that Environment.OSVersion
        'uses is depricated, so we use WMI instead.
        If ver > New Version(6, 2) Then 'Check greater than Windows 8, (internally version 6.2)
            Using searcher As New ManagementObjectSearcher("SELECT Caption, Version FROM Win32_OperatingSystem")
                Try
                    Using managementEntries = searcher.Get
                        Dim os = managementEntries.OfType(Of ManagementObject)().FirstOrDefault()
                        If os IsNot Nothing Then
                            Return New Version(CStr(os("Version")))
                        End If
                    End Using
                Catch
                End Try
            End Using
        End If

        Return ver
    End Function

#End Region

#Region ".NET Version"
    ''' <summary>
    ''' Returns the latest .NET version installed on the PC
    ''' </summary>
    ''' <returns></returns>
    Public Shared Function GetDotNetVersion() As String
        Const subkey As String = "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\"
        Using ndpKey As RegistryKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(subkey)
            If ndpKey IsNot Nothing AndAlso ndpKey.GetValue("Release") IsNot Nothing Then
                Return CheckFor45PlusVersion(CType(ndpKey.GetValue("Release"), Integer))
            Else
                Return String.Format("{0}.{1}", Environment.Version.Major, Environment.Version.Minor)
            End If

        End Using
    End Function

    ' Checking the version using >= will enable forward compatibility.
    Private Shared Function CheckFor45PlusVersion(releaseKey As Integer) As String
        If releaseKey >= 460798 Then
            Return "4.7"
        ElseIf releaseKey >= 394802 Then
            Return "4.6.2"
        ElseIf releaseKey >= 394254 Then
            Return "4.6.1"
        ElseIf releaseKey >= 393295 Then
            Return "4.6"
        ElseIf releaseKey >= 379893 Then
            Return "4.5.2"
        ElseIf releaseKey >= 378675 Then
            Return "4.5.1"
        ElseIf releaseKey >= 378389 Then
            Return "4.5"
        End If
        ' This code should never execute. A non-null release key should mean
        ' that 4.5 or later is installed.
        Return My.Resources.BPUtil_No45OrLaterVersionDetected
    End Function
#End Region

#Region " 64-bit Recognition "

    ''' <summary>
    ''' Property to indicate if this is a 64 bit operating system or not. This is
    ''' a lazily populated then cached value, on the assumption that the bitness of
    ''' the operating system cannot change within the lifetime of a process.
    ''' </summary>
    ''' <remarks>From .NET4 onwards, this is unnecessary since there is a static
    ''' property in <c>System.Environment</c> which provides the same data.</remarks>
    ''' <exception cref="Server.Domain.Models.OperationFailedException">If the call to IsWow64Process
    ''' failed</exception>
    Public Shared ReadOnly Property Is64BitOperatingSystem() As Boolean
        Get
            If m_64BitOS.HasValue Then Return m_64BitOS.Value

            ' If we're running inside a 64 bit process, then obviously it's 64-bit OS
            If IntPtr.Size = 8 Then m_64BitOS = True : Return True

            ' If this is a WOW64 (ie. 32-bit emulation) process, also 64-bit OS,
            ' since the emulation layer is not necessary in 32-bit OSes
            Using p As Process = Process.GetCurrentProcess()
                ' If the call works just return the value
                Dim isWow64 As Boolean = False
                Try
                    If IsWow64Process(p.Handle, isWow64) _
                     Then m_64BitOS = isWow64 : Return isWow64

                Catch
                    ' If it fails, odds are the IsWow64Process() function isn't there
                    ' that only happens in 32 bit OSes, so assume that's what this is
                    m_64BitOS = False
                    Return False
                End Try

                ' If it didn't work (for reasons other than function not present),
                ' throw the error along with the last windows error registered
                Throw New OperationFailedException(
                 My.Resources.BPUtil_ErrorTryingToTestForWOW64ProcessError0x0X8,
                 Marshal.GetLastWin32Error()
                )

            End Using
        End Get
    End Property

    ''' <summary>
    ''' Checks if the process with the given process ID is a 64 bit process or not.
    ''' </summary>
    ''' <param name="handle">The handle of the process to test</param>
    ''' <returns>True if the process with the given handle is a 64 bit process;
    ''' False if it is a 32 bit process.</returns>
    Public Shared Function Is64BitProcess(ByVal handle As IntPtr) As Boolean
        ' If this is a 32 bit OS, it can't possibly be a 64 bit process. Ipso Facto
        If Not Is64BitOperatingSystem Then Return False

        ' If it's a wow64 process in a 64 bit OS, that means it's 32 bit
        ' Otherwise, it's a 32 bit process in a 64 bit OS
        Dim isWow64 As Boolean = False
        If IsWow64Process(handle, isWow64) Then Return Not isWow64

        ' If it didn't work, throw the error
        Throw New OperationFailedException(
         My.Resources.BPUtil_ErrorTryingToTestForWOW64ProcessError0x0X8,
         Marshal.GetLastWin32Error()
        )

    End Function

    ''' <summary>
    ''' Checks if the process with the given process ID is a 64 bit process or not.
    ''' </summary>
    ''' <param name="proc">The process to test</param>
    ''' <returns>True if the given process is a 64 bit process; False if it is a
    ''' 32 bit process.</returns>
    Public Shared Function Is64BitProcess(ByVal proc As Process) As Boolean
        Return Is64BitProcess(proc.Handle)
    End Function

#End Region

#Region " I/O Utilities "

    ''' <summary>
    ''' Combines all the given paths using the Path.Combine() logic for each argument
    ''' in order to generate a full filesystem path.
    ''' </summary>
    ''' <param name="root">The first path to combine</param>
    ''' <param name="paths">The other paths to combine with the root</param>
    ''' <returns>The fully combined path made up of all the given paths.</returns>
    Public Shared Function CombinePaths(ByVal root As String, ByVal ParamArray paths() As String) _
     As String
        Dim accum As String = root
        For Each p As String In paths
            accum = Path.Combine(accum, p)
        Next
        Return accum
    End Function

    ''' <summary>
    ''' Gets a cryptographically strong, random filename, prepended by the path to
    ''' the user's temp path. The returned value will represent a file which is
    ''' guaranteed not to exist at the time of the function's return, though the file
    ''' is not created within this method.
    ''' </summary>
    ''' <returns>The path to a randomly named file in the user's temp directory.
    ''' </returns>
    Public Shared Function GetRandomFilePath() As String
        Dim filePath As String
        Do
            filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName())
        Loop While File.Exists(filePath)
        Return filePath
    End Function

    ''' <summary>
    ''' Cleans the given name such that it can be used as a filename
    ''' </summary>
    ''' <param name="name">The potential name to be used a filename</param>
    ''' <returns>The given name with any invalid characters replaced with underscores
    ''' </returns>
    Public Shared Function CleanFileName(ByVal name As String) As String
        Return CleanFileName(name, "_"c)
    End Function

    ''' <summary>
    ''' Cleans the given name such that it can be used as a filename
    ''' </summary>
    ''' <param name="name">The potential name to be used a filename</param>
    ''' <param name="replace">The character to replace any invalid characters in the
    ''' name with.</param>
    ''' <returns>The given name with any invalid characters replaced with underscores
    ''' </returns>
    ''' <exception cref="InvalidValueException">If the given replacement char is
    ''' itself invalid in a filename</exception>
    Public Shared Function CleanFileName(
     ByVal name As String, ByVal replace As Char) As String

        Dim badChars As ICollection(Of Char) = Path.GetInvalidFileNameChars()
        If badChars.Contains(replace) Then Throw New InvalidValueException(
         My.Resources.BPUtil_CannotReplaceBadCharsWith0ItIsItselfABadChar, replace)

        For Each c As Char In badChars
            name = name.Replace(c, replace)
        Next

        Return name

    End Function

    ''' <summary>
    ''' Builds a command line argument string from the given set of arguments.
    ''' The arguments are converted to strings using <c>ToString()</c>. Any null
    ''' elements in <paramref name="args"/> are ignored.
    ''' </summary>
    ''' <param name="args">The arguments from which to build a command line argument
    ''' string.</param>
    ''' <returns>The string which can be used for the command line arguments of a
    ''' process call</returns>
    Public Shared Function BuildCommandLineArgString(
     ParamArray args() As Object) As String

        Dim sb As New StringBuilder()
        For Each o As Object In args
            ' We ignore null arguments
            If o Is Nothing Then Continue For

            ' Convert to a string
            Dim s = o.ToString()

            ' *We* *ignore* *null* *arguments*
            If s.Length = 0 Then Continue For

            ' If we've built some string already, separate from previous arg with " "
            If sb.Length > 0 Then sb.Append(" "c)

            ' If it doesn't contain a 'special' char just append it and move on
            If Not s.ContainsAny(" ", """", vbVerticalTab, vbCr, vbLf) Then
                sb.Append(s)
                Continue For
            End If

            ' Otherwise we have to ensure that escape everything which needs escaping
            ' This implementation was cribbed primarily from:
            ' https://blogs.msdn.microsoft.com/twistylittlepassagesallalike/2011/04/23/everyone-quotes-command-line-arguments-the-wrong-way/

            sb.Append(""""c)
            Dim i = 0
            While i < s.Length
                Dim numBackslashes = 0
                While i < s.Length AndAlso s(i) = "\"
                    i += 1
                    numBackslashes += 1
                End While
                If i = s.Length Then
                    ' Escape all the backslashes, but let the terminating double
                    ' quote mark we add below be interpreted as a meta-character
                    sb.Append("\"c, numBackslashes * 2)
                    Exit While

                ElseIf s(i) = """"c Then
                    ' Escape all backslashes and the following quote mark
                    sb.Append("\"c, 1 + numBackslashes * 2)
                    sb.Append(s(i))

                Else
                    ' Backslashes aren't special here
                    sb.Append("\"c, numBackslashes)
                    sb.Append(s(i))

                End If
                i += 1

            End While

            sb.Append(""""c)

        Next
        Return sb.ToString()
    End Function

#End Region

#Region " XML Utilities "

    ''' <summary>
    ''' Creates a text XML element, appends it to a parent element, and returns it.
    ''' </summary>
    ''' <param name="parent">The parent node to which the newly created text
    ''' element should be appended</param>
    ''' <param name="name">The name of the new text element to create</param>
    ''' <param name="text">The text contents of the element to create</param>
    ''' <returns>A new XML element with the given text contents and the specified
    ''' name, after it has been appended to the specified parent element.</returns>
    Public Shared Function AppendTextElement(ByVal parent As XmlNode,
     ByVal name As String, ByVal text As String) As XmlElement
        Dim el As XmlElement = parent.OwnerDocument.CreateElement(name)
        el.InnerText = text
        parent.AppendChild(el)
        Return el
    End Function

    ''' <summary>
    ''' Creates a text XML element, appends it to a parent element, and returns it.
    ''' </summary>
    ''' <param name="parent">The parent node to which the newly created text
    ''' element should be appended</param>
    ''' <param name="name">The name of the new text element to create</param>
    ''' <param name="obj">The object whose text representation should form the text
    ''' contents of the element being created</param>
    ''' <returns>A new XML element with the given text contents and the specified
    ''' name, after it has been appended to the specified parent element.</returns>
    ''' <remarks>This gets the text from the object by calling ToString() on it. If
    ''' the given object is null, an empty string is used.</remarks>
    Public Shared Function AppendTextElement(ByVal parent As XmlNode,
     ByVal name As String, ByVal obj As Object) As XmlElement
        If obj Is Nothing _
         Then Return AppendTextElement(parent, name, "") _
         Else Return AppendTextElement(parent, name, obj.ToString())
    End Function

    ''' <summary>
    ''' Checks if the given value is valid XML, ie. contains no invalid characters.
    ''' Note that a null value is considered valid (I understand that it silently
    ''' converts it to an empty string), so IsValidXmlString(Nothing) will return
    ''' True.
    ''' </summary>
    ''' <param name="value">The value to test for valid XML</param>
    ''' <returns>True if the given value was valid for use in XML; False otherwise.
    ''' </returns>
    ''' <seealso cref="InvalidXmlCharRegex"/>
    ''' <seealso cref="SanitizeXmlString"/>
    Public Shared Function IsValidXmlString(ByVal value As String) As Boolean
        If value Is Nothing Then Return True
        Return Not InvalidXmlCharRegex.IsMatch(value)
    End Function

    ''' <summary>
    ''' Sanitizes the given string for use in XML, stripping out any characters which
    ''' cannot be represented in XML 1.0.
    ''' Note that if null is passed as the argument, null is returned.
    ''' </summary>
    ''' <param name="value">The value to sanitize</param>
    ''' <returns>The given string value with all invalid XML characters stripped out
    ''' of it or null if the argument given was null.</returns>
    ''' <seealso cref="InvalidXmlCharRegex"/>
    ''' <seealso cref="IsValidXmlString"/>
    Public Shared Function SanitizeXmlString(ByVal value As String) As String
        If value Is Nothing Then Return Nothing
        Return InvalidXmlCharRegex.Replace(value, "")
    End Function

#End Region

#Region " General Utilities "

    ''' <summary>
    ''' Gets an ordinal for a number e.g. st, nd rd
    ''' </summary>
    ''' <param name="value">The number to get an ordinal for</param>
    ''' <returns>A string that can be concatenated onto the number, or a blank
    ''' string on failure</returns>
    <Obsolete("use locale aware LTools.Ordinal() and friends")>
    Public Shared Function GetOrdinalIndicator(ByVal value As Integer) As String
        Try
            'special case - if it's between 10 and 20, then the
            'suffix is always a "th" (11th, 12th, 113th etc)
            If (((value \ 10) Mod 10) = 1) Then Return "th"

            Select Case (value Mod 10)
                Case 1 : Return "st"
                Case 2 : Return "nd"
                Case 3 : Return "rd"
                Case Else : Return "th"
            End Select
        Catch
            Return ""
        End Try
    End Function

    ''' <summary>
    ''' Finds the type corresponding with the given fully (ish) qualified class
    ''' name - it is not expected that the name will include the Assembly that
    ''' it is serialized from.
    ''' </summary>
    ''' <param name="typeName">The full name of the class including namespace.</param>
    ''' <returns>The type associated with the given type name in the current App Domain
    ''' or null if no such type could be found.</returns>
    Public Shared Function FindType(typeName As String) As Type
        If typeName = "" Then Throw New ArgumentNullException(NameOf(typeName),
         My.Resources.BPUtil_NoFaultContentsTypeGivenCannotUnwrapTheFault)

        Return mExceptionTypeCache.GetOrAdd(typeName,
         Function(n)
             ' If it's as simple as 'GetType' working - just use that.
             Dim tp = Type.GetType(n)
             If tp IsNot Nothing Then Return tp

             ' Otherwise, we must go through all available assemblies
             ' to see if Type is defined in there, based on its FullName
             Return AppDomain.CurrentDomain.GetAssemblies().
                SelectMany(Function(asm) asm.GetTypes()).
                Where(Function(t) t.FullName = n).
                FirstOrDefault()
         End Function
        )
    End Function

    ''' <summary>
    ''' Gets the value of an attribute attached to a given enum value
    ''' </summary>
    ''' <typeparam name="T">The type of attribute to look for and return</typeparam>
    ''' <param name="enu">The enum value from where to draw the attribute</param>
    ''' <returns>The instance of the attribute associated with the given enum value
    ''' or null if no such attribute was found.</returns>
    Public Shared Function GetAttributeValue(Of T As {Attribute})(
     ByVal enu As [Enum]) As T
        Return DirectCast(GetAttributeValue(GetType(T), enu), T)
    End Function

    ''' <summary>
    ''' Gets the value of an attribute attached to a given enum value
    ''' </summary>
    ''' <param name="attrType">The type of attribute to look for and return</param>
    ''' <param name="enu">The enum value from where to draw the attribute</param>
    ''' <returns>The instance of the attribute associated with the given enum value
    ''' or null if no such attribute was found.</returns>
    Public Shared Function GetAttributeValue(
     attrType As Type, enu As [Enum]) As Attribute
        Dim info As FieldInfo = enu.GetType().GetField(enu.ToString())
        Dim attribs() As Object =
         info.GetCustomAttributes(attrType, False)
        If attribs.Length = 0 Then Return Nothing
        Return DirectCast(attribs(0), Attribute)
    End Function

    ''' <summary>
    ''' Checks if the given object is null, returning <paramref name="ifNullValue"/>
    ''' if it is. Otherwise, it returns the value given cast into the required type.
    ''' </summary>
    ''' <typeparam name="T">The type required.</typeparam>
    ''' <param name="obj">The object which either holds null or the value required.
    ''' </param>
    ''' <param name="ifNullValue">The value to return if the given object was null.
    ''' </param>
    ''' <returns>The given value cast into the specified type, or <paramref
    ''' name="ifNullValue"/> if the given value was null.</returns>
    ''' <remarks>This is primarily written to make ExecuteScalar calls a bit easier
    ''' to write. eg. rather than:
    ''' <code>
    ''' Dim obj as Object = con.ExecuteReturnScalar(cmd)
    ''' If obj Is Nothing Then Return Guid.Empty
    ''' Return DirectCast(obj, Guid)
    ''' </code>
    ''' ... you can now use :
    ''' <code>
    ''' Return IfNull(con.ExecuteReturnScalar(cmd), Guid.Empty)
    ''' </code>
    ''' but it could be used for the converse - the setting of parameters
    ''' cmd.Parameters.AddWithValue("@name", IfNull(name, "[empty]"))
    ''' </remarks>
    Public Shared Function IfNull(Of T)(ByVal obj As Object, ByVal ifNullValue As T) As T

        ' If it's nada, return the default value
        If (obj Is Nothing OrElse IsDBNull(obj)) Then Return ifNullValue

        ' If the type matches, just cast it.
        If TypeOf obj Is T Then Return DirectCast(obj, T)

        Try
            ' Otherwise try a CType() (effectively convert) call
            Return CType(obj, T)
            ' Runtime conversion is quite a large area, but I think this should
            ' handle the majority of our cases. See:
            ' http://stackoverflow.com/questions/312858/how-can-i-convert-types-at-runtime/7942350
            ' for further discussion / suggestions, if this doesn't cut it any more.

        Catch ice As InvalidCastException

            ' Try and parse it if the type has a parse method - this is slowsville, but it's
            ' handy if conversion fails. Only useful if obj is a String
            If TypeOf obj Is String Then
                Dim m As MethodInfo = GetType(T).GetMethod("Parse", New Type() {GetType(String)})
                If m IsNot Nothing Then Return DirectCast(m.Invoke(Nothing, New Object() {obj}), T)
            End If

            ' If there is no Parse method or obj isn't a string, just rethrow the exception.
            ' not much else we can do
            Throw

        End Try

    End Function

    ''' <summary>
    ''' Special case of IfNull to deal with the fact that Guids can only be parsed
    ''' by passing a string into the Guid constructor - it has no Parse() method.
    ''' </summary>
    ''' <param name="obj">The object to convert into a Guid</param>
    ''' <param name="ifNullValue">The Guid value to return if the given object is
    ''' null</param>
    ''' <returns>The given object parsed into a Guid, or the null value if the given
    ''' object was null.</returns>
    ''' <exception cref="InvalidCastException">If the type of the given object was
    ''' not recognised as one which could be converted into a GUID.</exception>
    ''' <exception cref="FormatException">If the given string object could not be
    ''' parsed into a GUID</exception>
    Public Shared Function IfNull(ByVal obj As Object, ByVal ifNullValue As Guid) _
     As Guid
        ' If it's nada, return the default value
        If (obj Is Nothing OrElse IsDBNull(obj)) Then Return ifNullValue

        If TypeOf obj Is Guid Then Return DirectCast(obj, Guid)
        If TypeOf obj Is String Then Return New Guid(DirectCast(obj, String))
        ' We never use this, but what the hey...
        If TypeOf obj Is Byte() Then Return New Guid(DirectCast(obj, Byte()))

        Throw New InvalidCastException(
         String.Format(My.Resources.BPUtil_CannotConvert01IntoAGuid,
         obj, obj.GetType().ToString()))

    End Function

    ''' <summary>
    ''' Checks if the given match term matches the given text given the specified
    ''' conditions.
    ''' </summary>
    ''' <param name="text">The text to search in.</param>
    ''' <param name="matchTerm">The match term to apply to the text.</param>
    ''' <param name="isPartial">True to indicate that a partial match should
    ''' count as a match - ie. this function should return true if the match term
    ''' occurs within the text; False to indicate that the search term should be
    ''' a full match for the text - ie. that the text and the matchTerm represent
    ''' the same text.
    ''' </param>
    ''' <param name="caseSens">True to perform the match test case sensitively;
    ''' False to ignore case.</param>
    ''' <returns>True if the match term was found to be a match for the text;
    ''' False otherwise.</returns>
    ''' <remarks>If either of the given text value is null, this is not considered
    ''' a match, including the case where both text values are null.</remarks>
    Public Shared Function IsMatch(ByVal text As String,
     ByVal matchTerm As String, ByVal isPartial As Boolean, ByVal caseSens As Boolean) As Boolean

        ' Any nulls? Throw the toys out of the pram.
        If text Is Nothing OrElse matchTerm Is Nothing Then Return False

        ' Get the appropriate StringComparison value depending on case-sensitivity
        Dim comp As StringComparison = StringComparison.InvariantCultureIgnoreCase
        If caseSens Then comp = StringComparison.InvariantCulture

        ' Partial match, just check for the existence of the match term in the text
        If isPartial Then Return (text.IndexOf(matchTerm, comp) <> -1)

        ' Full match - check that it equals the text
        Return text.Equals(matchTerm, comp)

    End Function

    ''' <summary>
    ''' Very simplistic swap method to swap two values if a condition is met
    ''' </summary>
    ''' <typeparam name="T">The type of objects being swapped</typeparam>
    ''' <param name="condition">If true, swaps the values given; If false, this
    ''' acts as a no-op</param>
    ''' <param name="a">The first element to potentially swap</param>
    ''' <param name="b">The second element to potentially swap</param>
    Public Shared Sub SwapIf(Of T)(
     ByVal condition As Boolean, ByRef a As T, ByRef b As T)
        If condition Then
            Dim tmp As T = a
            a = b
            b = tmp
        End If
    End Sub

    ''' <summary>
    ''' Checks if the given number has precisely one bit set
    ''' </summary>
    ''' <param name="val">The value to check for a single bit being set</param>
    ''' <returns>True if the given value had exactly one bit set; False if it had 0
    ''' or more than one set.</returns>
    Public Shared Function HasSingleBitSet(ByVal val As Long) As Boolean
        Return (val = Long.MinValue OrElse (val <> 0 AndAlso (val And (val - 1)) = 0))
    End Function

    ''' <summary>
    ''' Checks if the given number has more than one bit set
    ''' </summary>
    ''' <param name="val">The value to check for multiple set bits</param>
    ''' <returns>True if the given value has multiple bits set in it; False otherwise
    ''' </returns>
    Public Shared Function HasMultipleBitsSet(ByVal val As Long) As Boolean
        Return (val <> Long.MinValue AndAlso (val And val - 1) <> 0)
    End Function

    ''' <summary>
    ''' Shorthand way of marshalling an unmanaged memory location into a managed
    ''' structure. This simply wraps <see cref="Marshal.PtrToStructure"/> into a
    ''' generic method.
    ''' </summary>
    ''' <typeparam name="T">The type of struct to marshal. This type must represent a
    ''' formatted non-generic structure.</typeparam>
    ''' <param name="ptr">A pointer to an unmanaged block of memory</param>
    ''' <returns>A managed object containing the data pointed to by the ptr parameter
    ''' </returns>
    ''' <exception cref="ArgumentException">The structure parameter layout is not
    ''' sequential or explicit, or it is a generic type</exception>
    Public Shared Function PtrToStructure(Of T As {Structure})(
     ByVal ptr As IntPtr) As T
        Return DirectCast(Marshal.PtrToStructure(ptr, GetType(T)), T)
    End Function

    ''' <summary>
    ''' Gets the process which owns the current foreground window, or null if there
    ''' was no foreground window available or if the process which owned the window
    ''' closed while trying to retrieve the process details.
    ''' </summary>
    ''' <returns>The process object representing the process which owns the
    ''' foreground window</returns>
    ''' <remarks>The Process object returned should be disposed of by the caller.
    ''' Also, there exists a race condition here which might occur in between getting
    ''' the foreground window and getting the process details (if the process exits
    ''' in between the two points). If the race condition is hit, this method will
    ''' return null.</remarks>
    Public Shared Function GetForegroundProcess() As Process
        Dim procId As Integer
        GetWindowThreadProcessId(GetForegroundWindow(), procId)
        If procId = 0 Then Return Nothing
        Try
            Return Process.GetProcessById(procId)
        Catch ae As ArgumentException
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Gets the top level windows associated with the given process ID.
    ''' </summary>
    ''' <param name="pid">The process ID for which the top level windows are
    ''' required.</param>
    ''' <returns>A collection of window handles representing the top level windows
    ''' associated with the given process ID.</returns>
    Public Shared Function GetTopLevelWindows(pid As Integer) As ICollection(Of IntPtr)
        Dim hwnds As New List(Of IntPtr)
        EnumWindows(
            Function(hwnd)
                Dim windowPid As Integer
                GetWindowThreadProcessId(hwnd, windowPid)
                If windowPid = pid Then hwnds.Add(hwnd)
                Return True
            End Function)
        Return hwnds
    End Function

    ''' <summary>
    ''' Performs an action on all windows belonging to a process, in a depth first
    ''' manner across the top level windows owned by the process.
    ''' </summary>
    ''' <param name="pid">The process ID for which all the owned windows are to be
    ''' enumerated</param>
    ''' <param name="proc">The procedure to call for each of the enumerated windows;
    ''' the parameter is the window handle of the enumerated window. This should
    ''' return true to continue enumeration; false to halt enumeration</param>
    Public Shared Sub EnumAllWindows(pid As Integer, proc As Func(Of IntPtr, Boolean))
        EnumWindows(
            Function(tlHwnd)
                ' Check if this is the PID we're looking for - if not, move onto
                ' the next window
                Dim windowPid As Integer
                GetWindowThreadProcessId(tlHwnd, windowPid)
                If windowPid <> pid Then Return True

                ' This is our process, so we call the provided procedure on the
                ' top level window itself, then all its descendents.
                If Not proc(tlHwnd) Then Return False
                EnumChildWindows(tlHwnd, proc)
                Return True
            End Function
        )
    End Sub

    ''' <summary>
    ''' Checks if <paramref name="value"/> is a valid type name or identifier.
    ''' Taken partially from ILSpy with context added from
    ''' https://referencesource.microsoft.com/#System/compmod/system/codedom/compiler/CodeGenerator.cs
    ''' . Primarily here because MS, in their wisdom, saw fit to make this an
    ''' internal method rather than a public one (you can check for a valid
    ''' identifier publically, but not type name and I need a type name for my test).
    ''' </summary>
    ''' <param name="value">The value to test to see if it represents a valid type
    ''' name or identifier.</param>
    ''' <param name="isTypeName">True to check for valid type names as well</param>
    ''' <returns>True if the value represents a valid type name or identifier; false
    ''' otherwise.</returns>
    Public Shared Function IsValidTypeNameOrIdentifier(
     value As String, isTypeName As Boolean) As Boolean
        Dim nextMustBeStartChar As Boolean = True
        If value.Length = 0 Then
            Return False
        End If

        For Each c As Char In value
            Select Case Char.GetUnicodeCategory(c)
                Case UnicodeCategory.UppercaseLetter, UnicodeCategory.LowercaseLetter,
                 UnicodeCategory.TitlecaseLetter, UnicodeCategory.ModifierLetter,
                 UnicodeCategory.OtherLetter, UnicodeCategory.LetterNumber
                    nextMustBeStartChar = False

                Case UnicodeCategory.NonSpacingMark, UnicodeCategory.SpacingCombiningMark,
                 UnicodeCategory.DecimalDigitNumber, UnicodeCategory.ConnectorPunctuation
                    ' Underscore is a valid starting character, even though it is a ConnectorPunctuation.
                    If nextMustBeStartChar AndAlso c <> "_"c Then Return False
                    nextMustBeStartChar = False

                Case UnicodeCategory.EnclosingMark, UnicodeCategory.OtherNumber,
                 UnicodeCategory.SpaceSeparator, UnicodeCategory.LineSeparator,
                 UnicodeCategory.ParagraphSeparator, UnicodeCategory.Control,
                 UnicodeCategory.Format, UnicodeCategory.Surrogate, UnicodeCategory.PrivateUse
                    If Not isTypeName OrElse Not IsSpecialTypeChar(c, nextMustBeStartChar) Then
                        Return False
                    End If

                Case Else
                    If Not isTypeName OrElse Not IsSpecialTypeChar(c, nextMustBeStartChar) Then
                        Return False
                    End If
            End Select
        Next
        Return True
    End Function

    ''' <summary>
    ''' This can be a special character like a separator that shows up in a type name
    ''' This is an odd set of characters.Some come from characters that are allowed
    ''' by C++, like &lt; and &gt;. Others are characters that are specified in the
    ''' type and assembly name grammar
    ''' Taken from https://referencesource.microsoft.com/#System/compmod/system/codedom/compiler/CodeGenerator.cs
    ''' </summary>
    ''' <param name="ch">The character to check</param>
    ''' <param name="nextMustBeStartChar">On exit, true if the character following
    ''' <paramref name="ch"/> must be a 'start' character.</param>
    ''' <returns>True if <paramref name="ch"/></returns>
    ''' <remarks></remarks>
    Private Shared Function IsSpecialTypeChar(ch As Char, ByRef nextMustBeStartChar As Boolean) As Boolean
        Select Case ch
            Case ":"c, "."c, "$"c, "+"c, "<"c, ">"c, "-"c, "["c, "]"c, ","c, "&"c, "*"c
                nextMustBeStartChar = True
                Return True

            Case "`"c
                Return True

            Case Else
                Return False

        End Select
    End Function

#End Region

#Region " Text Utilities "

    ''' <summary>
    ''' Gets the character representing a password character on this system.
    ''' If the system is Windows XP or above it returns a dot (unicode char 9679)
    ''' Otherwise it returns a "*"
    ''' </summary>
    ''' <returns>The password character</returns>
    Public Shared ReadOnly Property PasswordChar() As Char
        Get
            With Environment.OSVersion.Version
                ' If ver is 5.1 + (ie. XP or later), use unicode, else use asterisk
                If (.Major = 5 AndAlso .Minor >= 1) OrElse .Major >= 6 Then
                    Return ChrW(9679)
                Else
                    Return "*"c
                End If
            End With
        End Get
    End Property

    ''' <summary>
    ''' Replaces \r \n and \\ in strings correctly, i.e. '\\n' will be converted
    ''' correctly into a single '\' and an 'n'
    ''' </summary>
    ''' <param name="unescapedText">The text to escape</param>
    Public Shared Function Unescape(ByVal unescapedText As String) As String
        Dim val As New StringBuilder
        Dim esc As Boolean = False
        For Each c As Char In unescapedText
            If esc Then
                Select Case c
                    Case "\"c : val.Append(c)
                    Case "r"c : val.Append(vbCr)
                    Case "n"c : val.Append(vbLf)
                    Case Else : Throw New InvalidValueException(My.Resources.BPUtil_InvalidEscapeCharacter0, c)
                End Select
                esc = False
            Else
                Select Case c
                    Case "\"c : esc = True
                    Case Else : val.Append(c)
                End Select
            End If
        Next
        If esc Then
            Throw New InvalidValueException(My.Resources.BPUtil_UnterminatedEscapeSequence)
        End If

        Return val.ToString()
    End Function

    ''' <summary>
    ''' Scans a string, replacing a set of characters with their replacements.
    ''' </summary>
    ''' <param name="str">The string to search and replace within</param>
    ''' <param name="replacements">A map, keyed on the characters to search for,
    ''' against the character that each should be replaced with</param>
    ''' <returns>If no characters were replaced, this will return the original string
    ''' reference, otherwise it will return a new string with all of the characters
    ''' replaced as specified in <paramref name="replacements"/></returns>
    Public Shared Function ReplaceAny(
     ByVal str As String, ByVal replacements As IDictionary(Of Char, Char)) As String
        ' Ignore empty or null strings - just return the original reference
        If str = "" Then Return str

        Dim replacedAny As Boolean = False
        Dim sb As New StringBuilder(str.Length)
        For Each c As Char In str
            Dim replaceWith As Char
            If replacements.TryGetValue(c, replaceWith) AndAlso c <> replaceWith Then
                replacedAny = True
                sb.Append(replaceWith)
            Else
                sb.Append(c)
            End If
        Next
        ' If we didn't find any chars that needed replacing, just return the original
        ' string, rather than our built up one (so that 'If returned IsNot orig...'
        ' checks work as expected)
        If Not replacedAny Then Return str Else Return sb.ToString()

    End Function

    ''' <summary>
    ''' Scans a string, replacing a set of characters with their replacements.
    ''' </summary>
    ''' <param name="str">The string to search and replace within</param>
    ''' <param name="replacements">A map, keyed on the strings to search for,
    ''' against the string that each should be replaced with</param>
    ''' <returns>If the string has not changed, this will return the original string
    ''' reference, otherwise it will return a new string with all of the characters
    ''' replaced as specified in <paramref name="replacements"/></returns>
    ''' <remarks>This will replace the first string it comes across in the
    ''' dictionary, such that (eg):
    '''  ReplaceAny("teststring", { {"te","et"}, {"test", "sett"}, {"teststr", ""} })
    ''' would return "etststring"
    '''  ReplaceAny("teststring", { {"test","sett"}, {"te", "et"}, {"teststr", ""} })
    ''' would return "settstring".
    ''' Also note that replacement strings are never tested against the replacements;
    ''' the string is replaced and then processing skips to the next character
    ''' <em>in the original string</em>, rather than the next character in the
    ''' string being built, eg.
    '''  ReplaceAny("aaaaa", { {"aa","bb"}, {"b","c"}, { "a","d"} })
    ''' would return "bbbbd"
    ''' </remarks>
    Public Shared Function ReplaceAny(ByVal str As String,
     ByVal replacements As IDictionary(Of String, String)) As String
        ' Ignore empty or null strings - just return the original reference
        If str = "" Then Return str

        Dim ind As Integer = 0
        Dim sb As New StringBuilder()
        Dim replacedAny As Boolean = False

        While ind < str.Length

            For Each searchKey As String In replacements.Keys
                ' Sanity check - searchKey exists?
                If searchKey = "" Then Continue For

                ' If the search key is more chars than we have left, next search key
                If ind + searchKey.Length > str.Length Then Continue For

                ' Get the substring to test against
                Dim substr As String = str.Substring(ind, searchKey.Length)
                ' If it's not the same, move onto the next search key
                If substr <> searchKey Then Continue For

                ' So the substring matches our search key, get the replacement
                Dim replaceWith As String = replacements(searchKey)
                ' If it's the same, skip the replacement - pointless, next search key
                If replaceWith = substr Then Continue For

                ' Otherwise append the replacement into the buffer, and skip to the
                ' next character in the original string
                sb.Append(replaceWith)
                ind += searchKey.Length
                replacedAny = True
                Continue While
            Next
            ' No string to replace found, just add the character and inc by 1
            sb.Append(str(ind))
            ind += 1

        End While

        ' If we didn't find any chars that needed replacing, just return the original
        ' string, rather than our built up one (so that 'If returned IsNot orig...'
        ' checks work as expected)
        If Not replacedAny Then Return str Else Return sb.ToString()

    End Function

    ''' <summary>
    ''' Scans a string, replacing a set of characters with their replacements.
    ''' </summary>
    ''' <param name="str">The string to search and replace within</param>
    ''' <param name="pairs">The strings in pairs to search for and replace with,
    ''' respectively. The first string in any pair will be the search key, the
    ''' second will be the replacement. If an odd number of strings is passed, an
    ''' exception will be thrown</param>
    ''' <returns>If the string has not changed, this will return the original string
    ''' reference, otherwise it will return a new string with all of the characters
    ''' replaced as specified in <paramref name="pairs"/></returns>
    ''' <remarks>This will replace the first string it comes across in the
    ''' dictionary, such that (eg):
    '''  ReplaceAny("teststring", { {"te","et"}, {"test", "sett"}, {"teststr", ""} })
    ''' would return "etststring"
    '''  ReplaceAny("teststring", { {"test","sett"}, {"te", "et"}, {"teststr", ""} })
    ''' would return "settstring".
    ''' Also note that replacement strings are never tested against the replacements;
    ''' the string is replaced and then processing skips to the next character
    ''' <em>in the original string</em>, rather than the next character in the
    ''' string being built, eg.
    '''  ReplaceAny("aaaaa", { {"aa","bb"}, {"b","c"}, { "a","d"} })
    ''' would return "bbbbd"
    ''' </remarks>
    ''' <exception cref="InvalidArgumentException">If an odd number of strings was
    ''' passed in the <paramref name="pairs"/> parameter.</exception>
    Public Shared Function ReplaceAny(
     ByVal str As String, ByVal ParamArray pairs() As String) As String
        If pairs.Length Mod 2 > 0 Then Throw New InvalidArgumentException(
         "ReplaceAny() called with {0} pair argument(s) - must be even number",
         pairs.Length)
        Dim map As New clsOrderedDictionary(Of String, String)
        For i As Integer = 0 To pairs.Length - 2 Step 2
            map(pairs(i)) = pairs(i + 1)
        Next
        Return ReplaceAny(str, map)
    End Function

    ''' <summary>
    ''' Finds a unique string, based on a stub string
    ''' </summary>
    ''' <param name="stub">The stub name with a positional parameter 0 defined as
    ''' described in <see cref="String.Format"/> (eg. "Name {0}"). Other positional
    ''' parameters can be used - the arguments defined in <paramref name="args"/>
    ''' will be used to populate the parameters from "{1}" onwards.</param>
    ''' <param name="exists">The predicate to determine if a name exists already or
    ''' not. Note that this should return true <em>if the name exists</em>, not if it
    ''' is unique.</param>
    ''' <param name="args">Remaining arguments to be used in the format string
    ''' defined in <paramref name="stub"/></param>
    ''' <returns>The first unique name found using an incremental integer, starting
    ''' from 1, or null if a unique name could not be found with an integer in the
    ''' range 1-<see cref="Integer.MaxValue"/> (though the application will have hung
    ''' for so long by that point, the user will probably have terminated it).
    ''' </returns>
    Public Shared Function FindUnique(
     stub As String, exists As Predicate(Of String), ParamArray args() As Object) _
     As String
        Return FindUnique(stub, False, exists, args)
    End Function

    ''' <summary>
    ''' Finds a unique string, based on a stub string
    ''' </summary>
    ''' <param name="stub">The stub name with a positional parameter 0 defined as
    ''' described in <see cref="String.Format"/> (eg. "Name {0}"). Other positional
    ''' parameters can be used - the arguments defined in <paramref name="args"/>
    ''' will be used to populate the parameters from "{1}" onwards.</param>
    ''' <param name="checkNoNumberFirst">True to check without any number (ie. with
    ''' nothing in the {0} placeholder) - note that this will cause the string to
    ''' be trimmed of whitespace before being tested - eg. <c>"Thingy {0}"</c> will
    ''' first be checked as "Thingy" not as "Thingy " with a trailing space char.
    ''' False to skip the no number check and start with 1.</param>
    ''' <param name="exists">The predicate to determine if a name exists already or
    ''' not. Note that this should return true <em>if the name exists</em>, not if it
    ''' is unique.</param>
    ''' <param name="args">Remaining arguments to be used in the format string
    ''' defined in <paramref name="stub"/></param>
    ''' <returns>The first unique name found using an incremental integer, starting
    ''' from 1, or null if a unique name could not be found with an integer in the
    ''' range 1-<see cref="Integer.MaxValue"/> (though the application will have hung
    ''' for so long by that point, the user will probably have terminated it).
    ''' </returns>
    Public Shared Function FindUnique(
     stub As String,
     checkNoNumberFirst As Boolean,
     exists As Predicate(Of String),
     ParamArray args() As Object) As String
        ' Create an array 1 element larger than the 'args' array to hold both the
        ' current incremental number (at [0]) and the rest of the args (at [1+])
        Dim arr(args.Length) As Object
        args.CopyTo(arr, 1)

        For i = 0 To Integer.MaxValue
            Dim potential As String
            ' If first time in the loop, test with no number in the potential name
            ' unless that behaviour is inhibited by (checkNoNumberFirst == false)
            If i = 0 Then
                ' Ignore the i==0 iteration if we're not checking the no number name
                If Not checkNoNumberFirst Then Continue For
                ' Format with a null in arr(0) and trim the result
                potential = String.Format(stub, arr).Trim()
            Else
                ' Otherwise, format with the index as the first elem in the array
                arr(0) = i
                potential = String.Format(stub, arr)
            End If
            If Not exists(potential) Then Return potential
        Next
        Return Nothing
    End Function

#End Region

#Region " Font Utilities "

    ''' <summary>
    ''' Iterate the available fonts on the system and return the most favorable font requested
    ''' in the paramters
    ''' </summary>
    ''' <param name="names">The names of the font in order of preference</param>
    Private Shared Function AvailableFonts(ByVal ParamArray names() As String) As String
        Dim i As New InstalledFontCollection
        For Each s As String In names
            For Each f As FontFamily In i.Families
                If f.Name = s Then
                    Return s
                End If
            Next
        Next
        Return FontFamily.GenericSansSerif.Name
    End Function

    Private Shared mAvailableFont As String

    ''' <summary>
    ''' Return the most favorable available font. This method is lazy initialised.
    ''' </summary>
    Public Shared Function AvailableFont() As String
        If String.IsNullOrEmpty(mAvailableFont) Then
            mAvailableFont = AvailableFonts("Segoe UI", "Arial")
        End If
        Return mAvailableFont
    End Function

#End Region

#Region " Date/Time Utilities "

    ''' <summary>
    ''' Converts the given date/time to local time and returns a display value
    ''' for it. If the given date/time is a Min/Max Value, this will return an
    ''' empty string.
    ''' </summary>
    ''' <param name="dt">The date to convert and format</param>
    ''' <returns>A display form of the date as a string; empty string if the given
    ''' date was a boundary date.</returns>
    ''' <remarks>If <paramref name="dt"/> has a <see cref="DateTime.Kind"/>
    ''' indicating that it is already a local date/time, its value is not modified.
    ''' </remarks>
    Public Shared Function ConvertAndFormatUtcDateTime(dt As Date) As String
        If dt = Date.MinValue OrElse dt = DateMinValueUtc Then Return ""
        If dt = Date.MaxValue OrElse dt = DateMaxValueUtc Then Return ""
        Return dt.ToLocalTime().ToString()
    End Function

    ''' <summary>
    ''' Convert a datetime/timezone pair into a DateTimeOffset, taking care to
    ''' handle legacy datetimes that do not have timezone info.
    ''' </summary>
    ''' <param name="prov">The Data Provider</param>
    ''' <param name="dateColumn">The DateTime column to use</param>
    ''' <param name="offsetColumn">The Offset column to use</param>
    Public Shared Function ConvertDateTimeOffset(prov As IDataProvider, dateColumn As String, offsetColumn As String) As DateTimeOffset

        Dim dateValue = prov(dateColumn)
        Dim offsetValue = prov(offsetColumn)

        Dim d As DateTime? = If(dateValue Is Nothing, Nothing, CDate(dateValue))
        Dim o As Integer? = If(offsetValue Is Nothing, Nothing, CInt(offsetValue))

        Return ConvertDateTimeOffset(d, o)

    End Function


    ''' <summary>
    ''' Convert a datetime/timezone pair into a DateTimeOffset, taking care to
    ''' handle legacy datetimes that do not have timezone info.
    ''' </summary>
    ''' <param name="d">The datetime to use</param>
    ''' <param name="o">The offset to use</param>
    Public Shared Function ConvertDateTimeOffset(d As DateTime?, o As Integer?) As DateTimeOffset
        Try
            If Not d.HasValue Then
                Return DateTimeOffset.MinValue
            ElseIf Not o.HasValue Then
                Return New DateTimeOffset(d.Value)
            Else
                Return New DateTimeOffset(d.Value, TimeSpan.FromSeconds(o.Value))
            End If
        Catch
            'In some very edge cases such as upgrading from an old DB which had
            'sessions that had not completed and then switching timezones, we
            'could potentially fail. So rather than throw exceptions we just
            'return DateTimeOffset.MinValue
            Return DateTimeOffset.MinValue
        End Try
    End Function

#End Region

End Class
