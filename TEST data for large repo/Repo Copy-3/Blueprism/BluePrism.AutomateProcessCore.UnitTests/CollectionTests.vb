Imports BluePrism.Common.Security
Imports BluePrism.Server.Domain.Models
Imports System.Linq
Imports System.Xml.Linq
Imports FluentAssertions
Imports BluePrism.BPCoreLib

#If UNITTESTS Then


Imports NUnit.Framework

''' <summary>
''' Tests the clsCollection and associated classes/methods
''' </summary>
<TestFixture>
Public Class CollectionTests

    <Test>
    Public Sub TestEquals()
        Dim c1 As New clsCollection()
        Assert.That(c1.Equals(Nothing), [Is].False)
        Assert.That(c1.Equals(c1), [Is].True)
        Assert.That(Object.Equals(c1, Nothing), [Is].False)
        Assert.That(Object.Equals(Nothing, c1), [Is].False)
        Assert.That(Object.Equals(c1, c1), [Is].True)

        Dim c2 As New clsCollection()
        Assert.That(c1.Equals(c2), [Is].True)
        Assert.That(Object.Equals(c1, c2), [Is].True)
        Assert.That(c2.Equals(c1), [Is].True)
        Assert.That(Object.Equals(c2, c1), [Is].True)

        With c1.Definition
            .AddField("ID", DataType.number)
            .AddField("Name", DataType.text)
            .AddField("Date of birth", DataType.date)
        End With

        ' The definition does not, in itself, impact the equality of a collection,
        ' only its value
        Assert.That(c1.Equals(c2), [Is].True)

        Dim row1 As New clsCollectionRow()
        row1("ID") = 1
        row1("Name") = "Jack Daniels"
        row1("Date of birth") =
         New clsProcessValue(DataType.date, New Date(1875, 1, 23))

        c1.Add(row1)
        Assert.That(c1.Equals(c2), [Is].False)
        Assert.That(c2.Equals(c1), [Is].False)

        Dim row2 As New clsCollectionRow()

        row2("ID") = 1
        row2("Name") = "Jack Daniels"
        row2("Date of birth") =
         New clsProcessValue(DataType.date, New Date(1875, 1, 23))
        c2.Add(row2)

        Assert.That(row1.Equals(row2), [Is].True)
        Assert.That(row2.Equals(row1), [Is].True)
        Assert.That(c1.Equals(c2), [Is].True)
        Assert.That(c2.Equals(c1), [Is].True)

        ' Change capitalisation
        row2.Remove("Date of birth")
        row2("Date of Birth") =
         New clsProcessValue(DataType.date, New Date(1875, 1, 23))

        Assert.That(row1.Equals(row2), [Is].False)
        Assert.That(row2.Equals(row1), [Is].False)
        Assert.That(c1.Equals(c2), [Is].False)
        Assert.That(c2.Equals(c1), [Is].False)

        ' Set it back again
        row2.Remove("Date of Birth")
        row2("Date of birth") =
         New clsProcessValue(DataType.date, New Date(1875, 1, 23))

        Assert.That(row1.Equals(row2), [Is].True)
        Assert.That(row2.Equals(row1), [Is].True)
        Assert.That(c1.Equals(c2), [Is].True)
        Assert.That(c2.Equals(c1), [Is].True)

        ' Change the value
        row2("Date of birth") =
         New clsProcessValue(DataType.date, New Date(1875, 1, 24))

        Assert.That(row1.Equals(row2), [Is].False)
        Assert.That(row2.Equals(row1), [Is].False)
        Assert.That(c1.Equals(c2), [Is].False)
        Assert.That(c2.Equals(c1), [Is].False)

    End Sub

    Private Function NewRow(name As String, val As clsProcessValue) As clsCollectionRow
        Dim row As New clsCollectionRow()
        row(name) = val
        Return row
    End Function

    <Test>
    Public Sub TestControlCharsInInitValues()
        Dim coll As New clsCollection()
        With coll.Definition
            .AddField("Name", DataType.text)
        End With

        ' Build up the sub-collection first
        With New clsCollectionRow()
            .Item("Name") = "HE" ' This isn't just H, there's extra chars after
            coll.Add(.It())
        End With

        With New clsCollectionRow()
            .Item("Name") = "x"  ' This isn't just x), there's extra chars after
            coll.Add(.It())
        End With

        With New clsCollectionRow()
            .Item("Name") = "s" & Chr(18) & Chr(10) & Chr(15) & Chr(129) & "e"
            coll.Add(.It())
        End With

        Dim xml = coll.GenerateXML()
        Dim coll2 As New clsCollection()
        coll2.Parse(xml)

        Assert.That(coll2, Iz.EqualTo(coll))

        ' Check all the initial values have at least 1 character (meaning the 
        ' control chars have copied across
        For Each x As clsCollectionRow In coll2.Rows
            Assert.That(x.Values(0).ToString.Length, Iz.GreaterThan(1))
        Next

    End Sub

    <Test>
    Public Sub TestPasswordsXml()
        Dim coll As New clsCollection()
        With coll.Definition
            .AddField("ID", DataType.number)
            .AddField("Name", DataType.text)
            .AddField("Word", DataType.password)
            .AddField("Past Words", DataType.collection)
        End With
        With coll.Definition.GetField("Past Words").Children
            .AddField("Word", DataType.password)
        End With

        ' Build up the sub-collection first
        With New clsCollectionRow()
            .Item("ID") = 1
            .Item("Name") = "Jack Daniels"
            .Item("Word") = New SafeString("Hello World")
            .Item("Past Words") = New clsCollection()
            With .Item("Past Words").Collection
                .Add(NewRow("Word", New SafeString("Goodbye")))
                .Add(NewRow("Word", New SafeString("Cruel")))
                .Add(NewRow("Word", "World"))
            End With
            coll.Add(.It())
        End With

        Dim xml = coll.GenerateXML()
        Dim coll2 As New clsCollection()
        coll2.Parse(xml)

        Assert.That(coll2, Iz.EqualTo(coll))

    End Sub

    <Test>
    Public Sub TestPasswordLegacyXml()
        Dim xml =
            <collection>
                <row>
                    <field name="ID" type="number" value="1"/>
                    <field name="Name" type="text" value="Jack Daniels"/>
                    <field name=" Word" type="password" value="Hello World"/>
                    <field name="Past Words" type="collection" value="&lt;collection&gt;&lt;row&gt;&lt;field name=&quot;Word&quot; type=&quot;password&quot; value=&quot;Goodbye&quot; /&gt;&lt;/row&gt;&lt;row&gt;&lt;field name=&quot;Word&quot; type=&quot;password&quot; value=&quot;Cruel&quot; /&gt;&lt;/row&gt;&lt;row&gt;&lt;field name=&quot;Word&quot; type=&quot;password&quot; value=&quot;World&quot; /&gt;&lt;/row&gt;&lt;/collection&gt;"/>
                </row>
            </collection>.ToString()

        Dim coll As New clsCollection()
        coll.Parse(xml)

        For Each r As clsCollectionRow In coll.Rows
            Assert.That(r("ID"), Iz.EqualTo(New clsProcessValue(1)))
            Assert.That(r("Name"), Iz.EqualTo(New clsProcessValue("Jack Daniels")))
            Assert.That(r(" Word"), Iz.EqualTo(New clsProcessValue(New SafeString("Hello World"))))
            Dim innerColl As clsCollection = r("Past Words").Collection
            Dim e = innerColl.Rows.GetEnumerator()
            Assert.That(e.MoveNext, Iz.True)
            Assert.That(e.Current("Word"), Iz.EqualTo(New clsProcessValue(New SafeString("Goodbye"))))
            Assert.That(e.MoveNext, Iz.True)
            Assert.That(e.Current("Word"), Iz.EqualTo(New clsProcessValue(New SafeString("Cruel"))))
            Assert.That(e.MoveNext, Iz.True)
            Assert.That(e.Current("Word"), Iz.EqualTo(New clsProcessValue(New SafeString("World"))))
        Next

    End Sub

    <Test>
    Public Sub TestParsingOfLegacyStyleCollectionXml()
        Dim xml =
            <collection>
                <row>
                    <field name="Inner" type="collection"
                        value="&lt;collection&gt;&lt;row&gt;&lt;field name=&quot; DateOfBirth&quot; type=&quot;date&quot; value=&quot;1974/12/14&quot; /&gt;&lt;field name=&quot;TimeOfSomething&quot; type=&quot;time&quot; value=&quot;10:30:00&quot; /&gt;&lt;/row&gt;&lt;/collection&gt;"/>
                    <field name="Name" type="text" value="Stu"/>
                </row>
            </collection>

        Dim coll = New clsCollection()
        coll.Parse(xml.ToString())

        Dim nameField = coll.GetField("Name")

        Assert.That(nameField, Iz.Not.Null)
        Assert.That(nameField.DataType, Iz.EqualTo(DataType.text))

        Dim innerField = coll.GetField("Inner")

        Assert.That(innerField, Iz.Not.Null)
        Assert.That(innerField.DataType, Iz.EqualTo(DataType.collection))

        Assert.That(coll.Rows, Has.Count.EqualTo(1))

        Dim row = coll.Row(0)
        Assert.That(row("Name"), Iz.EqualTo(CType("Stu", clsProcessValue)))

        Dim innerValue = row("Inner")
        Assert.That(innerValue, Iz.Not.Null)
        Assert.That(innerValue.DataType, Iz.EqualTo(DataType.collection))
        Assert.That(innerValue.HasCollectionData, Iz.True)

        Dim innerColl = innerValue.Collection
        Assert.That(innerColl, Iz.Not.Null)
        Assert.That(innerColl.Rows, Has.Count.EqualTo(1))

        Dim innerRow = innerColl.Row(0)
        Assert.That(innerRow, Iz.Not.Null)
        Assert.That(innerRow, Has.Count.EqualTo(2))

        Dim dobValue = innerRow(" DateOfBirth")
        Dim expectedDob = New clsProcessValue(DataType.date, New Date(1974, 12, 14))
        Assert.That(dobValue, Iz.EqualTo(expectedDob))

        Dim timeValue = innerRow("TimeOfSomething")
        Dim expectedTime = New clsProcessValue(DataType.time, New Date(1, 1, 1, 10, 30, 0))
        Assert.That(timeValue, Iz.EqualTo(expectedTime))

    End Sub

    <Test>
    Public Sub TestParsingOfNewStyleCollectionXml()
        Dim xml =
            <collection>
                <row>
                    <field name="Inner" type="collection">
                        <row>
                            <field name=" DateOfBirth" type="date" value="1974/12/14"/>
                            <field name="TimeOfSomething" type="time" value="10:30:00"/>
                        </row>
                    </field>
                    <field name="Name" type="text" value="Stu"/>
                </row>
            </collection>

        Dim coll = New clsCollection()
        coll.Parse(xml.ToString())

        Dim nameField = coll.GetField("Name")

        Assert.That(nameField, Iz.Not.Null)
        Assert.That(nameField.DataType, Iz.EqualTo(DataType.text))

        Dim innerField = coll.GetField("Inner")

        Assert.That(innerField, Iz.Not.Null)
        Assert.That(innerField.DataType, Iz.EqualTo(DataType.collection))

        Assert.That(coll.Rows, Has.Count.EqualTo(1))

        Dim row = coll.Row(0)
        Assert.That(row("Name"), Iz.EqualTo(CType("Stu", clsProcessValue)))

        Dim innerValue = row("Inner")
        Assert.That(innerValue, Iz.Not.Null)
        Assert.That(innerValue.DataType, Iz.EqualTo(DataType.collection))
        Assert.That(innerValue.HasCollectionData, Iz.True)

        Dim innerColl = innerValue.Collection
        Assert.That(innerColl, Iz.Not.Null)
        Assert.That(innerColl.Rows, Has.Count.EqualTo(1))

        Dim innerRow = innerColl.Row(0)
        Assert.That(innerRow, Iz.Not.Null)
        Assert.That(innerRow, Has.Count.EqualTo(2))

        Dim dobValue = innerRow(" DateOfBirth")
        Dim expectedDob = New clsProcessValue(DataType.date, New Date(1974, 12, 14))
        Assert.That(dobValue, Iz.EqualTo(expectedDob))

        Dim timeValue = innerRow("TimeOfSomething")
        Dim expectedTime = New clsProcessValue(DataType.time, New Date(1, 1, 1, 10, 30, 0))
        Assert.That(timeValue, Iz.EqualTo(expectedTime))

    End Sub

    <Test>
    Public Sub TestSerializationOfNestedCollectionXml()

        Dim coll As New clsCollection()
        coll.AddField("Id", DataType.number)
        coll.AddField("Name", DataType.text)
        coll.AddField("Exams", DataType.collection)
        With coll.GetFieldDefinition("Exams").Children
            .AddField("Exam Date", DataType.date)
            .AddField(" Exam Id ", DataType.number)
            .AddField("Exam Title", DataType.text)
        End With

        Dim examList = New clsCollectionRow()
        examList.Add("Id", New clsProcessValue(1))
        examList.Add("Name", New clsProcessValue("The Exams"))
        examList.Add("Exams", New clsProcessValue(New clsCollection()))

        Dim exam = New clsCollectionRow()
        exam.Add(" Exam Id ", New clsProcessValue(1))
        exam.Add("Exam Date", New clsProcessValue(DataType.datetime, New DateTime(2019, 1, 1)))
        exam.Add("Exam Title", New clsProcessValue("The Exam"))

        examList.Item("Exams").Collection.Add(exam)
        coll.Add(examList)

        Dim xml = coll.GenerateXML()
        Dim expectedXml = <collection>
                              <row>
                                  <field name="Id" type="number" value="1"/>
                                  <field name="Name" type="text" value="The Exams"/>
                                  <field name="Exams" type="collection">
                                      <row>
                                          <field name=" Exam Id " type="number" value="1"/>
                                          <field name="Exam Date" type="datetime" value="2019-01-01 00:00:00Z"/>
                                          <field name="Exam Title" type="text" value="The Exam"/>
                                      </row>
                                  </field>
                              </row>
                          </collection>.ToString(SaveOptions.DisableFormatting)
        Assert.That(xml, Iz.EqualTo(expectedXml))
    End Sub

    <Test>
    Public Sub TestXmlSerializationOfSingleRowCollection()
        Dim coll As New clsCollection()
        coll.AddField("Id", DataType.number)
        coll.AddField("Name", DataType.text)

        coll.SingleRow = True

        coll.Add()
        coll.SetField("Id", 1)
        coll.SetField("Name", "First")


        Dim xml = coll.GenerateXML()
        Dim expectedXml = <collection>
                              <singlerow/>
                              <row>
                                  <field name="Id" type="number" value="1"/>
                                  <field name="Name" type="text" value="First"/>
                              </row>
                          </collection>.ToString(SaveOptions.DisableFormatting)
        Assert.That(xml, Iz.EqualTo(expectedXml))
    End Sub

    <Test>
    Public Sub TestSerializationOfEmptyNestedCollectionXml()

        Dim coll As New clsCollection()
        coll.AddField("Id", DataType.number)
        coll.AddField("Name", DataType.text)
        coll.AddField("Exams", DataType.collection)
        With coll.GetFieldDefinition("Exams").Children
            .AddField("Exam Date", DataType.date)
            .AddField("Exam Id", DataType.number)
            .AddField("Exam Title", DataType.text)
        End With

        Dim examList = New clsCollectionRow()
        examList.Add("Id", New clsProcessValue(1))
        examList.Add("Name", New clsProcessValue("The Exams"))
        examList.Add("Exams", New clsProcessValue(New clsCollection()))

        coll.Add(examList)

        Dim xml = coll.GenerateXML()
        Dim expectedXml = <collection>
                              <row>
                                  <field name="Id" type="number" value="1"/>
                                  <field name="Name" type="text" value="The Exams"/>
                                  <field name="Exams" type="collection"/>
                              </row>
                          </collection>.ToString(SaveOptions.DisableFormatting)
        Assert.That(xml, Iz.EqualTo(expectedXml))
    End Sub

    <Test>
    Public Sub TestSeveralLayersOfNestedCollection()
        Dim coll As New clsCollection()
        coll.AddField("Id", DataType.number)
        coll.AddField("Name", DataType.text)
        coll.AddField("Exams", DataType.collection)
        With coll.GetFieldDefinition("Exams").Children
            .AddField("Exam Date", DataType.date)
            .AddField("Exam ID", DataType.number)
            .AddField(" Exam Title ", DataType.text)
            .AddField("Invigilators", DataType.collection)
            With .GetField("Invigilators").Children
                .AddField("Id", DataType.number)
                .AddField("Name", DataType.text)
                .AddField("Promotions", DataType.collection)
                With .GetField("Promotions").Children
                    .AddField(" Date ", DataType.date)
                    .AddField("Salary", DataType.number)
                End With
            End With
            .AddField("Students", DataType.collection)
            With .GetField("Students").Children
                .AddField("Id", DataType.number)
                .AddField("Name", DataType.text)
            End With
        End With

        Dim invigilatorsColl As New clsCollection(
            <collection>
                <row>
                    <field name="Id" type="number" value="1"/>
                    <field name="Name" type="text" value="Fred"/>
                    <field name="Promotions" type="collection">
                        <row>
                            <field name=" Date " type="date" value="2015/01/06"/>
                            <field name="Salary" type="number" value="25000"/>
                        </row>
                        <row>
                            <field name=" Date " type="date" value="2016/01/01"/>
                            <field name="Salary" type="number" value="27500"/>
                        </row>
                        <row>
                            <field name=" Date " type="date" value="2017/06/01"/>
                            <field name="Salary" type="number" value="30000"/>
                        </row>
                    </field>
                </row>
                <row>
                    <field name="Id" type="number" value="2"/>
                    <field name="Name" type="text" value="Daphne"/>
                    <field name="Promotions" type="collection">
                        <row>
                            <field name=" Date " type="date" value="2018/01/01"/>
                            <field name="Salary" type="number" value="27000"/>
                        </row>
                    </field>
                </row>
                <row>
                    <field name="Id" type="number" value="3"/>
                    <field name="Name" type="text" value="Velma"/>
                    <field name="Promotions" type="collection">
                        <row>
                            <field name=" Date " type="date" value="2016/01/01"/>
                            <field name="Salary" type="number" value="32000"/>
                        </row>
                        <row>
                            <field name=" Date " type="date" value="2018/01/01"/>
                            <field name="Salary" type="number" value="35000"/>
                        </row>
                    </field>
                </row>
                <row>
                    <field name="Id" type="number" value="4"/>
                    <field name="Name" type="text" value="Shaggy"/>
                    <field name="Promotions" type="collection"/>
                </row>
            </collection>.ToString()
        )

        With coll.Row(coll.Add())
            .Item("Id") = 1
            .Item("Name") = "Sensibility"

            Dim examsColl As New clsCollection()
            AddRow(examsColl, New Dictionary(Of String, clsProcessValue) From {
                {"Exam Date", New Date(2009, 12, 17)},
                {"Exam ID", 1},
                {" Exam Title ", "Advanced Antipathy"},
                {"Invigilators", invigilatorsColl.Clone()}
            })
            AddRow(examsColl, New Dictionary(Of String, clsProcessValue) From {
                {"Exam Date", New Date(2009, 12, 18)},
                {"Exam ID", 2},
                {" Exam Title ", "Advanced Apathy"},
                {"Invigilators", invigilatorsColl.Clone()}
            })
            AddRow(examsColl, New Dictionary(Of String, clsProcessValue) From {
                {"Exam Date", New Date(2009, 12, 19)},
                {"Exam ID", 3},
                {" Exam Title ", "Advanced Allelopathy"},
                {"Invigilators", invigilatorsColl.Clone()}
            })
            AddRow(examsColl, New Dictionary(Of String, clsProcessValue) From {
                {"Exam Date", New Date(2009, 12, 20)},
                {"Exam ID", 4},
                {" Exam Title ", "Advanced Allopathy"},
                {"Invigilators", invigilatorsColl.Clone()}
            })

            .Item("Exams") = examsColl.Clone()

        End With

        ' So that's coll -> Exams -> Invigilators -> Promotions, all containing data
        ' Plus a few nested collections elsewhere that don't
        Dim xml = coll.GenerateXML()
        Dim roundTripColl = New clsCollection(xml)
        Assert.That(roundTripColl, Iz.EqualTo(coll))

        Dim roundTripXml = roundTripColl.GenerateXML()
        Assert.That(roundTripXml, Iz.EqualTo(xml))

    End Sub

    <Test>
    Public Sub TestSingleRowCollection()
        Dim source As New clsCollection()
        source.SingleRow = True
        Dim clone = source.Clone()
        source.Rows.Should().HaveCount(1, "because 'SingleRow' should add a row")
        clone.SingleRow.Should().BeTrue()
        clone.Rows.Should().HaveCount(1, "because clone should have the same rows as source")
    End Sub

    Private Sub AddRow(
     coll As clsCollection, map As IDictionary(Of String, clsProcessValue))
        Dim row As New clsCollectionRow()
        For Each entry In map : row(entry.Key) = entry.Value : Next
        coll.Add(row)
    End Sub

    <Test>
    Public Sub TestDeletingDuplicatedRowDeletesCorrectRow()
        Dim xml =
            <collection>
                <row>
                    <field name=" Name" type="text" value="Abbey"/>
                </row>
                <row>
                    <field name=" Name" type="text" value="Boris"/>
                </row>
                <row>
                    <field name=" Name" type="text" value="Boris"/>
                </row>
                <row>
                    <field name=" Name" type="text" value="Claude"/>
                </row>
                <row>
                    <field name=" Name" type="text" value="Doris"/>
                </row>
            </collection>.ToString()

        Dim coll As New clsCollection()
        coll.Parse(xml)

        Dim originalPositionOfDuplicate As Integer = 2
        coll.SetCurrentRow(originalPositionOfDuplicate)

        Dim row As clsCollectionRow = coll.GetCurrentRow()

        coll.DeleteCurrentRow()

        Assert.That(coll.ContainsByReference(row), Iz.False)
        Assert.That(coll.Rows.Count, Iz.EqualTo(4))
        Assert.That(coll.Row(0)(" Name").FormattedValue, Iz.EqualTo("Abbey"))
        Assert.That(coll.Row(1)(" Name").FormattedValue, Iz.EqualTo("Boris"))
        Assert.That(coll.Row(2)(" Name").FormattedValue, Iz.EqualTo("Claude"))
        Assert.That(coll.Row(3)(" Name").FormattedValue, Iz.EqualTo("Doris"))
    End Sub

    <Test>
    Public Sub CheckExceptionThrownSettingFieldIfNoCurrentRow()
        Dim collection As New clsCollection()

        Assert.Throws(Of NoCurrentRowException)(
            Sub() collection.SetField("ID", New clsProcessValue(1)),
            "Cannot set ID: The collection has no current row")
    End Sub

    <Test>
    <TestCase("first.second", "first", "second")>
    <TestCase("first.second.third", "first", "second.third")>
    <TestCase("first", "first", "")>
    <TestCase(" spaceBefore.spaceAfter ", " spaceBefore", "spaceAfter ")>
    Public Sub TestSplittingFieldsForNestedProperties(path As String, first As String, second As String)
        Dim result = clsCollection.SplitPath(path)
        Assert.That(result.Item1, Iz.EqualTo(first))
        Assert.That(result.Item2, Iz.EqualTo(second))
    End Sub

    <TestCase("")>
    <TestCase(".")>
    <TestCase("first.")>
    <TestCase("first. ")>
    <TestCase(".first")>
    <TestCase("first..second")>
    Public Sub TestSplittingFieldsForNestedPropertiesThrowsExceptionForInvalidPath(path As String)
        Assert.Throws(Of InvalidFormatException)(
            Sub() clsCollection.SplitPath(path),
            $"Invalid collection path to split: {path}")
    End Sub

    <Test>
    Public Sub CheckExceptionThrownSettingFieldIfNoFieldIsFound()
        Dim collection As New clsCollection(New clsCollectionRow())

        Assert.Throws(Of FieldNotFoundException)(
            Sub() collection.SetField("ID", New clsProcessValue(1)),
            "The field 'ID' doesn't exist within this collection")
    End Sub

    <Test>
    Public Sub CheckExceptionThrownSettingFieldIfDataTypesDoNotMatchWithoutCasting()
        Dim collection As New clsCollection()
        collection.AddField("ID", DataType.number)
        collection.Add()

        Assert.Throws(Of InvalidTypeException)(
            Sub() collection.SetField("ID", "1", tryCasting:=False),
            "Cannot set a text value into the field: ID - its data type is number")
    End Sub

    <Test>
    Public Sub TestSetDirectFieldsOfCollection()
        Dim collection As New clsCollection()
        collection.AddField("Date", DataType.date)
        collection.AddField("DateTime", DataType.datetime)
        collection.AddField("Flag", DataType.flag)
        collection.AddField("Number", DataType.number)
        collection.AddField("Password", DataType.password)
        collection.AddField("Text", DataType.text)
        collection.AddField(" Text With Spaces ", DataType.text)
        collection.AddField("Time", DataType.time)
        collection.AddField("TimeSpan", DataType.timespan)
        collection.AddField("Unknown", DataType.unknown)
        collection.Add()

        collection.SetField("Date", New clsProcessValue(DataType.date, Date.Today))
        collection.SetField("DateTime", New clsProcessValue(DataType.datetime, DateTime.Today))
        collection.SetField("Flag", New clsProcessValue(True))
        collection.SetField("Number", New clsProcessValue(1))
        collection.SetField("Password", New clsProcessValue(New SafeString("Password")))
        collection.SetField("Text", New clsProcessValue("Some Text"))
        collection.SetField(" Text With Spaces ", New clsProcessValue("Some Other Text"))
        collection.SetField("Time", New clsProcessValue(DataType.time, "10:30:00"))
        collection.SetField("TimeSpan", New clsProcessValue(New TimeSpan(0, 0, 5)))
        collection.SetField("Unknown", New clsProcessValue(DataType.unknown, ""))

        Assert.That(collection.GetField("Date"), Iz.EqualTo(New clsProcessValue(DataType.date, Date.Today)))
        Assert.That(collection.GetField("DateTime"), Iz.EqualTo(New clsProcessValue(DataType.datetime, DateTime.Today)))
        Assert.That(collection.GetField("Flag"), Iz.EqualTo(New clsProcessValue(True)))
        Assert.That(collection.GetField("Number"), Iz.EqualTo(New clsProcessValue(1)))
        Assert.That(collection.GetField("Password"), Iz.EqualTo(New clsProcessValue(New SafeString("Password"))))
        Assert.That(collection.GetField("Text"), Iz.EqualTo(New clsProcessValue("Some Text")))
        Assert.That(collection.GetField(" Text With Spaces "), Iz.EqualTo(New clsProcessValue("Some Other Text")))
        Assert.That(collection.GetField("Time"), Iz.EqualTo(New clsProcessValue(DataType.time, "10:30:00")))
        Assert.That(collection.GetField("TimeSpan"), Iz.EqualTo(New clsProcessValue(New TimeSpan(0, 0, 5))))
        Assert.That(collection.GetField("Unknown"), Iz.EqualTo(New clsProcessValue(DataType.unknown, "")))
    End Sub

    <Test>
    Public Sub TestSetDirectFieldOfCollectionWithParsing()
        Dim collection As New clsCollection()
        collection.AddField("Number", DataType.number)
        collection.Add()

        collection.SetField("Number", New clsProcessValue("1"), tryCasting:=True)

        Assert.That(collection.GetField("Number"), Iz.EqualTo(New clsProcessValue(1)))
    End Sub

    <Test>
    Public Sub CheckExceptionIsThrownSettingNestedPropertyIfFieldIsNotACollection()
        Dim collection As New clsCollection()
        collection.AddField("Number", DataType.number)
        collection.Add()

        Assert.Throws(Of InvalidFormatException)(
            Sub() collection.SetField("Number.Property", New clsProcessValue("1")),
            "Field 'Number' is not a collection (number), but sub-field notation was used")
    End Sub

    <Test>
    Public Sub CheckExceptionIsThrownSettingNestedPropertyOnEmptyCollection()
        Dim collection As New clsCollection()
        collection.AddField("Items", DataType.collection)
        collection.Add()

        Assert.Throws(Of NoCurrentRowException)(
            Sub() collection.SetField("Items.ID", New clsProcessValue(1)),
            "Cannot set Items.ID: The collection Items has no current row")
    End Sub

    <Test>
    Public Sub CheckExceptionIsThrownSettingNestedPropertyIfNoFieldIsFound()
        Dim collection As New clsCollection()
        collection.AddField("Items", DataType.collection)
        collection.Add()

        Dim items As New clsCollection()
        items.Add()
        collection.SetField("Items", New clsProcessValue(items))

        Assert.Throws(Of FieldNotFoundException)(
            Sub() collection.SetField("Items.Id", New clsProcessValue(1)),
            "The field 'ID' doesn't exist within the collection 'Items'")
    End Sub

    <Test>
    Public Sub TestSettingANestedProperty()
        Dim collection As New clsCollection()
        collection.AddField("Items", DataType.collection)
        collection.Add()

        Dim items As New clsCollection()
        items.AddField("Id", DataType.number)
        items.Add()
        collection.SetField("Items", New clsProcessValue(items))

        collection.SetField("Items.Id", New clsProcessValue(1))

        Assert.That(collection.GetField("Items.Id"), Iz.EqualTo(New clsProcessValue(1)))
    End Sub

    <Test>
    Public Sub TestSettingANestedProperty_WithSpaces()
        Dim collection As New clsCollection()
        collection.AddField("Items ", DataType.collection)
        collection.Add()

        Dim items As New clsCollection()
        items.AddField(" Id", DataType.number)
        items.Add()
        collection.SetField("Items ", New clsProcessValue(items))

        collection.SetField("Items . Id", New clsProcessValue(1))

        Assert.That(collection.GetField("Items . Id"), Iz.EqualTo(New clsProcessValue(1)))
    End Sub

    <Test>
    Public Sub TestCountRows_ShouldReturnCorrectNumber()
        Dim testColl As New clsCollection(
            <collection>
                <row><field name="Id" type="number" value="1"/></row>
                <row><field name="Name" type="text" value="Fred"/></row>
                <row>
                    <field name="Name" type="text" value="Fred"/>
                    <field name="Date" type="date" value="2015/01/06"/>
                </row>
            </collection>.ToString)

        Assert.That(testColl.Count, [Is].EqualTo(3))
    End Sub

    <Test>
    Public Sub TestCountRows_WithEmptyCollection_ShouldReturnZero()
        Dim testColl As New clsCollection()
        Assert.That(testColl.Count, [Is].EqualTo(0))
    End Sub

    <Test>
    Public Sub TestCountRows_WithNestedCollection_CountsOnlyTopLevelCollectionRows()
        Dim testColl As New clsCollection(
            <collection>
                <row>
                    <field name="Id" type="number" value="1"/>
                    <field name="Name" type="text" value="Fred"/>
                    <field name="Promotions" type="collection">
                        <row>
                            <field name="Date" type="date" value="2015/01/06"/>
                            <field name="Salary" type="number" value="25000"/>
                        </row>
                        <row>
                            <field name="Date" type="date" value="2016/01/01"/>
                            <field name="Salary" type="number" value="27500"/>
                        </row>
                    </field>
                </row>
                <row>
                    <field name="Id" type="number" value="2"/>
                    <field name="Name" type="text" value="Daphne"/>
                    <field name="Promotions" type="collection">
                        <row>
                            <field name="Date" type="date" value="2018/01/01"/>
                            <field name="Salary" type="number" value="27000"/>
                        </row>
                    </field>
                </row>
                <row>
                    <field name="Id" type="number" value="3"/>
                    <field name="Name" type="text" value="Velma"/>
                    <field name="Promotions" type="collection">
                        <row>
                            <field name="Date" type="date" value="2016/01/01"/>
                            <field name="Salary" type="number" value="32000"/>
                        </row>
                    </field>
                </row>
                <row>
                    <field name="Id" type="number" value="4"/>
                    <field name="Name" type="text" value="Shaggy"/>
                    <field name="Promotions" type="collection"/>
                </row>
            </collection>.ToString()
        )

        Assert.That(testColl.Count, [Is].EqualTo(4))
    End Sub

    <Test>
    Public Sub TestCountColumns_ShouldReturnCorrectNumber()

        Dim testColl As New clsCollection(
            <collection>
                <row><field name="Id" type="number" value="1"/></row>
                <row><field name="Name" type="text" value="Fred"/></row>
                <row>
                    <field name="Name" type="text" value="Fred"/>
                    <field name="Date" type="date" value="2015/01/06"/>
                    <field name="Type" type="text" value="New Customer"/>
                </row>
            </collection>.ToString)

        Assert.That(testColl.FieldCount, [Is].EqualTo(4))
    End Sub

    <Test>
    Public Sub TestCountColumns_WithEmptyCollection_ShouldReturnZero()
        Dim testColl As New clsCollection()
        Assert.That(testColl.FieldCount, [Is].EqualTo(0))
    End Sub

    <Test>
    Public Sub TestCountColumns_WithNestedCollection_CountsOnlyTopLevelCollectionFields()
        Dim testColl As New clsCollection(
            <collection>
                <row>
                    <field name="Id" type="number" value="1"/>
                    <field name="Name" type="text" value="Fred"/>
                    <field name="Promotions" type="collection">
                        <row>
                            <field name="Date" type="date" value="2015/01/06"/>
                            <field name="Salary" type="number" value="25000"/>
                        </row>
                        <row>
                            <field name="Date" type="date" value="2016/01/01"/>
                            <field name="Salary" type="number" value="27500"/>
                        </row>
                    </field>
                </row>
                <row>
                    <field name="CustomerRef" type="number" value="2"/>
                    <field name="Name" type="text" value="Daphne"/>
                    <field name="Payments" type="collection">
                        <row>
                            <field name="Date" type="date" value="2018/01/01"/>
                            <field name="Amount" type="number" value="27000"/>
                        </row>
                    </field>
                </row>
            </collection>.ToString()
        )

        Assert.That(testColl.FieldCount, [Is].EqualTo(5))
    End Sub

End Class

#End If

