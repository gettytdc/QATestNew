using BluePrism.BPServer.Properties;

namespace BluePrism.BPServer.FormValidationRules
{
    public class PortRangeRule : IFormValidationRule
    {
        private readonly int _port;

        public PortRangeRule(int port) => _port = port;

        public FormValidationResult IsValid()
        {
            const int portLowerLimit = 1024;
            const int portUpperLimit = 65353;
            var success = false;
            var message = string.Empty;

            if (_port >= portLowerLimit && _port <= portUpperLimit)
            {
                success = true;
            }
            else
            {
                message = Resources.InvalidPort;
            }

            return new FormValidationResult(success, message);
        }
    }
}
