
#If UNITTESTS Then

Imports BluePrism.AutomateProcessCore.WebApis
Imports NUnit.Framework
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq

<TestFixture>
Public Class JsonHelperTests

    <TestCase(Nothing)>
    <TestCase("{}")>
    <TestCase("[{}]")>
    <TestCase("[[[]]]")>
    Public Sub Test_DeserialiseGeneric_EmptyJson_DoesNotThrow(testJson As Object)
        Dim expectedTable = New DataTable
        Dim emptyObject As Object = testJson
        Assert.DoesNotThrow(Sub() JsonHelper.DeserializeGeneric(emptyObject, True))
    End Sub

    <Test>
    Public Sub Test_DeserialiseGeneric_EmptyJsonArray_ReturnsEmptyTable()
        Dim expectedTable = New DataTable
        Dim emptyArray As JArray = JArray.Parse("[{}]")
        Dim deserialisedArray = JsonHelper.DeserializeGeneric(emptyArray, True)
        Assert.IsTrue(CheckEachCellIsEqual(CType(deserialisedArray, DataTable), expectedTable))

    End Sub

    <Test>
    Public Sub Test_DeserialiseGeneric_EmptyJsonObject_ReturnsEmptyTable()
        Dim expectedTable = New DataTable
        Dim emptyObject As JObject = JObject.Parse("{}")
        Dim deserialisedArray = JsonHelper.DeserializeGeneric(emptyObject, True)
        Assert.IsTrue(CheckEachCellIsEqual(CType(deserialisedArray, DataTable), expectedTable))

    End Sub
    
    <Test>
    Public Sub Test_DeserializeGeneric_Nowt_DoesNotThrow()
        Dim collection = GetTestJson(New Nowt)
        Assert.DoesNotThrow(Sub() JsonHelper.DeserializeGeneric(collection, True))
    End Sub

    <Test>
    Public Sub Test_DeserialiseArray_ReturnsExpectedTable()
        Dim expectedTable = New DataTable
        With expectedTable
            .Columns.Add("Name", GetType(String))
            .Columns.Add("Age", GetType(String))
            .Rows.Add("Steve Jones", 44)
            .Rows.Add("Dave Smith", 95)
        End With

        Dim peopleArray As JArray = JArray.Parse(JsonConvert.SerializeObject({New Person("Steve Jones", 44), New Person("Dave Smith", 95)}))
        Dim deserialisedArray = JsonHelper.DeserializeArray(peopleArray, True)

        Assert.IsTrue(CheckEachCellIsEqual(deserialisedArray, expectedTable))

    End Sub
    
    <Test>
    Public Sub Test_DeserialiseArray_EmptyArray_ReturnsEmptyDataTable()
        Dim expectedTable = New DataTable()

        Dim emptyArray As JArray = New JArray
        Dim deserialisedArray = JsonHelper.DeserializeArray(emptyArray, True)
        Assert.IsTrue(CheckEachCellIsEqual(deserialisedArray, expectedTable))

    End Sub

    <Test>
    Public Sub Test_DeserialiseArray_DataMismatch_Throws()
        Dim expectedTable = New DataTable
        With expectedTable
            .Columns.Add("Name", GetType(String))
            .Columns.Add("Age", GetType(Object))
            .Rows.Add("Steve Jones", 44)
            .Rows.Add("Dave Smith", "SomethingElse")
        End With
        Dim peopleArray As JArray = JArray.Parse(JsonConvert.SerializeObject(expectedTable))

        Assert.Throws(Of InvalidOperationException)(Sub() JsonHelper.DeserializeArray(peopleArray, True))

    End Sub

    <Test>
    Public Sub Test_DeserializeCollection_ReturnsExpectedDataTable_SimpleObject()

        Dim json As JToken = GetTestJson(New SentimentData())
        Dim expectedTable As DataTable = GetExpectedDocumentSentimentTable()
        Dim deserializedCollection = JsonHelper.DeserializeCollection(json, "$.DocumentSentiment")

        Assert.IsTrue(CheckEachCellIsEqual(deserializedCollection, expectedTable))

    End Sub

    <Test>
    Public Sub Test_DeserializeCollection_ReturnsExpectedDataTable_NestedArrays()

        Dim json As JToken = GetTestJson(New SentimentData())
        Dim expectedTable As DataTable = GetExpectedSentimentTable()
        Dim deserializedCollection = JsonHelper.DeserializeCollection(json, "$.Sentences")

        Assert.IsTrue(CheckEachCellIsEqual(deserializedCollection, expectedTable))
    End Sub

    <Test>
    Public Sub Test_DeserializeCollection_ReturnsExpectedDataTable_Entities()

        Dim json As JToken = GetTestJson(New EntityData())
        Dim expectedTable As DataTable = GetExpectedEntitiesTable()
        Dim deserializedCollection = JsonHelper.DeserializeCollection(json, "$.Entities")

        Assert.IsTrue(CheckEachCellIsEqual(deserializedCollection, expectedTable))
    End Sub

    <Test>
    Public Sub CollectionWithDifferentFields_SerialisesCorrectly()
        Dim json = "{'results':[{'Name':'Bob', 'Age':5, 'FavouriteAnimal':'Dog'},{'Name':'Steve', 'Age':15, 'Eyes': [{'colour':'green'}]},{'Name':'Sheila', 'Age':25, 'FavouriteAnimal':'Cat', 'Eyes': [{'colour':'blue','crying':true},{'colour':'blue','crying':true}]}]}"

        Dim expectedTable = New DataTable()
        expectedTable.Columns.Add("Name", GetType(String))
        expectedTable.Columns.Add("Age", GetType(Integer))
        expectedTable.Columns.Add("FavouriteAnimal", GetType(String))
        expectedTable.Columns.Add("Eyes", GetType(DataTable))

        Dim steveEyesTable = New DataTable()
        With steveEyesTable
            .Columns.Add("colour", GetType(String))
            .Columns.Add("crying", GetType(Boolean))
            .Rows.Add("green", False)
        End With

        Dim sheilaEyesTable = New DataTable()
        With sheilaEyesTable
            .Columns.Add("colour", GetType(String))
            .Columns.Add("crying", GetType(Boolean))
            .Rows.Add("blue", True)
            .Rows.Add("blue", True)
        End With

        expectedTable.Rows.Add("Bob", 5, "Dog", New DataTable())
        expectedTable.Rows.Add("Steve", 15, "", steveEyesTable)
        expectedTable.Rows.Add("Sheila", 25, "Cat", sheilaEyesTable)

        Dim deserializedCollection = JsonHelper.DeserializeCollection(JToken.Parse(json), "$.results")

        Assert.IsTrue(CheckEachCellIsEqual(deserializedCollection, expectedTable))
    End Sub

    <Test>
    Public Sub CollectionOfArrays_SerialisesCorrectly()
        Dim json = "{'results':[[5,4,3,2,1],[1,2]]}"

        Dim firstArray = New DataTable()
        firstArray.Columns.Add("Value", GetType(String))
        firstArray.Rows.Add("5")
        firstArray.Rows.Add("4")
        firstArray.Rows.Add("3")
        firstArray.Rows.Add("2")
        firstArray.Rows.Add("1")

        Dim secondArray = New DataTable()
        secondArray.Columns.Add("Value", GetType(String))
        secondArray.Rows.Add("1")
        secondArray.Rows.Add("2")

        Dim expectedTable = New DataTable()
        expectedTable.Columns.Add("Value", GetType(DataTable))
        expectedTable.Rows.Add(firstArray)
        expectedTable.Rows.Add(secondArray)

        Dim deserializedCollection = JsonHelper.DeserializeCollection(JToken.Parse(json), "$.results")

        Assert.IsTrue(CheckEachCellIsEqual(deserializedCollection, expectedTable))
    End Sub

    Private Function CheckEachCellIsEqual(dt1 As DataTable, dt2 As DataTable) As Boolean
        For i = 0 To dt1.Rows.Count - 1
            Dim row = dt1.Rows(i)
            For j = 0 To dt1.Columns.Count - 1
                If row(j).GetType Is GetType(DataTable) Then
                    If CType(row(j), DataTable).Columns.Count = 0 Then
                        ' check other table is 0 too
                        If Not CType(dt2.Rows(i).Item(j), DataTable).Columns.Count = 0 Then Return False
                    Else
                        If Not CheckEachCellIsEqual(CType(row(j), DataTable), CType(dt2.Rows(i).Item(j), DataTable)) Then
                            Return False
                        End If
                    End If

                Else
                    If Not row(j).ToString.Equals(dt2.Rows(i).Item(j).ToString) Then
                        Return False
                    End If
                End If
            Next
        Next
        Return True
    End Function


    Shared Function GetTestJson(testClass As Object) As JToken
        Return JToken.Parse(JsonConvert.SerializeObject(testClass))
    End Function

    Shared Function GetTestJsonArray(testClass As Object) As JArray
        Return JArray.Parse(JsonConvert.SerializeObject(testClass))
    End Function


    Private Function GetExpectedDocumentSentimentTable() As DataTable
        Dim sentimentTable = New DataTable
        With sentimentTable
            .Columns.Add("Magnitude", GetType(Double))
            .Columns.Add("Score", GetType(Double))
            .Rows.Add(0.5, 0.4)
        End With
        Return sentimentTable
    End Function

    Private Function GetExpectedSentimentTable() As DataTable

        Dim textTable1 = New DataTable
        With textTable1
            .Columns.Add("Content", GetType(String))
            .Columns.Add("BeginOffset", GetType(String))
            .Rows.Add("This is not a very negative sentence", 44)
        End With

        Dim sentimentTable1 = New DataTable
        With sentimentTable1
            .Columns.Add("Magnitude", GetType(Double))
            .Columns.Add("Score", GetType(Double))
            .Rows.Add(0.4, -0.5)
        End With

        Dim textTable2 = New DataTable
        With textTable2
            .Columns.Add("Content", GetType(String))
            .Columns.Add("BeginOffset", GetType(String))
            .Rows.Add("I am so angry I might scream", 26)
        End With

        Dim sentimentTable2 = New DataTable
        With sentimentTable2
            .Columns.Add("Magnitude", GetType(Double))
            .Columns.Add("Score", GetType(Double))
            .Rows.Add(0.6, -2.0)
        End With

        Dim dt = New DataTable
        With dt
            .Columns.Add("Text", GetType(DataTable))
            .Columns.Add("Sentiment", GetType(DataTable))

            .Rows.Add(textTable1, sentimentTable1)
            .Rows.Add(textTable2, sentimentTable2)
        End With
        Return dt
    End Function

    Private Function GetExpectedEntitiesTable() As DataTable
        Dim metadataTable = New DataTable

        Dim mentionsTable1 = New DataTable
        With mentionsTable1
            .Columns.Add("Text", GetType(DataTable))
            .Columns.Add("Type", GetType(String))

            Dim textTable1 = New DataTable
            With textTable1
                .Columns.Add("Content", GetType(String))
                .Columns.Add("BeginOffset", GetType(String))
                .Rows.Add("example", 5)
            End With

            .Rows.Add(textTable1, "COMMON")
        End With

        Dim mentionsTable2 = New DataTable
        With mentionsTable2
            .Columns.Add("Text", GetType(DataTable))
            .Columns.Add("Type", GetType(String))

            Dim textTable2 = New DataTable
            With textTable2
                .Columns.Add("Content", GetType(String))
                .Columns.Add("BeginOffset", GetType(String))
                .Rows.Add("word", 25)
            End With

            .Rows.Add(textTable2, "COMMON")
        End With

        Dim dt = New DataTable
        With dt
            .Columns.Add("Metadata", GetType(DataTable))
            .Columns.Add("Mentions", GetType(DataTable))
            .Columns.Add("Name", GetType(String))
            .Columns.Add("Type", GetType(String))
            .Columns.Add("Salience", GetType(Double))


            .Rows.Add(metadataTable, mentionsTable1, "example", "OTHER", 0.59895617)
            .Rows.Add(metadataTable, mentionsTable2, "word", "OTHER", 0.16997313)
        End With
        Return dt
    End Function


    Public Class Person

        Property Name As String
        Property Age As Integer
        Sub New(name As String, age As Integer)
            Me.Name = name
            Me.Age = age
        End Sub
    End Class


    Public Class SentimentData

        Property DocumentSentiment As Sentiment = New Sentiment(0.5, 0.4)
        Property Sentences As IEnumerable(Of Sentence) = {New Sentence("This is not a very negative sentence", 44, 0.4, -0.5),
                                                          New Sentence("I am so angry I might scream", 26, 0.6, -2.0)}
        Public Class Sentiment
            Property Magnitude As Double
            Property Score As Double
            Sub New(mag As Double, score As Double)
                Me.Magnitude = mag
                Me.Score = score
            End Sub

        End Class

        Public Class Text
            Property Content As String
            Property BeginOffset As Integer

            Sub New(content As String, offset As Integer)
                Me.Content = content
                Me.BeginOffset = offset
            End Sub
        End Class

        Public Class Sentence
            Public Property Text As Text
            Public Property Sentiment As Sentiment
            Sub New(content As String, offset As Integer, mag As Double, score As Double)
                Me.Text = New Text(content, offset)
                Me.Sentiment = New Sentiment(mag, score)
            End Sub
        End Class

    End Class

    Public Class EntityData
        Public Property Entities As IEnumerable(Of Entity) = New List(Of Entity) From {
            New Entity("example", "OTHER", 0.59895617, New SentimentData.Text("example", 5), "COMMON"),
            New Entity("word", "OTHER", 0.16997313, New SentimentData.Text("word", 25), "COMMON")}

    End Class
    Public Class Entity
        Public Property Name As String
        Public Property Type As String
        Public Property Salience As Double
        Public Metadata As Metadata
        Public Mentions As IEnumerable(Of Mention)

        Public Sub New(ByVal name As String, ByVal type As String, ByVal salience As Double, ByVal mentionsText As SentimentData.Text, ByVal mentionsType As String)
            Me.Name = name
            Me.Type = type
            Me.Salience = salience
            Me.Metadata = New Metadata()
            Me.Mentions = New List(Of Mention) From {
                New Mention(mentionsText.Content, mentionsText.BeginOffset, mentionsType)
                }
        End Sub
    End Class

    Public Class Metadata
    End Class

    Public Class Mention
        Public Property Text As SentimentData.Text
        Public Property Type As String

        Public Sub New(ByVal content As String, ByVal offset As Integer, ByVal type As String)
            Me.Text = New SentimentData.Text(content, offset)
            Me.Type = type
        End Sub
    End Class
    
    Public Class Nowt

    End Class

End Class


#End If

