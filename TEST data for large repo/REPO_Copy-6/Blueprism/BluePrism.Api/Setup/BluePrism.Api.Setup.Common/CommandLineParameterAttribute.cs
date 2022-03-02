namespace BluePrism.Api.Setup.Common
{
    using System;

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class CommandLineParameterAttribute : Attribute
    {
        public string Name { get; }

        public CommandLineParameterAttribute(string name)
        {
            Name = name;
        }
    }
}
