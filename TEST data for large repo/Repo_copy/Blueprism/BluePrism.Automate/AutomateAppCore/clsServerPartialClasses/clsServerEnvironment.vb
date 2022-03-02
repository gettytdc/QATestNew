Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.Logging
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Data

Partial Public Class clsServer


    <SecuredMethod(True)>
    Public Sub SaveEnvironmentData(data As EnvironmentData, Optional applicationServerPortNumber As Integer = 0, Optional applicationServerFullyQualifiedDomainName As String = Nothing) Implements IServer.SaveEnvironmentData
        CheckPermissions()
        If GetPref(PreferenceNames.SystemSettings.EnableBpaEnvironmentData, True) Then
            Using con = GetConnection()
                SaveEnvironmentData(con, data, applicationServerPortNumber, applicationServerFullyQualifiedDomainName)
            End Using
        End If
    End Sub

    Protected Overridable Sub SaveEnvironmentData(con As IDatabaseConnection, data As EnvironmentData, applicationServerPortNumber As Integer, applicationServerFullyQualifiedDomainName As String)

        Using cmd = New SqlCommand("usp_CreateUpdateEnvironmentData")
            cmd.CommandType = CommandType.StoredProcedure
            With cmd.Parameters
                .AddWithValue("@environmentTypeId", data.EnvironmentType)
                .AddWithValue("@fqdn", data.FullyQualifiedDomainName)
                .AddWithValue("@portNumber", data.PortNumber)
                .AddWithValue("@version", data.VersionNumber)
                .AddWithValue("@certificateExpires", IIf(data.CertificateExpTime.HasValue, data.CertificateExpTime, DBNull.Value))
                .AddWithValue("@applicationServerPortNumber", applicationServerPortNumber)
                .AddWithValue("@applicationServerFullyQualifiedDomainName", If(applicationServerFullyQualifiedDomainName, String.Empty))
            End With
            con.Execute(cmd)
        End Using
    End Sub

    <SecuredMethod(True)>
    Public Function GetEnvironmentData() As List(Of EnvironmentData) Implements IServer.GetEnvironmentData
        CheckPermissions()

        Using con = GetConnection()
            Return GetEnvironmentData(con)
        End Using
    End Function

    <SecuredMethod(Permission.Resources.ViewResourceDetails)>
    Public Function GetResourceEnvironmentData(resourceName As String) As EnvironmentData Implements IServer.GetResourceEnvironmentData
        CheckPermissions()

        Using con = GetConnection()
            Return GetResourceEnvironmentData(con, resourceName)
        End Using
    End Function

    <SecuredMethod(Permission.Resources.ViewResourceDetails)>
    Public Function CheckResourceDetailPermission() As Boolean Implements IServer.CheckResourceDetailPermission
        CheckPermissions()
        Return True
    End Function

    Protected Overridable Function GetEnvironmentData(con As IDatabaseConnection) As List(Of EnvironmentData)
        Dim environmentData As New List(Of EnvironmentData)
        Using cmd = New SqlCommand("select e.fqdn, e.port, e.environmenttypeid, e.version, e.createddatetime, e.updateddatetime, e.CertificateExpires, " &
                                   "a.fqdn + ':' + cast(a.Port as varchar(10)) as 'ApplicationServer' " &
                                   "from BPAEnvironment e left join BPAEnvironment a on e.ApplicationServerId = a.Id")

            Using reader = con.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                Do While reader.Read()
                    environmentData.Add(CreateEnvironmentData(prov))
                Loop
            End Using
        End Using
        Return environmentData
    End Function

    Protected Overridable Function GetResourceEnvironmentData(con As IDatabaseConnection, resourceName As String) As EnvironmentData
        Try
            Using cmd = New SqlCommand($"select resources.fqdn, resources.resourcename, resources.version, resources.createddatetime,
                                    resources.updateddatetime, resources.port, resources.ApplicationServer, resources.EnvironmentTypeId,
                                    resources.CertificateExpires from (
								   select REPLACE(e.fqdn,'.','') as fqdn, e.port, REPLACE(e.fqdn,'.',':') + cast(e.Port as varchar(10)) as resourcename, e.version, e.createddatetime, e.updateddatetime, 
                                   REPLACE(e.fqdn,'.','') + ':' + cast(a.Port as varchar(10)) as ApplicationServer, e.environmenttypeid, e.CertificateExpires
                                   from BPAEnvironment e left join BPAEnvironment a on e.ApplicationServerId = a.Id
                                   ) as resources
								   where  resources.resourcename = '{resourceName}'")

                Using reader = con.ExecuteReturnDataReader(cmd)
                    Dim prov As New ReaderDataProvider(reader)
                    If reader.Read() Then
                        Return CreateEnvironmentData(prov)
                    End If
                End Using
            End Using
        Catch ex As Exception
            Log.Error(ex, "Resource environment data cannot be retrieved", Nothing)
        End Try

        Return Nothing
    End Function

    Private Function CreateEnvironmentData(prov As ReaderDataProvider) As EnvironmentData

        Return New EnvironmentData(CType(prov.GetInt("EnvironmentTypeID"), EnvironmentType),
                                   prov.GetString("fqdn"),
                                   prov.GetInt("port"),
                                   prov.GetString("Version"),
                                   prov.GetValue(Of Date)("CreatedDateTime", Nothing),
                                   prov.GetValue(Of Date)("UpdatedDateTime", Nothing),
                                   prov.GetValue(Of Date?)("CertificateExpires", Nothing),
                                   prov.GetString("ApplicationServer"))
    End Function
End Class
