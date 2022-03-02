Imports System.Runtime.Serialization


''' <summary>
''' Exception indicating that an XML object was expected but could not be found.
''' </summary>
<Serializable()>
Public Class MissingXmlObjectException : Inherits BluePrismException

    ''' <summary>
    ''' Create a new exception for invalid XML
    ''' </summary>
    ''' <param name="expected">
    ''' The name of the XML object that was expected to be found but wasn't
    ''' </param>
    Public Sub New(expected As String)
        MyBase.New(My.Resources.MissingXmlObjectException_CouldNotFind0, expected)
    End Sub


    ''' <summary>
    ''' Creates a new exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception should
    ''' draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub


End Class