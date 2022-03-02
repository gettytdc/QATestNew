Imports System.Linq
Imports System.Runtime.Serialization
Imports System.Xml.Linq
Imports BluePrism.Server.Domain.Models
Imports BluePrism.Core.Utility

Namespace WebApis

    ''' <summary>
    ''' Defines a parameter that can be specified when executing a WebApiAction. Certain
    ''' properties of the action can include tokens based on these parameters
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class ActionParameter : Implements IParameter

        <DataMember>
        Private mId As Integer

        <DataMember>
        Private mName As String

        <DataMember>
        Private mDescription As String

        <DataMember>
        Private mDataType As DataType

        <DataMember>
        Private mExposeToProcess As Boolean

        <DataMember>
        Private mInitialValue As clsProcessValue

        ''' <summary>
        ''' Creates a new action parameter without an ID.
        ''' </summary>
        ''' <param name="name">The name of the action parameter to create. Cannot contain a full-stop.</param>
        ''' <param name="description">A description of the action parameter</param>
        ''' <param name="dataType">The type of data required in the new parameter.
        ''' </param>
        ''' <param name="exposeToProcess">True to expose this parameter to the
        ''' process which is calling the action, allowing it to set the value to be
        ''' used by the action call; False to always use the initial value set in the
        ''' Web API service configuration.</param>
        ''' <param name="initialValue">The initial value of the parameter; used in
        ''' any action call if <paramref name="exposeToProcess"/> is set to false or
        ''' if the process does not provide a value for this parameter.</param>
        ''' <exception cref="ArgumentNullException">If either of
        ''' <paramref name="description"/> or <paramref name="initialValue"/> is null
        ''' </exception>
        ''' <exception cref="ArgumentException">If name is null or contains only
        ''' whitespace</exception>
        Public Sub New(
         name As String,
         description As String,
         dataType As DataType,
         exposeToProcess As Boolean,
         initialValue As clsProcessValue)
            Me.New(0, name, description, dataType, exposeToProcess, initialValue)
        End Sub


        ''' <summary>
        ''' Creates a new ActionParameter
        ''' </summary>
        ''' <param name="id">The id of the parameter</param>
        ''' <param name="name">The name of the parameter. Cannot contain a full-stop</param>
        ''' <param name="description">A description of the parameter</param>
        ''' <param name="dataType">The BluePrism data type used for the parameter
        ''' value</param>
        ''' <param name="exposeToProcess">Determines whether the value is obtained
        ''' from an input parameter when executing the action</param>
        ''' <param name="initialValue">The initial value for the parameter</param>
        ''' <exception cref="ArgumentNullException">If either of
        ''' <paramref name="description"/> or <paramref name="initialValue"/> is null
        ''' </exception>
        ''' <exception cref="ArgumentException">If name is null or contains only
        ''' whitespace</exception>
        Public Sub New(id As Integer, name As String, description As String,
         dataType As DataType, exposeToProcess As Boolean, initialValue As clsProcessValue)

            If String.IsNullOrWhiteSpace(name) Then _
             Throw New ArgumentException(NameOf(name))
            If name.Contains(".") Then _
             Throw New InvalidArgumentException(My.Resources.Resources.ActionParameter_NameCannotContainAFullStopCharacter)
            If description Is Nothing Then _
             Throw New ArgumentNullException(NameOf(description))
            If initialValue Is Nothing Then _
             Throw New ArgumentNullException(NameOf(initialValue))

            mId = id
            mName = name
            mDescription = description
            mDataType = dataType
            mExposeToProcess = exposeToProcess
            mInitialValue = initialValue

        End Sub

        ''' <summary>
        ''' The id of the parameter
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Id As Integer
            Get
                Return mId
            End Get
        End Property

        ''' <summary>
        ''' The full name of the parameter
        ''' </summary>
        Public ReadOnly Property Name As String Implements IParameter.Name
            Get
                Return mName
            End Get
        End Property

        ''' <summary>
        ''' Parameter description
        ''' </summary>
        Public ReadOnly Property Description As String
            Get
                Return mDescription
            End Get
        End Property

        ''' <summary>
        ''' The BluePrism data type used for the parameter value
        ''' </summary>
        Public ReadOnly Property DataType As DataType Implements IParameter.DataType
            Get
                Return mDataType
            End Get
        End Property

        ''' <summary>
        ''' Controls whether this can be specified when calling the WebApiAction from
        ''' a process
        ''' </summary>
        Public ReadOnly Property ExposeToProcess As Boolean
            Get
                Return mExposeToProcess
            End Get
        End Property

        ''' <summary>
        ''' The value used by default if the parameter is not specified via an input
        ''' parameter
        ''' </summary>
        Public ReadOnly Property InitialValue As clsProcessValue
            Get
                Return mInitialValue
            End Get
        End Property

        Public ReadOnly Property Direction As ParameterDirection Implements IParameter.Direction
            Get
                Return ParameterDirection.In
            End Get
        End Property

        ''' <summary>
        ''' Generates an XML element representation of this instance of the
        ''' <see cref="ActionParameter"/> object.
        ''' </summary>
        ''' <returns>
        ''' An XML Element representing this object
        ''' </returns>
        Public Function ToXElement() As XElement
            Return _
                <actionparameter
                    name=<%= Name %>
                    expose=<%= ExposeToProcess %>
                    datatype=<%= CInt(DataType) %>>
                    <initialvalue>
                        <%= From d In New XCData(InitialValue.EncodedValue).ToEscapedEnumerable()
                            Select d
                        %>
                    </initialvalue>
                    <description>
                        <%= From d In New XCData(Description).ToEscapedEnumerable()
                            Select d
                        %>
                    </description>
                </actionparameter>

        End Function

        ''' <summary>
        ''' Create a new instance of <see cref="ActionParameter"/> from an XML Element
        ''' that represents that object.
        ''' </summary>
        ''' <returns>
        ''' A new instance of <see cref="ActionParameter"/> from an XML Element
        ''' that represents that object.
        ''' </returns>
        Public Shared Function FromXElement(element As XElement) As ActionParameter

            If Not element.Name.LocalName.Equals("actionparameter") Then _
                Throw New MissingXmlObjectException("actionparameter")

            Dim name = element.Attribute("name")?.Value
            If name Is Nothing Then Throw New MissingXmlObjectException("name")

            Dim dataType = element.
                    Attribute("datatype")?.
                    Value(Of DataType)()
            If dataType Is Nothing Then Throw New MissingXmlObjectException("datatype")

            Dim expose = element.Attribute("expose")?.Value(Of Boolean)()

            If expose Is Nothing Then Throw New MissingXmlObjectException("expose")


            Dim initialValueEncoded = element.
                                         Elements.
                                         FirstOrDefault(Function(x) x.Name = "initialvalue")?.
                                         Nodes.
                                         OfType(Of XCData).
                                         GetConcatenatedValue()

            If initialValueEncoded Is Nothing Then Throw New MissingXmlObjectException("initialvalue")
            Dim initialValue = clsProcessValue.Decode(dataType.GetValueOrDefault(), initialValueEncoded)

            Dim description = element.
                                Elements.
                                FirstOrDefault(Function(x) x.Name = "description")?.
                                Nodes.
                                OfType(Of XCData).
                                GetConcatenatedValue()

            If description Is Nothing Then Throw New MissingXmlObjectException("description")

            Return New ActionParameter(name, description, dataType.GetValueOrDefault(),
                                       expose.GetValueOrDefault(), initialValue)

        End Function

    End Class
End Namespace
