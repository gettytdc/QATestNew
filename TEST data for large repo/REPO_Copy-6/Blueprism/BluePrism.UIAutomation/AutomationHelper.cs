namespace BluePrism.UIAutomation
{
    using System;

    using Utilities.Functional;

    /// <inheritdoc />
    public class AutomationHelper : IAutomationHelper
    {
        /// <summary>
        /// The automation factory
        /// </summary>
        private readonly IAutomationFactory _automationFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationHelper"/> class.
        /// </summary>
        /// <param name="automationFactory">The automation factory.</param>
        public AutomationHelper(
            IAutomationFactory automationFactory)
        {
            _automationFactory = automationFactory;
        }

        /// <inheritdoc />
        public IntPtr GetWindowHandle(IAutomationElement element)
        {
            var treeWalker = 
                _automationFactory.CreateNotCondition(
                    _automationFactory.CreatePropertyCondition(
                        PropertyType.NativeWindowHandle, 0))
                .Map(_automationFactory.CreateTreeWalker);

            var cacheRequest = _automationFactory.CreateCacheRequest();
            cacheRequest.Add(PropertyType.NativeWindowHandle);
            cacheRequest.AutomationElementMode = AutomationElementMode.Full;

            IntPtr result;
            var normalizedElement = treeWalker.Normalize(element, cacheRequest);

            try
            {
                result = normalizedElement.CachedNativeWindowHandle;
            }
            catch
            {
                result = normalizedElement.CurrentNativeWindowHandle;
            }

            return result;
        }

    }
}
