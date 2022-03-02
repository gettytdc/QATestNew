using System;
using BluePrism.BPCoreLib;

namespace BluePrism.Core
{
    public class FactoryAttribute : Attribute
    {
        // The shared instance of the factory object, used by all instances of this
        // attribute
        private static object _inst;

        // The type of the factory to generate
        private Type _factoryClass;

        public FactoryAttribute(Type factoryClass)
        {
            _factoryClass = factoryClass;
        }

        /// <summary>
        /// The factory class associated with this attribute
        /// </summary>
        public Type FactoryClass { get; private set; }

        /// <summary>
        /// Gets the static instance associated with this factory attribute.
        /// </summary>
        public object Instance
        {
            get
            {
                if (_inst == null)
                    _inst = Activator.CreateInstance(FactoryClass);
                return _inst;
            }
        }

        /// <summary>
        /// Gets the factory instance associated with the given enumeration
        /// </summary>
        /// <typeparam name="T">The type of factory instance required.</typeparam>
        /// <param name="val">The enum value from which the associated factory
        /// instance is required.</param>
        /// <returns></returns>
        public static T GetFactory<T>(Enum val)
        {
            FactoryAttribute attr = BPUtil.GetAttributeValue<FactoryAttribute>(val);
            if (attr == null)
                return default(T);
            return (T)attr.Instance;
        }

    }
}
