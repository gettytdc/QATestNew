Public Class UniqueUsernameGenerator : Implements IUniqueUsernameGenerator

    Public Function GenerateUsername(originalUsername As String) As String Implements IUniqueUsernameGenerator.GenerateUsername
        Return originalUsername + "-" + New Random(Guid.NewGuid().GetHashCode()).Next(1000, 10000).ToString()
    End Function
End Class
