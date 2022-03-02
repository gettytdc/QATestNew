Imports System.Xml

Imports BluePrism.BPCoreLib.Data
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateProcessCore
Imports System.Runtime.Serialization

''' <summary>
''' Component representing a visual business object.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class VBOComponent : Inherits ProcessComponent

#Region " Conflict Definitions "

    ''' <summary>
    ''' The conflict definitions which are handled by the business object component.
    ''' </summary>
    Private Class MyConflicts

        ''' <summary>
        ''' Conflict which occurs when a business object with the same ID as the
        ''' incoming ID is detected in the target environment.
        ''' Usually, this would overwrite the existing object with the new version
        ''' </summary>
        Public Shared ReadOnly Property VBOIdClash As ConflictDefinition
            Get
                If mVBOIdClash Is Nothing Then
                    mVBOIdClash = GenerateIdClashDefinition(PackageComponentType.BusinessObject)
                Else
                    mVBOIdClash.UpdateConflictDefinitionStrings(GenerateIdClashDefinition(PackageComponentType.BusinessObject))
                End If
                Return mVBOIdClash
            End Get
        End Property

        Private Shared mVBOIdClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when a process with the same ID as the incoming
        ''' ID is detected in the target environment.
        ''' </summary>
        Public Shared ReadOnly Property ProcessIDClash As ConflictDefinition
            Get
                If mProcessIDClash Is Nothing Then
                    mProcessIDClash = GenerateIdClashWithTypeDefinition(PackageComponentType.BusinessObject, PackageComponentType.Process)
                Else
                    mProcessIDClash.UpdateConflictDefinitionStrings(GenerateIdClashWithTypeDefinition(PackageComponentType.BusinessObject, PackageComponentType.Process))
                End If
                Return mProcessIDClash
            End Get
        End Property

        Private Shared mProcessIDClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when a business object <em>with a different ID</em>
        ''' but with the same name as the incoming business object is discovered in
        ''' the target environment.
        ''' Usually, this would result in a rename, either of the existing business
        ''' object or of the incoming one. Overwriting will cause the incoming
        ''' business object to assume the ID of the existing business object.
        ''' </summary>
        Public Shared ReadOnly Property VBONameClash As ConflictDefinition
            Get
                If mVBONameClash Is Nothing Then
                    mVBONameClash = GenerateNameClashDefinition(PackageComponentType.BusinessObject)
                Else
                    mVBONameClash.UpdateConflictDefinitionStrings(GenerateNameClashDefinition(PackageComponentType.BusinessObject))
                End If
                Return mVBONameClash
            End Get
        End Property

        Private Shared mVBONameClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when a process <em>with a different ID</em>, but
        ''' with the same name as the incoming object is detected in the target
        ''' environment.
        ''' </summary>
        Public Shared ReadOnly Property ProcessNameClash As ConflictDefinition
            Get
                If mProcessNameClash Is Nothing Then
                    mProcessNameClash = GenerateNameClashWithTypeDefinition(PackageComponentType.BusinessObject, PackageComponentType.Process)
                Else
                    mProcessNameClash.UpdateConflictDefinitionStrings(GenerateNameClashWithTypeDefinition(PackageComponentType.BusinessObject, PackageComponentType.Process))
                End If
                Return mProcessNameClash
            End Get
        End Property

        Private Shared mProcessNameClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when an object <em>with the same ID</em>, and
        ''' with the same name as the incoming object is detected in the target
        ''' environment.
        ''' </summary>
        Public Shared ReadOnly Property VBOID_VBONameClash As ConflictDefinition
            Get
                If mVBOID_VBONameClash Is Nothing Then
                    mVBOID_VBONameClash = GenerateIDClash_NameClashDefinition(PackageComponentType.BusinessObject)
                Else
                    mVBOID_VBONameClash.UpdateConflictDefinitionStrings(GenerateIDClash_NameClashDefinition(PackageComponentType.BusinessObject))
                End If
                Return mVBOID_VBONameClash
            End Get
        End Property

        Private Shared mVBOID_VBONameClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when an object <em>with the same ID</em>, and
        ''' a process with the same name as the incoming object is detected in the
        ''' target environment.
        ''' </summary>
        Public Shared ReadOnly Property VBOID_ProcessNameClash As ConflictDefinition
            Get
                If mVBOID_ProcessNameClash Is Nothing Then
                    mVBOID_ProcessNameClash = GenerateIDClash_NameClashWithType(PackageComponentType.BusinessObject, PackageComponentType.Process)
                Else
                    mVBOID_ProcessNameClash.UpdateConflictDefinitionStrings(GenerateIDClash_NameClashWithType(PackageComponentType.BusinessObject, PackageComponentType.Process))
                End If
                Return mVBOID_ProcessNameClash
            End Get
        End Property

        Private Shared mVBOID_ProcessNameClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when a process <em>with the same ID</em>, and
        ''' an object with the same name as the incoming object is detected in the
        ''' target environment.
        ''' </summary>
        Public Shared ReadOnly Property ProcessID_VBONameClash As ConflictDefinition
            Get
                If mProcessID_VBONameClash Is Nothing Then
                    mProcessID_VBONameClash = GenerateIDClashWithType_NameClash(PackageComponentType.BusinessObject, PackageComponentType.Process)
                Else
                    mProcessID_VBONameClash.UpdateConflictDefinitionStrings(GenerateIDClashWithType_NameClash(PackageComponentType.BusinessObject, PackageComponentType.Process))
                End If
                Return mProcessID_VBONameClash
            End Get
        End Property

        Private Shared mProcessID_VBONameClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when a process <em>with the same ID</em>, and
        ''' with the same name as the incoming object is detected in the target
        ''' environment.
        ''' </summary>
        Public Shared ReadOnly Property ProcessID_ProcessNameClash As ConflictDefinition
            Get
                If mProcessID_ProcessNameClash Is Nothing Then
                    mProcessID_ProcessNameClash = GenerateIDClashWithType_NameClashWithType(PackageComponentType.BusinessObject, PackageComponentType.Process)
                Else
                    mProcessID_ProcessNameClash.UpdateConflictDefinitionStrings(GenerateIDClashWithType_NameClashWithType(PackageComponentType.BusinessObject, PackageComponentType.Process))
                End If
                Return mProcessID_ProcessNameClash
            End Get
        End Property

        Private Shared mProcessID_ProcessNameClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when the incoming business object is set to be
        ''' retired, and the corresponding business object (ie. that with the same
        ''' ID) is not retired, or does not exist.
        ''' Usually, you would want all business objects in the two environments to
        ''' match up, hence 'Yes' being the recommended option.
        ''' </summary>
        Public Shared ReadOnly Property VBOToBeRetired As ConflictDefinition
            Get
                If mVBOToBeRetired Is Nothing Then
                    mVBOToBeRetired = GenerateToBeRetiredDefinition(PackageComponentType.BusinessObject)
                Else
                    mVBOToBeRetired.UpdateConflictDefinitionStrings(GenerateToBeRetiredDefinition(PackageComponentType.BusinessObject))
                End If
                Return mVBOToBeRetired
            End Get
        End Property

        Private Shared mVBOToBeRetired As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when the incoming business object is set to be
        ''' published, and the corresponding business object is not published, or
        ''' there is no corresponding business object.
        ''' Usually, you would want all business objects in the two environments to
        ''' match up, hence 'Yes' being the recommended option.
        ''' </summary>
        Public Shared ReadOnly Property VBOToBePublished As ConflictDefinition
            Get
                If mVBOToBePublished Is Nothing Then
                    mVBOToBePublished = GenerateToBePublishedDefinition(PackageComponentType.BusinessObject)
                Else
                    mVBOToBePublished.UpdateConflictDefinitionStrings(GenerateToBePublishedDefinition(PackageComponentType.BusinessObject))
                End If
                Return mVBOToBePublished
            End Get
        End Property

        Private Shared mVBOToBePublished As ConflictDefinition
    End Class

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new VBO component with data from the given provider.
    ''' This requires the same set of data as the corresponding constructor in
    ''' <see cref="ProcessComponent"/>
    ''' </summary>
    ''' <param name="prov">The provider of the data for this VBO. See the constructor
    ''' in <see cref="ProcessComponent"/> for the data required.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal prov As IDataProvider)
        MyBase.New(owner, prov)
    End Sub

    ''' <summary>
    ''' Creates a new VBO component based on the given process object.
    ''' </summary>
    ''' <param name="obj">The business object to represent as a component.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal obj As clsProcess)
        MyBase.New(owner, obj)
    End Sub

    ''' <summary>
    ''' Creates a new VBO component from the given properties.
    ''' </summary>
    ''' <param name="id">The ID of the VBO on the database.</param>
    ''' <param name="name">The name of the VBO</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal id As Guid, ByVal name As String)
        MyBase.New(owner, id, name)
    End Sub

    ''' <summary>
    ''' Creates a new business object component which draws its data from the given XML
    ''' reader.
    ''' </summary>
    ''' <param name="reader">The reader whence to draw the business object data.</param>
    ''' <param name="ctx">The context within which this component is being loaded.
    ''' </param>
    Public Sub New(ByVal owner As OwnerComponent, _
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Conflict Definition Properties "

    ''' <summary>
    ''' The single instance of an IdClash conflict definition for this object.
    ''' This is an ID clash with another VBO.
    ''' </summary>
    Protected Overrides ReadOnly Property IdClash() As ConflictDefinition
        Get
            Return MyConflicts.VBOIdClash
        End Get
    End Property

    ''' <summary>
    ''' The single instance of an IdClashWithOthertype conflict definition for this
    ''' object. This is an ID clash with a process.
    ''' </summary>
    Protected Overrides ReadOnly Property IdTypeClash() As ConflictDefinition
        Get
            Return MyConflicts.ProcessIDClash
        End Get
    End Property

    ''' <summary>
    ''' The single instance of an NameClash conflict definition for this object.
    ''' This is a name clash with another VBO.
    ''' </summary>
    Protected Overrides ReadOnly Property NameClash() As ConflictDefinition
        Get
            Return MyConflicts.VBONameClash
        End Get
    End Property

    ''' <summary>
    ''' The single instance of an NameClashWithOtherType conflict definition for this
    ''' object. This is a name clash with a process.
    ''' </summary>
    Protected Overrides ReadOnly Property NameTypeClash() As ConflictDefinition
        Get
            Return MyConflicts.ProcessNameClash
        End Get
    End Property

    Protected Overrides ReadOnly Property IdNameClash() As ConflictDefinition
        Get
            Return MyConflicts.VBOID_VBONameClash
        End Get
    End Property

    Protected Overrides ReadOnly Property IdNameTypeClash() As ConflictDefinition
        Get
            Return MyConflicts.VBOID_ProcessNameClash
        End Get
    End Property

    Protected Overrides ReadOnly Property IdTypeNameClash As ConflictDefinition
        Get
            Return MyConflicts.ProcessID_VBONameClash
        End Get
    End Property

    Protected Overrides ReadOnly Property IdTypeNameTypeClash As ConflictDefinition
        Get
            Return MyConflicts.ProcessID_ProcessNameClash
        End Get
    End Property

    ''' <summary>
    ''' The single instance of an ToBeRetired conflict definition for this object
    ''' </summary>
    Protected Overrides ReadOnly Property ToBeRetired() As ConflictDefinition
        Get
            Return MyConflicts.VBOToBeRetired
        End Get
    End Property

    ''' <summary>
    ''' The single instance of an ToBePublished conflict definition for this object
    ''' </summary>
    Protected Overrides ReadOnly Property ToBePublished() As ConflictDefinition
        Get
            Return MyConflicts.VBOToBePublished
        End Get
    End Property

#End Region

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public Overrides ReadOnly Property Type() As PackageComponentType
        Get
            Return PackageComponentType.BusinessObject
        End Get
    End Property

    ''' <summary>
    ''' The type that this component can clash with.
    ''' </summary>
    Public Overrides ReadOnly Property ClashType() As PackageComponentType
        Get
            Return PackageComponentType.Process
        End Get
    End Property

    ''' <summary>
    ''' The typekey for the namespace used by the output of this component. This
    ''' component uses an identical structure to <see cref="ProcessComponent"/>
    ''' so it can use the process's namespace.
    ''' </summary>
    Public Overrides ReadOnly Property NamespaceTypeKey() As String
        Get
            Return PackageComponentType.Process.Key
        End Get
    End Property

    ''' <summary>
    ''' Flag to indicate whether this component should always be in a group. Since
    ''' processes are always in a group, this must be overridden to indicate that
    ''' it is not always in a group.
    ''' </summary>
    Public Overrides ReadOnly Property AlwaysInGroup() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' The default group for this component, if it is not in a group. null for VBOs
    ''' </summary>
    Public Overrides ReadOnly Property DefaultGroup() As ComponentGroup
        Get
            Return Nothing
        End Get
    End Property

    ''' <summary>
    ''' Checks if this component represents a business object or not.
    ''' </summary>
    Public Overrides ReadOnly Property IsBusinessObject() As Boolean
        Get
            Return True
        End Get
    End Property

    ''' <summary>
    ''' Gets the name of the permission required by a user to import a component of
    ''' this type.
    ''' </summary>
    Public Overrides ReadOnly Property ImportPermission() As String
        Get
            Return Permission.ObjectStudio.ImportBusinessObject
        End Get
    End Property

End Class