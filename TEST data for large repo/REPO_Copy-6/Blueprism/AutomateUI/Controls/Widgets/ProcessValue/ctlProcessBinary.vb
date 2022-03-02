Imports System.IO
Imports BluePrism.AutomateProcessCore

''' Project  : Automate
''' Class    : ctlProcessBinary
''' 
''' <summary>
''' This control allows a user to edit an binary value.
''' </summary>
Friend Class ctlProcessBinary : Inherits UserControl : Implements IProcessValue

#Region "Windows Forms Designer Generated Code"

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlProcessBinary))
        Me.txtInfo = New AutomateControls.Textboxes.StyledTextBox()
        Me.btnExport = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnImport = New AutomateControls.Buttons.StandardStyledButton()
        Me.btnClear = New AutomateControls.Buttons.StandardStyledButton()
        Me.SuspendLayout()
        '
        'txtInfo
        '
        resources.ApplyResources(Me.txtInfo, "txtInfo")
        Me.txtInfo.Name = "txtInfo"
        Me.txtInfo.ReadOnly = True
        '
        'btnExport
        '
        resources.ApplyResources(Me.btnExport, "btnExport")
        Me.btnExport.Name = "btnExport"
        Me.btnExport.UseVisualStyleBackColor = True
        '
        'btnImport
        '
        resources.ApplyResources(Me.btnImport, "btnImport")
        Me.btnImport.Name = "btnImport"
        Me.btnImport.UseVisualStyleBackColor = True
        '
        'btnClear
        '
        resources.ApplyResources(Me.btnClear, "btnClear")
        Me.btnClear.Name = "btnClear"
        Me.btnClear.UseVisualStyleBackColor = True
        '
        'ctlProcessBinary
        '
        Me.Controls.Add(Me.btnClear)
        Me.Controls.Add(Me.btnImport)
        Me.Controls.Add(Me.btnExport)
        Me.Controls.Add(Me.txtInfo)
        Me.Name = "ctlProcessBinary"
        resources.ApplyResources(Me, "$this")
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub

#End Region

#Region " Published Events "

    ''' <summary>
    ''' Fired when the underlying value in this control changes
    ''' </summary>
    Public Event Changed As EventHandler Implements IProcessValue.Changed

#End Region

#Region " Member Variables "

    ' Flag indicating the readonly state of this control
    Private mReadOnly As Boolean

    ' The last value set or committed in this control
    Private mLastValue As clsProcessValue

    ' The current byte array represented in this control
    Private mArray As Byte()

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty binary control
    ''' </summary>
    Public Sub New()
        MyBase.New()
        InitializeComponent()
    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the value contained in the control.
    ''' </summary>
    Public Property Value() As clsProcessValue Implements IProcessValue.Value
        Get
            Return mArray
        End Get
        Set(ByVal value As clsProcessValue)
            mLastValue = Nothing
            If value Is Nothing Then value = New clsProcessValue(DataType.binary)
            mArray = CType(value, Byte())
            UpdateUI(value)
            mLastValue = value
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the readonly state of this control
    ''' </summary>
    Private Property [ReadOnly]() As Boolean Implements IProcessValue.ReadOnly
        Get
            Return mReadOnly
        End Get
        Set(ByVal value As Boolean)
            mReadOnly = value
            UpdateUI(Me.Value)
        End Set
    End Property

#End Region

#Region " Event Handlers / Overrides "

    ''' <summary>
    ''' Handles the user clicking on the 'Clear' button
    ''' </summary>
    Private Sub HandleClearClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnClear.Click
        mArray = Nothing
        Commit()
        btnImport.Focus()
    End Sub

    ''' <summary>
    ''' Handles the user clicking on the 'Import' button
    ''' </summary>
    Private Sub HandleImportClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnImport.Click
        Using fo As New OpenFileDialog()
            fo.Title = My.Resources.ImportBinaryData
            fo.Filter = My.Resources.AllFiles
            If fo.ShowDialog() = DialogResult.OK Then
                Try
                    mArray = File.ReadAllBytes(fo.FileName)
                    ' An empty array is a null array
                    If mArray.Length = 0 Then mArray = Nothing
                    Commit()
                Catch ex As Exception
                    UserMessage.Show(String.Format(My.Resources.FailedToImport0, ex.Message))
                End Try
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Handles the user clicking on the 'Export' button
    ''' </summary>
    Private Sub HandleExportClick(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnExport.Click

        'Can't export if there is no data...
        If mArray Is Nothing Then Return

        Using fs As New SaveFileDialog()
            fs.Filter = My.Resources.AllFiles
            fs.Title = My.Resources.ctlProcessBinary_ExportBinaryData
            If fs.ShowDialog() = DialogResult.OK Then
                File.WriteAllBytes(fs.FileName, mArray)
            End If
        End Using

    End Sub

    ''' <summary>
    ''' Raises the <see cref="Changed"/> event
    ''' </summary>
    ''' <param name="e">The args detailing the event</param>
    Protected Overridable Sub OnChanged(ByVal e As EventArgs)
        RaiseEvent Changed(Me, e)
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Selects this control
    ''' </summary>
    Public Sub SelectControl() Implements IProcessValue.SelectControl
        If btnClear.Enabled Then btnClear.Focus() Else btnImport.Focus()
    End Sub

    ''' <summary>
    ''' Updates the UI with the given process value.
    ''' </summary>
    ''' <param name="val">The value to update the UI with</param>
    ''' <exception cref="ArgumentNullException">If the given process value was null,
    ''' ie. it was a null reference, not that it had a null value.</exception>
    Private Sub UpdateUI(ByVal val As clsProcessValue)
        If val Is Nothing Then Throw New ArgumentNullException(NameOf(val))
        txtInfo.Text = val.FormattedValue

        btnClear.Enabled = Not mReadOnly AndAlso Not val.IsNull
        btnImport.Enabled = Not mReadOnly
        btnExport.Enabled = Not val.IsNull
    End Sub

    ''' <summary>
    ''' Commits the changes made in this control
    ''' </summary>
    Public Sub Commit() Implements IProcessValue.Commit
        If mLastValue Is Nothing Then Return
        Dim val As clsProcessValue = Value
        If val.Equals(mLastValue) Then Return

        mLastValue = val
        UpdateUI(val)
        OnChanged(EventArgs.Empty)
    End Sub

#End Region

End Class
