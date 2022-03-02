#If UNITTESTS Then

Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore.Processes

Imports NUnit.Framework
Imports FluentAssertions
Imports BluePrism.Server.Domain.Models

<TestFixture()>
Public Class ProcessSchemaTests

    <Test>
    Public Sub ProcessSchema_IsConstructed_Correctly()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)

        schema.Version.ShouldBeEquivalentTo("1.2")
        schema.IsEditable.ShouldBeEquivalentTo(True)
        schema.ProcessType.ShouldBeEquivalentTo(DiagramType.Process)
    End Sub

    <Test>
    Public Sub GetStageIndex_SingleMatch_ReturnsCorrectly()
        Dim id = Guid.NewGuid()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim stage = New clsActionStage(Nothing) With {.Id = id}
        schema.Stages.Add(stage)
        schema.GetStageIndex(id).ShouldBeEquivalentTo(0)
    End Sub

    <Test>
    Public Sub GetStageIndex_MultipleMatches_ReturnsFirstMatch()
        Dim id = Guid.NewGuid()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim firstStage = New clsActionStage(Nothing) With {.Id = id}
        Dim secondStage = New clsCalculationStage(Nothing) With {.Id = id}
        schema.Stages.Add(firstStage)
        schema.Stages.Add(secondStage)
        schema.GetStageIndex(id).ShouldBeEquivalentTo(0)
    End Sub

    <Test>
    Public Sub GetStageIndex_NoMatch_ReturnsDefault()
        Dim id = Guid.NewGuid()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetStageIndex(id).ShouldBeEquivalentTo(-1)
    End Sub

    <Test>
    Public Sub GetStageById_SingleMatch_ReturnsCorrectly()
        Dim id = Guid.NewGuid()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim stage = New clsActionStage(Nothing) With {.Id = id}
        schema.Stages.Add(stage)
        schema.GetStageById(id).ShouldBeEquivalentTo(stage)
    End Sub

    <Test>
    Public Sub GetStageById_MultipleMatches_ReturnsFirstMatch()
        Dim id = Guid.NewGuid()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim firstStage = New clsActionStage(Nothing) With {.Id = id}
        Dim secondStage = New clsCalculationStage(Nothing) With {.Id = id}
        schema.Stages.Add(firstStage)
        schema.Stages.Add(secondStage)
        schema.GetStageById(id).ShouldBeEquivalentTo(firstStage)
    End Sub

    <Test>
    Public Sub GetStageById_NoMatch_ReturnsDefault()
        Dim id = Guid.NewGuid()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetStageById(id).ShouldBeEquivalentTo(Nothing)
    End Sub

    <Test>
    Public Sub GetStageByIndex_HasMatch_ReturnsCorrectly()
        Dim id = Guid.NewGuid()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim firstStage = New clsActionStage(Nothing)
        Dim secondStage = New clsCalculationStage(Nothing)
        schema.Stages.Add(firstStage)
        schema.Stages.Add(secondStage)
        schema.GetStageByIndex(1).ShouldBeEquivalentTo(secondStage)
    End Sub

    <TestCase(-1)>
    <TestCase(2)>
    Public Sub GetStageByIndex_OutOfRange_ReturnsNothing(index As Integer)
        Dim id = Guid.NewGuid()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim firstStage = New clsActionStage(Nothing)
        Dim secondStage = New clsCalculationStage(Nothing)
        schema.Stages.Add(firstStage)
        schema.Stages.Add(secondStage)
        schema.GetStageByIndex(index).ShouldBeEquivalentTo(Nothing)
    End Sub

    <Test>
    Public Sub GetStageByName_NoMatch_ReturnsDefault()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetStageByName("Test stage").ShouldBeEquivalentTo(Nothing)
    End Sub

    <Test>
    Public Sub GetStageByName_SingleMatch_ReturnsCorrectly()
        Dim name = "Test stage"
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim stage = New clsActionStage(Nothing) With {.Name = name}
        schema.Stages.Add(stage)
        schema.GetStageByName(name).ShouldBeEquivalentTo(stage)
    End Sub

    <Test>
    Public Sub GetStageByName_MultipleMatches_ReturnsFirstMatch()
        Dim name = "Test stage"
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim firstStage = New clsActionStage(Nothing) With {.Name = name}
        Dim secondStage = New clsCalculationStage(Nothing) With {.Name = name}
        schema.Stages.Add(firstStage)
        schema.Stages.Add(secondStage)
        schema.GetStageByName(name).ShouldBeEquivalentTo(firstStage)
    End Sub

    <Test>
    Public Sub GetNextStage_CurrentStageNotFound_ThrowsException()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim firstStage = New clsActionStage(Nothing) With {.Id = Guid.NewGuid()}
        schema.Stages.Add(firstStage)
        Dim getNextStage As Action = Sub() schema.GetNextStage(Guid.NewGuid(), LinkType.OnSuccess)
        getNextStage.ShouldThrow(Of ArgumentException)
    End Sub

    <Test>
    Public Sub GetNextStage_CurrentStageIsDecision_LinkTypeTrue_CorrectResult()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim id = Guid.NewGuid()
        Dim onTrue = Guid.NewGuid()
        Dim firstStage = New clsDecisionStage(Nothing) With {.Id = id, .OnTrue = onTrue}
        schema.Stages.Add(firstStage)
        schema.GetNextStage(id, LinkType.OnTrue).ShouldBeEquivalentTo(onTrue)
    End Sub

    <Test>
    Public Sub GetNextStage_CurrentStageIsDecision_LinkTypeFalse_CorrectResult()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim id = Guid.NewGuid()
        Dim onFalse = Guid.NewGuid()
        Dim firstStage = New clsDecisionStage(Nothing) With {.Id = id, .OnFalse = onFalse}
        schema.Stages.Add(firstStage)
        schema.GetNextStage(id, LinkType.OnFalse).ShouldBeEquivalentTo(onFalse)
    End Sub

    <Test>
    Public Sub GetNextStage_CurrentStageIsDecision_LinkTypeOther_CorrectResult()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim id = Guid.NewGuid()
        Dim firstStage = New clsDecisionStage(Nothing) With {.Id = id, .OnTrue = Guid.NewGuid(), .OnFalse = Guid.NewGuid()}
        schema.Stages.Add(firstStage)
        schema.GetNextStage(id, LinkType.OnSuccess).ShouldBeEquivalentTo(Guid.Empty)
    End Sub

    <Test>
    Public Sub GetNextStage_CurrentStageIsNotDecision_LinkTypeSuccess_CorrectResult()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim id = Guid.NewGuid()
        Dim onsuccess = Guid.NewGuid()
        Dim firstStage = New clsActionStage(Nothing) With {.Id = id, .OnSuccess = onsuccess}
        schema.Stages.Add(firstStage)
        schema.GetNextStage(id, LinkType.OnSuccess).ShouldBeEquivalentTo(onsuccess)
    End Sub

    <Test>
    Public Sub GetNextStage_CurrentStageIsNotDecision_LinkTypeOther_CorrectResult()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim id = Guid.NewGuid()
        Dim onsuccess = Guid.NewGuid()
        Dim firstStage = New clsActionStage(Nothing) With {.Id = id, .OnSuccess = onsuccess}
        schema.Stages.Add(firstStage)
        schema.GetNextStage(id, LinkType.OnTrue).ShouldBeEquivalentTo(Guid.Empty)
    End Sub

    <Test>
    Public Sub UpdateNextStage_CurrentStageDoesNotExist_UnsuccessfulWithError()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim id = Guid.NewGuid()
        Dim errorMessage = ""
        schema.UpdateNextStage(id, LinkType.OnTrue, errorMessage).Should().BeFalse()
        errorMessage.Should().NotBeNullOrEmpty()
        id.ShouldBeEquivalentTo(id)
    End Sub

    <Test>
    Public Sub UpdateNextStage_NextStageIdEmpty_UnsuccessfulWithError()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim id = Guid.NewGuid()
        Dim firstStage = New clsActionStage(Nothing) With {.Id = id}
        schema.Stages.Add(firstStage)
        Dim errorMessage = ""
        schema.UpdateNextStage(id, LinkType.OnSuccess, errorMessage).Should().BeFalse()
        errorMessage.Should().NotBeNullOrEmpty()
        id.ShouldBeEquivalentTo(id)
    End Sub

    <Test>
    Public Sub UpdateNextStage_SetUpCorrectly_SuccessfulWithoutError()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim id = Guid.NewGuid()
        Dim onSuccess = Guid.NewGuid()
        Dim firstStage = New clsActionStage(Nothing) With {.Id = id, .OnSuccess = onSuccess}
        schema.Stages.Add(firstStage)
        Dim errorMessage = ""
        schema.UpdateNextStage(id, LinkType.OnSuccess, errorMessage).Should().BeTrue()
        errorMessage.Should().BeEmpty()
        id.ShouldBeEquivalentTo(onSuccess)
    End Sub

    <Test>
    Public Sub GetSubSheetStartStage_NoMatches_Empty()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetSubSheetStartStage(Guid.NewGuid()).ShouldBeEquivalentTo(Guid.Empty)
    End Sub

    <Test>
    Public Sub GetSubSheetStartStage_MultipleMatches_ReturnsFirstStartStage()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim firstStage = New clsStartStage(Nothing) With {.Id = Guid.NewGuid()}
        firstStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsStartStage(Nothing) With {.Id = Guid.NewGuid()}
        secondStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(secondStage)
        schema.GetSubSheetStartStage(subsheetId).ShouldBeEquivalentTo(firstStage.Id)
    End Sub

    <Test>
    Public Sub GetSubSheetStartStage_SingleMatch_CorrectStage()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim firstStage = New clsStartStage(Nothing) With {.Id = Guid.NewGuid()}
        firstStage.SetSubSheetID(Guid.NewGuid())
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsStartStage(Nothing) With {.Id = Guid.NewGuid()}
        Dim subsheetId = Guid.NewGuid()
        secondStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(secondStage)
        schema.GetSubSheetStartStage(subsheetId).ShouldBeEquivalentTo(secondStage.Id)
    End Sub

    <Test>
    Public Sub GetSubSheetEndStage_NoMatches_Empty()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetSubSheetEndStage(Guid.NewGuid()).ShouldBeEquivalentTo(Guid.Empty)
    End Sub

    <Test>
    Public Sub GetSubSheetEndStage_MultipleMatches_ReturnsFirstEndStage()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim firstStage = New clsEndStage(Nothing) With {.Id = Guid.NewGuid()}
        firstStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsEndStage(Nothing) With {.Id = Guid.NewGuid()}
        secondStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(secondStage)
        schema.GetSubSheetEndStage(subsheetId).ShouldBeEquivalentTo(firstStage.Id)
    End Sub

    <Test>
    Public Sub GetSubSheetEndStage_SingleMatch_CorrectStage()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim firstStage = New clsEndStage(Nothing) With {.Id = Guid.NewGuid()}
        firstStage.SetSubSheetID(Guid.NewGuid())
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsEndStage(Nothing) With {.Id = Guid.NewGuid()}
        Dim subsheetId = Guid.NewGuid()
        secondStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(secondStage)
        schema.GetSubSheetEndStage(subsheetId).ShouldBeEquivalentTo(secondStage.Id)
    End Sub

    <Test>
    Public Sub GetCollectionStage_NoMatches_Empty()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim outOfScope = False
        schema.GetCollectionStage("collection", New clsActionStage(Nothing), outOfScope).Should().BeNull()
        outOfScope.Should().BeTrue()
    End Sub

    <Test>
    Public Sub GetCollectionStage_MultipleMatches_ReturnsFirst()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim firstStage = New clsCollectionStage(Nothing) With {.Id = Guid.NewGuid(), .Name = "collection"}
        firstStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsEndStage(Nothing) With {.Id = Guid.NewGuid(), .Name = "collection"}
        secondStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(secondStage)
        Dim outOfScope = False
        schema.GetCollectionStage("collection.items", New clsActionStage(Nothing), outOfScope).ShouldBeEquivalentTo(firstStage)
        outOfScope.Should().BeTrue()
    End Sub

    <Test>
    Public Sub GetCollectionStage_SingleMatch_CorrectStage()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim firstStage = New clsCollectionStage(Nothing) With {.Id = Guid.NewGuid(), .Name = "collection"}
        firstStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsCollectionStage(Nothing) With {.Id = Guid.NewGuid(), .Name = "othercollection"}
        secondStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(secondStage)
        Dim outOfScope = False
        schema.GetCollectionStage("othercollection.name", New clsActionStage(Nothing), outOfScope).ShouldBeEquivalentTo(secondStage)
        outOfScope.Should().BeTrue()
    End Sub

    <Test>
    Public Sub GetGroupStages_NoMatches_Empty()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetGroupStages(Guid.NewGuid()).Should().BeEmpty()
    End Sub

    <Test>
    Public Sub GetGroupStages_MultipleMatches_ReturnsAll()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim groupId = Guid.NewGuid()
        Dim firstStage = New clsLoopStartStage(Nothing)
        firstStage.SetGroupID(groupId)
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsLoopEndStage(Nothing)
        secondStage.SetGroupID(groupId)
        schema.Stages.Add(secondStage)
        schema.GetGroupStages(groupId).ShouldBeEquivalentTo(New List(Of clsGroupStage) From {firstStage, secondStage})
    End Sub

    <Test>
    Public Sub GetStages_NoMatches_Empty()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetStages(Of clsActionStage).Should().BeEmpty()
    End Sub

    <Test>
    Public Sub GetStages_MultipleMatches_ReturnsAll()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim firstStage = New clsLoopStartStage(Nothing)
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsLoopEndStage(Nothing)
        schema.Stages.Add(secondStage)
        Dim thirdStage = New clsActionStage(Nothing)
        schema.Stages.Add(thirdStage)
        schema.GetStages(Of clsGroupStage).ShouldBeEquivalentTo(New List(Of clsGroupStage) From {firstStage, secondStage})
    End Sub

    <Test>
    Public Sub GetStagesWithSubsheetId_NoMatches_Empty()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetStages(Of clsActionStage)(Guid.NewGuid()).Should().BeEmpty()
    End Sub

    <Test>
    Public Sub GetStagesWithSubsheetId_HasMatches_ReturnsAll()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim firstStage = New clsLoopStartStage(Nothing)
        firstStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsLoopEndStage(Nothing)
        schema.Stages.Add(secondStage)
        Dim thirdStage = New clsActionStage(Nothing)
        thirdStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(thirdStage)
        schema.GetStages(Of clsGroupStage)(subsheetId).ShouldBeEquivalentTo(New List(Of clsGroupStage) From {firstStage})
    End Sub

    <Test>
    Public Sub GetDataStage_NoMatches_Empty()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim outOfScope = False

        schema.GetDataStage("name", Nothing, outOfScope).Should().BeNull()
        outOfScope.Should().BeFalse()
    End Sub

    <Test>
    Public Sub GetDataStage_HasMatch_ReturnsFirst()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim firstStage = New clsLoopStartStage(Nothing) With {.Name = "name"}
        firstStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsCollectionStage(Nothing) With {.Name = "name"}
        secondStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(secondStage)
        Dim thirdStage = New clsCollectionStage(Nothing) With {.Name = "name"}
        thirdStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(thirdStage)
        Dim outOfScope = False
        schema.GetDataStage("name", New clsActionStage(Nothing), outOfScope).ShouldBeEquivalentTo(secondStage)
        outOfScope.Should().BeTrue()
    End Sub

    <Test>
    Public Sub GetDataStage_HasMatchInScope_ReturnsFirst()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim firstStage = New clsCollectionStage(Nothing) With {.Name = "name"}
        firstStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(firstStage)
        Dim scopeStage = New clsActionStage(Nothing)
        scopeStage.SetSubSheetID(subsheetId)
        Dim outOfScope = False
        schema.GetDataStage("name", scopeStage, outOfScope).ShouldBeEquivalentTo(firstStage)
        outOfScope.Should().BeFalse()
    End Sub

    <Test>
    Public Sub GetGroupStage_NoMatches_Empty()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetGroupStage(Of clsChoiceStartStage)(New clsChoiceEndStage(Nothing)).Should().BeNull()
    End Sub

    <Test>
    Public Sub GetGroupStage_MultipleMatches_ReturnsFirst()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim groupId = Guid.NewGuid()
        Dim firstStage = New clsChoiceStartStage(Nothing)
        firstStage.SetGroupID(groupId)
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsChoiceStartStage(Nothing)
        secondStage.SetGroupID(groupId)
        schema.Stages.Add(secondStage)
        Dim stage = New clsChoiceEndStage(Nothing)
        stage.SetGroupID(groupId)
        schema.GetGroupStage(Of clsChoiceStartStage)(stage).ShouldBeEquivalentTo(firstStage)
    End Sub

    <Test>
    Public Sub GetGroupStage_SingleMatch_CorrectStage()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim groupId = Guid.NewGuid()
        Dim stage = New clsChoiceEndStage(Nothing) With {.Id = Guid.NewGuid()}
        stage.SetGroupID(groupId)
        schema.Stages.Add(stage)
        Dim firstStage = New clsChoiceEndStage(Nothing)
        firstStage.SetGroupID(groupId)
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsChoiceStartStage(Nothing)
        secondStage.SetGroupID(Guid.NewGuid())
        schema.Stages.Add(secondStage)
        Dim thirdStage = New clsChoiceStartStage(Nothing)
        thirdStage.SetGroupID(groupId)
        schema.Stages.Add(thirdStage)

        schema.GetGroupStage(Of clsChoiceStartStage)(stage).ShouldBeEquivalentTo(thirdStage)
    End Sub

    <Test>
    Public Sub GetDataAndCollectionStageByName_NoMatches_Empty()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetDataAndCollectionStageByName("anyName").Should().BeNull()
    End Sub

    <Test>
    Public Sub GetDataAndCollectionStageByName_MultipleMatches_ReturnsFirst()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim firstStage = New clsCollectionStage(Nothing) With {.Name = "rightName"}
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsCollectionStage(Nothing) With {.Name = "rightName"}
        schema.Stages.Add(secondStage)
        schema.GetDataAndCollectionStageByName("rightName").ShouldBeEquivalentTo(firstStage)
    End Sub

    <Test>
    Public Sub GetDataAndCollectionStageByName_SingleMatch_CorrectStage()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim firstStage = New clsCollectionStage(Nothing) With {.Name = "wrongName"}
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsChoiceStartStage(Nothing) With {.Name = "rightName"}
        schema.Stages.Add(secondStage)
        Dim thirdStage = New clsCollectionStage(Nothing) With {.Name = "rightName"}
        schema.Stages.Add(thirdStage)

        schema.GetDataAndCollectionStageByName("rightName").ShouldBeEquivalentTo(thirdStage)
    End Sub

    <Test>
    Public Sub GetStageByTypeAndSubSheet_NoMatches_Empty()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetStageByTypeAndSubSheet(StageTypes.Collection, Guid.NewGuid()).Should().BeNull()
    End Sub

    <Test>
    Public Sub GetStageByTypeAndSubSheet_MultipleMatches_ReturnsFirst()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim firstStage = New clsCollectionStage(Nothing)
        firstStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsCollectionStage(Nothing)
        secondStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(secondStage)
        schema.GetStageByTypeAndSubSheet(StageTypes.Collection, subsheetId).ShouldBeEquivalentTo(firstStage)
    End Sub

    <Test>
    Public Sub GetStageByTypeAndSubSheet_SingleMatch_CorrectStage()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim firstStage = New clsCollectionStage(Nothing)
        firstStage.SetSubSheetID(Guid.NewGuid())
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsChoiceStartStage(Nothing)
        secondStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(secondStage)
        Dim thirdStage = New clsCollectionStage(Nothing)
        thirdStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(thirdStage)

        schema.GetStageByTypeAndSubSheet(StageTypes.Collection, subsheetId).ShouldBeEquivalentTo(thirdStage)
    End Sub

    <Test>
    Public Sub GetStagesByTypeAndSubSheet_NoMatches_Empty()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetStagesByTypeAndSubSheet(StageTypes.Collection, Guid.NewGuid()).Should().BeEmpty()
    End Sub

    <Test>
    Public Sub GetStagesByTypeAndSubSheet_MultipleMatches_ReturnsAll()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim firstStage = New clsCollectionStage(Nothing)
        firstStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsCollectionStage(Nothing)
        secondStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(secondStage)
        schema.GetStagesByTypeAndSubSheet(StageTypes.Collection, subsheetId).ShouldBeEquivalentTo(New List(Of clsProcessStage) From {firstStage, secondStage})
    End Sub

    <Test>
    Public Sub GetStagesByType_NoMatches_Empty()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim stage = New clsActionStage(Nothing)
        schema.Stages.Add(stage)
        schema.GetStages(StageTypes.Collection).Should().BeEmpty()
    End Sub

    <Test>
    Public Sub GetStagesByType_MultipleMatches_ReturnsAll()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim firstStage = New clsCollectionStage(Nothing)
        schema.Stages.Add(firstStage)
        Dim secondStage = New clsCollectionStage(Nothing)
        schema.Stages.Add(secondStage)
        schema.GetStages(StageTypes.Collection).ShouldBeEquivalentTo(New List(Of clsProcessStage) From {firstStage, secondStage})
    End Sub

    <Test>
    Public Sub GetConflictingDataStages_NoMatches_Empty()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim id = Guid.NewGuid()
        Dim name = "name"
        Dim subsheetId = Guid.NewGuid()

        Dim wrongNameStage = New clsCollectionStage(Nothing) With {.Name = "otherName", .Id = Guid.NewGuid()}
        wrongNameStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(wrongNameStage)

        Dim sameStage = New clsCollectionStage(Nothing) With {.Name = name, .Id = id}
        sameStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(sameStage)

        Dim notInScopeStage = New clsCollectionStage(Nothing) With {.Name = name, .Id = Guid.NewGuid()}
        notInScopeStage.SetSubSheetID(Guid.NewGuid())
        schema.Stages.Add(notInScopeStage)

        Dim wrongTypeStage = New clsActionStage(Nothing) With {.Name = name, .Id = Guid.NewGuid()}
        wrongTypeStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(wrongTypeStage)

        Dim thisStage = New clsCollectionStage(Nothing) With {.Name = name, .Id = id}
        thisStage.SetSubSheetID(subsheetId)
        schema.GetConflictingDataStages(thisStage).Should().BeNull()
    End Sub

    <Test>
    Public Sub GetConflictingDataStages_MultipleMatches_ReturnsFirst()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim name = "name"
        Dim subsheetId = Guid.NewGuid()

        Dim firstStage = New clsCollectionStage(Nothing) With {.Name = name, .Id = Guid.NewGuid()}
        firstStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(firstStage)

        Dim secondStage = New clsCollectionStage(Nothing) With {.Name = name, .Id = Guid.NewGuid()}
        secondStage.SetSubSheetID(subsheetId)
        schema.Stages.Add(secondStage)

        Dim thisStage = New clsCollectionStage(Nothing) With {.Name = name, .Id = Guid.NewGuid()}
        thisStage.SetSubSheetID(subsheetId)
        schema.GetConflictingDataStages(thisStage).ShouldBeEquivalentTo(firstStage)
    End Sub

    <TestCase(StageTypes.Action, "Action")>
    <TestCase(StageTypes.Alert, "Alert")>
    <TestCase(StageTypes.Anchor, "Anchor")>
    <TestCase(StageTypes.Block, "Block")>
    <TestCase(StageTypes.Calculation, "Calc")>
    <TestCase(StageTypes.ChoiceEnd, "Otherwise")>
    <TestCase(StageTypes.ChoiceStart, "Choice")>
    <TestCase(StageTypes.Code, "Code")>
    <TestCase(StageTypes.Collection, "Coll")>
    <TestCase(StageTypes.Data, "Data")>
    <TestCase(StageTypes.Decision, "Decision")>
    <TestCase(StageTypes.End, "End")>
    <TestCase(StageTypes.Exception, "Exception")>
    <TestCase(StageTypes.LoopEnd, "Loop End")>
    <TestCase(StageTypes.LoopStart, "Loop Start")>
    <TestCase(StageTypes.MultipleCalculation, "Multi")>
    <TestCase(StageTypes.Navigate, "Navigate")>
    <TestCase(StageTypes.Note, "Note")>
    <TestCase(StageTypes.Process, "Process")>
    <TestCase(StageTypes.ProcessInfo, "Stage")>
    <TestCase(StageTypes.Read, "Reader")>
    <TestCase(StageTypes.Recover, "Recover")>
    <TestCase(StageTypes.Resume, "Resume")>
    <TestCase(StageTypes.Skill, "Skill")>
    <TestCase(StageTypes.Start, "Start")>
    <TestCase(StageTypes.SubSheet, "Page")>
    <TestCase(StageTypes.SubSheetInfo, "Stage")>
    <TestCase(StageTypes.Undefined, "Stage")>
    <TestCase(StageTypes.WaitEnd, "Time Out")>
    <TestCase(StageTypes.WaitStart, "Wait")>
    <TestCase(StageTypes.Write, "Writer")>
    Public Sub GetUniqueStageID_HasCorrectName(type As StageTypes, expected As String)
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetUniqueStageID(type).Should().Be(expected & "1")
    End Sub

    <Test>
    Public Sub GetUniqueStageID_HasExistingStages_GetsCorrectName()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim firstStage = New clsActionStage(Nothing) With {.Name = "Action1"}
        Dim secondStage = New clsActionStage(Nothing) With {.Name = "Action2"}
        schema.Stages.Add(firstStage)
        schema.Stages.Add(secondStage)
        schema.GetUniqueStageID(StageTypes.Action).Should().Be("Action3")
    End Sub

    <Test>
    Public Sub GetSubsheetName_NoMatch_ReturnsEmpty()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetSubsheetName(Guid.NewGuid()).ShouldAllBeEquivalentTo("")
    End Sub

    <Test>
    Public Sub GetSubsheetName_MultipleMatches_ReturnsFirst()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim first = New clsProcessSubSheet(Nothing) With {.Name = "First", .ID = subsheetId}
        schema.SubSheets.Add(first)
        Dim second = New clsProcessSubSheet(Nothing) With {.Name = "Second", .ID = subsheetId}
        schema.SubSheets.Add(second)

        schema.GetSubsheetName(subsheetId).ShouldAllBeEquivalentTo("First")
    End Sub

    <Test>
    Public Sub GetSubsheetName_SingleMatch_ReturnsCorrect()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim first = New clsProcessSubSheet(Nothing) With {.Name = "First", .ID = Guid.NewGuid()}
        schema.SubSheets.Add(first)
        Dim second = New clsProcessSubSheet(Nothing) With {.Name = "Second", .ID = subsheetId}
        schema.SubSheets.Add(second)

        schema.GetSubsheetName(subsheetId).ShouldAllBeEquivalentTo("Second")
    End Sub

    <Test>
    Public Sub GetSubsheetIndex_NoMatch_Returns()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetSubSheetIndex(Guid.NewGuid()).Should().Be(-1)
    End Sub

    <Test>
    Public Sub GetSubsheetIndex_MultipleMatches_ReturnsFirst()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim first = New clsProcessSubSheet(Nothing) With {.Name = "First", .ID = subsheetId}
        schema.SubSheets.Add(first)
        Dim second = New clsProcessSubSheet(Nothing) With {.Name = "Second", .ID = subsheetId}
        schema.SubSheets.Add(second)

        schema.GetSubSheetIndex(subsheetId).Should().Be(0)
    End Sub

    <Test>
    Public Sub GetSubsheetIndex_SingleMatch_ReturnsCorrect()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim first = New clsProcessSubSheet(Nothing) With {.Name = "First", .ID = Guid.NewGuid()}
        schema.SubSheets.Add(first)
        Dim second = New clsProcessSubSheet(Nothing) With {.Name = "Second", .ID = subsheetId}
        schema.SubSheets.Add(second)

        schema.GetSubSheetIndex(subsheetId).Should().Be(1)
    End Sub

    <Test>
    Public Sub GetSubsheetById_NoMatch_Returns()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetSubSheetByID(Guid.NewGuid()).Should().BeNull()
    End Sub

    <Test>
    Public Sub GetSubsheetById_MultipleMatches_ReturnsFirst()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim first = New clsProcessSubSheet(Nothing) With {.Name = "First", .ID = subsheetId}
        schema.SubSheets.Add(first)
        Dim second = New clsProcessSubSheet(Nothing) With {.Name = "Second", .ID = subsheetId}
        schema.SubSheets.Add(second)

        schema.GetSubSheetByID(subsheetId).Should().Be(first)
    End Sub

    <Test>
    Public Sub GetSubsheetById_SingleMatch_ReturnsCorrect()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim subsheetId = Guid.NewGuid()
        Dim first = New clsProcessSubSheet(Nothing) With {.Name = "First", .ID = Guid.NewGuid()}
        schema.SubSheets.Add(first)
        Dim second = New clsProcessSubSheet(Nothing) With {.Name = "Second", .ID = subsheetId}
        schema.SubSheets.Add(second)

        schema.GetSubSheetByID(subsheetId).Should().Be(second)
    End Sub

    <Test>
    Public Sub GetSubsheetId_NoMatch_Returns()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetSubSheetID("name").Should().Be(Guid.Empty)
    End Sub

    <Test>
    Public Sub GetSubsheetId_MultipleMatches_ReturnsFirst()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim first = New clsProcessSubSheet(Nothing) With {.Name = "Name", .ID = Guid.NewGuid()}
        schema.SubSheets.Add(first)
        Dim second = New clsProcessSubSheet(Nothing) With {.Name = "Name", .ID = Guid.NewGuid()}
        schema.SubSheets.Add(second)

        schema.GetSubSheetID("Name").Should().Be(first.ID)
    End Sub

    <Test>
    Public Sub GetSubsheetId_SingleMatch_ReturnsCorrect()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim first = New clsProcessSubSheet(Nothing) With {.Name = "First", .ID = Guid.NewGuid()}
        schema.SubSheets.Add(first)
        Dim second = New clsProcessSubSheet(Nothing) With {.Name = "Second", .ID = Guid.NewGuid()}
        schema.SubSheets.Add(second)

        schema.GetSubSheetID("Second").Should().Be(second.ID)
    End Sub

    <Test>
    Public Sub GetSubsheetIdBySafeName_NoMatch_Returns()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetSubSheetIDWithSafeName("process name").Should().Be(Guid.Empty)
    End Sub

    <Test>
    Public Sub GetSubsheetIdBySafeName_MultipleMatches_ReturnsFirst()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim first = New clsProcessSubSheet(Nothing) With {.Name = "process name", .ID = Guid.NewGuid()}
        schema.SubSheets.Add(first)
        Dim second = New clsProcessSubSheet(Nothing) With {.Name = "process name", .ID = Guid.NewGuid()}
        schema.SubSheets.Add(second)

        schema.GetSubSheetIDWithSafeName("process name").Should().Be(first.ID)
    End Sub

    <Test>
    Public Sub GetSubsheetIdBySafeName_SingleMatch_ReturnsCorrect()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim first = New clsProcessSubSheet(Nothing) With {.Name = "process name", .ID = Guid.NewGuid()}
        schema.SubSheets.Add(first)
        Dim second = New clsProcessSubSheet(Nothing) With {.Name = "process other name", .ID = Guid.NewGuid()}
        schema.SubSheets.Add(second)

        schema.GetSubSheetIDWithSafeName("process other name").Should().Be(second.ID)
    End Sub

    <Test>
    Public Sub GetMainPage_NoMatch_Returns()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetMainPage().Should().BeNull()
    End Sub

    <Test>
    Public Sub GetMainPage_MultipleMatches_ReturnsFirst()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim first = New clsProcessSubSheet(Nothing) With {.SheetType = SubsheetType.MainPage}
        schema.SubSheets.Add(first)
        Dim second = New clsProcessSubSheet(Nothing) With {.SheetType = SubsheetType.MainPage}
        schema.SubSheets.Add(second)

        schema.GetMainPage().Should().Be(first)
    End Sub

    <Test>
    Public Sub GetMainPage_SingleMatch_ReturnsCorrect()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim first = New clsProcessSubSheet(Nothing) With {.SheetType = SubsheetType.CleanUp}
        schema.SubSheets.Add(first)
        Dim second = New clsProcessSubSheet(Nothing) With {.SheetType = SubsheetType.MainPage}
        schema.SubSheets.Add(second)

        schema.GetMainPage().Should().Be(second)
    End Sub

    <Test>
    Public Sub GetCleanUpPage_NoMatch_Returns()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.GetCleanUpPage().Should().BeNull()
    End Sub

    <Test>
    Public Sub GetCleanUpPage_MultipleMatches_ReturnsFirst()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim first = New clsProcessSubSheet(Nothing) With {.SheetType = SubsheetType.CleanUp}
        schema.SubSheets.Add(first)
        Dim second = New clsProcessSubSheet(Nothing) With {.SheetType = SubsheetType.CleanUp}
        schema.SubSheets.Add(second)

        schema.GetCleanUpPage().Should().Be(first)
    End Sub

    <Test>
    Public Sub GetCleanUpPage_SingleMatch_ReturnsCorrect()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim first = New clsProcessSubSheet(Nothing) With {.SheetType = SubsheetType.MainPage}
        schema.SubSheets.Add(first)
        Dim second = New clsProcessSubSheet(Nothing) With {.SheetType = SubsheetType.CleanUp}
        schema.SubSheets.Add(second)

        schema.GetCleanUpPage().Should().Be(second)
    End Sub

    <Test>
    Public Sub GetNormalSheets_NoMatch_ReturnsEmpty()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.SubSheets.Add(New clsProcessSubSheet(Nothing) With {.SheetType = SubsheetType.CleanUp})
        schema.GetNormalSheets().Should().BeEmpty()
    End Sub

    <Test>
    Public Sub GetNormalSheets_MultipleMatches_ReturnsAll()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim first = New clsProcessSubSheet(Nothing) With {.SheetType = SubsheetType.Normal}
        schema.SubSheets.Add(first)
        Dim second = New clsProcessSubSheet(Nothing) With {.SheetType = SubsheetType.Normal}
        schema.SubSheets.Add(second)

        schema.GetNormalSheets().ShouldBeEquivalentTo(New List(Of clsProcessSubSheet)() From {first, second}.AsReadOnly())
    End Sub

    <Test>
    Public Sub HasExtraSheets_ReturnsCorrectly()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.HasExtraSheets().Should().BeFalse()

        schema.SubSheets.Add(New clsProcessSubSheet(Nothing) With {.SheetType = SubsheetType.Normal})
        schema.HasExtraSheets().Should().BeTrue()
    End Sub

    <Test>
    Public Sub RenameSubsheet_NoSubsheetWithId_UnsuccessfulWithError()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim errorMessage = ""
        schema.RenameSubSheet(Guid.NewGuid(), "newName", errorMessage).Should().BeFalse()
        errorMessage.Should().NotBeEmpty()
    End Sub

    <Test>
    Public Sub RenameSubsheet_SubsheetWithId_SuccessfulWithoutError()
        Dim subsheetId = Guid.NewGuid()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim stage = New clsSubsheetInfoStage(Nothing)
        stage.SetSubSheetID(subsheetId)
        schema.Stages.Add(stage)

        Dim subsheet = New clsProcessSubSheet(Nothing) With {.ID = subsheetId}
        schema.SubSheets.Add(subsheet)

        Dim errorMessage = ""
        schema.RenameSubSheet(subsheetId, "newName", errorMessage).Should().BeTrue()
        errorMessage.Should().BeEmpty()
        stage.Name.Should().Be("newName")
        subsheet.Name.Should().Be("newName")
    End Sub

    <Test>
    Public Sub SetSubsheetOrder_CountDifferent_ShouldThrow()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.SubSheets.Add(New clsProcessSubSheet(Nothing))
        schema.SubSheets.Add(New clsProcessSubSheet(Nothing))
        schema.SubSheets.Add(New clsProcessSubSheet(Nothing))

        Dim setOrder As Action = Sub() schema.SetSubSheetOrder({Guid.NewGuid(), Guid.NewGuid})
        setOrder.ShouldThrow(Of BluePrismException)()
    End Sub

    <Test>
    Public Sub SetSubsheetOrder_ChangesOrder()
        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        Dim first = New clsProcessSubSheet(Nothing) With {.ID = Guid.NewGuid(), .SheetType = SubsheetType.MainPage}
        Dim second = New clsProcessSubSheet(Nothing) With {.ID = Guid.NewGuid(), .SheetType = SubsheetType.Normal}
        Dim third = New clsProcessSubSheet(Nothing) With {.ID = Guid.NewGuid(), .SheetType = SubsheetType.CleanUp}
        Dim fourth = New clsProcessSubSheet(Nothing) With {.ID = Guid.NewGuid(), .SheetType = SubsheetType.Normal}
        Dim fifth = New clsProcessSubSheet(Nothing) With {.ID = Guid.NewGuid(), .SheetType = SubsheetType.Normal}
        Dim sixth = New clsProcessSubSheet(Nothing) With {.ID = Guid.NewGuid(), .SheetType = SubsheetType.Normal}

        schema.SubSheets.AddRange({first, second, third, fourth, fifth, sixth})

        schema.SetSubSheetOrder({second.ID, third.ID, sixth.ID, first.ID, fifth.ID, fourth.ID})

        schema.SubSheets(0).Should().Be(first)
        schema.SubSheets(1).Should().Be(third)
        schema.SubSheets(2).Should().Be(second)
        schema.SubSheets(3).Should().Be(sixth)
        schema.SubSheets(4).Should().Be(fifth)
        schema.SubSheets(5).Should().Be(fourth)
    End Sub

    <Test>
    Public Sub ReadDataItemsFromParameters_ReadsCorrectly()
        Dim arguments = New clsArgumentList()
        Dim parameters = New List(Of clsProcessParameter) From {
            New clsProcessParameter("param1", DataType.text, ParamDirection.In)}
        Dim stage = New clsCollectionStage(Nothing)
        Dim param = New clsProcessParameter("param1", DataType.text, ParamDirection.In)
        param.SetMap("5")
        stage.Parameters.Add(param)
        stage.Parameters.Add(New clsProcessParameter("param1", DataType.text, ParamDirection.Out))
        Dim errorMessage = ""

        Dim schema = New ProcessSchema(True, "1.2", DiagramType.Process)
        schema.ReadDataItemsFromParameters(parameters, stage, arguments, errorMessage, allowScopeChange:=True, isOutput:=False).Should().BeTrue()
        arguments.ShouldBeEquivalentTo(New clsArgumentList({New clsArgument("param1", New clsProcessValue("5"))}))
    End Sub
End Class

#End If
