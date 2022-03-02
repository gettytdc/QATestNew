Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.Core.Expressions
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages

''' Project  : Automate
''' Class    : frmStagePropertiesDecision
''' 
''' <summary>
''' The decision properties form.
''' </summary>
Friend Class frmStagePropertiesDecision
    Inherits AutomateUI.frmCalculationDecisionBase

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        Me.mBuilder.Validator = AddressOf Me.mBuilder.IsValidDecision
        Me.mBuilder.Tester = AddressOf Me.mBuilder.TestExpression

        mBuilder.StoreInVisible = False

    End Sub

    'Form overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesDecision))
        Me.SuspendLayout()
        '
        'frmStagePropertiesDecision
        '
        resources.ApplyResources(Me, "$this")
        Me.Name = "frmStagePropertiesDecision"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

#Region "PopulateStageData"

    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()
        Dim objDecision As Stages.clsDecisionStage = CType(mProcessStage, Stages.clsDecisionStage)
        Me.mBuilder.ExpressionText = objDecision.Expression.LocalForm
        Me.mBuilder.SetStage(objDecision)
        Me.mBuilder.ProcessViewer = Me.ProcessViewer
    End Sub

#End Region

#Region "Apply Changes"

    Protected Overrides Function ApplyChanges() As Boolean
        If Not MyBase.ApplyChanges() Then Return False

        Dim stg As clsDecisionStage = CType(mProcessStage, clsDecisionStage)
        stg.Expression = BPExpression.FromLocalised(mBuilder.ExpressionTrimmedText)
        Return True
    End Function

#End Region

#Region "IsValid"


#End Region


    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesDecision.htm"
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