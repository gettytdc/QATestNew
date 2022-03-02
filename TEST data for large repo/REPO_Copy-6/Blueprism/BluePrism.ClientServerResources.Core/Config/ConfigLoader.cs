using System;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using BluePrism.BPCoreLib;
using BluePrism.ClientServerResources.Core.Enums;
using BluePrism.ClientServerResources.Core.Properties;

namespace BluePrism.ClientServerResources.Core.Config
{
    public static class ConfigLoader
    {


        public static XmlNode SaveXML(ConnectionConfig cfg, XmlDocument doc)
        {
            if (cfg is null)
            {
                throw new ArgumentNullException(nameof(cfg));
            }

            if (doc is null)
            {
                throw new ArgumentNullException(nameof(doc));
            }

            try
            {
                var node = doc.CreateElement(ConnectionConfig.ElementName);

                BPUtil.AppendTextElement(node, nameof(ConnectionConfig.CallbackProtocol), cfg.CallbackProtocol);
                BPUtil.AppendTextElement(node, nameof(ConnectionConfig.HostName), cfg.HostName);
                BPUtil.AppendTextElement(node, nameof(ConnectionConfig.Port), cfg.Port);
                BPUtil.AppendTextElement(node, nameof(ConnectionConfig.Mode), cfg.Mode);
                BPUtil.AppendTextElement(node, nameof(ConnectionConfig.CertificateName), cfg.CertificateName);
                BPUtil.AppendTextElement(node, nameof(ConnectionConfig.ClientCertificateName), cfg.ClientCertificateName);
                BPUtil.AppendTextElement(node, nameof(ConnectionConfig.ServerStore), cfg.ServerStore);
                BPUtil.AppendTextElement(node, nameof(ConnectionConfig.ClientStore), cfg.ClientStore);

                return node;
            }
            catch(Exception ex)
            {
                throw new InvalidProgramException(Resources.FailedToSaveConfigXML, ex);
            }
        }

        public static ConnectionConfig LoadXML(XmlNode parent)
        {
            if (parent is null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            try
            {
                return new ConnectionConfig
                {
                    CallbackProtocol = parent[nameof(ConnectionConfig.CallbackProtocol)] != null ? (CallbackConnectionProtocol)Enum.Parse(typeof(CallbackConnectionProtocol), parent[nameof(ConnectionConfig.CallbackProtocol)].InnerText) : CallbackConnectionProtocol.None,
                    HostName = parent[nameof(ConnectionConfig.HostName)]?.InnerText ?? string.Empty,
                    Port = parent[nameof(ConnectionConfig.Port)] != null ? int.Parse(parent[nameof(ConnectionConfig.Port)].InnerText) : 10000,
                    Mode = parent[nameof(ConnectionConfig.Mode)] != null ? (InstructionalConnectionModes)Enum.Parse(typeof(InstructionalConnectionModes), parent[nameof(ConnectionConfig.Mode)].InnerText) : InstructionalConnectionModes.None,
                    CertificateName = parent[nameof(ConnectionConfig.CertificateName)]?.InnerText ?? string.Empty,
                    ClientCertificateName = parent[nameof(ConnectionConfig.ClientCertificateName)]?.InnerText ?? string.Empty,
                    ServerStore = parent[nameof(ConnectionConfig.ServerStore)] != null ? (StoreName)Enum.Parse(typeof(StoreName), parent[nameof(ConnectionConfig.ServerStore)].InnerText) : StoreName.TrustedPublisher,
                    ClientStore = parent[nameof(ConnectionConfig.ClientStore)] != null ? (StoreName)Enum.Parse(typeof(StoreName), parent[nameof(ConnectionConfig.ClientStore)].InnerText) : StoreName.TrustedPeople
                };
            }
            catch (Exception ex)
            {
                throw new InvalidProgramException(Resources.FailedToLoadConfigXML, ex);
            }
        }
    }


}
