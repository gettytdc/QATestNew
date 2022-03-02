#if UNITTESTS
using System;
using System.Data;
using BluePrism.BPCoreLib.Data;
using BluePrism.Server.Domain.Models;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{
    [TestFixture]
    public class DataTableDataProviderTests
    {
        [Test]
        public void TestTableProvider()
        {
            var dt = new DataTable();
            {
                var withBlock = dt.Columns;
                withBlock.Add("id", typeof(int));
                withBlock.Add("name", typeof(string));
                withBlock.Add("dob", typeof(DateTime));
            }

            {
                var withBlock1 = dt.Rows;
                withBlock1.Add(1, "Ted Danson", new DateTime(1947, 12, 29));
                withBlock1.Add(2, "Catherine The Great", new DateTime(1729, 5, 2));
                withBlock1.Add(3, "Jan Eggum", new DateTime(1951, 12, 8));
                withBlock1.Add(4, "Genghis Khan", DBNull.Value);
            }

            var prov = new DataTableDataProvider(dt);
            var count = 0;
            while (prov.MoveNext())
            {
                count += 1;
                switch (count)
                {
                    case 1:
                        {
                            Assert.That(prov.GetValue("id", 0), Is.EqualTo(1));
                            Assert.That(prov.GetString("name"), Is.EqualTo("Ted Danson"));
                            Assert.That(prov.GetValue("dob", DateTime.MinValue), Is.EqualTo(new DateTime(1947, 12, 29)));
                            Assert.That(prov.GetValue("fish", "cod"), Is.EqualTo("cod"));
                            break;
                        }

                    case 2:
                        {
                            Assert.That(prov.GetValue("id", 0), Is.EqualTo(2));
                            Assert.That(prov.GetString("name"), Is.EqualTo("Catherine The Great"));
                            Assert.That(prov.GetValue("dob", DateTime.MinValue), Is.EqualTo(new DateTime(1729, 5, 2)));
                            Assert.That(prov.GetString("bird"), Is.Null);
                            break;
                        }

                    case 3:
                        {
                            Assert.That(prov.GetValue("id", 0), Is.EqualTo(3));
                            Assert.That(prov.GetString("name"), Is.EqualTo("Jan Eggum"));
                            Assert.That(prov.GetValue("dob", DateTime.MinValue), Is.EqualTo(new DateTime(1951, 12, 8)));
                            Assert.ByVal(prov["insect"], Is.Null);
                            break;
                        }

                    case 4:
                        {
                            Assert.That(prov.GetValue("id", 0), Is.EqualTo(4));
                            Assert.That(prov.GetString("name"), Is.EqualTo("Genghis Khan"));
                            Assert.That(prov.GetValue("dob", DateTime.MaxValue), Is.EqualTo(DateTime.MaxValue));
                            Assert.ByVal(prov["ID"], Is.EqualTo(4));
                            break;
                        }

                    case 5:
                        {
                            Assert.Fail("5 is right out");
                            break;
                        }
                }
            }

            // We shouldn't be able to get data at this point
            try
            {
                prov.GetString("fish");
                Assert.Fail("Provider should have errored after data");
            }
            catch (InvalidStateException)
            {
                // Correct
            }
            // Multiple calls shouldn't be a problem
            Assert.That(prov.MoveNext(), Is.False);
        }
    }
}
#endif
