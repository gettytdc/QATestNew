Imports BluePrism.AutomateProcessCore.WebApis

''' <summary>
''' Extension of the <see cref="ActionParameter" /> class, so that it can be 
''' initialised with a collection schema, which is then stored and used to define
''' what dcollection ata can be set in the parameter. Note that this is a class used 
''' solely within the internal running of processes, and as such should not be sent 
''' over a server connection (hence why it is not set up to be serialized).
''' </summary>
Public Class ActionParameterWithCollection : Inherits ActionParameter

    ''' <summary>
    ''' The collection info for the parameter's underlying value.
    ''' </summary>
    Public ReadOnly Property CollectionInfo As clsCollectionInfo

    ''' <summary>
    ''' Create a new action parameter instance of data type collection, with the
    ''' collection info defined for the collection.
    ''' </summary>
    Public Sub New(name As String, description As String, collectionInfo As clsCollectionInfo,
        exposeToProcess As Boolean, initialValue As clsProcessValue)

        MyBase.New(0, name, description, DataType.collection, exposeToProcess, initialValue)
        Me.CollectionInfo = collectionInfo
    End Sub

End Class
