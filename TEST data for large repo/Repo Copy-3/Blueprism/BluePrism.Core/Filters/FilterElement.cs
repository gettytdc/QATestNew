using System;

namespace BluePrism.Core.Filters
{
    /// <summary>
    /// An element of a filter which outlines a test to do on an attribute which,
    /// if passed, allows the element through the filter.
    /// </summary>
    [Serializable]
    public class FilterElement
    {
        /// <summary>
        /// The type of comparison to perform
        /// </summary>
        public FilterComparison Comparison { get; set; }

        /// <summary>
        /// The name of the attribute to filter on
        /// </summary>
        public string AttributeName { get; set; }

        /// <summary>
        /// The value to test the attribute against; note that this should be of a
        /// serializable type if the filter is going to be passed across an
        /// application domain boundary, eg. being sent to a BP Server.
        /// </summary>
        public object Value { get; set; }
    }
}
