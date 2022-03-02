using BluePrism.BPServer.Properties;

namespace BluePrism.BPServer.FormValidationRules
{
    public class HostNamePopulatedRule : IFormValidationRule
    {
        private readonly string _hostName;

        public HostNamePopulatedRule(string hostName) => _hostName = hostName;

        public FormValidationResult IsValid()
        {
            var success = !string.IsNullOrWhiteSpace(_hostName);
            var errorMessage = success ? string.Empty : Resources.InvalidHostname;

            return new FormValidationResult(success, errorMessage);
        }
    }
}
