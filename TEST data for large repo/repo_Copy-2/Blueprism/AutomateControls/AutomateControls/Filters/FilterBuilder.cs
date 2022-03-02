using System.Collections.Generic;

namespace AutomateControls.Filters
{
    public class FilterBuilder
    {
        private IList<KeyValuePair<string, string>> _terms;

        public FilterBuilder()
        {
            _terms = new List<KeyValuePair<string, string>>();
        }

        public void AddTerm(string name, string constraint)
        {
            _terms.Add(new KeyValuePair<string, string>(name, constraint));
        }

    }
}
