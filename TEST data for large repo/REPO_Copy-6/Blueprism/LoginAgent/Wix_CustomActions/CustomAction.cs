using System;
using System.IO;
using System.Xml;
using BluePrism.LoginAgent.Sas;
using Microsoft.Deployment.WindowsInstaller;

namespace Wix_CustomActions
{
    public class CustomActions
    {

        [CustomAction]
        public static ActionResult SASService_WriteConfig(Session session)
        {
            session.Log("Begin writing BluePrismSASService Config File");

            try
            {
                string sessionSASproxy = session.CustomActionData["EnableSAS"];
                string sessionOverrideSAS = session.CustomActionData["OverrideSAS"];
                string sessionOverrideLegalMessage = session.CustomActionData["OverrideLegalMsg"];

                bool enableSASproxy = String.IsNullOrEmpty(sessionSASproxy) ?
                    false : bool.Parse(sessionSASproxy);
                bool overrideSASGPO = String.IsNullOrEmpty(sessionOverrideSAS) ?
                    false : bool.Parse(sessionOverrideSAS);
                bool overrideLegalMessageGPO = String.IsNullOrEmpty(sessionOverrideLegalMessage) ?
                    false : bool.Parse(sessionOverrideLegalMessage);

                string configFilePath = session.CustomActionData["ConfigFileLocation"];

                ServiceConfiguration config = new ServiceConfiguration
                    (enableSASproxy, overrideSASGPO, overrideLegalMessageGPO);

                config.Save(configFilePath);

                session.Log("Completed writing BluePrismSASService Config File");
                return ActionResult.Success;

            }
            catch (Exception e)
            {
                session.Log("Error writing Blue Prism SAS Service Config File: " + e.Message);
                return ActionResult.Failure;
            }
        }

        [CustomAction]
        public static ActionResult RemoveSASConfigFile(Session session)
        {
            try
            {
                session.Log("attempting to delete SAS config file");
                string configFilePath = session.CustomActionData["ConfigFileLocation"];

                if (File.Exists(configFilePath))
                {
                    session.Log("SAS Config File found");
                    File.Delete(configFilePath);
                    session.Log("SAS Config File deleted");
                }
                else
                {
                    session.Log($"Could not find SAS Config File at: {configFilePath}");
                }
            }
            catch (Exception e)
            {
                session.Log("Error in RemoveSASConfigFile: " + e.Message);
            }

            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult TryReadSASConfigFile(Session session)
        {
            try
            {
                session.Log("attempting to read SAS config file");
                string configFilePath = session["ConfigFileLocation"];
                if (File.Exists(configFilePath))
                {
                    session.Log("SAS Config File found");
                    ServiceConfiguration config = ServiceConfiguration.Load(configFilePath);
                    session.Log("SAS Config File loaded");

                    session["CFGENABLESASPROXY"] = config.SendSecureAttentionSequence ? "true" : null;
                    session["CFGATTEMPTOVERRIDESASGPO"] = config.OverrideSasGroupPolicy ? "true" : null;
                    session["CFGATTEMPTOVERRIDELEGALMSGGPO"] = config.OverrideLegalMessageGroupPolicy ? "true" : null;
                }
                else
                {
                    session.Log($"Could not find SAS Config File at: {configFilePath}");
                }
            }
            catch (Exception e)
            {
                session.Log("Error reading SAS Config File: " + e.Message);
                return ActionResult.Failure;
            }
            return ActionResult.Success;
        }

        [CustomAction]
        public static ActionResult TryReadLoginAgentConfigFile(Session session)
        {
            try
            {
                session.Log("Attempting to read Login Agent config file");
                string configFileDirectory = session["CONFIGDIR"];
                var configFilePath = Path.Combine(configFileDirectory, "LoginAgentService.config");

                if (File.Exists(configFilePath))
                {
                    session.Log("Login Agent Config File found");
                    var doc = new XmlDocument();
                    doc.Load(configFilePath);
                    var dbConNameElement = doc.DocumentElement.SelectSingleNode(
                        "/configuration/startuparguments/argument[@name='dbconname']/value");

                    session["CONNECTIONNAME"] = dbConNameElement.InnerText;
                    session.Log("Login Agent Config File loaded");
                }
                else
                {
                    session.Log($"Could not find Login Agent Config File at: {configFilePath}");
                }
            }
            catch (Exception e)
            {
                session.Log("Error reading Login Agent Config File: " + e.Message); 
                return ActionResult.NotExecuted; // do not return the error as this simply tries to pre-populate a user editable field.
            }
            return ActionResult.Success;
        }
    }
}