
Imports System.Net
Imports System.Web.Services.Description
Imports System.Xml
Imports BluePrism.Core.Xml
Imports System.IO
Imports System.Text

'''' Project     : AutomateProcessCore
'''' Class   : AutomateProcessCore.clsSoap
'''' 
'''' <summary>
'''' Implements clsWebServiceProtocol for SOAP messaging.
'''' </summary>
Public Class clsSoap
    Inherits clsWebServiceProtocol

    ''' <summary>
    ''' Standard prefix for soap envelopes
    ''' </summary>
    Public Const Prefix As String = "soap"

    ''' <summary>
    ''' The Namespace of the soap envelope
    ''' </summary>
    Public Const NamespaceURI As String = "http://schemas.xmlsoap.org/soap/envelope/"

    ''' <summary>
    ''' Standard prefix for the target namespace
    ''' </summary>
    Private Const TargetNamespacePrefix As String = "tns"

    ''' <summary>
    ''' The soap version of this class
    ''' </summary>
    Private mdtSoapVersion As SoapVersion

    ''' <summary>
    ''' The binding style for the soap messages
    ''' </summary>
    Private mdtBindingStyle As SoapBindingStyle

    ''' <summary>
    ''' A set of mappings from namespace alias to namespace
    ''' </summary>
    ''' <remarks></remarks>
    Private mNamespaces As New Dictionary(Of String, String)

    ''' <summary>
    ''' And enumeration of the currently available soap versions.
    ''' </summary>
    Public Enum SoapVersion
        Soap11
        Soap12
    End Enum

    ''' <summary>
    ''' Creates a new instance of the clsSoap class
    ''' </summary>
    ''' <param name="dtVersion">The version of the soap protocol to use.</param>
    Public Sub New(ByVal dtVersion As SoapVersion)
        mdtSoapVersion = dtVersion
    End Sub


    ''' <summary>
    ''' Provides access to the SoapBindingStyle
    ''' </summary>
    Public Property BindingStyle() As SoapBindingStyle
        Get
            Return mdtBindingStyle
        End Get
        Set(ByVal value As SoapBindingStyle)
            mdtBindingStyle = value
        End Set
    End Property

    ''' <summary>
    ''' Holds the SoapAction string which can be different from the Action name
    ''' </summary>
    Public Property SoapAction() As String
        Get
            Return msSoapAction
        End Get
        Set(ByVal value As String)
            msSoapAction = value
        End Set
    End Property
    Private msSoapAction As String


    ''' <summary>
    ''' Performs the web service action.
    ''' </summary>
    ''' <param name="parameters">The definition of the parameters</param>
    ''' <param name="inputs">The input parameter values</param>
    ''' <param name="outputs">On return, contains the output parameters from the
    ''' action as a clsArgumentList.</param>
    Public Overrides Sub DoAction(ByVal parameters As IList(Of clsProcessParameter), ByVal inputs As clsArgumentList, ByRef outputs As clsArgumentList)

        Try
            Dim objRequest As WebRequest = GetWebRequest(Location, mTimeout)
            Me.SetupRequest(objRequest)
            Dim objRequestStream As IO.Stream = objRequest.GetRequestStream
            Try
                SendMessage(objRequestStream, inputs, parameters)
            Finally
                If Not objRequestStream Is Nothing Then
                    objRequestStream.Close()
                End If
            End Try

            Dim objResponse As WebResponse = GetWebResponse(objRequest)

            Dim objHTTP As Net.HttpWebResponse = TryCast(objResponse, Net.HttpWebResponse)

            If Not objHTTP Is Nothing Then
                'Deal with HTTP exceptions here
                If objHTTP.StatusCode <> HttpStatusCode.OK Then
                    Select Case objHTTP.StatusCode
                        Case HttpStatusCode.Unauthorized
                            Throw New InvalidOperationException(My.Resources.Resources.clsSoap_YouAreNotAuthorisedToCallThisWebservice)
                        Case HttpStatusCode.InternalServerError
                                'Allowed because response can contain soapfaults
                        Case HttpStatusCode.BadRequest
                            'Allowed because response can contain soapfaults
                        Case Else
                            Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsSoap_GeneralHTTPError0, objHTTP.StatusCode.ToString))
                    End Select
                End If
            End If

            Using stream As Stream = objResponse.GetResponseStream()
                Dim xSoap = GetXmlDocumentFromStream(stream, objHTTP?.CharacterSet)

                RaiseDiags("Received SOAP message:" & xSoap.OuterXml)
                outputs = ReadMessage(xSoap, parameters)
            End Using

        Catch ex As Exception
            Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsSoap_ErrorInWebService0, ex.Message))
        End Try
    End Sub

    ''' <summary>
    ''' Reads an xml document from a given stream, specifying the encoding type 
    ''' if provided by the web response, otherwise defaulting to read from the stream
    ''' </summary>
    ''' <param name="stream">The stream to read the xml from.</param>
    ''' <param name="charset">Tcharset from which to get the encoding.</param>
    Public Shared Function GetXmlDocumentFromStream(stream As Stream, charset As String) As ReadableXmlDocument
        Dim encoding = GetEncoding(charset)

        If encoding IsNot Nothing Then
            Using reader = New StreamReader(stream, encoding)
                Return New ReadableXmlDocument(reader)
            End Using

        End If

        'If no valid encoding is specified, revert to legacy
        'approach where you just pass the stream to the XMLDocument 
        'And it tries to work out the encoding for itself.
        Return New ReadableXmlDocument(stream)
    End Function

    ''' <summary>
    ''' Gets the encoding from a charset if existing and valid, and the encoding type is recognised
    ''' </summary>
    ''' <param name="charset">The charset from which to get the encoding.</param>
    Public Shared Function GetEncoding(charset As String) As Encoding
        If charset Is Nothing Then _
            Return Nothing

        Try
            Return Encoding.GetEncoding(charset)
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    ''' <summary>
    ''' Encodes the list of arguments and Sends a soap message 
    ''' </summary>
    ''' <param name="objStream">The stream to write the message </param>
    ''' <param name="inputs">The inputs to send</param>
    ''' <param name="parameters">The action's paramters, in and out</param>
    Public Sub SendMessage(ByVal objStream As IO.Stream, ByVal inputs As clsArgumentList,
                           ByVal parameters As IList(Of clsProcessParameter))

        Dim eBody As XmlElement = Nothing
        Dim xSoap As XmlDocument = clsSoap.SetupSoapEnvelope(eBody, Nothing)

        ' setup the namespace, if it is empty, don't set it.
        If Not String.IsNullOrEmpty(MyBase.Namespace) Then
            Dim aTargetNameSpace As XmlAttribute =
                xSoap.CreateAttribute(xmlns.Prefix, TargetNamespacePrefix, xmlns.NamespaceURI)
            aTargetNameSpace.Value = MyBase.Namespace
            xSoap.DocumentElement.Attributes.Append(aTargetNameSpace)
        End If

        Dim eContent As XmlElement
        If mAction.InputMessage Is Nothing Then
            eContent = xSoap.CreateElement(TargetNamespacePrefix, mAction.GetName, Me.Namespace)
        Else
            eContent = xSoap.CreateElement(TargetNamespacePrefix, mAction.InputMessage, Me.Namespace)
        End If

        For Each objArg As clsArgument In inputs
            For Each objParam As clsWebParameter In parameters
                Dim objSoapParam As clsSoapParameter = TryCast(objParam, clsSoapParameter)

                If objSoapParam IsNot Nothing AndAlso
                objSoapParam.Direction = ParamDirection.In AndAlso
                objArg.Name = objSoapParam.Name Then

                    'Determine the correct namespace prefix to use...
                    Dim nsp As String = GetNamespaceAlias(xSoap, objSoapParam.NamespaceURI)

                    Select Case BindingStyle
                        Case SoapBindingStyle.Default, SoapBindingStyle.Rpc

                            Select Case objSoapParam.SoapUse
                                Case SoapBindingUse.Default, SoapBindingUse.Encoded
                                    'if no binding use is specified e.g bindinguse = default then assume encoded.
                                    eBody.SetAttribute("encodingStyle", clsSoap.NamespaceURI, objSoapParam.EncodingStyle)
                                Case SoapBindingUse.Literal
                                    'Do nothing
                            End Select

                            If objSoapParam.GetDataType() = DataType.collection Then
                                Throw New InvalidOperationException(My.Resources.Resources.clsSoap_ComplexInputsCollectionsAreOnlySupportedForDocumentStyleSOAPBindings)
                            End If

                            Dim eParam As XmlElement = xSoap.CreateElement(nsp, objSoapParam.Name, objSoapParam.NamespaceURI)
                            GenerateSimpleParameter(eParam, xSoap, objSoapParam, objArg.Value)
                            eContent.AppendChild(eParam)

                        Case SoapBindingStyle.Document

                            Dim eParam As XmlElement
                            If objSoapParam.GetDataType() = DataType.collection Then
                                If objSoapParam.CollectionInfo Is Nothing Then
                                    Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsSoap_NoCollectionInformationForParameter0, objSoapParam.Name))
                                End If
                                If objSoapParam.CollectionInfo.Flat And objSoapParam.CollectionInfo.Count = 1 Then
                                    GenerateFlatParameters(eContent, xSoap, objSoapParam, objArg.Value, nsp, objSoapParam.NamespaceURI)
                                ElseIf objSoapParam.CollectionInfo.Flat And objSoapParam.CollectionInfo.Count > 1 Then
                                    ' need to deal with this complex type.
                                    eParam = xSoap.CreateElement(nsp, objSoapParam.Name, objSoapParam.NamespaceURI)
                                    GenerateComplexParameter(eParam, xSoap, objSoapParam.CollectionInfo, objArg.Value, nsp, objSoapParam.NamespaceURI)
                                    eContent.AppendChild(eParam)
                                Else
                                    eParam = xSoap.CreateElement(nsp, objSoapParam.Name, objSoapParam.NamespaceURI)
                                    GenerateComplexParameter(eParam, xSoap, objSoapParam.CollectionInfo, objArg.Value, nsp, objSoapParam.NamespaceURI)
                                    eContent.AppendChild(eParam)
                                End If
                            Else
                                eParam = xSoap.CreateElement(nsp, objSoapParam.Name, objSoapParam.NamespaceURI)
                                GenerateSimpleParameter(eParam, xSoap, objSoapParam, objArg.Value)

                                eContent.AppendChild(eParam)
                            End If

                    End Select
                End If
            Next
        Next
        eBody.AppendChild(eContent)

        Dim sSoap As String = xSoap.OuterXml
        RaiseDiags("Sending SOAP message:" & sSoap)

        Using sw As New IO.StreamWriter(objStream, New Text.UTF8Encoding, 4096, True)
            sw.Write(sSoap)
            sw.Flush()
        End Using
    End Sub

    ''' <summary>
    ''' Get the correct namespace alias for the given namespace
    ''' </summary>
    ''' <param name="xSoap"></param>
    ''' <param name="namespace"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function GetNamespaceAlias(xSoap As XmlDocument, [namespace] As String) As String
        'Determine the correct namespace prefix to use...
        Dim nsp As String = TargetNamespacePrefix
        If Not String.IsNullOrEmpty([namespace]) AndAlso [namespace] <> MyBase.Namespace Then
            If mNamespaces.ContainsKey([namespace]) Then
                nsp = mNamespaces([namespace])
            Else
                nsp = "ns" & (mNamespaces.Count + 1).ToString()
                mNamespaces([namespace]) = nsp
                Dim nsa As XmlAttribute = xSoap.CreateAttribute(xmlns.Prefix, nsp, xmlns.NamespaceURI)
                nsa.Value = [namespace]
                xSoap.DocumentElement.Attributes.Append(nsa)
            End If
        End If
        Return nsp
    End Function

    ''' <summary>
    ''' Generate a flat list of parameters - i.e. output a collection in such a way
    ''' that each row is just a repeated element, rather than having a container.
    ''' </summary>
    ''' <param name="parent">The parent element, to which the parameters will
    ''' be appended</param>
    ''' <param name="xSoap">That parent SOAP XmlDocument.</param>
    ''' <param name="soapParam">The SOAP parameter that represents the collection.
    ''' </param>
    ''' <param name="objValue">The value - this should be a collection.</param>
    ''' <param name="nsp">The namespace prefix for all the child elements</param>
    ''' <param name="ns">The namespace for all the child elements</param>
    Private Sub GenerateFlatParameters(ByVal parent As XmlElement, ByVal xSoap As XmlDocument, ByVal soapParam As clsSoapParameter, ByVal objValue As clsProcessValue, ByVal nsp As String, ByVal ns As String)

        If soapParam.CollectionInfo.Count <> 1 Then
            Throw New InvalidOperationException(My.Resources.Resources.clsSoap_CanOnlyGenerateFlatParametersWhenThereIsOneField)
        End If
        Dim field As String = objValue.Collection.Definition(0).Name

        If objValue.Collection.StartIterate() Then
            Do

                Dim eParam As XmlElement
                eParam = xSoap.CreateElement(nsp, soapParam.Name, ns)
                GenerateSimpleParameter(eParam, xSoap, soapParam, objValue.Collection.GetField(field))
                parent.AppendChild(eParam)

            Loop While objValue.Collection.ContinueIterate()
        End If
    End Sub

    ''' <summary>
    ''' Creates a complex parameter (i.e. a collection)
    ''' </summary>
    ''' <param name="eParam">The parameter to which to add the xml</param>
    ''' <param name="xSoap">The xmldocument (needed for creating new elements)</param>
    ''' <param name="coll">The collection info which defines the collection fields</param>
    ''' <param name="objValue">The value of the collection</param>
    ''' <param name="nsp">The namespace prefix (this is usually tns: for example)</param>
    ''' <param name="ns">The namespace</param>
    Private Sub GenerateComplexParameter(ByVal eParam As XmlElement, ByVal xSoap As XmlDocument, ByVal coll As clsCollectionInfo, ByVal objValue As clsProcessValue, ByVal nsp As String, ByVal ns As String)
        If objValue.Collection.StartIterate() Then
            Do
                Dim parent As XmlElement
                If coll.NestingElement IsNot Nothing Then
                    Dim eRow As XmlElement = xSoap.CreateElement(nsp, coll.NestingElement, ns)
                    parent = eRow
                Else
                    parent = eParam
                End If
                For Each field As clsCollectionFieldInfo In coll
                    Dim objChildValue As clsProcessValue = Nothing
                    Try
                        objChildValue = objValue.Collection.GetField(field.Name)
                    Catch
                    End Try
                    Dim newNs As String = GetNamespaceAlias(xSoap, field.Namespace)
                    Dim eField As XmlElement = xSoap.CreateElement(newNs, field.Name, field.Namespace)

                    If objChildValue IsNot Nothing Then
                        If field.HasChildren() AndAlso
                         objChildValue.DataType = DataType.collection Then
                            ' Deal with case where there's a complex type but no entires
                            If objChildValue.Collection IsNot Nothing Then
                                GenerateComplexParameter(eField, xSoap, field.Children, objChildValue, newNs, field.Namespace)
                            End If
                        Else
                            Dim xType As XmlQualifiedName = clsWSDLProcess.AutomateTypeToXSD(objChildValue.DataType)
                            Dim sValue As String = clsWSDLProcess.AutomateValueToXSD(objChildValue, xType.Name)
                            Dim eValue As XmlText = xSoap.CreateTextNode(sValue)
                            eField.AppendChild(eValue)
                        End If
                    End If

                    parent.AppendChild(eField)
                Next
                If coll.NestingElement IsNot Nothing Then
                    eParam.AppendChild(parent)
                End If
            Loop While objValue.Collection.ContinueIterate()
        End If
    End Sub

    Private Sub GenerateSimpleParameter(ByVal eParam As XmlElement, ByVal xSoap As XmlDocument, ByVal objSoapParam As clsSoapParameter, ByVal value As clsProcessValue)

        Dim sValue As String = clsWSDLProcess.AutomateValueToXSD(value)

        If Not objSoapParam.SoapUse = SoapBindingUse.Literal Then
            Dim eType As XmlAttribute = xSoap.CreateAttribute(xsi.Prefix, "type", xsi.NamespaceURI)
            eType.Value = xsd.Prefix & ":" & objSoapParam.XSDType
            eParam.Attributes.Append(eType)
        End If

        Dim eValue As XmlText = xSoap.CreateTextNode(sValue)
        eParam.AppendChild(eValue)
    End Sub

    ''' <summary>
    ''' Reads a soap message and translates the results into a list of clsArgument.
    ''' </summary>
    ''' <param name="xSoap">The SOAP message.</param>
    ''' <param name="parameters">The definition of the parameters.</param>
    ''' <returns>The translated output parameters, as a clsArgumentList.</returns>
    Public Function ReadMessage(ByVal xSoap As XmlDocument, ByVal parameters As IList(Of clsProcessParameter)) As clsArgumentList

        Dim outputs As New clsArgumentList()

        For Each eChild As XmlNode In xSoap.ChildNodes
            If eChild.LocalName.ToLower = "envelope" Then
                Dim eEnvelope As XmlNode = eChild
                Dim sPrefix As String = eEnvelope.GetPrefixOfNamespace(clsSoap.NamespaceURI)
                For Each eGrandChild As XmlNode In eEnvelope.ChildNodes
                    If eGrandChild.LocalName.ToLower = "body" AndAlso eGrandChild.Prefix = sPrefix Then

                        Dim eBody As XmlNode = eGrandChild
                        For Each eGreatGrandChild As XmlNode In eBody.ChildNodes
                            If eGreatGrandChild.LocalName.ToLower = "fault" AndAlso eGreatGrandChild.Prefix = sPrefix Then
                                'Deal with soap faults here
                                Throw New clsSoapException(xSoap)
                            Else

                                Dim eReplyContent As XmlNode = eBody.FirstChild

                                For Each eParam As XmlElement In eReplyContent.ChildNodes

                                    For Each objParam As clsWebParameter In parameters
                                        If objParam.Direction = ParamDirection.Out Then
                                            If eParam.LocalName = objParam.Name Then

                                                Dim exarg As clsArgument = outputs(objParam.Name)
                                                If exarg IsNot Nothing Then

                                                    'We already have an output with this name - it must be part of
                                                    'an array, so we need to extend a collection...
                                                    If exarg.Value.DataType <> DataType.collection Then
                                                        Throw New InvalidOperationException(String.Format(My.Resources.Resources.clsSoap_ExpectedACollectionToExtendForParameter0, objParam.Name))
                                                    End If
                                                    'Create a collection from this element...
                                                    Dim excol As clsCollection = clsWSDLProcess.CreateCollectionFromXML(eParam, objParam.CollectionInfo)
                                                    'Add the rows (probably one one) to the collection in the existing
                                                    'argument. No need to clone, because we're going to ditch the
                                                    'source collection anyway.
                                                    For Each row As clsCollectionRow In excol.Rows
                                                        exarg.Value.Collection.Add(row)
                                                    Next

                                                Else

                                                    'We haven't encountered this output before (the normal non-array
                                                    'state of affairs) so we create a new argument...
                                                    Dim val As clsProcessValue
                                                    val = clsWSDLProcess.XSDToAutomateValue(objParam.GetDataType(), eParam, objParam.CollectionInfo)
                                                    outputs.Add(New clsArgument(objParam.Name, val))

                                                End If
                                                Exit For
                                            End If
                                        End If
                                    Next
                                Next
                            End If
                        Next
                    End If
                Next
            End If
        Next

        Return outputs

    End Function

    ''' <summary>
    ''' Sets up a soap envelope.
    ''' </summary>
    ''' <param name="xBody">For convenience on return, the body of the soap
    ''' envelope.</param>
    ''' <param name="ns">Main namespace, or Nothing to not include it.</param>
    ''' <returns>A new XmlDocument representing the soap envelope.</returns>
    Public Shared Function SetupSoapEnvelope(ByRef xBody As XmlElement, ByVal ns As String) As XmlDocument
        Dim xSoap As New XmlDocument
        Dim xDeclaration As XmlDeclaration = xSoap.CreateXmlDeclaration("1.0", "UTF-8", Nothing)
        xSoap.AppendChild(xDeclaration)

        Dim xEnvelope As XmlElement = xSoap.CreateElement(clsSoap.Prefix, "Envelope", clsSoap.NamespaceURI)
        xBody = xSoap.CreateElement(clsSoap.Prefix, "Body", clsSoap.NamespaceURI)

        xEnvelope.AppendChild(xBody)
        xSoap.AppendChild(xEnvelope)

        'Main namespace...
        If ns IsNot Nothing Then
            Dim mns As XmlAttribute = xSoap.CreateAttribute("xmlns")
            mns.Value = ns
            xSoap.DocumentElement.Attributes.Append(mns)
        End If

        'xsi namespace
        Dim xsins As XmlAttribute = xSoap.CreateAttribute(xmlns.Prefix, xsi.Prefix, xmlns.NamespaceURI)
        xsins.Value = Xml.Schema.XmlSchema.InstanceNamespace
        xSoap.DocumentElement.Attributes.Append(xsins)

        'xs namespace
        Dim xsns As XmlAttribute = xSoap.CreateAttribute(xmlns.Prefix, xsd.Prefix, xmlns.NamespaceURI)
        xsns.Value = Xml.Schema.XmlSchema.Namespace
        xSoap.DocumentElement.Attributes.Append(xsns)

        'soapenc namespace
        Dim spenc As XmlAttribute = xSoap.CreateAttribute(xmlns.Prefix, soapenc.Prefix, xmlns.NamespaceURI)
        spenc.Value = soapenc.NamespaceURI
        xSoap.DocumentElement.Attributes.Append(spenc)

        Return xSoap
    End Function

    ''' <summary>
    ''' Sets up the webrequest giving it the correct content type and header for the soap protocol version
    ''' in use.
    ''' </summary>
    ''' <param name="request">The request to set up</param>
    Public Sub SetupRequest(ByVal request As WebRequest)
        request.Method = "POST"

        If mdtSoapVersion = SoapVersion.Soap12 Then
            request.ContentType = "application/soap+xml"
        ElseIf mdtSoapVersion = SoapVersion.Soap11 Then
            request.ContentType = "text/xml"
            request.Headers.Add("SOAPAction", Me.SoapAction)
        End If
    End Sub

End Class
