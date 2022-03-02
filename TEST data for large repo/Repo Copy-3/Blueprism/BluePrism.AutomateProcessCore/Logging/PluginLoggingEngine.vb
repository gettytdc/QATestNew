Imports BluePrism.Core.Plugins

''' <summary>
''' A logging engine that uses a plugin to log events.
''' </summary>
Public Class PluginLoggingEngine : Inherits FunnelLoggingEngine

    Sub New(ctx As ILogContext)
        MyBase.New(ctx)
    End Sub

    Protected Overrides Sub Log(info As LogInfo, stg As clsProcessStage, eventName As String)

        If info.Inhibit Then Return

        Dim data As New Dictionary(Of String, Object)
        data("when") = DateTime.UtcNow()
        data("eventId") = eventName
        data("sessionid") = Context.SessionId
        data("sessionNumber") = Context.SessionNo
        data("resourceName") = Context.ResourceName
        data("mainprocessid") = Context.ProcessId
        data("mainprocessname") = Context.ProcessName
        data("currprocessid") = stg.Process.Id
        data("currprocessname") = stg.Process.Name
        data("currprocesstype") = stg.Process.ProcessType
        data("pageid") = stg.SubSheet.ID
        data("pagename") = stg.SubSheet.Name
        data("stageid") = stg.Id
        data("stagename") = stg.Name

        EventManager.GetInstance().SendEvent(data)

    End Sub

End Class
