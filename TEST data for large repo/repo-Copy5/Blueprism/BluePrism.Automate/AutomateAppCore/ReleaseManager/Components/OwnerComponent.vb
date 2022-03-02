Imports System.Runtime.Serialization

''' <summary>
''' An owner of other components - eg. a package or a release. Differs from a
''' component group due to increased responsibility for its members - it can usually
''' hold members of different types and user / creation data is stored along with it
''' </summary>
<Serializable, DataContract(IsReference:=True, [Namespace]:="bp"), KnownType("GetOwnerKnownTypes")>
Public MustInherit Class OwnerComponent : Inherits ComponentGroup

#Region " Class-scope Declarations "

    ''' <summary>
    ''' Gets all concrete group member types in this assembly
    ''' </summary>
    ''' <returns>An enumerable of all group member types found in this assembly
    ''' </returns>
    Protected Shared Function GetOwnerKnownTypes() As IEnumerable(Of Type)
        Return GetAllKnownTypes()
    End Function

#End Region

#Region " Member Variables "

    ' The creation date for this group component.
    <DataMember>
    Private mCreated As Date

    ' The user who created this component.
    <DataMember>
    Private mUser As String

    ' Cached user ID - retrieved lazily as required, based on the user name.
    <DataMember>
    Private mUserId As Guid

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new group component with the given name and ID, setting the
    ''' creation info to the specified date and username.
    ''' </summary>
    ''' <param name="id">The ID of the component</param>
    ''' <param name="name">The name of the component</param>
    ''' <param name="createdDate">The date of creation</param>
    ''' <param name="username">The user who created this component</param>
    Public Sub New(ByVal id As Object, ByVal name As String, _
     ByVal createdDate As Date, ByVal username As String)
        MyBase.New(Nothing, id, name)
        mCreated = createdDate
        mUser = username
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' The date that this component was created.
    ''' </summary>
    Public Property Created() As Date
        Get
            Return mCreated
        End Get
        Protected Set(ByVal value As Date)
            mCreated = value
        End Set
    End Property

    ''' <summary>
    ''' The created date in a format suitable for a list.
    ''' </summary>
    Public ReadOnly Property CreatedListFormat() As String
        Get
            Return mCreated.ToLocalTime().ToString()
        End Get
    End Property

    ''' <summary>
    ''' The created date in a format suitable for (long) display.
    ''' </summary>
    Public ReadOnly Property CreatedDisplayFormat(DisplayDateFormat As String) As String
        Get
            Return mCreated.ToLocalTime().ToString(DisplayDateFormat)
        End Get
    End Property

    ''' <summary>
    ''' The username representing the user who created this component.
    ''' </summary>
    Public Property UserName() As String
        Get
            Return mUser
        End Get
        Protected Set(ByVal value As String)
            If mUser = value Then Return
            mUser = value
            ' "Uncache" the user ID to ensure the correct one is returned when
            ' requested again.
            mUserId = Nothing
        End Set
    End Property

    ''' <summary>
    ''' The user ID representing the user who created this component.
    ''' </summary>
    Public ReadOnly Property UserId() As Guid
        Get
            If mUserId = Nothing Then mUserId = gSv.TryGetUserID(mUser)
            Return mUserId
        End Get
    End Property

    ''' <summary>
    ''' The type of this component type.
    ''' <seealso cref="PackageComponentType"/>
    ''' </summary>
    Public MustOverride Overrides ReadOnly Property Type() As PackageComponentType

#End Region

End Class
