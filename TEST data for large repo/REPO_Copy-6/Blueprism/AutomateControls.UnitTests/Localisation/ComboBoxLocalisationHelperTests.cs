using System.Linq;
using System.Resources;
using AutomateControls.Localisation;
using BluePrism.Server.Domain.Models;
using NUnit.Framework;

namespace AutomateControls.UnitTests.Localisation
{
    public class ComboBoxLocalisationHelperTests
    {
        private static readonly ResourceManager Resources = LocalisationTestResources.ResourceManager;

        [Test]
        public void CreateItem_ShouldUseResourceString()
        {
            var item1 = ComboBoxLocalisationHelper.CreateItem(Resources, "TestEnum_{0}", TestEnum.Value1);
            var item2 = ComboBoxLocalisationHelper.CreateItem(Resources, "TestEnum_{0}", TestEnum.Value2);

            AssertExpectedItem(item1, LocalisationTestResources.TestEnum_Value1, TestEnum.Value1);
            AssertExpectedItem(item2, LocalisationTestResources.TestEnum_Value2, TestEnum.Value2);
        }

        [Test]
        public void CreateItems_ShouldUseResourceStrings()
        {
            var items = ComboBoxLocalisationHelper.CreateItems(Resources, "TestEnum_{0}", new[] { TestEnum.Value1, TestEnum.Value2 })
                .ToList();

            Assert.That(items, Has.Count.EqualTo(2));
            AssertExpectedItem(items[0], LocalisationTestResources.TestEnum_Value1, TestEnum.Value1);
            AssertExpectedItem(items[1], LocalisationTestResources.TestEnum_Value2, TestEnum.Value2);
        }

        [Test]
        public void CreateItem_WithUnknownName_ShouldThrow()
        {
            Assert.Throws(typeof(MissingResourceException),
                () => ComboBoxLocalisationHelper.CreateItem(Resources, "TestEnum_{0}", "missing"));
        }

        [Test]
        public void CreateItemsFromEnum_ShouldUseResourceStrings()
        {
            var items = ComboBoxLocalisationHelper.CreateEnumItems<TestEnum>(Resources, "TestEnum_{0}")
                .ToList();

            Assert.That(items, Has.Count.EqualTo(3));
            AssertExpectedItem(items[0], LocalisationTestResources.TestEnum_Value1, TestEnum.Value1);
            AssertExpectedItem(items[1], LocalisationTestResources.TestEnum_Value2, TestEnum.Value2);
            AssertExpectedItem(items[2], LocalisationTestResources.TestEnum_Value3, TestEnum.Value3);
        }
        
        private static void AssertExpectedItem(ComboBoxItem item, string text, object tag)
        {
            Assert.That(item, Is.Not.Null);
            Assert.That(item.Text, Is.EqualTo(text));
            Assert.That(item.Tag, Is.EqualTo(tag));
        }

        public enum TestEnum
        {
            Value1,
            Value2,
            Value3
        }
    }
}
