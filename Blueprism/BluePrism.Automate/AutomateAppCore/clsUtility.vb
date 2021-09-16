Imports System.Drawing
Imports System.Text.RegularExpressions
Imports System.Net
Imports System.Net.NetworkInformation
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

''' Project  : AutomateAppCore
''' Class    : clsUtility
''' 
''' <summary>
''' A ragtag band of utility methods providing shared maths, graphics, date 
''' and string functions. Most of these shouldn't even exist. Those that should
''' are almost certainly in the wrong place.
''' </summary>
Public Class clsUtility

#Region "Maths Utilities"

    ''' Project  : Automate
    ''' Class    : clsUtility.MathsUtil
    ''' 
    ''' <summary>
    ''' A maths utilty class.
    ''' </summary>
    Public Class MathsUtil

        ''' <summary>
        ''' Calculates the distance between the two specified points.
        ''' </summary>
        ''' <param name="x1">The X coordinate of the first of the two points.</param>
        ''' <param name="y1">The Y coordinate of the first of the two points.</param>
        ''' <param name="x2">The X coordinate of the second of the two points.</param>
        ''' <param name="y2">The Y coordinate of the second of the two points.</param>
        ''' <returns>Returns the distance between the two specified points.</returns>
        Public Shared Function GetDistanceBetweenTwoPoints(ByVal x1 As Integer, ByVal y1 As Integer, ByVal x2 As Integer, ByVal y2 As Integer) As Double
            Return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2))
        End Function

        ''' <summary>
        ''' Returns the minimum of three doubles.
        ''' </summary>
        ''' <param name="a">The first of three doubles</param>
        ''' <param name="b">The second of three doubles</param>
        ''' <param name="c">The third of three doubles</param>
        ''' <returns>Returns the minimum of the three doubles.</returns>
        Public Shared Function Min(ByVal a As Double, ByVal b As Double, ByVal c As Double) As Double
            Return Math.Min(a, Math.Min(b, c))
        End Function

        ''' <summary>
        ''' Returns the maximum of three doubles.
        ''' </summary>
        ''' <param name="a">The first of three doubles</param>
        ''' <param name="b">The second of three doubles</param>
        ''' <param name="c">The third of three doubles</param>
        ''' <returns>Returns the maximum of the three doubles.</returns>
        Public Shared Function Max(ByVal a As Double, ByVal b As Double, ByVal c As Double) As Double
            Return Math.Max(a, Math.Max(b, c))
        End Function

#Region "Modular Arithmetic"

        ''' <summary>
        ''' Takes the absolute difference between two integers, reduces is by the
        ''' supplied modulus and returns the minimum distance round the circle to zero.
        ''' </summary>
        ''' <param name="x">The first of the two numbers to subtract.</param>
        ''' <param name="y">The second of the two numbers to subtract.</param>
        ''' <param name="Modulus">The modulus.</param>
        ''' <returns>Takes the absolute difference between two integers, reduces
        ''' is by the supplied modulus and returns the minimum distance round the
        ''' circle to zero.</returns>
        Public Shared Function ModularAbsDifference(ByVal x As Integer, ByVal y As Integer, ByVal Modulus As Integer) As Integer
            'check for witty programers
            If Not Modulus >= 2 Then
                Throw New InvalidArgumentException("Parameter Modulus must be at least 2")
            End If

            'return the minimum distance between the integers
            Return Math.Abs(ModularHalfRange(x - y, Modulus))
        End Function

        ''' <summary>
        ''' Reduces an integer by the specified modulus and ensures it is in the positive
        ''' range.
        ''' </summary>
        ''' <param name="x">The number to reduce.</param>
        ''' <param name="Modulus">The modulus.</param>
        ''' <returns>Reduces an integer by the specified modulus and ensures
        ''' it is in the positive range.</returns>
        Public Shared Function PositiveMod(ByVal x As Integer, ByVal Modulus As Integer) As Integer
            'check for witty programers
            If Not Modulus >= 2 Then
                Throw New InvalidArgumentException("Parameter Modulus must be at least 2")
            End If

            x = x Mod Modulus
            If x < 0 Then x += Modulus

            Return x
        End Function

        ''' <summary>
        ''' Reduces an integer by the specified modulus and ensures it is in the range
        ''' -modulus\2 &lt; result &lt;= modulus \ 2
        ''' </summary>
        ''' <param name="x">The number to reduce.</param>
        ''' <param name="Modulus">The modulus.</param>
        ''' <returns>Reduces an integer by the specified modulus and ensures it is
        ''' in the range -modulus\2 &lt; result &lt;= modulus \ 2</returns>
        Public Shared Function ModularHalfRange(ByVal x As Integer, ByVal Modulus As Integer) As Integer
            'check for witty programers
            If Not Modulus >= 2 Then
                Throw New InvalidArgumentException("Parameter Modulus must be at least 2")
            End If

            x = x Mod Modulus
            If x <= -Modulus \ 2 Then x += Modulus
            If x > Modulus \ 2 Then x -= Modulus

            Return x
        End Function

#End Region

    End Class

#End Region

#Region "String Utilities"

    ''' <summary>
    ''' Returns a string that is a single line and truncated to 50 characters.
    ''' </summary>
    ''' <param name="sIncomingText">The text to truncate</param>
    ''' <returns>The truncated text</returns>
    Public Shared Function TruncateMultiLine(ByVal sIncomingText As String) As String

        If sIncomingText.Length > 50 Then
            sIncomingText = sIncomingText.Substring(0, 50) & "..."
        End If
        sIncomingText = sIncomingText.Replace(vbCr, String.Empty)
        sIncomingText = sIncomingText.Replace(vbLf, String.Empty)
        Return sIncomingText
    End Function

    ''' <summary>
    ''' Formats text to fit into a given number of columns by adding carriage returns
    ''' at appropriate places in the text.
    ''' </summary>
    ''' <param name="Text">The text to be formatted.</param>
    ''' <param name="columnwidth">The maximum number of characters to be placed
    ''' on a line.</param>
    ''' <returns>Returns the number of lines spanned by the text.</returns>
    Public Shared Function FormatTextAsFixedWidth(ByRef Text As String, ByVal columnwidth As Integer) As Integer

        Dim iTextLength As Integer = Len(Text)
        Dim sStringToReturn As String = ""
        Dim Anchor As Integer = 0
        Dim Creeper As Integer

        While Anchor < iTextLength
            Creeper = columnwidth            'the amount we are about to chop off text for a new line
            If Len(Text) <= columnwidth Then
                Creeper = Len(Text)
            Else
                While Not Text.Mid(Creeper, 1) = " "                 'if we didn't end on a space then creep backwards till we find one
                    Creeper -= 1
                    If Creeper < 1 Then
                        Creeper = columnwidth - 1                        'but if we don't find one then just take a full row's worth. Later we add hyphen in this case
                        Exit While
                    End If
                End While
            End If
            sStringToReturn &= Text.Mid(1, Creeper) & vbCrLf
            If Creeper = columnwidth - 1 And Not Len(Text) = Creeper Then Text &= "-"
            Text = Text.Mid(Creeper + 1, iTextLength)          'this will not cause exception if Anchor too big - will return empty string
            Anchor += Creeper
        End While

        If sStringToReturn.Mid(Len(sStringToReturn) - 1, 2) = vbCrLf Then
            'strip trailing carriage return
            Text = sStringToReturn.Mid(1, Len(sStringToReturn) - 2)
        Else
            Text = sStringToReturn
        End If

        'count the number of lines in Text:
        Dim regex As New System.Text.RegularExpressions.Regex("\r\n", System.Text.RegularExpressions.RegexOptions.Multiline)
        Return regex.Matches(Text).Count + 1

    End Function

    ''' <summary>
    ''' Replaces single quotes with a double quotes.
    ''' </summary>
    ''' <param name="s">The original string</param>
    ''' <returns>An escaped string</returns>
    Public Shared Function EscapeQuotes(ByVal s As String) As String
        If Not s Is Nothing Then
            Return s.Replace("'", "''")
        Else
            Return ""
        End If
    End Function

    ''' <summary>
    ''' Inserts a space between the camel-case 'joins'.
    ''' </summary>
    ''' <param name="camel">The original string</param>
    ''' <returns>A string with spaces inserted</returns>
    Public Shared Function GetUnCamelled(ByVal camel As String) As String
        Return Regex.Replace(camel, "[a-z][A-Z]", AddressOf InsertSpace)
    End Function

    ''' <summary>
    ''' Takes a string formatted with parameter escape sequences and replaces them
    ''' with "..."
    ''' e.g. "The example was {0}" becomes "The example was ..."
    ''' </summary>
    ''' <param name="formatted">The original string</param>
    ''' <returns>A string with "..." inserted</returns>
    Public Shared Function GetUnformattedString(ByVal formatted As String) As String
        Return Regex.Replace(formatted, "{\d+}", "...")
    End Function

    Private Shared Function InsertSpace(ByVal match As Match) As String
        Return match.ToString().Chars(0) & " " & match.ToString().Chars(1)
    End Function

    ''' <summary>
    ''' Removes spaces and changes a string to camel-case.
    ''' </summary>
    ''' <param name="uncamelled">The string</param>
    Public Shared Sub MakeCamel(ByRef uncamelled As String)
        uncamelled = Regex.Replace(uncamelled, "\w\s\w", AddressOf RemoveSpace)
    End Sub

    ''' <summary>
    ''' Returns a camel-case string with no spaces.
    ''' </summary>
    ''' <param name="uncamelled"></param>
    ''' <returns></returns>
    Public Shared Function GetCamelled(ByVal uncamelled As String) As String
        Return Regex.Replace(uncamelled, "\w\s\w", AddressOf RemoveSpace)
    End Function

    Private Shared Function RemoveSpace(ByVal match As Match) As String
        Return match.ToString().Chars(0) & match.ToString().Chars(2)
    End Function


    ''' <summary>
    ''' Converts a timespan object into a friendly string. Includes all units even if
    ''' the timespan is small. THis way the result has a fixed width.
    ''' </summary>
    ''' <param name="t">The timespan to be converted.</param>
    ''' <returns>Returns a string representing the timespan in an intuitive human form
    ''' eg "2 Days, 3 Hrs, 45 Mins, 9 Secs"</returns>
    Public Shared Function CreateFriendlyFixedWidthTimespanStringFromTimespan(ByVal t As TimeSpan) As String
        Return t.Days.ToString.PadLeft(5, " "c) & " Days, " & t.Hours.ToString.PadLeft(2, " "c) & " Hrs, " & t.Minutes.ToString.PadLeft(2, " "c) & " Mins, " & t.Seconds.ToString.PadLeft(2, " "c) & " Secs"
    End Function


    ''' <summary>
    ''' Converts a timespan object into a friendly string.
    ''' </summary>
    ''' <param name="t">The timespan to be converted.</param>
    ''' <returns>Returns a string representing the timespan in an intuitive human form
    ''' eg "2 Days, 3 Hrs, 45 Mins, 9 Secs"</returns>
    Public Shared Function CreateFriendlyTimespanStringFromTimespan(ByVal t As TimeSpan) As String
        Dim stemp As String = ""
        Dim bPreceedingCommaNeeded As Boolean = False

        If t.Days > 0 Then
            stemp &= t.Days & " Days"
            bPreceedingCommaNeeded = True
        End If

        If t.Hours > 0 Then
            If bPreceedingCommaNeeded Then stemp &= ", "
            stemp &= t.Hours & " Hrs"
            bPreceedingCommaNeeded = True
        End If

        If t.Minutes > 0 Then
            If bPreceedingCommaNeeded Then stemp &= ", "
            stemp &= t.Minutes & " Mins"
            bPreceedingCommaNeeded = True
        End If

        If t.Seconds > 0 Then
            If bPreceedingCommaNeeded Then stemp &= ", "
            stemp &= t.Seconds & " Secs"
        Else
            'if nothing yet written then had better
            'put something in. This will do:
            If Not bPreceedingCommaNeeded Then stemp = "0 Secs"
        End If

        Return stemp
    End Function

    ''' <summary>
    ''' Interprets a friendly timespan string as a timespan and returns the
    ''' corresponding value.
    ''' 
    ''' An exception is thrown if the value cannot be interpreted as a friendly
    ''' timespan string of the sort returned by the methods named in the 
    ''' comments for the parameter 'sFriendlyTimespanString'.
    ''' </summary>
    ''' <param name="sFriendlyTimespanString">The string representation of the
    ''' timespan. Must be of the form returned by the methods
    ''' CreateFriendlyTimespanStringFromTimespan and
    ''' CreateFriendlyFixedWidthTimespanStringFromTimespan.
    ''' 
    ''' For example "3 Days, 22 Hrs, 1 Mins, 54 Secs" and "2 Days, 4 Mins"
    ''' are both valid arguments.
    '''  </param>
    ''' <returns>Returns a timespan representing the length of time
    ''' described by the friendly string. If the string cannot be interpreted then
    ''' an exception is thrown.</returns>
    Public Shared Function ParseFriendlyTimespanStringAsTimespan(ByVal sFriendlyTimespanString As String) As TimeSpan

        Dim R As New Regex("^\s*((?<days>\d+) [Dd]ays,?)?\s*((?<hrs>\d+) [Hh]rs,?)?\s*((?<mins>\d+) [Mm]ins,?)?\s*((?<secs>\d+) [Ss]ecs)?\s*$")
        Dim m As Match = R.Match(sFriendlyTimespanString)

        If m.Success Then
            Try
                Dim i(3) As Integer
                i(0) = CType(IIf(m.Groups("days").Success, m.Groups("days").Value, 0), Integer)
                i(1) = CType(IIf(m.Groups("hrs").Success, m.Groups("hrs").Value, 0), Integer)
                i(2) = CType(IIf(m.Groups("mins").Success, m.Groups("mins").Value, 0), Integer)
                i(3) = CType(IIf(m.Groups("secs").Success, m.Groups("secs").Value, 0), Integer)

                Return New TimeSpan(i(0), i(1), i(2), i(3))
            Catch ex As Exception
                Throw New InvalidOperationException("Failed to parse string as timespan")
            End Try
        End If

        Throw New InvalidOperationException("Failed to parse string as timespan")
    End Function

    ''' <summary>
    ''' Attempts to parse the given string into a timespan, returning its success
    ''' </summary>
    ''' <param name="str">The string to parse</param>
    ''' <param name="span">The out parameter to which the timespan should be written.
    ''' If the string fails to parse, this will contain <see cref="TimeSpan.Zero"/>
    ''' when the method returns. </param>
    ''' <returns>True to indicate that the value was successfully parsed; False
    ''' to indicate otherwise.</returns>
    Public Shared Function TryParse( _
     ByVal str As String, ByRef span As TimeSpan) As Boolean

        ' Default value if the Try fails.
        span = TimeSpan.Zero

        ' Let's get this out of the way straight away
        If str = "" Then Return False

        ' TimeSpan parses 1:02 as 1hr 2mins, we want it to be 1min 2secs, so
        ' pad out the string
        While str.Split(":"c).Length < 3
            str = "0:" & str
        End While

        If TimeSpan.TryParse(str, span) Then Return True
        Try
            span = ParseFriendlyTimespanStringAsTimespan(str)
            Return True

        Catch
            Return False

        End Try

    End Function

    ''' <summary>
    ''' Gets the cardinal suffix appropriate to the given number, e.g. 'nd' for 122 
    ''' or 'th' for 19.
    ''' </summary>
    ''' <param name="int">The number</param>
    ''' <returns>The suffix</returns>
    Public Shared Function GetCardinalNumberFromInteger(ByVal int As Integer) As String
        Dim sInt As String

        Try
            sInt = CStr(int)
            If sInt.Length >= 2 AndAlso sInt.Substring(sInt.Length - 2, 1) = "1" Then
                'number is in the 'teens' eg 13, 217 etc
                Return "th"
            Else
                Select Case CChar(sInt.Substring(sInt.Length - 1, 1))
                    Case "1"c
                        Return "st"
                    Case "2"c
                        Return "nd"
                    Case "3"c
                        Return "rd"
                    Case Else
                        Return "th"
                End Select
            End If
        Catch ex As Exception
            Return ""
        End Try
    End Function

    ''' <summary>
    ''' Gets the numerical value of a cardinal string, e.g. 19 from '19th'.
    ''' </summary>
    ''' <param name="cardinal">The string</param>
    ''' <param name="int">The number</param>
    ''' <returns>True if successful</returns>
    Public Shared Function MakeIntegerFromCardinalNumber(ByVal cardinal As String, ByRef int As Integer) As Boolean
        cardinal.Replace(",", "")        'remove commas
        Dim R As New Regex("^\d+")        'we want to greedily find all digits at start
        Dim M As Match = R.Match(cardinal)
        If (Not M Is Nothing) AndAlso M.Success Then
            Try
                int = CInt(M.Value)
                Return True
            Catch ex As Exception
                Return False
            End Try
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' See overload description. Defaults to including column headers.
    ''' </summary>
    ''' <param name="dt">The datatable.</param>
    ''' <returns>Returns html representation of datatable.</returns>
    Public Shared Function GenerateHtmlTableFromDatatable(ByVal dt As DataTable) As Xml.XmlDocument
        Return GenerateHtmlTableFromDatatable(dt, False)
    End Function

    ''' <summary>
    ''' Turns a datatable into an html table. Each cell in the data table is treated
    ''' as text.
    ''' </summary>
    ''' <param name="dt">The datatable to process. If this is null, the function
    ''' simply returns null.</param>
    ''' <returns>Returns a datatable as described in the summary.</returns>
    Public Shared Function GenerateHtmlTableFromDatatable(ByVal dt As DataTable, ByVal ExcludeColumnHeaders As Boolean) As Xml.XmlDocument
        If Not dt Is Nothing AndAlso dt.Rows.Count > 0 Then
            Dim xdoc As New Xml.XmlDocument
            Dim e1, e2, e3 As Xml.XmlElement

            e1 = xdoc.CreateElement("html")
            e2 = xdoc.CreateElement("body")
            e1.AppendChild(e2)
            e2 = xdoc.CreateElement("table")
            e1.AppendChild(e2)

            If Not ExcludeColumnHeaders Then
                e2 = xdoc.CreateElement("tr")
                For i As Integer = 0 To dt.Columns.Count - 1
                    e3 = xdoc.CreateElement("td")
                    e3.AppendChild(xdoc.CreateTextNode(dt.Columns(i).ColumnName))
                    e2.AppendChild(e3)
                Next
                e1.AppendChild(e2)
            End If

            For i As Integer = 0 To dt.Rows.Count - 1
                e2 = xdoc.CreateElement("tr")
                For j As Integer = 0 To dt.Columns.Count - 1
                    e3 = xdoc.CreateElement("td")
                    e3.AppendChild(xdoc.CreateTextNode(dt.Rows(i)(j).ToString))
                    e2.AppendChild(e3)
                Next
                e1.AppendChild(e2)
            Next
            xdoc.AppendChild(e1)

            Return xdoc
        Else
            Return Nothing
        End If
    End Function

    Public Shared Function MeasureString(font As Font, text As String) As Integer
        Using image = New Bitmap(1, 1)
            Using g = Graphics.FromImage(image)
                Return CType(Math.Ceiling(g.MeasureString(text, font).Width), Integer)
            End Using
        End Using
    End Function

#End Region

#Region "Date Utilities"

    ''' <summary>
    ''' Gets the later date of the two given dates.
    ''' </summary>
    ''' <param name="d1">The first date to test</param>
    ''' <param name="d2">The second date to test</param>
    ''' <returns>The maximum value of the two dates.</returns>
    Public Shared Function Max(ByVal d1 As Date, ByVal d2 As Date) As Date
        If d1 > d2 Then Return d1
        Return d2
    End Function

#End Region

#Region "GetOrder"

    ''' <summary>
    ''' Gets the string value of a cardinal number, e.g. 'twenty third' for 23.
    ''' </summary>
    ''' <param name="i">The number</param>
    ''' <returns>The cardinal string</returns>
    Public Shared Function GetOrder(ByVal i As Integer) As String

        Dim aUnits As String() = New String() {"", "first", "second", "third", "fourth", "fifth", "sixth", "seventh", "eighth", "ninth"}
        Dim aTeens As String() = New String() {"tenth", "eleventh", "twelfth", "thirteenth", "fourteenth", "fifteenth", "sixteenth", "seventeenth", "eighteenth", "ninteenth"}
        Dim aTens As String() = New String() {"twenty", "thirty", "forty", "fifty", "sixty", "seventy", "eighty", "ninety"}
        Dim iUnits, iTens As Integer

        If i < 1 Then
            Return ""
        Else
            If i < 10 Then
                Return aUnits(i)
            Else
                If i < 20 Then
                    Return aTeens(i)
                Else
                    If i < 100 Then
                        iTens = i \ 10
                        iUnits = i - iTens * 10
                        If iUnits = 0 Then
                            Return aTens(i).Replace("ty", "tieth")
                        Else
                            Return aTens(i) & " " & aUnits(i)
                        End If
                    Else
                        Return ""
                    End If
                End If
            End If
        End If

    End Function

#End Region

#Region "RemoveFromCollectionByName"
    ''' <summary>
    ''' Searches for and removes an object from a collection.
    ''' </summary>
    ''' <param name="col">The collection</param>
    ''' <param name="objsought">The object</param>
    ''' <returns>The position of the object</returns>
    Public Shared Function RemoveFromCollectionByName(ByRef col As Collection, ByVal objsought As Object) As Integer
        Dim Index As Integer = 1
        For Each Item As Object In col
            If Item.ToString = objsought.ToString Then
                col.Remove(Index)
                Exit For
            End If
            Index = Index + 1
        Next
    End Function
#End Region

#Region "Integer Bitwise Operations"

    ''' <summary>
    ''' Counts the number of ones in the binary representation of the integer.
    ''' </summary>
    ''' <param name="val">The value to count the set bits in.</param>
    ''' <returns>The number of set bits in the given value</returns>
    Public Shared Function CountSetBits(ByVal val As Long) As Integer
        Dim count As Integer = 0
        While val <> 0
            If (1L And val) > 0 Then count += 1
            val = val >> 1
        End While
        Return count
    End Function

    ''' <summary>
    ''' Returns the (1-based) position within the given value of the highest set bit
    ''' </summary>
    ''' <param name="val">The integer</param>
    ''' <returns>The position of the highest set bit within the given value where 1
    ''' represents the least significant bit and 64 the most significant bit. A
    ''' return value of 0 indicates that the value had no bits set - ie. it was 0
    ''' </returns>
    Public Shared Function GetHighestSetBit(ByVal val As Long) As Integer
        Dim posn As Integer = 0
        While val <> 0
            val = val >> 1
            posn += 1
        End While
        Return posn
    End Function

#End Region

#Region "OS/Environment Stuff"

    Public Shared Function GetFQDN() As String
        Dim domain As String = "." & IPGlobalProperties.GetIPGlobalProperties().DomainName
        Dim hostname As String = Dns.GetHostName()
        If hostname.EndsWith(domain) Then Return hostname
        Return hostname & domain
    End Function

#End Region

End Class
