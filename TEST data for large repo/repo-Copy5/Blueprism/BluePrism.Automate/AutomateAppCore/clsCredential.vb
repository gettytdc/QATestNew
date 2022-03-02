Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore.WebApis.Credentials
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Common.Security
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Class representing a Credential Manager Credential.
''' </summary>
<Serializable()>
<DataContract([Namespace]:="bp")>
<KnownType(GetType(clsSet(Of Guid)))>
Public Class clsCredential : Implements ICredential

    ' Credential statuses
    Public Enum Status
        All
        Valid
        Invalid
        Expired
    End Enum

    ' The IDs representing the processes which can access this credential
    <DataMember>
    Private mProcesses As IBPSet(Of Guid)

    ' The IDs representing the resources which can access this credential
    <DataMember>
    Private mResources As IBPSet(Of Guid)

    ' The user roles associated with this credential
    <DataMember>
    Private mRoles As List(Of Role)

    ' The properties associated with this credential
    <DataMember>
    Private mProperties As IDictionary(Of String, SafeString)

    ' The description of this credential
    <DataMember>
    Private mDescription As String

    ' The username for this credential
    <DataMember>
    Private mUsername As String

    ' The password for this credential
    <DataMember>
    Private mPassword As SafeString

    ''' <summary>
    ''' Field used to support serialization of Type property
    ''' </summary>
    <DataMember>
    Private mTypeName As String

    <NonSerialized>
    Private mType As CredentialType = Nothing

    <DataMember>
    Public ID As Guid

    <DataMember>
    Private mName As String

    <DataMember>
    Public ExpiryDate As DateTime

    <DataMember>
    Public IsInvalid As Boolean

    <DataMember>
    Public EncryptionKeyID As Integer

    ''' <summary>
    ''' The non-null name used to identify this credential
    ''' </summary>
    Public Property Name As String Implements ICredential.Name
        Get
            If mName Is Nothing Then Return "" Else Return mName
        End Get
        Set(ByVal value As String)
            mName = value
        End Set
    End Property
    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' The non-null user name for this credential.
    ''' </summary>
    Public Property Username() As String Implements ICredential.Username
        Get
            If mUsername Is Nothing Then Return "" Else Return mUsername
        End Get
        Set(ByVal value As String)
            mUsername = value
        End Set
    End Property

    ''' <summary>
    ''' The password for this credential
    ''' </summary>
    Public Property Password() As SafeString Implements ICredential.Password
        Get
            If mPassword Is Nothing Then Return New SafeString() Else Return mPassword
        End Get
        Set(ByVal value As SafeString)
            mPassword = value
        End Set
    End Property

    ''' <summary>
    ''' The current status of this credential
    ''' </summary>
    Public ReadOnly Property CurrentStatus() As Status
        Get
            If IsInvalid Then
                Return Status.Invalid
            ElseIf ExpiryDate <> Date.MinValue AndAlso ExpiryDate < Date.Today Then
                Return Status.Expired
            Else
                Return Status.Valid
            End If
        End Get
    End Property

    ''' <summary>
    ''' The non-null description for this credential
    ''' </summary>
    Public Property Description() As String
        Get
            If mDescription Is Nothing Then Return "" Else Return mDescription
        End Get
        Set(ByVal value As String)
            mDescription = value
        End Set
    End Property

    ''' <summary>
    ''' The set of process IDs which have access to this credential.
    ''' </summary>
    Public Property ProcessIDs() As IBPSet(Of Guid)
        Get
            If mProcesses Is Nothing Then mProcesses = New clsSet(Of Guid)
            Return mProcesses
        End Get
        Set(ByVal value As IBPSet(Of Guid))
            mProcesses = value
        End Set
    End Property

    ''' <summary>
    ''' The set of resource IDs which have access to this credential
    ''' </summary>
    Public Property ResourceIDs() As IBPSet(Of Guid)
        Get
            If mResources Is Nothing Then mResources = New clsSet(Of Guid)
            Return mResources
        End Get
        Set(ByVal value As IBPSet(Of Guid))
            mResources = value
        End Set
    End Property

    ''' <summary>
    ''' The roles allowed to access this credential. A collection with a single
    ''' null entry is treated as being accessible to all roles (or none).
    ''' </summary>
    Public ReadOnly Property Roles() As ICollection(Of Role)
        Get
            If mRoles Is Nothing Then mRoles = New List(Of Role)
            Return mRoles
        End Get
    End Property

    ''' <summary>
    ''' The set of propertes for this credential
    ''' </summary>
    Public Property Properties() As IDictionary(Of String, SafeString) Implements ICredential.Properties
        Get
            If mProperties Is Nothing Then mProperties = New Dictionary(Of String, SafeString)
            Return mProperties
        End Get
        Set(ByVal value As IDictionary(Of String, SafeString))
            mProperties = value
        End Set
    End Property

    ''' <summary>
    ''' The type of the credential.
    ''' </summary>
    ''' <returns>The type</returns>
    Public Property Type As CredentialType
        Get
            If mType Is Nothing Then
                mTypeName = If(mTypeName, CredentialType.General.Name)
                mType = CredentialType.GetByName(mTypeName)
            End If
            Return mType
        End Get
        Set
            If value Is Nothing Then _
                Throw New ArgumentNullException(NameOf(value))
            mTypeName = value.Name
            mType = value
        End Set
    End Property

    ''' <summary>
    ''' Gets whether this credential is accessible to all roles or not.
    ''' </summary>
    Public ReadOnly Property IsForAllRoles() As Boolean
        Get
            Return (Roles.Count = 1 AndAlso CollectionUtil.First(Roles) Is Nothing)
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating if this credential is allowed to be accessed from all
    ''' processes
    ''' </summary>
    Public ReadOnly Property IsForAllProcesses() As Boolean
        Get
            Return (ProcessIDs.Count = 1 _
             AndAlso CollectionUtil.First(ProcessIDs) = Guid.Empty)
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating if this credential is allowed to be accessed from all
    ''' resources
    ''' </summary>
    Public ReadOnly Property IsForAllResources() As Boolean
        Get
            Return (ResourceIDs.Count = 1 _
             AndAlso CollectionUtil.First(ResourceIDs) = Guid.Empty)
        End Get
    End Property

    ''' <summary>
    ''' Function to generate a random password using the supplied criteria.
    ''' </summary>
    ''' <param name="length">The length of the resulting password (min 1, max 20)
    ''' </param>
    ''' <param name="useUpper">Include uppercase letters</param>
    ''' <param name="useLower">Include lowercase letters</param>
    ''' <param name="useNumeric">Include numbers</param>
    ''' <param name="extras">Additional characters to include</param>
    ''' <returns>The generated password</returns>
    ''' <exception cref="InvalidArgumentException">If the <paramref name="length"/>
    ''' parameter is not between 1 and 20, inclusive</exception>
    ''' <exception cref="EmptyException">If none of <paramref name="useUpper"/>,
    ''' <paramref name="useLower"/>, <paramref name="useNumeric"/> or
    ''' <paramref name="extras"/> were set, meaning that the generating had an empty
    ''' character set to choose chars from.</exception>
    ''' <exception cref="DuplicateException">If duplicate characters were detected in
    ''' the <paramref name="extras">additional characters</paramref>.</exception>
    Public Shared Function GeneratePassword(
     ByVal length As Integer, ByVal useUpper As Boolean,
     ByVal useLower As Boolean, ByVal useNumeric As Boolean,
     ByVal extras As String) As SafeString
        ' Random number generator
        Using random As New CryptoRandom()

            ' Validate passed input parameters
            If length <= 0 Then Throw New InvalidArgumentException(
             My.Resources.clsCredential_InvalidLength0ItCannotBeZeroOrLess, length)

            If length > 65536 Then Throw New InvalidArgumentException(
             My.Resources.clsCredential_InvalidLength0ItCannotBeGreaterThanMaxLength, length)

            Dim extraChars As New clsSet(Of Char)
            Dim dupes As New clsSet(Of Char)
            For Each c As Char In extras
                If Not extraChars.Add(c) Then dupes.Add(c)
            Next
            If dupes.Count > 0 Then Throw New DuplicateException(
             My.Resources.clsCredential_AdditionalCharsMustBeUniqueFoundDuplicatesOf0,
             CollectionUtil.Join(dupes, ", "))

            ' Build up possible chars to generate password from
            Dim charsets As New List(Of String)
            If useUpper Then charsets.Add("ABCDEFGHIJKLMNOPQRSTUVWXYZ")
            If useLower Then charsets.Add("abcdefghijklmnopqrstuvwxyz")
            If useNumeric Then charsets.Add("0123456789")
            If extras <> "" Then charsets.Add(extras)

            If charsets.Count = 0 Then Throw New EmptyException(
             My.Resources.clsCredential_GivenCriteriaResultsInNoCharacterSets)

            ' Shuffle chars to create password of specified length
            Dim password As New SafeString()
            For i As Integer = 0 To length - 1
                Dim j As Integer = i Mod charsets.Count
                If j = 0 Then Shuffle(random, charsets)
                Dim r As Integer = random.Next(0, charsets(j).Length)
                password.AppendChar(charsets(j).Chars(r))
            Next
            Return password
        End Using
    End Function

    ''' <summary>
    ''' Shuffles a list of strings
    ''' </summary>
    ''' <param name="list">A list of strings</param>
    Private Shared Sub Shuffle(ByVal random As CryptoRandom, ByVal list As List(Of String))
        Dim n As Integer = list.Count
        While (n > 1)
            Dim k As Integer = random.Next(n)
            n -= 1
            Dim temp As String = list(n)
            list(n) = list(k)
            list(k) = temp
        End While
    End Sub

    ''' <summary>
    ''' Gets the user-facing status for this Credential for the current culture
    ''' </summary>
    Public ReadOnly Property GetLocalisedFriendlyName() As String
        Get
            Return CredentialsResources.ResourceManager.
                GetString($"CredentialTypes_{CurrentStatus().ToString()}_Status")
        End Get
    End Property
End Class
