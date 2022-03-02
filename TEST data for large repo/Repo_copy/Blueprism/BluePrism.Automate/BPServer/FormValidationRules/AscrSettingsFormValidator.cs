using System.Collections.Generic;
using System.Linq;

namespace BluePrism.BPServer.FormValidationRules
{
    public static class AscrSettingsFormValidator
    {
        public static IEnumerable<string> Validate(string hostName, decimal callbackPortNum, int serverPortNum, bool isGRPC)
        {
            var convertedCallbackPort = decimal.ToInt32(callbackPortNum);
            var rules = new List<IFormValidationRule>()
            {
                new UniquePortRule(convertedCallbackPort, serverPortNum),
                new PortRangeRule(convertedCallbackPort),
            };

            if (isGRPC)
            {
                rules.Add(new HostNamePopulatedRule(hostName));
            }

            var results = rules.Select(rule => rule.IsValid())
                .Where(x => !x.Result)
                .ToList();

            foreach (var errorMessage in results.Select(x => x.ErrorMessage))
            {
                yield return errorMessage;
            }
        }
    }
}
