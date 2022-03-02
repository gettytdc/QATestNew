using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BluePrism.BPServer.UrlReservations
{
    /// <summary>
    /// Stores a number of decisions that can then be used to determine the match type of two
    /// url reservations.
    /// </summary>
    public class UrlReservationMatchingDecisionTable : IEnumerable<UrlReservationMatchingDecision>  
    {
        /// <summary>
        /// The underlying list of decisions
        /// </summary>
        private List<UrlReservationMatchingDecision> _rows = new List<UrlReservationMatchingDecision>();
        
        /// <summary>
        /// Add a new decision to the decision table
        /// </summary>
        /// <param name="result">The match type to return if all of the decision's conditions
        /// return true</param>
        /// <param name="conditions">The functions that are used to check the match type of two url reservations </param>
        public void Add(UrlReservationMatchType result, params Func<BindingProperties, BindingProperties, bool>[] conditions)
        {
            _rows.Add(new UrlReservationMatchingDecision(result, conditions));
        }

        /// <summary>
        /// Returns the match type of two url reservations, using the first decision
        /// in the decision table where all of the conditions are met.
        /// </summary>
        /// <param name="url1">The first url reservation to compare</param>
        /// <param name="url2">The second url reservation to compare</param>
        /// <returns>The match type of two url reservations</returns>
        public UrlReservationMatchType MakeDecision(BindingProperties url1, BindingProperties url2)
        {
            var decision = _rows.FirstOrDefault(r => r.MatchesAll(url1, url2));
            
            // If no decisions meet all the conditions then return the default match type
            return (decision == null ? UrlReservationMatchType.None : decision.Result);
        }

        #region IEnumerable

        /// <summary>
        /// Returns an enumerator that iterates through the decisions
        /// </summary>
        /// <returns>An enumerator that iterates through the decisions</returns>
        public IEnumerator<UrlReservationMatchingDecision> GetEnumerator()
        {
            return _rows.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion

    }
}
