Imports System.IO
Imports BluePrism.AutomateAppCore
Imports NLog
Imports NLog.Config

Namespace Logging

    ''' <summary>
    ''' Configures NLog at application startup
    ''' </summary>
    Public Module AutomateCNLogConfig
    
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
                GlobalDiagnosticsContext.Set("AppName", "AutomateC")
            End If
        End Sub

    End Module
    
End NameSpace