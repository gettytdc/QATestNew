Imports BluePrism.AutomateProcessCore

''' <summary>
''' Interface that defines methods session classes must expose to allow them to be 
''' viewed and modified in frmStartParams.
''' </summary>
Public Interface ISession : Inherits ICloneable, IEquatable(Of ISession)

    ''' <summary>
    ''' The ID of the process within the session
    ''' </summary>
    Property ProcessID() As Guid

    ''' <summary>
    ''' The name of the resource that the session will run on
    ''' </summary>
    ReadOnly Property ResourceName() As String

    ''' <summary>
    ''' The startup arguments for the session. This should never return null - if a
    ''' session has no startup arguments, this should return an empty arg list.
    ''' </summary>
    Property Arguments() As clsArgumentList

    ''' <summary>
    ''' The Parameters within the process for which arguments need to be set.
    ''' </summary>
    Function GetStartParameters() As List(Of clsProcessParameter)

End Interface
