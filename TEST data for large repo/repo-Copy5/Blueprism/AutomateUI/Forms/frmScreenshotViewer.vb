Imports BluePrism.AutomateAppCore
Imports BluePrism.BPCoreLib

Friend Class frmScreenshotViewer
    Inherits frmForm

    ''' <summary>
    ''' Constructor
    ''' </summary>
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

    End Sub

    ''' <summary>
    ''' Sets the resource id, which then starts loading the appropriate screenshot.
    ''' </summary>
    Public Property ResourceId As Guid
        Get
            Return mResourceId
        End Get
        Set(value As Guid)
            mResourceId = value
            ScreenshotLoader.RunWorkerAsync()
        End Set
    End Property
    Private mResourceId As Guid

    ''' <summary>
    ''' Holds a reference to the screenshot image.
    ''' </summary>
    ''' <returns></returns>
    Private Property ScreenshotImage As Image

    ''' <summary>
    ''' Holds a reference to the screenshot details
    ''' </summary>
    Private Property Details As clsScreenshotDetails

    ''' <summary>
    ''' Loads the screenshot image
    ''' </summary>
    Private Sub DoWork(sender As Object, e As DoWorkEventArgs) Handles ScreenshotLoader.DoWork
        ScreenshotLoader.ReportProgress(0)

        Try
            Details = gSv.GetExceptionScreenshot(mResourceId)
        Catch ex As Exception
            UserMessage.Err(My.Resources.UnableToDisplayScreenCapture0, ex.Message)
            Close()
        End Try

        ScreenshotLoader.ReportProgress(30)

        Dim image As New clsPixRect(Details.Screenshot)

        ScreenshotLoader.ReportProgress(80)

        ScreenshotImage = image.ToBitmap()

        ScreenshotLoader.ReportProgress(100)
    End Sub

    ''' <summary>
    ''' Updates the details of the image when the image has loaded
    ''' </summary>
    Private Sub RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles ScreenshotLoader.RunWorkerCompleted
        ScreenshotPictureBox.Image = ScreenshotImage

        UpdateDetails(Details)

        LoadingProgessBar.Visible = False
    End Sub

    ''' <summary>
    ''' Updates details in the context menu.
    ''' </summary>
    ''' <param name="details">The details of the screenshot</param>
    Private Sub UpdateDetails(details As clsScreenshotDetails)
        If details Is Nothing Then Exit Sub

        Dim name = gSv.GetResourceName(ResourceId)
        ResourceLabel.Text = String.Format(My.Resources.Resource0, name)
        LocalTimeLabel.Text = String.Format(My.Resources.UserLocal0, details.Timestamp.ToLocalTime)
        UTCTimeLabel.Text = String.Format(My.Resources.UTCTime0, details.Timestamp.ToUniversalTime)
        ProcessLabel.Text = String.Format(My.Resources.Process0, details.ProcessName)

    End Sub

    ''' <summary>
    ''' Updates the progressbar while the screenshot is loading.
    ''' </summary>
    Private Sub ProgressChanged(sender As Object, e As ProgressChangedEventArgs) Handles ScreenshotLoader.ProgressChanged
        LoadingProgessBar.Value = e.ProgressPercentage
    End Sub

    ''' <summary>
    ''' Saves the screenshot to a file
    ''' </summary>
    Private Sub SaveClick(sender As Object, e As EventArgs) Handles SaveToolStripMenuItem.Click

        Dim image = ScreenshotImage
        If image Is Nothing Then Return

        If SaveFileDialog.ShowDialog = DialogResult.OK Then
            image.Save(SaveFileDialog.FileName, Imaging.ImageFormat.Png)
        End If
    End Sub
End Class