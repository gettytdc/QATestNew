Imports BluePrism.AutomateAppCore.Utility
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateAppCore.Resources
Imports BluePrism.BPCoreLib

''' <summary>
''' A form to display an alert as a pop up message. Note
''' that the form's content and border are created in the
''' paint event.
''' </summary>
''' <remarks></remarks>
Friend Class frmAlertPopUp

#Region "members"

    Private Const ciTextBorder As Integer = 10
    Private Const ciFormBorder As Integer = 5
    Private csTitle As String = String.Format(My.Resources.x0Alert, ApplicationProperties.ApplicationName)
    Private Const csngMaxParaHeight As Single = 5000.0F
    Private Const cdMaxOpacity As Double = 1
    Private Const cdMinOpacity As Double = 0

    Private moAlertEngine As clsAlertEngine

    Private mrWorkingArea As Rectangle
    Private miDisplayTime As Integer
    Private miDisplayTimeRemaining As Integer
    Private miPopUpTime As Integer
    Private mdOpacityStep As Double
    Private miDefaultHeight As Integer

    Private mptMessage As Point
    Private mptTitle As Point
    Private moAlertFont As Font
    Private moTitleFont As Font
    Private mrCloseButton As Rectangle
    Private mrMessage As Rectangle
    Private moAlertQueue As Generic.Queue(Of clsAlertEngine.Alert)
    Private moAlert As clsAlertEngine.Alert
    Private maAlertsToEnqueue As Generic.List(Of clsAlertEngine.Alert)
    Private moGraphics As Graphics
    Private moVisualStyleRenderer As VisualStyles.VisualStyleRenderer
    Private moContextMenu As ContextMenuStrip

    Private Shared maInstances As Generic.List(Of frmAlertPopUp)

    ''' <summary>
    ''' Indicetas whether the local machine is registered for Process Alerts.
    ''' Populated at instantiation, and then never changed.
    ''' </summary>
    ''' <remarks>A machine must register for alerts in the database,
    ''' so that a record can be kept of how many users are using the feature
    ''' (for licensing purposes).</remarks>
    Private mbLocalMachineRegistered As Boolean

#End Region

    ''' <summary>
    ''' Constructor
    ''' </summary>
    ''' <param name="AlertEngine"></param>
    ''' <param name="Alerts"></param>
    ''' <param name="PopUpTime"></param>
    ''' <param name="DisplayTime"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal AlertEngine As clsAlertEngine, ByVal Alerts As Generic.List(Of clsAlertEngine.Alert), Optional ByVal PopUpTime As Integer = 500, Optional ByVal DisplayTime As Integer = 2000)

        MyBase.New()
        Me.InitializeComponent()

        moAlertEngine = AlertEngine

        If Alerts.Count = 0 Then
            Me.Close()
        Else
            miDisplayTime = DisplayTime
            miDisplayTimeRemaining = miDisplayTime
            miPopUpTime = PopUpTime
            mdOpacityStep = (cdMaxOpacity - cdMinOpacity) / (miPopUpTime / moTimer.Interval)

            miDefaultHeight = Me.Height
            mptTitle = New Point(PictureBox1.Location.X + PictureBox1.Width + ciTextBorder, ciTextBorder)
            mptMessage = New Point(ciTextBorder, PictureBox1.Location.Y + PictureBox1.Height + ciTextBorder)

            moAlertQueue = New Generic.Queue(Of clsAlertEngine.Alert)(Alerts)
            moAlert = moAlertQueue.Dequeue
            SetSize(moAlert)

            mrWorkingArea = Screen.PrimaryScreen.WorkingArea
            Me.StartPosition = FormStartPosition.Manual
            Me.Location = New Point(mrWorkingArea.Width - Me.Width - ciFormBorder, mrWorkingArea.Height - Me.Height - ciFormBorder)
            Me.TopMost = True
            Me.Opacity = 0

            AddHandler Me.Paint, AddressOf Me_Paint
            AddHandler Me.MouseClick, AddressOf Me_MouseClick
            AddHandler Me.MouseEnter, AddressOf Me_MouseEnter
            AddHandler Me.MouseLeave, AddressOf Me_MouseLeave
            AddHandler Me.MouseMove, AddressOf Me_MouseMove
            AddHandler Me.FormClosing, AddressOf Me_FormClosing
            AddHandler moTimer.Tick, AddressOf PopUp
            moTimer.Start()

            If maInstances Is Nothing Then
                maInstances = New Generic.List(Of frmAlertPopUp)
            End If
            maInstances.Add(Me)

            Dim machineName = ResourceMachine.GetName()
            Me.mbLocalMachineRegistered = True
            If Not gSv.IsAlertMachineRegistered(machineName) Then
                Try
                    gSv.RegisterAlertMachine(machineName)
                Catch
                    Me.mbLocalMachineRegistered = False
                End Try
            End If

        End If

    End Sub

    ''' <summary>
    ''' Adds to the queue of alerts.
    ''' </summary>
    ''' <param name="Alerts"></param>
    ''' <remarks></remarks>
    Public Sub AddAlerts(ByVal Alerts As Generic.List(Of clsAlertEngine.Alert))

        For Each oAlert As clsAlertEngine.Alert In Alerts
            moAlertQueue.Enqueue(oAlert)
        Next

    End Sub

    ''' <summary>
    ''' Clears the queue of alerts.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub ClearAlerts()

        moAlertQueue.Clear()

    End Sub

    ''' <summary>
    ''' Clears pending alerts in all instances.
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared Sub ClearAllAlerts()

        If maInstances IsNot Nothing Then
            For Each oAlertPopUp As frmAlertPopUp In maInstances
                If oAlertPopUp IsNot Nothing Then
                    oAlertPopUp.ClearAlerts()
                End If
            Next
        End If

    End Sub


    ''' <summary>
    ''' Sets the size of the form according to the message.
    ''' </summary>
    ''' <param name="al">The message</param>
    Private Sub SetSize(ByVal al As clsAlertEngine.Alert)

        If moGraphics Is Nothing Then
            moGraphics = Me.CreateGraphics
        End If

        If moTitleFont Is Nothing Then
            moTitleFont = New Font("Segoe UI", 10, FontStyle.Bold)
        End If

        If moAlertFont Is Nothing Then
            moAlertFont = New Font("Segoe UI", 8)
        End If

        Dim szLayout As New SizeF(CSng(Me.Width - mptMessage.X - ciTextBorder), csngMaxParaHeight)
        Dim szMessage As SizeF = moGraphics.MeasureString(GetMessageText(al), moAlertFont, szLayout)

        Me.Height = miDefaultHeight
        If mptMessage.Y + szMessage.Height + ciTextBorder > Me.Height Then
            Me.Height = CInt(mptMessage.Y + szMessage.Height + ciTextBorder)
        End If

    End Sub

    ''' <summary>
    ''' Gets a message based on an alert.
    ''' </summary>
    ''' <param name="a">The alert</param>
    ''' <returns>The details of the given alert, or a warning message indicating that
    ''' the machine is not registered if that is the case.</returns>
    Private Function GetMessageText(ByVal a As clsAlertEngine.Alert) As String
        If Me.mbLocalMachineRegistered Then
            Return a.GetShortMessage()
        Else
            Return My.Resources.AlertsReceivedButCannotBeDisplayedBecauseThisMachineIsNotRegisteredReconfigureA
        End If
    End Function

    ''' <summary>
    ''' Sets the VisualStyleRenderer object
    ''' </summary>
    ''' <param name="e">The element</param>
    ''' <remarks></remarks>
    Private Sub SetVisualStyleRenderer(ByVal e As VisualStyles.VisualStyleElement)

        If moVisualStyleRenderer Is Nothing Then
            moVisualStyleRenderer = New VisualStyles.VisualStyleRenderer(e)
        Else
            moVisualStyleRenderer.SetParameters(e)
        End If

    End Sub

    ''' <summary>
    ''' Draws the border and message.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Me_Paint(ByVal sender As System.Object, ByVal e As System.Windows.Forms.PaintEventArgs)

        Dim oRectangle As Rectangle = New Rectangle(0, 0, Me.Width, Me.Height)
        Dim oBrush As New Drawing2D.LinearGradientBrush(oRectangle, Color.LightBlue, Color.White, Drawing2D.LinearGradientMode.Vertical)
        e.Graphics.FillRectangle(oBrush, oRectangle)

        'Draw the border.
        e.Graphics.DrawRectangle(Pens.WhiteSmoke, New Rectangle(0, 0, Me.Width - 1, Me.Height - 1))
        e.Graphics.DrawRectangle(Pens.LightGray, New Rectangle(1, 1, Me.Width - 3, Me.Height - 3))
        e.Graphics.DrawRectangle(Pens.WhiteSmoke, New Rectangle(2, 2, Me.Width - 5, Me.Height - 5))
        e.Graphics.DrawLine(Pens.Gray, 0, Me.Height - 1, Me.Width - 1, Me.Height - 1)
        e.Graphics.DrawLine(Pens.Gray, Me.Width - 1, Me.Height - 1, Me.Width - 1, 0)
        e.Graphics.DrawLine(Pens.Gray, 2, Me.Height - 3, 2, 2)
        e.Graphics.DrawLine(Pens.Gray, 2, 2, Me.Width - 3, 2)

        e.Graphics.DrawImage(Me.PictureBox1.Image, Me.PictureBox1.Location)

        ' Draw text to screen.
        Dim sMessage As String = GetMessageText(moAlert)

        ' Set maximum layout size.
        Dim szLayout As New SizeF(CSng(Me.Width - mptMessage.X - ciTextBorder), csngMaxParaHeight)

        ' Measure message.
        Dim szMessage As SizeF = e.Graphics.MeasureString(sMessage, moAlertFont, szLayout)

        e.Graphics.DrawString(csTitle, moTitleFont, Brushes.DarkBlue, mptTitle)
        e.Graphics.DrawString(sMessage, moAlertFont, Brushes.DarkBlue, New RectangleF(mptMessage, szMessage))
        mrMessage = New Rectangle(mptMessage, szMessage.ToSize)

        mrCloseButton = New Rectangle(Me.Width - 18, 3, 15, 15)
        If Application.RenderWithVisualStyles Then
            SetVisualStyleRenderer(VisualStyles.VisualStyleElement.Window.CloseButton.Normal)
            moVisualStyleRenderer.DrawBackground(e.Graphics, mrCloseButton)
        Else
            ControlPaint.DrawCaptionButton(e.Graphics, mrCloseButton, CaptionButton.Close, ButtonState.Normal)
        End If

    End Sub

    ''' <summary>
    ''' Closes the form if the click is on the X, or displays the history.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Me_MouseClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs)
        If e.Button = System.Windows.Forms.MouseButtons.Left Then
            If mrCloseButton.Contains(e.Location) Then
                Me.Close()

            Else
                If mrMessage.Contains(e.Location) Then
                    If Me.mbLocalMachineRegistered Then
                        'If user being shown this message then they have permission
                        'to view process alerts so no need to check again
                        moAlertEngine.ShowHistory()
                    Else
                        'Machine failed to register, so show the appropriate help
                        Try
                            OpenHelpFile(Me, "frmAlertsMachineManager.vb")
                        Catch
                            UserMessage.Err(My.Resources.CannotOpenOfflineHelp)
                        End Try
                    End If
                End If
            End If
        Else
            moContextMenu = New ContextMenuStrip
            moContextMenu.Items.Add(New ToolStripMenuItem(My.Resources.ViewHistory, Nothing, AddressOf Menu_History))
            Dim ConfigureItem As New ToolStripMenuItem(My.Resources.ConfigureAlerts, Nothing, AddressOf Menu_Config)
            ConfigureItem.Enabled = moAlertEngine.AlertsUser.HasPermission("Configure Process Alerts")
            moContextMenu.Items.Add(ConfigureItem)
            Dim IgnoreItem As New ToolStripMenuItem(My.Resources.IgnoreAlertsFromThisSession, Nothing, AddressOf Menu_IgnoreSession)
            IgnoreItem.Enabled = ConfigureItem.Enabled
            moContextMenu.Items.Add(IgnoreItem)

            moContextMenu.Show(Me, e.Location)
        End If
    End Sub

    ''' <summary>
    ''' Displays the frmAlertConfig history page.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Menu_History(ByVal sender As System.Object, ByVal e As System.EventArgs)
        moContextMenu = Nothing
        Me.Refresh()
        moAlertEngine.ShowHistory()
    End Sub

    ''' <summary>
    ''' Displays the frmAlertConfig config page.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Menu_Config(ByVal sender As System.Object, ByVal e As System.EventArgs)
        moContextMenu = Nothing
        Me.Refresh()
        moAlertEngine.ShowConfig()
    End Sub

    Private Sub Menu_IgnoreSession(ByVal sender As System.Object, ByVal e As System.EventArgs)
        moContextMenu = Nothing
        Me.Refresh()

        Try
            'Tell the alert engine to ignore this session.
            moAlertEngine.IgnoreSession(moAlert.SessionID)

            'Remove any queued alerts from the session.
            moTimer.Stop()

            Dim aTempQueue As clsAlertEngine.Alert() = moAlertQueue.ToArray()
            moAlertQueue = New Generic.Queue(Of clsAlertEngine.Alert)

            For Each a As clsAlertEngine.Alert In aTempQueue
                If Not a.SessionID.Equals(moAlert.SessionID) Then
                    moAlertQueue.Enqueue(a)
                End If
            Next

            moTimer.Start()

        Catch ex As Exception
            UserMessage.Show(
             My.Resources.BluePrismAlertsHasEncounteredAProblemWhileUpdatingUserDetails, ex)
        End Try

    End Sub

    ''' <summary>
    ''' Imitates a hyperlink.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Me_MouseMove(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs)

        If mrMessage.Contains(e.Location) Then
            Cursor = Cursors.Hand
        Else
            Cursor = Cursors.Default
        End If

    End Sub

    ''' <summary>
    ''' Pauses the pop up or pop down.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Me_MouseEnter(ByVal sender As System.Object, ByVal e As System.EventArgs)
        moTimer.Stop()
    End Sub

    ''' <summary>
    ''' Restarts the pop up or pop down.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Me_MouseLeave(ByVal sender As System.Object, ByVal e As System.EventArgs)

        If moContextMenu Is Nothing Then
            moTimer.Start()
        End If

    End Sub

    ''' <summary>
    ''' Close the form by pop down.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub Me_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs)

        e.Cancel = True
        moTimer.Stop()
        RemoveHandler moTimer.Tick, AddressOf PopUp
        AddHandler moTimer.Tick, AddressOf PopDown
        moTimer.Start()

    End Sub

    ''' <summary>
    ''' Pops the form up.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub PopUp(ByVal sender As System.Object, ByVal e As System.EventArgs)

        If Me.Opacity < cdMaxOpacity Then
            Me.Opacity += mdOpacityStep
        Else
            If miDisplayTimeRemaining > 0 Then
                miDisplayTimeRemaining -= moTimer.Interval
            Else
                Me.Close()
            End If
        End If

    End Sub

    ''' <summary>
    ''' Pops the form down.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub PopDown(ByVal sender As System.Object, ByVal e As System.EventArgs)

        If Me.Opacity > cdMinOpacity Then
            Me.Opacity -= mdOpacityStep
        Else
            If moAlertQueue.Count = 0 Then
                moTimer.Stop()
                RemoveHandler Me.FormClosing, AddressOf Me.Me_FormClosing
                maInstances.Remove(Me)
                Me.Close()
            Else
                moTimer.Stop()
                miDisplayTimeRemaining = miDisplayTime
                moAlert = moAlertQueue.Dequeue
                SetSize(moAlert)
                RemoveHandler moTimer.Tick, AddressOf PopDown
                AddHandler moTimer.Tick, AddressOf PopUp
                moTimer.Start()
            End If
        End If

    End Sub

End Class
