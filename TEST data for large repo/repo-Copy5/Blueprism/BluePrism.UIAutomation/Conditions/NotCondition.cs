using UIAutomationClient;

namespace BluePrism.UIAutomation.Conditions
{
    /// <summary>
    /// A condition which performs a logical NOT on a provided condition
    /// </summary>
    public class NotCondition : ConditionBase
    {
        private readonly IAutomationCondition _condition;

        /// <summary>
        /// Initializes a new instance of <see cref="NotCondition"/>
        /// </summary>
        /// <param name="automation">The base UIA object</param>
        /// <param name="condition">The condition to NOT</param>
        internal NotCondition(IUIAutomation automation, IAutomationCondition condition)
            : base(
                  condition.IsCustomCondition ? null : automation.CreateNotCondition(condition.Condition),
                  condition.IsCustomCondition)
        {
            _condition = condition;
        }

        /// <inheritDoc />
        public override bool Evaluate(IAutomationElement element, IAutomationCacheRequest cacheRequest) =>
            !_condition.Evaluate(element, cacheRequest);
    }
}
