Imports BluePrism.BPCoreLib
Imports BluePrism.Common.Security
Imports System.Runtime.Serialization
Imports BluePrism.Server.Domain.Models

Namespace Auth

    ''' <summary>
    ''' Class that holds Password rules loaded from the db, and also checks username 
    ''' and password rules against the currently loaded settings.
    ''' </summary>
    <Serializable()>
    <DataContract([Namespace]:="bp")>
    Public Class PasswordRules
        <DataMember>
        Public UseUpperCase As Boolean
        <DataMember>
        Public UseLowerCase As Boolean
        <DataMember>
        Public UseDigits As Boolean
        <DataMember>
        Public UseSpecialCharacters As Boolean
        <DataMember>
        Public UseBrackets As Boolean
        <DataMember>
        Public AdditionalCharacters As String
        <DataMember>
        Public PasswordLength As Integer
        <DataMember>
        Public noRepeats As Boolean
        <DataMember>
        Public noRepeatsDays As Boolean
        <DataMember>
        Public numberOfRepeats As Integer
        <DataMember>
        Public numberOfDays As Integer
        <DataMember>
        Public MaxAttempts As Nullable(Of Integer)
        <DataMember>
        Public PasswordExpiryInterval As Integer

        Private Const Uppercase As String = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
        Private Const Lowercase As String = "abcdefghijklmnopqrstuvwxyz"
        Private Const Digits As String = "0123456789"
        Private Const Special As String = "!""$%^&*_+=-:;@'~#,.?/\|`¬"
        Private Const Brackets As String = "()<>{}[]"


        Public Enum RuleViolation
            None
            Null
            Blank
            NonMatching
            TooShort
            NoUppercase
            NoLowercase
            NoDigits
            NoSpecial
            NoBrackets
            NoAdditional
        End Enum

        ''' <summary>
        ''' Checks password rules
        ''' </summary>
        ''' <param name="password">The password to check</param>
        ''' <param name="passwordConfirm">The confirmation of password field from the 
        ''' gui where the user has to enter thier password twice</param>
        ''' <exception cref="ArgumentNullException">
        ''' If <paramref name="password"/> is null.
        ''' </exception>
        Public Sub CheckPasswordRules(password As SafeString, passwordConfirm As SafeString)
            Select Case CheckPassword(password, passwordConfirm)
                Case RuleViolation.None
                    'Do nothing
                Case RuleViolation.Null
                    Throw New ArgumentNullException(NameOf(password),
                    My.Resources.PasswordRules_CannotHaveAnObfuscableSecureStringThatRepresentsANullString)
                Case RuleViolation.Blank
                    Throw New InvalidPasswordException(My.Resources.PasswordRules_PasswordCannotBeBlank)
                Case RuleViolation.NonMatching
                    Throw New InvalidPasswordException(My.Resources.PasswordRules_PasswordsDoNotMatch)
                Case RuleViolation.TooShort
                    Throw New InvalidPasswordException(String.Format(My.Resources.PasswordRules_PasswordIsShorterThanTheMinimumLengthOf0Characters, PasswordLength))
                Case RuleViolation.NoUppercase
                    Throw New InvalidPasswordException(My.Resources.PasswordRules_PasswordMustContainUppercaseCharacters)
                Case RuleViolation.NoLowercase
                    Throw New InvalidPasswordException(My.Resources.PasswordRules_PasswordMustContainLowercaseCharacters)
                Case RuleViolation.NoDigits
                    Throw New InvalidPasswordException(My.Resources.PasswordRules_PasswordMustContainDigitCharacters)
                Case RuleViolation.NoSpecial
                    Throw New InvalidPasswordException(My.Resources.PasswordRules_PasswordMustContainSpecialCharacters)
                Case RuleViolation.NoBrackets
                    Throw New InvalidPasswordException(My.Resources.PasswordRules_PasswordMustContainBrackets)
                Case RuleViolation.NoAdditional
                    Throw New InvalidPasswordException(My.Resources.PasswordRules_PasswordMustContainAtLeastOneOfTheFollowingCharacters & AdditionalCharacters)
            End Select

        End Sub

        Public Function CheckPassword(password As SafeString, passwordConfirm As SafeString) As RuleViolation

            If password Is Nothing Then Return _
                RuleViolation.Null

            If password.Length = 0 Then Return _
                RuleViolation.Blank

            'Create instances of the password secure strings, that will expose the
            'underlying chars and pin them in memory.
            Dim pinnedPassword As PinnedSecureString = Nothing
            Dim pinnedPasswordConfirm As PinnedSecureString = Nothing

            Try
                pinnedPassword = New PinnedSecureString(password)
                pinnedPasswordConfirm = New PinnedSecureString(passwordConfirm)

                If Not pinnedPassword.Equals(pinnedPasswordConfirm) Then _
                    Return RuleViolation.NonMatching

                If password.Length < PasswordLength Then _
                    Return RuleViolation.TooShort

                If UseUpperCase Then
                    Dim hasUppercase = False
                    For Each c As Char In Uppercase.ToCharArray
                        If pinnedPassword.Chars.Contains(c) Then
                            hasUppercase = True
                            Exit For
                        End If
                    Next
                    If Not hasUppercase Then
                        Return RuleViolation.NoUppercase
                    End If
                End If

                If UseLowerCase Then
                    Dim hasLowerCase = False
                    For Each c As Char In Lowercase.ToCharArray
                        If pinnedPassword.Chars.Contains(c) Then
                            hasLowerCase = True
                            Exit For
                        End If
                    Next
                    If Not hasLowerCase Then
                        Return RuleViolation.NoLowercase
                    End If
                End If

                If UseDigits Then
                    Dim hasDigits = False
                    For Each c As Char In Digits.ToCharArray
                        If pinnedPassword.Chars.Contains(c) Then
                            hasDigits = True
                            Exit For
                        End If
                    Next
                    If Not hasDigits Then
                        Return RuleViolation.NoDigits
                    End If
                End If

                If UseSpecialCharacters Then
                    Dim hasSpecialChars = False
                    For Each c As Char In Special.ToCharArray
                        If pinnedPassword.Chars.Contains(c) Then
                            hasSpecialChars = True
                            Exit For
                        End If
                    Next
                    If Not hasSpecialChars Then
                        Return RuleViolation.NoSpecial
                    End If
                End If

                If UseBrackets Then
                    Dim hasBrackets = False
                    For Each c As Char In Brackets.ToCharArray
                        If pinnedPassword.Chars.Contains(c) Then
                            hasBrackets = True
                            Exit For
                        End If
                    Next
                    If Not hasBrackets Then
                        Return RuleViolation.NoBrackets
                    End If
                End If

                If AdditionalCharacters <> String.Empty Then
                    Dim hasAdditionalChars = False
                    For Each c As Char In AdditionalCharacters.ToCharArray
                        If pinnedPassword.Chars.Contains(c) Then
                            hasAdditionalChars = True
                            Exit For
                        End If
                    Next
                    If Not hasAdditionalChars Then
                        Return RuleViolation.NoAdditional
                    End If
                End If

                Return RuleViolation.None
            Finally
                'Dispose so the passwords are zeroed in memory
                If pinnedPassword IsNot Nothing Then pinnedPassword.Dispose()
                If pinnedPasswordConfirm IsNot Nothing Then _
                    pinnedPasswordConfirm.Dispose()
            End Try

        End Function

        Public Overrides Function ToString() As String
            Dim result As String =
            My.Resources.PasswordRules_UseUpperCase &
            UseUpperCase.ToString &
            My.Resources.PasswordRules_UseLowerCase &
            UseLowerCase.ToString &
            My.Resources.PasswordRules_UseDigits &
            UseDigits.ToString &
            My.Resources.PasswordRules_UseSpecialCharacters &
            UseSpecialCharacters.ToString &
            My.Resources.PasswordRules_UseBrackets &
            UseBrackets.ToString &
            My.Resources.PasswordRules_AdditionalCharacters &
            AdditionalCharacters &
            My.Resources.PasswordRules_PasswordLength &
            PasswordLength.ToString

            If noRepeats Then
                result &= String.Format(My.Resources.PasswordRules_PasswordMayNotMatchMoreThan0Passwords, numberOfRepeats)
            End If
            If noRepeatsDays Then
                result &= String.Format(My.Resources.PasswordRules_PasswordMayNotMatchAPasswordUsedWithTheLast0Days, numberOfDays)
            End If

            If MaxAttempts.HasValue Then
                result &= My.Resources.PasswordRules_MaximumFailedAttempts & MaxAttempts
            End If

            If PasswordExpiryInterval > 0 Then
                result &= My.Resources.PasswordRules_ExpiryWarningDays & PasswordExpiryInterval
            End If

            Return result
        End Function
    End Class

End Namespace
