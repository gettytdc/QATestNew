Imports System.Globalization
Imports System.IO
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.BPCoreLib
Imports NLog
Imports NLog.Config

Namespace Logging

    Public Module AutomateNLogConfig
    
        Private Const ConfigurationFileName = "Automate.NLog.config"

        ''' <summary>
        ''' Configures logging based on configuration file. This should be called immediately on
        ''' application startup.
        ''' </summary>
        Public Sub Configure()
            Dim filePath = Path.Combine(clsFileSystem.CommonAppDataDirectory, ConfigurationFileName)
            If File.Exists(filePath) Then
                LogManager.Configuration = New XmlLoggingConfiguration(filePath, New LogFactory With {
                    .ThrowConfigExceptions = False
                })
            End If
        End Sub

        ''' <summary>
        ''' Initialises NLog prior to interpreting of startup parameters, providing a default
        ''' application name to use during startup within NLog's GlobalDiagnostics content. This 
        ''' is used in the log configuration to generate separate log files for each instance 
        ''' (referenced in NLog.config file as ${gdc:item=AppName}. An additional property is 
        ''' also added for the event log source. It should be called as soon as possible during 
        ''' application startup.
        ''' </summary>
        ''' <remarks>
        ''' The event log source is defined here to ensure that the source of entries written via NLog
        ''' remain consistent with the previous mechanism used to write to the event log.
        ''' </remarks>
        Public Sub SetStartUpAppProperties()
            GlobalDiagnosticsContext.Set("AppName", "AutomateStartUp")
            GlobalDiagnosticsContext.Set("AppEventLogSource", EventLogHelper.DefaultSource)
        End Sub

        ''' <summary>
        ''' Initialises the name of the application within NLog's GlobalDiagnostics content. This is used
        ''' in the log configuration to generate separate log files for each instance (referenced in
        ''' NLog.config file as ${gdc:item=AppName}. An additional property is also added for the
        ''' event log source. It should be called as soon as startup parameters have been interpreted.
        ''' </summary>
        ''' <remarks>
        ''' The event log source is defined here to ensure that the source of entries written via NLog
        ''' remain consistent with the previous mechanism used to write to the event log.
        ''' </remarks>
        Public Sub SetAppProperties(resourcePc As Boolean, loginAgent As Boolean, port As Integer)

            Dim name As String
            Dim eventLogSource As String
            If resourcePc Then
                name = "ResourcePC"
                If loginAgent Then
                    name = name & "-LoginAgent"
                End If
                If port <> ResourceMachine.DefaultPort Then
                    name = name & "-" & port.ToString(CultureInfo.InvariantCulture)
                End If
                eventLogSource = EventLogHelper.GetResourcePcSource(port)
            Else
                name = "Automate"
                eventLogSource = EventLogHelper.DefaultSource
            End If
            GlobalDiagnosticsContext.Set("AppName", name)
            GlobalDiagnosticsContext.Set("AppEventLogSource", eventLogSource)
        End Sub

    End Module
End NameSpace
