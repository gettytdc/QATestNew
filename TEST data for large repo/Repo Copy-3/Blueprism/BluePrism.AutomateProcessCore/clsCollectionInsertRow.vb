Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore.My.Resources

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsCollectionInsertRow
''' 
''' <summary>
''' An internal business object action to insert a row into the collection.
''' </summary>
Public Class clsCollectionInsertRow
    Inherits clsCollectionBusinessObjectAction

    ''' <summary>
    ''' Initializes a new instance of the clsCollectionInsertRow class.
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)
        SetName(NameOf(IboResources.clsCollectionActions_Action_AddRow))
    End Sub

    ''' <summary>
    ''' Gets whether single row collections are allowed or not by this action.
    ''' You cannot add a row to a single row collection, hence false in this case.
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
        Return _
         IboResources.clsCollectionInsertRow_TheCollectionWillHaveAnAdditionalEmptyRowAndThisWillBeTheCurrentRow
    End Function

    ''' <summary>
    ''' This performs the insert operation
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="session">The session under which the call is being made,
    ''' or Nothing if unknown.</param>
    ''' <param name="scopeStg">The stage used to resolve scope within the business
    ''' object action. This is needed to check permission to modify the collection of
    ''' interest, and to resolve the correct collection by name when more than one
    ''' collection shares this name. Must not be null.</param>
    ''' <param name="sErr">An error message if unsuccessful</param>
    ''' <returns>True if successful</returns>
    Public Overrides Function [Do](
     ByVal process As clsProcess,
     ByVal session As clsSession,
     ByVal scopeStg As clsProcessStage,
     ByRef sErr As String) As Boolean

        Try
            Dim collSchema = GetCollectionAndStage(process, scopeStg)
            Dim stg As clsCollectionStage = collSchema.Item1
            Dim col As clsCollection = collSchema.Item2

            If col Is Nothing Then
                If stg.Definition Is Nothing Then Return SendError(sErr,
                 IboResources.clsCollectionInsertRow_CannotAddARowToACollectionWithNoFieldsDefined)

                ' Adding a row to a null collection stage that has field definitions.
                ' Create an empty collection with the appropriate definiton first...
                col = New clsCollection()
                col.Definition.SetFrom(stg.Definition)
                stg.SetValue(col)
            End If

            Dim i As Integer = col.Add()
            col.SetCurrentRow(i)
            Return True

        Catch ex As Exception
            Return SendError(sErr, ex.Message)

        End Try

    End Function


End Class
