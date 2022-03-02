Imports System.Runtime.Serialization

''' <summary>
''' Exception indicating that a part of the program has been reached which should
''' never actually be reached. Generally just used to indicate to the programmer that
''' the code is unreachably - obviously it should never actually be thrown
''' </summary>
<Serializable()> _
Public Class UnreachableException : Inherits BluePrismException

    ''' <summary>
    ''' Creates a new exception with no message
    ''' </summary>
    Public Sub New()
        MyBase.New(My.Resources.UnreachableException_ReachedASupposedlyUnreachablePartOfTheProgram)
    End Sub

    ''' <summary>
    ''' Creates a new exception from the given de-serializer
    ''' </summary>
    ''' <param name="info">The serialization info from which this exception should
    ''' draw its data.</param>
    ''' <param name="ctx">The context defining the context for the current
    ''' deserialization stream.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal ctx As StreamingContext)
        MyBase.New(info, ctx)
    End Sub


End Class
