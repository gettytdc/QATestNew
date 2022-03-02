Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.clsProcessDataTypes

''' Project  : Automate
''' Class    : ctlDataTypeTips
''' 
''' <summary>
''' Provides simple tips on the nature and usage of particular datatypes.
''' </summary>
Public Class ctlDataTypeTips
    Inherits System.Windows.Forms.UserControl

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call

    End Sub

    'UserControl overrides dispose to clean up the component list.
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
    Friend WithEvents lblTypeName As System.Windows.Forms.Label
    Friend WithEvents lblDescription As System.Windows.Forms.Label
    Friend WithEvents Panel1 As clsPanel
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlDataTypeTips))
        Me.lblTypeName = New System.Windows.Forms.Label()
        Me.lblDescription = New System.Windows.Forms.Label()
        Me.Panel1 = New AutomateUI.clsPanel()
        Me.Panel1.SuspendLayout()
        Me.SuspendLayout()
        '
        'lblTypeName
        '
        resources.ApplyResources(Me.lblTypeName, "lblTypeName")
        Me.lblTypeName.BackColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(165, Byte), Integer), CType(CType(203, Byte), Integer))
        Me.lblTypeName.ForeColor = System.Drawing.Color.White
        Me.lblTypeName.Name = "lblTypeName"
        '
        'lblDescription
        '
        resources.ApplyResources(Me.lblDescription, "lblDescription")
        Me.lblDescription.BackColor = System.Drawing.Color.White
        Me.lblDescription.Name = "lblDescription"
        '
        'Panel1
        '
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.BorderColor = System.Drawing.Color.FromArgb(CType(CType(159, Byte), Integer), CType(CType(165, Byte), Integer), CType(CType(203, Byte), Integer))
        Me.Panel1.BorderStyle = AutomateUI.clsPanel.BorderMode.[On]
        Me.Panel1.BorderWidth = 1
        Me.Panel1.Controls.Add(Me.lblTypeName)
        Me.Panel1.Controls.Add(Me.lblDescription)
        Me.Panel1.Name = "Panel1"
        '
        'ctlDataTypeTips
        '
        Me.BackColor = System.Drawing.Color.White
        Me.Controls.Add(Me.Panel1)
        Me.Name = "ctlDataTypeTips"
        resources.ApplyResources(Me, "$this")
        Me.Panel1.ResumeLayout(False)
        Me.ResumeLayout(False)

    End Sub

#End Region

    ''' <summary>
    ''' Updates the display to give information about the given data type. This may
    ''' be set to nothing to clear all current information and leave the display 
    ''' blank.
    ''' </summary>
    ''' <param name="dt">The data type of interest.</param>
    Public Sub ShowTipForType(ByVal dt As DataType)
        ShowTip(GetFriendlyName(dt), GetFriendlyDescription(dt))
    End Sub

    ''' <summary>
    ''' Updates the display to give information.
    ''' </summary>
    ''' <param name="Title">The title of the tip</param>
    ''' <param name="Tip">The text of the tip</param>
    Public Sub ShowTip(ByVal Title As String, ByVal Tip As String)
        lblTypeName.Text = Title
        lblDescription.Text = Tip
    End Sub

End Class
