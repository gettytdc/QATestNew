Imports System.Text.RegularExpressions
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Server.Domain.Models

''' Project:   AutomateAppCore
''' Class:     clsSessionVariable
''' <summary>
''' Holds a representation of a session variable on a remote session. This
''' information is received from a Resource PC and used within the Control Room
''' environment.
''' </summary>
<Serializable()>
Public Class clsSessionVariable

    ''' <summary>
    ''' Pre-compiled regex for use by the parser; Regex is immutable and inherently
    ''' thread-safe so no synchronization is required around this
    ''' </summary>
    Private Shared ReadOnly Parsex As New Regex("\[(.+)\] (\w+) ""(.*?)"" ""(.*?)""")

    ''' <summary>
    ''' The session id this session variable came from.
    ''' </summary>
    Public Property SessionID As Guid

    ''' <summary>
    ''' The ID of the Resource handling this session variable. Note that in the case
    ''' of Pools, this is NOT the individual Resource that's running the process,
    ''' it's the pool controller.
    ''' </summary>
    Public Property ResourceID As Guid

    ''' <summary>
    ''' The identifier to display to the user.
    ''' </summary>
    Public Property SessionIdentifier As String

    ''' <summary>
    ''' The name of the session variable.
    ''' </summary>
    Public Property Name As String

    ''' <summary>
    ''' The value of the session variable. This clsProcessValue instance has the
    ''' Description property filled in.
    ''' </summary>
    Public Property Value As clsProcessValue

    ' Whether this session variable is indeterminate or not; A session variable is
    ' marked indeterminate if it represents a compound value from several similarly
    ' named variables with different values.
    Public Property Indeterminate As Boolean

    ''' <summary>
    ''' Creates a new empty session variable instance
    ''' </summary>
    Public Sub New()
    End Sub

    ''' <summary>
    ''' Creates a new session variable instance with the given properties
    ''' </summary>
    ''' <param name="name">The name of the variable</param>
    ''' <param name="val">The value of the variable</param>
    Public Sub New(ByVal name As String, ByVal val As clsProcessValue)
        Me.Name = name
        Me.Value = val
    End Sub

    ''' <summary>
    ''' Returns an escaped and encoded string that represents the session variable
    ''' The returned value can be parsed using the parse method below
    ''' </summary>
    ''' <param name="description">Whether to append a description of the session var
    ''' </param>
    Public Function ToEscapedString(ByVal description As Boolean) As String
        Dim fmt As String = "[{0}] {1} ""{2}"""
        If description Then fmt &= " ""{3}"""
        Return String.Format(fmt, Name, _
         Value.EncodedType, Escape(Value.EncodedValue), Escape(Value.Description))
    End Function

    ''' <summary>
    ''' Gets a string representation of this session variable; this is the escaped
    ''' variable, complete with description - ie. it is equivalent to calling
    ''' <see cref="ToEscapedString"/> with an argument of <c>True</c>.
    ''' </summary>
    ''' <returns>A string representation of this variable</returns>
    Public Overrides Function ToString() As String
        Return ToEscapedString(True)
    End Function

    ''' <summary>
    ''' Escapes various characters in the string so that it can be represented on one
    ''' line.
    ''' </summary>
    Private Function Escape(ByVal val As String) As String
        If val = "" Then Return ""
        val = val.Replace("\", "\\")
        val = val.Replace("""", "\""")
        val = val.Replace(vbCr, "\r")
        val = val.Replace(vbLf, "\n")
        Return val
    End Function

    ''' <summary>
    ''' Unescapes characters in the string returning them to thier literal values.
    ''' </summary>
    Private Shared Function UnEscape(ByVal val As String) As String
        If val = "" Then Return ""
        val = val.Replace("\""", """")
        val = val.Replace("\r", vbCr)
        val = val.Replace("\n", vbLf)
        val = val.Replace("\\", "\")
        Return val
    End Function

    ''' <summary>
    ''' Parse a line of text containing the representation of a session variable and
    ''' its current value, as sent by a Resource PC over the telnet protocol.
    ''' </summary>
    ''' <param name="txt">The line of text to parse.</param>
    ''' <returns>A newly instantiated clsSessionVariable.</returns>
    ''' <remarks>Throws an expcetion if something goes wrong.</remarks>
    Public Shared Function Parse(ByVal txt As String) As clsSessionVariable
        Dim m As Match = Parsex.Match(txt)
        If Not m.Success Then Throw New InvalidFormatException(
         "Failed to parse session variable details from '{0}'", txt)

        Dim var As New clsSessionVariable()
        var.Name = m.Groups(1).Value
        var.Value =
         clsProcessValue.Decode(m.Groups(2).Value, UnEscape(m.Groups(3).Value))
        var.Value.Description = UnEscape(m.Groups(4).Value)
        Return var
    End Function

    ''' <summary>
    ''' Get a deep clone of this object - i.e. the underlying clsProcessValue is
    ''' also cloned.
    ''' </summary>
    ''' <returns>A new clsSessionVariable.</returns>
    Public Function DeepClone() As clsSessionVariable
        Dim v As clsSessionVariable = CType(MemberwiseClone(), clsSessionVariable)
        v.Value = Value.Clone()
        Return v
    End Function

End Class
