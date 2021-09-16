#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Auth

Namespace DataContractRoundTrips.Generators

    Public Class LogonOptionsTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim opt As New LogonOptions() With {
                    .AutoPopulate = AutoPopulateMode.SystemUser,
                    .ShowUserList = True,
                    .SingleSignon = True}

            Yield Create("Null", New LogonOptions())

            Yield Create("Populated", opt)

        End Function

    End Class

End Namespace
#End If
