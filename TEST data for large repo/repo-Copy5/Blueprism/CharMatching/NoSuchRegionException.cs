using BluePrism.CharMatching.Properties;
using System;
using BluePrism.Server.Domain.Models;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace BluePrism.CharMatching
{
    /// <summary>
    /// Exception indicating that a requested image region could not be found.
    /// </summary>
    [Serializable]
    public class NoSuchImageRegionException : NoSuchElementException
    {
        /// <summary>
        /// Gets the level at which the image region search failed, where 0 is the 
        /// final region required, 1 is that region's parent, 2 is the grandparent 
        /// and so forth.
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// Creates a new exception indicating that an image region could not be 
        /// located at a specified level of the search.
        /// </summary>
        /// <param name="level">The level at which the search failed to find a region
        /// where 0 is the last region to be found (ie. the target) and each
        /// increment above that represents the next immediate parent in the
        /// hierarchy.</param>
        public NoSuchImageRegionException(int level)
            : base(level == 0
                  ? Resources.CannotFindRegionIdentifiedAsAnImage
                  : Resources.ARelativeParent0AboveTheTargetRegionCouldNotBeIdentifiedAsAnImage, level)
        {
            Level = level;
        }

        #region - Serialization handling -

        /// <summary>
        /// Creates and deserializes an exception from the given serialization
        /// objects.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data
        /// </param>
        /// <param name="ctx">The contextual information about the source or
        /// destination</param>
        protected NoSuchImageRegionException(SerializationInfo info, StreamingContext ctx)
            : base(info, ctx)
        {
            Level = (int)info.GetValue("NoSuchRegionException.Level", typeof(int));
        }

        /// <summary>
        /// Gets the object data for this exception into the given serialization
        /// info object.
        /// </summary>
        /// <param name="info">The info into which this object's data should be
        /// stored.</param>
        /// <param name="ctx">The context for the streaming</param>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(
            SerializationInfo info, StreamingContext ctx)
        {
            base.GetObjectData(info, ctx);
            info.AddValue("NoSuchRegionException.Level", Level, typeof(int));
        }
    
        #endregion

    }

}
