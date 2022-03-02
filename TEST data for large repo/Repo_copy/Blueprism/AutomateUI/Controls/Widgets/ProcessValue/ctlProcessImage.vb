Imports System.IO
Imports System.Drawing.Imaging
Imports BluePrism.AutomateProcessCore

''' <summary>
''' This control allows a user to edit an automate image.
''' </summary>
Friend Class ctlProcessImage : Inherits UserControl : Implements IProcessValue

#Region " Published Events "

    ''' <summary>
    ''' Event fired when the underlying value of this control has changed
    ''' </summary>
    Public Event Changed As EventHandler Implements IProcessValue.Changed

#End Region

#Region " Member Variables "

    ' Flag indicating the readonly state of this control
    Private mReadOnly As Boolean

    ' The current value of this control
    Private mValue As clsProcessValue

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new empty image value control
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
            Return mValue
        End Get
        Set(ByVal Value As clsProcessValue)
            mValue = Value
            UpdateControls()
        End Set
    End Property

    ''' <summary>
    ''' Gets or sets the ReadOnly state of this control
    ''' </summary>
    Public Property [ReadOnly]() As Boolean Implements IProcessValue.ReadOnly
        Get
            Return mReadOnly
        End Get
        Set(ByVal value As Boolean)
            If value = mReadOnly Then Return
            mReadOnly = value
            UpdateControls()
        End Set
    End Property

#End Region

#Region " Event Handlers/Senders "

    ''' <summary>
    ''' Raises the <see cref="Changed"/> event
    ''' </summary>
    ''' <param name="e">The arguments detailing the event</param>
    Protected Overridable Sub OnChanged(ByVal e As EventArgs)
        RaiseEvent Changed(Me, e)
    End Sub

    ''' <summary>
    ''' Handles the Clear button being clicked
    ''' </summary>
    Private Sub btnClear_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnClear.Click
        Value = New clsProcessValue(DataType.image)
        UpdateControls()
        OnChanged(EventArgs.Empty)
        btnImport.Focus()
    End Sub

    ''' <summary>
    ''' Handles the View button being clicked
    ''' </summary>
    Private Sub btnView_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnView.Click
        If mValue.IsNull Then Return
        Using b As Bitmap = CType(mValue, Bitmap)
            Using f As New frmProcessImageViewer(b)
                f.Owner = TryCast(TopLevelControl, Form)
                f.StartPosition = FormStartPosition.CenterParent
                f.ShowInTaskbar = False
                f.ShowDialog()
            End Using
        End Using
    End Sub

    ''' <summary>
    ''' Handles the Import button being clicked
    ''' </summary>
    Private Sub btnImport_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnImport.Click
        Using fo As New OpenFileDialog()
            fo.Title = My.Resources.ctlProcessImage_ImportImage
            fo.Filter = My.Resources.ctlProcessImage_AllFilesBitmapFilesBmpGifJpgPng
            If fo.ShowDialog() = DialogResult.OK Then
                Try
                    Using b As New Bitmap(fo.FileName) : Value = b : End Using
                    UpdateControls()
                    OnChanged(EventArgs.Empty)
                Catch ex As Exception
                    UserMessage.Err(ex, My.Resources.ctlProcessImage_FailedToImport0, ex.Message)
                End Try
            End If
        End Using
    End Sub

    ''' <summary>
    ''' Handles the Export button being clicked
    ''' </summary>
    Private Sub btnExport_Click(ByVal sender As Object, ByVal e As EventArgs) _
     Handles btnExport.Click

        'Can't export if there is no image...
        If mValue.IsNull Then Return

        Using fs As New SaveFileDialog()
            fs.Filter =
             My.Resources.ctlProcessImage_PNGImagePngBitmapImageBmpGifImageGifJPEGImageJpg
            fs.Title = My.Resources.ctlProcessImage_ExportImage
            If fs.ShowDialog() = DialogResult.OK Then
                Try
                    Using b As Bitmap = CType(mValue, Bitmap)
                        Using s As FileStream = CType(fs.OpenFile(), FileStream)
                            Select Case fs.FilterIndex
                                Case 1 : b.Save(s, ImageFormat.Png)
                                Case 2 : b.Save(s, ImageFormat.Bmp)
                                Case 3 : b.Save(s, ImageFormat.Gif)
                                Case 4 : b.Save(s, ImageFormat.Jpeg)
                            End Select
                        End Using
                    End Using
                Catch ex As Exception
                    UserMessage.Err(ex, My.Resources.ctlProcessImage_FailedToExport0, ex.Message)
                End Try
            End If
        End Using
    End Sub

#End Region

#Region " Other Methods "

    ''' <summary>
    ''' Populate the info box with a 'description' of the image.
    ''' </summary>
    Private Sub UpdateControls()
        txtInfo.Text = mValue.FormattedValue

        ' View if there is a value to view
        btnView.Enabled = Not mValue.IsNull

        ' Clear if there's a value to clear and it's not readonly
        btnClear.Enabled = Not mValue.IsNull AndAlso Not mReadOnly

        ' Import whether there's a value there or not as long as it's not readonly
        btnImport.Enabled = Not mReadOnly

        ' Export if there is a value to view/export
        btnExport.Enabled = Not mValue.IsNull

    End Sub

    ''' <summary>
    ''' Selects this control by focusing on the first available button
    ''' </summary>
    Public Sub SelectControl() Implements IProcessValue.SelectControl
        If btnClear.Enabled Then btnClear.Focus() : Return
        If btnView.Enabled Then btnView.Focus() : Return
        btnImport.Focus()
    End Sub

    ''' <summary>
    ''' Commits the changes made in this control
    ''' </summary>
    Public Sub Commit() Implements IProcessValue.Commit
        ' The committing is done at each point that any action is performed within
        ' this control, so there's no work to do here
    End Sub

#End Region

End Class
