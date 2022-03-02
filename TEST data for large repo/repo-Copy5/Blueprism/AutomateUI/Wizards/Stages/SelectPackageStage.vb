Imports BluePrism.AutomateAppCore

''' <summary>
''' Stage class for selecting a package.
''' </summary>
Public Class SelectPackageStage : Inherits WizardStage

    ''' <summary>
    ''' The ID of this stage
    ''' </summary>
    Public Const StageId As String = "select-package"

    ' The selected package
    Private mPackage As clsPackage

    ' Flag indicating if the package is an adhoc or pre-existing package.
    Private mPreconfigured As Boolean

    ''' <summary>
    ''' Creates a new select package stage
    ''' </summary>
    Public Sub New()
        MyBase.New(My.Resources.SelectPackageStage_SelectThePackage, My.Resources.SelectPackageStage_SelectWhichPackageToCreateTheReleaseFrom)
    End Sub

    ''' <summary>
    ''' The ID of this stage.
    ''' </summary>
    Public Overrides ReadOnly Property Id() As String
        Get
            Return StageId
        End Get
    End Property

    ''' <summary>
    ''' Gets the selector control for this stage, or null if it is not currently
    ''' available.
    ''' </summary>
    Private ReadOnly Property Selector() As ctlPackageSelector
        Get
            Return DirectCast(mControl, ctlPackageSelector)
        End Get
    End Property

    ''' <summary>
    ''' The currently selected package in this stage. Null if nothing is selected.
    ''' </summary>
    Public Property SelectedPackage() As clsPackage
        Get
            If Selector IsNot Nothing Then mPackage = Selector.SelectedPackage
            Return mPackage
        End Get
        Set(ByVal value As clsPackage)
            mPackage = value
            If Selector IsNot Nothing Then Selector.SelectedPackage = value
        End Set
    End Property

    ''' <summary>
    ''' Flag indicating if the current package is a preconfigured package or adhoc.
    ''' </summary>
    Public Property IsPreconfigured() As Boolean
        Get
            If Selector IsNot Nothing Then mPreconfigured = Selector.IsPreconfigured
            Return mPreconfigured
        End Get
        Set(ByVal value As Boolean)
            mPreconfigured = value
            If Selector IsNot Nothing Then Selector.IsPreconfigured = value
        End Set
    End Property

    ''' <summary>
    ''' Creates the control necessary to accept the input for this stage.
    ''' </summary>
    ''' <returns>The control to use on this stage</returns>
    Protected Overrides Function CreateControl() As ctlWizardStageControl
        Dim s As New ctlPackageSelector()
        s.Packages = gSv.GetPackages()
        AddHandler s.PackageActivated, AddressOf HandlePackageActivated
        Return s
    End Function

    ''' <summary>
    ''' Handles the package being activated, ie. double clicked.
    ''' </summary>
    ''' <param name="sender">The sender of the event.</param>
    ''' <param name="pkg">The package which has been activated.</param>
    Private Sub HandlePackageActivated(ByVal sender As Object, ByVal pkg As clsPackage)
        OnActivated(New ActivationEventArgs(pkg))
    End Sub

    ''' <summary>
    ''' Commits this stage, ensuring that a package has been selected.
    ''' </summary>
    Protected Overrides Sub OnCommitting(ByVal e As StageCommittingEventArgs)
        If Selector.SelectedPackage Is Nothing Then
            UserMessage.Show(My.Resources.SelectPackageStage_YouMustSelectAPackage)
            e.Cancel = True
        Else
            MyBase.OnCommitting(e)
        End If
    End Sub

    ''' <summary>
    ''' Handles this stage being committed by saving the selected package to the
    ''' member variable so that the control can be disposed of.
    ''' </summary>
    Protected Overrides Sub OnCommitted(ByVal e As StageCommittedEventArgs)
        mPackage = Selector.SelectedPackage
        MyBase.OnCommitted(e)
    End Sub

End Class
