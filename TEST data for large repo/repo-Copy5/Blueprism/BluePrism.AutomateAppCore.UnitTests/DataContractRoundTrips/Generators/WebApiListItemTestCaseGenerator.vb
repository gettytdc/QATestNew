#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore.WebApis

Namespace DataContractRoundTrips.Generators

    Public Class WebApiListItemTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim webApiList = New WebApiListItem
            webApiList.Enabled = True
            webApiList.Name = "A Friendly Web API Name"
            webApiList.Id = Guid.NewGuid
            webApiList.NumberOfActions = 3
            webApiList.LastUpdated = DateTime.Now

            Yield Create("Standard", webApiList)
        End Function
    End Class
End Namespace
#End If
