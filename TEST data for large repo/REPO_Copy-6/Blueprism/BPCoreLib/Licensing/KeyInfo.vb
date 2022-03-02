Imports System.Xml
Imports System.Security.Cryptography.Xml
Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Encapsulation of details of a particular key that's stored in the system.
''' </summary>
<Serializable>
<DataContract(Namespace:="bp")>
Public NotInheritable Class KeyInfo

    ''' <summary>
    ''' The license key used in the event of a database error, or in the absence of a
    ''' valid key. This key puts the product in a mode where pretty much nothing can
    ''' be done, other than fixing the licensing problem.
    ''' </summary>
    Private Const DefaultLicenseKey As String =
        "<license><type>none</type><licensee>Unlicensed</licensee><maxprocesses>0</maxprocesses><maxresources>0</maxresources><maxconcurrentsessions>0</maxconcurrentsessions><maxprocessalerts>0</maxprocessalerts><starts>0001-01-01</starts><expires>2099-01-01</expires></license>"

    ''' <summary>
    ''' The default licence key, in place when there are no other licences available
    ''' in the environment.
    ''' </summary>
    Public Shared ReadOnly DefaultLicense As New KeyInfo(DefaultLicenseKey)

    ' The ID of this key in the backing store (if it has one)
    <DataMember>
    Private mId As Integer

    ' The actual key, in string form. This may be base64-encoded XML or raw XML
    <DataMember>
    Private mKey As String

    ' The date/time the key data was set
    <DataMember>
    Private mSetAt As Date

    ' The ID of the user who set the key data
    <DataMember>
    Private mSetBy As Guid

    <DataMember>
    Private mActivationResponse As String

    ' The key data in XML form
    <NonSerialized>
    Private mLicenseXml As XmlDocument

    ' Whether this key uses the transaction model for payments - ie. should the
    ' work queue operations be recording all transactions
    <DataMember>
    Private mTransactionModel As Nullable(Of Boolean)

    <DataMember>
    Private mActivationInfo As ICollection(Of ActivationInfo)

    <DataMember>
    Private mEnvironmentId As String

    ''' <summary>
    ''' Creates a new KeyInfo object, encapsulating the specified key data.
    ''' </summary>
    ''' <param name="key">The data representing the licence, XML in either raw or
    ''' base64-encoded form.</param>
    Public Sub New(key As String)
        Me.New(key, Date.MinValue, Guid.Empty)
    End Sub

    ''' <summary>
    ''' Creates a new KeyInfo object, encapsulating the specified key data.
    ''' </summary>
    ''' <param name="key">The data representing the licence, XML in either raw or
    ''' base64-encoded form.</param>
    ''' <param name="at">The date/time at which the key was added</param>
    ''' <param name="by">The ID of the user who added the key</param>
    Public Sub New(key As String, at As DateTime, by As Guid)
        Me.New(0, key, at, by, Nothing, Nothing, Nothing)
    End Sub

    Public Sub New(id As Int32, key As String, at As DateTime, by As Guid)
        Me.New(id, key, at, by, Nothing, Nothing, Nothing)
    End Sub

    Public Sub New(id As Int32, key As String, at As DateTime, by As Guid, activationResponse As String,
                   activationInfo As ICollection(Of ActivationInfo), environmentId As String)
        Me.Key = key
        Me.mId = id
        SetAt = at
        SetBy = by
        mActivationResponse = activationResponse
        mActivationInfo = activationInfo
        mEnvironmentId = environmentId
    End Sub

    Public ReadOnly Property ActivationStatus As LicenseActivationStatus
        Get
            If Not RequiresActivation Then
                Return LicenseActivationStatus.NotApplicable
            ElseIf LicenseHasValidActivationResponse() Then
                Return LicenseActivationStatus.Activated
            Else
                Return LicenseActivationStatus.NotActivated
            End If
        End Get
    End Property

    Public ReadOnly Property ActivationStatusLabel As String
        Get
            Dim statusString As String
            Select Case ActivationStatus
                Case LicenseActivationStatus.Activated
                    statusString = My.Resources.KeyInfo_ActivationStatus_Activated
                Case LicenseActivationStatus.NotActivated
                    statusString = My.Resources.KeyInfo_ActivationStatus_NotActivated
                Case Else
                    statusString = My.Resources.KeyInfo_ActivationStatus_NotApplicable
            End Select
            Return statusString
        End Get
    End Property

    Public ReadOnly Property Effective As Boolean
        Get
            Return (Me.Key IsNot Nothing AndAlso Me.Started AndAlso Not Me.Expired AndAlso
                (ActivationStatus = LicenseActivationStatus.NotApplicable OrElse
                 (ActivationStatus = LicenseActivationStatus.Activated OrElse Me.IsWithinGracePeriod)))
        End Get
    End Property

    Private Function LicenseHasValidActivationResponse() As Boolean
        'Cache the result in a static variable
        Static valid As Boolean = False
        If valid Then Return True

        If Not String.IsNullOrWhiteSpace(Me.ActivationResponse) Then
            Dim ixisContent = Licensing.GetLicenseActivationJSONContent(Me.ActivationResponse)
            If ixisContent IsNot Nothing Then

                If String.Equals(ixisContent.EnvironmentID, mEnvironmentId, StringComparison.InvariantCultureIgnoreCase) _
                    AndAlso mActivationInfo.Any(Function(x) x.Reference = ixisContent.ActivationReference AndAlso x.RequestId = ixisContent.ActivationRequestID) Then
                    valid = True
                End If
            End If
        End If
        Return valid
    End Function

    Public ReadOnly Property Id As Integer
        Get
            Return Me.mId
        End Get
    End Property

    ''' <summary>
    ''' Gets/sets the underlying key defining this licence. This is XML in either
    ''' raw form or base-64 encoded form.
    ''' </summary>
    Public Property Key As String
        Get
            Return mKey
        End Get
        Private Set(value As String)
            If value = "" Then mKey = Nothing : LicenseData = Nothing : Return
            Try
                'Accept base64 encoded licenses (that's how we ship them) as well as
                'plain XML...
                Dim doc As New XmlDocument() With {.XmlResolver = Nothing}
                doc.LoadXml(GetKeyXml(value))

                ' Everything converted / parsed correctly, we can update our data.
                mKey = value
                LicenseData = doc

            Catch ex As Exception
                Throw New InvalidFormatException(
                 My.Resources.KeyInfo_TheLicenseKeyFormatIsNotValidForThisVersionOfBluePrism)

            End Try

        End Set
    End Property

    ''' <summary>
    ''' Gets The XML data which is represented by the current <see cref="Key"/>
    ''' </summary>
    Public Property LicenseData As XmlDocument
        Get
            ' If this key info has been serialized, the license XML may not be
            ' present (it can't be serialized), so we may need to reparse the key
            ' in order to generate a new XML document.
            If mLicenseXml Is Nothing Then
                ' No license xml and no key means we have nothing to return
                If String.IsNullOrWhiteSpace(mKey) Then Return Nothing
                Try
                    Dim doc As New XmlDocument() With {.XmlResolver = Nothing}
                    doc.LoadXml(GetKeyXml(mKey))
                    mLicenseXml = doc
                Catch ex As Exception
                    ' This really shouldn't happen - the key cannot be set without
                    ' passing through a parse in the 'Key' property, but just in
                    ' case, ensure that we don't return anything if we can't parse
                    ' the currently set key
                    Debug.Fail(ex.ToString())
                    Return Nothing
                End Try
            End If
            Return mLicenseXml
        End Get
        Private Set(value As XmlDocument)
            mLicenseXml = value
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the date/time that this licence was set.
    ''' </summary>
    Public Property SetAt As DateTime
        Get
            Return mSetAt
        End Get
        Private Set(value As DateTime)
            mSetAt = value
        End Set
    End Property

    ''' <summary>
    ''' Gets/sets the ID of the user who set this licence.
    ''' </summary>
    Public Property SetBy As Guid
        Get
            Return mSetBy
        End Get
        Private Set(value As Guid)
            mSetBy = value
        End Set
    End Property

    Public Property ActivationResponse As String
        Get
            Return mActivationResponse
        End Get
        Private Set(value As String)
            mActivationResponse = value
        End Set
    End Property

    ''' <summary>
    ''' Checks if this licence allows unlimited published processes.
    ''' </summary>
    Public ReadOnly Property AllowsUnlimitedPublishedProcesses() As Boolean
        Get
            Return (NumPublishedProcesses = Integer.MaxValue)
        End Get
    End Property

    ''' <summary>
    ''' Gets the label to use for the number of process alert PCs allowed by this
    ''' key.
    ''' </summary>
    Public ReadOnly Property NumPublishedProcessesLabel As String
        Get
            Return GetLabel(AllowsUnlimitedPublishedProcesses, NumPublishedProcesses)
        End Get
    End Property

    ''' <summary>
    ''' The number of processes that this license permits to be published.
    ''' </summary>
    Public ReadOnly Property NumPublishedProcesses() As Integer
        Get
            Return GetInt("/license/maxprocesses", Integer.MaxValue)
        End Get
    End Property

    ''' <summary>
    ''' Checks if this licence allows unlimited resources
    ''' </summary>
    Public ReadOnly Property AllowsUnlimitedResourcePCs() As Boolean
        Get
            Return (NumResourcePCs = Integer.MaxValue)
        End Get
    End Property

    ''' <summary>
    ''' The number of (unretired) resource PCs that this license permits
    ''' to be registered.
    ''' </summary>
    Public ReadOnly Property NumResourcePCs() As Integer
        Get
            Return GetInt("/license/maxresources", Integer.MaxValue)
        End Get
    End Property

    ''' <summary>
    ''' Gets the label to use for the number of resources allowed by this key.
    ''' </summary>
    Public ReadOnly Property NumResourcePCsLabel As String
        Get
            Return GetLabel(AllowsUnlimitedResourcePCs, NumResourcePCs)
        End Get
    End Property

    ''' <summary>
    ''' Checks if this licence allows unlimited sessions
    ''' </summary>
    Public ReadOnly Property AllowsUnlimitedSessions() As Boolean
        Get
            Return (NumConcurrentSessions = Integer.MaxValue)
        End Get
    End Property

    ''' <summary>
    ''' The number of concurrent sessions that this license permits
    ''' to be run.
    ''' </summary>
    Public ReadOnly Property NumConcurrentSessions() As Integer
        Get
            Return GetInt("/license/maxconcurrentsessions", Integer.MaxValue)
        End Get
    End Property

    ''' <summary>
    ''' Gets the label to use for the number of concurrent sessions allowed by this
    ''' key.
    ''' </summary>
    Public ReadOnly Property NumConcurrentSessionsLabel As String
        Get
            Return GetLabel(AllowsUnlimitedSessions, NumConcurrentSessions)
        End Get
    End Property


    ''' <summary>
    ''' Checks if this licence allows an unlimited number of process alerts apps
    ''' running.
    ''' </summary>
    Public ReadOnly Property AllowsUnlimitedProcessAlertsPCs() As Boolean
        Get
            Return (NumProcessAlertsPCs = Integer.MaxValue)
        End Get
    End Property

    ''' <summary>
    ''' Gets the label to use for the number of process alert PCs allowed by this
    ''' key.
    ''' </summary>
    Public ReadOnly Property NumProcessAlertsPCsLabel As String
        Get
            Return GetLabel(AllowsUnlimitedProcessAlertsPCs, NumProcessAlertsPCs)
        End Get
    End Property

    ''' <summary>
    ''' The number of machines on which this license permits process alerts to be
    ''' run.
    ''' </summary>
    Public ReadOnly Property NumProcessAlertsPCs() As Integer
        Get
            Return GetInt("/license/maxprocessalerts", Integer.MaxValue)
        End Get
    End Property

    ''' <summary>
    ''' The signature of the license
    ''' </summary>
    Public ReadOnly Property SignatureValue() As String
        Get
            Return GetString("/license/*[local-name()='Signature']/*[local-name()='SignatureValue']")?.Replace(vbCr, "")?.Replace(vbCrLf, "")
        End Get
    End Property

    ''' <summary>
    ''' Property to indicate whether this installation uses the transaction licence
    ''' model (i.e. billed according to work queue activity).
    ''' </summary>
    Public ReadOnly Property TransactionModel() As Boolean
        Get
            If Not mTransactionModel.HasValue Then
                Dim txn = GetString("/license/transactionmodel")
                mTransactionModel = (txn IsNot Nothing AndAlso CBool(txn))
            End If
            Return mTransactionModel.Value
        End Get
    End Property

    ''' <summary>
    ''' The type of license.
    ''' </summary>
    Public ReadOnly Property LicenseType() As LicenseTypes
        Get
            Dim lic As LicenseTypes
            Dim licStr = GetString("/license/type")
            If Not clsEnum.TryParse(licStr, True, lic) Then _
             Throw New InvalidTypeException(My.Resources.KeyInfo_InvalidLicenseType0, licStr)
            Return lic
        End Get
    End Property


    ''' <summary>
    ''' Name of the recipient of this license.
    ''' </summary>
    Public ReadOnly Property LicenseOwner() As String
        Get
            Return GetString("/license/licensee")
        End Get
    End Property

    ''' <summary>
    ''' The date at which this license expires
    ''' </summary>
    Public ReadOnly Property ExpiryDate() As DateTime
        Get
            Dim endText = GetString("/license/expires")
            Return If(endText Is Nothing, Date.MaxValue, Date.Parse(endText))
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether the license has expired
    ''' </summary>
    ''' <value>True if expired, False otherwise</value>
    Public ReadOnly Property Expired() As Boolean
        Get
            Return (ExpiryDate < DateTime.Today)
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether the license is close to expiry (i.e. within 31 days)
    ''' </summary>
    ''' <value>True if close to expiry, False otherwise</value>
    Public ReadOnly Property ExpiresSoon() As Boolean
        Get
            Return (Not Expired AndAlso
                ExpiryDate < DateTime.Today.AddDays(LicenseChanges.NumberOfDays))
        End Get
    End Property

    ''' <summary>
    ''' The date on which this license comes into effect
    ''' </summary>
    Public ReadOnly Property StartDate() As DateTime
        Get
            ' Earlier licences didn't have start dates so we need to check for that
            Dim startsText = GetString("/license/starts")
            Return If(startsText Is Nothing, Date.MinValue, Date.Parse(startsText))
        End Get
    End Property

    ''' <summary>
    ''' Name of the recipient of this license.
    ''' </summary>
    Public ReadOnly Property Standalone() As Boolean
        Get
            Return GetString("/license/standalone") = "true"
        End Get
    End Property

    ''' <summary>
    ''' Does this license have decipher permissions?
    ''' </summary>
    Public ReadOnly Property Decipher() As Boolean
        Get
            Return GetString("/license/decipher") = "true"
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether the license is yet to come into effect
    ''' </summary>
    Public ReadOnly Property Started() As Boolean
        Get
            Return (StartDate <= DateTime.Today)
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether the license is close to taking effect (i.e. within 31 days)
    ''' </summary>
    ''' <value>True if close to taking effect, False otherwise</value>
    Public ReadOnly Property StartsSoon() As Boolean
        Get
            Return (Not Started AndAlso
                StartDate < DateTime.Today.AddDays(LicenseChanges.NumberOfDays))
        End Get
    End Property

    Public ReadOnly Property GracePeriodDays() As Integer
        Get
            Dim gracePeriodText = GetString("/license/graceperiod")
            Return If(gracePeriodText Is Nothing, 0, Integer.Parse(gracePeriodText))
        End Get
    End Property

    Public ReadOnly Property GracePeriodEndDate() As DateTime
        Get
            If GracePeriodDays() = 0 Then
                Return DateTime.MinValue
            Else
                Return Me.StartDate.AddDays(Me.GracePeriodDays())
            End If
        End Get
    End Property

    Public ReadOnly Property GracePeriodEndsSoon() As Boolean
        Get
            If (Me.GracePeriodDays() > 0 And Not Me.ExpiresSoon) Then
                Return (DateTime.UtcNow > Me.GracePeriodEndDate().AddDays(-LicenseChanges.GracePeriodNumberOfDays))
            End If

            Return False
        End Get
    End Property

    Public ReadOnly Property RequiresActivation() As Boolean
        Get
            Dim requiresActivationText = GetString("/license/activation")
            Return If(requiresActivationText Is Nothing, False, Boolean.Parse(requiresActivationText))
        End Get
    End Property

    Public ReadOnly Property IsWithinGracePeriod() As Boolean
        Get
            Return DateTime.UtcNow < Me.GracePeriodEndDate
        End Get
    End Property

    Public ReadOnly Property IsWithinStartAndEndDate() As Boolean
        Get
            Return Me.Started And Not Me.Expired
        End Get
    End Property

    ''' <summary>
    ''' Gets a status string describing this license
    ''' </summary>
    Public ReadOnly Property StatusLabel() As String
        Get
            If Not Started Then
                Return My.Resources.KeyInfo_Future
            ElseIf Expired Then
                Return My.Resources.KeyInfo_Expired
            ElseIf ExpiresSoon Then
                Return My.Resources.KeyInfo_ExpiresSoon
            ElseIf GracePeriodEndsSoon Then
                Return My.Resources.KeyInfo_GracePeriodEndsSoon
            ElseIf RequiresActivation And String.IsNullOrWhiteSpace(ActivationResponse) Then
                Return My.Resources.KeyInfo_RequiresActivation
            End If
            Return My.Resources.KeyInfo_Active
        End Get
    End Property

    ''' <summary>
    ''' Gets the <see cref="XmlNode.InnerText">InnerText</see> value of the XML node
    ''' found at the given path within the XML in this KeyInfo, or null if the path
    ''' did not describe an existing element in the XML.
    ''' </summary>
    ''' <param name="path">The path to the node for which the InnerText value is
    ''' required.</param>
    ''' <returns>The InnerText value at the given node or null if no node was found
    ''' at the specified path</returns>
    Private Function GetString(path As String) As String
        Dim node = LicenseData?.SelectSingleNode(path)
        Return If(node Is Nothing, Nothing, node.InnerText)
    End Function

    ''' <summary>
    ''' Gets the integer value in the <see cref="XmlNode.InnerText">InnerText</see>
    ''' value of the XML node found at the given path within the XML.
    ''' </summary>
    ''' <param name="path">The path to the node for which the InnerText value is
    ''' required.</param>
    ''' <param name="defaultValue">The default value to return if no node was found
    ''' at the given path</param>
    ''' <returns>The integer value found in the InnerText at the given node or
    ''' <paramref name="defaultValue"/> if no node was found at the specified path
    ''' </returns>
    Private Function GetInt(path As String, defaultValue As Integer) As Integer
        Dim str = GetString(path)
        Return If(str Is Nothing, defaultValue, CInt(str))
    End Function

    ''' <summary>
    ''' Gets the XML represented by the given key. If it is XML already, it is
    ''' returned unmolested. If if is base64-encoded, it is decoded first and the
    ''' underlying data is returned. No parsing or interpretation of the data is
    ''' performed in this method.
    ''' </summary>
    ''' <param name="key">The raw XML or base64-encoded XML to extract the XML from.
    ''' </param>
    ''' <returns>The XML represented by the given key</returns>
    Private Function GetKeyXml(key As String) As String
        If key Is Nothing Then Return Nothing
        If key.StartsWith("<") Then
            Return key
        Else
            Return Encoding.UTF8.GetString(Convert.FromBase64String(key))
        End If

    End Function

    ''' <summary>
    ''' Gets the label for the given value within this licence key.
    ''' </summary>
    ''' <param name="unlim">True if the value is unlimited</param>
    ''' <param name="value">The value currently set within the licence</param>
    ''' <returns>The label to display for the licence value.</returns>
    Friend Shared Function GetLabel(unlim As Boolean, value As Integer) As String
        If unlim Then Return My.Resources.KeyInfo_Unlimited
        Return CStr(value)
    End Function

    ''' <summary>
    ''' Verify that the XML document in this keyinfo is signed by a particular key.
    ''' </summary>
    ''' <remarks>Throws an exception if the document is not correctly signed</remarks>
    Friend Sub Verify()
        Dim signedXml As New SignedXml(Me.LicenseData) With {.Resolver = Nothing}
        Dim nodeList As XmlNodeList = Me.LicenseData.GetElementsByTagName("Signature")

        'There should be one and only one signature.
        If nodeList.Count <> 1 Then
            Throw New InvalidOperationException(My.Resources.KeyInfo_VerificationFailedSignatureCountInvalid)
        End If

        signedXml.LoadXml(CType(nodeList(0), XmlElement))
        If signedXml.CheckSignature(Licensing.LicensePublicKey) Then
            Return
        Else
            Throw New InvalidOperationException(
                My.Resources.KeyInfo_VerificationFailedSignatureIsNotValid)
        End If
    End Sub

    ''' <summary>
    ''' Get an Icon to override the product icon with, if the license specifies
    ''' one.
    ''' </summary>
    ''' <returns>The Base64-encoded icon to use, or Nothing if the license doesn't
    ''' override it.</returns>
    Public ReadOnly Property OverrideIcon() As String
        Get
            Dim n As XmlNode = LicenseData.SelectSingleNode("/license/branding/icon")
            If n Is Nothing Then Return Nothing
            Return n.InnerText
        End Get
    End Property

    ''' <summary>
    ''' Get an Image to override the large product logo with, if the license specifies
    ''' one.
    ''' </summary>
    ''' <returns>The Base64-encoded image to use, or Nothing if the license doesn't
    ''' override it.</returns>
    Public ReadOnly Property OverrideLargeLogo() As String
        Get
            Dim n As XmlNode = LicenseData.SelectSingleNode("/license/branding/largelogo")
            If n Is Nothing Then Return Nothing
            Return n.InnerText
        End Get
    End Property

    ''' <summary>
    ''' Get some text to override the product title bar with, if the license specifies
    ''' one.
    ''' </summary>
    ''' <returns>The text to use, or Nothing if the license doesn't override it.</returns>
    Public ReadOnly Property OverrideTitle() As String
        Get
            Dim n As XmlNode = LicenseData.SelectSingleNode("/license/branding/title")
            If n Is Nothing Then Return Nothing
            Return n.InnerText
        End Get
    End Property

    Public ReadOnly Property SalesOrderId() As String
        Get
            Dim n As XmlNode = LicenseData.SelectSingleNode("/license/salesorderid")
            If n Is Nothing Then Return Nothing
            Return n.InnerText
        End Get
    End Property

    Public ReadOnly Property LicenseRequestID() As String
        Get
            Dim n As XmlNode = LicenseData.SelectSingleNode("/license/licenserequestid")
            If n Is Nothing Then Return Nothing
            Return n.InnerText
        End Get
    End Property

    ''' <summary>
    ''' Return a basic string summary of the license, e.g. for auditing purposes.
    ''' </summary>
    ''' <returns>Basic summary</returns>
    Public Overrides Function ToString() As String
        Return String.Format(
            My.Resources.KeyInfo_Type0Owner1StartDate2ExpiryDate3,
            LicenseType, LicenseOwner, StartDate, ExpiryDate)
    End Function

    ''' <summary>
    ''' Checks if this key info object is equal to the given object. It is equal if
    ''' it is a non-null KeyInfo object with the same <see cref="Key"/> value as this
    ''' object. Note that the date/user id that the data was set is not taken into
    ''' account when checking for equality of the key.
    ''' </summary>
    ''' <param name="obj">The object to check against</param>
    ''' <returns>True if <paramref name="obj"/> is a KeyInfo object representing the
    ''' same key as this object.</returns>
    Public Overrides Function Equals(obj As Object) As Boolean
        Dim ki = TryCast(obj, KeyInfo)
        Return (ki IsNot Nothing AndAlso ki.Key = mKey)
    End Function

    ''' <summary>
    ''' Gets the hashcode representing this key info object. Note that this does not
    ''' take into account the <see cref="SetAt"/> or <see cref="SetBy"/> properties
    ''' of this object, being purely a function of the <see cref="Key"/>, in line
    ''' with the <see cref="Equals"/> implementation.
    ''' </summary>
    ''' <returns>An integer hash of the key that this object represents.</returns>
    Public Overrides Function GetHashCode() As Integer
        Return &HAEEEEEEE Xor If(mKey, "").GetHashCode()
    End Function

End Class

