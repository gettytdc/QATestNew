Imports BluePrism.AutomateAppCore

''' <summary>
''' A stage, ie. a single part of a wizard, represented by a control which sits in
''' the wizard.
''' </summary>
Public Class ReleaseDifferenceStage : Inherits WizardStage

    ''' <summary>
    ''' The ID of this stage
    ''' </summary>
    Public Const StageId As String = "release.diff"

    ' The differences being represented in this stage
    Private mDifferences As IDictionary(Of PackageComponent, String)

    ''' <summary>
    ''' Creates a new description stage.
    ''' </summary>
    Public Sub New()
        MyBase.New(My.Resources.ReleaseDifferenceStage_ReleaseComparisons,
         My.Resources.ReleaseDifferenceStage_DifferencesBetweenTheCurrentPackageAndTheSpecifiedRelease)
    End Sub

    ''' <summary>
    ''' The unique ID for this stage.
    ''' </summary>
    Public Overrides ReadOnly Property Id() As String
        Get
            If StageId Is Nothing Then Throw New NotImplementedException(My.Resources.ReleaseDifferenceStage_StageIDNotSet)
            Return StageId
        End Get
    End Property

    ''' <summary>
    ''' The differences being represented in this stage.
    ''' </summary>
    Public Property Differences() As IDictionary(Of PackageComponent, String)
        Get
            Return mDifferences
        End Get
        Set(ByVal value As IDictionary(Of PackageComponent, String))
            mDifferences = value
            If mControl IsNot Nothing Then _
             DirectCast(mControl, ctlReleaseDifference).Differences = value
        End Set
    End Property

    ''' <summary>
    ''' Creates the control which displays or modifies the contents of this stage
    ''' </summary>
    ''' <returns>The control used to display and modify this stage's contents.
    ''' </returns>
    Protected Overrides Function CreateControl() As ctlWizardStageControl
        Dim differ As New ctlReleaseDifference()
        differ.Differences = mDifferences
        Return differ
    End Function

End Class
