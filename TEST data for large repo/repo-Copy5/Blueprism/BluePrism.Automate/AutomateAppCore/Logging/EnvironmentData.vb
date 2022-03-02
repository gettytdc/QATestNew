Imports System.Runtime.Serialization

Namespace Logging
    <Serializable>
    <DataContract([Namespace]:="bp")>
    Public Class EnvironmentData

        <DataMember>
        Private ReadOnly mEnvironmentType As EnvironmentType

        <DataMember>
        Private ReadOnly mFullyQualifiedDomainName As String

        <DataMember>
        Private mPortNumber As Integer?

        <DataMember>
        Private ReadOnly mVersionNumber As String

        <DataMember>
        Private ReadOnly mCreatedDateTime As Date

        <DataMember>
        Private ReadOnly mUpdatedDateTime As Date

        <DataMember>
        Private ReadOnly mCertificateExpTime As DateTime?

        <DataMember>
        Private ReadOnly mApplicationServer As String

        Public Sub New(environmentType As EnvironmentType,
                       fullyQualifiedDomainName As String,
                       portNumber As Integer?,
                       versionNumber As String,
                       createdDateTime As Date,
                       updatedDateTime As Date,
                       certificateExpTime As DateTime?)

            Me.New(environmentType, fullyQualifiedDomainName, portNumber, versionNumber, createdDateTime, updatedDateTime, certificateExpTime, Nothing)

        End Sub

        Public Sub New(environmentType As EnvironmentType,
                       fullyQualifiedDomainName As String,
                       portNumber As Integer?,
                       versionNumber As String,
                       createdDateTime As Date,
                       updatedDateTime As Date,
                       certificateExpTime As DateTime?,
                       applicationServer As String)

            mEnvironmentType = environmentType
            mFullyQualifiedDomainName = fullyQualifiedDomainName
            mPortNumber = portNumber
            mVersionNumber = versionNumber
            mCreatedDateTime = createdDateTime
            mUpdatedDateTime = updatedDateTime
            mCertificateExpTime = certificateExpTime
            mApplicationServer = applicationServer

        End Sub

        Public ReadOnly Property EnvironmentType As EnvironmentType
            Get
                Return mEnvironmentType
            End Get
        End Property

        Public ReadOnly Property FullyQualifiedDomainName As String
            Get
                Return mFullyQualifiedDomainName
            End Get
        End Property

        Public ReadOnly Property PortNumber As Integer?
            Get
                Return mPortNumber
            End Get
        End Property

        Public ReadOnly Property VersionNumber As String
            Get
                Return mVersionNumber
            End Get
        End Property

        Public ReadOnly Property CreatedDateTime As Date
            Get
                Return mCreatedDateTime
            End Get
        End Property

        Public ReadOnly Property UpdatedDateTime As Date
            Get
                Return mUpdatedDateTime
            End Get
        End Property

        Public ReadOnly Property CertificateExpTime As DateTime?
            Get
                Return mCertificateExpTime
            End Get
        End Property

        Public ReadOnly Property ApplicationServer As String
            Get
                Return mApplicationServer
            End Get
        End Property

    End Class
End Namespace

