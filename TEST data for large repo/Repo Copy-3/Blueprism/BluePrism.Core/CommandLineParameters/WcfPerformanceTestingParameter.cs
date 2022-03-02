using System;
using BluePrism.Core.Properties;

namespace BluePrism.Core.CommandLineParameters
{
    public class WcfPerformanceTestingParameter
    {
        public int PerformanceTestDurationMinutes { get; }

        public WcfPerformanceTestingParameter(string rawParameterValue)
        {
            if (!ValidateWcfPerformanceParameter(rawParameterValue))
            {
                throw new ArgumentException(Resource.WCFPerformance_Invalid);
            }

            PerformanceTestDurationMinutes = int.Parse(rawParameterValue);
        }

        private static bool ValidateWcfPerformanceParameter(string wcfPerformanceMinutes)
        {
            const int maxMinutes = 1140;
            const int minMinutes = 0;

            if (!int.TryParse(wcfPerformanceMinutes, out var convertedWcfPerformanceMinutes))
                return false;

            return convertedWcfPerformanceMinutes > minMinutes 
                   && convertedWcfPerformanceMinutes <= maxMinutes;
        }
    }
}