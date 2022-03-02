using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;


namespace BluePrism.Datapipeline.Logstash
{
    public class LogstashMonitoringApi : ILogstashMonitoringApi
    {

        private readonly int _monitoringApiPort;
        public LogstashMonitoringApi(int monitoringApiPort = 9600)
        {
            _monitoringApiPort = monitoringApiPort;
        }


        public LogstashMonitoringApiResult QueryApi()
        {
            try
            {
                HttpResponseMessage response;
                using (var client = new HttpClient())
                {
                    response = client.GetAsync($"http://localhost:{_monitoringApiPort}/_node/stats/pipelines?pretty").Result;
                }

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException(Properties.Resources.UnableToQueryMonitorApi);
                }

                dynamic monitoringResults = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                var processedEventStats = monitoringResults.pipelines.main.events;

                return new LogstashMonitoringApiResult(
                   (int)processedEventStats.@in,
                   (int)processedEventStats.@out);

            }
            catch 
            {
                throw new InvalidOperationException(Properties.Resources.UnableToQueryMonitorApi);
            }
        }
    }
}
