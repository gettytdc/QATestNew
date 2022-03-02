Imports System.Net.Http
Imports AutomateControls
Imports AutomateUI.Controls.Widgets.SystemManager.WebApi.Request
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports BluePrism.Core.Utility

Namespace Controls.Widgets.SystemManager.WebApi

    ''' <summary>
    ''' Panel which can be used to view and edit Web Api action requests.
    ''' </summary>
    Friend Class WebApiActionRequestPanel : Implements IActionRequestPanel, IGuidanceProvider

        Public Event BodyTypeChanged As BodyTypeChangedEventHandler

        Private mRequest As WebApiActionRequestDetails

        ' The body type panel used to edit the content details
        Private WithEvents mBodyContentPanel As IBodyContentPanel

        Public Sub New()

            InitializeComponent()
            cmbMethod.Items.AddRange(HttpMethods.Standard())
            cmbBodyType.BindToLocalisedEnumItems(Of WebApiRequestBodyType)(
                WebApiResources.ResourceManager, "RequestBodyType_{0}_Title")

        End Sub

        ''' <summary>
        ''' The configuration panel used to configure the body content details
        ''' </summary>
        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Private Property BodyContentPanel As IBodyContentPanel
            Get
                Return mBodyContentPanel
            End Get
            Set(value As IBodyContentPanel)
                mBodyContentPanel = value
                Dim ctl = TryCast(mBodyContentPanel, Control)

                With pnlBodyTypeConfiguration.Controls
                    .Clear()
                    If ctl IsNot Nothing Then
                        ctl.Dock = DockStyle.Fill
                        .Add(ctl)
                    End If
                End With
            End Set
        End Property

        ' The action which is being managed by this panel
        Public Property Request As WebApiActionRequestDetails
            Get
                Return mRequest
            End Get
            Set(value As WebApiActionRequestDetails)
                If (value IsNot Nothing) Then
                    mRequest = value
                    If cmbMethod.Items.Contains(value.Method) Then
                        cmbMethod.SelectedItem = value.Method
                    Else
                        cmbMethod.Text = value.Method.Method
                    End If

                    txtUrlPath.Text = value.UrlPath
                    cmbBodyType.SelectedValue = value.BodyContent.Type
                    ShowBodyTypePanel(value.BodyContent)
                End If
            End Set
        End Property

        Private Sub cmbMethod_Validating(sender As Object, e As CancelEventArgs) _
            Handles cmbMethod.Validating

            cmbMethod.Text = cmbMethod.Text.ToUpper
            Try
                Dim method = New HttpMethod(cmbMethod.Text)
            Catch ex As FormatException
                e.Cancel = True
                UserMessage.Err(ex.Message)
            End Try
        End Sub

        ''' <summary>
        ''' Handles the request HTTP method being validated, ensuring that the underlying
        ''' action is updated with the new method.
        ''' </summary>
        Private Sub HandleMethod_Validated(sender As Object, e As EventArgs) _
            Handles cmbMethod.Validated

            If Request Is Nothing Then Return
            Request.Method = New HttpMethod(cmbMethod.Text)
        End Sub

        ''' <summary>
        ''' Gets the guidance text for this panel.
        ''' </summary>
        <Browsable(False), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
        Public ReadOnly Property GuidanceText As String _
         Implements IGuidanceProvider.GuidanceText
            Get
                Return WebApi_Resources.GuidanceActionRequestPanel
            End Get
        End Property

        ''' <summary>
        ''' Handles the URL path being validated, ensuring that the underlying
        ''' action is updated with the new URL path
        ''' </summary>
        Private Sub HandleUrlPathValidating(sender As Object, e As EventArgs) _
            Handles txtUrlPath.Validating

            Dim inputToValidate As String = txtUrlPath.Text

            If HasUrlPathChanged(inputToValidate) Then
                If inputToValidate.ContainsLeadingOrTrailingWhitespace() Then
                    UserMessage.Show(WebApi_Resources.UrlWhitespaceWarning)
                End If
            End If
        End Sub

        Private Function HasUrlPathChanged(urlPath As String) As Boolean
            Return Not mRequest.UrlPath.Equals(urlPath)
        End Function

        Private Sub HandleBaseUrlValidated(sender As Object, e As EventArgs) _
            Handles txtUrlPath.Validated

            If Request IsNot Nothing Then Request.UrlPath = txtUrlPath.Text
        End Sub

        ''' <summary>
        ''' Handles the Body Type combo box value changing, and and loads the required panel
        ''' </summary>
        Private Sub HandleBodyTypeChanged(sender As Object, e As EventArgs) Handles cmbBodyType.SelectedIndexChanged
            If mRequest Is Nothing Then Return

            Dim bodyType = cmbBodyType.GetSelectedValueOrDefault(Of WebApiRequestBodyType)
            If (Not mRequest.BodyContent.Type.Equals(bodyType)) Then
                mRequest.BodyContent = GetNewBodyContent(bodyType)
            End If

            ShowBodyTypePanel(mRequest.BodyContent)

            OnBodyTypeChanged(New BodyContentChangedEventArgs(mRequest.BodyContent))
        End Sub

        Private Sub HandleConfigurationChanged(sender As Object, e As BodyContentChangedEventArgs) Handles mBodyContentPanel.ConfigurationChanged
            mRequest.BodyContent = e.BodyContent
        End Sub

        Private Sub OnBodyTypeChanged(e As BodyContentChangedEventArgs)
            RaiseEvent BodyTypeChanged(Me, e)
        End Sub

        ''' <summary>
        ''' Create a new instance of the 
        ''' </summary>
        Public Sub ShowBodyTypePanel(bodyContent As IBodyContent) Implements IActionRequestPanel.ShowBodyTypePanel
            Select Case bodyContent.Type
                Case WebApiRequestBodyType.Template
                    BodyContentPanel = New TemplateBodyContentPanel(DirectCast(bodyContent, TemplateBodyContent))
                Case WebApiRequestBodyType.MultiFile
                    BodyContentPanel = New MultipleFileBodyContentPanel(DirectCast(bodyContent, FileCollectionBodyContent))
                Case WebApiRequestBodyType.SingleFile
                    BodyContentPanel = New SingleFileBodyContentPanel(DirectCast(bodyContent, SingleFileBodyContent))
                Case WebApiRequestBodyType.CustomCode
                    BodyContentPanel = New CustomCodeBodyContentPanel(DirectCast(bodyContent, CustomCodeBodyContent), mRequest.Action)
                Case Else
                    BodyContentPanel = Nothing
            End Select
        End Sub

        Private Function GetNewBodyContent(bodyType As WebApiRequestBodyType) As IBodyContent Implements IActionRequestPanel.GetNewBodyContent

            Select Case bodyType
                Case WebApiRequestBodyType.Template
                    Return New TemplateBodyContent()
                Case WebApiRequestBodyType.MultiFile
                    Return New FileCollectionBodyContent()
                Case WebApiRequestBodyType.SingleFile
                    Return New SingleFileBodyContent()
                Case WebApiRequestBodyType.CustomCode
                    Return New CustomCodeBodyContent(String.Empty)
                Case Else
                    Return New NoBodyContent()
            End Select
        End Function

    End Class

End Namespace