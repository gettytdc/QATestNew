Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : ctlProcessValueEdit
''' 
''' <summary>
''' This class is used by frmStartParams and frmSessionVariables, to build up a
''' scrollable panel of input controls, where a clsProcessValue can be edited.
''' </summary>
Public Class ctlProcessValueEdit : Inherits Panel
    Implements IActivatableProcessValue

#Region " Published Events "

    ''' <summary>
    ''' The changed event is fired whenever the underlying process value is changed.
    ''' </summary>
    Public Event Changed As EventHandler Implements IProcessValue.Changed

    ''' <summary>
    ''' The activated event is fired when the underlying process value control is
    ''' activated.
    ''' </summary>
    ''' <param name="sender">The control which has been activated</param>
    ''' <param name="e">The args detailing the event.</param>
    Public Event Activated(ByVal sender As IActivatableProcessValue, ByVal e As EventArgs) _
     Implements IActivatableProcessValue.Activated

#End Region

#Region " Member Variables "

    Private Const CanvasWidth As Integer = 550
    Private WithEvents lblDatatype As New Label()
    Private WithEvents lblTitle As New Label()
    Friend WithEvents pbTick As New PictureBox()

    ''' <summary>
    ''' The control that allows the value to be edited, this control is set
    ''' dynamically depending on the data type of the value being edited. The control
    ''' must also implement IProcessValue to function correctly.
    ''' </summary>
    Private WithEvents mControl As Control

    ''' <summary>
    ''' The icon mode in effect. See the constructor documentation.
    ''' </summary>
    Private mIconMode As Boolean

    ' Flag indicating if this edit control should be displaying the value as
    ' indeterminate. Typically used to denote a single edit which changes multiple
    ' value items.
    Private mIndeterminate As Boolean

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Constructor that takes a name and type, the name is used for the title of the
    ''' Process Value Edit control, and the data type is displayed next to the edit
    ''' control.
    ''' </summary>
    ''' <param name="name">The name of the value, to be used in the title.</param>
    ''' <param name="t">The DataType of the value.</param>
    ''' <param name="icon">The icon to use</param>
    ''' <param name="iconMode">Currently only two possible values - True means the
    ''' icon is used to denote whether a 'value' has been entered. This is used for
    ''' the Startup Parameters form. False means the icon is applied to indeterminate
    ''' values.</param>
    Public Sub New(ByVal name As String, ByVal t As DataType, ByVal icon As Image, ByVal iconMode As Boolean, Optional ByVal ctlName As String = "")

        MyBase.New()

        mIconMode = iconMode

        Me.SuspendLayout()
        
        Width = CanvasWidth
        BackColor = System.Drawing.Color.White
        
        BorderStyle = BorderStyle.FixedSingle
        Me.Name = ctlName
        
        'Title label...
        lblTitle.Location = New System.Drawing.Point(0, 0)
        lblTitle.Width = Width
        lblTitle.Height = 16
        lblTitle.Anchor = CType(((AnchorStyles.Top Or AnchorStyles.Left) _
         Or AnchorStyles.Right), AnchorStyles)
        lblTitle.BackColor = System.Drawing.SystemColors.Control
        lblTitle.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        lblTitle.TabIndex = 7
        lblTitle.Text = name

        'Icon...
        pbTick.Image = icon
        pbTick.Location = New System.Drawing.Point(Width - 16 - 5, 0)
        pbTick.Size = New System.Drawing.Size(16, 16)
        pbTick.Anchor = AnchorStyles.Top Or AnchorStyles.Right
        pbTick.TabIndex = 10
        pbTick.TabStop = False
        pbTick.Visible = False
        pbTick.BackColor = lblTitle.BackColor

        DisplayEditControl(t)

        'DataType label...
        lblDatatype.AutoSize = True
        lblDatatype.BackColor = System.Drawing.Color.White
        lblDatatype.Font = New System.Drawing.Font("Segoe UI", 8.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        lblDatatype.Location = New System.Drawing.Point(8, (mControl.Top+ (mControl.Height\2)-lblDatatype.Height\2))
        lblDatatype.TextAlign = ContentAlignment.MiddleLeft
        lblDatatype.TabIndex = 8
        lblDatatype.Text = clsProcessDataTypes.GetFriendlyName(t)

        Controls.Add(pbTick)
        Controls.Add(lblDatatype)
        Controls.Add(lblTitle)
        
        Height = lblTitle.Height + 8 + mControl.Height + 8

        ResumeLayout(False)
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Provides access to the underlying Process Value.
    ''' </summary>
    ''' <value>The Process Value</value>
    Public Property Value() As clsProcessValue Implements IProcessValue.Value
        Get
            Return CType(mControl, IProcessValue).Value
        End Get
        Set(ByVal Value As clsProcessValue)
            CType(mControl, IProcessValue).Value = Value
            If mIconMode Then
                Checked = Not Value.IsNull
            Else
                Checked = mIndeterminate
            End If
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the readonly state of this edit control
    ''' </summary>
    Public Property [ReadOnly]() As Boolean Implements IProcessValue.ReadOnly
        Get
            Return CType(mControl, IProcessValue).ReadOnly
        End Get
        Set(ByVal value As Boolean)
            CType(mControl, IProcessValue).ReadOnly = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the indeterminate state of this edit control.
    ''' Note that this only has a visible effect if the 'icon mode' of this control
    ''' is set to false when constructed, obviously.
    ''' </summary>
    Public Property Indeterminate() As Boolean
        Get
            Return mIndeterminate
        End Get
        Set(ByVal value As Boolean)
            mIndeterminate = value
            If Not mIconMode Then Checked = value
        End Set
    End Property

    ''' <summary>
    ''' This property allows access to the control's title label.
    ''' </summary>
    ''' <value>The title</value>
    Public Property Title() As String
        Get
            Return lblTitle.Text
        End Get
        Set(ByVal Value As String)
            lblTitle.Text = Value
        End Set
    End Property

    ''' <summary>
    ''' This property allows access to the control's checked state.
    ''' </summary>
    ''' <value>The checked state</value>
    Public Property Checked() As Boolean
        Get
            Return pbTick.Visible
        End Get
        Set(ByVal Value As Boolean)
            pbTick.Visible = Value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' This function displays the correct control for editing the ProcessValue.
    ''' </summary>
    ''' <param name="t">The data type needed to choose the correct control</param>
    Private Sub DisplayEditControl(ByVal t As DataType)

        mControl = clsProcessValueControl.GetControl(t)

        'inset the control by 20% of the parent width
        Dim insetWidth As Integer = Width \ 5
        mControl.Top = lblTitle.Height + 8
        mControl.Left = insetWidth
        mControl.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Top
        AddHandler CType(mControl, IProcessValue).Changed, AddressOf HandleValueEdited

        Dim activatable As IActivatableProcessValue = TryCast(mControl, IActivatableProcessValue)
        If activatable IsNot Nothing Then
            AddHandler activatable.Activated, AddressOf HandleEditControlActivated
        End If

        Controls.Add(mControl)
        mControl.Focus()
    End Sub

    ''' <summary>
    ''' This is the event handler for the checkbox click event
    ''' </summary>
    Private Sub cbTick_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles pbTick.Click
        If mIconMode Then
            If Checked Then
                ' There is a very good reason for the next line
                ' we want to make sure the check happens before any events are raised
                Checked = False
                mControl.ResetText()
                Commit()
            End If
            Checked = False
        End If
    End Sub

    ''' <summary>
    ''' The event handler for the controls IProcessValue.Changed event.
    ''' </summary>
    ''' <param name="sender">The object sending the event</param>
    ''' <param name="e">The event arguments</param>
    Private Sub HandleValueEdited(ByVal sender As Object, ByVal e As EventArgs)
        Commit()
    End Sub

    ''' <summary>
    ''' Event handler for the control's IActivatableProcessValue.Activated event.
    ''' This just chains on the event to any other listeners.
    ''' </summary>
    ''' <param name="sender">The control which fired the event.</param>
    ''' <param name="e">The args detailing the event.</param>
    Private Sub HandleEditControlActivated( _
     ByVal sender As IActivatableProcessValue, ByVal e As EventArgs)
        ' Just chain the event on...
        RaiseEvent Activated(sender, e)
    End Sub

    ''' <summary>
    ''' This sub is called when we have done editing the control's value.
    ''' </summary>
    Private Sub Commit() Implements IProcessValue.Commit
        Value = CType(mControl, IProcessValue).Value
        RaiseEvent Changed(Me, EventArgs.Empty)
        If mIconMode Then Checked = Not Value.IsNull
    End Sub

    ''' <summary>
    ''' overrides the ongotfocus method to set the focus to the embedded control
    ''' rather than this control.
    ''' </summary>
    ''' <param name="e">The event arguments</param>
    Protected Overrides Sub OnGotFocus(ByVal e As EventArgs)
        mControl.Focus()
    End Sub

    ''' <summary>
    ''' Selects this control
    ''' </summary>
    Public Sub SelectControl() Implements IProcessValue.SelectControl
        mControl.Select()
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.ResumeLayout(False)

    End Sub

#End Region

End Class
