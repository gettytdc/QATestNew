Imports BluePrism.AutomateAppCore

''' <summary>
''' A stage, ie. a single part of a wizard, represented by a control which sits in
''' the wizard.
''' </summary>
Public Class PackageBundlerStage : Inherits WizardStage

    ''' <summary>
    ''' The ID of this stage
    ''' </summary>
    Public Const StageId As String = "package.bundler"

    ' The components modelled in this stage
    Private mComponents As ICollection(Of PackageComponent)

    ''' <summary>
    ''' Creates a new description stage.
    ''' </summary>
    Public Sub New()
        MyBase.New(My.Resources.PackageBundlerStage_ConfigureThePackage, My.Resources.PackageBundlerStage_DragAndDropTheRequiredContentsOfThePackage)
    End Sub

    ''' <summary>
    ''' Gets the package bundler used by this stage.
    ''' </summary>
    Private ReadOnly Property Bundler() As ctlPackageBundler
        Get
            Return DirectCast(mControl, ctlPackageBundler)
        End Get
    End Property

    ''' <summary>
    ''' The unique ID for this stage.
    ''' </summary>
    Public Overrides ReadOnly Property Id() As String
        Get
            If StageId Is Nothing Then Throw New NotImplementedException(My.Resources.PackageBundlerStage_StageIDNotSet)
            Return StageId
        End Get
    End Property

    ''' <summary>
    ''' Creates the control which displays or modifies the contents of this stage
    ''' </summary>
    ''' <returns>The control used to display and modify this stage's contents.
    ''' </returns>
    Protected Overrides Function CreateControl() As ctlWizardStageControl
        Return New ctlPackageBundler()
        If mComponents IsNot Nothing Then Bundler.PackageComponents = mComponents
    End Function

    ''' <summary>
    ''' The package components currently set in this stage.
    ''' </summary>
    Public Property PackageComponents() As ICollection(Of PackageComponent)
        Get
            If Bundler Is Nothing Then Return mComponents
            Return Bundler.PackageComponents
        End Get
        Set(ByVal value As ICollection(Of PackageComponent))
            mComponents = value
            If Bundler IsNot Nothing Then Bundler.PackageComponents = value
        End Set
    End Property

    ''' <summary>
    ''' Handles this stage committing - this checks that some components have been
    ''' chosen before allowing the commit to continue
    ''' </summary>
    Protected Overrides Sub OnCommitting(ByVal e As StageCommittingEventArgs)
        If Bundler.PackageComponents.Count = 0 Then
            UserMessage.Show(My.Resources.PackageBundlerStage_YouMustChooseAtLeastOneComponentForThePackage)
            e.Cancel = True
        Else
            MyBase.OnCommitting(e)
        End If
    End Sub

    ''' <summary>
    ''' Handles this stage being committed by saving the components to the member
    ''' variable so that the control can be disposed of.
    ''' </summary>
    Protected Overrides Sub OnCommitted(ByVal e As StageCommittedEventArgs)
        mComponents = Bundler.PackageComponents
        MyBase.OnCommitted(e)
    End Sub

End Class
