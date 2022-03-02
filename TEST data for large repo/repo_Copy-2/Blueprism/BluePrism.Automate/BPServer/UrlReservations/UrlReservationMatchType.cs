namespace BluePrism.BPServer.UrlReservations
{
    /// <summary>
    /// Represents result of comparing the URLs used to create URL reservations to see if
    /// they match or conflict
    /// </summary>
    public enum UrlReservationMatchType
    {
        /// <summary>
        /// The URLs do not match or conflict and can be used to create separate
        /// URL reservations.
        /// </summary>
        None,
        /// <summary>
        /// The URLs are an exact match 
        /// </summary>
        ExactMatch,
        /// <summary>
        /// The URLs conflict and separate URL reservations cannot be created 
        /// </summary>
        Conflict
    }
}