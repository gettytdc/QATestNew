Imports System.Xml

Imports System.Data.SqlClient
Imports System.Threading
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.BackgroundJobs

Imports BluePrism.AutomateProcessCore

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections

Imports BluePrism.Scheduling
Imports BluePrism.Scheduling.Calendar

Imports BluePrism.AutomateAppCore.ProcessComponent
Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Common.Security

Imports ProcessAttributes = BluePrism.AutomateProcessCore.ProcessAttributes
Imports BluePrism.Core.Xml
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.Data
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.DataAccess
Imports BluePrism.Skills
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.EnvironmentVariables
Imports NameAlreadyExistsException = BluePrism.Server.Domain.Models.NameAlreadyExistsException

Imports BluePrism.Server.Domain.Models

' Partial class which separates the env locking from the rest of the clsServer
' methods just in order to keep the file size down to a sane level and make it
' easier to actually find functions
Partial Public Class clsServer

#Region " Public Methods "

    ''' <summary>
    ''' Saves a new version of the given package, using the properties currently set
    ''' in the supplied object.
    ''' </summary>
    ''' <param name="pkg">The package to save.</param>
    ''' <remarks>
    ''' This will insert a new package instance with the same ID as it current has.
    ''' If it doesn't yet have an ID, this will insert a brand new package instance
    ''' with a newly generated ID.
    ''' On exit, the package object will have its ID and Ident properties updated to
    ''' match that on the database.
    ''' Note that there are no redundancy checks, so if the current version of the
    ''' package is the same as that which has just been passed, it will still save
    ''' a new version of the package.
    ''' </remarks>
    <SecuredMethod(Permission.ReleaseManager.CreateEditPackage)>
    Public Function SavePackage(ByVal pkg As clsPackage) As clsPackage Implements IServer.SavePackage
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            SavePackage(con, pkg)
            con.CommitTransaction()
            Return pkg
        End Using
    End Function

    ''' <summary>
    ''' Initiates import of the given release into the database. The import is run
    ''' as a background job.
    ''' </summary>
    ''' <param name="release">The release to import</param>
    ''' <param name="notifier">The notifier used to signal that updates have been made
    ''' as the job progresses</param>
    ''' <returns>An identifier for the background job under which the import is 
    ''' running</returns>
    <SecuredMethod(Permission.ReleaseManager.ImportRelease)>
    Public Function ImportRelease(release As clsRelease, unlockProcesses As Boolean, notifier As BackgroundJobNotifier) As BackgroundJob Implements IServer.ImportRelease
        CheckPermissions()
        CleanUpExpiredBackgroundJobs()
        Dim jobId = GetNewBackgroundJobId()
        ThreadPool.QueueUserWorkItem(Sub()
                                         RunReleaseImport(jobId, notifier, release, unlockProcesses)
                                     End Sub)
        Return New BackgroundJob(jobId)
    End Function


    ''' <summary>
    ''' Initiates import of the given release into the database. The import is run
    ''' as a background job.
    ''' </summary>
    ''' <param name="release">The release to import</param>
    ''' <param name="notifier">The notifier used to signal that updates have been made
    ''' as the job progresses</param>
    ''' <returns>An identifier for the background job under which the import is 
    ''' running</returns>
    <SecuredMethod(Permission.ObjectStudio.ImportBusinessObject, Permission.ProcessStudio.ImportProcess)>
    Public Function ImportProcessOrObjectAsRelease(release As clsRelease, unlockProcesses As Boolean, notifier As BackgroundJobNotifier) As BackgroundJob Implements IServer.ImportProcessOrObjectAsRelease
        CheckPermissions()
        CleanUpExpiredBackgroundJobs()
        Dim jobId = GetNewBackgroundJobId()
        ThreadPool.QueueUserWorkItem(Sub()
            RunReleaseImport(jobId, notifier, release, unlockProcesses)
        End Sub)
        Return New BackgroundJob(jobId)
    End Function
    ''' <summary>
    ''' Gets the package with the given ID
    ''' </summary>
    ''' <param name="id">The ID of the package required.</param>
    ''' <returns>The package corresponding to the given ID.</returns>
    ''' <exception cref="NoSuchElementException">If no package was found with the
    ''' given ID.</exception>
    <SecuredMethod(Permission.ReleaseManager.ViewReleaseManager)>
    Public Function GetPackage(ByVal id As Integer) As clsPackage Implements IServer.GetPackage
        CheckPermissions()
        Using con = GetConnection()
            Return GetPackage(con, id, True)
        End Using
    End Function

    ''' <summary>
    ''' Gets the package with the given name.
    ''' </summary>
    ''' <param name="name">The name of the required package.</param>
    ''' <returns>The package (with release data) corresponding to the given name,
    ''' or null if no such package exists.</returns>
    <SecuredMethod(Permission.ReleaseManager.ViewReleaseManager)>
    Public Function GetPackage(ByVal name As String) As clsPackage Implements IServer.GetPackage
        CheckPermissions()
        Using con = GetConnection()
            Return GetPackage(con, name, True)
        End Using
    End Function

    ''' <summary>
    ''' Gets all the packages in the system and their contents, but not any related
    ''' releases. These can be retrieved by calling <see cref="PopulateReleases"/>
    ''' for the package for which releases are required.
    ''' </summary>
    ''' <returns>A collection of all packages on the system.</returns>
    <SecuredMethod(Permission.ReleaseManager.ViewReleaseManager)>
    Public Function GetPackages() As ICollection(Of clsPackage) Implements IServer.GetPackages
        CheckPermissions()
        Using con = GetConnection()
            Return GetPackages(con, Nothing, False)
        End Using
    End Function

    ''' <summary>
    ''' Gets all the packages in the system, optionally along with their releases.
    ''' </summary>
    ''' <param name="includeReleases">True to retrieve and populate the releases into
    ''' the returned package objects.</param>
    ''' <returns>The packages on the system.</returns>
    <SecuredMethod(Permission.ReleaseManager.ViewReleaseManager)>
    Public Function GetPackages(ByVal includeReleases As Boolean) As ICollection(Of clsPackage) Implements IServer.GetPackages
        CheckPermissions()
        Using con = GetConnection()
            Return GetPackages(con, Nothing, includeReleases)
        End Using
    End Function

    ''' <summary>
    ''' Gets the packages (without release data) which contain the given process.
    ''' </summary>
    ''' <param name="id">The ID of the process to check packages for.</param>
    ''' <returns>The collection of packages which contain the required process.
    ''' </returns>
    <SecuredMethod(Permission.ReleaseManager.ViewReleaseManager)>
    Public Function GetPackagesWithProcess(ByVal id As Guid) As ICollection(Of clsPackage) Implements IServer.GetPackagesWithProcess
        CheckPermissions()
        Using con = GetConnection()
            Return GetPackagesWithProcess(con, id)
        End Using
    End Function

    ''' <summary>
    ''' Deletes the given package, and any related data.
    ''' </summary>
    ''' <param name="pkg">The package to delete.</param>
    <SecuredMethod(Permission.ReleaseManager.DeletePackage)>
    Public Sub DeletePackage(ByVal pkg As clsPackage) Implements IServer.DeletePackage
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            DeletePackage(con, pkg)
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Creates the specified release on the database.
    ''' </summary>
    ''' <param name="rel">The release to create.</param>
    ''' <returns>The newly created release with its ID set.</returns>
    <SecuredMethod(Permission.ReleaseManager.CreateRelease)>
    Public Function CreateRelease(ByVal rel As clsRelease) As clsRelease Implements IServer.CreateRelease
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            CreateRelease(con, rel, RelmanEventCode.CreateRelease)
            con.CommitTransaction()
            Return rel
        End Using
    End Function


    ''' <summary>
    ''' Checks if the release is valid and would successfully update the database.
    ''' Currently, this only checks for a duplicate release name within the package.
    ''' </summary>
    ''' <param name="rel">The release to check</param>
    ''' <returns>True if the name on the release is valid; False if such a name
    ''' already exists on the release's package.</returns>
    <SecuredMethod(Permission.ReleaseManager.ViewReleaseManager)>
    Public Function IsValidRelease(ByVal rel As clsRelease) As Boolean Implements IServer.IsValidRelease
        CheckPermissions()
        Using con = GetConnection()
            Return IsValidRelease(con, rel)
        End Using
    End Function

#End Region

#Region " Private Package Methods "

    ''' <summary>
    ''' Gets the base information regarding the packages currently held in the
    ''' system
    ''' </summary>
    ''' <param name="con">The connection over which the package information should
    ''' be retrieved.</param>
    ''' <returns>The basic information regarding the packages - not including its
    ''' contents or releases.
    ''' </returns>
    Private Function GetPackagesInfo(ByVal con As clsDBConnection) As ICollection(Of clsPackage)
        Dim map As New SortedDictionary(Of String, clsPackage)
        Dim cmd As New SqlCommand(
         " select p.id, p.name, p.description, u.username, p.created" &
         " from BPAPackage p" &
         "   join BPAUser u on p.userid = u.userid"
        )
        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()
                map(prov.GetString("name")) = New clsPackage(prov)
            End While
            Return map.Values
        End Using

    End Function


    ''' <summary>
    ''' Gets the single package with the given name, including the releases as
    ''' required.
    ''' </summary>
    ''' <param name="con">The connection over which to retrieve the package.</param>
    ''' <param name="name">The name of the required package.</param>
    ''' <param name="includeReleases">True to include the release data with the
    ''' package; False to retrieve the package data only.</param>
    ''' <returns>The package (with release data) corresponding to the given name,
    ''' or null if no such package exists.</returns>
    Private Function GetPackage(ByVal con As IDatabaseConnection,
     ByVal name As String, ByVal includeReleases As Boolean) As clsPackage
        Dim cmd As New SqlCommand("select id from BPAPackage where name=@name")
        cmd.Parameters.AddWithValue("@name", name)
        Dim id As Integer = IfNull(con.ExecuteReturnScalar(cmd), -1)
        If id = -1 Then Return Nothing
        Return GetPackage(con, id, includeReleases)
    End Function

    ''' <summary>
    ''' Gets the package with the given ID using the given database connection.
    ''' </summary>
    ''' <param name="con">The connection to use.</param>
    ''' <param name="id">The ID of the package required.</param>
    ''' <returns>The package corresponding to the given ID.</returns>
    ''' <exception cref="NoSuchElementException">If no package was found with the
    ''' given ID.</exception>
    Private Function GetPackage(con As IDatabaseConnection, id As Integer, includeReleases As Boolean) As clsPackage

        Dim packages = GetPackages(con, New Integer() {id}, includeReleases)

        If packages.Any() Then Return packages.First()

        Throw New NoSuchElementException(My.Resources.clsServer_CouldNotFindAPackageWithTheID0, id)

    End Function

    ''' <summary>
    ''' Gets the packages (without release data) which contain the given process.
    ''' </summary>
    ''' <param name="con">The connection to the database to use.</param>
    ''' <param name="id">The ID of the process to check packages for.</param>
    ''' <returns>The collection of packages which contain the required process.
    ''' </returns>
    Private Function GetPackagesWithProcess(ByVal con As IDatabaseConnection, ByVal id As Guid) As ICollection(Of clsPackage)
        Dim cmd As New SqlCommand(
         "select packageid from BPAPackageProcess where processid = @id")
        cmd.Parameters.AddWithValue("@id", id)

        Dim ids As New List(Of Integer)
        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()
                ids.Add(prov.GetValue("packageid", 0))
            End While
        End Using
        If ids.Count = 0 Then Return GetEmpty.ICollection(Of clsPackage)()
        Return GetPackages(con, ids, False)
    End Function

    ''' <summary>
    ''' Gets the packages with the given IDs using the given database connection.
    ''' </summary>
    ''' <param name="con">The connection to use.</param>
    ''' <param name="ids">The IDs of the packages required. If this is null, then all
    ''' packages will be retrieved.</param>
    ''' <returns>The package corresponding to the given ID.</returns>
    ''' <exception cref="NoSuchElementException">If no package was found with the
    ''' given ID.</exception>
    Private Function GetPackages(
     ByVal con As IDatabaseConnection,
     ByVal ids As ICollection(Of Integer),
     ByVal includeReleases As Boolean) As ICollection(Of clsPackage)

        ' Check if we're getting all the packages first...
        If ids Is Nothing Then ' ids==null => Get All Packages. Pick up all the IDs
            ids = New List(Of Integer)
            Using reader =
             con.ExecuteReturnDataReader(New SqlCommand("select id from BPAPackage"))
                While reader.Read()
                    ids.Add(reader.GetInt32(0))
                End While
            End Using
        End If

        ' No IDs to retrieve? Return no packages. Seems fair.
        If ids.Count = 0 Then Return New clsPackage() {}

        Dim cmd As New SqlCommand()
        ' First retrieve the basic package details, then each of the package contents.
        ' Unfortunately, they're all represented in different ways (integers, GUIDs, strings)
        ' so they all need to be processed in different ways.

        cmd.CommandText =
         " select p.id, p.name, p.description, u.username, p.created" &
         " from BPAPackage p" &
         "   join BPAUser u on p.userid = u.userid" &
         " where p.id = @id;" & _
 _
         " select p.processid as id, p.processtype, p.name, p.attributeid" &
         " from BPAPackageProcess pp" &
         "   join BPAProcess p on pp.processid = p.processid" &
         " where pp.packageid = @id" & _
 _
         " select q.ident as id, q.name" &
         " from BPAPackageWorkQueue pq" &
         "   join BPAWorkQueue q on pq.queueident = q.ident" &
         " where pq.packageid = @id" & _
 _
         " select c.id, c.name, cproc.processid" &
         " from BPAPackageCredential pc" &
         "   join BPACredentials c on pc.credentialid = c.id" &
         "   left join BPACredentialsProcesses cproc on c.id = cproc.credentialid" &
         " where packageid = @id" & _
 _
         " select s.id, s.name" &
         " from BPAPackageSchedule ps" &
         "   join BPASchedule s on ps.scheduleid = s.id" &
         " where ps.packageid = @id" & _
 _
         " select c.id, c.name" &
         " from BPAPackageCalendar pc" &
         "   join BPACalendar c on pc.calendarid = c.id" &
         " where pc.packageid = @id" & _
 _
         " select sl.id, sl.listtype, sl.name" &
         " from BPAPackageScheduleList psl" &
         "   join BPAScheduleList sl on psl.schedulelistid = sl.id" &
         " where packageid = @id" & _
 _
         " select ws.serviceid as id, ws.servicename as name" &
         " from BPAPackageWebService pws" &
         "   join BPAWebService ws on pws.webserviceid = ws.serviceid" &
         " where packageid = @id" & _
 _
         " select api.serviceid as id, api.name as name" &
         " from BPAPackageWebApi pwa" &
         "   join BPAWebApiService api on pwa.webapiid = api.serviceid" &
         " where packageid = @id" & _
 _
         " select ev.name" &
         " from BPAPackageEnvironmentVar pev" &
         "   join BPAEnvironmentVar ev on pev.name = ev.name" &
         " where pev.packageid = @id" & _
 _
         " select f.name" &
         " from BPAPackageFont pf" &
         "   join BPAFont f on pf.name = f.name" &
         " where pf.packageid = @id" & _
 _
         " select t.id, t.name" &
         " from BPAPackageTile pt" &
         "   join BPATile t on pt.tileid = t.id" &
         " where pt.packageid = @id" & _
 _
         " select d.id, d.name" &
         " from BPAPackageDashboard pd" &
         "   join BPADashboard d on pd.dashid = d.id" &
         " where pd.packageid = @id"

        Dim param As SqlParameter = cmd.Parameters.Add("@id", SqlDbType.Int)

        ' Keep track of the IDs we've processed - no point in getting a package twice
        Dim idsRetrieved As New clsSet(Of Integer)
        Dim packages As New List(Of clsPackage)

        For Each id As Integer In ids
            ' If we've already processed this ID, move on.
            If Not idsRetrieved.Add(id) Then Continue For

            ' Set the parameter and retrieve it
            param.Value = id

            Dim containsInaccessibleItems = False

            Using reader = con.ExecuteReturnDataReader(cmd)
                ' There's a number of result sets to go through here.

                ' First is the basic package details... if they aren't there, then
                ' there's no package on the DB with the given ID.
                If Not reader.Read() Then Throw New NoSuchElementException(
                 My.Resources.clsServer_CouldNotFindAPackageWithTheID0, id)

                Dim dataProvider As New ReaderDataProvider(reader)

                Dim package As New clsPackage(dataProvider)

                ' Now we start going through the contents, and creating them.
                ' The order is :-
                ' * Process/Object
                ' * Process Group
                ' * Work Queue
                ' * Credential
                ' * Schedule
                ' * Schedule List
                ' * Web Service
                ' * Web API
                ' * Environment Variable
                ' * Font
                ' * Tile
                ' * Dashboard

                Dim packageComponents As New List(Of PackageComponent)  ' The list of components to add to the package

                Dim exceptionsState As String = My.Resources.clsServer_ProcessesObjects ' state for the exceptions
                If Not reader.NextResult() Then Throw New BluePrismException(My.Resources.clsServer_FailedToGet0, exceptionsState)
                Dim processes As New Dictionary(Of Guid, ProcessComponent)
                While reader.Read()
                    Dim process As ProcessComponent = ProcessComponent.Create(package, dataProvider) ' Determines whether process or VBO.
                    packageComponents.Add(process)
                    processes(process.IdAsGuid) = process ' save in dict for process groups to use.

                    If Not GetEffectiveMemberPermissionsForProcess(process.IdAsGuid).HasAnyPermissions(mLoggedInUser) Then
                        containsInaccessibleItems = True
                    End If
                End While

                exceptionsState = My.Resources.clsServer_WorkQueues  ' state for the exceptions
                If Not reader.NextResult() Then Throw New BluePrismException(
                    My.Resources.clsServer_FailedToGet0, exceptionsState)
                While reader.Read()
                    packageComponents.Add(New WorkQueueComponent(package, dataProvider))
                End While

                exceptionsState = My.Resources.clsServer_Credentials  ' state for the exceptions
                If Not reader.NextResult() Then Throw New BluePrismException(
                    My.Resources.clsServer_FailedToGet0, exceptionsState)
                While reader.Read()
                    CredentialComponent.MergeInto(package, packageComponents, processes, dataProvider)
                End While

                exceptionsState = My.Resources.clsServer_Schedules    ' state for the exceptions
                If Not reader.NextResult() Then Throw New BluePrismException(
                    My.Resources.clsServer_FailedToGet0, exceptionsState)
                While reader.Read()
                    packageComponents.Add(New ScheduleComponent(package, dataProvider))
                End While

                exceptionsState = My.Resources.clsServer_Calendars    ' state for the exceptions
                If Not reader.NextResult() Then Throw New BluePrismException(
                    My.Resources.clsServer_FailedToGet0, exceptionsState)
                While reader.Read()
                    packageComponents.Add(New CalendarComponent(package, dataProvider))
                End While

                exceptionsState = My.Resources.clsServer_ScheduleLists   ' state for the exceptions
                If Not reader.NextResult() Then Throw New BluePrismException(
                    My.Resources.clsServer_FailedToGet0, exceptionsState)
                While reader.Read()
                    packageComponents.Add(New ScheduleListComponent(package, dataProvider))
                End While

                exceptionsState = My.Resources.clsServer_WebServices ' state for the exceptions
                If Not reader.NextResult() Then Throw New BluePrismException(
                    My.Resources.clsServer_FailedToGet0, exceptionsState)
                While reader.Read()
                    packageComponents.Add(New WebServiceComponent(package, dataProvider))
                End While

                exceptionsState = My.Resources.clsServer_WebAPIs ' state for the exceptions
                If Not reader.NextResult() Then Throw New BluePrismException(
                    My.Resources.clsServer_FailedToGet0, exceptionsState)
                While reader.Read()
                    packageComponents.Add(New WebApiComponent(package, dataProvider))
                End While

                exceptionsState = My.Resources.clsServer_EnvironmentVariables    ' state for the exceptions
                If Not reader.NextResult() Then Throw New BluePrismException(
                    My.Resources.clsServer_FailedToGet0, exceptionsState)
                While reader.Read()
                    packageComponents.Add(New EnvironmentVariableComponent(package, dataProvider))
                End While

                exceptionsState = My.Resources.clsServer_Fonts    ' state for the exceptions
                If Not reader.NextResult() Then Throw New BluePrismException(My.Resources.clsServer_FailedToGet0, exceptionsState)
                While reader.Read()
                    packageComponents.Add(New FontComponent(package, dataProvider))
                End While

                exceptionsState = My.Resources.clsServer_Tiles    ' state for the exceptions
                If Not reader.NextResult() Then Throw New BluePrismException(My.Resources.clsServer_FailedToGet0, exceptionsState)
                While reader.Read()
                    packageComponents.Add(New TileComponent(package, dataProvider))
                End While

                exceptionsState = My.Resources.clsServer_Dashboards   ' state for the exceptions
                If Not reader.NextResult() Then Throw New BluePrismException(My.Resources.clsServer_FailedToGet0, exceptionsState)
                While reader.Read()
                    packageComponents.Add(New DashboardComponent(package, dataProvider))
                End While

                ' Okay, we now have a list of all components in a package. Add that into the
                ' package object and we're good to go
                ' (Note: the releases are lazily retrieved - they aren't needed in all cases,
                ' so we don't load them here).
                package.AddAll(packageComponents)
                package.ContainsInaccessibleItems = containsInaccessibleItems
                packages.Add(package)

            End Using

        Next

        ' FIXME: This could be more efficient...
        If includeReleases Then
            For Each pkg As clsPackage In packages
                PopulateReleases(con, pkg)
            Next
        End If
        Return packages

    End Function

    ''' <summary>
    ''' Saves a new version of the given package over the specified connection, using
    ''' the properties currently set in the supplied object.
    ''' </summary>
    ''' <param name="con">The database connection to use to save the package.</param>
    ''' <param name="pkg">The package to save.</param>
    ''' <remarks>
    ''' This will insert a new package instance with the same ID as it current has.
    ''' If it doesn't yet have an ID, this will insert a brand new package instance
    ''' with a newly generated ID.
    ''' On exit, the package object will have its ID and Ident properties updated to
    ''' match that on the database.
    ''' Note that there are no redundancy checks, so if the current version of the
    ''' package is the same as that which has just been passed, it will still save
    ''' a new version of the package.
    ''' </remarks>
    ''' <exception cref="NameAlreadyExistsException">If the name on the package
    ''' already exists on the database and thus this package cannot be inserted or
    ''' updated as it is.</exception>
    Private Sub SavePackage(ByVal con As IDatabaseConnection, ByVal pkg As clsPackage)

        Dim userId As Guid = GetUserID(con, pkg.UserName) ' The ID of the user defined on the package

        Dim eventCode As RelmanEventCode ' The event code to record due to this save
        If pkg.IdAsInteger = 0 _
         Then eventCode = RelmanEventCode.NewPackage _
         Else eventCode = RelmanEventCode.ModifyPackage


        Using cmd As New SqlCommand()

            ' What we need to do :-
            ' If this is a new package, insert it;
            ' Otherwise, update it and delete the contents records ready to be re-added.
            ' Return the ID of the package (whether new or not)
            cmd.CommandText =
             " if @packageid is null" &
             " begin" &
             "   insert into BPAPackage(name,description,userid,created) values" &
             "     (@name, @description, @userid, @created)" &
             "   select cast(scope_identity() as int)" &
             " end" &
             " else" &
             " begin" &
             "   update BPAPackage set" &
             "     name=@name," &
             "     description=@description," &
             "     userid=@userid," &
             "     created=@created" &
             "   where id=@packageid" &
             "   delete from BPAPackageProcess where packageid=@packageid" &
             "   delete from BPAPackageWorkQueue where packageid=@packageid" &
             "   delete from BPAPackageCredential where packageid=@packageid" &
             "   delete from BPAPackageSchedule where packageid=@packageid" &
             "   delete from BPAPackageCalendar where packageid=@packageid" &
             "   delete from BPAPackageScheduleList where packageid=@packageid" &
             "   delete from BPAPackageWebService where packageid=@packageid" &
             "   delete from BPAPackageWebApi where packageid=@packageid" &
             "   delete from BPAPackageEnvironmentVar where packageid=@packageid" &
             "   delete from BPAPackageFont where packageid=@packageid" &
             "   delete from BPAPackageTile where packageid=@packageid" &
             "   delete from BPAPackageDashboard where packageid=@packageid" &
             "   select @packageid" &
             " end"

            With cmd.Parameters
                .AddWithValue("@packageid",
                 IIf(pkg.IdAsInteger = 0, DBNull.Value, pkg.IdAsInteger))
                .AddWithValue("@name", pkg.Name)
                .AddWithValue("@description", pkg.Description)
                .AddWithValue("@userid", userId)
                .AddWithValue("@created", pkg.Created)
            End With

            Try
                pkg.Id = DirectCast(con.ExecuteReturnScalar(cmd), Integer)

            Catch ex As SqlException When ex.Number = DatabaseErrorCode.UniqueConstraintError
                Throw New NameAlreadyExistsException(
                 My.Resources.clsServer_ThePackageName0AlreadyExists, pkg.Name)

            End Try

        End Using

        Dim inserters As New Dictionary(Of String, SqlCommand) ' cache of satellite sql commands
        For Each component As PackageComponent In pkg.Members
            Dim cmd As SqlCommand = Nothing
            If Not inserters.TryGetValue(component.TypeKey, cmd) OrElse cmd.CommandText = "" Then
                cmd = New SqlCommand()
                Select Case component.Type

                    Case PackageComponentType.Process, PackageComponentType.BusinessObject
                        cmd.CommandText =
                         "insert into BPAPackageProcess (packageid,processid) " &
                         "values (@ident, @entityid)"

                    Case PackageComponentType.Credential
                        cmd.CommandText =
                         "insert into BPAPackageCredential(packageid, credentialid) " &
                         "values (@ident, @entityid)"

                    Case PackageComponentType.Queue
                        cmd.CommandText =
                         "insert into BPAPackageWorkQueue (packageid,queueident) " &
                         "values (@ident, @entityid)"

                    Case PackageComponentType.Schedule
                        cmd.CommandText =
                         "insert into BPAPackageSchedule (packageid,scheduleid) " &
                         "values (@ident, @entityid)"

                    Case PackageComponentType.Calendar
                        cmd.CommandText =
                         "insert into BPAPackageCalendar (packageid,calendarid) " &
                         "values (@ident, @entityid)"

                    Case PackageComponentType.ScheduleList
                        cmd.CommandText =
                         "insert into BPAPackageScheduleList (packageid,schedulelistid) " &
                         "values (@ident, @entityid)"

                    Case PackageComponentType.WebService
                        cmd.CommandText =
                         "insert into BPAPackageWebService (packageid,webserviceid) " &
                         "values (@ident, @entityid)"

                    Case PackageComponentType.WebApi
                        cmd.CommandText =
                            "insert into BPAPackageWebApi (packageid,webapiid) " &
                            "values (@ident, @entityid)"

                    Case PackageComponentType.EnvironmentVariable
                        cmd.CommandText =
                         "insert into BPAPackageEnvironmentVar (packageid,name) " &
                         "values (@ident, @entityid)"

                    Case PackageComponentType.Font
                        cmd.CommandText =
                         "insert into BPAPackageFont (packageid, name) " &
                         "values (@ident, @entityid)"

                    Case PackageComponentType.Tile
                        cmd.CommandText =
                         "insert into BPAPackageTile (packageid, tileid) " &
                         "values (@ident, @entityid)"

                    Case PackageComponentType.Dashboard
                        cmd.CommandText =
                         "insert into BPAPackageDashboard (packageid, dashid) " &
                         "values (@ident, @entityid)"

                End Select
                cmd.Parameters.AddWithValue("@ident", pkg.Id)
                inserters(component.TypeKey) = cmd

            End If
            If cmd.CommandText <> "" Then
                With cmd.Parameters
                    Dim param As SqlParameter = .AddWithValue("@entityid", component.Id)
                    con.Execute(cmd)
                    .Remove(param)
                End With
            End If
        Next

        ' Finally record the save on an audit event
        RecordReleaseManagerAuditEvent(con, eventCode, pkg)

    End Sub

    ''' <summary>
    ''' Deletes the given package and any related data.
    ''' </summary>
    ''' <param name="con">The connection over which to delete the package.</param>
    ''' <param name="pkg">The package to delete.</param>
    Private Sub DeletePackage(ByVal con As IDatabaseConnection, ByVal pkg As clsPackage)
        Dim cmd As New SqlCommand("delete from BPAPackage where id = @id")
        cmd.Parameters.AddWithValue("@id", pkg.Id)
        con.Execute(cmd)
        RecordReleaseManagerAuditEvent(con, RelmanEventCode.DeletePackage, pkg)
    End Sub

#End Region

#Region " Private Release Authoring methods "


    ''' <summary>
    ''' Checks if the release name in the given release is valid.
    ''' </summary>
    ''' <param name="con">The connection to use.</param>
    ''' <param name="rel">The release to check</param>
    ''' <returns>True if the name on the release is valid; False if such a name
    ''' already exists.</returns>
    Private Function IsValidRelease(
     ByVal con As IDatabaseConnection, ByVal rel As clsRelease) As Boolean
        Dim cmd As New SqlCommand(
         " select 1 from BPARelease where packageid = @packageid and name = @name")
        With cmd.Parameters
            .AddWithValue("@packageid", rel.Package.Id)
            .AddWithValue("@name", rel.Name)
        End With
        Return (BPUtil.IfNull(con.ExecuteReturnScalar(cmd), 0) = 0)
    End Function

    ''' <summary>
    ''' Creates the specified release on the database.
    ''' </summary>
    ''' <param name="con">The connection over which the release should be created.
    ''' </param>
    ''' <param name="rel">The release to create.</param>
    ''' <param name="evt">The event code for the audit event which should be recorded
    ''' as a result of this release creation (could be any of :- <list>
    ''' <item><see cref="RelmanEventCode.CreateRelease"/></item>
    ''' <item><see cref="RelmanEventCode.ImportReleaseExistingPackage"/></item>
    ''' <item><see cref="RelmanEventCode.ImportReleaseNewPackage"/></item></list>)
    ''' </param>
    Private Sub CreateRelease(ByVal con As IDatabaseConnection,
     ByVal rel As clsRelease, ByVal evt As RelmanEventCode)
        Dim userId As Guid = GetUserID(con, rel.UserName)
        Dim cmd As New SqlCommand()
        cmd.CommandText =
         " insert into BPARelease (packageid, name, created, userid, notes, local) " &
         " values (@packageid, @name, @created, @userid, @notes, @areyoulocal) " &
         " select cast(scope_identity() as int)"
        With cmd.Parameters
            .AddWithValue("@packageid", rel.Package.Id)
            .AddWithValue("@name", rel.Name)
            .AddWithValue("@created", clsDBConnection.UtilDateToSqlDate(rel.Created))
            .AddWithValue("@userid", userId)
            .AddWithValue("@notes", rel.ReleaseNotes)
            .AddWithValue("@areyoulocal", rel.Local)
        End With
        Try
            rel.Id = con.ExecuteReturnScalar(cmd)
        Catch sqlex As SqlException When sqlex.Number = DatabaseErrorCode.UniqueConstraintError
            Throw New NameAlreadyExistsException(
             My.Resources.clsServer_AReleaseWithTheName0AlreadyExistsForThisPackage, rel.Name)
        End Try
        cmd.CommandText =
         " insert into BPAReleaseEntry (releaseid, typekey, entityid, name)" &
         " values (@releaseid, @typekey, @id, @name)" &
         " select cast(scope_identity() as int)"

        ' releaseid stays the same for each member - typekey, id and name are different
        ' for each component, so separate them into their own objects.
        Dim typeKeyParam As SqlParameter, idParam As SqlParameter, nameParam As SqlParameter
        With cmd.Parameters
            .Clear()
            .AddWithValue("@releaseid", rel.Id)
            typeKeyParam = .Add("@typekey", SqlDbType.NVarChar)
            idParam = .Add("@id", SqlDbType.NVarChar)
            nameParam = .Add("@name", SqlDbType.NVarChar)
        End With

        For Each component As PackageComponent In rel.Members
            typeKeyParam.Value = component.TypeKey
            If component.Id Is Nothing Then
                idParam.Value = ""
            Else
                idParam.Value = component.Id.ToString()
            End If
            nameParam.Value = component.Name
            con.Execute(cmd)
        Next

        If rel.FileName IsNot Nothing Then
            RecordReleaseManagerAuditEvent(con, evt, rel.Package, rel, Nothing,
             My.Resources.clsServer_ImportedFromFile0, rel.FileName)
        Else
            RecordReleaseManagerAuditEvent(con, evt, rel)
        End If


    End Sub

    ''' <summary>
    ''' Populates the given package with its releases.
    ''' This will add the releases to the given package.
    ''' </summary>
    ''' <param name="con">The connection over which the releases should be retrieved
    ''' </param>
    ''' <param name="pkg">The package to populate.</param>
    Private Sub PopulateReleases(ByVal con As IDatabaseConnection, ByVal pkg As clsPackage)

        Dim rels As New List(Of clsRelease)

        ' We need to treat 'entityid' as an id for the purpose of loading the release
        ' members dynamically (all components treat 'id' as their own id when loading
        ' from a data provider)
        Dim cmd As New SqlCommand(
         " select" &
         "   r.id as releaseid," &
         "   r.name as releasename," &
         "   r.created," &
         "   r.local," &
         "   u.username," &
         "   r.notes, " &
         "   re.typekey," &
         "   re.entityid as id," &
         "   re.name" &
         " from BPARelease r" &
         "   join BPAUser u on r.userid = u.userid" &
         "   join BPAReleaseEntry re on re.releaseid = r.id" &
         " where packageid=@packageid"
        )
        cmd.Parameters.AddWithValue("@packageid", pkg.Id)

        Dim releases As New Dictionary(Of Integer, clsRelease)
        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            While reader.Read()
                Dim rel As clsRelease = Nothing
                Dim relId As Integer = prov.GetValue("releaseid", 0)
                If Not releases.TryGetValue(relId, rel) Then
                    ' We can't just use the provider constructor because release thinks "id" is
                    ' for it,  whereas all the release entries think ID relates to them.
                    rel = New clsRelease(pkg, prov.GetString("releasename"),
                     prov.GetValue("created", Date.UtcNow), prov.GetString("username"), False)
                    rel.Id = relId
                    rel.Description = prov.GetString("notes")
                    rel.Local = prov.GetValue("local", True)
                    releases(relId) = rel
                End If
                ' Add the member in this line to the release.
                rel.Members.Add(
                 PackageComponentType.NewComponent(rel, prov.GetString("typekey"), prov))
            End While
        End Using
        pkg.Releases.Clear()
        pkg.AddReleases(releases.Values)

    End Sub

#End Region

#Region " Private Release Importing methods "

    ''' <summary>
    ''' Runs release import background job.
    ''' </summary>
    ''' <param name="jobId">Identifier of the background job for the import</param>
    ''' <param name="notifier">The notifier used to signal that updates have been made
    ''' as the job progresses</param>
    ''' <param name="release">The release to import</param>
    ''' <param name="unlockProcesses">Controls whether locked processes are unlocked</param>
    Private Sub RunReleaseImport(jobId As Guid, notifier As BackgroundJobNotifier,
                                 release As clsRelease, unlockProcesses As Boolean)
        UpdateBackgroundJob(jobId, notifier, 10, BackgroundJobStatus.Running)

        Try
            Using con = GetConnection()
                con.BeginTransaction()

                Dim componentsImported = ImportRelease(jobId, notifier, con, release, unlockProcesses)
                con.CommitTransaction()
                If Not componentsImported Then
                    UpdateBackgroundJob(jobId, notifier, 100, BackgroundJobStatus.Success, My.Resources.clsServer_NoComponentsImportedAllComponentsWereSkipped)
                Else
                    ' Make id of release available using result data - note that this will be zero if
                    ' importing a legacy release (no package or release record)
                    UpdateBackgroundJob(jobId, notifier, 100, BackgroundJobStatus.Success, resultData:=release.Id)
                End If
            End Using
        Catch ex As Exception
            UpdateBackgroundJob(jobId, notifier, 0, BackgroundJobStatus.Failure, exception:=ex)
            Log.Error(ex, "Error importing release")
        End Try
    End Sub

    ''' <summary>
    ''' Imports the given release, updating the background job as it progresses.
    ''' </summary>
    ''' <param name="jobId">Identifier of the background job for the import</param>
    ''' <param name="notifier">The notifier used to signal that updates have been made
    ''' as the job progresses</param>
    ''' <param name="con">The connection over which to import the release</param>
    ''' <param name="rel">The release to import</param>
    ''' <remarks><para>When this method returns normally, the release will have been
    ''' updated with current data - ie. any components which were skipped and not
    ''' processed will be removed from the release, any components for which there
    ''' existed records on the database will have their IDs and associated data
    ''' updated with the records that were updated as a result of the import. Any
    ''' new objects will have their generated IDs set into the component and
    ''' associated data.</para>
    ''' <para>Also, if the release is not a <see cref="clsRelease.IsLegacy">legacy</see>
    ''' release, a record will have been created for it on the BPARelease table.
    ''' </para></remarks>
    ''' <returns>A boolean value indicating whether any components were imported</returns>
    Private Function ImportRelease(jobId As Guid, notifier As BackgroundJobNotifier,
                                   con As IDatabaseConnection, rel As clsRelease,
                                   unlockProcesses As Boolean) As Boolean
        ' Original count of the release's members - if components are being skipped,
        ' then they are deleted from the release, so we need to save this now.
        Dim totalComponentCount As Integer = rel.Members.Count
        Dim currentComponentNumber = 0
        Dim componentsImported = False
        Dim progress = 10
        Dim versionDataNames As New HashSet(Of String)
        ' Prepare release data for import
        rel.InitialiseComponentsForImport()

        'us-3107/bg-2150 - with some skill imports the only course of action is to not import, and to abort import of all components;
        'NOTE the original implementation had a bug which removed skip from any subsequent import containing a credential
        Dim abortComponent As Boolean = False
        If rel IsNot Nothing AndAlso rel.IsSkill = True Then
            abortComponent = (From comp In rel Where comp.Modifications.ContainsKey(ModificationType.Skip)).Count() > 0
        End If

        ' Create list of components to process - a separate list is used so that we
        ' can remove items from the release while iterating. We import the components 
        ' in specific order as some components are reliant on referenced components 
        ' being saved first.
        Dim components = (From comp In rel
                          Let importOrder = GetImportOrder(comp)
                          Order By importOrder
                          Select comp).ToList
        For Each comp As PackageComponent In components
            UpdateBackgroundJob(jobId, notifier, progress, BackgroundJobStatus.Running, My.Resources.clsServer_Processing + comp.Name)
            currentComponentNumber += 1

            ' If this component is a member of a group, we can create it on the root of the tree and it will be moved into the correct
            ' group when the group is imported.
            Dim isMemberOfAGroup = components.OfType(Of GroupComponent).Any(Function(x) x.Any(Function(c) c.IdAsGuid = comp.IdAsGuid))
            Dim imported = ImportComponent(con, rel, unlockProcesses, progress, comp, isMemberOfAGroup, abortComponent)

            If imported AndAlso Not String.IsNullOrEmpty(comp.VersionDataName) Then
                versionDataNames.Add(comp.VersionDataName)
            End If
            componentsImported = imported OrElse componentsImported
            ' Set progress between 10 and 85 as components imported
            progress = 10 + CInt(currentComponentNumber / totalComponentCount * 75)
        Next

        If componentsImported Then
            ' Create package if we have imported any components (ie. not all were set to be skipped
            ' in the conflict resolution stage)
            If Not rel.IsLegacy Then
                ' We're importing a new release (as opposed to a temp wrapper around a legacy process
                ' / object XML file)
                UpdateBackgroundJob(jobId, notifier, 85, BackgroundJobStatus.Running,
                    My.Resources.clsServer_UpdatingPackageOnDatabase)
                ' So now we want to create a package for this release, if there isn't one already.
                ' But first, we need to prepare it - ie. give it a unique name, set the user to the
                ' importing user, ensure the package is there - that sort of thing
                CreateIncomingRelease(con, rel)
            End If
        End If

        UpdateMonitoringVersionData(con, versionDataNames)

        Return componentsImported

    End Function

    ''' <summary>
    ''' Updates version data that is monitored to detect when updates have
    ''' been made. We defer the version update until all changes required
    ''' by the release have been made to the database. It is then updated
    ''' immediately before the transaction is committed. This minimises
    ''' the time during which the BPADataTracker database table will be locked
    ''' due to updates made within the current transaction.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="dataNames">The names of the types of data that have changed</param>
    Private Sub UpdateMonitoringVersionData(con As IDatabaseConnection, dataNames As HashSet(Of String))
        For Each dataName In dataNames
            IncrementDataVersion(con, dataName)
        Next
    End Sub

    ''' <summary>
    ''' Gets number indicating order in which a component should be processed during
    ''' the import. This ensures that depended-on components are processed first. 
    ''' Credentials need processes to be imported first (in case of new IDs), schedules 
    ''' need calendars to be imported first, dashboards need tiles and groups need 
    ''' their members imported first. 
    ''' </summary>
    ''' <param name="comp">The package component</param>
    ''' <returns>A number indicating the order (currently either 1 or 2)</returns>
    Private Function GetImportOrder(comp As PackageComponent) As Integer
        Dim hasDependees = comp.Type = PackageComponentType.Schedule _
            OrElse comp.Type = PackageComponentType.Credential _
            OrElse comp.Type = PackageComponentType.Dashboard _
            OrElse TypeOf comp Is GroupComponent
        If Not hasDependees Then
            ' First pass
            Return 1
        Else
            ' Second pass
            Return 2
        End If
    End Function


    ''' <summary>
    ''' Imports a single component from the specified release.
    ''' </summary>
    ''' <param name="con">The connection to the database on which the component
    ''' should be imported.</param>
    ''' <param name="rel">The release being imported.</param>
    ''' <param name="progress">The amount of progress to report to the monitor
    ''' </param>
    ''' <param name="comp">The component to be imported</param>
    ''' <param name="isInGroup">Is the component a member of a group in this release</param>
    Private Function ImportComponent(con As IDatabaseConnection, rel As clsRelease, unlockProcesses As Boolean, progress As Integer, comp As PackageComponent, isInGroup As Boolean, abort As Boolean) As Boolean

        ' If we're skipping import of this component, we want to remove it
        ' from the release (so that when we save the release to the database
        ' it does not include the components that weren't imported)
        ' We must also remove it from the package (which relies on foreign key
        ' relationships, and thus can't work if the component is not imported)
        If comp.Modifications.ContainsKey(ModificationType.Skip) Or abort = True Then
            rel.Remove(comp)
            If rel.Package IsNot Nothing Then rel.Package.Remove(comp)
            Return False
        End If

        Dim auditComment As String = Nothing
        Dim hasImported As Boolean = True

        Select Case comp.Type
            Case PackageComponentType.Process, PackageComponentType.BusinessObject
                ImportProcessOrVbo(con, rel, unlockProcesses,
                 DirectCast(comp, ProcessComponent), isInGroup)
            Case PackageComponentType.Schedule
                ImportSchedule(con, rel, DirectCast(comp, ScheduleComponent))
            Case PackageComponentType.EnvironmentVariable
                hasImported = ImportEnvironmentVariable(con, rel, DirectCast(comp, EnvironmentVariableComponent), auditComment)
            Case PackageComponentType.Queue
                ImportQueue(con, rel, DirectCast(comp, WorkQueueComponent))
            Case PackageComponentType.Credential
                ImportCredential(con, rel, DirectCast(comp, CredentialComponent))
            Case PackageComponentType.Calendar
                ImportCalendar(con, rel, DirectCast(comp, CalendarComponent))
            Case PackageComponentType.WebService
                ImportWebService(con, rel, DirectCast(comp, WebServiceComponent))
            Case PackageComponentType.WebApi
                ImportWebApi(con, rel, DirectCast(comp, WebApiComponent))
            Case PackageComponentType.Font
                ImportFont(con, rel, DirectCast(comp, FontComponent))
            Case PackageComponentType.DataSource
                ImportDataSource(con, rel, DirectCast(comp, DataSourceComponent))
            Case PackageComponentType.Tile
                ImportTile(con, rel, DirectCast(comp, TileComponent))
            Case PackageComponentType.Dashboard
                ImportDashboard(con, rel, DirectCast(comp, DashboardComponent))
            Case PackageComponentType.Skill
                ImportSkill(con, rel, DirectCast(comp, SkillComponent))
            Case Else
                If TypeOf comp Is GroupComponent Then
                    ImportGroup(con, rel, DirectCast(comp, GroupComponent))
                Else
                    Throw New BluePrismException(My.Resources.clsServer_0HasNoImportProcessingImplemented,
                     comp.Type.Key)
                End If

        End Select

        If hasImported = True Then
            If rel.IsLegacy AndAlso rel.FileName IsNot Nothing Then
                ' If it's a legacy import, then we don't create an incoming release.
                ' So save the file information with the component audit record (since
                ' legacy releases contained only 1 component, this is pretty safe)
                RecordReleaseManagerAuditEvent(con, RelmanEventCode.ImportComponent,
                 rel.Package, rel, comp, My.Resources.clsServer_ImportedFromFile0, rel.FileName)
            Else
                RecordReleaseManagerAuditEvent(con,
                     RelmanEventCode.ImportComponent, rel.Package, rel, comp, auditComment)
            End If
        End If

        Return True
    End Function

    ''' <summary>
    ''' Creates a release record for the given incoming release.
    ''' This ensures that a package record exists for the release first, and that
    ''' the release's metadata is up to date and correct (including new
    ''' auto-generated name, and importing user and import date instead of creating
    ''' user and creation date)
    ''' </summary>
    ''' <param name="con">The connection over which to create the release record.
    ''' </param>
    ''' <param name="rel">The incoming release to be created.</param>
    Private Sub CreateIncomingRelease(ByVal con As IDatabaseConnection, ByVal rel As clsRelease)
        Dim cmd As New SqlCommand()

        ' First off, we locate the package
        Dim pkg As clsPackage = rel.Package
        cmd.CommandText = "select id from BPAPackage where name=@packagename"
        cmd.Parameters.AddWithValue("@packagename", pkg.Name)
        Dim id As Integer = BPUtil.IfNull(con.ExecuteReturnScalar(cmd), 0)
        Dim eventCode As RelmanEventCode ' the event code to use to record the import release
        ' If id is zero, that means that it's a new package
        If id = 0 Then
            ' No package exists... well, let's remedy that, shall we?
            ' First, we want to make sure it has no ID - if it has an ID from the environment
            ' it came from, that would *not* be a good one to use in this environment.
            eventCode = RelmanEventCode.ImportReleaseNewPackage
            pkg.Id = Nothing
            SavePackage(con, pkg)
            ' SavePackage() sets the new ID into the package so we can safely
            ' use that from now on

        Else
            ' Well, we want to use that ID, then - the one that's come in with
            ' the release is the one local to the source environment. We need to
            ' use the one that's local to the target environment, ie. this one.
            eventCode = RelmanEventCode.ImportReleaseExistingPackage
            pkg.Id = id

        End If

        ' So now we want to get a unique release name for this release...
        cmd.CommandText = "select 1 from BPARelease where packageid=@packageid and name=@name"
        Dim paramName As New SqlParameter("@name", SqlDbType.NVarChar)
        With cmd.Parameters
            .Clear()
            .AddWithValue("@packageid", pkg.Id)
            .Add(paramName)
        End With
        Dim foundUniqueName As Boolean = False

        ' Set the base name for the release
        Dim baseName As String = rel.Name
        ' Then we add a suffix with an incrementing number (starting at [1])
        Dim iter As Integer = 0
        Do
            iter += 1
            rel.Name = baseName & " [" & iter & "]"
            paramName.Value = rel.Name
            ' Keep going until we get a unique name.
        Loop While con.ExecuteReturnScalar(cmd) IsNot Nothing

        ' Okay, so now, we have a package on the database, and we have a release with
        ' a unique name to add to the database. 
        ' Next job - the user ID should be the importing user and the 'created' date
        ' should be the date that it was imported to the database.
        ' That's all handled by the SetUserToImportingUser method.
        rel.SetUserToImportingUser(GetLoggedInUserName())

        ' All the components on the release should be up to date, and the release
        ' metadata is now all correct, so add the release.
        CreateRelease(con, rel, eventCode)

    End Sub

    ''' <summary>
    ''' Imports the given process component, representing a process or VBO, from the
    ''' given release into the database over the specified connection.
    ''' </summary>
    ''' <param name="con">The connection to import the process over</param>
    ''' <param name="rel">The release from which the process/vbo is coming.</param>
    ''' <param name="unlockProcesses">True to unlock any locked processes/VBOs before
    ''' importing over the top of them.</param>
    ''' <param name="procComp">The process / vbo component which is to be imported.
    ''' </param>
    ''' <param name="isInGroup">If this process / vbo is a member of a group component in this release</param>
    ''' <exception cref="Server.Domain.Models.AlreadyLockedException">If the process/VBO is locked and
    ''' either <paramref name="unlockProcesses"/> is <c>false</c> or it could not be
    ''' unlocked.</exception>
    Private Sub ImportProcessOrVbo(ByVal con As IDatabaseConnection,
     ByVal rel As clsRelease, ByVal unlockProcesses As Boolean,
     ByVal procComp As ProcessComponent, isInGroup As Boolean)

        Dim proc As clsProcess = procComp.AssociatedProcess
        Dim type As PackageComponentType = procComp.Type
        Dim typeLabel As String = PackageComponentType.GetLocalizedFriendlyName(type)

        Dim mods As IDictionary(Of ModificationType, Object) = procComp.Modifications

        ' Let's get the mods out of the way first.
        Dim data As Object = Nothing ' placeholder for TryGetValue() calls on mods

        ' We change the existing name first (if it's set to be changed)
        If mods.TryGetValue(ModificationType.ExistingName, data) Then
            Dim newName As String = DirectCast(data, String)

            ' First, we get the process with the same name as this process (which must
            ' exist, or we wouldn't be changing the existing name)
            Dim existingId As Guid = Nothing
            Dim sameType As Boolean = True
            IsProcessNameUnique(con, Nothing, existingId, procComp.Name, procComp.IsBusinessObject, Nothing, Nothing)
            If existingId = Nothing Then
                sameType = False
                IsProcessNameUnique(con, Nothing, existingId, procComp.Name, Not procComp.IsBusinessObject, Nothing, Nothing)
                If existingId = Nothing Then Throw New BluePrismException(
                    My.Resources.clsServer_SetToModifyExistingProcessObjectNameButNoProcessObjectCalled0Exists,
                     procComp.Name)
            End If

            ' The rest is largely taken from frmProcessImport
            Dim existingXML As String = GetProcessXML(con, existingId)

            Dim doc As New ReadableXmlDocument(existingXML)

            'Change name within xml
            doc.SelectSingleNode("process/@name").InnerText = newName

            ' And re-get the XML
            Dim newXml As String = doc.OuterXml

            Dim procVer As String = doc.SelectSingleNode("process/@version").InnerText
            Dim procDesc As String = doc.SelectSingleNode("process/@narrative").InnerText

            ' We have everything we need - do the actual work
            ' First, ensure that the processes are unlocked if necessary.
            If unlockProcesses Then
                UnlockProcess(con, existingId)
            End If

            LockProcess(con, existingId)
            Try

                Dim clashTypeLabel As String = PackageComponentType.GetLocalizedFriendlyName(procComp.ClashType, capitalize:=True)
                EditProcess(con, existingId, procComp.IsBusinessObject, newName, procVer, procDesc, newXml, Nothing, procComp.Dependencies)
                AuditRecordProcessOrVboEvent(con, ProcessOrVboEventCode.Modify,
                 procComp.IsBusinessObject, existingId,
                 String.Format(
                  My.Resources.clsServer_0RenamedFrom1WhileImportingFromRelease2,
                  IIf(sameType, typeLabel, clashTypeLabel), procComp.Name, rel.Name),
                 newXml,
                 String.Format(My.Resources.clsServer_0RenamedByImportingRelease1,
                               IIf(sameType, typeLabel, clashTypeLabel), rel.Name)
                )

            Finally
                'Lock process does not check the permissions, so calling delete process lock directly 
                DeleteProcessLock(con, existingId)
            End Try

        End If

        ' Then we deal with the incoming changes..
        If mods.TryGetValue(ModificationType.IncomingId, data) Then
            ' Set the ID into both the component and the associated process
            Dim newId As Guid = DirectCast(data, Guid)
            procComp.Id = newId
            proc.Id = newId
        End If
        If mods.TryGetValue(ModificationType.IncomingName, data) Then
            ' Set the name into the component and associated process
            procComp.Name = DirectCast(data, String)
            proc.Name = procComp.Name
        End If

        ' If the mods contain Retire / Publish flags - that indicates that the 
        ' incoming component had retire/publish set *and* the user elected to
        ' retire/publish the process after import. Ensure that the component is
        ' up to date with the user's decision and set the attributes on the
        ' process accordingly.
        procComp.Retired = mods.ContainsKey(ModificationType.Retire)
        If procComp.Retired Then _
            proc.Attributes = (proc.Attributes Or ProcessAttributes.Retired)

        procComp.Published = mods.ContainsKey(ModificationType.Publish)
        If procComp.Published Then _
            proc.Attributes = (proc.Attributes Or ProcessAttributes.Published)

        If proc.Attributes = ProcessAttributes.Published AndAlso
         Not mods.ContainsKey(ModificationType.Publish) Then
            proc.Attributes = ProcessAttributes.None
        End If

        ' If we're overwriting an existing process, then do so
        Dim id As Guid = proc.Id
        Dim name As String = proc.Name
        Dim ver As String = proc.Version
        Dim desc As String = proc.Description

        If mods.ContainsKey(ModificationType.OverwritingExisting) Then

            Dim originalAttributes = GetProcessAttributes(id)

            ' First, ensure that the processes are unlocked if necessary.
            If unlockProcesses Then UnlockProcess(con, id)
            ' Also ensure there's no autosaves - this will be rolled back with
            ' everything else if the process remains locked
            DeleteProcessAutoSaves(con, id)

            LockProcess(con, id)
            Try
                Dim newXml As String = proc.GenerateXML(False)
                EditProcess(con, id, procComp.IsBusinessObject, name, ver, desc, newXml, Nothing, procComp.Dependencies)
                AuditRecordProcessOrVboEvent(con, ProcessOrVboEventCode.Modify,
                 procComp.IsBusinessObject, id, String.Format(
                 My.Resources.clsServer_OverwritingExisting0With0ImportedFromRelease1,
                 typeLabel, rel.Name), newXml,
                 String.Format(My.Resources.clsServer_0OverwrittenByImportingARelease, typeLabel)
                )

                ' work out if we need to change attributes
                If proc.Attributes <> originalAttributes AndAlso Not CBool(originalAttributes And ProcessAttributes.Retired) Then
                    OverwriteProcessAttributes(con, id, originalAttributes, proc.Attributes)
                    AuditRecordProcessOrVboEvent(con, ProcessOrVboEventCode.ChangedAttributes,
                      procComp.IsBusinessObject, id, String.Format(
                     My.Resources.clsServer_ChangedAttributesTo0WhileImportingARelease, proc.Attributes),
                     Nothing, My.Resources.clsServer_AttributesChanged)
                End If

            Finally
                'Lock process does not check the permissions, so calling delete process lock directly 
                DeleteProcessLock(con, id)
            End Try

        Else ' ie. we're creating a new one
            Dim newXml As String = proc.GenerateXML(False)
            WriteProcess(con, id, name, ver, desc, newXml,
             False, procComp.IsBusinessObject, procComp.Dependencies, Guid.Empty, isInGroup, False)
            AuditRecordProcessOrVboEvent(con, ProcessOrVboEventCode.Import,
             procComp.IsBusinessObject, id, String.Format(
             My.Resources.clsServer_CreatingNew0ImportedFromRelease1, typeLabel, rel.Name),
             newXml, String.Format(My.Resources.clsServer_0CreatedByImportingARelease, typeLabel))

            ' We only want to set these if the attributes are actually set to something.
            ' Otherwise, we want to leave them as they are at the moment
            If proc.Attributes <> ProcessAttributes.None Then
                SetProcessAttributes(con, id, proc.Attributes)
                AuditRecordProcessOrVboEvent(con, ProcessOrVboEventCode.ChangedAttributes,
                  procComp.IsBusinessObject, id, String.Format(
                 My.Resources.clsServer_ChangedAttributesTo0WhileImportingARelease, proc.Attributes),
                 Nothing, My.Resources.clsServer_AttributesChanged)
            End If
        End If

        ' Update exception types table with any used within the imported process/vbo
        For Each stage As clsExceptionStage In proc.GetStages(StageTypes.Exception)
            If stage.ExceptionType IsNot Nothing Then
                AddExceptionType(stage.ExceptionType.Trim(), con)
            End If
        Next

    End Sub

    ''' <summary>
    ''' Imports the given web service over the given connection.
    ''' This will overwrite an existing web service with the same name, rename the
    ''' existing web service or rename the incoming web service, depending on whether
    ''' the <see cref="PackageComponent.Modifications"/> of the component contain
    ''' <see cref="PackageComponent.ModificationType.OverwritingExisting"/>,
    ''' <see cref="PackageComponent.ModificationType.ExistingName"/> or
    ''' <see cref="PackageComponent.ModificationType.IncomingName"/> respectively.
    ''' </summary>
    ''' <param name="con">The connection over which to import the component.
    ''' This connection should be used exclusively to allow full rollback in case of
    ''' any database errors.</param>
    ''' <param name="rel">The release that this component is being imported on
    ''' behalf of. Primarily used for error messages.</param>
    ''' <param name="wsComp">The web service to import.</param>
    Private Sub ImportWebService(ByVal con As IDatabaseConnection, ByVal rel As clsRelease,
     ByVal wsComp As WebServiceComponent)
        ' We want to be certain that the IDs match, since we use the component's
        ' ID later while creating the incoming release record.
        Dim data As Object = Nothing
        Dim wsIncoming As clsWebServiceDetails = wsComp.AssociatedWebService

        If wsComp.Modifications.TryGetValue(ModificationType.ExistingName, data) Then
            Dim name As String = DirectCast(data, String)
            ' Get the web service, and give its ID to the incoming service
            Dim ws As clsWebServiceDetails = GetWebServiceDefinition(con, wsComp.Name)
            wsIncoming.Id = ws.Id

            ' Thereafter, give it the new name and a new ID and save it
            ws.FriendlyName = name
            ws.Id = Guid.NewGuid()
            SaveWebServiceDefinition(con, ws)

        ElseIf wsComp.Modifications.TryGetValue(ModificationType.IncomingName, data) Then
            Dim name As String = DirectCast(data, String)
            ' Give the incoming service a new ID so it definitely doesn't clash with
            ' the existing service, and give it its shiny new name
            wsIncoming.Id = Guid.NewGuid()
            wsIncoming.FriendlyName = name

        ElseIf wsComp.Modifications.ContainsKey(ModificationType.OverwritingExisting) Then
            ' Give the incoming service the same ID as the similarly named service on the system
            wsIncoming.Id = GetWebServiceId(con, wsIncoming.FriendlyName)

        End If
        ' At this point, wsIncoming represents the service we want to save. Set its
        ' details into the component, so we know that it's up to date
        wsComp.Id = wsIncoming.Id
        wsComp.Name = wsIncoming.FriendlyName

        ' And save it - this determines by ID whether the web service currently exists or not.
        SaveWebServiceDefinition(con, wsComp.AssociatedWebService)
    End Sub

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Imports the given web api over the given connection.
    ''' This will overwrite an existing web api with the same name, rename the
    ''' existing api or rename the incoming api, depending on whether
    ''' the <see cref="PackageComponent.Modifications"/> of the component contain
    ''' <see cref="PackageComponent.ModificationType.OverwritingExisting"/>,
    ''' <see cref="PackageComponent.ModificationType.ExistingName"/> or
    ''' <see cref="PackageComponent.ModificationType.IncomingName"/> respectively.
    ''' </summary>
    ''' <param name="con">The connection over which to import the component.
    ''' This connection should be used exclusively to allow full rollback in case of
    ''' any database errors.</param>
    ''' <param name="rel">The release that this component is being imported on
    ''' behalf of. Primarily used for error messages.</param>
    ''' <param name="apiComp">The web api to import.</param>
    ''' -----------------------------------------------------------------------------
    Private Sub ImportWebApi(ByVal con As IDatabaseConnection, ByVal rel As clsRelease,
     ByVal apiComp As WebApiComponent)
        Dim data As Object = Nothing
        ' As we cannot amend a webapi's id, we use property placeholders until we're 
        ' ready to recreate and overwrite the api at the end of this method.
        Dim incomingId As Guid = Guid.Empty
        Dim incomingName As String = String.Empty

        Dim apiToSave As WebApi = Nothing
        Dim webApiDataAccess = New WebApiDataAccess(con)

        If apiComp.Modifications.TryGetValue(ModificationType.IncomingName, data) Then
            Dim name As String = DirectCast(data, String)
            ' Give the incoming api a new ID so it definitely doesn't clash with
            ' the existing api, and give it its shiny new name
            incomingId = Guid.NewGuid()
            incomingName = name

        ElseIf apiComp.Modifications.ContainsKey(ModificationType.OverwritingExisting) Then
            ' Give the incoming api the same ID as the similarly named api on the system
            Dim name As String = apiComp.AssociatedWebApi.Name
            Dim dataAccess As New WebApiDataAccess(con)
            incomingId = dataAccess.GetWebApiId(name)
            incomingName = name
        Else
            incomingId = apiComp.IdAsGuid
            incomingName = apiComp.Name
        End If

        Dim apiIncoming As WebApi = apiComp.AssociatedWebApi
        ' At this point, apiIncoming represents the api we want to save except its Id
        ' and name may need to change. We cannot amend a WebApi's Id, so must recreate 
        ' the api with these values and overwrite the existing one. 

        ' First, set its details into the component, so we know that it's up to date
        apiComp.Id = incomingId
        apiComp.Name = incomingName

        apiToSave = New WebApi(incomingId, ' from conflict resolution
                               incomingName, ' from conflict resolution
                               apiIncoming.Enabled, ' from incoming api
                               apiIncoming.Configuration) ' from incoming api

        ' And save it - this determines by ID whether the web api currently exists or not.
        webApiDataAccess.Save(apiToSave)
        UpdateWebApiRefs(con, apiToSave.Name)
    End Sub

    ''' <summary>
    ''' Imports the given schedule over the given connection
    ''' </summary>
    ''' <param name="con">The connection over which to import the component.
    ''' This connection should be used exclusively to allow full rollback in case of
    ''' any database errors.</param>
    ''' <param name="rel">The release that this component is being imported on
    ''' behalf of. Primarily used for error messages.</param>
    ''' <param name="comp">The schedule to import.</param>
    Private Sub ImportCalendar(ByVal con As IDatabaseConnection,
     ByVal rel As clsRelease, ByVal comp As CalendarComponent)

        Dim cal As ScheduleCalendar = comp.AssociatedCalendar

        Dim newId As Integer = 0 ' The new ID for the calendar, replacing the temp ID

        ' Let's see if we have a calendar with this name already
        ' We can safely give a null schema since we're not going to be using
        ' the calendar to test dates - it's only the data we're interested in
        Dim currCal As ScheduleCalendar = GetCalendar(con, cal.Name, Nothing)

        If currCal Is Nothing Then
            ' If there's none there, just save the new calendar to the database
            ' Make sure we get the calendar ID into the component and the calendar
            newId = CreateCalendar(con, cal, False)

        Else
            ' Otherwise, we update the existing calendar
            currCal.NonWorkingDays = cal.NonWorkingDays
            currCal.PublicHolidayGroup = cal.PublicHolidayGroup
            currCal.PublicHolidayOverrides = cal.PublicHolidayOverrides
            currCal.WorkingWeek = cal.WorkingWeek
            currCal.Description = cal.Description

            ' And update it.
            UpdateCalendar(con, currCal, False)

            newId = currCal.Id

        End If

        ' We now need to update any schedules which have dependents on this
        ' calendar. So we go through all the release's schedule components,
        ' check if their triggers relate to the old calendar ID, and replace
        ' any such triggers with one which points to the new calendar ID.
        Dim oldId As Integer = cal.Id
        For Each pc As PackageComponent In rel
            Dim schedComp As ScheduleComponent = TryCast(pc, ScheduleComponent)
            If schedComp IsNot Nothing Then
                Dim sched As ISchedule = schedComp.AssociatedSchedule
                Dim replacements As New Dictionary(Of ITrigger, TriggerMetaData)
                ' Store each affected trigger into a map with its replacement metadata
                For Each t As ITrigger In sched.Triggers.Members
                    Dim md As TriggerMetaData = t.PrimaryMetaData
                    If md.CalendarId = oldId Then
                        md.CalendarId = newId
                        replacements(t) = md
                    End If
                Next
                ' Go through each affected trigger and replace it with the new
                ' trigger from the related metadata.
                For Each t As ITrigger In replacements.Keys
                    Dim md As TriggerMetaData = replacements(t)
                    sched.Triggers.Remove(t)
                    sched.Triggers.Add(md)
                Next
            End If
        Next

        ' Let's make sure the calendar components and objects themselves are up
        ' to date too.
        cal.Id = newId
        comp.Id = newId

    End Sub

    ''' <summary>
    ''' Imports the given schedule over the given connection
    ''' </summary>
    ''' <param name="con">The connection over which to import the component.
    ''' This connection should be used exclusively to allow full rollback in case of
    ''' any database errors.</param>
    ''' <param name="rel">The release that this component is being imported on
    ''' behalf of. Primarily used for error messages.</param>
    ''' <param name="comp">The schedule to import.</param>
    Private Sub ImportSchedule(ByVal con As IDatabaseConnection,
     ByVal rel As clsRelease, ByVal comp As ScheduleComponent)

        Dim newSched As SessionRunnerSchedule = comp.AssociatedSchedule
        Dim currentScheduleData = SchedulerGetScheduleData(con, newSched.Name)


        ' If we're not overwriting, it's easy, just create it and return
        If currentScheduleData Is Nothing Then
            SchedulerCreateSchedule(con, newSched, False)
            ' Make sure the component has the new ID set in it.
            comp.Id = newSched.Id
            Return
        End If

        ' Otherwise, we want to modify the existing schedule with the new data.
        ' From the wiki :-
        '
        ' If a schedule with the same name exists within the target environment, its description,
        ' timing data and task structure is overwritten with the incoming schedule definition.
        '
        ' * Note that if there exist any tasks with the same name as those being imported with the
        '   schedule, their descriptions and their "On Success" and "On Exception" settings are 
        '   overwritten, but the list of scheduled sessions will be left alone.
        '
        ' * Also note that if the existing schedule is 'retired', it will not be rejuvenated by
        '   importing a schedule over the top of it, although its data will change - it will have
        '   to be 'unretired' manually if required. 

        ' Make sure they are relating to the same store/scheduler (and, most
        ' importantly, calendars)
        Dim currSched As SessionRunnerSchedule = New SessionRunnerSchedule(Nothing, currentScheduleData, Me.mLoggedInUser)
        currSched.Owner = newSched.Owner

        currSched.Description = comp.Description

        ' Clear the existing triggers and replace them with the incoming ones.
        currSched.Triggers.Clear()
        For Each trig As ITrigger In newSched.Triggers.Members
            currSched.Triggers.Add(trig.Copy())
        Next

        ' Tasks are a bit more awkward, we need to delete those which are not part of the
        ' incoming schedule, retaining those with the same name as any which are incoming

        ' Go through each task in the schedule - if it's not in the incoming schedule, remove it
        For Each task As ScheduledTask In New List(Of ScheduledTask)(currSched)
            If Not newSched.Contains(task.Name) Then currSched.Remove(task)
        Next

        ' Now existing schedule is left with only those tasks which exist on the incoming
        ' schedule. We need to add the new tasks and copy them into the schedule
        For Each task As ScheduledTask In newSched
            If Not currSched.Contains(task.Name) Then currSched.Add(task.Copy())
        Next

        ' The existing schedule now contains all the tasks from 'sched', with sessions
        ' in place from how it was before if tasks with the same name exist.
        ' We now need to deal with changing the still existing tasks to point to the
        ' correct tasks in their OnSuccess / OnFailure properties.
        For Each task As ScheduledTask In currSched
            With newSched(task.Name) ' With the incoming equivalent task
                Dim succ As ScheduledTask = .OnSuccess
                If succ Is Nothing Then task.OnSuccess = Nothing Else task.OnSuccess = currSched(succ.Name)

                Dim fail As ScheduledTask = .OnFailure
                If fail Is Nothing Then task.OnFailure = Nothing Else task.OnFailure = currSched(fail.Name)

                task.DelayAfterEnd = .DelayAfterEnd

                If task.Sessions.Count <> 0 Then task.ClearSessions()

                For Each newSession As ISession In .Sessions
                    Dim resourceId = GetResourceId(newSession.ResourceName)
                    task.AddSession(New ScheduledSession(0, newSession.ProcessID,
                                                         newSession.ResourceName,
                                                         resourceId,
                                                         GetEffectiveMemberPermissionsForProcess(con, newSession.ProcessID).HasPermission(mLoggedInUser, Permission.ProcessStudio.ImpliedViewProcess),
                                                         GetEffectiveMemberPermissionsForResource(con, resourceId).HasPermission(mLoggedInUser, Permission.Resources.ImpliedViewResource),
                                                         newSession.Arguments))
                Next

                task.Description = .Description
            End With
        Next

        ' Finally, we set the initial task to the appropriate one
        Dim init As ScheduledTask = newSched.InitialTask
        If init IsNot Nothing Then currSched.InitialTask = currSched(init.Name)

        ' Done, save the schedule and let's get out of here.
        SchedulerUpdateSchedule(con, currSched, False)

        ' Now the 'currSched' correctly represents the imported component,
        ' set this as the associated data in the component, and ensure that the component's
        ' ID is up to date and correct
        comp.AssociatedData = currSched
        comp.Id = currSched.Id

    End Sub

    ''' <summary>
    ''' Compares imported environmental variable to the database
    ''' </summary>
    ''' <param name="importEnvironmentVariable">The imported environmental variable and it's data</param>
    ''' <param name="currentEnvironmentVariable">The database environmental variable and it's data, if it exists</param>
    ''' <returns>True identical, False if not</returns>
    Private Function CompareImportedEnvironmentalVariableToDatabase(ByRef importEnvironmentVariable As clsEnvironmentVariable,
                                                                    ByRef currentEnvironmentVariable As clsEnvironmentVariable) As Boolean
        Dim environmentVariableIdentical As Boolean = False

        If importEnvironmentVariable IsNot Nothing AndAlso currentEnvironmentVariable IsNot Nothing _
           AndAlso importEnvironmentVariable.Name = currentEnvironmentVariable.Name Then
            ' bg-2537 - existing variables are not allowed to have their datatype or value changed, hence check description only
            If importEnvironmentVariable.Description = currentEnvironmentVariable.Description Then
                environmentVariableIdentical = True
            End If
        End If

        Return environmentVariableIdentical
    End Function

    ''' <summary>
    ''' Imports the given env var over the given connection
    ''' </summary>
    ''' <param name="con">The connection over which to import the component.
    ''' This connection should be used exclusively to allow full rollback in case of
    ''' any database errors.</param>
    ''' <param name="release">The release that this component is being imported on
    ''' behalf of. Primarily used for error messages.</param>
    ''' <param name="component">The env var to import.</param>
    Private Function ImportEnvironmentVariable(ByVal con As IDatabaseConnection,
     ByVal release As clsRelease,
     ByVal component As EnvironmentVariableComponent,
     ByRef auditComment As String) As Boolean
        Dim importedEnvironmentVariable As clsEnvironmentVariable = component.AssociatedEnvVar
        Dim currentEnvironmentVariable As clsEnvironmentVariable = GetEnvironmentVariable(con, importedEnvironmentVariable.Name)
        Dim hasImported As Boolean = True
        Dim environmentVariableIdentical As Boolean = False

        'bg-2399 and bg-2537 - import rules
        '1 no existing database variable, import YES audit YES
        '2 variable exists with same description, class as identical, import NO audit NO
        '3 variable exists with different description, class as modified, import description ONLY, import YES audit YES

        If currentEnvironmentVariable Is Nothing Then
            SaveEnvironmentVariable(con, importedEnvironmentVariable)
            Return hasImported
        End If

        environmentVariableIdentical = CompareImportedEnvironmentalVariableToDatabase(importedEnvironmentVariable, currentEnvironmentVariable)

        If environmentVariableIdentical = True Then
            hasImported = False
            Return hasImported
        End If

        Dim strDescription As String = importedEnvironmentVariable.Description
        importedEnvironmentVariable = New clsEnvironmentVariable(currentEnvironmentVariable.Name, currentEnvironmentVariable.Value, strDescription)

        Dim environmentVariablesAuditEventFactory _
         As New ModifiedEnvironmentVariablesAuditEventGenerator(
          currentEnvironmentVariable, importedEnvironmentVariable, mLoggedInUser)
        Dim auditEvent = environmentVariablesAuditEventFactory.Generate(
         EnvironmentVariableEventCode.Modified, mLoggedInUser)
        If auditEvent IsNot Nothing Then auditComment = auditEvent.Comment

        currentEnvironmentVariable.Description = importedEnvironmentVariable.Description

        component.AssociatedData = currentEnvironmentVariable

        SaveEnvironmentVariable(con, currentEnvironmentVariable)

        Return hasImported
    End Function

    ''' <summary>
    ''' Imports the given queue over the given connection
    ''' </summary>
    ''' <param name="con">The connection over which to import the component.
    ''' This connection should be used exclusively to allow full rollback in case of
    ''' any database errors.</param>
    ''' <param name="rel">The release that this component is being imported on
    ''' behalf of. Primarily used for error messages.</param>
    ''' <param name="comp">The queue to import.</param>
    Private Sub ImportQueue(ByVal con As IDatabaseConnection,
     ByVal rel As clsRelease, ByVal comp As WorkQueueComponent)
        Dim wq As clsWorkQueue = comp.AssociatedQueue

        Dim currQueue As clsWorkQueue = Nothing
        For Each q As clsWorkQueue In GetQueuesWithoutStats(con)
            If q.Name = wq.Name Then currQueue = q : Exit For
        Next

        If currQueue Is Nothing Then
            ' No queue with that name, create a new one
            CreateWorkQueue(con, wq)
            comp.Id = wq.Ident
        Else
            ' Otherwise, we overwrite the existing queue with our one
            ' Obviously the name is already the same (or we wouldn't have found it),
            ' and we want to keep the existing ID/Ident.
            currQueue.KeyField = wq.KeyField
            currQueue.MaxAttempts = wq.MaxAttempts
            UpdateWorkQueue(con, currQueue)

            ' Update the component with the appopriate data.
            comp.AssociatedData = currQueue
            comp.Id = currQueue.Ident

        End If
    End Sub

    ''' <summary>
    ''' Imports the given credential over the given connection
    ''' </summary>
    ''' <param name="con">The connection over which to import the component.
    ''' This connection should be used exclusively to allow full rollback in case of
    ''' any database errors.</param>
    ''' <param name="rel">The release that this component is being imported on
    ''' behalf of. Primarily used for error messages.</param>
    ''' <param name="comp">The credential to import.</param>
    Private Sub ImportCredential(ByVal con As IDatabaseConnection,
     ByVal rel As clsRelease, ByVal comp As CredentialComponent)

        Dim incomingCred As clsCredential = DirectCast(comp.AssociatedData, clsCredential)

        ' First, we ensure that any incoming processes which were registered with
        ' the credential are assigned to the associated object.

        ' NOTE: This requires the process database handling to be done first so
        ' that the process components held in the credential have been updated with
        ' their new IDs, if they were getting them
        With incomingCred.ProcessIDs
            .Clear()
            For Each proc As ProcessComponent In comp.Members
                .Add(proc.IdAsGuid)
            Next
        End With

        ' Try getting the credential with the same name first
        Dim currCred As clsCredential = Nothing
        Try
            currCred = GetCredential(con, incomingCred.Name)
        Catch ex As NoSuchElementException
            ' If the credential doesn't exist, we want to add it.
            ' The data in the credential should be valid (name, password, etc)
            CreateCredential(con, incomingCred)
            SaveCredentialAssociatedData(con, incomingCred)
            ' Update the component with the ID from the credential
            comp.Id = incomingCred.ID
            Return
        End Try

        ' Otherwise, we want to merge it with the current credential.
        ' Primarily, we need to update the processes with the incoming
        ' process assignments.
        ' First check to see if the current credential is allowed for all processes
        ' If it is, then we don't need to do anything
        If Not currCred.IsForAllProcesses Then
            ' That means that either currCred has no processes allowed, or
            ' its processes are not 'all processes'
            ' Union with our process IDs to ensure our processes are allowed
            currCred.ProcessIDs.Union(incomingCred.ProcessIDs)
        End If
        currCred.Description = incomingCred.Description
        currCred.ExpiryDate = incomingCred.ExpiryDate
        currCred.IsInvalid = incomingCred.IsInvalid
        For Each incomingProp As KeyValuePair(Of String, SafeString) In incomingCred.Properties
            Dim addProp As Boolean = True
            For Each currProp As KeyValuePair(Of String, SafeString) In currCred.Properties
                If currProp.Key = incomingProp.Key Then
                    addProp = False
                    Exit For
                End If
            Next
            If addProp Then currCred.Properties.Add(incomingProp)
        Next
        UpdateCredentialInfo(con, currCred)
        SaveCredentialAssociatedData(con, currCred)
        ' Finally update the component's ID with the ID from the credential
        comp.Id = currCred.ID

    End Sub

    ''' <summary>
    ''' Imports the given font over the specified connection.
    ''' </summary>
    ''' <param name="con">The connection to the database.</param>
    ''' <param name="rel">The release from which the font is drawn</param>
    ''' <param name="comp">The font component to import.</param>
    Private Sub ImportFont(ByVal con As IDatabaseConnection,
     ByVal rel As clsRelease, ByVal comp As FontComponent)
        ' We don't need to check the mods - it's either Skip (handled by
        ' ImportComponent()) or save it.
        SaveFont(con, comp.Name, comp.Name, comp.Version, comp.AssociatedFontData, False)
    End Sub

    ''' <summary>
    ''' Imports the given group over the specified connection.
    ''' </summary>
    ''' <param name="con">The connection to the database.</param>
    ''' <param name="rel">The release from which the group is drawn</param>
    ''' <param name="comp">The group component to import.</param>
    Private Sub ImportGroup(con As IDatabaseConnection, rel As clsRelease, comp As GroupComponent)

        'If no members were imported, don't import this group
        Dim unskippedMember As PackageComponent = comp.Members.FirstOrDefault(
            Function(c) Not c.Modifications.ContainsKey(ModificationType.Skip))
        If unskippedMember Is Nothing Then Return

        Dim tree As GroupTreeType
        Dim mem As GroupMember

        'Determine tree type and group members
        Select Case comp.MembersType
            Case PackageComponentType.BusinessObject
                tree = GroupTreeType.Objects
                mem = New ObjectGroupMember()
            Case PackageComponentType.Process
                tree = GroupTreeType.Processes
                mem = New ProcessGroupMember()
            Case PackageComponentType.Queue
                tree = GroupTreeType.Queues
                mem = New QueueGroupMember()
            Case PackageComponentType.Tile
                tree = GroupTreeType.Tiles
                mem = New TileGroupMember()
            Case Else
                Return
        End Select

        'Get (or create) the destination group, from root down
        Dim grpID As Guid = GetOrCreateGroups(con, tree, comp)

        'Add each member to the group (assuming they have been imported)
        For Each m As PackageComponent In comp.Members
            If Not m.Modifications.ContainsKey(ModificationType.Skip) Then
                mem.Id = m.Id
                AddToGroup(con, grpID, mem, True)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Imports the given data source over the specified connection.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="rel">The release containing the data source</param>
    ''' <param name="comp">The component representing the data source</param>
    Private Sub ImportDataSource(con As IDatabaseConnection, rel As clsRelease, comp As DataSourceComponent)
        Dim script As SqlCommand() = comp.SQL

        'Execute each section of the data source script
        For Each c As SqlCommand In script
            con.Execute(c)
            c.Dispose()
        Next
    End Sub

    ''' <summary>
    ''' Imports the given tile over the specified connection.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="rel">The release containing the tile</param>
    ''' <param name="comp">The compoenent representing the tile</param>
    Private Sub ImportTile(con As IDatabaseConnection, rel As clsRelease, comp As TileComponent)
        Dim data As Object = Nothing
        Dim incomingTile As Tile = comp.AssociatedTile

        If comp.Modifications.TryGetValue(ModificationType.ExistingName, data) Then
            'Rename the existing tile & save it
            Dim existingTile As Tile = GetTileDefinition(con, comp.ExistingTileID)
            existingTile.Name = DirectCast(data, String)
            UpdateTile(con, existingTile)
        ElseIf comp.Modifications.TryGetValue(ModificationType.IncomingName, data) Then
            'Rename the incoming tile
            incomingTile.Name = DirectCast(data, String)
        End If
        If comp.Modifications.TryGetValue(ModificationType.IncomingId, data) Then
            'Assign a new ID to the incoming tile
            incomingTile.ID = DirectCast(data, Guid)
        End If
        'Set new values into component
        comp.Id = incomingTile.ID
        comp.Name = incomingTile.Name

        'Save the incoming tile
        If GetTileNameByID(con, incomingTile.ID) <> String.Empty _
            Then UpdateTile(con, incomingTile) _
            Else CreateTile(con, Guid.Empty, incomingTile)
    End Sub

    ''' <summary>
    ''' Imports the given dashboard over the specified connection.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="rel">The release containing the dashboard</param>
    ''' <param name="comp">The compoenent representing the dashboard</param>
    Private Sub ImportDashboard(con As IDatabaseConnection, rel As clsRelease, comp As DashboardComponent)
        Dim data As Object = Nothing
        Dim incomingDashboard As Dashboard = comp.AssociatedDashboard

        If comp.Modifications.TryGetValue(ModificationType.ExistingName, data) Then
            'Rename the existing dashboard & save it
            Dim existingDashboard As New Dashboard(
                DashboardTypes.Global, comp.ExistingDashboardID, comp.Name)
            existingDashboard.Tiles = GetDashboardTiles(con, existingDashboard.ID)
            existingDashboard.Name = DirectCast(data, String)
            SaveDashboard(con, existingDashboard, False)
        ElseIf comp.Modifications.TryGetValue(ModificationType.IncomingName, data) Then
            'Rename the incoming dashboard
            incomingDashboard.Name = DirectCast(data, String)
        End If
        If comp.Modifications.TryGetValue(ModificationType.IncomingId, data) Then
            'Assign a new ID to the incoming dashboard
            incomingDashboard.ID = DirectCast(data, Guid)
        End If
        'Set new values into component
        comp.Id = incomingDashboard.ID
        comp.Name = incomingDashboard.Name

        'Check if any tiles being imported along with the dashboard are having new IDs
        'If so, update the IDs of the dashboard tiles
        For Each dt As DashboardTile In incomingDashboard.Tiles
            Dim tileComp As PackageComponent = rel.Members.SingleOrDefault(
                Function(t) TypeOf t Is TileComponent AndAlso CType(t, TileComponent).Name = dt.Tile.Name)
            If tileComp IsNot Nothing AndAlso tileComp.Modifications.TryGetValue(ModificationType.IncomingId, data) Then
                dt.Tile.ID = DirectCast(data, Guid)
            End If
        Next
        'Save the incoming dashboard
        If GetDashboardNameByID(con, incomingDashboard.ID) <> String.Empty _
            Then SaveDashboard(con, incomingDashboard, False) _
            Else SaveDashboard(con, incomingDashboard, True)
    End Sub

    Private Sub ImportSkill(con As IDatabaseConnection, rel As clsRelease, comp As SkillComponent)
        Dim skill = UnpackSkillInformation(comp.IdAsGuid, comp.Name, CStr(comp.AssociatedData))
        Dim dataAccess = New SkillDataAccess(con)
        If Not dataAccess.CheckExists(skill.Id) Then dataAccess.Insert(skill)
        dataAccess.InsertOrUpdateVersion(skill.Id, CType(skill.Versions(0), WebSkillVersion), mLoggedInUser.Id)
    End Sub

    Private Function UnpackSkillInformation(id As Guid, name As String, encryptedSkill As String) As Skill
        Dim doc = New XmlDocument()
        doc.LoadXml(Skill.DecryptAndVerify(encryptedSkill))

        Dim node As XmlNode

        node = doc.DocumentElement.SelectSingleNode("/skillinfo/selectedwebapi")
        Dim serviceId = New Guid(node.InnerText)

        node = doc.DocumentElement.SelectSingleNode("/skillinfo/categoryid")
        Dim categoryId = CType(node.InnerText, SkillCategory)

        node = doc.DocumentElement.SelectSingleNode("/skillinfo/icon")
        Dim icon As Byte() = Convert.FromBase64String(node.InnerText)

        node = doc.DocumentElement.SelectSingleNode("/skillinfo/versionnumber")
        Dim versionNumber = node.InnerText

        node = doc.DocumentElement.SelectSingleNode("/skillinfo/description")
        Dim description = node.InnerText

        node = doc.DocumentElement.SelectSingleNode("/skillinfo/blueprismversioncreated")
        Dim versionCreated = node.InnerText

        node = doc.DocumentElement.SelectSingleNode("/skillinfo/blueprismversiontested")
        Dim versionTested = node.InnerText

        node = doc.DocumentElement.SelectSingleNode("/skillinfo/provider")
        Dim provider = node.InnerText

        Dim skillVersion = New WebSkillVersion(
            serviceId, "", True, name, categoryId, versionNumber, description, icon, versionCreated, versionTested, DateTime.UtcNow, New List(Of String))
        Return New Skill(id, provider, True, {skillVersion})
    End Function
#End Region

End Class
