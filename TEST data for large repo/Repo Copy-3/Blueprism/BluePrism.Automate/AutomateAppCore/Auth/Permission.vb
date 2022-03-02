
Imports System.Runtime.Serialization
Imports BluePrism.AutomateAppCore.clsServerPartialClasses
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Utilities.Functional

Namespace Auth

    ''' <summary>
    ''' Class which encapsulates a permission in the product.
    ''' </summary>
    <Serializable()>
    <DataContract(Name:="p", Namespace:="aac")>
    Public Class Permission

#Region " Class-scope declarations "

        ''' <summary>
        ''' A readonly, empty collection of permissions for use as an indicator of
        ''' 'no required permissions'.
        ''' </summary>
        Public Shared ReadOnly None As ICollection(Of Permission) = _
         GetEmpty.ICollection(Of Permission)()

        ' The permissions keyed on their names
        Private Shared mByName As IDictionary(Of String, Permission)

        ' The permissions keyed on their IDs
        Private Shared mById As IDictionary(Of Integer, Permission)

        ''' <summary>
        ''' Initialises the Permissions using the
        ''' <see cref="clsServer.GetPermissionData"/> method. This will have no
        ''' effect if the permissions are already initialised. See
        ''' <c>Init(bool force)</c> to force a re-initialisation of the permissions.
        ''' </summary>
        Friend Shared Sub Init(ByVal sv As IServer)
            If sv Is Nothing Then Return

            Dim data As PermissionData = sv.GetPermissionData()
            Populate(data.Permissions)
            PermissionGroup.Populate(data.PermissionGroups)
        End Sub

        ''' <summary>
        ''' Initialises the permissions lookups using the given provider.
        ''' </summary>
        ''' <param name="perms">The permissions to initialise this class with</param>
        Private Shared Sub Populate( _
         ByVal perms As IDictionary(Of Integer, Permission))
            Dim byName As New Dictionary(Of String, Permission)
            Dim byId As New Dictionary(Of Integer, Permission)

            ' We could use the given dictionary as the byId dictionary, but we can't
            ' be sure whether it will be referred to by the calling instance, so it's
            ' safer to create our own
            For Each p As Permission In perms.Values
                byName(p.Name) = p
                byId(p.Id) = p
            Next

            mByName = GetReadOnly.IDictionary(GetSynced.IDictionary(byName))
            mById = GetReadOnly.IDictionary(GetSynced.IDictionary(byId))
        End Sub

        ''' <summary>
        ''' Loads the permission data from the given multiple data provider,
        ''' returning a collection of permission objects which it generated.
        ''' </summary>
        ''' <param name="prov">The provider which provides the data for the
        ''' permissions to be created</param>
        ''' <returns>A collection of permissions loaded from the given data</returns>
        Friend Shared Function Load(ByVal prov As IMultipleDataProvider) _
         As IDictionary(Of Integer, Permission)
            Dim perms As New Dictionary(Of Integer, Permission)
            While prov.MoveNext()
                Dim p As New Permission(prov)
                perms(p.Id) = p
            End While
            Return perms
        End Function

        ''' <summary>
        ''' Gets the permission with the given ID, or null if no such permission
        ''' was found.
        ''' </summary>
        ''' <param name="id">The ID of the required permission</param>
        ''' <returns>The permission object corresponding to the given ID or null if
        ''' no such permission was found.</returns>
        ''' <exception cref="NotInitialisedException">If the permissions have not yet
        ''' been <see cref="IsInitialised">initialised</see>, meaning that there is
        ''' no lookup data for permissions.</exception>
        Public Shared Function GetPermission(ByVal id As Integer) As Permission
            If mById Is Nothing Then Throw New NotInitialisedException( _
             "Permission class has not been initialised")
            Dim p As Permission = Nothing
            If mById.TryGetValue(id, p) Then Return p
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets the permission with the given ID, or null if no such permission
        ''' was found.
        ''' </summary>
        ''' <param name="name">The name of the required permission</param>
        ''' <returns>The permission object corresponding to the given name or null
        ''' if no such permission was found.</returns>
        ''' <exception cref="NotInitialisedException">If the permissions have not yet
        ''' been <see cref="IsInitialised">initialised</see>, meaning that there is
        ''' no lookup data for permissions.</exception>
        Public Shared Function GetPermission(ByVal name As String) As Permission
            If mByName Is Nothing Then Throw New NotInitialisedException( _
             "Permission class has not been initialised")
            Dim p As Permission = Nothing
            If mByName.TryGetValue(name, p) Then Return p
            Return Nothing
        End Function

        ''' <summary>
        ''' Gets the permissions with the given names.
        ''' </summary>
        ''' <param name="names">The names for which the permissions are required.
        ''' If any name corresponds to a permission group then all of the permissions
        ''' in that group are included in the resultant collection.</param>
        ''' <returns>The collection of permissions corresponding to the given names.
        ''' </returns>
        ''' <exception cref="NotInitialisedException">If the permissions have not yet
        ''' been <see cref="IsInitialised">initialised</see>, meaning that there is
        ''' no lookup data for permissions.</exception>
        ''' <exception cref="NoSuchPermissionException">If a name was found for which
        ''' there is no corresponding permission or permission group</exception>
        Public Shared Function ByName(ByVal ParamArray names() As String) _
         As ICollection(Of Permission)
            Return ByName(DirectCast(names, ICollection(Of String)))
        End Function

        ''' <summary>
        ''' Gets the permissions with the given names.
        ''' </summary>
        ''' <param name="names">The names for which the permissions are required.
        ''' If any name corresponds to a permission group then all of the permissions
        ''' in that group are included in the resultant collection.</param>
        ''' <returns>The permissions corresponding to the given names in a readonly
        ''' collection.</returns>
        ''' <exception cref="NotInitialisedException">If the permissions have not yet
        ''' been <see cref="IsInitialised">initialised</see>, meaning that there is
        ''' no lookup data for permissions.</exception>
        ''' <exception cref="NoSuchPermissionException">If a name was found for which
        ''' there is no corresponding permission</exception>
        Public Shared Function ByName(ByVal names As ICollection(Of String)) _
         As ICollection(Of Permission)
            If Not IsInitialised Then Throw New NotInitialisedException( _
             "Permission class has not been initialised")

            ' If there's nothing there, no point in messing with the loop, just
            ' return an empty collection
            If CollectionUtil.IsNullOrEmpty(names) Then _
             Return GetEmpty.ICollection(Of Permission)()

            ' Otherwise, go through each name and get the corresponding perm
            Dim perms As New List(Of Permission)
            For Each name As String In names
                ' Start to add all the required permissions to the group
                Dim p As Permission = Nothing
                If mByName.TryGetValue(name, p) Then
                    ' We have the permission; add that to our list
                    perms.Add(p)
                Else
                    ' Otherwise - it's not a permission; see if it is a perm-group
                    Dim pg As PermissionGroup = PermissionGroup.GetGroup(name)
                    ' If not a group, throw an exception
                    If pg Is Nothing Then Throw New NoSuchPermissionException(name)
                    ' Otherwise, add all of the permissions in the group
                    perms.AddRange(pg)
                End If
            Next

            ' Wrap the list into a readonly collection
            Return GetReadOnly.ICollection(perms)

        End Function

        ''' <summary>
        ''' Checks if this class has been initialised or not
        ''' </summary>
        ''' <returns>True if this class has been initialised and has registered
        ''' permissions; False otherwise.</returns>
        Public Shared ReadOnly Property IsInitialised() As Boolean
            Get
                Return (mByName IsNot Nothing AndAlso mByName.Count > 0)
            End Get
        End Property

#End Region

#Region " Member Variables "

        ' The name of this permission
        <DataMember>
        Private mName As String


        ' The database id of this permission
        <DataMember>
        Private mId As Integer

        <DataMember>
        Private mRequiredFeature As Feature

#End Region

#Region " Constructors "

        ''' <summary>
        ''' DO NOT USE - for data contract unit testing 
        ''' </summary>
        ''' <param name="id">The ID of the permission</param>
        ''' <param name="name">The name of the permission</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Shared Function CreatePermission(ByVal id As Integer, ByVal name As String) As Permission
            Return New Permission(id, name, Feature.None)
        End Function

        ''' <summary>
        ''' Creates a new permission.
        ''' </summary>
        ''' <param name="id">The ID of the permission</param>
        ''' <param name="name">The name of the permission</param>
        Protected Sub New(ByVal id As Integer, ByVal name As String, requiredFeature As Feature)
            mId = id
            mName = name
            mRequiredFeature = requiredFeature
        End Sub

        ''' <summary>
        ''' Creates a new permission
        ''' </summary>
        ''' <param name="prov">The data provider, which is expected to provide two
        ''' arguments: <c>"id"</c>, an integer providing the permission's ID, and
        ''' <c>"name"</c>, a string providing the permission's name.</param>
        Private Sub New(ByVal prov As IDataProvider)
            Me.New(
                prov.GetValue("id", 0),
                prov.GetString("name"),
                prov.GetValue("requiredFeature", "None").
                      Map(Function(x) If(String.IsNullOrEmpty(x), "None", x)).
                      Map(Function(x) DirectCast([Enum].Parse(GetType(Feature), x), Feature)))
        End Sub

#End Region

#Region " Properties "

        ''' <summary>
        ''' The database's integer ID for this permission.
        ''' </summary>
        Public ReadOnly Property Id() As Integer
            Get
                Return mId
            End Get
        End Property

        ''' <summary>
        ''' The name of this permission
        ''' </summary>
        Public ReadOnly Property Name() As String
            Get
                Return mName
            End Get
        End Property

        Public ReadOnly Property RequiredFeature As Feature
            Get
                Return mRequiredFeature
            End Get
        End Property

#End Region

#Region " Methods "

        ''' <summary>
        ''' Checks if this permission is equal to another object.
        ''' </summary>
        ''' <param name="obj">The object to test against this permission for equality
        ''' </param>
        ''' <returns>True if the given object is a non-null permission object with
        ''' the same ID as this object. As permissions are immutable, the ID should
        ''' be enough to determine equality.</returns>
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Dim p As Permission = TryCast(obj, Permission)
            Return (p IsNot Nothing AndAlso p.Id = Id)
        End Function

        ''' <summary>
        ''' Gets an integer hash for this object.
        ''' </summary>
        ''' <returns>An integer hash which represents this permission; this is just
        ''' a function of the <see cref="Id"/> property.</returns>
        Public Overrides Function GetHashCode() As Integer
            Return mId
        End Function

        ''' <summary>
        ''' Gets a string representation of this permission; this is equivalent to
        ''' the <see cref="Name"/> property.
        ''' </summary>
        ''' <returns>A string representation of this permission.</returns>
        Public Overrides Function ToString() As String
            Return Name
        End Function

#End Region

#Region "Permission Constants"

        Public Class ControlRoom
            Public Const GroupName = "Control Room"
            Public Const ManageQueuesFullAccess As String = "Full Access to Queue Management"
            Public Const ManageQueuesReadOnly As String = "Read Access to Queue Management"
        End Class

        Public Class Analytics
            Public Const GroupName = "Analytics"
            Public Const CreateEditDeleteTiles = "Create/Edit/Delete Tiles"
            Public Const DesignGlobalDashboards = "Design Global Dashboards"
            Public Const DesignPersonalDashboards = "Design Personal Dashboards"
            Public Const DesignPublishedDashboards = "Design Published Dashboards"
            Public Const ImportGlobalDashboard = "Import Global Dashboard"
            Public Const ImportPublishedDashboard = "Import Published Dashboard"
            Public Const ImportTile = "Import Tile"
            Public Const ViewDashboards = "View Dashboards"
        End Class

        Public Class ObjectStudio
            Public Const GroupName = "Object Studio"
            Public Const CreateBusinessObject = "Create Business Object"
            Public Const DeleteBusinessObject = "Delete Business Object"
            Public Const EditBusinessObject = "Edit Business Object"
            Public Const EditObjectGroups = "Edit Object Groups"
            Public Const ExportBusinessObject = "Export Business Object"
            Public Const ImportBusinessObject = "Import Business Object"
            Public Const ExecuteBusinessObject = "Execute Business Object"
            Public Const ExecuteBusinessObjectAsWebService = "Execute Business Object as Web Service"
            Public Const ViewBusinessObject = "View Business Object Definition"
            Public Const ManageBusinessObjectAccessRights = "Manage Business Object Access Rights"
            ''' <summary>
            ''' Shorthand for testing if View Business Object is available directly
            ''' (or indirectly implied by a higher permission)
            ''' </summary>
            Public Shared ImpliedViewBusinessObject As String() = {ViewBusinessObject, EditBusinessObject, CreateBusinessObject}
            ''' <summary>
            ''' Shorthand for testing if Execute Business Object is available directly
            ''' (or indirectly implied by a higher permission)
            ''' </summary>
            Public Shared ImpliedExecuteBusinessObject As String() = {ExecuteBusinessObject, EditBusinessObject, CreateBusinessObject}
            ''' <summary>
            ''' Shorthand for testing if Edit Business Object is available directly
            ''' (or indirectly implied by a higher permission)
            ''' </summary>
            Public Shared ImpliedEditBusinessObject As String() = {EditBusinessObject, CreateBusinessObject}


            Public Shared AllObjectPermissionsAllowingTreeView As String() = {CreateBusinessObject,
                                                                DeleteBusinessObject,
                                                                EditBusinessObject,
                                                                EditObjectGroups,
                                                                ExportBusinessObject,
                                                                ExecuteBusinessObject,
                                                                ExecuteBusinessObjectAsWebService,
                                                                ViewBusinessObject,
                                                                ManageBusinessObjectAccessRights}

        End Class

        Public Class ProcessAlerts
            Public Const GroupName = "Process Alerts"
            Public Const ConfigureProcessAlerts = "Configure Process Alerts"
            Public Const SubscribeToProcessAlerts = "Subscribe to Process Alerts"
        End Class

        Public Class ProcessStudio
            Public Const GroupName = "Process Studio"
            Public Const CreateProcess = "Create Process"
            Public Const DeleteProcess = "Delete Process"
            Public Const EditProcess = "Edit Process"
            Public Const EditProcessGroups = "Edit Process Groups"
            Public Const ExportProcess = "Export Process"
            Public Const ImportProcess = "Import Process"
            Public Const ExecuteProcess = "Execute Process"
            Public Const ExecuteProcessAsWebService = "Execute Process as Web Service"
            Public Const ViewProcess = "View Process Definition"
            Public Const ManageProcessAccessRights = "Manage Process Access Rights"
            ''' <summary>
            ''' Shorthand for testing if View Process is available directly
            ''' (or indirectly implied by a higher permission)
            ''' </summary>
            Public Shared ImpliedViewProcess As String() = {ViewProcess, EditProcess, CreateProcess}
            ''' <summary>
            ''' Shorthand for testing if Execute Process is available directly
            ''' (or indirectly implied by a higher permission)
            ''' </summary>
            Public Shared ImpliedExecuteProcess As String() = {ExecuteProcess, EditProcess, CreateProcess}
            ''' <summary>
            ''' Shorthand for testing if Edit Process is available directly
            ''' (or indirectly implied by a higher permission)
            ''' </summary>
            Public Shared ImpliedEditProcess As String() = {EditProcess, CreateProcess}

            Public Shared AllProcessPermissionsAllowingTreeView As String() = {CreateProcess,
                                                                DeleteProcess,
                                                                EditProcess,
                                                                EditProcessGroups,
                                                                ExportProcess,
                                                                ExecuteProcess,
                                                                ExecuteProcessAsWebService,
                                                                ViewProcess,
                                                                ManageProcessAccessRights}

            Public Shared AllProcessPermissions As String() = AllProcessPermissionsAllowingTreeView.Concat({ImportProcess}).ToArray()
        End Class

        Public Class Resources
            Public Const GroupName = "Resources"
            Public Const ViewResource = "View Resource"
            Public Const ConfigureResource = "Configure Resource"
            Public Const ControlResource = "Control Resource"
            Public Const ViewResourceScreenCaptures = "View Resource Screen Captures"
            Public Const ViewResourceDetails = "View Resource Details"
            Public Const AuthenticateAsResource = "Authenticate as Resource"
            Public Const ManageResourceAccessrights = "Manage Resource Access Rights"
            Public Const EditResourceGroups = "Edit Resource Groups"
            ''' <summary>
            ''' Shorthand for testing if View Resource is available directly
            ''' (or indirectly implied by a higher permission)
            ''' </summary>
            Public Shared ImpliedViewResource As String() = {ViewResource, ViewResourceScreenCaptures, ConfigureResource, ControlResource, ManageResourceAccessrights, ViewResourceDetails}
            ''' <summary>
            ''' Shorthand for testing if the user has access to Resource Management in System
            ''' </summary>
            Public Shared ImpliedManageResources As String() = {ConfigureResource, EditResourceGroups, ManageResourceAccessrights}

            Public Shared AllResourcePermissionsAllowingTreeView As String() = {ViewResource, ViewResourceScreenCaptures, ConfigureResource, ControlResource, ManageResourceAccessrights, ViewResourceDetails}
        End Class

        Public Class Skills
            Public Const GroupName = "Skills"
            Public Const ViewSkill = "View Skill"
            Public Const ManageSkill = "Manage Skill"
            Public Const ImportSkill = "Import Skill"
            Public Shared ImpliedViewSkill As String() = {ViewSkill, ManageSkill}
        End Class

        Public Class DocumentProcessing
            Public Const GroupName = "Decipher"
            Public Const RedirectOutput = "Decipher - Redirect Output"
            Public Const Configure = "Decipher - Configuration"
        End Class

        Public Class ReleaseManager
            Public Const GroupName = "Release Manager"
            Public Const CreateRelease = "Create Release"
            Public Const CreateEditPackage = "Create/Edit Package"
            Public Const DeletePackage = "Delete Package"
            Public Const ViewReleaseManager = "View Release Manager"
            Public Const ImportRelease = "Import Release"
        End Class

        Public Class Scheduler
            Public Const GroupName = "Scheduler"
            Public Const CreateSchedule = "Create Schedule"
            Public Const DeleteSchedule = "Delete Schedule"
            Public Const EditSchedule = "Edit Schedule"
            Public Const RetireSchedule = "Retire Schedule"
            Public Const ViewSchedule = "View Schedule"
        End Class

        Public Class SystemManager
            Public Const GroupName = "System Manager"

            Public Class Audit
                Public Const Alerts = "Audit - Alerts"
                Public Const AuditLogs = "Audit - Audit Logs"
                Public Const BusinessObjectsLogs = "Audit - Business Object Logs"
                Public Const ConfigureDesignControls = "Audit - Configure Design Controls"
                Public Const ProcessLogs = "Audit - Process Logs"
                Public Const Statistics = "Audit - Statistics"
                Public Const ViewDesignControls = "Audit - View Design Controls"
            End Class

            Public Class BusinessObjects
                Public Const ConfigureEnvironmentVariables =
                    "Business Objects - Configure Environment Variables"
                Public Const ExceptionTypes = "Business Objects - Exception Types"
                Public Const Exposure = "Business Objects - Exposure"
                Public Const External = "Business Objects - External"
                Public Const History = "Business Objects - History"
                Public Const Management = "Business Objects - Management"
                Public Const ViewEnvironmentVariables =
                    "Business Objects - View Environment Variables"
                Public Const WebServicesSoap = "Business Objects - SOAP Web Services"
                Public Const WebServicesWebApi = "Business Objects - Web API Services"
                Public Const WebConnectionSettings = "Business Objects - Web Connection Settings"
            End Class

            Public Class Processes
                Public Const ConfigureEnvironmentVariables =
                    "Processes - Configure Environment Variables"
                Public Const ExceptionTypes = "Processes - Exception Types"
                Public Const Exposure = "Processes - Exposure"
                Public Const Grouping = "Processes - Grouping"
                Public Const History = "Processes - History"
                Public Const Management = "Processes - Management"
                Public Const ViewEnvironmentVariables =
                    "Processes - View Environment Variables"
            End Class

            Public Class Resources
                Public Const Pools = "Resources - Pools"
            End Class

            Public Class Security
                Public Const Credentials = "Security - Manage Credentials"
                Public Const ManageEncryptionSchemes = "Security - Manage Encryption Schemes"
                Public Const SignOnSettings = "Security - Sign-on Settings"
                Public Const UserRoles = "Security - User Roles"
                Public Const Users = "Security - Users"
                Public Const ViewEncryptionSchemes = "Security - View Encryption Scheme Configuration"
            End Class

            Public Class System
                Public Const Archiving = "System - Archiving"
                Public Const Calendars = "System - Calendars"
                Public Const Fonts = "System - Fonts"
                Public Const License = "System - License"
                Public Const Reporting = "System - Reporting"
                Public Const Scheduler = "System - Scheduler"
                Public Const Settings = "System - Settings"
            End Class

            Public Class Workflow
                Public Const EnvironmentLocking = "Workflow - Environment Locking"
                Public Const WorkQueueConfiguration = "Workflow - Work Queue Configuration"
            End Class

            Public Class DataGateways
                Public Const ControlRoom = "Data Gateways - Control Room"
                Public Const Configuration = "Data Gateways - Configuration"
                Public Const AdvancedConfiguration = "Data Gateways - Advanced Configuration"

                Public Shared ImpliedConfiguration As String() = {Configuration, AdvancedConfiguration}
            End Class

            Public Class AuthenticationServer
                Public Const MapAuthenticationServerUsers = "Authentication Server - Map Users"
            End Class

        End Class

#End Region

    End Class

End Namespace
