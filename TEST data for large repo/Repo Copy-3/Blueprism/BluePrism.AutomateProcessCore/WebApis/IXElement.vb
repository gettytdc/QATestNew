Imports System.Xml.Linq

Namespace WebApis

    Public Interface IXElement

        ''' <summary>
        ''' Generates an XML element representation of this instance of the
        ''' contracted object.
        ''' </summary>
        ''' <returns>
        ''' An XML Element representing this object
        ''' </returns>
        Function ToXElement() As XElement
    End Interface

End Namespace