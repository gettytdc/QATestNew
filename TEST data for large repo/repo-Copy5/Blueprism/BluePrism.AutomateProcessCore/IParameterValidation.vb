Imports System.Xml

Public Interface IParameterValidation

    ''' <summary>
    ''' Parameter to represent the validation data, string is parsed
    ''' </summary>
    ''' <returns>Parameter data</returns>
    Property Parameter As String

    ''' <summary>
    ''' Function to validated the supplied data
    ''' </summary>
    ''' <param name="map"></param>
    ''' <returns></returns>
    Function Validate(map As String) As Boolean

    ''' <summary>
    ''' Error message if validation fails
    ''' </summary>
    ''' <returns></returns>
    Function Message() As String

    ''' <summary>
    ''' Creates a copy of the data
    ''' </summary>
    ''' <returns>clone of current object</returns>
    Function Clone() As RangeParameterValidation

    ''' <summary>
    ''' Helper function to convert object to xml representation
    ''' </summary>
    ''' <param name="parentDocument"></param>
    ''' <returns></returns>
    Function ToXML(ByVal parentDocument As XmlDocument) As XmlElement

End Interface

