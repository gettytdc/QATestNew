Imports System.Runtime.Serialization
Imports System.Xml

Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data
Imports BluePrism.BPCoreLib.Collections

Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Processes

Imports BluePrism.AutomateAppCore.Auth
Imports BluePrism.AutomateAppCore.ConflictOption

Imports ProcessType = BluePrism.AutomateProcessCore.Processes.DiagramType
Imports ProcessAttributes = BluePrism.AutomateProcessCore.ProcessAttributes

Imports BluePrism.AutomateAppCore.Groups
Imports BluePrism.Core.Extensions
Imports BluePrism.Server.Domain.Models

''' <summary>
''' Component representing a process.
''' </summary>
<Serializable, DataContract(IsReference:=True, [Namespace]:="bp")>
Public Class ProcessComponent : Inherits RetirableComponent

#Region " Conflict Definitions "

#Region " Definition Generation "

    ''' <summary>
    ''' Generates an IdClash definition for the given component type.
    ''' This just allows the text to be changed in one place to change the conflict
    ''' text for both processes and business objects.
    ''' </summary>
    ''' <param name="type">The type of package component to generate the conflict
    ''' definition for. Should be either <see cref="PackageComponentType.Process"/>
    ''' or <see cref="PackageComponentType.BusinessObject"/></param>
    ''' <returns>The conflict definition for the specified type.</returns>
    Protected Shared Function GenerateIdClashDefinition(
     type As PackageComponentType) As ConflictDefinition
        Dim typeLabel As String = PackageComponentType.GetLocalizedFriendlyName(type)

        Select Case type
            Case PackageComponentType.BusinessObject
                Return New ConflictDefinition(String.Format("{0}IdClash", type.Key),
                                              My.Resources.ProcessComponent_ABusinessObjectWithTheSameInternalIDAlreadyExistsInTheDatabase,
                                              My.Resources.ProcessComponent_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                              New ConflictOption(UserChoice.Overwrite, GetConflictMessageResource(type)),
                                              New ConflictOption(UserChoice.NewId, My.Resources.ProcessComponent_AssignANewInternalIDToTheIncomingBusinessObject),
                                              New ConflictOption(UserChoice.Skip,
                                                                 My.Resources.ProcessComponent_DonTImportThisBusinessObject)) _
                    With {.DefaultInteractiveResolution = UserChoice.Overwrite,
                        .DefaultNonInteractiveResolution = UserChoice.Overwrite}
            Case PackageComponentType.Process
                Return New ConflictDefinition(String.Format("{0}IdClash", type.Key),
                                              My.Resources.ProcessComponent_AProcessWithTheSameInternalIDAlreadyExistsInTheDatabase,
                                              My.Resources.ProcessComponent_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                              New ConflictOption(UserChoice.Overwrite, GetConflictMessageResource(type)),
                                              New ConflictOption(UserChoice.NewId, My.Resources.ProcessComponent_AssignANewInternalIDToTheIncomingProcess),
                                              New ConflictOption(UserChoice.Skip,
                                                                 My.Resources.ProcessComponent_DonTImportThisProcess)) _
                    With {.DefaultInteractiveResolution = UserChoice.Overwrite,
                        .DefaultNonInteractiveResolution = UserChoice.Overwrite}
            Case Else
                Return New ConflictDefinition(String.Format("{0}IdClash", type.Key),
                                              String.Format(My.Resources.ProcessComponent_A0WithTheSameInternalIDAlreadyExistsInTheDatabase, typeLabel),
                                              My.Resources.ProcessComponent_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                              New ConflictOption(UserChoice.Overwrite, String.Format(GetConflictMessageResource(type), typeLabel)),
                                              New ConflictOption(UserChoice.NewId, String.Format(My.Resources.ProcessComponent_AssignANewInternalIDToTheIncoming0, typeLabel)),
                                              New ConflictOption(UserChoice.Skip,
                                                                 String.Format(My.Resources.ProcessComponent_DonTImportThis0, typeLabel))) _
                    With {.DefaultInteractiveResolution = UserChoice.Overwrite,
                        .DefaultNonInteractiveResolution = UserChoice.Overwrite}
        End Select


    End Function

    ''' <summary>
    ''' Generates an ID clash with another type (i.e. if the ID of an incoming
    ''' process clashes with the ID of an existing Object)
    ''' </summary>
    ''' <param name="thisType">This type</param>
    ''' <param name="clashType">The type clashed with</param>
    ''' <returns>The conflict definition</returns>
    Protected Shared Function GenerateIdClashWithTypeDefinition(
     thisType As PackageComponentType, clashType As PackageComponentType) As ConflictDefinition
        Dim typeLabel As String = PackageComponentType.GetLocalizedFriendlyName(thisType)

        Dim clashDefinition = GetClashDefinition(clashType, thisType)
        Dim descriptionText As String
        Dim newIdText As String
        Dim skipText As String

        Select Case clashDefinition
            Case ClashWithTypeDefinition.ProcessProcess
                descriptionText = My.Resources.ProcessComponent_AProcessWithTheSameInternalIDAsThisProcessAlreadyExistsInTheDatabase
                newIdText = My.Resources.ProcessComponent_AssignANewInternalIDToTheIncomingProcess
                skipText = My.Resources.ProcessComponent_DonTImportThisProcess
            Case ClashWithTypeDefinition.ProcessBusinessObject
                descriptionText = My.Resources.ProcessComponent_AProcessWithTheSameInternalIDAsThisBusinessObjectAlreadyExistsInTheDatabase
                newIdText = My.Resources.ProcessComponent_AssignANewInternalIDToTheIncomingProcess
                skipText = My.Resources.ProcessComponent_DonTImportThisProcess
            Case ClashWithTypeDefinition.BusinessObjectBusinessObject
                descriptionText = My.Resources.ProcessComponent_ABusinessObjectWithTheSameInternalIDAsThisBusinessObjectAlreadyExistsInTheDatabase
                newIdText = My.Resources.ProcessComponent_AssignANewInternalIDToTheIncomingBusinessObject
                skipText = My.Resources.ProcessComponent_DonTImportThisBusinessObject
            Case ClashWithTypeDefinition.BusinessObjectProcess
                descriptionText = My.Resources.ProcessComponent_ABusinessObjectWithTheSameInternalIDAsThisProcessAlreadyExistsInTheDatabase
                newIdText = My.Resources.ProcessComponent_AssignANewInternalIDToTheIncomingBusinessObject
                skipText = My.Resources.ProcessComponent_DonTImportThisBusinessObject
            Case Else
                descriptionText = String.Format(My.Resources.ProcessComponent_A0WithTheSameInternalIDAsThis1AlreadyExistsInTheDatabase,
                    PackageComponentType.GetLocalizedFriendlyName(clashType),
                    typeLabel)
                newIdText = String.Format(My.Resources.ProcessComponent_AssignANewIDToTheIncoming0, typeLabel)
                skipText = String.Format(My.Resources.ProcessComponent_DonTImportThis0, typeLabel)
        End Select

        Return New ConflictDefinition(String.Format("{0}IdClash", clashType.Key),
                                      descriptionText,
                                      My.Resources.ProcessComponent_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict,
                                      New ConflictOption(UserChoice.NewId, newIdText),
                                      New ConflictOption(UserChoice.Skip, skipText)
                                      ) With {.DefaultInteractiveResolution = UserChoice.NewId,
                                              .DefaultNonInteractiveResolution = UserChoice.Fail}

    End Function

    ''' <summary>
    ''' Generates an NameClash definition for the given component type.
    ''' This just allows the text to be changed in one place to change the conflict
    ''' text for both processes and business objects.
    ''' </summary>
    ''' <param name="type">The type of package component to generate the conflict
    ''' definition for. Should be either <see cref="PackageComponentType.Process"/>
    ''' or <see cref="PackageComponentType.BusinessObject"/></param>
    ''' <returns>The conflict definition for the specified type.</returns>
    Protected Shared Function GenerateNameClashDefinition(
     type As PackageComponentType) As ConflictDefinition
        Dim typeLabel As String = PackageComponentType.GetLocalizedFriendlyName(type)

        Return New ConflictDefinition(String.Format("{0}NameClash", type.Key), _
 _
         String.Format(
          My.Resources.ProcessComponent_A0WithTheSameNameAlreadyExistsInTheDatabase,
          typeLabel), _
 _
         My.Resources.ProcessComponent_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict, _
 _
         New ConflictOption(UserChoice.Overwrite, String.Format(GetConflictMessageResource(type), typeLabel)), _
 _
         New ConflictOption(UserChoice.Rename,
          String.Format(My.Resources.ProcessComponent_RenameTheIncoming0, typeLabel),
          New ConflictDataHandler("NewName",
           String.Format(My.Resources.ProcessComponent_PleaseEnterANewNameForThe0, typeLabel),
           New ConflictArgument($"{type.Label} Name", "", String.Format(My.Resources.ProcessComponent_0Name, PackageComponentType.GetLocalizedFriendlyName(type)).ToSentenceCase))), _
 _
         New ConflictOption(UserChoice.RenameExisting,
          String.Format(My.Resources.ProcessComponent_RenameTheExisting0, typeLabel),
          New ConflictDataHandler("NewName",
           String.Format(My.Resources.ProcessComponent_PleaseEnterANewNameForThe0, typeLabel),
           New ConflictArgument($"{type.Label} Name", "", String.Format(My.Resources.ProcessComponent_0Name, PackageComponentType.GetLocalizedFriendlyName(type)).ToSentenceCase))), _
 _
         New ConflictOption(UserChoice.Skip,
          String.Format(My.Resources.ProcessComponent_DonTImportThis0, typeLabel))
        ) With {.DefaultNonInteractiveResolution = UserChoice.Overwrite}

    End Function

    ''' <summary>
    ''' Generates a name clash with another type (i.e. if the name of an incoming
    ''' process clashes with the name of an existing Object)
    ''' </summary>
    ''' <param name="thisType">This type</param>
    ''' <param name="clashType">The type clashed with</param>
    ''' <returns>The conflict definition</returns>
    Protected Shared Function GenerateNameClashWithTypeDefinition(
     thisType As PackageComponentType, clashType As PackageComponentType) As ConflictDefinition
        Dim typeLabel As String = PackageComponentType.GetLocalizedFriendlyName(thisType)
        Dim clashTypeLabel As String = PackageComponentType.GetLocalizedFriendlyName(clashType)

        Return New ConflictDefinition(String.Format("{0}NameClash", clashType.Key), _
 _
         String.Format(
          My.Resources.ProcessComponent_A0WithTheSameNameAsThis1AlreadyExistsInTheDatabase,
          clashTypeLabel, typeLabel), _
 _
         My.Resources.ProcessComponent_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict, _
 _
         New ConflictOption(UserChoice.Rename,
          String.Format(My.Resources.ProcessComponent_RenameTheIncoming0, typeLabel),
          New ConflictDataHandler("NewName",
           String.Format(My.Resources.ProcessComponent_PleaseEnterANewNameForThe0, typeLabel),
           New ConflictArgument($"{thisType.Label} Name", "", String.Format(My.Resources.ProcessComponent_0Name, PackageComponentType.GetLocalizedFriendlyName(thisType)).ToSentenceCase())
         )), _
 _
         New ConflictOption(UserChoice.RenameExisting,
          String.Format(My.Resources.ProcessComponent_RenameTheExisting0, clashTypeLabel),
          New ConflictDataHandler("NewName",
           String.Format(My.Resources.ProcessComponent_PleaseEnterANewNameForThe0, clashTypeLabel),
           New ConflictArgument($"{clashType.Label} Name", "", String.Format(My.Resources.ProcessComponent_0Name, PackageComponentType.GetLocalizedFriendlyName(clashType)).ToSentenceCase()))), _
 _
         New ConflictOption(UserChoice.Skip,
          String.Format(My.Resources.ProcessComponent_DonTImportThis0, typeLabel))
        ) With {.DefaultNonInteractiveResolution = UserChoice.Fail}


    End Function

    ''' <summary>
    ''' Generates an ID and name clash with same type (i.e. if the ID and name of an
    ''' incoming process clashes with the ID and name of an existing Process)
    ''' </summary>
    ''' <param name="type">This type</param>
    ''' <returns>The conflict definition</returns>
    Protected Shared Function GenerateIDClash_NameClashDefinition(
     type As PackageComponentType) As ConflictDefinition

        Dim typeName = PackageComponentType.GetLocalizedFriendlyName(type)

        Return New ConflictDefinition(String.Format("{0}ID_{0}NameClash", type.Key), _
 _
         String.Format(
          My.Resources.ProcessComponent_A0WithTheSameInternalIDAndNameAlreadyExistsInTheDatabase,
          typeName), _
 _
         My.Resources.ProcessComponent_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict, _
 _
         New ConflictOption(UserChoice.Overwrite, String.Format(GetConflictMessageResource(type), typeName)), _
 _
         New ConflictOption(UserChoice.NewId,
          String.Format(My.Resources.ProcessComponent_AssignANewIDAndRenameTheIncoming0, typeName),
          New ConflictDataHandler("NewName",
           String.Format(My.Resources.ProcessComponent_PleaseEnterANewNameForThe0, typeName),
           New ConflictArgument($"{type.Label} Name", "", String.Format(My.Resources.ProcessComponent_0Name, PackageComponentType.GetLocalizedFriendlyName(type)).ToSentenceCase()))), _
 _
         New ConflictOption(UserChoice.Skip,
          String.Format(My.Resources.ProcessComponent_DonTImportThis0, typeName))
        ) With {.DefaultInteractiveResolution = UserChoice.Overwrite,
                .DefaultNonInteractiveResolution = UserChoice.Overwrite}

    End Function

    Private Shared Function GetConflictMessageResource(type As PackageComponentType) As String
        Dim labelText = String.Empty
        Select Case type
            Case PackageComponentType.BusinessObject
                labelText = My.Resources.OverwriteTheExistingBusinessObjectWithTheIncomingBusinessObject
            Case PackageComponentType.Process
                labelText = My.Resources.OverwriteTheExistingProcessWithTheIncomingProcess
            Case Else
                labelText = My.Resources.ProcessComponent_OverwriteTheExisting0WithTheIncoming0
        End Select
        Return labelText
    End Function

    ''' <summary>
    ''' Generates an ID (same type) and name (other type) clash (i.e. if the ID
    ''' of an incoming Process clashes with the ID of an existing Process, AND the
    ''' name of the incoming Process clashes with the name of an existing Object).
    ''' </summary>
    ''' <param name="thisType">This type</param>
    ''' <param name="clashType">The type clashed with</param>
    ''' <returns>The conflict definition</returns>
    Protected Shared Function GenerateIDClash_NameClashWithType(
     thisType As PackageComponentType, clashType As PackageComponentType) As ConflictDefinition
        Dim typeLabel As String = PackageComponentType.GetLocalizedFriendlyName(thisType)

        Return New ConflictDefinition(String.Format("{0}ID_{1}NameClash", thisType.Key, clashType.Key), _
 _
         String.Format(
          My.Resources.ProcessComponent_A0WithTheSameInternalIDAndA1WithTheSameNameAsThis0AlreadyExistInTheDatabase,
          typeLabel, PackageComponentType.GetLocalizedFriendlyName(clashType)), _
 _
         My.Resources.ProcessComponent_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict, _
 _
         New ConflictOption(UserChoice.NewId,
          String.Format(My.Resources.ProcessComponent_AssignANewIDAndRenameTheIncoming0, typeLabel),
          New ConflictDataHandler("NewName",
           String.Format(My.Resources.ProcessComponent_PleaseEnterANewNameForThe0, typeLabel),
           New ConflictArgument($"{thisType.Label} Name", "", String.Format(My.Resources.ProcessComponent_0Name, PackageComponentType.GetLocalizedFriendlyName(thisType)).ToSentenceCase()))), _
 _
         New ConflictOption(UserChoice.Skip,
          String.Format(My.Resources.ProcessComponent_DonTImportThis0, typeLabel))
        ) With {.DefaultNonInteractiveResolution = UserChoice.Fail}

    End Function

    ''' <summary>
    ''' Generates an ID (other type) and name (same type) clash (i.e. if the ID
    ''' of an incoming Process clashes with the ID of an existing Object, AND the
    ''' name of the incoming Process clashes with the name of an existing Process).
    ''' </summary>
    ''' <param name="thisType">This type</param>
    ''' <param name="clashType">The type clashed with</param>
    ''' <returns>The conflict definition</returns>
    Protected Shared Function GenerateIDClashWithType_NameClash(
     thisType As PackageComponentType, clashType As PackageComponentType) As ConflictDefinition
        Dim typeLabel As String = PackageComponentType.GetLocalizedFriendlyName(thisType)

        Return New ConflictDefinition(String.Format("{0}ID_{1}NameClash", clashType.Key, thisType.Key), _
 _
         String.Format(
          My.Resources.ProcessComponent_A0WithTheSameInternalIDAndA1WithTheSameNameAsThis1AlreadyExistInTheDatabase,
          PackageComponentType.GetLocalizedFriendlyName(clashType), typeLabel), _
 _
         My.Resources.ProcessComponent_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict, _
 _
         New ConflictOption(UserChoice.NewId,
          String.Format(My.Resources.ProcessComponent_AssignANewIDAndRenameTheIncoming0, typeLabel),
          New ConflictDataHandler("NewName",
           String.Format(My.Resources.ProcessComponent_PleaseEnterANewNameForThe0, typeLabel),
           New ConflictArgument($"{thisType.Label} Name", "", String.Format(My.Resources.ProcessComponent_0Name, PackageComponentType.GetLocalizedFriendlyName(thisType)).ToSentenceCase()))), _
 _
         New ConflictOption(UserChoice.Skip,
          String.Format(My.Resources.ProcessComponent_DonTImportThis0, typeLabel))
        ) With {.DefaultNonInteractiveResolution = UserChoice.Fail}

    End Function

    ''' <summary>
    ''' Generates an ID and name clash with another type (i.e. if the ID and name of
    ''' an incoming Process clashes with the ID and name of an existing Object)
    ''' </summary>
    ''' <param name="thisType">This type</param>
    ''' <param name="clashType">The type clashed with</param>
    ''' <returns>The conflict definition</returns>
    Protected Shared Function GenerateIDClashWithType_NameClashWithType(
     thisType As PackageComponentType, clashType As PackageComponentType) As ConflictDefinition
        Dim typeLabel As String = PackageComponentType.GetLocalizedFriendlyName(thisType)

        Return New ConflictDefinition(String.Format("{0}ID_{1}NameClash", clashType.Key, clashType.Key), _
 _
         String.Format(
          My.Resources.ProcessComponent_A0WithTheSameInternalIDAndNameAsThis1AlreadyExistsInTheDatabase,
          PackageComponentType.GetLocalizedFriendlyName(clashType), typeLabel), _
 _
         My.Resources.ProcessComponent_PleaseChooseOneOfTheFollowingWaysToResolveThisConflict, _
 _
         New ConflictOption(UserChoice.NewId,
          String.Format(My.Resources.ProcessComponent_AssignANewIDAndRenameTheIncoming0, typeLabel),
          New ConflictDataHandler("NewName",
           String.Format(My.Resources.ProcessComponent_PleaseEnterANewNameForThe0, typeLabel),
           New ConflictArgument($"{thisType.Label} Name", "", String.Format(My.Resources.ProcessComponent_0Name, PackageComponentType.GetLocalizedFriendlyName(thisType)).ToSentenceCase()))), _
 _
         New ConflictOption(UserChoice.Skip,
          String.Format(My.Resources.ProcessComponent_DonTImportThis0, typeLabel))
        ) With {.DefaultNonInteractiveResolution = UserChoice.Fail}

    End Function

    ''' <summary>
    ''' Generates an ToBeRetired definition for the given component type.
    ''' This just allows the text to be changed in one place to change the conflict
    ''' text for both processes and business objects.
    ''' </summary>
    ''' <param name="type">The type of package component to generate the conflict
    ''' definition for. Should be either <see cref="PackageComponentType.Process"/>
    ''' or <see cref="PackageComponentType.BusinessObject"/></param>
    ''' <returns>The conflict definition for the specified type.</returns>
    Protected Shared Function GenerateToBeRetiredDefinition(
     ByVal type As PackageComponentType) As ConflictDefinition
        Dim typeLabel As String = PackageComponentType.GetLocalizedFriendlyName(type)

        Return New ConflictDefinition(
         String.Format("{0}ToBeRetired", type.Label), _
 _
         String.Format(My.Resources.ProcessComponent_TheIncoming0HasBeenRetired, typeLabel), _
 _
         String.Format(My.Resources.ProcessComponent_DoYouWantToRetireThe0InThisDatabaseAfterImporting,
         typeLabel), _
 _
         New ConflictOption(UserChoice.PerformPostProcess, My.Resources.ProcessComponent_Yes),
         New ConflictOption(UserChoice.OmitPostProcess, My.Resources.ProcessComponent_No)
        ) With {.DefaultInteractiveResolution = UserChoice.PerformPostProcess,
                .DefaultNonInteractiveResolution = UserChoice.PerformPostProcess}

    End Function

    ''' <summary>
    ''' Generates an ToBePublished definition for the given component type.
    ''' This just allows the text to be changed in one place to change the conflict
    ''' text for both processes and business objects.
    ''' </summary>
    ''' <param name="type">The type of package component to generate the conflict
    ''' definition for. Should be either <see cref="PackageComponentType.Process"/>
    ''' or <see cref="PackageComponentType.BusinessObject"/></param>
    ''' <returns>The conflict definition for the specified type.</returns>
    Protected Shared Function GenerateToBePublishedDefinition(
     ByVal type As PackageComponentType) As ConflictDefinition
        Dim typeLabel = PackageComponentType.GetLocalizedFriendlyName(type)

        Return New ConflictDefinition(
         String.Format("{0}ToBePublished", type.Label), _
 _
         String.Format(My.Resources.ProcessComponent_TheIncoming0HasBeenPublished, typeLabel), _
 _
         String.Format(My.Resources.ProcessComponent_DoYouWantToPublishThe0InThisDatabaseAfterImporting,
         typeLabel), _
 _
         New ConflictOption(UserChoice.PerformPostProcess, My.Resources.ProcessComponent_Yes),
         New ConflictOption(UserChoice.OmitPostProcess, My.Resources.ProcessComponent_No)
        ) With {.DefaultInteractiveResolution = UserChoice.PerformPostProcess,
                .DefaultNonInteractiveResolution = UserChoice.PerformPostProcess}

    End Function

#End Region

    ''' <summary>
    ''' The conflict definitions which are handled by the process component.
    ''' </summary>
    Private Class MyConflicts

        ''' <summary>
        ''' Conflict which occurs when a process with the same ID as the incoming
        ''' ID is detected in the target environment.
        ''' Usually, this would overwrite the existing process with the new version
        ''' </summary>
        Public Shared ReadOnly Property ProcessIdClash As ConflictDefinition
            Get
                If mProcessIdClash Is Nothing Then
                    mProcessIdClash = GenerateIdClashDefinition(PackageComponentType.Process)
                Else
                    mProcessIdClash.UpdateConflictDefinitionStrings(GenerateIdClashDefinition(PackageComponentType.Process))
                End If
                Return mProcessIdClash
            End Get
        End Property

        Private Shared mProcessIdClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when an object with the same ID as the incoming
        ''' ID is detected in the target environment.
        ''' </summary>
        Public Shared ReadOnly Property VBOIDClash As ConflictDefinition
            Get
                If mVBOIDClash Is Nothing Then
                    mVBOIDClash = GenerateIdClashWithTypeDefinition(PackageComponentType.Process, PackageComponentType.BusinessObject)
                Else
                    mVBOIDClash.UpdateConflictDefinitionStrings(GenerateIdClashWithTypeDefinition(PackageComponentType.Process, PackageComponentType.BusinessObject))
                End If
                Return mVBOIDClash
            End Get
        End Property

        Private Shared mVBOIDClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when a process <em>with a different ID</em>, but
        ''' with the same name as the incoming process is discovered in the target
        ''' environment.
        ''' Usually, this would result in a rename, either of the existing process
        ''' or of the incoming one. Overwriting will cause the incoming process to
        ''' assume the ID of the existing process.
        ''' </summary>
        Public Shared ReadOnly Property ProcessNameClash As ConflictDefinition
            Get
                If mProcessNameClash Is Nothing Then
                    mProcessNameClash = GenerateNameClashDefinition(PackageComponentType.Process)
                Else
                    mProcessNameClash.UpdateConflictDefinitionStrings(GenerateNameClashDefinition(PackageComponentType.Process))
                End If
                Return mProcessNameClash
            End Get
        End Property

        Private Shared mProcessNameClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when an object <em>with a different ID</em>, but
        ''' with the same name as the incoming process is detected in the target
        ''' environment.
        ''' </summary>
        Public Shared ReadOnly Property VBONameClash As ConflictDefinition
            Get
                If mVBONameClash Is Nothing Then
                    mVBONameClash = GenerateNameClashWithTypeDefinition(PackageComponentType.Process, PackageComponentType.BusinessObject)
                Else
                    mVBONameClash.UpdateConflictDefinitionStrings(GenerateNameClashWithTypeDefinition(PackageComponentType.Process, PackageComponentType.BusinessObject))
                End If
                Return mVBONameClash
            End Get
        End Property

        Private Shared mVBONameClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when a process <em>with the same ID</em>, and
        ''' with the same name as the incoming process is detected in the target
        ''' environment.
        ''' </summary>
        Public Shared ReadOnly Property ProcessID_ProcessNameClash As ConflictDefinition
            Get
                If mProcessID_ProcessNameClash Is Nothing Then
                    mProcessID_ProcessNameClash = GenerateIDClash_NameClashDefinition(PackageComponentType.Process)
                Else
                    mProcessID_ProcessNameClash.UpdateConflictDefinitionStrings(GenerateIDClash_NameClashDefinition(PackageComponentType.Process))
                End If
                Return mProcessID_ProcessNameClash
            End Get
        End Property

        Private Shared mProcessID_ProcessNameClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when a process <em>with the same ID</em>, and
        ''' an object with the same name as the incoming process is detected in the
        ''' target environment.
        ''' </summary>
        Public Shared ReadOnly Property ProcessID_VBONameClash As ConflictDefinition
            Get
                If mProcessID_VBONameClash Is Nothing Then
                    mProcessID_VBONameClash = GenerateIDClash_NameClashWithType(PackageComponentType.Process, PackageComponentType.BusinessObject)
                Else
                    mProcessID_VBONameClash.UpdateConflictDefinitionStrings(GenerateIDClash_NameClashWithType(PackageComponentType.Process, PackageComponentType.BusinessObject))
                End If
                Return mProcessID_VBONameClash
            End Get
        End Property

        Private Shared mProcessID_VBONameClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when an object <em>with the same ID</em>, and
        ''' a process with the same name as the incoming process is detected in the
        ''' target environment.
        ''' </summary>
        Public Shared ReadOnly Property VBOID_ProcessNameClash As ConflictDefinition
            Get
                If mVBOID_ProcessNameClash Is Nothing Then
                    mVBOID_ProcessNameClash = GenerateIDClashWithType_NameClash(PackageComponentType.Process,
                                                                           PackageComponentType.BusinessObject)
                Else
                    mVBOID_ProcessNameClash.UpdateConflictDefinitionStrings(GenerateIDClashWithType_NameClash(PackageComponentType.Process, PackageComponentType.BusinessObject))
                End If
                Return mVBOID_ProcessNameClash
            End Get
        End Property

        Private Shared mVBOID_ProcessNameClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when an object <em>with the same ID</em>, and
        ''' with the same name as the incoming process is detected in the target
        ''' environment.
        ''' </summary>
        Public Shared ReadOnly Property VBOID_VBONameClash As ConflictDefinition
            Get
                If mVBOID_VBONameClash Is Nothing Then
                    mVBOID_VBONameClash = GenerateIDClashWithType_NameClashWithType(PackageComponentType.Process, PackageComponentType.BusinessObject)
                Else
                    mVBOID_VBONameClash.UpdateConflictDefinitionStrings(GenerateIDClashWithType_NameClashWithType(PackageComponentType.Process, PackageComponentType.BusinessObject))
                End If
                Return mVBOID_VBONameClash
            End Get
        End Property

        Private Shared mVBOID_VBONameClash As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when the incoming process is set to be retired,
        ''' and the corresponding process (ie. that with the same ID) is not
        ''' retired, or does not exist.
        ''' Usually, you would want all processes in the two environments to match
        ''' up, hence 'Yes' being the recommended option.
        ''' </summary>
        Public Shared ReadOnly Property ProcessToBeRetired As ConflictDefinition
            Get
                If mProcessToBeRetired Is Nothing Then
                    mProcessToBeRetired = GenerateToBeRetiredDefinition(PackageComponentType.Process)
                Else
                    mProcessToBeRetired.UpdateConflictDefinitionStrings(GenerateToBeRetiredDefinition(PackageComponentType.Process))
                End If
                Return mProcessToBeRetired
            End Get
        End Property

        Private Shared mProcessToBeRetired As ConflictDefinition

        ''' <summary>
        ''' Conflict which occurs when the incoming process is set to be published,
        ''' and the corresponding process is not published, or there is no
        ''' corresponding process.
        ''' Usually, you would want all processes in the two environments to match
        ''' up, hence 'Yes' being the recommended option.
        ''' </summary>
        Public Shared ReadOnly Property ProcessToBePublished As ConflictDefinition
            Get
                If mProcessToBePublished Is Nothing Then
                    mProcessToBePublished = GenerateToBePublishedDefinition(PackageComponentType.Process)
                Else
                    mProcessToBePublished.UpdateConflictDefinitionStrings(GenerateToBePublishedDefinition(PackageComponentType.Process))
                End If
                Return mProcessToBePublished
            End Get
        End Property

        Private Shared mProcessToBePublished As ConflictDefinition
    End Class

#End Region

#Region " ProcessWrapper Class "

    ''' <summary>
    ''' Process wrapper object - clsProcess is not serializable, and it would be a
    ''' mammoth undertaking to make it so - rather than that, this just wraps the
    ''' XML and attributes (the two primary things needed to import a process), and
    ''' will generate a clsProcess object as required.
    ''' </summary>
    <Serializable, DataContract(IsReference:=True)>
    Friend Class ProcessWrapper


        ' The process, lazily created by the Process property.
        <NonSerialized()>
        Private mCachedProcess As clsProcess


        ' The process component containing this process wrapper.
        <DataMember>
        Private mOuter As ProcessComponent

        ' The XML of the process being wrapped
        <DataMember>
        Private mXml As String

        ''' <summary>
        ''' Creates a new process wrapper for the given component, made up of the
        ''' given XML. The process ID and attributes are gleaned from the owning
        ''' process component when necessary.
        ''' </summary>
        ''' <param name="outer">The component for which this wrapper is required.
        ''' </param>
        ''' <param name="xml">The XML representing the process.</param>
        Public Sub New(ByVal outer As ProcessComponent, ByVal xml As String)
            mOuter = outer
            mXml = xml
        End Sub

        ''' <summary>
        ''' Gets the process object represented by this wrapper.
        ''' </summary>
        Public ReadOnly Property Process() As clsProcess
            Get
                If mCachedProcess IsNot Nothing Then Return mCachedProcess

                Dim proc As clsProcess = clsProcess.FromXML(clsGroupObjectDetails.Empty, mXml, True, "")
                If proc Is Nothing Then Return Nothing

                proc.Id = mOuter.IdAsGuid
                proc.Attributes = mOuter.Attributes
                Return proc
            End Get
        End Property

    End Class

#End Region

#Region " Member variables "

    ' Flag indicating if this process is published or not
    <DataMember>
    Private mPublished As Boolean

    ' The process ID of the existing process with the same name as this process
    <DataMember>
    Private mExistingProcessWithSameNameId As Guid

    Private mOriginalId As Guid

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new process data object using data from the given provider.
    ''' </summary>
    ''' <param name="prov">The provider from which the data should be drawn. This
    ''' expects the entries :-<list>
    ''' <item>id : Guid</item>
    ''' <item>name : String</item>
    ''' <item>attributeid : Integer</item>
    ''' </list></param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal prov As IDataProvider)
        Me.New(owner, prov.GetValue("id", Guid.Empty), prov.GetString("name"))
        Dim attrs As Integer = prov.GetValue("attributeid", 0)
        Me.Retired = ((attrs And ProcessAttributes.Retired) <> 0)
        Me.Published = ((attrs And ProcessAttributes.Published) <> 0)
    End Sub

    ''' <summary>
    ''' Creates a new process data object using the given process object.
    ''' </summary>
    ''' <param name="proc">The process to use for this data object.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal proc As clsProcess)
        Me.New(owner, proc.Id, proc.Name)
        Me.Retired = ((proc.Attributes And ProcessAttributes.Retired) <> 0)
        Me.Published = ((proc.Attributes And ProcessAttributes.Published) <> 0)
    End Sub

    ''' <summary>
    ''' Creates a new process component for the process with the given ID.
    ''' This will retrieve the process details from the database immediately
    ''' </summary>
    ''' <param name="id">The process ID to use for this process component.</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal id As Guid)
        MyBase.New(owner, id, gSv.GetProcessNameByID(id))
        OriginalId = id
    End Sub

    ''' <summary>
    ''' Creates a new process data object using the given id and name.
    ''' </summary>
    ''' <param name="id">The process ID that this object should represent.</param>
    ''' <param name="name">The name of the process</param>
    Public Sub New(ByVal owner As OwnerComponent, ByVal id As Guid, ByVal name As String)
        MyBase.New(owner, id, name)
        OriginalId = id
    End Sub

    ''' <summary>
    ''' Creates a new process component which draws its data from the given XML
    ''' reader.
    ''' </summary>
    ''' <param name="reader">The reader whence to draw the process data.</param>
    ''' <param name="ctx">The loading context for the XML reading</param>
    Public Sub New(ByVal owner As OwnerComponent,
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.New(owner, reader, ctx)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Flag to indicate whether this component should always be in a group
    ''' </summary>
    Public Overrides ReadOnly Property AlwaysInGroup() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' The type of this component.
    ''' </summary>
    Public Overrides ReadOnly Property Type() As PackageComponentType
        Get
            Return PackageComponentType.Process
        End Get
    End Property

    ''' <summary>
    ''' The type that this component can clash with.
    ''' </summary>
    Public Overridable ReadOnly Property ClashType() As PackageComponentType
        Get
            Return PackageComponentType.BusinessObject
        End Get
    End Property

    ''' <summary>
    ''' Whether the process represented by this object is published or not.
    ''' </summary>
    Public Property Published() As Boolean
        Get
            Return mPublished
        End Get
        Set(ByVal value As Boolean)
            mPublished = value
        End Set
    End Property

    ''' <summary>
    ''' Checks if this process component represents a business object or not
    ''' </summary>
    Public Overridable ReadOnly Property IsBusinessObject() As Boolean
        Get
            Return False
        End Get
    End Property

    ''' <summary>
    ''' The Process object associated with this component.
    ''' </summary>
    Public Property AssociatedProcess() As clsProcess
        Get
            Dim wrapper As ProcessWrapper = TryCast(AssociatedData, ProcessWrapper)
            If wrapper Is Nothing Then Return Nothing Else Return wrapper.Process
        End Get
        Set(ByVal value As clsProcess)
            If value Is Nothing Then AssociatedData = Nothing : Return
            AssociatedData = New ProcessWrapper(Me, value.GenerateXML(False))
            Me.Attributes = value.Attributes
        End Set
    End Property

    ''' <summary>
    ''' The process attributes currently in this process component.
    ''' </summary>
    Public Property Attributes() As ProcessAttributes
        Get
            Dim attrs As ProcessAttributes = ProcessAttributes.None
            If Me.Published Then attrs = attrs Or ProcessAttributes.Published
            If Me.Retired Then attrs = attrs Or ProcessAttributes.Retired
            Return attrs
        End Get
        Set(ByVal value As ProcessAttributes)
            Me.Published = ((value And ProcessAttributes.Published) <> 0)
            Me.Retired = ((value And ProcessAttributes.Retired) <> 0)
        End Set
    End Property

    <DataMember>
    Public Property Dependencies() As clsProcessDependencyList

    ''' <summary>
    ''' Gets the name of the permission required by a user to import a component of
    ''' this type.
    ''' </summary>
    Public Overrides ReadOnly Property ImportPermission() As String
        Get
            Return Permission.ProcessStudio.ImportProcess
        End Get
    End Property

    ''' <summary>
    ''' The original (on import) ID of this component
    ''' </summary>
    <DataMember>
    Public Property OriginalId As Guid
        Get
            Return mOriginalId
        End Get
        Private Set(value As Guid)
            mOriginalId = value
        End Set
    End Property

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Creates a ProcessData object from the given provider, after determining
    ''' whether it should be a ProcessData or the descendent ObjectData class.
    ''' </summary>
    ''' <param name="prov">The data provider giving the data for the process / object
    ''' data object. This expects the following fields :-<list>
    ''' <item>id: Guid</item>
    ''' <item>name: String</item>
    ''' <item>processtype: String ("P" or "O")</item>
    ''' <item>attributeid: Integer (corresponding to ProcessAttributes)
    ''' </item>
    ''' </list></param>
    ''' <returns>A ProcessComponent or a VBOComponent as appropriate, populated
    ''' from the given provider.</returns>
    Public Shared Function Create(ByVal owner As OwnerComponent, ByVal prov As IDataProvider) As ProcessComponent
        If prov.GetString("processtype") = "O" Then
            Return New VBOComponent(owner, prov)
        Else
            Return New ProcessComponent(owner, prov)
        End If
    End Function

    ''' <summary>
    ''' Creates a ProcessData object from the given provider, after determining
    ''' whether it should be a ProcessData or the descendent ObjectData class.
    ''' </summary>
    ''' <param name="proc">The process object for which a process component is
    ''' required.</param>
    ''' <returns>A ProcessComponent or a VBOComponent as appropriate, populated
    ''' from the given provider.</returns>
    Public Shared Function Create(ByVal owner As OwnerComponent, ByVal proc As clsProcess) As ProcessComponent
        If proc.ProcessType = DiagramType.Object Then
            Return New VBOComponent(owner, proc)
        Else
            Return New ProcessComponent(owner, proc)
        End If
    End Function

    ''' <summary>
    ''' Loads the database data for this component. Note that this has a side effect
    ''' of setting the <see cref="Published"/> and <see cref="Retired"/> properties
    ''' of this component with the associated data from the database.
    ''' </summary>
    ''' <returns>The data associated with this component.</returns>
    Protected Overrides Function LoadData() As Object
        Me.Attributes = gSv.GetProcessAttributes(IdAsGuid)
        Return New ProcessWrapper(Me, gSv.GetProcessXML(IdAsGuid))
    End Function

    ''' <summary>
    ''' A very simplistic comparison method, which just checks if the exportable data
    ''' in the given component differs from the data in this component.
    ''' </summary>
    ''' <param name="comp">The component to check against.</param>
    ''' <returns>True if the given component differs from this component. False if
    ''' its data is identical.</returns>
    Public Overrides Function Differs(ByVal comp As PackageComponent) As Boolean

        ' If any base stuff differs, then we don't need to even check.
        If MyBase.Differs(comp) Then Return True

        Dim pcomp As ProcessComponent = DirectCast(comp, ProcessComponent)

        ' Nice simple checks first
        If Me.IsBusinessObject <> pcomp.IsBusinessObject OrElse Me.Retired <> pcomp.Retired _
         OrElse Me.Published <> pcomp.Published Then Return True

        Dim mine As clsProcess = AssociatedProcess
        Dim theirs As clsProcess = pcomp.AssociatedProcess

        ' We're going to have to go broadbrush with this.
        Return (mine.GenerateXML(False) <> theirs.GenerateXML(False))

    End Function

    ''' <summary>
    ''' Returns any group assignments for the object/process represented by this
    ''' component.
    ''' </summary>
    Public Overrides Function GetGroupInfo() As IDictionary(Of Guid, String)
        Dim mem As GroupMember
        If IsBusinessObject Then
            mem = New ObjectGroupMember()
        Else
            mem = New ProcessGroupMember()
        End If
        mem.Id = Id
        Return gSv.GetPathsToMember(mem)
    End Function

#End Region

#Region " Conflict/Resolution Handling "

#Region " Conflict Definition Properties "
    ''' <summary>
    ''' The single instance of an IdClash conflict definition for this process.
    ''' This is an ID clash with another process.
    ''' </summary>
    Protected Overridable ReadOnly Property IdClash() As ConflictDefinition
        Get
            Return MyConflicts.ProcessIdClash
        End Get
    End Property

    ''' <summary>
    ''' The single instance of an IdClashWithOtherType conflict definition for this
    ''' process. This is an ID clash with a VBO.
    ''' </summary>
    Protected Overridable ReadOnly Property IdTypeClash() As ConflictDefinition
        Get
            Return MyConflicts.VBOIDClash
        End Get
    End Property

    ''' <summary>
    ''' The single instance of an NameClash conflict definition for this process.
    ''' This is an ID clash with another process.
    ''' </summary>
    Protected Overridable ReadOnly Property NameClash() As ConflictDefinition
        Get
            Return MyConflicts.ProcessNameClash
        End Get
    End Property

    ''' <summary>
    ''' The single instance of an NameClashWithOtherType conflict definition for this
    ''' process. This is an ID clash with a VBO.
    ''' </summary>
    Protected Overridable ReadOnly Property NameTypeClash() As ConflictDefinition
        Get
            Return MyConflicts.VBONameClash
        End Get
    End Property

    Protected Overridable ReadOnly Property IdNameClash() As ConflictDefinition
        Get
            Return MyConflicts.ProcessID_ProcessNameClash
        End Get
    End Property

    Protected Overridable ReadOnly Property IdNameTypeClash() As ConflictDefinition
        Get
            Return MyConflicts.ProcessID_VBONameClash
        End Get
    End Property

    Protected Overridable ReadOnly Property IdTypeNameClash As ConflictDefinition
        Get
            Return MyConflicts.VBOID_ProcessNameClash
        End Get
    End Property

    Protected Overridable ReadOnly Property IdTypeNameTypeClash As ConflictDefinition
        Get
            Return MyConflicts.VBOID_VBONameClash
        End Get
    End Property

    ''' <summary>
    ''' The single instance of an ToBeRetired conflict definition for this object
    ''' </summary>
    Protected Overridable ReadOnly Property ToBeRetired() As ConflictDefinition
        Get
            Return MyConflicts.ProcessToBeRetired
        End Get
    End Property

    ''' <summary>
    ''' The single instance of an ToBePublished conflict definition for this object
    ''' </summary>
    Protected Overridable ReadOnly Property ToBePublished() As ConflictDefinition
        Get
            Return MyConflicts.ProcessToBePublished
        End Get
    End Property

#End Region

    ''' <summary>
    ''' Returns true if the passed process type is the same as the current process
    ''' represented by this component.
    ''' </summary>
    Private Function SameProcessType(tp As ProcessType) As Boolean
        If (tp = DiagramType.Object AndAlso IsBusinessObject()) OrElse
         (tp = DiagramType.Process AndAlso Not IsBusinessObject()) _
            Then Return True _
            Else Return False
    End Function

    ''' <summary>
    ''' Gets collisions between this component and the current database.
    ''' </summary>
    ''' <returns>A collection of collision types which will occur when importing this
    ''' component into the database.</returns>
    Protected Overrides Function FindConflicts() As ICollection(Of ConflictDefinition)
        ' Start from scratch, so ensure 'same name' ID is blanked
        mExistingProcessWithSameNameId = Nothing

        Dim conflicts As New List(Of ConflictDefinition)

        'Determine if this ID is in use
        Dim existingTypeForID As ProcessType = DiagramType.Unset
        gSv.GetProcessInfo(Me.IdAsGuid, Nothing, Nothing, Nothing, Nothing, existingTypeForID)

        'Determine if this name is in use
        Dim existingIDForName As Guid = gSv.GetProcessIDByName(Me.Name, True)
        Dim existingTypeForName As ProcessType = DiagramType.Unset
        If existingIDForName <> Guid.Empty Then
            gSv.GetProcessInfo(existingIDForName, Nothing, Nothing, Nothing, Nothing, existingTypeForName)
        End If

        If existingTypeForID <> DiagramType.Unset AndAlso existingIDForName = Guid.Empty Then
            'ID match only
            If SameProcessType(existingTypeForID) _
                Then conflicts.Add(IdClash) _
                Else conflicts.Add(IdTypeClash)
        ElseIf existingTypeForID = DiagramType.Unset AndAlso existingIDForName <> Guid.Empty Then
            'Name match only
            mExistingProcessWithSameNameId = existingIDForName
            If SameProcessType(existingTypeForName) _
                Then conflicts.Add(NameClash) _
                Else conflicts.Add(NameTypeClash)
        ElseIf existingTypeForID <> DiagramType.Unset AndAlso existingIDForName <> Guid.Empty Then
            'ID and name matches
            If existingTypeForID = existingTypeForName Then
                If SameProcessType(existingTypeForID) _
                    Then conflicts.Add(IdNameClash) _
                    Else conflicts.Add(IdTypeNameTypeClash)
            Else
                If SameProcessType(existingTypeForID) _
                    Then conflicts.Add(IdNameTypeClash) _
                    Else conflicts.Add(IdTypeNameClash)
            End If
        End If

        ' If this process is neither retired nor published, we don't need to worry
        ' about whether it currently is in the environment.
        If Retired Or Published Then
            Dim attrs As ProcessAttributes = ProcessAttributes.None
            Try
                attrs = gSv.GetProcessAttributes(IdAsGuid)
            Catch nsee As NoSuchElementException
                ' Treat 'process missing' as 'process doesn't have attributes' for
                ' the purpose of these conflicts
            End Try
            If Me.Retired AndAlso (attrs And ProcessAttributes.Retired) = 0 Then
                conflicts.Add(ToBeRetired)
            End If
            If Me.Published AndAlso (attrs And ProcessAttributes.Published) = 0 Then
                conflicts.Add(ToBePublished)
            End If
        End If
        Return conflicts
    End Function

    ''' <summary>
    ''' Applies the given conflict resolutions to all processes in the given release.
    ''' This cannot be an instance method since it must check across all processes
    ''' (and potentially VBOs) rather than just checking the single process data.
    ''' As such, this method is primarily tasked with cross-component checking - any
    ''' single-component validation is handled in the <see cref="ApplyResolutions"/>
    ''' instance method - which this method calls before it performs all the
    ''' necessary cross-component validation.
    ''' </summary>
    ''' <param name="rel">The release to which conflict resolutions are being applied
    ''' </param>
    ''' <param name="resolutions">The resolutions to apply</param>
    ''' <param name="errors">The error log to which any errors are appended. If this
    ''' method does not append any errors, then the processes in the given release
    ''' are considered 'clean' and configured to be submitted to the database</param>
    Private Shared Sub ApplyConflictResolutions(ByVal rel As clsRelease,
     ByVal resolutions As ICollection(Of ConflictResolution),
     ByVal errors As clsErrorLog)

        ' Let's get all the single-component validation out of the way first...
        For Each comp As PackageComponent In rel
            Dim proc As ProcessComponent = TryCast(comp, ProcessComponent)
            If proc IsNot Nothing Then proc.ApplyResolutions(resolutions, errors)
        Next

        ' Now deal with the cross-component validation

        ' Build up a matrix of suggested names for the processes
        Dim map As New Dictionary(Of ProcessComponent, KeyValuePair(Of Boolean, String))

        ' Also a map of new names (that is renames) against whether they are renaming
        ' the incoming process (true) or the existing process (false)
        Dim renamingIncoming As New Dictionary(Of ProcessComponent, Boolean)

        ' First get them from the release... set the key to 'True' to indicate
        ' that this is the original name
        For Each comp As PackageComponent In rel
            Dim proc As ProcessComponent = TryCast(comp, ProcessComponent)
            If proc IsNot Nothing Then
                map(proc) = New KeyValuePair(Of Boolean, String)(True, proc.Name)
            End If
        Next

        ' Now override any original names from the conflicts
        For Each res As ConflictResolution In resolutions
            Dim proc As ProcessComponent = TryCast(res.Conflict.Component, ProcessComponent)
            If proc IsNot Nothing Then
                Dim newName As String =
                 res.GetArgumentString(String.Format("{0} Name", proc.Type.Label))
                If newName <> "" Then
                    ' The key becomes 'False' to indicate not the original name
                    map(proc) = New KeyValuePair(Of Boolean, String)(False, newName)
                    renamingIncoming(proc) = (res.ConflictDataHandler.Id = "RenameIncoming")
                End If
            End If
        Next

        ' Okay the map now contains <original-flag>:<name> mapped against each component.
        ' We now need to check if any of these names are the same, and add errors as
        ' appropriate.
        ' First we test against processes which aren't having their name changed
        Dim originalNames As New clsSet(Of String)

        ' While we're doing that record the list of process components changing to
        ' each name, for later checks - it should be 1:1, if it's 1:n where n>1,
        ' then that's an error reported below
        Dim renames As New clsGeneratorDictionary(Of String, List(Of ProcessComponent))

        ' Store all the original (ie. not being renamed) names
        For Each pair As KeyValuePair(Of Boolean, String) In map.Values
            If pair.Key Then originalNames.Add(pair.Value)
        Next

        ' Check no renames clash with processes with the same (original) name
        For Each comp As ProcessComponent In map.Keys
            Dim pair As KeyValuePair(Of Boolean, String) = map(comp)
            If pair.Key Then Continue For
            renames(pair.Value).Add(comp)
            If originalNames.Contains(pair.Value) Then
                errors.Add(comp,
                 My.Resources.ProcessComponent_The01CannotBeRenamedTo2ThereIsAProcessBusinessObjectInThisReleaseWithThatName,
                 comp.TypeKey, comp.Name, pair.Value)
            End If
        Next

        ' Finally, cross-check the proposed names to ensure that two processes aren't
        ' being renamed to the same thing
        For Each pair As KeyValuePair(Of String, List(Of ProcessComponent)) In renames
            If pair.Value.Count > 1 Then
                Dim sb As New StringBuilder()
                sb.AppendFormat(
                 My.Resources.ProcessComponent_0ProcessesObjectsAreSetToBeRenamedTo1,
                 pair.Value.Count, pair.Key)

                ' Find the incoming ones and the existing ones and enumerate them into the
                ' string builder for the error message.
                Dim incoming As New List(Of ProcessComponent)
                Dim existing As New List(Of ProcessComponent)
                For Each proc As ProcessComponent In pair.Value
                    If renamingIncoming(proc) Then incoming.Add(proc) Else existing.Add(proc)
                Next
                If incoming.Count > 0 Then
                    sb.Append(My.Resources.ProcessComponent_IncomingProcesses)
                    For i As Integer = 0 To incoming.Count - 1
                        sb.Append(incoming(i).Name)
                        If i < incoming.Count - 1 Then sb.Append(", ")
                    Next
                    sb.Append("; ")
                End If
                If existing.Count > 0 Then
                    sb.Append(My.Resources.ProcessComponent_ExistingProcesses)
                    For i As Integer = 0 To existing.Count - 1
                        sb.Append(existing(i).Name)
                        If i < existing.Count - 1 Then sb.Append(", ")
                    Next
                End If
                errors.Add(pair.Value, sb.ToString())
            End If
        Next

    End Sub

    ''' <summary>
    ''' Gets the delegate responsible for applying the conflict resolutions.
    ''' </summary>
    Public Overrides ReadOnly Property ResolutionApplier() As Conflict.Resolver
        Get
            Return AddressOf ApplyConflictResolutions
        End Get
    End Property

    ''' <summary>
    ''' Applies the given conflict resolution to this component.
    ''' </summary>
    ''' <param name="res">The (single) resolution to apply.</param>
    ''' <param name="mods">The dictionary of modifications to set in this component
    ''' ready for the database to act upon.</param>
    ''' <returns>The error message caused by an invalid resolution of a conflict,
    ''' or null if there was no error.</returns>
    Private Function ApplyResolution(
     ByVal res As ConflictResolution, ByVal mods As IDictionary(Of ModificationType, Object)) _
     As clsError

        ' The definition of that conflict
        Dim defn As ConflictDefinition = res.Conflict.Definition

        ' The resolution option chosen (may be null if no option is chosen)
        Dim opt As ConflictOption = res.ConflictOption

        ' If the option is non-null, that's a programming error, it should have
        ' been checked by now.
        If opt Is Nothing Then Throw New ArgumentNullException("res.ConflictOption")

        ' The proposed name - null if not applicable
        Dim proposedName As String = res.GetArgumentString(String.Format("{0} Name", IIf(defn Is NameTypeClash, ClashType.Label, Type.Label)))

        If proposedName Is Nothing And opt.Choice = UserChoice.Rename Then
            proposedName = res.GetArgumentString(String.Format("{0} Name", IIf(defn Is NameTypeClash, Type.Label, ClashType.Label)))

        End If


        Dim typeLabel As String = PackageComponentType.GetLocalizedFriendlyName(Type)
        Dim clashTypeLabel As String = PackageComponentType.GetLocalizedFriendlyName(ClashType)

        ' Now, validate and apply the resolutions for the (chosen) options.
        Select Case True

            Case opt.Choice = UserChoice.Skip
                mods(ModificationType.Skip) = True

            Case ((defn Is IdClash OrElse defn Is IdNameClash) AndAlso opt.Choice = UserChoice.Overwrite)
                ' we don't need to do anything if we're just overwriting the same ID.
                mods(ModificationType.OverwritingExisting) = True
                If Published Then mods(ModificationType.Publish) = True

            Case ((defn Is IdClash OrElse defn Is IdTypeClash) AndAlso opt.Choice = UserChoice.NewId)
                ' we are setting a new ID.
                mods(ModificationType.IncomingId) = Guid.NewGuid()

            Case (defn Is NameClash AndAlso opt.Choice = UserChoice.Overwrite)
                ' We're overwriting the process with the same name, so we need to
                ' set the incoming process to the same ID
                mods(ModificationType.IncomingId) = mExistingProcessWithSameNameId
                mods(ModificationType.OverwritingExisting) = True

            Case ((defn Is NameClash OrElse defn Is NameTypeClash) AndAlso opt.Choice = UserChoice.Rename)
                If proposedName = "" Then
                    Return New clsError(Me,
                     My.Resources.ProcessComponent_YouMustProvideAnAlternativeNameForIncoming01,
                     typeLabel, Name)

                    ' Check that it's unique - again, no free pass
                    ' since we're effectively saving a new process.
                ElseIf Not gSv.IsProcessNameUnique(Nothing, Nothing, proposedName) Then
                    Return New clsError(Me,
                     My.Resources.ProcessComponent_YouCannotRenameTheIncoming01To2ThatNameIsAlreadyInUseInThisEnvironment,
                     typeLabel, Name, proposedName)

                Else
                    ' We have a non-empty, unique name. Yay us! Again!
                    mods(ModificationType.IncomingName) = proposedName

                End If

            Case ((defn Is NameClash OrElse defn Is NameTypeClash) AndAlso opt.Choice = UserChoice.RenameExisting)

                ' Make sure the proposed name is not empty
                If proposedName = "" Then
                    ' We can use this component's name in the error - we know it's the same
                    ' as the existing process since we have a ProcessNameClash conflict
                    Return New clsError(Me,
                     My.Resources.ProcessComponent_YouMustProvideAnAlternativeNameForTheExisting01,
                     IIf(defn Is NameClash, typeLabel, clashTypeLabel), Name)

                    ' Check it's unique - this time, don't fail the check when testing
                    ' the existing process that we already know is there.
                ElseIf Not gSv.IsProcessNameUnique(
                 mExistingProcessWithSameNameId, Nothing, proposedName) Then
                    Return New clsError(Me,
                     My.Resources.ProcessComponent_YouCannotRenameTheExisting01To2ThatNameIsAlreadyInUseInThisEnvironment,
                     IIf(defn Is NameClash, typeLabel, clashTypeLabel), Name, proposedName)

                Else
                    ' Non-empty, unique name. Meh. Seen it all before.
                    mods(ModificationType.ExistingName) = proposedName

                End If
            Case ((defn Is IdNameClash OrElse defn Is IdTypeNameClash OrElse
                   defn Is IdNameTypeClash OrElse defn Is IdTypeNameTypeClash) AndAlso opt.Choice = UserChoice.NewId)
                mods(ModificationType.IncomingId) = Guid.NewGuid()

                If proposedName = "" Then
                    Return New clsError(Me,
                     My.Resources.ProcessComponent_YouMustProvideAnAlternativeNameForIncoming01,
                     typeLabel, Name)

                    ' Check that it's unique - again, no free pass
                    ' since we're effectively saving a new process.
                ElseIf Not gSv.IsProcessNameUnique(Nothing, Nothing, proposedName) Then
                    Return New clsError(Me,
                     My.Resources.ProcessComponent_YouCannotRenameTheIncoming01To2ThatNameIsAlreadyInUseInThisEnvironment,
                     typeLabel, Name, proposedName)
                Else
                    ' We have a non-empty, unique name. Yay us! Again!
                    mods(ModificationType.IncomingName) = proposedName
                End If

            Case (defn Is ToBePublished _
             AndAlso opt.Choice = UserChoice.PerformPostProcess)
                If Not mods.ContainsKey(ModificationType.Publish) Then mods.Add(ModificationType.Publish, Nothing)

            Case (defn Is ToBeRetired _
             AndAlso opt.Choice = UserChoice.PerformPostProcess)
                If Not mods.ContainsKey(ModificationType.Retire) Then mods.Add(ModificationType.Retire, Nothing)

            Case (opt.Choice = UserChoice.OmitPostProcess _
             AndAlso (defn Is ToBePublished OrElse defn Is ToBeRetired))
                ' The existence of the mod implies post processing should be
                ' performed - its omission indicates that it should not...
                If mods.ContainsKey(ModificationType.Publish) Then mods.Remove(ModificationType.Publish)
                If mods.ContainsKey(ModificationType.Retire) Then mods.Remove(ModificationType.Retire)

        End Select
        Return Nothing

    End Function

    ''' <summary>
    ''' Checks the given conflict resolution, returning any error messages which
    ''' occurred due to the resolution being incomplete / invalid.
    ''' </summary>
    ''' <param name="resolutions">The resolutions to check</param>
    ''' <param name="errors">The error log to append any errors for this component
    ''' due to the specified conflict resolution.</param>
    Private Sub ApplyResolutions(
     ByVal resolutions As ICollection(Of ConflictResolution), ByVal errors As clsErrorLog)

        Dim mods As IDictionary(Of ModificationType, Object) = Modifications
        Dim typeLabel As String = PackageComponentType.GetLocalizedFriendlyName(Type)

        ' Clear down the modifications so we're starting from scratch
        mods.Clear()

        For Each res As ConflictResolution In resolutions
            ' The conflict that the resolution is resolving
            Dim con As Conflict = res.Conflict

            ' Skip those that aren't me
            If con.Component IsNot Me Then Continue For

            ' The definition of that conflict
            Dim defn As ConflictDefinition = con.Definition

            ' The resolution option chosen (may be null if no option is chosen)
            Dim opt As ConflictOption = res.ConflictOption

            ' Deal with the user not choosing an option first - this is an
            ' error for all (current) conflict types.
            If res.ConflictOption Is Nothing Then
                Select Case True
                    Case (defn Is IdClash OrElse defn Is IdTypeClash)
                        errors.Add(Me,
                         My.Resources.ProcessComponent_YouMustChooseHowToHandleTheConflictingIDFor01,
                         typeLabel, Name)

                    Case (defn Is NameClash OrElse defn Is NameTypeClash)
                        errors.Add(Me,
                         My.Resources.ProcessComponent_YouMustChooseHowToHandleTheConflictingNameFor01,
                         typeLabel, Name)

                    Case (defn Is IdNameClash OrElse defn Is IdTypeNameClash OrElse
                          defn Is IdNameTypeClash OrElse defn Is IdTypeNameTypeClash)
                        errors.Add(Me,
                         My.Resources.ProcessComponent_YouMustChooseHowToHandleTheConflictingIDAndNameFor01,
                         typeLabel, Name)

                    Case defn Is ToBePublished
                        errors.Add(Me,
                         My.Resources.ProcessComponent_YouMustChooseWhetherToPublishThe01, typeLabel, Name)

                    Case defn Is ToBeRetired
                        errors.Add(Me,
                         My.Resources.ProcessComponent_YouMustChooseWhetherToRetireThe01, typeLabel, Name)

                End Select
                ' Move onto the next resolution - without an option there's little we can
                ' do for this one.
                Continue For

            Else
                Dim err As clsError = ApplyResolution(res, mods)
                If err Is Nothing Then res.Passed = True Else errors.Add(err)

            End If

        Next

    End Sub


    ''' <summary>
    ''' Enum to name Clashes slightly saner to manage
    ''' Each combination of clashes gets its own enum to allow for slightly easier readability
    ''' and to keep code spaghetti to a minimum/confined space
    ''' </summary>
    Enum ClashWithTypeDefinition
        ProcessProcess
        BusinessObjectBusinessObject
        ProcessBusinessObject
        BusinessObjectProcess
        Other
    End Enum

    ''' <summary>
    ''' Takes two PackageComponentTypes and produces a ClashWithTypeDefinition enum to dictate the "type" of clash between them
    ''' The order matters, as you can get for example BusinessObjectProcess or ProcessBusinessObject
    ''' </summary>
    ''' <param name="firstType">The first param to compare</param>
    ''' <param name="secondType">The second param to compare</param>
    ''' <returns>Returns an ClashWithTypeDefinition</returns>
    Private Shared Function GetClashDefinition(firstType As PackageComponentType, secondType As PackageComponentType) As ClashWithTypeDefinition
        Select Case firstType
            Case PackageComponentType.BusinessObject
                Select Case secondType
                    Case PackageComponentType.BusinessObject
                        Return ClashWithTypeDefinition.BusinessObjectBusinessObject
                    Case PackageComponentType.Process
                        Return ClashWithTypeDefinition.BusinessObjectProcess
                End Select
            Case PackageComponentType.Process
                Select Case secondType
                    Case PackageComponentType.BusinessObject
                        Return ClashWithTypeDefinition.ProcessBusinessObject
                    Case PackageComponentType.Process
                        Return ClashWithTypeDefinition.ProcessProcess
                End Select
        End Select
        Return ClashWithTypeDefinition.Other
    End Function
#End Region

#Region " Xml Handling "

    ''' <summary>
    ''' Writes the head of the XML for this component to the given writer. This
    ''' leaves an element open that subclasses can write to if necessary in the
    ''' XML Body writer.
    ''' </summary>
    ''' <param name="writer">The writer to which the head of the XML representing
    ''' this component should be written.</param>
    Protected Overrides Sub WriteXmlHead(ByVal writer As XmlWriter)
        MyBase.WriteXmlHead(writer)
        If Published Then writer.WriteAttributeString("published", XmlConvert.ToString(True))
    End Sub

    ''' <summary>
    ''' Writes this process out to the given XML writer.
    ''' </summary>
    ''' <param name="writer">The writer to which this process should be written.
    ''' </param>
    Protected Overrides Sub WriteXmlBody(ByVal writer As XmlWriter)
        ' Just write out the process XML as is
        writer.WriteRaw(AssociatedProcess.GenerateXML(False))
        ' Unload the process as we no longer need it (and it's XML could be large)
        Me.AssociatedProcess = Nothing
    End Sub

    ''' <summary>
    ''' Reads the head of the XML for this component from the given reader.
    ''' </summary>
    ''' <param name="reader">The reader from where to draw this component's header
    ''' data.</param>
    ''' <param name="ctx">The context for the read operation</param>
    Protected Overrides Sub ReadXmlHead( _
     ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        MyBase.ReadXmlHead(reader, ctx)
        mPublished = (reader.GetAttribute("published") IsNot Nothing)
    End Sub

    ''' <summary>
    ''' Reads the XML body from the given reader
    ''' </summary>
    ''' <param name="reader">The reader from where to draw the process data.</param>
    ''' <param name="ctx">The object providing context for reading this data.</param>
    Protected Overrides Sub ReadXmlBody(ByVal reader As XmlReader, ByVal ctx As IComponentLoadingContext)
        Dim xml As String = reader.ReadInnerXml()
        AssociatedData = New ProcessWrapper(Me, xml)
        Dim proc As clsProcess = clsProcess.FromXml(
            Options.Instance.GetExternalObjectsInfo(), xml, True)
        Dependencies = proc.GetDependencies(False)
    End Sub

    ''' <summary>
    ''' Imports a standalone process from a legacy XML file - ie. the old process /
    ''' business object export format.
    ''' </summary>
    ''' <param name="reader">The XML reader onto the XML file with a process /
    ''' object represented in it.</param>
    ''' <returns>A single process component representing the process in the
    ''' specified XML.</returns>
    Public Shared Function ImportLegacy( _
     ByVal owner As OwnerComponent, ByVal reader As XmlReader) As ProcessComponent
        Dim id As Guid = BPUtil.IfNull(reader("preferredid"), Guid.NewGuid())
        Dim published As Boolean = BPUtil.IfNull(reader("published"), False)
        Dim name As String = reader("name")
        Dim tp As String = reader("type")
        Dim comp As ProcessComponent
        If tp = "object" _
         Then comp = New VBOComponent(owner, id, name) _
         Else comp = New ProcessComponent(owner, id, name)

        Dim errmsg As String = Nothing
        Dim proc As clsProcess = clsProcess.FromXml(
         Options.Instance.GetExternalObjectsInfo(), reader.ReadOuterXml(), True)
        proc.Id = id
        If published Then proc.Attributes = ProcessAttributes.Published
        comp.AssociatedProcess = proc
        comp.Dependencies = proc.GetDependencies(False)
        Return comp
    End Function

#End Region

End Class
