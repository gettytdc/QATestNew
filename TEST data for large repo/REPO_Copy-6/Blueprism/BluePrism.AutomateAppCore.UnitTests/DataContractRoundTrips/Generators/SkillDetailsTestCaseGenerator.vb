#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Skills

Namespace DataContractRoundTrips.Generators
    Public Class SkillDetailsTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim category = CType(SkillCategory.VisualPerception, Integer).ToString()
            Dim mockObject = New SkillDetails("0", "Test Web Api", "Test Decrypted Skill", "oiaujsdoiajsdoad", category)

            Yield Create("Basic Object", mockObject)
        End Function
    End Class
End Namespace
#End If
