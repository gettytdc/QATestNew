Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent

Namespace Controls.Widgets.SystemManager.WebApi.Request

    Public Class MultipleFileBodyContentPanel : Implements IBodyContentPanel

        Private mFileCollection As FileCollectionBodyContent

        ''' <inheritdoc/>
        Public Event ConfigurationChanged As BodyTypeChangedEventHandler _
            Implements IBodyContentPanel.ConfigurationChanged

        Public Sub New(bodyContent As FileCollectionBodyContent)

            InitializeComponent()

            mFileCollection = If(bodyContent IsNot Nothing, bodyContent, New FileCollectionBodyContent())

            txtFileCollectionParamName.Text = mFileCollection.FileInputParameterName
        End Sub

        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public ReadOnly Property Configuration As IBodyContent _
            Implements IBodyContentPanel.Configuration
            Get
                Return mFileCollection
            End Get
        End Property

        Private Sub FileCollectionParamNameChanged(sender As Object, e As EventArgs) Handles txtFileCollectionParamName.Validating
            mFileCollection = New FileCollectionBodyContent(txtFileCollectionParamName.Text)
            OnConfigurationChanged(New BodyContentChangedEventArgs(mFileCollection))
        End Sub

        Public Sub OnConfigurationChanged(e As BodyContentChangedEventArgs) Implements IBodyContentPanel.OnConfigurationChanged
            RaiseEvent ConfigurationChanged(Me, e)
        End Sub

    End Class

End Namespace