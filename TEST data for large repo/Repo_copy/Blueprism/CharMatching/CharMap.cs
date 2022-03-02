using System;
using System.Collections.Generic;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// A map of CharData against the masks which are used to recognise them.
    /// </summary>
    [Serializable]
    public class CharMap : Dictionary<Mask,CharData>
    {
        /// <summary>
        /// Creates a new empty char map
        /// </summary>
        public CharMap() : this(null) { }

        /// <summary>
        /// Creates a new charmap from the given data.
        /// Note that if more than 1 element in the given collection use the same
        /// mask, only the latest one will be stored in this map.
        /// </summary>
        /// <param name="chars">The chars to generate the map from or null to create
        /// an empty char map</param>
        public CharMap(ICollection<CharData> chars)
        {
            if (chars != null)
            {
                foreach (CharData cd in chars)
                {
                    this[cd.Mask] = cd;
                }
            }
        }
    }
}
