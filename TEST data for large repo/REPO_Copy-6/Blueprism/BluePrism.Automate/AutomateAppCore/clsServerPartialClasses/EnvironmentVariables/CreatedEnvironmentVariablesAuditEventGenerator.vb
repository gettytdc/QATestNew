Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore

Namespace clsServerPartialClasses.EnvironmentVariables

    Public Class CreatedEnvironmentVariablesAuditEventGenerator

        Private ReadOnly mNewEnvironmentVariable As clsEnvironmentVariable

        Public Sub New(newEnvironmentVariable As clsEnvironmentVariable)
            mNewEnvironmentVariable = newEnvironmentVariable
        End Sub

        Public Function Generate(eventCode As EnvironmentVariableEventCode, user As IUser) As EnvironmentVariablesAuditEvent
            Return GetAuditEventForModifiedEnvironmentVariable(eventCode, user)
        End Function

        Private Function GetAuditEventForModifiedEnvironmentVariable(eventCode As EnvironmentVariableEventCode, user As IUser) As EnvironmentVariablesAuditEvent

            Dim comment As New StringBuilder()

            AppendCreatedValueToAuditComment(comment, My.Resources.CreatedEnvironmentVariablesAuditEventGenerator_Name,
                                             mNewEnvironmentVariable.Name)
            AppendCreatedValueToAuditComment(comment, My.Resources.CreatedEnvironmentVariablesAuditEventGenerator_DataType,
                                             clsDataTypeInfo.GetLocalizedFriendlyName(mNewEnvironmentVariable.DataType))
            AppendCreatedValueToAuditComment(comment, My.Resources.CreatedEnvironmentVariablesAuditEventGenerator_Description,
                                             mNewEnvironmentVariable.Description)
            AppendCreatedValueToAuditComment(comment, My.Resources.CreatedEnvironmentVariablesAuditEventGenerator_Value,
                                             mNewEnvironmentVariable.Value)

            Dim auditComment = comment.ToString()

            If String.IsNullOrEmpty(auditComment) Then Return Nothing

            Return New EnvironmentVariablesAuditEvent(eventCode, user, mNewEnvironmentVariable.Name, auditComment)

        End Function

        Private Sub AppendCreatedValueToAuditComment(auditComment As StringBuilder, valueDescription As String, newValue As Object)

            If newValue Is Nothing Then Return
            auditComment.AppendFormat(My.Resources.CreatedEnvironmentVariablesAuditEventGenerator_AddCreatedValueToAuditComment, valueDescription, GetStringFromValue(newValue))

        End Sub

        Private Function GetStringFromValue(value As Object) As String

            If value Is Nothing Then Return String.Empty

            If TypeOf value Is DateTime Then Return DirectCast(value, DateTime).ToUniversalTime.ToString("O")

            Return value.ToString()

        End Function

    End Class

End Namespace