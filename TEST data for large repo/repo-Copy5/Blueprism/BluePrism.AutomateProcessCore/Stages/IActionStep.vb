Imports System.Xml
Imports BluePrism.Server.Domain.Models
Imports BluePrism.ApplicationManager.AMI

''' <summary>
''' This interface is implemented by Step-like classes that have an AMI Action and
''' an associated list of arguments. You can see this kind of item represented in the
''' Automate UI as a list of steps, each potentially having a corresponding pane of
''' input arguments.
''' 
''' Examples are in the Naviate, Read and Wait stages. Because they're also in Wait
''' stages, which are not clsStep but clsChoice-based, the stuff implemented by this
''' interface cannot simply be part of clsStep.
''' </summary>
Public Interface IActionStep

    ''' <summary>
    ''' The values that have been assigned to arguments. A dictionary with the key
    ''' being the Argument ID, and the value being the value, in Automate-encoded
    ''' format, matching the datatype of the argument.
    ''' </summary>
    ReadOnly Property ArgumentValues() As IDictionary(Of String, String)

    ReadOnly Property OutputValues As IDictionary(Of String, String)

    ''' <summary>
    ''' The action to take against the target element. This also contains the
    ''' specification of the required arguments. Note that this could actually be
    ''' the child class clsConditionTypeInfo. Code dealing with this interface can
    ''' be expected to know when that matters, and cast it accordingly.
    ''' </summary>
    Property Action() As clsActionTypeInfo

    ''' <summary>
    ''' Gets the action ID for this step or null if no action has been set in this
    ''' step.
    ''' </summary>
    ReadOnly Property ActionId() As String

    ''' <summary>
    ''' The ID of the element that this step uses.
    ''' </summary>
    Property ElementId() As Guid

    ''' <summary>
    ''' Parameters to this step
    ''' </summary>
    ReadOnly Property Parameters() As List(Of clsApplicationElementParameter)

End Interface

''' <summary>
''' Helper functions to perform common IActionArgumentStep-related tasks.
''' </summary>
Friend Class clsActionStep

    ''' <summary>
    ''' Get a dictionary of argument names and values to be passed into an AMI
    ''' action (or condition).
    ''' </summary>
    ''' <param name="st">The IActionArgumentStep.</param>
    ''' <param name="stage">The stage it belongs to.</param>
    ''' <returns>The argument names and values, or Nothing if an error occurred.
    ''' </returns>
    ''' <exception cref="BluePrismException">If any error occurs.</exception>
    Public Shared Function GetArguments(
     ByVal st As IActionStep, ByVal stage As clsProcessStage) As Dictionary(Of String, String)

        Dim args As New Dictionary(Of String, String)

        For Each argName As String In st.ArgumentValues.Keys
            ' If this argument is not part of the action, ignore it
            If Not st.Action.Arguments.ContainsKey(argName) Then Continue For

            ' If the action has no argument info for it, ignore it (which of these
            ' two states indicates 'not part of the action'?)
            Dim argInfo As clsArgumentInfo = st.Action.Arguments(argName)
            If argInfo Is Nothing Then Continue For

            Dim dtype As DataType = clsProcessDataTypes.Parse(argInfo.DataType)

            Dim expr As String = st.ArgumentValues(argName)

            'Users are allowed to leave parameters blank, indicating a 'null' value
            'We don't pass on null values
            If expr = "" Then Continue For

            Dim sErr As String = Nothing

            Dim res As clsProcessValue = Nothing
            If Not clsExpression.EvaluateExpression(expr, res, stage, False, Nothing, sErr) Then
                Throw New BluePrismException(My.Resources.Resources.FailedToEvaluateArgument01,
                 argName, sErr)
            End If

            If res.DataType <> dtype Then
                Try
                    res = res.CastInto(dtype)
                Catch ex As BadCastException
                    Throw New BluePrismException(
                     My.Resources.Resources.clsActionStep_InvalidValuePassedToArgument0CouldNotCastResultOfExpressionToRequiredDataType1,
                     argInfo.Name, ex.Message)
                End Try
            End If

            args.Add(argName, res.EncodedValue)

        Next

        Return args

    End Function

    Public Shared Function GetOutputs(st As IActionStep, stage As clsProcessStage) As Dictionary(Of String, String)

        Dim outputs As New Dictionary(Of String, String)

        For Each outputName As String In st.OutputValues.Keys
            ' If this output is not part of the action, ignore it
            If Not st.Action.Outputs.ContainsKey(outputName) Then Continue For

            ' If the action has no argument info for it, ignore it (which of these
            ' two states indicates 'not part of the action'?)
            Dim argInfo As clsArgumentInfo = st.Action.Outputs(outputName)
            If argInfo Is Nothing Then Continue For

            Dim value As String = st.OutputValues(outputName)

            'Users are allowed to leave parameters blank, indicating a 'null' value
            'We don't pass on null values
            If value = String.Empty Then Continue For

            outputs.Add(outputName, value)

        Next

        Return outputs

    End Function


    ''' <summary>
    ''' Add arguments from the given "arguments" XML element.
    ''' </summary>
    ''' <param name="st">The IActionArgumentStep to add the arguments to.</param>
    ''' <param name="e">The 'root' XML element, which will generally be named
    ''' 'arguments' in the source document.</param>
    Public Shared Sub AddArgumentsFromXML(ByVal st As IActionStep, ByVal e As XmlElement)

        For Each el As XmlElement In e.ChildNodes
            If el.Name = "argument" Then
                Dim id = String.Empty, value = String.Empty
                For Each childElement As XmlElement In el.ChildNodes
                    GetXmlIdAndValue(childElement, id, value)
                Next
                st.ArgumentValues(id) = value
            End If
        Next

    End Sub

    Public Shared Sub AddOutputsFromXML(ByVal st As IActionStep, ByVal e As XmlElement)

        For Each el As XmlElement In e.ChildNodes
            If el.Name = "output" Then
                Dim id = String.Empty, value = String.Empty
                For Each childElement As XmlElement In el.ChildNodes
                    GetXmlIdAndValue(childElement, id, value)
                Next
                st.OutputValues(id) = value
            End If
        Next

    End Sub


    ''' <summary>
    ''' Generate XML for the argument values.
    ''' </summary>
    ''' <param name="st">The IActionArgumentStep to generate the XML for.</param>
    ''' <param name="doc">The XML document to generate into.</param>
    ''' <param name="el">The XML element to attach to.</param>
    Public Shared Sub ArgumentsToXML(ByVal st As IActionStep, ByVal doc As XmlDocument, ByVal el As XmlElement)

        Dim child2, child3, child4 As XmlElement
        If st.ArgumentValues.Count > 0 Then
            child2 = doc.CreateElement("arguments")
            For Each sArgID As String In st.ArgumentValues.Keys
                child4 = doc.CreateElement("argument")
                child3 = doc.CreateElement("id")
                child3.InnerText = sArgID
                child4.AppendChild(child3)
                child3 = doc.CreateElement("value")
                child3.InnerText = st.ArgumentValues.Item(sArgID)
                child4.AppendChild(child3)
                child2.AppendChild(child4)
            Next
            el.AppendChild(child2)
        End If

    End Sub

    Public Shared Sub OutputsToXML(ByVal st As IActionStep, ByVal doc As XmlDocument, ByVal el As XmlElement)

        Dim child2, child3, child4 As XmlElement
        If st.OutputValues.Count > 0 Then
            child2 = doc.CreateElement("outputs")
            For Each sArgID As String In st.OutputValues.Keys
                child4 = doc.CreateElement("output")
                child3 = doc.CreateElement("id")
                child3.InnerText = sArgID
                child4.AppendChild(child3)
                child3 = doc.CreateElement("value")
                child3.InnerText = st.OutputValues.Item(sArgID)
                child4.AppendChild(child3)
                child2.AppendChild(child4)
            Next
            el.AppendChild(child2)
        End If

    End Sub

    Private Shared Sub GetXmlIdAndValue(element As XmlElement, ByRef id As String, ByRef value As String)
        Select Case element.Name
            Case "id"
                id = element.FirstChild.Value
            Case "value"
                If Not element.FirstChild Is Nothing Then
                    value = element.FirstChild.Value
                Else
                    value = String.Empty
                End If
        End Select
    End Sub

End Class
