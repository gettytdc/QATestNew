Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth

Public Class ctlDataGatewayConfigurationContainer
    Implements IStubbornChild, IPermission, IHelp


    Private mContentControl As Control

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        RefreshUI()
    End Sub

    Public Sub RefreshUI()

        Dim isCustomConfiguration = gSv.IsCustomConfiguration("Default")

        If RequiresNewContentControl(isCustomConfiguration) Then
            If mContentControl IsNot Nothing Then
                contentPanel.Controls.Remove(mContentControl)
            End If

            If Not isCustomConfiguration Then
                mContentControl = New ctlDataPipelineConfiguration(Me)
            Else
                mContentControl = New ctlDataGatewayCustomConfiguration(Me)
            End If
            mContentControl.Dock = DockStyle.Fill
            contentPanel.Controls.Add(mContentControl)
        End If



    End Sub

    Private Function RequiresNewContentControl(isCustomConfiguration As Boolean) As Boolean
        If mContentControl Is Nothing Then Return True

        Dim currentContentControlType = mContentControl.GetType()

        Return (isCustomConfiguration AndAlso currentContentControlType = GetType(ctlDataPipelineConfiguration)) OrElse
               (Not isCustomConfiguration AndAlso currentContentControlType = GetType(ctlDataGatewayCustomConfiguration))

    End Function
    Public ReadOnly Property RequiredPermissions As ICollection(Of Permission) Implements IPermission.RequiredPermissions
        Get
            Return DirectCast(mContentControl, IPermission).RequiredPermissions
        End Get
    End Property

    Public Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return DirectCast(mContentControl, IChild).ParentAppForm
        End Get
        Set(value As frmApplication)
            DirectCast(mContentControl, IChild).ParentAppForm = value
        End Set
    End Property

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Return DirectCast(mContentControl, IStubbornChild).CanLeave
    End Function

    Public Function GetHelpFile() As String Implements IHelp.GetHelpFile
        Return "data-gateways.htm"
    End Function

End Class
