using NUnit.Framework;
using BluePrism.BPCoreLib;

namespace AutomateUI.UnitTests.Classes
{
    public class ExtensionsTests
    {
        private const string WordToTest = "wordToTest";
        private const string NullWord = null;

        [Test]
        public void TestLeftReturnsSameAsVisualBasicLeft()
        {
            var expected = Microsoft.VisualBasic.Strings.Left(WordToTest,4);
            var actual = WordToTest.Left(4);

            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void TestLeftReturnsSameAsVisualBasicLeftWhenLengthIsLongerThanWord()
        {
            var expected = Microsoft.VisualBasic.Strings.Left(WordToTest, WordToTest.Length + 2);
            var leftWord = WordToTest.Left(WordToTest.Length +2);

            Assert.AreEqual(expected, leftWord);
        }
        [Test]
        public void TestLeftReturnsSameAsVisualBasicLeftWhenWordIsNull()
        {
            var expected = Microsoft.VisualBasic.Strings.Left(null,2);
            var actual = NullWord.Left(2);

            Assert.AreEqual(expected, actual);
        }

        //Mid Tests
        [Test]
        public void TestMidReturnsSameAsVisualBasicMidOneParam()
        {
            var expected = Microsoft.VisualBasic.Strings.Mid(WordToTest,4);
            var actual = WordToTest.Mid(4);

            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void TestMidReturnsSameAsVisualBasicMidTwoParam()
        {
            var expected = Microsoft.VisualBasic.Strings.Mid(WordToTest,4,2);
            var actual = WordToTest.Mid(4,2);

            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void TestMidReturnsSameAsVisualBasicMidTwoParamLengthOutsideStringLength()
        {
            var expected = Microsoft.VisualBasic.Strings.Mid(WordToTest, 4, 10);
            var actual = WordToTest.Mid(4, 10);

            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void TestMidReturnsSameAsVisualBasicMidNullWordOnePram()
        {
            var expected = Microsoft.VisualBasic.Strings.Mid(NullWord, 4);
            var actual = NullWord.Mid(4);

            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void TestMidReturnsSameAsVisualBasicMidNullWordTwoPram()
        {
            var expected = Microsoft.VisualBasic.Strings.Mid(NullWord, 4, 2);
            var actual = NullWord.Mid(4,2);

            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void TestRightReturnsSameAsVisualBasicRight()
        {
            var expected = Microsoft.VisualBasic.Strings.Right(WordToTest , 4);
            var actual = WordToTest.Right(4);
            Assert.AreEqual(expected, actual);
        }
        //if str.Length<start then return string.empty
       
        [Test]
        public void TestRightReturnsSameAsVisualBasicRightWhenStartIsGreaterThanWordLength()
        {
            var expected = Microsoft.VisualBasic.Strings.Right(WordToTest, 80);
            var actual = WordToTest.Right(80);
            Assert.AreEqual(expected, actual);
        }

        //Right Tests
        [Test]
        public void TestRightReturnsSameAsVisualBasicRightWhenWordIsNull()
        {
            var expected = Microsoft.VisualBasic.Strings.Right(NullWord , 4);
            var actual = NullWord.Right(4);
            Assert.AreEqual(expected, actual);
        }
        [Test]
        public void TestRightReturnsSameAsVisualBasicRightWhenLengthIsLongerThanWord()
        {
            var expected = Microsoft.VisualBasic.Strings.Right(WordToTest, 80);
            var actual = WordToTest.Right(80);
            Assert.AreEqual(expected, actual);
        }
    }
}
