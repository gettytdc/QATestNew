Imports System.Xml
Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib.Data

<Serializable, DataContract([Namespace]:="bp")>
Public Class SkillComponent : Inherits PackageComponent

    Public Overrides ReadOnly Property Type As PackageComponentType
        Get
            Return PackageComponentType.Skill
        End Get
    End Property

    Public Overrides ReadOnly Property ImportPermission() As String
        Get
            Return Permission.Skills.ImportSkill
        End Get
    End Property


    Public Sub New(owner As OwnerComponent, prov As IDataProvider)
        Me.New(owner, prov.GetValue("id", Guid.Empty), prov.GetString("name"))
    End Sub

    Public Sub New(owner As OwnerComponent, id As Guid, name As String)
        MyBase.New(owner, id, name)
    End Sub

    Public Sub New(owner As OwnerComponent, reader As XmlReader, ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

    Protected Overrides Sub ReadXmlBody(r As XmlReader, ctx As IComponentLoadingContext)
        r.Read()
        AssociatedData = r.Value
    End Sub

End Class
