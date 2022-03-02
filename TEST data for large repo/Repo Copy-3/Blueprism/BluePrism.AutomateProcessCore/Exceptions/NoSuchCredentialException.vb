Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Exception thrown when a requested credential does not exist
''' </summary>
<Serializable()> _
Public Class NoSuchCredentialException : Inherits NoSuchElementException

    ''' <summary>
    ''' Creates a new exception indicating that no credential with the specified name
    ''' was found.
    ''' </summary>
    ''' <param name="credName">The name of the missing credential</param>
    Public Sub New(ByVal credName As String)
        MyBase.New(ExceptionResources.NoSuchCredentialWithNameErrorTemplate, credName)
    End Sub

    ''' <summary>
    ''' Creates a new exception indicating that no credential with the specified ID
    ''' was found
    ''' </summary>
    ''' <param name="credId">The ID of the missing credential</param>
    Public Sub New(ByVal credId As Guid)
        MyBase.New(ExceptionResources.NoSuchCredentialWithIdErrorTemplate, credId)
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
