#If UNITTESTS Then
Namespace DataContractRoundTrips.Generators

    Public Class TagMaskTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim tagMask1 = New clsTagMask(Nothing)
            Yield Create("No tags", tagMask1)

            Dim tagMask2 = New clsTagMask("tag1;+tag2;-tag3;-tag4;-tag5")
            Yield Create("On and Off Tags", tagMask2)

        End Function
    End Class

End Namespace
#End If
