Imports BluePrism.AutomateProcessCore.My.Resources
''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsCollectionCopyRows
''' 
''' <summary>
''' An internal business object action to copy a subset of the rows of a collection
''' </summary>
Public Class clsCollectionCopyRows
    Inherits clsCollectionBusinessObjectAction

    ''' <summary>
    ''' A class encapsulating the parameter names used in encryption actions
    ''' </summary>
    Protected Class Params
        Public Shared StartRow As String = NameOf(IboResources.clsCollectionActions_Params_StartRow)
        Public Shared EndRow As String = NameOf(IboResources.clsCollectionActions_Params_EndRow)
        Public Shared Result As String = NameOf(IboResources.clsCollectionActions_Params_Result)
    End Class

    ''' <summary>
    ''' Initializes a new instance of the clsCollectionCopyRows class.
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)
        SetName(NameOf(IboResources.clsCollectionActions_Action_CopyRows))

        AddParameter(Params.StartRow, DataType.number, ParamDirection.In,
         IboResources.clsCollectionCopyRows_TheIndexOfTheFirstRowToCopyTheIndexIsFrom0ToN1WhereNIsTheTotalNumberOfRowsInThe)
        AddParameter(Params.EndRow, DataType.number, ParamDirection.In,
         IboResources.clsCollectionCopyRows_TheIndexOfTheLastRowToCopyTheIndexIsFrom0ToN1WhereNIsTheTotalNumberOfRowsInTheC)
        AddParameter(Params.Result, DataType.collection, ParamDirection.Out,
         IboResources.clsCollectionCopyRows_ACollectionWhichContainsTheRowsCopiedFromTheInputCollection)

    End Sub

    ''' <summary>
    ''' Hardcoded function that returns the endpoint text for this collection.
    ''' </summary>
    Public Overrides Function GetEndpoint() As String
        Return IboResources.clsCollectionCopyRows_TheCollectionIsUnchanged
    End Function

    ''' <summary>
    ''' This performs the operation
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="session">The session under which the call is being made,
    ''' or Nothing if unknown.</param>
    ''' <param name="scopeStg">The stage used to resolve scope within the business
    ''' object action. This is needed to check permission to modify the collection of interest,
    ''' and to resolve the correct collection by name when more than one collection
    ''' shares this name. Must not be null.</param>
    ''' <param name="sErr">An error message if unsuccessful</param>
    ''' <returns>True if successful</returns>
    Public Overrides Function [Do](ByVal process As clsProcess,
     ByVal session As clsSession,
     ByVal scopeStg As clsProcessStage,
     ByRef sErr As String) As Boolean

        ' Use MinValue as a 'not likely to be set by accident by the user' indicator
        Const NotSet As Integer = Integer.MinValue

        ' Validate the row indices first
        Dim startRow As Integer = NotSet
        Dim endRow As Integer = NotSet
        For Each arg As clsArgument In Inputs
            If arg.Value.IsNull Then Continue For
            Select Case arg.Name
                Case "Start Row" : startRow = CInt(arg.Value)
                Case "End Row" : endRow = CInt(arg.Value)
            End Select
        Next

        If startRow = NotSet OrElse endRow = NotSet Then Return SendError(sErr,
         IboResources.clsCollectionCopyRows_MissingParameterS012,
         IIf(startRow = NotSet, IboResources.clsCollectionActions_Params_StartRow, ""),
         IIf(startRow = NotSet AndAlso endRow = NotSet, IboResources.clsCollectionCopyRows_Comma, ""),
         IIf(endRow = NotSet, IboResources.clsCollectionActions_Params_EndRow, "")
        )

        If endRow < startRow Then Return SendError(sErr,
         IboResources.clsCollectionCopyRows_EndRowMustBeGreaterThanOrEqualToStartRow)

        ' Now try and get the collection; return an error if we can't for some reason
        Dim col As clsCollection = Nothing
        Try
            col = GetCollection(process, scopeStg)
        Catch ex As Exception
            Return SendError(sErr, ex.Message)
        End Try

        If startRow < 0 OrElse startRow > col.Count - 1 Then Return SendError(sErr,
         IboResources.clsCollectionCopyRows_StartRowOutOfRange)

        If endRow < 0 OrElse endRow > col.Count - 1 Then Return SendError(sErr,
         IboResources.clsCollectionCopyRows_EndRowOutOfRange)

        Dim c As New clsCollection()
        For r As Integer = startRow To endRow
            c.Add(col.Row(r))
        Next

        AddOutput("Result", c)
        Return True

    End Function
End Class
