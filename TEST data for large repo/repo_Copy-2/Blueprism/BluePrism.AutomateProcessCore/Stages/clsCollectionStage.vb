Imports System.Xml
Imports BluePrism.BPCoreLib.Collections
Imports System.Runtime.Serialization
Imports System.Linq

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsCollectionStage
    ''' 
    ''' <summary>
    ''' This class of objects represents collection stages.
    ''' Collection stages store data in a tabular form including rows and columns.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsCollectionStage
        Inherits clsDataStage
        Implements ICollectionDefinitionManager

#Region " Static methods "

        ''' <summary>
        ''' Generates a unique field name for the given collection definition
        ''' manager.
        ''' This is present on the collection stage class, simply because that is
        ''' where a unique field name is more likely to be needed (from a UI
        ''' perspective), but really it could live anywhere.
        ''' </summary>
        ''' <param name="stem">The prefix for the field name which is required.
        ''' </param>
        ''' <param name="mgr">The manager for which the name should be unique.
        ''' </param>
        ''' <returns>A string containing a field name which is guaranteed to be
        ''' unique within the specified manager (though not necessarily its
        ''' descendents)
        ''' </returns>
        Public Shared Function GenerateUniqueFieldName(
         ByVal stem As String, ByVal mgr As ICollectionDefinitionManager) As String

            Dim names As New clsSet(Of String)

            For Each field As clsCollectionFieldInfo In mgr.FieldDefinitions
                names.Add(field.Name)
            Next

            For i As Integer = 1 To Integer.MaxValue
                Dim candidate As String = stem & i
                If Not names.Contains(candidate) Then Return candidate
            Next

            Throw New IndexOutOfRangeException(My.Resources.Resources.clsCollectionStage_RanOutOfAvailableFieldNames)

        End Function

#End Region

#Region " Nested Value Operation Support "

        ''' <summary>
        ''' Class to handle the parameters used when performing operations on nested
        ''' collection values.
        ''' </summary>
        Friend Class CollectionDelegateParams

#Region " Member variables "

            ' The owner of the field for the scope of the operation.
            Private mOwner As clsCollection

            ' The current index being processed within the path
            Private mIndex As Integer

            ' The path describing the fields from the root (the stage) to the leaf
            ' (the target field itself)
            Private mPath As IList(Of String)

            ' The definition which is affected by the change detailed in these params
            Private mDefn As clsCollectionInfo

            ' The old field - used for renames and removes
            Private mOldField As clsCollectionFieldInfo

            ' The new field - used for renames and adds
            Private mNewField As clsCollectionFieldInfo

            ' The Boolean value in the params - used for SingleRow changes
            Private mBoolValue As Boolean
#End Region

#Region " CollectionDelegateParams Constructors "

            ''' <summary>
            ''' Creates a new params object detailing the given fields and path.
            ''' </summary>
            ''' <param name="defn">The collection schema that is affected by these
            ''' parameters</param>
            ''' <param name="oldFld">The old field - should be set to the old field
            ''' for renames, the field to be deleted for deletes, and null for adds.
            ''' </param>
            ''' <param name="newFld">The new field - should be set to the new field
            ''' for renames, the field to be added for adds, and null for removes.
            ''' </param>
            Public Sub New(
             ByVal defn As clsCollectionInfo,
             ByVal oldFld As clsCollectionFieldInfo,
             ByVal newFld As clsCollectionFieldInfo)
                Me.New(defn, oldFld, newFld, Nothing)
            End Sub

            ''' <summary>
            ''' Creates a new params object detailing the given value and path.
            ''' </summary>
            ''' <param name="defn">The collection schema that is affected by these
            ''' parameters</param>
            ''' <param name="value">The boolean value to be detailed in these params.
            ''' </param>
            Public Sub New(ByVal defn As clsCollectionInfo, ByVal value As Boolean)
                Me.New(defn, Nothing, Nothing, value)
            End Sub

            ''' <summary>
            ''' Creates a new params object detailing the given fields and path.
            ''' </summary>
            ''' <param name="defn">The collection schema that is affected by these
            ''' parameters
            ''' <param name="oldFld">The old field - should be set to the old field
            ''' for renames, the field to be deleted for deletes, and null for adds.
            ''' </param>
            ''' <param name="newFld">The new field - should be set to the new field
            ''' for renames, the field to be added for adds, and null for removes.
            ''' </param>
            ''' <param name="value">The boolean value to be detailed in these params.
            ''' </param>
            ''' </param>
            Private Sub New(
             ByVal defn As clsCollectionInfo,
             ByVal oldFld As clsCollectionFieldInfo,
             ByVal newFld As clsCollectionFieldInfo,
             ByVal value As Boolean)

                mDefn = defn
                mPath = SetPath(defn)
                mOldField = oldFld
                mNewField = newFld
                mBoolValue = value
                ResetPathCounter()
            End Sub



#End Region

#Region " Properties "

            ''' <summary>
            ''' The owner of the field for the scope of the operation.
            ''' This is likely to change multiple times within the lifetime of a
            ''' single parameters object as the operation is performed on multiple
            ''' collections which contain the same fields.
            ''' </summary>
            Public Property Owner() As clsCollection
                Get
                    Return mOwner
                End Get
                Set(ByVal value As clsCollection)
                    mOwner = value
                End Set
            End Property

            ''' <summary>
            ''' The old field - this represents the state of the field before the
            ''' operation. It will be present in renames, replaces, moves and
            ''' deletes.
            ''' It will be null for an add operation.
            ''' </summary>
            Public ReadOnly Property OldField() As clsCollectionFieldInfo
                Get
                    Return mOldField
                End Get
            End Property

            ''' <summary>
            ''' The new field - this represents the target state of the field after
            ''' the operation. It will be present in renames, replaces, moves and
            ''' adds.
            ''' It will be null for a delete operation.
            ''' </summary>
            Public ReadOnly Property NewField() As clsCollectionFieldInfo
                Get
                    Return mNewField
                End Get
            End Property

            ''' <summary>
            ''' The boolean value of these params - used to indicate the new value of
            ''' the <see cref="clsCollectionInfo.SingleRow">SingleRow</see> flag in
            ''' the collection definition.
            ''' </summary>
            Public ReadOnly Property BooleanValue() As Boolean
                Get
                    Return mBoolValue
                End Get
            End Property

#End Region

#Region " Path handling "

            ''' <summary>
            ''' Checks if the current path element is the last one in the path - ie.
            ''' we are currently at the leaf of the collection element represented by
            ''' the path.
            ''' </summary>
            Public ReadOnly Property LastInPath() As Boolean
                Get
                    Return mIndex = mPath.Count - 1
                End Get
            End Property

            ''' <summary>
            ''' Moves the path index onto the next field and returns it.
            ''' </summary>
            ''' <returns>The name of the next field in the path.</returns>
            ''' <exception cref="IndexOutOfRangeException">If the path index is
            ''' already at or beyond the end of the path.</exception>
            Public Function NextField() As String
                If mIndex < 0 OrElse mIndex >= mPath.Count - 1 Then
                    Throw New IndexOutOfRangeException(String.Format(
                     My.Resources.Resources.CollectionDelegateParams_PathHas0ElementsIndexIsCurrently1, mPath.Count, mIndex))
                End If
                mIndex += 1
                Return mPath(mIndex)
            End Function

            ''' <summary>
            ''' The current field within the path that this params object is pointing
            ''' to.
            ''' </summary>
            Public ReadOnly Property CurrentField() As String
                Get
                    If mIndex < 0 OrElse mIndex >= mPath.Count Then
                        Throw New IndexOutOfRangeException(
                         My.Resources.Resources.CollectionDelegateParams_ThereIsNoCurrentStageInThePath)
                    End If
                    Return mPath(mIndex)
                End Get
            End Property

            ''' <summary>
            ''' Moves the path index to the previous field and returns it.
            ''' </summary>
            ''' <returns>The name of the previous field in the path</returns>
            ''' <exception cref="IndexOutOfRangeException">If the path index is
            ''' already at the beginning of the path</exception>
            Public Function PreviousField() As String
                If mIndex = 0 Then
                    Throw New IndexOutOfRangeException(
                     My.Resources.Resources.CollectionDelegateParams_CurrentlyAtTheBeginningOfThePathCannotDecrementAnyFurther)
                End If
                mIndex -= 1
                Return mPath(mIndex)
            End Function

            ''' <summary>
            ''' Resets the path counter to point to the beginning of the path.
            ''' </summary>
            Public Sub ResetPathCounter()
                mIndex = 0
            End Sub

#End Region

        End Class

        ''' <summary>
        ''' Operation to be performed on nested values within nested collections.
        ''' Such delegates will probably be called multiple times with different
        ''' <see cref="CollectionDelegateParams.Owner">owners</see> as the 
        ''' collection tree is walked.
        ''' </summary>
        ''' <param name="params">The parameters defining the data to be used in
        ''' the operation.</param>
        Friend Delegate Sub NestedValueOperation(ByVal params As CollectionDelegateParams)

#End Region

        ''' <summary>
        ''' The definition (i.e. fields, data types) for this collection stage. This
        ''' can be Nothing, indicating that the stage will accept any kind of
        ''' collection. Otherwise the stage will only accept collections that match
        ''' the definition.
        ''' 
        ''' Note that modifying this definition directly, e.g. adding a field, is
        ''' NOT the correct way to go about it. Instead, use the methods of
        ''' clsCollectionStage (such as AddField) to make modifications to the
        ''' definition. Doing so allows consistent modifications to be made - for
        ''' example, removing a field in this will make appropriate changes to the
        ''' initial value as well.
        ''' </summary>
        Public ReadOnly Property Definition() As clsCollectionInfo
            Get
                Return mDefinition
            End Get
        End Property
        <DataMember>
        Private WithEvents mDefinition As clsCollectionInfo

        ''' <summary>
        ''' Creates a new instance of the clsCollectionStage class and sets its
        ''' parent</summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
            DataType = DataType.collection
        End Sub


        ''' <summary>
        ''' The collection in the initial value of this stage, or null if it has no
        ''' initial value.
        ''' </summary>
        Public ReadOnly Property InitialCollection() As clsCollection
            Get
                Dim pv As clsProcessValue = InitialValue
                If pv Is Nothing Then Return Nothing Else Return pv.Collection
            End Get
        End Property

        ''' <summary>
        ''' The collection in the current value of this stage, or null if it has no
        ''' current value.
        ''' </summary>
        Public ReadOnly Property CurrentCollection() As clsCollection
            Get
                Dim pv As clsProcessValue = Value
                If pv Is Nothing Then Return Nothing Else Return pv.Collection
            End Get
        End Property

        ''' <summary>
        ''' Checks if this stage has a current (non-null) collection value.
        ''' </summary>
        Public ReadOnly Property HasCollectionValue() As Boolean
            Get
                Return (CurrentCollection IsNot Nothing)
            End Get
        End Property

        ''' <summary>
        ''' Start iterating through the collection - i.e. set the current row to be
        ''' the first row.
        ''' </summary>
        ''' <returns>True if successful, or False if there is nothing to be iterated.
        ''' </returns>
        Public Function CollectionStartIterate() As Boolean
            'Can't iterate an empty (i.e. undefined) collection
            Return (HasCollectionValue AndAlso CurrentCollection.StartIterate())
        End Function

        ''' <summary>
        ''' Continue iterating through the collection - i.e. set the current row to
        ''' be the next row on from the current one
        ''' </summary>
        ''' <returns>True if successful, or False if there is nothing further to be
        ''' iterated.</returns>
        Public Function CollectionContinueIterate() As Boolean
            Return (HasCollectionValue AndAlso CurrentCollection.ContinueIterate())
        End Function


        ''' <summary>
        ''' Gets a number showing the current row number whilst iterating the
        ''' collection at runtime.
        ''' </summary>
        ''' <remarks>Relevant only if this stage is a collection stage</remarks>
        ''' <exception cref="InvalidDataTypeException">If this stage contains a value
        ''' which is of a different type to a collection</exception>
        ''' <exception cref="ApplicationException">If the collection backing this
        ''' stage is not currently in iteration.</exception>
        Public Function CollectionCurrentIterationIndex() As Integer

            If mValue IsNot Nothing Then
                If mValue.DataType <> DataType.collection Then _
                 Throw New InvalidDataTypeException(My.Resources.Resources.clsCollectionStage_BadDatatype0, mValue.DataType)

                If mValue.Collection IsNot Nothing Then
                    Dim index As Integer = mValue.Collection.CurrentRowIndex
                    If index <> -1 Then Return index
                End If
            End If

            ' If we reach here then either :
            ' mValue is null; or mValue.Collection is null; or CurrentRowIndex returned -1
            ' Whichever, it means the collection is not currently in iteration.
            Throw New InvalidOperationException(My.Resources.Resources.clsCollectionStage_TheCollectionIsNotCurrentlyInAnIterationAndThereforeTheIterationIndexIsNotDefin)

        End Function

        ''' <summary>
        ''' Gets the number of rows held in the Collection represented by this stage.
        ''' Relevant only when the stage type is Collection.
        ''' </summary>
        ''' <value>The number of rows of data currently held.</value>
        ''' <remarks>Relevant only if this stage is a collection stage</remarks>
        Public ReadOnly Property CollectionRowCount() As Integer
            Get
                If HasCollectionValue Then Return mValue.Collection.Count
                Return 0
            End Get
        End Property


        ''' <summary>
        ''' Determines if the the stage is currently in a collection iteration.
        ''' </summary>
        ''' <returns>True if this stage is a collection stage and it is currently in
        ''' a loop iteration. False otherwise.</returns>
        Public Function CollectionIsInIteration() As Boolean
            Return HasCollectionValue AndAlso mValue.Collection.IsInIteration
        End Function


        ''' <summary>
        ''' Gets the current row from the collection
        ''' </summary>
        ''' <value>The current row of the collection or nothing if
        ''' there is no current row</value>
        ''' <remarks>Relevant only if this stage is a collection stage</remarks>
        ''' <exception cref="InvalidDataTypeException">If there is a value held in
        ''' this stage which is not a collection value.</exception>
        Public ReadOnly Property CollectionCurrentRow() As clsCollectionRow
            Get
                ' If there's nothing there - either no value at all or a null
                ' collection, return null to indicate "no current row"
                If mValue Is Nothing Then Return Nothing

                ' If there is a value there, but it's not a collection... well,
                ' that's "bad". Make it stop.
                If mValue.DataType <> DataType.collection Then _
                 Throw New InvalidDataTypeException(My.Resources.Resources.clsCollectionStage_BadDatatype0, mValue.DataType)

                ' Again - null collection means "no current row"
                If mValue.Collection Is Nothing Then Return Nothing

                Return mValue.Collection.GetCurrentRow()

            End Get
        End Property


        ''' <summary>
        ''' Get the value of a given field from the CURRENT row of this collection.
        ''' </summary>
        ''' <param name="fieldName">The name of the field to get</param>
        ''' <param name="value">A reference to a clsProcessValue, which will
        ''' contain the value on return.</param>
        ''' <param name="sErr">On failure, an error message.</param>
        ''' <returns>True if successful, False otherwise</returns>
        Public Function CollectionGetField(ByVal fieldName As String, ByRef value As clsProcessValue, ByRef sErr As String) As Boolean

            Try
                value = GetFieldValue(fieldName)
                Return True
            Catch ex As Exception
                sErr = ex.Message
                Return False
            End Try

        End Function

        ''' <summary>
        ''' Gets the value for the given field within the current row of this
        ''' collection.
        ''' </summary>
        ''' <param name="fldName">The name of the field for which the value of the
        ''' current row is required</param>
        ''' <returns>The value within the current row of the specified field.
        ''' </returns>
        ''' <exception cref="EmptyCollectionException">If the collection in this
        ''' stage has no data, according to <see cref="HasCollectionValue"/>
        ''' </exception>
        Public Function GetFieldValue(ByVal fldName As String) As clsProcessValue
            If Not HasCollectionValue Then Throw New EmptyCollectionException(
             My.Resources.Resources.clsCollectionStage_CollectionHasNoData)
            Return mValue.Collection.GetField(fldName)
        End Function

        ''' <summary>
        ''' Checks if this stage currently contains a field with the given name.
        ''' </summary>
        ''' <param name="name">The name to search this collection definition for.</param>
        ''' <returns>True if there exists a field in the definition managed by this
        ''' object with the given name; False otherwise.</returns>
        Public Function ContainsField(ByVal name As String) As Boolean _
         Implements ICollectionDefinitionManager.ContainsField
            Return (GetFieldDefinition(name) IsNot Nothing)
        End Function


        ''' <summary>
        ''' Gets the collection field definition within this stage or any of its
        ''' descendents for the given fully qualified name.
        ''' If any of the fields specified in the FQN are not found then null is
        ''' returned.
        ''' </summary>
        ''' <param name="fullyQualifiedName">The fully qualified name of the field
        ''' for which the field definition object is required. Note that the first
        ''' element in the fully qualified name is the stage name, or a placeholder
        ''' indicating that the stage name is unknown - whatever it is, it will be
        ''' ignored by this method, such that "&lt;?&gt;.Field.Inner.Other Field"
        ''' will be treated exactly the same as "$.Field.Inner.Other Field".</param>
        ''' <returns>The collection field definition representing the given fully
        ''' qualified name, or null if no such definition was found within this
        ''' stage or its descendents.</returns>
        Public Function FindFieldDefinition(ByVal fullyQualifiedName As String) As clsCollectionFieldInfo
            If String.IsNullOrEmpty(fullyQualifiedName) Then Return Nothing
            Dim tree() As String = fullyQualifiedName.Split(New Char() {"."c})

            ' Ignore the first one - that's the stage name.
            Dim mgr As ICollectionDefinitionManager = Me
            Dim fld As clsCollectionFieldInfo = Nothing

            ' Loop through each leaf in the tree, getting each field definition
            ' in turn.
            For i As Integer = 1 To tree.Length - 1

                ' If we have no definition for this field, or the field does
                ' not contain the current child field, then the required field
                ' was not found in this stage - return as much to the caller.
                If mgr Is Nothing OrElse Not mgr.ContainsField(tree(i)) Then Return Nothing

                ' Otherwise, save the field and update the manager to point to
                ' its children.
                fld = mgr.GetFieldDefinition(tree(i))
                mgr = fld.Children

            Next

            ' We've come out of the loop - the field will be either the latest
            ' one found - ie. the last element of the 'tree' array, or null,
            ' indicating the loop was not entered - ie. the FQN contained 1 element.
            Return fld

        End Function

        ''' <summary>
        ''' Gets the collection field definition from this stage with the specified
        ''' name
        ''' </summary>
        ''' <param name="name">The name of the field definition required.</param>
        ''' <returns>The field corresponding to the given name, or null if no such
        ''' field was found in this stage.</returns>
        Public Function GetFieldDefinition(ByVal name As String) As clsCollectionFieldInfo _
         Implements ICollectionDefinitionManager.GetFieldDefinition
            If mDefinition Is Nothing Then Return Nothing
            Return mDefinition.GetField(name)
        End Function

        ''' <summary>
        ''' Gets the collection field definitions which are currently being held by this
        ''' stage.
        ''' </summary>
        Public ReadOnly Property FieldDefinitions() As IEnumerable(Of clsCollectionFieldInfo) _
         Implements ICollectionDefinitionManager.FieldDefinitions
            Get
                If mDefinition Is Nothing Then Return New clsCollectionFieldInfo() {}
                Return mDefinition.FieldDefinitions
            End Get
        End Property

        ''' <summary>
        ''' Gets the number of field definitions currently held by this stage.
        ''' </summary>
        Public ReadOnly Property FieldCount() As Integer _
         Implements ICollectionDefinitionManager.FieldCount
            Get
                If mDefinition Is Nothing Then Return 0
                Return mDefinition.Count
            End Get
        End Property

        ''' <summary>
        ''' Adds a field described in the given XML element to the definition
        ''' managed by this object.
        ''' </summary>
        ''' <param name="el">The element containing the field to add.</param>
        Private Sub AddField(ByVal el As XmlElement)
            AddField(New clsCollectionFieldInfo(Nothing, el))
        End Sub

        ''' <summary>
        ''' Add a new field to the collection's definition.
        ''' </summary>
        ''' <param name="name">The name of the new field.</param>
        ''' <param name="datatype">The data type of the new field.</param>
        Public Sub AddField(ByVal name As String, ByVal datatype As DataType) _
         Implements ICollectionDefinitionManager.AddField
            AddField(New clsCollectionFieldInfo(name, datatype, String.Empty))
        End Sub

        ''' <summary>
        ''' Add a new field to the collection's definition.
        ''' </summary>
        ''' <param name="name">The name of the new field.</param>
        ''' <param name="datatype">The data type of the new field.</param>
        ''' <param name="ns">The namespace of the new field.</param>
        ''' <remarks></remarks>
        Public Sub AddField(ByVal name As String, ByVal datatype As DataType, ns As String) _
            Implements ICollectionDefinitionManager.AddField
            AddField(New clsCollectionFieldInfo(name, datatype, ns))
        End Sub


        ''' <summary>
        ''' Adds a field containing the same data as the given field info to the
        ''' definition managed by this object.
        ''' Note that the actual object given is not added, but an object with the
        ''' same value (ie. name, description, datatype, children) as the given
        ''' field.
        ''' </summary>
        ''' <param name="fld">The field to add to this definition manager.</param>
        Public Sub AddField(ByVal fld As clsCollectionFieldInfo) _
         Implements ICollectionDefinitionManager.AddField

            If mDefinition Is Nothing Then mDefinition = New clsCollectionInfo(Me)
            mDefinition.AddField(fld)

            ' The definition throws 'FieldAdded' events which this stage uses to ensure that
            ' any nested collections are kept up to date with schema changes.

        End Sub

#Region " Definition Modification Handlers "

        ''' <summary>
        ''' Registers a change in the collection schema defined in this stage.
        ''' This ensures that the values held in this stage are updated with the new
        ''' structure.
        ''' Note that the field in question may not be owned directly by this stage -
        ''' it may come from a collection field in this stage, or nested further
        ''' down the structure - its heritage can be examined using the fully
        ''' qualified name of the field.
        ''' </summary>
        ''' <param name="defn">The definition which is the ultimate ancestor of the
        ''' field that has been added.</param>
        ''' <param name="fld">The field that has been added.</param>
        Private Sub HandleFieldAdded(
         ByVal defn As clsCollectionInfo, ByVal fld As clsCollectionFieldInfo) _
         Handles mDefinition.FieldAdded

            PerformNestedFieldOperation(
             New CollectionDelegateParams(defn, Nothing, fld),
             AddressOf AddFieldOperation)

        End Sub

        ''' <summary>
        ''' Handles a field being removed within the nested structure of this stage.
        ''' </summary>
        ''' <param name="defn">The collection definition in which the field deletion
        ''' took place.</param>
        ''' <param name="fld">The field that was removed from the definition.</param>
        Private Sub HandleFieldRemoved(
         ByVal defn As clsCollectionInfo, ByVal fld As clsCollectionFieldInfo) _
         Handles mDefinition.FieldRemoved

            PerformNestedFieldOperation(
             New CollectionDelegateParams(defn, fld, Nothing),
             AddressOf DeleteFieldOperation)

        End Sub

        ''' <summary>
        ''' Handles a field within the nested structure of this stage being modified
        ''' (renamed or data type changing).
        ''' </summary>
        ''' <param name="defn">The collection definition in which the field
        ''' modification took place.</param>
        ''' <param name="oldFld">The field object representing the old state of the
        ''' collection field.</param>
        ''' <param name="newFld">The field object representing the new state of the
        ''' collection field.</param>
        Private Sub HandleFieldModified(
         ByVal defn As clsCollectionInfo,
         ByVal oldFld As clsCollectionFieldInfo, ByVal newFld As clsCollectionFieldInfo) _
         Handles mDefinition.FieldModified

            PerformNestedFieldOperation(
             New CollectionDelegateParams(defn, oldFld, newFld),
             AddressOf ModifyFieldOperation)

        End Sub

        ''' <summary>
        ''' Handles the SingleRow property of a nested collection definition within
        ''' this stage being changed.
        ''' </summary>
        ''' <param name="defn">The collection definition on which the property
        ''' modification took place.</param>
        ''' <param name="value">The new value for the SingleRow property.</param>
        Private Sub HandleSingleRowChanged(
         ByVal defn As clsCollectionInfo, ByVal value As Boolean) _
         Handles mDefinition.SingleRowChanged

            PerformNestedFieldOperation(
             New CollectionDelegateParams(defn, value),
             AddressOf ChangeSingleRowOperation)

        End Sub


        ''' <summary>
        ''' Performs the required operation, using the specified parameters on the
        ''' values currently held in this stage - both initial and current values,
        ''' if they exist.
        ''' </summary>
        ''' <param name="params">The parameters containing the metadata for the
        ''' operation.</param>
        ''' <param name="op">The delegate which performs the operation</param>
        Private Sub PerformNestedFieldOperation(
         ByVal params As CollectionDelegateParams, ByVal op As NestedValueOperation)

            Dim v As clsProcessValue = GetInitialValue()
            If v.Collection IsNot Nothing Then
                v.Collection.PerformNestedValueOperation(params, op)
            End If
            If HasCollectionValue Then
                mValue.Collection.PerformNestedValueOperation(params, op)
            End If

        End Sub

#End Region

#Region " Nested Collection Value Operations "

        ''' <summary>
        ''' Operation to add a field to a nested collection value.
        ''' </summary>
        ''' <param name="params">The parameters defining the add operation.</param>
        Private Sub AddFieldOperation(ByVal params As CollectionDelegateParams)
            params.Owner.AddField(params.NewField, False)
        End Sub

        ''' <summary>
        ''' Operation to delete a field from a nested collection.
        ''' </summary>
        ''' <param name="params">The parameters defining the delete operation.</param>
        Private Sub DeleteFieldOperation(ByVal params As CollectionDelegateParams)
            params.Owner.DeleteField(params.OldField.Name)
        End Sub

        ''' <summary>
        ''' Operation to modify a field in a nested collection.
        ''' </summary>
        ''' <param name="params">The parameters defining the modify operation.</param>
        Private Sub ModifyFieldOperation(ByVal params As CollectionDelegateParams)

            Dim owner As clsCollection = params.Owner
            Dim oldFld As clsCollectionFieldInfo = params.OldField
            Dim newFld As clsCollectionFieldInfo = params.NewField

            If oldFld.Name <> newFld.Name Then _
             owner.RenameField(oldFld.Name, newFld.Name)

            If oldFld.DataType <> newFld.DataType Then _
             owner.ChangeFieldDataType(newFld.Name, newFld.DataType)

            If oldFld.Description <> newFld.Description Then _
             owner.SetFieldDescription(newFld.Name, newFld.Description)

        End Sub

        ''' <summary>
        ''' Operation to remove a field from a nested collection value.
        ''' </summary>
        ''' <param name="params">The parameters defining the remove operation.</param>
        Private Sub RenameFieldOperation(ByVal params As CollectionDelegateParams)
            params.Owner.RenameField(params.OldField.Name, params.NewField.Name)
        End Sub

        ''' <summary>
        ''' Operation to change the datatype in a nested collection value.
        ''' </summary>
        ''' <param name="params">The parameter defining the operation.</param>
        Private Sub ChangeDataTypeOperation(ByVal params As CollectionDelegateParams)
            params.Owner.ChangeFieldDataType(params.OldField.Name, params.NewField.DataType)
        End Sub

        ''' <summary>
        ''' Operation to change the 'SingleRow' attribute in a nested collection value.
        ''' </summary>
        ''' <param name="params">The parameters defining the operation.</param>
        Private Sub ChangeSingleRowOperation(ByVal params As CollectionDelegateParams)
            params.Owner.SingleRow = params.BooleanValue
        End Sub

#End Region

        ''' <summary>
        ''' Delete a field from the collection. Corresponding values are deleted from
        ''' existing rows. Deleting the last field means the stage no longer has a
        ''' definition.
        ''' </summary>
        ''' <param name="name">The name of the field to delete.</param>
        Public Sub DeleteField(ByVal name As String) Implements ICollectionDefinitionManager.DeleteField
            If mDefinition Is Nothing Then Return

            mDefinition.DeleteField(name)

            'Last field deleted? No more definition.
            If mDefinition.Count = 0 Then
                mDefinition = Nothing
            End If

        End Sub

        ''' <summary>
        ''' Clears the fields on this stage - this has the effect of clearing the
        ''' initial value of the stage too - since no initial value can exist if no
        ''' fields exist.
        ''' </summary>
        Public Sub ClearFields() Implements ICollectionDefinitionManager.ClearFields
            If mDefinition IsNot Nothing Then
                mDefinition.ClearFields()
                mDefinition = Nothing
            End If
        End Sub

        ''' <summary>
        ''' Rename a field in the collection. Values are also renamed in existing
        ''' rows. (Note - this should be irrelevant, it's an artefact of historical
        ''' silliness that the rows also contain named items!)
        ''' </summary>
        ''' <param name="oldname">The old (existing) name.</param>
        ''' <param name="newname">The new name.</param>
        Public Sub RenameField(ByVal oldname As String, ByVal newname As String) Implements ICollectionDefinitionManager.RenameField
            mDefinition.RenameField(oldname, newname)
        End Sub


        ''' <summary>
        ''' Change the datatype of a field in the collection definition.
        ''' </summary>
        ''' <param name="name">The name of the field.</param>
        ''' <param name="newtype">The new datatype.</param>
        Public Sub ChangeFieldDataType(ByVal name As String, ByVal newtype As DataType) Implements ICollectionDefinitionManager.ChangeFieldDataType
            mDefinition.ChangeFieldDataType(name, newtype)
        End Sub

        ''' <summary>
        ''' Set the description of a field in the collection definition.
        ''' </summary>
        ''' <param name="name">The name of the field which needs its description
        ''' changing</param>
        ''' <param name="desc">The description of the field to set.</param>
        Public Sub SetFieldDescription(ByVal name As String, ByVal desc As String) Implements ICollectionDefinitionManager.SetFieldDescription
            mDefinition.SetFieldDescription(name, desc)
        End Sub

        ''' <summary>
        ''' True if the collection is a single-row collection. Changing the value of
        ''' this property will result in any underlying collection data being altered!
        ''' </summary>
        Public Property SingleRow() As Boolean Implements ICollectionDefinitionManager.SingleRow
            Get
                If mDefinition Is Nothing Then Return False
                Return mDefinition.SingleRow
            End Get
            Set(ByVal value As Boolean)
                'Setting this when there is no definition is a valid thing to do - it
                'creates the definition.
                If mDefinition Is Nothing Then
                    mDefinition = New clsCollectionInfo(Me)
                End If
                mDefinition.SingleRow = value
            End Set
        End Property

        ''' <summary>
        ''' Initialises the given process value with the collection definition from 
        ''' this stage.
        ''' This will only initialise it if it has no current collection, or it
        ''' contains a collection with no definition data.
        ''' </summary>
        ''' <param name="pv">The process value to initialise with the definition data
        ''' from this stage.</param>
        ''' <returns>The given process value with the collection initialised as
        ''' specified.</returns>
        Private Function InitFieldDefinition(ByVal pv As clsProcessValue) As clsProcessValue
            If pv IsNot Nothing Then
                ' This is a collection stage - the process value should have a
                ' collection and should be a collection datatype
                pv.DataType = DataType.collection
                Dim coll As clsCollection = pv.Collection
                If mDefinition IsNot Nothing AndAlso coll IsNot Nothing AndAlso
                 coll.Definition.Count = 0 Then
                    For Each f As clsCollectionFieldInfo In mDefinition
                        coll.AddField(f)
                    Next
                End If
            End If
            Return pv
        End Function

        ''' <summary>
        ''' Get the initial value for this stage.
        ''' If the process value is uninitialised, this will initialise it with the
        ''' current field definition as defined in this stage.
        ''' </summary>
        ''' <returns>The initial value as a clsProcessValue</returns>
        Public Overrides Function GetInitialValue() As clsProcessValue
            Dim pv As clsProcessValue = MyBase.GetInitialValue()
            ' Initial value of a stage can never be null (where
            ' current value *can*)
            If pv.Collection Is Nothing Then pv.Collection = New clsCollection()
            Return InitFieldDefinition(pv)
        End Function

        ''' <summary>
        ''' Get the current value for this stage.
        ''' If the current value is uninitialised, this will will initialise it with
        ''' the current field definition as defined in this stage.
        ''' </summary>
        ''' <returns>The current value as a clsProcessValue</returns>
        Public Overrides Function GetValue() As clsProcessValue
            Return InitFieldDefinition(MyBase.GetValue())
        End Function

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of a collection stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsCollectionStage(mParent)
        End Function


        ''' <summary>
        ''' Creates a deep copy of the collection stage.
        ''' </summary>
        ''' <returns>A copy of this collection stage</returns>
        Public Overrides Function Clone() As clsProcessStage
            Dim copy As clsCollectionStage = CType(MyBase.Clone(), clsCollectionStage)
            If mDefinition IsNot Nothing Then
                Dim info As New clsCollectionInfo(copy)
                For Each fld As clsCollectionFieldInfo In mDefinition
                    info.AddField(fld)
                Next
                copy.mDefinition = info
                ' No need to check singlerow if there's no definition - it wil be false.
                copy.SingleRow = Me.SingleRow
            End If
            Return copy
        End Function


        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Collection
            End Get
        End Property

        ''' <summary>
        ''' Finds any repeated names in this collection.
        ''' Collections should not be able to exist with repeated names, but at
        ''' the moment, there are no safeguards in place, so any using code must be
        ''' aware of the potential problem and check using this function.
        ''' </summary>
        ''' <returns>A collection of names which were found to be repeated within
        ''' this collection.</returns>
        Public Function FindRepeatedNames() As ICollection(Of String)

            Dim names As New clsSet(Of String)
            Dim repeats As ICollection(Of String) = Nothing

            If mDefinition IsNot Nothing Then
                For Each field As clsCollectionFieldInfo In mDefinition
                    If Not names.Add(field.Name) Then
                        If repeats Is Nothing Then repeats = New clsOrderedSet(Of String)
                        repeats.Add(field.Name)
                    End If
                Next
            End If
            If repeats Is Nothing Then Return New String() {}
            Return repeats

        End Function


        ''' <summary>
        ''' Generates a unique field name for this collection, based on the given
        ''' stem.
        ''' </summary>
        ''' <param name="stem">The prefix for the field name which is required,
        ''' typically "New Field " or some such.</param>
        ''' <returns>A field name based on the given stem which is unique to this
        ''' collection.</returns>
        Public Function GenerateUniqueFieldName(ByVal stem As String) As String
            Return GenerateUniqueFieldName(stem, Me)
        End Function

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult
            Return New StageResult(False, "Internal", My.Resources.Resources.clsCollectionStage_CanTExecuteStage & GetName())
        End Function

        Public Overrides Sub FromXML(ByVal elem As XmlElement)
            MyBase.FromXML(elem)

            mAlwaysInit = True

            ' We do two passes of the attributes defined for the stage in the XML
            ' For the first pass, we build up the collection definition for the stage
            Dim initValElem As XmlNode = Nothing
            For Each attrElem As XmlElement In elem.ChildNodes
                Select Case attrElem.Name
                    Case "collectionsource"
                        'This is for backwards compatability
                        'collectionsource will not be saved instead collectioninfo will be saved
                        Dim bo As clsBusinessObject =
                         mParent.GetBusinessObjects().FindObjectReference(
                         attrElem.GetAttribute("object"))
                        If bo Is Nothing Then Continue For

                        Dim act As clsBusinessObjectAction =
                         bo.GetAction(attrElem.GetAttribute("action"))
                        If act Is Nothing Then Continue For

                        Dim param As clsProcessParameter = act.GetParameter(
                         attrElem.GetAttribute("output"), ParamDirection.Out)
                        If param IsNot Nothing Then mDefinition = param.CollectionInfo

                    Case "collectioninfo"
                        Dim added As Boolean = False
                        Dim singleRow As Boolean = False

                        For Each defnElem As XmlElement In attrElem.ChildNodes
                            Select Case defnElem.Name
                                Case "singlerow" : singleRow = True
                                    ' Use AddField() to ensure that the contained collection
                                    ' is kept up to date with the correct structure
                                Case "field" : AddField(defnElem) : added = True
                            End Select
                        Next
                        'Only set if if there were any actual fields, because old XML can contain
                        'an empty 'collectioninfo' element, which means there is no definition -
                        'the same as not having one at all.
                        If added Then mDefinition.SingleRow = singleRow

                    Case "noalwaysinit"
                        mAlwaysInit = False

                    Case "initialvalue"
                        initValElem = attrElem
                End Select
            Next

            ' Now we set the initial value
            ' If we have no initial value, no need to continue.
            If initValElem Is Nothing Then Return

            ' Otherwise, set it as the initial value
            SetInitialValue(clsCollection.ParseWithoutRoot(initValElem.InnerXml))

            ' Finally, we want to clear any columns in the initial value of this
            ' stage which aren't in the stage's definition

            ' If there is no definition for this stage, then any columns are welcome
            If mDefinition Is Nothing OrElse mDefinition.Count = 0 Then Return

            ' If there's no initial value, there's nothing to trim
            If InitialCollection Is Nothing Then Return

            ' If there's no collection on that initial value or it has no fields
            ' defined, again - nothing to trim
            Dim coll As clsCollection = InitialCollection
            If coll.Count = 0 OrElse coll.Definition.Count = 0 Then Return

            ' Otherwise we have a definition with some fields defined.
            ' Ensure that the initial value adheres to it.
            RationaliseDefinitions(coll)

        End Sub

        ''' <summary>
        ''' Trims the values in this stage, removing any fields in those values which
        ''' are not present in the populated definition on this stage. If this stage
        ''' has no definition or it is empty, the values are left as they are.
        ''' Equally, if a value is not set or it has no definition or collection, it
        ''' is left as it is
        ''' </summary>
        Public Sub TrimValuesToDefinition()
            RationaliseDefinitions(InitialCollection)
            RationaliseDefinitions(CurrentCollection)
        End Sub

        Private Shared Function SetPath(defn As clsCollectionInfo) As IList(Of String)
            Dim fullyQualifiedNamePath = defn.FullyQualifiedName?.Split("."c)
            If fullyQualifiedNamePath IsNot Nothing Then
                Return fullyQualifiedNamePath
            End If
            'clsProcessStage->msName can be null from the constructor.  in this case just create list to control the programs flow.
            Return {String.Empty}.ToList()
        End Function

        ''' <summary>
        ''' Trims the excess fields from the target definition from within this
        ''' collection (it may be the top level or nested within it) which do not
        ''' appear in the guide definition. 
        ''' 
        ''' Also, ensures that any field definition defined in 
        ''' <paramref name="guide"/> are honoured in the <paramref name="target"/>.
        ''' 
        ''' This acts recursively to ensure that the target is fully contained by the
        ''' guide.
        ''' </summary>
        ''' <param name="coll">The collection on which the fields are being
        ''' rationalised </param>
        ''' <param name="guide">The guide definition to adhere to - the definition
        ''' which contains all of the columns which are allowed to be present in the
        ''' target definition, and whose single row configuration may be set in the 
        ''' target definition.</param>
        ''' <param name="target">The target definition whose excess columns should be
        ''' trimmed</param>
        Private Sub RationaliseDefinitions(coll As clsCollection,
         guide As clsCollectionInfo, target As clsCollectionInfo)

            Dim removals As New List(Of clsCollectionFieldInfo)

            For Each fld As clsCollectionFieldInfo In target.FieldDefinitions
                Dim guideFld = guide.GetField(fld.Name)

                If guideFld Is Nothing Then
                    removals.Add(fld)
                Else
                    fld.Children.SetFrom(guideFld.Children)
                End If
            Next

            For Each fld As clsCollectionFieldInfo In removals
                ' We use the top level definition for the nested value operation to
                ' ensure that we get the right point within the nested collections
                coll.PerformNestedValueOperation(
                 New CollectionDelegateParams(guide, fld, Nothing),
                 AddressOf DeleteFieldOperation)
                target.DeleteField(fld.Name)
            Next

            ' We now want to recurse through each level to ensure that nested
            ' fields are trimmed too
            For Each fld As clsCollectionFieldInfo In target.FieldDefinitions
                ' We, er, don't drill down into non-collection fields
                If fld.DataType <> DataType.collection Then Continue For

                ' Get the corresponding definition from the guide - we know that all
                ' of the fields in the target exist in the guide (we've just removed
                ' any that do not), so this is safe
                RationaliseDefinitions(
                 coll, guide.GetField(fld.Name).Children, fld.Children)
            Next

        End Sub

        ''' <summary>
        ''' Trims the excess fields from a collection's definition and its data which
        ''' do not appear in this stage's definition. This acts recursively to ensure
        ''' that the target is fully contained by the guide.
        ''' 
        ''' Also, ensures that any field definitions defined in this stage's
        ''' definition are honoured in <paramref name="coll"/>.
        ''' 
        ''' This acts recursively to ensure that the target is fully contained by the
        ''' guide.
        ''' </summary>
        ''' <param name="coll">The collection which is to be trimmed of its excess
        ''' fields and corresponding data.</param>
        ''' <remarks>If either the definition in this stage or the definition in the
        ''' given collection are null, then no action is performed by this method
        ''' </remarks>
        Private Sub RationaliseDefinitions(ByVal coll As clsCollection)
            If mDefinition Is Nothing OrElse mDefinition.Count = 0 Then Return
            If coll Is Nothing OrElse coll.Definition.Count = 0 Then Return
            RationaliseDefinitions(coll, mDefinition, coll.Definition)
        End Sub

        ''' <summary>
        ''' Serializes this collection stage to XML.
        ''' </summary>
        ''' <param name="doc">The XML document to use to create the XML elements.
        ''' </param>
        ''' <param name="stgElem">The element representing the stage to which the
        ''' XML should be appended.</param>
        ''' <param name="selectionOnly">True to only serialize this stage if it is
        ''' <see cref="IsSelected">selected</see>; False to serialize, regardless.
        ''' </param>
        Public Overrides Sub ToXml(
         doc As XmlDocument, stgElem As XmlElement, selectionOnly As Boolean)
            MyBase.ToXml(doc, stgElem, selectionOnly)

            If mDefinition IsNot Nothing Then
                Dim defnElem = doc.CreateElement("collectioninfo")
                If mDefinition.SingleRow Then defnElem.AppendChild(
                    doc.CreateElement("singlerow"))

                For Each fld As clsCollectionFieldInfo In mDefinition
                    fld.ToXml(doc, defnElem)
                Next

                stgElem.AppendChild(defnElem)
            End If

            'Save the collection initial value
            Dim initVal As clsProcessValue = GetInitialValue()
            If initVal.HasCollectionData Then
                Dim coll = initVal.Collection
                Dim initValElem = doc.CreateElement("initialvalue")

                ' Write the inital values out as an xml string
                ' Done differently in case there's control characters 
                ' in the initial values.
                stgElem.AppendChild(initValElem)
                coll.GenerateXMLInternal(doc, initValElem, False)
            End If

            If Not mAlwaysInit Then stgElem.AppendChild(
                doc.CreateElement("noalwaysinit"))

        End Sub

        Public Overrides Function CheckForErrors(ByVal bAttemptRepair As Boolean, ByVal SkipObjects As Boolean) As ValidationErrorList

            Dim errors As ValidationErrorList = MyBase.CheckForErrors(bAttemptRepair, SkipObjects)

            'Validate field names in the definition...
            If mDefinition IsNot Nothing Then
                For Each f As clsCollectionFieldInfo In mDefinition
                    If Not clsCollectionInfo.IsValidFieldName(f.Name) Then
                        errors.Add(New ValidateProcessResult(Me, "frmStagePropertiesCollection.htm", 113, f.Name))
                    End If
                Next
            End If

            'Validate field names in the initial value...
            Dim v As clsProcessValue = GetInitialValue()
            If v.Collection IsNot Nothing Then
                For Each f As clsCollectionFieldInfo In v.Collection.Definition
                    If Not clsCollectionInfo.IsValidFieldName(f.Name) Then
                        errors.Add(New ValidateProcessResult(Me, "frmStagePropertiesCollection.htm", 114, f.Name))
                    End If
                Next
            End If

            Return errors

        End Function

    End Class
End Namespace
