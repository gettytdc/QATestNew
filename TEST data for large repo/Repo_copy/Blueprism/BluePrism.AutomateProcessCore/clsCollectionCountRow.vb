Imports BluePrism.AutomateProcessCore.My.Resources

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsCollectionCountRow
''' 
''' <summary>
''' An internal business object action to Count the number of rows in a
''' collection
''' </summary>
Public Class clsCollectionCountRow
    Inherits clsCollectionBusinessObjectAction

    ''' <summary>
    ''' Initializes a new instance of the clsCollectionCountRow class.
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)
        SetName(NameOf(IboResources.clsCollectionActions_Action_CountRows))
        AddParameter(NameOf(IboResources.clsCollectionActions_Params_Count), DataType.number, ParamDirection.Out,
   IboResources.clsCollectionCountRow_TheNumberOfRowsCountedInTheCollection)
    End Sub

    ''' <summary>
    ''' Hardcoded function that returns the endpoint text for this collection.
    ''' </summary>
    ''' <returns>The endpoint for this action</returns>
    Public Overrides Function GetEndpoint() As String
        Return IboResources.clsCollectionCountRow_TheOutputDataItemWillContainTheRowCount
    End Function

    ''' <summary>
    ''' This performs the count operation
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="session">The session under which the call is being made,
    ''' or Guid.Empty if unknown.</param>
    ''' <param name="scopeStg">The stage used to resolve scope within the business
    ''' object action. This is needed to check permission to modify the collection of
    ''' interest, and to resolve the correct collection by name when more than one
    ''' collection shares this name. Must not be null.</param>
    ''' <param name="sErr">An errormessage if unsuccessful</param>
    ''' <returns>True if successful</returns>
    Public Overrides Function [Do](ByVal process As clsProcess,
     ByVal session As clsSession,
     ByVal scopeStg As clsProcessStage,
     ByRef sErr As String) As Boolean

        Dim count As Integer
        Try
            count = GetCollection(process, scopeStg).Count
        Catch ece As EmptyCollectionException
            'CG: I would have thought we should give an error when the collection is null. However,
            'this is for backwards compatibility...
            count = 0
        Catch ex As Exception
            Return SendError(sErr, ex.Message)
        End Try
        AddOutput("Count", count)
        Return True

    End Function


End Class
