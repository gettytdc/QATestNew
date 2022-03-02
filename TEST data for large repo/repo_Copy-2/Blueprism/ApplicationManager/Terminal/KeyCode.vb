''' <summary>
''' Provides a way of representing a keycode which can be either a 
''' KeyCodeMapping or a character.
''' </summary>
Public Class KeyCode

    ''' <summary>
    ''' Provides access to the Code of the key
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Code As KeyCodeMappings

    ''' <summary>
    ''' Provides access to the character of the key
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Character As Char

    ''' <summary>
    ''' Returns true if the keycode is a character
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsCharacter As Boolean

    ''' <summary>
    ''' Returns true if the keycode is a code
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsCode As Boolean
        Get
            Return Not IsCharacter
        End Get
    End Property

    ''' <summary>
    ''' Create a keycode using a keycodemapping
    ''' </summary>
    ''' <param name="c"></param>
    Public Sub New(c As KeyCodeMappings)
        Code = c
        IsCharacter = False
    End Sub

    ''' <summary>
    ''' Create a keycode using a character
    ''' </summary>
    ''' <param name="c"></param>
    Public Sub New(c As Char)
        Character = c
        IsCharacter = True
    End Sub

    ''' <summary>
    ''' Gets attributes of a given type from the KeyCodeMapping
    ''' </summary>
    ''' <typeparam name="T">The attribute type</typeparam>
    ''' <param name="code">The keycodemapping to get the attribute for</param>
    ''' <returns>The attribute</returns>
    Public Shared Function GetAttribute(Of T As Attribute)(code As KeyCodeMappings) As T
        Dim type = code.GetType()
        Dim info = type.GetField(code.ToString())
        Dim attribs() = info.GetCustomAttributes(GetType(T), False)
        If attribs.Length > 0 Then Return TryCast(attribs(0), T) Else Return Nothing
    End Function

End Class
