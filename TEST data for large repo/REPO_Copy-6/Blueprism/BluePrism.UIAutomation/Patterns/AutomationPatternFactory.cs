namespace BluePrism.UIAutomation.Patterns
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class AutomationPatternFactory : IAutomationPatternFactory
    {
        private readonly Func<(PatternType, IAutomationElement), IAutomationPattern> _patternFactoryMethod;
        private readonly Func<(Type, IAutomationElement), IAutomationPattern> _patternTypeFactoryMethod;

        public AutomationPatternFactory(
            Func<(PatternType, IAutomationElement), IAutomationPattern> patternFactoryMethod,
            Func<(Type, IAutomationElement), IAutomationPattern> patternTypeFactoryMethod)
        {
            _patternFactoryMethod = patternFactoryMethod;
            _patternTypeFactoryMethod = patternTypeFactoryMethod;
        }

        public IAutomationPattern GetCurrentPattern(
            IAutomationElement element, PatternType patternType)
        {
            return _patternFactoryMethod((patternType, element));
        }

        public TPattern GetCurrentPattern<TPattern>(
            IAutomationElement element) where TPattern : IAutomationPattern
        {
            return (TPattern)_patternTypeFactoryMethod((typeof(TPattern), element));
        }

        public IEnumerable<PatternType> GetSupportedPatterns(IAutomationElement element)
        {
            return Enum.GetValues(typeof(PatternType))
                .Cast<PatternType>()
                .Where(x => element.Element.CheckAndGetCurrentPattern(x) != null);
        }
    }
}
