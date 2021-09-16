namespace BluePrism.UIAutomation.Conditions
{
    using UIAutomationClient;

    public abstract class ConditionBase : IAutomationCondition
    {
        /// <inheritDoc />
        public IUIAutomationCondition Condition { get; }
        /// <inheritDoc />
        public bool IsCustomCondition { get; }

        protected ConditionBase(IUIAutomationCondition condition, bool isCustomCondition)
        {
            Condition = condition;
            IsCustomCondition = isCustomCondition;
        }

        /// <inheritDoc />
        public abstract bool Evaluate(IAutomationElement element, IAutomationCacheRequest cacheRequest);
    }
}