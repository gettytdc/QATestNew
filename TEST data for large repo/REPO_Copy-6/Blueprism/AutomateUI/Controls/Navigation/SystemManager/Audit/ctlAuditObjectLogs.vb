Imports AutomateUI
Imports BluePrism.AutomateAppCore.Auth

Public Class ctlAuditObjectLogs
    Implements IChild, IPermission, IStubbornChild

    Private Sub ctlAuditObjectLogs_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        logsObj.ParentAppForm = ParentAppForm
        logsObj.Populate()
    End Sub

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Return True
    End Function

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public ReadOnly Property RequiredPermissions() As System.Collections.Generic.ICollection(Of BluePrism.AutomateAppCore.Auth.Permission) Implements BluePrism.AutomateAppCore.Auth.IPermission.RequiredPermissions
        Get
            Return Permission.ByName("Audit - Business Object Logs")
        End Get
    End Property
End Class
