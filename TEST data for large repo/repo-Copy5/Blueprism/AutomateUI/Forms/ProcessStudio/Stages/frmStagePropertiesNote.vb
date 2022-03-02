Imports BluePrism.AutomateAppCore.Utility

''' Project  : Automate
''' Class    : frmStagePropertiesNote
''' 
''' <summary>
''' The note properties form.
''' </summary>
Friend Class frmStagePropertiesNote
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
    Friend WithEvents Label1 As System.Windows.Forms.Label

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmStagePropertiesNote))
        Me.Label1 = New System.Windows.Forms.Label()
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
        'mTitleBar
        '
        resources.ApplyResources(Me.mTitleBar, "mTitleBar")
        Me.mTitleBar.SubTitle = ""
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.Name = "Label1"
        '
        'frmStagePropertiesNote
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.Label1)
        Me.Name = "frmStagePropertiesNote"
        Me.Controls.SetChildIndex(Me.Label1, 0)
        Me.Controls.SetChildIndex(Me.txtDescription, 0)
        Me.Controls.SetChildIndex(Me.mTitleBar, 0)
        Me.Controls.SetChildIndex(Me.txtName, 0)
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

    Private Sub frmStageProperties_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'Make sure we have a valid index...
        If mProcessStage Is Nothing Then
            UserMessage.Show(My.Resources.frmStagePropertiesNote_PropertiesDialogIsNotProperlyConfigured)
            Exit Sub
        End If

        'Fill in all the fields...
        MyBase.txtName.Text = mProcessStage.GetName()
        MyBase.txtDescription.Text = mProcessStage.GetNarrative()
        Me.txtDescription.Select()
    End Sub


    ''' <summary>
    ''' Extends the validation performed in the base class.
    ''' </summary>
    ''' <returns>Returns true if validation successful, false otherwise.</returns>
    Protected Overrides Function ApplyChanges() As Boolean
        If MyBase.ApplyChanges Then

            'add any validation or saving that you want to do here

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
        Return "frmStagePropertiesNote.htm"
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
