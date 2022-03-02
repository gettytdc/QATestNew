#If UNITTESTS Then
Imports System.Text
Imports BluePrism.AutomateAppCore.clsServerPartialClasses.Processes

Namespace DataContractRoundTrips.Generators


    Public Class ProcessDetailsTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim processDetailsTest = New ProcessDetails With
                    {
                    .Attributes = AutomateProcessCore.ProcessAttributes.Retired,
                    .LastModified = DateTime.UtcNow,
                    .Xml = Encoding.Unicode.GetBytes("the quick brown fox jumped over the lazy dog"),
                    .Zipped = True
                    }

            Yield Create("Standard", processDetailsTest)

        End Function
    End Class


End Namespace
#End If
