Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateProcessCore.Stages

''' Project  : Automate
''' Class    : frmStagePropertiesLoopEnd
''' 
''' <summary>
''' The loop end properties form.
''' </summary>
Friend Class frmStagePropertiesGroupEnd
    Inherits frmProperties


#Region " Windows Form Designer generated code "


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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesGroupEnd))
        Me.SuspendLayout()
        '
        'mTitleBar
        '
        resources.ApplyResources(Me.mTitleBar, "mTitleBar")
        '
        'frmStagePropertiesGroupEnd
        '
        resources.ApplyResources(Me, "$this")
        Me.Name = "frmStagePropertiesGroupEnd"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        mProcessStage = Nothing

    End Sub

    Protected Overrides Sub PopulateStageData()
        MyBase.PopulateStageData()
        If TypeOf mProcessStage Is clsWaitEndStage Then
            Me.Text = My.Resources.frmStagePropertiesGroupEnd_WaitEndProperties
        ElseIf TypeOf mProcessStage Is clsChoiceEndStage Then
            Me.Text = My.Resources.frmStagePropertiesGroupEnd_ChoiceEndProperties
        Else
            Me.Text = My.Resources.frmStagePropertiesGroupEnd_LoopEndProperties
        End If
    End Sub

    Private Sub frmStagePropertiesLoopEnd_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Make sure we have a valid index...
        If mProcessStage Is Nothing Then
            UserMessage.Show(My.Resources.frmStagePropertiesGroupEnd_PropertiesDialogIsNotProperlyConfigured)
            Exit Sub
        End If

        'Fill in all the fields...
        MyBase.txtName.Text = mProcessStage.GetName()
        MyBase.txtDescription.Text = mProcessStage.GetNarrative()

    End Sub

    ''' <summary>
    ''' Gets the name of the associated help file.
    ''' </summary>
    ''' <returns>The file name</returns>
    Public Overrides Function GetHelpFile() As String
        Return "frmStagePropertiesLoopEnd.htm"
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
