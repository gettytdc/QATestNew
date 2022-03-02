Imports BluePrism.AutomateProcessCore.My.Resources

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsCollectionCountColumn
''' 
''' <summary>
''' An internal business object action to Count the number of columns in a
''' collection
''' </summary>
Public Class clsCollectionCountColumn
    Inherits clsCollectionBusinessObjectAction

    ''' <summary>
    ''' Initializes a new instance of the clsCollectionCountColumn class.
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)
        SetName(NameOf(IboResources.clsCollectionActions_Action_CountColumns))
        AddParameter(NameOf(IboResources.clsCollectionActions_Params_Count), DataType.number, ParamDirection.Out,
   IboResources.clsCollectionCountColumn_TheNumberOfColumnsCountedInTheCollection)
    End Sub

    ''' <summary>
    ''' Hardcoded function that returns the endpoint text for this collection.
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function GetEndpoint() As String
        Return IboResources.clsCollectionCountColumn_TheOutputDataItemWillContainTheColumnCount
    End Function

    ''' <summary>
    ''' This performs the count operation
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="session">The session under which the call is being made,
    ''' or Nothing if unknown.</param>
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
        Try
            AddOutput("Count", GetCollection(process, scopeStg).Definition.Count)
            Return True
        Catch ex As Exception
            Return SendError(sErr, ex.Message)
        End Try
    End Function

End Class
