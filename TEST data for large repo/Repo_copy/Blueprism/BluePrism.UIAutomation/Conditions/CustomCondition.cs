namespace BluePrism.UIAutomation.Conditions
{
    using System;

    public class CustomCondition : ConditionBase
    {
        /// <summary>
        /// The automation factory
        /// </summary>
        private readonly IAutomationFactory _automationFactory;

        /// <summary>
        /// The evaluate function
        /// </summary>
        private readonly Func<IAutomationElement, IAutomationCacheRequest, bool> _evaluate;

        public CustomCondition(
            Func<IAutomationElement, IAutomationCacheRequest, bool> evaluate,
            IAutomationFactory automationFactory)
            : base(null, true)
        {
            _evaluate = evaluate;
            _automationFactory = automationFactory;
        }

        /// <inheritDoc />
        public override bool Evaluate(IAutomationElement element, IAutomationCacheRequest cacheRequest) =>
            _evaluate(element, cacheRequest);
    }
}
