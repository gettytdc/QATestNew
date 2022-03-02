Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent

Namespace Controls.Widgets.SystemManager.WebApi.Request

    ''' <summary>
    ''' Interface that describes a panel used to make changes to the configuration of
    ''' a Web API's Request Body Content settings.
    ''' </summary>
    Friend Interface IBodyContentPanel

        ''' <summary>
        ''' The underlying Body Type Configuration
        ''' </summary>
        ReadOnly Property Configuration As IBodyContent

        ''' <summary>
        ''' The event raised when a change has been made to the configuration on the 
        ''' panel
        ''' </summary>
        Event ConfigurationChanged As BodyTypeChangedEventHandler

        ''' <summary>
        ''' The method to call when a change has been the the configuration on this
        ''' panel, which should then raise the <see cref="ConfigurationChanged"/>
        ''' event
        ''' </summary>
        ''' <param name="e">The event args object</param>
        Sub OnConfigurationChanged(e As BodyContentChangedEventArgs)

    End Interface

End Namespace
