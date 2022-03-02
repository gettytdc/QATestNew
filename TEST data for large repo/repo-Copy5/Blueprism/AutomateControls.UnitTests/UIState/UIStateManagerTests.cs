using AutomateControls.UIState;
using FluentAssertions;
using NUnit.Framework;
using System;

namespace AutomateControls.UnitTests.UIState
{
    public class UIStateManagerTests
    {
        private readonly UIControlConfig _uiControlConfig;

        public UIStateManagerTests()
        {
            _uiControlConfig = new UIControlConfig
            {
                Id = "TestElement",
                Value = 0.53567
            };
        }

        [Test]
        public void TestSaveAndLoadUserLayout()
        {
            UIStateManager.Instance.SetControlConfig(_uiControlConfig);

            var loadedUIConfigElement = UIStateManager.Instance.GetControlConfig(_uiControlConfig.Id);

            loadedUIConfigElement.Value.Should().BeOfType<double>().And.Be(0.53567);
        }

        [Test]
        public void TestLoadInvalidItem()
        {
            var itemDoesntExist = UIStateManager.Instance.GetControlConfig("InvalidId");

            itemDoesntExist.Should().BeNull();
        }

        [Test]
        public void TestSaveMultipleTimes()
        {
            Action action = () => UIStateManager.Instance.SetControlConfig(_uiControlConfig);

            action.Invoke();
            action.ShouldNotThrow(because: "calling save a second time is safe");
            action.ShouldNotThrow(because: "calling save a third time is safe");
        }
    }
}
