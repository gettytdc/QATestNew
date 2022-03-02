Imports System.Data.SqlTypes
Imports System.Data.SqlClient

Imports BluePrism.BPCoreLib.Licensing
Imports BluePrism.BPCoreLib.Collections

Imports BluePrism.AutomateProcessCore
Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore.My.Resources
Imports BluePrism.AutomateProcessCore.Stages

''' Project  : AutomateAppCore
''' Class    : clsWorkQueuesBusinessObject
'''
''' <summary>
''' This class represents the Work Queues Internal Business Object
''' </summary>
Public Class clsWorkQueuesBusinessObject
    Inherits clsInternalBusinessObject

    ''' <summary>
    ''' A list of items that are currently locked by this instance of the
    ''' business object.
    ''' </summary>
    Private mLockedItems As ICollection(Of Guid)

    ''' <summary>
    ''' The new constructor just creates the Internal Business Object Actions
    ''' </summary>
    ''' <param name="process">A reference to the process calling the object</param>
    ''' <param name="session">The session the object is running under</param>
    Public Sub New(ByVal process As clsProcess, ByVal session As clsSession)
        MyBase.New(process, session,
          "Blueprism.Automate.clsWorkQueuesActions",
          IboResources.clsWorkQueuesActions_WorkQueues)

        Narrative =
         IboResources.clsWorkQueuesActions_ThisInternalBusinessObjectProvidesTheAbilityForProcessesToInteractWithTheWorkQu

        AddAction(New GetQueueNames(Me))
        AddAction(New AddToQueueAction(Me))
        AddAction(New SetDataAction(Me))
        AddAction(New DeferAction(Me))
        AddAction(New DeleteProcessedItemsAction(Me))
        AddAction(New GetPendingItemsAction(Me))
        AddAction(New GetLockedItemsAction(Me))
        AddAction(New UnlockItemAction(Me))
        AddAction(New GetCompletedItemsAction(Me))
        AddAction(New GetExceptionItemsAction(Me))
        AddAction(New GetNextItemAction(Me))
        AddAction(New MarkCompletedAction(Me))
        AddAction(New MarkExceptionAction(Me))
        AddAction(New UpdateStatusAction(Me))
        AddAction(New DeleteItemAction(Me))
        AddAction(New GetItemDataAction(Me))
        AddAction(New IsItemInQueueAction(Me))
        AddAction(New TagItemAction(Me))
        AddAction(New UntagItemAction(Me))
        AddAction(New GetReportDataAction(Me))
        AddAction(New GetTransactionDataAction(Me))
        AddAction(New SetPriorityAction(Me))
        AddAction(New CopyToQueueAction(Me))
        AddAction(New GetItemByIdAction(Me))
    End Sub

    ''' <summary>
    ''' Initialise the business object, as per API documentation.
    ''' </summary>
    ''' <returns>A clsProcessStage.Result</returns>
    Public Overrides Function DoInit() As StageResult
        mLockedItems = New clsSet(Of Guid)
        Return StageResult.OK
    End Function

    Public Overrides Function DoCleanUp(
     ByVal sessionIdentifier As SessionIdentifier) As StageResult

        'If any items have been left unlocked, mark them as an exception. When
        'we mark an exception in this way, we allow it to be retried up to the
        'maximum number of attempts defined for the queue.

        For Each id As Guid In mLockedItems
            Try
                gSv.WorkQueueMarkException(sessionId:=sessionIdentifier.Id, itemid:=id,
                                           reason:=IboResources.clsWorkQueuesActions_AutomaticallySetExceptionAtCleanUp, retry:=True, keepLocked:=False, retried:=False, queueID:=Nothing, manualChange:=False)
            Catch ' Ignore any errors - if it can't be done, it can't be done.
            End Try
        Next

        Return StageResult.OK
    End Function

    Public Overrides Function CheckLicense() As Boolean
        Return True
    End Function

    ''' <summary>
    ''' Class encapsulating a work queue action which just allows access to the
    ''' locked items collection of the owning business object to its subclasses.
    ''' </summary>
    Private MustInherit Class WorkQueueAction : Inherits clsInternalBusinessObjectAction

        ''' <summary>
        ''' Class to hold the parameter names as constants.
        ''' </summary>
        Protected Class Params

            Public Shared QueueNames As String = NameOf(IboResources.clsWorkQueuesActions_Params_QueueNames)
            Public Shared QueueName As String = NameOf(IboResources.clsWorkQueuesActions_Params_QueueName)
            Public Shared Data As String = NameOf(IboResources.clsWorkQueuesActions_Params_Data)
            Public Shared DeferUntil As String = NameOf(IboResources.clsWorkQueuesActions_Params_DeferUntil)
            Public Shared Priority As String = NameOf(IboResources.clsWorkQueuesActions_Params_Priority)
            Public Shared Tags As String = NameOf(IboResources.clsWorkQueuesActions_Params_Tags)
            Public Shared Status As String = NameOf(IboResources.clsWorkQueuesActions_Params_Status)
            Public Shared BatchSize As String = NameOf(IboResources.clsWorkQueuesActions_Params_BatchSize)
            Public Shared ItemIDs As String = NameOf(IboResources.clsWorkQueuesActions_Params_ItemIDs)
            Public Shared ItemID As String = NameOf(IboResources.clsWorkQueuesActions_Params_ItemID)
            Public Shared NewItemID As String = NameOf(IboResources.clsWorkQueuesActions_Params_NewItemID)
            Public Shared Tag As String = NameOf(IboResources.clsWorkQueuesActions_Params_Tag)
            Public Shared Until As String = NameOf(IboResources.clsWorkQueuesActions_Params_Until)
            Public Shared DateThreshold As String = NameOf(IboResources.clsWorkQueuesActions_Params_DateThreshold)
            Public Shared DeleteFromAllQueues As String = NameOf(IboResources.clsWorkQueuesActions_Params_DeleteFromAllQueues)
            Public Shared DeletionCount As String = NameOf(IboResources.clsWorkQueuesActions_Params_DeletionCount)
            Public Shared KeyFilter As String = NameOf(IboResources.clsWorkQueuesActions_Params_KeyFilter)
            Public Shared TagFilter As String = NameOf(IboResources.clsWorkQueuesActions_Params_TagFilter)
            Public Shared Attempts As String = NameOf(IboResources.clsWorkQueuesActions_Params_Attempts)
            Public Shared Unlocked As String = NameOf(IboResources.clsWorkQueuesActions_Params_Unlocked)
            Public Shared Maximum As String = NameOf(IboResources.clsWorkQueuesActions_Params_Maximum)
            Public Shared Skip As String = NameOf(IboResources.clsWorkQueuesActions_Params_Skip)
            Public Shared PendingItems As String = NameOf(IboResources.clsWorkQueuesActions_Params_PendingItems)
            Public Shared KeepLocked As String = NameOf(IboResources.clsWorkQueuesActions_Params_KeepLocked)
            Public Shared Retry As String = NameOf(IboResources.clsWorkQueuesActions_Params_Retry)
            Public Shared LockedItems As String = NameOf(IboResources.clsWorkQueuesActions_Params_LockedItems)
            Public Shared StartDate As String = NameOf(IboResources.clsWorkQueuesActions_Params_StartDate)
            Public Shared EndDate As String = NameOf(IboResources.clsWorkQueuesActions_Params_EndDate)
            Public Shared MaximumRows As String = NameOf(IboResources.clsWorkQueuesActions_Params_MaximumRows)
            Public Shared CompletedItems As String = NameOf(IboResources.clsWorkQueuesActions_Params_CompletedItems)
            Public Shared ExceptionItems As String = NameOf(IboResources.clsWorkQueuesActions_Params_ExceptionItems)
            Public Shared Key As String = NameOf(IboResources.clsWorkQueuesActions_Params_Key)
            Public Shared Worktime As String = NameOf(IboResources.clsWorkQueuesActions_Params_Worktime)
            Public Shared AttemptWorktime As String = NameOf(IboResources.clsWorkQueuesActions_Params_AttemptWorktime)
            Public Shared LoadedDateTime As String = NameOf(IboResources.clsWorkQueuesActions_Params_LoadedDateTime)
            Public Shared DeferredDateTime As String = NameOf(IboResources.clsWorkQueuesActions_Params_DeferredDateTime)
            Public Shared CompletedDateTime As String = NameOf(IboResources.clsWorkQueuesActions_Params_CompletedDateTime)
            Public Shared ExceptionDateTime As String = NameOf(IboResources.clsWorkQueuesActions_Params_ExceptionDateTime)
            Public Shared ExceptionReason As String = NameOf(IboResources.clsWorkQueuesActions_Params_ExceptionReason)
            Public Shared IncludePending As String = NameOf(IboResources.clsWorkQueuesActions_Params_IncludePending)
            Public Shared IncludeDeferred As String = NameOf(IboResources.clsWorkQueuesActions_Params_IncludeDeferred)
            Public Shared IncludeCompleted As String = NameOf(IboResources.clsWorkQueuesActions_Params_IncludeCompleted)
            Public Shared IncludeTerminated As String = NameOf(IboResources.clsWorkQueuesActions_Params_IncludeTerminated)
            Public Shared Result As String = NameOf(IboResources.clsWorkQueuesActions_Params_Result)
            Public Shared ExcludedQueueNames As String = NameOf(IboResources.clsWorkQueuesActions_Params_ExcludedQueueNames)
            Public Shared StartDateTime As String = NameOf(IboResources.clsWorkQueuesActions_Params_StartDateTime)
            Public Shared EndDateTime As String = NameOf(IboResources.clsWorkQueuesActions_Params_EndDateTime)
            Public Shared CountItemCreations As String = NameOf(IboResources.clsWorkQueuesActions_Params_CountItemCreations)
            Public Shared CountItemLocks As String = NameOf(IboResources.clsWorkQueuesActions_Params_CountItemLocks)
            Public Shared CountItemDeferrals As String = NameOf(IboResources.clsWorkQueuesActions_Params_CountItemDeferrals)
            Public Shared CountItemCompletions As String = NameOf(IboResources.clsWorkQueuesActions_Params_CountItemCompletions)
            Public Shared CountItemExceptionsWithAutomaticRetry As String = NameOf(IboResources.clsWorkQueuesActions_Params_CountItemExceptionsWithAutomaticRetry)
            Public Shared CountItemExceptionsWithoutAutomaticRetry As String = NameOf(IboResources.clsWorkQueuesActions_Params_CountItemExceptionsWithoutAutomaticRetry)
            Public Shared CountItemDeletions As String = NameOf(IboResources.clsWorkQueuesActions_Params_CountItemDeletions)
            Public Shared CountItemForcedRetries As String = NameOf(IboResources.clsWorkQueuesActions_Params_CountItemForcedRetries)
            Public Shared IncludedQueues As String = NameOf(IboResources.clsWorkQueuesActions_Params_IncludedQueues)
            Public Shared Created As String = NameOf(IboResources.clsWorkQueuesActions_Params_Created)
            Public Shared Locked As String = NameOf(IboResources.clsWorkQueuesActions_Params_Locked)
            Public Shared Deferred As String = NameOf(IboResources.clsWorkQueuesActions_Params_Deferred)
            Public Shared Completed As String = NameOf(IboResources.clsWorkQueuesActions_Params_Completed)
            Public Shared ExceptionsWithRetry As String = NameOf(IboResources.clsWorkQueuesActions_Params_ExceptionsWithRetry)
            Public Shared ExceptionsWithNoRetry As String = NameOf(IboResources.clsWorkQueuesActions_Params_ExceptionsWithNoRetry)
            Public Shared Deletions As String = NameOf(IboResources.clsWorkQueuesActions_Params_Deletions)
            Public Shared ForceRetries As String = NameOf(IboResources.clsWorkQueuesActions_Params_ForceRetries)
            Public Shared TotalCount As String = NameOf(IboResources.clsWorkQueuesActions_Params_TotalCount)
            Public Shared FinishedStartDate As String = NameOf(IboResources.clsWorkQueuesActions_Params_FinishedStartDate)
            Public Shared FinishedEndDate As String = NameOf(IboResources.clsWorkQueuesActions_Params_FinishedEndDate)
            Public Shared LoadedStartDate As String = NameOf(IboResources.clsWorkQueuesActions_Params_LoadedStartDate)
            Public Shared LoadedEndDate As String = NameOf(IboResources.clsWorkQueuesActions_Params_LoadedEndDate)
            Public Shared ResourceName As String = NameOf(IboResources.clsWorkQueueActions_Params_ResourceName)
            Public Shared ResourceNames As String = NameOf(IboResources.clsWorkQueuesActions_Params_ResourceNames)
            Public Shared IncludeUnworkedItems As String = NameOf(IboResources.clsWorkQueuesActions_Params_IncludeUnworkedItems)
            Public Shared IncludeDeferredItems As String = NameOf(IboResources.clsWorkQueuesActions_Params_IncludeDeferredItems)
            Public Shared IncludeCompletedItems As String = NameOf(IboResources.clsWorkQueuesActions_Params_IncludeCompletedItems)
            Public Shared IncludeExceptionItems As String = NameOf(IboResources.clsWorkQueuesActions_Params_IncludeExceptionItems)
            Public Shared TreatEachAttemptSeparately As String = NameOf(IboResources.clsWorkQueuesActions_Params_TreatEachAttemptSeparately)
            Public Shared ItemCount As String = NameOf(IboResources.clsWorkQueuesActions_Params_ItemCount)
            Public Shared TimeTotal As String = NameOf(IboResources.clsWorkQueuesActions_Params_TimeTotal)
            Public Shared LeastTime As String = NameOf(IboResources.clsWorkQueuesActions_Params_LeastTime)
            Public Shared MostTime As String = NameOf(IboResources.clsWorkQueuesActions_Params_MostTime)
            Public Shared MedianTime As String = NameOf(IboResources.clsWorkQueuesActions_Params_MedianTime)
            Public Shared MeanTime As String = NameOf(IboResources.clsWorkQueuesActions_Params_MeanTime)
            Public Shared TagMask As String = NameOf(IboResources.clsWorkQueuesActions_Params_TagMask)

        End Class
        Public Shared Function _T(ByVal param As String) As String
            Return IboResources.ResourceManager.GetString(param, New Globalization.CultureInfo("en"))
        End Function

        ''' <summary>
        ''' The collection of IDs representing the items locked by the owning
        ''' business object.
        ''' </summary>
        Protected ReadOnly Property LockedItems() As ICollection(Of Guid)
            Get
                Return DirectCast(mParent, clsWorkQueuesBusinessObject).mLockedItems
            End Get
        End Property

        ''' <summary>
        ''' Creates a new work queue action based on the given business object.
        ''' </summary>
        ''' <param name="obj">The business object which acts as the owner of this
        ''' action.</param>
        Public Sub New(ByVal obj As clsWorkQueuesBusinessObject)
            MyBase.New(obj)
        End Sub

        ''' <summary>
        ''' Gets the Item ID from the inputs under the name 'Item ID', or
        ''' <see cref="Guid.Empty"/> if no item ID was found, or if the value under
        ''' that name could not be parsed into a GUID.
        ''' </summary>
        Protected ReadOnly Property ItemId() As Guid
            Get
                Try
                    Return CType(Inputs.GetValue(_T(Params.ItemID)), Guid)
                Catch
                End Try
                Return Nothing
            End Get
        End Property

        ''' <summary>
        ''' Performs this action.
        ''' </summary>
        ''' <param name="process"></param>
        ''' <param name="session"></param>
        ''' <param name="scopeStage"></param>
        ''' <param name="sErr"></param>
        ''' <returns></returns>
        Public MustOverride Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopeStage As clsProcessStage,
         ByRef sErr As String) As Boolean

        ''' <summary>
        ''' Gets the endpoint that this action ends at after a successful execution.
        ''' </summary>
        ''' <returns>The text detailing the endpoint of this action.</returns>
        Public MustOverride Overrides Function GetEndpoint() As String

        ''' <summary>
        ''' Gets the collection of preconditions for this action.
        ''' </summary>
        ''' <returns>This implementation returns an empty set of preconditions
        ''' indicating that there are none; Subclasses should override this if they
        ''' possess preconditions.</returns>
        Public Overrides Function GetPreConditions() As Collection
            Return EmptyPreCondition()
        End Function

    End Class

    ''' <summary>
    ''' Action used to retrieve the the current work queue names within the system.
    ''' </summary>
    Private Class GetQueueNames : Inherits WorkQueueAction

        ''' <summary>
        ''' Creates a new action for dealing with getting pending items.
        ''' </summary>
        ''' <param name="bo">The business object that this action is a part of.</param>
        Public Sub New(ByVal bo As clsWorkQueuesBusinessObject)
            MyBase.New(bo)

            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_GetQueueNames))
            SetNarrative(IboResources.clsWorkQueuesActions_GetQueueNames_RetrievesTheNamesOfAllWorkQueuesInTheCurrentEnvironment)

            ' Output params
            Dim p As clsProcessParameter =
             AddParameter(Params.QueueNames, DataType.collection, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetQueueNames_TheNamesOfTheQueuesFoundWithinTheCurrentEnvironmentContainsASingleTextColumnQue)

            Dim ci As New clsCollectionInfo()
            ci.AddField("Queue Name", DataType.text)
            ci.GetField("Queue Name").Description = IboResources.clsWorkQueuesActions_GetQueueNames_TheNameOfTheQueue
            ci.GetField("Queue Name").DisplayName = IboResources.clsWorkQueuesActions_Params_QueueName

            p.CollectionInfo = ci

        End Sub

        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopeStage As clsProcessStage, ByRef sErr As String) As Boolean

            Dim queue As String = Nothing
            Dim key As String = Nothing
            Dim tags As clsTagMask = Nothing
            Dim max As Integer = 0
            Dim skip As Integer = 0

            Try

                Outputs.Clear()

                Dim coll As New clsCollection()
                For Each name As String In gSv.WorkQueueGetAllQueueNames()
                    Dim row As New clsCollectionRow()
                    row.Add("Queue Name", New clsProcessValue(DataType.text, name))
                    coll.Add(row)
                Next
                AddOutput(_T(Params.QueueNames), coll)

                Return True

            Catch ex As Exception
                sErr = ex.Message
                Return False

            End Try

        End Function

        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_GetQueueNames_TheCollectionOfQueueNamesIsReturned
        End Function

    End Class

    ''' Project  : Automate
    ''' Class    : AddToQueueAction
    '''
    ''' <summary>
    ''' Implements the "Add To Queue" action for the "Work Queues" Internal Business
    ''' Object.
    ''' </summary>
    Private Class AddToQueueAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_AddToQueue))
            SetNarrative(String.Format(
             IboResources.clsWorkQueuesActions_AddToQueueAction_TheDataCollectionContainsTheItemsToBeAddedToTheQueueTheCollectionMayContainSing,
             clsWorkQueueItem.MaxLengths.KeyValue, vbCrLf))

            AddParameter(Params.QueueName, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_AddToQueueAction_TheNameOfTheQueueToAddItemsTo)
            AddParameter(Params.Data, DataType.collection, ParamDirection.In,
             IboResources.clsWorkQueuesActions_AddToQueueAction_ACollectionContainingTheDataOneRowForEachItemToBeAdded)
            AddParameter(Params.DeferUntil, DataType.datetime, ParamDirection.In,
             IboResources.clsWorkQueuesActions_AddToQueueAction_OptionallyTheDateTimeTheNewItemSShouldBeDeferredUntil)
            AddParameter(Params.Priority, DataType.number, ParamDirection.In,
             IboResources.clsWorkQueuesActions_AddToQueueAction_OptionallyThePriorityForTheNewItemSLowerNumbersRepresentHigherPrioritiesTheDefa)
            AddParameter(Params.Tags, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_AddToQueueAction_OptionallyASemiColonSeparatedSetOfTagsToApplyToTheQueueItemS)
            AddParameter(Params.Status, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_AddToQueueAction_OptionallyTheInitialStatusRequiredOfTheWorkQueueItemS)
            AddParameter(Params.BatchSize, DataType.number, ParamDirection.In, IboResources.clsWorkQueuesActions_AddToQueueAction_BatchSizeDescription,
                         New RangeParameterValidation(0, 25000))

            Dim p As clsProcessParameter =
             AddParameter(Params.ItemIDs, DataType.collection, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_AddToQueueAction_TheIDsOfTheItemsWhichHaveBeenAddedToTheQueueInTheOrderOfTheCollectionOfDataPass)

            Dim ci As New clsCollectionInfo()
            ci.AddField("Item ID", DataType.text)
            ci.GetField("Item ID").Description = IboResources.clsWorkQueuesActions_AddToQueueAction_TheItemIDOfTheWorkQueueItem
            ci.GetField("Item ID").DisplayName = IboResources.clsWorkQueuesActions_Params_ItemIDs

            p.CollectionInfo = ci

        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_AddToQueueAction_TheItemsAreAddedToTheQueue
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or Nothing
        ''' if unknown.</param>
        ''' <param name="session">The session under which the call is being made,
        ''' or Nothing if unknown.</param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopestage As clsProcessStage,
         ByRef sErr As String) As Boolean

            Dim defaultWorkQueueBatchSize As Integer = 1000
            Dim data As clsCollection = Nothing
            Dim queuename As String = Nothing
            Dim defer As Date = Date.MinValue
            Dim priority As Integer = 0
            Dim tags As String = Nothing
            Dim status As String = Nothing
            For Each arg As clsArgument In Inputs
                If arg.Value.IsNull Then Continue For
                Select Case arg.Name
                    Case "Data" : data = arg.Value.Collection
                    Case "Queue Name" : queuename = CStr(arg.Value)
                    Case "Defer Until" : defer = arg.Value.GetValueAsUTCDateTime()
                    Case "Priority" : priority = CInt(arg.Value)
                    Case "Tags" : tags = CStr(arg.Value)
                    Case "Status" : status = CStr(arg.Value)
                    Case "Batch Size" : defaultWorkQueueBatchSize = CInt(arg.Value)
                End Select
            Next

            If data Is Nothing OrElse data.Rows Is Nothing Then Return SendError(sErr, IboResources.clsWorkQueuesActions_AddToQueueAction_NoDataSpecified)

            If String.IsNullOrWhiteSpace(queuename) Then Return SendError(sErr, IboResources.clsWorkQueuesActions_NoQueueNameSpecified)

            Try
                Dim coll As New clsCollection()
                ' Add the items, getting the newly generated item IDs back.
                ' Add those IDs to a collection and set it into the outputs.

                For Each item As IEnumerable(Of clsCollectionRow) In data.Rows.Batch(defaultWorkQueueBatchSize)
                    For Each itemId As Guid In gSv.WorkQueueAddItems(queuename, item, session.ID, defer, priority, tags, status)
                        Dim row As New clsCollectionRow()
                        row("Item ID") = itemId
                        coll.Add(row)
                    Next
                Next

                Outputs.Clear()
                AddOutput(_T(Params.ItemIDs), coll)

                Return True

            Catch ex As Exception
                sErr = ex.Message
                Return False

            End Try

        End Function

    End Class

    Private Class SetDataAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_SetData))
            SetNarrative(IboResources.clsWorkQueuesActions_SetDataAction_SetsTheDataOnTheSpecifiedItemToTheGivenValueTheCollectionMustContainOneRowExact
            )

            AddParameter(Params.ItemID, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_SetDataAction_TheIDOfTheWorkItemToSetTheDataOnThisItemShouldBeLockedByTheCallingSession)
            AddParameter(Params.Data, DataType.collection, ParamDirection.In,
             IboResources.clsWorkQueuesActions_SetDataAction_ACollectionWithOneRowContainingTheDataToBeSetOnTheItemIfTheItemHasAKeyValueAndT)

        End Sub

        ''' <summary>
        ''' Performs the action of setting the data on a work item.
        ''' </summary>
        ''' <param name="process">The process calling the action.</param>
        ''' <param name="session">The current session.</param>
        ''' <param name="scopeStage">The stage which represents this action.</param>
        ''' <param name="sErr">Any error message from performing this action.</param>
        ''' <returns>True to indicate success; False to indicate failure.</returns>
        Public Overrides Function [Do](ByVal process As clsProcess, ByVal session As clsSession,
         ByVal scopeStage As clsProcessStage, ByRef sErr As String) As Boolean

            sErr = Nothing

            Dim id As Guid = ItemId
            If id = Nothing Then Return SendError(sErr, IboResources.clsWorkQueuesActions_SetDataAction_ItemIDIsInvalid)

            If Not LockedItems.Contains(id) Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_SetDataAction_TheSpecifiedItemWasNotLockedByThisBusinessObject)


            Dim data As clsCollection = Nothing
            Dim collVal As clsProcessValue = Inputs.GetValue(_T(Params.Data))
            If collVal IsNot Nothing Then data = collVal.Collection

            If data Is Nothing OrElse data.Count = 0 Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_SetDataAction_YouMustProvideACollectionToSetOnTheItem)

            If data.Count > 1 Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_SetDataAction_OnlyCollectionsWithASingleRowCanBeSetOnAWorkItemThisCollectionHas0Rows, data.Count)

            Try
                gSv.WorkQueueItemSetData(id, data)
                Return True

            Catch ex As Exception
                Return SendError(sErr,
                 IboResources.clsWorkQueuesActions_SetDataAction_AnErrorOccurredSettingTheData0, ex.Message)

            End Try

        End Function

        ''' <summary>
        ''' Gets the preconditions for the set data action.
        ''' </summary>
        ''' <returns>A collection of preconditions for the action</returns>
        Public Overrides Function GetPreconditions() As Collection
            Return BuildCollection(
             IboResources.clsWorkQueuesActions_SetDataAction_TheWorkItemMustHaveBeenLockedByTheSession,
             IboResources.clsWorkQueuesActions_SetDataAction_TheCollectionMustContainASingleRow,
             IboResources.clsWorkQueuesActions_SetDataAction_TheCollectionMustHaveTheSameKeyValueAsTheItemBeingUpdated
            )
        End Function

        ''' <summary>
        ''' Gets the expected endpoint of the action.
        ''' </summary>
        ''' <returns>The post-condition of a successful call to the set data action.
        ''' </returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_SetDataAction_TheItemSDataCollectionIsSetToTheGivenValue
        End Function
    End Class

    ''' Project  : Automate
    ''' Class    : clsWorkQueuesGetNextItem
    '''
    ''' <summary>
    ''' Implements the "Get Next Item" action for the "Work Queues" Internal Business
    ''' Object.
    ''' </summary>
    Private Class GetNextItemAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_GetNextItem))
            SetNarrative(IboResources.clsWorkQueuesActions_GetNextItemAction_ThisActionGetsTheNextItemToBeWorkedFromTheQueueReturningItsDetailsAndLockingItS)

            AddParameter(Params.QueueName, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetNextItemAction_TheNameOfTheQueue)
            AddParameter(Params.KeyFilter, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetNextItemAction_OptionallyAKeyValueToFilterByOnlyItemsWithThisKeyValueWillBeConsidered)
            AddParameter(Params.TagFilter, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetNextItemAction_OptionallyATagMaskToFilterByThisCanConsistOfAnyNumberOfTagSearchesEachTermCanBe)

            AddParameter(Params.ItemID, DataType.text, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetNextItemAction_TheIDOfTheItemRetrievedEmptyIfThereAreNoneAvailable)
            AddParameter(Params.Data, DataType.collection, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetNextItemAction_TheDataAssociatedWithTheItemASingleCollectionRow)
            AddParameter(Params.Status, DataType.text, ParamDirection.Out, IboResources.clsWorkQueuesActions_GetNextItemAction_TheStatusOfTheItem)
            AddParameter(Params.Attempts, DataType.number, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetNextItemAction_TheNumberOfAttemptsAlreadyMadeToWorkThisItem)

            DefaultLoggingInhibitMode = LogInfo.InhibitModes.Never
        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_GetNextItemAction_AnItemIsRetrievedFromTheQueueIfAvailable
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or Nothing
        ''' if unknown.</param>
        ''' <param name="session">The session under which the call is being made,
        ''' or Nothing if unknown.</param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopestage As clsProcessStage,
         ByRef sErr As String) As Boolean

            Dim queuename As String = Nothing
            Dim keyfilter As String = Nothing
            Dim tagMask As clsTagMask = Nothing
            For Each arg As clsArgument In Inputs
                If arg.Value.IsNull Then Continue For
                Select Case arg.Name
                    Case "Queue Name" : queuename = CStr(arg.Value)
                    Case "Key Filter" : keyfilter = CStr(arg.Value)
                    Case "Tag Filter"
                        Try
                            ' Check for errors in the tag filter
                            tagMask = New clsTagMask(CStr(arg.Value), True)
                        Catch ex As Exception
                            sErr = IboResources.clsWorkQueuesActions_GetNextItemAction_InvalidTagFilter & ex.Message
                            Return False
                        End Try
                End Select
            Next

            If String.IsNullOrWhiteSpace(queuename) Then Return SendError(sErr, IboResources.clsWorkQueuesActions_NoQueueNameSpecified)

            Dim item As clsWorkQueueItem = Nothing

            Const maxDeadlockRetries As Integer = 5

            Dim count As Integer = 0
            Dim success As Boolean = False

            ' Try and get the item MaxDeadlockRetries times - if there's a deadlock,
            ' increment the count and try again until either we run out of retries or
            ' we reach the item that we're trying to retrieve.
            While Not success ' can't test for null - null indicates "no item"
                Try
                    item = gSv.WorkQueueGetNext(
                     session.ID, queuename, keyfilter, tagMask)
                    success = True

                Catch databaseException As SqlException
                    If _
                        databaseException.Number <> DatabaseErrorCode.GetAppLockFailed And
                    databaseException.Number <> DatabaseErrorCode.LockRequestTimeOutPeriodExceeded And
                        databaseException.Number <> DatabaseErrorCode.DeadlockVictim And
                        databaseException.Message.IndexOf(
                            Convert.ToInt32(DatabaseErrorCode.LockRequestTimeOutPeriodExceeded).ToString(),
                            StringComparison.Ordinal) <= -1 And
                        databaseException.Message.IndexOf(
                                Convert.ToInt32(DatabaseErrorCode.GetAppLockFailed).ToString(),
                                StringComparison.Ordinal) <= -1 And
                        databaseException.Message.IndexOf(Convert.ToInt32(DatabaseErrorCode.DeadlockVictim).ToString(),
                                           StringComparison.Ordinal) <= -1 Then
                        Return SendError(sErr, databaseException.Message)
                    Else
                        count += 1
                    End If
                    ' If we've reached our limit, pass out the error to the process
                    If count > maxDeadlockRetries Then
                        Return SendError(sErr, $"Retries {count}, " + databaseException.Message)
                    End If

                Catch ex As Exception ' Any other error is passed out directly
                    Return SendError(sErr, ex.Message)

                End Try
            End While

            Outputs.Clear()
            If item Is Nothing Then
                AddOutput(_T(Params.ItemID), Guid.Empty)
                'These values are irrelevant when no item is being returned, but
                'we output something 'sensible' just in case...
                AddOutput(_T(Params.Data), New clsCollection())
                AddOutput(_T(Params.Status), "")
                AddOutput(_T(Params.Attempts), 0)
            Else
                AddOutput(_T(Params.ItemID), item.ID)
                AddOutput(_T(Params.Data), item.Data)
                AddOutput(_T(Params.Status), item.Status)
                AddOutput(_T(Params.Attempts), item.AttemptsSoFar)
                LockedItems.Add(item.ID)
            End If

            Return True
        End Function

    End Class

    Private Class GetItemByIdAction : Inherits WorkQueueAction

        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_GetItemById))
            SetNarrative(IboResources.clsWorkQueuesActions_GetItemByIdAction_ThisActionGetsTheItemCorrespondingToTheId)

            AddParameter(Params.QueueName, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetItemByIdAction_TheNameOfTheQueue)
            AddParameter(Params.ItemID, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetItemByIdAction_TheIdOfTheQueueItem)

            AddParameter(Params.ItemID, DataType.text, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetNextItemAction_TheIDOfTheItemRetrievedEmptyIfThereAreNoneAvailable)
            AddParameter(Params.Data, DataType.collection, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetNextItemAction_TheDataAssociatedWithTheItemASingleCollectionRow)
            AddParameter(Params.Status, DataType.text, ParamDirection.Out, IboResources.clsWorkQueuesActions_GetNextItemAction_TheStatusOfTheItem)
            AddParameter(Params.Attempts, DataType.number, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetNextItemAction_TheNumberOfAttemptsAlreadyMadeToWorkThisItem)

            DefaultLoggingInhibitMode = LogInfo.InhibitModes.Never
        End Sub

        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_GetItemByIdAction_AnItemIsRetrievedFromTheQueueIfAvailable
        End Function

        Public Overrides Function [Do](
            ByVal process As clsProcess,
            ByVal session As clsSession,
            ByVal scopestage As clsProcessStage,
            ByRef sErr As String) As Boolean

            Dim queueName As String = Nothing
            Dim itemId = Guid.Empty
            For Each arg As clsArgument In Inputs
                If arg.Value.IsNull Then Continue For
                Select Case arg.Name
                    Case "Queue Name" : queueName = CStr(arg.Value)
                    Case "Item ID" : itemId = Guid.Parse(CStr(arg.Value))
                End Select
            Next
            If String.IsNullOrWhiteSpace(queueName) Then Return SendError(sErr, IboResources.clsWorkQueuesActions_NoQueueNameSpecified)
            If itemId = Guid.Empty Then Return SendError(sErr, IboResources.clsWorkQueuesActions_GetItemByIdAction_NoItemIdSpecified)

            Dim item As clsWorkQueueItem = Nothing

            Const maxDeadlockRetries As Integer = 5

            Dim count As Integer = 0
            Dim success As Boolean = False

            ' Try and get the item MaxDeadlockRetries times - if there's a deadlock,
            ' increment the count and try again until either we run out of retries or
            ' we reach the item that we're trying to retrieve.
            While Not success ' can't test for null - null indicates "no item"
                Try
                    item = gSv.WorkQueueGetById(queueName, session.ID, itemId)
                    success = True

                Catch databaseException As SqlException
                    If _
                        databaseException.Number <> DatabaseErrorCode.GetAppLockFailed And
                    databaseException.Number <> DatabaseErrorCode.LockRequestTimeOutPeriodExceeded And
                        databaseException.Number <> DatabaseErrorCode.DeadlockVictim And
                        databaseException.Message.IndexOf(
                            Convert.ToInt32(DatabaseErrorCode.LockRequestTimeOutPeriodExceeded).ToString(),
                            StringComparison.Ordinal) <= -1 And
                        databaseException.Message.IndexOf(
                                Convert.ToInt32(DatabaseErrorCode.GetAppLockFailed).ToString(),
                                StringComparison.Ordinal) <= -1 And
                        databaseException.Message.IndexOf(Convert.ToInt32(DatabaseErrorCode.DeadlockVictim).ToString(),
                                           StringComparison.Ordinal) <= -1 Then
                        Return SendError(sErr, databaseException.Message)
                    Else
                        count += 1
                    End If
                    ' If we've reached our limit, pass out the error to the process
                    If count > maxDeadlockRetries Then
                        Return SendError(sErr, $"Retries {count}, " + databaseException.Message)
                    End If

                Catch ex As Exception ' Any other error is passed out directly
                    Return SendError(sErr, ex.Message)

                End Try
            End While

            Outputs.Clear()
            If item Is Nothing Then
                AddOutput(_T(Params.ItemID), Guid.Empty)
                'These values are irrelevant when no item is being returned, but
                'we output something 'sensible' just in case...
                AddOutput(_T(Params.Data), New clsCollection())
                AddOutput(_T(Params.Status), "")
                AddOutput(_T(Params.Attempts), 0)
            Else
                AddOutput(_T(Params.ItemID), item.ID)
                AddOutput(_T(Params.Data), item.Data)
                AddOutput(_T(Params.Status), item.Status)
                AddOutput(_T(Params.Attempts), item.AttemptsSoFar)
                LockedItems.Add(item.ID)
            End If

            Return True
        End Function

    End Class

    ''' Project  : Automate
    ''' Class    : clsWorkQueuesMarkCompleted
    '''
    ''' <summary>
    ''' Implements the "Mark Completed" action for the "Work Queues" Internal
    ''' Business Object.
    ''' </summary>
    Private Class MarkCompletedAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_MarkCompleted))
            SetNarrative(IboResources.clsWorkQueuesActions_MarkCompletedAction_MarksTheItemAsCompletedByUpdatingTheCompletedTimestampAndSettingLockedToNullThe)
            AddParameter(Params.ItemID, DataType.text, ParamDirection.In, IboResources.clsWorkQueuesActions_MarkCompletedAction_TheIDOfTheItemToMark)
        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_MarkCompletedAction_TheItemIsMarkedAsCompletedAndUnlocked
        End Function

        ''' <summary>
        ''' Get the preconditions for this action.
        ''' </summary>
        ''' <returns>The preconditions as a Collection containing a String for each
        ''' precondition.</returns>
        Public Overrides Function GetPreConditions() As Collection
            Return SingletonPreCondition(IboResources.clsWorkQueuesActions_MarkCompletedAction_TheItemMustHaveBeenLockedByThisProcess)
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or Nothing
        ''' if unknown.</param>
        ''' <param name="session">The session under which the call is being made,
        ''' or Guid.Empty if unknown.</param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopestage As clsProcessStage,
         ByRef sErr As String) As Boolean
            Dim id As Guid = ItemId
            If id = Nothing Then Return SendError(sErr, IboResources.clsWorkQueuesActions_MarkCompletedAction_InvalidItemID)

            If Not LockedItems.Contains(id) Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_MarkCompletedAction_TheSpecifiedItemWasNotLockedByThisBusinessObject)

            Try
                gSv.WorkQueueMarkComplete(session.ID, ItemId)
            Catch ex As Exception
                sErr = ex.Message
                Return False
            End Try

            LockedItems.Remove(ItemId)
            Return True

        End Function

    End Class

    ''' Project  : Automate
    ''' Class    : clsWorkQueuesMarkException
    '''
    ''' <summary>
    ''' Implements the "Mark Exception" action for the "Work Queues" Internal
    ''' Business Object.
    ''' </summary>
    Private Class MarkExceptionAction : Inherits WorkQueueAction

        Private Const ExceptionReasonArgName As String = "Exception Reason"
        Private Const RetryArgName As String = "Retry"
        Private Const KeepLockedArgName As String = "Keep Locked"

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)

            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_MarkException))
            SetNarrative(IboResources.clsWorkQueuesActions_MarkExceptionAction_MarksTheItemAsAnExceptionOptionallyRetryingItByCloningTheItemAndReturningTheIDO)
            mParameters.Add(New clsProcessParameter(Params.ItemID, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_MarkExceptionAction_TheIDOfTheItemToMark))
            mParameters.Add(New clsProcessParameter(Params.ExceptionReason, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_MarkExceptionAction_ADescriptionOfTheReasonForTheException))
            mParameters.Add(New clsProcessParameter(Params.Retry, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_MarkExceptionAction_WhetherOrNotToRetryTheItemUpToTheMaximumNumberOfAttemptsSpecifiedForTheQueueOpt))
            mParameters.Add(New clsProcessParameter(Params.KeepLocked, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_MarkExceptionAction_WhetherOrNotTheCloneOfTheItemShouldRemainLockedAfterTheExceptionHasBeenRegister))
            mParameters.Add(New clsProcessParameter(Params.NewItemID, DataType.text, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_MarkExceptionAction_TheItemIDOfTheNewlyClonedItemThisItemWillBeLockedIfKeepLockedIsTrueThisWillBeEm))

        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_MarkExceptionAction_AnExceptionIsMarkedOnTheItemAndItIsUnlockedPossiblyAClonedItemIsCreatedAndLocked
        End Function

        ''' <summary>
        ''' Get the preconditions for this action.
        ''' </summary>
        ''' <returns>The preconditions as a Collection containing a String for each
        ''' precondition.</returns>
        Public Overrides Function GetPreConditions() As Collection
            Return SingletonPreCondition(IboResources.clsWorkQueuesActions_MarkExceptionAction_TheItemMustHaveBeenLockedByThisProcess)
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or Nothing
        ''' if unknown.</param>
        ''' <param name="session">The session under which the call is being made,
        ''' or Guid.Empty if unknown.</param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopestage As clsProcessStage,
         ByRef sErr As String) As Boolean

            Dim id As Guid = ItemId
            If id = Nothing Then Return SendError(sErr, IboResources.clsWorkQueuesActions_MarkExceptionAction_InvalidItemID)

            Dim reason As String = Nothing
            Dim retry As Boolean = True
            Dim keepLocked As Boolean = False
            For Each arg As clsArgument In Inputs
                If arg.Value.IsNull Then Continue For
                Select Case arg.Name
                    Case ExceptionReasonArgName : reason = CStr(arg.Value)
                    Case RetryArgName : retry = CBool(arg.Value)
                    Case KeepLockedArgName : keepLocked = CBool(arg.Value)
                End Select
            Next

            If reason Is Nothing Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_MarkExceptionAction_ExceptionReasonMustBeSpecified)

            If Not LockedItems.Contains(id) Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_MarkExceptionAction_TheSpecifiedItemWasNotLockedByThisBusinessObject)

            Dim retried As Boolean = False
            Try
                gSv.WorkQueueMarkException(
                 session.ID, id, reason, retry, keepLocked, retried, Nothing, False)

            Catch ex As Exception
                sErr = ex.Message
                Return False

            End Try

            If Not (retried AndAlso keepLocked) Then LockedItems.Remove(id)

            ' Set the new ID if a retry has occurred.
            ' Actually, this is always the same as the last ID - it is not given a
            ' new one on a retry, only a new IDENT and attempt number.
            Outputs.Clear()
            If retried Then AddOutput(_T(Params.NewItemID), id) Else AddOutput(_T(Params.NewItemID), "")

            Return True

        End Function

    End Class

    ''' Project  : Automate
    ''' Class    : DeferAction
    '''
    ''' <summary>
    ''' Implements the "Defer" action for the "Work Queues" Internal Business Object.
    ''' </summary>
    Private Class DeferAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_Defer))
            SetNarrative(IboResources.clsWorkQueuesActions_DeferAction_MarksTheItemAsDeferredItIsUnlockedButItWillNotBeMadeAvailableForProcessingAgain)
            mParameters.Add(New clsProcessParameter(Params.ItemID, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_DeferAction_TheIDOfTheItemToDefer))
            mParameters.Add(New clsProcessParameter(Params.Until, DataType.datetime, ParamDirection.In,
             IboResources.clsWorkQueuesActions_DeferAction_WhenToResumeProcessingTheCase))
        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_DeferAction_TheItemIsDeferredToTheSpecifiedTimeAndUnlocked
        End Function

        ''' <summary>
        ''' Get the preconditions for this action.
        ''' </summary>
        ''' <returns>The preconditions as a Collection containing a String for each
        ''' precondition.</returns>
        Public Overrides Function GetPreConditions() As Collection
            Return SingletonPreCondition(IboResources.clsWorkQueuesActions_DeferAction_TheItemMustHaveBeenLockedByThisProcess)
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or Nothing
        ''' if unknown.</param>
        ''' <param name="session">The session under which the call is being made,
        ''' or Nothing if unknown.</param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopestage As clsProcessStage,
         ByRef sErr As String) As Boolean

            Dim id As Guid = ItemId
            If id = Nothing Then Return SendError(sErr, IboResources.clsWorkQueuesActions_DeferAction_InvalidItemID)

            Dim until As Date = CDate(Inputs.GetValue(_T(Params.Until)))
            If until = Date.MinValue Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_DeferAction_UntilMustBeSpecified)

            If Not LockedItems.Contains(id) Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_DeferAction_TheSpecifiedItemWasNotLockedByThisBusinessObject)

            Try
                gSv.WorkQueueDefer(session.ID, id, until, Nothing)
            Catch ex As Exception
                sErr = ex.Message
                Return False
            End Try

            LockedItems.Remove(id)
            Return True
        End Function

    End Class

    ''' Project  : Automate
    ''' Class    : UpdateStatusAction
    '''
    ''' <summary>
    ''' Implements the "Update Status" action for the "Work Queues" Internal
    ''' Business Object.
    ''' </summary>
    Private Class UpdateStatusAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_UpdateStatus))
            SetNarrative(IboResources.clsWorkQueuesActions_UpdateStatusAction_UpdatesTheStatusFieldForTheItemTheStatusFieldCanBeUsedToFlagWhatHasBeenDoneInAP)
            mParameters.Add(New clsProcessParameter(Params.ItemID, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_UpdateStatusAction_TheIDOfTheItemToMark))
            mParameters.Add(New clsProcessParameter(Params.Status, DataType.text, ParamDirection.In,
             String.Format(IboResources.clsWorkQueuesActions_UpdateStatusAction_TheNewStatus0CharactersMaximum,
             clsWorkQueueItem.MaxLengths.Status)))
        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_UpdateStatusAction_TheItemSStatusIsUpdatedItRemainsLocked
        End Function

        ''' <summary>
        ''' Get the preconditions for this action.
        ''' </summary>
        ''' <returns>The preconditions as a Collection containing a String for each
        ''' precondition.</returns>
        Public Overrides Function GetPreConditions() As Collection
            Return SingletonPreCondition(IboResources.clsWorkQueuesActions_UpdateStatusAction_TheItemMustHaveBeenLockedByThisProcess)
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or Nothing
        ''' if unknown.</param>
        ''' <param name="session">The session under which the call is being made,
        ''' or Nothing if unknown.</param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopestage As clsProcessStage,
         ByRef sErr As String) As Boolean

            Dim id As Guid = ItemId
            If id = Nothing Then Return SendError(sErr, IboResources.clsWorkQueuesActions_UpdateStatusAction_InvalidItemID)

            Dim status As String = CStr(Inputs.GetValue(_T(Params.Status)))
            If status Is Nothing Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_UpdateStatusAction_StatusMustBeSpecified)

            If Not LockedItems.Contains(id) Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_UpdateStatusAction_TheSpecifiedItemWasNotLockedByThisBusinessObject)

            Try
                gSv.WorkQueueUpdateStatus(id, status)
                Return True

            Catch aoore As ArgumentOutOfRangeException
                sErr = IboResources.clsWorkQueuesActions_UpdateStatusAction_TheGivenStatusWasTooLong

            Catch ex As Exception
                sErr = ex.Message

            End Try
            Return False

        End Function

    End Class

    ''' Project  : Automate
    ''' Class    : DeleteProcessItemsAction
    '''
    ''' <summary>
    ''' Implements the "Delete Processed Items" action for the "Work Queues" Internal
    ''' Business Object.
    ''' </summary>
    Private Class DeleteProcessedItemsAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_DeleteProcessedItems))
            mParameters.Add(New clsProcessParameter(Params.QueueName, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_DeleteProcessedItemsAction_TheNameOfTheQueueToDeleteItemsFrom))
            mParameters.Add(New clsProcessParameter(Params.DateThreshold, DataType.datetime, ParamDirection.In,
             IboResources.clsWorkQueuesActions_DeleteProcessedItemsAction_TheThresholdDateItemsCompletedOrMarkedWithAnExceptionBeforeThisDateWillBeDelete))
            mParameters.Add(New clsProcessParameter(Params.DeleteFromAllQueues, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_DeleteProcessedItemsAction_IndicatesWhetherToDeleteItemsFromAllQueues))
            mParameters.Add(New clsProcessParameter(Params.DeletionCount, DataType.number, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_DeleteProcessedItemsAction_IndicatesTheNumberOfItemsWhichWereDeletedWhichMayLegitimatelyBeZero))
        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_DeleteProcessedItemsAction_ItemsProcessedBeforeTheThresholdDateWillHaveBeenDeleted
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or Nothing
        ''' if unknown.</param>
        ''' <param name="session">The session under which the call is being made,
        ''' or Guid.Empty if unknown.</param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopestage As clsProcessStage,
         ByRef sErr As String) As Boolean

            Dim queuename As String = Nothing
            Dim threshold As DateTime = DateTime.MaxValue
            Dim allqueues As Boolean = False
            For Each arg As clsArgument In Inputs
                If arg.Value.IsNull Then Continue For
                Select Case arg.Name
                    Case "Queue Name" : queuename = CStr(arg.Value)
                    Case "Date Threshold" : threshold = arg.Value.GetValueAsUTCDateTime()
                    Case "Delete From All Queues" : allqueues = CBool(arg.Value)
                End Select
            Next

            Outputs.Clear()
            Try
                Dim deleted As Integer = gSv.WorkQueueClearWorkedByDate(queuename, threshold, allqueues)
                AddOutput(_T(Params.DeletionCount), deleted)
                Return True

            Catch ex As Exception
                sErr = ex.Message
                Return False
            End Try

        End Function

    End Class

    ''' Project  : Automate
    ''' Class    : UnlockItemAction
    ''' <summary>
    ''' Action to unlock a particular work queue item.
    ''' </summary>
    Private Class UnlockItemAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Creates a new action to unlock a work queue item for the given business
        ''' object
        ''' </summary>
        ''' <param name="bo">The business object to which this action will belong.
        ''' </param>
        Public Sub New(ByVal bo As clsWorkQueuesBusinessObject)
            MyBase.New(bo)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_UnlockItem))
            SetNarrative(IboResources.clsWorkQueuesActions_UnlockItemAction_UnlocksACurrentlyLockedWorkQueueItem)

            AddParameter(Params.ItemID, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_UnlockItemAction_TheIDOfTheWorkQueueItemWhichShouldBeUnlocked)

            AddParameter(Params.Unlocked, DataType.flag, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_UnlockItemAction_WhetherTheItemWasUnlockedAsAResultOfThisActionOrNotIfTheItemHadFinishedBeingWor)

        End Sub

        ''' <summary>
        ''' Performs the action of unlocking a work queue item, returning a flag
        ''' indicating whether the item was unlocked as a result of this action
        ''' or not.
        ''' </summary>
        ''' <param name="process">The process performing this action.</param>
        ''' <param name="session">The session that this action is
        ''' being performed within.</param>
        ''' <param name="scopeStage">The stage. With scope.</param>
        ''' <param name="sErr">The error message if this action fails for any
        ''' reason.</param>
        ''' <returns>True to indicate that the action was successful, false to
        ''' indicate that it failed.</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopeStage As clsProcessStage, ByRef sErr As String) As Boolean

            Try
                Dim workQueueItemId As Guid = ItemId
                If workQueueItemId = Nothing Then Return SendError(sErr, IboResources.clsWorkQueuesActions_UnlockItemAction_InvalidItemID)

                Dim unlocked As Boolean = gSv.WorkQueueUnlockItem(workQueueItemId)
                LockedItems.Remove(workQueueItemId)

                Outputs.Clear()
                AddOutput(_T(Params.Unlocked), unlocked)
                Return True

            Catch ex As Exception
                sErr = ex.Message
                Return False

            End Try

        End Function

        ''' <summary>
        ''' Gets the endpoint for this action after succesful execution.
        ''' </summary>
        ''' <returns>The endpoint of this action.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_UnlockItemAction_TheSpecifiedItemIsNoLongerLocked
        End Function

        ''' <summary>
        ''' Gets the preconditions for this action.
        ''' </summary>
        ''' <returns>The preconditions for this action.</returns>
        Public Overrides Function GetPreConditions() As Collection
            Return SingletonPreCondition(IboResources.clsWorkQueuesActions_UnlockItemAction_TheItemBeingUnlockedIsCurrentlyLocked)
        End Function
    End Class

    ''' <summary>
    ''' Action used to retrieve the current pending items from a work queue
    ''' </summary>
    Private Class GetPendingItemsAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Creates a new action for dealing with getting pending items.
        ''' </summary>
        ''' <param name="bo">The business object that this action is a part of.</param>
        Public Sub New(ByVal bo As clsWorkQueuesBusinessObject)
            MyBase.New(bo)

            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_GetPendingItems))
            SetNarrative(
             IboResources.clsWorkQueuesActions_GetPendingItemsAction_RetrievesTheIDsForAllCurrentlyPendingWorkQueueItemsInTheSpecifiedQueueMatchingT)

            ' Input params
            AddParameter(Params.QueueName, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetPendingItemsAction_TheNameOfTheQueueForWhichThePendingItemsAreRequired)
            AddParameter(Params.KeyFilter, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetPendingItemsAction_OptionallyAKeyValueToFilterByOnlyItemsWithThisKeyValueWillBeConsidered)
            AddParameter(Params.TagFilter, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetPendingItemsAction_OptionallyATagMaskToFilterByThisCanConsistOfAnyNumberOfTagSearchesEachTermCanBe)
            AddParameter(Params.Maximum, DataType.number, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetPendingItemsAction_OptionallyTheMaximumNumberOfItemsToReturnDefaultIsToReturnAllItems)
            AddParameter(Params.Skip, DataType.number, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetPendingItemsAction_OptionallyTheNumberOfItemsToSkipBeforeReturningDefaultIsZeroIeReturnAllItemsFro)

            ' Output params
            Dim ci As New clsCollectionInfo()
            ci.AddField("Item ID", DataType.text)
            ci.GetField("Item ID").Description = IboResources.clsWorkQueuesActions_GetPendingItemsAction_TheItemIDOfTheWorkQueueItem
            ci.GetField("Item ID").DisplayName = IboResources.clsWorkQueuesActions_Params_ItemIDs


            AddParameter(Params.PendingItems, ParamDirection.Out, ci,
             IboResources.clsWorkQueuesActions_GetPendingItemsAction_TheCurrentlyPendingItemsInTheQueueReferencedByIDTheSingleColumnIsItemIDTextTheE)

        End Sub

        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopeStage As clsProcessStage, ByRef sErr As String) As Boolean

            Dim queue As String = Nothing
            Dim key As String = Nothing
            Dim tags As clsTagMask = Nothing
            Dim max As Integer = 0
            Dim skip As Integer = 0

            Try
                For Each arg As clsArgument In Inputs
                    Dim val As clsProcessValue = arg.Value
                    If val.IsNull Then Continue For

                    Select Case arg.Name
                        Case "Queue Name" : queue = CStr(val)
                        Case "Key Filter" : key = CStr(val)
                        Case "Tag Filter" : tags = New clsTagMask(CStr(val), True)
                        Case "Maximum" : max = CInt(val)
                        Case "Skip" : skip = CInt(val)
                    End Select
                Next

                If String.IsNullOrWhiteSpace(queue) Then _
                 Throw New InvalidOperationException(IboResources.clsWorkQueuesActions_NoQueueNameSpecified)

                Dim coll As New clsCollection()
                For Each id As Guid In gSv.WorkQueueGetPending(queue, key, tags, max, skip)
                    Dim row As New clsCollectionRow()
                    row.Add("Item ID", New clsProcessValue(DataType.text, id.ToString()))
                    coll.Add(row)
                Next

                AddOutput(_T(Params.PendingItems), coll)
                Return True

            Catch ex As Exception
                Return SendError(sErr, ex.Message)

            End Try

        End Function

        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_GetPendingItemsAction_TheCurrentlyPendingItemsHaveBeenReturned
        End Function

    End Class

    ''' Project  : Automate
    ''' Class    : GetLockedItemsAction
    ''' <summary>
    ''' Action to get data on the locked items within a specified work queue.
    ''' </summary>
    Private Class GetLockedItemsAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Creates a new action for dealing with getting locked item data.
        ''' </summary>
        ''' <param name="bo">The business object that this action is a part of.</param>
        Public Sub New(ByVal bo As clsWorkQueuesBusinessObject)
            MyBase.New(bo)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_GetLockedItems))
            SetNarrative(
             IboResources.clsWorkQueuesActions_GetLockedItemsAction_RetrievesTheIDsAndLockedTimesForAllCurrentlyLockedWorkQueueItemsInTheSpecifiedQ)

            ' Input params
            AddParameter(Params.QueueName, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetLockedItemsAction_TheNameOfTheQueueForWhichTheLockedItemsAreRequired)
            AddParameter(Params.KeyFilter, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetLockedItemsAction_OptionallyAKeyValueToFilterByOnlyItemsWithThisKeyValueWillBeConsidered)
            AddParameter(Params.Tags, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetLockedItemsAction_OptionallyATagMaskToFilterByThisCanConsistOfAnyNumberOfTagSearchesEachTermCanBe)

            ' Output params
            Dim ci As New clsCollectionInfo()
            ci.AddField("Item ID", DataType.text)
            ci.GetField("Item ID").Description = IboResources.clsWorkQueuesActions_GetLockedItemsAction_TheItemIDOfTheWorkQueueItem
            ci.GetField("Item ID").DisplayName = IboResources.clsWorkQueuesActions_Params_ItemIDs


            ci.AddField("Locked", DataType.datetime)
            ci.GetField("Locked").Description = IboResources.clsWorkQueuesActions_GetLockedItemsAction_TheDateTimeWhenTheItemWasLocked
            ci.GetField("Locked").DisplayName = IboResources.clsWorkQueuesActions_Params_Locked

            AddParameter(Params.LockedItems, ParamDirection.Out, ci,
             IboResources.clsWorkQueuesActions_GetLockedItemsAction_TheCurrentlyLockedItemsInTheQueueColumnsAreItemIDTextAndLockedDatetimeTheElemen)

        End Sub

        ''' <summary>
        ''' Performs this action with the given parameters.
        ''' </summary>
        ''' <param name="process">The process that is calling this action.</param>
        ''' <param name="session">The session that this action is being performed
        ''' within.</param>
        ''' <param name="scopeStage">The stage 'used to resolve the scope'.</param>
        ''' <param name="sErr">Reference parameter which holds any error message if
        ''' this action fails.</param>
        ''' <returns>True on successful execution of this action; False if any failures
        ''' occur while attempting to carry it out.</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopeStage As clsProcessStage,
         ByRef sErr As String) As Boolean

            Try
                Dim qName As String = Nothing
                Dim keyFilter As String = Nothing
                Dim tags As clsTagMask = Nothing

                For Each arg As clsArgument In Inputs
                    If arg.Value.IsNull Then Continue For
                    Select Case arg.Name
                        Case "Queue Name" : qName = CStr(arg.Value)
                        Case "Key Filter" : keyFilter = CStr(arg.Value)
                        Case "Tags" : tags = New clsTagMask(CStr(arg.Value), True)
                    End Select
                Next
                If qName Is Nothing Then _
                    Throw New ArgumentException(IboResources.clsWorkQueuesActions_NoQueueNameSpecified)

                Outputs.Clear()

                Dim coll As New clsCollection()
                For Each entry As KeyValuePair(Of Guid, Date) In
                 gSv.WorkQueueGetLocked(qName, keyFilter, tags)

                    Dim row As New clsCollectionRow()
                    row.Add("Item ID", entry.Key)
                    row.Add("Locked", entry.Value)
                    coll.Add(row)

                Next
                AddOutput(_T(Params.LockedItems), coll)

                Return True

            Catch ex As Exception
                sErr = ex.Message
                Return False

            End Try

        End Function

        ''' <summary>
        ''' Gets the endpoint for this action
        ''' </summary>
        ''' <returns>This action has no side effects, thus there is no endpoint</returns>
        Public Overrides Function GetEndpoint() As String
            Return ""
        End Function

    End Class

    ''' Project  : Automate
    ''' Class    : GetCompletedItemsAction
    ''' <summary>
    ''' Implements the "Get Completed Items" action for the "Work Queues" Internal
    ''' Business Object.
    ''' </summary>
    Private Class GetCompletedItemsAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Creates a new WorkQueue.GetCompletedItems action belonging to the given
        ''' business object
        ''' </summary>
        ''' <param name="parent">The business object on which this action is
        ''' published</param>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)

            ' Name of the action
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_GetCompletedItems))
            SetNarrative(IboResources.clsWorkQueuesActions_GetCompletedItemsAction_GetsTheWorkItemsFromTheSpecifiedQueueWhichHaveBeenMarkedAsComplete)

            ' Input params
            AddParameter(Params.QueueName, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetCompletedItemsAction_TheNameOfTheQueueToRetrieveTheCompletedItemsFrom)
            AddParameter(Params.StartDate, DataType.datetime, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetCompletedItemsAction_TheStartDateFromWhichAnyItemsMarkedAsCompleteShouldBeReturnedOptionalNoDateIndi)
            AddParameter(Params.EndDate, DataType.datetime, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetCompletedItemsAction_TheEndDateUpToWhichAnyItemsMarkedAsCompleteShouldBeReturnedOptionalNoDateIndica)
            AddParameter(Params.KeyFilter, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetCompletedItemsAction_OptionallyAKeyValueToFilterByOnlyItemsWithThisKeyValueWillBeConsidered)
            AddParameter(Params.TagFilter, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetCompletedItemsAction_OptionallyATagMaskToFilterByThisCanConsistOfAnyNumberOfTagSearchesEachTermCanBe)
            AddParameter(Params.MaximumRows, DataType.number, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetCompletedItemsAction_TheMaximumNumberOfRowsToReturnOptionalNoValueIndicatesThatAllRowsShouldBeReturn)

            ' Output params
            Dim ci As New clsCollectionInfo()
            ci.AddField("Item ID", DataType.text)
            ci.GetField("Item ID").Description = IboResources.clsWorkQueuesActions_GetCompletedItemsAction_TheItemIDOfTheWorkQueueItem
            ci.GetField("Item ID").DisplayName = IboResources.clsWorkQueuesActions_Params_ItemID

            AddParameter(Params.CompletedItems, ParamDirection.Out, ci,
             IboResources.clsWorkQueuesActions_GetCompletedItemsAction_TheItemsWhichWereMarkedAsCompleteWithinTheGivenDateRangeUpToTheMaximumCountIfOn)

        End Sub

        ''' <summary>
        ''' Perfomrs the action of getting completed queue items
        ''' </summary>
        ''' <param name="process">A reference to the process calling this action</param>
        ''' <param name="session">The session under which the call is being made</param>
        ''' <param name="ScopeStage">The stage used to resolve the scope</param>
        ''' <param name="sErr">Set with error text if this action fails</param>
        ''' <returns>True to indicate success, False otherwise.</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal ScopeStage As clsProcessStage,
         ByRef sErr As String) As Boolean

            Dim name As String = Nothing
            Dim startDate As Date = Date.MinValue
            Dim endDate As Date = Date.MaxValue
            Dim keyFilter As String = Nothing
            Dim tags As clsTagMask = Nothing
            Dim max As Integer = 0

            Try
                For Each arg As clsArgument In Inputs
                    Dim val As clsProcessValue = arg.Value
                    If val.IsNull Then Continue For

                    Select Case arg.Name
                        Case "Queue Name" : name = CStr(val)
                        Case "Start Date" : startDate = CDate(val)
                        Case "End Date" : endDate = CDate(val)
                        Case "Key Filter" : keyFilter = CStr(val)
                        Case "Tag Filter" : tags = New clsTagMask(CStr(val), True)
                        Case "Maximum Rows" : max = CInt(arg.Value)
                    End Select
                Next

                Outputs.Clear()

                Dim c As New clsCollection()
                For Each id As Guid In gSv.WorkQueueGetCompleted(name, startDate, endDate, keyFilter, tags, max)
                    Dim r As New clsCollectionRow()
                    r.Add("Item ID", id)
                    c.Add(r)
                Next
                AddOutput(_T(Params.CompletedItems), c)

                Return True

            Catch ex As Exception
                Dim errorMessage As String
                If String.IsNullOrWhiteSpace(name) Then
                    errorMessage = IboResources.clsWorkQueuesActions_NoQueueNameSpecified
                Else
                    errorMessage = ex.Message
                End If
                Return SendError(sErr, errorMessage)
            End Try

        End Function

        ''' <summary>
        ''' Gets the post-condition of this action
        ''' </summary>
        ''' <returns>A single string containing the result of performing this action</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_GetCompletedItemsAction_CompletedItemsInTheRequestedDateRangeWillBeReturnedSubjectToTheMaximumNumberOfR
        End Function

    End Class

    ''' Project  : Automate
    ''' Class    : GetExceptionItemsAction
    '''
    ''' <summary>
    ''' Implements the "Get Exception Items" action for the "Work Queues" Internal
    ''' Business Object.
    ''' </summary>
    Private Class GetExceptionItemsAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_GetExceptionItems))

            SetNarrative(IboResources.clsWorkQueuesActions_GetExceptionItemsAction_GetsTheWorkItemsFromTheSpecifiedQueueWhichHaveBeenMarkedWithAnException)

            ' Input params
            AddParameter(Params.QueueName, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetExceptionItemsAction_TheNameOfTheQueueToRetrieveTheExceptionItemsFrom)
            AddParameter(Params.StartDate, DataType.datetime, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetExceptionItemsAction_OptionallyTheStartThresholdDateAnyItemsReturnedWillHaveBeenMarkedAsAnExceptionA)
            AddParameter(Params.EndDate, DataType.datetime, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetExceptionItemsAction_OptionallyTheEndThresholdDateAnyItemsReturnedWillHaveBeenMarkedAsAnExceptionBef)
            AddParameter(Params.KeyFilter, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetExceptionItemsAction_OptionallyAKeyValueToFilterByOnlyItemsWithThisKeyValueWillBeConsidered)
            AddParameter(Params.TagFilter, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetExceptionItemsAction_OptionallyATagMaskToFilterByThisCanConsistOfAnyNumberOfTagSearchesEachTermCanBe)
            AddParameter(Params.MaximumRows, DataType.number, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetExceptionItemsAction_OptionallyTheMaximumNumberOfRowsToReturnNoValueIndicatesThatAllRowsShouldBeRetu)
            AddParameter(Params.ResourceName, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueueActions_GetExceptionItemsAction_TheNameOfTheResourceThatWorkedTheExceptionedItems())

            ' Output params
            Dim ci As New clsCollectionInfo()
            ci.AddField("Item ID", DataType.text)
            ci.GetField("Item ID").Description = IboResources.clsWorkQueuesActions_GetExceptionItemsAction_TheItemIDOfTheWorkQueueItem
            ci.GetField("Item ID").DisplayName = IboResources.clsWorkQueuesActions_Params_ItemID

            AddParameter(Params.ExceptionItems, ParamDirection.Out, ci,
             IboResources.clsWorkQueuesActions_GetExceptionItemsAction_TheItemsWhichWereMarkedWithAnExceptionWithinTheGivenDateRangeUpToTheMaximumCoun)

        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_GetExceptionItemsAction_ExceptionItemsInTheRequestedDateRangeWillBeReturnedSubjectToTheMaximumNumberOfR
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or Nothing
        ''' if unknown.</param>
        ''' <param name="session">The session under which the call is being made</param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](ByVal process As clsProcess, ByVal session As clsSession, ByVal scopestage As clsProcessStage, ByRef sErr As String) As Boolean

            Dim name As String = Nothing
            Dim startDate As DateTime = Date.MinValue
            Dim endDate As DateTime = Date.MaxValue
            Dim keyFilter As String = Nothing
            Dim tags As clsTagMask = Nothing
            Dim max As Integer = 0
            Dim resourceName As String = Nothing

            Try
                For Each arg As clsArgument In Inputs
                    Dim val As clsProcessValue = arg.Value
                    If val.IsNull Then Continue For

                    Select Case arg.Name
                        Case "Queue Name" : name = CStr(val)
                        Case "Start Date" : startDate = CDate(val)
                        Case "End Date" : endDate = CDate(val)
                        Case "Key Filter" : keyFilter = CStr(val)
                        Case "Tag Filter" : tags = New clsTagMask(CStr(val), True)
                        Case "Maximum Rows" : max = CInt(val)
                        Case "Resource Name" : resourceName = CStr(val)
                    End Select
                Next

                Outputs.Clear()

                Dim c As New clsCollection()
                For Each id As Guid In gSv.WorkQueueGetExceptions(
                 name, startDate, endDate, keyFilter, tags, max, resourceName)
                    Dim r As New clsCollectionRow()
                    r.Add("Item ID", id)
                    c.Add(r)
                Next
                AddOutput(_T(Params.ExceptionItems), c)

                Return True

            Catch ex As Exception
                Dim errorMessage As String
                If String.IsNullOrWhiteSpace(name) Then
                    errorMessage = IboResources.clsWorkQueuesActions_NoQueueNameSpecified
                Else
                    errorMessage = ex.Message
                End If
                Return SendError(sErr, errorMessage)
            End Try
        End Function


    End Class

    ''' Project  : Automate
    ''' Class    : DeleteItemAction
    '''
    ''' <summary>
    ''' Implements the "Delete Item" action for the "Work Queues" Internal Business
    ''' Object.
    ''' </summary>
    Private Class DeleteItemAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_DeleteItem))
            SetNarrative(IboResources.clsWorkQueuesActions_DeleteItemAction_DeleteTheGivenItemFromAQueueNoteThatThereIsNoLockingInvolvedInThisIEYouShouldOn)
            Dim p As New clsProcessParameter(Params.ItemID, DataType.text, ParamDirection.In)
            p.Narrative = IboResources.clsWorkQueuesActions_DeleteItemAction_TheIDOfTheItemToDeleteAnErrorWillBeRaisedIfThisItemDoesNotExistOrIsLocked
            mParameters.Add(p)
        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_DeleteItemAction_TheSpecifiedItemWillHaveBeenDeletedAndItsDetailsReturnedAsOutputs
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or Nothing
        ''' if unknown.</param>
        ''' <param name="session">The session under which the call is being made.</param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](ByVal process As clsProcess, ByVal session As clsSession, ByVal scopestage As clsProcessStage, ByRef sErr As String) As Boolean
            Try
                Dim id As Guid = ItemId
                If id = Nothing Then Return SendError(sErr, IboResources.clsWorkQueuesActions_DeleteItemAction_BadItemIDSupplied)
                Return gSv.WorkQueueDeleteItem(id)

            Catch ex As Exception
                Return SendError(sErr, ex.Message)
            End Try

        End Function

    End Class

    ''' Project  : Automate
    ''' Class    : GetItemDataAction
    ''' <summary>
    ''' Implements the "Get Item Data" action for the "Work Queues" Internal Business
    ''' Object.
    ''' </summary>
    Private Class GetItemDataAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_GetItemData))
            SetNarrative(IboResources.clsWorkQueuesActions_GetItemDataAction_GetDataRelatingToAnItemInAQueueYouDoNotNeedToHaveALockOnTheItemToDoThis)

            ' Single input parameter
            AddParameter(Params.ItemID, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetItemDataAction_TheIDOfTheItemToGetDataFromAnErrorWillBeRaisedIfThisItemDoesNotExist)

            ' Output parameters
            AddParameter(Params.Key, DataType.text, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetItemDataAction_TheKeyValueOfTheItem)
            AddParameter(Params.Priority, DataType.number, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetItemDataAction_ThePriorityOfTheItem)
            AddParameter(Params.Status, DataType.text, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetItemDataAction_TheStatusOfTheItem)
            AddParameter(Params.Attempts, DataType.number, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetItemDataAction_TheNumberOfAttemptsAlreadyMadeToWorkThisItem)
            AddParameter(Params.Worktime, DataType.timespan, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetItemDataAction_TheTotalTimeSpentOnThisItemIncludingPreviousAttempts)
            AddParameter(Params.AttemptWorktime, DataType.timespan, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetItemDataAction_TheAmountOfTimeSpentOnThisParticularAttemptOfTheItem)
            AddParameter(Params.LoadedDateTime, DataType.datetime, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetItemDataAction_TheDateAndTimeAtWhichThisItemWasFirstLoadedIntoTheQueue)
            AddParameter(Params.DeferredDateTime, DataType.datetime, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetItemDataAction_TheDateAndTimeAtWhichThisItemHasBeenDeferredToEmptyIfThisItemHasNotBeenDeferred)
            AddParameter(Params.CompletedDateTime, DataType.datetime, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetItemDataAction_TheDateAndTimeAtWhichThisItemWasMarkedAsCompleteEmptyIfThisItemHasNotBeenMarked)
            AddParameter(Params.ExceptionDateTime, DataType.datetime, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetItemDataAction_TheDateAndTimeAtWhichThisItemWasMarkedAsAnExceptionEmptyIfThereHasNeverBeenAnEx)
            AddParameter(Params.ExceptionReason, DataType.text, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetItemDataAction_TheExceptionReasonRecordedAgainstThisItem)
            AddParameter(Params.Tags, DataType.text, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetItemDataAction_TheSemiColonSeparatedSetOfTagsWhichApplyToTheItem)
            AddParameter(Params.Data, DataType.collection, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetItemDataAction_TheDataAssociatedWithTheItemASingleCollectionRow)

        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_GetItemDataAction_TheItemWillBeUnchanged
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or Nothing
        ''' if unknown.</param>
        ''' <param name="session">The session under which the call is being made.</param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess, ByVal session As clsSession, ByVal scopestage As clsProcessStage,
         ByRef sErr As String) As Boolean

            Try
                Dim id As Guid = ItemId
                If id = Nothing Then Return SendError(sErr, IboResources.clsWorkQueuesActions_GetItemDataAction_InvalidItemID)

                Dim item As clsWorkQueueItem = gSv.WorkQueueGetItem(id)

                ' If the item ID could not be found, it will return null
                If item Is Nothing Then Return SendError(sErr,
                 IboResources.clsWorkQueuesActions_GetItemDataAction_TheRequestedItemDoesNotExist)

                AddOutput(_T(Params.Key), item.KeyValue)
                AddOutput(_T(Params.Priority), item.Priority)
                AddOutput(_T(Params.Status), item.Status)
                AddOutput(_T(Params.Attempts), item.AttemptsSoFar)
                AddOutput(_T(Params.Worktime), item.WorkTimeSpan)
                AddOutput(_T(Params.AttemptWorktime), item.AttemptWorkTimeSpan)
                AddOutput(_T(Params.LoadedDateTime), DataType.datetime, item.Loaded)
                AddOutput(_T(Params.DeferredDateTime), DataType.datetime, item.Deferred)
                AddOutput(_T(Params.CompletedDateTime), DataType.datetime, item.CompletedDate)
                AddOutput(_T(Params.ExceptionDateTime), DataType.datetime, item.ExceptionDate)
                AddOutput(_T(Params.ExceptionReason), item.ExceptionReason)
                AddOutput(_T(Params.Tags), item.TagString)
                AddOutput(_T(Params.Data), item.Data)

                Return True

            Catch fe As FormatException
                Return SendError(sErr, IboResources.clsWorkQueuesActions_GetItemDataAction_FailedToParseItemID)

            Catch ex As Exception
                Return SendError(sErr, IboResources.clsWorkQueuesActions_GetItemDataAction_ErrorGettingItemData & ex.Message)

            End Try

        End Function

    End Class

    ''' Project  : Automate
    ''' Class    : IsItemInQueueAction
    '''
    ''' <summary>
    ''' Implements the "Is Item In Queue" action for the "Work Queues" Internal Business
    ''' Object.
    ''' </summary>
    Private Class IsItemInQueueAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)

            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_IsItemInQueue))
            SetNarrative(IboResources.clsWorkQueuesActions_IsItemInQueueAction_CheckIfAnItemWithTheGivenKeyIsAlreadyInTheQueue)

            ' Left foot in.
            AddParameter(Params.QueueName, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_IsItemInQueueAction_TheNameOfTheQueueToCheck)

            AddParameter(Params.Key, DataType.text, ParamDirection.In, IboResources.clsWorkQueuesActions_IsItemInQueueAction_TheKeyToCheck)

            AddParameter(Params.IncludePending, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_IsItemInQueueAction_CheckAnyItemsInTheQueueWhichAreCurrentlyAwaitingBeingWorkedThisWillIncludeAnyDe)
            AddParameter(Params.IncludeDeferred, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_IsItemInQueueAction_CheckAnyItemsInTheQueueWhichAreCurrentlyDeferredToALaterDateOptionalDefaultIsTr)
            AddParameter(Params.IncludeCompleted, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_IsItemInQueueAction_CheckAnyItemsInTheQueueWhichAreCompletedOptionalDefaultIsTrue)
            AddParameter(Params.IncludeTerminated, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_IsItemInQueueAction_CheckAnyItemsInTheQueueWhichHaveBeenFullyTerminatedOptionalDefaultIsTrue)

            ' Left foot out.
            AddParameter(Params.Result, DataType.flag, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_IsItemInQueueAction_TrueIfAnItemWithTheGivenKeyIsInTheQueueFalseOtherwise)

            Dim ci As New clsCollectionInfo()
            ci.AddField("Item ID", DataType.text)
            ci.GetField("Item ID").Description = IboResources.clsWorkQueuesActions_IsItemInQueueAction_TheItemIDOfTheWorkQueueItem
            ci.GetField("Item ID").DisplayName = IboResources.clsWorkQueuesActions_Params_ItemID


            AddParameter(Params.ItemIDs, ParamDirection.Out, ci,
             IboResources.clsWorkQueuesActions_IsItemInQueueAction_TheItemIDsWhichRepresentTheWorkItemsFoundWithTheGivenKey)

        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_IsItemInQueueAction_TheItemWillBeUnchanged
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or Nothing
        ''' if unknown.</param>
        ''' <param name="session">The session under which the call is being made.</param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](ByVal process As clsProcess, ByVal session As clsSession, ByVal scopestage As clsProcessStage, ByRef sErr As String) As Boolean
            Dim key As String = Nothing
            Dim qname As String = Nothing
            Dim pending As Boolean = True, deferred As Boolean = True,
             completed As Boolean = True, terminated As Boolean = True

            For Each arg As clsArgument In Inputs

                Dim val As clsProcessValue = arg.Value
                ' We always check if val is null - and we always leave at default if it is...
                ' might as well shortcut out now.
                If val.IsNull Then Continue For

                Select Case arg.Name
                    Case "Key" : key = CStr(val)
                    Case "Queue Name" : qname = CStr(val)
                    Case "Include Pending" : pending = CBool(val)
                    Case "Include Deferred" : deferred = CBool(val)
                    Case "Include Completed" : completed = CBool(val)
                    Case "Include Terminated" : terminated = CBool(val)
                End Select
            Next

            If key Is Nothing Then Return SendError(sErr, IboResources.clsWorkQueuesActions_IsItemInQueueAction_NoKeySpecified)
            If String.IsNullOrWhiteSpace(qname) Then Return SendError(sErr, IboResources.clsWorkQueuesActions_NoQueueNameSpecified)

            Outputs.Clear()
            Try
                Dim ids As ICollection(Of Guid) = gSv.WorkQueueIsItemInQueue(
                 qname, key, pending, deferred, completed, terminated)

                AddOutput(_T(Params.Result), ids.Count > 0)
                Dim coll As New clsCollection()
                For Each itemId As Guid In ids
                    Dim row As New clsCollectionRow()
                    row("Item ID") = New clsProcessValue(itemId)
                    coll.Add(row)
                Next
                AddOutput(_T(Params.ItemIDs), coll)
                Return True

            Catch ex As Exception
                Return SendError(sErr, ex.Message)

            End Try
        End Function

    End Class

    ''' Project  : Automate
    ''' Class    : TagItemAction
    ''' <summary>
    ''' Implements the "Tag Item" action for the "Work Queues" Internal Business Object.
    ''' </summary>
    Private Class TagItemAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_TagItem))
            SetNarrative(IboResources.clsWorkQueuesActions_TagItemAction_AddsATagToAWorkQueueItem)
            mParameters.Add(New clsProcessParameter(Params.ItemID, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_TagItemAction_TheIDOfTheQueueItemToAddTheTagTo))
            mParameters.Add(New clsProcessParameter(Params.Tag, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_TagItemAction_TheTagToAddToTheItemNoteThatThisCannotStartWithAPlusOrMinusCharacterAndCannotCo))
        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_TagItemAction_TheItemHasTheGivenTagAppliedToIt
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or Nothing
        ''' if unknown.</param>
        ''' <param name="session">The session under which the call is being made.</param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopestage As clsProcessStage,
         ByRef sErr As String) As Boolean
            Try
                Dim id As Guid = ItemId
                If id = Nothing Then Return SendError(sErr, IboResources.clsWorkQueuesActions_TagItemAction_NoQueueItemSpecified)

                Dim tag As String = CStr(Inputs.GetValue(_T(Params.Tag)))

                If tag IsNot Nothing Then tag = tag.Trim()
                If tag = "" Then Return SendError(sErr, IboResources.clsWorkQueuesActions_TagItemAction_TagCannotBeEmpty)

                Dim lockedByCurrentProcess = LockedItems.Contains(id)

                gSv.WorkQueueItemAddTag(id, tag, lockedByCurrentProcess)

                Return True
            Catch ex As Exception
                sErr = ex.Message
                Return False
            End Try
        End Function

    End Class

    ''' Project  : Automate
    ''' Class    : UntagItemAction
    ''' <summary>
    ''' Implements the "Untag Item" action for the "Work Queues" Internal Business Object
    ''' </summary>
    Private Class UntagItemAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_UntagItem))
            SetNarrative(IboResources.clsWorkQueuesActions_UntagItemAction_RemovesATagFromAWorkQueueItem)
            mParameters.Add(New clsProcessParameter(Params.ItemID, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_UntagItemAction_TheIDOfTheQueueItemToRemoveTheTagFrom))
            mParameters.Add(New clsProcessParameter(Params.Tag, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_UntagItemAction_TheTagToRemoveFromTheItemNoteThatThisCannotStartWithAPlusOrMinusCharacterAndCan))
        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_UntagItemAction_TheItemWillNotHaveTheSpecifiedTagAppliedToIt
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or Nothing
        ''' if unknown.</param>
        ''' <param name="session">The session under which the call is being made.</param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopestage As clsProcessStage,
         ByRef sErr As String) As Boolean
            Try
                Dim id As Guid = ItemId
                If id = Nothing Then Return SendError(sErr, IboResources.clsWorkQueuesActions_UntagItemAction_NoQueueItemSpecified)

                Dim tag As String = Inputs.GetString(_T(Params.Tag))

                If tag IsNot Nothing Then tag = tag.Trim()
                If tag = "" Then Return SendError(sErr, IboResources.clsWorkQueuesActions_UntagItemAction_TagCannotBeEmpty)

                Dim lockedByCurrentProcess = LockedItems.Contains(id)

                gSv.WorkQueueItemRemoveTag(id, tag, lockedByCurrentProcess)

                Return True
            Catch ex As Exception
                sErr = ex.Message
                Return False
            End Try

        End Function

    End Class

    ''' <summary>
    ''' Gets transactional data from the work queue log.
    ''' </summary>
    Private Class GetTransactionDataAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Creates a new action to get transactional data.
        ''' </summary>
        ''' <param name="parent">The work queue business object that this action forms
        ''' a part of.</param>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)

            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_GetTransactionData))
            SetNarrative(IboResources.clsWorkQueuesActions_GetTransactionDataAction_RetrievesTransactionCountsForAllQueuesExceptThoseSpecifiedUsingTheGivenParamete)

            ' Input parameters...
            ' Define the fields on the collection
            Dim info As New clsCollectionInfo()
            info.AddField("Queue Name", DataType.text)
            info.GetField("Queue Name").Description = IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheNameOfTheQueueToExclude
            info.GetField("Queue Name").DisplayName = IboResources.DocumentProcessingBusinessObject_Params_QueueName

            AddParameter(Params.ExcludedQueueNames, ParamDirection.In, info,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheNamesOfTheQueuesToExcludeFromTheTransactionCountAnyQueuesNotSpecifiedInThisP)

            ' The date ranges
            AddParameter(Params.StartDateTime, DataType.datetime, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheUTCDateTimeFromWhichActivityCountsShouldBeRetrievedThisIsInclusive)
            AddParameter(Params.EndDateTime, DataType.datetime, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheUTCDateTimeUpToAndIncludingWhichActivityCountsShouldBeRetrievedThisIsInclusi)

            ' Include flags
            AddParameter(Params.CountItemCreations, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TrueToCountTheWorkQueueItemsBeingCreatedDefaultIsFalse)
            AddParameter(Params.CountItemLocks, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TrueToCountTheWorkQueueItemsBeingLockedDefaultIsFalse)
            AddParameter(Params.CountItemDeferrals, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TrueToCountTheWorkQueueItemsBeingDeferredDefaultIsFalse)
            AddParameter(Params.CountItemCompletions, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TrueToCountTheWorkQueueItemsBeingCompletedSuccessfullyDefaultIsFalse)
            AddParameter(Params.CountItemExceptionsWithAutomaticRetry, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TrueToCountTheWorkQueueItemsBeingMarkedWithAnExceptionAndAutomaticallyRetriedDe)
            AddParameter(Params.CountItemExceptionsWithoutAutomaticRetry, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TrueToCountTheWorkQueueItemsBeingMarkedWithAnExceptionAndWithoutAnAutomaticRetr)
            AddParameter(Params.CountItemDeletions, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TrueToCountTheWorkQueueItemsBeingDeletedDefaultIsFalse)
            AddParameter(Params.CountItemForcedRetries, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TrueToCountTheWorkQueueItemsWhichWereForceRetriedIeTheyHadBeenMarkedWithAnExcep)

            ' Output parameters
            ' Define the fields on the collection
            Dim outfo As New clsCollectionInfo()
            outfo.AddField("Queue Name", DataType.text)
            outfo.GetField("Queue Name").Description = IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheNameOfTheIncludedQueue
            outfo.GetField("Queue Name").DisplayName = IboResources.DocumentProcessingBusinessObject_Params_QueueName

            AddParameter(Params.IncludedQueues, ParamDirection.Out, outfo,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheQueuesThatWereIncludedInTheReportContainsASingleTextColumnCalledQueueName)

            AddParameter(Params.Created, DataType.number, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheNumberOfItemCreationsCountedZeroIfTheyWereNotCounted)
            AddParameter(Params.Locked, DataType.number, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheNumberOfItemLocksCountedZeroIfTheyWereNotCounted)
            AddParameter(Params.Deferred, DataType.number, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheNumberOfDeferralsCountedZeroIfTheyWereNotCounted)
            AddParameter(Params.Completed, DataType.number, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheNumberOfItemCompletionsCountedZeroIfTheyWereNotCounted)
            AddParameter(Params.ExceptionsWithRetry, DataType.number, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheNumberOfItemExceptionsWithAutomaticRetryCountedZeroIfTheyWereNotCounted)
            AddParameter(Params.ExceptionsWithNoRetry, DataType.number, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheNumberOfItemExceptionsWithoutAutomaticRetryCountedZeroIfTheyWereNotCounted)
            AddParameter(Params.Deletions, DataType.number, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheNumberOfItemDeletionsCountedZeroIfTheyWereNotCounted)
            AddParameter(Params.ForceRetries, DataType.number, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheNumberOfItemForcedRetriesCountedZeroIfTheyWereNotCounted)

            AddParameter(Params.TotalCount, DataType.number, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheTotalNumberQueueItemOperationsCounted)

        End Sub

        ''' <summary>
        ''' Method to add an operation to a set of work queue operations depending on
        ''' a specified process value.
        ''' </summary>
        ''' <param name="ops">The set of operations to potentially add the given
        ''' operation to.</param>
        ''' <param name="op">The operation to add</param>
        ''' <param name="val">The value which will determine whether to add the operation
        ''' or not.</param>
        Private Sub AddOp(
         ByVal ops As clsSet(Of WorkQueueOperation),
         ByVal op As WorkQueueOperation,
         ByVal val As clsProcessValue)
            If CBool(val) Then ops.Add(op)
        End Sub

        ''' <summary>
        ''' Performs the action of getting the transactional queue data.
        ''' </summary>
        ''' <param name="process">The process calling this action.</param>
        ''' <param name="session">The session in which this action is being called.</param>
        ''' <param name="scopeStage">A scope stage.</param>
        ''' <param name="sErr">The output error message if any error should occur.
        ''' </param>
        ''' <returns>True if successful, False otherwise.</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess, ByVal session As clsSession,
         ByVal scopeStage As clsProcessStage, ByRef sErr As String) As Boolean

            If Not License.TransactionModel Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheCurrentlyInstalledLicenceDoesNotSupportQueueTransactions)

            Dim qnameColl As clsCollection = Nothing
            Dim startDate As Date = Date.MinValue
            Dim endDate As Date = Date.MaxValue
            Dim ops As New clsSet(Of WorkQueueOperation)
            For Each arg As clsArgument In Inputs
                Dim val As clsProcessValue = arg.Value
                If val.IsNull Then Continue For
                Select Case arg.Name
                    Case "Excluded Queue Names" : qnameColl = val.Collection
                    Case "Start Date/Time" : startDate = val.GetValueAsUTCDateTime()
                    Case "End Date/Time" : endDate = val.GetValueAsUTCDateTime()
                    Case "Count item creations"
                        AddOp(ops, WorkQueueOperation.ItemCreated, val)
                    Case "Count item locks"
                        AddOp(ops, WorkQueueOperation.ItemLocked, val)
                    Case "Count item deferrals"
                        AddOp(ops, WorkQueueOperation.ItemDeferred, val)
                    Case "Count item completions"
                        AddOp(ops, WorkQueueOperation.ItemCompletedSuccessfully, val)
                    Case "Count item exceptions with automatic retry"
                        AddOp(ops, WorkQueueOperation.ItemRetryInitiated, val)
                    Case "Count item exceptions without automatic retry"
                        AddOp(ops, WorkQueueOperation.ItemCompletedWithException, val)
                    Case "Count item deletions"
                        AddOp(ops, WorkQueueOperation.ItemDeleted, val)
                    Case "Count item forced retries"
                        AddOp(ops, WorkQueueOperation.ItemForceRetried, val)
                End Select
            Next

            Try

                If startDate = Date.MinValue Then
                    Throw New ArgumentException(
                     IboResources.clsWorkQueuesActions_GetTransactionDataAction_YouMustProvideAnInclusiveUTCStartDateTime, IboResources.clsWorkQueuesActions_Params_StartDateTime)
                End If

                Dim excludedQueues As New clsSet(Of String)
                If qnameColl IsNot Nothing Then
                    For Each row As clsCollectionRow In qnameColl.Rows
                        If Not row.ContainsKey("Queue Name") Then
                            Throw New ArgumentException(
                             IboResources.clsWorkQueuesActions_GetTransactionDataAction_NoQueueNameColumnFoundInTheExcludedQueuesCollection,
                             IboResources.clsWorkQueuesActions_Params_QueueNames)
                        End If
                        excludedQueues.Add(CStr(row("Queue Name")))
                    Next
                End If

                ' Get all the work queues
                Dim includedQueues As IBPSet(Of String) = gSv.WorkQueueGetAllQueueNames()

                ' includedQueues currently contains all queue names... so let's remove some.
                includedQueues.Subtract(excludedQueues)

                Dim results As IDictionary(Of WorkQueueOperation, Integer) =
                 gSv.WorkQueueLogCountEntries(includedQueues, startDate, endDate, ops)

                Outputs.Clear()

                ' First, detail which queues we actually reported on.
                Dim coll As New clsCollection()
                For Each qName As String In includedQueues
                    Dim row As New clsCollectionRow()
                    row("Queue Name") = qName
                    coll.Add(row)
                Next
                AddOutput(_T(Params.IncludedQueues), coll)

                ' And then all the actual results.
                AddOutput(_T(Params.Created), results(WorkQueueOperation.ItemCreated))
                AddOutput(_T(Params.Locked), results(WorkQueueOperation.ItemLocked))
                AddOutput(_T(Params.Deferred), results(WorkQueueOperation.ItemDeferred))
                AddOutput(_T(Params.Completed), results(WorkQueueOperation.ItemCompletedSuccessfully))
                AddOutput(_T(Params.ExceptionsWithRetry), results(WorkQueueOperation.ItemRetryInitiated))
                AddOutput(_T(Params.ExceptionsWithNoRetry), results(WorkQueueOperation.ItemCompletedWithException))
                AddOutput(_T(Params.Deletions), results(WorkQueueOperation.ItemDeleted))
                AddOutput(_T(Params.ForceRetries), results(WorkQueueOperation.ItemForceRetried))

                AddOutput(_T(Params.TotalCount), results(WorkQueueOperation.None))

                Return True

            Catch ex As Exception
                sErr = ex.Message
                Return False

            End Try
        End Function

        Public Overrides Function GetPreConditions() As Collection
            Return SingletonPreCondition(
             IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheInstalledLicenseMustSupportQueueTransactions)
        End Function

        ''' <summary>
        ''' Gets the expected endpoint for this action.
        ''' </summary>
        ''' <returns>A string indicating the endpoint of the action.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_GetTransactionDataAction_TheCountOfTheSpecifiedWorkQueueOperationsWillBeReturned
        End Function
    End Class

    ''' <summary>
    ''' Gets statistical and operational data for the work queues.
    ''' </summary>
    Private Class GetReportDataAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)

            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_GetReportData))
            SetNarrative(IboResources.clsWorkQueuesActions_GetReportDataAction_RetrievesQueueDataForAnalysisTheQueueNameAndAtLeastOneOfTheDateParametersMustBe)

            ' Input parameters...
            AddParameter(Params.QueueName, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetReportDataAction_TheNameOfTheQueueForWhichTheDetailsAreRequiredThisParameterIsMandatory)

            ' The date ranges
            AddParameter(Params.FinishedStartDate, DataType.datetime, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetReportDataAction_TheItemFinishCompleteOrExceptionDateTimeToStartResults)
            AddParameter(Params.FinishedEndDate, DataType.datetime, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetReportDataAction_TheItemFinishCompleteOrExceptionDateTimeToEndResultsIfEmptyThisIsTheEndOfTheDay)
            AddParameter(Params.LoadedStartDate, DataType.datetime, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetReportDataAction_TheItemLoadedAddedToQueueDateTimeToStartResultsIfEmptyThenTheLoadedStartDateIsI)
            AddParameter(Params.LoadedEndDate, DataType.datetime, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetReportDataAction_TheItemLoadedAddedToQueueDateTimeToEndResultsIfEmptyThisIsTheEndOfTheDayFromThe)

            ' More filters
            AddParameter(Params.ResourceNames, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetReportDataAction_TheSemiColonSeparatedNamesOfTheResourcesWhichWorkedTheItemIfEmptyThisWillRetrie)
            AddParameter(Params.Tags, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetReportDataAction_OptionallyATagMaskToFilterByThisCanConsistOfAnyNumberOfTagSearchesEachTermCanBe)

            ' Include flags
            AddParameter(Params.IncludeUnworkedItems, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetReportDataAction_IncludeAnyItemsWhichAreCurrentlyPendingDefaultIsFalse)
            AddParameter(Params.IncludeDeferredItems, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetReportDataAction_IncludeAnyItemsWhichAreCurrentlyDeferredDefaultIsFalse)
            AddParameter(Params.IncludeCompletedItems, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetReportDataAction_IncludeAnyItemsWhichAreCurrentlyCompletedDefaultIsFalse)
            AddParameter(Params.IncludeExceptionItems, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetReportDataAction_IncludeAnyItemsWhichAreCurrentlyExceptionedDefaultIsFalse)

            ' The treat-as-one parameter
            AddParameter(Params.TreatEachAttemptSeparately, DataType.flag, ParamDirection.In,
             IboResources.clsWorkQueuesActions_GetReportDataAction_EachTimeAQueueItemExceptionsANewLinkedRetryItemIsCreatedWithAnIncrementedAttemp)

            ' Output parameters
            ' The count
            AddParameter(Params.ItemCount, DataType.number, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetReportDataAction_TheTotalNumberOfQueueItemsMatchedWithTheGivenParameters)

            ' The time - totals, boundaries and averages
            AddParameter(Params.TimeTotal, DataType.timespan, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetReportDataAction_TheTotalAmountOfTimeSpentWorkingWithTheMatchedQueueItems)
            AddParameter(Params.LeastTime, DataType.timespan, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetReportDataAction_TheLeastAmountOfTimeSpentWorkingOnASingleMatchedQueueItem)
            AddParameter(Params.MostTime, DataType.timespan, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetReportDataAction_TheMostAmountOfTimeSpentWorkingOnASingleMatchedQueueItem)
            AddParameter(Params.MedianTime, DataType.timespan, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetReportDataAction_TheMedianAmountOfTimeSpentByTheMatchedQueueItems)
            AddParameter(Params.MeanTime, DataType.timespan, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_GetReportDataAction_TheMeanAmountOfTimeSpentByEachMatchedQueueItem)

            ' The item IDs
            Dim ci As New clsCollectionInfo()
            ci.AddField("Item ID", DataType.text)
            ci.GetField("Item ID").Description = IboResources.clsWorkQueuesActions_GetReportDataAction_TheItemIDOfTheWorkQueueItem
            ci.GetField("Item ID").DisplayName = IboResources.clsWorkQueuesActions_Params_ItemID

            AddParameter(Params.ItemIDs, ParamDirection.Out, ci,
            IboResources.clsWorkQueuesActions_GetReportDataAction_TheItemIDsOfAllWorkQueueItemsMatchedByTheSearchNoteThatAllAttemptsOfTheSameItem)

        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return ""
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or Nothing
        ''' if unknown.</param>
        ''' <param name="session">The session under which the call is being made.</param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopestage As clsProcessStage,
         ByRef sErr As String) As Boolean

            ' Let's parse some args...
            Try

                ' Now prepare the output - the ReportParams object parses the input for us.
                Dim report As ReportData = gSv.WorkQueueGetReportData(New ReportParams(Me.Inputs))

                Outputs.Clear()

                AddOutput(_T(Params.ItemCount), report.Count)
                AddOutput(_T(Params.TimeTotal), report.TotalTime)
                AddOutput(_T(Params.LeastTime), report.LeastTime)
                AddOutput(_T(Params.MostTime), report.MostTime)
                AddOutput(_T(Params.MedianTime), report.MedianTime)
                AddOutput(_T(Params.MeanTime), report.MeanTime)

                Dim coll As New clsCollection()
                For Each itemId As Guid In report.Items
                    Dim row As New clsCollectionRow()
                    row("Item ID") = itemId
                    coll.Add(row)
                Next
                AddOutput(_T(Params.ItemIDs), coll)
                Return True

            Catch ae As ArgumentException
                Return SendError(sErr,
                 IboResources.clsWorkQueuesActions_GetReportDataAction_ErrorInParameter01, ae.ParamName, ae.Message)

            Catch appe As ApplicationException
                Return SendError(sErr, IboResources.clsWorkQueuesActions_GetReportDataAction_ErrorRetrievingData0, appe.Message)

            Catch ex As Exception
                Return SendError(sErr, ex.Message)

            End Try

        End Function

    End Class

    ''' <summary>
    ''' Action which sets the priority of a work item within the queues business
    ''' object
    ''' </summary>
    Private Class SetPriorityAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_SetPriority))
            SetNarrative(
             IboResources.clsWorkQueuesActions_SetPriorityAction_SetsThePriorityOfTheLatestAttemptOfAWorkQueueItem)
            AddParameter(Params.ItemID, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_SetPriorityAction_TheIDOfTheQueueItemToSetThePriorityIn)
            AddParameter(Params.Priority, DataType.number, ParamDirection.In,
             IboResources.clsWorkQueuesActions_SetPriorityAction_TheNewPriorityToSetWithinTheWorkQueueItem)
        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_SetPriorityAction_TheItemWillHaveTheNewPrioritySetWithinIt
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or
        ''' Nothing if unknown.</param>
        ''' <param name="session">The session under which the call is being made.
        ''' </param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopestage As clsProcessStage,
         ByRef sErr As String) As Boolean

            Dim itemId As Guid = Nothing
            Dim priority As Integer = -1
            For Each arg As clsArgument In Inputs
                If arg.Value.IsNull Then Continue For
                Select Case arg.Name
                    Case "Item ID" : itemId = CType(arg.Value, Guid)
                    Case "Priority" : priority = CInt(arg.Value)
                End Select
            Next

            If itemId = Nothing Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_SetPriorityAction_NoQueueItemSpecified)
            If priority = -1 Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_SetPriorityAction_NoPrioritySpecified)

            Try
                gSv.WorkQueueSetPriority(itemId, priority)
                Return True
            Catch ex As Exception
                Return SendError(sErr,
                 IboResources.clsWorkQueuesActions_SetPriorityAction_ErrorWhileSettingPriority0, ex.Message)
            End Try

        End Function

    End Class

    ''' <summary>
    ''' Action which sets the priority of a work item within the queues business
    ''' object
    ''' </summary>
    Private Class CopyToQueueAction : Inherits WorkQueueAction

        ''' <summary>
        ''' Constructor - sets the details of the action.
        ''' </summary>
        Public Sub New(ByVal parent As clsWorkQueuesBusinessObject)
            MyBase.New(parent)
            SetName(NameOf(IboResources.clsWorkQueuesActions_Action_CopyItemToQueue))
            SetNarrative(
             IboResources.clsWorkQueuesActions_CopyToQueueAction_CopiesTheDataRepresentingAWorkItemToADifferentQueueIncludingItsTagsPriorityAndS)
            AddParameter(Params.ItemID, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_CopyToQueueAction_TheIDOfTheWorkItemToCopy)
            AddParameter(Params.QueueName, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_CopyToQueueAction_TheNameOfTheQueueToCopyTheItemTo)
            AddParameter(Params.TagMask, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_CopyToQueueAction_TheTagsToAddNewTagOrRemoveOldTagToFromTheNewItemTagChangesCanBeCombinedUsingSem)
            AddParameter(Params.Status, DataType.text, ParamDirection.In,
             IboResources.clsWorkQueuesActions_CopyToQueueAction_TheStatusTextToSetInTheNewItemIfNotSetTheItemWillUseTheOldItemSStatus)
            AddParameter(Params.Priority, DataType.number, ParamDirection.In,
             IboResources.clsWorkQueuesActions_CopyToQueueAction_ThePriorityToSetInTheNewItemIfNotSetTheItemWillUseTheOldItemSPriority)
            AddParameter(Params.DeferUntil, DataType.datetime, ParamDirection.In,
             IboResources.clsWorkQueuesActions_CopyToQueueAction_TheDateTimeToDeferTheNewItemUntilIfNotSetCreatesTheItemUndeferred)

            AddParameter(Params.NewItemID, DataType.text, ParamDirection.Out,
             IboResources.clsWorkQueuesActions_CopyToQueueAction_TheIDOfTheNewItemWithinTheTargetQueue)
        End Sub

        ''' <summary>
        ''' Get the endpoint text for this action.
        ''' </summary>
        ''' <returns>The endpoint as a String.</returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.clsWorkQueuesActions_CopyToQueueAction_TheSourceItemWillBeUnlockedAndMarkedCompleteTheNewItemWillBeCreatedInTheNamedWo
        End Function

        ''' <summary>
        ''' Gets the collection of preconditions for this action.
        ''' </summary>
        ''' <returns>The set of preconditions which apply to this action.</returns>
        Public Overrides Function GetPreConditions() As Collection
            Return BuildCollection(
                IboResources.clsWorkQueuesActions_CopyToQueueAction_TheWorkItemMustExistInAWorkQueue,
                IboResources.clsWorkQueuesActions_CopyToQueueAction_TheItemMustBeLockedByTheCurrentSession,
                IboResources.clsWorkQueuesActions_CopyToQueueAction_TheTargetQueueMustExist
            )
        End Function

        ''' <summary>
        ''' Perform the action.
        ''' </summary>
        ''' <param name="process">A reference to the process making the call, or
        ''' Nothing if unknown.</param>
        ''' <param name="session">The session under which the call is being made.
        ''' </param>
        ''' <param name="scopestage">The stage used to resolve the scope.</param>
        ''' <param name="sErr">On return, an error message if unsuccessful</param>
        ''' <returns>True if successful</returns>
        Public Overrides Function [Do](
         ByVal process As clsProcess,
         ByVal session As clsSession,
         ByVal scopestage As clsProcessStage,
         ByRef sErr As String) As Boolean

            Dim itemId As Guid = Nothing
            Dim queueName As String = Nothing
            Dim tagMask As String = Nothing
            Dim status As String = Nothing
            Dim priority As Integer = -1
            Dim defer As Date = Date.MinValue
            For Each arg As clsArgument In Inputs
                If arg.Value.IsNull Then Continue For
                Select Case arg.Name
                    Case "Item ID" : itemId = CType(arg.Value, Guid)
                    Case "Queue Name" : queueName = CStr(arg.Value)
                    Case "Tag Mask" : tagMask = CStr(arg.Value)
                    Case "Status" : status = CStr(arg.Value)
                    Case "Priority" : priority = CInt(arg.Value)
                    Case "Defer Until" : defer = arg.Value.GetValueAsUTCDateTime()
                End Select
            Next

            ' Only two mandatory parameters
            If String.IsNullOrWhiteSpace(queueName) Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_NoQueueNameSpecified)
            If itemId = Nothing Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_CopyToQueueAction_NoWorkItemSpecified)

            ' Check we have the item locked
            If Not LockedItems.Contains(itemId) Then Return SendError(sErr,
             IboResources.clsWorkQueuesActions_CopyToQueueAction_TheSpecifiedItemWasNotLockedByThisBusinessObject)

            Try
                ' If successfully copied, unlock the original work item (it will have
                ' been marked complete by the operation) and return the new item ID
                Dim newId As Guid = gSv.CopyWorkItem(
                    itemId, queueName, session.ID, defer, priority, tagMask, status)
                LockedItems.Remove(itemId)
                AddOutput(_T(Params.NewItemID), newId.ToString())

                Return True

            Catch ex As Exception
                Return SendError(sErr,
                 IboResources.clsWorkQueuesActions_CopyToQueueAction_ErrorWhileCopyingWorkItem0, ex.Message)

            End Try

        End Function

    End Class


    ''' <summary>
    ''' Class to encapsulate the work queue report parameters
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp")>
    Public Class ReportParams

        ' Queue Name
        <DataMember>
        Public Name As String

        ' Data boundaries
        <DataMember>
        Private mFinishStartDate As Date, mFinishEndDate As Date
        <DataMember>
        Private mAddedStartDate As Date, mAddedEndDate As Date

        ' Item data filtering - private to enforce non-null at interface level
        <DataMember>
        Private mResourceNames As List(Of String)
        <DataMember>
        Private mTags As clsTagMask

        ' Item state filtering
        <DataMember>
        Public Unworked As Boolean
        <DataMember>
        Public Deferred As Boolean
        <DataMember>
        Public Completed As Boolean
        <DataMember>
        Public Exceptioned As Boolean

        ' Treat attempts separately
        <DataMember>
        Public IsEachAttemptSeparate As Boolean

        ''' <summary>
        ''' Empty constructor - primarily here for the unit tests to be able to set
        ''' some parameters without an argument list
        ''' </summary>
        Friend Sub New()
            ' Set some defaults
            FinishStartDate = Nothing
            FinishEndDate = Nothing
            AddedStartDate = Nothing
            AddedEndDate = Nothing

            mResourceNames = New List(Of String)
            mTags = New clsTagMask()

            Unworked = False
            Deferred = False
            Completed = False
            Exceptioned = False

            IsEachAttemptSeparate = False

        End Sub

        ''' <summary>Case 
        ''' Createsa  new report parameters object initialised with values from the
        ''' given argument list.
        ''' </summary>
        ''' <param name="args">The list of arguments from where the report parameters
        ''' should be drawn.</param>
        Public Sub New(ByVal args As clsArgumentList)

            Me.New()

            ' Parse the arguments
            For Each arg As clsArgument In args
                Dim val As clsProcessValue = arg.Value
                If val.IsNull Then Continue For

                Select Case arg.Name

                    Case "Queue Name" : Name = CStr(val)

                    Case "Finished Start Date"
                        FinishStartDate = val.GetValueAsUTCDateTime()

                    Case "Finished End Date"
                        FinishEndDate = val.GetValueAsUTCDateTime()

                    Case "Loaded Start Date"
                        AddedStartDate = val.GetValueAsUTCDateTime()

                    Case "Loaded End Date"
                        AddedEndDate = val.GetValueAsUTCDateTime()

                    Case "Resource Names"
                        For Each res As String In CStr(val).Split(";"c)
                            res = res.Trim()
                            If res.Length > 0 Then
                                mResourceNames.Add(res)
                            End If
                        Next

                    Case "Tags" : mTags.ApplyTags(CStr(val))
                    Case "Include unworked items?" : Unworked = CBool(val)
                    Case "Include deferred items?" : Deferred = CBool(val)
                    Case "Include completed items?" : Completed = CBool(val)
                    Case "Include exception items?" : Exceptioned = CBool(val)

                    Case "Treat each attempt separately?"
                        IsEachAttemptSeparate = CBool(val)

                End Select
            Next

            If String.IsNullOrWhiteSpace(Name) Then Throw New InvalidOperationException(IboResources.clsWorkQueuesActions_NoQueueNameSpecified)

            ' If no dates set... that's an error that is...
            If FinishStartDate = Nothing AndAlso AddedStartDate = Nothing Then _
             Throw New ArgumentException(IboResources.clsWorkQueuesActions_ReportParams_FinishedStartDateLoadedStartDate,
              IboResources.clsWorkQueuesActions_ReportParams_AtLeastOneStartDateValueMustBeProvided)

            ' Resolve the end date if it's not been set explicitly by the arguments
            FinishEndDate = ResolveEndDate(FinishStartDate, FinishEndDate)
            AddedEndDate = ResolveEndDate(AddedStartDate, AddedEndDate)

            ' Reduce these to null if nothing has been entered - a minor optimisation
            ' in preparation for potential serialization / deserialization with BPServer
            If mResourceNames.Count = 0 Then mResourceNames = Nothing
            If mTags.IsEmpty() Then mTags = Nothing

        End Sub


        ''' <summary>
        ''' Resolves the given end date in the following way :
        ''' <list>
        ''' <item>If the end date has a value, it just returns that value</item>
        ''' <item>If the end date is empty and the start date has a value, it returns
        ''' the end of the day that the start date falls on</item>
        ''' <item>If neither the end nor the start dates have a value, it returns
        ''' <see cref="DateTime.MaxValue"/></item>
        ''' </list>
        ''' </summary>
        ''' <param name="sd">The start date value</param>
        ''' <param name="ed">The end date value</param>
        ''' <returns>The resolved end date</returns>
        Private Function ResolveEndDate(ByVal sd As Date, ByVal ed As Date) As Date

            If ed <> Nothing Then Return ed
            ' Not set - set to the end of the day on which startDate falls
            ' (inclusive, so +1 day less one 'unit')

            If sd <> Nothing Then
                ' Awkwardly, there's no single value which you can use as an
                ' inclusive date - an 'sql' tick is longer than a 'normal' tick.
                ' So, in its place you have this monstrosity
                Dim dt As SqlDateTime = New SqlDateTime(sd.Date.AddDays(1))
                If dt.TimeTicks > 0 Then
                    dt = New SqlDateTime(dt.DayTicks, dt.TimeTicks - 1)
                Else
                    dt = New SqlDateTime(dt.DayTicks - 1, (SqlDateTime.SQLTicksPerHour * 24) - 1)
                End If
                Return CDate(dt)
            End If

            ' start date also not set - set to eternity
            Return Date.MaxValue
        End Function


#Region "Accessor properties for internally controlled objects"

        ''' <summary>
        ''' The list of resource names to filter the work queue items on.
        ''' </summary>
        Public ReadOnly Property ResourceNames() As IList(Of String)
            Get
                If mResourceNames Is Nothing Then
                    mResourceNames = New List(Of String)
                End If
                Return mResourceNames
            End Get
        End Property

        ''' <summary>
        ''' The tags with which the work queue items should be filtered.
        ''' </summary>
        Public ReadOnly Property Tags() As clsTagMask
            Get
                If mTags Is Nothing Then
                    mTags = New clsTagMask()
                End If
                Return mTags
            End Get
        End Property

        ''' <summary>
        ''' The date providing the lower boundary for an item's "finish" date.
        ''' </summary>
        Public Property FinishStartDate() As Date
            Get
                Return mFinishStartDate
            End Get
            Set(ByVal value As Date)
                mFinishStartDate = value
            End Set
        End Property

        ''' <summary>
        ''' The date providing the upper boundary for an item's "finish" date.
        ''' </summary>
        Public Property FinishEndDate() As Date
            Get
                Return ResolveEndDate(mFinishStartDate, mFinishEndDate)
            End Get
            Set(ByVal value As Date)
                mFinishEndDate = value
            End Set
        End Property

        ''' <summary>
        ''' The date providing the lower boundary for an item's "loaded/added" date.
        ''' </summary>
        Public Property AddedStartDate() As Date
            Get
                Return mAddedStartDate
            End Get
            Set(ByVal value As Date)
                mAddedStartDate = value
            End Set
        End Property

        ''' <summary>
        ''' The date providing the upper boundary for an item's "loaded/added" date.
        ''' </summary>
        Public Property AddedEndDate() As Date
            Get
                Return ResolveEndDate(mAddedStartDate, mAddedEndDate)
            End Get
            Set(ByVal value As Date)
                mAddedEndDate = value
            End Set
        End Property

#End Region

    End Class

    ''' <summary>
    ''' The output data object for the work queue report
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp")>
    Public Class ReportData

        ' The number of items matched in the report
        <DataMember>
        Public Count As Integer
        ' The total work time of the items matched
        <DataMember>
        Public TotalTime As TimeSpan
        ' The shortest work time of the items matched
        <DataMember>
        Public LeastTime As TimeSpan
        ' The longest work time of the items matched
        <DataMember>
        Public MostTime As TimeSpan
        ' The median work time of the items matched
        <DataMember>
        Public MedianTime As TimeSpan
        ' The mean work time of the items matched
        <DataMember>
        Public MeanTime As TimeSpan

        ' The (distinct) IDs of the items matched
        <DataMember>
        Private mItems As List(Of Guid)

        ''' <summary>
        ''' The collection of Item IDs of all the items matched in the report.
        ''' </summary>
        Public ReadOnly Property Items() As ICollection(Of Guid)
            Get
                If mItems Is Nothing Then mItems = New List(Of Guid)
                Return mItems
            End Get
        End Property

    End Class

End Class
