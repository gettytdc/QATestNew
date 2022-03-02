
''' Project: AutomateProcessCore
'''
''' <summary>
''' Interface describing a data field. This primarily ensures that the
''' responsibility for knowing a field's fully qualified name is encapsulated
''' in the field itself, rather than the interface code.
''' </summary>
''' <remarks>This was originally put in place to handle drag/drop operations
''' in the Automate project so they each didn't have to check if the field
''' being dragged was a) a data stage or b) a collection field, which had
''' to be treated separately since they had no shared inheritance tree.
''' </remarks>
Public Interface IDataField

    ''' <summary>
    ''' The fully qualified name for this data field.
    ''' </summary>
    ReadOnly Property FullyQualifiedName() As String

    ''' <summary>
    ''' The current value of this field.
    ''' </summary>
    Property Value() As clsProcessValue

    ''' <summary>
    ''' The datatype represented by this field
    ''' </summary>
    Property DataType() As DataType

End Interface
