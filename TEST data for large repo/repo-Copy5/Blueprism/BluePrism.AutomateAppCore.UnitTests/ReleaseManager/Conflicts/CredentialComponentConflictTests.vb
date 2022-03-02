#If UNITTESTS Then
Imports System.Text
Imports BluePrism.AutomateProcessCore
Imports FluentAssertions
Imports NUnit.Framework

Namespace ReleaseManager.Conflicts

    ''' <summary>
    ''' Test class for Conflict Definitions from NewCredential method
    ''' </summary>
    <TestFixture()>
    Public Class CredentialComponentConflictTests

        ''' <summary>
        ''' A space character to be used in strings
        ''' </summary>
        Private Const mSpace As String = " "

        <Test()>
        Public Sub NewConflictDefinition_NullCredentialType_Throws()

            Dim newDefinitionAction As Action = Sub() CredentialComponent.MyConflicts.NewCredential(Nothing)

            newDefinitionAction.ShouldThrow(Of ArgumentNullException)

        End Sub
        
        <TestCaseSource("AllCredentialTypes")>
        Public Sub NewConflictDefinition_CredentialTypes_CorrectHintText(credentialType As CredentialType)

            Dim sut = CredentialComponent.MyConflicts.NewCredential(credentialType)
            'Dim credentialTypeLabelText = ComponentResources.New_Credential_Credential_Type_Template

            Dim textBuilder = New StringBuilder()
            textBuilder.Append(ComponentResources.New_Credential_Conflict_Text)
            textBuilder.Append(mSpace)
            textBuilder.Append(String.Format(ComponentResources.New_Credential_Credential_Type_Template, credentialType.LocalisedTitle))
            textBuilder.AppendLine()
            textBuilder.Append(credentialType.LocalisedDescription)

            sut.Text.Should.Be(textBuilder.ToString())
            sut.Hint.Should.Be(ComponentResources.New_Credential_Conflict_Hint)

        End Sub

        <TestCaseSource("AllCredentialTypes")>
        Public Sub NewConflictDefinition_CredentialTypes_OptionsHaveCorrectText(credentialType As CredentialType)

            Dim conflictDefinition = CredentialComponent.MyConflicts.NewCredential(credentialType)

            Dim extraInfoOption = conflictDefinition.Options.First(Function(o) o.Choice = ConflictOption.UserChoice.ExtraInfo)
            Dim skipOption = conflictDefinition.Options.First(Function(o) o.Choice = ConflictOption.UserChoice.Skip)

            extraInfoOption.Text.Should.Be(ComponentResources.New_Credential_Conflict_Option_Import)
            skipOption.Text.Should.Be(ComponentResources.New_Credential_conflict_Option_Dont_Import)

        End Sub

        <TestCaseSource("AllCredentialTypes")>
        Public Sub NewConflictDefinition_SetsConflictArgumentsBasedOnCredentialType(credentialType As CredentialType)

            Dim definition = CredentialComponent.MyConflicts.NewCredential(credentialType)

            Dim handler = definition.Options.
                    Single(Function(o) o.Choice = ConflictOption.UserChoice.ExtraInfo).
                    Handlers("CredentialDetails")


            Dim expectedPasswordArgument = New With { 
                    .Name = "Password", 
                    .Value = New clsProcessValue(DataType.password, ""),
                    .CustomTitle = credentialType.LocalisedPasswordPropertyTitle 
                    }
            Dim expectedArguments = { expectedPasswordArgument }.ToList()
            If credentialType.IsUsernameVisible Then
                Dim expectedUsernameArgument = New With { 
                        .Name = "Username", 
                        .Value = New clsProcessValue(DataType.text, ""),
                        .CustomTitle = credentialType.LocalisedUsernamePropertyTitle 
                        }
                expectedArguments.Insert(0, expectedUsernameArgument)
            End If
            handler.Arguments.ShouldAllBeEquivalentTo(expectedArguments, Function(options)options.ExcludingMissingMembers)

        End Sub

        Public Shared Function AllCredentialTypes As IEnumerable(Of CredentialType)
            Return CredentialType.GetAll
        End Function
    
    End Class

End NameSpace
#End If
