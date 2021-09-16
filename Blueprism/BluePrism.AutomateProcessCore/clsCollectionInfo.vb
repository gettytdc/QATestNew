Imports System.Xml
Imports System.Text
Imports System.Runtime.Serialization

Imports BluePrism.Server.Domain.Models

Imports BluePrism.AutomateProcessCore.Stages
Imports LocaleTools

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsCollectionInfo
''' 
''' <summary>
''' Metadata about a collection. In particular, contains a list of fields
''' (and their data types) contained in a collection.
''' </summary>
<Serializable, DataContract([Namespace]:="bp"), KnownType(GetType(clsCollectionFieldInfo))>
Public Class clsCollectionInfo
    Inherits CollectionBase
    Implements ICloneable
    Implements ICollectionDefinitionManager

#Region " Event Definitions "

    ''' <summary>
    ''' Event fired when a field is added to this collection definition or a
    ''' definition nested within it.
    ''' Note that with nested definitions, the event is only fired by the top
    ''' definition - it is chained up to the top and only then is passed outside
    ''' of this class in the form of an event.
    ''' </summary>
    ''' <param name="sender">The definition on which the field operation took place.
    ''' Note that this is nested definition whose fields were directly affected by
    ''' the field operation - ie. the nested definition, which may or may not be
    ''' the top level definition which fired the event, but certainly will be
    ''' within its scope.</param>
    ''' <param name="fld">The field that was added.</param>
    Public Event FieldAdded(ByVal sender As clsCollectionInfo, ByVal fld As clsCollectionFieldInfo)

    ''' <summary>
    ''' Event fired when a field is removed from this collection definition or a
    ''' definition nested within it.
    ''' Note that with nested definitions, the event is only fired by the top
    ''' definition - it is chained up to the top and only then is passed outside
    ''' of this class in the form of an event.
    ''' </summary>
    ''' <param name="sender">The definition on which the field operation took place.
    ''' Note that this is nested definition whose fields were directly affected by
    ''' the field operation - ie. the nested definition, which may or may not be
    ''' the top level definition which fired the event, but certainly will be
    ''' within its scope.</param>
    ''' <param name="fld">The field that was removed.</param>
    Public Event FieldRemoved(ByVal sender As clsCollectionInfo, ByVal fld As clsCollectionFieldInfo)

    ''' <summary>
    ''' Event fired when a field is modified within this collection definition or a
    ''' definition nested within it.
    ''' Note that with nested definitions, the event is only fired by the top
    ''' definition - it is chained up to the top and only then is passed outside
    ''' of this class in the form of an event.
    ''' </summary>
    ''' <param name="sender">The definition on which the field operation took place.
    ''' Note that this is nested definition whose fields were directly affected by
    ''' the field operation - ie. the nested definition, which may or may not be
    ''' the top level definition which fired the event, but certainly will be
    ''' within its scope.</param>
    ''' <param name="oldValue">The old value of the field - ie. the value before
    ''' the operation took place.</param>
    ''' <param name="newValue">The new value of the field - ie. the value after
    ''' the operation has taken place.</param>
    Public Event FieldModified(ByVal sender As clsCollectionInfo,
     ByVal oldValue As clsCollectionFieldInfo, ByVal newValue As clsCollectionFieldInfo)

    ''' <summary>
    ''' Event fired when the value of the <see cref="Flat"/> property of a collection
    ''' definition has changed.
    ''' Note that with nested definitions, the event is only fired by the top
    ''' definition - it is chained up to the top and only then is passed outside
    ''' of this class in the form of an event.
    ''' </summary>
    ''' <param name="sender">The definition on which the field operation took place.
    ''' Note that this is nested definition whose fields were directly affected by
    ''' the field operation - ie. the nested definition, which may or may not be
    ''' the top level definition which fired the event, but certainly will be
    ''' within its scope.</param>
    ''' <param name="value">The new value of the <see cref="Flat"/> property.
    ''' </param>
    ''' <remarks>FIXME: I must admit, I don't really understand what 'Flat'
    ''' represents, and whether value collections need to be altered in some way when
    ''' the property is changed. The event is here as it is for all other data
    ''' properties in a collection schema, but the stage isn't (currently) listening
    ''' for this event, so it won't be propogating the change to its initial or
    ''' current values. The 'FIXME' is to check that this is correct.</remarks>
    Public Event FlatChanged(ByVal sender As clsCollectionInfo, ByVal value As Boolean)

    ''' <summary>
    ''' Event fired when the value of the <see cref="SingleRow"/> property of a
    ''' collection definition has changed.
    ''' Note that with nested definitions, the event is only fired by the top
    ''' definition - it is chained up to the top and only then is passed outside
    ''' of this class in the form of an event.
    ''' </summary>
    ''' <param name="sender">The definition on which the field operation took place.
    ''' Note that this is nested definition whose fields were directly affected by
    ''' the field operation - ie. the nested definition, which may or may not be
    ''' the top level definition which fired the event, but certainly will be
    ''' within its scope.</param>
    ''' <param name="value">The new value of the <see cref="SingleRow"/> property.
    ''' </param>
    Public Event SingleRowChanged(ByVal sender As clsCollectionInfo, ByVal value As Boolean)

#End Region

#Region " Static Methods / Operators "
    ''' <summary>
    ''' Helper method to test if a collection definition is empty.
    ''' It is considered empty if it is null, or it has no fields, and has no
    ''' boolean attributes set.
    ''' </summary>
    ''' <param name="defn">The definition to test for emptiness.</param>
    ''' <returns>True if the given definition is considered empty.</returns>
    Private Shared Function IsEmpty(ByVal defn As clsCollectionInfo) As Boolean
        Return defn Is Nothing OrElse
         (Not defn.SingleRow AndAlso Not defn.Flat AndAlso defn.FieldCount = 0)
    End Function

    ''' <summary>
    ''' Equality operator for collection definitions.
    ''' </summary>
    ''' <param name="first">The first definition to compare</param>
    ''' <param name="second">The second definition to compare</param>
    ''' <returns>True if they are both null or they are equal as according to the
    ''' <see cref="Equals"/> method; False otherwise.</returns>
    Public Shared Operator =(ByVal first As clsCollectionInfo, ByVal second As clsCollectionInfo) As Boolean
        If IsEmpty(first) Then Return IsEmpty(second)
        Return first.Equals(second)
    End Operator

    ''' <summary>
    ''' Inequality operator for collection definitions.
    ''' </summary>
    ''' <param name="first">The first definition to compare</param>
    ''' <param name="second">The second definition to compare</param>
    ''' <returns>False if they are both null or they are equal as according to the
    ''' <see cref="Equals"/> method, True otherwise.</returns>
    Public Shared Operator <>(ByVal first As clsCollectionInfo, ByVal second As clsCollectionInfo) As Boolean
        Return Not (first = second)
    End Operator

    ''' <summary>
    ''' Gets a label depicting info about the given collection definition.
    ''' Returns any of :-
    ''' <list>
    ''' <item>"Not Defined" if there was no definition, or no fields within the
    '''        definition.</item>
    ''' <item>"[n] field(s)" if the given definition has any fields on it.</item>
    ''' </list>
    ''' </summary>
    ''' <param name="defn">The collection definition for which the info label is
    ''' required.</param>
    ''' <returns>A label with summary information about the given collection
    ''' definition.</returns>
    Public Shared Function GetInfoLabel(ByVal defn As clsCollectionInfo) As String

        If defn Is Nothing OrElse defn.Count = 0 Then
            Return My.Resources.Resources.NotDefined
        Else
            Return LTools.Format(My.Resources.Resources.clsCollectionInfo_CountFields, "COUNT", defn.Count)
        End If

    End Function

    ''' <summary>
    ''' Determine if the given name is valid as a collection field name.
    ''' </summary>
    ''' <param name="name">The name to check.</param>
    ''' <returns>True if the name is valid, False otherwise.</returns>
    Public Shared Function IsValidFieldName(ByVal name As String) As Boolean
        If name.IndexOfAny(New Char() {"."c, "["c, "]"c}) <> -1 Then Return False
        Return True
    End Function

#End Region

    Public Sub New(ByVal base As clsCollectionInfo)
        For Each fld As clsCollectionFieldInfo In base
            AddField(fld)
        Next
    End Sub

    ''' <summary>
    ''' The collection stage that owns this collection definiton. This can be Nothing
    ''' when the collection is not owned by a collection stage at all. It is used
    ''' only to allow the Automate UI to retrieve a FQN via clsCollectionFieldInfo.
    ''' Ideally it would not be here at all, but there is too much code using this
    ''' facility in a disconnected drag/drop manner to allow it to be safely altered.
    ''' </summary>
    Public ReadOnly Property ParentStage() As clsCollectionStage
        Get
            Return mParentStage
        End Get
    End Property
    <NonSerialized()>
    Private mParentStage As clsCollectionStage

    ''' <summary>
    ''' The field definition which owns this collection info, ie. the field that
    ''' this object defines the structure of.
    ''' </summary>
    Public ReadOnly Property ParentField() As clsCollectionFieldInfo
        Get
            Return mParentField
        End Get
    End Property

    <DataMember>
    Private mParentField As clsCollectionFieldInfo

    ''' <summary>
    ''' Gets the root collection stage which ultimately owns this collection
    ''' definition, or null if it is not nested somewhere within a collection stage
    ''' </summary>
    Public ReadOnly Property RootStage() As clsCollectionStage
        Get
            If mParentStage IsNot Nothing Then Return mParentStage
            If mParentField IsNot Nothing AndAlso mParentField.Parent IsNot Nothing Then
                Return mParentField.Parent.RootStage
            End If
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Gets the fully qualified name that this collection definition describes. This
    ''' is intuited from the parent - either the field representing the collection
    ''' this object describes, or the collection stage that this object describes.
    ''' If this definition is orphaned, ie. not attached to a field or a stage, then
    ''' it effectively has no name and null is returned. This can happen if a
    ''' definition describes a collection which hasn't been assigned to a particular
    ''' stage or nested field.
    ''' </summary>
    Friend ReadOnly Property FullyQualifiedName() As String
        Get
            If mParentField IsNot Nothing Then Return mParentField.FullyQualifiedName
            If mParentStage IsNot Nothing Then Return mParentStage.GetName()
            ' We are the collection definition with no name <whistles...>
            Return Nothing
        End Get
    End Property

#Region " Constructors "

    ''' <summary>
    ''' Creates a new <em>disconnected</em> collection definition. This will have
    ''' no 'owner' - ie. it will not be defining the fields on any collection stage,
    ''' nor the fields within a collection field. It simply 'defines some collection
    ''' fields'.
    ''' See <see cref="ParentStage"/> and <see cref="ParentField"/> for more
    ''' information on ownership of a collection definition.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, Nothing, False, False)
    End Sub

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="parent">The parent collection stage. See the documentation for
    ''' ParentStage for more details.</param>
    Public Sub New(ByVal parent As clsCollectionStage)
        Me.New(parent, Nothing, False, False)
    End Sub

    ''' <summary>
    ''' Creates a new collection definition, which defines the collection specified
    ''' by the given field
    ''' </summary>
    ''' <param name="parent">The field object representing a collection that this
    ''' field info object defines.</param>
    Friend Sub New(ByVal parent As clsCollectionFieldInfo)
        Me.New(Nothing, parent, False, False)
    End Sub

    ''' <summary>
    ''' Creates a new collection definition with the given attributes
    ''' </summary>
    ''' <param name="parentStg">The parent collection stage - ie. the collection
    ''' stage object whose fields are specified by this definition. Null if this
    ''' definition does not directly specify a stage's fields.</param>
    ''' <param name="parentFld">The parent field - ie. a field within a collection,
    ''' which is itself a collection and whose fields are specified by this
    ''' definition. Null if this definition does not directly specify a collection
    ''' field's fields.</param>
    ''' <param name="singleRow">True to make this new collection a single row
    ''' collection; False otherwise.</param>
    ''' <param name="flat">True to make this collection flat, false otherwise.
    ''' </param>
    Private Sub New(
     ByVal parentStg As clsCollectionStage, ByVal parentFld As clsCollectionFieldInfo,
     ByVal singleRow As Boolean, ByVal flat As Boolean)

        mParentStage = parentStg
        mParentField = parentFld
        mSingleRow = singleRow
        mFlat = flat

    End Sub

#End Region


    ''' <summary>
    ''' Gets the collection field definitions which are currently being held by this
    ''' stage.
    ''' </summary>
    Public ReadOnly Property FieldDefinitions() As IEnumerable(Of clsCollectionFieldInfo) _
     Implements ICollectionDefinitionManager.FieldDefinitions
        Get
            Dim arr(List.Count - 1) As clsCollectionFieldInfo
            List.CopyTo(arr, 0)
            Return arr
        End Get
    End Property

    ''' <summary>
    ''' Gets the number of field definitions currently held by this stage.
    ''' </summary>
    Public ReadOnly Property FieldCount() As Integer Implements ICollectionDefinitionManager.FieldCount
        Get
            Return Me.Count
        End Get
    End Property

    ''' <summary>
    ''' Add a new field to the collection definition.
    ''' </summary>
    ''' <param name="name">The name of the new field.</param>
    ''' <param name="datatype">The data type of the new field.</param>
    ''' <param name="ns">The namespace of the new field</param>
    Public Sub AddField(ByVal name As String, ByVal datatype As DataType, ns As String) Implements ICollectionDefinitionManager.AddField
        InnerAddField(New clsCollectionFieldInfo(Me, name, datatype, ns), False)
    End Sub

    ''' <summary>
    ''' Add a new field to the collection definition.
    ''' </summary>
    ''' <param name="name">The name of the new field.</param>
    ''' <param name="datatype">The data type of the new field.</param>
    Public Sub AddField(ByVal name As String, ByVal datatype As DataType) Implements ICollectionDefinitionManager.AddField
        InnerAddField(New clsCollectionFieldInfo(Me, name, datatype, String.Empty), False)
    End Sub

    ''' <summary>
    ''' Adds or sets a field containing the same data as the given field info to the
    ''' definition managed by this object.
    ''' Note that the actual object given is not added, but an object with the same
    ''' value (ie. name, description, datatype, children) as the given field.
    ''' If a field with the given name already exists within this definition, it
    ''' is replaced with the value from the given field.
    ''' </summary>
    ''' <param name="fld">The field to add to this definition manager.</param>
    Public Sub AddField(ByVal fld As clsCollectionFieldInfo) Implements ICollectionDefinitionManager.AddField
        InnerAddField(fld, True)
    End Sub

    ''' <summary>
    ''' Adds a collection field definition object derived from the given XML element.
    ''' This is only really used when parsing the XML from an external source.
    ''' </summary>
    ''' <param name="fieldElement">The XML element representing the field.</param>
    ''' <remarks>See the <see cref="clsCollectionFieldInfo"/> constructor which
    ''' accepts an XML Element for details.</remarks>
    Friend Sub AddField(ByVal fieldElement As XmlElement)
        InnerAddField(New clsCollectionFieldInfo(Me, fieldElement), False)
    End Sub

    ''' <summary>
    ''' Adds the field or a copy of it to the given collection definition.
    ''' </summary>
    ''' <param name="fld">The field to add</param>
    ''' <param name="createCopy">True to create a copy of the given field and add
    ''' that; False to use the actual field object itself.</param>
    Private Sub InnerAddField(ByVal fld As clsCollectionFieldInfo, ByVal createCopy As Boolean)
        ' See if we have a field with this name already
        ' If we have, set the index and the oldField to the current index of the field,
        ' and the value of the old field respectively
        Dim index As Integer = -1
        Dim oldField As clsCollectionFieldInfo = FindFieldWithoutRecursion(fld.Name, index)

        ' Copy the field info if we have been directed to do so
        Dim newField As clsCollectionFieldInfo = fld
        If createCopy Then newField = New clsCollectionFieldInfo(Me, fld)

        If index >= 0 Then ' replace the existing field, if there
            List(index) = newField
            OnFieldModified(Me, oldField, newField)

        Else ' otherwise append it to the list
            List.Add(newField)
            ' We need to inform further up the chain that there is a new field
            OnFieldAdded(Me, newField)
        End If

    End Sub

    ''' <summary>
    ''' Event handler for a field being added to this collection schema or its
    ''' descendants.
    ''' </summary>
    ''' <param name="fld">The field that has been added.</param>
    ''' <remarks>This just passes the event up the ancestry ladder until there are
    ''' no more schema parents, at which point an event is raised for interested
    ''' listeners outside this class.</remarks>
    Protected Overridable Sub OnFieldAdded(
     ByVal defn As clsCollectionInfo, ByVal fld As clsCollectionFieldInfo)
        If ParentField IsNot Nothing AndAlso ParentField.Parent IsNot Nothing Then
            ParentField.Parent.OnFieldAdded(defn, fld)
        Else
            RaiseEvent FieldAdded(defn, fld)
        End If
    End Sub

    ''' <summary>
    ''' Event handler for a field being added to this collection schema or its
    ''' descendants.
    ''' </summary>
    ''' <param name="fld">The field that has been added.</param>
    ''' <remarks>This just passes the event up the ancestry ladder until there are
    ''' no more schema parents, at which point an event is raised for interested
    ''' listeners outside this class.</remarks>
    Protected Overridable Sub OnFieldRemoved(
     ByVal defn As clsCollectionInfo, ByVal fld As clsCollectionFieldInfo)
        If ParentField IsNot Nothing AndAlso ParentField.Parent IsNot Nothing Then
            ParentField.Parent.OnFieldRemoved(defn, fld)
        Else
            RaiseEvent FieldRemoved(defn, fld)
        End If
    End Sub

    ''' <summary>
    ''' Event handler for a field being modified within this collection schema or
    ''' its descendants.
    ''' </summary>
    ''' <param name="oldFld">The value of the field before modification.</param>
    ''' <param name="newFld">The value of the field after modification.</param>
    ''' <remarks>This just passes the event up the ancestry ladder until there are
    ''' no more schema parents, at which point an event is raised for interested
    ''' listeners outside this class.</remarks>
    Protected Friend Overridable Sub OnFieldModified(
     ByVal defn As clsCollectionInfo,
     ByVal oldFld As clsCollectionFieldInfo, ByVal newFld As clsCollectionFieldInfo)
        If ParentField IsNot Nothing AndAlso ParentField.Parent IsNot Nothing Then
            ParentField.Parent.OnFieldModified(defn, oldFld, newFld)
        Else
            RaiseEvent FieldModified(defn, oldFld, newFld)
        End If
    End Sub

    ''' <summary>
    ''' Event handler for the <see cref="Flat"/> property being changed.
    ''' This chains the event up the ancestry ladder until the top definition is
    ''' reached, at which point an event is fired for other interested observers.
    ''' </summary>
    ''' <param name="value">The new value of the flat property</param>
    Protected Overridable Sub OnFlatChanged(ByVal defn As clsCollectionInfo, ByVal value As Boolean)
        If ParentField IsNot Nothing AndAlso ParentField.Parent IsNot Nothing Then
            ParentField.Parent.OnFlatChanged(defn, value)
        Else
            RaiseEvent FlatChanged(defn, value)
        End If
    End Sub

    ''' <summary>
    ''' Event handler for the <see cref="SingleRow"/> property being changed.
    ''' This chains the event up the ancestry ladder until the top definition is
    ''' reached, at which point an event is fired for other interested observers.
    ''' </summary>
    ''' <param name="value">The new value of the flat property</param>
    Protected Overridable Sub OnSingleRowChanged(
     ByVal defn As clsCollectionInfo, ByVal value As Boolean)
        If ParentField IsNot Nothing AndAlso ParentField.Parent IsNot Nothing Then
            ParentField.Parent.OnSingleRowChanged(defn, value)
        Else
            RaiseEvent SingleRowChanged(defn, value)
        End If
    End Sub


    ''' <summary>
    ''' Delete a field from the collection definition.
    ''' </summary>
    ''' <param name="name">The name of the field to delete.</param>
    Public Sub DeleteField(ByVal name As String) Implements ICollectionDefinitionManager.DeleteField
        Dim info As clsCollectionFieldInfo = GetField(name)
        If info IsNot Nothing Then
            MyBase.List.Remove(info)
            OnFieldRemoved(Me, info)
        End If
    End Sub

    ''' <summary>
    ''' Finds the field with the given name within this collection definition and
    ''' returns it and its index within the underlying list.
    ''' Note that this does not support searching for nested collection definitions.
    ''' For that see <see cref="FindField"/>.
    ''' </summary>
    ''' <param name="name">The name of the field to look for.</param>
    ''' <param name="index">On exit, this contains the index at which the field was
    ''' found within the underlying list held by this definition, or -1 if the field
    ''' was not found.</param>
    ''' <returns>The field corresponding to the given name found in this definition,
    ''' or null if no such field was found.</returns>
    Private Function FindFieldWithoutRecursion(
     ByVal name As String, ByRef index As Integer) As clsCollectionFieldInfo

        Dim i As Integer = 0
        For Each fld As clsCollectionFieldInfo In Me.List
            If fld.Name = name Then
                ' Found it
                index = i
                Return fld
            End If
            i += 1
        Next
        index = -1
        Return Nothing
    End Function

    ''' <summary>
    ''' Rename a field in the collection definition.
    ''' </summary>
    ''' <param name="oldname">The old (existing) name.</param>
    ''' <param name="newname">The new name.</param>
    Public Sub RenameField(ByVal oldname As String, ByVal newname As String) Implements ICollectionDefinitionManager.RenameField
        If Not IsValidFieldName(newname) Then
            'This should never actually happen - the UI code should check beforehand.
            Throw New InvalidOperationException(My.Resources.Resources.NotAValidNameForACollectionField)
        End If

        ' Just double check that the names are not the same
        If oldname = newname Then Return

        ' With that done, ensure that the new field name specified does not already
        ' exist in the definition
        If FindFieldWithoutRecursion(newname, 0) IsNot Nothing Then
            Throw New AlreadyExistsException(
             My.Resources.Resources.TheField0AlreadyExistsInThisCollectionDefinition, newname)
        End If

        Dim index As Integer = -1
        Dim oldField As clsCollectionFieldInfo = FindFieldWithoutRecursion(oldname, index)

        If oldField IsNot Nothing Then
            Dim newField As clsCollectionFieldInfo = New clsCollectionFieldInfo(Me, oldField)
            newField.Name = newname
            List(index) = newField
            OnFieldModified(Me, oldField, newField)
        End If
    End Sub

    ''' <summary>
    ''' Change the datatype of a field in the collection definition.
    ''' </summary>
    ''' <param name="name">The name of the field.</param>
    ''' <param name="newtype">The new datatype.</param>
    Public Sub ChangeFieldDataType(ByVal name As String, ByVal newtype As DataType) _
     Implements ICollectionDefinitionManager.ChangeFieldDataType
        Dim index As Integer = -1
        Dim oldField As clsCollectionFieldInfo = FindFieldWithoutRecursion(name, index)
        If oldField IsNot Nothing Then
            Dim newField As clsCollectionFieldInfo = New clsCollectionFieldInfo(Me, oldField)
            newField.DataType = newtype
            List(index) = newField
            OnFieldModified(Me, oldField, newField)
        End If
    End Sub

    ''' <summary>
    ''' Set the description of a field in the collection definition.
    ''' </summary>
    ''' <param name="name">The name of the field which needs its description
    ''' changing</param>
    ''' <param name="desc">The description of the field to set.</param>
    Public Sub SetFieldDescription(ByVal name As String, ByVal desc As String) _
     Implements ICollectionDefinitionManager.SetFieldDescription
        Dim index As Integer = -1
        Dim oldField As clsCollectionFieldInfo = FindFieldWithoutRecursion(name, index)
        If oldField IsNot Nothing Then
            Dim newField As clsCollectionFieldInfo = New clsCollectionFieldInfo(Me, oldField)
            newField.Description = desc
            List(index) = newField
            OnFieldModified(Me, oldField, newField)
        End If
    End Sub

    ''' <summary>
    ''' Sets this collection definition from the definition given by the supplied
    ''' manager. When this method exits, this object will match the given manager
    ''' in terms of fields and properties.
    ''' </summary>
    ''' <param name="mgr">The manager from which to draw the collection definition.
    ''' A null manager has the effect of clearing all the fields in this collection
    ''' and setting 'SingleRow' to false.</param>
    Public Sub SetFrom(ByVal mgr As ICollectionDefinitionManager)
        If mgr Is Me Then Return ' Really broken code if this happens
        If mgr Is Nothing Then
            ClearFields()
            mSingleRow = False
            Return
        End If
        ' Find any that we are holding that the given manager doesn't contain
        ' and prepare them for deletion.
        Dim del As New List(Of clsCollectionFieldInfo)
        For Each fld As clsCollectionFieldInfo In Me.List
            If Not mgr.ContainsField(fld.Name) Then del.Add(fld)
        Next
        ' Now delete them.
        For Each fld As clsCollectionFieldInfo In del
            DeleteField(fld.Name)
        Next
        ' Now 'add' each one in the given manager. This will replace any definitions
        ' with new ones and fire a modified event where appropriate.
        For Each fld As clsCollectionFieldInfo In mgr.FieldDefinitions
            AddField(fld)
        Next
        Me.SingleRow = mgr.SingleRow
    End Sub

    ''' <summary>
    ''' True if the collection is a single-row collection. Changing the value of
    ''' this property will result in any underlying collection data being altered!
    ''' </summary>
    Public Property SingleRow() As Boolean Implements ICollectionDefinitionManager.SingleRow
        Get
            Return mSingleRow
        End Get
        Set(ByVal value As Boolean)
            Dim changed As Boolean = (value <> mSingleRow)
            mSingleRow = value
            If changed Then OnSingleRowChanged(Me, value)
        End Set
    End Property

    <DataMember>
    Private mSingleRow As Boolean


    ''' <summary>
    ''' True if the collection is 'flat'. This is used only to store underlying
    ''' state information about the style of formatting for a SOAP array or other
    ''' repeating type. Normally it just defaults to False and stays that way.
    ''' </summary>
    Public Property Flat() As Boolean
        Get
            Return mFlat
        End Get
        Set(ByVal value As Boolean)
            Dim changed As Boolean = (value <> mFlat)
            mFlat = value
            If changed Then OnFlatChanged(Me, value)
        End Set
    End Property
    <DataMember>
    Private mFlat As Boolean


    ''' <summary>
    ''' Provides access to a field according to the zero-based index
    ''' of the order in which fields were added.
    ''' </summary>
    ''' <value>Returns the field at the specified index.</value>
    Default Public Property Item(ByVal index As Integer) As clsCollectionFieldInfo
        Get
            Return CType(MyBase.List(index), clsCollectionFieldInfo)
        End Get
        Set(ByVal Value As clsCollectionFieldInfo)
            MyBase.List(index) = Value
        End Set
    End Property

    ''' <summary>
    ''' Gets the first field found with the specified name, or a null reference if no
    ''' such field exists.
    ''' </summary>
    ''' <param name="name">The name of the field of interest.</param>
    ''' <returns>Returns the field of interest, or Nothing if not found.</returns>
    Public Function GetField(ByVal name As String) As clsCollectionFieldInfo _
     Implements ICollectionDefinitionManager.GetFieldDefinition

        For Each fld As clsCollectionFieldInfo In MyBase.List
            If fld.Name = name Then
                Return fld
            End If
        Next
        Return Nothing
    End Function

    ''' <summary>
    ''' Finds the field with the given qualified name.
    ''' This will search within this definition for the given name, iterating
    ''' through the specified fields until either the field is found, or a link
    ''' in the field chain is missing (ie. a specified field is not found).
    ''' </summary>
    ''' <param name="name">The qualified name of the field to find.</param>
    ''' <returns>The field corresponding to the given qualified name</returns>
    ''' <exception cref="FieldNotFoundException">If any of the fields specified in
    ''' the tokens could not be found within this definition. The message will
    ''' indicate which field could not be found (and, by implication, those fields
    ''' above it which were found).</exception>
    Public Function FindField(ByVal name As String) As clsCollectionFieldInfo
        Return FindField(name.Split("."c), Nothing)
    End Function

    ''' <summary>
    ''' Finds the field identified by the given list of tokens derived from the
    ''' required collection field path.
    ''' </summary>
    ''' <param name="nameTokens">The tokens with which to find the field. Each
    ''' token should represent a field defined within this schema object, so, for
    ''' example a search for the field:
    ''' <code>[StageName.Collection.Inner Collection.Another]</code> would be
    ''' represented by the tokens:
    ''' <code>{"StageName", "Collection", "Inner Collection", "Another"}</code>.
    ''' </param>
    ''' <param name="indices">On return, the list of indices within their respective
    ''' collection definitions of the fields that were searched matching up to the
    ''' list of fields. If any of the specified fields could not be found, this list
    ''' will be empty upon return.</param>
    ''' <returns>The field definition object representing the specified field.
    ''' </returns>
    ''' <exception cref="FieldNotFoundException">If any of the fields specified in
    ''' the tokens could not be found within this definition. The message will
    ''' indicate which field could not be found (and, by implication, those fields
    ''' above it which were found).</exception>
    Private Function FindField(
     ByVal nameTokens As IList(Of String), ByVal indices As IList(Of Integer)) _
     As clsCollectionFieldInfo

        If indices Is Nothing Then indices = New List(Of Integer)
        indices.Clear()

        Dim currentField As New StringBuilder()
        Dim foundField As clsCollectionFieldInfo = Nothing
        Dim defn As clsCollectionInfo = Me

        For Each fldName As String In nameTokens
            ' We keep track of where we're searching so that we can give some
            ' useful information back out if a nested field doesn't exist.
            If currentField.Length > 0 Then currentField.Append("."c)
            currentField.Append(fldName)
            ' By definition, if we're here, we haven't found the field.
            ' If 'foundField' is set to something, then it's the next field in the
            ' chain that we need to search in order to find the field.
            If foundField IsNot Nothing Then defn = foundField.Children
            foundField = Nothing

            ' Find the field within the definition.
            Dim i As Integer = 0
            For Each fld As clsCollectionFieldInfo In defn.List
                If fld.Name = fldName Then
                    ' Found it - add it onto our indices and set the found field
                    indices.Add(i)
                    foundField = fld
                    ' Exit the inner for loop
                    ' If we need to go deeper, we will get the field's children
                    ' and operate on them, otherwise we have our field.
                    Exit For
                End If
                i += 1
            Next
            ' If we get this far and we didn't find the field (either the actual
            ' field we're looking for or a nested collection field to search
            ' within), then there's no point in continuing...
            If foundField Is Nothing Then
                indices.Clear()
                Throw New FieldNotFoundException(defn, currentField.ToString())
            End If
        Next
        Return foundField
    End Function

    ''' <summary>
    ''' Removes the fields in this collection info.
    ''' </summary>
    Public Sub ClearFields() Implements ICollectionDefinitionManager.ClearFields
        Dim fields As New List(Of clsCollectionFieldInfo)
        For Each fld As clsCollectionFieldInfo In MyBase.List
            fields.Add(fld)
        Next
        List.Clear()
        For Each fld As clsCollectionFieldInfo In fields
            OnFieldRemoved(Me, fld)
        Next
    End Sub

    ''' <summary>
    ''' Determines whether there exists a field with the specified name.
    ''' </summary>
    ''' <param name="sField">The name of the field of interest.</param>
    ''' <returns>Returns True if a field exists with the specified name; returns
    ''' False otherwise.</returns>
    Public Function Contains(ByVal sField As String) As Boolean Implements ICollectionDefinitionManager.ContainsField
        Return Contains(sField, False)
    End Function

    ''' <summary>
    ''' Determines whether there exists a field with the specified name, which may
    ''' or may not be qualified, depending on whether a search of nested fields
    ''' within this definition is required.
    ''' </summary>
    ''' <param name="name">The fieldname to look for - if searching nested fields,
    ''' this should contain the qualified field name.</param>
    ''' <param name="searchNestedFields">True to search within the nested fields
    ''' specified in the qualified name; False to search just within this level
    ''' of the collection definition.</param>
    ''' <returns>True if a field with the given name was found; False otherwise.
    ''' </returns>
    Public Function Contains(ByVal name As String, ByVal searchNestedFields As Boolean) As Boolean

        ' If we're not performing a full search, just use FindFieldWithoutRecursion()
        If Not searchNestedFields Then Return (FindFieldWithoutRecursion(name, -1) IsNot Nothing)

        ' Otherwise, search for the field - if the FieldNotFoundException is thrown,
        ' then it was not found.
        Try
            FindField(name)
            Return True

        Catch fnfe As FieldNotFoundException
            Return False

        End Try

    End Function

    ''' <summary>
    ''' Determines if this collection info can be mapped into another collection
    ''' info, by comparing the fields. This is used in 'check for errors' and is a
    ''' design-time only check. At runtime, a mapping that this says is ok may not
    ''' actually be ok - but only in the case where the source is undefined and the
    ''' target is defined.
    ''' </summary>
    ''' <param name="extinfo">The collection info to which this one should be
    ''' compared. Can be Nothing, which would indicate mapping into a stage with
    ''' no definition.</param>
    ''' <param name="reason">Carries back a brief explanation as to why the
    ''' collections are not equal, where appropriate.</param>
    ''' <param name="localName">A name to use for this collection, in the resulting
    ''' 'reason' message.</param>
    ''' <param name="extName">A name to use for the external collection, in the
    ''' resulting 'reason' message.</param>
    ''' <returns>Returns True if this collection can be mapped into the supplied
    ''' target collection info. Otherwise returns False, and 'reason' describes
    ''' why.</returns>
    ''' <remarks>This collection is considered mappable if all of the fields defined
    ''' here match exactly the fields defined in the target collection, or if either
    ''' one collection is dynamic (not defined).</remarks>
    Public Function CanMapInto(ByVal extinfo As clsCollectionInfo, ByRef reason As String, ByVal localName As String, ByVal extName As String) As Boolean

        If extinfo Is Nothing Then
            'If the target is a 'dynamic' (undefined) collection stage, then anything
            'can map into it!
            Return True
        End If

        'For each field defined locally, the external collection must have a matching
        'definition()...
        For Each objField As clsCollectionFieldInfo In Me.List
            If extinfo.Contains(objField.Name) Then
                If objField.DataType <> extinfo.GetField(objField.Name).DataType Then
                    reason = String.Format(My.Resources.Resources.TheField0IsDefinedInEachCollectionButTheDataTypesDoNotMatch, objField.Name)
                    Return False
                End If
            Else
                reason = String.Format(My.Resources.Resources.TheField0DefinedIn1IsNotPresentIn2, objField.Name, localName, extName)
                Return False
            End If
        Next

        'Moreover, the external collection is not allowed to define extra fields
        If extinfo.Count > Me.Count Then
            reason = String.Format(My.Resources.Resources.x0DefinesExtraFieldsNotPresentIn1, extName, localName)
            Return False
        End If

        Return True

    End Function

    ''' <summary>
    ''' Clones this collection info, creating a disassociated copy, ie. a new
    ''' collection info object <em>with no parent stage</em>, even if this
    ''' object has a parent stage.
    ''' </summary>
    ''' <returns>A collection info object with the same field information
    ''' as this object.</returns>
    Public Function Clone() As Object Implements ICloneable.Clone
        Dim copy As New clsCollectionInfo(Nothing, Nothing, mSingleRow, mFlat)
        For Each fld As clsCollectionFieldInfo In Me.List
            copy.AddField(fld)
        Next
        Return copy
    End Function

    ''' <summary>
    ''' Checks for equality against the given object.
    ''' </summary>
    ''' <param name="obj">The object to compare this object to.</param>
    ''' <returns>True if the given object is a non-null clsCollectionInfo object with
    ''' the same properties (attributes and fields) as this one. False otherwise.
    ''' <strong>Note: </strong> The ancestry of the definitions is not taken into
    ''' account in an equality test - just the attributes and fields of the
    ''' definition (and their descendants).</returns>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Return Equals(TryCast(obj, clsCollectionInfo))
    End Function

    ''' <summary>
    ''' Checks for equality against the given object.
    ''' </summary>
    ''' <param name="defn">The object to compare this object to.</param>
    ''' <returns>True if the given object is a non-null clsCollectionInfo with the
    ''' same properties (attributes and fields) as this one. False otherwise.
    ''' <strong>Note: </strong> The ancestry of the definitions is not taken into
    ''' account in an equality test - just the attributes and fields of the
    ''' definition (and their descendants).</returns>
    Public Overloads Function Equals(ByVal defn As clsCollectionInfo) As Boolean
        If defn Is Nothing Then Return False
        If defn Is Me Then Return True ' Shortcut check
        If Me.SingleRow <> defn.SingleRow _
         OrElse Me.Flat <> defn.Flat _
         OrElse Me.List.Count <> defn.List.Count Then Return False

        ' Collection definitions can't contain duplicates, so this check is sound.
        ' Loop through our fields and ensure that they match.
        For Each fld As clsCollectionFieldInfo In Me.List
            If Not defn.List.Contains(fld) Then Return False
        Next
        Return True
    End Function

    ''' <summary>
    ''' When used as a web service collection this holds the element
    ''' in the wsdl under which the collection is nested
    ''' </summary>
    ''' <value></value>
    Public Property NestingElement() As String
        Get
            Return mNestingElement
        End Get
        Set(ByVal value As String)
            mNestingElement = value
        End Set
    End Property
    <DataMember>
    Private mNestingElement As String

    ''' <summary>
    ''' Method that is called immediately after deserialization of an object in an 
    ''' object graph, and is used to set the Parent of each <see cref="clsCollectionFieldInfo "/>
    ''' instance in the underlying list.
    ''' </summary>
    ''' <remarks>
    ''' The Parent is set here rather than being included in the serialized data,
    ''' otherwise the XML generated exceeds the max depth when there are over 40 fields
    ''' in the collection.
    ''' </remarks>
    <OnDeserialized>
    Private Sub OnDeserialized(ByVal context As StreamingContext)
        For Each fld As clsCollectionFieldInfo In List

            If fld.Parent Is Nothing Then fld.Parent = Me

            ' If the field within the collection is also a collection, then we also
            ' need to loop through each field within that collection definition and set
            ' the parent
            For Each child As clsCollectionFieldInfo In fld.Children
                If child.Parent Is Nothing Then child.Parent = fld.Children
            Next
        Next
    End Sub

End Class
