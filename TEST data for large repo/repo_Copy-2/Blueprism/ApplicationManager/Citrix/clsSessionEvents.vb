Imports System.Runtime.InteropServices.ComTypes

Public Class clsSessionEvents
    Implements ISessionEvents

    Public Event OnPingAck(ByVal pingInfo As String, ByVal roundTripTime As Integer)
    Public Event OnWindowCreate(ByVal window As IWindow)
    Public Event OnWindowDestroy(ByVal window As IWindow)
    Public Event OnWindowForeground(ByVal WindowID As Integer)

    Public Sub New(ByVal objSession As ISession)
        Dim objConnectionPointContainer As IConnectionPointContainer = TryCast(objSession, IConnectionPointContainer)
        Dim riid As New Guid(modClassID.ISessionEvent)
        Dim objConnectionPoint As IConnectionPoint = Nothing
        objConnectionPointContainer.FindConnectionPoint(riid, objConnectionPoint)
        Dim cookie As Integer
        objConnectionPoint.Advise(Me, cookie)
    End Sub

    Public Sub RaisePingAck(ByVal pingInfo As String, ByVal roundTripTime As Integer) Implements ISessionEvents.OnPingAck
        RaiseEvent OnPingAck(pingInfo, roundTripTime)
    End Sub

    Public Sub RaiseWindowCreate(ByVal window As IWindow) Implements ISessionEvents.OnWindowCreate
        RaiseEvent OnWindowCreate(window)
    End Sub

    Public Sub RaiseWindowDestroy(ByVal window As IWindow) Implements ISessionEvents.OnWindowDestroy
        RaiseEvent OnWindowDestroy(window)
    End Sub

    Public Sub RaiseWindowForeground(ByVal WindowID As Integer) Implements ISessionEvents.OnWindowForeground
        RaiseEvent OnWindowForeground(WindowID)
    End Sub
End Class
