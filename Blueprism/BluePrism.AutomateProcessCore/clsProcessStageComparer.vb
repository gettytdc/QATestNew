Imports BluePrism.Server.Domain.Models

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsProcessStageSorter
''' 
''' <summary>
''' Compares two stage objects for sorting purposes.
''' </summary>
Public Class clsProcessStageComparer
    Implements IComparer


    ''' <summary>
    ''' Compares stages by ID and then by name. An exception is thrown if one of the
    ''' arguments passed is null, or if the types of the two parameters do not match.
    ''' one of the stages is null, or one of the objects passed is not of 
    ''' type clsprocessstage.
    ''' </summary>
    ''' <param name="x">First stage. May be a stage object or a string, provided
    ''' the type matches that of the second argument.</param>
    ''' <param name="y">Second stage. May be a stage object or a string, provided
    ''' the type matches that of the first argument.</param>
    ''' <returns>If two stages share the same ID then zero is returned.
    ''' Otherwise returns result of comparison between stage names. </returns>
    Public Function Compare(ByVal x As Object, ByVal y As Object) As Integer Implements System.Collections.IComparer.Compare

        If Not (x Is Nothing OrElse y Is Nothing) Then
            Select Case True
                Case TypeOf x Is clsProcessStage AndAlso TypeOf y Is clsProcessStage
                    If CType(x, clsProcessStage).GetStageID.Equals(CType(y, clsProcessStage).GetStageID) Then
                        Return 0
                    End If
                    Return CType(x, clsProcessStage).GetName.CompareTo(CType(y, clsProcessStage).GetName)

                Case TypeOf x Is String AndAlso TypeOf y Is String
                    Return CType(x, String).CompareTo(CType(y, String))

                Case Else
                    Throw New InvalidArgumentException(My.Resources.Resources.clsProcessStageComparer_BadValuesPassedToStageComparer)
            End Select
        Else
            Throw New InvalidArgumentException(My.Resources.Resources.clsProcessStageComparer_BadValuesPassedToStageComparer)
        End If
    End Function
End Class
