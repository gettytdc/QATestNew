#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.UnitTesting.TestSupport
Imports NUnit.Framework

Namespace DataContractRoundTrips


    ''' <summary>
    ''' Tests that serializing - deserializing a <see cref="clsCollection" /> object, 
    ''' will return an object that is the same. This in turn tests the 
    ''' <see cref="clsCollectionRow" /> class as the Equals function goes through and
    ''' compares each row in the collection and also all of the data
    ''' members in <see cref="clsCollectionInfo" /> and <see cref="clsCollectionFieldInfo" /> 
    ''' which are also serialized whenever  <see cref="clsCollection" /> is serialized.
    ''' </summary>
    ''' <remarks>
    ''' This is separate to the tests generated using the test case generators because
    ''' fluent assertions seems to fall over itself when trying to compare two
    ''' clsCollection objects. Just use the Equals function defined within clsCollection
    ''' instead.
    ''' </remarks>
    <TestFixture>
    Public Class CollectionRoundTripTest

        <Test>
        Public Sub CollectionRoundTripTest_EmptyCollection()

            Dim coll As New clsCollection()
            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)
            Assert.That(coll.Equals(roundTrip), Iz.True)
            Assert.That(coll.Definition.Equals(roundTrip.Definition))

        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_EmptyCollectionWithDefinition()

            Dim coll As New clsCollection()

            coll.Definition.AddField("text", DataType.text)
            coll.Definition.AddField("time", DataType.time)
            coll.Definition.AddField("datetime", DataType.datetime)
            coll.Definition.AddField("date", DataType.date)
            coll.Definition.AddField("number", DataType.number)
            coll.Definition.AddField("binary", DataType.binary)
            coll.Definition.AddField("collection", DataType.collection)
            coll.Definition.AddField("flag", DataType.flag)

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)
            Assert.That(coll.Equals(roundTrip))
            Assert.That(coll.Definition.Equals(roundTrip.Definition))

        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_SingleRowCollection()

            Dim coll = TestHelper.CreateCollectionWithData(1)
            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)
            Assert.That(coll.Equals(roundTrip))

        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_CollectionWithTwoRows()

            Dim coll = TestHelper.CreateCollectionWithData(2)
            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)
            Assert.That(coll.Equals(roundTrip))

        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_CurrentRow()

            Dim coll = TestHelper.CreateCollectionWithData(2)
            coll.SetCurrentRow(1)

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)

            Assert.That(roundTrip.CurrentRowIndex.Equals(1))

        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_CollectionRow_DeletedFlag()

            Dim coll = TestHelper.CreateCollectionWithData(2)
            TestCaseGenerator.SetField(coll, "mCurrentRowDeleted", True)

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)

            Assert.That(TestCaseGenerator.GetField(roundTrip, "mCurrentRowDeleted").Equals(True))

        End Sub

        <TestCase(100)>
        <TestCase(1000)>
        <TestCase(10000)>
        Public Sub CollectionRoundTripTest_Definition_MultipleFields(numOfFields As Integer)

            Dim coll As New clsCollection()
            For i = 1 To numOfFields Step 1
                coll.Definition.AddField(String.Format("Text{0}", i), DataType.text)
            Next

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)

            Assert.That(coll.Equals(roundTrip))
            Assert.That(coll.Definition.Equals(roundTrip.Definition))

        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_Definition_NestingElement()

            Dim coll As New clsCollection()
            coll.Definition.NestingElement = "el"

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)

            Assert.That(roundTrip.Definition.NestingElement.Equals("el"))

        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_Definition_SingleRow()

            Dim coll As New clsCollection()
            coll.Definition.SingleRow = True

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)

            Assert.That(roundTrip.Definition.SingleRow, Iz.True)

        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_Definition_Flat()

            Dim coll As New clsCollection()
            coll.Definition.Flat = True

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)

            Assert.That(roundTrip.Definition.Flat, Iz.True)

        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_FieldDefinition_Name()

            Dim coll As New clsCollection()
            coll.Definition.AddField("Level 1", DataType.text)

            Dim field = coll.FieldDefinitions.First

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)
            Dim roundTripField1 = roundTrip.FieldDefinitions.First

            Assert.That(field.Name, [Iz].Not.Null.And.Not.Empty)
            Assert.That(roundTripField1.Name, [Iz].Not.Null.And.Not.Empty)
            Assert.That(roundTripField1.Name.Equals("Level 1"))


        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_FieldDefinition_Description()

            Dim coll As New clsCollection()
            coll.Definition.AddField("Level 1", DataType.text)

            Dim field = coll.FieldDefinitions.First
            field.Description = "Some text describing something"

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)
            Dim roundTripField1 = roundTrip.FieldDefinitions.First

            Assert.That(field.Description, [Iz].Not.Null.And.Not.Empty)
            Assert.That(roundTripField1.Description, [Iz].Not.Null.And.Not.Empty)
            Assert.That(roundTripField1.Description.Equals("Some text describing something"))

        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_FieldDefinition_DataType()

            Dim coll As New clsCollection()

            coll.Definition.AddField("text", DataType.text)
            coll.Definition.AddField(" text with spaces ", DataType.text)
            coll.Definition.AddField("time", DataType.time)
            coll.Definition.AddField("datetime", DataType.datetime)
            coll.Definition.AddField("date", DataType.date)
            coll.Definition.AddField("number", DataType.number)
            coll.Definition.AddField("binary", DataType.binary)
            coll.Definition.AddField("collection", DataType.collection)
            coll.Definition.AddField("flag", DataType.flag)

            Dim dataTypes = coll.Definition.FieldDefinitions.Select(Function(x) x.DataType)

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)
            Dim roundTripDataTypes = roundTrip.Definition.FieldDefinitions.Select(Function(x) x.DataType)

            Assert.That(dataTypes.SequenceEqual(roundTripDataTypes))

        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_FieldDefinition_Parent()

            Dim coll As New clsCollection()
            coll.Definition.AddField("Level 1", DataType.text)

            Dim field1 = coll.FieldDefinitions.First

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)
            Dim roundTripField1 = roundTrip.FieldDefinitions.First

            Assert.That(roundTripField1.Parent.Equals(coll.Definition))


        End Sub


        <Test(Description:="Tests that the OnDeserialized attribute used to populate
the Parent on clsCollectionInfo in DataContract Serialization does not adversely
affect Binary Serialization")>
        Public Sub CollectionRoundTripTest_FieldDefinition_Parent_Binary()

            Dim coll As New clsCollection()
            coll.Definition.AddField("Level 1", DataType.text)

            Dim field1 = coll.FieldDefinitions.First

            Dim roundTrip = ServiceUtil.DoBinarySerializationRoundTrip(coll)
            Dim roundTripField1 = roundTrip.FieldDefinitions.First

            Assert.That(roundTripField1.Parent.Equals(coll.Definition))


        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_FieldDefinition_Children()

            Dim coll As New clsCollection()
            coll.Definition.AddField("Level 1", DataType.collection)

            Dim field1 = coll.FieldDefinitions.First
            field1.Children.AddField("Level 2", DataType.collection)

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)
            Dim roundTripField1 = roundTrip.Definition.FieldDefinitions.First

            Assert.That(field1.HasChildren)
            Assert.That(roundTripField1.HasChildren)
            Assert.That(field1.Children.Equals(roundTripField1.Children))

        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_FieldDefinition_Namespace()

            Dim coll As New clsCollection()
            coll.Definition.AddField("Level 1", DataType.collection, "ns1")

            Dim field1 = coll.FieldDefinitions.First

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)
            Dim roundTripField1 = roundTrip.FieldDefinitions.First

            Assert.That(field1.Namespace, [Iz].Not.Null)
            Assert.That(roundTripField1.Namespace, [Iz].Not.Null)
            Assert.That(field1.Namespace.Equals(roundTripField1.Namespace))

        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_FieldDefinition_Child_ParentField()

            Dim coll As New clsCollection()
            coll.Definition.AddField("Level 1", DataType.collection)

            Dim field1 = coll.FieldDefinitions(0)
            field1.Children.AddField("Level 2", DataType.collection)

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)
            Dim roundTripField1 = roundTrip.FieldDefinitions.First

            Assert.That(roundTripField1.Children.ParentField, [Iz].Not.Null)
            Assert.That(roundTripField1.Children.ParentField, [Iz].Not.Null)
            Assert.That(roundTripField1.Children.ParentField.
                           Equals(roundTripField1.Children.ParentField))
        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_FieldDefinition_Child_ParentFieldParent()

            Dim coll As New clsCollection()
            coll.Definition.AddField("Level 1", DataType.collection)

            Dim field1 = coll.FieldDefinitions.First()
            field1.Children.AddField("Level 2", DataType.collection)

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)
            Dim roundTripField1 = roundTrip.FieldDefinitions.First

            Assert.That(field1.Children.ParentField.Parent.
                           Equals(roundTripField1.Children.ParentField.Parent))


        End Sub

        <Test(Description:="Tests that the OnDeserialized attribute used to populate
the ParentField.Parent on clsCollectionInfo in DataContract Serialization does not 
adversely affect Binary Serialization")>
        Public Sub CollectionRoundTripTest_FieldDefinition_Child_ParentFieldParent_Binary()

            Dim coll As New clsCollection()
            coll.Definition.AddField("Level 1", DataType.collection)

            Dim field1 = coll.FieldDefinitions.First()
            field1.Children.AddField("Level 2", DataType.collection)

            Dim roundTrip = ServiceUtil.DoBinarySerializationRoundTrip(coll)
            Dim roundTripField1 = roundTrip.FieldDefinitions.First

            Assert.That(field1.Children.ParentField.Parent.
                           Equals(roundTripField1.Children.ParentField.Parent))

        End Sub

        <Test>
        Public Sub CollectionRoundTripTest_FieldDefinition_GrandChild_ParentFieldParent()

            Dim coll As New clsCollection()
            coll.Definition.AddField("Level 1", DataType.collection)

            Dim field1 = coll.FieldDefinitions.First()
            field1.Children.AddField("Level 2  ", DataType.collection)

            Dim field2 = field1.Children.FieldDefinitions.First
            field2.Children.AddField(" Level 3", DataType.collection)

            Dim roundTrip = ServiceUtil.DoDataContractRoundTrip(coll)
            Dim roundTripField2 = roundTrip.FieldDefinitions.First.Children.FieldDefinitions.First

            Assert.That(field2.Children.ParentField.Parent.
                           Equals(roundTripField2.Children.ParentField.Parent))

        End Sub


    End Class

End Namespace
#End If
