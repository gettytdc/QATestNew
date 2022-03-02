''' Project   : AutomateProcessCore
''' Interface : AutomateProcessCore.ICollectionDefinitionManager
''' 
''' <summary>
''' This interface defines the operations available for managing a collection
''' definition, such as adding and removing fields. There are several places where
''' this interface is implemented:
''' 
'''  clsCollectionInfo - an actual collection definition
'''  clsCollection - an instance of a collection, which has a definition
'''  clsCollectionStage - a collection stage, which has both an (optional) definition
'''                       and instances of collections within (e.g. the initial
'''                       value)
''' 
''' When dealing with any of these entities, it is important to call the interface
''' methods on the highest level in the chain (the list above goes from lowest to
''' highest). An example of why this is necessary is that if you add a field to
''' a collection stage's definition, it must also update the initial value. Likewise
''' if you modify a collection's definition, it must also update its values.
''' 
''' </summary>
Public Interface ICollectionDefinitionManager

    ''' <summary>
    ''' Checks if this manager currently contains a field with the given name.
    ''' </summary>
    ''' <param name="name">The name to search this collection definition for.</param>
    ''' <returns>True if there exists a field in the definition managed by this
    ''' object with the given name; False otherwise.</returns>
    Function ContainsField(ByVal name As String) As Boolean

    ''' <summary>
    ''' Add a new field to the collection definition.
    ''' </summary>
    ''' <param name="name">The name of the new field.</param>
    ''' <param name="datatype">The data type of the new field.</param>
    Sub AddField(ByVal name As String, ByVal datatype As DataType)

    ''' <summary>
    ''' Add a new field to the collection definition allowing custom namespace
    ''' </summary>
    ''' <param name="name">The name of the new field.</param>
    ''' <param name="datatype">The data type of the new field.</param>
    ''' <param name="ns">The custom namespace</param>
    Sub AddField(ByVal name As String, ByVal datatype As DataType, ns As String)

    ''' <summary>
    ''' Adds a field containing the same data as the given field info to the
    ''' definition managed by this object.
    ''' Note that the actual object given is not added, but an object with the same
    ''' value (ie. name, description, datatype, children) as the given field.
    ''' </summary>
    ''' <param name="fld">The field to add to this definition manager.</param>
    Sub AddField(ByVal fld As clsCollectionFieldInfo)

    ''' <summary>
    ''' Delete a field from the collection definition.
    ''' </summary>
    ''' <param name="name">The name of the field to delete.</param>
    Sub DeleteField(ByVal name As String)

    ''' <summary>
    ''' Clears all the fields from the definition being managed by this object.
    ''' </summary>
    Sub ClearFields()

    ''' <summary>
    ''' Rename a field in the collection definition.
    ''' </summary>
    ''' <param name="oldname">The old (existing) name.</param>
    ''' <param name="newname">The new name.</param>
    Sub RenameField(ByVal oldname As String, ByVal newname As String)

    ''' <summary>
    ''' Change the datatype of a field in the collection definition.
    ''' </summary>
    ''' <param name="name">The name of the field.</param>
    ''' <param name="newtype">The new datatype.</param>
    Sub ChangeFieldDataType(ByVal name As String, ByVal newtype As DataType)

    ''' <summary>
    ''' Set the description of a field in the collection definition.
    ''' </summary>
    ''' <param name="name">The name of the field which needs its description
    ''' changing</param>
    ''' <param name="desc">The description of the field to set.</param>
    Sub SetFieldDescription(ByVal name As String, ByVal desc As String)

    ''' <summary>
    ''' Gets the field with the given name from this manager.
    ''' </summary>
    ''' <param name="name">The name of the field required</param>
    ''' <returns>The collection field definition held by this manager with the 
    ''' required name, or null if no such field definition exists.</returns>
    Function GetFieldDefinition(ByVal name As String) As clsCollectionFieldInfo

    ''' <summary>
    ''' Gets a count of the number of field definitions held by this manager.
    ''' </summary>
    ReadOnly Property FieldCount() As Integer

    ''' <summary>
    ''' Gets the fields currently held by this manager.
    ''' </summary>
    ReadOnly Property FieldDefinitions() As IEnumerable(Of clsCollectionFieldInfo)

    ''' <summary>
    ''' True if the collection is a single-row collection. Changing the value of
    ''' this property will result in any underlying collection data being altered!
    ''' </summary>
    Property SingleRow() As Boolean



End Interface

