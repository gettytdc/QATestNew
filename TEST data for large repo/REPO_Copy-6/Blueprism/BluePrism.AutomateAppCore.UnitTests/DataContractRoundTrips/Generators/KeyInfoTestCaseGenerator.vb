#If UNITTESTS Then
Imports System.Xml.Linq
Imports BluePrism.BPCoreLib

Namespace DataContractRoundTrips.Generators


    Public Class KeyInfoTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim lic As XElement = <license>
                                      <type>none</type>
                                      <licensee>Test</licensee>
                                      <maxprocesses>100</maxprocesses>
                                      <maxresources>10</maxresources>
                                      <maxconcurrentsessions>10</maxconcurrentsessions>
                                      <maxprocessalerts>10</maxprocessalerts>
                                      <starts>2010-01-01</starts>
                                      <expires>2099-01-01</expires>
                                      <transactionmodel>true</transactionmodel>
                                  </license>

            Dim key = New KeyInfo(lic.ToString, DateTime.Now, Guid.NewGuid())
            TestCaseGenerator.SetField(key, "mId", 100)

            Yield Create("Standard", key)

            Yield CreateWithCustomState("Standard - testing ID", key,
                                        Function(a) TestCaseGenerator.GetField(a, "mId"))

        End Function

    End Class

End Namespace
#End If
