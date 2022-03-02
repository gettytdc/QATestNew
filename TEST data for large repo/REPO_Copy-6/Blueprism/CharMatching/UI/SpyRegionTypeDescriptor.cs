using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using BluePrism.Core.Utility;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// ICustomTypeDescriptor implementation to customise display of properties of
    /// a SpyRegion in the PropertyGrid control
    /// </summary>
    internal class SpyRegionTypeDescriptor : CustomTypeDescriptor
    {
        /// <summary>
        /// The SpyRegion being described
        /// </summary>
        private readonly SpyRegion _instance;

        public SpyRegionTypeDescriptor(ICustomTypeDescriptor parent, SpyRegion instance) : base(parent)
        {
            _instance = instance;
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var modifiedProperties = ModifyProperties(base.GetProperties(attributes));
            // Return properties sorted using the Sort Order property.
            // Note that this will only work in the property grid if the PropertySort
            // is set to CategorizedAlphabetical or Alphabetical
            return modifiedProperties.Sort(new PropertySortOrderComparer());
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var modifiedProperties = ModifyProperties(base.GetProperties());
            // Return properties sorted using the Sort Order property.
            // Note that this will only work in the property grid if the PropertySort
            // is set to CategorizedAlphabetical or Alphabetical
            return modifiedProperties.Sort(new PropertySortOrderComparer());
        }

        /// <summary>
        /// Changes meta data to control how properties are displayed based on state of
        /// the SpyRegion 
        /// </summary>
        /// <param name="originalProperties">The PropertyDescriptors returned by the parent 
        /// (standard) descriptor for the type</param>
        /// <returns>A new collection containing the modified PropertyDescriptors</returns>
        private PropertyDescriptorCollection ModifyProperties(PropertyDescriptorCollection originalProperties)
        {
            var properties = originalProperties.OfType<PropertyDescriptor>().ToList();
            Action<List<PropertyDescriptor>> modify;
            switch (_instance)
            {
                case ListSpyRegion _:
                case GridSpyRegion _:
                    modify = ModifyListRegionProperties;
                    break;
                case var s when s.LocationMethod == RegionLocationMethod.Image:
                    modify = ModifyImageSpyRegionProperties;
                    break;
                default:
                    modify = ModifyCoordinatesSpyRegionProperties;
                    break;
            }
            modify(properties);
            return new PropertyDescriptorCollection(properties.ToArray(), true);
        }

        private void ModifyCommonRegionProperties(List<PropertyDescriptor> properties)
        {
            // Only show relative parent if position is set to Relative
            var relativeParentBrowsable = _instance.RegionPosition == RegionPosition.Relative;
            WrapProperty(properties, PropertyNames.RelativeParent, relativeParentBrowsable, false);
        }

        /// <summary>
        /// Modifies any PropertyDescriptors that apply to ListSpyRegions or GridSpyRegions
        /// </summary>
        /// <param name="properties">The PropertyDescriptors returned by the parent 
        /// (standard) descriptor for the type - this list is modified in-place</param>
        private void ModifyListRegionProperties(List<PropertyDescriptor> properties)
        {
            ModifyCommonRegionProperties(properties);

            WrapProperty(properties, PropertyNames.LocationMethod, false, false);
            WrapProperty(properties, PropertyNames.ImageSearchPadding, false, false);
            WrapProperty(properties, PropertyNames.ImageSearchAllPadding, false, false);
            WrapProperty(properties, PropertyNames.ImageSearchLeftPadding, false, false);
            WrapProperty(properties, PropertyNames.ImageSearchRightPadding, false, false);
            WrapProperty(properties, PropertyNames.ImageSearchTopPadding, false, false);
            WrapProperty(properties, PropertyNames.ImageSearchBottomPadding, false, false);
            WrapProperty(properties, PropertyNames.ColourTolerance, false, false);
            WrapProperty(properties, PropertyNames.Greyscale, false, false);
        }

        /// <summary>
        /// Modifies any PropertyDescriptors that apply to SpyRegions set to a location method
        /// of Image.
        /// </summary>
        /// <param name="properties">The PropertyDescriptors returned by the parent 
        /// (standard) descriptor for the type - this list is modified in-place</param>
        private void ModifyImageSpyRegionProperties(List<PropertyDescriptor> properties)
        {
            ModifyCommonRegionProperties(properties);

            WrapProperty(properties, PropertyNames.RetainImage, true, true);

            // Show image search specific properties
            var paddingBrowsable = _instance.RegionPosition != RegionPosition.Anywhere;
            WrapProperty(properties, PropertyNames.ImageSearchPadding, false, false);
            WrapProperty(properties, PropertyNames.ImageSearchAllPadding, paddingBrowsable, false);
            WrapProperty(properties, PropertyNames.ImageSearchLeftPadding, paddingBrowsable, false);
            WrapProperty(properties, PropertyNames.ImageSearchRightPadding, paddingBrowsable, false);
            WrapProperty(properties, PropertyNames.ImageSearchTopPadding, paddingBrowsable, false);
            WrapProperty(properties, PropertyNames.ImageSearchBottomPadding, paddingBrowsable, false);
            WrapProperty(properties, PropertyNames.ColourTolerance, true, false);
            WrapProperty(properties, PropertyNames.Greyscale, true, false);
        }

        /// <summary>
        /// Modifies any PropertyDescriptors that apply to SpyRegions set to a location method
        /// of coordinates.
        /// </summary>
        /// <param name="properties">The PropertyDescriptors returned by the parent 
        /// (standard) descriptor for the type - this list is modified in-place</param>
        private void ModifyCoordinatesSpyRegionProperties(List<PropertyDescriptor> properties)
        {
            ModifyCommonRegionProperties(properties);

            // Hide image search specific properties
            WrapProperty(properties, PropertyNames.ImageSearchPadding, false, false);

            WrapProperty(properties, PropertyNames.ImageSearchAllPadding, false, false);
            WrapProperty(properties, PropertyNames.ImageSearchLeftPadding, false, false);
            WrapProperty(properties, PropertyNames.ImageSearchRightPadding, false, false);
            WrapProperty(properties, PropertyNames.ImageSearchTopPadding, false, false);
            WrapProperty(properties, PropertyNames.ImageSearchBottomPadding, false, false);

            WrapProperty(properties, PropertyNames.ColourTolerance, false, false);
            WrapProperty(properties, PropertyNames.Greyscale, false, false);
            
            FindPropertyByName(properties, PropertyNames.Greyscale).SetValue(_instance, false);

            // Don't allow region position to be "anywhere" when using co-ordinate search
            if (_instance.RegionPosition == UI.RegionPosition.Anywhere)
                FindPropertyByName(properties, PropertyNames.RegionPosition).SetValue(_instance, RegionPosition.Fixed);
        }

        /// <summary>
        /// Replaces descriptor within the supplied list with a new instance, overriding the 
        /// IsBrowsable and IsReadOnly values
        /// </summary>
        /// <param name="properties">The list containing the PropertyDescriptor</param>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="isBrowsable">Override value for isBrowsable, which determines whether
        /// it is visible</param>
        /// <param name="isReadOnly">Override value for isReadOnly, which determines whether
        /// the property is editable</param>
        private void WrapProperty(List<PropertyDescriptor> properties, string propertyName, bool isBrowsable, bool isReadOnly)
        {
            var property = FindPropertyByName(properties, propertyName);
            int index = properties.IndexOf(property);
            var customProperty = new CustomPropertyDescriptor(property, isBrowsable, isReadOnly);
            properties[index] = customProperty;
        }

        private static PropertyDescriptor FindPropertyByName(List<PropertyDescriptor> properties, string name)
        {
            var property = properties.SingleOrDefault(x => x.Name == name);
            if (property == null)
            {
                throw new ArgumentException(string.Format(Resources.CouldNotFindProperty0, name));
            }
            return property;
        }

        /// <summary>
        /// Internal class used to avoid hard-coded property name strings
        /// </summary>
        private static class PropertyNames
        {
            public static readonly string LocationMethod =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.LocationMethod);

            public static readonly string RetainImage =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.RetainImage);

            public static readonly string RegionPosition =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.RegionPosition);

            public static readonly string ImageSearchPadding =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.ImageSearchPadding);

            public static readonly string ImageSearchAllPadding =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.ImageSearchAllPadding);

            public static readonly string ImageSearchLeftPadding =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.ImageSearchLeftPadding);

            public static readonly string ImageSearchRightPadding =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.ImageSearchRightPadding);

            public static readonly string ImageSearchTopPadding =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.ImageSearchTopPadding);

            public static readonly string ImageSearchBottomPadding =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.ImageSearchBottomPadding);

            public static readonly string RelativeParent =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.RelativeParent);

            public static readonly string ColourTolerance =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.ColourTolerance);

            public static readonly string Greyscale =
                MemberAccessHelper.GetMemberName((SpyRegion x) => x.Greyscale);
        }
    }
}