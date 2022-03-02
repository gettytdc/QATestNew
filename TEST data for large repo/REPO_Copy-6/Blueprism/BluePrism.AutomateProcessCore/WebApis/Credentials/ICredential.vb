Imports BluePrism.Common.Security

Namespace WebApis.Credentials

    ''' <summary>
    ''' Contains details of a credentials used to access a system
    ''' </summary>
    Public Interface ICredential

        ''' <summary>
        ''' Gets the name used to identify the credential
        ''' </summary>
        ReadOnly Property Name As String

        ''' <summary>
        ''' Gets the credential user name
        ''' </summary>
        Readonly Property Username As String

        ''' <summary>
        ''' Gets a SafeString containing the password stored for the credential
        ''' </summary>
        ReadOnly Property Password As SafeString

        ''' <summary>
        ''' Gets a dictionary of the properties associated with this credential
        ''' </summary>
        ReadOnly Property Properties As IDictionary(Of String, SafeString)

    End Interface

End Namespace