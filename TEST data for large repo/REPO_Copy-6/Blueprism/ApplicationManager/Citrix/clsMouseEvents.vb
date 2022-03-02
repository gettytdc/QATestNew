Imports System.Runtime.InteropServices.ComTypes

Public Class clsMouseEvents
    Implements IMouseEvents

    Public Event OnMouseDoubleClick()
    Public Event OnMouseUp(ByVal buttonstate As Integer, ByVal modifierstate As Integer, ByVal XPos As Integer, ByVal YPos As Integer)
    Public Event OnMouseDown(ByVal buttonstate As Integer, ByVal modifierstate As Integer, ByVal XPos As Integer, ByVal YPos As Integer)
    Public Event OnMouseMove(ByVal buttonstate As Integer, ByVal modifierstate As Integer, ByVal XPos As Integer, ByVal YPos As Integer)


    Public Sub New(ByVal objMouse As IMouse)
        Dim objConnectionPointContainer As IConnectionPointContainer = TryCast(objMouse, IConnectionPointContainer)
        Dim riid As New Guid(modClassID.IMouseEvents)
        Dim objConnectionPoint As IConnectionPoint = Nothing
        objConnectionPointContainer.FindConnectionPoint(riid, objConnectionPoint)
        Dim cookie As Integer
        objConnectionPoint.Advise(Me, cookie)
    End Sub

    Public Sub RaiseDoubleClick() Implements IMouseEvents.OnDoubleClick
        RaiseEvent OnMouseDoubleClick()
    End Sub

    Public Sub RaiseMouseDown(ByVal buttonState As Integer, ByVal modifierState As Integer, ByVal XPos As Integer, ByVal YPos As Integer) Implements IMouseEvents.OnMouseDown
        RaiseEvent OnMouseDown(buttonState, modifierState, XPos, YPos)
    End Sub

    Public Sub RaiseMouseUp(ByVal buttonState As Integer, ByVal modifierState As Integer, ByVal XPos As Integer, ByVal YPos As Integer) Implements IMouseEvents.OnMouseUp
        RaiseEvent OnMouseUp(buttonState, modifierState, XPos, YPos)
    End Sub

    Public Sub RaiseMove(ByVal buttonState As Integer, ByVal modifierState As Integer, ByVal XPos As Integer, ByVal YPos As Integer) Implements IMouseEvents.OnMove
        RaiseEvent OnMouseMove(buttonState, modifierState, XPos, YPos)
    End Sub
End Class
