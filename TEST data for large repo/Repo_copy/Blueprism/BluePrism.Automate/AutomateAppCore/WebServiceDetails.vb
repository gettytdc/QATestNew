Imports System.Runtime.Serialization

''' <summary>
''' Class holding details of an Process/Object exposed as a web service.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class WebServiceDetails

    ''' <summary>
    ''' WebService Exposure name
    ''' </summary>
    <DataMember>
    Public Property WebServiceName() As String

    ''' <summary>
    ''' Force Document Literal Friendly Name
    ''' </summary>
    <DataMember>
    Public Property EncodingFormatEnum() As WebServiceEncodingFormat

    ''' <summary>
    ''' States which Namespace Structure to use
    ''' </summary>
    <DataMember>
    Public Property UseLegacyNamespaceStructure() As Boolean

    ''' <summary>
    ''' Default Constructor
    ''' </summary>
    Public Sub New()

        WebServiceName = ""
        EncodingFormatEnum = WebServiceEncodingFormat.Rpc
        UseLegacyNamespaceStructure = False

    End Sub

    ''' <summary>
    ''' Default Constructor with paramater overloading
    ''' </summary>
    ''' <param name="wsName">States which web service name to use</param>
    ''' <param name="forceDocumentLiteral">Sets the document force literal flag</param>
    ''' <param name="useLegacyNamespace">States if the web service should use the Legacy Namespace Format</param>
    Public Sub New(ByVal wsName As String, ByVal forceDocumentLiteral As WebServiceEncodingFormat, ByVal useLegacyNamespace As Boolean)

        WebServiceName = wsName
        EncodingFormatEnum = forceDocumentLiteral
        UseLegacyNamespaceStructure = useLegacyNamespace

    End Sub

    ''' <summary>
    ''' Default Constructor with paramater overloading
    ''' </summary>
    ''' <param name="wsName">States which web service name to use</param>
    ''' <param name="documentLiteralEnum">Sets the document force literal flag</param>
    ''' <param name="useLegacyNamespace">States if the web service should use the Legacy Namespace Format</param>
    Public Sub New(ByVal wsName As String, ByVal documentLiteralEnum As Boolean, ByVal useLegacyNamespace As Boolean)

        WebServiceName = wsName
        EncodingFormatEnum = ConvertBoolToEnum(documentLiteralEnum)
        UseLegacyNamespaceStructure = useLegacyNamespace

    End Sub

    ''' <summary>
    ''' Default Constructor with paramater overloading
    ''' </summary>
    ''' <param name="data">Get the object member data of WSDetails</param>
    Public Sub New(ByVal data As WebServiceDetails)

        WebServiceName = data.WebServiceName
        EncodingFormatEnum = data.EncodingFormatEnum
        UseLegacyNamespaceStructure = data.UseLegacyNamespaceStructure
    End Sub

    ''' <summary>
    ''' Convert a boolean value into a WebServiceEncodingFormat Enum
    ''' </summary>
    ''' <param name="value">A boolean value converted to a WebServiceEncodingFormat</param>
    Private Function ConvertBoolToEnum(ByVal value As Boolean) As WebServiceEncodingFormat

        Return If(value, WebServiceEncodingFormat.DocumentLiteral, WebServiceEncodingFormat.Rpc)

    End Function


    ''' <summary>
    ''' State if the web service encoding type is or is not Document/literal
    ''' </summary>
    Public Function IsDocumentLiteral() As Boolean

        Return EncodingFormatEnum = WebServiceEncodingFormat.DocumentLiteral

    End Function

    ''' <summary>
    ''' Equality Operator overloading
    ''' </summary>
    ''' <param name="objOne">WebServiceDetails The first object to be compared</param>
    ''' <param name="objTwo">WebServiceDetails The second object to be compared</param>
    Public Shared Operator =(ByVal objOne As WebServiceDetails, ByVal objTwo As WebServiceDetails) As Boolean
        If objOne.EncodingFormatEnum = objTwo.EncodingFormatEnum AndAlso
            objOne.UseLegacyNamespaceStructure = objTwo.UseLegacyNamespaceStructure AndAlso
            objOne.WebServiceName = objTwo.WebServiceName Then
            Return True
        End If
        Return False
    End Operator

    ''' <summary>
    ''' Negation Operator overloading
    ''' </summary>
    ''' <param name="objOne">WebServiceDetails The first object to be compared</param>
    ''' <param name="objTwo">WebServiceDetails The second object to be compared</param>
    Public Shared Operator <>(ByVal objOne As WebServiceDetails, ByVal objTwo As WebServiceDetails) As Boolean
        Return Not objOne = objTwo
    End Operator

End Class
