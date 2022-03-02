Imports System.Xml

Imports BluePrism.Core.Expressions
Imports BluePrism.BPCoreLib
Imports BluePrism.AMI
Imports System.Runtime.Serialization

''' <summary>
''' Encapsulates the notion of an element parameter, as passed
''' as a dynamic value to an attribute of an element, at runtime.
''' </summary>
<Serializable, DataContract([Namespace]:="bp")>
Public Class clsApplicationElementParameter : Implements IExpressionHolder

#Region " Member Variables "

    <DataMember>
    Private mName As String

    <DataMember>
    Private mDataType As DataType

    <DataMember>
    Private mExpression As BPExpression

    <DataMember>
    Private mComparison As clsAMI.ComparisonTypes

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty parameter with an
    ''' <see cref="clsAMI.ComparisonTypes.Equal">equals</see> comparison type.
    ''' </summary>
    Public Sub New()
        Me.New(Nothing, Nothing, Nothing, clsAMI.ComparisonTypes.Equal)
    End Sub

    ''' <summary>
    ''' Creates a new parameter with the given values.
    ''' </summary>
    ''' <param name="nm">The name of the parameter</param>
    ''' <param name="dtype">The datatype of the parameter</param>
    ''' <param name="expr">The expression which provides the value to pass for this
    ''' parameter.</param>
    ''' <param name="comp">The comparison type governing this parameter.</param>
    Public Sub New(ByVal nm As String, ByVal dtype As DataType, _
     ByVal expr As BPExpression, ByVal comp As clsAMI.ComparisonTypes)
        Me.Name = nm
        Me.DataType = dtype
        Me.Expression = expr
        Me.ComparisonType = comp
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the name of the parameter. This should match the name of the
    ''' attribute whose value is to be specified.
    ''' </summary>
    Public Property Name() As String
        Get
            Return mName
        End Get
        Set(ByVal value As String)
            mName = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the data type expected. This should match the data type of the
    ''' attribute whose value is being specified.
    ''' </summary>
    Public Property DataType() As DataType
        Get
            Return mDataType
        End Get
        Set(ByVal value As DataType)
            mDataType = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the expression specifying the value to be specified for the
    ''' attribute in question.
    ''' </summary>
    ''' <remarks>This can never be null - if null is passed when setting this value,
    ''' it is silently changed to be <see cref="BPExpression.Empty"/></remarks>
    Public Property Expression() As BPExpression _
     Implements IExpressionHolder.Expression
        Get
            Return mExpression
        End Get
        Set(ByVal value As BPExpression)
            If value Is Nothing Then value = BPExpression.Empty
            mExpression = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the type of comparison to be used when matching the attribute's
    ''' value.
    ''' </summary>
    Public Property ComparisonType() As clsAMI.ComparisonTypes
        Get
            Return mComparison
        End Get
        Set(ByVal value As clsAMI.ComparisonTypes)
            mComparison = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Serialises the class to xml.
    ''' </summary>
    ''' <param name="doc">The document to which the generated xml should be added.
    ''' </param>
    ''' <returns>Returns an xml element representing the current object.</returns>
    Public Function ToXML(ByVal doc As XmlDocument) As XmlElement
        Dim elem As XmlElement = doc.CreateElement("elementparameter")

        BPUtil.AppendTextElement(elem, "name", Name)
        BPUtil.AppendTextElement(elem, "expression", Expression.NormalForm)
        BPUtil.AppendTextElement(elem, "datatype", DataType.ToString())
        BPUtil.AppendTextElement(elem, "comparisontype", ComparisonType.ToString())

        Return elem
    End Function

    ''' <summary>
    ''' Deserializes the given xml element into an app element parameter
    ''' </summary>
    ''' <param name="e">The XML element which contains the structure defining the
    ''' application element parameter</param>
    ''' <returns>The parameter object, initialised with the data from the given XML
    ''' element.</returns>
    ''' <exception cref="ArgumentNullException">If <paramref name="e"/> is null.
    ''' </exception>
    ''' <exception cref="ArgumentException">If the XML element's local name was
    ''' something other than <c>"elementparameter"</c> -or- the 'comparisontype'
    ''' value was not recognised.</exception>
    ''' <exception cref="InvalidDataTypeException">If the 'datatype' value within the
    ''' XML element was not recognised</exception>
    Public Shared Function FromXML(ByVal e As XmlElement) _
     As clsApplicationElementParameter
        If e Is Nothing Then Throw New ArgumentNullException(NameOf(e))
        If e.Name <> "elementparameter" Then Throw New ArgumentException( _
         "Name of element must be 'elementparameter'", NameOf(e))

        Dim param As New clsApplicationElementParameter()

        For Each child As XmlNode In e.ChildNodes
            Select Case child.Name
                Case "name"
                    param.Name = child.InnerText
                Case "datatype"
                    param.DataType = clsProcessDataTypes.Parse(child.InnerText)
                Case "expression"
                    param.Expression = BPExpression.FromNormalised(child.InnerText)
                Case "comparisontype"
                    param.ComparisonType = _
                     clsEnum(Of clsAMI.ComparisonTypes).Parse(child.InnerText)
            End Select
        Next

        Return param

    End Function

    ''' <summary>
    ''' Clones the current object.
    ''' </summary>
    ''' <returns>Returns a deep clone of the current object.</returns>
    Public Function Clone() As clsApplicationElementParameter
        ' The members are either enums or immutable, so a shallow clone is fine
        Return DirectCast(MemberwiseClone(), clsApplicationElementParameter)
    End Function

    ''' <summary>
    ''' Validates the parameter.
    ''' </summary>
    ''' <param name="parentStage">The stage owning this parameter. Used as the scope
    ''' stage when evaluating expressions.</param>
    ''' <param name="bAttemptRepair">Set to true to have errors automatically
    ''' repaired, where possible.</param>
    ''' <param name="loc">Additional information about the location of the error.
    ''' It should either be an empty string, or something like " in row 4". Note
    ''' the leading space.</param>
    ''' <returns>Returns a list of errors found.</returns>
    Friend Function CheckForErrors(ByVal parentStage As clsProcessStage, _
     ByVal bAttemptRepair As Boolean, ByVal loc As String) As ValidationErrorList
        Return clsExpression.CheckExpressionForErrors(
         Expression, parentStage, DataType,
         loc, New clsExpressionInfo(), Nothing)
    End Function

#End Region

End Class
