using BluePrism.CharMatching.Properties;
using System;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Attribute that can be applied to specify the range of values available for a property.
    /// This is currently only used by <see cref="SpyRegion"/> but could be moved
    /// to a shared location if needed to be used by other properties.</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class RangeAttribute : Attribute
    {
        /// <summary>
        /// Creates a new instance of <see cref="RangeAttribute"/>, that allows you to specify
        /// the range of values that is valid for a property.
        /// </summary>
        /// <param name="min">The lowest acceptable value of the range</param>
        /// <param name="max">The highest acceptable value of the range</param>
        /// <exception cref="ArgumentException">If <paramref name="min"/> and <paramref name="max"/> are not the same type</exception>
        /// <exception cref="ArgumentException">If <paramref name="min"/> does not implement IComparable</exception>
        /// <exception cref="ArgumentException">If <paramref name="max"/> does not implement IComparable</exception>
        /// <exception cref="ArgumentException">If <paramref name="min"/> is greater than <paramref name="max"/></exception>
        public RangeAttribute(object min, object max)
        {
            // In order to evaluate whether a value falls within a specific range, both the
            // min and max values must implement IComparable
            if (min.GetType() != max.GetType()) throw new ArgumentException(string.Format(Resources.MinAndMaxMustBeTheSameType01, min.GetType(), max.GetType())); 
            if (!(min is IComparable)) throw new ArgumentException(Resources.DoesNotSupportIComparable, nameof(min)); 
            if (!(max is IComparable)) throw new ArgumentException(Resources.DoesNotSupportIComparable, nameof(max));
            
            IComparable comparable = (IComparable)min;
            if (((IComparable)min).CompareTo(max) > 0) throw new ArgumentException(Resources.MinCannotBeGreaterThanMax); ;

            Min = min;
            Max = max;
        }
        
        /// <summary>
        /// The minimum acceptable value for a property
        /// </summary>
        public object Min { get; private set; }

        /// <summary>
        /// The maximum acceptable value for a property
        /// </summary>
        public object Max { get; private set; }

        /// <summary>
        /// Returns True, if the specified value falls within the range
        /// </summary>
        /// <param name="value">The value to check is within the range</param>
        /// <returns>True, if the specified value falls within the range</returns>
        public bool IsInRange(object value)
        {
            IComparable comparable = (IComparable)value;
            return comparable != null && comparable.CompareTo(Max) <= 0 && comparable.CompareTo(Min) >= 0;                              
        }

        /// <summary>
        /// Returns True, if the specified value is above the specified range
        /// </summary>
        /// <param name="value">The value to check is greater than the range</param>
        /// <returns>True, if the specified value is above the specified range</returns>
        public bool IsGreaterThanRange(object value)
        { 
            IComparable comparable = (IComparable) value;
            return comparable != null && comparable.CompareTo(Max) > 0;             
        }

        /// <summary>
        /// Returns True, if the specified value is below the specified range
        /// </summary>
        /// <param name="value">The value to check is less than the range</param>
        /// <returns>True, if the specified value is below the specified range</returns
        public bool IsLessThanRange(object value)
        { 
            IComparable comparable = (IComparable) value;
            return comparable != null && comparable.CompareTo(Min) < 0;             
        }
    }
}
