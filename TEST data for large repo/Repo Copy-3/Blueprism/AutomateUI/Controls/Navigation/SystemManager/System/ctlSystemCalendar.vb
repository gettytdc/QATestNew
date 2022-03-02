Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Auth

Public Class ctlSystemCalendar
    Implements IStubbornChild
    Implements IPermission

    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        calCalendar.Store = New DatabaseBackedScheduleStore(gSv)
        calCalendar.Populate()
    End Sub

    Private mParent As frmApplication
    Friend Property ParentAppForm As frmApplication Implements IChild.ParentAppForm
        Get
            Return mParent
        End Get
        Set(value As frmApplication)
            mParent = value
        End Set
    End Property

    Public Function CanLeave() As Boolean Implements IStubbornChild.CanLeave
        Return calCalendar.CanLeave()
    End Function

    Public ReadOnly Property RequiredPermissions() As System.Collections.Generic.ICollection(Of BluePrism.AutomateAppCore.Auth.Permission) Implements BluePrism.AutomateAppCore.Auth.IPermission.RequiredPermissions
        Get
            Return Permission.ByName("System - Calendars")
        End Get
    End Property
End Class
