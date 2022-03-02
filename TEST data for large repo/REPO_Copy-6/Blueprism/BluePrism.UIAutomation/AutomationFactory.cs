namespace BluePrism.UIAutomation
{
    using System;
    using System.Linq;
    using System.Drawing;

    using Conditions;
    using Patterns;
    using UIAutomationClient;
    using UIA_PropertyIds = Interop.UIA_PropertyIds;
    using IAccessible = Accessibility.IAccessible;
    using System.Diagnostics;
    using Utilities.Functional;

    /// <inheritdoc />
    public class AutomationFactory : IAutomationFactory
    {
        /// <summary>
        /// The base UIA object
        /// </summary>
        private readonly IUIAutomation _automation;

        /// <summary>
        /// The pattern factory
        /// </summary>
        private readonly IAutomationPatternFactory _patternFactory;

        /// <summary>
        /// The element factory
        /// </summary>
        private readonly Func<IUIAutomationElement, IAutomationElement> _elementFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationFactory" /> class.
        /// </summary>
        /// <param name="automation">The base UIA object.</param>
        /// <param name="patternFactory">The pattern factory.</param>
        /// <param name="elementFactory">The element factory.</param>
        public AutomationFactory(
            IUIAutomation automation,
            IAutomationPatternFactory patternFactory,
            Func<UIAutomationClient.IUIAutomationElement, IAutomationElement> elementFactory)
        {
            _automation = automation;
            _patternFactory = patternFactory;
            _elementFactory = elementFactory;
        }

        /// <inheritdoc />
        public IAutomationElement GetFocusedElement() =>
            _automation.GetFocusedElement()
            ?.Map(FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement GetRootElement() =>
            _automation.GetRootElement()
            ?.Map(FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement GetParentElement(IAutomationElement element) => GetParentElement(element, null);

        /// <inheritdoc />
        public IAutomationElement GetParentElement(IAutomationElement element, IAutomationCacheRequest cacheRequest) =>
            cacheRequest == null
                ? CreateControlTreeWalker().GetParent(element)
                : CreateControlTreeWalker().GetParent(element, cacheRequest);

        /// <inheritdoc />
        public IAutomationElement FromHandle(IntPtr handle) =>
            _automation.ElementFromHandle(handle)
            ?.Map(FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement FromHandle(IntPtr handle, IAutomationCacheRequest cache) =>
            _automation.ElementFromHandleBuildCache(handle, cache.CacheRequest)
                ?.Map(FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement FromPoint(Point point) =>
            point.ToTagPoint()
            .Map(_automation.ElementFromPoint)
            ?.Map(FromUIAutomationElement);

        /// <inheritdoc />
        public IAutomationElement FromIAccessible(IAccessible acc, int childId)
        {
            var uiaAcc = acc as UIAutomationClient.IAccessible;
            Debug.Assert(uiaAcc != null);
            return FromUIAutomationElement(
                _automation.ElementFromIAccessible(uiaAcc, childId));
        }

        /// <inheritdoc />
        public IAutomationElement FromUIAutomationElement(IUIAutomationElement element) =>
            element == null ? null : _elementFactory(element);

        /// <inheritdoc />
        public IAutomationCacheRequest CreateCacheRequest() =>
            new AutomationCacheRequest(_automation.CreateCacheRequest());

        /// <inheritdoc />
        public IAutomationTextRange TextRangeFromUIAutomationObject(IUIAutomationTextRange textRange) =>
            new AutomationTextRange(textRange, this);

        /// <inheritdoc />
        public IAutomationCondition CreateAndCondition(IAutomationCondition condition1, IAutomationCondition condition2) =>
            new AndCondition(_automation, condition1, condition2);

        /// <inheritdoc />
        public IAutomationCondition CreateAndCondition(params IAutomationCondition[] conditions) =>
            conditions.Length < 2
            ? throw new ArgumentException("Not enough conditions provided to form an and condition")
            : conditions.Length == 2
            ? CreateAndCondition(conditions[0], conditions[1])
            : CreateAndCondition(conditions[0], CreateAndCondition(conditions.Skip(1).ToArray()));

        /// <inheritdoc />
        public IAutomationCondition CreateFalseCondition() =>
            new FalseCondition(_automation);

        /// <inheritdoc />
        public IAutomationCondition CreateNotCondition(IAutomationCondition condition) =>
            new NotCondition(_automation, condition);

        /// <inheritdoc />
        public IAutomationCondition CreateOrCondition(IAutomationCondition condition1, IAutomationCondition condition2) =>
            new OrCondition(_automation, condition1, condition2);

        /// <inheritdoc />
        public IAutomationCondition CreatePropertyCondition(PropertyType propertyType, object value) =>
            new PropertyCondition(_automation, propertyType, value);

        /// <inheritdoc />
        public IAutomationCondition CreateTrueCondition() =>
            new TrueCondition(_automation);

        /// <inheritdoc />
        public IAutomationCondition CreateCustomCondition(Func<IAutomationElement, bool> evaluate)
        {
            Func<IAutomationElement, IAutomationCacheRequest, bool> evaluateWithCache =
                (e, c) => evaluate(e);
            return new CustomCondition(evaluateWithCache, this);
        }

        /// <inheritdoc />
        public IAutomationCondition CreateCustomCondition(Func<IAutomationElement, IAutomationCacheRequest, bool> evaluate) =>
            new CustomCondition(evaluate, this);

        /// <inheritdoc />
        public IAutomationTreeWalker CreateTreeWalker(IAutomationCondition condition) =>
            new AutomationTreeWalker(
                _automation.CreateTreeWalker(condition.Condition),
                this);

        /// <inheritdoc />
        public IAutomationTreeWalker CreateRawTreeWalker() =>
            new AutomationTreeWalker(
                _automation.CreateTreeWalker(
                    _automation.CreateTrueCondition()),
                this);

        /// <inheritdoc />
        public IAutomationTreeWalker CreateControlTreeWalker() =>
            new AutomationTreeWalker(
                _automation.CreateTreeWalker(
                    _automation.CreateNotCondition(
                        _automation.CreatePropertyCondition(
                            UIA_PropertyIds.UIA_IsControlElementPropertyId, false))),
                this);

        /// <inheritdoc />
        public IAutomationTreeWalker CreateContentTreeWalker() =>
            new AutomationTreeWalker(
                _automation.CreateTreeWalker(
                    _automation.CreateNotCondition(
                        _automation.CreateOrCondition(
                            _automation.CreatePropertyCondition(
                                UIA_PropertyIds.UIA_IsControlElementPropertyId, false),
                            _automation.CreatePropertyCondition(
                                UIA_PropertyIds.UIA_IsContentElementPropertyId, false)
                        )
                     )
                ),
                this);

        /// <inheritdoc />
        public IAutomationGridText GetGridTextProvider() =>
            new AutomationGridText();

    }
}
