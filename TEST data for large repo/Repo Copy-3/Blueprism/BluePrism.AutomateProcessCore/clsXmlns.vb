''' <summary>
''' Provides the default namespace and prefix for XMLNS
''' </summary>
Public Module xmlns
    ''' <summary>
    ''' The namespace for XMLNS
    ''' </summary>
    Public Const NamespaceURI As String = "http://www.w3.org/2000/xmlns/"
    ''' <summary>
    ''' The prefix for XMLNS (xmlns)
    ''' </summary>
    Public Const Prefix As String = "xmlns"
End Module

''' <summary>
''' Provides the default namespace and prefix for XSI
''' </summary>
Public Module xsi
    ''' <summary>
    ''' The XSI namespace
    ''' </summary>
    Public Const NamespaceURI As String = Xml.Schema.XmlSchema.InstanceNamespace
    ''' <summary>
    ''' The xsi prefix (xsi)
    ''' </summary>
    Public Const Prefix As String = "xsi"
End Module

''' <summary>
''' Provides default namespaces and prefixes for xsd
''' </summary>
Public Module xsd
    ''' <summary>
    ''' The xsd namespace
    ''' </summary>
    Public Const NamespaceURI As String = Xml.Schema.XmlSchema.Namespace
    ''' <summary>
    ''' The xsd prefix (xs)
    ''' </summary>
    Public Const Prefix As String = "xs"
End Module

''' <summary>
''' Provides the default namespace and prefix for soapenc
''' </summary>
Public Module soapenc
    ''' <summary>
    ''' The soapenc namespace
    ''' </summary>
    Public Const NamespaceURI As String = "http://schemas.xmlsoap.org/soap/encoding/"
    ''' <summary>
    ''' The soapenc prefix (soapenc)
    ''' </summary>
    Public Const Prefix As String = "soapenc"
End Module

''' <summary>
''' Provides the default namespace and prefixe for wsdl
''' </summary>
Public Module wsdl
    ''' <summary>
    ''' The WSDL namespace
    ''' </summary>
    Public Const NamespaceURI As String = "http://schemas.xmlsoap.org/wsdl/"
    ''' <summary>
    ''' The WSDL prefix (wsdl)
    ''' </summary>
    Public Const Prefix As String = "wsdl"
End Module
