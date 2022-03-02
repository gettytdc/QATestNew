using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace BluePrism.Scheduling
{
    /// <summary>
    /// Utility class to enable some (fairly basic) operations on data table
    /// and related objects.
    /// </summary>
    internal static class DataTableHelper
    {
        /// <summary>
        /// Helper method to get a date/time from a given data row, or
        /// DateTime.MinValue if the entry within the row is null.
        /// </summary>
        /// <param name="row">The row from which to draw the data.</param>
        /// <param name="colName">The column whose data is required</param>
        /// <returns>The value found in the given row at the given column,
        /// or <c>DateTime.MinValue</c> if no value was present.
        /// </returns>
        public static DateTime GetDateTime(DataRow row, string colName)
        {
            return GetDateTime(row, colName, DateTime.MinValue);
        }

        /// <summary>
        /// Helper method to get a date/time from a given data row, or get a
        /// default value if the entry within the row is null.
        /// </summary>
        /// <param name="row">The row from which to draw the data.</param>
        /// <param name="colName">The column whose data is required</param>
        /// <param name="defaultValue">The default date/time to use if the
        /// entry for that column in the given row is null.</param>
        /// <returns>The value found in the given row at the given column,
        /// or <paramref name="defaultValue"/> if no value was present.
        /// </returns>
        public static DateTime GetDateTime(DataRow row, string colName, DateTime defaultValue)
        {
            return (DateTime)(row.IsNull(colName) ? defaultValue : row[colName]);
        }

        /// <summary>
        /// Helper method to get a string from a given data row, or get a
        /// null value if the entry within the row is null
        /// </summary>
        /// <param name="row">The row from which to draw the data</param>
        /// <param name="colName">The column whose data is required</param>
        /// <returns>The value found in the given row at the given column,
        /// or null if no value was found.</returns>
        public static string GetString(DataRow row, string colName)
        {
            return (string)(row.IsNull(colName) ? null : row[colName]);
        }

        /// <summary>
        /// Helper method to get an int from a given data row.
        /// </summary>
        /// <param name="row">The row from which to draw the data</param>
        /// <param name="colName">The column whose data is required</param>
        /// <returns>The value found in the given row at the given column,
        /// or 0 if no value was found.</returns>
        public static int GetInt(DataRow row, string colName)
        {
            return GetInt(row, colName, 0);
        }

        /// <summary>
        /// Helper method to get an int from a given data row.
        /// </summary>
        /// <param name="row">The row from which to draw the data</param>
        /// <param name="colName">The column whose data is required</param>
        /// <param name="defaultValue">The value to return if the entry
        /// specified had a NULL value.</param>
        /// <returns>The value found in the given row at the given column,
        /// or 0 if no value was found.</returns>
        public static int GetInt(DataRow row, string colName, int defaultValue)
        {
            if (row.IsNull(colName))
                return defaultValue;

            // Just check and ensure that this isn't a byte (tinyint)
            // cos for some reason you can't cast that into an int.
            object obj = row[colName];
            if (obj is byte)
                return (int)(byte)obj; // <stares in abject horror>

            return (int)obj;
        }

        /// <summary>
        /// Helper method to get an bool from a given data row.
        /// </summary>
        /// <param name="row">The row from which to draw the data</param>
        /// <param name="colName">The column whose data is required</param>
        /// <returns>The value found in the given row at the given column,
        /// or false if no value was found.</returns>
        public static bool GetBool(DataRow row, string colName)
        {
            return !row.IsNull(colName) && ((bool)row[colName]);
        }


        /// <summary>
        /// Helper method to get a long value from a given data row, or zero if
        /// the given cell in the row is null.
        /// </summary>
        /// <param name="row">The row from which to draw the long value.</param>
        /// <param name="colName">The column name indicating which cell within
        /// the row contains the desired long value.</param>
        /// <returns>The long value found at the given cell, or zero if the cell
        /// value was null.</returns>
        public static long GetLong(DataRow row, string colName)
        {
            return GetLong(row, colName, 0L);
        }

        /// <summary>
        /// Helper method to get a long value from a given data row, or the
        /// specified default if the given cell in the row is null.
        /// </summary>
        /// <param name="row">The row from which to draw the long value.</param>
        /// <param name="colName">The column name indicating which cell within
        /// the row contains the desired long value.</param>
        /// <param name="defaultValue">The value to return if the value at the
        /// specified cell is null.</param>
        /// <returns>The long value found at the given cell, or the specified
        /// default value if the cell value was null.</returns>
        public static long GetLong(DataRow row, string colName, long defaultValue)
        {
            if (row.IsNull(colName))
                return defaultValue;

            object obj = row[colName];
            if (obj is byte)
                return (long)(byte)obj;

            return (long)obj;           
        }
    }
}
