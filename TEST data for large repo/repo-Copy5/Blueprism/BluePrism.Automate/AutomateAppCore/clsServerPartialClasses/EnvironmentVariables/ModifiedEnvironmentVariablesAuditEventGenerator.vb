Imports BluePrism.AutomateAppCore.Auth

Namespace clsServerPartialClasses.EnvironmentVariables
    Public Class ModifiedEnvironmentVariablesAuditEventGenerator

        Private ReadOnly mOldEnvironmentVariable As clsEnvironmentVariable
        Private ReadOnly mNewEnvironmentVaraible As clsEnvironmentVariable
        Private ReadOnly mLoggedInUser As IUser

        Public Sub New(oldEnvironmentVariable As clsEnvironmentVariable, newEnvironmentVariable As clsEnvironmentVariable, loggedInUser As IUser)
            mOldEnvironmentVariable = oldEnvironmentVariable
            mNewEnvironmentVaraible = newEnvironmentVariable
            mLoggedInUser = loggedInUser
        End Sub

        Public Function Generate(EventCode As EnvironmentVariableEventCode, User As IUser) As EnvironmentVariablesAuditEvent
            Return GetAuditEventForModifiedEnvironmentVariable(EventCode, User)
        End Function

        Private Function GetAuditEventForModifiedEnvironmentVariable(EventCode As EnvironmentVariableEventCode, User As IUser) _
         As EnvironmentVariablesAuditEvent

            Dim comment As New StringBuilder()

            AppendModifiedValueToAuditComment(comment, "Name", mOldEnvironmentVariable.Name, mNewEnvironmentVaraible.Name)
            AppendModifiedValueToAuditComment(comment, "Data Type", mOldEnvironmentVariable.DataType, mNewEnvironmentVaraible.DataType)
            AppendModifiedValueToAuditComment(comment, "Description", mOldEnvironmentVariable.Description, mNewEnvironmentVaraible.Description)
            AppendModifiedValueToAuditComment(comment, "Value", mOldEnvironmentVariable.Value, mNewEnvironmentVaraible.Value)

            Dim auditComment = comment.ToString()

            If String.IsNullOrEmpty(auditComment) Then Return Nothing

            Return New EnvironmentVariablesAuditEvent(EventCode, User, mNewEnvironmentVaraible.Name, auditComment)

        End Function

        Private Sub AppendModifiedValueToAuditComment(auditComment As StringBuilder, valueDescription As String,
                                                     oldValue As Object, newValue As Object)

            If (oldValue Is Nothing AndAlso newValue Is Nothing) Then Return
            If oldValue IsNot Nothing AndAlso oldValue.Equals(newValue) Then Return

            auditComment.AppendFormat(My.Resources.ModifiedEnvironmentVariablesAuditEventGenerator_AppendModifiedValueToAuditComment_0ValueChangedOldValue1NewValue2, valueDescription,
                                      If(oldValue IsNot Nothing, GetStringFromValue(oldValue), String.Empty),
                                      If(newValue IsNot Nothing, GetStringFromValue(newValue), String.Empty))
        End Sub

        Private Function GetStringFromValue(value As Object) As String

            If value Is Nothing Then Return String.Empty

            If value.GetType() Is GetType(DateTime) Then Return DirectCast(value, DateTime).ToUniversalTime.ToString("O")

            Return value.ToString()

        End Function


    End Class

End Namespace
