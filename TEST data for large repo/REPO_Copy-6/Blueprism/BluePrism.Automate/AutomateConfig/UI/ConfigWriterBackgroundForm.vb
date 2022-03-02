Imports BluePrism.AutomateAppCore
Imports System.Runtime.Remoting.Channels.Ipc
Imports System.Runtime.Remoting.Channels
Imports System.Runtime.Remoting

Public Class ConfigWriterBackgroundForm

    Private mChannel As IpcServerChannel

    Friend Sub New()
        Me.New(-1)
    End Sub

    Friend Sub New(ByVal procId As Integer)

        If procId > 0 Then

            ' select channel to communicate
            Dim settings As New Hashtable()
            settings("portName") = "automateconfig-server-" & procId
            settings("authorizedGroup") = "Everyone"
            mChannel = New IpcServerChannel(settings, Nothing)


            ' register channel
            ChannelServices.RegisterChannel(mChannel, True)

            ' register remote object
            RemotingConfiguration.RegisterWellKnownServiceType( _
             GetType(ConfigManager), _
             "ConfigManager", _
             WellKnownObjectMode.Singleton)
        End If

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

    End Sub

    Private Sub HandleExit(ByVal sender As Object, ByVal e As EventArgs) Handles menuExit.Click
        Dispose()
        Close()
        Application.Exit()
    End Sub

    Protected Overrides Sub Dispose(ByVal explicit As Boolean)
        'Form overrides dispose to clean up the component list.
        Try
            If explicit Then
                If components IsNot Nothing Then components.Dispose() : components = Nothing
                If explicit AndAlso mChannel IsNot Nothing Then ChannelServices.UnregisterChannel(mChannel) : mChannel = Nothing
            End If
        Catch ex As Exception
            Debug.WriteLine("Error disposing of ConfigWriterBackgroundForm: " & ex.ToString())
        Finally
            MyBase.Dispose(explicit)
        End Try
    End Sub


End Class