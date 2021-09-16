namespace BluePrism.UIAutomation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using BPCoreLib;
    using BPCoreLib.Collections;

    using Utilities.Functional;

    using Patterns;

    using UIAutomationClient;
    using BluePrism.Server.Domain.Models;

    /// <summary>
    /// This class does the job that a DI framework should do until such
    /// a time as we have DI in the project.
    /// </summary>
    public static class AutomationTypeProvider
    {
        private static readonly IReadOnlyDictionary<Type, Func<object>> _typeFactories = new Dictionary<Type, Func<object>>
        {
            {typeof(IAutomationFactory), CreateIAutomationFactory},
            {typeof(IAutomationHelper), CreateIAutomationHelper },
        };

        /// <summary>
        /// Gets the requested type.
        /// </summary>
        /// <typeparam name="T">The type to get</typeparam>
        /// <returns>An instance of the requested type</returns>
        public static T GetType<T>() where T : class => _typeFactories[typeof(T)]() as T;

        private static IAutomationFactory _automationFactory;
        private static IAutomationFactory CreateIAutomationFactory() =>
            _automationFactory
            ?? (_automationFactory = new AutomationFactory(new UIAutomationClient.CUIAutomation(), CreateIAutomationPatternFactory(), CreateIAutomationElement));

        private static IAutomationElement CreateIAutomationElement(IUIAutomationElement comElement) =>
            new AutomationElement(
                comElement,
                CreateIAutomationPatternFactory(),
                CreateIAutomationFactory(),
                CreateIAutomationTreeNavigationHelper());

        private static IAutomationHelper CreateIAutomationHelper() =>
            new AutomationHelper(CreateIAutomationFactory());

        private static IAutomationPatternFactory CreateIAutomationPatternFactory() =>
            new AutomationPatternFactory(CreateIAutomationPattern, CreateIAutomationPatternByType);

        private static IReadOnlyDictionary<PatternType, ConstructorInfo> _patternTypes;

        private static IAutomationPattern CreateIAutomationPattern(
            (PatternType patternType, IAutomationElement element) parameters)
        {
            if (_patternTypes == null)
                _patternTypes = GetPatternTypes();

            if (!_patternTypes.TryGetValue(
                parameters.patternType,  out ConstructorInfo constructor))
                return null;

            var pattern = ConstructPattern(constructor, parameters.element);
            return (pattern == null
                ? null
                : pattern.IsSupportedBy(parameters.element) ? pattern : null
            );
        }

        private static IAutomationPattern CreateIAutomationPatternByType((Type type, IAutomationElement element) parameters)
        {
            if (_patternTypes == null)
                _patternTypes = GetPatternTypes();

            return
                _patternTypes.Values
                .SingleOrDefault(x => parameters.type.IsAssignableFrom(x.DeclaringType))
                ?.Map(ConstructPattern(parameters.element))
                .Map(NullIfNotSupported(parameters.element));
        }

        private static IAutomationTreeNavigationHelper CreateIAutomationTreeNavigationHelper() =>
            new AutomationTreeNavigationHelper(CreateIAutomationFactory());



        private static
            IReadOnlyDictionary<PatternType, ConstructorInfo> GetPatternTypes()
        {
            // Get all the constructors for all the concrete IAutomationPattern
            // implementations
            var constructors = Assembly.GetExecutingAssembly()
                .GetConcreteImplementations<IAutomationPattern>()
                .Select(x => x.GetConstructors().Single());

            // Invoke each constructor and store the resulting instance against the
            // pattern type that it represents.
            var map = new Dictionary<PatternType, ConstructorInfo>();
            foreach (var c in constructors)
            {
                var instance = ConstructPattern(c, null);
                if (instance != null)
                {
                    map[instance.PatternType] = c;
                }
            }
            return GetReadOnly.IReadOnlyDictionary(map);
        }

        private static Func<ConstructorInfo, IAutomationPattern> ConstructPattern(IAutomationElement element) =>
            constructor =>
                ConstructPattern(constructor, element);

        private static IAutomationPattern ConstructPattern(ConstructorInfo constructor, IAutomationElement element)
        {
            object[] args =
                GetPatternConstructorArguments(constructor, element).ToArray();
            return constructor.Invoke(args) as IAutomationPattern;
        }

        private static IEnumerable<object> GetPatternConstructorArguments(
            ConstructorInfo constructor, IAutomationElement element)
        {
            return
                from param in constructor.GetParameters()
                select GetPatternConstructorArgument(element, param.ParameterType);
        }

        private static Func<IAutomationPattern, IAutomationPattern> NullIfNotSupported(IAutomationElement element) =>
            pattern =>
                pattern.IsSupportedBy(element) ? pattern : null;

        private static object GetPatternConstructorArgument(
            IAutomationElement element, Type type)
        {
            if (type == typeof(IAutomationElement))
                return element;

            if (type == typeof(IAutomationFactory))
                return CreateIAutomationFactory();

            throw new InvalidArgumentException(
                "Type '{0}' requested in pattern constructor is not supported", type);
        }

    }
}
