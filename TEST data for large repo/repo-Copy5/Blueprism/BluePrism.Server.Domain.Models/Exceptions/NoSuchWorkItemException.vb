Imports System.Runtime.Serialization

''' <summary>
''' Exception thrown when a work item with a specified ID or identity does not exist
''' in the search target (typically the database).
''' </summary>
<Serializable()> _
Public Class NoSuchWorkItemException : Inherits NoSuchElementException

    ''' <summary>
    ''' Creates a new exception indicating that no item with the specified ID
    ''' was found.
    ''' </summary>
    Public Sub New(itemId As Guid)
        MyBase.New(My.Resources.NoSuchWorkItemException_NoWorkItemWasFoundWithTheID0, itemId)
    End Sub

    ''' <summary>
    ''' Creates a new exception indicating that no item with the specified identity
    ''' was found.
    ''' </summary>
    Public Sub New(ident As Long)
        MyBase.New(My.Resources.NoSuchWorkItemException_NoWorkItemWasFoundWithTheIdent0, ident)
    End Sub

    ''' <summary>
    ''' Creates a new exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception
    ''' should draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub

End Class
