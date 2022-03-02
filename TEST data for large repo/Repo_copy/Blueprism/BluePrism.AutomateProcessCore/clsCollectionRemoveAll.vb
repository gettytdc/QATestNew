Imports BluePrism.AutomateProcessCore.My.Resources

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsCollectionRemoveRow
''' 
''' <summary>
''' An internal business object action to Remove rows from a collection
''' </summary>
Public Class clsCollectionRemoveAll
    Inherits clsCollectionBusinessObjectAction

    ''' <summary>
    ''' Initializes a new instance of the clsCollectionRemoveAll class.
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)
        SetName(NameOf(IboResources.clsCollectionActions_Action_RemoveAllRows))
    End Sub

    ''' <summary>
    ''' Gets whether single row collections are allowed or not by this action.
    ''' You cannot clear a single row collection, hence false in this case.
    ''' </summary>
    Protected Overrides ReadOnly Property AllowsSingleRowCollections() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Hardcoded function that returns the endpoint text for this collection.
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function GetEndpoint() As String
        Return IboResources.clsCollectionRemoveAll_AllRowsWillBeDeletedAnyAttemptToAccessDataInTheCollectionWillResultInAnErrorUnl
    End Function

    ''' <summary>
    ''' This performs the remove operation
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="session">The session ID under which the call is being made,
    ''' or Nothing if unknown.</param>
    ''' <param name="scopeStg">The stage used to resolve scope within the business
    ''' object action. This is needed to check permission to modify the collection of interest,
    ''' and to resolve the correct collection by name when more than one collection
    ''' shares this name. Must not be null.</param>
    ''' <param name="sErr">An errormessage if unsuccessful</param>
    ''' <returns>True if successful</returns>
    Public Overrides Function [Do](ByVal process As clsProcess, _
     ByVal session As clsSession, _
     ByVal scopeStg As clsProcessStage, _
     ByRef sErr As String) As Boolean
        Try
            GetCollection(process, scopeStg).Clear()
            Return True
        Catch ex As Exception
            Return SendError(sErr, ex.Message)
        End Try
    End Function

End Class
