Imports System.Data.SqlClient
Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Data
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.DataAccess
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Utilities.Functional

Partial Public Class clsServer

    ''' <summary>
    ''' Gets all the web service definitions in this environment
    ''' </summary>
    ''' <returns>A collection of web service definitions in this environment.
    ''' </returns>
    <SecuredMethod(AllowLocalUnsecuredCalls:=True)>
    Public Function GetWebServiceDefinitions() As ICollection(Of clsWebServiceDetails) Implements IServer.GetWebServiceDefinitions
        CheckPermissions()
        Using con = GetConnection()
            Return GetWebServiceDefinitions(con, Nothing)
        End Using
    End Function

    ''' <summary>
    ''' Gets all the web service definitions in this environment
    ''' </summary>
    ''' <param name="con">The connection over which to retrieve the definitions.
    ''' </param>
    ''' <returns>A collection of web service definitions in this environment.
    ''' </returns>
    Private Function GetWebServiceDefinitions(con As IDatabaseConnection, serviceIds As List(Of Guid)) As List(Of clsWebServiceDetails)

        Dim cmd As New SqlCommand()
        cmd.CommandText =
         " select w.serviceid as id," &
         "   w.enabled," &
         "   w.servicename As name," &
         "   w.url," &
         "   w.wsdl," &
         "   w.settingsxml," &
         "   w.timeout," &
         "   a.assettype," &
         "   a.assetxml" &
         " from BPAWebService w" &
         "    left join BPAWebServiceAsset a on w.serviceid = a.serviceid"

        If serviceIds?.Any() Then
            Dim serviceIdParameterNames As New List(Of String)
            For i As Integer = 0 To serviceIds.Count - 1
                Dim paramName = $"@serviceId_{i}"
                serviceIdParameterNames.Add(paramName)
                cmd.Parameters.AddWithValue(paramName, serviceIds(i))
            Next

            cmd.CommandText = cmd.CommandText & $" where w.serviceid in ({String.Join(",", serviceIdParameterNames)})"
        End If

        Dim services As New List(Of clsWebServiceDetails)
        Using reader = con.ExecuteReturnDataReader(cmd)
            Dim prov As New ReaderDataProvider(reader)
            Dim map As New Dictionary(Of Guid, clsWebServiceDetails)
            While reader.Read()
                Dim serviceId As Guid = prov.GetValue("id", Guid.Empty)
                Dim ws As clsWebServiceDetails = Nothing
                If Not map.TryGetValue(serviceId, ws) Then
                    ws = New clsWebServiceDetails(prov)
                    map(serviceId) = ws
                End If
                Select Case prov.GetValue("assettype", AssetType.Unknown)
                    Case AssetType.WSDL : ws.ExtraWSDL.Add(prov.GetString("assetxml"))
                    Case AssetType.XSD : ws.Schemas.Add(prov.GetString("assetxml"))
                End Select
            End While
            Return map.Values.ToList()
        End Using
    End Function

    ''' <summary>
    ''' Get a collection of all web apis in this environment, containing just the 
    ''' basic top level info that can be displayed on a list control
    ''' </summary>
    ''' <returns>A collection of Web APIs</returns>
    <SecuredMethod(True)>
    Public Function GetWebApiListItems() As ICollection(Of WebApiListItem) _
     Implements IServer.GetWebApiListItems
        CheckPermissions()

        Using con = GetConnection()
            Dim dataAccess As New WebApiDataAccess(con)
            Return dataAccess.GetWebApiListItems()
        End Using
    End Function

    ''' <summary>
    ''' Get a collection of all the web apis in this environment
    ''' </summary>
    ''' <returns>A collection of web apis</returns>
    <SecuredMethod(True)>
    Function GetWebApis() As ICollection(Of WebApi) Implements IServer.GetWebApis
        CheckPermissions()
        Using con = GetConnection()
            Dim dataAccess As New WebApiDataAccess(con)
            Return dataAccess.GetWebApis()
        End Using
    End Function

    ''' <summary>
    ''' Gets the web api  with the given ID.
    ''' </summary>
    ''' <param name="id">The id of the required web api</param>
    ''' <returns>The <see cref="WebApi"/> object represented by the given ID.
    ''' </returns>
    ''' <exception cref="NoSuchElementException">If no web api exists with
    ''' the specified ID.</exception>
    <SecuredMethod(True)>
    Public Function GetWebApi(ByVal id As Guid) As WebApi Implements IServer.GetWebApi
        CheckPermissions()
        Using con = GetConnection()
            Dim dataAccess As New WebApiDataAccess(con)
            Return dataAccess.GetWebApi(id)
        End Using
    End Function

    ''' <summary>
    ''' Gets the web service definition with the given ID.
    ''' </summary>
    ''' <param name="id">The ID of the required web service</param>
    ''' <returns>The web service details object represented by the given ID.
    ''' </returns>
    ''' <exception cref="NoSuchElementException">If no web service exists with
    ''' the specified ID.</exception>
    <SecuredMethod(True)>
    Public Function GetWebServiceDefinition(ByVal id As Guid) As clsWebServiceDetails _
        Implements IServer.GetWebServiceDefinition
        CheckPermissions()
        Using con = GetConnection()
            Return GetWebServiceDefinition(con, id)
        End Using
    End Function

    ''' <summary>
    ''' Gets the ID of the web service with the given name.
    ''' </summary>
    ''' <param name="name">The name of the web service to retrieve.</param>
    ''' <returns>The ID of the web service with the given name or Guid.Empty, if no
    ''' such web service exists.</returns>
    <SecuredMethod(True)>
    Public Function GetWebServiceId(ByVal name As String) As Guid Implements IServer.GetWebServiceId
        CheckPermissions()
        Using con = GetConnection()
            Return GetWebServiceId(con, name)
        End Using
    End Function

    ''' <summary>
    ''' Gets the ID of the web service with the given name.
    ''' </summary>
    ''' <param name="cOn">The connection to use to retrieve the web service.</param>
    ''' <param name="name">The name of the web service to retrieve.</param>
    ''' <returns>The ID of the web service with the given name or Guid.Empty, if no
    ''' such web service exists.</returns>
    Private Function GetWebServiceId(ByVal con As IDatabaseConnection, ByVal name As String) As Guid
        Dim cmd As New SqlCommand("Select serviceid from BPAWebService where servicename=@name")
        cmd.Parameters.AddWithValue("@name", name)
        Return IfNull(con.ExecuteReturnScalar(cmd), Guid.Empty)
    End Function

    ''' <summary>
    ''' Get the id of a web api with the given name
    ''' </summary>
    ''' <param name="name">The name of the web api to return the id of</param>
    ''' <returns>The id of a web api</returns>
    <SecuredMethod(True)>
    Public Function GetWebApiId(ByVal name As String) As Guid Implements IServer.GetWebApiId
        CheckPermissions()
        Using con = GetConnection()
            Dim dataAccess As New WebApiDataAccess(con)
            Return dataAccess.GetWebApiId(name)
        End Using
    End Function


    ''' <summary>
    ''' Gets the web service definition with the given ID.
    ''' </summary>
    ''' <param name="cOn">The connection over which to retrieve the web service
    ''' definition.</param>
    ''' <param name="id">The ID of the required web service</param>
    ''' <returns>The web service details object represented by the given ID.
    ''' </returns>
    ''' <exception cref="NoSuchElementException">If no web service exists with
    ''' the specified ID.</exception>
    Private Function GetWebServiceDefinition(ByVal con As IDatabaseConnection, ByVal id As Guid) _
     As clsWebServiceDetails
        Dim cmd As New SqlCommand(
         " Select w.serviceid As id," &
         "   w.enabled," &
         "   w.servicename As name," &
         "   w.url," &
         "   w.wsdl," &
         "   w.settingsxml," &
         "   w.timeout," &
         "   a.assettype," &
         "   a.assetxml" &
         " from BPAWebService w" &
         "    left join BPAWebServiceAsset a On w.serviceid = a.serviceid" &
         " where w.serviceid=@id")

        cmd.Parameters.AddWithValue("@id", id)

        Using reader = con.ExecuteReturnDataReader(cmd)
            If Not reader.Read() Then Throw New NoSuchElementException(
             My.Resources.clsServer_NoWebServiceFoundWithId0, id)

            Dim prov As New ReaderDataProvider(reader)
            Dim ws As New clsWebServiceDetails(prov)
            Do
                Select Case prov.GetValue("assettype", AssetType.Unknown)
                    Case AssetType.WSDL : ws.ExtraWSDL.Add(prov.GetString("assetxml"))
                    Case AssetType.XSD : ws.Schemas.Add(prov.GetString("assetxml"))
                End Select
            Loop While reader.Read

            Return ws
        End Using
    End Function

    ''' <summary>
    ''' Gets the web service definition with the given name.
    ''' </summary>
    ''' <param name="cOn">The connection over which to retrieve the web service
    ''' definition.</param>
    ''' <param name="name">The name of the required web service</param>
    ''' <returns>The web service details object represented by the given IDname
    ''' </returns>
    ''' <exception cref="NoSuchElementException">If no web service exists with
    ''' the specified name.</exception>
    Private Function GetWebServiceDefinition(ByVal con As IDatabaseConnection, ByVal name As String) _
      As clsWebServiceDetails
        Dim id As Guid = GetWebServiceId(con, name)
        If id = Nothing Then Throw New NoSuchElementException(
          My.Resources.clsServer_NoWebServiceFoundWithName0, name)
        Return GetWebServiceDefinition(con, id)
    End Function

    ''' <summary>
    ''' Saves the given web service to the database, overwriting the current
    ''' definition if it exists, creating a new one if it doesn't
    ''' </summary>
    ''' <param name="ws">The web service to save</param>
    <SecuredMethod(Permission.SystemManager.BusinessObjects.WebServicesSoap)>
    Public Sub SaveWebServiceDefinition(ByVal ws As clsWebServiceDetails) Implements IServer.SaveWebServiceDefinition
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            SaveWebServiceDefinition(con, ws)
            AuditRecordObjectEvent(con, ObjectEventCode.AddObject, ws.FriendlyName, String.Format(My.Resources.ChosenWSDLIs0AndChosenServiceIs1AndChosenMethodsAre2, ws.URL, ws.ServiceToUse, ws.EnabledActions))
            con.CommitTransaction()
        End Using
    End Sub

    ''' <summary>
    ''' Saves the given web service to the database, overwriting the current
    ''' definition if it exists, creating a new one if it doesn't
    ''' </summary>
    ''' <param name="cOn">The connection to use</param>
    ''' <param name="ws">The web service to save</param>
    Private Sub SaveWebServiceDefinition(
     ByVal con As IDatabaseConnection, ByVal ws As clsWebServiceDetails)

        Dim cmd As New SqlCommand(
         " If exists (Select 1 from BPAWebService where serviceid=@id)" &
         "   update BPAWebService Set" &
         "     enabled=@enabled," &
         "     servicename=@name," &
         "     url=@url," &
         "     wsdl=@wsdl," &
         "     settingsxml=@settingsxml," &
         "     timeout=@timeout" &
         "   where serviceid = @id" &
         " Else" &
         "   insert into BPAWebService " &
         "     (serviceid, enabled, servicename, url, wsdl, settingsxml, timeout) " &
         "     values (@id, @enabled, @name, @url, @wsdl, @settingsxml, @timeout)"
        )

        With cmd.Parameters
            .AddWithValue("@id", ws.Id)
            .AddWithValue("@enabled", ws.Enabled)
            .AddWithValue("@name", ws.FriendlyName)
            .AddWithValue("@url", IfNull(ws.URL, ""))
            .AddWithValue("@wsdl", IfNull(ws.WSDL, ""))
            .AddWithValue("@settingsxml", IfNull(ws.GetSettings(), ""))
            .AddWithValue("@timeout", ws.Timeout)
        End With

        con.Execute(cmd)
        UpdateWebServiceRefs(con, ws.FriendlyName)

        cmd = New SqlCommand(
        "delete from BPAWebServiceAsset where serviceid=@serviceid")
        cmd.Parameters.AddWithValue("@serviceid", ws.Id)
        con.Execute(cmd)

        cmd = New SqlCommand(
        "insert into BPAWebServiceAsset " &
        "(serviceid, assettype, assetxml) " &
        " values (@serviceid, @assettype, @assetxml)"
        )

        Dim typeParam As SqlParameter
        Dim xmlParam As SqlParameter
        With cmd.Parameters
            .AddWithValue("@serviceid", ws.Id)
            typeParam = .Add("@assettype", SqlDbType.TinyInt)
            xmlParam = .Add("@assetxml", SqlDbType.NVarChar)
        End With

        typeParam.Value = AssetType.WSDL
        For Each s As String In ws.ExtraWSDL
            xmlParam.Value = s
            con.Execute(cmd)
        Next

        typeParam.Value = AssetType.XSD
        For Each s As String In ws.Schemas
            xmlParam.Value = s
            con.Execute(cmd)
        Next

    End Sub

    ''' <summary>
    ''' Saves the given web api to the database, overwriting the current
    ''' web api if it exists, creating a new one if it doesn't
    ''' </summary>
    ''' <param name="webApi">The web api to save</param>
    <SecuredMethod(Permission.SystemManager.BusinessObjects.WebServicesWebApi)>
    Public Sub SaveWebApi(webApi As WebApi) Implements IServer.SaveWebApi
        CheckPermissions()
        Using con = GetConnection()

            Dim eventCode = If(webApi.Id = Guid.Empty,
                                ObjectEventCode.AddObject,
                                ObjectEventCode.UpdateObject)

            con.BeginTransaction()

            Dim dataAccess As New WebApiDataAccess(con)
            dataAccess.Save(webApi)
            AuditRecordObjectEvent(con, eventCode, webApi.Name,
                        String.Format(My.Resources.clsServer_ChosenWebApiNameIs0, webApi.Name))

            UpdateWebApiRefs(con, webApi.Name)
            con.CommitTransaction()

        End Using
    End Sub

    ''' <summary>
    ''' Update the state of a particular web service, i.e. whether it is enabled or
    ''' not.
    ''' </summary>
    ''' <param name="gServiceID">The service identifier</param>
    ''' <param name="bEnabled">The new state</param>
    <SecuredMethod(Permission.SystemManager.BusinessObjects.WebServicesSoap)>
    Public Sub UpdateWebServiceEnabled(ByVal gServiceID As Guid,
                                            ByVal bEnabled As Boolean) Implements IServer.UpdateWebServiceEnabled
        CheckPermissions()

        Using con = GetConnection()
            Dim iBit As Integer
            If bEnabled Then
                iBit = 1
            Else
                iBit = 0
            End If
            Dim cmd As New SqlCommand("UPDATE BPAWebService Set enabled = @Enabled WHERE serviceid = @ServiceID")
            With cmd.Parameters
                .AddWithValue("@Enabled", iBit)
                .AddWithValue("@ServiceID", gServiceID.ToString)
            End With
            con.Execute(cmd)
        End Using
    End Sub

    '''<inheritdoc/>
    <SecuredMethod(Permission.SystemManager.BusinessObjects.WebServicesWebApi)>
    Public Sub UpdateWebApiEnabled(webApiId As Guid, enabled As Boolean) Implements IServer.UpdateWebApiEnabled
        CheckPermissions()
        Using con = GetConnection()
            Dim dataAccess As New WebApiDataAccess(con)
            dataAccess.UpdateWebApiEnabledFlag(webApiId, enabled)
        End Using
    End Sub

    ''' <summary>
    ''' Deletes a web service from the database.
    ''' </summary>
    ''' <param name="sServiceName">A name of the web service to delete</param>
    <SecuredMethod(Permission.SystemManager.BusinessObjects.WebServicesSoap)>
    Public Sub DeleteWebservice(ByVal sServiceName As String) Implements IServer.DeleteWebservice
        CheckPermissions()
        Using con = GetConnection()
            Dim cmd As New SqlCommand("DELETE FROM BPAWebService WHERE servicename = @Name")
            With cmd.Parameters
                .AddWithValue("@Name", sServiceName)
            End With
            con.Execute(cmd)
        End Using
    End Sub

    ''' <inheritdoc/>
    <SecuredMethod(Permission.SystemManager.BusinessObjects.WebServicesWebApi)>
    Public Sub DeleteWebApi(ByVal id As Guid, name As String) Implements IServer.DeleteWebApi
        CheckPermissions()
        Using con = GetConnection()
            con.BeginTransaction()
            Dim dataAccess As New WebApiDataAccess(con)
            dataAccess.Delete(id)
            AuditRecordObjectEvent(con, ObjectEventCode.DeleteObject, name, "")
            con.CommitTransaction()
        End Using
    End Sub

    <SecuredMethod(Permission.SystemManager.BusinessObjects.WebServicesSoap)>
    Public Sub DeleteWebservices(ByVal serviceIds As List(Of Guid)) Implements IServer.DeleteWebservices
        CheckPermissions()
        Using con = GetConnection()
            Dim definitions = GetWebServiceDefinitions(con, serviceIds)
            Dim sSQL As String = "DELETE FROM BPAWebservice WHERE "
            For Each gID As Guid In serviceIds
                sSQL &= "serviceid = '" & gID.ToString & "' OR "
            Next
            'remove the last OR
            sSQL = sSQL.Remove(sSQL.Length - 3, 3)
            Dim cmd As New SqlCommand(sSQL)
            con.Execute(cmd)

            definitions.ForEach(Sub(definition) AuditRecordObjectEvent(con, ObjectEventCode.DeleteObject, definition.FriendlyName, String.Empty))
        End Using
    End Sub

End Class
