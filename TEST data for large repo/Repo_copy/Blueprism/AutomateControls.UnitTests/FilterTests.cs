using System;
using NUnit.Framework;
using AutomateControls.Filters;


namespace AutomateControls.UnitTests
{
    [TestFixture]
    public class FilterTests
    {

        [Test]
        public void TestIntegerDefinition()
        {
            IntegerFilterDefinition defn = new IntegerFilterDefinition("defn");
            FilterItem item;

            item = defn.Parse("> 2");
            Assert.That(item.DisplayValue, Is.EqualTo("> 2"));

            item = defn.Parse(">2,<=5");
            Console.WriteLine("Got item :" + item);

            item = defn.Parse("5");
            Console.WriteLine("Got item :" + item);

        }
    }
}