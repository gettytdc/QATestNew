Imports System.Runtime.Serialization

Namespace Resources

    ''' <summary>
    ''' Class to hold the settings used to start up a runtime resource
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class ResourcePCStartUpOptions 
        Implements IResourceRunnerStartUpOptions

        Public Sub New()
        End Sub

        <DataMember>
        Private mResourceRunnerType As ResourceRunnerType = ResourceRunnerType.Listener

        Public ReadOnly Property ResourceRunnerType As ResourceRunnerType Implements IResourceRunnerStartUpOptions.ResourceRunnerType
            Get
                Return mResourceRunnerType
            End Get
        End Property

        ''' <summary>
        ''' The port number that the resource is listening on
        ''' Set by /port
        ''' </summary>
        <DataMember>
        Public Property Port() As Integer = ResourceMachine.DefaultPort

        ''' <summary>
        ''' Indicates that this is a public resource
        ''' Set by /public
        ''' </summary>
        <DataMember>
        Public Property IsPublic() As Boolean = False

        ''' <summary>
        ''' True if this Resource is only accessible to the local machine
        ''' Set by /local
        ''' </summary>
        <DataMember>
        Public Property IsLocal() As Boolean = False

        ''' <summary>
        ''' True if this Resource is an automatic (via the interactive client) instance
        ''' </summary>
        <DataMember>
        Public Property IsAuto() As Boolean = False

        ''' <summary>
        ''' Indicates that this is a Login Agent resource
        ''' Set by /loginagent
        ''' </summary>
        <DataMember>
        Public Property IsLoginAgent() As Boolean = False

        ''' <summary>
        ''' User name to restrict access to on startup, or an empty string
        ''' for open access.
        ''' </summary>
        <DataMember>
        Public Property Username() As String = ""

        ''' <summary>
        ''' The hash ('thumbprint') for the SSL certificate we should use, or Nothing
        ''' for plain-text mode
        ''' Set by /sslcert
        ''' </summary>
        <DataMember>
        Public Property SSLCertHash() As String = Nothing

        ''' <summary>
        ''' The overridden addressable location of hosted web services
        ''' Set by /wslocationprefix
        ''' </summary>
        <DataMember>
        Public Property WebServiceAddressPrefix() As String = Nothing

        ''' <summary>
        ''' Indicates whether or not this resource can receive HTTP communications
        ''' Default is True, but can be disabled with the /nohttp switch
        ''' </summary>
        <DataMember>
        Public Property HTTPEnabled() As Boolean = True

    End Class

End Namespace