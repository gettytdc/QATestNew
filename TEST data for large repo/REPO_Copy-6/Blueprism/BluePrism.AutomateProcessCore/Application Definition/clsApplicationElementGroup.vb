''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsApplicationElementGroup
''' 
''' <summary>
''' Represents a group of application elements.
''' </summary>
<DebuggerDisplay("ElementGroup: {FullPath}", Name:="{mName}")> _
Public Class clsApplicationElementGroup
    Inherits clsApplicationMember

    ''' <summary>
    ''' Creates a new empty app element group with no name
    ''' </summary>
    Friend Sub New()
        Me.New("")
    End Sub

    ''' <summary>
    ''' Creates an empty app element group with the given name
    ''' </summary>
    ''' <param name="groupName">The name of the group</param>
    Public Sub New(ByVal groupName As String)
        MyBase.New(groupName)
    End Sub

    ''' <summary>
    ''' Gets the XML element name for application members of this type.
    ''' </summary>
    Friend Overrides ReadOnly Property XmlName() As String
        Get
            Return "group"
        End Get
    End Property

End Class
