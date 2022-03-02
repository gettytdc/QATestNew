namespace BluePrism.Api.UnitTests.Validators
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Security;
    using Api.Validators;
    using FluentValidation.TestHelper;
    using Models;
    using NUnit.Framework;
    using Utilities.Testing;

    [TestFixture]
    public class DataCollectionModelValidatorTests : UnitTestBase<DataCollectionModelValidator>
    {
        [Test]
        public void TestValidateDataCollection_WhenDataRowsHasBindErrorSetAsTrue_ShouldHaveError()
        {
            var dictionary = new Dictionary<string, DataValueModel>
            {
                {"key1", new DataValueModel() { HasBindError = true, ValueType = DataValueType.Number }}
            };
            var result = ClassUnderTest.TestValidate(dictionary);
            result.ShouldHaveValidationErrorFor(x => x.Values);
        }

        [Test]
        public void TestValidateDataCollection_WhenDataKeyIsNotSet_ShouldHaveError()
        {
            var dictionary = new Dictionary<string, DataValueModel>
            {
                {"", new DataValueModel() { HasBindError = false, ValueType = DataValueType.Number, Value = 1}}
            };
            var result = ClassUnderTest.TestValidate(dictionary);
            result.ShouldHaveValidationErrorFor(x => x.Keys);
        }

        [TestCaseSource(nameof(DataValueModelTestCaseSources))]
        public void TestValidateDataCollection_WhenDataValueIsNotExpectedType_ShouldHaveError(DataValueType dataValueType, object dataValue, bool expectValidationError)
        {
            var dictionary = new Dictionary<string, DataValueModel>
            {
                {"Key1", new DataValueModel() { HasBindError = false, ValueType = dataValueType, Value = dataValue}}
            };
            var result = ClassUnderTest.TestValidate(dictionary);

            if(expectValidationError)
            {
                result.ShouldHaveValidationErrorFor(x => x.Values);
            }
            else
            {
                result.ShouldNotHaveValidationErrorFor(x => x.Values);
            }
        }

        private static IEnumerable<TestCaseData> DataValueModelTestCaseSources
        {
            get
            {
                yield return new TestCaseData(DataValueType.Number, "string", true);
                yield return new TestCaseData(DataValueType.Number, "string", true);
                yield return new TestCaseData(DataValueType.Number, (decimal)1, false);
                yield return new TestCaseData(DataValueType.Text, 1, true);
                yield return new TestCaseData(DataValueType.Text, "string", false);
                yield return new TestCaseData(DataValueType.Flag, "string", true);
                yield return new TestCaseData(DataValueType.Flag, true, false);
                yield return new TestCaseData(DataValueType.DateTime, "string", true);
                yield return new TestCaseData(DataValueType.DateTime, new DateTimeOffset(2020, 1, 1, 10, 0, 0, TimeSpan.Zero), false);
                yield return new TestCaseData(DataValueType.DateTime, 1, true);
                yield return new TestCaseData(DataValueType.Date, new DateTimeOffset(2020, 1, 1, 0, 0, 0, TimeSpan.Zero), false);
                yield return new TestCaseData(DataValueType.Date, 1, true);
                yield return new TestCaseData(DataValueType.Time, new DateTimeOffset(1, 1, 1, 10, 0, 0, TimeSpan.Zero), false);
                yield return new TestCaseData(DataValueType.Time, 1, true);
                yield return new TestCaseData(DataValueType.TimeSpan, 1, true);
                yield return new TestCaseData(DataValueType.TimeSpan, new TimeSpan(1, 1, 1), false);
                yield return new TestCaseData(DataValueType.Password, 1, true);
                yield return new TestCaseData(DataValueType.Password, new SecureString(), false);
                yield return new TestCaseData(DataValueType.Binary, 1, true);
                yield return new TestCaseData(DataValueType.Binary, new byte[] { }, false);
                yield return new TestCaseData(DataValueType.Image, 1, true);
                yield return new TestCaseData(DataValueType.Image, new Bitmap(100, 100), false);
                yield return new TestCaseData(DataValueType.Collection, 1, true);
                yield return new TestCaseData(DataValueType.Collection, new DataCollectionModel()
                {
                    Rows = new List<IReadOnlyDictionary<string, DataValueModel>>()
                    {
                        new Dictionary<string, DataValueModel>
                        {
                            {"Key1", new DataValueModel()}
                        }
                    }
                }, false);

            }
        }
    }
}
