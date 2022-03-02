Public Class RunnerRequest

    Public Property ResumeSession As Boolean
    Public Property SessionId As Guid
    Public Property ProcessId As Guid
    Public Property StarterUserId As Guid
    Public Property StarterResourceId As Guid
    Public Property RunningResourceId As Guid
    Public Property QueueIdent As Integer
    Public Property AutoInstance As Boolean
    Public Property AuthorisationToken As clsAuthToken

End Class
