#If UNITTESTS Then
Namespace DataContractRoundTrips.Generators

    Public Class ValidationInfoTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim info = New clsValidationInfo()
            With info
                .Enabled = True
                .CheckID = 3
                .TypeID = clsValidationInfo.Types.Warning
                .CatID = clsValidationInfo.Categories.DocumentationControl
                .Message = "Test message"
            End With

            Yield Create("Standard", info)

        End Function
    End Class
End Namespace
#End If
