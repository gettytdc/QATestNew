#if UNITTESTS

using System;
using NUnit.Framework;
using BluePrism.BPCoreLib;

namespace BluePrism.Core.UnitTests
{
    [TestFixture]
    public class InternalCultureTests
    {
        [Test]
        public void TestParseSingle()
        {
            using (new CultureBlock("en-GB"))
            {
                Assert.That(InternalCulture.Sng("0.5"), Is.EqualTo(0.5f));
                Assert.That(InternalCulture.Sng("0"), Is.EqualTo(0f));
                Assert.That(InternalCulture.Sng("-1,000"), Is.EqualTo(-1000f));
                Assert.That(InternalCulture.Sng(" 1,000.25 "), Is.EqualTo(1000.25f));
                try
                {
                    InternalCulture.Sng(null);
                    Assert.Fail("InternalCulture.Sng(null) worked - it should fail");
                }
                catch (ArgumentNullException) { /* correct */ }
                catch (Exception e)
                {
                    Assert.Fail("{0} found - InternalCulture.Sng(null) should throw"+
                        " ArgumentNullException: Message: {1}",
                        e.GetType(), e.Message);
                }
                try
                {
                    InternalCulture.Sng("");
                    Assert.Fail("InternalCulture.Sng(\"\") worked - it should fail");
                }
                catch (FormatException) { /* correct */ }
                catch (Exception e)
                {
                    Assert.Fail("{0} found - InternalCulture.Sng(\"\") should throw" +
                        " FormatException: Message: {1}",
                        e.GetType(), e.Message);
                }
                try
                {
                    InternalCulture.Sng("1234BANG!");
                    Assert.Fail(
                        "InternalCulture.Sng(\"1234BANG\") worked - it should fail");
                }
                catch (FormatException) { /* correct */ }
                catch (Exception e)
                {
                    Assert.Fail(
                        "{0} found - InternalCulture.Sng(\"1234BANG\") should throw" +
                        " FormatException: Message: {1}",
                        e.GetType(), e.Message);
                }
            }
            using (new CultureBlock("fr-FR"))
            {
                Assert.That(InternalCulture.Sng("0.5"), Is.EqualTo(0.5f));
                Assert.That(InternalCulture.Sng("0"), Is.EqualTo(0f));
                Assert.That(InternalCulture.Sng("-1,000"), Is.EqualTo(-1000f));
                Assert.That(InternalCulture.Sng(" 1,000.25 "), Is.EqualTo(1000.25f));
                try
                {
                    InternalCulture.Sng(null);
                    Assert.Fail("InternalCulture.Sng(null) worked - it should fail");
                }
                catch (ArgumentNullException) { /* correct */ }
                catch (Exception e)
                {
                    Assert.Fail("{0} found - InternalCulture.Sng(null) should throw" +
                        " ArgumentNullException: Message: {1}",
                        e.GetType(), e.Message);
                }
                try
                {
                    InternalCulture.Sng("");
                    Assert.Fail("InternalCulture.Sng(\"\") worked - it should fail");
                }
                catch (FormatException) { /* correct */ }
                catch (Exception e)
                {
                    Assert.Fail("{0} found - InternalCulture.Sng(\"\") should throw" +
                        " FormatException: Message: {1}",
                        e.GetType(), e.Message);
                }
                try
                {
                    InternalCulture.Sng("1234BANG!");
                    Assert.Fail(
                        "InternalCulture.Sng(\"1234BANG\") worked - it should fail");
                }
                catch (FormatException) { /* correct */ }
                catch (Exception e)
                {
                    Assert.Fail(
                        "{0} found - InternalCulture.Sng(\"1234BANG\") should throw" +
                        " FormatException: Message: {1}",
                        e.GetType(), e.Message);
                }
            }
            using (new CultureBlock("en-GB"))
            {
                float f;
                Assert.That(InternalCulture.TrySng("0.5", out f), Is.True);
                Assert.That(f, Is.EqualTo(0.5f));
                Assert.That(InternalCulture.TrySng("0", out f), Is.True);
                Assert.That(f, Is.EqualTo(0f));
                Assert.That(InternalCulture.TrySng("-1,000", out f), Is.True);
                Assert.That(f, Is.EqualTo(-1000f));
                Assert.That(InternalCulture.TrySng(" 1,000.25 ", out f), Is.True);
                Assert.That(f, Is.EqualTo(1000.25f));
                Assert.That(InternalCulture.TrySng(null, out f), Is.False);
                Assert.That(f, Is.EqualTo(0f));
                f = 1000;
                Assert.That(InternalCulture.TrySng("", out f), Is.False);
                Assert.That(f, Is.EqualTo(0f));
                f = 1000;
                Assert.That(InternalCulture.TrySng("1234BANG!", out f), Is.False);
                Assert.That(f, Is.EqualTo(0f));
            }
            using (new CultureBlock("fr-FR"))
            {
                float f;
                Assert.That(InternalCulture.TrySng("0.5", out f), Is.True);
                Assert.That(f, Is.EqualTo(0.5f));
                Assert.That(InternalCulture.TrySng("0", out f), Is.True);
                Assert.That(f, Is.EqualTo(0f));
                Assert.That(InternalCulture.TrySng("-1,000", out f), Is.True);
                Assert.That(f, Is.EqualTo(-1000f));
                Assert.That(InternalCulture.TrySng(" 1,000.25 ", out f), Is.True);
                Assert.That(f, Is.EqualTo(1000.25f));
                Assert.That(InternalCulture.TrySng(null, out f), Is.False);
                Assert.That(f, Is.EqualTo(0f));
                f = 1000;
                Assert.That(InternalCulture.TrySng("", out f), Is.False);
                Assert.That(f, Is.EqualTo(0f));
                f = 1000;
                Assert.That(InternalCulture.TrySng("1234BANG!", out f), Is.False);
                Assert.That(f, Is.EqualTo(0f));
            }
        }

        [Test]
        public void TestParseDouble()
        {
            using (new CultureBlock("en-GB"))
            {
                Assert.That(InternalCulture.Dbl("0.5"), Is.EqualTo(0.5d));
                Assert.That(InternalCulture.Dbl("0"), Is.EqualTo(0d));
                Assert.That(InternalCulture.Dbl("-1,000"), Is.EqualTo(-1000d));
                Assert.That(InternalCulture.Dbl(" 1,000.25 "), Is.EqualTo(1000.25d));
                try
                {
                    InternalCulture.Dbl(null);
                    Assert.Fail("InternalCulture.Dbl(null) worked - it should fail");
                }
                catch (ArgumentNullException) { /* correct */ }
                catch (Exception e)
                {
                    Assert.Fail("{0} found - InternalCulture.Dbl(null) should throw" +
                        " ArgumentNullException: Message: {1}",
                        e.GetType(), e.Message);
                }
                try
                {
                    InternalCulture.Dbl("");
                    Assert.Fail("InternalCulture.Dbl(\"\") worked - it should fail");
                }
                catch (FormatException) { /* correct */ }
                catch (Exception e)
                {
                    Assert.Fail("{0} found - InternalCulture.Dbl(\"\") should throw" +
                        " FormatException: Message: {1}",
                        e.GetType(), e.Message);
                }
                try
                {
                    InternalCulture.Dbl("1234BANG!");
                    Assert.Fail(
                        "InternalCulture.Dbl(\"1234BANG\") worked - it should fail");
                }
                catch (FormatException) { /* correct */ }
                catch (Exception e)
                {
                    Assert.Fail(
                        "{0} found - InternalCulture.Dbl(\"1234BANG\") should throw" +
                        " FormatException: Message: {1}",
                        e.GetType(), e.Message);
                }
            }
            using (new CultureBlock("fr-FR"))
            {
                Assert.That(InternalCulture.Dbl("0.5"), Is.EqualTo(0.5d));
                Assert.That(InternalCulture.Dbl("0"), Is.EqualTo(0d));
                Assert.That(InternalCulture.Dbl("-1,000"), Is.EqualTo(-1000d));
                Assert.That(InternalCulture.Dbl(" 1,000.25 "), Is.EqualTo(1000.25d));
                try
                {
                    InternalCulture.Dbl(null);
                    Assert.Fail("InternalCulture.Dbl(null) worked - it should fail");
                }
                catch (ArgumentNullException) { /* correct */ }
                catch (Exception e)
                {
                    Assert.Fail("{0} found - InternalCulture.Dbl(null) should throw" +
                        " ArgumentNullException: Message: {1}",
                        e.GetType(), e.Message);
                }
                try
                {
                    InternalCulture.Dbl("");
                    Assert.Fail("InternalCulture.Dbl(\"\") worked - it should fail");
                }
                catch (FormatException) { /* correct */ }
                catch (Exception e)
                {
                    Assert.Fail("{0} found - InternalCulture.Dbl(\"\") should throw" +
                        " FormatException: Message: {1}",
                        e.GetType(), e.Message);
                }
                try
                {
                    InternalCulture.Dbl("1234BANG!");
                    Assert.Fail(
                        "InternalCulture.Dbl(\"1234BANG\") worked - it should fail");
                }
                catch (FormatException) { /* correct */ }
                catch (Exception e)
                {
                    Assert.Fail(
                        "{0} found - InternalCulture.Dbl(\"1234BANG\") should throw" +
                        " FormatException: Message: {1}",
                        e.GetType(), e.Message);
                }
            }
            using (new CultureBlock("en-GB"))
            {
                double d;
                Assert.That(InternalCulture.TryDbl("0.5", out d), Is.True);
                Assert.That(d, Is.EqualTo(0.5d));
                Assert.That(InternalCulture.TryDbl("0", out d), Is.True);
                Assert.That(d, Is.EqualTo(0d));
                Assert.That(InternalCulture.TryDbl("-1,000", out d), Is.True);
                Assert.That(d, Is.EqualTo(-1000d));
                Assert.That(InternalCulture.TryDbl(" 1,000.25 ", out d), Is.True);
                Assert.That(d, Is.EqualTo(1000.25d));
                Assert.That(InternalCulture.TryDbl(null, out d), Is.False);
                Assert.That(d, Is.EqualTo(0d));
                d = 1000;
                Assert.That(InternalCulture.TryDbl("", out d), Is.False);
                Assert.That(d, Is.EqualTo(0d));
                d = 1000;
                Assert.That(InternalCulture.TryDbl("1234BANG!", out d), Is.False);
                Assert.That(d, Is.EqualTo(0d));
            }
            using (new CultureBlock("fr-FR"))
            {
                double d;
                Assert.That(InternalCulture.TryDbl("0.5", out d), Is.True);
                Assert.That(d, Is.EqualTo(0.5d));
                Assert.That(InternalCulture.TryDbl("0", out d), Is.True);
                Assert.That(d, Is.EqualTo(0d));
                Assert.That(InternalCulture.TryDbl("-1,000", out d), Is.True);
                Assert.That(d, Is.EqualTo(-1000d));
                Assert.That(InternalCulture.TryDbl(" 1,000.25 ", out d), Is.True);
                Assert.That(d, Is.EqualTo(1000.25d));
                Assert.That(InternalCulture.TryDbl(null, out d), Is.False);
                Assert.That(d, Is.EqualTo(0d));
                d = 1000;
                Assert.That(InternalCulture.TryDbl("", out d), Is.False);
                Assert.That(d, Is.EqualTo(0d));
                d = 1000;
                Assert.That(InternalCulture.TryDbl("1234BANG!", out d), Is.False);
                Assert.That(d, Is.EqualTo(0d));
            }
        }

    }
}

#endif