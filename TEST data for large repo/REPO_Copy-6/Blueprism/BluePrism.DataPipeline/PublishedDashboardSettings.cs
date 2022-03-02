using System;
using System.Runtime.Serialization;

namespace BluePrism.DataPipeline
{
    [DataContract(Namespace = "bp"), Serializable]
    public class PublishedDashboardSettings
    {
        [DataMember]
        private Guid _dashboardId;

        [DataMember]
        private string _dashboardName;

        [DataMember]
        private int _publishToDatagatewayInterval;

        [DataMember]
        private DateTime _lastSent;

        public PublishedDashboardSettings(Guid dashboardId, string dashboardName, int interval, DateTime lastSent)
        {
            _dashboardId = dashboardId;
            _dashboardName = dashboardName;
            _publishToDatagatewayInterval = interval;
            _lastSent = lastSent;
        }

        public Guid DashboardId => _dashboardId;

        public string DashboardName => _dashboardName;

        public int PublishToDataGatewayInterval { get => _publishToDatagatewayInterval; set => _publishToDatagatewayInterval = value; }

        public DateTime LastSent => _lastSent;
    }
}
