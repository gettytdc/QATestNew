using UIAutomationClient;

namespace BluePrism.UIAutomation.Conditions
{
    /// <summary>
    /// A condition which always returns true
    /// </summary>
    public class TrueCondition : ConditionBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TrueCondition"/>
        /// </summary>
        /// <param name="automation">The base UIA object</param>
        internal TrueCondition(IUIAutomation automation)
            : base(automation.CreateTrueCondition(), false)
        {
        }

        /// <inheritDoc />
        public override bool Evaluate(IAutomationElement element, IAutomationCacheRequest cacheRequest) => true;
    }
}
