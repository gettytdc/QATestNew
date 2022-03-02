#If UNITTESTS Then

Imports System.Linq
Imports Moq
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.AutomateProcessCore.Processes
Imports NUnit.Framework

''' <summary>
''' Tests the clsCollectionCopyRows class
''' </summary>
<TestFixture>
Public Class CollectionCopyRowTests
    Private businessObject As clsInternalBusinessObject
    Private process As clsProcess
    Private session As clsSession
    Private sut As clsCollectionCopyRows

    <SetUp>
    Public Sub SetUp()
        Dim mock = (New Mock(Of IGroupObjectDetails)).Object
        process = New clsProcess(mock, DiagramType.Process, False)
        session = New clsSession(Guid.NewGuid(), 1, New WebConnectionSettings(5, 5, 10,  New List(Of UriWebConnectionSettings)()))
        businessObject = New clsCollectionBusinessObject(process, session)
        sut = New clsCollectionCopyRows(businessObject)
    End Sub

    <Test>
    Public Sub Object_IsCorrectlyInitialised()
        Assert.That(sut.GetName(), Iz.EqualTo("Copy Rows"))
        Assert.That(sut.Parent, Iz.EqualTo(businessObject))
        Assert.That(sut.Inputs.Count, Iz.EqualTo(0))
        Assert.That(sut.Outputs.Count, Iz.EqualTo(0))

        Dim params = sut.GetParameters()
        Assert.That(params.Count, Iz.EqualTo(4))
        Dim param = TryCast(params.Item(0), clsProcessParameter)
        Assert.That(param.Name, Iz.EqualTo("Collection Name"))
        Dim startParam = TryCast(params.Item(1), clsProcessParameter)
        Assert.That(startParam.Name, Iz.EqualTo("Start Row"))
        Dim endParam = TryCast(params.Item(2), clsProcessParameter)
        Assert.That(endParam.Name, Iz.EqualTo("End Row"))
        Dim resultParam = TryCast(params.Item(3), clsProcessParameter)
        Assert.That(resultParam.Name, Iz.EqualTo("Result"))
    End Sub

    <Test>
    Public Sub CopyRows_MissingParameters_HasError()
        Dim collection = SetUpWithCollection("Collection1", "Collection1")

        Dim errorFound = String.Empty
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo("Missing Parameter(s): Start Row, End Row"))
        Assert.That(sut.Outputs.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub CopyRows_EndBeforeStart_HasError()
        Dim collection = SetUpWithCollection("Collection1", "Collection1")

        Dim errorFound = String.Empty
        sut.Inputs.Add(New clsArgument("Start Row", New clsProcessValue(1)))
        sut.Inputs.Add(New clsArgument("End Row", New clsProcessValue(0)))
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo("End Row must be greater than or equal to Start Row"))
        Assert.That(sut.Outputs.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub CopyRows_StartLessThanZero_HasError()
        Dim collection = SetUpWithCollection("Collection1", "Collection1")

        Dim errorFound = String.Empty
        sut.Inputs.Add(New clsArgument("Start Row", New clsProcessValue(-1)))
        sut.Inputs.Add(New clsArgument("End Row", New clsProcessValue(0)))
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo("Start row out of range"))
        Assert.That(sut.Outputs.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub CopyRows_StartGreaterThanRowCount_HasError()
        Dim collection = SetUpWithCollection("Collection1", "Collection1")
        collection.Add()

        Dim errorFound = String.Empty
        sut.Inputs.Add(New clsArgument("Start Row", New clsProcessValue(1)))
        sut.Inputs.Add(New clsArgument("End Row", New clsProcessValue(1)))
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo("Start row out of range"))
        Assert.That(sut.Outputs.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub CopyRows_EndGreaterThanRowCount_HasError()
        Dim collection = SetUpWithCollection("Collection1", "Collection1")
        collection.Add()

        Dim errorFound = String.Empty
        sut.Inputs.Add(New clsArgument("Start Row", New clsProcessValue(0)))
        sut.Inputs.Add(New clsArgument("End Row", New clsProcessValue(1)))
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo("End row out of range"))
        Assert.That(sut.Outputs.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub CopyRowsOnNestedCollection_StartGreaterThanRowCount_HasError()
        Dim collection = SetUpWithCollection("Collection1.Items", "Collection1")
        collection.AddField("ID", DataType.number)
        collection.AddField("Items", DataType.collection)
        collection.Add()

        Dim errorFound = String.Empty
        sut.Inputs.Add(New clsArgument("Start Row", New clsProcessValue(1)))
        sut.Inputs.Add(New clsArgument("End Row", New clsProcessValue(1)))
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo("Start row out of range"))
        Assert.That(sut.Outputs.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub CopyRowsOnNestedCollection_NoParentRow_HasError()
        Dim collection = SetUpWithCollection("Collection1.Items", "Collection1")
        collection.AddField("ID", DataType.number)
        collection.AddField("Items", DataType.collection)

        Dim errorFound = String.Empty
        sut.Inputs.Add(New clsArgument("Start Row", New clsProcessValue(0)))
        sut.Inputs.Add(New clsArgument("End Row", New clsProcessValue(0)))
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo("The collection has no current row"))
        Assert.That(sut.Outputs.Count, Iz.EqualTo(0))
    End Sub

    <Test>
    Public Sub CopyRows_CopiesToOutputCorrectly()
        Dim collection = SetUpWithCollection("Collection1", "Collection1")
        collection.AddField("ID", DataType.number)
        collection.Add()
        collection.Add()
        collection.Add()
        collection.Rows(0).SetField("ID", 0)
        collection.Rows(1).SetField("ID", 1)
        collection.Rows(2).SetField("ID", 2)

        Dim errorFound = String.Empty
        sut.Inputs.Add(New clsArgument("Start Row", New clsProcessValue(0)))
        sut.Inputs.Add(New clsArgument("End Row", New clsProcessValue(1)))
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo(String.Empty))
        Assert.That(collection.Count, Iz.EqualTo(3))
        Assert.That(sut.Outputs.Count, Iz.EqualTo(1))

        Dim expectedResult = New clsCollection()
        expectedResult.AddField("ID", DataType.number)
        expectedResult.Add()
        expectedResult.Add()
        expectedResult.Rows(0).SetField("ID", 0)
        expectedResult.Rows(1).SetField("ID", 1)

        Assert.That(sut.Outputs.Item("Result").Value,
                    Iz.EqualTo(New clsProcessValue(expectedResult)))
    End Sub

    <Test>
    Public Sub CopyRowsOfNestedCollection_CopiesToOutputCorrectly()
        Dim items0 = New clsCollection()
        items0.AddField("Name", DataType.text)
        items0.Add()
        items0.Add()
        items0.Rows(0).SetField("Name", "00")
        items0.Rows(1).SetField("Name", "01")
        Dim items1 = New clsCollection()
        items1.AddField("Name", DataType.text)
        items1.Add()
        items1.SetField("Name", "10")
        Dim collection = SetUpWithCollection("Collection1", "Collection1")
        collection.AddField("ID", DataType.number)
        collection.AddField("Items", DataType.collection)
        collection.Add()
        collection.Add()
        collection.Rows(0).SetField("ID", 0)
        collection.Rows(1).SetField("ID", 1)
        collection.Rows(0).SetField("Items", items0)
        collection.Rows(1).SetField("Items", items1)

        Dim errorFound = String.Empty
        sut.Inputs.Add(New clsArgument("Start Row", New clsProcessValue(1)))
        sut.Inputs.Add(New clsArgument("End Row", New clsProcessValue(1)))
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo(String.Empty))
        Assert.That(collection.Count, Iz.EqualTo(2))
        Assert.That(sut.Outputs.Count, Iz.EqualTo(1))

        Dim expectedItems = New clsCollection()
        expectedItems.AddField("Name", DataType.text)
        expectedItems.Add()
        expectedItems.SetField("Name", "10")
        Dim expectedResult = New clsCollection()
        expectedResult.AddField("ID", DataType.number)
        expectedResult.AddField("Items", DataType.collection)
        expectedResult.Add()
        expectedResult.SetField("ID", 1)
        expectedResult.SetField("Items", New clsProcessValue(expectedItems))

        Assert.That(sut.Outputs.Item("Result").Value,
           Iz.EqualTo(New clsProcessValue(expectedResult)))
    End Sub

    <Test>
    Public Sub CopyRowsOfNestedCollection_WithSpaces_CopiesToOutputCorrectly()
        Dim items0 = New clsCollection()
        items0.AddField(" Name ", DataType.text)
        items0.Add()
        items0.Add()
        items0.Rows(0).SetField(" Name ", "00")
        items0.Rows(1).SetField(" Name ", "01")
        Dim items1 = New clsCollection()
        items1.AddField(" Name ", DataType.text)
        items1.Add()
        items1.SetField(" Name ", "10")
        Dim collection = SetUpWithCollection("Collection1", "Collection1")
        collection.AddField("ID", DataType.number)
        collection.AddField("Items", DataType.collection)
        collection.Add()
        collection.Add()
        collection.Rows(0).SetField("ID", 0)
        collection.Rows(1).SetField("ID", 1)
        collection.Rows(0).SetField("Items", items0)
        collection.Rows(1).SetField("Items", items1)

        Dim errorFound = String.Empty
        sut.Inputs.Add(New clsArgument("Start Row", New clsProcessValue(1)))
        sut.Inputs.Add(New clsArgument("End Row", New clsProcessValue(1)))
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo(String.Empty))
        Assert.That(collection.Count, Iz.EqualTo(2))
        Assert.That(sut.Outputs.Count, Iz.EqualTo(1))

        Dim expectedItems = New clsCollection()
        expectedItems.AddField(" Name ", DataType.text)
        expectedItems.Add()
        expectedItems.SetField(" Name ", "10")
        Dim expectedResult = New clsCollection()
        expectedResult.AddField("ID", DataType.number)
        expectedResult.AddField("Items", DataType.collection)
        expectedResult.Add()
        expectedResult.SetField("ID", 1)
        expectedResult.SetField("Items", New clsProcessValue(expectedItems))

        Assert.That(sut.Outputs.Item("Result").Value,
           Iz.EqualTo(New clsProcessValue(expectedResult)))
    End Sub

    <Test>
    Public Sub CopyRows_HasEmptyNestedCollection_CopiesToOutputCorrectly()
        Dim collection = SetUpWithCollection("Collection1", "Collection1")
        collection.AddField("ID", DataType.number)
        collection.AddField("Items", DataType.collection)
        collection.Add()
        collection.SetField("ID", 0)
        collection.SetField("Items", New clsCollection())

        Dim errorFound = String.Empty
        sut.Inputs.Add(New clsArgument("Start Row", New clsProcessValue(0)))
        sut.Inputs.Add(New clsArgument("End Row", New clsProcessValue(0)))
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo(String.Empty))
        Assert.That(collection.Count, Iz.EqualTo(1))
        Assert.That(sut.Outputs.Count, Iz.EqualTo(1))

        Dim expectedItems = New clsCollection()
        expectedItems.AddField("Name", DataType.text)
        expectedItems.Add()
        expectedItems.SetField("Name", "10")
        Dim expectedResult = New clsCollection()
        expectedResult.AddField("ID", DataType.number)
        expectedResult.AddField("Items", DataType.collection)
        expectedResult.Add()
        expectedResult.SetField("ID", 0)
        expectedResult.SetField("Items", New clsCollection())

        Assert.That(sut.Outputs.Item("Result").Value,
           Iz.EqualTo(New clsProcessValue(expectedResult)))
    End Sub

    <Test>
    Public Sub CopyRowsOfNestedCollection_EmptyCollection_CopiesToOutputCorrectly()
        Dim items = New clsCollection()
        items.AddField("Name", DataType.text)
        items.Add()
        items.Add()
        items.Rows(0).SetField("Name", "00")
        items.Rows(1).SetField("Name", "01")

        Dim collection = SetUpWithCollection("Collection1.Items", "Collection1")
        collection.AddField("ID", DataType.number)
        collection.AddField("Items", DataType.collection)
        collection.Add()
        collection.SetField("ID", 0)
        collection.SetField("Items", items)

        Dim errorFound = String.Empty
        sut.Inputs.Add(New clsArgument("Start Row", New clsProcessValue(1)))
        sut.Inputs.Add(New clsArgument("End Row", New clsProcessValue(1)))
        sut.Execute(process, session, New clsCollectionStage(process), errorFound)

        Assert.That(errorFound, Iz.EqualTo(String.Empty))
        Assert.That(sut.Outputs.Count, Iz.EqualTo(1))

        Dim expectedItems = New clsCollection()
        expectedItems.AddField("Name", DataType.text)
        expectedItems.Add()
        expectedItems.SetField("Name", "01")

        Assert.That(sut.Outputs.Item("Result").Value,
           Iz.EqualTo(New clsProcessValue(expectedItems)))
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
