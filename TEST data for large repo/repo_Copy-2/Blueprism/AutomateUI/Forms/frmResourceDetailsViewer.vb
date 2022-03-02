Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Logging

Friend Class FrmResourceDetailsViewer
    Inherits frmForm
    Public ReadOnly Property ResourceName As String

    Public Sub New(resourceName As String)
        Me.ResourceName = resourceName
        InitializeComponent()
    End Sub

    Private Function FormatText(textToFormat As String) As String
        Return IIf(String.IsNullOrWhiteSpace(textToFormat), My.Resources.NA, textToFormat).ToString()
    End Function

    Private Function FormatDateToString(dateToFormat As Date) As String
        Return IIf(dateToFormat = Date.MinValue, String.Empty, dateToFormat.ToString).ToString()
    End Function

    Private Sub BtnClose_Click(sender As Object, e As EventArgs) Handles btnClose.Click
        Close()
    End Sub

    Private Sub FrmResourceDetailsViewer_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        Dim resourceDetails = gSv.GetResourceEnvironmentData(ResourceName)
        lblFqdn.Text = FormatText(resourceDetails?.FullyQualifiedDomainName)
        lblPort.Text = FormatText(resourceDetails?.PortNumber.ToString())
        lblVersion.Text = FormatText(resourceDetails?.VersionNumber)
        lblFirstConnected.Text =
            FormatText(
                FormatDateToString(If(resourceDetails Is Nothing, Date.MinValue, resourceDetails.CreatedDateTime)))
        lblLastUpdated.Text =
            FormatText(
                FormatDateToString(If(resourceDetails Is Nothing, Date.MinValue, resourceDetails.UpdatedDateTime)))
        lblApplicationServer.Text = FormatText(resourceDetails?.ApplicationServer)
        If resourceDetails IsNot Nothing Then
            lblEnvironmentType.Text = FormatText(GetEnvironmentType(resourceDetails.EnvironmentType))
        Else
            lblEnvironmentType.Text = FormatText(My.Resources.NA)
        End If
    End Sub

    Private Function GetEnvironmentType(environmentType As EnvironmentType) As String
        Select Case environmentType
            Case EnvironmentType.Client
                Return My.Resources.EnvironmentTypeClient
            Case EnvironmentType.Server
                Return My.Resources.EnvironmentTypeServer
            Case EnvironmentType.Resource
                Return My.Resources.EnvironmentTypeResource
            Case EnvironmentType.None
                Return My.Resources.EnvironmentTypeNone
            Case Else
                Return My.Resources.NA
        End Select

    End Function
End Class
