Imports System.Runtime.Serialization
Imports BluePrism.Skills

<Serializable, DataContract([Namespace]:="bp")>
Public Class SkillDetails : Implements IObjectDetails

    <DataMember>
    Private ReadOnly mSkillId As String

    <DataMember>
    Private ReadOnly mWebApiName As String

    <DataMember>
    Private ReadOnly mSkillName As String

    <DataMember>
    Private ReadOnly mCategory As SkillCategory

    <DataMember>
    Private ReadOnly mImageBytes As Byte()

    Public Property SkillId As String Implements IObjectDetails.FriendlyName
        Get
            Return mSkillId
        End Get
        Set(value As String)
            Throw New NotImplementedException()
        End Set
    End Property

    Public ReadOnly Property WebApiName As String
        Get
            Return mWebApiName
        End Get
    End Property

    Public ReadOnly Property SkillName As String
        Get
            Return mSkillName
        End Get
    End Property

    Public ReadOnly Property Category As SkillCategory
        Get
            Return mCategory
        End Get
    End Property

    Public ReadOnly Property ImageBytes As Byte()
        Get
            Return mImageBytes
        End Get
    End Property

    Public Sub New(skill As Skill)

        mSkillId = skill.Id.ToString()
        mWebApiName = skill.GetWebApiName()
        mSkillName = skill.LatestVersion.Name
        mImageBytes = skill.LatestVersion.Icon
        mCategory = skill.LatestVersion.Category
    End Sub

    Public Sub New(id As String, webApiName As String, decryptedSkillName As String, decryptedSkillIcon As String, decryptedSkillCategory As String)
        ValidateParameter(id, NameOf(id))
        ValidateParameter(webApiName, NameOf(webApiName))
        ValidateParameter(decryptedSkillName, NameOf(decryptedSkillName))
        ValidateParameter(decryptedSkillIcon, NameOf(decryptedSkillIcon))
        ValidateParameter(decryptedSkillCategory, NameOf(decryptedSkillCategory))

        mSkillId = id
        mWebApiName = webApiName
        mSkillName = decryptedSkillName
        mImageBytes = Convert.FromBase64String(decryptedSkillIcon)
        mCategory = CType(decryptedSkillCategory, SkillCategory)
    End Sub

    Private Sub ValidateParameter(parameter As String, nameOfParameter As String)
        If String.IsNullOrWhiteSpace(parameter) Then
            Throw New ArgumentNullException(nameOfParameter)
        End If
    End Sub
End Class
