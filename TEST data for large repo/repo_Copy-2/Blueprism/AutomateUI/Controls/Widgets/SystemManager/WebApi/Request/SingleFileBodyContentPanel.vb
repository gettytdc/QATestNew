Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent

Namespace Controls.Widgets.SystemManager.WebApi.Request

    Public Class SingleFileBodyContentPanel : Implements IBodyContentPanel

        Private mSingleFileBodyContent As SingleFileBodyContent

        Public Event ConfigurationChanged As BodyTypeChangedEventHandler Implements IBodyContentPanel.ConfigurationChanged

        Public Sub New(bodyContent As SingleFileBodyContent)
            InitializeComponent()

            mSingleFileBodyContent = If(bodyContent IsNot Nothing, bodyContent, New SingleFileBodyContent())

            txtFileParameterName.Text = mSingleFileBodyContent.FileInputParameterName
        End Sub

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public ReadOnly Property Configuration As IBodyContent Implements IBodyContentPanel.Configuration
            Get
                Return mSingleFileBodyContent
            End Get
        End Property

        Private Sub FileCollectionParamNameChanged(sender As Object, e As EventArgs) Handles txtFileParameterName.Validating
            mSingleFileBodyContent = New SingleFileBodyContent(txtFileParameterName.Text)
            OnConfigurationChanged(New BodyContentChangedEventArgs(mSingleFileBodyContent))
        End Sub

        Public Sub OnConfigurationChanged(e As BodyContentChangedEventArgs) Implements IBodyContentPanel.OnConfigurationChanged
            RaiseEvent ConfigurationChanged(Me, e)
        End Sub

    End Class

End Namespace