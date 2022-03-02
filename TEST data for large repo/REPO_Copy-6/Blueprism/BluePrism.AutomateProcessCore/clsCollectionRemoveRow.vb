Imports BluePrism.AutomateProcessCore.My.Resources

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsCollectionRemoveRow
''' 
''' <summary>
''' An internal business object action to Remove rows from a collection
''' </summary>
Public Class clsCollectionRemoveRow
    Inherits clsCollectionBusinessObjectAction

    ''' <summary>
    ''' Initializes a new instance of the clsCollectionRemoveRow class.
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)
        SetName(NameOf(IboResources.clsCollectionActions_Action_RemoveRow))
    End Sub

    ''' <summary>
    ''' Gets whether single row collections are allowed or not by this action.
    ''' You cannot remove a row in a single row collection, hence false in this case.
    ''' </summary>
    Protected Overrides ReadOnly Property AllowsSingleRowCollections() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this action requires there to be a current row set in the
    ''' target collection. This further implies that the target collection must have
    ''' some data.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Protected Overrides ReadOnly Property RequiresCurrentRow As Boolean
        Get
            Return True
        End Get
    End Property

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Hardcoded function that returns the endpoint text for this collection.
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function GetEndpoint() As String
        Return _
         IboResources.clsCollectionRemoveRow_TheCurrentRowWillBeDeletedTheCurrentRowWillNoLongerExistSoAnyAttemptToAccessThe
    End Function

    ''' <summary>
    ''' Hardcoded function that returns the Preconditions
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function GetPreConditions() As Collection
        Return AppendPreConditions(
         IboResources.clsCollectionRemoveRow_MustHaveACurrentRowEitherByAlreadyBeingInALoopOrBySomeOtherMeansEgAddingARowMan
        )
    End Function

    ''' <summary>
    ''' This performs the remove operation
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="session">The session under which the call is being made,
    ''' or Guid.Empty if unknown.</param>
    ''' <param name="scopeStg">The stage used to resolve scope within the business
    ''' object action. This is needed to check permission to modify the collection of
    ''' interest, and to resolve the correct collection by name when more than one
    ''' collection shares this name. Must not be null.</param>
    ''' <param name="sErr">An error message if unsuccessful</param>
    ''' <returns>True if successful</returns>
    Public Overrides Function [Do](ByVal process As clsProcess,
     ByVal session As clsSession,
     ByVal scopeStg As clsProcessStage,
     ByRef sErr As String) As Boolean

        Try
            ' If we succeed, say so
            If GetCollection(process, scopeStg).DeleteCurrentRow() Then Return True

            ' Otherwise, return an error condition
            Return SendError(sErr, IboResources.clsCollectionRemoveRow_TheDeleteFailed)

        Catch ex As Exception
            Return SendError(sErr, ex.Message)

        End Try

    End Function

End Class
