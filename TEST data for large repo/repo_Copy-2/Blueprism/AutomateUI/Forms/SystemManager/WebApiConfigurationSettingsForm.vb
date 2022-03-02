Imports AutomateUI.Controls.Widgets.SystemManager.WebApi
Imports BluePrism.AutomateProcessCore.WebApis.Authentication

Public Class WebApiConfigurationSettingsForm
    Property mConfigurationSettings As WebApiConfigurationSettingsDetails
    Property mRequiresAuthFields As Boolean

    Private Readonly mTimeoutMin As Integer = 1
    Private Readonly mTimeoutMax As Integer = 100000

    Public Sub Setup(settings As WebApiConfigurationSettingsDetails, authType As AuthenticationType)
        mConfigurationSettings = settings
        mRequiresAuthFields = authType = AuthenticationType.OAuth2ClientCredentials OrElse
                                    authType = AuthenticationType.OAuth2JwtBearerToken
        SetFields()
        AddToolTips()

        Dim txtRequestTimeout = DirectCast(intHttpRequestConnectionTimeout.Controls(1), TextBox)
        Dim txtAuthserverTimeout = DirectCast(intAuthServerRequestConnectionTimeout.Controls(1), TextBox)
        AddHandler txtRequestTimeout.Validating, AddressOf RequestTimeoutValidating
        AddHandler txtAuthserverTimeout.Validating, AddressOf AuthServerTimeoutValidating
    End Sub

    Private Sub AddToolTips()
        Dim toolTip = New ToolTip()
        toolTip.SetToolTip(intHttpRequestConnectionTimeout, WebApi_Resources.ToolTip_HttpRequestTimeoutConfigSetting)
        toolTip.SetToolTip(intAuthServerRequestConnectionTimeout, WebApi_Resources.ToolTip_AuthServerRequestTimeoutConfigSetting)
    End Sub

    Private Sub OnLoaded() Handles Me.VisibleChanged
        UpdateAuthSettingVisibility()
        UpdateLayout()
    End Sub

    Private Sub UpdateLayout()
        Dim labelX = 6
        Dim txtBoxX = 8
        Dim unitLabelX = 88
        Dim firstLabelY = 23
        Dim firstTxtBoxY = 40
        Dim firstUnitLabelY = 43

        lblHttpRequestTimeout.Location = New Point(labelX, firstLabelY)
        intHttpRequestConnectionTimeout.Location = New Point(txtBoxX, firstTxtBoxY)
        lblSecondsHttpTimeout.Location = New Point(unitLabelX, firstUnitLabelY)

        Dim gapBetweenFields = 45
        Dim numberOfFields = 1

        If lblAuthServerTimeout.Visible Then
            lblAuthServerTimeout.Location = New Point(labelX, firstLabelY + numberOfFields * gapBetweenFields)
            intAuthServerRequestConnectionTimeout.Location = New Point(txtBoxX, firstTxtBoxY + numberOfFields * gapBetweenFields)
            lblSecondsAuthTimeout.Location = New Point(unitLabelX, firstUnitLabelY + numberOfFields * gapBetweenFields)
            numberOfFields += 1
        End If

        btnRestoreDefaults.Location = New Point(txtBoxX, firstLabelY + 10 + numberOfFields * gapBetweenFields)
    End Sub

    Private Sub UpdateAuthSettingVisibility()

        lblAuthServerTimeout.Visible = mRequiresAuthFields
        intAuthServerRequestConnectionTimeout.Visible = mRequiresAuthFields
        lblSecondsAuthTimeout.Visible = mRequiresAuthFields
    End Sub

    Private Sub SetFields()
        intHttpRequestConnectionTimeout.Value = mConfigurationSettings.HttpRequestConnectionTimeout
        intAuthServerRequestConnectionTimeout.Value = mConfigurationSettings.AuthServerRequestConnectionTimeout
    End Sub

    Private Sub handlesRestoreDefaults() Handles btnRestoreDefaults.Click
        
        Dim response = UserMessage.OkCancel(WebApi_Resources.ConfirmationText_RestoreDefaultConfigSettings)

        If response = MsgBoxResult.Ok Then
            WebApiConfigurationSettingsDetails.ResetWithDefaults(mConfigurationSettings)
            SetFields()
        End If
    End Sub

    Private Sub HandleHttpTimeoutValidated(sender As Object, e As EventArgs) _
     Handles intHttpRequestConnectionTimeout.Validated
        If mConfigurationSettings IsNot Nothing Then _
            mConfigurationSettings.HttpRequestConnectionTimeout = Decimal.ToInt32(intHttpRequestConnectionTimeout.Value)
    End Sub

    Private Sub HandleAuthServerTimeoutValidated(sender As Object, e As EventArgs) _
     Handles intAuthServerRequestConnectionTimeout.Validated
        If mConfigurationSettings IsNot Nothing Then _
            mConfigurationSettings.AuthServerRequestConnectionTimeout = Decimal.ToInt32(intAuthServerRequestConnectionTimeout.Value)
    End Sub

    Private Sub RequestTimeoutValidating(sender As Object, e As CancelEventArgs)
        Dim input = intHttpRequestConnectionTimeout.Value
        If Not IsInteger(input) OrElse input < mTimeoutMin OrElse input > mTimeoutMax Then
            e.Cancel = True
            UserMessage.Err(WebApi_Resources.ErrorInvalidRequestTimoutValue)
        End If
    End Sub

    Private Sub AuthServerTimeoutValidating(sender As Object, e As CancelEventArgs)
        Dim input = intAuthServerRequestConnectionTimeout.Value
        If Not IsInteger(input) OrElse input < mTimeoutMin OrElse input > mTimeoutMax Then
            e.Cancel = True
            UserMessage.Err(WebApi_Resources.ErrorInvalidRequestTimoutValue)
        End If
    End Sub

    Private Function IsInteger(input As Decimal) As Boolean
       Return input = Int(input)
    End Function

End Class
