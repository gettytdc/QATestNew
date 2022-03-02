Imports System.IO
Imports System.Xml
Imports System.Runtime.Serialization
Imports BluePrism.BPCoreLib
Imports BluePrism.Core.Xml
Imports BluePrism.Server.Domain.Models

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsArgumentList
''' 
''' <summary>
''' Represents a list of clsArgument (named values)
''' </summary>
<Serializable()>
Public Class clsArgumentList
    Inherits List(Of clsArgument)
    Implements ISerializable, ICloneable
    ''' <summary>
    ''' Appends the given argument list to the specified XML writer, ensuring that
    ''' an empty element is created if the list is null / empty.
    ''' </summary>
    ''' <param name="xw">The writer to write the argument list to</param>
    ''' <param name="lst">The list to be written</param>
    ''' <param name="output">True to indicate that the list is a list of output
    ''' arguments; False to indicate that it is a list of input arguments</param>
    Public Shared Sub AppendToXml( _
     ByVal xw As XmlWriter, ByVal lst As clsArgumentList, ByVal output As Boolean)
        Dim elemName As String = CStr(IIf(output, "outputs", "inputs"))
        If lst Is Nothing _
         Then xw.WriteElementString(elemName, "") _
         Else lst.ArgumentsToXML(xw, output)
    End Sub

    Private mIsRunning As Boolean

    ''' <summary>
    ''' Creates a new empty argument list.
    ''' </summary>
    Public Sub New()
    End Sub

    ''' <summary>
    ''' Creates a new argument list, with the supplied arguments.
    ''' </summary>
    ''' <param name="arguments">The sequence of clsArguments instances. </param>
    Public Sub New(arguments As IEnumerable(Of clsArgument))
        MyBase.New(arguments)
    End Sub

    ''' <summary>
    ''' Creates a new argument list from the given serialized data
    ''' </summary>
    ''' <param name="info">The info containing the data for this argument list
    ''' </param>
    ''' <param name="context">The context of the serialization</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        Populate(info.GetString("xmlAsInput"), True)
    End Sub

    ''' <summary>
    ''' Gets the clsArgument corresponding to the given name, or null if no such
    ''' argument with that name was found.
    ''' Since technically, you can create multiple arguments with the same
    ''' name (though quite why you'd want to, I have no idea), this property will
    ''' return the <em>first</em> argument with the specified name found.
    ''' </summary>
    ''' <param name="name">The name of the argument.</param>
    ''' <remarks>This is read only, just because the arg list currently supports
    ''' multiple arguments with the same name, and a Set becomes disconnected
    ''' from its associated Get in those circumstances (eg. after setting 
    ''' 'obj("a") = "b" : obj("a") = "c"'
    ''' then calling : 
    ''' 'x = obj("a")'
    ''' would set x to "b", not "c" as might be expected.
    ''' </remarks>
    Default Public Overloads ReadOnly Property Item(ByVal name As String) As clsArgument
        Get
            For Each arg As clsArgument In Me
                If arg.Name = name Then Return arg
            Next
            Return Nothing
        End Get
        'TODO: Above indicates this is readonly on purpose, but why would you
        ' ever want an arg list to contain 2 arguments with the same name?
        'Set(ByVal value As clsArgument)
        '   For i As Integer = 0 To Me.Count - 1
        '       If Me(i).Name = name Then
        '           If value Is Nothing Then Me.RemoveAt(i) Else Me(i) = value
        '           Return
        '       End If
        '   Next
        '   If value IsNot Nothing Then Add(value)
        'End Set
    End Property

    ''' <summary>
    ''' Gets the process value corresponding to the given argument name, or null
    ''' if no argument with that name was found.
    ''' </summary>
    ''' <param name="name">The name of the argument for which the value is
    ''' required.</param>
    ''' <returns>The process value corresponding to the argument with the
    ''' specified name.</returns>
    Public Function GetValue(ByVal name As String) As clsProcessValue
        Dim arg As clsArgument = Me(name)
        If arg IsNot Nothing Then Return arg.Value
        Return Nothing
    End Function

    ''' <summary>
    ''' Gets the string corresponding to the given argument name, or null if no
    ''' argument with that name was found. Specifically, this will return the
    ''' <see cref="clsProcessValue.EncodedValue">EncodedValue</see> of the value
    ''' set in this argument list with the given name.
    ''' </summary>
    ''' <param name="name">The name of the argument from which to draw the string.
    ''' </param>
    ''' <returns>The encoded value of the argument with the given name, or null if
    ''' no such argument was found in this object.</returns>
    ''' <exception cref="BadCastException">If the argument referenced by the given
    ''' name was of a type which could not be converted into a string, ie. binary,
    ''' image, collection or unknown.</exception>
    Public Function GetString(ByVal name As String) As String
        Dim arg As clsArgument = Me(name)
        If arg IsNot Nothing Then Return CStr(arg.Value)
        Return Nothing
    End Function

    ''' <summary>
    ''' Sets the first argument in this argument list with the given name (or a
    ''' new argument, if no such argument currently exists) to have the specified
    ''' value, returning the argument that was set as a result.
    ''' </summary>
    ''' <param name="name">The name of the argument to set</param>
    ''' <param name="value">The value to set the argument to</param>
    ''' <returns>The argument that was added or modified as a result of this
    ''' method.</returns>
    Public Function SetValue(ByVal name As String, ByVal value As clsProcessValue) _
     As clsArgument
        Dim arg As clsArgument = Me(name)
        If arg IsNot Nothing Then
            arg.Value = value
        Else
            arg = New clsArgument(name, value)
            Add(arg)
        End If
        Return arg
    End Function

    ''' <summary>
    ''' Property indicating if any of the arguments in this list are set, ie. are
    ''' <em>not</em> null, according to the <see cref="clsProcessValue.IsNull"/>
    ''' property.
    ''' Note that this will return false if the argument list is empty.
    ''' </summary>
    Public ReadOnly Property AreAnyArgsSet() As Boolean
        Get
            If Count = 0 Then Return False
            For Each arg As clsArgument In Me
                If Not arg.Value.IsNull Then Return True
            Next
            Return False
        End Get
    End Property

    ''' <summary>
    ''' Gets whether all of the arguments in this list are set, ie. are <em>not</em>
    ''' null, according to the <see cref="clsProcessValue.IsNull"/> property.
    ''' Note that this will return false if the argument list is empty.
    ''' </summary>
    Public ReadOnly Property AreAllArgsSet() As Boolean
        Get
            If Count = 0 Then Return False
            For Each arg As clsArgument In Me
                If arg.Value.IsNull Then Return False
            Next
            Return True
        End Get
    End Property

    ''' <summary>
    ''' Deserializes this object into the given context.
    ''' </summary>
    ''' <param name="info">The info to which this list should add itself.
    ''' </param>
    ''' <param name="context">The context of this serialization</param>
    Public Sub GetObjectData( _
     ByVal info As SerializationInfo, ByVal context As StreamingContext) _
     Implements ISerializable.GetObjectData
        info.AddValue("xmlAsInput", ArgumentsToXML(True), GetType(String))
    End Sub

    ''' <summary>
    ''' Serializes this argument list into XML using the given XML writer
    ''' </summary>
    ''' <param name="xw">The writer to which this argument list should be appended.
    ''' </param>
    ''' <param name="isOutput">True for an output list; False for an input list
    ''' </param>
    ''' <param name="action">Typically null, only populated with an action for COM
    ''' business objects; this forces a backwards compatible form of XML which
    ''' ensures that null arguments are processed accordingly</param>
    Public Sub ArgumentsToXML( _
     ByVal xw As XmlWriter, ByVal isOutput As Boolean, _
     Optional ByVal action As clsBusinessObjectAction = Nothing)
        Using s As New StringWriter()
            Using xr As New XmlTextWriter(s)

                If Not isOutput _
                 Then xw.WriteStartElement("inputs") _
                 Else xw.WriteStartElement("outputs")

                If action Is Nothing Then
                    'This is the normal way of doing things
                    For Each a As clsArgument In Me
                        a.ArgumentToXML(xw, isOutput, False)
                    Next
                Else
                    ' This allows backwards compatability with old binary business 
                    ' objects that don't have a concept of a null argument!
                    Dim dirn As ParamDirection
                    If isOutput _
                     Then dirn = ParamDirection.Out Else dirn = ParamDirection.In

                    For Each param As clsProcessParameter In action.GetParameters()
                        ' Only write args for parameters going the same direction that
                        ' this argument list is going (bug 5905).
                        If param.Direction <> dirn Then Continue For

                        Dim found As Boolean = False
                        For Each a As clsArgument In Me
                            If a.Name = param.Name Then
                                a.ArgumentToXML(xw, isOutput, False)
                                found = True
                                Exit For
                            End If
                        Next
                        If Not found Then CreateBlankArgument(
                         xw, param.Name, param.GetDataType(), isOutput)

                    Next

                End If

                xw.WriteEndElement() 'inputs|outputs

            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Turn arguments into xml. This is used by stages that have inputs and outputs 
    ''' such as actions and page references. It is also used by read and write stages
    ''' that need to log arguments.
    ''' </summary>
    ''' <param name="bOutput">Determines whether the arguments are outputs or not.
    ''' </param>
    ''' <returns>A well formatted XML string</returns>
    Public Function ArgumentsToXML(ByVal bOutput As Boolean, _
     Optional ByVal action As clsBusinessObjectAction = Nothing) As String
        Using s As New StringWriter()
            Using xr As New XmlTextWriter(s)
                ArgumentsToXML(xr, bOutput, action)
                Return s.ToString()
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Creates a blank argument for compatability with old binary business objects.
    ''' </summary>
    ''' <param name="xr">An xmlwriter to append the data to</param>
    ''' <param name="name">The name of the argument</param>
    ''' <param name="dtype">the datatype of the argument</param>
    ''' <param name="bOutput">whether the argument is an output or not</param>
    Private Sub CreateBlankArgument(ByVal xr As XmlWriter, ByVal name As String,
     ByVal dtype As DataType, ByVal bOutput As Boolean)
        If Not bOutput _
         Then xr.WriteStartElement("input") _
         Else xr.WriteStartElement("output")

        xr.WriteAttributeString("name", name)
        xr.WriteAttributeString("type", dtype.ToString())

        'A blank collection simply contains no row elements
        If dtype <> DataType.collection Then xr.WriteAttributeString("value", "")

        xr.WriteEndElement()
    End Sub

    ''' <summary>
    ''' Determines whether the given argument list is empty
    ''' </summary>
    ''' <param name="coll">the argument list to check</param>
    ''' <returns>True if empty, else False</returns>
    Public Shared Function IsEmpty(ByVal coll As clsArgumentList) As Boolean
        Return (coll Is Nothing OrElse coll.Count = 0)
    End Function

    ''' <summary>
    ''' This is a helper function used by internal business objects and webservices
    ''' to create an object model of the inputsxml or outputsxml
    ''' </summary>
    ''' <param name="argXml">The xml to convert</param>
    ''' <returns>A collection of clsArgument</returns>
    Public Shared Function XMLToArguments( _
     ByVal argXml As String, ByVal bOutput As Boolean) As clsArgumentList

        Dim argList As New clsArgumentList()
        If argXml = "" Then Return argList
        argList.Populate(argXml, bOutput)

        Return argList
    End Function

    ''' <summary>
    ''' Populate this argument list from the given XML.
    ''' Note that this should only be called internally, and only immediately
    ''' after the list is created. If called when the list is already populated,
    ''' strange things may occur
    ''' </summary>
    ''' <param name="sArgumentXML">The XML to load the argument list from.</param>
    ''' <param name="bOutput">Whether the arg list is for output arguments or
    ''' input arguments.</param>
    Private Sub Populate(ByVal sArgumentXML As String, ByVal bOutput As Boolean)

        Dim x As New ReadableXmlDocument(sArgumentXML)

        'Arguments are in the following syntax
        'For inputs:
        '<inputs><input name="" type="" value="" /></input>...</inputs>
        'For outputs:
        '<outputs><output name="" type="" value="" /></output>...</outputs>

        For Each xChild As XmlElement In x.ChildNodes
            If Not bOutput Then
                If Not xChild.Name = "inputs" Then
                    Throw New InvalidOperationException(My.Resources.Resources.clsArgumentList_InputsToTheProcessWereNotValid)
                End If
            Else
                If Not xChild.Name = "outputs" Then
                    Throw New InvalidOperationException(My.Resources.Resources.clsArgumentList_OutputsFromExternalProcessWereNotValid)
                End If
            End If

            For Each xGrandChild As Xml.XmlElement In xChild.ChildNodes
                If Not bOutput Then
                    If Not xGrandChild.Name = "input" Then
                        Throw New InvalidOperationException(My.Resources.Resources.clsArgumentList_InputToTheProcessWasNotValid)
                    End If
                Else
                    If Not xGrandChild.Name = "output" Then
                        Throw New InvalidOperationException(My.Resources.Resources.clsArgumentList_OutputFromExternalProcessWasNotValid)
                    End If
                End If

                Add(clsArgument.XMLToArgument(xGrandChild))
            Next
        Next

    End Sub


    ''' <summary>
    ''' Gets the first argument from the list with the given name
    ''' </summary>
    ''' <param name="name">The name of the argument to get</param>
    ''' <returns>The argument, or nothing if not found</returns>
    Public Function GetArgumentByName(ByVal name As String) As clsArgument
        For Each param As clsArgument In Me
            If param.Name = name Then
                Return param
            End If
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' Indicates that the output is not complete because the action is still 
    ''' running
    ''' </summary>
    Public Property IsRunning() As Boolean
        Get
            Return mIsRunning
        End Get
        Set(ByVal value As Boolean)
            mIsRunning = value
        End Set
    End Property

    ''' <summary>
    ''' Deep-clones this argument list returning the result.
    ''' </summary>
    ''' <returns>An argument list with different instances of the same arguments
    ''' defined within it.</returns>
    Private Function CloneObject() As Object Implements System.ICloneable.Clone
        Return Clone()
    End Function

    ''' <summary>
    ''' Deep-clones this argument list returning the result.
    ''' </summary>
    ''' <returns>An argument list with different instances of the same arguments
    ''' defined within it.</returns>
    Public Function Clone() As clsArgumentList
        ' Extending List<T> makes it very difficult to use Clone() properly,
        ' since it is not, in itself, cloneable
        Dim al As clsArgumentList = New clsArgumentList()
        For i As Integer = 0 To Me.Count - 1
            al.Add(Me(i).Clone())
        Next
        Return al
    End Function

End Class
