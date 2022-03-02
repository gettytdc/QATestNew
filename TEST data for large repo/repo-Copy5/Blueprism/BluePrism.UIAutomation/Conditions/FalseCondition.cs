using UIAutomationClient;

namespace BluePrism.UIAutomation.Conditions
{
    /// <summary>
    /// A condition which always returns false
    /// </summary>
    public class FalseCondition : ConditionBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="FalseCondition"/>
        /// </summary>
        /// <param name="automation">The base UIA object</param>
        internal FalseCondition(IUIAutomation automation)
            : base(automation.CreateFalseCondition(), false)
        {
        }

        /// <inheritDoc />
        public override bool Evaluate(IAutomationElement element, IAutomationCacheRequest cacheRequest) => false;
    }
}
