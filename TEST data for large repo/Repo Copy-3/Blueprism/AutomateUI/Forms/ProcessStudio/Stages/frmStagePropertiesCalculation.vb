Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.Core.Expressions
Imports BluePrism.AutomateProcessCore.Stages

''' Project  : Automate
''' Class    : frmStagePropertiesCalculation
''' 
''' <summary>
''' A calculation properties form.
''' </summary>
Friend Class frmStagePropertiesCalculation
    Inherits frmCalculationDecisionBase

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()
    End Sub

#Region " Windows Form Designer generated code "

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesCalculation))
        Me.SuspendLayout()
        '
        'txtName
        '
        resources.ApplyResources(Me.txtName, "txtName")
        '
        'txtDescription
        '
        resources.ApplyResources(Me.txtDescription, "txtDescription")
        '
        'frmStagePropertiesCalculation
        '
        resources.ApplyResources(Me, "$this")
        Me.Name = "frmStagePropertiesCalculation"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()
        Me.mBuilder.SetStage(Me.mProcessStage)
        Me.mBuilder.ProcessViewer = Me.ProcessViewer
    End Sub

    ''' <summary>
    ''' Extends the validation performed in the base class.
    ''' </summary>
    ''' <returns>Returns true if validation successful, false otherwise.</returns>
    Protected Overrides Function ApplyChanges() As Boolean
        If Not MyBase.ApplyChanges() Then Return False

        With CType(mProcessStage, clsCalculationStage)
            .StoreIn = mBuilder.StoreInText
            .Expression = BPExpression.FromLocalised(mBuilder.ExpressionText)
        End With
        Return True

    End Function

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesCalculation.htm"
    End Function

    ''' <summary>
    ''' Opens the help file whether online or offline.
    ''' </summary>
    Public Overrides Sub OpenHelp()
        Try
            OpenHelpFile(Me, GetHelpFile())
        Catch
            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
        End Try
    End Sub
End Class
