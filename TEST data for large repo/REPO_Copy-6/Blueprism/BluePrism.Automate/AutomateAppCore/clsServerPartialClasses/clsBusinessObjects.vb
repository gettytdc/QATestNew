Imports BluePrism.AutomateAppCore.Auth

Partial Public Class clsServer
    <SecuredMethod()>
    Public Sub BusinessObjectAdded(name As String) Implements IServer.BusinessObjectAdded
        CheckPermissions()
        Using con = GetConnection()
            AuditRecordObjectEvent(con, ObjectEventCode.AddObject, name, String.Empty)
        End Using
    End Sub

    <SecuredMethod()>
    Public Sub BusinessObjectDeleted(name As String) Implements IServer.BusinessObjectDeleted
        CheckPermissions()
        Using con = GetConnection()
            AuditRecordObjectEvent(con, ObjectEventCode.DeleteObject, name, String.Empty)
        End Using
    End Sub
End Class
