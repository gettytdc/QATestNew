Imports System.ComponentModel
Imports BluePrism.AutomateAppCore
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Common.Security
Imports System.ServiceModel
Imports System.ServiceModel.Security
Imports System.Net
Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.Utility

Public Class ConnectionDetail

#Region " Class Scope Declarations "

    ''' <summary>
    ''' The height to use for a visible row in the table panel
    ''' </summary>
    Private Const AvailableRowHeight As Integer = 55

    ''' <summary>
    ''' Additional height for a visible row in the table panel.
    ''' Used when table rows contain a caption that exceeds one line.
    ''' </summary>
    Private Const AdditionalRowHeight As Integer = 5

    ''' <summary>
    ''' Event fired when the name of this connection detail is being validated - it
    ''' can be cancelled by an interested listener with more context for validating
    ''' connection names
    ''' </summary>
    Public Event NameValidating(ByVal sender As Object, ByVal e As NameValidatingEventArgs)

    ''' <summary>
    ''' Event fired when the connection held in this detail panel has had its name
    ''' changed.
    ''' </summary>
    Public Event NameValidated As EventHandler

    ''' <summary>
    ''' Event fired when the connection type is changed.
    ''' </summary>
    Public Event ConnectionTypeChanged As EventHandler

    ''' <summary>
    ''' Enumeration of the available rows in the connection detail table panel.
    ''' Each connection type displays a different set of rows.
    ''' </summary>
    Private Enum ConnectionDetailRow
        ConnectionName = 0
        ConnectionType = 1
        DBServer = 2
        DBName = 3
        UserID = 4
        Password = 5
        BPServer = 6
        BPServerConnectionMode = 7
        BPServerPort = 8
        CallbackPort = 9
        AvailabilityGroup = 10
        ExtraConnectionParams = 11
        CustomConnectionString = 12
        Ordered = 13
    End Enum

    ''' <summary>
    ''' Class encapsulating the connection type and its display properties
    ''' (ie. its label and the rows in the table panel which should be available
    ''' for each connection type)
    ''' </summary>
    <DebuggerDisplay("{mLabel}")>
    Private Class ConnectionTemplate

        ''' <summary>
        ''' SQL Server connection with SQL Server authentication
        ''' </summary>
        Public Shared ReadOnly SqlServerAuth As _
         New ConnectionTemplate(ConnectionType.Direct,
          False,
          ConnectionDetailRow.ConnectionName,
          ConnectionDetailRow.ConnectionType,
          ConnectionDetailRow.DBServer,
          ConnectionDetailRow.DBName,
          ConnectionDetailRow.UserID,
          ConnectionDetailRow.Password,
          ConnectionDetailRow.ExtraConnectionParams)

        ''' <summary>
        ''' SQL Server connection with Windows authentication
        ''' </summary>
        Public Shared ReadOnly SqlWindowsAuth As _
         New ConnectionTemplate(ConnectionType.Direct,
          True,
          ConnectionDetailRow.ConnectionName,
          ConnectionDetailRow.ConnectionType,
          ConnectionDetailRow.DBServer,
          ConnectionDetailRow.DBName,
          ConnectionDetailRow.ExtraConnectionParams)

        ''' <summary>
        ''' Blue Prism connection
        ''' </summary>
        Public Shared ReadOnly BPServer As _
         New ConnectionTemplate(ConnectionType.BPServer,
          Nothing,
          ConnectionDetailRow.ConnectionName,
          ConnectionDetailRow.ConnectionType,
          ConnectionDetailRow.BPServer,
          ConnectionDetailRow.BPServerPort,
          ConnectionDetailRow.CallbackPort,
          ConnectionDetailRow.BPServerConnectionMode,
          ConnectionDetailRow.Ordered)

        ''' <summary>
        ''' SQL Server Availability Group connection with SQL Server authentication
        ''' </summary>
        Public Shared ReadOnly AvailabilitySqlAuth As _
         New ConnectionTemplate(ConnectionType.Availability,
          False,
          ConnectionDetailRow.ConnectionName,
          ConnectionDetailRow.ConnectionType,
          ConnectionDetailRow.DBServer,
          ConnectionDetailRow.DBName,
          ConnectionDetailRow.UserID,
          ConnectionDetailRow.Password,
          ConnectionDetailRow.AvailabilityGroup,
          ConnectionDetailRow.ExtraConnectionParams)

        ''' <summary>
        ''' SQL Server Availability Group connection with Windows authentication
        ''' </summary>
        Public Shared ReadOnly AvailabilityWinAuth As _
         New ConnectionTemplate(ConnectionType.Availability,
          True,
          ConnectionDetailRow.ConnectionName,
          ConnectionDetailRow.ConnectionType,
          ConnectionDetailRow.DBServer,
          ConnectionDetailRow.DBName,
          ConnectionDetailRow.AvailabilityGroup,
          ConnectionDetailRow.ExtraConnectionParams)

        ''' <summary>
        ''' Connection string specified by the user directly
        ''' </summary>
        Public Shared ReadOnly SqlConnectionString As New ConnectionTemplate(
            ConnectionType.CustomConnectionString,
            False, ' this connection type may or may not be windows auth, but this parameter will just be ignored anyway
            ConnectionDetailRow.ConnectionName,
            ConnectionDetailRow.ConnectionType,
            ConnectionDetailRow.CustomConnectionString)

        ''' <summary>
        ''' Gets the connection template which corresponds to the given connection
        ''' type and windows auth setting.
        ''' </summary>
        ''' <param name="tp">The type of connection for which the template is
        ''' required.</param>
        ''' <param name="winAuth">True to get the 'windows authentication' version of
        ''' the template for the connection type; False to retrieve the 'SQL Server
        ''' authentication' version instead. Ignored for BP Server connection types.
        ''' </param>
        ''' <returns>The connection template associated with the given connection
        ''' type and authentication mode, or null if no associated template could be
        ''' found.</returns>
        Public Shared Function GetCorrespondingTemplate(
         ByVal tp As ConnectionType, ByVal winAuth As Boolean) As ConnectionTemplate
            Select Case tp
                Case ConnectionType.BPServer
                    Return ConnectionTemplate.BPServer

                Case ConnectionType.Availability
                    If winAuth Then Return ConnectionTemplate.AvailabilityWinAuth
                    Return ConnectionTemplate.AvailabilitySqlAuth

                Case ConnectionType.Direct
                    If winAuth Then Return ConnectionTemplate.SqlWindowsAuth
                    Return ConnectionTemplate.SqlServerAuth

                Case ConnectionType.CustomConnectionString
                    Return ConnectionTemplate.SqlConnectionString
            End Select
            Return Nothing
        End Function

        ' The label for this connection type
        Private mLabel As String

        ' The rows which are visible for this connection
        Private mRowsVisible As ICollection(Of ConnectionDetailRow)

        ' The underlying connection type for this connection
        Private mType As ConnectionType

        ' Flag indicating if this template is a windows-auth template or not
        Private mWindowsAuth As Boolean

        ''' <summary>
        ''' Creates a new connection template with the given label and the list of
        ''' rows within the table layout panel which should be visible for this
        ''' type of connection.
        ''' </summary>
        ''' <param name="tp">The connection type which corresponds to this template.
        ''' </param>
        ''' <param name="winAuth">True to indicate that this template represents a
        ''' connection type using windows authentication; false otherwise. Note that
        ''' this is effectively ignored for BP Server connections (for which the
        ''' win authentication option has no meaning).</param>
        ''' <param name="rowsVisible">The rows which should be displayed when this
        ''' template is being used in the panel.</param>
        Private Sub New(ByVal tp As ConnectionType,
         ByVal winAuth As Boolean,
         ByVal ParamArray rowsVisible() As ConnectionDetailRow)
            mType = tp
            mWindowsAuth = winAuth
            mRowsVisible = New clsSet(Of ConnectionDetailRow)(rowsVisible)
        End Sub

        ''' <summary>
        ''' A string representation of this connection template - this is just the
        ''' label used to represent this type of connection in the UI
        ''' </summary>
        ''' <returns>A string representation of this connection template</returns>
        Public Overrides Function ToString() As String
            Return Label
        End Function

        Property Label As String


        ''' <summary>
        ''' Checks if this connection template requires the given row number
        ''' (corresponding to the <see cref="ConnectionDetailRow"/> enumeration) in
        ''' order to be configured or not.
        ''' </summary>
        ''' <param name="rowNum">The row number to check whether it is needed for
        ''' this connection template.</param>
        ''' <returns>True if this connection template requires the given row to be
        ''' displayed; False otherwise.</returns>
        Public Function IncludesRow(ByVal rowNum As Integer) As Boolean
            Return IncludesRow(DirectCast(rowNum, ConnectionDetailRow))
        End Function

        ''' <summary>
        ''' Checks if this connection template requires the given row in order to be
        ''' configured or not.
        ''' </summary>
        ''' <param name="row">The row to check whether it is needed for this
        ''' connection template.</param>
        ''' <returns>True if this connection template requires the given row to be
        ''' displayed; False otherwise.</returns>
        Public Function IncludesRow(ByVal row As ConnectionDetailRow) As Boolean
            Return mRowsVisible.Contains(row)
        End Function

        ''' <summary>
        ''' The visible rows for this connection template.
        ''' </summary>
        Public ReadOnly Property VisibleRows() As ICollection(Of ConnectionDetailRow)
            Get
                Return GetReadOnly.ICollection(Of ConnectionDetailRow)(mRowsVisible)
            End Get
        End Property

        ''' <summary>
        ''' The type of connection that this template represents
        ''' </summary>
        Public ReadOnly Property ConnectionType() As ConnectionType
            Get
                Return mType
            End Get
        End Property

        ''' <summary>
        ''' Gets whether this template represents a type of connection which uses
        ''' windows authentication.
        ''' </summary>
        Public ReadOnly Property WindowsAuth() As Boolean
            Get
                Return (mType <> ConnectionType.BPServer AndAlso mWindowsAuth)
            End Get
        End Property

    End Class

#End Region

#Region " Member Variables "

    ' The connection setting currently being modelled in this UI
    Private mConnection As clsDBConnectionSetting

    ' The last set connection type - ie. the current state of the UI
    Private mCurrConnectionTemplate As ConnectionTemplate = Nothing

    ' The last port used. Kept so that if disabled then re-enabled 
    ' we can put the old value back.
    Private mOldCallbackPort As Integer = 0

#End Region

#Region " Constructors "

    Public Sub New()

        DoubleBuffered = True

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Initialize captions
        CaptionControl1.Caption = My.Resources.TheNameByWhichThisConnectionWillBeRemembered
        CaptionControl7.Caption = My.Resources.TheHostnameOfTheBluePrismServer
        CaptionControl6.Caption = My.Resources.ThePasswordOfTheUserNamedAbove
        CaptionControl5.Caption = My.Resources.TheDatabaseUserNameToUse
        CaptionControl4.Caption = My.Resources.TheNameOfTheDatabaseToConnectTo
        CaptionControl3.Caption = My.Resources.TheHostnameOfTheDatabaseServer
        CaptionControl2.Caption = My.Resources.TheTypeOfConnectionToUse
        CaptionedControl1.Caption = My.Resources.SemiColonSeparatedParametersToAddToTheConnectionString
        CaptionedControl2.Caption = My.Resources.ThisMustMatchTheModeConfiguredOnTheBluePrismServerS
        CaptionedControl3.Caption = My.Resources.ThisMustMatchTheListeningPortConfiguredOnTheBluePrismServerS
        CaptionedControl4.Caption = My.Resources.ThePortOnThisDeviceWhichReceivesCallbackCommunication
        CaptionedControl5.Caption = My.Resources.TheCompleteSQLConnectionStringToUse

        ' Add any initialization after the InitializeComponent() call.
        With cmbConnType.Items
            .Add(ConnectionTemplate.SqlServerAuth)
            .Add(ConnectionTemplate.SqlWindowsAuth)
            .Add(ConnectionTemplate.BPServer)
            .Add(ConnectionTemplate.AvailabilitySqlAuth)
            .Add(ConnectionTemplate.AvailabilityWinAuth)
            .Add(ConnectionTemplate.SqlConnectionString)
        End With

        ConnectionTemplate.SqlServerAuth.Label = My.Resources.SQLServerSQLAuthentication
        ConnectionTemplate.SqlWindowsAuth.Label = My.Resources.SQLServerWindowsAuthentication
        ConnectionTemplate.BPServer.Label = My.Resources.BluePrismServer
        ConnectionTemplate.AvailabilitySqlAuth.Label = My.Resources.AvailabilityGroupSQLAuthentication
        ConnectionTemplate.AvailabilityWinAuth.Label = My.Resources.AvailabilityGroupWindowsAuthentication
        ConnectionTemplate.SqlConnectionString.Label = My.Resources.SQLServerCustomConnectionString

        cmbConnType.SelectedItem = ConnectionTemplate.SqlServerAuth
        mCurrConnectionTemplate = Nothing
        cmbConnectionMode.DataSource =
            [Enum].GetValues(GetType(ServerConnection.Mode))
        ConnectionSetting = Nothing

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The connection in this connection detail panel.
    ''' </summary>
    Public Property ConnectionSetting() As clsDBConnectionSetting
        Get
            Return mConnection
        End Get
        Set(ByVal val As clsDBConnectionSetting)
            mConnection = val
            If val Is Nothing Then
                txtConnectionName.Text = ""
                cmbConnType.SelectedItem = Nothing
                txtDbName.Text = ""
                txtDbServer.Text = ""
                txtUserId.Text = ""
                txtPassword.SecurePassword = New SafeString()
                txtBpServer.Text = ""
                txtConnectionString.Text = ""
                numServerPort.Value = Options.Instance.DefaultServerPort
                numCallbackPort.Value = 0
                chkOrdered.Checked = True
                For i As Integer = 0 To panMain.RowCount - 1
                    SetRowEnabled(i, False)
                Next
                cmbConnectionMode.SelectedItem =
                    ServerConnection.Mode.WCFSOAPMessageWindows
                txtExtraParams.Text = ""
                cbDisableCallBack.Checked = False
                numCallbackPort.Text = "0"
            Else
                txtConnectionName.Text = val.ConnectionName
                ' bug 8924: if there is no type (eg. new connection), default to
                ' the first entry in the combo
                If val.ConnectionType = ConnectionType.None Then
                    cmbConnType.SelectedIndex = 0
                Else
                    cmbConnType.SelectedItem =
                     ConnectionTemplate.GetCorrespondingTemplate(
                     val.ConnectionType, val.WindowsAuth)
                End If
                txtDbName.Text = val.DatabaseName
                txtDbServer.Text = val.DBServer
                txtUserId.Text = val.DBUserName
                txtPassword.SecurePassword = val.DBUserPassword
                txtBpServer.Text = val.DBServer
                txtConnectionString.Text = val.CustomConnectionString
                numServerPort.Value = val.Port
                numAGPort.Value = val.AGPort

                cmbConnectionMode.SelectedItem = val.ConnectionMode

                cbMultiSubnetFailover.Checked = val.MultiSubnetFailover
                txtConnectionName.Select()
                txtExtraParams.Text = val.ExtraParams

                If val.CallbackPort = -1 Then
                    numCallbackPort.Text = ""
                    cbDisableCallBack.Checked = True
                Else
                    numCallbackPort.Value = val.CallbackPort
                    cbDisableCallBack.Checked = False
                End If
                chkOrdered.Checked = If(val.Ordered, True)

            End If
        End Set
    End Property

    Public Shared Property IsConnectionNameEmpty As Boolean
    Public Shared Property IsDBServerEmpty As Boolean
    Public Shared Property IsDBNameEmpty As Boolean
    Public Shared Property IsUserIDEmpty As Boolean

    Public Shared Property SelectedConnectionIndex As Integer = 0
    Public Shared Property CurrentConnectionName As String
#End Region

#Region " Methods "

    ''' <summary>
    ''' Sets the given rows enabled status to the specified value.
    ''' </summary>
    ''' <param name="row">The row number to set enabled</param>
    ''' <param name="enable">True to enable the given row; False to disable it
    ''' </param>
    Private Sub SetRowEnabled(ByVal row As Integer, ByVal enable As Boolean)
        For i As Integer = 0 To panMain.ColumnCount - 1
            Dim ctl As Control = panMain.GetControlFromPosition(i, row)
            If ctl IsNot Nothing Then ctl.Enabled = enable
        Next
    End Sub

#End Region

#Region " Event Handlers "

    ''' <summary>
    ''' Raises a NameValidating event with the given args
    ''' </summary>
    Protected Overridable Sub OnNameValidating(ByVal e As NameValidatingEventArgs)
        RaiseEvent NameValidating(Me, e)
    End Sub

    ''' <summary>
    ''' Raises a NameChanged event with the given args
    ''' </summary>
    Protected Overridable Sub OnNameValidated(ByVal e As EventArgs)
        RaiseEvent NameValidated(Me, e)
    End Sub

    ''' <summary>
    ''' Handles the connection type changing, ensuring that the appropriate rows in
    ''' the table layout panel are displayed / hidden
    ''' </summary>
    Private Sub HandleConnectionTemplateChanged(
     ByVal sender As Object, ByVal e As EventArgs) _
     Handles cmbConnType.SelectedIndexChanged
        Dim tp As ConnectionTemplate =
         DirectCast(cmbConnType.SelectedItem, ConnectionTemplate)

        SelectedConnectionIndex = cmbConnType.SelectedIndex

        ' Check if the type is actually the same, and don't bother re-laying out
        ' the table if it is
        If tp Is mCurrConnectionTemplate Then Return

        ' Store the current connection type
        mCurrConnectionTemplate = tp

        ' If the type is null, disable the entire panel and return
        panMain.Enabled = (tp IsNot Nothing)
        If tp Is Nothing Then Return

        ' Otherwise, go through each row and enable / display it if the new connection
        ' type requires it to be enabled / displayed.
        panMain.SuspendLayout()
        Try
            For index = 0 To panMain.RowCount - 1
                If tp.IncludesRow(index) Then
                    panMain.RowStyles(index).Height =
                        If(index = ConnectionDetailRow.BPServerPort OrElse index = ConnectionDetailRow.ExtraConnectionParams OrElse index = ConnectionDetailRow.BPServerConnectionMode, AvailableRowHeight + AdditionalRowHeight, AvailableRowHeight)
                    SetRowEnabled(index, True)
                Else
                    panMain.RowStyles(index).Height = 0
                    SetRowEnabled(index, False)
                End If
            Next
        Finally
            panMain.ResumeLayout()
        End Try

        If mConnection IsNot Nothing Then
            mConnection.ConnectionType = tp.ConnectionType
            mConnection.WindowsAuth = tp.WindowsAuth
            SetCallbackPortVisibility()
            SetOrderedVisibility()
            RaiseEvent ConnectionTypeChanged(Me, EventArgs.Empty)
        End If
    End Sub

    Private Sub HandleConnectionNameChange(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtConnectionName.TextChanged
        If mConnection IsNot Nothing Then mConnection.ConnectionName = txtConnectionName.Text.Trim()
    End Sub

    Private Sub HandleDBServerChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtDbServer.TextChanged
        If mConnection IsNot Nothing Then mConnection.DBServer = txtDbServer.Text
    End Sub

    Private Sub HandleDbNameChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtDbName.TextChanged
        If mConnection IsNot Nothing Then mConnection.DatabaseName = txtDbName.Text
    End Sub

    Private Sub HandleUserIdChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtUserId.TextChanged
        If mConnection IsNot Nothing Then mConnection.DBUserName = txtUserId.Text
    End Sub

    Private Sub HandlePasswordChanged(ByVal sender As Object, ByVal e As EventArgs) _
        Handles txtPassword.TextChanged
        If mConnection IsNot Nothing Then _
            mConnection.DBUserPassword = txtPassword.SecurePassword
    End Sub

    Private Sub HandleBpServerChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtBpServer.TextChanged
        If mConnection IsNot Nothing Then mConnection.DBServer = txtBpServer.Text
    End Sub

    Private Sub HandleExtraParamsChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtExtraParams.TextChanged
        If mConnection IsNot Nothing Then mConnection.ExtraParams = txtExtraParams.Text
    End Sub

    Private Sub HandleServerPortChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles numServerPort.ValueChanged
        If mConnection IsNot Nothing Then mConnection.Port = CInt(numServerPort.Value)
    End Sub

    Private Sub HandleAGPortChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles numAGPort.ValueChanged
        If mConnection IsNot Nothing Then mConnection.AGPort = CInt(numAGPort.Value)
    End Sub

    Private Sub HandleCallbackPortChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles numCallbackPort.ValueChanged
        If mConnection IsNot Nothing Then mConnection.CallbackPort = CInt(numCallbackPort.Value)
    End Sub

    Private Sub HandleConnectionModeSelectedIndexChanged(sender As Object, e As EventArgs) _
     Handles cmbConnectionMode.SelectedIndexChanged
        If mConnection IsNot Nothing Then
            mConnection.ConnectionMode = CType(cmbConnectionMode.SelectedItem, ServerConnection.Mode)
        End If

        SetCallbackPortVisibility()
        SetOrderedVisibility()
    End Sub

    Private Sub HandleConnectionStringChanged(ByVal sender As Object, ByVal e As EventArgs) _
        Handles txtConnectionString.TextChanged
        If mConnection IsNot Nothing Then
            mConnection.CustomConnectionString = txtConnectionString.Text
        End If
    End Sub

    ''' <summary>
    ''' Show/Hide the Callback Port row. This row will only ever be visible on the 
    ''' BP Server template, and should only be visible for .NET Remoting connection 
    ''' modes.
    ''' </summary>
    Private Sub SetCallbackPortVisibility()
        If mCurrConnectionTemplate IsNot Nothing AndAlso
            mCurrConnectionTemplate.ConnectionType = ConnectionType.BPServer Then
            Dim row As Integer = ConnectionDetailRow.CallbackPort
            Dim selectedMode =
                CType(cmbConnectionMode.SelectedItem, ServerConnection.Mode)
            Try
                If selectedMode = ServerConnection.Mode.DotNetRemotingInsecure OrElse
                        selectedMode = ServerConnection.Mode.DotNetRemotingSecure Then
                    panMain.RowStyles(row).Height = AvailableRowHeight
                    SetRowEnabled(row, True)
                Else
                    panMain.RowStyles(row).Height = 0
                    SetRowEnabled(row, False)
                End If
            Finally
                panMain.ResumeLayout()
            End Try
        End If
    End Sub

    Private Sub SetOrderedVisibility()
        If mCurrConnectionTemplate IsNot Nothing AndAlso
           mCurrConnectionTemplate.ConnectionType = ConnectionType.BPServer Then
            Dim row As Integer = ConnectionDetailRow.Ordered
            Dim selectedMode =
                    CType(cmbConnectionMode.SelectedItem, ServerConnection.Mode)
            Try
                If selectedMode = ServerConnection.Mode.WCFInsecure OrElse
                   selectedMode = ServerConnection.Mode.WCFSOAPMessageWindows OrElse
                   selectedMode = ServerConnection.Mode.WCFSOAPTransport OrElse
                   selectedMode = ServerConnection.Mode.WCFSOAPTransportWindows Then
                    panMain.RowStyles(row).Height = AvailableRowHeight
                    SetRowEnabled(row, True)
                Else
                    panMain.RowStyles(row).Height = 0
                    SetRowEnabled(row, False)
                End If
            Finally
                panMain.ResumeLayout()
            End Try
        End If
    End Sub


    Private Sub HandleMultiSubnetFailoverCheckedChanged(ByVal sender As Object, ByVal e As EventArgs) _
     Handles cbMultiSubnetFailover.CheckedChanged
        If mConnection IsNot Nothing Then mConnection.MultiSubnetFailover = cbMultiSubnetFailover.Checked
    End Sub

    ''' <summary>
    ''' Handles the validating of the connection name textbox.
    ''' </summary>
    Private Sub HandleTextBoxValidating(ByVal sender As Object, ByVal e As CancelEventArgs) _
     Handles txtConnectionName.Validating, txtDbServer.Validating, txtDbName.Validating, txtUserId.Validating, txtPassword.Validating
        If mConnection IsNot Nothing Then
            IsUserIDEmpty = String.IsNullOrEmpty(txtUserId.Text.Trim())
            IsDBNameEmpty = String.IsNullOrEmpty(txtDbName.Text.Trim())
            IsDBServerEmpty = String.IsNullOrEmpty(txtDbServer.Text.Trim())
            IsConnectionNameEmpty = String.IsNullOrEmpty(txtConnectionName.Text.Trim())
        End If
    End Sub

    ''' <summary>
    ''' Handles the name textbox being validated
    ''' </summary>
    Private Sub HandleNameValidated(ByVal sender As Object, ByVal e As EventArgs) _
     Handles txtConnectionName.Validated
        CurrentConnectionName = txtConnectionName.Text
        OnNameValidated(e)
    End Sub

    Private Sub DoTestClick(ByVal sender As Object, ByVal e As EventArgs) Handles btnTest.Click

        If mConnection Is Nothing Then Return

        If Not mConnection.IsComplete() Then
            UserMessage.Show(My.Resources.PleaseFillInAllConnectionDetailsBeforeTestingTheConnection)
            Return
        End If

        Try
            Dim builder = New SqlConnectionStringBuilder(mConnection.ExtraParams)
        Catch ex As Exception
            UserMessage.Show(My.Resources.InvalidAdditionalSqlParametersEntered)
            Return
        End Try

        If mConnection.ConnectionType = ConnectionType.CustomConnectionString Then
            Try
                Cursor = Cursors.WaitCursor
                Dim builder = New SqlConnectionStringBuilder(mConnection.CustomConnectionString)
                mConnection.DatabaseName = If(builder?.InitialCatalog, "")
            Catch ex As Exception
                ' Ignore this for now, the invalid string will be reported correctly to the user
            Finally
                Cursor = Cursors.Default
            End Try
        End If

        Try
            Cursor = Cursors.WaitCursor
            mConnection.Validate()

            If mConnection.ConnectionType = ConnectionType.BPServer Then
                UserMessage.Show(My.Resources.ConnectionValid)
            Else
                UserMessage.Show(My.Resources.DatabaseValid)
            End If

        Catch mse As MessageSecurityException When _
            TypeOf mse.InnerException Is FaultException
            'An unsecured or incorrectly secured fault was received from the other party.
            UserMessage.Err(My.Resources.EnsureThatTheSelectedConnectionModeMatchesTheConfiguredConnectionModeOnTheServer, mse)
            Return

        Catch snw As SecurityNegotiationException When _
            TypeOf snw.InnerException Is WebException

            Dim webException = TryCast(snw.InnerException, WebException)
            If webException IsNot Nothing Then
                Dim status = webException.Status
                If status = WebExceptionStatus.TrustFailure Then
                    'Could not establish trust relationship for the SSL/TLS secure channel with authority '{0}'.
                    UserMessage.Err(My.Resources.VerifyThatTheCertificateOnTheServerHasBeenAppropriatelyConfiguredAndIsValidForT, snw)
                    Return
                End If
            End If

            UserMessage.Err(snw)
            Return

        Catch snf As SecurityNegotiationException When _
            TypeOf snf.InnerException Is FaultException

            Dim faultException = TryCast(snf.InnerException, FaultException)
            If faultException IsNot Nothing AndAlso faultException.Code IsNot Nothing AndAlso faultException.Code.SubCode IsNot Nothing Then
                If faultException.Code.SubCode.Name <> "FailedAuthentication" Then
                    'Secure channel cannot be opened because security negotiation with the remote endpoint has failed.
                    UserMessage.Err(My.Resources.EnsureThatTheConnectionModeConfiguredOnTheClientMatchesTheConnectionModeConfigu, snf)
                    Return
                End If
            End If

            UserMessage.Err(snf)
            Return

        Catch enf As EndpointNotFoundException
            'There was no endpoint listening at {0} that could accept the message.
            UserMessage.Err(My.Resources.EnsureThatTheSpecifiedServerAddressMatchesTheBindingConfiguredOnTheServer, enf)
            Return

        Catch ce As CommunicationException When _
            TypeOf ce.InnerException Is WebException

            Dim webException = TryCast(ce.InnerException, WebException)
            If webException IsNot Nothing Then
                Dim status = webException.Status
                If status = WebExceptionStatus.ReceiveFailure OrElse status = WebExceptionStatus.SendFailure Then
                    'An error occurred while receiving the HTTP response to {0}.
                    UserMessage.Err(My.Resources.EnsureThatTheConnectionModeConfiguredOnTheClientMatchesTheConnectionModeConfigu, ce)
                    Return
                End If
            End If
            UserMessage.Err(ce)
            Return

        Catch pe As ProtocolException
            'The remote endpoint has sent an unrecognized fault with namespace, http://www.w3.org/2003/05/soap-envelope, name Sender, and reason The message could not be processed.
            UserMessage.Err(My.Resources.EnsureThatTheSelectedConnectionModeMatchesTheConfiguredConnectionModeOnTheServer, pe)
            Return

        Catch ex As Exception
            UserMessage.Err(ex)
            Return
        Finally
            Cursor = Cursors.Default
        End Try

        Return
    End Sub

    ''' <summary>
    ''' Action for when disable callback is checked
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub HandleDisableCallBack(sender As Object, e As EventArgs) Handles cbDisableCallBack.CheckedChanged
        numCallbackPort.Enabled = Not cbDisableCallBack.Checked
        If mConnection IsNot Nothing Then
            If cbDisableCallBack.Checked Then
                mOldCallbackPort = CInt(numCallbackPort.Value)
                mConnection.CallbackPort = -1
            Else
                If mConnection.CallbackPort = -1 Then
                    ' set "text" over "value" as if it is zero it won't be shown
                    numCallbackPort.Text = mOldCallbackPort.ToString
                End If
                mConnection.CallbackPort = CInt(numCallbackPort.Value)
            End If
        End If
    End Sub

    Private Sub ChkOrdered_CheckedChanged(sender As Object, e As EventArgs) Handles chkOrdered.CheckedChanged
        If mConnection IsNot Nothing Then mConnection.Ordered = chkOrdered.Checked
    End Sub

#End Region

End Class
