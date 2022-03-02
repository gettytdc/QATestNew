using BluePrism.BPCoreLib;

namespace BluePrism.CharMatching.UI
{
    /// <summary>
    /// Class describing the schema (ie. rows and columns) of a
    /// <see cref="GridSpyRegion"/>. It is handled in a property grid as an
    /// expandable object, showing a "rows x columns" overview by default.
    /// Full editing takes place using the <see cref="GridSpyRegionSchemaTypeEditor"/>
    /// </summary>
    /// <remarks>This class has largely been moved into BPCoreLib - this shell
    /// remains so that the UI editor definitions can remain in a UI-centred assembly
    /// rather than a more general one.</remarks>
    public class GridSpyRegionSchema : GridSchema
    {
        #region - Constructors -

        /// <summary>
        /// Creates a new schema for a grid spy region with 2 rows and 2 columns,
        /// each occupying 50% of their respective measurements
        /// </summary>
        public GridSpyRegionSchema() : base() { }

        /// <summary>
        /// Creates a new schema for a grid spy region with the given number of
        /// columns and rows.
        /// </summary>
        /// <param name="cols">The number of columns to create in the new schema.
        /// <param name="rows">The number of rows to create in the new schema.</param>
        /// </param>
        public GridSpyRegionSchema(int cols, int rows) : base(cols, rows) { }

        public GridSpyRegionSchema(string encoded) : base(encoded) { }

        #endregion

        #region - Methods -

        /// <summary>
        /// Creates and returns a deep clone of this schema
        /// </summary>
        /// <returns>A deep clone of this schema</returns>
        /// <remarks>This just returns a clone of the base class cast into this
        /// class for ease of code usage where it is needed.</remarks>
        public new GridSpyRegionSchema Clone()
        {
            return (GridSpyRegionSchema)base.Clone();
        }

        #endregion
    }

}
