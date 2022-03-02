Imports System.Linq
Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models

Namespace Processes

    <Serializable, DataContract([Namespace]:="bp"), KnownType(NameOf(ProcessSchema.GetAllKnownTypes))>
    Public Class ProcessSchema

        Friend Shared Function GetAllKnownTypes() As IEnumerable(Of Type)
            Return clsProcessStage.GetAllKnownTypes()
        End Function

        <DataMember>
        Private mIsEditable As Boolean
        <DataMember>
        Private mStages As List(Of clsProcessStage) = New List(Of clsProcessStage)
        <DataMember>
        Private mSubSheets As List(Of clsProcessSubSheet) = New List(Of clsProcessSubSheet)
        <DataMember>
        Private mVersion As String
        <DataMember>
        Private mProcessType As DiagramType

        Public Sub New(editable As Boolean, version As String, type As DiagramType)
            mIsEditable = editable
            mVersion = version
            mProcessType = type
        End Sub

        Public Property CreatedBy As String
        Public Property CreatedDate As DateTime
        Public Property Description As String
        Public Property EndPoint As String
        Public Property Id As Guid
        Public Property ModifiedBy As String
        Public Property ModifiedDate As DateTime
        Public Property Name As String

        Public ReadOnly Property IsEditable As Boolean
            Get
                Return mIsEditable
            End Get
        End Property

        Public ReadOnly Property ProcessType As DiagramType
            Get
                Return mProcessType
            End Get
        End Property

        Public ReadOnly Property Stages As List(Of clsProcessStage)
            Get
                Return mStages
            End Get
        End Property

        Public ReadOnly Property SubSheets As List(Of clsProcessSubSheet)
            Get
                Return mSubSheets
            End Get
        End Property

        Public ReadOnly Property Version As String
            Get
                Return mVersion
            End Get
        End Property

        Public Function GetStageIndex(stageId As Guid) As Integer
            Return Stages.FindIndex(Function(s) stageId = s.GetStageID())
        End Function

        Public Function GetStageById(stageId As Guid) As clsProcessStage
            Return Stages.FirstOrDefault(Function(s) stageId = s.GetStageID())
        End Function

        Public Function GetStageByIndex(index As Integer) As clsProcessStage
            If index < 0 OrElse index >= Stages.Count Then Return Nothing
            Return Stages(index)
        End Function

        Public Function GetStageByName(name As String) As clsProcessStage
            Return Stages.FirstOrDefault(Function(s) s.Name = name)
        End Function

        Public Function GetNextStage(ByVal currentStageId As Guid, ByVal linkType As LinkType) As Guid

            Dim stage = GetStageById(currentStageId)
            If stage Is Nothing Then
                Throw New ArgumentException(My.Resources.Resources.clsProcess_CouldNotFindStageWithSuppliedID, currentStageId.ToString())
            End If

            If stage.StageType = StageTypes.Decision Then
                If linkType = LinkType.OnTrue Then
                    Return CType(stage, clsDecisionStage).OnTrue
                ElseIf linkType = LinkType.OnFalse Then
                    Return CType(stage, clsDecisionStage).OnFalse
                End If
            ElseIf linkType = LinkType.OnSuccess Then
                Return TryCast(stage, clsLinkableStage).OnSuccess
            End If

            Return Guid.Empty

        End Function

        Public Function UpdateNextStage(ByRef runStageID As Guid, ByVal linkType As LinkType, ByRef errorResult As String) As Boolean
            Dim currentStage As clsProcessStage = GetStageById(runStageID)
            If currentStage Is Nothing Then
                errorResult = String.Format(My.Resources.Resources.clsProcess_CouldNotFindCurrentRunstageFromId0, runStageID.ToString)
                Return False
            End If

            Dim subsheetName = currentStage.GetSubSheetName()
            subsheetName = CType(IIf(subsheetName = clsProcess.MainPageName, My.Resources.Resources.clsProcess_DefaultMainPage, subsheetName), String)

            Dim errorMessage As String =
                String.Format(My.Resources.Resources.clsProcess_FailedToFindStageLinkedFromStage0OnPage1, currentStage.Name, subsheetName)

            Try
                Dim tempNextStage As Guid = GetNextStage(runStageID, linkType)
                If tempNextStage.Equals(Guid.Empty) Then
                    errorResult = errorMessage
                    Return False
                Else
                    runStageID = tempNextStage
                    Return True
                End If
            Catch ex As Exception
                errorResult = String.Format(My.Resources.Resources.clsProcess_0ErrorMessageWas1, errorMessage, ex.Message)
                Return False
            End Try
        End Function

        Public Function GetSubSheetStartStage(ByVal subsheetId As Guid) As Guid
            Return If(GetStageByTypeAndSubSheet(StageTypes.Start, subsheetId)?.Id, Guid.Empty)
        End Function

        Public Function GetSubSheetEndStage(ByVal subsheetId As Guid) As Guid
            Return If(GetStageByTypeAndSubSheet(StageTypes.End, subsheetId)?.Id, Guid.Empty)
        End Function

        Public Function GetCollectionStage(name As String, scopeStg As clsProcessStage, ByRef outOfScope As Boolean) As clsCollectionStage
            Dim stageName = If(name.Contains("."c), clsCollection.SplitPath(name).Item1, name)
            Return TryCast(GetDataStage(stageName, scopeStg, outOfScope), clsCollectionStage)
        End Function

        Public Function GetGroupStages(ByVal groupID As Guid) As List(Of clsGroupStage)
            Return GetStages(Of clsGroupStage)().Where(Function(s) s.GetGroupID = groupID).ToList()
        End Function

        Public Function GetStages(Of StageType As clsProcessStage)() As IEnumerable(Of StageType)
            Return Stages.Where(Function(s) TypeOf s Is StageType).Cast(Of StageType)
        End Function

        Public Function GetStages(Of StageType As clsProcessStage)(ByVal subsheetId As Guid) As IEnumerable(Of StageType)
            Return GetStages(Of StageType)().Where(Function(s) s.GetSubSheetID() = subsheetId)
        End Function

        Public Function GetDataStage(ByVal name As String, ByVal objScopeStage As clsProcessStage, ByRef outOfScope As Boolean) As clsDataStage
            outOfScope = False

            Dim stages = GetDataAndCollectionStagesByName(name).ToList()

            'If scope checking was requested, look only at stages that
            'are in scope first...
            If objScopeStage IsNot Nothing Then
                Dim dataStageInScope = stages.FirstOrDefault(Function(s) s.IsInScope(objScopeStage))
                If dataStageInScope IsNot Nothing Then Return dataStageInScope
            End If

            'Now look at all stages. If scope checking was requested and
            'we got here, any stage we find must be out of scope...
            If objScopeStage IsNot Nothing Then outOfScope = True

            Return stages.FirstOrDefault()
        End Function

        Public Function GetGroupStage(Of T As clsGroupStage)(ByVal stage As clsGroupStage) As T
            Dim groupId = stage.GetGroupID()
            If groupId.Equals(Guid.Empty) Then Return Nothing

            Dim stageId = stage.GetStageID()
            Return GetStages(Of T)().FirstOrDefault(Function(s) s.GetStageID <> stageId AndAlso groupId.Equals(s.GetGroupID()))
        End Function

        Public Function GetDataAndCollectionStageByName(name As String) As clsDataStage
            Return GetDataAndCollectionStagesByName(name).FirstOrDefault()
        End Function

        Private Function GetDataAndCollectionStagesByName(name As String) As IEnumerable(Of clsDataStage)
            Return Stages.Where(Function(s) s.Name = name And (s.StageType = StageTypes.Data Or s.StageType = StageTypes.Collection)).OfType(Of clsDataStage)
        End Function

        Public Function GetStageByTypeAndSubSheet(ByVal stageType As StageTypes, ByVal subSheetId As Guid) As clsProcessStage
            Return GetStagesByTypeAndSubSheet(stageType, subSheetId).FirstOrDefault()
        End Function

        Public Function GetStagesByTypeAndSubSheet(ByVal stageType As StageTypes, ByVal subSheetId As Guid) As IEnumerable(Of clsProcessStage)
            Return Stages.Where(Function(s) s.StageType = stageType AndAlso s.IsOnSubSheet(subSheetId))
        End Function

        Friend Function GetStagesByIdAndSubSheet(ByVal stageId As Guid, ByVal subSheetId As Guid) As clsProcessStage
            Return Stages.Where(Function(s) s.GetStageID = stageId AndAlso s.IsOnSubSheet(subSheetId)).FirstOrDefault
        End Function

        Public Function GetStages(ByVal type As StageTypes) As List(Of clsProcessStage)
            If type = StageTypes.Undefined Then Return Stages
            Return Stages.Where(Function(s) (s.StageType() And type) <> 0).ToList()
        End Function

        Public Function GetConflictingDataStages(stage As clsDataStage) As clsDataStage
            Return Stages.OfType(Of clsDataStage).FirstOrDefault(Function(s) s.GetStageID() <> stage.GetStageID() AndAlso
                                                                    s.Name = stage.Name AndAlso
                                                                    (stage.IsInScope(s) OrElse s.IsInScope(stage)))
        End Function

        Public Function GetUniqueStageID(ByVal stype As StageTypes) As String
            Select Case stype
                Case StageTypes.Action
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Action)
                Case StageTypes.Anchor
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Anchor)
                Case StageTypes.Block
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Block)
                Case StageTypes.Calculation
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Calc)
                Case StageTypes.ChoiceEnd
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Otherwise)
                Case StageTypes.ChoiceStart
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Choice)
                Case StageTypes.Collection
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Coll)
                Case StageTypes.Data
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Data)
                Case StageTypes.Decision
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Decision)
                Case StageTypes.End
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_End)
                Case StageTypes.LoopEnd
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Loop_End)
                Case StageTypes.LoopStart
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Loop_Start)
                Case StageTypes.Navigate
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Navigate)
                Case StageTypes.Note
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Note)
                Case StageTypes.Process
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Process)
                Case StageTypes.Read
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Reader)
                Case StageTypes.Skill
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Skill)
                Case StageTypes.Start
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Start)
                Case StageTypes.SubSheet
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Page)
                Case StageTypes.WaitEnd
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Time_Out)
                Case StageTypes.WaitStart
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Wait)
                Case StageTypes.Write
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Writer)
                Case StageTypes.Alert
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Alert)
                Case StageTypes.MultipleCalculation
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Multi)
                Case StageTypes.Resume
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Resume)
                Case StageTypes.Recover
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Recover)
                Case StageTypes.Exception
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Exception)
                Case StageTypes.Code
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Code)
                Case Else
                    Return GetUniqueStageID(My.Resources.Resources.clsProcess_GetUniqueStageId_Stage)
            End Select
        End Function

        Private Function GetUniqueStageID(ByVal stub As String) As String
            Dim sequence = 1

            Do
                Dim name = stub & sequence

                If GetStageByName(name) Is Nothing Then _
                    Return name

                sequence = sequence + 1
            Loop
        End Function

        Public Function GetSubsheetName(sheetId As Guid) As String
            Return If(SubSheets.FirstOrDefault(Function(s) s.ID = sheetId)?.Name, "")
        End Function

        Public Function GetSubSheetIndex(sheetId As Guid) As Integer
            Return SubSheets.FindIndex(Function(s) s.ID.Equals(sheetId))
        End Function

        Public Function GetSubSheetByID(id As Guid) As clsProcessSubSheet
            Return SubSheets.FirstOrDefault(Function(s) s.ID.Equals(id))
        End Function

        Public Function GetSubSheetID(name As String) As Guid
            Return If(SubSheets.FirstOrDefault(Function(s) s.Name = name)?.ID, Guid.Empty)
        End Function

        Public Function GetSubSheetIDWithSafeName(ByVal name As String) As Guid
            Dim safeName = clsProcess.GetSafeName(name)
            Return If(SubSheets.FirstOrDefault(Function(s) clsProcess.GetSafeName(s.Name) = safeName)?.ID, Guid.Empty)
        End Function

        Public Function GetMainPage() As clsProcessSubSheet
            Return GetPageByType(SubsheetType.MainPage)
        End Function

        Public Function GetCleanUpPage() As clsProcessSubSheet
            Return GetPageByType(SubsheetType.CleanUp)
        End Function

        Private Function GetPageByType(type As SubsheetType) As clsProcessSubSheet
            Return SubSheets.FirstOrDefault(Function(s) s.SheetType = type)
        End Function

        Public Function GetNormalSheets() As IEnumerable(Of clsProcessSubSheet)
            Return GetReadOnly.ICollection(SubSheets.Where(Function(s) s.IsNormal).ToList())
        End Function

        Public Function HasExtraSheets() As Boolean
            Return SubSheets.Any(Function(s) s.IsNormal)
        End Function

        Public Function RenameSubSheet(id As Guid, newName As String, ByRef errorMessage As String) As Boolean
            'Rename the SubSheetInfo stage on the subsheet...
            For Each stage In GetStages(StageTypes.SubSheetInfo).
                                            Where(Function(s) id.Equals(s.GetSubSheetID()))
                stage.Name = newName
            Next

            'Rename the subsheet itself...
            Dim subsheet = GetSubSheetByID(id)
            If subsheet IsNot Nothing Then
                subsheet.Name = newName
                Return True
            End If

            errorMessage = My.Resources.Resources.clsProcess_PageNotFound
            Return False
        End Function

        Public Sub SetSubSheetOrder(subSheetIDs As ICollection(Of Guid))
            If subSheetIDs.Count <> SubSheets.Count Then Throw New BluePrismException(
         My.Resources.Resources.clsProcess_WrongNumberOfSheetsProcessHas0GivenCollectionHas1,
         SubSheets.Count, subSheetIDs.Count)

            If SubSheets.Count = 0 Then Return

            ' We add the non-normal ones first, to fix any processes affected by
            ' bug 5364 - ie. 'normal' sheets nestling in between 'special' ones.

            Dim newSheets As New List(Of clsProcessSubSheet)(SubSheets.Where(Function(s) Not s.IsNormal))

            ' Then we go through the 'normal' sheets and add them in the order defined
            ' in the given collection
            For Each id As Guid In subSheetIDs
                Dim subsheet As clsProcessSubSheet = GetSubSheetByID(id)
                If subsheet Is Nothing Then _
             Throw New NoSuchElementException(My.Resources.Resources.clsProcess_MissingSubsheetWithID0, id)
                ' We don't add the non-normal ones, they've already been added.
                If subsheet.IsNormal Then newSheets.Add(subsheet)
            Next

            SubSheets.Clear()
            SubSheets.AddRange(newSheets)
        End Sub

        Public Function ReadDataItemsFromParameters(ByVal parameters As List(Of clsProcessParameter), ByVal stage As clsProcessStage,
                                                ByRef arguments As clsArgumentList, ByRef errorMessage As String, allowScopeChange As Boolean,
                                                isOutput As Boolean) As Boolean

            If arguments Is Nothing Then arguments = New clsArgumentList()

            For Each mapped In parameters
                Dim val As clsProcessValue = Nothing
                Dim param = stage.GetParameter(mapped.Name, mapped.Direction)

                'If the parameter is not defined in the stage then this generally means that
                'the signature of the thing being called has been altered, but the calling
                'stage does not match. So as per Bug#5104 we just pass a null value.
                'i.e.leave val = Nothing. As such we can just Continue For, since the code
                'below checks if val is nothing and we just dont add it to the Argument list.
                If param Is Nothing Then Continue For

                If (isOutput AndAlso Not param.Direction = ParamDirection.Out) Then Continue For
                If (Not isOutput AndAlso Not param.Direction = ParamDirection.In) Then Continue For

                ' If the map type is set to be None, work out the map type based on
                ' whether it is an Output or Input Paramater
                Dim paramMapType = param.GetMapType()
                If paramMapType = MapType.None Then
                    paramMapType = If(isOutput, MapType.Stage, MapType.Expr)
                End If

                'Get the Values
                Select Case paramMapType
                    Case MapType.Expr
                        'If no expression is specified, we won't pass the parameter at all. This would
                        'be the case for an optional parameter, for example.
                        Dim mappingInformation = param.GetMap()

                        If String.IsNullOrEmpty(mappingInformation) Then Continue For

                        If Not clsExpression.EvaluateExpression(mappingInformation, val, stage, False, Nothing, errorMessage) Then
                            Return False
                        End If

                        Try
                            val = val.CastInto(param.GetDataType())
                        Catch ex As Exception
                            errorMessage = String.Format(My.Resources.Resources.clsProcess_CanTConvertParameter0From1To23,
                                             param.FriendlyName,
                                             clsProcessDataTypes.GetFriendlyName(val.DataType),
                                             clsProcessDataTypes.GetFriendlyName(param.GetDataType()),
                                             ex.Message)
                        End Try

                    Case MapType.Stage
                        Dim outOfScope As Boolean
                        Dim colindex As Integer = param.GetMap().IndexOf(".")

                        If colindex <> -1 Then
                            Dim dataStage = GetDataStage(param.GetMap().Substring(0, colindex), stage, outOfScope)
                            If dataStage Is Nothing Then
                                errorMessage = String.Format(My.Resources.Resources.clsProcess_MissingStage0ForParameter, param.GetMap)
                                Return False
                            End If

                            Dim colstage As clsCollectionStage = TryCast(dataStage, clsCollectionStage)
                            If colstage Is Nothing Then
                                errorMessage = String.Format(My.Resources.Resources.clsProcess_0RefersToACollectionFieldButThatStageIsNotACollection, param.GetMap)
                                Return False
                            End If

                            If Not allowScopeChange AndAlso outOfScope Then
                                errorMessage = String.Format(My.Resources.Resources.clsProcess_CannotUse0ForParameterBecauseItIsOnAnotherPageAndHasBeenHidden, param.GetMap)
                                Return False
                            End If

                            If Not colstage.CollectionGetField(param.GetMap().Substring(colindex + 1), val, errorMessage) Then
                                errorMessage = String.Format(My.Resources.Resources.clsProcess_CannotRead01, param.GetMap, errorMessage)
                                Return False
                            End If
                        Else
                            Dim dataStage = GetDataStage(param.GetMap(), stage, outOfScope)
                            If dataStage Is Nothing Then
                                errorMessage = String.Format(My.Resources.Resources.clsProcess_MissingStage0ForParameter, param.GetMap)
                                Return False
                            End If

                            If Not allowScopeChange AndAlso outOfScope Then
                                errorMessage = String.Format(My.Resources.Resources.clsProcess_CannotUse0ForParameterBecauseItIsOnAnotherPageAndHasBeenHidden, param.GetMap)
                                Return False
                            End If

                            val = dataStage.GetValue()
                        End If

                    Case Else
                        errorMessage = String.Format(My.Resources.Resources.clsProcess_InvalidMapType0ForParameterInvalidProcess, param.GetMapType)
                        Return False
                End Select

                If val IsNot Nothing Then _
                    arguments.Add(New clsArgument(param.Name, val))

            Next

            Return True

        End Function
    End Class

End Namespace
