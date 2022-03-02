Imports BluePrism.Server.Domain.Models
Imports System.Runtime.Serialization

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsLoopStartStage
    ''' 
    ''' <summary>
    ''' The loop start stage represents the start point for a loop, this stage is
    ''' jumped to after the loop end stage is reached, when in a loop iteration.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsLoopStartStage
        Inherits clsGroupStage

        ''' <summary>
        ''' The type of loop represented by this stage; currently only "ForEach"
        ''' loops are supported.
        ''' </summary>
        <DataMember>
        Private msLoopType As String = "ForEach"

        Public Property LoopType As String
            Get
                Return msLoopType
            End Get
            Set(value As String)
                msLoopType = value
            End Set
        End Property

        ''' <summary>
        ''' The Loop Data. For looptype "ForEach", it is the collection name.
        ''' </summary>
        <DataMember>
        Private msLoopData As String = ""

        Public Property LoopData As String
            Get
                Return msLoopData
            End Get
            Set(value As String)
                msLoopData = value
            End Set
        End Property

        ''' <summary>
        ''' Gets the name of the collection that this loop stage is looping over.
        ''' </summary>
        ''' <remarks>There's a lot of handling of other 'loop types' in this class,
        ''' but as it stands, only one is implemented and lots of things depend on
        ''' that one being a 'ForEach' over a collection. We might as well make it
        ''' easy to use in that circumstance rather than trying to be generic for
        ''' some possibly never attained future.
        ''' </remarks>
        Public ReadOnly Property CollectionName() As String
            Get
                Return LoopData
            End Get
        End Property

        ''' <summary>
        ''' Gets the collection stage associated with this loop start stage, or null
        ''' if this stage is not within a process or the collection stage is not
        ''' reachable from this stage, or if no collection name is associated with
        ''' this stage.
        ''' </summary>
        Public ReadOnly Property CollectionStage() As clsCollectionStage
            Get
                If mParent Is Nothing Then Return Nothing
                Return mParent.GetCollectionStage(CollectionName, Me, False)
            End Get
        End Property

        ''' <summary>
        ''' Gets the ID of the collection stage associated with this loop start
        ''' stage. This will return <see cref="Guid.Empty"/> if either: <list>
        ''' <item>This stage is not within a process -or-</item>
        ''' <item>No collection name is set on this stage -or-</item>
        ''' <item>The collection stage is not reachable from this stage</item>
        ''' </list>
        ''' </summary>
        Public ReadOnly Property CollectionStageId() As Guid?
            Get
                Return CollectionStage?.Id
            End Get
        End Property

        ''' <summary>
        ''' Creates a new instance of the clsLoopStartStage class and sets its parent.
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of an loop start stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsLoopStartStage(mParent)
        End Function

        ''' <summary>
        ''' Creates a deep copy of the loop start stage.
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function Clone() As clsProcessStage
            Dim copy As clsLoopStartStage = CType(MyBase.Clone, clsLoopStartStage)
            copy.LoopType = Me.LoopType
            copy.LoopData = Me.LoopData
            Return copy
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.LoopStart
            End Get
        End Property

        ''' <summary>
        ''' Returns items referred to by this stage, so just the collection name we
        ''' are looping over.
        ''' </summary>
        ''' <param name="inclInternal">Indicates internal references required</param>
        ''' <returns>List of dependencies</returns>
        Public Overrides Function GetDependencies(inclInternal As Boolean) As List(Of clsProcessDependency)
            Dim deps As List(Of clsProcessDependency) = MyBase.GetDependencies(inclInternal)

            If inclInternal Then
                If CollectionName <> String.Empty Then
                    Dim outOfScope As Boolean
                    Dim stage = mParent.GetDataStage(CollectionName, Me, outOfScope)
                    If Not outOfScope AndAlso stage IsNot Nothing Then _
                        deps.Add(New clsProcessDataItemDependency(stage))
                End If
            End If

            Return deps
        End Function

        ''' <summary>
        ''' Get the target collection for this loop stage. Calculated by looking at
        ''' the loop data, which may contain nested field names as well as the top
        ''' level collection name.
        ''' </summary>
        ''' <returns>The clsCollection to loop over. This can be Nothing, which
        ''' indicates an attempt to loop over a collection with no data. This is
        ''' an allowed thing to do - it's a loop with no iterations.</returns>
        ''' <remarks>Throws an ApplicationException if something goes wrong.
        ''' </remarks>
        Friend Function GetTargetCollection() As clsCollection

            Dim colname As String = CollectionName
            Dim subs As String = Nothing

            ' Can't do a loop without a collection being set.
            If String.IsNullOrEmpty(colname) Then
                Throw New InvalidArgumentException(My.Resources.Resources.clsLoopStartStage_AForEachLoopMustSpecifyACollection)
            End If

            Dim index As Integer = colname.IndexOf("."c)
            If index <> -1 Then
                subs = colname.Substring(index + 1)
                colname = colname.Substring(0, index)
            End If

            Dim outOfScope As Boolean
            Dim colStage As clsCollectionStage = CType(mParent.GetDataStage(colname, Me, outOfScope), clsCollectionStage)
            If colStage Is Nothing Then
                Throw New InvalidArgumentException(My.Resources.Resources.clsLoopStartStage_InvalidCollectionSuppliedForForEachLoop)
            End If
            If outOfScope Then
                Throw New InvalidStateException(My.Resources.Resources.clsLoopStartStage_LoopCollectionIsNotAccessibleBecauseItLiesOnADifferentPageAndIsMarkedAsHidden)
            End If
            Dim col As clsCollection = colStage.GetValue().Collection
            If col Is Nothing Then
                Return Nothing
            End If

            While subs IsNot Nothing
                Dim thisname As String = subs
                index = subs.IndexOf("."c)
                If index <> -1 Then
                    subs = thisname.Substring(index + 1)
                    thisname = thisname.Substring(0, index)
                Else
                    subs = Nothing
                End If
                'Note that the following also throws ApplicationExceptions...
                Dim val As clsProcessValue = col.GetField(thisname)
                If val.DataType <> DataType.collection Then
                    Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsLoopStartStage_NestedValue0IsNotACollection, thisname))
                End If
                col = val.Collection

            End While
            Return col

        End Function


        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            LoopStartPrologue(logger)

            Dim sErr As String = Nothing

            If LoopType = "" Then
                Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsLoopStartStage_LoopTypeNotConfiguredOnStage0PleaseVisitTheStageAndSetTheLoopType, GetName()))
            End If
            If LoopType <> "ForEach" Then
                Return New StageResult(False, "Internal", My.Resources.Resources.clsLoopStartStage_UnsupportedLoopType)
            End If

            'Get the collection we're looping over...
            Dim col As clsCollection
            Try
                col = GetTargetCollection()
            Catch ex As Exception
                Return New StageResult(False, "Internal", ex.Message)
            End Try

            If col IsNot Nothing AndAlso col.StartIterate() Then

                If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then
                    Return New StageResult(False, "Internal", sErr)
                End If

                Dim sCount As String = String.Format(My.Resources.Resources.clsLoopStartStage_1Of0, col.Count.ToString())
                LoopStartEpilogue(logger, sCount)

            Else
                'The collection is empty, so we will skip
                'straight to the end of the loop.
                Dim objStage As clsProcessStage
                objStage = mParent.GetLoopEnd(Me)
                If objStage Is Nothing Then
                    Return New StageResult(False, "Internal", My.Resources.Resources.clsLoopStartStage_CanTFindEndStageForLoop)
                End If
                gRunStageID = objStage.GetStageID()
                If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then
                    Return New StageResult(False, "Internal", sErr)
                End If
            End If

            Return New StageResult(True)

        End Function

        Private Sub LoopStartPrologue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.LoopStartPrologue(info, Me)
        End Sub

        Private Sub LoopStartEpilogue(logger As CompoundLoggingEngine, ByVal sCount As String)
            Dim info = GetLogInfo()
            logger.LoopStartEpiLogue(info, Me, sCount)
        End Sub

        Public Overrides Sub FromXML(ByVal e2 As System.Xml.XmlElement)
            MyBase.FromXML(e2)
            For Each e3 As Xml.XmlElement In e2.ChildNodes
                Select Case e3.Name
                    Case "looptype"
                        LoopType = e3.InnerText
                    Case "loopdata"
                        LoopData = e3.InnerText
                End Select
            Next
        End Sub

        Public Overrides Sub ToXml(ByVal ParentDocument As System.Xml.XmlDocument, ByVal StageElement As System.Xml.XmlElement, ByVal bSelectionOnly As Boolean)
            MyBase.ToXml(ParentDocument, StageElement, bSelectionOnly)

            Dim e2 As Xml.XmlElement
            e2 = ParentDocument.CreateElement("looptype")
            e2.AppendChild(ParentDocument.CreateTextNode(Me.LoopType))
            StageElement.AppendChild(e2)
            e2 = ParentDocument.CreateElement("loopdata")
            e2.AppendChild(ParentDocument.CreateTextNode(Me.LoopData))
            StageElement.AppendChild(e2)
        End Sub

        Public Overrides Function CheckForErrors(ByVal bAttemptRepair As Boolean, ByVal SkipObjects As Boolean) As ValidationErrorList
            Dim errors As ValidationErrorList = MyBase.CheckForErrors(bAttemptRepair, SkipObjects)

            'Check group integrity
            If mgGroupID.Equals(Guid.Empty) Then
                errors.Add(New ValidateProcessResult(Me, 105))
            Else
                Dim GroupEnd As clsGroupStage = mParent.GetLoopEnd(Me)
                If GroupEnd Is Nothing Then
                    errors.Add(New ValidateProcessResult(Me, 107))
                Else
                    'Check that pages of loopstart and loopend match
                    'This must only be done inside else clause because
                    'otherwise would get null reference exception!
                    If Not GroupEnd.GetSubSheetID().Equals(Me.GetSubSheetID) Then
                        errors.Add(New ValidateProcessResult(Me, 108))
                    End If
                End If
            End If
            'Check that the loop type is supported
            Select Case LoopType
                Case "ForEach"
                    'OK - do nothing
                Case ""
                    errors.Add(New ValidateProcessResult(Me, "frmStagePropertiesLoop.htm", 116))
                Case Else
                    errors.Add(New ValidateProcessResult(Me, "frmStagePropertiesLoop.htm", 117))
            End Select
            'Check the target collection is appropriate
            Dim outOfScope As Boolean
            Dim colname As String = LoopData
            If colname IsNot Nothing Then
                Dim index As Integer = colname.IndexOf("."c)
                If index <> -1 Then
                    colname = colname.Substring(0, index)
                End If
            End If
            Dim colStage As clsCollectionStage = TryCast(mParent.GetDataStage(colname, Me, outOfScope), clsCollectionStage)
            If colStage Is Nothing Then
                If LoopData Is Nothing OrElse LoopData.Length = 0 Then
                    errors.Add(New ValidateProcessResult(Me, "frmStagePropertiesLoop.htm", 118))
                Else
                    errors.Add(New ValidateProcessResult(Me, "frmStagePropertiesLoop.htm", 119, colname))
                End If
            Else
                If outOfScope Then
                    errors.Add(New ValidateProcessResult(Me, "frmStagePropertiesLoop.htm", 120))
                End If

            End If

            Return errors
        End Function

    End Class

End Namespace
