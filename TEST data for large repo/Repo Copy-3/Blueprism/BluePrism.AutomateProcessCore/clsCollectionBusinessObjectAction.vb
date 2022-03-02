Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Base class for collection business object actions. This provides mechanisms which
''' all of the collection business object actions use.
''' </summary>
Public MustInherit Class clsCollectionBusinessObjectAction
    Inherits clsInternalBusinessObjectAction

    ''' <summary>
    ''' Creates a new collection business object action within the given business
    ''' object parent.
    ''' </summary>
    ''' <param name="parent">The business object that this is an action within
    ''' </param>
    Protected Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)
        AddParameter("clsCollectionActions_Params_CollectionName", DataType.text, ParamDirection.In,
         My.Resources.Resources.clsCollectionBusinessObjectAction_TheNameOfTheCollectionToActUpon)
    End Sub

    ''' <summary>
    ''' Gets whether single row collections are allowed or not by this action.
    ''' By default, they are allowed, but subclasses can override this property to
    ''' alter the behaviour of <see cref="GetCollection"/>
    ''' </summary>
    Protected Overridable ReadOnly Property AllowsSingleRowCollections As Boolean
        Get
            Return True
        End Get
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets whether this action requires there to be a current row set in the
    ''' target collection. This further implies that the target collection must have
    ''' some data.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Protected Overridable ReadOnly Property RequiresCurrentRow As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Gets the name of the collection which is being operated on in this action.
    ''' This may be null if this action has no collection name parameter, or an
    ''' empty string if the parameter exists, but no argument has been set.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Protected ReadOnly Property CollectionName As String
        Get
            Return Inputs.GetString("Collection Name")
        End Get
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the default preconditions for a collection business object action.
    ''' This is simply stating that the the stage must exist and be within scope.
    ''' </summary>
    ''' <returns>The default preconditions for this action</returns>
    Private Function GetBasePreConditions() As Collection
        If AllowsSingleRowCollections Then
            Return BuildCollection(
             My.Resources.Resources.clsCollectionBusinessObjectAction_TheCollectionStageMustExist,
             My.Resources.Resources.clsCollectionBusinessObjectAction_TheCollectionStageMustBeWithinTheScopeOfTheActionStage)
        Else
            Return BuildCollection(
             My.Resources.Resources.clsCollectionBusinessObjectAction_TheCollectionStageMustExist,
             My.Resources.Resources.clsCollectionBusinessObjectAction_TheCollectionStageMustBeWithinTheScopeOfTheActionStage,
             My.Resources.Resources.clsCollectionBusinessObjectAction_MustNotBeASingleRowCollection)
        End If
    End Function

    ''' <summary>
    ''' Gets the preconditions for this action. By default, this will return the
    ''' <see cref="GetBasePreConditions">base preconditions</see> for all collection
    ''' business object actions.
    ''' </summary>
    ''' <returns>The preconditions for this action</returns>
    Public Overrides Function GetPreConditions() As Collection
        Return GetBasePreConditions()
    End Function

    ''' <summary>
    ''' Appends preconditions to the default preconditions for actions on this
    ''' business object.
    ''' </summary>
    ''' <param name="lines">The preconditions to append</param>
    ''' <returns>The collection containing all of the preconditions; the default ones
    ''' for this action and the specific ones specified in the subclass's call.
    ''' </returns>
    Protected Function AppendPreConditions(ByVal ParamArray lines() As String) _
     As Collection
        Dim coll As Collection = GetBasePreConditions()
        For Each line As String In lines
            coll.Add(line)
        Next
        Return coll
    End Function

    ''' <summary>
    ''' Gets the collection stage specified in the collection name argument on this
    ''' action.
    ''' </summary>
    ''' <param name="proc">The process on which to get the corresponding collection
    ''' stage</param>
    ''' <param name="scopeStg">The stage which is requesting access to the collection
    ''' stage. This is used to ensure that it has the correct scope permissions to
    ''' view the collection stage.</param>
    ''' <returns>The collection stage which is referred to in the 'Collection Name'
    ''' input argument to this action.</returns>
    ''' <exception cref="EmptyArgumentException">If no collection name is specified
    ''' in this action's input arguments</exception>
    ''' <exception cref="NoSuchStageException">If no collection stage with the
    ''' specified name could be found in the process</exception>
    ''' <exception cref="OutOfScopeException">If the referenced collection stage is
    ''' out of scope of the given <paramref name="scopeStg">scope stage</paramref>
    ''' </exception>
    ''' <remarks>The exceptions thrown by this method all have messages which are
    ''' appropriate for displaying to the user</remarks>
    Protected Function GetCollectionStage(
     ByVal proc As clsProcess, ByVal scopeStg As clsProcessStage) _
     As clsCollectionStage

        Dim name As String = CollectionName
        If name = "" Then Throw New EmptyArgumentException(
         My.Resources.Resources.clsCollectionBusinessObjectAction_NoCollectionNameWasSpecified)

        Dim outOfScope As Boolean
        Dim stg As clsCollectionStage =
         proc.GetCollectionStage(name, scopeStg, outOfScope)

        If stg Is Nothing Then Throw New NoSuchStageException(
         My.Resources.Resources.clsCollectionBusinessObjectAction_FailedToFindAnyCollectionStageWithName0, name)

        If outOfScope Then Throw New OutOfScopeException(
         My.Resources.Resources.clsCollectionBusinessObjectAction_TheReferencedCollectionStage0CanNotBeAccessedBecauseItResidesOnADifferentPageAn, name)

        Return stg

    End Function

    ''' <summary>
    ''' Gets the collection value referred to in the inputs to this action.
    ''' </summary>
    ''' <param name="proc">The process on which to get the corresponding collection
    ''' </param>
    ''' <param name="scopeStg">The stage which is requesting access to the collection
    ''' stage. This is used to ensure that it has the correct scope permissions to
    ''' view the collection stage.</param>
    ''' <returns>The collection object referred to by the name specified in this
    ''' action's inputs.</returns>
    ''' <exception cref="EmptyArgumentException">If no collection name is specified
    ''' in this action's input arguments</exception>
    ''' <exception cref="NoSuchStageException">If no collection stage with the
    ''' specified name could be found in the process</exception>
    ''' <exception cref="OutOfScopeException">If the referenced collection stage is
    ''' out of scope of the given <paramref name="scopeStg">scope stage</paramref>
    ''' </exception>
    ''' <exception cref="EmptyCollectionException">If the stage was found to contain
    ''' no collection data</exception>
    ''' <exception cref="InvalidDataTypeException">If the collection is a
    ''' <see cref="clsCollection.SingleRow">single row</see> collection and the
    ''' <see cref="AllowsSingleRowCollections"/> property indicates that single row
    ''' collections are <em>not</em> allowed -or- if the value identified by the
    ''' 'Collection Name' argument is not a collection value.</exception>
    ''' <remarks>The exceptions thrown by this method all have messages which are
    ''' appropriate for displaying to the user</remarks>
    Protected Function GetCollection(
     proc As clsProcess, scopeStg As clsProcessStage) As clsCollection
        Return GetCollectionAndStage(proc, scopeStg).Item2
    End Function

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Gets the collection value referred to in the inputs to this action.
    ''' </summary>
    ''' <param name="proc">The process on which to get the corresponding collection
    ''' </param>
    ''' <param name="scopeStg">The stage which is requesting access to the collection
    ''' stage. This is used to ensure that it has the correct scope permissions to
    ''' view the collection stage.</param>
    ''' <returns>The collection stage and the actual collection referred to by the
    ''' name specified in this action's inputs.</returns>
    ''' <exception cref="EmptyArgumentException">If no collection name is specified
    ''' in this action's input arguments</exception>
    ''' <exception cref="NoSuchStageException">If no collection stage with the
    ''' specified name could be found in the process</exception>
    ''' <exception cref="OutOfScopeException">If the referenced collection stage is
    ''' out of scope of the given <paramref name="scopeStg">scope stage</paramref>
    ''' </exception>
    ''' <exception cref="EmptyCollectionException">If the target collection was found
    ''' to contain no collection data and <see cref="RequiresCurrentRow"/> is
    ''' True.</exception>
    ''' <exception cref="NoCurrentRowException">If the target collection was found to
    ''' have no current row set and <see cref="RequiresCurrentRow"/> is True.
    ''' </exception>
    ''' <exception cref="InvalidDataTypeException">If the collection is a
    ''' <see cref="clsCollection.SingleRow">single row</see> collection and the
    ''' <see cref="AllowsSingleRowCollections"/> property indicates that single row
    ''' collections are <em>not</em> allowed -or- if the value identified by the
    ''' 'Collection Name' argument is not a collection value.</exception>
    ''' <remarks>The exceptions thrown by this method all have messages which are
    ''' appropriate for displaying to the user</remarks>
    ''' -----------------------------------------------------------------------------
    Protected Function GetCollectionAndStage(
     proc As clsProcess,
     scopeStg As clsProcessStage) As (clsCollectionStage, clsCollection)

        Dim stg As clsCollectionStage = GetCollectionStage(proc, scopeStg)
        Dim path As (stage As String, fields As String) =
         clsCollection.SplitPath(CollectionName)

        Dim value = stg.Value
        If path.fields <> "" Then
            If value.DataType <> DataType.collection Then Throw New InvalidFormatException(
             My.Resources.Resources.clsCollectionBusinessObjectAction_TheStage0IsOfType1ButCollectionNotationIsUsed2,
             stg.Name, stg.DataType, CollectionName)

            value = value.Collection.GetField(path.fields)

        End If

        If value.DataType <> DataType.collection Then Throw New InvalidDataTypeException(
         My.Resources.Resources.clsCollectionBusinessObjectAction_0IsOnlyValidForCollections1IsOfType2,
         GetName(), CollectionName, value.DataTypeName)

        Dim coll = value.Collection

        ' I'm about 90% certain that this should never be the case,
        ' but just in case...
        If coll Is Nothing Then
            If RequiresCurrentRow Then Throw New EmptyCollectionException()
            Return (stg, Nothing)
        End If

        If Not AllowsSingleRowCollections AndAlso coll.SingleRow Then _
         Throw New InvalidDataTypeException(My.Resources.Resources.clsCollectionBusinessObjectAction_CannotActOnASingleRowCollection)

        If RequiresCurrentRow And coll.CurrentRowIndex < 0 Then Throw New NoCurrentRowException(
         My.Resources.Resources.clsCollectionBusinessObjectAction_TheCollectionFoundAt0HasNoCurrentRow, CollectionName)

        Return (stg, coll)
    End Function

End Class
