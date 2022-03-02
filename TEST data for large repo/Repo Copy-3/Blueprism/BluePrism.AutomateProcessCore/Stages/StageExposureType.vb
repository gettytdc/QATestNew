Imports System.Resources
Imports System.Runtime.CompilerServices
Imports BluePrism.Core.Utility

Namespace Stages

    Public Enum StageExposureType
        None
        Statistic
        Environment
        Session
    End Enum

    Public Module StageExposureTypeExtensions

        Public Const ResourceTemplate As String = "StageExposureType_{0}"

        <Extension()>
        Public Function ToLocalizedString(exposureType As StageExposureType, resourceManager As ResourceManager) As String
            Return resourceManager.EnsureString(ResourceTemplate, exposureType)
        End Function

    End Module

End Namespace