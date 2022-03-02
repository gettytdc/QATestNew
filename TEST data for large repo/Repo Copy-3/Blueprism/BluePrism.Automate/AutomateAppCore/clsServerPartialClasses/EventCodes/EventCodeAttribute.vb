Imports System.Globalization
''' <summary>
''' Attribute class to associate event code strings with
''' respective enums
''' </summary>
Public Class EventCodeAttribute
    Inherits Attribute

    ''' <summary>
    ''' Empty attribute - contains an empty code value and no narrative
    ''' </summary>
    Public Shared ReadOnly Empty As EventCodeAttribute = New EventCodeAttribute("")


    ''' <summary>
    ''' The string value of the event code represented by this attribute
    ''' </summary>
    Public ReadOnly Property Code() As String

    ''' <summary>
    ''' The resource key which forms the narrative for this event code.
    ''' null if the narrative is not set in this attribute.
    ''' </summary>
    Public ReadOnly Property Key() As String

    ''' <summary>
    ''' Constructor takes a event code string
    ''' </summary>
    ''' <param name="c">The event code.</param>
    Public Sub New(c As String)
        Me.New(c, Nothing)
    End Sub

    ''' <summary>
    ''' Constructor takes a event code string and a formatted narrative
    ''' </summary>
    ''' <param name="c">The event code.</param>
    Public Sub New(ByVal c As String, ByVal n As String)
        Code = c
        Key = n
    End Sub

    ''' <summary>
    ''' Formats the narrative for this attribute using the given arguments.
    ''' </summary>
    ''' <param name="args">The arguments with the auxiliary data for the narrative.
    ''' The requirements for these will be dictated by the event code themselves
    ''' and documented (or self-documented) with their definitions.</param>
    ''' <returns>A formatted string containing the narrative with the given 
    ''' arguments applied, or an empty string if no narrative is associated with
    ''' this attribute.</returns>
    Public Function FormatNarrative(auditLocale As CultureInfo, ParamArray args() As Object) As String
        If Key Is Nothing Then Return ""
        Return String.Format(GetLocalizedFriendlyName(auditLocale, Key), args)
    End Function

    ''' <summary>
    ''' Gets the localized friendly name For narrative according To the current culture.
    ''' </summary>
    ''' <param name="key">The key for the format string</param>
    ''' <returns>The narrative attribute format string for the current culture</returns>
    Public Shared Function GetLocalizedFriendlyName(auditLocale As CultureInfo, key As String) As String
        Dim res = My.Resources.ResourceManager.GetString(key, auditLocale)
        If res Is Nothing Then res = My.Resources.ResourceManager.GetString(key)
        If res Is Nothing Then Throw New KeyNotFoundException()
        Return res
    End Function

End Class