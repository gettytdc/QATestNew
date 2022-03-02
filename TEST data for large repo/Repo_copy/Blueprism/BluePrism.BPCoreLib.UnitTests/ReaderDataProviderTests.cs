#if UNITTESTS

using System;
using System.Data;
using BluePrism.BPCoreLib.Data;
using BluePrism.Server.Domain.Models;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{
    [TestFixture]
    public class ReaderDataProviderTests
    {

        /// <summary>
        /// Tests the reader data provider using a datatable.
        /// Most of this is half-inched from
        /// <see cref="DataTableDataProviderTests.TestTableProvider"/> just to save time
        /// and because it's already testing the same sort of data, so it should work
        /// the same.
        /// </summary>
        [Test]
        public void TestDataTableReaderProvider()
        {
            var dt = new DataTable();
            dt.Columns.Add("id", typeof(int));
            dt.Columns.Add("name", typeof(string));
            dt.Columns.Add("dob", typeof(DateTime));

            dt.Rows.Add(1, "Ted Danson", new DateTime(1947, 12, 29));
            dt.Rows.Add(2, "Catherine The Great", new DateTime(1729, 5, 2));
            dt.Rows.Add(3, "Jan Eggum", new DateTime(1951, 12, 8));
            dt.Rows.Add(4, "Genghis Khan", DBNull.Value);

            using (var prov = new ReaderMultiDataProvider(dt.CreateDataReader()))
            {
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
                            Assert.That(prov.GetValue("dob", DateTime.MinValue),
                                Is.EqualTo(new DateTime(1947, 12, 29)));
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
}

#endif
