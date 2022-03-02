namespace BluePrism.Api.BpLibAdapters.UnitTests.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Drawing;
    using System.Linq;
    using System.Security;
    using System.Text;
    using AutomateProcessCore;
    using BpLibAdapters.Mappers;
    using CommonTestClasses;
    using Domain;
    using FluentAssertions;
    using NUnit.Framework;
    using DataValueType = Domain.DataValueType;

    [TestFixture(Category = "Unit Test")]
    public class DataCollectionMapperTests
    {
        [Test]
        public void DataCollection_ToBluePrismObject__WithOneRow_ShouldReturnMappedBluePrismCollection()
        {
            var dictionaryValues = new Dictionary<string, DataValue>
            {
                {"Key1", new DataValue() {Value = 1, ValueType = DataValueType.Number}}

            };
            var list = new List<IReadOnlyDictionary<string, DataValue>> { dictionaryValues };
            var dictionary = new ReadOnlyCollection<IReadOnlyDictionary<string, DataValue>>(list);

            var dataCollection = new DataCollection()
            {
                Rows = dictionary
            };

            var result = dataCollection.ToBluePrismObject();
            result.Should().BeOfType<clsCollection>();
            result.Rows.Should().HaveCount(1);
            result.Rows.Single().Should().HaveCount(dictionaryValues.Count);
        }

        [Test]
        public void DataCollection_ToBluePrismObject__WithMoreThanOneRow_ShouldReturnMappedBluePrismCollection()
        {
            var dictionaryValues = new Dictionary<string, DataValue>
            {
                {"Key1", new DataValue() {Value = 1, ValueType = DataValueType.Number}},
                {"Key2", new DataValue() {Value = 1, ValueType = DataValueType.Number}},
                {"Key3", new DataValue() {Value = 1, ValueType = DataValueType.Number}}
            };
            var list = new List<IReadOnlyDictionary<string, DataValue>> { dictionaryValues };
            var dictionary = new ReadOnlyCollection<IReadOnlyDictionary<string, DataValue>>(list);

            var dataCollection = new DataCollection()
            {
                Rows = dictionary
            };

            var result = dataCollection.ToBluePrismObject();
            result.Should().BeOfType<clsCollection>();
            result.Rows.Count.Should().Be(1);
            result.Rows.First().Count.Should().Be(dictionaryValues.Count);
        }


        [Test]
        public void DataValue_ToBluePrismObject_WithValidNumber_ShouldReturnMappedNumberValue()
        {
            var result = new DataValue() { Value = 1, ValueType = DataValueType.Number }.ToBluePrismObject();
            result.Should().BeOfType<clsProcessValue>();
            result.Should().Be(1);
            result.DataType.Should().Be(DataType.number);
        }

        [Test]
        public void DataValue_ToBluePrismObject_WithValidText_ShouldReturnMappedText()
        {
            var result = new DataValue() { Value = "Some Text", ValueType = DataValueType.Text }.ToBluePrismObject();
            result.Should().BeOfType<clsProcessValue>();
            result.Should().Be("Some Text");
            result.DataType.Should().Be(DataType.text);
        }

        [Test]
        public void DataValue_ToBluePrismObject_WithNullText_ShouldReturnMappedText()
        {
            var result = new DataValue() { Value = null, ValueType = DataValueType.Text }.ToBluePrismObject();
            result.Should().BeOfType<clsProcessValue>();
            result.Should().Be(string.Empty);
            result.DataType.Should().Be(DataType.text);
        }

        [Test]
        public void DataValue_ToBluePrismObject_WithEmptyString_ShouldReturnMappedText()
        {
            var result = new DataValue() { Value = string.Empty, ValueType = DataValueType.Text }.ToBluePrismObject();
            result.Should().BeOfType<clsProcessValue>();
            result.Should().Be(string.Empty);
            result.DataType.Should().Be(DataType.text);
        }

        [Test]
        public void DataValue_ToBluePrismObject_WithValidByteArray_ShouldReturnMappedByteArray()
        {
            var byteArray = Encoding.ASCII.GetBytes("Some Data");
            var result = new DataValue() { Value = byteArray, ValueType = DataValueType.Binary }.ToBluePrismObject();
            result.Should().BeOfType<clsProcessValue>();
            ((byte[])result).SequenceEqual(byteArray).Should().Be(true);
            result.DataType.Should().Be(DataType.binary);
        }

        [Test]
        public void DataValue_ToBluePrismObject_WithValidBitmap_ShouldReturnMappedByteArray()
        {
            var image = new Bitmap(100, 200);
            var result = new DataValue() { Value = image, ValueType = DataValueType.Image }.ToBluePrismObject();
            result.Should().BeOfType<clsProcessValue>();
            result.IsValid.Should().Be(true);
            result.DataType.Should().Be(DataType.image);
        }

        [Test]
        public void DataValue_ToBluePrismObject_WithValidBooleanTrue_ShouldReturnMappedBooleanValue()
        {
            var result = new DataValue() { Value = true, ValueType = DataValueType.Flag }.ToBluePrismObject();
            result.Should().BeOfType<clsProcessValue>();
            result.Should().Be(true);
            result.DataType.Should().Be(DataType.flag);
        }

        [Test]
        public void DataValue_ToBluePrismObject_WithValidBooleanFalse_ShouldReturnMappedBooleanValue()
        {
            var result = new DataValue() { Value = false, ValueType = DataValueType.Flag }.ToBluePrismObject();
            result.Should().BeOfType<clsProcessValue>();
            result.Should().Be(false);
            result.DataType.Should().Be(DataType.flag);
        }

        [Test]
        public void DataValue_ToBluePrismObject_WithValidCollection_ShouldReturnMappedCollectionValue()
        {
            var dictionaryValues = new Dictionary<string, DataValue>
            {
                {"Key1", new DataValue() {Value = 1, ValueType = DataValueType.Number}}

            };
            var list = new List<IReadOnlyDictionary<string, DataValue>> { dictionaryValues };
            var dictionary = new ReadOnlyCollection<IReadOnlyDictionary<string, DataValue>>(list);

            var dataCollection = new DataCollection()
            {
                Rows = dictionary
            };

            var result = new DataValue() { Value = dataCollection, ValueType = DataValueType.Collection }.ToBluePrismObject();
            result.Should().BeOfType<clsProcessValue>();
            result.Collection.Rows.Count.Should().Be(1);
            result.Collection.Rows.First().Count.Should().Be(dictionaryValues.Count);
            result.DataType.Should().Be(DataType.collection);
        }

        [Test]
        public void DataValue_ToBluePrismObject_WithValidSafeString_ShouldReturnMappedPasswordValue()
        {
            var result = new DataValue() { Value = DataHelper.ToSecureString("Password"), ValueType = DataValueType.Password }.ToBluePrismObject();
            result.Should().BeOfType<clsProcessValue>();
            result.IsValid.Should().Be(true);
            result.DataType.Should().Be(DataType.password);
        }

        [Test]
        public void DataValue_ToBluePrismObject_WithValidDateTimeOffset_ShouldReturnMappedDateTimeValue()
        {
            var result = new DataValue() { Value = new DateTimeOffset(2020, 1, 1, 20, 0, 0, TimeSpan.Zero), ValueType = DataValueType.DateTime }.ToBluePrismObject();
            result.Should().BeOfType<clsProcessValue>();
            result.IsValid.Should().Be(true);
            result.DataType.Should().Be(DataType.datetime);
        }

        [Test]
        public void DataValue_ToBluePrismObject_WithValidDate_ShouldReturnMappedDateTimeValue()
        {
            var result = new DataValue() { Value = new DateTime(2020, 1, 1, 0, 0, 0), ValueType = DataValueType.Date }.ToBluePrismObject();
            result.Should().BeOfType<clsProcessValue>();
            result.IsValid.Should().Be(true);
            result.DataType.Should().Be(DataType.date);
        }

        [Test]
        public void DataValue_ToBluePrismObject_WithValidTime_ShouldReturnMappedDateTimeValue()
        {
            var result = new DataValue() { Value = new DateTimeOffset(1, 1, 1, 10, 0, 0, TimeSpan.FromHours(1)), ValueType = DataValueType.Time }.ToBluePrismObject();
            result.Should().BeOfType<clsProcessValue>();
            result.IsValid.Should().Be(true);
            result.DataType.Should().Be(DataType.time);
        }

        [Test]
        public void DataValue_ToBluePrismObject_WithValidTimeSpan_ShouldReturnMappedDateTimeValue()
        {
            var result = new DataValue() { Value = new TimeSpan(1,1,1), ValueType = DataValueType.TimeSpan }.ToBluePrismObject();
            result.Should().BeOfType<clsProcessValue>();
            result.IsValid.Should().Be(true);
            result.DataType.Should().Be(DataType.timespan);
        }

        [Test]
        public void DataValue_ToBluePrismObject_WithInvalidDataType_ShouldThrowException()
        {
            var dataType = DataValueType.Binary + -1;
            Assert.Throws<ArgumentException>(() => new DataValue() { Value = false, ValueType = dataType }.ToBluePrismObject());

        }
    }
}
