Namespace Controls.Widgets.SystemManager.WebApi.Authentication
    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class AuthenticationCredentialPanel
        Inherits System.Windows.Forms.UserControl

        'UserControl overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose(ByVal disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(AuthenticationCredentialPanel))
            Me.txtParameterName = New AutomateControls.Textboxes.StyledTextBox()
            Me.lblParamName = New System.Windows.Forms.Label()
        Me.chkExposeToProcess = New System.Windows.Forms.CheckBox()
        Me.grpCredential = New System.Windows.Forms.GroupBox()
        Me.cmbCredentialName = New AutomateControls.StyledComboBox()
        Me.grpCredential.SuspendLayout
        Me.SuspendLayout
        '
        'txtParameterName
        '
        resources.ApplyResources(Me.txtParameterName, "txtParameterName")
        Me.txtParameterName.Name = "txtParameterName"
        '
        'lblParamName
        '
        resources.ApplyResources(Me.lblParamName, "lblParamName")
        Me.lblParamName.Name = "lblParamName"
        '
        'chkExposeToProcess
        '
        resources.ApplyResources(Me.chkExposeToProcess, "chkExposeToProcess")
        Me.chkExposeToProcess.Name = "chkExposeToProcess"
            Me.chkExposeToProcess.UseVisualStyleBackColor = True
            '
            'grpCredential
            '
            Me.grpCredential.Controls.Add(Me.cmbCredentialName)
            Me.grpCredential.Controls.Add(Me.chkExposeToProcess)
            Me.grpCredential.Controls.Add(Me.lblParamName)
            Me.grpCredential.Controls.Add(Me.txtParameterName)
            resources.ApplyResources(Me.grpCredential, "grpCredential")
            Me.grpCredential.Name = "grpCredential"
            Me.grpCredential.TabStop = False
            '
            'cmbCredentialName
            '
            resources.ApplyResources(Me.cmbCredentialName, "cmbCredentialName")
            Me.cmbCredentialName.Checkable = False
            Me.cmbCredentialName.FormattingEnabled = True
            Me.cmbCredentialName.Name = "cmbCredentialName"
            '
            'AuthenticationCredentialPanel
            '
            resources.ApplyResources(Me, "$this")
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None
            Me.Controls.Add(Me.grpCredential)
        resources.ApplyResources(Me, "$this")
        Me.Name = "AuthenticationCredentialPanel"
        Me.grpCredential.ResumeLayout(false)
        Me.grpCredential.PerformLayout
        Me.ResumeLayout(false)

End Sub

        Friend WithEvents txtParameterName As AutomateControls.Textboxes.StyledTextBox
        Friend WithEvents lblParamName As Label
        Friend WithEvents chkExposeToProcess As CheckBox
        Friend WithEvents cmbCredentialName As AutomateControls.StyledComboBox
        Friend WithEvents grpCredential As GroupBox
    End Class
End NameSpace