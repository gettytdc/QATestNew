Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore


' ReSharper disable once CheckNamespace
Public Class LicenseActivationHistory

    Private ReadOnly mLicenseKeyInfo As KeyInfo
    Private ReadOnly mBlueColor As Color = Color.FromArgb(&H11, &H7E, &HC2)
    Private ReadOnly mCharcoalGreyColor As Color = Color.FromArgb(&H43, &H4A, &H4F)
    Private ReadOnly mLicenseHistory As List(Of LicenseActivationEvent)
    Private ReadOnly mTextFont As Font = New Font("Segoe UI", 16.0!, FontStyle.Regular, GraphicsUnit.Pixel)

#Region "Constructor"
    Public Sub New(license As KeyInfo)
        ' This call is required by the designer.
        InitializeComponent()

        mLicenseKeyInfo = license
        mLicenseHistory = gSv.GetAuditLogDataForLicense(license.Id).ToList()
        mLicenseHistory = mLicenseHistory.OrderByDescending(Function(l) l.EventDateTime).ToList()

        SetupLicenseName()

        PopulateFlowPanel()
    End Sub
#End Region
    Private Sub PopulateFlowPanel()

        Dim pass = 0
        TableLayoutPanel1.SuspendLayout()
        TableLayoutPanel1.RowStyles.Clear()
        Dim rowStyle = New RowStyle(SizeType.Absolute)
        rowStyle.Height = 22
        TableLayoutPanel1.RowStyles.Add(rowStyle)

        For Each e In mLicenseHistory
            pass += 1

            Dim eventImage As Image
            If e.Success = True AndAlso e.EventType = LicenseEventTypes.LicenseActivated Then
                eventImage = ActivationWizardResources.Timeline_point___success_16_16
            ElseIf e.Success = False Then
                eventImage = ActivationWizardResources.Timeline_point___failed_17_17
            Else
                eventImage = ActivationWizardResources.Timeline_point_16_16
            End If

            Dim eventTypeImage As Image
            Select Case e.EventType
                Case LicenseEventTypes.LicenseActivationRequestGenerated
                    eventTypeImage = ActivationWizardResources.Globe_online_18_18
                Case Else
                    eventTypeImage = ActivationWizardResources.Globe_18_18
            End Select

            Dim eventPictureBox As New PictureBox With {
                    .Image = eventImage,
                    .Padding = Padding.Empty,
                    .Anchor = AnchorStyles.top ,
                    .SizeMode = PictureBoxSizeMode.CenterImage,
                    .Size = New Size(20, 20)
            }

            'add the lines to the flowPanel
            Dim eventDateLabel As New Label With {
                    .Font = mTextFont,
                    .Text = $"{e.EventDateTime.ToLocalTime.ToShortTimeString()} {e.EventDateTime.ToLocalTime.ToString("dddd d MMM")}",
                    .Padding = Padding.Empty,
                    .AutoSize = True
            }

            Dim eventDetailsLabel As New Label With {
                    .Font = mTextFont,
                    .Text = GetEventDetails(e.EventType),
                    .Padding = Padding.Empty,
                    .AutoSize = True
            }

            Dim eventUserLabel As New Label With {
                    .Font = mTextFont,
                    .Text = e.EventUser,
                    .Padding = Padding.Empty,
                    .AutoSize = True
                    }

            Dim eventTypePictureBox As New PictureBox With {
            .Image = eventTypeImage,
            .Padding = Padding.Empty,
            .Anchor = AnchorStyles.left,
            .SizeMode = PictureBoxSizeMode.CenterImage,
            .Size = New Size(20, 20)
            }
            
            TableLayoutPanel1.Controls.Add(eventPictureBox, 0, TableLayoutPanel1.RowCount - 1)
            TableLayoutPanel1.Controls.Add(eventDateLabel, 1, TableLayoutPanel1.RowCount - 1)
            TableLayoutPanel1.Controls.Add(eventDetailsLabel, 2, TableLayoutPanel1.RowCount - 1)
            TableLayoutPanel1.Controls.Add(eventUserLabel, 3, TableLayoutPanel1.RowCount - 1)
            TableLayoutPanel1.Controls.Add(eventTypePictureBox, 4, TableLayoutPanel1.RowCount - 1)
          
            If pass < mLicenseHistory.Count Then
                TableLayoutPanel1.RowCount += 1

                Dim connectImage As New PictureBox With {
                        .Image = ActivationWizardResources.Timeline___connector_1_18,
                        .Padding = Padding.Empty,
                        .Anchor = AnchorStyles.None,
                        .SizeMode = PictureBoxSizeMode.CenterImage,
                        .Size = New Size(20, 20)
                        }
                TableLayoutPanel1.Controls.Add(connectImage, 0, TableLayoutPanel1.RowCount - 1)

                TableLayoutPanel1.RowCount += 1
            End If
        Next
        Dim scrollWidth = SystemInformation.VerticalScrollBarWidth
        Dim horizontalScrollableSize = TableLayoutPanel1.PreferredSize.Width
        If horizontalScrollableSize < TableLayoutPanel1.Width + scrollWidth Then
            TableLayoutPanel1.Padding = New Padding(0, 0, scrollWidth, 0)
        End If

        PerformAutoScale()
        TableLayoutPanel1.ResumeLayout()
    End Sub

    Private Shared Function GetEventDetails(eventType As LicenseEventTypes) As String
        Select Case eventType
            Case LicenseEventTypes.LicenseImported
                Return My.Resources.LicenseImportedText
            Case LicenseEventTypes.LicenseActivationRequestGenerated
                Return My.Resources.LicenseActivationRequestGeneratedText
            Case LicenseEventTypes.LicenseActivated
                Return My.Resources.LicenseActivationLicesnseActivatedText
            Case Else
                Return ""
        End Select
    End Function
    Private Sub SetupLicenseName()

        'General
        LicenseNameLabel.Text = mLicenseKeyInfo.LicenseOwner
        LicenseNameLabel.ForeColor = mBlueColor
        LicenseNameLabel.Left = BorderPanel.Width \ 2 - LicenseNameLabel.Width \ 2
    End Sub

#Region "Drag And Drop Window"

    Dim mMouseDownLocation As Point

    Private Sub BorderPanel_MouseDown(sender As Object, e As MouseEventArgs) Handles BorderPanel.MouseDown
        If e.Button = MouseButtons.Left Then mMouseDownLocation = e.Location
    End Sub
    Private Sub BorderPanel_MouseMove(sender As Object, e As MouseEventArgs) Handles BorderPanel.MouseMove
        If e.Button = MouseButtons.Left Then
            Left += e.Location.X - mMouseDownLocation.X
            Top += e.Location.Y - mMouseDownLocation.Y
        End If
    End Sub

#End Region

#Region "Event Handlers"

    Sub HorizontalLinePaint(s As Object, e As PaintEventArgs) Handles BorderPanel.Paint
        Dim tRectangle As Rectangle
        Dim lineColor As Color
        tRectangle = New Rectangle(143, 258, 514, 2)
        lineColor = mCharcoalGreyColor

        Dim brush = New SolidBrush(lineColor)
        e.Graphics.FillRectangle(brush, tRectangle)
    End Sub
    Private Sub CloseButtonClicked(sender As Object, e As EventArgs) Handles PictureBox1.Click
        Close()
    End Sub

#End Region
End Class