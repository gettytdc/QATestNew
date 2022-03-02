using System;
using System.ComponentModel;
using System.Linq;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Custom PropertyDescriptor implementation that overrides a property's attributes
    /// in addition to overriding the Browsable, ReadOnly and SetValue behaviour
    /// </summary>
    /// <remarks>
    /// <para>
    /// We need to override the Attributes property to support filtering via 
    /// TypeDescriptor.GetProperties(Attribute[] attributes)), which is based on the
    /// descriptor's attributes. 
    /// </para>
    /// <para>
    /// This is currently only used by <see cref="SpyRegionTypeDescriptor"/> but could be moved
    /// to a shared location if used in descriptor extensions for other types</para>
    /// </remarks>
    internal class CustomPropertyDescriptor : PropertyDescriptor
    {
        private readonly PropertyDescriptor _inner;
        private readonly bool _browsable;
        private readonly bool _readOnly;
        private readonly AttributeCollection _attributes;

        /// <summary>
        /// Creates a new CustomPropertyDescriptor
        /// </summary>
        public CustomPropertyDescriptor(PropertyDescriptor inner, bool browsable, bool readOnly)
            : base(inner)
        {
            _inner = inner;
            _browsable = browsable;
            _readOnly = readOnly;
            _attributes = ModifyBaseAttributes();
        }

        /// <summary>
        /// Overrides attributes based on values we are overriding
        /// </summary>
        /// <returns></returns>
        protected AttributeCollection ModifyBaseAttributes()
        {
            bool updatedBrowsable = false;
            var attributes = base.Attributes.Cast<Attribute>().Select(attribute =>
            {
                var browsableAttribute = attribute as BrowsableAttribute;
                if (browsableAttribute != null && browsableAttribute.Browsable != _browsable)
                {
                    updatedBrowsable = true;
                    return new BrowsableAttribute(_browsable);
                }
                else
                {
                    return attribute;
                }

            }).ToList();
            if (!updatedBrowsable)
            {
                attributes.Add(new BrowsableAttribute(_browsable));
            }
            return new AttributeCollection(attributes.ToArray());
        }

        #region Custom overrides
        
        /// <summary>
        /// Returns our modified attributes which are used by 
        /// TypeDescriptor.GetProperties(Attribute[] attributes)), which is used when
        /// finding the properties to display in the PropertyGrid
        /// </summary>
        public override AttributeCollection Attributes
        {
            get { return _attributes; }
        }

        /// <summary>
        /// Override browsable based on specified value. We can't delegate this to the 
        /// inner descriptor (internal ReflectPropertyDescriptor implementation within 
        /// System.ComponentModel, whose IsReadonly and IsBrowsable are based on the 
        /// property's attributes) as we haven't changed the attributes of inner descriptor.
        /// </summary>
        public override bool IsBrowsable
        {
            get { return _browsable; }
        }

        /// <summary>
        /// Override readonly based on specified value. We can't delegate this to the 
        /// inner descriptor (internal ReflectPropertyDescriptor implementation within 
        /// System.ComponentModel, whose IsReadonly and IsBrowsable are based on the 
        /// property's attributes) as we haven't changed the attributes of inner descriptor.
        /// </summary>
        public override bool IsReadOnly
        {
            get { return _readOnly; }
        }

        /// <summary>
        /// Override the set value property. This will delegate to the inner instance's
        /// SetValue function, but if there is a range attribute it will first check to
        /// see if the value falls within the range. If it doesn't and the value is higher
        /// than the range then it will clip the value to be the range's max. If it doesn't 
        /// and the value is lower than the range then it will clip the value to be the range's min.
        /// Any values that fall within the range specified by the range attribute will remain
        /// unaffected.
        /// </summary>
        public override void SetValue(object component, object value)
        {
            // If the property has a range attribute, then clip any values that fall
            // outside this range to be within the range
            RangeAttribute range = Attributes.OfType<RangeAttribute>().FirstOrDefault();
            if (range != null)
            {
                if (range.IsGreaterThanRange(value)) value = range.Max;
                if (range.IsLessThanRange(value)) value = range.Min;
            }

            _inner.SetValue(component, value);
        }

        #endregion

        #region Overrides delegated to inner instance 

        public override bool CanResetValue(object component)
        {
            return _inner.CanResetValue(component);
        }

        public override object GetValue(object component)
        {
            return _inner.GetValue(component);
        }

        public override void ResetValue(object component)
        {
            _inner.ResetValue(component);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return _inner.ShouldSerializeValue(component);
        }

        public override Type ComponentType
        {
            get { return _inner.ComponentType; }
        }
        
        public override Type PropertyType
        {
            get { return _inner.PropertyType; }
        }

        #endregion
    }
}