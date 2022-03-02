using System.Collections.Generic;
using System.Text;
using BluePrism.ClientServerResources.Core.Enums;

namespace BluePrism.ClientServerResources.Core.Events
{
    public delegate void ResourcesChangedEventHandler(object sender, ResourcesChangedEventArgs e);

    public class ResourcesChangedEventArgs : BaseResourceEventArgs
    {
        // The individual status changes keyed on resource name
        private readonly IDictionary<string, ResourceStatusChange> _changes;

        
        /// <summary>
        /// Creates a new resource change event args object.
        /// </summary>
        /// <param name="overall">The overall changes in all resources</param>
        /// <param name="changes">The individual changes for each resource keyed on the
        /// resource name.</param>
        public ResourcesChangedEventArgs(ResourceStatusChange overall, IDictionary<string, ResourceStatusChange> changes)
        {
            OverallChange = overall;
            _changes = changes;
        }

        /// <summary>
        /// The overall change, combined from all changes in all changed resources.
        /// </summary>
        public ResourceStatusChange OverallChange { get; }

        /// <summary>
        /// The individual changes for each resource, keyed on the resource name.
        /// </summary>
        public IDictionary<string, ResourceStatusChange> Changes
        {
            get
            {
                if (_changes is null)
                {
                    return new Dictionary<string, ResourceStatusChange>();
                }
                return _changes;
            }
        }

        /// <summary>
        /// Gets a string representation of this event args object
        /// </summary>
        /// <returns>This event args object's properties in string form.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder(200);
            sb.Append("ResourcesChangedEventArgs:{").Append(OverallChange.ToString()).Append(" = [");
            var first = true;
            foreach (var pair in Changes)
            {
                if (!first)
                {
                    sb.Append("; ");
                }
                sb.Append(pair.Key).Append(" (").Append(pair.Value.ToString()).Append(")");
                first = false;
            }

            sb.Append("]}");
            return sb.ToString();
        } 
    }
}
