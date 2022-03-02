Imports System.Globalization
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore.Auth
Imports LocaleTools

Namespace Groups

    ''' <summary>
    ''' Attribute which can define a tree used in grouping items together in the
    ''' product
    ''' </summary>
    Public Class TreeDefinitionAttribute : Inherits Attribute

#Region " Class-scope Declarations "

        ' An empty attribute to use as a placeholder
        Friend Shared ReadOnly Empty As New TreeDefinitionAttribute(Nothing, Nothing, Nothing)

        ' A map of tree definition attributes to group tree types, so we don't need
        ' to reflect them every time we want to build them
        Private Shared ReadOnly TreeDefnMap _
            As IDictionary(Of GroupTreeType, TreeDefinitionAttribute)

        ''' <summary>
        ''' Sets up the mapping of tree definition attributes to tree types, so that
        ''' we can just use them in the future.
        ''' </summary>
        Shared Sub New()
            Dim map As New Dictionary(Of GroupTreeType, TreeDefinitionAttribute)
            For Each val As GroupTreeType In [Enum].GetValues(GetType(GroupTreeType))
                map(val) = BPUtil.GetAttributeValue(Of TreeDefinitionAttribute)(val)
            Next
            TreeDefnMap = GetReadOnly.IDictionary(map)
        End Sub

        ''' <summary>
        ''' Gets the tree definition attribute for a specified enum value.
        ''' </summary>
        ''' <param name="enumValue">The value for which the tree definition attribute
        ''' is required.</param>
        ''' <returns>The tree definition attribute corresponding to the given enum
        ''' value or null if it has no such attribute associated with it.</returns>
        Public Shared Function GetAttributeFor(enumValue As GroupTreeType) _
         As TreeDefinitionAttribute
            Dim attr As TreeDefinitionAttribute = Nothing
            TreeDefnMap.TryGetValue(enumValue, attr)
            Return attr
        End Function

        ''' <summary>
        ''' Gets the plural name in the tree definition attribute associated with
        ''' the given enum value, or an empty string if there is no such attribute
        ''' </summary>
        ''' <param name="enumValue">The value for which the plural name is required.
        ''' </param>
        ''' <returns>The plural name defined on the tree definition attribute
        ''' associated with the given enum value, or an empty string if the attribute
        ''' has no plural name defined, or if the enum value has no such attribute
        ''' associated with it.</returns>
        Public Shared Function GetPluralNameFor(enumValue As GroupTreeType) As String
            Return If(GetAttributeFor(enumValue), Empty).PluralName
        End Function

        ''' <summary>
        ''' Gets the singular name in the tree definition attribute associated with
        ''' the given enum value, or an empty string if there is no such attribute
        ''' </summary>
        ''' <param name="enumValue">The value for which the singular name is required
        ''' </param>
        ''' <returns>The singular name defined on the tree definition attribute
        ''' associated with the given enum value, or an empty string if the attribute
        ''' has no singular name defined, or if the enum value has no such attribute
        ''' associated with it.</returns>
        Public Shared Function GetSingularNameFor(enumValue As GroupTreeType) As String
            Return If(GetAttributeFor(enumValue), Empty).SingularName
        End Function

        ''' <summary>
        ''' Gets the image key in the tree definition attribute associated with the
        ''' given enum value, or an empty string if there is no such attribute or if
        ''' the image key is not specified in the attribute.
        ''' </summary>
        ''' <param name="enumValue">The value for which the image key is required.
        ''' </param>
        ''' <returns>The image key corresponding to the given enum value, or an empty
        ''' string if the value had no corresponding tree definition attribute, or
        ''' the attribute had no image key set in it.</returns>
        Public Shared Function GetImageKeyFor(enumValue As GroupTreeType) As String
            Return If(GetAttributeFor(enumValue), Empty).ImageKey
        End Function

        ''' <summary>
        ''' Gets the supported group member types defined in the tree definition
        ''' attribute associated with the given enum value, or an empty collection if
        ''' there is no such attribute
        ''' </summary>
        ''' <param name="enumValue">The value for which the supported group member
        ''' types are required.</param>
        ''' <returns>The supported group member types defined on the tree definition
        ''' attribute associated with the given enum value, or an empty collection if
        ''' the attribute has no group member types defined, or if the enum value has
        ''' no such attribute associated with it.</returns>
        Public Shared Function GetSupportedTypesFor(enumValue As GroupTreeType) _
         As ICollection(Of GroupMemberType)
            Return If(GetAttributeFor(enumValue), Empty).SupportedMemberTypes
        End Function

        ''' <summary>
        ''' Gets the localized friendly name for attribute according To the current culture.
        ''' The resources are created from GroupTreeType.vb
        ''' </summary>
        ''' <param name="attribute">The attribute string</param>
        ''' <param name="isNoun">Should the attribute be treated as a noun</param>
        ''' <returns>The localised attribute string for the current culture</returns>
        Public Shared Function GetLocalizedFriendlyName(attribute As String, isNoun As Boolean) As String
            Dim res = GetLocalizedFriendlyName(attribute)

            If res IsNot Nothing AndAlso isNoun AndAlso CultureInfo.CurrentCulture().Name <> "de-DE" Then
                res = res.ToLowerInvariant
            End If

            Return CStr(IIf(res Is Nothing, attribute, res))
        End Function

        ''' <summary>
        ''' Gets the localized friendly name for attribute according To the current culture.
        ''' The resources are created from GroupTreeType.vb
        ''' </summary>
        ''' <param name="attribute">The attribute string</param>
        ''' <returns>The localised attribute string for the current culture</returns>
        Public Shared Function GetLocalizedFriendlyName(attribute As String) As String
            Dim resxKey = "TreeDefinitionAttribute_" & attribute
            Dim res As String = My.Resources.ResourceManager.GetString($"{resxKey}")
            Return CStr(IIf(res Is Nothing, attribute, res))
        End Function

        ''' <summary>
        ''' Gets the localized name for group member according To the current culture.
        ''' </summary>
        ''' <returns>The localised attribute name for the group member</returns>
        Public Function GetLocalizedNameForGroupMember(groupMemberName As String) As String
            Try
                Return LTools.GetC(groupMemberName, mSingleName.ToLower())
            Catch ex As Exception
                Return groupMemberName
            End Try
           
        End Function

#End Region

#Region " Member Variables "

        ' The name to use for a single item in the tree
        Private mSingleName As String

        ' The name to use for a number of items in the tree. Also used as the
        ' name of the root node of the tree
        Private mPluralName As String

        Private mGroupName As String

        ' The types supported by the tree (subgroups are automatically supported
        ' in all trees)
        Private mSupportedTypes As IBPSet(Of GroupMemberType)

        ' The image key for the root element of this tree
        Private mImageKey As String

        ' The permission needed to edit the groups in the tree
        Private mEditPermissionName As String

        ' The permission needed to manage access rights in the tree
        Private mAccessRightsPermission As String

        ' The permission needed to create items in the tree
        Private mCreateItemPermission As String

        ' The permission needed to delete items in the tree
        Private mDeleteItemPermission As String

        ' The permission needed to export items in the tree
        Private mExportPermission As String


#End Region

#Region " Constructors "

        ''' <summary>
        ''' Creates a new TreeDefinition attribute
        ''' </summary>
        ''' <param name="singular">The name used to reference a single (non-group)
        ''' item in the tree.</param>
        ''' <param name="plural">The name used to reference multiple (non-group)
        ''' items in the tree. Also used as the name of the root of the tree.</param>
        ''' <param name="memberTypes">The member types supported by this tree.
        ''' Subgroups are automatically supported by all trees, so do not need to be
        ''' included in this list</param>
        Public Sub New(singular As String,
                       plural As String,
                       groupName As String,
                       ParamArray memberTypes() As GroupMemberType)
            Me.New(singular, plural, groupName, Nothing, memberTypes)
        End Sub

        ''' <summary>
        ''' Creates a new TreeDefinition attribute.
        ''' </summary>
        ''' <param name="singular">The name used to reference a single (non-group)
        ''' item in the tree.</param>
        ''' <param name="plural">The name used to reference multiple (non-group)
        ''' items in the tree. Also used as the name of the root of the tree.</param>
        ''' <param name="imgKey">The image key to use for the root element of this
        ''' tree.</param>
        ''' <param name="memberTypes">The member types supported by this tree.
        ''' Subgroups are automatically supported by all trees, so do not need to be
        ''' included in this list</param>
        Public Sub New(singular As String,
                       plural As String,
                       groupName As String,
                       imgKey As String,
                       ParamArray memberTypes() As GroupMemberType)
            Me.New(singular, plural, groupName, imgKey, "", "", "", "", "", memberTypes)
        End Sub

        ''' <summary>
        ''' Creates a new TreeDefinition attribute.
        ''' </summary>
        ''' <param name="singular">The name used to reference a single (non-group)
        ''' item in the tree.</param>
        ''' <param name="plural">The name used to reference multiple (non-group)
        ''' items in the tree. Also used as the name of the root of the tree.</param>
        ''' <param name="imgKey">The image key to use for the root element of this
        ''' tree.</param>
        ''' <param name="editPerm">Specifies which
        ''' permission a user needs to edit the tree.</param>
        ''' <param name="rightsPerm">Specifies which permission a user needs to
        ''' manage access rights within the tree</param>
        ''' <param name="createPerm">Specifies which permission a user needs to
        ''' create items in the tree</param>
        ''' <param name="deletePerm">Specifies which permission a user needs to
        ''' delete items in the tree</param>
        ''' <param name="memberTypes">The member types supported by this tree.
        ''' Subgroups are automatically supported by all trees, so do not need to be
        ''' included in this list</param>
        Public Sub New(singular As String,
                       plural As String,
                       groupName As String,
                       imgKey As String,
                       editPerm As String,
                       rightsPerm As String,
                       createPerm As String,
                       deletePerm As String,
                       exportPerm As String,
                       ParamArray memberTypes() As GroupMemberType)

            mSingleName = singular
            mPluralName = plural
            mGroupName = groupName
            mImageKey = imgKey
            mEditPermissionName = editPerm
            mAccessRightsPermission = rightsPerm
            mCreateItemPermission = createPerm
            mDeleteItemPermission = deletePerm
            mExportPermission = exportPerm
            ' Create a set of group member types and ensure that 'Group' is included
            mSupportedTypes = New clsOrderedSet(Of GroupMemberType)()
            mSupportedTypes.Add(GroupMemberType.Group)
            mSupportedTypes.Union(memberTypes)

            ' We only include 'Group' members if this definition supports *any*
            ' other type - ie. if the only supported type is Group, we clear it.
            If mSupportedTypes.Count = 1 Then mSupportedTypes.Clear()

        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' The name used to reference a single (non-group) item in the tree.
        ''' </summary>
        Public ReadOnly Property SingularName As String
            Get
                Return If(mSingleName, "")
            End Get
        End Property

        ''' <summary>
        ''' The name used to reference multiple (non-group) items in the tree. Also
        ''' used as the name of the root of the tree.
        ''' </summary>
        Public ReadOnly Property PluralName As String
            Get
                Return If(mPluralName, "")
            End Get
        End Property

        Public ReadOnly Property GroupName As String
            Get
                Return If(mGroupName, "")
            End Get
        End Property

        ''' <summary>
        ''' Gets the data name of trees of this type, used for keeping track of data
        ''' versions within the database.
        ''' </summary>
        Friend ReadOnly Property DataName As String
            Get
                Return "GroupTree:" & PluralName
            End Get
        End Property

        ''' <summary>
        ''' The image key to use for root elements of trees defined by this attribute
        ''' or an empty string if no image key is set.
        ''' </summary>
        Public ReadOnly Property ImageKey As String
            Get
                Return If(mImageKey, "")
            End Get
        End Property

        ''' <summary>
        ''' The member types supported by this tree. This will include
        ''' <see cref="GroupMemberType.Group"/>, regardless of whether the attribute
        ''' was created with the type listed or not.
        ''' </summary>
        Public ReadOnly Property SupportedMemberTypes _
         As ICollection(Of GroupMemberType)
            Get
                Return GetReadOnly.ICollection(mSupportedTypes)
            End Get
        End Property

        ''' <summary>
        ''' Provides access to a permission defined on the tree type, specifing which
        ''' permission a user needs to edit the tree.
        ''' </summary>
        Public ReadOnly Property EditPermission As Permission
            Get
                Return If(mEditPermissionName = "",
                          Nothing, Permission.GetPermission(mEditPermissionName))
            End Get
        End Property

        ''' <summary>
        ''' Provides access to a permission defined on the tree type, specifying
        ''' which permission a user needs to manage access rights within the tree.
        ''' </summary>
        Public ReadOnly Property AccessRightsPermission As Permission
            Get
                Return If(mAccessRightsPermission = "",
                          Nothing, Permission.GetPermission(mAccessRightsPermission))
            End Get
        End Property

        ''' <summary>
        ''' Provides access to a permission defined on the tree type, specifying
        ''' which permission a user needs to add items to the tree
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property CreateItemPermission As Permission
            Get
                Return If(mCreateItemPermission = "",
                    Nothing, Permission.GetPermission(mCreateItemPermission))
            End Get
        End Property

        ''' <summary>
        ''' Provides access to a permission defined on the tree type, specifying
        ''' which permission a user needs to remove items from the tree
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property DeleteItemPermission As Permission
            Get
                Return If(mDeleteItemPermission = "",
                    Nothing, Permission.GetPermission(mDeleteItemPermission))
            End Get
        End Property


        Public ReadOnly Property ExportItemPermission As Permission
            Get
                Return If(mExportPermission = "",
                    Nothing, Permission.GetPermission(mExportPermission))
            End Get
        End Property

#End Region

    End Class

End Namespace
