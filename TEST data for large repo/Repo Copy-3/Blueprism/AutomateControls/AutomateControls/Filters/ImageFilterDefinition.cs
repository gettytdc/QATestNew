using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.ObjectModel;

namespace AutomateControls.Filters
{
    /// <summary>
    /// Class to represent a filter definition which uses images to represent the
    /// filter terms.
    /// </summary>
    public class ImageFilterDefinition : BaseFilterDefinition
    {
        /// <summary>
        /// The filter items to display in filters using this definition.
        /// </summary>
        private ICollection<FilterItem> _items;

        /// <summary>
        /// Creates a new filter definition using images 
        /// </summary>
        /// <param name="name">The name to use in the filter described by this
        /// definition.</param>
        public ImageFilterDefinition(string name)
            : this(name, (IDictionary<string, Image>)null, true) { }

        /// <summary>
        /// Creates a new filter definition using images defined in the given
        /// map of keys to images, prepending the images with a 'null filter item'
        /// to indicate that no filtering should be applied.
        /// </summary>
        /// <param name="name">The name of the filter that this definition describes
        /// </param>
        /// <param name="dict">The map of images mapped to the filter terms that
        /// they represent.</param>
        public ImageFilterDefinition(string name, IDictionary<string, Image> dict)
            : this(name, dict, true) { }

        /// <summary>
        /// Creates a new image filter definition with the given values.
        /// </summary>
        /// <param name="name">The name of the filter described by this definition.
        /// </param>
        /// <param name="dict">The map of images onto the filter terms that the
        /// images represent.</param>
        /// <param name="prefixNullItem">True to prepend the list of images with
        /// a 'null' filter item, ie. an item indicating that no filtering should
        /// be applied.</param>
        public ImageFilterDefinition(string name, IDictionary<string, Image> dict, bool prefixNullItem)
            : this(name, ExtractFilterItems(dict, prefixNullItem)) { }

        /// <summary>
        /// Creates a new image filter definition with the given filter items.
        /// </summary>
        /// <param name="name">The name of the filter defined by this definition.
        /// </param>
        /// <param name="items">The filter items representing the terms to display
        /// in the filter. The <see cref="FilterItem.DisplayValue"/> property is
        /// expected to be an <see cref="Image"/>, though a string will work too.
        /// </param>
        public ImageFilterDefinition(string name, IList<FilterItem> items)
            : this(name, items, true) { }

        /// <summary>
        /// Creates a new image filter definition with the given terms.
        /// </summary>
        /// <param name="name">The name of the filter described by this definition.
        /// </param>
        /// <param name="items">The filter items representing the terms to display
        /// in the filter. The <see cref="FilterItem.DisplayValue"/> property is
        /// expected to be an <see cref="Image"/>, though a string will work too.
        /// <param name="clone">True to clone the list of items, false to just
        /// use the given list as the backing value of this definition.</param>
        private ImageFilterDefinition(String name, IList<FilterItem> items, bool clone)
            : base(name, false)
        {
            _items = new ReadOnlyCollection<FilterItem>(
                clone ? new List<FilterItem>(items) : items);
        }

        /// <summary>
        /// Creates a list of filter items from the given map.
        /// </summary>
        /// <param name="dict">The map of images onto the corresponding filter
        /// terms from which the filter items should be drawn.</param>
        /// <param name="prefixWithNull">True to prefix the filter items list
        /// with the standard 'null' filter item. False to just add the filter
        /// items specified.</param>
        /// <returns>A list of filter items representing the given map of terms
        /// to images.</returns>
        private static IList<FilterItem> ExtractFilterItems(
            IDictionary<string, Image> dict, bool prefixWithNull)
        {
            IList<FilterItem> items = new List<FilterItem>();
            if (prefixWithNull)
                items.Add(new FilterItem());

            if (dict != null)
            {
                foreach (string term in dict.Keys)
                {
                    items.Add(new FilterItem(term, term, dict[term]));
                }
            }
            return items;
        }

        /// <summary>
        /// The collection of images that this filter definition uses to
        /// represent filter terms graphically.
        /// </summary>
        public ICollection<Image> Images
        {
            get
            {
                List<Image> imgs = new List<Image>();
                foreach (FilterItem item in _items)
                {
                    Image img = item.DisplayValue as Image;
                    if (img != null)
                        imgs.Add(img);
                }
                return imgs;
            }
        }

        /// <summary>
        /// Checks if this filter definition describes a filter represented by
        /// images. This definition does, so this will always return true.
        /// </summary>
        public override bool RepresentedByImages
        {
            get { return true; }
        }

        /// <summary>
        /// Gets the items to display in a filter described by this definition.
        /// </summary>
        public override ICollection<FilterItem> Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Creates an empty combo box for use by filters described by this
        /// definition. This definition requires an <see cref="ImageComboBox"/>.
        /// </summary>
        /// <returns>An empty combo box for use by filters described by this
        /// definition.</returns>
        protected override ComboBox CreateEmptyComboBox()
        {
            return new ImageComboBox();
        }

    }
}
