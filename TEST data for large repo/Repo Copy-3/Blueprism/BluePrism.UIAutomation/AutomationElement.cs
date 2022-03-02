// ReSharper disable BitwiseOperatorOnEnumWithoutFlags
namespace BluePrism.UIAutomation
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Conditions;
    using Utilities.Functional;

    using Patterns;

    /// <inheritdoc />
    public class AutomationElement : IAutomationElement
    {
        /// <summary>
        /// The slash replace regex
        /// </summary>
        private static readonly Regex SlashReplaceRegex =
            new Regex(@"(\\|/)", RegexOptions.Compiled);

        /// <summary>
        /// The pattern factory
        /// </summary>
        private readonly IAutomationPatternFactory _patternFactory;

        /// <summary>
        /// The automation factory
        /// </summary>
        private readonly IAutomationFactory _automationFactory;

        /// <summary>
        /// The tree navigation helper
        /// </summary>
        private readonly IAutomationTreeNavigationHelper _treeNavigationHelper;

        /// <inheritdoc />
        public UIAutomationClient.IUIAutomationElement Element { get; }

        /// <inheritdoc />
        public Rectangle CurrentBoundingRectangle =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentBoundingRectangle.ToRectangle());
        /// <inheritdoc />
        public Point CurrentCentrePoint =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentBoundingRectangle.ToRectangle()
                .Map(GetRectangleCentre));
        /// <inheritdoc />
        public string CurrentClassName =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentClassName);
        /// <inheritdoc />
        public string CurrentAutomationId =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentAutomationId);
        /// <inheritdoc />
        public string CurrentLocalizedControlType =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentLocalizedControlType);
        /// <inheritdoc />
        public bool CurrentIsPassword =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentIsPassword != 0);
        /// <inheritdoc />
        public bool CurrentIsRequiredForForm =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentIsRequiredForForm != 0);
        /// <inheritdoc />
        public string CurrentName =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentName);
        /// <inheritdoc />
        public string CurrentItemStatus =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentItemStatus);
        /// <inheritdoc />
        public string CurrentItemType =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentItemType);
        /// <inheritdoc />
        public bool CurrentIsOffscreen =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentIsOffscreen != 0);
        /// <inheritdoc />
        public IntPtr CurrentNativeWindowHandle =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentNativeWindowHandle);
        /// <inheritdoc />
        public IAutomationElement CurrentLabeledBy =>
            ComHelper.TryGetComValue(() =>
                _automationFactory.FromUIAutomationElement(Element.CurrentLabeledBy));
        /// <inheritdoc />
        public bool CurrentIsEnabled =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentIsEnabled != 0);
        /// <inheritdoc />
        public string CurrentAcceleratorKey =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentAcceleratorKey);
        /// <inheritdoc />
        public string CurrentAccessKey =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentAccessKey);
        /// <inheritdoc />
        public bool CurrentHasKeyboardFocus =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentHasKeyboardFocus != 0);
        /// <inheritdoc />
        public string CurrentHelpText =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentHelpText);
        /// <inheritdoc />
        public int CurrentProcessId =>
            ComHelper.TryGetComValue(() =>
                Element.CurrentProcessId);
        /// <inheritdoc />
        public OrientationType CurrentOrientation =>
            ComHelper.TryGetComValue(() =>
                (OrientationType) Element.CurrentOrientation);
        /// <inheritdoc />
        public ControlType CurrentControlType =>
            ComHelper.TryGetComValue(() =>
                (ControlType)Element.CurrentControlType);

        /// <inheritdoc />
        public Rectangle CachedBoundingRectangle =>
            ComHelper.TryGetComValue(() =>
                Element.CachedBoundingRectangle.ToRectangle());
        /// <inheritdoc />
        public string CachedClassName =>
            ComHelper.TryGetComValue(() =>
                Element.CachedClassName);
        /// <inheritdoc />
        public string CachedAutomationId =>
            ComHelper.TryGetComValue(() =>
                Element.CachedAutomationId);
        /// <inheritdoc />
        public string CachedLocalizedControlType =>
            ComHelper.TryGetComValue(() =>
                Element.CachedLocalizedControlType);
        /// <inheritdoc />
        public bool CachedIsPassword =>
            ComHelper.TryGetComValue(() =>
                Element.CachedIsPassword != 0);
        /// <inheritdoc />
        public bool CachedIsRequiredForForm =>
            ComHelper.TryGetComValue(() =>
                Element.CachedIsRequiredForForm != 0);
        /// <inheritdoc />
        public string CachedName =>
            ComHelper.TryGetComValue(() =>
                Element.CachedName);
        /// <inheritdoc />
        public string CachedItemStatus =>
            ComHelper.TryGetComValue(() =>
                Element.CachedItemStatus);
        /// <inheritdoc />
        public string CachedItemType =>
            ComHelper.TryGetComValue(() =>
                Element.CachedItemType);
        /// <inheritdoc />
        public bool CachedIsOffscreen =>
            ComHelper.TryGetComValue(() =>
                Element.CachedIsOffscreen != 0);
        /// <inheritdoc />
        public IntPtr CachedNativeWindowHandle =>
            ComHelper.TryGetComValue(() =>
                Element.CachedNativeWindowHandle);
        /// <inheritdoc />
        public IAutomationElement CachedLabeledBy =>
            ComHelper.TryGetComValue(() =>
                _automationFactory.FromUIAutomationElement(
                    Element.CachedLabeledBy));
        /// <inheritdoc />
        public bool CachedIsEnabled =>
            ComHelper.TryGetComValue(() =>
                Element.CachedIsEnabled != 0);
        /// <inheritdoc />
        public string CachedAcceleratorKey =>
            ComHelper.TryGetComValue(() =>
                Element.CachedAcceleratorKey);
        /// <inheritdoc />
        public string CachedAccessKey =>
            ComHelper.TryGetComValue(() =>
                Element.CachedAccessKey);
        /// <inheritdoc />
        public bool CachedHasKeyboardFocus =>
            ComHelper.TryGetComValue(() =>
                Element.CachedHasKeyboardFocus != 0);
        /// <inheritdoc />
        public string CachedHelpText =>
            ComHelper.TryGetComValue(() =>
                Element.CachedHelpText);
        /// <inheritdoc />
        public int CachedProcessId =>
            ComHelper.TryGetComValue(() =>
                Element.CachedProcessId);
        /// <inheritdoc />
        public OrientationType CachedOrientation => 
            ComHelper.TryGetComValue(() =>
                (OrientationType)Element.CachedOrientation);
        /// <inheritdoc />
        public ControlType CachedControlType =>
            ComHelper.TryGetComValue(() =>
                (ControlType) Element.CachedControlType);

        /// <inheritdoc />
        public string CurrentPath => GetElementPath(this);

        /// <inheritdoc />
        public string CachedPath =>
            GetElementPath(this, _automationFactory.CreateCacheRequest());

        /// <inheritdoc />
        public IAutomationElementFacade Cached =>
            new CachedAutomationElementFacade(this);

        /// <inheritdoc />
        public IAutomationElementFacade Current =>
            new CurrentAutomationElementFacade(this);

        /// <summary>
        /// Initializes a new instance of the <see cref="AutomationElement" /> class.
        /// </summary>
        /// <param name="element">The base UIA element.</param>
        /// <param name="patternFactory">The pattern factory.</param>
        /// <param name="automationFactory">The automation factory.</param>
        /// <param name="treeNavigationHelper">The tree navigation helper.</param>
        public AutomationElement(
            UIAutomationClient.IUIAutomationElement element,
            IAutomationPatternFactory patternFactory,
            IAutomationFactory automationFactory,
            IAutomationTreeNavigationHelper treeNavigationHelper)
        {
            _patternFactory = patternFactory;
            _automationFactory = automationFactory;
            _treeNavigationHelper = treeNavigationHelper;
            Element = element;
        }

        /// <inheritdoc />
        public IAutomationElement GetCurrentParent() =>
            _automationFactory.CreateTreeWalker(_automationFactory.CreateTrueCondition())
            .GetParent(this);

        /// <inheritdoc />
        public IAutomationElement GetCachedParent(IAutomationCacheRequest cacheRequest) =>
            _automationFactory.CreateTreeWalker(_automationFactory.CreateTrueCondition())
            .GetParent(this, cacheRequest);
            

        ///<inheritdoc />
        public Rectangle GetCurrentBoundingRelativeClientRectangle()
        {
            var rect = CurrentBoundingRectangle;

            var trueWindowCondition = _automationFactory.CreatePropertyCondition(
                PropertyType.ControlType, ControlType.Window);
            var treeWalker = _automationFactory.CreateTreeWalker(trueWindowCondition);

            var parent = treeWalker.Normalize(this);
            if (parent == null)
                return rect;

            var parentRect = parent.CurrentBoundingRectangle;
            rect.Offset(-parentRect.X, -parentRect.Y);
            return rect;
        }

        /// <inheritdoc />
        public IAutomationElement FindFirst(TreeScope scope, IAutomationCondition condition) =>
            FindFirst(scope, condition, _automationFactory.CreateCacheRequest());

        /// <inheritdoc />
        public IEnumerable<IAutomationElement> FindAll(TreeScope scope, IAutomationCondition condition) =>
            FindAll(scope, condition, _automationFactory.CreateCacheRequest());

        /// <inheritdoc />
        public IEnumerable<IAutomationElement> FindAll(TreeScope scope) =>
            FindAll(scope, _automationFactory.CreateCustomCondition(e => true));

        /// <inheritdoc />
        public IEnumerable<IAutomationElement> FindAll(TreeScope scope, IAutomationCacheRequest cacheRequest) =>
            FindAll(scope, _automationFactory.CreateCustomCondition(e => true), cacheRequest);

        /// <inheritdoc />
        public IEnumerable<IAutomationElement> FindAll(TreeScope scope, ControlType controlType) =>
            FindAll(scope, _automationFactory.CreatePropertyCondition(PropertyType.ControlType, controlType));

        /// <inheritdoc />
        public IAutomationElement FindFirst(
            TreeScope scope,
            IAutomationCondition condition,
            IAutomationCacheRequest cacheRequest) 
            =>
            condition.IsCustomCondition
            ? 
                _treeNavigationHelper.FindWithTreeWalker(this, scope, condition, cacheRequest)
                .FirstOrDefault()
            :
                Element.FindFirstBuildCache(
                    (UIAutomationClient.TreeScope)scope,
                    condition.Condition,
                    cacheRequest.CacheRequest)
                .Map(_automationFactory.FromUIAutomationElement);

        /// <inheritdoc />
        public IEnumerable<IAutomationElement> FindAll(
            TreeScope scope,
            IAutomationCondition condition,
            IAutomationCacheRequest cacheRequest)
            =>
            condition.IsCustomCondition
            ? _treeNavigationHelper.FindWithTreeWalker(this, scope, condition, cacheRequest)
            : 
                Element.FindAllBuildCache(
                    (UIAutomationClient.TreeScope)scope,
                    condition.Condition,
                    cacheRequest.CacheRequest)
                .ToEnumerable()
                .Select(_automationFactory.FromUIAutomationElement);

        /// <inheritdoc />
        public object GetCurrentPropertyValue(PropertyType propertyType) =>
            Element.GetCurrentPropertyValue((int)propertyType);

        /// <inheritdoc />
        public bool SetFocus()
        {
            try
            {
                Element.SetFocus();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc />
        public IAutomationPattern GetCurrentPattern(PatternType pattern)
        {
            try
            {
                return _patternFactory.GetCurrentPattern(this, pattern);
            }
            catch (ArgumentException)
            {
                // If the operating system doesn't support a particular pattern, and
                // we attempt to get an instance of that pattern, it throws an
                // ArgumentException with the message
                // "Value does not fall within the expected range".
                // An ArgumentOutOfRangeException would be nicer to handle, but this
                // is as specific as they get. Anyway, all we can do is return null
                // to indicate that the given pattern is not supported on this
                // element - this is true; it's not even supported on this O/S.
                return null;
            }
        }

        /// <inheritdoc />
        public TPattern GetCurrentPattern<TPattern>() where TPattern : IAutomationPattern
        {
            return (TPattern) GetCurrentPattern(
                RepresentsPatternTypeAttribute.GetPatternType<TPattern>());
        }

        /// <inheritdoc />
        public IEnumerable<PatternType> GetSupportedPatterns() =>
            _patternFactory.GetSupportedPatterns(this);

        /// <inheritdoc />
        public bool PatternIsSupported(PatternType pattern) =>
            _patternFactory.GetSupportedPatterns(this).Contains(pattern);

        /// <inheritdoc />
        internal string GetElementPath(IAutomationElement element) =>
            GetElementPath(element, null);

        /// <inheritdoc />
        internal string GetElementPath(
            IAutomationElement element, IAutomationCacheRequest cacheRequest) =>
            element
            .Map(GetElementAncestors)
            .Reverse()
            .Select(x => x.CurrentAutomationId)
            .Select(x => x?.Map(y => SlashReplaceRegex.Replace(y, @"\$1")))
            .Map(x => string.Join("/", x));

        /// <summary>
        /// Gets the element's ancestors.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>A collection of the element's ancestors starting with the element itself.</returns>
        private IEnumerable<IAutomationElement> GetElementAncestors(IAutomationElement element)
        {
            var parent = element;
            do
            {
                yield return parent;
            } while ((parent = _automationFactory.GetParentElement(parent)) != null);
        }

        /// <summary>
        /// Gets the rectangle's center point.
        /// </summary>
        /// <param name="rectangle">The rectangle.</param>
        /// <returns>A <see cref="Point"/> which represents the centre of the given rectangle.</returns>
        private static Point GetRectangleCentre(Rectangle rectangle) =>
            rectangle
                .Map(x => (
                    centreX: x.Left + x.Width / 2,
                    centreY: x.Top + x.Height / 2))
                .Map(x => new Point(x.centreX, x.centreY));
    }
}