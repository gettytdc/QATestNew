using FluentAssertions;
using NUnit.Framework;

namespace AutomateUI.UnitTests.Classes
{
    public class PropertyChangedWithValuesEventArgsTest
    {
        [Test]
        public void PropertyChangedWithValuesEventArgs_PopulatedWithCorrectParameters()
        {
            var propertyName = "Username";
            var oldValue = "OldUsername";
            var newValue = "NewUsername";
            var propertyChangedWithValuesEventArgs = new PropertyChangedWithValuesEventArgs(propertyName, oldValue, newValue);

            propertyChangedWithValuesEventArgs.PropertyName.Should().Be(propertyName);
            propertyChangedWithValuesEventArgs.OldValue.Should().Be(oldValue);
            propertyChangedWithValuesEventArgs.NewValue.Should().Be(newValue);
        }
    }
}
