Namespace Events

    Public Enum CurrentStepTypes
        Install
        CreateInstance
        ConfigureDatabase
        CreateDatabase
        UpgradeDatabase
        Complete
    End Enum

    Public Delegate Sub ReportCurrentStepEventHandler(sender As Object, e As ReportCurrentStepEventArgs)

    Public Class ReportCurrentStepEventArgs : Inherits EventArgs
        Private _currentStep As CurrentStepTypes

        Public Sub New(currentStep As CurrentStepTypes)
            Me._currentStep = currentStep
        End Sub

        Public ReadOnly Property CurrentStep As CurrentStepTypes
            Get
                Return Me._currentStep
            End Get
        End Property
    End Class

End Namespace
