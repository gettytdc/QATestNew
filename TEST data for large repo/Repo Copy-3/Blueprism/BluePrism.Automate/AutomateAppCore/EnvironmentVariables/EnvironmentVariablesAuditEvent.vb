Imports BluePrism.AutomateAppCore.Auth

Public Class EnvironmentVariablesAuditEvent
#Region "Member variables"

    Private mUser As IUser

#End Region

#Region "Constructors"

    Public Sub New(inputCode As EnvironmentVariableEventCode,
                    inputUser As IUser,
                    inputEnvVarName As String)
        Me.New(inputCode, inputUser, inputEnvVarName, Nothing)
    End Sub

    Public Sub New(inputCode As EnvironmentVariableEventCode,
                    inputUser As IUser,
                    inputEnvVarName As String,
                    inputComment As String)
        Code = inputCode
        EnvironmentVariableName = inputEnvVarName
        Comment = inputComment
        mUser = inputUser
    End Sub

#End Region

#Region "Properties"

    Public ReadOnly Property UserId() As Guid
        Get
            Return mUser.Id
        End Get
    End Property

    Public ReadOnly Property Code As EnvironmentVariableEventCode

    Public ReadOnly Property EnvironmentVariableName As String

    Public ReadOnly Property Comment As String

#End Region

End Class
