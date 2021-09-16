using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Diagnostics;
using System.Xml.Linq;

namespace BluePrism.LoginAgent.Sas
{
    /// <summary>
    /// Class to encapsulate the configuration of the SAS Service
    /// </summary>
    public class ServiceConfiguration
    {
                    
        public static readonly string DefaultConfigFile = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            @"Blue Prism Limited\Automate V3\SASService.config");

        
        public static readonly string DefaultWorkingDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            @"Blue Prism Limited\Blue Prism Automate\");

                    
        public readonly bool SendSecureAttentionSequence;

        public readonly bool OverrideSasGroupPolicy;

        public readonly bool OverrideLegalMessageGroupPolicy;


        public ServiceConfiguration(bool? sendSas, bool? overrideSasGroupPolicy, bool? overrideLegalMessageGroupPolicy)
        {                        
            SendSecureAttentionSequence = sendSas.GetValueOrDefault();
            OverrideSasGroupPolicy = overrideSasGroupPolicy.GetValueOrDefault();
            OverrideLegalMessageGroupPolicy = overrideLegalMessageGroupPolicy.GetValueOrDefault();
        }

        public static ServiceConfiguration Load(string configFilePath)
        {
            try
            {
                var configuration = XDocument.Load(configFilePath ?? DefaultConfigFile);
                return FromXElement(configuration);
            }
            catch
            {
                return new ServiceConfiguration(null, null, null);
            }
        }

        public static ServiceConfiguration LoadConfigFromDefaultLocation()
        {
            try
            {
                var configuration = XDocument.Load(DefaultConfigFile);
                return FromXElement(configuration);
            }
            catch
            {
                return new ServiceConfiguration(null, null, null);
            }
        }

        public void Save(string configFilePath)
        {
           using (XmlTextWriter xmlWriter = new XmlTextWriter(configFilePath, Encoding.UTF8))
            {
                xmlWriter.Formatting = Formatting.Indented;
                xmlWriter.WriteStartDocument();
                ToXElement().WriteTo(xmlWriter);
                xmlWriter.WriteEndDocument();
            }

        }


        private XElement ToXElement() => new XElement("configuration",
                                            new XElement("enablesasproxy", SendSecureAttentionSequence),
                                            new XElement("attemptoverridesasgrouppolicy", OverrideSasGroupPolicy),
                                            new XElement("attemptoverridelegalmessagegrouppolicy", OverrideLegalMessageGroupPolicy));


        private static ServiceConfiguration FromXElement(XDocument configuration)
        {
            var sendSas = GetValueOrDefaultFromConfigElement(configuration, "enablesasproxy", false);
            var overrideSasGroupPolicy = GetValueOrDefaultFromConfigElement(configuration, "attemptoverridesasgrouppolicy", false);
            var overrideLegalMessageGroupPolicy = GetValueOrDefaultFromConfigElement(configuration, "attemptoverridelegalmessagegrouppolicy", false);

            return new ServiceConfiguration(sendSas, overrideSasGroupPolicy, overrideLegalMessageGroupPolicy);

        }

        private static bool GetValueOrDefaultFromConfigElement(XDocument configuration, string elementName, bool defaultValue) =>
            bool.TryParse(configuration
                            .Root
                            ?.Element(elementName)
                            ?.Value, out bool parsedValue) ? parsedValue : defaultValue;
      
    
                
    }
}
