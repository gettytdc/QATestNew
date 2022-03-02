using System;
using System.ComponentModel;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Registered with the TypeDescriptor class to create SpyRegionTypeDescriptor instances
    /// </summary>
    internal class SpyRegionTypeDescriptorProvider : TypeDescriptionProvider
    {
        public SpyRegionTypeDescriptorProvider()
            : base(TypeDescriptor.GetProvider(typeof(SpyRegion)))
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            var baseDescriptor = base.GetTypeDescriptor(objectType, instance);

            return new SpyRegionTypeDescriptor(baseDescriptor, (SpyRegion) instance);
        }
    }
}