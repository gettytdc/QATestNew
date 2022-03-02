namespace BluePrism.Api.Setup.Common
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using Utilities.Functional;

    public static class CommandLineParser
    {
        private static readonly Regex CommandLineParseRegex = new Regex(@"-(?<a>[^\s]+)(?: (?:""(?<v>.*?(?<!\\))""|(?<v>[^-][^\s]*)))?", RegexOptions.Compiled, TimeSpan.FromSeconds(5));

        public static T ParseCommandLine<T>(string commandLine) where T : new()
        {
            var propertySetters = GetTargetProperties<T>().ToDictionary(k => k.Name, v => v.SetValue);
            var result = new T();
            GetParameters(commandLine)
                .Where(x => propertySetters.ContainsKey(x.Argument))
                .ForEach(x => propertySetters[x.Argument](result, x.Value))
                .Evaluate();

            return result;
        }

        private static IEnumerable<(string Name, Action<T, string> SetValue)> GetTargetProperties<T>() =>
            typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(x => (SetValue: GetPropertySetter<T>(x), ParameterAttributes: x.GetCustomAttributes(typeof(CommandLineParameterAttribute), true).Cast<CommandLineParameterAttribute>()))
            .Where(x => x.ParameterAttributes.Any())
            .SelectMany(x => x.ParameterAttributes.Select(p => (p.Name, SetValue: x.SetValue)));

        private static Action<TParent, string> GetPropertySetter<TParent>(PropertyInfo property)
        {
            // Convert to use C# 8.0 switch statement when able
            var converter =
                property.PropertyType == typeof(string) ? x => x
                : property.PropertyType == typeof(bool) ? _ => true
                : GetParseOrConvertMethod(property.PropertyType);

            var setter = property.GetSetMethod();

            return (parent, value) => setter.Invoke(parent, new[] { converter(value) });
        }

        private static Func<string, object> GetParseOrConvertMethod(Type propertyType) =>
            (Func<string, object>)propertyType.GetMethod("Parse", new[] { typeof(string) })?.Map<MethodInfo, Func<object, object>>(x => value => x.Invoke(null, new[] { value }))
            ?? (value => TypeDescriptor.GetConverter(propertyType).ConvertFromString(value));

        private static IEnumerable<(string Argument, string Value)> GetParameters(string commandLine) =>
            CommandLineParseRegex
                .Matches(commandLine)
                .Cast<Match>()
                .Select(x => (Argument: x.Groups["a"].Value, Value: x.Groups["v"].Value.Replace("\\\"", "\"")));
    }
}
