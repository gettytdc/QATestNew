using BluePrism.Core.Resources;
using System.Collections.Generic;

namespace BluePrism.Core.Extensions
{
    public static class ResourceAttributeExtensions
    {
        public static bool IsPrivate(this ResourceAttribute @this)
            => @this.HasFlag(ResourceAttribute.Private);

        public static bool IsRetired(this ResourceAttribute @this)
            => @this.HasFlag(ResourceAttribute.Retired);

        /// <summary>
        /// Construct a sequence of the components of this Attribute.
        /// This could probably be a generic enum extension. Theres nothing particular about it.
        /// </summary>
        public static IEnumerable<int> ToIntList(this ResourceAttribute @this)
        {
            for(int i = 1; i < 255; i <<= 1)
            {
                var attr = (ResourceAttribute)i;
                if (@this.HasFlag(attr))
                    yield return (int)attr;
            }
        }
    }
}
