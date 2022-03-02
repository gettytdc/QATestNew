Imports BluePrism.AutomateProcessCore.Stages

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsWebService
''' 
''' <summary>
''' A class that represents a webservice, this class makes web services compatible
''' with business objects.
''' </summary>
Public Class clsWebService
    Inherits clsBusinessObject
    Implements IDiagnosticEmitter

    ''' <summary>
    ''' Event raised when there is diagnostics information (e.g. protocol messages)
    ''' </summary>
    Public Event Diags(ByVal msg As String) Implements IDiagnosticEmitter.Diags

    ''' <summary>
    ''' Holds the url of the WSDL
    ''' </summary>
    Private msWSDLLocation As String

    ''' <summary>
    ''' Holds the location of the webservice, this is the url to call
    ''' when executing an action
    ''' </summary>
    Private msServiceLocation As String

    ''' <summary>
    ''' Holds the internal servicename of the web  service, this should not
    ''' be seen by the user.
    ''' </summary>
    Private msInternalName As String

    Private msWSDL As String

    ''' <summary>
    ''' Provides access to the internal servicename
    ''' </summary>
    ''' <value></value>
    Public Property InternalName() As String
        Get
            Return msInternalName
        End Get
        Set(ByVal Value As String)
            msInternalName = Value
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the location of the WSDL
    ''' </summary>
    ''' <value></value>
    Public Property WSDLLocation() As String
        Get
            Return msWSDLLocation
        End Get
        Set(ByVal Value As String)
            msWSDLLocation = Value
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the location of the service.
    ''' </summary>
    ''' <value></value>
    Public Property ServiceLocation() As String
        Get
            Return msServiceLocation
        End Get
        Set(ByVal Value As String)
            msServiceLocation = Value
        End Set
    End Property

    ''' <summary>
    ''' Provides access to the Web Service Description Language (WSDL)
    ''' </summary>
    ''' <value></value>
    Public Property WSDL() As String
        Get
            Return msWSDL
        End Get
        Set(ByVal Value As String)
            msWSDL = Value
        End Set
    End Property

    ''' <summary>
    ''' Initializes a new instance of the clsWebService class.
    ''' </summary>
    Public Sub New()
        MyBase.New()
        'Web services are never configurable...
        mConfigurable = False
        'And they don't support lifecycle management...
        mLifecycle = False
        mValid = True
    End Sub

    ''' <summary>
    ''' Handles anything that must be done to dispose the object.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub DisposeTasks()
        'Nothing required here.
    End Sub

    ''' <summary>
    ''' Initialise the business object, as per API documentation.
    ''' </summary>
    ''' <returns>A clsProcessStage.Result</returns>
    Public Overrides Function DoInit() As StageResult
        Return New StageResult(True)
    End Function

    ''' <summary>
    ''' Clean up the business object, as per API documentation.
    ''' </summary>
    ''' <returns>A clsProcessStage.Result</returns>
    Public Overrides Function DoCleanUp() As StageResult
        Return New StageResult(True)
    End Function

    ''' <summary>
    ''' Performs the action on the web service.
    ''' </summary>
    ''' <param name="sActionName">The name of the action.</param>
    ''' <param name="ScopeStage">The stage used to resolve scope within the business
    ''' object action. Not relevant for web services. May be null.</param>
    ''' <param name="inputs">The inputsxml of the action.</param>
    ''' <param name="outputs">The outputsxml of the action.</param>
    ''' <returns>A clsProcessStage.Result</returns>
    Protected Overrides Function DoDoAction(ByVal sActionName As String, ByVal ScopeStage As clsProcessStage, ByVal inputs As clsArgumentList, ByRef outputs As clsArgumentList) As StageResult

        For Each act As clsWebServiceAction In mActions
            If act.GetName = sActionName Then
                act.Inputs = inputs
                act.DoAction(msServiceLocation)
                outputs = act.Outputs
            End If
        Next

        Return New StageResult(True)
    End Function

    ''' <summary>
    ''' Provides access to the runmode of the webservice, webservices always run in the
    ''' background.
    ''' </summary>
    ''' <value></value>
    Public Overrides Property Runmode() As BusinessObjectRunMode
        Get
            Return BusinessObjectRunMode.Background
        End Get
        Set(ByVal Value As BusinessObjectRunMode)
        End Set
    End Property


    ''' <summary>
    ''' Show Config UI on the Business Object
    ''' </summary>
    ''' <param name="sErr">On failure, an error description</param>
    ''' <returns>True if successful, False otherwise</returns>
    Public Overrides Function ShowConfigUI(ByRef sErr As String) As Boolean
        sErr = My.Resources.Resources.clsWebService_AWebServiceCannotBeConfigured
        Return False
    End Function

    ''' <summary>
    ''' Get configuration on the Business Object
    ''' </summary>
    ''' <param name="sErr">On failure, an error description, otherwise an
    ''' empty string</param>
    ''' <returns>The ConfigXML of the Businessobject</returns>
    Public Overrides Function GetConfig(ByRef sErr As String) As String
        sErr = My.Resources.Resources.clsWebService_NoConfigurationAvailableForAWebService
        Return ""
    End Function

    ''' <summary>
    ''' Gets the html preamble for the web service.
    ''' </summary>
    ''' <param name="xr"></param>
    Protected Overrides Sub GetHTMLPreamble(ByVal xr As System.Xml.XmlTextWriter)
        xr.WriteElementString("h1", My.Resources.Resources.clsWebService_WebServiceDefinition)
        xr.WriteElementString("div", My.Resources.Resources.clsWebService_TheInformationContainedInThisDocumentIsTheProprietaryInformationOfThirdPartyHos)
        xr.WriteElementString("h2", My.Resources.Resources.clsWebService_AboutThisDocument)
        xr.WriteElementString(My.Resources.Resources.clsWebService_Div, My.Resources.Resources.clsWebService_TheWebServiceDefinitionDescribesTheAPIsAvailableWithinASingleWebServiceTheirPar)
    End Sub

    ''' <summary>
    ''' Handler for protocol diagnostics messages.
    ''' </summary>
    ''' <param name="msg">The message.</param>
    Public Sub ProtocolDiags(ByVal msg As String)
        RaiseEvent Diags(msg)
    End Sub

End Class
