using UIAutomationClient;

namespace BluePrism.UIAutomation.Conditions
{
    /// <summary>
    /// A condition which checks the state of a given property
    /// </summary>
    public class PropertyCondition : ConditionBase
    {
        private readonly PropertyType _propertyType;
        private readonly object _value;

        /// <summary>
        /// Initializes a new instance of <see cref="PropertyCondition"/>
        /// </summary>
        /// <param name="automation">The base UIA object</param>
        /// <param name="propertyType">The property being checked</param>
        /// <param name="value">The value to check the property for</param>
        internal PropertyCondition(IUIAutomation automation, PropertyType propertyType, object value)
            : base(
                automation.CreatePropertyCondition((int)propertyType, value),
                false)
        {
            _propertyType = propertyType;
            _value = value;
        }

        /// <inheritDoc />
        public override bool Evaluate(IAutomationElement element, IAutomationCacheRequest cacheRequest) =>
            element.Element.GetCurrentPropertyValue((int) _propertyType).Equals(_value);
    }
}
