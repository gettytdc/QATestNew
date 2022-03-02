Public Class WorkQueueItemAttemptViewer

    Public Sub New(ByVal instanceList As ctlWorkQueueInstanceList)

        InitializeComponent()

        Text = String.Format(My.Resources.ctlWorkQueueContents_WorkQueueItems)
        titleBar.Title = String.Format(My.Resources.ctlWorkQueueContents_WorkQueueItems)

        tableLayout.Controls.Add(instanceList, 0, 1)
        instanceList.Dock = DockStyle.Fill
        instanceList.BringToFront()
        Size = instanceList.PreferredSize

        ' The title bar needs to be visible
        Height = Size.Height + titleBar.Height

        ' Just make sure that we're still within the screen bounds.
        Dim working As Rectangle = Screen.PrimaryScreen.WorkingArea
        If Left + Width > working.Width Then
            Width = working.Width - Left
        End If
        If Top + Height > working.Height Then
            Height = working.Height - Top
        End If

    End Sub

End Class
