Imports System.Xml
Imports System.Linq
Imports BluePrism.AutomateProcessCore.Stages

Namespace WebApis

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' A class that represents a Web API service, this class makes Web API services compatible
    ''' with business objects.
    ''' </summary>
    ''' -----------------------------------------------------------------------------
    Public Class WebApiBusinessObject
        Inherits clsBusinessObject
        Implements IDiagnosticEmitter

        Public ReadOnly WebApi As WebApi

        ''' <summary>
        ''' Event raised with diagnostics messages
        ''' </summary>
        Public Event Diags(ByVal message As String) Implements IDiagnosticEmitter.Diags

        Friend Sub RaiseDiags(ByVal msg As String)
            RaiseEvent Diags(msg)
        End Sub

        ''' <summary>
        ''' Creates a new Instance of WebApiBusinessObject for use in Process
        ''' Studio.
        ''' </summary>
        ''' <param name="webApiService">The WebApi instance which contains data
        ''' about the connection. </param>
        Public Sub New(ByRef webApiService As WebApi)

            MyBase.New(webApiService.Name, webApiService.FriendlyName)

            WebApi = webApiService

            mConfigurable = False
            mLifecycle = False
            mValid = True

            Dim actions = WebApi.Configuration.Actions.
                Where(Function(x) x.Enabled).
                Select(Function(x) _
                        New WebApiBusinessObjectAction(
                            x,
                            WebApi.Id,
                            WebApi.Configuration))

            For Each action In actions
                AddAction(action)
                AddHandler action.Diags, AddressOf RaiseDiags
            Next
        End Sub

        ''' <inheritdoc />
        Public Overrides Sub DisposeTasks()
            'Nothing required here.
        End Sub

        ''' <inheritdoc />
        Protected Overrides Sub GetHTMLPreamble(xr As XmlTextWriter)
            xr.WriteElementString("h1", My.Resources.Resources.WebApiBusinessObject_WebAPIServiceDefinition)
            xr.WriteElementString("div", My.Resources.Resources.WebApiBusinessObject_TheInformationContainedInThisDocumentIsTheProprietaryInformationOfTheThirdParty)
            xr.WriteElementString("h2", My.Resources.Resources.WebApiBusinessObject_AboutThisDocument)
            xr.WriteElementString("div", My.Resources.Resources.WebApiBusinessObject_TheWebAPIServiceDefinitionDescribesTheActionsAvailableWithinASingleWebAPITheirP)
        End Sub

        ''' <inheritdoc />
        Public Overrides Function DoInit() As StageResult
            Return New StageResult(True)
        End Function

        ''' <inheritdoc />
        Public Overrides Function DoCleanUp() As StageResult
            Return New StageResult(True)
        End Function

        ''' <inheritdoc />
        Public Overrides Function ShowConfigUI(ByRef sErr As String) As Boolean
            sErr = My.Resources.Resources.WebApiBusinessObject_AWebAPIServiceCannotBeConfigured
            Return False
        End Function

        ''' <inheritdoc />
        Public Overrides Function GetConfig(ByRef sErr As String) As String
            sErr = My.Resources.Resources.WebApiBusinessObject_NoConfigurationAvailableForAWebAPIServices
            Return ""
        End Function

        ''' <inheritdoc />
        Protected Overrides Function DoDoAction(actionName As String,
                                                scopeStage As clsProcessStage,
                                                inputs As clsArgumentList,
                                                ByRef outputs As clsArgumentList) As StageResult

            Dim action = GetAction(actionName)

            If action Is Nothing Then
                Dim message = String.Format(WebApiResources.UnknownActionErrorTemplate, actionName)
                Return New StageResult(False, My.Resources.Resources.WebApiBusinessObject_Internal, message)
            End If

            Dim webApiAction = DirectCast(GetAction(actionName), WebApiBusinessObjectAction)
            outputs = webApiAction.DoAction(inputs, scopeStage)

            Return New StageResult(True)
        End Function
    End Class
End Namespace

