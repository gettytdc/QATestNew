Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateProcessCore.Stages

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsExpressionInfo
''' 
''' <summary>
''' This class contains information about an expression, which is
''' gathered during the process of evaluating/validating it. An
''' instance of this class is returned by clsProcess.EvaluateExpression.
''' </summary>
Public Class clsExpressionInfo

    ''' <summary>
    ''' A collection containing the stage ID (guid) of each data
    ''' item referenced by the expression.
    ''' </summary>
    Public ReadOnly Property DataItems() As List(Of Guid)
        Get
            Return mDataItems
        End Get
    End Property
    Private mDataItems As List(Of Guid)

    ''' <summary>
    ''' Private member to store public property CollectionFields()
    ''' </summary>
    Private mDefinedCollFields As IDictionary(Of Guid, clsSet(Of String))
    ''' <summary>
    ''' A list of the defined collection fields used in the expression for each collection
    ''' data item. Defined means that these fields are defined at design-time in the owning collection.
    ''' Keyed by collection stage ID, values are lists of strings naming
    ''' the fields used for each collection. Any collection listed here will also
    ''' be present in the DataItems list, and vice versa.
    ''' </summary>
    ''' <value></value>
    Public ReadOnly Property DefinedCollectionFields() As IDictionary(Of Guid, clsSet(Of String))
        Get
            Return mDefinedCollFields
        End Get
    End Property


    ''' <summary>
    ''' Private member to store public property CollectionFields()
    ''' </summary>
    Private mUndefinedCollFields As IDictionary(Of Guid, clsSet(Of String))
    ''' <summary>
    ''' A list of the undefined collection fields used in the expression for each collection
    ''' data item. Undefined means that these fields are NOT defined at design-time in the owning collection.
    ''' Keyed by collection stage ID, values are lists of strings naming
    ''' the fields used for each collection. Any collection listed here will also
    ''' be present in the DataItems list, and vice versa.
    ''' </summary>
    Public ReadOnly Property UndefinedCollectionFields() As IDictionary(Of Guid, clsSet(Of String))
        Get
            Return mUndefinedCollFields
        End Get
    End Property

    ''' <summary>
    ''' Gets the field names referred to in this expression which are associated with
    ''' the collection stage identified by the given ID.
    ''' </summary>
    ''' <param name="stgId">The ID of the collection stage for which the field names
    ''' are required</param>
    ''' <returns>The field names, both defined and undefined, within the specified
    ''' collection which appear in this expression.</returns>
    Public Function GetFieldNames(ByVal stgId As Guid) As ICollection(Of String)
        Dim fldNames As New clsSet(Of String)
        Dim names As clsSet(Of String) = Nothing
        If mDefinedCollFields.TryGetValue(stgId, names) Then fldNames.Union(names)
        If mUndefinedCollFields.TryGetValue(stgId, names) Then fldNames.Union(names)
        Return fldNames
    End Function

    ''' <summary>
    ''' Private member to store public property FunctionNames()
    ''' </summary>
    Private mFunctions As Dictionary(Of String, clsFunction)
    ''' <summary>
    ''' A list of the functions used in the expression, keyed by name.
    ''' </summary>
    ''' <value></value>
    Public ReadOnly Property Functions() As Dictionary(Of String, clsFunction)
        Get
            Return mFunctions
        End Get
    End Property

    ''' <summary>
    ''' Constructor
    ''' </summary>
    Public Sub New()
        mDataItems = New List(Of Guid)
        mFunctions = New Dictionary(Of String, clsFunction)
        mDefinedCollFields = New clsGeneratorDictionary(Of Guid, clsSet(Of String))
        mUndefinedCollFields = New clsGeneratorDictionary(Of Guid, clsSet(Of String))
    End Sub

    ''' <summary>
    ''' Add a data item reference. No action is taken if the reference
    ''' has already been recorded.
    ''' </summary>
    ''' <param name="gStageID">The ID of the data item used in the expression
    ''' of interest.</param>
    ''' <remarks>
    ''' To be called during expression evaluation only.
    ''' </remarks>
    Public Sub AddDataItem(ByVal gStageID As Guid)
        'Check if we already have this ID in the collection...
        Dim gID As Guid
        For Each gID In mDataItems
            'Already got it, so exit...
            If gID.Equals(gStageID) Then Exit Sub
        Next
        'Add to internal collection...
        mDataItems.Add(gStageID)
    End Sub

    ''' <summary>
    ''' Adds a function. No action is taken if another instance of the
    ''' same function (ie sharing the same name) has been added.
    ''' </summary>
    ''' <param name="fn">The function used in the expression of interest.</param>
    ''' <remarks>
    ''' To be called during expression evaluation only.
    ''' </remarks>
    Public Sub AddFunction(ByVal fn As clsFunction)
        mFunctions(fn.Name) = fn
    End Sub

    ''' <summary>
    ''' Adds a collection field against the ID of the owning collection.
    ''' The collection itself is added automatically first, if needs be.
    ''' If the collection field is already listed then no action is taken.
    ''' </summary>
    ''' <param name="stg">The collection owning the name field.</param>
    ''' <param name="fldName">The name of the field of interest. This may be
    ''' a "dynamic" field - i.e. one which does not exist in the collection.</param>
    Public Sub AddCollectionField(ByVal stg As clsCollectionStage, ByVal fldName As String)
        AddDataItem(stg.Id)

        Dim defn As clsCollectionInfo = stg.Definition
        If defn IsNot Nothing AndAlso defn.Contains(fldName, True) Then
            mDefinedCollFields(stg.Id).Add(fldName)
        Else
            mUndefinedCollFields(stg.Id).Add(fldName)
        End If

    End Sub

End Class

