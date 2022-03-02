using System;
using BluePrism.BPCoreLib;
using BluePrism.Server.Domain.Models;

namespace BluePrism.Core.Data
{
    /// <summary>
    /// Basic implementation of the item info used within instances of the
    /// <see cref="DataGridViewItemCell"/> class within a data grid view.
    /// This typically wouldn't be used directly (though there's no reason why it
    /// couldn't be)
    /// </summary>
    public class BasicItemHeader : IItemHeader
    {
        /// <summary>
        /// Override of the basic info class, which does not allow the properties to
        /// be modified after creation. Currently only used to designate an 'empty'
        /// item info instance.
        /// </summary>
        private class ReadOnlyDataGridViewItemInfo : BasicItemHeader
        {
            public ReadOnlyDataGridViewItemInfo() : this(null, null, null) { }
            public ReadOnlyDataGridViewItemInfo(
                string title, string subtitle, string imgKey)
            {
                base.Title = title;
                base.SubTitle = subtitle;
                base.ImageKey = imgKey;
            }
            public override string Title
            {
                get { return base.Title; }
                set { throw new NotModifiableException(); }
            }
            public override string SubTitle
            {
                get { return base.SubTitle; }
                set { throw new NotModifiableException(); }
            }
            public override string ImageKey
            {
                get { return base.ImageKey; }
                set { throw new NotModifiableException(); }
            }
        }

        /// <summary>
        /// An empty item info object (note: this instance is readonly)
        /// </summary>
        public static readonly BasicItemHeader Empty =
            new ReadOnlyDataGridViewItemInfo();

        // The main title of the item
        private string _title;

        // The subtitle of the item
        private string _subtitle;

        // The image key of the item
        private string _imageKey;

        /// <summary>
        /// The title of the item that this info is regarding
        /// </summary>
        public virtual string Title
        {
            get { return _title ?? ""; }
            set { _title = value; }
        }

        /// <summary>
        /// The subtitle of the item that this info is regarding
        /// </summary>
        public virtual string SubTitle
        {
            get { return _subtitle ?? ""; }
            set { _subtitle = value; }
        }

        /// <summary>
        /// The image key associated with the item that this info is regarding
        /// </summary>
        public virtual string ImageKey
        {
            get { return _imageKey ?? ""; }
            set { _imageKey = value; }
        }

        /// <summary>
        /// Shall I compare thee to this basic item header?
        /// This compares by image key (type) first, then title, then subtitle.
        /// </summary>
        /// <param name="comp">The item header to compare thee to.</param>
        /// <returns> A value that indicates the relative order of the objects being
        /// compared. The return value has these meanings: Value Meaning Less than
        /// zero This instance precedes obj in the sort order. Zero This instance
        /// occurs in the same position in the sort order as obj. Greater than zero
        /// This instance follows obj in the sort order.
        /// </returns>
        /// <exception cref="ArgumentNullException">If <paramref name="comp"/> is
        /// null.</exception>
        /// <exception cref="ArgumentException">If <paramref name="comp"/> is not an
        /// instance of <see cref="IItemHeader"/>.</exception>
        public virtual int CompareTo(object comp)
        {
            if (comp == null) throw new ArgumentNullException(nameof(comp));

            IItemHeader hdr = comp as IItemHeader;
            if (hdr == null) throw new ArgumentException(string.Format(
                "Cannot compare this item header to an object of type: {0}",
                comp.GetType()), nameof(comp));

            int resp = ImageKey.CompareTo(hdr.ImageKey);
            if (resp != 0) return resp;

            resp = Title.CompareTo(hdr.Title);
            if (resp != 0) return resp;

            return SubTitle.CompareTo(hdr.SubTitle);
        }
    }
}
