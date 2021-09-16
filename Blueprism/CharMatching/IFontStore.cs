using System;
using System.Collections.Generic;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// A storage interface for fonts - this allows access to a list of available
    /// fonts and offers methods to load, save and delete them.
    /// Typically, this would be backed by the database, but it could be backed by
    /// a set of files or embedded fonts if necessary.
    /// </summary>
    public interface IFontStore
    {
        /// <summary>
        /// Get the definition of the specified font.
        /// </summary>
        /// <param name="name">The name of the font.</param>
        /// <returns>The font corresponding to the given name</returns>
        /// <exception cref="NoSuchFontException">If no font was found with the
        /// given name.</exception>
        BPFont GetFont(string name);
        string GetFontOcrPlus(string name);

        /// <summary>
        /// Saves the given font to the store
        /// </summary>
        /// <param name="font">The font to store</param>
        /// <exception cref="Exception">If the font could not be stored for any
        /// reason</exception>
        void SaveFont(BPFont font);
        void SaveFontOcrPlus(string name, string data);

        /// <summary>
        /// Deletes the font with the given name from the store
        /// </summary>
        /// <param name="name">The name of the font to delete</param>
        /// <returns>True if the font was found in the store and deleted, false if
        /// no font was found in the store with the specified name</returns>
        bool DeleteFont(string name);

        /// <summary>
        /// Gets a collection of available font names in this store
        /// </summary>
        ICollection<string> AvailableFontNames { get; }
    }
}
