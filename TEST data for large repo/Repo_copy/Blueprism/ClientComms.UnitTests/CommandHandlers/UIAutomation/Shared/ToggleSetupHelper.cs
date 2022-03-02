using BluePrism.UIAutomation;
using BluePrism.UIAutomation.Patterns;
using Moq;

namespace ClientComms.UnitTests.CommandHandlers.UIAutomation.Shared
{

    /// <summary>
    /// Shared test setup functionality for toggle elements
    /// </summary>
    public static class ToggleSetupHelper
    {

        /// <summary>
        /// Sets up an element to implement the toggle pattern, setting the
        /// CurrentToggleState property and toggling it as Toggle is called
        /// </summary>
        /// <param name="elementMock">The mock element object</param>
        /// <param name="initialState">The initial toggle state</param>
        /// <returns></returns>
        public static Mock<ITogglePattern> SetupState(Mock<IAutomationElement> elementMock, ToggleState initialState)
        {
            var patternMock = elementMock.MockPattern<ITogglePattern>();
            var currentState = initialState;
            patternMock.Setup(p => p.CurrentToggleState).Returns(() => currentState);
            patternMock.Setup(p => p.Toggle()).Callback(() => currentState = currentState == ToggleState.On ? ToggleState.Off : ToggleState.On);
            return patternMock;
        }
    }
}