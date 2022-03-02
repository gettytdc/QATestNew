﻿#If UNITTESTS Then

Imports System.Linq
Imports Moq
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore.Processes
Imports NUnit.Framework

''' <summary>
''' Tests the clsCollectionRemoveRow class
''' </summary>
<TestFixture>
Public Class CollectionRemoveRowTests
    Private businessObject As clsInternalBusinessObject
    Private process As clsProcess
    Private session As clsSession
    Private sut As clsCollectionRemoveRow

    <SetUp>
    Public Sub SetUp()
        Dim mock = (New Mock(Of IGroupObjectDetails)).Object
        process = New clsProcess(mock, DiagramType.Process, False)
        session = New clsSession(Guid.NewGuid(), 1, New WebConnectionSettings(5, 5, 10,  New List(Of UriWebConnectionSettings)()))
        businessObject = New clsCollectionBusinessObject(process, session)
        sut = New clsCollectionRemoveRow(businessObject)
    End Sub

    <Test>
    Public Sub Object_IsCorrectlyInitialised()
        Assert.That(sut.GetName(), Iz.EqualTo("Remove Row"))
        Assert.That(sut.Parent, Iz.EqualTo(businessObject))
        Assert.That(sut.Inputs.Count, Iz.EqualTo(0))
        Assert.That(sut.Outputs.Count, Iz.EqualTo(0))

        Dim params = sut.GetParameters()
        Assert.That(params.Count, Iz.EqualTo(1))
        Dim param = TryCast(params.Item(0), clsProcessParameter)
        Assert.That(param.Name, Iz.EqualTo("Collection Name"))
    End Sub

    <Test>
    Public Sub RemoveRow_EmptyCollection_HasError()
        Dim collection = SetUpWithCollection("Collection1", "Collection1")

        Dim errorFound = String.Empty
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo("The collection found at 'Collection1' has no current row"))
        Assert.That(collection.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub RemoveRow_DeletesRow()
        Dim collection = SetUpWithCollection("Collection1", "Collection1")
        collection.Add()

        Dim errorFound = String.Empty
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo(String.Empty))
        Assert.That(collection.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub RemoveRow_DeletesRow_WithSpaces()
        Dim collection = SetUpWithCollection(" Collection1 ", " Collection1 ")
        collection.Add()

        Dim errorFound = String.Empty
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo(String.Empty))
        Assert.That(collection.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub RemoveRow_DeletesCorrectRow()
        Dim collection = SetUpWithCollection("Collection1", "Collection1")
        collection.AddField("ID", DataType.number)
        collection.Add()
        collection.Add()
        collection.Add()
        collection.Rows(0).SetField("ID", 1)
        collection.Rows(1).SetField("ID", 2)
        collection.Rows(2).SetField("ID", 3)

        Dim errorFound = String.Empty
        collection.SetCurrentRow(1)
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo(String.Empty))
        Assert.That(collection.Count, Iz.EqualTo(2))
        Assert.That(collection.Rows(0).GetField("ID"), Iz.EqualTo(New clsProcessValue(1)))
        Assert.That(collection.Rows(1).GetField("ID"), Iz.EqualTo(New clsProcessValue(3)))
    End Sub

    <Test>
    Public Sub RemoveRowFromNestedCollection_EmptyCollection_HasError()
        Dim collection = SetUpWithCollection("Collection1.Items", "Collection1")
        collection.AddField("Items", DataType.collection)
        collection.Add()

        Dim errorFound = String.Empty
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo("The collection found at 'Collection1.Items' has no current row"))
        Assert.That(collection.Count, Iz.EqualTo(1))
        Assert.That(collection.GetField("Items").Collection.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub RemoveRowFromNestedCollection_NoParentRow_HasError()
        Dim collection = SetUpWithCollection("Collection1.Items", "Collection1")
        collection.AddField("Items", DataType.collection)

        Dim errorFound = String.Empty
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo("The collection has no current row"))
        Assert.That(collection.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub RemoveRow_HasNestedCollection_DoesRemove()
        Dim collection = SetUpWithCollection("Collection1", "Collection1")
        collection.AddField("Items", DataType.collection)
        collection.Add()
        collection.SetField("Items", New clsCollection())

        Dim errorFound = String.Empty
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo(String.Empty))
        Assert.That(collection.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub RemoveRow_HasNestedCollection_SpacesInNames_DoesRemove()
        Dim collection = SetUpWithCollection(" Collection1 ", " Collection1 ")
        collection.AddField("  Items ", DataType.collection)
        collection.Add()
        collection.SetField("  Items ", New clsCollection())

        Dim errorFound = String.Empty
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo(String.Empty))
        Assert.That(collection.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub RemoveRowFromNestedCollection_DoesDelete()
        Dim collection = SetUpWithCollection("Collection1.Items", "Collection1")
        collection.AddField("Items", DataType.collection)
        collection.Add()
        Dim items = New clsCollection()
        items.Add()
        collection.SetField("Items", items)

        Dim errorFound = String.Empty
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo(String.Empty))
        Assert.That(collection.Count, Iz.EqualTo(1))
        Assert.That(items.Count, Iz.EqualTo(0))
    End Sub

    Private Function SetUpWithCollection(collectionName As String, dataStageName As String) As clsCollection
        sut.Inputs.Add(New clsArgument("Collection Name", New clsProcessValue(collectionName)))
        Dim stage = process.AddDataStage(dataStageName, DataType.collection)
        Dim outOfScope = False
        Dim collectionStage = process.GetCollectionStage(dataStageName, New clsCollectionStage(process), outOfScope)
        collectionStage.Value.Collection = New clsCollection()
        Return collectionStage.Value.Collection
    End Function
End Class

#End If