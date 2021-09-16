Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent

Namespace Controls.Widgets.SystemManager.WebApi.Request

    Public Delegate Sub BodyTypeChangedEventHandler(sender As Object, e As BodyContentChangedEventArgs)

    ''' <summary>
    ''' Describes an event where the body content of a Web Api configuration changes
    ''' </summary>
    Public Class BodyContentChangedEventArgs : Inherits EventArgs

        Public Sub New(newBodyContent As IBodyContent)
            BodyContent = newBodyContent
        End Sub

        ''' <summary>
        ''' The updated body content configuration
        ''' </summary>
        Public ReadOnly BodyContent As IBodyContent

    End Class
End Namespace