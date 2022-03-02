Imports System.Xml
Imports System.Security.Cryptography.X509Certificates
Imports System.Runtime.Serialization


Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Common.Security
Imports BluePrism.Core.Xml

''' <summary>
''' Enumeration for the various asset types within the web service details.
''' </summary>
Public Enum AssetType
    Unknown
    WSDL
    XSD
End Enum

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsWebServiceDetails
''' 
''' <summary>
''' This class is used to store and transfer details about a particular web service
''' registered with Automate and available to processes.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class clsWebServiceDetails : Implements IObjectDetails

    ''' <summary>
    ''' Creates a new, empty, web service details object.
    ''' </summary>
    Public Sub New()
    End Sub

    ''' <summary>
    ''' Wrapper class around X509Certificate2 to make it serializable
    ''' Serialisation for .NET remoting is done via a byte array.
    ''' This class also provides convenience methods for converting
    ''' to base64, so it can be stored in xml.
    ''' </summary>
    <Serializable>
    Private Class SerialisableCertificate : Implements ISerializable
        <NonSerialized()> _
        Private mCertificate As X509Certificate2

        Public Sub New()
        End Sub

        ''' <summary>
        ''' Handles creation of the certificate via serialisation.
        ''' </summary>
        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
            If info.MemberCount > 0 Then 'Assume there is only one member as per GetObjectData()
                mCertificate = New X509Certificate2(CType(info.GetValue("cert", GetType(Byte())), Byte()))
            End If
        End Sub

        ''' <summary>
        ''' Serialises the certificate
        ''' </summary>
        Public Sub GetObjectData(ByVal info As System.Runtime.Serialization.SerializationInfo, _
        ByVal context As System.Runtime.Serialization.StreamingContext) _
        Implements System.Runtime.Serialization.ISerializable.GetObjectData
            If mCertificate IsNot Nothing Then
                info.AddValue("cert", mCertificate.Export(X509ContentType.SerializedCert))
            End If
        End Sub

        ''' <summary>
        ''' Provides access to the underlying X509Certificate2
        ''' </summary>
        Public Property Value() As X509Certificate2
            Get
                Return mCertificate
            End Get
            Set(ByVal value As X509Certificate2)
                mCertificate = value
            End Set
        End Property

        ''' <summary>
        ''' Converts the certificate to a Base64 string
        ''' </summary>
        Public Function ToBase64String() As String
            Dim cert() As Byte = mCertificate.Export(X509ContentType.SerializedCert)
            Return Convert.ToBase64String(cert)
        End Function

        ''' <summary>
        ''' Converts the certificate from a Base64 string
        ''' </summary>
        Public Shared Function FromBase64String(ByVal s As String) As SerialisableCertificate
            Dim cert As Byte() = Convert.FromBase64String(s)
            Dim ser As New SerialisableCertificate
            ser.mCertificate = New X509Certificate2(cert)
            Return ser
        End Function
    End Class

    ''' <summary>
    ''' Creates a new web service details object, initialised from the given provider
    ''' </summary>
    ''' <param name="prov">The data provider whence to draw the data. This
    ''' constructor expects the following attributes in the provider: <list>
    ''' <item>id: Guid</item>
    ''' <item>name: String</item>
    ''' <item>enabled: Boolean</item>
    ''' <item>url: String</item>
    ''' <item>wsdl: String</item>
    ''' <item>timeout: Integer</item>
    ''' <item>settings: String</item></list></param>
    Public Sub New(ByVal prov As IDataProvider)
        mId = prov.GetValue("id", Guid.Empty)
        mEnabled = prov.GetValue("enabled", False)
        mFriendlyName = prov.GetString("name")
        mURL = prov.GetString("url")
        mWSDL = prov.GetString("wsdl")
        mTimeout = prov.GetValue("timeout", 0)
        SetSettings(prov.GetString("settingsxml"))
    End Sub

    ''' <summary>
    ''' The service ID for the web service represented by these details.
    ''' </summary>
    Public Property Id() As Guid
        Get
            Return mId
        End Get
        Set(ByVal value As Guid)
            mId = value
        End Set
    End Property
    <DataMember>
    Private mId As Guid

    ''' <summary>
    ''' The URL of the web service - this is the URL used to retrieve
    ''' the WSDL for the service.
    ''' </summary>
    Public Property URL() As String
        Get
            Return mURL
        End Get
        Set(ByVal Value As String)
            mURL = Value
        End Set
    End Property
    <DataMember>
    Private mURL As String

    ''' <summary>
    ''' The WSDL for the web service.
    ''' </summary>
    Public Property WSDL() As String
        Get
            Return mWSDL
        End Get
        Set(ByVal Value As String)
            mWSDL = Value
        End Set
    End Property
    <DataMember>
    Private mWSDL As String

    ''' <summary>
    ''' Extra WSDL documents referenced by the root document.
    ''' </summary>
    Public ReadOnly Property ExtraWSDL() As List(Of String)
        Get
            If mExtraWSDL Is Nothing Then
                mExtraWSDL = New List(Of String)
            End If
            Return mExtraWSDL
        End Get
    End Property
    <DataMember>
    Private mExtraWSDL As List(Of String)

    ''' <summary>
    ''' Schemas relating to the web service.
    ''' </summary>
    Public ReadOnly Property Schemas() As List(Of String)
        Get
            If mSchemas Is Nothing Then
                mSchemas = New List(Of String)
            End If
            Return mSchemas
        End Get
    End Property
    <DataMember>
    Private mSchemas As List(Of String)

    ''' <summary>
    ''' The timeout to use, in milliseconds, when interactive with the web service.
    ''' </summary>
    <DataMember>
    Public Property Certificate() As X509Certificate2
        Get
            If mCertificate Is Nothing Then Return Nothing
            Return mCertificate.Value
        End Get
        Set(ByVal value As X509Certificate2)
            If mCertificate Is Nothing Then mCertificate = New SerialisableCertificate()
            mCertificate.Value = value
        End Set
    End Property
    Private mCertificate As New SerialisableCertificate

    ''' <summary>
    ''' The timeout to use, in milliseconds, when interactive with the web service.
    ''' </summary>
    Public Property Timeout() As Integer
        Get
            Return mTimeout
        End Get
        Set(ByVal value As Integer)
            mTimeout = value
        End Set
    End Property
    <DataMember>
    Private mTimeout As Integer

    ''' <summary>
    ''' The 'friendly name' for the web service - this is the name that is presented
    ''' to users when editing a process.
    ''' </summary>
    Public Property FriendlyName() As String Implements IObjectDetails.FriendlyName
        Get
            Return mFriendlyName
        End Get
        Set(ByVal Value As String)
            mFriendlyName = Value
        End Set
    End Property
    <DataMember>
    Private mFriendlyName As String

    ''' <summary>
    ''' The name of the service to use
    ''' </summary>
    Public Property ServiceToUse() As String
        Get
            Return mServiceToUse
        End Get
        Set(ByVal Value As String)
            mServiceToUse = Value
        End Set
    End Property
    <DataMember>
    Private mServiceToUse As String

    ''' <summary>
    ''' Value indicating that the web service has been loaded and we are not just
    ''' making a preliminary investigation based on the wsdl.
    ''' </summary>
    Public Property Loaded() As Boolean
        Get
            Return mbLoaded
        End Get
        Set(ByVal value As Boolean)
            mbLoaded = value
        End Set
    End Property
    <DataMember>
    Private mbLoaded As Boolean

    ''' <summary>
    ''' Flag indicating whether the web service is enabled or not.
    ''' </summary>
    Public Property Enabled() As Boolean
        Get
            Return mEnabled
        End Get
        Set(ByVal value As Boolean)
            mEnabled = value
        End Set
    End Property
    <DataMember>
    Private mEnabled As Boolean = True

    ''' <summary>
    ''' The username needed for the web service. This value can be set to Nothing in
    ''' which case no credentials will be used for the web service
    ''' </summary>
    Public Property Username() As String
        Get
            Return mUsername
        End Get
        Set(ByVal value As String)
            mUsername = value
        End Set
    End Property
    <DataMember>
    Private mUsername As String

    ''' <summary>
    ''' The 'Secret' (password) needed for the web service. If the Username or Secret
    ''' is set to nothing no credentials will be used for the web service
    ''' </summary>
    Public Property Secret() As SafeString
        Get
            Return mSecret
        End Get
        Set(ByVal value As SafeString)
            mSecret = value
        End Set
    End Property
    <DataMember>
    Private mSecret As SafeString

    ''' <summary>
    ''' A list of actions that should be enabled in the web service
    ''' </summary>
    Public ReadOnly Property Actions() As Dictionary(Of String, Boolean)
        Get
            Return mActions
        End Get
    End Property
    <DataMember>
    Private mActions As New Dictionary(Of String, Boolean)

    ''' <summary>
    ''' Returns whether an action with a given name is enabled
    ''' </summary>
    Public Function ActionEnabled(ByVal name As String) As Boolean
        If mActions.ContainsKey(name) Then
            Return mActions(name)
        End If
        Return False
    End Function

    ''' <summary>
    ''' Returns a comma-space delimited string of the enabled actions.
    ''' </summary>
    Public Function EnabledActions() As String
        Dim list As New List(Of String)
        For Each s As String In mActions.Keys
            If mActions(s) Then
                list.Add(s)
            End If
        Next
        Return CollectionUtil.Join(list, ", ")
    End Function

    ''' <summary>
    ''' Boolean indicating whether the service details are complete, determined by
    ''' whether the details have a non empty id.
    ''' </summary>
    Public ReadOnly Property Complete() As Boolean
        Get
            Return Id <> Guid.Empty
        End Get
    End Property

    ''' <summary>
    ''' Set the configuration for the web service from XML - this is Automate
    ''' specific, and contains details on how we interact with
    ''' the web service - e.g. which methods are supported.
    ''' </summary>
    ''' <param name="xml">The XML containing the configuration</param>
    Public Sub SetSettings(xml As String)
        If mActions Is Nothing Then
            mActions = New Dictionary(Of String, Boolean)
        Else
            mActions.Clear()
        End If
        If String.IsNullOrEmpty(xml) Then Return

        Dim xd As New ReadableXmlDocument(xml)
        Dim service As XmlElement = TryCast(xd.SelectSingleNode("/settings/service"), XmlElement)
        mServiceToUse = service.GetAttribute("name")

        Dim hasUsername = service.HasAttribute("username")

        'A secret attribute indicates that the password is stored in the old
        'unsafe way as a plain text string. Retrieving this needs to be
        'supported for backwards compatibility.
        Dim usingUnsafeString = service.HasAttribute("secret")
        If hasUsername AndAlso usingUnsafeString Then
            mUsername = service.GetAttribute("username")
            mSecret = New SafeString(service.GetAttribute("secret"))
        End If

        For Each child As Xml.XmlElement In service.ChildNodes
            'Get the username and secret if the secret is being stored as an XML
            'encoded safestring
            If hasUsername AndAlso Not usingUnsafeString AndAlso _
                child.Name = SafeString.XmlElementName Then
                mUsername = service.GetAttribute("username")
                mSecret = SafeString.FromXml(child)
                Continue For
            End If

            If child.Name = "method" Then
                Dim name As String = child.GetAttribute("name")
                Dim enabled As Boolean = child.GetAttribute("enabled").ToLower = "true"
                mActions.Add(name, enabled)
            End If
        Next

        Dim xCertificate As XmlElement = TryCast(xd.SelectSingleNode("/settings/service/certificate"), XmlElement)
        If xCertificate IsNot Nothing Then
            mCertificate = SerialisableCertificate.FromBase64String(xCertificate.InnerText)
        End If
    End Sub

    ''' <summary>
    ''' Get the configuration XML for the web service - this is Automate
    ''' specific, and contains details on how we interact with
    ''' the web service - e.g. which methods are supported.
    ''' </summary>
    ''' <returns>The XML containing the configuration</returns>
    Public Function GetSettings() As String
        Dim xd As New XmlDocument()
        xd.AppendChild(xd.CreateXmlDeclaration("1.0", "", ""))

        Dim root As XmlElement = xd.CreateElement("settings")
        Dim service As XmlElement = xd.CreateElement("service")
        service.SetAttribute("name", mServiceToUse)

        If mUsername IsNot Nothing AndAlso mSecret IsNot Nothing Then
            service.SetAttribute("username", mUsername)
            service.AppendChild(mSecret.ToXml(xd))
        End If

        For Each item As String In mActions.Keys
            Dim meth As XmlElement = xd.CreateElement("method")
            meth.SetAttribute("name", item)
            meth.SetAttribute("enabled", XmlConvert.ToString(mActions(item)))
            service.AppendChild(meth)
        Next

        If Certificate IsNot Nothing Then
            Dim sCert As String = mCertificate.ToBase64String()
            Dim xCert As XmlElement = xd.CreateElement("certificate")
            Dim xCertText As XmlText = xd.CreateTextNode(sCert)
            xCert.AppendChild(xCertText)
            service.AppendChild(xCert)
        End If

        root.AppendChild(service)
        xd.AppendChild(root)
        Return xd.OuterXml
    End Function

End Class
