namespace BluePrism.Data
{
    /// <summary>
    /// The database server which is currently being accessed by the DB connection.
    /// </summary>
    public enum DatabaseServer
    {
        /// <summary>
        /// Unspecified / unknown / unrecognised database server, or an error occurred
        /// while trying to get the version number.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// SQL Server 2000 - the integer value 8 is the major version number.
        /// </summary>
        SqlServer2000 = 8,

        /// <summary>
        /// SQL Server 2005 - the integer value 9 is the major version number
        /// </summary>
        SqlServer2005 = 9,

        /// <summary>
        /// SQL Server 2008 - the integer value 10 is the major version number
        /// </summary>
        SqlServer2008 = 10,

        /// <summary>
        /// SQL Server 2012 - the integer value 11 is the major version number
        /// </summary>
        SqlServer2012 = 11,

        /// <summary>
        /// SQL Server 2014 - the integer value 12 is the major version number
        /// </summary>
        SqlServer2014 = 12,

        /// <summary>
        /// SQL Server 2016 - the integer value 13 is the major version number
        /// </summary>
        SqlServer2016 = 13,

        /// <summary>
        /// SQL Server 2017 - the integer value 14 is the major version number
        /// </summary>
        SqlServer2017 = 14,
        /// <summary>
        /// SQL Server 2019 - the integer value 15 is the major version number
        /// </summary>
        SqlServer2019 = 15,
    }
}