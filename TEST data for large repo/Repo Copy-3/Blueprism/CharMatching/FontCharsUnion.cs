using BluePrism.CharMatching.Properties;
using System;
using System.Collections.Generic;
using BluePrism.BPCoreLib.Collections;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// Class to represent a union between the chars on a font and a free collection
    /// of chars.
    /// It provides a mechanism for separating those which occur only in the font,
    /// only in the free chars and in both.
    /// </summary>
    public class FontCharsUnion : IEnumerable<CharData>
    {
        #region - Published Events -

        /// <summary>
        /// Event fired when chars in this union have been merged into its font
        /// </summary>
        public event EventHandler FontMerged;

        #endregion

        #region - Member Variables -

        // The font comprising part of this union - may be null
        BPFont _font;

        // Non-font chars in this union. This collection is set within this class
        // and can only be added to / removed from externally, not set to an
        // externally maintained collection.
        IBPSet<CharData> _chars;

        #endregion

        #region - Constructors -

        /// <summary>
        /// Creates a new empty union of font chars and free chars
        /// </summary>
        public FontCharsUnion() : this(null, null) { }

        /// <summary>
        /// Creates a new union of font chars and free chars using the given font and
        /// free character collection
        /// </summary>
        /// <param name="f">The font to draw the font characters from</param>
        /// <param name="freeChars">The collection of free characters to use. Any
        /// duplicates (ie. characters represented by the same glyph, regardless of
        /// the character associated with them) will be stripped when transferred to
        /// the collection within this union.</param>
        public FontCharsUnion(BPFont f, ICollection<CharData> freeChars)
        {
            _chars = new clsSet<CharData>(CharData.CharlessEqualityComparer);
            _font = f;
            if (freeChars != null)
                _chars.Union(freeChars);
        }

        #endregion

        #region - Mutable Properties -

        /// <summary>
        /// The font which comprises part of this union
        /// </summary>
        public BPFont Font
        {
            get { return _font; }
        }

        /// <summary>
        /// Modifiable set of the free characters in this union. This is the only
        /// modifiable collection exposed by this class.
        /// Also note that this compares the CharData objects using the
        /// <see cref="CharData.CharlessComparer"/> comparer, so that only the glyphs
        /// representing the characters are taken into account when testing for
        /// equality with other CharData objects.
        /// </summary>
        public IBPSet<CharData> FreeChars
        {
            get { return _chars; }
        }

        #endregion

        #region - Fully ReadOnly Properties -

        /// <summary>
        /// Gets a readonly collection of the chars that exist in both the font
        /// and in the collection of free characters held in this union
        /// </summary>
        public ICollection<CharData> CharsInFontAndFree
        {
            get
            {
                // Anything empty means that there's nothing in both
                if (_font == null || _font.IsEmpty || FreeChars.Count == 0)
                    return GetEmpty.ICollection<CharData>();

                // Otherwise, get the freechars into a set and intersect
                // them with the characters in the font
                IBPSet<CharData> charSet = new clsSet<CharData>(
                    CharData.CharlessEqualityComparer, _font.CharacterData);
                charSet.Intersect(FreeChars);
                return GetReadOnly.ICollection(new clsAutoSortedList<CharData>(
                    CharData.CharOnlyComparer, charSet));
            }
        }

        /// <summary>
        /// Gets a readonly collection of the chars that exist in the font, but not
        /// in the collection of free characters in this union
        /// </summary>
        public ICollection<CharData> CharsInFontOnly
        {
            get
            {
                // If the font is null or empty, there are no chars to return
                if (_font == null || _font.IsEmpty)
                    return GetEmpty.ICollection<CharData>();

                // If there are no free chars, we can just return a readonly
                // wrapper around the font chars
                if (CollectionUtil.IsNullOrEmpty(_chars))
                    return GetReadOnly.ICollection(new clsAutoSortedList<CharData>(
                    CharData.CharOnlyComparer, _font.CharacterData));

                // Otherwise, get the font's chars into a set and remove any
                // which exist in the free chars
                IBPSet<CharData> charSet = new clsSet<CharData>(
                    CharData.CharlessEqualityComparer, _font.CharacterData);
                charSet.Subtract(FreeChars);
                return GetReadOnly.ICollection(new clsAutoSortedList<CharData>(
                    CharData.CharOnlyComparer, charSet));
            }
        }

        /// <summary>
        /// Gets a readonly collection of the chars that exist in the free characters
        /// but not in the font registered in this union
        /// </summary>
        public ICollection<CharData> CharsInFreeOnly
        {
            get
            {
                // If the font is null or empty, all the free chars should return;
                // just wrap it into a readonly collection first.
                if (_font == null || _font.IsEmpty)
                    return GetReadOnly.ICollection(new clsAutoSortedList<CharData>(
                    CharData.CharOnlyComparer, FreeChars));

                // If there are no free chars, there's nothing to return
                if (CollectionUtil.IsNullOrEmpty(_chars))
                    return GetEmpty.ICollection<CharData>();

                // Otherwise, get the free chars into a set and remove any which
                // exist in the font.
                IBPSet<CharData> charSet =
                    new clsSet<CharData>(CharData.CharlessEqualityComparer, FreeChars);
                charSet.Subtract(_font.CharacterData);
                return GetReadOnly.ICollection(new clsAutoSortedList<CharData>(
                    CharData.CharOnlyComparer, charSet));
            }
        }

        /// <summary>
        /// Gets the read-only collection of chars on the font set in this union,
        /// or an empty collection if no font is set or the font has no characters.
        /// </summary>
        public ICollection<CharData> FontChars
        {
            get
            {
                if (_font == null || _font.IsEmpty)
                    return GetEmpty.ICollection<CharData>();
                return GetReadOnly.ICollection(_font.CharacterData);
            }
        }

        /// <summary>
        /// Gets a readonly collection of all the chars in this union
        /// </summary>
        public ICollection<CharData> All
        {
            get
            {
                IBPSet<CharData> charSet = new clsSet<CharData>(FreeChars);
                charSet.Union(FontChars);
                return GetReadOnly.ICollection(charSet);
            }
        }

        #endregion

        #region - Enumerable Implementations -

        /// <summary>
        /// Gets a enumerator over all the chars in this union
        /// </summary>
        /// <returns>An enumerator over all of the chars in this union</returns>
        public IEnumerator<CharData> GetEnumerator()
        {
            return All.GetEnumerator();
        }

        /// <summary>
        /// Gets a enumerator over all the chars in this union
        /// </summary>
        /// <returns>An enumerator over all of the chars in this union</returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
            
        }

        #endregion

        #region - Methods -

        /// <summary>
        /// Raises the <see cref="FontMerged"/> event
        /// </summary>
        protected virtual void OnFontMerged(EventArgs e)
        {
            EventHandler h = this.FontMerged;
            if (h != null)
                h(this, e);
        }

        /// <summary>
        /// Merges the free chars on this union into the font on this union.
        /// </summary>
        /// <exception cref="NoSuchFontException">If no font is currently set in this
        /// union and thus the characters cannot be merged.</exception>
        public void MergeFreeCharsIntoFont()
        {
            if (_font == null)
                throw new NoSuchFontException(
                    Resources.NoFontSetInUnionCannotMergeCharacters);
            _font.Merge(CollectionUtil.Filter(FreeChars,
                delegate(CharData cd) { return cd.HasValue; }));
            OnFontMerged(EventArgs.Empty);
        }

        /// <summary>
        /// Removes the given collection of chars from this union, removing them from
        /// both the 'free' characters and the font characters.
        /// </summary>
        /// <param name="chars">The characters to remove from this union.</param>
        public void RemoveChars(ICollection<CharData> chars)
        {
            _chars.Subtract(chars);
            if (_font != null)
                _font.RemoveAll(chars);
        }

        #endregion

    }
}
