Imports System.Data.SqlClient
Imports System.Net.Http
Imports System.Xml.Linq
Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.AutomateProcessCore.WebApis.Authentication
Imports BluePrism.AutomateProcessCore.WebApis.RequestHandling.BodyContent
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.Data
Imports BluePrism.Server.Domain.Models

Namespace clsServerPartialClasses.DataAccess

    ''' <summary>
    ''' Data access class containing Web API business logic
    ''' </summary>
    Friend Class WebApiDataAccess

        ''' <summary>
        ''' The database connection that is used to execute all sql commands in this
        ''' instance
        ''' </summary>
        Private ReadOnly mConnection As IDatabaseConnection

        ''' <summary>
        ''' Create a new instance of the web api data access class
        ''' </summary>
        ''' <param name="connection">
        ''' The database connection to use for all sql commands
        ''' </param>
        Public Sub New(connection As IDatabaseConnection)
            mConnection = connection
        End Sub

        ''' <summary>
        ''' Saves the given Web API to the database, overwriting the current
        ''' web api if it exists and creating a new one if it doesn't
        ''' </summary>
        ''' <param name="webApi">The web api to save</param>
        Public Sub Save(webApi As WebApi)

            Dim id = webApi.Id
            Dim insert = id = Guid.Empty OrElse Not Exists(id)
            If insert Then
                id = InsertWebApi(webApi)
            Else
                UpdateWebApi(webApi)
                DeleteObsoleteItems(webApi)
            End If

            For Each commonParam In webApi.Configuration.CommonParameters
                SaveCommonParameter(commonParam, id)
            Next

            For Each commonHeader In webApi.Configuration.CommonRequestHeaders
                SaveCommonHeader(commonHeader, id)
            Next

            For Each action In webApi.Configuration.Actions
                SaveAction(action, id)
            Next

        End Sub

        ''' <summary>
        ''' Delete a Web API from the database (and all its child records)
        ''' </summary>
        Public Sub Delete(webApiId As Guid)
            DeleteActions(webApiId)
            DeleteCommonHeaders(webApiId)
            DeleteCommonParameters(webApiId)
            DeleteWebApi(webApiId)
        End Sub

        ''' <summary>
        ''' Delete any child records for the web api that are in the database,
        ''' but have been removed from the web api object
        ''' </summary>
        ''' <param name="webApi">
        ''' The web api to delete child records for
        ''' </param>
        Private Sub DeleteObsoleteItems(webApi As WebApi)

            ' Delete any common parameters that are in the db but have been removed
            ' from the Web Api
            Dim commonParamIds = webApi.
                                    Configuration.
                                    CommonParameters.
                                    Select(Function(x) x.Id)
            DeleteCommonParameters(webApi.Id, commonParamIds)


            ' Delete any common headers that are in the db but have been removed from
            ' the Web Api
            Dim commonHeaderIds = webApi.
                                    Configuration.
                                    CommonRequestHeaders.
                                    Select(Function(x) x.Id)
            DeleteCommonHeaders(webApi.Id, commonHeaderIds)


            For Each action In webApi.Configuration.Actions
                ' Delete any parameters for this action that are in the db but have been
                ' removed from the Web Api
                Dim actionParamIds = action.
                                        Parameters.
                                        Select(Function(x) x.Id)
                DeleteActionParameters(action.Id, actionParamIds)

                Dim outputParamIds = action.
                                        OutputParameterConfiguration.
                                        Parameters.
                                        Select(Function(x) x.Id)
                DeleteCustomOutputParameters(action.Id, outputParamIds)



                ' Delete any headers for this action that are in the db but have been
                ' removed from the Web Api
                Dim actionHeaderIds = action.
                                        Request.
                                        Headers.
                                        Select(Function(x) x.Id)
                DeleteActionHeaders(action.Id, actionHeaderIds)
            Next

            ' Delete any actions that are in the db but have been removed from the
            ' Web Api
            Dim actionIds = webApi.
                                Configuration.
                                Actions.
                                Select(Function(x) x.Id)
            DeleteActions(webApi.Id, actionIds)

        End Sub

        ''' <summary>
        ''' Saves the given Web API Common parameter to the database, overwriting the
        ''' current parameter if it exists and creating a new one if it doesn't
        ''' </summary>
        ''' <param name="parameter">The parameter to save</param>
        ''' <param name="webApiId">The id of web api the parameter belongs to</param>
        Private Sub SaveCommonParameter(parameter As ActionParameter, webApiId As Guid)
            If parameter.Id = 0 Then
                InsertCommonParameter(parameter, webApiId)
            Else
                UpdateParameter(parameter)
            End If

        End Sub

        ''' <summary>
        ''' Saves the given Web API action parameter to the database, overwriting the
        ''' current parameter if it exists and creating a new one if it doesn't
        ''' </summary>
        ''' <param name="parameter">The parameter to save</param>
        ''' <param name="actionId">The id of action the parameter belongs to</param>
        Private Sub SaveActionParameter(parameter As ActionParameter, actionId As Integer)
            If parameter.Id = 0 Then
                InsertActionParameter(parameter, actionId)
            Else
                UpdateParameter(parameter)
            End If

        End Sub

        Private Sub SaveCustomOutputParameter(parameter As ResponseOutputParameter, actionId As Integer)
            If parameter.Id = 0 Then
                InsertCustomOutputParameter(parameter, actionId)
            Else
                UpdateCustomOutputParameter(parameter)
            End If

        End Sub


        ''' <summary>
        ''' Saves the given Web API Common header to the database, overwriting the
        ''' current header if it exists and creating a new one if it doesn't
        ''' </summary>
        ''' <param name="header">The header to save</param>
        ''' <param name="webApiId">The id of web api the header belongs to</param>
        Private Sub SaveCommonHeader(header As HttpHeader, webApiId As Guid)

            If header.Id = 0 Then
                InsertCommonHeader(header, webApiId)
            Else
                UpdateHeader(header)
            End If

        End Sub

        ''' <summary>
        ''' Saves the given Web API action header to the database, overwriting the
        ''' current header if it exists and creating a new one if it doesn't
        ''' </summary>
        ''' <param name="header">The header to save</param>
        ''' <param name="actionId">The id of action the header belongs to</param>
        Private Sub SaveActionHeader(header As HttpHeader, actionId As Integer)

            If header.Id = 0 Then
                InsertActionHeader(header, actionId)
            Else
                UpdateHeader(header)
            End If

        End Sub

        ''' <summary>
        ''' Saves the given Web API action  to the database, overwriting the
        ''' current action if it exists and creating a new one if it doesn't
        ''' </summary>
        ''' <param name="action">The action to save</param>
        ''' <param name="webApiId">The id of the web api that the aciton belongs to</param>
        Private Sub SaveAction(action As WebApiAction, webApiId As Guid)
            Dim actionId As Integer

            If action.Id = 0 Then
                actionId = InsertAction(action, webApiId)
            Else
                UpdateAction(action)
                actionId = action.Id
            End If

            For Each header In action.Request.Headers
                SaveActionHeader(header, actionId)
            Next

            For Each parameter In action.Parameters
                SaveActionParameter(parameter, actionId)
            Next

            For Each parameter In action.OutputParameterConfiguration.Parameters
                SaveCustomOutputParameter(parameter, actionId)
            Next

        End Sub


#Region " Create Methods "

        ''' <summary>
        ''' Inserts a new web api record into the database. The Id value will be 
        ''' inserted as-is if it is not empty, otherwise a new Id value will be
        ''' generated. 
        ''' </summary>
        ''' <param name="webApi">The web api to create a new record for</param>
        ''' <returns>The id of the record that was inserted</returns>
        Private Function InsertWebApi(webApi As WebApi) As Guid

            Dim cmd As New SqlCommand(
             "  insert into BPAWebApiService output inserted.serviceid
                    values (isnull(@id, newid()),
                            @name,
                            @enabled,
                            getutcdate(),
                            @baseurl,
                            @authenticationtype,
                            @authenticationconfig,
                            @commoncodeproperties,
                            @httpRequestConnectionTimeout,     
                            @authServerRequestConnectionTimeout)")

            Dim idParameterValue As Object = GetDbNullIfEmpty(webApi.Id)
            With cmd.Parameters
                .AddWithValue("@id", idParameterValue)
                .AddWithValue("@name", webApi.Name)
                .AddWithValue("@baseurl", webApi.Configuration?.BaseUrl)
                .AddWithValue("@enabled", webApi.Enabled)
                .AddWithValue("@authenticationtype", webApi.
                                                        Configuration.
                                                        CommonAuthentication.
                                                        Type)
                .AddWithValue("@authenticationconfig", webApi.
                                                        Configuration.
                                                        CommonAuthentication.
                                                        ToXElement().
                                                        ToString())
                .AddWithValue("@commoncodeproperties", webApi.
                                                        Configuration.
                                                        CommonCode.
                                                        ToXElement().
                                                        ToString())
                .AddWithValue("@httpRequestConnectionTimeout", webApi.
                                                        Configuration.
                                                        ConfigurationSettings.
                                                        HttpRequestConnectionTimeout)
                .AddWithValue("@authServerRequestConnectionTimeout", webApi.
                                                        Configuration.
                                                        ConfigurationSettings.
                                                        AuthServerRequestConnectionTimeout)
            End With

            Return CType(mConnection.ExecuteReturnScalar(cmd), Guid)
        End Function

        ''' <summary>
        ''' Checks a Guid value and returns the original value if it is not empty or 
        ''' <c cref="DBNull.Value" /> if it is empty. Intended for setting SqlParameter 
        ''' values.
        ''' </summary>
        ''' <param name="value">The value to check</param>
        ''' <returns>The resulting value</returns>
        Private Function GetDbNullIfEmpty(value As Guid) As Object
            If value <> Guid.Empty Then
                Return value
            Else
                Return DBNull.Value
            End If
        End Function

        ''' <summary>
        ''' Inserts a new common parameter record into the database.
        ''' </summary>
        ''' <param name="parameter">The common parameter to create a new record for</param>
        ''' <param name="webApiId">The id of the service the parameter belongs to.</param>
        ''' <exception cref="ArgumentNullException"> Throws exception if
        ''' <paramref name="webApiId"/> is null</exception>
        ''' <remarks> A common parameter is stored in the BPAWebApiParameter table,
        ''' with its serviceid set as the id of the eb api it belongs to and actionid
        ''' set to null</remarks>
        Private Sub InsertCommonParameter(parameter As ActionParameter,
                                             webApiId As Guid)

            If webApiId = Nothing Then _
                Throw New ArgumentNullException(NameOf(webApiId))

            Dim cmd As New SqlCommand("
                insert into  BPAWebApiParameter
                    values (@serviceid,
                            null,
                            @name,
                            @description,
                            @expose,
                            @datatype,
                            @initvalue)")

            With cmd.Parameters
                .AddWithValue("@serviceid", webApiId)
                .AddWithValue("@name", parameter.Name)
                .AddWithValue("@description", parameter.Description)
                .AddWithValue("@expose", parameter.ExposeToProcess)
                .AddWithValue("@datatype", parameter.DataType)
                .AddWithValue("@initvalue", parameter.InitialValue.EncodedValue)
            End With

            mConnection.Execute(cmd)

        End Sub

        ''' <summary>
        ''' Inserts a new action parameter record into the database
        ''' </summary>
        ''' <param name="parameter">The action parameter to create a new record for</param>
        ''' <param name="actionId">The id of the action the parameter belongs to.</param>
        ''' <exception cref="ArgumentNullException"> Throws exception if
        ''' <paramref name="actionId"/> is null</exception>
        ''' <remarks> An action parameter is stored in the BPAWebApiParameter table,
        ''' with its serviceid set to null and actionid set to the id of th action it
        ''' belongs to</remarks>
        Private Sub InsertActionParameter(parameter As ActionParameter,
                                             actionId As Integer)

            If actionId = Nothing Then _
                Throw New ArgumentNullException(NameOf(actionId))

            Dim cmd As New SqlCommand("
                insert into  BPAWebApiParameter
                    values (null,
                            @actionid,
                            @name,
                            @description,
                            @expose,
                            @datatype,
                            @initvalue)")

            With cmd.Parameters
                .AddWithValue("@actionid", actionId)
                .AddWithValue("@name", parameter.Name)
                .AddWithValue("@description", parameter.Description)
                .AddWithValue("@expose", parameter.ExposeToProcess)
                .AddWithValue("@datatype", parameter.DataType)
                .AddWithValue("@initvalue", parameter.InitialValue.EncodedValue)
            End With

            mConnection.Execute(cmd)


        End Sub

        Private Sub InsertCustomOutputParameter(parameter As ResponseOutputParameter,
                                             actionId As Integer)

            If actionId = Nothing Then _
                Throw New ArgumentNullException(NameOf(actionId))

            Dim cmd As New SqlCommand("
                insert into  BPAWebApiCustomOutputParameter
                    values (@actionid,
                            @name,
                            @path,
                            @datatype,
                            @outputparametertype,
                            @description)")

            With cmd.Parameters
                .AddWithValue("@actionid", actionId)
                .AddWithValue("@name", parameter.Name)
                .AddWithValue("@path", parameter.Path)
                .AddWithValue("@datatype", parameter.DataType)
                .AddWithValue("@outputparametertype", CInt(parameter.Type))
                .AddWithValue("@description", parameter.Description)
            End With

            mConnection.Execute(cmd)

        End Sub

        ''' <summary>
        ''' Inserts a new common header record into the database
        ''' </summary>
        ''' <param name="header">The common header to create a new record for</param>
        ''' <param name="webApiId">The id of the service the header belongs to.</param>
        ''' <exception cref="ArgumentNullException"> Throws exception if
        ''' <paramref name="webApiId"/> is null</exception>
        ''' <remarks> A common header is stored in the BPAWebApiHeader table,
        ''' with its serviceid set as the id of the web api it belongs to and actionid
        ''' set to null</remarks>
        Private Sub InsertCommonHeader(header As HttpHeader, webApiId As Guid)


            If webApiId = Nothing Then _
                Throw New ArgumentNullException(NameOf(webApiId))

            Dim cmd As New SqlCommand("
                    insert into  BPAWebApiHeader
                        values (@serviceid,
                                null,
                                @name,
                                @value)")

            With cmd.Parameters
                .AddWithValue("@serviceid", webApiId)
                .AddWithValue("@name", header.Name)
                .AddWithValue("@value", header.Value)
            End With

            mConnection.Execute(cmd)

        End Sub

        ''' <summary>
        ''' Inserts a new action header record into the database
        ''' </summary>
        ''' <param name="header">The action header to create a new record for</param>
        ''' <param name="actionId">The id of the action the header belongs to.</param>
        ''' <exception cref="ArgumentNullException"> Throws exception if
        ''' <paramref name="actionId"/> is null</exception>
        ''' <remarks> An action header is stored in the BPAWebApiHeader table,
        ''' with its serviceid set to null and actionid set to the id of th action it
        ''' belongs to</remarks>
        Private Sub InsertActionHeader(header As HttpHeader, actionId As Integer)

            If actionId = Nothing Then _
                Throw New ArgumentNullException(NameOf(actionId))

            Dim cmd As New SqlCommand("
                    insert into  BPAWebApiHeader
                        values (null,
                                @actionid,
                                @name,
                                @value)")

            With cmd.Parameters
                .AddWithValue("@actionid", actionId)
                .AddWithValue("@name", header.Name)
                .AddWithValue("@value", header.Value)
            End With

            mConnection.Execute(cmd)

        End Sub

        ''' <summary>
        ''' Inserts a new action record into the database
        ''' </summary>
        ''' <param name="action">The action to create a new record for</param>
        ''' <param name="webApiId">The id of the web api the action belongs to.</param>
        ''' <returns>The id of the record that was inserted</returns>
        Private Function InsertAction(action As WebApiAction, webApiId As Guid) As Integer

            Dim cmd As New SqlCommand(
                "   insert into  BPAWebApiAction output inserted.actionid
                    values (@serviceid,
                            @name,
                            @description,
                            @enabled,
                            @requesthttpmethod,
                            @requesturlpath,
                            @requestbodytypeid,
                            @requestbodycontent,
                            @enableRequestOutputParameter,
                            @disableSendingOfRequest,
                            @outputparametercode)")

            With cmd.Parameters
                .AddWithValue("@serviceid", webApiId)
                .AddWithValue("@name", action.Name)
                .AddWithValue("@description", action.Description)
                .AddWithValue("@enabled", action.Enabled)
                .AddWithValue("@enableRequestOutputParameter", action.EnableRequestDataOutputParameter)
                .AddWithValue("@disableSendingOfRequest", action.DisableSendingOfRequest)
                .AddWithValue("@requesthttpmethod", action.Request.HttpMethod.Method)
                .AddWithValue("@requesturlpath", action.Request.UrlPath)
                .AddWithValue("@requestbodytypeid", CInt(action.Request.BodyContent.Type))
                .AddWithValue("@requestbodycontent", action.Request.BodyContent.ToXElement().ToString())
                .AddWithValue("@outputparametercode", If(CObj(action.OutputParameterConfiguration.Code), DBNull.Value))
            End With

            Dim actionId As Integer = CInt(mConnection.ExecuteReturnScalar(cmd))

            Return actionId

        End Function

#End Region

#Region " Read Methods "

        ''' <summary>
        ''' Indicates whether a web API exists in the database with the given id
        ''' </summary>
        ''' <param name="id">The id to test</param>
        ''' <returns>A boolean value indicating whether the web API exists</returns>
        Public Function Exists(id As Guid) As Boolean
            Dim cmd As New SqlCommand("
                                select 1
                                from BPAWebApiService
                                where serviceid = @id")

            cmd.Parameters.AddWithValue("@id", id)

            Dim result = mConnection.ExecuteReturnScalar(cmd)
            Return Equals(1, result)

        End Function

        ''' <summary>
        ''' Get a collection of all web apis from the database, containing just the
        ''' basic top level info that can be displayed on a list control
        ''' </summary>
        ''' <returns>A collection of Web APIs</returns>
        Public Function GetWebApiListItems() As ICollection(Of WebApiListItem)

            Dim cmd As New SqlCommand(
                    "   select
                            w.serviceid,
                            w.name,
                            w.enabled,
                            w.lastupdated,
                            w.baseurl,
                            isnull(a.actioncount, 0) as numberofactions
                        from  BPAWebApiService w
                            left join
                                (   select
                                        serviceid,
                                        count(actionid) as actioncount
                                    from
                                        BPAWebApiAction
                                    group by
                                        serviceid
                                ) as a
                            on w.serviceid = a.serviceid")

            Dim webApis As New List(Of WebApiListItem)
            Dim map As New Dictionary(Of Guid, WebApiListItem)
            Using reader = mConnection.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                While reader.Read()
                    Dim webApiId As Guid = prov.GetGuid("serviceid")
                    Dim ws As WebApiListItem = Nothing
                    If Not map.TryGetValue(webApiId, ws) Then
                        ws = New WebApiListItem
                        ws.Enabled = prov.GetBool("enabled")
                        ws.Name = prov.GetString("name")
                        ws.Id = prov.GetGuid("serviceid")
                        ws.LastUpdated =
                            prov.GetValue("lastupdated", DateTime.MinValue)
                        ws.NumberOfActions = prov.GetInt("numberofactions")
                        map(webApiId) = ws
                    End If
                End While
            End Using

            Dim skillDataAccess = New SkillDataAccess(mConnection)
            For Each api In map.Values
                api.AssociatedSkills = skillDataAccess.GetSkillNamesForWebApi(api.Id)
            Next
            Return map.Values
        End Function


        ''' <summary>
        ''' Get a web api from the database
        ''' </summary>
        ''' <param name="webApiId">The ID of the web service</param>
        ''' <returns>A web api object
        ''' </returns>
        ''' <exception cref="NoSuchElementException">If no web api exists with
        ''' the specified ID.</exception>
        Public Function GetWebApi(webApiId As Guid) As WebApi

            Dim name As String
            Dim enabled As Boolean
            Dim baseUrl As String
            Dim authenticationXml As String
            Dim commonCode As CodeProperties
            Dim settings As WebApiConfigurationSettings
            Dim cmd As New SqlCommand(
                            "   select  name,
                                        enabled,
                                        baseurl,
                                        authenticationconfig,
                                        commoncodeproperties,
                                        httpRequestConnectionTimeout,
                                        authServerRequestConnectionTimeout
                                from BPAWebApiService
                                where serviceid = @serviceid")

            cmd.Parameters.AddWithValue("@serviceid", webApiId)


            Using reader = mConnection.ExecuteReturnDataReader(cmd)
                Dim prov As New ReaderDataProvider(reader)
                If Not reader.Read() Then Throw New NoSuchElementException(
                    "No web api service found with id: {0}", webApiId)

                name = prov.GetString("name")
                enabled = prov.GetBool("enabled")
                baseUrl = prov.GetString("baseurl")
                authenticationXml = prov.GetString("authenticationconfig")
                Dim codePropertiesXml = XElement.Parse(prov.GetString("commoncodeproperties"))
                commonCode = CodeProperties.FromXElement(codePropertiesXml)
                settings = New WebApiConfigurationSettings(
                    prov.GetInt("httpRequestConnectionTimeout"),
                    prov.GetInt("authServerRequestConnectionTimeout"))
            End Using

            Dim actions = GetActions(webApiId)
            Dim commonHeaders = GetCommonHeaders(webApiId)
            Dim commonParams = GetCommonParameters(webApiId)
            Dim commonAuth = AuthenticationDeserializer.Deserialize(authenticationXml)
            Dim configuration = New WebApiConfiguration(baseUrl,
                                                        commonHeaders,
                                                        commonParams,
                                                        commonCode,
                                                        actions,
                                                        commonAuth,
                                                        settings)

            Return New WebApi(webApiId, name, enabled, configuration)

        End Function

        ''' <summary>
        ''' Get a collection of all the web apis from the database
        ''' </summary>
        ''' <returns>A collection of all the web apis form the database</returns>
        Public Function GetWebApis() As ICollection(Of WebApi)

            Dim cmd As New SqlCommand(
                             " select serviceid,
                                    name,
                                    enabled,
                                    baseurl,
                                    authenticationconfig,
                                    commoncodeproperties,
                                    httpRequestConnectionTimeout,
                                    authServerRequestConnectionTimeout
                            from BPAWebApiService")


            ' Store the results of the top-level query to the web api table
            Dim serviceDataList As New List(Of (id As Guid,
                                        name As String,
                                        enabled As Boolean,
                                        baseUrl As String,
                                        authConfig As String,
                                        commonCode As String,
                                        httpRequestConnectionTimeout As Integer,
                                        authServerRequestConnectionTimeout As Integer))

            Using reader = mConnection.ExecuteReturnDataReader(cmd)

                While reader.Read()
                    Dim prov As New ReaderDataProvider(reader)



                    serviceDataList.Add((prov.GetGuid("serviceid"),
                                         prov.GetString("name"),
                                         prov.GetBool("enabled"),
                                         prov.GetString("baseurl"),
                                         prov.GetString("authenticationconfig"),
                                         prov.GetString("commoncodeproperties"),
                                         prov.GetInt("httpRequestConnectionTimeout"),
                                         prov.GetInt("authServerRequestConnectionTimeout")))
                End While

            End Using

            Dim webApis As New List(Of WebApi)

            ' Loop through the top level results, query the database to get child
            ' records for each web api and create a new web api object to return
            For Each serviceData In serviceDataList

                Dim actions = GetActions(serviceData.id)
                Dim commonHeaders = GetCommonHeaders(serviceData.id)
                Dim commonParams = GetCommonParameters(serviceData.id)
                Dim commonAuth = AuthenticationDeserializer.Deserialize(serviceData.authConfig)
                Dim commonCodeProps = CodeProperties.FromXElement(XElement.Parse(serviceData.commonCode))
                Dim settings = New WebApiConfigurationSettings(
                                serviceData.httpRequestConnectionTimeout,
                                serviceData.authServerRequestConnectionTimeout)

                Dim configuration = New WebApiConfiguration(serviceData.baseUrl,
                                                            commonHeaders,
                                                            commonParams,
                                                            commonCodeProps,
                                                            actions,
                                                            commonAuth,
                                                            settings)

                webApis.Add(New WebApi(serviceData.id,
                                       serviceData.name,
                                       serviceData.enabled,
                                       configuration))
            Next

            Return webApis

        End Function

        ''' <summary>
        ''' Get a collection of a web api's common headers from the database
        ''' </summary>
        ''' <param name="webApiId">The id of the web api to get the common headers
        ''' for</param>
        ''' <returns>A collection of common headers</returns>
        Private Function GetCommonHeaders(webApiId As Guid) _
         As IReadOnlyCollection(Of HttpHeader)
            Dim cmd As New SqlCommand(
                              " select  headerid,
                                        name,
                                        value
                                from BPAWebApiHeader
                                where serviceid = @serviceid")

            cmd.Parameters.AddWithValue("@serviceid", webApiId)

            Using reader = mConnection.ExecuteReturnDataReader(cmd)
                Dim commonHeaders = New List(Of HttpHeader)
                While reader.Read()
                    Dim prov As New ReaderDataProvider(reader)
                    commonHeaders.Add(New HttpHeader(prov.GetInt("headerid"),
                                                 prov.GetString("name"),
                                                 prov.GetString("value")))
                End While

                Return commonHeaders
            End Using
        End Function

        ''' <summary>
        ''' Get a collection of a web api's common parameters from the database
        ''' </summary>
        ''' <param name="webApiId">The id of the web api to get the common parameters
        ''' for</param>
        ''' <returns>A collection of common parameters</returns>
        Private Function GetCommonParameters(webApiId As Guid) _
         As IReadOnlyCollection(Of ActionParameter)
            Dim cmd As New SqlCommand(
                              " select  parameterid,
                                        name,
                                        description,
                                        exposetoprocess,
                                        datatype,
                                        initvalue
                                from BPAWebApiParameter
                                where serviceid = @serviceid")

            cmd.Parameters.AddWithValue("@serviceid", webApiId)

            Using reader = mConnection.ExecuteReturnDataReader(cmd)
                Dim commonParameters = New List(Of ActionParameter)
                While reader.Read()
                    Dim prov As New ReaderDataProvider(reader)

                    Dim dataType = CType(prov.GetInt("datatype"), DataType)
                    Dim initialValue = clsProcessValue.Decode(dataType, prov.GetString("initvalue"))

                    commonParameters.Add(New ActionParameter(
                        prov.GetInt("parameterid"),
                        prov.GetString("name"),
                        prov.GetString("description"),
                        dataType,
                        prov.GetBool("exposetoprocess"),
                        initialValue))
                End While

                Return commonParameters
            End Using
        End Function

        ''' <summary>
        ''' Get a collection of an action's headers from the database
        ''' </summary>
        ''' <param name="actionId">The id of the action to get the headers for</param>
        ''' <returns>A collection of action headers</returns>
        Private Function GetActionHeaders(actionId As Integer) As IReadOnlyCollection(Of HttpHeader)
            Dim cmd As New SqlCommand(
                              " select  headerid,
                                        name,
                                        value
                                from BPAWebApiHeader
                                where actionid = @actionid")

            cmd.Parameters.AddWithValue("@actionid", actionId)

            Using reader = mConnection.ExecuteReturnDataReader(cmd)
                Dim commonHeaders = New List(Of HttpHeader)
                While reader.Read()
                    Dim prov As New ReaderDataProvider(reader)
                    commonHeaders.Add(New HttpHeader(prov.GetInt("headerid"),
                                                 prov.GetString("name"),
                                                 prov.GetString("value")))
                End While

                Return commonHeaders
            End Using
        End Function

        ''' <summary>
        ''' Get a collection of an action's parameters from the database
        ''' </summary>
        ''' <param name="actionId">The id of the action to get the parameters for</param>
        ''' <returns>A collection of action parameters</returns>
        Private Function GetActionParameters(actionId As Integer) As IReadOnlyCollection(Of ActionParameter)
            Dim cmd As New SqlCommand(
                              " select  parameterid,
                                        name,
                                        description,
                                        exposetoprocess,
                                        datatype,
                                        initvalue
                                from BPAWebApiParameter
                                where actionid = @actionid")

            cmd.Parameters.AddWithValue("@actionid", actionId)

            Using reader = mConnection.ExecuteReturnDataReader(cmd)
                Dim commonParameters = New List(Of ActionParameter)
                While reader.Read()
                    Dim prov As New ReaderDataProvider(reader)

                    Dim dataType = CType(prov.GetInt("datatype"), DataType)
                    Dim initialValue = clsProcessValue.Decode(dataType, prov.GetString("initvalue"))

                    commonParameters.Add(New ActionParameter(
                        prov.GetInt("parameterid"),
                        prov.GetString("name"),
                        prov.GetString("description"),
                        dataType,
                        prov.GetBool("exposetoprocess"),
                        initialValue)
)
                End While

                Return commonParameters
            End Using
        End Function

        ''' <summary>
        ''' Get a collection of a web api's actions from the database
        ''' </summary>
        ''' <param name="webApiId">The id of the web api to get the actions for</param>
        ''' <returns>A collection of actions</returns>
        Private Function GetActions(webApiId As Guid) As IReadOnlyCollection(Of WebApiAction)

            Dim actions As New List(Of WebApiAction)

            Dim cmd As New SqlCommand(
                              " select  actionid,
                                        serviceid,
                                        name,
                                        description,
                                        enabled,
                                        enableRequestOutputParameter,
                                        disableSendingOfRequest,
                                        requesthttpmethod,
                                        requesturlpath,
                                        requestbodytypeid,
                                        requestbodycontent,
                                        outputparametercode
                                from BPAWebApiAction
                                where serviceid = @serviceid")

            cmd.Parameters.AddWithValue("@serviceid", webApiId)

            Dim results As New List(Of (actionId As Integer, name As String, description As String,
                                    enabled As Boolean,
                                    enableRequestOutputParameter As Boolean, disableSendingOfRequest As Boolean,
                                    requestHttpMethod As String, requestUrlPath As String,
                                    requestBodyTypeId As Integer, requestBodyContent As String, outputParameterCode As String))

            Using reader = mConnection.ExecuteReturnDataReader(cmd)
                While reader.Read()
                    Dim prov As New ReaderDataProvider(reader)

                    Dim actionId = prov.GetInt("actionid")
                    Dim name = prov.GetString("name")
                    Dim description = prov.GetString("description")
                    Dim enabled = prov.GetBool("enabled")
                    Dim enableRequestOutputParameter = prov.GetBool("enableRequestOutputParameter")
                    Dim disableSendingOfRequest = prov.GetBool("disableSendingOfRequest")

                    Dim requestHttpMethod = prov.GetString("requesthttpmethod")
                    Dim requestUrlPath = prov.GetString("requesturlpath")
                    Dim requestBodyTypeId = prov.GetInt("requestbodytypeid")
                    Dim requestBodyContent = prov.GetString("requestbodycontent")

                    Dim outputParameterCode = prov.GetString("outputparametercode")

                    results.Add((actionId, name, description,
                             enabled, enableRequestOutputParameter, disableSendingOfRequest,
                             requestHttpMethod, requestUrlPath,
                             requestBodyTypeId, requestBodyContent, outputParameterCode))
                End While
            End Using

            ' Loop through each record from the BPAWebApiAction table, get their child
            ' parameters and headers and create a new WebApiAction for the record
            For Each action In results

                Dim actionHeaders = GetActionHeaders(action.actionId)
                Dim actionParameters = GetActionParameters(action.actionId)
                Dim customOutputParameters = GetCustomOutputParameters(action.actionId)

                Dim request = New WebApiRequest(New HttpMethod(action.requestHttpMethod),
                                                               action.requestUrlPath,
                                                               actionHeaders,
                                                               BodyContentDeserializer.Deserialize(action.requestBodyContent))

                Dim outputParameterConfiguration = New OutputParameterConfiguration(customOutputParameters, action.outputParameterCode)

                actions.Add(New WebApiAction(action.actionId,
                                                      action.name,
                                                      action.description,
                                                      action.enabled,
                                                      action.enableRequestOutputParameter,
                                                      action.disableSendingOfRequest,
                                                      request,
                                                      actionParameters,
                                                      outputParameterConfiguration))

            Next

            Return actions


        End Function

        Private Function GetCustomOutputParameters(actionId As Integer) As IReadOnlyCollection(Of ResponseOutputParameter)
            Dim cmd As New SqlCommand(
                              " select id, 
                                        name,
                                        path,
                                        datatype,
                                        outputparametertype,
                                        description
                                from BPAWebApiCustomOutputParameter
                                where actionid = @actionid")

            cmd.Parameters.AddWithValue("@actionid", actionId)

            Using reader = mConnection.ExecuteReturnDataReader(cmd)
                Dim parameters = New List(Of ResponseOutputParameter)
                While reader.Read()
                    Dim prov As New ReaderDataProvider(reader)
                    Dim type = CType(prov.GetInt("outputparametertype"), OutputMethodType)
                    Dim id = prov.GetInt("id")
                    Dim name = prov.GetString("name")
                    Dim path = prov.GetString("path")
                    Dim dataType = CType(prov.GetInt("datatype"), DataType)
                    Dim description = prov.GetString("description")

                    Dim outputParameter As ResponseOutputParameter

                    Select Case type
                        Case OutputMethodType.JsonPath
                            outputParameter = New JsonPathOutputParameter(id, name, description, path, dataType)
                        Case OutputMethodType.CustomCode
                            outputParameter = New CustomCodeOutputParameter(id, name, description, dataType)
                        Case Else
                            Throw New NotImplementedException()
                    End Select

                    parameters.Add(outputParameter)

                End While

                Return parameters
            End Using
        End Function

        ''' <summary>
        ''' Get the id of a web api
        ''' </summary>
        ''' <param name="name">The name of the web api to return the id of</param>
        ''' <returns>The id of a web api</returns>
        Public Function GetWebApiId(name As String) As Guid
            Dim cmd As New SqlCommand("
                                select serviceid
                                from BPAWebApiService
                                where name = @name")

            cmd.Parameters.AddWithValue("@name", name)

            Dim result = mConnection.ExecuteReturnScalar(cmd)
            Return If(result IsNot Nothing, CType(result, Guid), Guid.Empty)

        End Function

#End Region

#Region " Update Methods "

        ''' <summary>
        ''' Update a web api record in the database
        ''' </summary>
        ''' <param name="webApi">The web api to update the existing data with</param>
        Private Sub UpdateWebApi(webApi As WebApi)

            Dim cmd As New SqlCommand(
             "      update
                        BPAWebApiService
                    set
                        name = @name,
                        enabled = @enabled,
                        baseurl = @baseurl,
                        lastupdated = getutcdate(),
                        authenticationtype = @authtype,
                        authenticationconfig = @authconfig,
                        commoncodeproperties = @commoncode,
                        httpRequestConnectionTimeout = @httpRequestConnectionTimeout,
                        authServerRequestConnectionTimeout = @authServerRequestConnectionTimeout
                    where
                        serviceid = @serviceid"
            )

            With cmd.Parameters
                .AddWithValue("@serviceid", webApi.Id)
                .AddWithValue("@name", webApi.Name)
                .AddWithValue("@baseurl", webApi.Configuration?.BaseUrl)
                .AddWithValue("@enabled", webApi.Enabled)
                .AddWithValue("@authtype", CInt(webApi.Configuration.CommonAuthentication.Type))
                .AddWithValue("@authconfig", webApi.Configuration.CommonAuthentication.ToXElement().ToString())
                .AddWithValue("@commoncode", webApi.Configuration.CommonCode.ToXElement().ToString())
                .AddWithValue("@httpRequestConnectionTimeout", webApi.Configuration.ConfigurationSettings.HttpRequestConnectionTimeout)
                .AddWithValue("@authServerRequestConnectionTimeout", webApi.Configuration.ConfigurationSettings.AuthServerRequestConnectionTimeout)
            End With

            mConnection.ExecuteReturnScalar(cmd)

        End Sub

        ''' <summary>
        ''' Update a parameter record in the database
        ''' </summary>
        ''' <param name="parameter">The parameter to update the existing data with</param>
        Private Sub UpdateParameter(parameter As ActionParameter)
            Dim cmd As New SqlCommand("
                update
                    BPAWebApiParameter
                set
                    name = @name,
                    description = @description,
                    exposetoprocess = @expose,
                    datatype = @datatype,
                    initvalue = @initvalue
                where
                    parameterid = @parameterid")

            With cmd.Parameters
                .AddWithValue("@parameterid", parameter.Id)
                .AddWithValue("@name", parameter.Name)
                .AddWithValue("@description", parameter.Description)
                .AddWithValue("@expose", parameter.ExposeToProcess)
                .AddWithValue("@datatype", parameter.DataType)
                .AddWithValue("@initvalue", parameter.InitialValue.EncodedValue)
            End With

            mConnection.ExecuteReturnScalar(cmd)

        End Sub

        Private Sub UpdateCustomOutputParameter(parameter As ResponseOutputParameter)
            Dim cmd As New SqlCommand("
                update
                    BPAWebApiCustomOutputParameter
                set
                    name = @name,
                    path = @path,
                    datatype = @datatype,
                    outputparametertype = @outputparametertype,
                    description = @description

                where
                    id = @id")

            With cmd.Parameters
                .AddWithValue("@id", parameter.Id)
                .AddWithValue("@name", parameter.Name)
                .AddWithValue("@path", parameter.Path)
                .AddWithValue("@datatype", parameter.DataType)
                .AddWithValue("@outputparametertype", CInt(parameter.Type))
                .AddWithValue("@description", parameter.Description)
            End With

            mConnection.ExecuteReturnScalar(cmd)

        End Sub

        ''' <summary>
        ''' Update a header record in the database
        ''' </summary>
        ''' <param name="header">The header to update the existing data with</param>
        Private Sub UpdateHeader(header As HttpHeader)

            Dim cmd As New SqlCommand(
                "   update
                        BPAWebApiHeader
                    set
                        name = @name,
                        value = @value
                    where
                        headerid = @headerid")

            With cmd.Parameters
                .AddWithValue("@headerid", header.Id)
                .AddWithValue("@name", header.Name)
                .AddWithValue("@value", header.Value)
            End With

            mConnection.Execute(cmd)

        End Sub

        ''' <summary>
        ''' Update a action record in the database
        ''' </summary>
        ''' <param name="action">The action to update the existing data with</param>
        Private Sub UpdateAction(action As WebApiAction)

            Dim cmd As New SqlCommand(
                "   update
                        BPAWebApiAction
                    set
                        name = @name,
                        description = @description,
                        enabled = @enabled,
                        enableRequestOutputParameter = @enableRequestOutputParameter,
                        disableSendingOfRequest = @disableSendingOfRequest,
                        requesthttpmethod =  @requesthttpmethod,
                        requesturlpath = @requesturlpath,
                        requestbodytypeid = @requestbodytypeid,
                        requestbodycontent = @requestbodycontent,
                        outputparametercode = @outputparametercode
                    where
                        actionid = @actionid")

            With cmd.Parameters
                .AddWithValue("@actionid", action.Id)
                .AddWithValue("@name", action.Name)
                .AddWithValue("@description", action.Description)
                .AddWithValue("@enabled", action.Enabled)
                .AddWithValue("@enableRequestOutputParameter", action.EnableRequestDataOutputParameter)
                .AddWithValue("@disableSendingOfRequest", action.DisableSendingOfRequest)
                .AddWithValue("@requesthttpmethod", action.Request.HttpMethod.Method)
                .AddWithValue("@requesturlpath", action.Request.UrlPath)
                .AddWithValue("@requestbodytypeid", CInt(action.Request.BodyContent.Type))
                .AddWithValue("@requestbodycontent", action.Request.BodyContent.ToXElement().ToString())
                .AddWithValue("@outputparametercode", If(CObj(action.OutputParameterConfiguration.Code), DBNull.Value))
            End With

            mConnection.Execute(cmd)

        End Sub

        ''' <summary>
        ''' Update the web api service enabled flag
        ''' </summary>
        ''' <param name="webApiId">The id of the web api to update the value for</param>
        ''' <param name="enabled">The value to set the enabled flag to</param>
        Public Sub UpdateWebApiEnabledFlag(webApiId As Guid, enabled As Boolean)
            Dim cmd As New SqlCommand("
                              update BPAWebApiService
                              set enabled = @enabled
                              where serviceid = @serviceid")
            With cmd.Parameters
                .AddWithValue("@enabled", enabled)
                .AddWithValue("@serviceid", webApiId)
            End With
            mConnection.Execute(cmd)
        End Sub


#End Region

#Region " Delete Methods "

        ''' <summary>
        ''' Delete a web api record from the database
        ''' </summary>
        ''' <param name="webApiId">The id of the web api to delete</param>
        Private Sub DeleteWebApi(webApiId As Guid)
            Dim cmd As New SqlCommand("delete from BPAWebApiService
                                            where serviceid = @serviceid")
            With cmd.Parameters
                .AddWithValue("@serviceid", webApiId)
            End With

            mConnection.Execute(cmd)

        End Sub

        ''' <summary>
        ''' Delete the common parameters of a web api from the database
        ''' </summary>
        ''' <param name="webApiId">The id of the web api to delete the common
        ''' parameters for </param>
        Private Sub DeleteCommonParameters(webApiId As Guid)
            DeleteCommonParameters(webApiId, Enumerable.Empty(Of Integer))
        End Sub

        ''' <summary>
        ''' Delete the common parameters of a web api from the database
        ''' </summary>
        ''' <param name="webApiId">The id of the web api to delete the common
        ''' parameters for </param>
        ''' <param name="excludedIds">
        ''' The ids of any parameters that should be excluded from the delete
        ''' statement
        ''' </param>
        Private Sub DeleteCommonParameters(
         webApiId As Guid, excludedIds As IEnumerable(Of Integer))

            Dim cmd As New SqlCommand()
            cmd.CommandText =
                " delete from
                    BPAWebApiParameter
                  where
                    serviceid = @serviceid and
                    actionid is null and
                    not " & cmd.BuildSqlInStatement("parameterid", excludedIds)

            With cmd.Parameters
                .AddWithValue("@serviceid", webApiId)
            End With

            mConnection.Execute(cmd)

        End Sub

        ''' <summary>
        ''' Delete the parameters of a web api action from the database
        ''' </summary>
        ''' <param name="actionid">The id of the web api action to delete the
        ''' parameters for </param>
        Private Sub DeleteActionParameters(actionId As Integer)
            DeleteActionParameters(actionId, Enumerable.Empty(Of Integer))
        End Sub

        ''' <summary>
        ''' Delete the parameters of a web api action from the database
        ''' </summary>
        ''' <param name="actionid">The id of the web api action to delete the
        ''' parameters for </param>
        ''' <param name="excludedIds">
        ''' The ids of any parameters that should be excluded from the delete
        ''' statement
        ''' </param>
        Private Sub DeleteActionParameters(
         actionId As Integer, excludedIds As IEnumerable(Of Integer))

            Dim cmd = New SqlCommand()

            cmd.CommandText =
                " delete from
                    BPAWebApiParameter
                  where
                    serviceid is null and
                    actionid = @actionid and
                    not " & cmd.BuildSqlInStatement("parameterid", excludedIds)

            With cmd.Parameters
                .AddWithValue("@actionid", actionId)
            End With

            mConnection.Execute(cmd)

        End Sub

        Private Sub DeleteCustomOutputParameters(
         actionid As Integer, excludedIds As IEnumerable(Of Integer))

            Dim cmd = New SqlCommand()

            cmd.CommandText =
                " delete from
                    BPAWebAPICustomOutputParameter
                  where
                    actionid = @actionid and
                    not " & cmd.BuildSqlInStatement("id", excludedIds)

            With cmd.Parameters
                .AddWithValue("@actionid", actionid)
            End With

            mConnection.Execute(cmd)

        End Sub

        ''' <summary>
        ''' Delete the common headers of a web api from the database
        ''' </summary>
        ''' <param name="webApiId">The id of the web api to delete the common
        ''' headers for </param>
        Private Sub DeleteCommonHeaders(webApiId As Guid)
            DeleteCommonHeaders(webApiId, Enumerable.Empty(Of Integer))
        End Sub

        ''' <summary>
        ''' Delete the common headers of a web api from the database
        ''' </summary>
        ''' <param name="webApiId">The id of the web api to delete the common
        ''' headers for </param>
        ''' <param name="excludedIds">
        ''' The ids of any headers that should be excluded from the delete statement
        ''' </param>
        Private Sub DeleteCommonHeaders(
         webApiId As Guid, excludedIds As IEnumerable(Of Integer))

            Dim cmd = New SqlCommand()

            cmd.CommandText =
                " delete from
                    BPAWebApiHeader
                  where
                    serviceid = @serviceid and
                    actionid is null and
                    not " & cmd.BuildSqlInStatement("headerid", excludedIds)

            With cmd.Parameters
                .AddWithValue("@serviceid", webApiId)
            End With

            mConnection.Execute(cmd)

        End Sub

        ''' <summary>
        ''' Delete the headers of a web api action from the database
        ''' </summary>
        ''' <param name="actionid">The id of the web api action to delete the
        ''' headers for </param>
        Private Sub DeleteActionHeaders(actionId As Integer)
            DeleteActionHeaders(actionId, Enumerable.Empty(Of Integer))
        End Sub

        ''' <summary>
        ''' Delete the headers of a web api action from the database
        ''' </summary>
        ''' <param name="actionid">The id of the web api action to delete the
        ''' headers for </param>
        ''' <param name="excludedIds">
        ''' The ids of any headers that should be excluded from the delete statement
        ''' </param>
        Private Sub DeleteActionHeaders(
         actionId As Integer, excludedIds As IEnumerable(Of Integer))
            Dim cmd = New SqlCommand()

            cmd.CommandText =
                " delete from
                    BPAWebApiHeader
                  where
                    serviceid is null and
                    actionid = @actionid and
                    not " & cmd.BuildSqlInStatement("headerid", excludedIds)

            With cmd.Parameters
                .AddWithValue("@actionid", actionId)
            End With

            mConnection.Execute(cmd)

        End Sub

        ''' <summary>
        ''' Delete the actions of a web api from the database
        ''' </summary>
        ''' <param name="webApiId">The id of the web api to delete the
        ''' actions for </param>
        Private Sub DeleteActions(webApiId As Guid)
            DeleteActions(webApiId, Enumerable.Empty(Of Integer))
        End Sub

        ''' <summary>
        ''' Delete the actions of a web api from the database. This is a cascade
        ''' delete and will also delete any of the actions parameters and headers.
        ''' </summary>
        ''' <param name="webApiId">The id of the web api to delete the
        ''' actions for </param>
        ''' <param name="excludedActionIds">
        ''' The ids of any actions that should be excluded from the delete statement
        ''' </param>
        Private Sub DeleteActions(webApiId As Guid,
            excludedActionIds As IEnumerable(Of Integer))

            Dim cmd = New SqlCommand()

            Dim actionIdInExcludedIds =
                cmd.BuildSqlInStatement("a.actionid", excludedActionIds)

            cmd.CommandText = $" delete p
                                 from BPAWebApiParameter p
                                 left join BPAWebApiAction a
                                    on p.actionid = a.actionid
                                 where
                                    p.serviceid is null and
                                    p.actionid is not null and
                                    a.serviceid = @serviceid and
                                    not {actionIdInExcludedIds};

                                 delete p
                                 from BPAWebApiCustomOutputParameter p
                                 left join BPAWebApiAction a
                                    on p.actionid = a.actionid
                                 where
                                    a.serviceid = @serviceid and
                                    p.actionid is not null and
                                    not {actionIdInExcludedIds};

                                 delete h
                                 from BPAWebApiHeader h
                                 left join BPAWebApiAction a
                                    on h.actionid = a.actionid
                                 where
                                    h.serviceid is null and
                                    h.actionid is not null and
                                    a.serviceid = @serviceid and
                                    not {actionIdInExcludedIds};

                                 delete a
                                 from BPAWebApiAction a
                                 where
                                    serviceid = @serviceid and
                                 not {actionIdInExcludedIds};"

            cmd.Parameters.AddWithValue("@serviceid", webApiId)

            mConnection.Execute(cmd)

        End Sub
#End Region

    End Class

End Namespace
