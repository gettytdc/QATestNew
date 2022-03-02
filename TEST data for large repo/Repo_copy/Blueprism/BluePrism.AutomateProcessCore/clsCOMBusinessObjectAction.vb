
Imports System.Xml

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsBusinessObjectAction
''' 
''' <summary>
''' A class representing a particular action on a Business Object.
''' Instances of this class are retrieved via clsBusinessObject - once
''' retrieved, information such as the parameters, preconditions etc.
''' can be retrieved.
''' </summary>
Public Class clsCOMBusinessObjectAction
    Inherits clsBusinessObjectAction

    ''' <summary>
    ''' The name of this action
    ''' </summary>
    Private msName As String

    ''' <summary>
    ''' The precondition of the action.
    ''' </summary>
    Private mcolPreconditions As Collection

    ''' <summary>
    ''' The endpoint of the action.
    ''' </summary>
    Private msEndpoint As String


    ''' <summary>
    ''' Public accessor for presondition
    ''' </summary>
    ''' <param name="sPrecondition"></param>
    Public Sub SetPrecondition(ByVal sPrecondition As String)
        mcolPreconditions.Add(sPrecondition)
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="sEndPoint"></param>
    Public Sub SetEndpoint(ByVal sEndPoint As String)
        msEndpoint = sEndPoint
    End Sub

    ''' <summary>
    ''' Get a description of the endpoint for the action.
    ''' </summary>
    ''' <returns>A string containing the endpoint description</returns>
    Public Overrides Function GetEndpoint() As String
        If msEndpoint Is Nothing Then Return ""
        Return msEndpoint
    End Function

    ''' <summary>
    ''' Get a description of the preconditions for the action.
    ''' </summary>
    ''' <returns>A Collection of strings, each describing an precondition
    ''' </returns>
    Public Overrides Function GetPreConditions() As Collection
        Return mcolPreconditions
    End Function

    Public Sub New()
        MyBase.New()
        mcolPreconditions = New Collection
    End Sub

    Public Function SetParamsFromXML(ByVal rootel As XmlElement, ByVal bOutput As Boolean, ByRef sErr As String) As Boolean
        Dim sDirection As String = ""
        Try
            Dim curel As XmlElement
            If bOutput Then
                sDirection = "output"
            Else
                sDirection = "input"
            End If
            Dim sDir = CStr(IIf(sDirection.Equals("output"), "outputs", "inputs"))

            If rootel.Name <> sDir Then
                sErr = String.Format(My.Resources.Resources.clsCOMBusinessObjectAction_Missing0, sDir)
                Return False
            End If

            For Each curel In rootel.ChildNodes
                Dim param As New clsProcessParameter()
                Select Case curel.Name
                    Case "input"
                        param.Direction = ParamDirection.In
                    Case "output"
                        param.Direction = ParamDirection.Out
                    Case Else
                        param.Direction = ParamDirection.None
                End Select

                param.Name = curel.GetAttribute("name")
                param.FriendlyName = curel.GetAttribute("friendlyname")
                param.SetDataType(clsProcessDataTypes.Parse(curel.GetAttribute("type")))

                If param.GetDataType = DataType.collection Then
                    SetCollectionInfo(curel, bOutput, param)
                End If
                param.Narrative = curel.GetAttribute("narrative")
                mParameters.Add(param)
            Next
        Catch e As XmlException
            sErr = String.Format(CStr(IIf(sDirection.Equals("output"), My.Resources.Resources.clsCOMBusinessObjectAction_ErrorInOutputs0, My.Resources.Resources.clsCOMBusinessObjectAction_ErrorInInputs0)), e.Message)
            Return False
        Catch
            sErr = CStr(IIf(sDirection.Equals("output"), My.Resources.Resources.clsCOMBusinessObjectAction_FailedToProcessOutputs, My.Resources.Resources.clsCOMBusinessObjectAction_FailedToProcessInputs))
            Return False
        End Try

        Return True
    End Function

    Private Sub SetCollectionInfo(ByVal el As XmlElement, ByVal bOutput As Boolean, ByVal objParam As clsProcessParameter)
        Dim bDefined As Boolean = False
        For Each el2 As XmlElement In el.ChildNodes
            If el2.Name = "row" Then
                For Each el3 As XmlElement In el2.ChildNodes
                    If el3.Name = "field" Then
                        objParam.CollectionInfo.AddField(el3.GetAttribute("name"), clsProcessDataTypes.DataTypeId(el3.GetAttribute("type")))
                        bDefined = True
                    End If
                Next
            ElseIf el2.Name = "field" Then
                'This is here to deal with badly formed capabilities XML
                'e.g <output type="collection" ><field name="....
                'instead of:
            '<output type="collection" ><row><field name="....
            'the collection definitions in the xml do not need to have rows.
            objParam.CollectionInfo.AddField(el2.GetAttribute("name"), clsProcessDataTypes.DataTypeId(el2.GetAttribute("type")))
                bDefined = True
            Else
                bDefined = False
            End If
        Next


    End Sub

End Class
