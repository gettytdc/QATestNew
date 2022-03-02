#If UNITTESTS Then

Imports NUnit.Framework
Imports Moq
Imports BluePrism.AutomateProcessCore.WebApis
Imports System.Net.Http
Imports BluePrism.AutomateProcessCore.Stages
Imports System.Linq
Imports BluePrism.AutomateProcessCore.Processes
Imports BluePrism.Utilities.Functional

<TestFixture()>
Public Class ProcessParameterTests
    Private mCollectionInfo As clsCollectionInfo
    Private mActionStage As clsActionStage
    Private mCollectionStage As clsCollectionStage
    Private mCollectionStageId As Guid
    Private mDataStageId As Guid
    Private mDataStage As clsDataStage
    Private mParameter As clsProcessParameter
    Private mProcessMock As Mock(Of clsProcess)
    Private mAction1 As clsBusinessObjectAction
    Private mErrors As ValidationErrorList

    <SetUp>
    Public Sub SetUp()

        mErrors = New ValidationErrorList()

        Dim groupObjectDetailsMock As New Mock(Of IGroupObjectDetails)

        mParameter = New clsProcessParameter("Files", DataType.collection, ParamDirection.In)

        mCollectionInfo = New clsCollectionInfo()
        mCollectionInfo.AddField("File", DataType.collection)
        mCollectionInfo.AddField("FileName", DataType.text)
        mCollectionInfo.AddField("Name", DataType.text)
        mCollectionInfo.AddField("ContentType", DataType.text)

        mParameter.CollectionInfo = mCollectionInfo

        mProcessMock = New Mock(Of clsProcess)(MockBehavior.Strict, New Object() {groupObjectDetailsMock.Object, DiagramType.Process, True})
        Dim config = New WebApiConfigurationBuilder().
                                WithBaseUrl("https://www.myapi.org/").
                                WithAction("Action 1", HttpMethod.Post, "/action1").
                                WithAction("Action 2", HttpMethod.Post, "/action1").
                                Build()


        Dim businessObject As New WebApiBusinessObject(New WebApi(Guid.NewGuid, "My Web API", True, config))
        mAction1 = businessObject.GetAction("Action 1")
        mAction1.AddParameter(mParameter)

        mProcessMock.Setup(Function(x) x.GetBusinessObjectRef(Moq.It.IsAny(Of String))).Returns(businessObject)

        mParameter.Process = mProcessMock.Object

        mActionStage = New clsActionStage(mProcessMock.Object)
        mActionStage.SetResource("My Web API", "Action 1")

        mCollectionStage = New clsCollectionStage(mProcessMock.Object)
        mCollectionStage.Name = "new"
        mCollectionStageId = Guid.NewGuid()
        mCollectionStage.Id = mCollectionStageId
        mProcessMock.Setup(Function(x) x.GetStage(mCollectionStageId)).Returns(mCollectionStage)

        mDataStage = New clsDataStage(mProcessMock.Object)
        mDataStageId = Guid.NewGuid()
        mDataStage.Id = mDataStageId
        mProcessMock.Setup(Function(x) x.GetStage(mDataStageId)).Returns(mDataStage)

    End Sub

    <Test>
    Public Sub GetWebApiBusinessObjectAction_ActionStageReferencesCorrectAction_ShouldReturnAction()
        Dim result = mParameter.GetWebApiBusinessObjectAction(mActionStage)
        Assert.That(result, Iz.EqualTo(mAction1))
    End Sub


    <Test>
    Public Sub GetWebApiBusinessObjectAction_ActionStageReferencesDifferentAction_ShouldReturnNothing()
        mActionStage.SetResource("My Web API", "Action that does not exist")
        Dim result = mParameter.GetWebApiBusinessObjectAction(mActionStage)
        Assert.That(result, Iz.Null)
    End Sub

    <Test>
    Public Sub CheckWebApiActionInputCollectionErrors_NotAnActionStage_ShouldAddNoErrors()
        Dim startStage = New clsStartStage(mProcessMock.Object)
        mParameter.CheckWebApiActionInputCollectionErrors(startStage, New clsExpressionInfo(), "", mErrors)
        Assert.That(mErrors.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub CheckWebApiActionInputCollectionErrors_ParameterNotAnInput_ShouldAddNoErrors()
        mParameter.Direction = ParamDirection.Out
        mParameter.CheckWebApiActionInputCollectionErrors(mActionStage, New clsExpressionInfo(), "", mErrors)
        Assert.That(mErrors.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub CheckWebApiActionInputCollectionErrors_InputParameterCollectionInfoIsNothing_ShouldAddNoErrors()
        mParameter.CollectionInfo = Nothing
        mParameter.CheckWebApiActionInputCollectionErrors(mActionStage, New clsExpressionInfo(), "", mErrors)
        Assert.That(mErrors.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub CheckWebApiActionInputCollectionErrors_ExpressionContainsNoDataItems_ShouldAddNoErrors()
        Dim expressionInfo As New clsExpressionInfo()
        mParameter.CheckWebApiActionInputCollectionErrors(mActionStage, expressionInfo, "", mErrors)
        Assert.That(mErrors.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub CheckWebApiActionInputCollectionErrors_ExpressionContainsNonCollectionDataItem_ShouldAddNoErrors()
        Dim expressionInfo As New clsExpressionInfo()
        expressionInfo.AddDataItem(mDataStageId)
        mParameter.CheckWebApiActionInputCollectionErrors(mActionStage, expressionInfo, "", mErrors)
        Assert.That(mErrors.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub CheckWebApiActionInputCollectionErrors_CollectionDataItemHasNoSchema_ShouldAddSchemaNotDefinedError()
        Dim expressionInfo As New clsExpressionInfo()
        expressionInfo.AddDataItem(mCollectionStageId)

        mParameter.CheckWebApiActionInputCollectionErrors(mActionStage, expressionInfo, "", mErrors)

        Assert.That(mErrors.
                        OfType(Of ValidateProcessResult).
                        Where(Function(x) x.CheckID = 145).
                        Count(),
                    Iz.EqualTo(1))
    End Sub

    <Test>
    Public Sub CheckWebApiActionInputCollectionErrors_CollectionDataItemHasMatchingSchema_ShouldAddNoError()
        Dim expressionInfo As New clsExpressionInfo()
        expressionInfo.AddDataItem(mCollectionStageId)

        mCollectionInfo.
            FieldDefinitions.
            ForEach(Sub(x) mCollectionStage.AddField(x)).
            Evaluate()

        mParameter.CheckWebApiActionInputCollectionErrors(mActionStage, expressionInfo, "", mErrors)

        Assert.That(mErrors.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub CheckWebApiActionInputCollectionErrors_CollectionDataItemSchemaDoesNotMatch_ShouldAddCollectionMismatchError()

        Dim expressionInfo As New clsExpressionInfo()
        expressionInfo.AddDataItem(mCollectionStageId)
        mCollectionStage.AddField(New clsCollectionFieldInfo("Some Number", DataType.number, "ns"))
        mCollectionStage.AddField(New clsCollectionFieldInfo("Some Text", DataType.text, "ns"))

        mParameter.CheckWebApiActionInputCollectionErrors(mActionStage, expressionInfo, "", mErrors)

        Assert.That(mErrors.
                        OfType(Of ValidateProcessResult).
                        Where(Function(x) x.CheckID = 144).
                        Count(),
                    Iz.EqualTo(1))
    End Sub

End Class

#End If
