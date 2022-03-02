Imports AutomateControls
Imports BluePrism.AutomateAppCore
Imports BluePrism.BPCoreLib
Imports NLog

''' Project  : Automate
''' Class    : UserMessage
''' 
''' <summary>
''' A class used to display user messages.
''' </summary>
''' <remarks>
''' This code is not compatible with unit testing. Any new code MUST use IUserMessage
''' </remarks>
Public Class UserMessage

    Private Shared ReadOnly Log As Logger = LogManager.GetCurrentClassLogger()

    Const MaxLineLength As Integer = 80    'used in autoresized message boxes
    Const DetailShowButtonWidth As Integer = 95

    Private Shared WithEvents mobjFrmErrorMessage As frmErrorMessage
    Private Shared mResult As MsgBoxResult

    Private Shared _Spacer As Integer
    Friend Const ExceptionDetailBoxName As String = "txtDetail"

    ''' <summary>
    ''' Shows a message to the user
    ''' </summary>
    ''' <param name="prompt">The message to show</param>
    Public Shared Sub Show(ByVal prompt As String)
        Show(Nothing, prompt)
    End Sub

    ''' <summary>
    ''' Shows a user message.
    ''' </summary>
    ''' <param name="owner">The owner control, the top level window of which will
    ''' be nominated as the parent form of the resultant dialog</param>
    ''' <param name="prompt">The message</param>
    Public Shared Sub Show(ByVal owner As Control, ByVal prompt As String)
        Show(owner, prompt, -1, "")
    End Sub

    ''' <summary>
    ''' Shows a user message.
    ''' </summary>
    ''' <param name="prompt">The message</param>
    ''' <param name="helpTopicNo">The number of the help topic to display
    ''' if the user clicks the help button.</param>
    Public Shared Sub Show(ByVal prompt As String, ByVal helpTopicNo As Integer)
        Show(Nothing, prompt, helpTopicNo, "")
    End Sub

    ''' <summary>
    ''' Shows an exception message
    ''' </summary>
    ''' <param name="exception">The exception</param>
    Public Shared Sub ShowExceptionMessage(exception As Exception)
        Show(Nothing, String.Format(My.Resources.FollowingErrorHasOccuredMessage, Environment.NewLine, exception.Message))
    End Sub

    ''' <summary>
    ''' Shows the message indicating that the user doesn't have permission to perform
    ''' a particular action.
    ''' </summary>
    Public Shared Sub ShowPermissionMessage()
        Show(
         String.Format(My.Resources.YouDoNotHavePermissionToPerformThatActionIfYouBelieveThatThisIsIncorrectThenPle, ApplicationProperties.ApplicationName), 1048586)
    End Sub


    Public Shared Sub ShowCantExecuteProcessMessage()
        Show(
        My.Resources.AutomateUIClasses_Resources.ExecuteDenied, 1048586)
    End Sub

    ''' <summary>
    ''' Shows a user message.
    ''' </summary>
    ''' <param name="prompt">The message</param>
    ''' <param name="helpTopicNo">The number of the help topic to display
    ''' if the user clicks the help button.</param>
    Public Shared Sub Show(
     ByVal prompt As String, ByVal helpTopicNo As Integer, ByVal helpPage As String)
        Show(Nothing, prompt, helpTopicNo, helpPage)
    End Sub

    ''' <summary>
    ''' Shows a user message.
    ''' </summary>
    ''' <param name="owner">The owner control, the top level window of which will
    ''' be nominated as the parent form of the resultant dialog</param>
    ''' <param name="prompt">The message</param>
    ''' <param name="helpTopicNo">The number of the help topic to display
    ''' if the user clicks the help button.</param>
    Public Shared Sub Show(ByVal owner As Control,
      ByVal prompt As String, ByVal helpTopicNo As Integer, ByVal helpPage As String)
        If Environment.UserInteractive Then
            Using f As Form = CreateDialog(prompt, helpTopicNo, helpPage)
                Dim ownerWindow As Control = Nothing
                If owner IsNot Nothing Then ownerWindow = owner.TopLevelControl
                f.ShowInTaskbar = False
                f.StartPosition = FormStartPosition.CenterParent
                f.ShowDialog(ownerWindow)
            End Using
        Else
            'Log if not interactive
            Log.Warn("User message not valid in non-interactive environment: {0}", prompt)
        End If
    End Sub

    ''' <summary>
    ''' Creates a user message dialog
    ''' </summary>
    ''' <param name="Prompt">The prompt to display to the user</param>
    ''' <param name="HelpTopic">The help topic</param>
    ''' <param name="HelpPage">The help page</param>
    ''' <returns>A frmErrorMessage instance for displaying a message to the user.
    ''' </returns>
    Private Shared Function CreateDialog(ByVal Prompt As String,
     Optional ByVal HelpTopic As Integer = -1, Optional ByVal HelpPage As String = "") _
     As frmErrorMessage

        Dim errmsg As New frmErrorMessage(Prompt)
        errmsg.BringToFront()
        errmsg.HelpTopicNumber = HelpTopic
        errmsg.HelpPage = HelpPage
        Return errmsg
    End Function

    ''' <summary>
    ''' Shows the given parameterized error message
    ''' </summary>
    ''' <param name="msg">The message to display</param>
    ''' <returns>False, whenever called. This just provides a shortcut for the common
    ''' requirement to display an error and return false from a validate method or
    ''' similar.</returns>
    Public Shared Function Err(ByVal msg As String) As Boolean
        Return Err(Nothing, msg)
    End Function

    ''' <summary>
    ''' Shows the given parameterized error message
    ''' </summary>
    ''' <param name="msg">The message with argument placeholders</param>
    ''' <param name="args">The arguments informing the message</param>
    ''' <returns>False, whenever called. This just provides a shortcut for the common
    ''' requirement to display an error and return false from a validate method or
    ''' similar.</returns>
    Public Shared Function Err(
     ByVal msg As String, ByVal ParamArray args() As Object) As Boolean
        Return Err(Nothing, String.Format(msg, args))
    End Function

    ''' <summary>
    ''' Shows the given parameterized error message
    ''' </summary>
    ''' <param name="ex">The exception which initiated this error</param>
    ''' <param name="msg">The message with argument placeholders</param>
    ''' <param name="args">The arguments informing the message</param>
    ''' <returns>False, whenever called. This just provides a shortcut for the common
    ''' requirement to display an error and return false from a validate method or
    ''' similar.</returns>
    Public Shared Function Err(ByVal ex As Exception,
     ByVal msg As String, ByVal ParamArray args() As Object) As Boolean
        Return Err(ex, String.Format(msg, args))
    End Function

    ''' <summary>
    ''' Shows the given parameterized error message
    ''' </summary>
    ''' <param name="ex">The exception which initiated this error</param>
    ''' <param name="msg">The message to display</param>
    ''' <returns>False, whenever called. This just provides a shortcut for the common
    ''' requirement to display an error and return false from a validate method or
    ''' similar.</returns>
    Public Shared Function Err(ByVal ex As Exception, ByVal msg As String) As Boolean
        Show(msg, ex)
        Return False
    End Function

    ''' <summary>
    ''' Creates a message dialog with a populated detail section.
    ''' </summary>
    ''' <param name="prompt">The text to prompt the user with</param>
    ''' <param name="detailText">The text to set in the detail section</param>
    ''' <param name="helpTopic">The help topic, or -1 if no help topic is known /
    ''' appropriate</param>
    ''' <param name="helpPage">The help page or an empty string if not known /
    ''' appropriate</param>
    ''' <returns>An unshown error message form with the specified attributes.
    ''' </returns>
    Private Shared Function CreateDetailDialog(
     ByVal prompt As String, ByVal detailText As String,
     ByVal helpTopic As Integer, ByVal helpPage As String) As frmErrorMessage

        Dim f As frmErrorMessage = CreateDialog(prompt, helpTopic, helpPage)

        Dim pan As Panel = CType(f.Controls("pnlError"), Panel)
        Dim txtMessage As TextBox = CType(pan.Controls("txtMessage"), TextBox)
        Dim btnCopy As Button = CType(pan.Controls("btnCopyToClipboard"), Button)
        Dim txtDetail As New TextBox()
        With txtDetail
            .Multiline = True
            .Size = txtMessage.Size
            .Location = New Point(pan.Left, pan.Bottom)
            .Name = ExceptionDetailBoxName
            .ReadOnly = True
            .Text = detailText
            .BackColor = Color.White
            .ScrollBars = ScrollBars.Vertical
            .Visible = False
        End With
        f.Controls.Add(txtDetail)

        _Spacer = f.ClientSize.Height - btnCopy.Bottom

        Dim b As New AutomateControls.Buttons.StandardStyledButton()
        b.FlatStyle = FlatStyle.System
        b.Text = My.Resources.btnDetailShow
        b.Location = New Point(btnCopy.Right + _Spacer, btnCopy.Top)
        b.Height = btnCopy.Height
        b.Width = DetailShowButtonWidth
        AddHandler b.Click, AddressOf HandleDetailClick
        pan.Controls.Add(b)

        Return f

    End Function

    ''' <summary>
    ''' Shows the given prompt and optionally the exception detail.
    ''' </summary>
    ''' <param name="prompt">The prompt to show to the user</param>
    ''' <param name="ex">The exception to show in the detail box, or null if no
    ''' exception (hence no detail) should be shown</param>
    ''' <param name="topic">The help topic number, or -1 if not known/appropriate.
    ''' </param>
    ''' <param name="helpPage">The name/path to the help page or an empty string if
    ''' not known/appropriate.</param>
    Public Shared Sub Show(ByVal prompt As String, ByVal ex As Exception,
     Optional ByVal topic As Integer = -1, Optional ByVal helpPage As String = "")

        ' If there's no exception here, just use the default Show() method
        If ex Is Nothing Then Show(prompt, topic, helpPage) : Return

        If Not Integer.TryParse(ex.HelpLink, topic) Then topic = -1

        If Environment.UserInteractive Then
            Using f As frmErrorMessage =
             CreateDetailDialog(prompt, ex.ToString(), topic, ex.HelpLink)
                f.ShowInTaskbar = False
                f.StartPosition = FormStartPosition.CenterParent
                f.ShowDialog()
            End Using
        Else
            'Log if not interactive
            Log.Warn("User message not valid in non-interactive environment: {0} - {1}", prompt, ex)
        End If

    End Sub

    ''' <summary>
    ''' Shows the given prompt and detail text.
    ''' </summary>
    ''' <param name="prompt">The prompt to show to the user</param>
    ''' <param name="detail">The text to show in the detail box</param>
    ''' <param name="topic">The help topic number, or -1 if not known/appropriate.
    ''' </param>
    ''' <param name="helpPage">The name/path to the help page or an empty string if
    ''' not known/appropriate.</param>
    Public Shared Sub ShowDetail(ByVal prompt As String, ByVal detail As String,
     Optional ByVal topic As Integer = -1, Optional ByVal helpPage As String = "")

        If Environment.UserInteractive Then
            Using f As frmErrorMessage =
               CreateDetailDialog(prompt, detail, topic, helpPage)
                f.ShowInTaskbar = False
                f.StartPosition = FormStartPosition.CenterParent
                f.ShowDialog()
            End Using
        Else
            'Log if not interactive
            Log.Warn("User message not valid in non-interactive environment: {0} - {1}", prompt, detail)
        End If

    End Sub

    ''' <summary>
    ''' Handles the detail button created by <see cref="CreateDetailDialog"/> being
    ''' pressed by showing/hiding the detail text box.
    ''' </summary>
    Private Shared Sub HandleDetailClick(ByVal sender As Object, ByVal e As EventArgs)

        Dim btnDetail As Button = CType(sender, Button)
        Dim frm As Control = btnDetail.TopLevelControl
        Dim pan As Panel = CType(frm.Controls("pnlError"), Panel)
        Dim txtPrompt As TextBox = CType(pan.Controls("txtMessage"), TextBox)
        Dim btnCopy As Button = CType(pan.Controls("btnCopyToClipboard"), Button)
        Dim txtDetail As TextBox = CType(frm.Controls(ExceptionDetailBoxName), TextBox)

        If btnDetail.Text = My.Resources.btnDetailShow Then
            btnDetail.Text = My.Resources.btnDetailHide
            frm.ClientSize =
             New Size(frm.ClientSize.Width, txtDetail.Bottom + _Spacer)
            txtDetail.Visible = True

        Else
            btnDetail.Text = My.Resources.btnDetailShow
            frm.ClientSize =
             New Size(frm.ClientSize.Width, pan.Top + btnCopy.Bottom + _Spacer)
            txtDetail.Visible = False

        End If

    End Sub


    ''' <summary>
    ''' Shows user a message and places the message box at the specified location.
    ''' </summary>
    ''' <param name="sPrompt">The message to show.</param>
    ''' <param name="xCoord">The X screen of the upper left hand corner of the form.
    ''' </param>
    ''' <param name="yCoord">The Y screen of the upper left hand corner of the form.</param>
    Public Shared Sub Show(ByVal sPrompt As String, ByVal xCoord As Integer, ByVal yCoord As Integer)
        UserMessage.Show(sPrompt, New Point(xCoord, yCoord))
    End Sub

    ''' <summary>
    ''' Shows the user a message and places the form at the specified location.
    ''' </summary>
    ''' <param name="sPrompt">The message to show.</param>
    ''' <param name="Location">The location in screen coordinates of where to place
    ''' the form</param>
    Public Shared Sub Show(ByVal sPrompt As String, ByVal Location As Point)
        If (Not Location.Equals(Point.Empty)) AndAlso (Location.X > 0) AndAlso (Location.Y > 0) Then
            Dim objErrorMessage As New frmErrorMessage(sPrompt)
            objErrorMessage.StartPosition = FormStartPosition.Manual
            objErrorMessage.Location = Location
            objErrorMessage.BringToFront()
            objErrorMessage.ShowInTaskbar = False
            objErrorMessage.ShowDialog()
            objErrorMessage.Dispose()
        Else
            UserMessage.Show(sPrompt)
        End If
    End Sub

    ''' <summary>
    ''' Shows a floating tooltip with the supplied message.
    ''' </summary>
    ''' <param name="Parent">The control or form with which to 
    ''' associate the tooltip. Must not be null.</param>
    ''' <param name="sTitle">The title to display on the tooltip.</param>
    ''' <param name="sPrompt">The message to display.</param>
    ''' <param name="Location">The location relative to the parent.</param>
    ''' <param name="Duration">The time for which the tooltip
    ''' should be visible, in milliseconds.</param>
    Public Shared Sub ShowFloating(ByVal Parent As Control, ByVal Icon As ToolTipIcon, ByVal sTitle As String, ByVal sPrompt As String, ByVal Location As Point, Optional ByVal Duration As Integer = DefaultFloatingDuration, Optional ByVal InBalloonStyle As Boolean = False)
        CancelFloatingMessage(Parent)
        mTooltip = New ToolTip
        mTooltip.ToolTipTitle = sTitle
        mTooltip.InitialDelay = 0
        mTooltip.AutoPopDelay = 0
        mTooltip.ToolTipIcon = Icon
        mTooltip.AutomaticDelay = 0
        mTooltip.IsBalloon = InBalloonStyle
        AddHandler mTooltip.Disposed, AddressOf HandleTooltipParentDisposed
        AddHandler Parent.Disposed, AddressOf HandleTooltipParentDisposed

        'We call this twice to work around a .NET bug described at:
        'https://connect.microsoft.com/visualstudio/feedback/viewfeedback.aspx?feedbackid=98281
        mTooltip.Active = False
        mTooltip.Show(sPrompt, Parent, Location, Duration)
        mTooltip.Active = True
        mTooltip.Show(sPrompt, Parent, Location, Duration)
    End Sub

    ''' <summary>
    ''' The duration, in milliseconds of the tooltip message,
    ''' when none is specified by the client.
    ''' </summary>
    Friend Const DefaultFloatingDuration As Integer = 5000

    Public Shared Sub ShowFloating(ByVal Parent As Control, ByVal Icon As ToolTipIcon, ByVal sTitle As String, ByVal sPrompt As String, ByVal Location As Point, ByVal InBalloonStyle As Boolean)
        ShowFloating(Parent, Icon, sTitle, sPrompt, Location, DefaultFloatingDuration, InBalloonStyle)
    End Sub


    ''' <summary>
    ''' Cancels any visible floating message
    ''' </summary>
    ''' <param name="Parent">The control whose message is to be canceled,
    ''' or nothing to clear all.</param>
    Public Shared Sub CancelFloatingMessage(ByVal Parent As Control)
        If mTooltip IsNot Nothing Then
            If Parent IsNot Nothing Then
                mTooltip.Hide(Parent)
            Else
                HandleTooltipParentDisposed(Nothing, EventArgs.Empty)
            End If
        End If
    End Sub

    ''' <summary>
    ''' Tooltip used for showing messages to user.
    ''' </summary>
    Private Shared mTooltip As ToolTip


    Private Shared Sub HandleTooltipParentDisposed(ByVal sender As Object, ByVal e As EventArgs)
        Select Case True
            Case TypeOf sender Is ToolTip
                RemoveHandler CType(sender, ToolTip).Disposed, AddressOf HandleTooltipParentDisposed
            Case TypeOf sender Is Control
                RemoveHandler CType(sender, Control).Disposed, AddressOf HandleTooltipParentDisposed
        End Select
        If Not mTooltip Is Nothing Then
            mTooltip.Dispose()
            mTooltip = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Gets the width of the form that would be displayed simply by calling
    ''' the Show() method.
    ''' </summary>
    ''' <returns>The width.</returns>
    Public Shared Function GetDefaultWidthOfUserMessageForm() As Integer
        Dim dummy As New frmErrorMessage("")
        Return dummy.Width
    End Function

    ''' <summary>
    ''' Shows a message using frmErrorMessage.
    ''' </summary>
    ''' <param name="sPrompt">The message</param>
    ''' <param name="sComment">The comment</param>
    ''' <param name="BluebarTitle">The form title</param>
    ''' <param name="WindowWidth">The form width</param>
    ''' <param name="WindowHeight">The form height</param>
    Public Shared Sub ShowBlueBarMessage(ByVal sPrompt As String, ByVal sComment As String, ByVal BluebarTitle As String, ByVal colorManager As IEnvironmentColourManager, Optional ByVal HelpTopicNumber As Integer = -1, Optional ByVal HelpPage As String = "", Optional ByVal WindowWidth As Integer = 0, Optional ByVal WindowHeight As Integer = 0)
        Using f As New frmErrorMessage(sPrompt, Comment:=sComment, type:=frmErrorMessage.MessageType.BlueBarOkMessage)
            f.SetEnvironmentColours(colorManager)
            f.HelpTopicNumber = HelpTopicNumber
            f.HelpPage = HelpPage

            If (WindowWidth = 0 OrElse WindowHeight = 0) Then
                f.Size = New Size(600, 480)
            Else
                f.Size = New Size(WindowWidth, WindowHeight)
            End If

            f.objBlueBar.Title = BluebarTitle

            f.pnlBlueBarMessage.Location = New Point(0, 0)
            f.pnlBlueBarMessage.Size = f.ClientSize

            f.FormBorderStyle = FormBorderStyle.Sizable
            f.BringToFront()
            f.ShowInTaskbar = False
            f.StartPosition = FormStartPosition.CenterParent
            f.ShowDialog()
        End Using
    End Sub

    Private Shared Function ShowDialog(ByVal sPrompt As String, ByVal type As frmErrorMessage.MessageType, Optional ByVal bAutoResize As Boolean = False, Optional ByVal ComboBoxItems As ICollection(Of String) = Nothing, Optional ByRef ComboBoxText As String = "", Optional ByVal sCheckboxPrompt As String = "", Optional ByVal Handler As EventHandler = Nothing, Optional ByVal LinkLabelText As String = "", Optional ByVal YesButtonText As String = "", Optional ByVal NoButtonText As String = "", Optional ByVal OKButtonText As String = "", Optional ByVal CancelButtonText As String = "") As MsgBoxResult

        Dim messagelabelheight As Integer
        Dim checkboxlabelheight As Integer
        Dim checkboxlabelwidth As Integer
        Dim messagelabelwidth As Integer
        Dim innerpanelheight As Integer
        Dim innerpanelwidth As Integer
        Dim NumLines As Integer

        Select Case type

            Case frmErrorMessage.MessageType.YesNoCancelMessage

                mobjFrmErrorMessage = New frmErrorMessage(sPrompt, type)
                mobjFrmErrorMessage.lblYesNoCancel.MaximumSize = New Size(mobjFrmErrorMessage.Width - 16 - mobjFrmErrorMessage.lblYesNoCancel.Left, 0)
                mobjFrmErrorMessage.pnlYesNoCancel.Height += 60

                mobjFrmErrorMessage.lblYesNoCancel.AutoSize = True
                mobjFrmErrorMessage.AutoSize = True

            Case frmErrorMessage.MessageType.YesNoCheckBoxMessage
                Dim numcheckboxlines As Integer
                If bAutoResize Then
                    NumLines = FormatFixedWidth(sPrompt)
                    numcheckboxlines = FormatFixedWidth(sCheckboxPrompt)
                End If
                mobjFrmErrorMessage = New frmErrorMessage(sPrompt, type, sCheckboxPrompt)
                If bAutoResize Then
                    messagelabelheight = NumLines * 14
                    messagelabelwidth = CInt(MaxLineLength * 5.1)
                    checkboxlabelheight = numcheckboxlines * 14
                    checkboxlabelwidth = CInt(MaxLineLength * 5.1 - 15)
                    innerpanelheight = messagelabelheight + checkboxlabelheight + 60
                    innerpanelwidth = messagelabelwidth + 20
                Else
                    mobjFrmErrorMessage = New frmErrorMessage(sPrompt, type)
                    messagelabelheight = 80
                    messagelabelwidth = 300
                    innerpanelheight = messagelabelheight + 40
                    innerpanelwidth = messagelabelwidth + 10
                End If
                mobjFrmErrorMessage.Width = innerpanelwidth + 30
                mobjFrmErrorMessage.Height = innerpanelheight + 40

                mobjFrmErrorMessage.pnlYesNoCheckBox.Width = innerpanelwidth
                mobjFrmErrorMessage.pnlYesNoCheckBox.Height = innerpanelheight

                mobjFrmErrorMessage.lblYesNoCheckBox.Width = messagelabelwidth
                mobjFrmErrorMessage.lblYesNoCheckBox.Height = messagelabelheight

                mobjFrmErrorMessage.lblYesNoCheckboxCaption.Width = checkboxlabelwidth
                mobjFrmErrorMessage.lblYesNoCheckboxCaption.Height = checkboxlabelheight

                mobjFrmErrorMessage.lblYesNoCheckboxCaption.Top = mobjFrmErrorMessage.lblYesNoCheckBox.Top + mobjFrmErrorMessage.lblYesNoCheckBox.Height + 20
                mobjFrmErrorMessage.chkbxYesNoCheckbox.Top = mobjFrmErrorMessage.lblYesNoCheckBox.Top + mobjFrmErrorMessage.lblYesNoCheckBox.Height + 20
                mobjFrmErrorMessage.pnlyesnocheckboxbuttons.Top = mobjFrmErrorMessage.lblYesNoCheckboxCaption.Top + mobjFrmErrorMessage.lblYesNoCheckboxCaption.Height + 5
                mobjFrmErrorMessage.pnlYesNoCheckBox.Left = mobjFrmErrorMessage.Left + mobjFrmErrorMessage.Width \ 2 - mobjFrmErrorMessage.pnlYesNoCheckBox.Width \ 2
                mobjFrmErrorMessage.pnlyesnocheckboxbuttons.Left = mobjFrmErrorMessage.pnlYesNoCheckBox.Left + mobjFrmErrorMessage.pnlYesNoCheckBox.Width \ 2 - mobjFrmErrorMessage.pnlyesnocheckboxbuttons.Width \ 2
                mobjFrmErrorMessage.lblYesNoCheckBox.Left = mobjFrmErrorMessage.pnlYesNoCheckBox.Left

            Case frmErrorMessage.MessageType.YesNoCancelLinkLabelMessage
                mobjFrmErrorMessage = New frmErrorMessage(sPrompt, type)
                AddHandler mobjFrmErrorMessage.llYesNoLink.Click, Handler

                mobjFrmErrorMessage.llYesNoLink.Text = LinkLabelText
                If Not YesButtonText = "" Then mobjFrmErrorMessage.btnYesNoCancelLinkYes.Text = YesButtonText
                If Not NoButtonText = "" Then mobjFrmErrorMessage.btnYesNoCancelLinkNo.Text = NoButtonText
                If Not CancelButtonText = "" Then mobjFrmErrorMessage.btnYesNoCancelLinkCancel.Text = CancelButtonText


            Case frmErrorMessage.MessageType.YesNoMessage
                mobjFrmErrorMessage = New frmErrorMessage(sPrompt, type)

                'Find out how much space needed for message
                Dim g As Graphics = mobjFrmErrorMessage.lblYesNo.CreateGraphics
                Dim l As Label = mobjFrmErrorMessage.lblYesNo
                Dim S As SizeF = g.MeasureString(l.Text, l.Font, New Size(l.Width, Integer.MaxValue), StringFormat.GenericDefault)

                'Anchor the panel and resize it accordingly - everything else should follow automatically.
                Dim Offset As Integer = CInt(S.Height) - l.Size.Height
                mobjFrmErrorMessage.pnlYesNo.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
                mobjFrmErrorMessage.Height += Offset

                'Apply optional button text changes
                If Not YesButtonText = "" Then mobjFrmErrorMessage.btnYes1.Text = YesButtonText
                If Not NoButtonText = "" Then mobjFrmErrorMessage.btnNo1.Text = NoButtonText

            Case frmErrorMessage.MessageType.OkCancelWithComboBoxMessage
                mobjFrmErrorMessage = New frmErrorMessage(sPrompt, type)
                mobjFrmErrorMessage.lblOKCancelWithComboBoxPrompt.MaximumSize = New Size(mobjFrmErrorMessage.Width - 20 - mobjFrmErrorMessage.lblOKCancelWithComboBoxPrompt.Left, Integer.MaxValue)
                mobjFrmErrorMessage.lblOKCancelWithComboBoxPrompt.AutoSize = True
                mobjFrmErrorMessage.cmbOKCancelWithComboBoxChoice.Top = mobjFrmErrorMessage.lblOKCancelWithComboBoxPrompt.Bottom + 8
                mobjFrmErrorMessage.btnOKCancelWithComboOK.Top = mobjFrmErrorMessage.cmbOKCancelWithComboBoxChoice.Bottom + 8
                mobjFrmErrorMessage.btnOKCancelWithComboCancel.Top = mobjFrmErrorMessage.btnOKCancelWithComboOK.Top
                mobjFrmErrorMessage.Height -= mobjFrmErrorMessage.ClientSize.Height - (mobjFrmErrorMessage.btnOKCancelWithComboOK.Bottom + 4)
                If ComboBoxItems IsNot Nothing Then
                    For Each s As String In ComboBoxItems
                        mobjFrmErrorMessage.cmbOKCancelWithComboBoxChoice.Items.Add(s)
                    Next
                    If ComboBoxText IsNot Nothing Then
                        mobjFrmErrorMessage.cmbOKCancelWithComboBoxChoice.Text = ComboBoxText
                    End If
                    mobjFrmErrorMessage.cmbOKCancelWithComboBoxChoice.TabIndex = 0
                End If
                If Not String.IsNullOrEmpty(OKButtonText) Then mobjFrmErrorMessage.btnOKCancelWithComboOK.Text = OKButtonText
                If Not String.IsNullOrEmpty(CancelButtonText) Then mobjFrmErrorMessage.btnOKCancelWithComboCancel.Text = CancelButtonText

            Case Else
                mobjFrmErrorMessage = New frmErrorMessage(sPrompt, type)


        End Select

        mResult = MsgBoxResult.No

        mobjFrmErrorMessage.BringToFront()
        mobjFrmErrorMessage.ShowInTaskbar = False
        mobjFrmErrorMessage.ShowDialog()

        If type = frmErrorMessage.MessageType.OkCancelWithComboBoxMessage Then
            ComboBoxText = mobjFrmErrorMessage.cmbOKCancelWithComboBoxChoice.Text
        End If

        mobjFrmErrorMessage.Dispose()
        Return mResult
    End Function

    ''' <summary>
    ''' Shows a Yes/No dialog box and returns whether the user clicked on 'Yes' or
    ''' not.
    ''' </summary>
    ''' <param name="prompt">The message to display to the user</param>
    ''' <returns>True if the user clicked on the 'Yes' button; False otherwise.
    ''' </returns>
    Public Shared Function Yes(ByVal prompt As String) As Boolean
        Return (YesNo(prompt) = MsgBoxResult.Yes)
    End Function

    ''' <summary>
    ''' Shows a Yes/No dialog box and returns whether the user clicked on 'Yes' or
    ''' not.
    ''' </summary>
    ''' <param name="formattedPrompt">The message to display to the user, with
    ''' placeholder strings for the arguments.</param>
    ''' <param name="args">The arguments for the formatted message.</param>
    ''' <returns>True if the user clicked on the 'Yes' button; False otherwise.
    ''' </returns>
    Public Shared Function Yes(
     ByVal formattedPrompt As String, ByVal ParamArray args() As Object) As Boolean
        Return Yes(String.Format(formattedPrompt, args))
    End Function

    ''' <summary>
    ''' Shows a Yes/No dialogue message.
    ''' </summary>
    ''' <param name="sPrompt">The message</param>
    ''' <returns>The dialogue result</returns>
    Public Shared Function YesNo(ByVal sPrompt As String) As MsgBoxResult
        Return ShowDialog(sPrompt, frmErrorMessage.MessageType.YesNoMessage)
    End Function

    ''' <summary>
    ''' Shows a yes/no dialog box.
    ''' </summary>
    ''' <param name="formattedPrompt">The prompt to display in the message, with
    ''' optional formatting markers.</param>
    ''' <param name="args">The arguments to use in the formatting of the message.
    ''' </param>
    ''' <returns>The result indicating the user's response to the dialog.</returns>
    Public Shared Function YesNo(ByVal formattedPrompt As String, ByVal ParamArray args() As Object) As MsgBoxResult
        Return YesNo(String.Format(formattedPrompt, args))
    End Function

    ''' <summary>
    ''' Shows a dialog box with two buttons displaying the specified text.
    ''' </summary>
    ''' <param name="sPrompt">The prompt for the user</param>
    ''' <param name="YesButtonText">The text to use for the 'Yes' button in the
    ''' dialog box.</param>
    ''' <param name="NoButtonText">The text to use for the 'No' button in the
    ''' dialog box.</param>
    ''' <returns>The result from the dialog box.</returns>
    Public Shared Function TwoButtonsWithCustomText(ByVal sPrompt As String, ByVal YesButtonText As String, ByVal NoButtonText As String) As MsgBoxResult
        Return ShowDialog(sPrompt, frmErrorMessage.MessageType.YesNoMessage, YesButtonText:=YesButtonText, NoButtonText:=NoButtonText)
    End Function

    ''' <summary>
    ''' Shows a dialog box with two buttons displaying the specified text.
    ''' </summary>
    ''' <param name="sFormattedPrompt">The prompt for the user, with optional
    ''' formatting markers.</param>
    ''' <param name="YesButtonText">The text to use for the 'Yes' button in the
    ''' dialog box.</param>
    ''' <param name="NoButtonText">The text to use for the 'No' button in the
    ''' dialog box.</param>
    ''' <param name="args">The arguments to use to format the prompt.</param>
    ''' <returns>The result from the dialog box.</returns>
    Public Shared Function TwoButtonsWithCustomText(
     ByVal sFormattedPrompt As String, ByVal YesButtonText As String, ByVal NoButtonText As String, ByVal ParamArray args() As Object) As MsgBoxResult
        Return TwoButtonsWithCustomText(String.Format(sFormattedPrompt, args), YesButtonText, NoButtonText)
    End Function

    ''' <summary>
    ''' Shows a Yes/No dialogue message.
    ''' </summary>
    ''' <param name="sPrompt">The message</param>
    ''' <param name="bAutoResize">A flag to allow form resizing</param>
    ''' <returns>The dialogue result</returns>
    Public Shared Function YesNoCancel(ByVal sPrompt As String, Optional ByVal bAutoResize As Boolean = False) As MsgBoxResult
        Return ShowDialog(sPrompt, frmErrorMessage.MessageType.YesNoCancelMessage, bAutoResize)
    End Function

    ''' <summary>
    ''' Shows a OK dialogue message.
    ''' </summary>
    ''' <param name="sPrompt">The message</param>
    ''' <returns>The dialogue result</returns>
    Public Shared Function OK(ByVal sPrompt As String) As MsgBoxResult
        Return ShowDialog(sPrompt, frmErrorMessage.MessageType.OkMessage)
    End Function

    ''' <summary>
    ''' Shows a dialog box with the given formatted prompt and an OK button.
    ''' </summary>
    ''' <param name="formattedPrompt">The prompt to display, using the format
    ''' specifiers as defined in the <see cref="String.Format"/> method.</param>
    ''' <param name="args">The arguments to use in the given format string.</param>
    ''' <returns>The result of the dialog</returns>
    Public Shared Function OK(ByVal formattedPrompt As String, ByVal ParamArray args() As Object) As MsgBoxResult
        Return OK(String.Format(formattedPrompt, args))
    End Function


    ''' <summary>
    ''' Shows a OK/Cancel dialogue message.
    ''' </summary>
    ''' <param name="sPrompt">The message</param>
    ''' <returns>The dialogue result</returns>
    Public Shared Function OkCancel(ByVal sPrompt As String) As MsgBoxResult
        Return ShowDialog(sPrompt, frmErrorMessage.MessageType.OkCancelMessage)
    End Function

    ''' <summary>
    ''' Shows a dialog box with the given formatted prompt and OK and Cancel buttons.
    ''' </summary>
    ''' <param name="formatPrompt">The prompt to display, using the format
    ''' specifiers as defined in the <see cref="String.Format"/> method.</param>
    ''' <param name="args">The arguments to use in the given format string.</param>
    ''' <returns>The result of the dialog</returns>
    Public Shared Function OkCancel(
     ByVal formatPrompt As String, ByVal ParamArray args() As Object) As MsgBoxResult
        Return OkCancel(String.Format(formatPrompt, args))
    End Function

    ''' <summary>
    ''' Shows a OK/Cancel dialogue message with a combo box.
    ''' </summary>
    ''' <param name="sPrompt">The message</param>
    ''' <param name="ComboBoxItems">The items to be populated into the combo box</param>
    ''' <param name="ComboBoxText">In: the text to be populated into the combobox.
    ''' Out: carries back the text selected by the user in the combobox.</param>
    ''' <returns>The dialogue result</returns>
    Public Shared Function OkCancelWithComboBox(ByVal sPrompt As String, ByVal ComboBoxItems As ICollection(Of String), ByRef ComboBoxText As String, Optional ByVal OKButtonText As String = "Ok", Optional ByVal CancelButtonText As String = "Cancel") As MsgBoxResult
        Dim OKButtonTextL10n = My.Resources.btnOk
        If (OKButtonText.Equals("Ok")) Then
            OKButtonTextL10n = "Ok"
        End If
        Dim CancelButtonTextL10n = My.Resources.btnCancel
        If (CancelButtonText.Equals("Cancel")) Then
            CancelButtonTextL10n = "Cancel"
        End If
        Return ShowDialog(sPrompt, frmErrorMessage.MessageType.OkCancelWithComboBoxMessage, ComboBoxItems:=ComboBoxItems, ComboBoxText:=ComboBoxText, OKButtonText:=OKButtonTextL10n, CancelButtonText:=CancelButtonTextL10n)
    End Function

    ''' <summary>
    ''' Shows a Yes/No/Cancel dialogue with an additional hyperlink.
    ''' </summary>
    ''' <param name="sPrompt">The message</param>
    ''' <param name="LinkLabelText">The hyperlink text</param>
    ''' <param name="LinkLabelHandler">The hyperlink click event handler</param>
    ''' <param name="YesButtonText">Yes button text</param>
    ''' <param name="NoButtonText">No Button Text</param>
    ''' <param name="CancelButtonText">Cancel Button Text</param>
    ''' <returns>The dialogue result</returns>
    Public Shared Function YesNoCancelWithLinkLabel(ByVal sPrompt As String, ByVal LinkLabelText As String, ByVal LinkLabelHandler As EventHandler, Optional ByVal YesButtonText As String = "", Optional ByVal NoButtonText As String = "", Optional ByVal CancelButtonText As String = "") As MsgBoxResult
        Return ShowDialog(sPrompt, frmErrorMessage.MessageType.YesNoCancelLinkLabelMessage, Handler:=LinkLabelHandler, LinkLabelText:=LinkLabelText, YesButtonText:=YesButtonText, NoButtonText:=NoButtonText, CancelButtonText:=CancelButtonText)
    End Function

    Private Shared Sub Button_Click(ByVal result As MsgBoxResult) Handles mobjFrmErrorMessage.ClickButton
        mResult = result
    End Sub

    Private Shared Function FormatFixedWidth(ByRef sPrompt As String) As Integer

        Dim iPromptLength As Integer = Len(sPrompt)
        Dim sStringToReturn As String = ""
        Dim iTemp As Integer = 0
        While iTemp < iPromptLength
            Dim itemp2 As Integer = MaxLineLength            'the amount we are about to chop off sPrompt for a new line
            If Len(sPrompt) <= MaxLineLength Then
                itemp2 = Len(sPrompt)
            Else
                itemp2 = MaxLineLength
                While Not sPrompt.Mid(itemp2, 1) = " "
                    itemp2 -= 1
                    If itemp2 < 1 Then
                        itemp2 = MaxLineLength
                        Exit While
                    End If
                End While
            End If
            sStringToReturn &= sPrompt.Mid(1, itemp2) & vbCrLf
            sPrompt = sPrompt.Mid(itemp2 + 1, iPromptLength)
            iTemp += itemp2
        End While

        If sStringToReturn.Mid(Len(sStringToReturn) - 1, 2) = vbCrLf Then
            'strip trailing carriage return
            sPrompt = sStringToReturn.Mid(1, Len(sStringToReturn) - 2)
        Else
            sPrompt = sStringToReturn
        End If

        'count the number of lines in Prompt:
        Dim regex As New System.Text.RegularExpressions.Regex("\r\n", System.Text.RegularExpressions.RegexOptions.Multiline)
        Return regex.Matches(sPrompt).Count + 1

    End Function

End Class
