''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsVBOAction
''' 
''' <summary>
''' A class representing a particular action on a Business Object.
''' Instances of this class are retrieved via clsBusinessObject - once
''' retrieved, information such as the parameters, preconditions etc.
''' can be retrieved.
''' </summary>
Public Class clsVBOAction
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
    ''' Public accessor for precondition
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

End Class

