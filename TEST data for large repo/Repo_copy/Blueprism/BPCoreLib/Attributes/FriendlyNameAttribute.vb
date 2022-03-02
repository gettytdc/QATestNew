''' <summary>
''' Attribute which provides a display name for an element. If that element is an 
''' enum you can use the <see cref="Extensions.GetFriendlyName([Enum])"/> and
''' <see cref="Extensions.GetFriendlyName([Enum], Boolean)"/> extension methods 
''' to return the friendly name.
''' </summary>
Public Class FriendlyNameAttribute : Inherits Attribute

    ''' <summary>
    ''' The name in this attribute
    ''' </summary>
    Private mName As String

    ''' <summary>
    ''' Creates a new FriendlyName attribute
    ''' </summary>
    ''' <param name="name">The name to use in a friendly name</param>
    Public Sub New(ByVal name As String)
        mName = name
    End Sub

    ''' <summary>
    ''' The friendly name given for this attribute
    ''' </summary>
    Public ReadOnly Property Name() As String
        Get
            Return If(mName, "")
        End Get
    End Property

End Class

