Imports System.Runtime.Serialization
Imports System.Security.Permissions

Imports BluePrism.Server.Domain.Models

''' <summary>
''' Exception thrown when a specified field could not be found within a
''' collection.
''' </summary>
<Serializable()> _
Public Class FieldNotFoundException : Inherits BluePrismException

    ''' <summary>
    ''' The definition on which the field could not be found.
    ''' </summary>
    Private mDefinition As clsCollectionInfo

    ''' <summary>
    ''' Creates a new exception with the given formatted message, and the 
    ''' specified parent.
    ''' </summary>
    ''' <param name="parent">The definition in which the specified field
    ''' was not found.</param>
    ''' <param name="field">The qualified fieldname that could not be found.
    ''' </param>
    Public Sub New(ByVal parent As clsCollectionInfo, ByVal field As String)
        MyBase.New("The field '{0}' doesn't exist within this collection", field)
        mDefinition = parent
    End Sub

    ''' <summary>
    ''' Creates a new exception with the given formatted message, the 
    ''' specified parent and the specified collection name.
    ''' </summary>
    ''' <param name="parent">The definition in which the specified field
    ''' was not found.</param>
    ''' <param name="field">The qualified fieldname that could not be found.
    ''' </param>
    ''' <param name="collection">The name of the collection on which the fieldname 
    ''' could not be found.</param>
    Public Sub New(parent As clsCollectionInfo, field As String, collection As String)
        MyBase.New($"The field '{field}' doesn't exist within the collection '{collection}'")
        mDefinition = parent
    End Sub

    ''' <summary>
    ''' The collection definition on which the field was not found.
    ''' May be null if the initial definition was not found.
    ''' </summary>
    Public ReadOnly Property Definition() As clsCollectionInfo
        Get
            Return mDefinition
        End Get
    End Property

#Region " Serialization Handling "

    ''' <summary>
    ''' Creates a new blue prism exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception
    ''' should draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
        mDefinition = DirectCast(info.GetValue("FieldNotFoundException.mDefinition", _
         GetType(clsCollectionInfo)), clsCollectionInfo)
    End Sub

    ''' <summary>
    ''' Gets the object data for this exception into the given serialization
    ''' info object.
    ''' </summary>
    ''' <param name="info">The info into which this object's data should be
    ''' stored.</param>
    ''' <param name="ctx">The context for the streaming</param>
    <SecurityPermission(SecurityAction.Demand, SerializationFormatter:=True)>
    Public Overrides Sub GetObjectData( _
     ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.GetObjectData(info, ctx)
        info.AddValue("FieldNotFoundException.mDefinition", mDefinition, _
         GetType(clsCollectionInfo))
    End Sub

#End Region

End Class
