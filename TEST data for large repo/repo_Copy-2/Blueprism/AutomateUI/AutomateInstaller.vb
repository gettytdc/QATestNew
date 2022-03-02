
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Resources

''' <summary>
''' Installer class responsible for handling post-installation tasks which the
''' Automate project rely upon.
''' Currently, this installer just ensures that an event log source is created as a
''' fallback for resource PCs to log to if they cannot create a source specifically
''' for their own port number.
''' </summary>
Public Class AutomateInstaller

    ''' <summary>
    ''' Creates a new version of the automate installer
    ''' </summary>
    Public Sub New()
        InitializeComponent()
    End Sub

    ''' <summary>
    ''' Callback which handles the post-installation tasks for automate
    ''' </summary>
    ''' <param name="stateSaver">A dictionary into which the state can be saved.
    ''' </param>
    Public Overrides Sub Install(ByVal stateSaver As IDictionary)
        MyBase.Install(stateSaver)

        ' Create the general PC event log source and the source for the resource
        ' PC on the default port, if they don't exist. This ensures that the 
        ' sources defined in the default NLog logging configuration are ready for
        ' use (a good time to do it as installer running with elevated permissions).
        Try
            EventLogHelper.CreateDefaultLog()
        Catch
        End Try
        Try
            Dim source = EventLogHelper.GetResourcePcSource(ResourceMachine.DefaultPort)
            EventLogHelper.CreateSource(source, EventLogHelper.DefaultLogName)
        Catch
        End Try
        Try
            EventLogHelper.CreateSource(clsArchiver.EventLogSource, EventLogHelper.DefaultLogName)
        Catch
        End Try

    End Sub

End Class
