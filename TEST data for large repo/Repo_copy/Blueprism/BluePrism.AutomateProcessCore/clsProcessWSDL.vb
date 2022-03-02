Imports System.IO
Imports System.Xml
Imports System.Xml.Schema
Imports System.Text
Imports System.Web.Services.Description
Imports BluePrism.AutomateProcessCore.Processes
Imports System.Globalization
Imports BluePrism.Core.Xml

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsProcessWSDL
''' 
''' <summary>
''' This class handles the generation of a WSDL description to fit the
''' signature of a particular process.
''' </summary>
Public Class clsProcessWSDL

    ''' <summary>
    ''' Get the data type for the given parameter. This may be a standard XSD
    ''' data type, or a complex type may be created specifically for it and
    ''' inserted into the schema.
    ''' </summary>
    ''' <param name="opname">The operation name.</param>
    ''' <param name="param">The parameter in question.</param>
    ''' <param name="paramstage">The stage the parameters are coming from. This
    ''' is needed to handle collection definitions, where the parameter is
    ''' defined in the stage but the field definition is derived from the linked
    ''' data item.</param>
    ''' <param name="sd">The ServiceDescription the Message is destined to be
    ''' added to. Schema items may be defined and stored in Schemas(0) within
    ''' this ServiceDescription.</param>
    ''' <returns>The XmlQualifiedName of the data type.</returns>
    Private Shared Function GetDataType(ByVal opname As String, ByVal param As clsProcessParameter, ByVal paramstage As clsProcessStage, ByVal sd As ServiceDescription) As XmlQualifiedName

        'Deal with the simple case first...
        If param.GetDataType() <> DataType.collection Then
            Return clsWSDLProcess.AutomateTypeToXSD(param.GetDataType)
        Else
            Return AddCollection(opname & "-" & param.Direction.ToString(), param.Name, clsVBO.GetCollectionDefinition(param, paramstage), sd)
        End If

    End Function

    Private Shared Function AddCollection(prefix As String, name As String, cd As clsCollectionInfo, sd As ServiceDescription) As XmlQualifiedName

        'For a collection, we need to do extra work...

        'And in fact, some extra extra work, to get the collection
        'definition because it's not defined in the actual parameter
        'parameter...
        If cd Is Nothing Then
            Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsProcessWSDL_CannotExposeCollectionParameter0BecauseTheCollectionFieldsAreNotDefined, name))
        End If

        'We need to define a complex type that represents a row of
        'the collection, and add it to the Service Description as
        'a new type.
        Dim rowel As New XmlSchemaComplexType()
        Dim collName As String = prefix & "-" & clsProcess.GetSafeName(name)
        Dim rowname As String = collName & "-Row"
        rowel.Name = rowname
        Dim seq As New XmlSchemaSequence()
        For Each f As clsCollectionFieldInfo In cd.FieldDefinitions
            If f.DataType <> DataType.collection Then
                Dim field As New XmlSchemaElement()
                field.Name = clsProcess.GetSafeName(f.Name)
                field.SchemaTypeName = clsWSDLProcess.AutomateTypeToXSD(f.DataType)
                seq.Items.Add(field)
            Else
                Dim p As New XmlSchemaElement()
                p.Name = clsProcess.GetSafeName(f.Name)
                p.SchemaTypeName = AddCollection(collName, f.Name, f.Children, sd)
                seq.Items.Add(p)
            End If
        Next
        rowel.Particle = seq
        sd.Types.Schemas(0).Items.Add(rowel)

        'And another type for the collection itself, which is just a
        'sequence of the row type we just defined...
        Dim cel As New XmlSchemaComplexType()
        cel.Name = collName
        seq = New XmlSchemaSequence()
        Dim cfield As New XmlSchemaElement()
        cfield.Name = "row"
        cfield.SchemaTypeName = New XmlQualifiedName("tns:" & rowname)
        cfield.MinOccurs = 0
        cfield.MaxOccursString = "unbounded"
        seq.Items.Add(cfield)
        cel.Particle = seq
        sd.Types.Schemas(0).Items.Add(cel)

        Return New XmlQualifiedName("tns:" & cel.Name)

    End Function

    ''' <summary>
    ''' Create a Message description.
    ''' </summary>
    ''' <param name="opname">The operation name.</param>
    ''' <param name="dir">The direction, in (request) or out (response)</param>
    ''' <param name="params">The parameters that will be included, as a List of
    ''' clsProcessParameter instances. This can include both inputs and outputs
    ''' but only the relevant ones (see the dir parameter) will be used.</param>
    ''' <param name="docliteral">True for document/literal (wrapped) style, or
    ''' False for rpc/encoded.</param>
    ''' <param name="paramstage">The stage the parameters are coming from. This
    ''' is needed to handle collection definitions, where the parameter is
    ''' defined in the stage but the field definition is derived from the linked
    ''' data item.</param>
    ''' <param name="sd">The ServiceDescription the Message is destined to be
    ''' added to. Schema items may be defined and stored in Schemas(0) within
    ''' this ServiceDescription.</param>
    ''' <returns>The new Message description.</returns>
    Private Shared Function CreateMessage(ByVal opname As String, ByVal dir As ParamDirection, ByVal params As List(Of clsProcessParameter), ByVal docliteral As Boolean, ByVal paramstage As clsProcessStage, ByVal sd As ServiceDescription) As Message

        Dim msg As New Message()
        msg.Name = opname & CStr(IIf(dir = ParamDirection.In, "Request", "Response"))
        If docliteral Then
            'For document/literal, we define a new ComplexType which will be
            'the only MessagePart, and our parameters go as a Sequence within
            'that type. (following the document/literal wrapped pattern)
            'We need to define a complex type that represents a row of
            'the collection, and add it to the Service Description as
            'a new type.

            Dim wrapelel As New XmlSchemaElement()
            'The element for the request is named the same as the operation, which
            'means it serves as the method name as well, when interpreted the same
            'way as an RPC call.
            Dim wrapname As String = opname & CStr(IIf(dir = ParamDirection.In, "", "Response"))
            wrapelel.Name = wrapname
            Dim wrapel As New XmlSchemaComplexType()
            Dim seq As New XmlSchemaSequence()
            For Each param As clsProcessParameter In params
                If param.Direction = dir Then
                    Dim p As New XmlSchemaElement()
                    p.Name = clsProcess.GetSafeName(param.Name)
                    p.SchemaTypeName = GetDataType(opname, param, paramstage, sd)
                    seq.Items.Add(p)
                End If
            Next
            wrapel.Particle = seq
            wrapelel.SchemaType = wrapel
            sd.Types.Schemas(0).Items.Add(wrapelel)

            Dim mp As New MessagePart()
            mp.Name = msg.Name  'Ok to re-use that name I think
            mp.Element = New XmlQualifiedName(wrapelel.Name, sd.TargetNamespace)
            msg.Parts.Add(mp)

        Else
            'For rpc/encoded, we have a MessagePart for each parameter...
            For Each param As clsProcessParameter In params
                If param.Direction = dir Then
                    Dim mp As New MessagePart()
                    mp.Name = clsProcess.GetSafeName(param.Name)
                    mp.Type = GetDataType(opname, param, paramstage, sd)
                    msg.Parts.Add(mp)
                End If
            Next
        End If
        Return msg

    End Function


    ''' <summary>
    ''' Decide whether to use document/literal or rpc/encoded.
    ''' </summary>
    ''' <param name="startstage">The start stage of the relevant page.</param>
    ''' <param name="endstage">The end stage of the relevant page.</param>
    ''' <returns>True to use document/literal - False to use rpc/encoded.</returns>
    Private Shared Function UseDocLiteral(ByVal startstage As clsProcessStage, ByVal endstage As clsProcessStage) As Boolean

        'Look at all the parameters and decide if we should use rpc/encoded or
        'document/literal for this operation. We use rpc/encoded by default, because
        'our original implementation always used it and we don't want to break
        'compatibility. However, if there are collection outputs, we can't represent
        'them unless we use document/literal, so we switch in that case only (which
        'covers operations that would have previously been unsupported anyway).
        'See bug #6205.
        Dim docliteral As Boolean = False
        Dim allparams As New List(Of clsProcessParameter)
        allparams.AddRange(startstage.GetParameters())
        allparams.AddRange(endstage.GetParameters())
        For Each objParam As clsProcessParameter In allparams
            If objParam.GetDataType = DataType.collection Then
                docliteral = True
            End If
        Next
        Return docliteral

    End Function


    ''' <summary>
    ''' Used internally to add an operation to a ServiceDescription that is being
    ''' constructed.
    ''' </summary>
    ''' <param name="serviceName">The exposed service name</param>
    ''' <param name="sd">The ServiceDescription to add to</param>
    ''' <param name="b">The Binding to add the operation to</param>
    ''' <param name="objProcess">The source process</param>
    ''' <param name="page">The page to be added, or Nothing to add the main
    ''' process.</param>
    ''' <returns>The newly created operation</returns>
    Private Shared Function AddOperation(serviceName As String, sd As ServiceDescription, b As Binding, objProcess As clsProcess, page As clsProcessSubSheet, ByVal enforceDocLiteral As Boolean) As Operation

        'Determine the name of the operation we're adding...
        Dim opname As String
        If page Is Nothing Then
            opname = serviceName
        Else
            opname = page.Name
        End If
        opname = clsProcess.GetSafeName(opname)

        'We need the appropriate start and end stages in order to retrieve the input
        'and output parameters...

        Dim startstage As clsProcessStage
        Dim endstage As clsProcessStage
        If page Is Nothing Then
            startstage = objProcess.GetStage(objProcess.GetStartStage())
            endstage = objProcess.GetStage(objProcess.GetEndStage())
        Else
            startstage = objProcess.GetStageByTypeAndSubSheet(StageTypes.Start, page.ID)
            endstage = objProcess.GetStageByTypeAndSubSheet(StageTypes.End, page.ID)
        End If

        ' If we aren't forcing the document / literal SOAP format then use this function
        Dim docliteral = enforceDocLiteral OrElse UseDocLiteral(startstage, endstage)

        'Create input message
        Dim inparams As New List(Of clsProcessParameter)
        If page IsNot Nothing AndAlso page.Name <> "Initialise" Then
            inparams.Add(New clsProcessParameter("bpInstance", DataType.text, ParamDirection.In))
        End If
        inparams.AddRange(startstage.GetParameters)
        Dim msgin As Message = CreateMessage(opname, ParamDirection.In, inparams, docliteral, startstage, sd)
        sd.Messages.Add(msgin)

        'Create output message
        Dim outparams As New List(Of clsProcessParameter)
        If page IsNot Nothing AndAlso page.Name = "Initialise" Then
            outparams.Add(New clsProcessParameter("bpInstance", DataType.text, ParamDirection.Out))
        End If
        outparams.AddRange(endstage.GetParameters())
        Dim msgout As Message = CreateMessage(opname, ParamDirection.Out, outparams, docliteral, endstage, sd)
        sd.Messages.Add(msgout)

        'Create the operation...
        Dim op As New Operation()
        op.Name = opname
        Dim input As OperationMessage = CType(New OperationInput(), OperationMessage)
        input.Message = New XmlQualifiedName(msgin.Name, sd.TargetNamespace)
        Dim output As OperationMessage = CType(New OperationOutput(), OperationMessage)
        output.Message = New XmlQualifiedName(msgout.Name, sd.TargetNamespace)
        op.Messages.Add(input)
        op.Messages.Add(output)

        'SOAP binding nonsense...
        Dim ob As New OperationBinding()
        ob.Name = opname

        Dim sob As New SoapOperationBinding()
        If docliteral Then
            sob.Style = SoapBindingStyle.Document
        Else
            sob.Style = SoapBindingStyle.Rpc
        End If
        ob.Extensions.Add(sob)

        'Add the input binding...
        Dim sib As New InputBinding()
        Dim sbb As New SoapBodyBinding()
        If docliteral Then
            sbb.Use = SoapBindingUse.Literal
        Else
            sbb.Use = SoapBindingUse.Encoded
        End If
        sbb.Encoding = "http://schemas.xmlsoap.org/soap/encoding/"
        sbb.Namespace = sd.TargetNamespace
        sib.Extensions.Add(sbb)
        ob.Input = sib

        'Add the ouput binding...
        Dim soutb As New OutputBinding()
        soutb.Extensions.Add(sbb)
        ob.Output = soutb

        'Add the OperationBinding to the SOAP binding
        b.Operations.Add(ob)

        Return op

    End Function


    ''' <summary>
    ''' Generate some WSDL
    ''' </summary>
    ''' <param name="serviceName">The exposed service name</param>
    ''' <param name="objprocess">The clsProcess object for the process that the WSDL
    ''' will be based on. For an ordinary process, the WSDL will contain a single
    ''' method (named after the process), while for a Business Object there will be
    ''' one for each published Action.
    ''' </param>
    ''' <param name="baseurl">The base URL where the web service will be
    ''' accessible. e.g. "http://host/ws". The URL where the web service is
    ''' actually accessed will then be http://host/ws/Process_Name.</param>
    ''' <returns>The WSDL as a String.</returns>
    Public Shared Function Generate(serviceName As String, objProcess As clsProcess, baseurl As String, ByVal enforceDocLiteral As Boolean) As String

        Dim procname As String = clsProcess.GetSafeName(serviceName)

        Dim sd As New ServiceDescription()
        sd.TargetNamespace = "urn:blueprism:webservice:" & procname.ToLower(CultureInfo.InvariantCulture)

        'Add a schema, which we will need if we need to define any custom data types.
        'It will always be sd.Types.Schemas(0)
        Dim schema As New XmlSchema()
        schema.AttributeFormDefault = XmlSchemaForm.Qualified
        schema.ElementFormDefault = XmlSchemaForm.Qualified
        schema.TargetNamespace = sd.TargetNamespace
        sd.Types.Schemas.Add(schema)

        sd.Name = procname & "Service"

        'Create and add the PortType...
        Dim pt As New PortType()
        pt.Name = procname & "PortType"
        sd.PortTypes.Add(pt)

        'Create the SOAP binding...
        Dim b As New Binding()
        b.Name = procname & "SoapBinding"
        b.Type = New XmlQualifiedName(procname & "PortType", sd.TargetNamespace)

        Dim sb As New SoapBinding()
        sb.Transport = SoapBinding.HttpTransport
        b.Extensions.Add(sb)
        sd.Bindings.Add(b)

        'Create and add all the operations...
        Dim op As Operation
        If objProcess.ProcessType = DiagramType.Process Then
            op = AddOperation(serviceName, sd, b, objProcess, Nothing, enforceDocLiteral)
            pt.Operations.Add(op)
        Else
            For Each page As clsProcessSubSheet In objProcess.SubSheets
                If page.Published Then
                    op = AddOperation(serviceName, sd, b, objProcess, page, enforceDocLiteral)
                    pt.Operations.Add(op)
                End If
            Next
        End If

        'Add a port
        Dim p As New Port
        p.Name = procname & "Soap"
        Dim soapaddress As New SoapAddressBinding
        soapaddress.Location = baseurl & procname
        p.Extensions.Add(soapaddress)
        p.Binding = New XmlQualifiedName(b.Name, sd.TargetNamespace)

        'Add a Service
        Dim sv As New Service
        sv.Name = procname & "Service"
        sv.Ports.Add(p)
        sd.Services.Add(sv)

        'Set the URL...
        sd.RetrievalUrl = baseurl & "/" & procname

        'Convert the output to some usable WSDL...
        Using ms As New MemoryStream()
            Dim xw As New XmlTextWriter(ms, Encoding.UTF8)
            xw.Formatting = Formatting.Indented
            sd.Write(xw)
            ms.Seek(0, SeekOrigin.Begin)
            Using sr As New StreamReader(ms)
                Return sr.ReadToEnd()
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Translate the outputs from a process into a SOAP message to return to a
    ''' client that called the process via a web service.
    ''' </summary>
    ''' <param name="outputs">The outputs from the process</param>
    ''' <param name="proc">The process that this web service is exposing.</param>
    ''' <param name="methodName">The name of the method that these are the outputs
    ''' from. This is the page or process name, but in 'safe' format.</param>
    ''' <param name="session">Either Nothing, or a session token to be inserted
    ''' into the outputs as a parameter named "bpInstance". This is given to the
    ''' caller as an instance identifier to be used to refer to the object instance
    ''' that has just been created.</param>
    ''' <returns>The SOAP message to be sent to the client, in String format.</returns>
    Public Shared Function OutputsToSOAP(ByVal outputs As clsArgumentList, ByVal proc As clsProcess, ByVal methodName As String, ByVal session As String, ByVal enforceDocLiteral As Boolean, ByVal legacyXmlEncoding As Boolean) As String

        Dim webServiceNamespace As String = "urn:blueprism:webservice:" & clsProcess.GetSafeName(proc.Name).ToLower(CultureInfo.InvariantCulture)
        Dim responseNamespace As String
        Dim outputNamespace As String

        'Decide on document/literal or rpc/encoded. This is analogous to the
        'decision we make in AddOperation() but we have to arrive at the stages
        'used by a different route...
        Dim startstage As clsProcessStage
        Dim endstage As clsProcessStage
        If proc.ProcessType = DiagramType.Object Then
            Dim pageid As Guid = proc.GetSubSheetIDSafeName(methodName)
            If pageid = Guid.Empty And methodName <> clsProcess.InitPageName Then
                Throw New InvalidOperationException(My.Resources.Resources.clsProcessWSDL_CouldnTFindPageMatchingRequestedMethod)
            End If
            startstage = proc.GetStageByTypeAndSubSheet(StageTypes.Start, pageid)
            endstage = proc.GetStageByTypeAndSubSheet(StageTypes.End, pageid)
        Else
            startstage = proc.GetStage(proc.GetStartStage())
            endstage = proc.GetStage(proc.GetEndStage())
        End If

        ' If we aren't forcing the document / literal SOAP format then use the UseDocLiteral function
        Dim docliteral As Boolean = enforceDocLiteral OrElse UseDocLiteral(startstage, endstage)

        If docliteral Then
            ' This is a "document style" operation. There is a predefined schema 
            ' for our response data (complexType definition in the WSDL) and it 
            ' belongs to the web service namespace. 
            responseNamespace = webServiceNamespace
            outputNamespace = webServiceNamespace
        Else
            ' This is an "RPC style" operation. The response data element does 
            ' not have a defined schema - it's an arbitrary XML data structure in 
            ' the global namespace.
            If legacyXmlEncoding Then
                ' Preserve old incorrect behaviour, where response element from 
                ' RPC style operation used the web service namespace
                responseNamespace = webServiceNamespace
            Else
                responseNamespace = ""
            End If
            outputNamespace = ""
        End If


        Dim xBody As XmlElement = Nothing
        Dim xSoap As Xml.XmlDocument = clsSoap.SetupSoapEnvelope(xBody, Nothing)

        Dim wrapname As String = clsProcess.GetSafeName(methodName)
        If docliteral Then wrapname &= "Response"
        Dim xMethod As XmlNode = xSoap.CreateElement(wrapname, responseNamespace)

        For Each output As clsArgument In outputs

            Dim paramName As String
            paramName = clsProcess.GetSafeName(output.Name)
            If output.Value.DataType = DataType.collection Then
                Dim cTypeName As String = methodName & "-" & "Out" & "-" & clsProcess.GetSafeName(paramName)
                xMethod.AppendChild(AddComplexType(paramName, cTypeName, xSoap, xBody, responseNamespace, output.Value.Collection, docliteral))
            Else
                xMethod.AppendChild(AddSimpleType(paramName, xSoap, xBody, outputNamespace, output, docliteral))
            End If

        Next

        'Add the bpInstance parameter if required...
        If session IsNot Nothing Then
            Dim xParam As XmlNode = xSoap.CreateElement("bpInstance", responseNamespace)
            Dim xValue As XmlNode = xSoap.CreateTextNode(session)
            Dim xType As XmlAttribute = xSoap.CreateAttribute("type", xsi.NamespaceURI)
            Dim dType As DataType = DataType.text

            Dim xName As XmlQualifiedName = clsWSDLProcess.AutomateTypeToXSD(dType)
            xType.Value = xBody.GetPrefixOfNamespace(xName.Namespace) & ":" & xName.Name

            xParam.Attributes.Append(xType)
            xParam.AppendChild(xValue)
            xMethod.AppendChild(xParam)
        End If

        xBody.AppendChild(xMethod)

        Return xSoap.OuterXml

    End Function

    Private Shared Function AddSimpleType(paramName As String, xSoap As XmlDocument, xBody As XmlElement, outputNamespace As String, output As clsArgument, docliteral As Boolean) As XmlNode
        'Simple data type...
        Dim xType As XmlAttribute = xSoap.CreateAttribute("type", xsi.NamespaceURI)

        Dim xParam = xSoap.CreateElement(paramName, outputNamespace)
        Dim val As clsProcessValue = output.Value
        Dim xName As XmlQualifiedName = clsWSDLProcess.AutomateTypeToXSD(output.Value.DataType)
        Dim xValue As XmlNode = xSoap.CreateTextNode(clsWSDLProcess.AutomateValueToXSD(val, xName.Name))

        If Not docliteral Then
            xType.Value = xBody.GetPrefixOfNamespace(xName.Namespace) & ":" & xName.Name
            xParam.Attributes.Append(xType)
        End If

        xParam.AppendChild(xValue)
        Return xParam
    End Function

    Private Shared Function AddComplexType(paramName As String, cTypeName As String, xSoap As XmlDocument, xBody As XmlElement, responseNamespace As String, coll As clsCollection, docliteral As Boolean) As XmlNode
        'Collection - complex data type...
        Dim xType As XmlAttribute = xSoap.CreateAttribute("type", xsi.NamespaceURI)

        Dim xParam = xSoap.CreateElement(paramName, responseNamespace)
        If Not docliteral Then
            xType.Value = "tns:" & cTypeName
            xParam.Attributes.Append(xType)
        End If
        For Each row As clsCollectionRow In coll.Rows
            Dim xRow As XmlNode = xSoap.CreateElement("row", responseNamespace)
            For Each field As clsCollectionFieldInfo In coll.FieldDefinitions
                Dim safeName As String = clsProcess.GetSafeName(field.Name)
                If field.DataType = DataType.collection Then
                    xRow.AppendChild(AddComplexType(safeName, cTypeName & "-" & safeName, xSoap, xBody, responseNamespace, row(field.Name).Collection, docliteral))
                Else
                    Dim xField As XmlElement = xSoap.CreateElement(safeName, responseNamespace)
                    Dim xfName As XmlQualifiedName = clsWSDLProcess.AutomateTypeToXSD(field.DataType)
                    If Not docliteral Then
                        Dim xrType As XmlAttribute = xSoap.CreateAttribute("type", xsi.NamespaceURI)
                        xrType.Value = xBody.GetPrefixOfNamespace(xfName.Namespace) & ":" & xfName.Name
                        xField.Attributes.Append(xrType)
                    End If
                    xField.AppendChild(xSoap.CreateTextNode(clsWSDLProcess.AutomateValueToXSD(row(field.Name), xfName.Name)))
                    xRow.AppendChild(xField)
                End If
            Next
            xParam.AppendChild(xRow)
        Next
        Return xParam
    End Function

    ''' <summary>
    ''' Takes a SOAP message containing the inputs for a process-based web service
    ''' and converts them into a set of arguments appropriate for passing to the
    ''' actual process.
    ''' </summary>
    ''' <param name="sInputs">The SOAP message. This may be in either rpc/encoded
    ''' or document/literal form.</param>
    ''' <param name="proc">The target process</param>
    ''' <param name="sMethod">If successful, this contains the requested method on
    ''' return.</param>
    ''' <param name="sSession">If successful, this contains the value of any parameter
    ''' called bpInstance that was found in the inputs. That parameter is never included
    ''' in the inputs XML returned by this method. If it was not present, sSession will
    ''' be Nothing.</param>
    ''' <param name="sErr">In the event of an error, contains an error message</param>
    ''' <returns>The corresponding argument list (as a clsArgumentList), or Nothing
    ''' if an error occurred.</returns>
    Public Shared Function ProcessSOAPInputs(ByVal sInputs As String, ByVal proc As clsProcess, ByRef sMethod As String, ByRef sSession As String, ByRef sErr As String) As clsArgumentList
        Try
            Dim inputs As New clsArgumentList()
            sMethod = Nothing
            sSession = Nothing

            Try
                ' Sanitise the inputs which should only contain the soap envelope xml,
                ' any cr/lf/null characters before and after the envelope can safely
                ' be disregarded.
                sInputs = sInputs.Trim(ControlChars.Cr, ControlChars.Lf, ControlChars.NullChar)

                Dim xDoc As New ReadableXmlDocument(sInputs)
                Dim nsm As New XmlNamespaceManager(xDoc.NameTable)
                nsm.AddNamespace("s", clsSoap.NamespaceURI)
                Dim xBody As XmlNode = xDoc.SelectSingleNode("/s:Envelope/s:Body", nsm)

                For Each xOperation As Xml.XmlNode In xBody.ChildNodes
                    If sMethod IsNot Nothing Then
                        sErr = My.Resources.Resources.clsProcessWSDL_CanOnlyDealWithASingleRequest
                        Return Nothing
                    End If
                    'Because of the way our wsdl is defined, this works for both rpc
                    'and document/literal calls...
                    sMethod = xOperation.LocalName

                    Dim pstage As clsProcessStage
                    If proc.ProcessType = DiagramType.Object Then
                        Dim gSubID As Guid = proc.GetSubSheetIDSafeName(sMethod)
                        pstage = proc.GetStageByTypeAndSubSheet(StageTypes.Start, gSubID)
                    Else
                        pstage = proc.GetStage(proc.GetStartStage())
                    End If
                    If pstage Is Nothing Then
                        sErr = My.Resources.Resources.clsProcessWSDL_CouldNotFindAppropriateStartStageToTranslateParametersFrom
                        Return Nothing
                    End If

                    For Each xParam As Xml.XmlElement In xOperation.ChildNodes
                        If xParam.LocalName = "bpInstance" Then
                            sSession = xParam.InnerText
                        Else

                            Dim pname As String = Nothing
                            Dim pnameFriendly As String = Nothing
                            Dim ptype As DataType
                            Dim pmap As String = Nothing

                            For Each p As clsProcessParameter In pstage.GetParameters()
                                If clsProcess.GetSafeName(p.Name) = xParam.LocalName Then
                                    pname = p.Name
                                    pnameFriendly = p.FriendlyName
                                    ptype = p.GetDataType()
                                    pmap = p.GetMap()
                                End If
                            Next
                            If pname Is Nothing Then
                                sErr = String.Format(My.Resources.Resources.clsProcessWSDL_CouldNotProcessParameter0, xParam.LocalName)
                                Return Nothing
                            End If

                            Dim def As clsCollectionInfo = Nothing
                            If ptype = DataType.collection Then
                                Dim bOutOfScope As Boolean
                                Dim collStg As Stages.clsCollectionStage = TryCast(proc.GetDataStage(pmap, pstage, bOutOfScope), Stages.clsCollectionStage)
                                If collStg IsNot Nothing AndAlso Not bOutOfScope Then
                                    def = New clsCollectionInfo(collStg.Definition)
                                    SetDefaultNestingElement(def)
                                End If
                            End If

                            Dim dType As DataType
                            Dim sType As String = xParam.GetAttribute("type", xsi.NamespaceURI)
                            If sType.Length = 0 Then
                                'If there's no type attribute, it must be document/literal
                                dType = ptype
                            Else
                                'If there's a type attribute, it must be rpc/encoded, and
                                'we need to check that the data type is correct.

                                'Don't like this but it seems to be the only way to do it.
                                sType = sType.Replace(xBody.GetPrefixOfNamespace(xsd.NamespaceURI) & ":", String.Empty)

                                dType = clsWSDLProcess.XSDToAutomateType(sType)
                                If dType <> ptype Then
                                    'We will allow incoming text to be placed into a password
                                    'parameter, because there is no concept of a password
                                    'in the SOAP message, but anything else is a type
                                    'mismatch and results in an error...
                                    If ptype = DataType.password And dType = DataType.text Then
                                        dType = DataType.password
                                    Else
                                        sErr = String.Format(My.Resources.Resources.clsProcessWSDL_SOAPParameterConversion01DidNotMatchExpected2, sType, dType.ToString(), ptype.ToString())
                                        Return Nothing
                                    End If
                                End If
                            End If
                            Dim val As clsProcessValue = clsWSDLProcess.XSDToAutomateValue(dType, xParam, def)

                            Dim input As New clsArgument(pname, val)
                            inputs.Add(input)
                        End If
                    Next
                Next

                If sMethod Is Nothing Then
                    sErr = My.Resources.Resources.clsProcessWSDL_NoMethodCalled
                    Return Nothing
                End If

            Finally
                proc.Dispose()
            End Try

            Return inputs

        Catch ex As Exception
            sErr = ex.ToString()
            Return Nothing
        End Try

    End Function

    Private Shared Sub SetDefaultNestingElement(def As clsCollectionInfo)
        def.NestingElement = "row"
        For Each f As clsCollectionFieldInfo In def.FieldDefinitions()
            If f.HasChildren() Then SetDefaultNestingElement(f.Children)
        Next
    End Sub

End Class
