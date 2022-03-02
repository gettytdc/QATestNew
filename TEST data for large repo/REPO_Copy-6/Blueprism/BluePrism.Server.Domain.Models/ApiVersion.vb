Public Class ApiVersion

    Public Property Version As Version
    Public Property ApiHash As String

    Public Sub New(apiVersion As String)
        Try
            Dim split = apiVersion.Split("-"c)
            Version = Version.Parse(split(0))
            If split.Length > 1 Then
                ApiHash = split(1)
            End If
        Catch 'If anythin goes wrong just store the full string
            Version = Nothing
            ApiHash = apiVersion
        End Try
    End Sub

    Public Shared Function Parse(apiVersion As String) As ApiVersion
        Return New ApiVersion(apiVersion)
    End Function

    Public Overrides Function ToString() As String
        If Version Is Nothing Then Return ApiHash
        If String.IsNullOrEmpty(ApiHash) Then Return Version.ToString
        Return $"{Version}-{ApiHash}"
    End Function

    Public Function ToFriendlyString() As String
        If Version Is Nothing Then Return ApiHash
        Return $"{Version.Major}.{Version.Minor}.{Version.Build}"
    End Function

    Public Shared Function AreCompatible(a As String, b As String) As Boolean
        Return AreCompatible(Parse(a), Parse(b))
    End Function

    Public Shared Function AreCompatible(a As ApiVersion, b As ApiVersion) As Boolean
        Return a.Version.Major = b.Version.Major AndAlso
               a.Version.Minor = b.Version.Minor AndAlso
               a.ApiHash = b.ApiHash
    End Function

End Class

