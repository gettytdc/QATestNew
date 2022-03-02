''' <summary>
''' Provides an attribute for adding a description to the keycodemappings
''' </summary>
Public Class KeyCodeInfo : Inherits Attribute
    ''' <summary>
    ''' Provides access to the description
    ''' </summary>
    ''' <returns></returns>
    Public Property Description As String

    ''' <summary>
    ''' Creates KeyCodeInfo with the given description
    ''' </summary>
    ''' <param name="description">The description</param>
    Public Sub New(description As String)
        Me.Description = description
    End Sub
End Class