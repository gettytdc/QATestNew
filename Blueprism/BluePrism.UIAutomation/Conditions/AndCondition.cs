using UIAutomationClient;

namespace BluePrism.UIAutomation.Conditions
{
    /// <summary>
    /// A condition which performs a logical AND of two other conditions
    /// </summary>
    public class AndCondition : ConditionBase
    {
        private readonly IAutomationCondition _firstCondition;
        private readonly IAutomationCondition _secondCondition;

        /// <summary>
        /// Initializes a new instance of <see cref="AndCondition"/>.
        /// </summary>
        /// <param name="automation">The base UIA object</param>
        /// <param name="firstCondition">The first condition</param>
        /// <param name="secondCondition">The second condition</param>
        internal AndCondition(IUIAutomation automation, IAutomationCondition firstCondition, IAutomationCondition secondCondition)
            : base(
                  firstCondition.IsCustomCondition || secondCondition.IsCustomCondition
                      ? null
                      : automation.CreateAndCondition(firstCondition.Condition, secondCondition.Condition),
                  firstCondition.IsCustomCondition || secondCondition.IsCustomCondition)
        {
            _firstCondition = firstCondition;
            _secondCondition = secondCondition;
        }

        /// <inheritDoc />
        public override bool Evaluate(IAutomationElement element, IAutomationCacheRequest cacheRequest) =>
            _firstCondition.Evaluate(element, cacheRequest) && _secondCondition.Evaluate(element, cacheRequest);
    }
}
