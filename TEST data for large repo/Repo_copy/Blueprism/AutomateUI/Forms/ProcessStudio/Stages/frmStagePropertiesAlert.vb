Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.Core.Expressions
Imports BluePrism.AutomateProcessCore.Stages

Friend Class frmStagePropertiesAlert

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        mBuilder.Validator = AddressOf Me.mBuilder.IsValidAlert
        mBuilder.Tester = AddressOf Me.mBuilder.TestExpression

        mBuilder.StoreInVisible = False

        Me.Text = My.Resources.frmStagePropertiesAlert_AlertProperties

    End Sub

    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()
        Dim stg As clsAlertStage = CType(mProcessStage, clsAlertStage)
        mBuilder.ExpressionText = stg.Expression.LocalForm
        mBuilder.SetStage(stg)
        mBuilder.ProcessViewer = Me.ProcessViewer
    End Sub

    Protected Overrides Function ApplyChanges() As Boolean
        If MyBase.ApplyChanges Then
            Dim stg As clsAlertStage = CType(mProcessStage, clsAlertStage)

            stg.Expression = BPExpression.FromLocalised(mBuilder.ExpressionTrimmedText)
            Return True
        Else
            Return False
        End If
    End Function

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesAlert.htm"
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
