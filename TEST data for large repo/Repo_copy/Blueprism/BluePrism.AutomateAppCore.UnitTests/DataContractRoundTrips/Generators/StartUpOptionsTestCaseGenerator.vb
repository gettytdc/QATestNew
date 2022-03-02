#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Resources

Namespace DataContractRoundTrips.Generators
    Public Class StartUpOptionsTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim opt As New ResourcePCStartUpOptions() With {
                    .HTTPEnabled = False,
                    .IsAuto = True,
                    .IsLocal = False,
                    .IsLoginAgent = False,
                    .IsPublic = False,
                    .Port = 9100,
                    .SSLCertHash = "53cc2ab98f122d0779d428207f6417026ea6eb3d",
                    .Username = "TestUser",
                    .WebServiceAddressPrefix = "http://blueprismws.myorg.com"}

            Yield Create("Null", New ResourcePCStartUpOptions())

            Yield Create("Populated", opt)

        End Function

    End Class
End NameSpace
#End If
