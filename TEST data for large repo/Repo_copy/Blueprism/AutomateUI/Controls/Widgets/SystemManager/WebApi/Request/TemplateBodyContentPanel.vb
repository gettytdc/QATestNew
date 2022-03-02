Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent

Namespace Controls.Widgets.SystemManager.WebApi.Request

    Public Class TemplateBodyContentPanel : Implements IBodyContentPanel

        Private mTemplateContent As TemplateBodyContent

        ''' <inheritdoc/>
        Public Event ConfigurationChanged As BodyTypeChangedEventHandler Implements IBodyContentPanel.ConfigurationChanged

        Public Sub New(bodyContent As TemplateBodyContent)
            InitializeComponent()

            mTemplateContent = If(bodyContent IsNot Nothing, bodyContent, New TemplateBodyContent())

            txtTemplate.Text = mTemplateContent.Template

        End Sub

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public ReadOnly Property Configuration As IBodyContent Implements IBodyContentPanel.Configuration
            Get
                Return mTemplateContent
            End Get
        End Property

        Private Sub FileCollectionParamNameChanged(sender As Object, e As EventArgs) Handles txtTemplate.Validating
            mTemplateContent = New TemplateBodyContent(txtTemplate.Text)
            OnConfigurationChanged(New BodyContentChangedEventArgs(mTemplateContent))
        End Sub

        Public Sub OnConfigurationChanged(e As BodyContentChangedEventArgs) Implements IBodyContentPanel.OnConfigurationChanged
            RaiseEvent ConfigurationChanged(Me, e)
        End Sub

    End Class

End Namespace