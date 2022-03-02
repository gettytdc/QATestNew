Imports BluePrism.Server.Domain.Models

Friend Class UserAuthType : Inherits UserControl

#Region " Windows Form Designer generated code "

    'UserControl overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
        End If
        MyBase.Dispose(disposing)
    End Sub

    Friend WithEvents rdoNative As AutomateControls.StyledRadioButton
    Friend WithEvents pnlGroupAuthTypes As Panel
    Friend WithEvents lblPrompt As Label
    Friend WithEvents rdoActiveDirectory As AutomateControls.StyledRadioButton

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(UserAuthType))
        Me.pnlGroupAuthTypes = New System.Windows.Forms.Panel()
        Me.rdoActiveDirectory = New AutomateControls.StyledRadioButton()
        Me.rdoNative = New AutomateControls.StyledRadioButton()
        Me.lblPrompt = New System.Windows.Forms.Label()
        Me.pnlGroupAuthTypes.SuspendLayout()
        Me.SuspendLayout()
        '
        'pnlGroupAuthTypes
        '
        resources.ApplyResources(Me.pnlGroupAuthTypes, "pnlGroupAuthTypes")
        Me.pnlGroupAuthTypes.Controls.Add(Me.rdoActiveDirectory)
        Me.pnlGroupAuthTypes.Controls.Add(Me.rdoNative)
        Me.pnlGroupAuthTypes.Name = "pnlGroupAuthTypes"
        '
        'rdoActiveDirectory
        '
        resources.ApplyResources(Me.rdoActiveDirectory, "rdoActiveDirectory")
        Me.rdoActiveDirectory.ButtonHeight = 21
        Me.rdoActiveDirectory.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.rdoActiveDirectory.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.rdoActiveDirectory.FocusDiameter = 16
        Me.rdoActiveDirectory.FocusThickness = 3
        Me.rdoActiveDirectory.FocusYLocation = 9
        Me.rdoActiveDirectory.ForceFocus = True
        Me.rdoActiveDirectory.ForeColor = System.Drawing.SystemColors.ControlText
        Me.rdoActiveDirectory.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.rdoActiveDirectory.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.rdoActiveDirectory.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoActiveDirectory.Name = "rdoActiveDirectory"
        Me.rdoActiveDirectory.RadioButtonDiameter = 12
        Me.rdoActiveDirectory.RadioButtonThickness = 2
        Me.rdoActiveDirectory.RadioYLocation = 7
        Me.rdoActiveDirectory.StringYLocation = 1
        Me.rdoActiveDirectory.TextColor = System.Drawing.Color.Black
        Me.rdoActiveDirectory.UseVisualStyleBackColor = True
        '
        'rdoNative
        '
        resources.ApplyResources(Me.rdoNative, "rdoNative")
        Me.rdoNative.ButtonHeight = 21
        Me.rdoNative.Checked = True
        Me.rdoNative.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer), CType(CType(212, Byte), Integer))
        Me.rdoNative.FocusColor = System.Drawing.Color.FromArgb(CType(CType(255, Byte), Integer), CType(CType(195, Byte), Integer), CType(CType(0, Byte), Integer))
        Me.rdoNative.FocusDiameter = 16
        Me.rdoNative.FocusThickness = 3
        Me.rdoNative.FocusYLocation = 9
        Me.rdoNative.ForceFocus = True
        Me.rdoNative.ForeColor = System.Drawing.SystemColors.ControlText
        Me.rdoNative.ForeGroundColor = System.Drawing.Color.FromArgb(CType(CType(67, Byte), Integer), CType(CType(74, Byte), Integer), CType(CType(79, Byte), Integer))
        Me.rdoNative.HoverColor = System.Drawing.Color.FromArgb(CType(CType(184, Byte), Integer), CType(CType(201, Byte), Integer), CType(CType(216, Byte), Integer))
        Me.rdoNative.MouseLeaveColor = System.Drawing.Color.White
        Me.rdoNative.Name = "rdoNative"
        Me.rdoNative.RadioButtonDiameter = 12
        Me.rdoNative.RadioButtonThickness = 2
        Me.rdoNative.RadioYLocation = 7
        Me.rdoNative.StringYLocation = 1
        Me.rdoNative.TabStop = True
        Me.rdoNative.TextColor = System.Drawing.Color.Black
        Me.rdoNative.UseVisualStyleBackColor = True
        '
        'lblPrompt
        '
        resources.ApplyResources(Me.lblPrompt, "lblPrompt")
        Me.lblPrompt.Name = "lblPrompt"
        '
        'UserAuthType
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.lblPrompt)
        Me.Controls.Add(Me.pnlGroupAuthTypes)
        Me.Name = "UserAuthType"
        Me.pnlGroupAuthTypes.ResumeLayout(False)
        Me.pnlGroupAuthTypes.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

#Region " Member Variables "

    Dim mAuthTypeCount As Integer = 0

#End Region

#Region " Constructors "

    Public Sub New()
        Me.New(AuthMode.Native, True, False)
    End Sub

    Public Sub New(authType As AuthMode,
                   displayNative As Boolean,
                   displayActiveDirectory As Boolean)
        MyBase.New()
        InitializeComponent()
        SetDefaultAuthType(authType)
        SetVisibleControls(displayNative, displayActiveDirectory)
    End Sub
    Public ReadOnly Property AuthTypeCount() As Integer
        Get
            Return mAuthTypeCount
        End Get
    End Property
    Protected Overrides Sub OnLoad(e As EventArgs)
        MyBase.OnLoad(e)
        lblPrompt.Text = My.Resources.UserAuthType_Prompt
        rdoNative.Text = My.Resources.UserAuthType_Native
        rdoActiveDirectory.Text = My.Resources.UserAuthType_ActiveDirectory

    End Sub

#End Region
    Private Sub SetVisibleControls(displayNative As Boolean,
                                   displayActiveDirectory As Boolean)
        Dim ctrlYCoord As Integer = 15
        Dim ctrlRowDistance As Integer = 32

        rdoNative.Visible = False
        rdoActiveDirectory.Visible = False

        If displayNative Then
            mAuthTypeCount += 1
            rdoNative.Location =
                New Point(rdoNative.Location.X, ctrlYCoord)
            rdoNative.Visible = True
            ctrlYCoord += ctrlRowDistance
        End If

        If displayActiveDirectory Then
            mAuthTypeCount += 1
            rdoActiveDirectory.Location =
                New Point(rdoActiveDirectory.Location.X, ctrlYCoord)
            rdoActiveDirectory.Visible = True
            ctrlYCoord += ctrlRowDistance
        End If

    End Sub
    Public Function GetSelectedAuthType() As AuthMode
        If rdoNative.Checked Then
            Return AuthMode.Native
        End If
        If rdoActiveDirectory.Checked Then
            Return AuthMode.MappedActiveDirectory
        End If
        ' will only get here if new button added and not mapped to a auth type
        Throw New InvalidOperationException("Auth selection not mapped")
    End Function
    Private Sub SetDefaultAuthType(authType As AuthMode)
        Select Case authType
            Case AuthMode.Native
                rdoNative.Checked = True
            Case AuthMode.MappedActiveDirectory
                rdoActiveDirectory.Checked = True
            Case Else
                Throw New ArgumentException($"{authType} not supported")
        End Select
    End Sub
End Class
