using System;
using Color = System.Drawing.Color;
using BluePrism.ApplicationManager.ApplicationManagerUtilities;
using BluePrism.Server.Domain.Models;
using NUnit.Framework;

namespace ApplicationManagerUtilities.UnitTests
{
    /// <summary>
    /// Unit Test Coverage for the clsQuery class
    /// </summary>
    [TestFixture()]
    public class clsQueryTest
    {
        [Test(Description = "Tests \"true\" Returns True")]
        public void TestGetBoolParam_True_ReturnsTrue()
        {
            var q = clsQuery.Parse("TestMethod value1=true");
            bool result = q.GetBoolParam("value1");
            Assert.That(result.Equals(true));
        }

        [Test(Description = "Tests \"false\" Returns False")]
        public void TestGetBoolParam_False_ReturnsFalse()
        {
            var q = clsQuery.Parse("TestMethod value1=false");
            bool result = q.GetBoolParam("value1");
            Assert.That(result.Equals(false));
        }

        [Test(Description = "Tests \"rubbish\" Throws InvalidValueException")]
        public void TestGetBoolParam_Rubbish_ThrowsException()
        {
            var q = clsQuery.Parse("TestMethod value1=rubbish");
            Assert.Throws<InvalidValueException>(() => q.GetBoolParam("value1"));
        }

        [Test(Description = "Tests empty string Returns False")]
        public void TestGetBoolParam_EmptyString_ReturnsFalse()
        {
            var q = clsQuery.Parse("TestMethod value1=");
            bool result = q.GetBoolParam("value1");
            Assert.That(result.Equals(false));
        }

        [Test(Description = "Tests empty string and not allowing empty values throws InvalidValueException")]
        public void TestGetIntParam_EmptyStringAndNotAllowEmpty_ThrowsException()
        {
            var q = clsQuery.Parse("TestMethod value1=");
            Assert.Throws<InvalidValueException>(() => q.GetIntParam("value1", false));
        }

        [Test(Description = "Tests empty string and allowing empty values Returns 0")]
        public void TestGetIntParam_EmptyStringAndAllowEmpty_ReturnsFalse()
        {
            var q = clsQuery.Parse("TestMethod value1=");
            int result = q.GetIntParam("value1", true);
            Assert.That(result.Equals(0));
        }

        [Test(Description = "Tests that the GetIntParam overload with no AllowEmpty parameter, defaults to AllowEmpty=True")]
        public void TestGetIntParam_EmptyStringAndNoAllowEmptySpecified_ReturnsFalse()
        {
            var q = clsQuery.Parse("TestMethod value1=");
            int result = q.GetIntParam("value1");
            Assert.That(result.Equals(0));
        }

        [Test(Description = "Tests integer string returns correct integer")]
        public void TestGetIntParam_Integer_ReturnsCorrectly()
        {
            var q = clsQuery.Parse("TestMethod value1=5000");
            int result = q.GetIntParam("value1");
            Assert.That(result.Equals(5000));
        }

        [Test(Description = "Tests negative integer string returns correct negative integer")]
        public void TestGetIntParam_NegativeInteger_ReturnsCorrectly()
        {
            var q = clsQuery.Parse("TestMethod value1=-5000");
            int result = q.GetIntParam("value1");
            Assert.That(result.Equals(-5000));
        }

        [Test(Description = "Tests \"rubbish\" Throws InvalidValueException")]
        public void TestGetIntParam_Rubbish_ThrowsException()
        {
            var q = clsQuery.Parse("TestMethod value1=rubbish");
            Assert.Throws<InvalidValueException>(() => q.GetIntParam("value1"));
        }

        [Test(Description = "Tests decimal string returns correct rounded integer")]
        public void TestGetIntParam_Decimal_ReturnsRoundedInteger()
        {
            var q = clsQuery.Parse("TestMethod value1=5.2");
            int result = q.GetIntParam("value1");
            Assert.That(result.Equals(5));
        }

        [Test(Description = "Tests number larger than max int throws exception")]
        public void TestGetIntParam_MassiveNumber_ThrowsException()
        {
            var q = clsQuery.Parse("TestMethod value1=9999999999999999");
            Assert.Throws<InvalidValueException>(() => q.GetIntParam("value1"));
        }

        [Test(Description = "Tests empty string and not allowing empty values throws InvalidValueException")]
        public void TestGetDecimalParam_EmptyStringAndNotAllowEmpty_ThrowsException()
        {
            var q = clsQuery.Parse("TestMethod value1=");
            Assert.Throws<InvalidValueException>(() => q.GetDecimalParam("value1", false));
        }

        [Test(Description = "Tests empty string and allowing empty values Returns 0")]
        public void TestGetDecimalParam_EmptyStringAndAllowEmpty_ReturnsFalse()
        {
            var q = clsQuery.Parse("TestMethod value1=");
            decimal result = q.GetDecimalParam("value1", true);
            Assert.That(result.Equals(0));
        }

        [Test(Description = "Tests that the GetDecimalParam overload with no AllowEmpty parameter, defaults to AllowEmpty=True")]
        public void TestGetDecimalParam_EmptyStringAndNoAllowEmptySpecified_ReturnsFalse()
        {
            var q = clsQuery.Parse("TestMethod value1=");
            decimal result = q.GetDecimalParam("value1");
            Assert.That(result.Equals(0));
        }

        [Test(Description = "Tests decimal string returns correct rounded integer")]
        public void TestGetDecimalParam_Decimal_ReturnsDecimal()
        {
            var q = clsQuery.Parse("TestMethod value1=5.2");
            decimal result = q.GetDecimalParam("value1");
            Assert.That(result.Equals(5.2M));
        }

        [Test(Description = "Tests negative integer string returns correct negative integer")]
        public void TestGetDecimalParam_NegativeDecimal_ReturnsCorrectly()
        {
            var q = clsQuery.Parse("TestMethod value1=-5.2");
            decimal result = q.GetDecimalParam("value1");
            Assert.That(result.Equals(-5.2M));
        }

        [Test(Description = "Tests \"rubbish\" Throws InvalidValueException")]
        public void TestGetDecimalParam_Rubbish_ThrowsException()
        {
            var q = clsQuery.Parse("TestMethod value1=rubbish");
            Assert.Throws<InvalidValueException>(() => q.GetDecimalParam("value1"));
        }

        [Test(Description = "Tests number larger than max int throws exception")]
        public void TestGetDecimalParam_MassiveNumber_ThrowsException()
        {
            var q = clsQuery.Parse("TestMethod value1=99999999999999999999999999999999999999999999999999999999999");
            Assert.Throws<InvalidValueException>(() => q.GetDecimalParam("value1"));
        }

        [Test(Description = "Tests GetImageParam allowing empty value, with an empty string, returns Nothing")]
        public void TestGetImageParam_EmptyStringAndAllowEmpty_ReturnNull()
        {
            var q = clsQuery.Parse("TestMethod image=");
            var result = q.GetImageParam("image", true);
            Assert.That(result is null);
        }

        [Test(Description = "Tests GetImageParam not allowing empty value, with an empty string, throws an exception")]
        public void TestGetImageParam_EmptyStringAndNotAllowEmpty_ReturnNull()
        {
            var q = clsQuery.Parse("TestMethod image=");
            Assert.Throws<InvalidValueException>(() => q.GetImageParam("image", false));
        }

        [Test(Description = "Tests GetImageParam with a non-image string throws an exception")]
        public void TestGetImageParam_NonImageString_ThrowsException()
        {
            var q = clsQuery.Parse("TestMethod image=3423");
            Assert.Throws<InvalidValueException>(() => q.GetImageParam("image", false));
        }

        //[Test(Description = "Tests GetImageParam converts image from string correctly")]
        //public void TestGetImageParam_WithImageQueryString_ReturnsCorrectImage()
        //{
        //    var bitMap = CreateTestPixRect();
        //    string bitmapString = bitMap.ToString();
        //    var q = clsQuery.Parse(string.Format("TestMethod image={0}", bitmapString));
        //    var result = q.GetImageParam("image", true);
        //    Assert.That(result.Equals(bitMap));
        //}
        
        ///// <summary>
        ///// Create clsPixRect instance using a bitmap created in memory
        ///// </summary>
        //private clsPixRect CreateTestPixRect()
        //{
        //    var generator = new TestBitmapGenerator().WithColour('R', Color.Red).WithColour('W', Color.White);

        //    return new clsPixRect(generator.WithPixels("RWWWWWWR").Create());
        //}

        [Test(Description = "Tests that encoding nothing throws an exception")]
        public void TestEncodeValue_ValueIsNothing_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => clsQuery.EncodeValue(null));
        }
        
       [TestCase("RequiresNoEscaping", ExpectedResult = "RequiresNoEscaping")]
        [TestCase("Requires Quotes", ExpectedResult = "\"Requires Quotes\"")]
        [TestCase(@"Backslash\DoesNotRequireEscaping", ExpectedResult = @"Backslash\DoesNotRequireEscaping")]
        [TestCase(@"Backslash\ DoesRequireEscaping", ExpectedResult = @"""Backslash\\ DoesRequireEscaping""")]
        [TestCase("\"QuotesRequireEscaping\"", ExpectedResult = @"""\""QuotesRequireEscaping\""""")]
        [TestCase("Curly Brace } Requires Escaping", ExpectedResult = @"""Curly Brace \c Requires Escaping""")]
        [TestCase("Carriage Return Needs Escaping" + "\r", ExpectedResult = @"""Carriage Return Needs Escaping\r""")]
        [TestCase("Line Feed Needs Escaping" + "\n", ExpectedResult = @"""Line Feed Needs Escaping\n""")]
        public string TestEncodeValue(string input)
        {
            return clsQuery.EncodeValue(input);
        }
    }
}
