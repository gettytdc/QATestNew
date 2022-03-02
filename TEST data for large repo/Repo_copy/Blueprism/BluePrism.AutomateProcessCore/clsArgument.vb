Imports System.Xml
Imports System.Runtime.Serialization

Imports BluePrism.Common.Security

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsArgument
''' 
''' <summary>
''' Associates a name with a clsProcessValue. An argument is a named value.
''' </summary>
<DataContract([Namespace]:="bp")>
<Serializable()>
Public Class clsArgument

    ''' <summary>
    ''' Construct a new instance.
    ''' </summary>
    ''' <param name="sName">The name of the argument</param>
    ''' <param name="val">The associated value</param>
    Public Sub New(ByVal sName As String, ByVal val As clsProcessValue)
        Name = sName
        Value = val
    End Sub

    ''' <summary>
    ''' The name of the argument
    ''' </summary>
    <DataMember()> _
    Public Name As String

    ''' <summary>
    ''' The value of the argument.
    ''' </summary>
    <DataMember()> _
    Public Value As clsProcessValue

    ''' <summary>
    ''' Turn an argument into XML.
    ''' </summary>
    ''' <param name="xr">An XmlWriter to write the argument XML into</param>
    ''' <param name="bOutput">Determines whether the arguments are outputs or not.
    ''' </param>
    ''' <param name="loggableValue">True to write values in 'loggable' form, False
    ''' for normal format.</param>
    Public Sub ArgumentToXML(ByVal xr As XmlWriter, _
     ByVal bOutput As Boolean, ByVal loggableValue As Boolean)
        If Not bOutput _
         Then xr.WriteStartElement("input") _
         Else xr.WriteStartElement("output")

        xr.WriteAttributeString("name", Name)
        xr.WriteAttributeString("type", Value.EncodedType)
        If Value.Description <> Nothing Then _
            xr.WriteAttributeString("description", Value.Description)


        If Value.DataType = DataType.collection Then
            'Handle collections here
            If Not Value.Collection Is Nothing Then
                If Value.Collection.Rows.Count > 0 Then
                    For Each oRow As clsCollectionRow In Value.Collection.Rows
                        xr.WriteStartElement("row")
                        For Each sField As String In oRow.FieldNames
                            xr.WriteStartElement("field")
                            Dim fld As clsProcessValue = oRow(sField)
                            xr.WriteAttributeString("name", sField)
                            xr.WriteAttributeString("type", fld.EncodedType)
                            If loggableValue _
                             Then xr.WriteAttributeString("value", fld.LoggableValue) _
                             Else xr.WriteAttributeString("value", fld.EncodedValue)

                            xr.WriteEndElement() 'field
                        Next
                        xr.WriteEndElement() 'row
                    Next
                Else
                    xr.WriteAttributeString("encvalue", "<collection />")
                End If
            End If
        ElseIf Value.DataType = DataType.password Then
            If loggableValue _
             Then xr.WriteAttributeString("value", Value.LoggableValue) _
             Else xr.WriteAttributeString("encvalue", Value.EncodedValue)

        Else
            If loggableValue _
             Then xr.WriteAttributeString("value", Value.LoggableValue) _
             Else xr.WriteAttributeString("value", Value.EncodedValue)
        End If

        xr.WriteEndElement() 'input|output
    End Sub

    ''' <summary>
    ''' Convert an XML representation of an argument into a clsArgument.
    ''' </summary>
    ''' <param name="el">The base XmlElement that represents the argument.</param>
    ''' <returns>A clsArgument constructed from the XML.</returns>
    Friend Shared Function XMLToArgument(ByVal el As XmlElement) As clsArgument
        Dim typeStr As String = el.GetAttribute("type")
        Dim valStr As String = el.GetAttribute("value")
        Dim encval As String = el.GetAttribute("encvalue")
        Dim desc As String = Nothing
        If el.HasAttribute("description") Then desc = el.GetAttribute("description")

        Dim strict As Boolean = (el.GetAttribute("treatastext") <> "yes")
        Dim val As clsProcessValue
        If typeStr = "collection" Then
            Select Case encval
                Case "<collection />"
                    val = New clsCollection
                Case "<null collection/>"
                    val = Nothing
                Case Else
                    val = clsCollection.ParseWithoutRoot(el.InnerXml)
            End Select
        ElseIf typeStr = "password" Then
            If encval <> "" Then
                val = clsProcessValue.Decode(typeStr, encval)
            Else
                val = New SafeString(valStr)
            End If
        Else
            val = clsProcessValue.Decode(typeStr, valStr, strict)
        End If
        val.Description = desc
        Return New clsArgument(el.GetAttribute("name"), val)
    End Function

    ''' <summary>
    ''' Decide what to do with xml that has the treatastext attribute. If the
    ''' attribute treatastext="yes" is present we try to parse it and make valid data
    ''' out of it, if not we allow an Invalid clsProcessValue.
    ''' </summary>
    ''' <param name="el">The xml element containing the process value</param>
    ''' <param name="dtype">The datatype of the process value</param>
    ''' <returns>A clsProcessValue that has been treated as text </returns>
    Friend Shared Function TreatAsTextIfNeeded( _
     ByVal el As XmlElement, ByVal dtype As DataType) As clsProcessValue
        Return clsProcessValue.Decode(dtype.ToString(), _
         el.GetAttribute("value"), el.GetAttribute("treatastext") <> "yes")
    End Function

    ''' <summary>
    ''' Deep clones this argument object.
    ''' </summary>
    ''' <returns>A full clone of this argument</returns>
    Public Function Clone() As clsArgument
        Return New clsArgument(Name, Value.Clone())
    End Function

End Class