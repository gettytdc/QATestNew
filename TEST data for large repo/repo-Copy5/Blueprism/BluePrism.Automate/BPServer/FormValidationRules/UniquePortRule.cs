using BluePrism.BPServer.Properties;

namespace BluePrism.BPServer.FormValidationRules
{
    public class UniquePortRule : IFormValidationRule
    {
        private readonly int _port;
        private readonly int _comparisonPort;

        public UniquePortRule(int port, int comparisonPort)
        {
            _port = port;
            _comparisonPort = comparisonPort;
        }

        public FormValidationResult IsValid()
        {
            var success = false;
            var message = string.Empty;

            if (_port == _comparisonPort)
            {
                message = Resources.ConflictServerBindingPort;
            }
            else
            {
                success = true;
            }

            return new FormValidationResult(success, message);
        }
    }
}
