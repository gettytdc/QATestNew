

''' <summary>
''' Exception thrown when an operation requiring the credential master
''' key is attempted, and the master key does not exist.
''' </summary>
<Serializable>
Public Class MissingCredentialMasterKeyException : Inherits ConfigurationException

    ''' <summary>
    ''' Creates a new exception with the default message.
    ''' </summary>
    Public Sub New()
        MyBase.New(My.Resources.MissingCredentialMasterKeyException_CredentialMasterKeyWasNotFound)
    End Sub

End Class
