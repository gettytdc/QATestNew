Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes
Imports System.Data.SqlClient
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.Data

' Partial class which separates the Dependency Tracking stuff from the rest of the
' clsServer methods just in order to keep the file size down to a sane level and make
' it easier to actually find functions
Partial Public Class clsServer

    <Serializable>
    Public Enum DependencyStates As Integer
        Invalid = 0
        Building = 1
        Valid = 2
    End Enum

#Region " Runmode calculation "

    ''' <summary>
    ''' Get the effective run mode for the given process. This is the 'most hungry'
    ''' run mode of the process itself combined with all its dependencies.
    ''' </summary>
    ''' <param name="procid">The ID of the main process.</param>
    ''' <param name="sb">Optionally, can be a StringBuilder buffer to receive a
    ''' report to help explain why the run mode is what it is.</param>
    ''' <returns>The run mode</returns>
    <SecuredMethod(True)>
    Public Function GetEffectiveRunMode(procid As Guid, nonVBORunModes As Dictionary(Of String, BusinessObjectRunMode),
     Optional ByRef sb As StringBuilder = Nothing) As BusinessObjectRunMode Implements IServer.GetEffectiveRunMode
        CheckPermissions()
        Dim rm As BusinessObjectRunMode = BusinessObjectRunMode.Background
        Dim checked As New HashSet(Of Guid)

        checked.Add(procid)
        Using con = GetConnection()
            AggregateRunMode(con, procid, nonVBORunModes, checked, rm, sb, 0)
            If sb IsNot Nothing Then
                sb.Replace(sb.ToString().Substring(0, sb.ToString().IndexOf(Environment.NewLine)),
                           String.Format("+ {0} [Overall:{1}]", GetProcessNameById(con, procid), rm))
            End If
        End Using

        Return rm
    End Function

    Private Sub AggregateRunMode(con As IDatabaseConnection, procid As Guid,
     nonVBORunModes As Dictionary(Of String, BusinessObjectRunMode), ByRef checked As HashSet(Of Guid),
     ByRef runmode As BusinessObjectRunMode, sb As StringBuilder, indent As Integer)
        Dim rm As BusinessObjectRunMode

        rm = GetProcessRunMode(con, procid)
        If sb IsNot Nothing Then
            sb _
                 .Append(" "c, indent * 2) _
                 .AppendFormat("- {0}: {1}", GetProcessNameById(con, procid), rm) _
                 .AppendLine()
        End If
        If rm = BusinessObjectRunMode.Background Then
            'Stay as we are
        ElseIf rm = BusinessObjectRunMode.Foreground AndAlso runmode = BusinessObjectRunMode.Background Then
            'Upgrade to foreground
            runmode = BusinessObjectRunMode.Foreground
        ElseIf rm = BusinessObjectRunMode.Exclusive AndAlso runmode <> BusinessObjectRunMode.Exclusive Then
            'Upgrade to exclusive
            runmode = BusinessObjectRunMode.Exclusive
        End If

        Dim deps As clsProcessDependencyList = GetExternalDependencies(con, procid, True)
        For Each dep As clsProcessDependency In deps.Dependencies
            If TypeOf (dep) Is clsProcessNameDependency Then
                Dim procname As String = DirectCast(dep, clsProcessNameDependency).RefProcessName
                If nonVBORunModes.ContainsKey(procname) Then
                    rm = nonVBORunModes(procname)
                    If sb IsNot Nothing Then
                        sb _
                             .Append(" "c, (indent + 1) * 2) _
                             .AppendFormat("- {0}: {1}", procname, rm) _
                             .AppendLine()
                    End If
                    If rm = BusinessObjectRunMode.Background Then
                        'Stay as we are
                    ElseIf rm = BusinessObjectRunMode.Foreground AndAlso runmode = BusinessObjectRunMode.Background Then
                        'Upgrade to foreground
                        runmode = BusinessObjectRunMode.Foreground
                    ElseIf rm = BusinessObjectRunMode.Exclusive AndAlso runmode <> BusinessObjectRunMode.Exclusive Then
                        'Upgrade to exclusive
                        runmode = BusinessObjectRunMode.Exclusive
                    End If
                    Continue For
                End If
                procid = GetProcessIDByName(procname, True)
                If procid = Guid.Empty Then Throw New InvalidOperationException(String.Format(
                 My.Resources.clsServerDependencyUnableToCalculateRunModeObject0DoesNotExist, procname))
                
            ElseIf TypeOf (dep) Is clsProcessIDDependency Then
                procid = DirectCast(dep, clsProcessIDDependency).RefProcessID

            Else
                Continue For
            End If
            If Not checked.Contains(procid) Then
                checked.Add(procid)
                AggregateRunMode(con, procid, nonVBORunModes, checked, runmode, sb, indent + 1)
            End If
        Next

    End Sub

#End Region

#Region " Getting dependencies "

    ''' <summary>
    ''' Get the external dependencies of a given Process from the database.
    ''' </summary>
    ''' <param name="procid">The ID of the process to get dependencies for.</param>
    ''' <returns>A clsProcessDependecyList containing them all.</returns>
    ''' <remarks></remarks>
    <SecuredMethod(True)>
    Public Function GetExternalDependencies(procid As Guid) As clsProcessDependencyList Implements IServer.GetExternalDependencies
        CheckPermissions()
        Using con = GetConnection()
            Return GetExternalDependencies(con, procid, False)
        End Using
    End Function

    Private Function GetExternalDependencies(con As IDatabaseConnection, procid As Guid,
     Optional processOnly As Boolean = False) As clsProcessDependencyList Implements IServerPrivate.GetExternalDependencies
        Dim deps As New clsProcessDependencyList()
        Dim types As New List(Of String)()

        If processOnly Then
            types = CType(clsProcessDependency.RunModeTypes(), List(Of String))
        Else
            types = CType(clsProcessDependency.ExternalTypes(), List(Of String))
        End If

        For Each type As String In types
            Dim cmd As New SqlCommand(String.Format(
                        "select id,{0} from BPA{1} where processID=@procid",
                        String.Join(",", clsProcessDependency.ValueNames(type)),
                        type))
            cmd.Parameters.AddWithValue("@procid", procid)
            Dim dt As DataTable = con.ExecuteReturnDataTable(cmd)
            For Each dr As DataRow In dt.Rows
                deps.Add(clsProcessDependency.Create(type, dr.ItemArray))
            Next
        Next

        Return deps
    End Function

#End Region

#Region " Updating dependencies "

    ''' <summary>
    ''' Updates external references for the passed process. Called when creating or
    ''' amending processes.
    ''' </summary>
    ''' <param name="con"></param>
    ''' <param name="procid"></param>
    ''' <param name="refs"></param>
    Private Sub UpdateProcessDependencies(con As IDatabaseConnection, procid As Guid, refs As clsProcessDependencyList)
        Dim sql As String

        'Get any existing references
        Dim dblst As New clsProcessDependencyList()
        For Each type As String In clsProcessDependency.ExternalTypes()

            ' Check the table name is valid and safe
            Dim tableName As String = ValidateTableName(con, "BPA" & type)

            ' Checl the field names are valid and safe to use.
            Dim values As New List(Of String)
            clsProcessDependency.ValueNames(type).ToList().ForEach(
                Sub(x) values.Add(ValidateFieldName(con, tableName, x)))

            sql = String.Format("select id,{0} from BPA{1} where processID=@procid",
             String.Join(",", values),
             type)
            Dim cmd As New SqlCommand(sql)
            cmd.Parameters.AddWithValue("@procid", procid)
            Dim dt As DataTable = con.ExecuteReturnDataTable(cmd)
            For Each dr As DataRow In dt.Rows
                dblst.Add(clsProcessDependency.Create(type, dr.ItemArray))
            Next
        Next

        'Add missing ones to the database...
        For Each dep As clsProcessDependency In refs.Dependencies
            If Not dblst.Has(dep) Then
                ' Check the table name is valid and safe
                Dim table As String = ValidateTableName(con, "BPA" + dep.TypeName)
                Dim vals As IDictionary(Of String, Object) = dep.GetValues()

                ' Validate the keys are valid and safe
                vals.Keys.ToList().ForEach(Sub(x) ValidateFieldName(con, table, x))

                sql = String.Format("insert into {0} (processID, {1}) values (@procid, {2})",
                 table,
                 String.Join(",", vals.Keys),
                 String.Join(",", vals.Keys.Select(Function(k) "@" & k).ToList()))
                Dim cmd As New SqlCommand(sql)
                cmd.Parameters.AddWithValue("@procid", procid)
                For Each k As String In vals.Keys
                    cmd.Parameters.AddWithValue("@" & k, vals(k))
                Next
                con.Execute(cmd)
            End If
        Next

        'Delete ones that are no longer valid...
        For Each dep As clsProcessDependency In dblst.Dependencies
            If Not refs.Has(dep) Then
                Dim table As String = ValidateTableName(con, "BPA" + dep.TypeName)
                sql = String.Format("delete from {0} where id=@id", table)
                Dim cmd As New SqlCommand(sql)
                cmd.Parameters.AddWithValue("@id", dep.ID)
                con.Execute(cmd)
            End If
        Next

    End Sub

    Private Sub UpdateWebServiceRefs(con As IDatabaseConnection, wsName As String)
        Dim cmd As New SqlCommand("insert into BPAProcessWebServiceDependency (processid, refservicename)" & _
                                  " select processid, refprocessname from BPAProcessNameDependency" & _
                                  " where refprocessname=@name;" & _
                                  "delete from BPAProcessNameDependency where refprocessname=@name;")
        With cmd.Parameters
            .AddWithValue("@name", wsName)
        End With
        con.Execute(cmd)
    End Sub

    Private Sub UpdateWebApiRefs(con As IDatabaseConnection, apiName As String)
        Dim cmd As New SqlCommand("insert into BPAProcessWebApiDependency (processid, refapiname)" &
                                  " select processid, refprocessname from BPAProcessNameDependency" &
                                  " where refprocessname=@name;" &
                                  "delete from BPAProcessNameDependency where refprocessname=@name;")
        With cmd.Parameters
            .AddWithValue("@name", apiName)
        End With
        con.Execute(cmd)
    End Sub

    ''' <summary>
    ''' Updates external dependencies for all processes. Called after upgrade or via
    ''' AutomateC command option.
    ''' </summary>
    ''' <param name="force">Force rebuild regardless of stale flag</param>
    <SecuredMethod(AllowLocalUnsecuredCalls:=True)>
    Public Sub RebuildDependencies(Optional force As Boolean = False) Implements IServer.RebuildDependencies

        CheckPermissions()

        Dim dt As DataTable
        Using con = GetConnection()
            Dim cmd As New SqlCommand()

            'If references not forcibly being refreshed then check if they are stale
            'and skip if they are not.
            If Not force AndAlso GetDependenciesStatus(con) <> DependencyStates.Invalid Then
                Return
            End If

            'Build list of objects/processes
            cmd.CommandText = "select processid, processtype from BPAProcess order by processid"
            dt = con.ExecuteReturnDataTable(cmd)

            'Indicate that dependencies are being updated
            SetDependencyStatus(con, DependencyStates.Building)

            'Setup command to update each process
            Dim objInfo = Options.Instance.GetExternalObjectsInfo()
            cmd.CommandText = "update BPAProcess set runmode=@runmode where processid=@procid"
            Dim runmodeParam As SqlParameter = cmd.Parameters.Add("@runmode", SqlDbType.Int)
            Dim procParam As SqlParameter = cmd.Parameters.Add("@procid", SqlDbType.UniqueIdentifier)

            'Re-build dependencies for each object/process
            Dim row As Integer = 0
            For Each dr As DataRow In dt.Rows
                row += 1

                Dim procid As Guid = DirectCast(dr(0), Guid)
                Dim type As String = DirectCast(dr(1), String)
                Dim procxml As String = GetProcessXML(con, procid)
                Dim proc As clsProcess = clsProcess.FromXml(objInfo, procxml, False)
                Dim deps As clsProcessDependencyList = proc.GetDependencies(False)
                proc.Dispose()

                runmodeParam.Value = deps.RunMode
                procParam.Value = procid

                con.BeginTransaction()
                UpdateProcessDependencies(con, procid, deps)
                con.Execute(cmd)
                AuditRecordProcessOrVboEvent(con, ProcessOrVboEventCode.RefreshDependencies,
                 CBool(IIf(type = "O", True, False)), procid, String.Format(
                 My.Resources.clsServer_BulkDependencyRefreshProcess0Of1, row, dt.Rows.Count),
                 Nothing, CStr(IIf(force, My.Resources.clsServer_DependenciesForciblyRefreshed, My.Resources.clsServer_DependenciesRefreshed)))
                con.CommitTransaction()
            Next

            'Mark dependencies as valid upon successful refresh
            SetDependencyStatus(con, DependencyStates.Valid)
        End Using
    End Sub

#End Region

#Region " Finding References "

    ''' <summary>
    ''' Generic function to test whether or not the passed values are referenced by
    ''' any objects/processes.
    ''' </summary>
    ''' <param name="dep">The dependency object to test for</param>
    ''' <returns>True if a reference is found, otherwise false</returns>
    <SecuredMethod(True)>
    Public Function IsReferenced(dep As clsProcessDependency) As Boolean Implements IServer.IsReferenced
        CheckPermissions()
        Using con = GetConnection()
            Return IsReferenced(con, dep)
        End Using
    End Function

    ''' <summary>
    ''' Generic function to test wether or not the passed values are referenced by
    ''' any objects/processes.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="dep">The dependency object to test for</param>
    ''' <returns>true if a reference is found, otherwise false</returns>
    Private Function IsReferenced(con As IDatabaseConnection, dep As clsProcessDependency) As Boolean
        Dim vals As IDictionary(Of String, Object) = dep.GetValues()
        Dim cmd As New SqlCommand(String.Format("select top 1 processID from BPA{0} where {1}",
         dep.TypeName, String.Join(" and ", vals.Keys.Select(Function(k) k & "=@" & k).ToList())))

        For Each k As String In vals.Keys
            cmd.Parameters.AddWithValue("@" & k, vals(k))
        Next

        Dim obj As Object = con.ExecuteReturnScalar(cmd)
        If obj IsNot Nothing Then Return True
    End Function

    ''' <summary>
    ''' Takes a list of dependency objects to test references for, and removes those
    ''' that are not referenced by objects/processes.
    ''' </summary>
    ''' <param name="deps">List of dependency objects to test</param>
    ''' <returns>List of referenced dependencies</returns>
    <SecuredMethod(True)>
    Public Function FilterUnReferenced(deps As clsProcessDependencyList) As clsProcessDependencyList Implements IServer.FilterUnReferenced
        CheckPermissions()
        Dim referencedDeps As New clsProcessDependencyList()
        Dim gatewaysCredName As String = Nothing

        Using con = GetConnection()

            If deps.Dependencies.Any(Function(x) TypeOf (x) Is clsProcessCredentialsDependency) Then
                Dim datapipelineSettings = GetDataAccesss(con).GetDataPipelineSettings()

                If datapipelineSettings.UseIntegratedSecurity = False Then
                    gatewaysCredName = datapipelineSettings.DatabaseUserCredentialName
                End If
            End If

            For Each dep As clsProcessDependency In deps.Dependencies
                If gatewaysCredName IsNot Nothing AndAlso TypeOf (dep) Is clsProcessCredentialsDependency Then
                    If CType(dep, clsProcessCredentialsDependency).RefCredentialsName = gatewaysCredName Then
                        CType(dep, clsProcessCredentialsDependency).GatewaysCredential = True
                    End If
                End If

                If IsReferenced(con, dep) Then
                    If TypeOf (dep) Is clsProcessCredentialsDependency AndAlso CType(dep, clsProcessCredentialsDependency).GatewaysCredential = True Then
                        CType(dep, clsProcessCredentialsDependency).SharedCredential = True
                    End If

                    referencedDeps.Add(dep)
                ElseIf TypeOf (dep) Is clsProcessCredentialsDependency AndAlso CType(dep, clsProcessCredentialsDependency).GatewaysCredential = True Then
                    referencedDeps.Add(dep)
                End If
            Next
        End Using

        Return referencedDeps
    End Function

    ''' <summary>
    ''' Generic function to return object/process references for the passed value.
    ''' </summary>
    ''' <param name="dep">The dependency object to find</param>
    ''' <param name="hiddenItems">Returns True if the dependency object is
    ''' referenced by objects/process for which the current user cannot access</param>
    ''' <returns>A list containing referencing objects/processes</returns>
    <SecuredMethod()>
    Public Function GetReferences(dep As clsProcessDependency, ByRef hiddenItems As Boolean) As ICollection(Of ProcessInfo) Implements IServer.GetReferences
        CheckPermissions()
        Using con = GetConnection()
            Dim vals As IDictionary(Of String, Object) = dep.GetValues()
            Dim cmd As New SqlCommand(String.Format("select b.ProcessType, a.processID, b.name, b.description" &
             " from BPA{0} a inner join BPAProcess b on a.processid=b.processid where {1}",
             dep.TypeName, String.Join(" and ", vals.Keys.Select(Function(k) k & "=@" & k).ToList())))
            'Special case - include parent references if looking for objects
            If TypeOf dep Is clsProcessNameDependency Then
                cmd.CommandText &= " union select b.ProcessType, a.processID, b.name, b.description" &
                 " from BPAProcessParentDependency a inner join BPAProcess b on a.processid=b.processid" &
                 " where a.refParentName=@refProcessName"
            End If

            For Each k As String In vals.Keys
                cmd.Parameters.AddWithValue("@" & k, vals(k))
            Next

            Dim processList As New List(Of ProcessInfo)
            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov = New ReaderDataProvider(reader)
                While reader.Read()
                    processList.Add(New ProcessInfo() With {
                                    .Id = prov.GetGuid("processId"),
                                    .Type = If(prov.GetString("processtype") = "O",
                                        DiagramType.Object, DiagramType.Process),
                                    .Name = prov.GetString("name"),
                                    .Description = prov.GetString("description")})
                End While
            End Using

            Dim inaccessibleIds As New List(Of Guid)
            For Each p In processList
                Dim permissions = GetEffectiveMemberPermissionsForProcess(con, p.Id)
                If Not permissions.HasAnyPermissions(mLoggedInUser) Then
                    inaccessibleIds.Add(p.Id)
                ElseIf permissions.HasPermission(mLoggedInUser,
                        If(p.Type = DiagramType.Object,
                            Permission.ObjectStudio.ImpliedViewBusinessObject,
                            Permission.ProcessStudio.ImpliedViewProcess)) Then
                    p.CanViewDefinition = True
                End If
            Next

            hiddenItems = inaccessibleIds.Count > 0
            processList.RemoveAll(Function(p) inaccessibleIds.Contains(p.Id))
            Return processList
        End Using
    End Function


    ''' <summary>
    ''' Returns a list of processes referenced either directly or indirectly by [processName], which contain
    ''' references to items the current user does not have permission to use.
    ''' </summary>
    ''' <param name="processName">The name of the process</param>
    ''' <returns>A list of process which contain references to items this user does not have permission to access.</returns>
    <SecuredMethod(True)>
    Public Function GetInaccessibleReferencesByProcessName(processName As String) As ICollection(Of String) Implements IServer.GetInaccessibleReferencesByProcessName
        CheckPermissions()
        Using connection = GetConnection()
            Dim id = GetProcessIDByName(connection, processName, True)
            Return New ProcessDependencyPermissionLogic(Me).GetInaccessibleReferences(connection, id, mLoggedInUser,
                                                                                      Function(s As String) NonCachedCachedProcessIDLookup(connection, s))
        End Using
    End Function

    <SecuredMethod(True)>
    Public Function GetInaccessibleReferencesByProcessNames(processNames As List(Of String)) As Dictionary(Of String, List(Of String)) _
        Implements IServer.GetInaccessibleReferencesByProcessNames
        CheckPermissions()
        Using connection = GetConnection()
            Dim permissionLogic = New ProcessDependencyPermissionLogic(Me)
            Dim result As New Dictionary(Of String, List(Of String))
            Dim processIDLookupCache As New Dictionary(Of String, Guid)

            Dim lookupFunction = Function(s As String) CachedProcessIDLookup(processIDLookupCache, connection, s)

            For Each processName As String In processNames
                Dim processID = GetProcessIDByName(connection, processName, True)
                Dim badReferences = permissionLogic.GetInaccessibleReferences(connection, processID,
                                                                                       mLoggedInUser, lookupFunction).ToList()

                If badReferences.Any Then result.Add(processName, badReferences)
            Next

            Return result
        End Using
    End Function

    Private Function NonCachedCachedProcessIDLookup(connection As IDatabaseConnection, processName As String) As Guid
        Return GetProcessIDByName(connection, processName, True)
    End Function

    Private Function CachedProcessIDLookup(cache As Dictionary(Of String, Guid), connection As IDatabaseConnection, processName As String) As Guid
        Dim result As Guid
        If Not cache.TryGetValue(processName, result) Then
            result = GetProcessIDByName(connection, processName, True)
            cache.Add(processName, result)
        End If
        Return result
    End Function

    ''' <summary>
    ''' Returns a list of processes referenced either directly or indirectly by [processID], which contain
    ''' references to items the current user does not have permission to use.
    ''' </summary>
    ''' <param name="processID">. The Id of the process</param>
    ''' <returns>A list of process which contain references to items this user does not have permission to access.</returns>
    <SecuredMethod(True)>
    Public Function GetInaccessibleReferencesByProcessID(processID As Guid) As ICollection(Of String) Implements IServer.GetInaccessibleReferencesByProcessID
        CheckPermissions()
        Using connection = GetConnection()
            Dim f = Function(s As String) NonCachedCachedProcessIDLookup(connection, s)
            Return New ProcessDependencyPermissionLogic(Me).GetInaccessibleReferences(connection, processID, mLoggedInUser, f)
        End Using
    End Function

    ''' <summary>
    ''' Returns references to a parent's shared application model elements.
    ''' </summary>
    ''' <param name="parentName">The parent object name</param>
    ''' <returns>The list of references</returns>
    <SecuredMethod(True)>
    Public Function GetSharedModelReferences(parentName As String) As clsProcessDependencyList Implements IServer.GetSharedModelReferences
        CheckPermissions()
        Dim deps As New clsProcessDependencyList()
        Using con = GetConnection()
            Dim sb As New StringBuilder("select b.refElementID from BPAProcessParentDependency a")
            sb.Append(" inner join BPAProcessElementDependency b on a.processid=b.processID")
            sb.Append(" where a.refParentName=@parent")
            Dim cmd As New SqlCommand(sb.ToString())
            cmd.Parameters.AddWithValue("@parent", parentName)

            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    deps.Add(New clsProcessElementDependency(parentName, prov.GetGuid("refElementID")))
                End While
            End Using
        End Using

        Return deps
    End Function

    ''' <summary>
    ''' Returns parent object names associated with the passed list of objects.
    ''' </summary>
    ''' <param name="objIDs">The list of object IDs to check</param>
    ''' <returns>The associated parents</returns>
    <SecuredMethod(True)>
    Public Function GetParentReferences(objIDs As List(Of Guid)) As Dictionary(Of Guid, String) Implements IServer.GetParentReferences
        CheckPermissions()
        Dim objParents As New Dictionary(Of Guid, String)
        Dim parent As String

        Using con = GetConnection()
            For Each id As Guid In objIDs
                parent = GetParentReference(con, id)
                If parent <> String.Empty Then objParents.Add(id, parent)
            Next
        End Using

        Return objParents
    End Function

    ''' <summary>
    ''' Returns parent object name for the passed object.
    ''' </summary>
    ''' <param name="id">The object ID to check</param>
    ''' <returns>The parent name</returns>
    <SecuredMethod(True)>
    Public Function GetParentReference(id As Guid) As String Implements IServer.GetParentReference
        CheckPermissions()
        Using con = GetConnection()
            Return GetParentReference(con, id)
        End Using
    End Function

    ''' <summary>
    ''' Returns parent object name for the passed object.
    ''' </summary>
    ''' <param name="con">The database connection</param>
    ''' <param name="id">The object ID to check</param>
    ''' <returns>The parent name</returns>
    Private Function GetParentReference(con As IDatabaseConnection, id As Guid) As String
        Dim cmd As New SqlCommand("select refParentName from BPAProcessParentDependency where processID=@id")
        cmd.Parameters.AddWithValue("@id", id)
        Return IfNull(con.ExecuteReturnScalar(cmd), String.Empty)
    End Function

#End Region

#Region " Dependency Status gets/sets "

    <SecuredMethod(True)>
    Public Function GetDependenciesStatus() As DependencyStates Implements IServer.GetDependenciesStatus
        CheckPermissions()
        Using con = GetConnection()
            Return GetDependenciesStatus(con)
        End Using
    End Function

    Private Function GetDependenciesStatus(con As IDatabaseConnection) As DependencyStates
        Dim cmd As New SqlCommand("select DependencyState from BPASysConfig")
        Dim obj As Object = con.ExecuteReturnScalar(cmd)
        Return CType(obj, DependencyStates)
    End Function

    Private Sub SetDependencyStatus(con As IDatabaseConnection, state As DependencyStates)
        Dim cmd As New SqlCommand("update BPASysConfig set DependencyState=@state")
        cmd.Parameters.AddWithValue("@state", state)
        con.Execute(cmd)
    End Sub

#End Region

End Class

