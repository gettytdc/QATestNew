#If UNITTESTS Then
Imports BluePrism.AutomateAppCore.Auth

Namespace DataContractRoundTrips.Generators

    Public Class PasswordRulesTestCaseGenerator
        Inherits TestCaseGenerator

        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)

            Dim pwr1 As New PasswordRules() With {
                    .AdditionalCharacters = "/'",
                    .noRepeats = True,
                    .noRepeatsDays = False,
                    .numberOfRepeats = 3,
                    .numberOfDays = 0,
                    .PasswordExpiryInterval = 4,
                    .PasswordLength = 12,
                    .UseBrackets = True,
                    .UseDigits = True,
                    .UseLowerCase = True,
                    .UseSpecialCharacters = True,
                    .UseUpperCase = True}
            Yield Create("With null maxAttempts", pwr1)

            Dim pwr2 As New PasswordRules() With {
                    .MaxAttempts = 3,
                    .AdditionalCharacters = "/'",
                    .noRepeats = False,
                    .noRepeatsDays = True,
                    .numberOfRepeats = 0,
                    .numberOfDays = 30,
                    .PasswordExpiryInterval = 4,
                    .PasswordLength = 12,
                    .UseBrackets = True,
                    .UseDigits = True,
                    .UseLowerCase = True,
                    .UseSpecialCharacters = True,
                    .UseUpperCase = True}
            Yield Create("With not null maxAttempts", pwr2)

        End Function

    End Class

End Namespace
#End If
