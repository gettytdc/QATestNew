#If UNITTESTS Then
Imports BluePrism.BPCoreLib.Data

Namespace DataContractRoundTrips.Generators

    Public Class ScheduleLogEntryTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim entry As New ScheduleLogEntry(ScheduleLogEventType.ScheduleTerminated,
                                              874, 1268, "Some termination reason")
            Yield Create("No Timestamp", entry)

            Dim entry2 As New ScheduleLogEntry(New MockScheduleEntryDataProvider)
            Yield Create("With timestamp", entry2)
        End Function


        ''' <summary>
        ''' Mock data provider for use in instantiating a schedule log entry object 
        ''' which includes a timestamp.
        ''' </summary>
        Private Class MockScheduleEntryDataProvider
            Implements IMultipleDataProvider

            Public Function GetGuid(name As String) As Guid Implements IDataProvider.GetGuid
                Throw New NotImplementedException
            End Function

            Public Function GetInt(name As String) As Integer Implements IDataProvider.GetInt
                Select Case name
                    Case "taskid"
                        Return 8745
                    Case "logsessionnumber"
                        Return 164
                    Case Else
                        Return Nothing
                End Select
            End Function

            Public Function GetInt(name As String, defaultValue As Integer) As Integer Implements IDataProvider.GetInt
                Select Case name
                    Case "taskid"
                        Return 8745
                    Case "logsessionnumber"
                        Return 164
                    Case Else
                        Return Nothing
                End Select
            End Function

            Public Function GetString(name As String) As String Implements IDataProvider.GetString
                Select Case name
                    Case "terminationreason"
                        Return "Some test termination reason"
                    Case "stacktrace"
                        Return "Some test stack trace string"
                    Case Else
                        Return String.Empty
                End Select
            End Function

            Public Function GetValue(Of T)(name As String, defaultValue As T) As T Implements IDataProvider.GetValue
                Dim obj As Object
                Select Case GetType(T)
                    Case GetType(String)
                        obj = GetString(name)
                    Case GetType(Integer)
                        obj = GetInt(name)
                    Case GetType(ScheduleLogEventType)
                        obj = ScheduleLogEventType.SessionFailedToStart
                    Case GetType(DateTime)
                        obj = DateTime.Today
                    Case Else
                        Return Nothing
                End Select
                Return CType(obj, T)
            End Function

            Public Function GetBool(name As String) As Boolean Implements IDataProvider.GetBool
                Return False
            End Function

            Default Public ReadOnly Property Item(name As String) As Object Implements IDataProvider.Item
                Get
                    Throw New NotImplementedException
                End Get
            End Property

            Public Function MoveNext() As Boolean Implements IMultipleDataProvider.MoveNext
                Return False
            End Function
        End Class
    End Class

End Namespace
#End If
