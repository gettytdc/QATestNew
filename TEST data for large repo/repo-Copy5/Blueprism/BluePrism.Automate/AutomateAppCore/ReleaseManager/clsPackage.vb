

Imports BluePrism.AutomateAppCore.Auth

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports System.Runtime.Serialization
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models


''' <summary>
''' Class to represent a package.
''' </summary>
<Serializable>
<DataContract([Namespace]:="bp", IsReference:=True), KnownType(GetType(List(Of clsRelease)))>
Public Class clsPackage : Inherits OwnerComponent

#Region " Class Scope Declarations "


    ''' <summary>
    ''' Gets a unique name for a new package - ie. one which does not occur in the
    ''' given collection of packages.
    ''' </summary>
    ''' <param name="coll">The collection of packages that the name should not occur
    ''' in</param>
    ''' <returns>A unique name for a new package.</returns>
    Public Shared Function GetUniqueName(ByVal coll As ICollection(Of clsPackage)) As String
        Dim fmt As String = My.Resources.clsPackage_NewPackage0
        For i As Integer = 1 To Integer.MaxValue
            Dim name As String = String.Format(fmt, i)
            If Not NameExists(name, coll) Then Return name
        Next
        Throw New OverflowException(My.Resources.clsPackage_RanOutOfPotentialPackageNames)
    End Function

#End Region

#Region " Private Members "

    ' The releases which have been made from this package
    ' Note that the property is <DataMember>'ed in order to maintain the coll type
    Private mReleases As ICollection(Of clsRelease)

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new package using data from the given data provider.
    ''' This constructor expects the following fields :- <list>
    ''' <item>id: Integer</item>
    ''' <item>name: String</item>
    ''' <item>created: Date</item>
    ''' <item>username: String</item>
    ''' <item>description: String</item></list>
    ''' </summary>
    ''' <param name="prov">The data provider for the data to use to populate this
    ''' package.</param>
    Public Sub New(ByVal prov As IDataProvider)
        Me.New(
         prov.GetString("name"), prov.GetValue("created", Date.UtcNow), prov.GetString("username"))
        Me.Id = prov.GetValue("id", 0)
        Me.Description = prov.GetString("description")

    End Sub

    ''' <summary>
    ''' Creates a new package with no name, using the current time and the current
    ''' logged in user as the creation metadata.
    ''' </summary>
    Public Sub New()
        Me.New("", DateTime.UtcNow, User.CurrentName)
    End Sub

    ''' <summary>
    ''' Creates a new package with no name or the default adhoc package name if the
    ''' given flag indicates an adhoc package. It sets the current time and the
    ''' currently logged in user as the creation metadata.
    ''' </summary>
    ''' <param name="adhoc">True to indicate that this is an adhoc package. An ID
    ''' will be generated indicating an adhoc package.</param>
    Public Sub New(ByVal adhoc As Boolean)
        Me.New(CStr(IIf(adhoc, "<Adhoc Package>", "")), DateTime.UtcNow, User.Current.Name)
        If adhoc Then Me.Id = -1 ' Negative ID indicates adhoc package
    End Sub

    ''' <summary>
    ''' Creates a new package with the given name, using the current time and the
    ''' current logged in user as the creation metadata.
    ''' </summary>
    ''' <param name="name">The name of the package.</param>
    Public Sub New(ByVal name As String)
        Me.New(name, DateTime.UtcNow, User.CurrentName)
    End Sub

    ''' <summary>
    ''' Creates a new package with the given name and creation metadata.
    ''' </summary>
    ''' <param name="name">The name of the package.</param>
    ''' <param name="createdDate">The date/time that this package was created. For
    ''' consistency, this should be in UTC.</param>
    ''' <param name="username">The username of the user which created this package.
    ''' </param>
    Public Sub New(ByVal name As String, ByVal createdDate As Date, ByVal username As String)
        Me.New(name, createdDate, username, Nothing, Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new package with the given properties.
    ''' </summary>
    ''' <param name="name">The name of the package.</param>
    ''' <param name="createdDate">The date/time that this package was created. For
    ''' consistency, this should be in UTC.</param>
    ''' <param name="username">The username of the user which created this package.
    ''' </param>
    ''' <param name="comps">The components which make up the contents of this
    ''' package.</param>
    ''' <param name="rels">The releases which have been made from this package.
    ''' </param>
    Public Sub New(ByVal name As String, ByVal createdDate As Date, ByVal username As String,
     ByVal comps As ICollection(Of PackageComponent), ByVal rels As ICollection(Of clsRelease))

        MyBase.New(Nothing, name, createdDate, username)

        AddAll(comps)
        AddReleases(rels)

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public Overrides ReadOnly Property Type() As PackageComponentType
        Get
            Return PackageComponentType.Package
        End Get
    End Property

    ''' <summary>
    ''' Flag indicating if this package is an adhoc package (ie. created on the fly
    ''' by an export process action) or not.
    ''' </summary>
    Public ReadOnly Property IsAdHoc() As Boolean
        Get
            Return (IdAsInteger < 0)
        End Get
    End Property

    ''' <summary>
    ''' The collection of releases which have been made from this (version of the)
    ''' package.
    ''' </summary>
    <DataMember>
    Public Property Releases() As ICollection(Of clsRelease)
        Get
            If mReleases Is Nothing Then mReleases = New List(Of clsRelease)
            Return mReleases
        End Get
        Private Set(value As ICollection(Of clsRelease))
            mReleases = New List(Of clsRelease)(
                If(value, GetEmpty.ICollection(Of clsRelease))
            )
        End Set
    End Property

    ''' <summary>
    ''' The latest release of this package.
    ''' </summary>
    Public ReadOnly Property LatestRelease() As clsRelease
        Get
            If mReleases Is Nothing Then Return Nothing

            Dim latest As clsRelease = Nothing
            For Each rel As clsRelease In mReleases
                If latest Is Nothing OrElse rel.Created > latest.Created Then latest = rel
            Next
            Return latest
        End Get
    End Property

    ''' <summary>
    ''' A suggested (unique) release name for this package
    ''' </summary>
    Public ReadOnly Property SuggestedReleaseName() As String
        Get
            Return GetUniqueReleaseName()
        End Get
    End Property

    <DataMember>
    Public Property ContainsInaccessibleItems As Boolean = False

#End Region

#Region " Public Methods "

    ''' <summary>
    ''' A package cannot have an owner, so ensure that it doesn't get one via this
    ''' property.
    ''' </summary>
    ''' <exception cref="BluePrismException">If an attempt is made to set the owner
    ''' on this package.</exception>
    Public Overrides Property Owner() As OwnerComponent
        Get
            Return Nothing
        End Get
        Set(ByVal value As OwnerComponent)
            Throw New BluePrismException(
             My.Resources.clsPackage_APackageCannotHaveAnOwnerAttemptedToSetItTo0, value)
        End Set
    End Property

    ''' <summary>
    ''' Adds the given releases to this package.
    ''' This ensures that all releases are marked as being borned from this package.
    ''' </summary>
    ''' <param name="rels">The releases to be associated with this version of the 
    ''' package.</param>
    Public Sub AddReleases(ByVal rels As IEnumerable(Of clsRelease))
        If rels Is Nothing Then Return
        With Me.Releases
            For Each r As clsRelease In rels
                .Add(r)
                r.Package = Me
            Next
        End With
    End Sub

    ''' <summary>
    ''' Checks if this package contains a release with the given name.
    ''' </summary>
    ''' <param name="name">The name to check within this package's releases for.
    ''' </param>
    ''' <returns>True if this package has a release with the given name; False
    ''' otherwise.</returns>
    Public Function ContainsReleaseNamed(ByVal name As String) As Boolean
        For Each rel As clsRelease In Me.Releases
            If rel.Name = name Then Return True
        Next
        Return False
    End Function

#Region " Create Release "

    ''' <summary>
    ''' Creates a release with a unique name, and populates its contents, adding
    ''' it to this package and returning it.
    ''' </summary>
    ''' <returns>The new release generated from this package.</returns>
    Public Function CreateRelease() As clsRelease
        Return CreateRelease(GetUniqueReleaseName(), True)
    End Function

    ''' <summary>
    ''' Creates a release with the specified name and populates its contents, adding
    ''' it to this package and returning it.
    ''' </summary>
    ''' <param name="name">The name of the release to create.</param>
    ''' <returns>The required release.</returns>
    Public Function CreateRelease(ByVal name As String) As clsRelease
        Return CreateRelease(name, True)
    End Function

    ''' <summary>
    ''' Creates a release with the specified name and populates its contents, adding
    ''' it to this package as specified and returning it.
    ''' </summary>
    ''' <param name="name">The name of the release to create.</param>
    ''' <param name="autoAddToPackage">True to automatically add the generated
    ''' release to this package, false otherwise.</param>
    ''' <returns>The newly generated release.</returns>
    ''' <exception cref="NameAlreadyExistsException">If a release with the given name
    ''' already exists in this package.</exception>
    Public Function CreateRelease(
     ByVal name As String, ByVal autoAddToPackage As Boolean) As clsRelease
        For Each currRel As clsRelease In Releases
            If currRel.Name = name Then Throw New NameAlreadyExistsException(
             My.Resources.clsPackage_AReleaseWithTheName0AlreadyExistsForThisPackage, name)
        Next

        Dim rel As New clsRelease(Me, name, True)
        If autoAddToPackage Then Releases.Add(rel)
        Return rel
    End Function

#End Region

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Gets a unique release name for this package.
    ''' </summary>
    ''' <returns>A release name which doesn't clash with other releases in
    ''' this package.</returns>
    Private Function GetUniqueReleaseName() As String

        If mReleases Is Nothing OrElse mReleases.Count = 0 Then Return String.Format(My.Resources.clsPackage_0Release1, Me.Name)

        ' Set up the format string.
        Dim str As String = Me.Name
        If str.IndexOfAny("{}".ToCharArray()) >= 0 Then
            str = str.Replace("{", "{{").Replace("}", "}}")
        End If

        ' Go through to int.MaxValue to find a unique name
        For i As Integer = 1 To Integer.MaxValue
            Dim candidate As String = String.Format(My.Resources.clsPackage_0Releasex1, str, i)
            For Each rel As clsRelease In mReleases
                If rel.Name = candidate Then ' Can't use this name - nullify it and break
                    candidate = Nothing
                    Exit For
                End If
            Next
            ' If we made it through the release check, we have a winner
            If candidate IsNot Nothing Then Return candidate
        Next
        Throw New OverflowException(My.Resources.clsPackage_CouldnTFindAUniqueReleaseName)

    End Function

#End Region

End Class

