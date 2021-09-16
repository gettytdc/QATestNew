#If UNITTESTS Then

Imports BluePrism.AutomateProcessCore.WebApis.Credentials
Imports BluePrism.Common.Security

Namespace UnitTests.WebApis
    ''' <summary>
    ''' Class used to create credentials for unit tests
    ''' </summary>
    Public Class TestCredential : Implements ICredential

        Public Shared ReadOnly Frank As New TestCredential("Frank", "FrankM", "secureaf")

        Public Shared ReadOnly Barry As New TestCredential("Barry", "BarryS", "b4rryru13z")

        Public Shared ReadOnly BobWithSecret As TestCredential = CreateBobWithSecret()

        Private Shared Function CreateBobWithSecret() As TestCredential
            Dim bob = New TestCredential("Bob", "bob", "secret")
            bob.Properties.Add("Secret", New SafeString("theSecret"))
            Return bob
        End Function

        Sub New(name As String, username As String, password As String)
            Me.Name = name
            Me.Username = username
            Me.Password = New SafeString(password)
            Me.Properties = New Dictionary(Of String, SafeString)
        End Sub

        Public ReadOnly Property Name As String Implements ICredential.Name
        Public ReadOnly Property Username As String Implements ICredential.Username
        Public ReadOnly Property Password As SafeString Implements ICredential.Password

        Public Property Properties As IDictionary(Of String, SafeString) Implements ICredential.Properties
    End Class
End Namespace

#End If

