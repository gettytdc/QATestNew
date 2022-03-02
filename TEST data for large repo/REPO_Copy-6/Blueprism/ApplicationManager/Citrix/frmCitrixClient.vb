Public Class frmCitrixClient

    Private WithEvents objClient As ctlClient
    Private objSession As ISession
    Private WithEvents objSessionEvents As clsSessionEvents
    Private WithEvents objMouseEvents As clsMouseEvents

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        'Dim o As Object = CreateObject("Citrix.ICAClient")

        objClient = New ctlClient

        With objClient
            .Dock = DockStyle.Fill
            .BeginInit()
            Me.Controls.Add(objClient)
            .EndInit()
        End With

    End Sub

    Public ReadOnly Property Client() As ctlClient
        Get
            Return objClient
        End Get
    End Property

    Private Sub objClient_Connected() Handles objClient.OnConnect
        objSession = objClient.Session
        objSessionEvents = New clsSessionEvents(objSession)
        objMouseEvents = New clsMouseEvents(objSession.Mouse)
    End Sub

    Private Sub objMouseEvents_OnMouseMove(ByVal buttonstate As Integer, ByVal modifierstate As Integer, ByVal XPos As Integer, ByVal YPos As Integer) Handles objMouseEvents.OnMouseMove
        lblPosition.Text = "X: " & XPos & " Y: " & YPos
    End Sub

    Private Sub frmCitrixClient_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If Not objClient Is Nothing Then
            objClient.Disconnect()
            e.Cancel = True 'Dont close until we have disconnected
        End If
    End Sub

    Private Sub objClient_OnDisconnect() Handles objClient.OnDisconnect
        objSession = Nothing
        objSessionEvents = Nothing
        objMouseEvents = Nothing
        objClient.Dispose()
        objClient = Nothing
        Me.Close()
    End Sub
End Class
