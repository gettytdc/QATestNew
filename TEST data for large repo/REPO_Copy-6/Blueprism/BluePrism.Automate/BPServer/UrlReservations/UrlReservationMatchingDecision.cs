using System;

namespace BluePrism.BPServer.UrlReservations
{
    /// <summary>
    /// Class that contains information about a single Url Reservation matching decision.
    /// Instances of this class are used to populate the <see cref="UrlReservationMatchingDecisionTable"/>.
    /// </summary>
    public class UrlReservationMatchingDecision
    {
        /// <summary>
        /// The result that is returned if all of the conditions are met
        /// </summary>
        public UrlReservationMatchType Result { get; private set; }

        /// <summary>
        /// All of the matching conditions that need to Return True if the decision is to be returned
        /// </summary>
        public Func<BindingProperties, BindingProperties, bool>[] Conditions { get; private set; }

        /// <summary>
        /// Create a new url reservation matching decision
        /// </summary>
        public UrlReservationMatchingDecision(UrlReservationMatchType result, Func<BindingProperties, BindingProperties, bool>[] conditions)
        {
            Result = result;
            Conditions = conditions;
        }

        /// <summary>
        /// Returns True if all of the matching conditions are met, when applied to
        /// to url reservatons.
        /// </summary>
        /// <param name="url1">The first url reservation to compare</param>
        /// <param name="url2">The second url reservation to compare</param>
        /// <returns>True if all matching conditions are met</returns>
        public bool MatchesAll(BindingProperties url1, BindingProperties url2)
        {
            var allConditionsMet = true;

            for (int i = 0; i < Conditions.Length; i++)
            {
                allConditionsMet &= Conditions[i](url1, url2);
                // Return false as soon as one of the conditions fails
                if (!allConditionsMet) return false;
            }

            return allConditionsMet;
        }
    }
    
}
