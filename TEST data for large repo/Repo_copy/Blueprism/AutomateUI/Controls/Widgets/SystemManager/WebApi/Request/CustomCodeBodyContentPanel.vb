Imports BluePrism.AutomateProcessCore.Compilation
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.CustomCode
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent

Namespace Controls.Widgets.SystemManager.WebApi.Request

    Public Class CustomCodeBodyContentPanel
        Implements IBodyContentPanel

        Public Event CodeChanged As CustomCodeChangedEventHandler

        Private WithEvents mCodeEditor As CustomCodeControl
        Private mCodeContent As CustomCodeBodyContent
        Private mAction As WebApiActionDetails

        Public Event ConfigurationChanged As BodyTypeChangedEventHandler Implements IBodyContentPanel.ConfigurationChanged

        Friend Sub New(content As CustomCodeBodyContent, action As WebApiActionDetails)

            InitializeComponent()

            mCodeContent = If(content, New CustomCodeBodyContent(String.Empty))
            mAction = action

            mCodeEditor = New CustomCodeControl(mAction.Api.CommonCode, GetMethodDefinition(action)) With {
                .Dock = DockStyle.Fill
            }

            Controls.Add(mCodeEditor)
        End Sub

        Private Function GetMethodDefinition(action As WebApiActionDetails) As MethodDefinition

            Dim configBuilder = New WebApiConfigurationBuilder()
            Dim config = configBuilder.
                            WithParameters(action.Api.CommonParameters).
                            WithAction(action.Name, action.Request.Method,
                                   action.Request.UrlPath, parameters:=(action.Parameters),
                                   bodyContent:=New CustomCodeBodyContent(mCodeContent.Code)).
                            WithCommonCode(action.Api.CommonCode).
                            Build()
            Dim methodDefinition = CustomCodeMethodType.RequestContent.CreateMethods(config).FirstOrDefault()
            Return methodDefinition
        End Function

        Public ReadOnly Property Configuration As IBodyContent Implements IBodyContentPanel.Configuration
            Get
                Return mCodeContent
            End Get
        End Property

        Public Sub OnConfigurationChanged(e As BodyContentChangedEventArgs) Implements IBodyContentPanel.OnConfigurationChanged
            RaiseEvent ConfigurationChanged(Me, e)
        End Sub

        Private Sub HandleCodeChanged(sender As Object, e As CustomCodeControlEventArgs) Handles mCodeEditor.CodeChanged
            mCodeContent = New CustomCodeBodyContent(e.CodeContent)

            RaiseEvent ConfigurationChanged(Me, New BodyContentChangedEventArgs(mCodeContent))
        End Sub
    End Class
End Namespace
