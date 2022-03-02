
Imports System.Web.Services.Description
Imports System.Xml.Serialization
Imports System.Xml.Schema
Imports System.Xml
Imports System.Net
Imports System.Runtime.Remoting.Metadata.W3cXsd2001

Imports BluePrism.Common.Security
Imports System.IO

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsWSDLProcess
''' 
''' <summary>
''' This class loads a WSDL description and picks up the necessary information to
''' call the web service.
''' </summary>
Public Class clsWSDLProcess

    Public Event Diagnostic(ByVal message As String)

    ''' <summary>
    ''' This is used to hold the various schemas relating to the webservice
    ''' and is used to obtain information during the investigation
    ''' </summary>
    Private mSchemas As New XmlSchemas()

    ''' <summary>
    ''' This collection holds web services in most cases there will only one web
    ''' service but the collection provides useful functions like:
    ''' allServices.GetPortType(b.Type)
    ''' </summary>
    Private mServiceDescriptions As ServiceDescriptionCollection

    ''' <summary>
    ''' Holds a reference to the web operation currently under investigation.
    ''' this is added to the mobjWebServices collection of actions as and when 
    ''' the operation has been fully investigated.
    ''' </summary>
    Private mobjAction As clsWebServiceAction

    ''' <summary>
    ''' Holds the current direction of the parameter
    ''' </summary>
    Private mCurrentDir As ParamDirection

    ''' <summary>
    ''' Holds the current use of the parameter
    ''' </summary>
    Private mCurrentUse As SoapBindingUse

    Private mCurrentNarrative As String

    ''' <summary>
    ''' Holds the current encoding of the parameter
    ''' </summary>
    Private mCurrentEncoding As String

    ''' <summary>
    ''' Holds the details of the web service including the wsdl and the settingsXML.
    ''' </summary>
    Private mServiceDetails As clsWebServiceDetails

    ''' <summary>
    ''' Retrieves and imports a WSDL definition from a URL, which can be any of the
    ''' protocol variants supported by the WebRequest class, e.g. http:, file: or
    ''' ftp:.
    ''' </summary>
    ''' <param name="wsdlURL">The URL of the location of the WSDL</param>
    ''' <param name="objServiceDetails">The object containing credentials and also
    ''' to import the service details into.</param>
    ''' <returns>A Dictionary of clsWebService instances, keyed on the name of the
    ''' service.</returns>
    Public Function Import(ByVal wsdlURL As String, ByVal objServiceDetails As clsWebServiceDetails) As Dictionary(Of String, clsWebService)
        'Make sure urls are in standard format i.e. using / instead of \
        Dim url As New Uri(wsdlURL)
        Return Import(url, objServiceDetails)
    End Function

    ''' <summary>
    ''' Retrieves and imports a WSDL definition from a URL, which can be any of the
    ''' protocol variants supported by the WebRequest class, e.g. http:, file: or
    ''' ftp:.
    ''' </summary>
    ''' <param name="Url">The URL of the location of the WSDL</param>
    ''' <param name="objServiceDetails">The object containing credentials and also
    ''' to import the service details into.</param>
    ''' <returns>A Dictionary of clsWebService instances, keyed on the name of the
    ''' service.</returns>
    Public Function Import(ByVal Url As Uri, ByVal objServiceDetails As clsWebServiceDetails) As Dictionary(Of String, clsWebService)
        mServiceDetails = objServiceDetails

        Dim discovery As New System.Web.Services.Discovery.DiscoveryClientProtocol
        discovery.AllowAutoRedirect = True

        If mServiceDetails.Username IsNot Nothing AndAlso mServiceDetails.Secret IsNot Nothing Then
            discovery.Credentials = New Net.NetworkCredential(mServiceDetails.Username, mServiceDetails.Secret)
        Else
            discovery.Credentials = CredentialCache.DefaultCredentials
        End If

        If mServiceDetails.Certificate IsNot Nothing Then
            discovery.ClientCertificates.Add(mServiceDetails.Certificate)
        End If

        'Try to resolve the wsdl document to ensure there are no errors
        'downloading the document
        Try
            Dim webRequest As WebRequest = webRequest.Create(Url.ToString)
            If mServiceDetails.Username IsNot Nothing AndAlso mServiceDetails.Secret IsNot Nothing Then
                webRequest.Credentials = New Net.NetworkCredential(mServiceDetails.Username, mServiceDetails.Secret)
            Else
                webRequest.Credentials = CredentialCache.DefaultCredentials
            End If

            Dim httpWebRequest As HttpWebRequest = TryCast(webRequest, HttpWebRequest)
            If httpWebRequest IsNot Nothing AndAlso mServiceDetails.Certificate IsNot Nothing Then
                httpWebRequest.ClientCertificates.Add(mServiceDetails.Certificate)
            End If
            webRequest.GetResponse()
            webRequest.Abort()
        Catch ex As WebException
            If ex.InnerException IsNot Nothing Then
                Throw ex.InnerException
            Else
                Throw ex
            End If
        End Try

        discovery.DiscoverAny(Url.ToString)
        discovery.ResolveAll()

        mServiceDescriptions = New ServiceDescriptionCollection

        Dim refs As Web.Services.Discovery.DiscoveryClientReferenceCollection = discovery.References

        Dim root As Web.Services.Discovery.ContractReference = TryCast(refs.Item(Url.ToString), Web.Services.Discovery.ContractReference)

        For Each s As String In refs.Keys
            Dim cr As Web.Services.Discovery.ContractReference = TryCast(refs.Item(s), Web.Services.Discovery.ContractReference)
            If cr IsNot Nothing Then
                RaiseEvent Diagnostic(String.Format(My.Resources.Resources.clsWSDLProcess_RetrievedWsdlFrom0, cr.Ref))
                Dim contract As ServiceDescription = cr.Contract
                mServiceDescriptions.Add(contract)
                If cr IsNot root Then
                    Dim sWSDL As String = GetWSDL(contract)
                    mServiceDetails.ExtraWSDL.Add(sWSDL)
                End If
            End If
            Dim sr As Web.Services.Discovery.SchemaReference = TryCast(refs.Item(s), Web.Services.Discovery.SchemaReference)
            If sr IsNot Nothing Then
                RaiseEvent Diagnostic(String.Format(My.Resources.Resources.clsWSDLProcess_RetrievedSchemaFrom0, sr.Ref))
                Dim schema As XmlSchema = sr.Schema
                mSchemas.Add(schema)
                Dim sSchema As String = GetSchema(schema)
                mServiceDetails.Schemas.Add(sSchema)
            End If
        Next

        mServiceDetails.WSDL = GetWSDL(root.Contract)

        Try
            Dim services As Dictionary(Of String, clsWebService)
            services = Investigate()

            mServiceDetails.Loaded = True

            RaiseEvent Diagnostic(My.Resources.Resources.clsWSDLProcess_SUCCESSRetrievedAllWebServiceDetails)
            Return services
        Catch ex As Exception
            RaiseEvent Diagnostic(ex.Message)
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Convert a service description into string
    ''' </summary>
    ''' <param name="service">The service description</param>
    Private Function GetWSDL(ByVal service As ServiceDescription) As String
        Using ms As New MemoryStream()
            Using xw As New XmlTextWriter(ms, Text.Encoding.UTF8)
                xw.Formatting = Formatting.Indented
                service.Write(xw)
                ms.Seek(0, SeekOrigin.Begin)
                Using sr As New StreamReader(ms)
                    Return sr.ReadToEnd()
                End Using
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Convert an xml schema into a string
    ''' </summary>
    ''' <param name="schema">The xml schema</param>
    Private Function GetSchema(ByVal schema As XmlSchema) As String
        Using ms As New MemoryStream()
            Using xw As New XmlTextWriter(ms, Text.Encoding.UTF8)
                xw.Formatting = Formatting.Indented
                schema.Write(xw)
                ms.Seek(0, SeekOrigin.Begin)
                Using sr As New StreamReader(ms)
                    Return sr.ReadToEnd()
                End Using
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Loads the service description from an clsWebServiceDetails class.
    ''' </summary>
    ''' <param name="serviceDetails">A clsWebserviceDetails details class
    ''' containing the WSDL and configXML of the web service.</param>
    ''' <returns>A clsWebservice class representing the web service.</returns>
    Public Function Load(ByVal serviceDetails As clsWebServiceDetails) As clsWebService
        Dim webService As clsWebService

        Try
            mServiceDetails = serviceDetails
            mServiceDetails.Loaded = True

            mServiceDescriptions = New ServiceDescriptionCollection()
            LoadDescription(mServiceDetails.WSDL)
            For Each s As String In mServiceDetails.ExtraWSDL
                LoadDescription(s)
            Next
            LoadSchemas(serviceDetails)

            CompileSchemas()

            For Each sd As ServiceDescription In mServiceDescriptions
                For Each sv As Service In sd.Services
                    If String.Equals(sv.Name, serviceDetails.ServiceToUse, StringComparison.InvariantCultureIgnoreCase) Then
                        webService = Investigate(sv)
                        webService.FriendlyName = serviceDetails.FriendlyName
                        'the name stored in the xml should also be the friendlyname
                        webService.Name = serviceDetails.FriendlyName
                        webService.WSDL = mServiceDetails.WSDL
                        Return webService
                    End If
                Next
            Next
            Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsWSDLProcess_Service0NotDefined, serviceDetails.ServiceToUse))

        Catch ex As Exception
            webService = New clsWebService()
            webService.Valid = False
            webService.ErrorMessage = My.Resources.Resources.clsWSDLProcess_CouldNotLoadWebServiceDescriptionLanguage & ex.Message
            Return webService
        End Try

    End Function

    Private Sub LoadDescription(ByVal sWSDL As String)
        Using tr As New IO.StringReader(sWSDL)
            Using xtr As New XmlTextReader(tr)
                Try
                    Dim root As ServiceDescription = ServiceDescription.Read(xtr, True)
                    mServiceDescriptions.Add(root)
                Catch ex As Exception
                    Dim sErr As String = ex.InnerException.Message
                End Try
            End Using
        End Using
    End Sub

    Private Sub LoadSchemas(ByVal serviceDetails As clsWebServiceDetails)
        For Each sSchema As String In serviceDetails.Schemas
            Using tr As New IO.StringReader(sSchema)
                Using xtr As New XmlTextReader(tr)
                    Dim x As XmlSchema = XmlSchema.Read(xtr, Nothing)
                    mSchemas.Add(x)
                End Using
            End Using
        Next
    End Sub

    Private Sub CompileSchemas()
        For Each serviceDescription As ServiceDescription In mServiceDescriptions
            For Each schema As XmlSchema In serviceDescription.Types.Schemas
                If Not mSchemas.Contains(schema) Then
                    mSchemas.Add(schema)
                End If
            Next
        Next
        Try
            mSchemas.Compile(AddressOf CompileValidationEventHandler, True)
        Catch 'Messages will be reported using the event handler so ignore the exception.
        End Try
    End Sub

    Private Sub CompileValidationEventHandler(ByVal sender As Object, ByVal e As ValidationEventArgs)
        RaiseEvent Diagnostic(e.Message)
    End Sub


    ''' <summary>
    ''' Get information about all the web services described by the url used in
    ''' Import().
    ''' </summary>
    ''' <returns>A Dictionary of clsWebService instances, keyed on the name of the
    ''' service.</returns>
    Private Function Investigate() As Dictionary(Of String, clsWebService)

        CompileSchemas()
        Dim services As New Dictionary(Of String, clsWebService)(StringComparer.InvariantCultureIgnoreCase)
        For Each sd As ServiceDescription In mServiceDescriptions
            For Each sv As Service In sd.Services
                Dim ws As clsWebService = Investigate(sv)
                services.Add(ws.InternalName, ws)
            Next
        Next

        Return services

    End Function


    ''' <summary>
    ''' Used internally to investigate the parts of the web service.
    ''' </summary>
    ''' <param name="sv">The service to investigate</param>
    ''' <returns>a clsWebService containing the collection info about the service</returns>
    Private Function Investigate(ByVal sv As Service) As clsWebService

        'Get the internal servicename
        Dim webService As New clsWebService()
        webService.InternalName = sv.Name
        webService.Narrative = sv.Documentation

        RaiseEvent Diagnostic(String.Format(My.Resources.Resources.clsWSDLProcess_FoundService0, sv.Name))

        Dim bSoapBindingFound As Boolean
        For Each p As Port In sv.Ports

            RaiseEvent Diagnostic(String.Format(My.Resources.Resources.clsWSDLProcess_FoundServicePort0, p.Name))

            'Get the binding
            Dim b As Binding = mServiceDescriptions.GetBinding(p.Binding)

            'Check to see if we have a soap binding
            Dim sb As SoapBinding = CType(b.Extensions.Find(GetType(SoapBinding)), SoapBinding)
            If TypeOf sb Is Soap12Binding Then

                'Need to support soap12 bindings

            ElseIf TypeOf sb Is SoapBinding Then
                bSoapBindingFound = True
                Dim sab As SoapAddressBinding = CType(p.Extensions.Find(GetType(SoapAddressBinding)), SoapAddressBinding)
                webService.ServiceLocation = sab.Location

                'Check to see if the soap transport is http
                If sb.Transport = SoapBinding.HttpTransport Then
                    For Each opb As OperationBinding In b.Operations
                        InvestigateOperations(b, sb, opb, webService)
                    Next
                End If
            End If
        Next

        If Not bSoapBindingFound Then
            Throw New NotImplementedException(My.Resources.Resources.clsWSDLProcess_OnlySoapBindingsAreSupportedAtThisTimeThisWebServiceDoesNotHaveAnySoapBindings)
        End If

        Return webService
    End Function


    ''' <summary>
    ''' Investigates the operations of the web service
    ''' </summary>
    ''' <param name="b">The binding to use to investigate the operation</param>
    ''' <param name="sb">The SOAP binding to use to investigate the operation</param>
    ''' <param name="opb">The operation binding used to investigate the operation.
    ''' </param>
    ''' <param name="webService">The clsWebService object to write the information
    ''' to.</param>
    Private Sub InvestigateOperations(ByVal b As Binding, ByVal sb As SoapBinding,
                                      ByVal opb As OperationBinding,
                                      ByVal webService As clsWebService)

        Dim pt As PortType = mServiceDescriptions.GetPortType(b.Type)

        For Each op As Operation In pt.Operations
            If op.IsBoundBy(opb) Then
                Dim sob As SoapOperationBinding =
                    CType(opb.Extensions.Find(GetType(SoapOperationBinding)),
                        SoapOperationBinding)

                mobjAction = New clsWebServiceAction()
                mobjAction.SetName(op.Name)

                RaiseEvent Diagnostic(String.Format(My.Resources.Resources.clsWSDLProcess_FoundOperation0,
                                                    op.Name))

                Dim objSoapProtocol As New clsSoap(clsSoap.SoapVersion.Soap11)
                objSoapProtocol.SoapAction = sob.SoapAction
                AddHandler objSoapProtocol.Diags, AddressOf webService.ProtocolDiags

                'Determine the binding style (document/rpc)...
                If sob.Style <> SoapBindingStyle.Default Then
                    'Explicity specified for this operation...
                    objSoapProtocol.BindingStyle = sob.Style
                Else
                    If sb.Style <> SoapBindingStyle.Default Then
                        'Not specified, but we can use the one from the binding...
                        '(as per http://www.w3.org/TR/wsdl#_soap:binding, and see
                        'bug #6137)
                        objSoapProtocol.BindingStyle = sb.Style
                    Else
                        'Assume document style in the absence of being told. (again,
                        'see above link to spec)
                        objSoapProtocol.BindingStyle = SoapBindingStyle.Document
                    End If
                End If

                If mServiceDetails.Username IsNot Nothing AndAlso
                     mServiceDetails.Secret IsNot Nothing Then
                    objSoapProtocol.Credentials =
                        New NetworkCredential(mServiceDetails.Username,
                                              mServiceDetails.Secret)
                End If

                objSoapProtocol.Certificate = mServiceDetails.Certificate
                objSoapProtocol.Timeout = mServiceDetails.Timeout
                mobjAction.Protocol = objSoapProtocol

                mobjAction.SetNarrative(op.Documentation)

                If mServiceDetails.Loaded AndAlso
                    mServiceDetails.ActionEnabled(op.Name) Then
                    webService.AddAction(mobjAction)
                ElseIf Not mServiceDetails.Loaded Then
                    webService.AddAction(mobjAction)
                End If


                Dim m As Message
                Select Case op.Messages.Flow
                    Case OperationFlow.RequestResponse

                        'Input Message
                        m = mServiceDescriptions.GetMessage(op.Messages.Input.Message)
                        InvestigateMessage(m, opb, ParamDirection.In)

                        'Output message
                        m = mServiceDescriptions.GetMessage(op.Messages.Output.Message)
                        InvestigateMessage(m, opb, ParamDirection.Out)

                        'Fault messages
                        For Each f As OperationFault In op.Faults
                            m = mServiceDescriptions.GetMessage(f.Message)
                        Next

                End Select
            End If
        Next
    End Sub


    ''' <summary>
    ''' Investigates a message within the operation
    ''' </summary>
    ''' <param name="m"></param>
    ''' <param name="opb"></param>
    ''' <param name="dir"></param>
    Private Sub InvestigateMessage(ByVal m As Message, ByVal opb As OperationBinding, ByVal dir As ParamDirection)

        RaiseEvent Diagnostic(String.Format(My.Resources.Resources.clsWSDLProcess_FoundMessage0, m.Name))

        For Each prt As MessagePart In m.Parts
            mVisitedTypes.Clear()

            Dim sbb As SoapBodyBinding = CType(opb.Input.Extensions.Find(GetType(SoapBodyBinding)), SoapBodyBinding)

            mCurrentDir = dir
            mCurrentUse = sbb.Use
            mCurrentEncoding = sbb.Encoding

            mCurrentNarrative = prt.Documentation

            ' Set a default namespace for the message
            If dir = ParamDirection.Out Then
                mobjAction.OutputMessageNamespace = sbb.Namespace
            Else
                mobjAction.InputMessageNamespace = sbb.Namespace
            End If


            If Not prt.Type.IsEmpty Then

                'RPC style....

                If sbb.Use = SoapBindingUse.Encoded Or sbb.Use = SoapBindingUse.Default Then
                    Dim xName As New XmlQualifiedName(prt.Name, sbb.Namespace)
                    CreateParameter(prt.Type, xName)
                Else
                    'odd behaviour here we pass GetType(xmlComplexType) but expect XmlComplexType or XmlSimpleType back
                    Dim ty As XmlSchemaType = CType(mSchemas.Find(prt.Type, GetType(XmlSchemaComplexType)), XmlSchemaType)
                    If Not ty Is Nothing Then
                        CollapseParameter(ty)

                        'Get the correct namespace for the message direction.
                        If dir = ParamDirection.Out Then
                            mobjAction.InputMessageNamespace = ty.QualifiedName.Namespace
                        Else
                            mobjAction.OutputMessageNamespace = ty.QualifiedName.Namespace
                        End If
                    End If

                End If
            End If

            If Not prt.Element.IsEmpty Then

                'Document style...

                If sbb.Use = SoapBindingUse.Encoded Or sbb.Use = SoapBindingUse.Default Then
                    Throw New InvalidOperationException(My.Resources.Resources.clsWSDLProcess_DocumentEncodedIsNotSupported)
                Else
                    Dim el As XmlSchemaElement = CType(mSchemas.Find(prt.Element, GetType(XmlSchemaElement)), XmlSchemaElement)
                    If Not el Is Nothing Then

                        CollapseParameter(el.ElementSchemaType)

                        ' Depending on the direction set the matching name and namespace for the message.
                        Select Case dir
                            Case ParamDirection.In
                                mobjAction.InputMessage = el.Name
                                mobjAction.InputMessageNamespace = el.QualifiedName.Namespace
                            Case ParamDirection.Out
                                mobjAction.OutputMessage = el.Name
                                mobjAction.OutputMessageNamespace = el.QualifiedName.Namespace
                        End Select
                    End If
                End If
            End If
        Next
    End Sub

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Function to register a type. if the type is registered more than once we have a 
    ''' recursive definition.
    ''' </summary>
    ''' <param name="xsdType">The type to register</param>
    ''' -----------------------------------------------------------------------------
    Private Sub RegisterType(ByVal xsdType As XmlSchemaObject)
        If mVisitedTypes.Contains(xsdType) Then
            Throw New InvalidOperationException("ERROR: Blue Prism does not currently support recursive complex data types.")
        End If

        mVisitedTypes.Add(xsdType)
    End Sub

    Private mVisitedTypes As New List(Of XmlSchemaObject)


    ''' <summary>
    ''' Simple class to allow passing of context down the chain of recursivley called
    ''' functions.
    ''' </summary>
    Private Class Context
        Public Delegate Sub CreateDelegate(ByVal el As XmlSchemaElement, ByVal c As Context)
        Public CreateOperation As CreateDelegate
        Public CollectionInfo As clsCollectionInfo
    End Class

    ''' <summary>
    ''' Collapses a parameter of the web service into an automate parameter
    ''' </summary>
    ''' <param name="xsdType">The xsd schema type to be collapsed</param>
    Private Sub CollapseParameter(ByVal xsdType As XmlSchemaType)
        Dim ctx As New Context
        mVisitedTypes.Clear()

        ctx.CreateOperation = New Context.CreateDelegate(AddressOf CreateParameter)
        CollapseType(xsdType, ctx)
    End Sub


    ''' <summary>
    ''' Used to collapse complex document types into parameters
    ''' </summary>
    ''' <param name="xsdType">The xsd schema type to be collapsed</param>
    ''' <param name="ctx">The context in which to save the result</param>
    Private Sub CollapseType(ByVal xsdType As XmlSchemaType, ByVal ctx As Context)
        If TypeOf xsdType Is XmlSchemaComplexType Then
            CollapseType(CType(xsdType, XmlSchemaComplexType), ctx)
        End If
    End Sub


    ''' <summary>
    ''' Used to collapse complex document types into parameters
    ''' </summary>
    ''' <param name="xsdType">The xsd schema type to be collapsed</param>
    ''' <param name="ctx">The context in which to save the result</param>
    Private Sub CollapseType(ByVal xsdType As XmlSchemaComplexType, ByVal ctx As Context)
        RegisterType(xsdType)

        If xsdType.Particle IsNot Nothing Then
            CollapseType(xsdType.Particle, ctx)
        ElseIf xsdType.ContentTypeParticle IsNot Nothing Then
            CollapseType(xsdType.ContentTypeParticle, ctx)
        End If

        mVisitedTypes.Remove(xsdType)
    End Sub



    ''' <summary>
    ''' Used to collapse complex document types into parameters
    ''' </summary>
    ''' <param name="xsdType">The xsd schema type to be collapsed</param>
    ''' <param name="ctx">The context in which to save the result</param>
    Private Sub CollapseType(ByVal xsdType As XmlSchemaParticle, ByVal ctx As Context)
        If TypeOf xsdType Is XmlSchemaSequence Then
            CollapseType(CType(xsdType, XmlSchemaSequence), ctx)
        ElseIf TypeOf xsdType Is XmlSchemaAll Then
            CollapseType(CType(xsdType, XmlSchemaAll), ctx)
        End If
    End Sub


    ''' <summary>
    ''' Used to collapse complex document types into parameters
    ''' </summary>
    ''' <param name="xsdType">The xsd schema type to be collapsed</param>
    ''' <param name="ctx">The context in which to save the result</param>
    Private Sub CollapseType(ByVal xsdType As XmlSchemaSequence, ByVal ctx As Context)
        For Each el As XmlSchemaObject In xsdType.Items
            Dim ss As XmlSchemaSequence = TryCast(el, XmlSchemaSequence)
            If ss IsNot Nothing Then
                CollapseType(ss, ctx)
                Continue For
            End If
            Dim se As XmlSchemaElement = TryCast(el, XmlSchemaElement)
            If se IsNot Nothing Then
                Dim ct As XmlSchemaComplexType = TryCast(se.ElementSchemaType, XmlSchemaComplexType)
                If ct IsNot Nothing AndAlso se.MaxOccurs > 1 AndAlso xsdType.Items.Count = 1 Then
                    Dim wci As clsCollectionInfo = ctx.CollectionInfo
                    If wci IsNot Nothing Then
                        wci.NestingElement = se.Name
                    End If
                    CollapseType(ct, ctx)
                Else
                    ctx.CreateOperation.Invoke(se, ctx)
                End If
            End If
        Next
    End Sub


    ''' <summary>
    ''' Used to collapse complex document types into parameters
    ''' </summary>
    ''' <param name="xsdType">The xsd schema type to be collapsed</param>
    ''' <param name="ctx">The context in which to save the result</param>
    Private Sub CollapseType(ByVal xsdType As XmlSchemaAll, ByVal ctx As Context)
        For Each el As XmlSchemaElement In xsdType.Items
            ctx.CreateOperation.Invoke(el, ctx)
        Next
    End Sub

    ''' <summary>
    ''' Creates a Action parameter for the given xsd type and name pair.
    ''' </summary>
    ''' <param name="xsdType">The qualified Type name of the schema element being investigated</param>
    ''' <param name="xsdName">The qualified Name of the schema element being investigated</param>
    Private Sub CreateParameter(ByVal xsdType As XmlQualifiedName, ByVal xsdName As XmlQualifiedName)
        Dim soapParameter As New clsSoapParameter()
        Dim name As String = xsdName.Name
        soapParameter.Name = name
        soapParameter.XSDType = xsdType.Name
        soapParameter.Direction = mCurrentDir
        soapParameter.SoapUse = mCurrentUse
        soapParameter.EncodingStyle = mCurrentEncoding
        soapParameter.Narrative = mCurrentNarrative
        soapParameter.NamespaceURI = xsdName.Namespace
        Dim tt As DataType = XSDToAutomateType(xsdType.Name)
        soapParameter.SetDataType(tt)

        RaiseEvent Diagnostic(String.Format(My.Resources.Resources.clsWSDLProcess_FoundPart0, name))

        'If it's a collection (from an array) it needs a field...
        If tt = DataType.collection Then
            soapParameter.CollectionInfo = New clsCollectionInfo()
            If Not xsdType.Name.EndsWith("[]") Then
                Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsWSDLProcess_DonTKnowHowToGetTheTypeForTheFieldOf0, xsdName.Name))
            End If
            tt = XSDToAutomateType(xsdType.Name.Substring(0, xsdType.Name.Length - 2))
            If tt = DataType.unknown Then
                Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsWSDLProcess_UnrecognisedFieldTypeFor0, xsdName.Name))
            End If
            soapParameter.CollectionInfo.AddField(xsdName.Name, tt, xsdName.Namespace)
        End If

        mobjAction.AddParameter(soapParameter)
    End Sub

    ''' <summary>
    ''' Creates a parameter based on the types defined in the wsdl schema
    ''' </summary>
    ''' <param name="xElement">The element of the schema currently being investigated.</param>
    ''' <param name="ctx">The context</param>
    Private Sub CreateParameter(ByVal xElement As XmlSchemaElement, ByVal ctx As Context)

        Dim flat As Boolean

        Dim soapParameter As New clsSoapParameter()
        Dim name As String = xElement.QualifiedName.Name
        soapParameter.Name = name
        soapParameter.XSDType = xElement.SchemaTypeName.Name
        soapParameter.Direction = mCurrentDir
        soapParameter.SoapUse = mCurrentUse
        soapParameter.EncodingStyle = mCurrentEncoding
        soapParameter.Narrative = mCurrentNarrative
        soapParameter.NamespaceURI = xElement.QualifiedName.Namespace

        RaiseEvent Diagnostic(String.Format(My.Resources.Resources.clsWSDLProcess_FoundPart0, name))

        Dim dtDataType As DataType = GetComplexAutomateType(xElement, True, flat)
        If dtDataType = DataType.collection Then
            'Create a new context for collecting the fields within the collection...
            Dim nctx As New Context()
            nctx.CreateOperation = New Context.CreateDelegate(AddressOf CreateCollectionField)
            nctx.CollectionInfo = New clsCollectionInfo()
            'TODO: It's never a single row collection currently. This
            'corresponds to it being a compound structure. We also need
            'to detect arrays - in that case, it would not be a single
            'row collection, and it should only have one field.
            nctx.CollectionInfo.SingleRow = False
            nctx.CollectionInfo.Flat = flat
            CollapseType(xElement.ElementSchemaType, nctx)
            soapParameter.CollectionInfo = nctx.CollectionInfo
            'If it's a collection and it has no fields, we should be able to
            'find a base type and make that the field type for a single field
            'we create...
            If soapParameter.CollectionInfo.Count = 0 Then
                Dim tt As DataType = GetComplexAutomateType(xElement, False, flat)
                'Last resort...
                If tt = DataType.unknown Then tt = DataType.text
                soapParameter.CollectionInfo.AddField(xElement.Name, tt, xElement.QualifiedName.Namespace)
            End If
        End If
        soapParameter.SetDataType(dtDataType)

        mobjAction.AddParameter(soapParameter)

    End Sub

    ''' <summary>
    ''' Gets the complex datatype from the given xmlschema type name
    ''' </summary>
    ''' <param name="xElement">The element of the schema currently being investigated.</param>
    ''' <param name="allowCollections">True if collections are allowed.</param>
    ''' <param name="flat">On return, contains True if the resulting type is a
    ''' 'flattened' collection. This is one that is defined as a simple data type,
    ''' but has maxOccurs > 1 - i.e. the element just repeats as necessary, with
    ''' no container.</param>
    ''' <returns>The automate data type</returns>
    Private Function GetComplexAutomateType(ByVal xElement As XmlSchemaElement, ByVal allowCollections As Boolean, ByRef flat As Boolean) As DataType
        Dim dt As DataType
        flat = False

        If allowCollections AndAlso xElement.MaxOccurs > 1 Then
            dt = DataType.collection
            flat = True
        End If

        If dt = DataType.unknown Then
            Select Case xElement.SchemaTypeName.Namespace
                Case xsd.NamespaceURI
                    dt = XSDToAutomateType(xElement.SchemaTypeName.Name)
                    'case wsdl.Namespace
                    'case Soapenc.NamespaceURI
                Case Else
                    Dim xBaseType As XmlSchemaType = xElement.ElementSchemaType.BaseXmlSchemaType
                    If Not xBaseType Is Nothing Then
                        Dim xBaseTypeName As XmlQualifiedName = xBaseType.QualifiedName
                        If xBaseTypeName.Namespace = xsd.NamespaceURI Then
                            dt = XSDToAutomateType(xBaseTypeName.Name)
                        End If
                    End If

                    ' These case weren't being picked up, so added specific if blocks to look for the correct data type.
                    If dt = DataType.unknown AndAlso
                        TypeOf (xElement.ElementSchemaType) Is XmlSchemaSimpleType AndAlso
                        xElement.ElementSchemaType.TypeCode = XmlTypeCode.String Then
                        dt = DataType.text
                    ElseIf allowCollections AndAlso dt = DataType.unknown AndAlso
                            TypeOf (xElement.ElementSchemaType) Is XmlSchemaComplexType Then
                        dt = DataType.collection
                    Else
                        If allowCollections AndAlso dt = DataType.unknown Then
                            For Each sch As Schema.XmlSchema In mSchemas
                                If Not String.IsNullOrEmpty(sch.TargetNamespace) AndAlso sch.TargetNamespace = xElement.SchemaTypeName.Namespace Then
                                    dt = DataType.collection
                                    Exit Select
                                End If
                            Next
                            dt = DataType.unknown
                        End If
                    End If
            End Select
        End If

        Return dt
    End Function

    ''' <summary>
    ''' Creates collection info based on the soap parameter. This function is called recursively
    ''' </summary>
    ''' <param name="xElement">The element of the schema currently being investigated.</param>
    ''' <param name="parent">The parent context</param>
    Private Sub CreateCollectionField(ByVal xElement As XmlSchemaElement, ByVal parent As Context)
        Dim flat As Boolean
        Dim dtDataType As DataType = GetComplexAutomateType(xElement, True, flat)
        parent.CollectionInfo.AddField(xElement.QualifiedName.Name, dtDataType,
                                       xElement.QualifiedName.Namespace)
        Dim field As clsCollectionFieldInfo =
            parent.CollectionInfo.GetField(xElement.QualifiedName.Name)
        If dtDataType = DataType.collection Then
            Dim child As New Context()
            child.CreateOperation = parent.CreateOperation
            child.CollectionInfo = New clsCollectionInfo()

            'TODO: It's never a single row collection currently. This
            'corresponds to it being a compound structure. We also need
            'to detect arrays - in that case, it would not be a single
            'row collection, and it should only have one field.
            child.CollectionInfo.SingleRow = False
            child.CollectionInfo.Flat = flat
            CollapseType(xElement.ElementSchemaType, child)
            field.CopyChildren(child.CollectionInfo)
            field.Children.NestingElement = child.CollectionInfo.NestingElement
            'If it's a collection and it has no fields, we should be able to
            'find a base type and make that the field type for a single field
            'we create...
            If Not field.HasChildren() Then
                Dim tt As DataType = GetComplexAutomateType(xElement, False, flat)
                'Last resort...
                If tt = DataType.unknown Then tt = DataType.text
                field.Children.AddField(xElement.Name, tt, xElement.QualifiedName.Namespace)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Converts an xsd datatype into an automate datatype
    ''' </summary>
    ''' <param name="sType">An XSD data type identifier</param>
    ''' <returns>The corresponding Automate DataType.</returns>
    Public Shared Function XSDToAutomateType(ByVal sType As String) As DataType
        Select Case sType
            Case "float", "double", "pDecimal", "decimal",
             "integer",
             "nonPositiveInteger", "nonNegativeInteger", "long",
             "negativeInteger", "unsignedLong", "int",
             "positiveInteger", "unsignedInt", "short",
             "unsignedShort", "byte",
             "unsignedByte"
                Return DataType.number
            Case "dateTime"
                Return DataType.datetime
            Case "date"
                Return DataType.date
            Case "time"
                Return DataType.time
            Case "duration"
                Return DataType.timespan
            Case "string", "normalizedString"
                Return DataType.text
            Case "boolean"
                Return DataType.flag
            Case "base64Binary"
                Return DataType.binary
            Case Else
                If sType.IndexOf("[]") <> -1 Then
                    Return DataType.collection
                End If
                Return DataType.unknown
        End Select
    End Function


    ''' <summary>
    ''' Converts an Automate value into an XSD value
    ''' </summary>
    ''' <param name="value">The clsProcessValue to convert</param>
    ''' <returns>The XSD representation of the value.</returns>
    Public Shared Function AutomateValueToXSD(ByVal value As clsProcessValue) As String
        Dim xRequiredType As XmlQualifiedName = AutomateTypeToXSD(value.DataType)
        If xRequiredType Is Nothing Then
            Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsWSDLProcess_DonTKnowHowToTranslate0ToXSD, clsDataTypeInfo.GetLocalizedFriendlyName(value.DataType)))
        End If
        Dim sRequiredType As String = xRequiredType.Name
        Return AutomateValueToXSD(value, sRequiredType)
    End Function


    ''' <summary>
    ''' Converts an Automate value into an XSD value
    ''' </summary>
    ''' <param name="value">The clsProcessValue to convert</param>
    ''' <param name="sRequiredType">The required XSD data type</param>
    ''' <returns>The XSD representation of the value.</returns>
    Public Shared Function AutomateValueToXSD(ByVal value As clsProcessValue, ByVal sRequiredType As String) As String
        Select Case sRequiredType
            Case "float", "double", "pDecimal", "decimal",
             "integer",
             "nonPositiveInteger", "nonNegativeInteger", "long",
             "negativeInteger", "unsignedLong", "int",
             "positiveInteger", "unsignedInt", "short",
             "unsignedShort", "byte",
             "unsignedByte"
                If value.DataType = DataType.number Then
                    Return XmlConvert.ToString(CDec(value))
                Else
                    Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsWSDLProcess_CannotConvertANumberIntoAXsd0, sRequiredType))
                End If
            Case "date"
                Select Case value.DataType
                    Case DataType.date
                        Return value.FormatDate("yyyy-MM-dd")
                    Case Else
                        Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsWSDLProcess_CannotConvertA0IntoAXsd1, clsDataTypeInfo.GetLocalizedFriendlyName(value.DataType), sRequiredType))
                End Select
            Case "dateTime"
                Select Case value.DataType
                    Case DataType.datetime
                        Dim sDate As String = value.FormatDate("yyyy-MM-ddTHH:mm:ss.fffffffZ")
                        Return sDate
                    Case Else
                        Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsWSDLProcess_CannotConvertA0IntoAXsd1, clsDataTypeInfo.GetLocalizedFriendlyName(value.DataType), sRequiredType))
                End Select
            Case "time"
                Select Case value.DataType
                    Case DataType.time
                        Return value.FormatDate("HH:mm:ss")
                    Case Else
                        Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsWSDLProcess_CannotConvertA0IntoAXsd1, clsDataTypeInfo.GetLocalizedFriendlyName(value.DataType), sRequiredType))
                End Select
            Case "duration"
                Select Case value.DataType
                    Case DataType.timespan
                        Return SoapDuration.ToString(CType(value, TimeSpan))
                    Case Else
                        Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsWSDLProcess_CannotConvertA0IntoAXsd1, clsDataTypeInfo.GetLocalizedFriendlyName(value.DataType), sRequiredType))
                End Select
            Case "string", "normalizedString"
                If value.DataType = DataType.text Or value.DataType = DataType.password Then
                    Return CStr(value)
                Else
                    Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsWSDLProcess_CannotConvertA0IntoAXsd1, clsDataTypeInfo.GetLocalizedFriendlyName(value.DataType), sRequiredType))
                End If
            Case "boolean"
                If value.DataType = DataType.flag Then
                    Return XmlConvert.ToString(CBool(value))
                Else
                    Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsWSDLProcess_CannotConvertA0IntoAXsd1, clsDataTypeInfo.GetLocalizedFriendlyName(value.DataType), sRequiredType))
                End If
            Case "base64Binary"
                If value.DataType = DataType.binary Then
                    ' Make sure we don't try and convert a null array -
                    ' Convert.ToBase64String() doesn't like that
                    Dim arr() As Byte = CType(value, Byte())
                    If arr Is Nothing Then arr = New Byte() {}
                    Return Convert.ToBase64String(arr)
                Else
                    Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsWSDLProcess_CannotConvertA0IntoAXsd1, clsDataTypeInfo.GetLocalizedFriendlyName(value.DataType), sRequiredType))
                End If
        End Select

        Return Nothing
    End Function

    ''' <summary>
    ''' Populate a collection using element data from a SOAP message.
    ''' </summary>
    ''' <param name="xml">The element to populate from</param>
    ''' <param name="def">The definition of the target collection.</param>
    Friend Shared Function CreateCollectionFromXML(ByVal xml As XmlNode, ByVal def As clsCollectionInfo) As clsCollection

        Dim col As New clsCollection()
        col.CopyDefinition(def)
        If xml.ChildNodes.Count = 0 Then Return col

        Dim wci As clsCollectionInfo = def
        If wci Is Nothing OrElse wci.NestingElement Is Nothing Then
            AppendCollectionField(xml, col, def)
        Else
            For Each nRow As XmlNode In xml.ChildNodes
                Dim eRow As XmlElement = TryCast(nRow, XmlElement)
                If eRow IsNot Nothing AndAlso eRow.LocalName = wci.NestingElement Then
                    AppendCollectionField(eRow, col, def)
                End If
            Next
        End If
        Return col
    End Function

    ''' <summary>
    ''' Populate a field in a collection using element data from a SOAP message.
    ''' </summary>
    ''' <param name="xml">The element to populate from</param>
    ''' <param name="def">The definition of the target collection.</param>
    Private Shared Sub AppendCollectionField(ByVal xml As XmlNode, ByVal col As clsCollection, ByVal def As clsCollectionInfo)
        Dim fval As clsProcessValue
        'We can only ever get a single row here. Multiple rows in collections
        'can come by being added to a row at a time by repeated elements,
        'which is dealt with in ReadMessage (and also 'recursively' within
        'this loop!).
        Dim row As New clsCollectionRow()

        For i As Integer = 0 To xml.ChildNodes.Count - 1
            Dim nField As XmlNode = xml.ChildNodes(i)

            If TypeOf nField Is XmlText Then
                'This is a special case for arrays in document/literal...
                Dim arrayInfo As clsCollectionFieldInfo = def(0)
                If i > 0 Then
                    col.Add(row)
                    row = New clsCollectionRow()
                End If
                fval = clsWSDLProcess.XSDToAutomateValue(arrayInfo.DataType, nField, Nothing)
                row.Add(arrayInfo.Name, fval)

            Else
                Dim eField As XmlElement = TryCast(nField, XmlElement)
                If eField IsNot Nothing Then
                    For Each f As clsCollectionFieldInfo In def
                        If eField.LocalName = clsProcess.GetSafeName(f.Name) Then
                            fval = clsWSDLProcess.XSDToAutomateValue(f.DataType, eField, f.Children)
                            'Add it to the row...
                            If row.ContainsKey(eField.LocalName) Then
                                'But it might be a repeat, if it's an array. This is
                                'analagous to the similar shenanigans in ReadMessage!
                                Dim tval As clsProcessValue = row(eField.LocalName)
                                If tval.DataType <> DataType.collection OrElse fval.DataType <> DataType.collection Then
                                    col.Add(row)
                                    row = New clsCollectionRow()
                                    row.Add(f.Name, fval)
                                Else
                                    For Each row2 As clsCollectionRow In fval.Collection.Rows
                                        tval.Collection.Add(row2)
                                    Next
                                End If
                            Else
                                row.Add(f.Name, fval)
                            End If
                        End If
                    Next

                End If
            End If
        Next
        col.Add(row)
    End Sub

    ''' <summary>
    ''' Converts an XSD value into an Automate value
    ''' </summary>
    ''' <param name="dtDataType">The expected data type for the value</param>
    ''' <param name="xml">The XSD representation of the value to be converted.</param>
    ''' <param name="def">The definition of the collection the value should conform to.</param>
    ''' <returns>A clsProcessValue containing the converted value.</returns>
    Public Shared Function XSDToAutomateValue(ByVal dtDataType As DataType, ByVal xml As XmlNode, ByVal def As clsCollectionInfo) As clsProcessValue
        Select Case dtDataType
            Case DataType.collection
                Dim col As clsCollection = CreateCollectionFromXML(xml, def)
                Return New clsProcessValue(col)
            Case DataType.date
                'Note - our format is the same as XSD, but XSD can optionally have
                'timezone information on the date which we don't want...
                Return New clsProcessValue(dtDataType, xml.InnerText.Substring(0, 10).Replace("-", "/"))
            Case DataType.datetime
                Return New clsProcessValue(dtDataType, Date.Parse(xml.InnerText).ToUniversalTime().ToString("u"))
            Case DataType.time
                'Note - we just want the HH:MM:SS part, not any of the optional guff
                'that may follow...
                Return New clsProcessValue(dtDataType, xml.InnerText.Substring(0, 8))
            Case DataType.timespan
                Dim ts As TimeSpan = SoapDuration.Parse(xml.InnerText)
                Return New clsProcessValue(dtDataType, ts.ToString())
            Case DataType.flag
                Dim sText As String = xml.InnerText
                Select Case sText.ToLower
                    Case "true", "1"
                        sText = "True"
                    Case "false", "0"
                        sText = "False"
                End Select
                Return New clsProcessValue(dtDataType, sText)
            Case DataType.password
                Return New clsProcessValue(New SafeString(xml.InnerText))
            Case Else
                Return New clsProcessValue(dtDataType, xml.InnerText)
        End Select
    End Function


    ''' <summary>
    ''' Converts an Automate Datatype into an XSD datatype
    ''' </summary>
    ''' <param name="dtDataType">The Automate Datatype to convert</param>
    ''' <returns>An XmlQualifiedName containing the XSD datatype corresponding to
    ''' the given Automate DataType.</returns>
    Public Shared Function AutomateTypeToXSD(ByVal dtDataType As DataType) As XmlQualifiedName
        Select Case dtDataType
            Case DataType.date
                Return New XmlQualifiedName("date", xsd.NamespaceURI)
            Case DataType.datetime
                Return New XmlQualifiedName("dateTime", xsd.NamespaceURI)
            Case DataType.flag
                Return New XmlQualifiedName("boolean", xsd.NamespaceURI)
            Case DataType.number
                Return New XmlQualifiedName("decimal", xsd.NamespaceURI)
            Case DataType.password, DataType.text
                Return New XmlQualifiedName("string", xsd.NamespaceURI)
            Case DataType.time
                Return New XmlQualifiedName("time", xsd.NamespaceURI)
            Case DataType.timespan
                Return New XmlQualifiedName("duration", xsd.NamespaceURI)
            Case DataType.binary, DataType.image
                Return New XmlQualifiedName("base64Binary", xsd.NamespaceURI)
            Case Else
                Return Nothing
        End Select
    End Function

End Class