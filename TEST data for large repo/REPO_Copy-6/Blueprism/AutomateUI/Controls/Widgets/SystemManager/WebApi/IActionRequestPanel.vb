Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent

Namespace Controls.Widgets.SystemManager.WebApi
    Friend Interface IActionRequestPanel
        Sub ShowBodyTypePanel(bodyContent As IBodyContent)
        Function GetNewBodyContent(bodyType As WebApiRequestBodyType) As IBodyContent
    End Interface
End Namespace
