
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.BPCoreLib

''' Project  : Automate
''' Class    : frmCalculationZoom
''' 
''' <summary>
''' A form to automatically display calculation details when debugging a process.
''' </summary>
Friend Class frmCalculationZoom
    Inherits frmForm

#Region " Windows Form Designer generated code "

    'Form overrides dispose to clean up the component list.
    '<summary>
    'Summary of Dispose.
    '</summary>
    '<param name=disposing></param>

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
    Friend WithEvents rtbExpression As ctlExpressionRichTextBox
    Friend WithEvents llCalculationPage As System.Windows.Forms.LinkLabel
    Friend WithEvents llDataItemPage As System.Windows.Forms.LinkLabel
    Friend WithEvents FadeTimer As System.Windows.Forms.Timer
    '<summary>
    'Summary of InitializeComponent.
    '</summary>
    Friend WithEvents pbCalcIllustration As System.Windows.Forms.PictureBox

    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmCalculationZoom))
        Me.rtbExpression = New AutomateUI.ctlExpressionRichTextBox()
        Me.llCalculationPage = New System.Windows.Forms.LinkLabel()
        Me.llDataItemPage = New System.Windows.Forms.LinkLabel()
        Me.pbCalcIllustration = New System.Windows.Forms.PictureBox()
        Me.FadeTimer = New System.Windows.Forms.Timer(Me.components)
        CType(Me.pbCalcIllustration, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'rtbExpression
        '
        Me.rtbExpression.AllowDrop = True
        resources.ApplyResources(Me.rtbExpression, "rtbExpression")
        Me.rtbExpression.BackColor = System.Drawing.Color.White
        Me.rtbExpression.Cursor = System.Windows.Forms.Cursors.Default
        Me.rtbExpression.DetectUrls = False
        Me.rtbExpression.HideSelection = False
        Me.rtbExpression.HighlightingEnabled = True
        Me.rtbExpression.Name = "rtbExpression"
        Me.rtbExpression.PasswordChar = ChrW(0)
        Me.rtbExpression.ReadOnly = True
        '
        'llCalculationPage
        '
        resources.ApplyResources(Me.llCalculationPage, "llCalculationPage")
        Me.llCalculationPage.Name = "llCalculationPage"
        Me.llCalculationPage.TabStop = True
        '
        'llDataItemPage
        '
        resources.ApplyResources(Me.llDataItemPage, "llDataItemPage")
        Me.llDataItemPage.Name = "llDataItemPage"
        Me.llDataItemPage.TabStop = True
        '
        'pbCalcIllustration
        '
        resources.ApplyResources(Me.pbCalcIllustration, "pbCalcIllustration")
        Me.pbCalcIllustration.Name = "pbCalcIllustration"
        Me.pbCalcIllustration.TabStop = False
        '
        'frmCalculationZoom
        '
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.rtbExpression)
        Me.Controls.Add(Me.pbCalcIllustration)
        Me.Controls.Add(Me.llDataItemPage)
        Me.Controls.Add(Me.llCalculationPage)
        Me.ForeColor = System.Drawing.SystemColors.ControlText
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "frmCalculationZoom"
        Me.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide
        CType(Me.pbCalcIllustration, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub

#End Region

#Region " Class-scope Declarations "

    ''' <summary>
    ''' The page hyperlink text.
    ''' </summary>
    Private PageLinkTip As String = My.Resources.frmCalculationZoom_ClickToGoToPage

    ''' <summary>
    ''' The time the form is displayed before fading out.
    ''' </summary>
    Private Const PopUpTime As Integer = 500

#End Region

#Region "Members"

    ' The data item stage to be assigned the result of the calculation.
    Private mResultStage As clsDataStage

    ' The process that the calc stage is on
    Private mProcess As clsProcess

    ' The area of the calculation icon
    Private mCalcIconRect As RectangleF

    ' The area of the result stage icon
    Private mResultIconRect As RectangleF

    ' Stage icon tooltip
    Private mStageTooltip As ToolTip

    ' Page hyperlink tooltip
    Private mSubsheetTooltip As ToolTip

    ' Flag indicating if the form is closing
    Private mbIsClosing As Boolean

    ' The calculation stage.
    Private mStage As clsCalculationStage

    ' The parent ctlProcessViewer
    Private mViewer As ctlProcessViewer

    ' The step taken when increasing and decreasing the form's opacity.
    Private mOpacityStep As Double

    ' The maximum opacity applied to the form.
    Private mMaxOpacity As Double

    ' A list of parent ctlProcessViewer controls that have disabled calculation
    ' zoom. This is to cater for multiple simultaneous edit sessions.
    Private Shared mDisabledControls As New List(Of ctlProcessViewer)

    ' Flag indicating that the debug step speed is too fast to fade this form in/out
    Private mDebugTooFastForFading As Boolean

#End Region

#Region "Properties"

    ''' <summary>
    ''' Indicates that calculation zoom has been disabled for the current process.
    ''' </summary>
    ''' <param name="parent">The parent control</param>
    ''' <value>True if disabled</value>
    Public Shared Property Disabled(ByVal parent As ctlProcessViewer) As Boolean
        Get
            Return mDisabledControls.Contains(parent)
        End Get
        Set(ByVal value As Boolean)
            If value And Not mDisabledControls.Contains(parent) Then
                mDisabledControls.Add(parent)
            End If
            If Not value And mDisabledControls.Contains(parent) Then
                mDisabledControls.Remove(parent)
            End If
        End Set
    End Property

    ''' <summary>
    ''' Indicates that the form is in the process of closing down.
    ''' </summary>
    ''' <value>True if closing</value>
    Public ReadOnly Property IsClosing() As Boolean
        Get
            Return mbIsClosing
        End Get
    End Property

    ''' <summary>
    ''' The calculation stage to display.
    ''' </summary>
    ''' <value>The stage object</value>
    Public ReadOnly Property ProcessStage() As clsProcessStage
        Get
            Return mStage
        End Get
    End Property

#End Region

#Region "Constructor"

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <param name="stg">The calculation stage object</param>
    ''' <param name="interval">The process studio debug interval</param>
    Public Sub New(ByVal stg As clsProcessStage, Optional ByVal interval As Integer = 1000)
        MyBase.New()
        InitializeComponent()

        'make a reference to the calculation stage
        mStage = CType(stg, Stages.clsCalculationStage)
        mProcess = mStage.Process

        ShowInTaskbar = False
        StartPosition = FormStartPosition.Manual

        If interval < 150 Then
            'The debug interval is too fast for fading in and out.
            mMaxOpacity = 1
            Me.Opacity = mMaxOpacity
            mDebugTooFastForFading = True
        Else
            'Start with the form transparent and set values
            'to control how the fade will work.
            Me.Opacity = 0
            mMaxOpacity = 0.8
            mOpacityStep = 0.1
            FadeTimer.Interval = CInt(Math.Ceiling(interval / 30))
            AddHandler FadeTimer.Tick, AddressOf FadeIn
        End If

        'a tooltip for the data item graphics and expression
        mStageTooltip = New ToolTip
        mStageTooltip.AutoPopDelay = 3000
        mStageTooltip.InitialDelay = 500
        mStageTooltip.ReshowDelay = 500
        mStageTooltip.ShowAlways = True

        'a tooltip for the page hyperlinks
        mSubsheetTooltip = New ToolTip
        mSubsheetTooltip.AutoPopDelay = mStageTooltip.AutoPopDelay
        mSubsheetTooltip.InitialDelay = mStageTooltip.InitialDelay
        mSubsheetTooltip.ReshowDelay = mStageTooltip.ReshowDelay
        mSubsheetTooltip.ShowAlways = mStageTooltip.ShowAlways

    End Sub

#End Region

    ''' <summary>
    ''' Closes the form with or without a fade out.
    ''' </summary>
    ''' <param name="bFadeOut">True to fade the form out (unless the debug step
    ''' interval is too fast to fade it out); False to just close it</param>
    Public Overloads Sub Close(ByVal bFadeOut As Boolean)

        If mDebugTooFastForFading OrElse Not bFadeOut Then
            'Close without fading.
            RemoveHandler Me.Closing, AddressOf frmCalculationZoom_Closing
            Me.Close()
        Else
            'Fade then close.
            RemoveHandler Me.Closing, AddressOf frmCalculationZoom_Closing
            RemoveHandler FadeTimer.Tick, AddressOf FadeIn
            AddHandler FadeTimer.Tick, AddressOf FadeOutAndClose
            Me.FadeTimer.Start()
        End If

    End Sub



#Region "SetParent"

    ''' <summary>
    ''' Sets the parent control.
    ''' </summary>
    ''' <param name="f">The parent control</param>
    Public Sub SetParentProcessViewer(ByRef f As ctlProcessViewer)
        mViewer = f
        'mobjBusinessObjects = f.GetParent.GetObjRefs
    End Sub

#End Region

#Region "Show"

    ''' <summary>
    ''' Displays the form and in 'Running' mode closes it after a short wait.
    ''' </summary>
    Public Shadows Sub Show()
        Try
            Populate()
            DrawGraphics()
            Dim mAdjPoint As System.Drawing.Point = mViewer.PointToClient(New Point(mViewer.Location.X, mViewer.Location.Y))
            Me.Location = New Point((-1 * mAdjPoint.X) + mViewer.Location.X + mViewer.Width - Me.Width - 60, (-1 * mAdjPoint.Y) + mViewer.Location.Y + mViewer.Height - Me.Height - 30)
            MyBase.Show()
            If Me.Opacity = 0 Then
                FadeTimer.Start()
            End If
        Catch ex As Exception
            Me.mViewer.ParentForm.Activate()
            UserMessage.ShowExceptionMessage(ex)
        End Try
    End Sub

#End Region



#Region "Populate"

    ''' <summary>
    ''' Sets up the two page hyperlinks and populates the expression text box. 
    ''' </summary>
    Private Sub Populate()
        Const MaxLen As Integer = 20

        'populate the expression
        rtbExpression.Text = mStage.Expression.LocalForm
        rtbExpression.ColourText()

        'put the calculation page id in the link's tag and add the click event handler
        Dim sheetId As Guid = mStage.GetSubSheetID()
        llCalculationPage.Tag = sheetId
        AddHandler llCalculationPage.Click, AddressOf SelectTabPage

        'define the calculation page link text and tool tip
        Dim sheetName As String
        If sheetId.Equals(Guid.Empty) Then
            sheetName = My.Resources.frmCalculationZoom_MainPage
        Else
            sheetName = mStage.Process.GetSubSheetName(sheetId)
        End If

        If sheetName.Length > MaxLen Then
            llCalculationPage.Text = sheetName.Mid(1, MaxLen) & My.Resources.frmCalculationZoom_Ellipsis
        Else
            llCalculationPage.Text = sheetName
        End If
        llCalculationPage.LinkArea = New LinkArea(0, llCalculationPage.Text.Length)
        mSubsheetTooltip.SetToolTip(llCalculationPage, PageLinkTip)

        'get the result data item
        Dim resultStgName As String = mStage.StoreIn
        mResultStage = mStage.StoreInStage
        If mResultStage Is Nothing Then
            'hide the link if the result data item is undefined
            mResultStage = Nothing
            llDataItemPage.Visible = False
        Else
            'put the result page id in the link's tag and add the click event handler
            llDataItemPage.Visible = True
            Dim resultSheetId As Guid = mResultStage.GetSubSheetID()
            llDataItemPage.Tag = resultSheetId
            AddHandler llDataItemPage.Click, AddressOf SelectTabPage

            'define the result page link text and tool tip
            Dim resultSheetName As String
            If resultSheetId.Equals(Guid.Empty) Then
                resultSheetName = My.Resources.frmCalculationZoom_MainPage
            Else
                resultSheetName = mResultStage.Process.GetSubSheetName(resultSheetId)
            End If

            If resultSheetName.Length > MaxLen Then
                llDataItemPage.Text = resultSheetName.Mid(1, MaxLen) & My.Resources.frmCalculationZoom_Ellipsis
            Else
                llDataItemPage.Text = resultSheetName
            End If
            llDataItemPage.LinkArea = New LinkArea(0, llDataItemPage.Text.Length)
            mSubsheetTooltip.SetToolTip(llDataItemPage, PageLinkTip)

            Me.UpdateLinkLabels()
        End If

    End Sub

    ''' <summary>
    ''' Updates the link labels, according to which page is currently
    ''' visible
    ''' </summary>
    Private Sub UpdateLinkLabels()
        Dim CalcPageID As Guid = CType(llCalculationPage.Tag, Guid)
        llCalculationPage.Enabled = Not Me.mProcess.GetActiveSubSheet.Equals(CalcPageID)

        Dim DataPageID As Guid = CType(llDataItemPage.Tag, Guid)
        Me.llDataItemPage.Enabled = Not Me.mProcess.GetActiveSubSheet.Equals(DataPageID)
    End Sub
#End Region

#Region "SelectTabPage"

    ''' <summary>
    ''' An event handler for the page hyperlink click events. 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub SelectTabPage(ByVal sender As Object, ByVal e As EventArgs)
        Dim objTabPage As TabPage
        Dim gSheetId As Guid
        Dim objConrol As Control

        objConrol = CType(sender, Control)
        If Not objConrol.Tag Is Nothing Then
            'get the page guid from the link's tag and look through the tab pages in frmProcess
            gSheetId = CType(objConrol.Tag, Guid)
            For Each objTabPage In mViewer.tcPages.TabPages
                'it seems the name property is set to Guid.ToString or 'Main Page'
                If objTabPage.Name = gSheetId.ToString Or (objTabPage.Name = My.Resources.frmCalculationZoom_MainPage And gSheetId.Equals(Guid.Empty)) Then
                    mViewer.tcPages.SelectedTab = objTabPage
                    Exit For
                End If
            Next
        End If

        Me.UpdateLinkLabels()
    End Sub

#End Region

#Region "DrawGraphics"

    ''' <summary>
    ''' Draws the lozenge, arrow and parallelogram icons on to the form. 
    ''' </summary>
    Private Sub DrawGraphics()

        Dim objGraphics As Graphics
        Dim objPen As Pen
        Dim objFormat As New StringFormat
        Dim objBrush As Drawing2D.LinearGradientBrush
        Dim objCalculationColor, objResultColor As Color
        Dim sName As String

        Dim aParallelogramPoints, aLozengePoints, iArrowPoints As Point()
        Dim iX, iY As Integer
        Dim iLozengeWidth As Integer = 100
        Dim iParallelogramWidth As Integer = 100
        Dim iHeight As Integer = 50
        Dim iArrowWidth As Integer = 50
        Dim iArrowHeight As Integer = 25
        Dim iTopMargin As Integer = 4
        Dim iSideMargin As Integer = 8
        Dim iMaxNameLength As Integer = 15

        Dim b As New Bitmap(Me.pbCalcIllustration.Width, Me.pbCalcIllustration.Height)
        objGraphics = Graphics.FromImage(b)
        objGraphics.IntersectClip(Me.pbCalcIllustration.ClientRectangle)

        'start with x and y as the top right point just left of the arrow area
        iX = CInt((Me.pbCalcIllustration.Width - iArrowWidth) * 0.5 - iSideMargin)
        iY = iTopMargin
        'save the calculation area as a rectangle
        mCalcIconRect = New RectangleF(iX - iLozengeWidth, iY, iLozengeWidth, iHeight)

        'the lozenge is the quasi-hexagonal calculation stage shape
        aLozengePoints = New Point() {
        New Point(iX, CInt(iY + iHeight * 0.5)),
        New Point(CInt(iX - iLozengeWidth * 0.125), iY + iHeight),
        New Point(CInt(iX - iLozengeWidth * 0.875), iY + iHeight),
        New Point(CInt(iX - iLozengeWidth), CInt(iY + iHeight * 0.5)),
        New Point(CInt(iX - iLozengeWidth * 0.875), iY),
        New Point(CInt(iX - iLozengeWidth * 0.125), iY)}

        'move x to the right of the arrow
        iX = CInt((Me.pbCalcIllustration.ClientSize.Width + iArrowWidth) * 0.5 + iSideMargin)
        aParallelogramPoints = New Point() {
        New Point(CInt(iX + iParallelogramWidth * 0.25), iY),
        New Point(iX + iParallelogramWidth, iY),
        New Point(CInt(iX + iParallelogramWidth * 0.75), iY + iHeight),
        New Point(iX, iY + iHeight)}
        'save the result area as a rectangle
        mResultIconRect = New RectangleF(iX, iY, iParallelogramWidth, iHeight)

        'move x to the centre and y to the top point of the arrow
        iX = CInt(Me.pbCalcIllustration.ClientSize.Width * 0.5)
        iY = CInt(iY + (iHeight - iArrowHeight) / 2)
        iArrowPoints = New Point() {
        New Point(iX, iY),
        New Point(CInt(iX + iArrowWidth * 0.5), CInt(iY + iArrowHeight * 0.5)),
        New Point(iX, iY + iArrowHeight),
        New Point(iX, CInt(iY + iArrowHeight * 0.66)),
        New Point(CInt(iX - iArrowWidth * 0.5), CInt(iY + iArrowHeight * 0.66)),
        New Point(CInt(iX - iArrowWidth * 0.5), CInt(iY + iArrowHeight * 0.33)),
        New Point(iX, CInt(iY + iArrowHeight * 0.33))}

        objGraphics.Clear(Me.BackColor)

        objCalculationColor = Color.Orange
        objResultColor = Color.White

        'draw the calculation lozenge shape
        objPen = New Pen(Color.Black, -1)
        objGraphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        objGraphics.FillPolygon(New SolidBrush(objCalculationColor), aLozengePoints)
        objGraphics.DrawPolygon(objPen, aLozengePoints)

        'draw the arrow using a gradient fill
        objBrush = New Drawing2D.LinearGradientBrush(
        New PointF(CSng((Me.pbCalcIllustration.ClientSize.Width - iArrowWidth) * 0.5), 0),
        New PointF(CSng((Me.pbCalcIllustration.ClientSize.Width + iArrowWidth) * 0.5), 0),
        objCalculationColor,
        objResultColor)
        objGraphics.FillPolygon(objBrush, iArrowPoints)
        objGraphics.DrawPolygon(objPen, iArrowPoints)

        'draw the result parallelogram
        objGraphics.FillPolygon(New SolidBrush(objResultColor), aParallelogramPoints)
        objGraphics.DrawPolygon(objPen, aParallelogramPoints)

        sName = mStage.GetName
        If sName.Length > iMaxNameLength Then
            sName = sName.Mid(1, iMaxNameLength) & My.Resources.frmCalculationZoom_Ellipsis
        End If
        objFormat.Alignment = StringAlignment.Center
        objFormat.LineAlignment = StringAlignment.Center
        objGraphics.DrawString(sName, llCalculationPage.Font, Brushes.Black, mCalcIconRect, objFormat)

        If mResultStage Is Nothing Then
            sName = My.Resources.frmCalculationZoom_Undefined
        Else
            sName = mResultStage.GetName
            If sName.Length > iMaxNameLength Then
                sName = sName.Mid(1, iMaxNameLength) & My.Resources.frmCalculationZoom_Ellipsis
            End If
        End If
        objGraphics.DrawString(sName, llCalculationPage.Font, Brushes.Black, mResultIconRect, objFormat)

        'discard the result area rectangle if the result data item is undefined
        If mResultStage Is Nothing Then
            mResultIconRect = RectangleF.Empty
        End If

        Me.pbCalcIllustration.Image = b
    End Sub

#End Region

#Region "DoRefresh"

    ''' <summary>
    ''' Refreshes the form and re-validates the expression if necessary. 
    ''' </summary>
    Private Sub DoRefresh()

        mStage = CType(mProcess.GetStage(mStage.GetStageID), clsCalculationStage)
        mViewer.BringToFront()
        Populate()
        DrawGraphics()

        Try
            'Validate the expression...
            Dim sErr As String = Nothing
            Dim res As clsProcessValue = Nothing
            If Not clsExpression.EvaluateExpression(
             mStage.Expression, res, mStage, True, Nothing, sErr) Then _
             Throw New InvalidExpressionException(sErr)

            'Check the result can be stored...
            If mResultStage IsNot Nothing AndAlso
             Not mResultStage.CanSetValue(res) Then _
             Throw New InvalidExpressionException(My.Resources.frmCalculationZoom_TheCalculationResultIsTheWrongDataType)

        Catch ex As Exception
            UserMessage.Show(My.Resources.frmCalculationZoom_YourExpressionDoesNotAppearToBeValid & ex.Message)
        End Try

    End Sub

#End Region

#Region "GetExpressionDataItemNameFromCharIndex"

    ''' <summary>
    ''' Extracts any data item name found at the given character position. 
    ''' </summary>
    ''' <param name="i">The character index</param>
    ''' <returns>The data item name found or an empty string</returns>
    Private Function GetExpressionDataItemNameFromCharIndex(ByVal i As Integer) As String
        Dim sRight As String = rtbExpression.Text.Mid(i + 1)
        Dim sLeft As String = rtbExpression.Text.Mid(1, i)
        Dim iOpenRight, iCloseRight, iOpenLeft, iCloseLeft As Integer
        Dim sName As String = ""

        iOpenRight = sRight.IndexOf("[")
        iCloseRight = sRight.IndexOf("]")
        If iCloseRight > -1 And (iOpenRight > iCloseRight Or iOpenRight = -1) Then
            'a closing bracket exists to right of the click position before any occurance of an open bracket
            iOpenLeft = sLeft.LastIndexOf("[")
            iCloseLeft = sLeft.LastIndexOf("]")
            If iOpenLeft > -1 And iOpenLeft > iCloseLeft Then
                'an opening bracket exists to the left of the click position after any occurance of a closing bracket
                sName = sLeft.Mid(iOpenLeft + 2) & sRight.Mid(1, iCloseRight)
            End If
        End If
        Return sName
    End Function

#End Region



#Region "rtbExpression_MouseLeave"

    ''' <summary>
    ''' Changes the cursor back.
    ''' </summary>
    ''' <param name="sender">The event source</param>
    ''' <param name="e">The event</param>
    Private Sub rtbExpression_MouseLeave(ByVal sender As System.Object, ByVal e As EventArgs) Handles rtbExpression.MouseLeave
        Cursor = Cursors.Default
    End Sub

#End Region

#Region "rtbExpression_MouseMove"

    ''' <summary>
    ''' Displays a tooltip if the mouse is over a data item in the expression.
    ''' </summary>
    ''' <param name="sender">The event source</param>
    ''' <param name="e">The event</param>
    Private Sub rtbExpression_MouseMove(ByVal sender As System.Object, ByVal e As MouseEventArgs) Handles rtbExpression.MouseMove
        Dim strMousePoint, strCharacterPoint As Point
        Dim iIndex As Integer
        Dim iTolerance As Integer = 10
        Dim objValue As clsProcessValue
        Dim sName, sDescription, sTip, sSheetName, sField As String
        Dim objDataItem As clsDataStage
        Dim gSheetId As Guid

        'get the mouse point and the actual location of the character nearest the mouse
        strMousePoint = New Point(e.X, e.Y)
        iIndex = rtbExpression.GetCharIndexFromPosition(strMousePoint)
        strCharacterPoint = rtbExpression.GetPositionFromCharIndex(iIndex)

        'check that the mouse is tolerably close to the character
        If strMousePoint.X <= strCharacterPoint.X + iTolerance And strMousePoint.X >= strCharacterPoint.X - iTolerance _
         And strMousePoint.Y <= strCharacterPoint.Y + iTolerance And strMousePoint.Y >= strCharacterPoint.Y - iTolerance Then

            'look for a data item name
            sName = GetExpressionDataItemNameFromCharIndex(iIndex)

            If sName = "" Then
                Cursor = Cursors.Default
            Else
                Cursor = Cursors.Hand

                'get the data item object and make a tool tip from it

                If sName.IndexOf(".") = -1 Then
                    objDataItem = CType(mStage.Process.GetStage(sName), clsDataStage)
                Else
                    objDataItem = CType(mStage.Process.GetStage(sName.Mid(1, sName.IndexOf("."))), clsDataStage)
                End If
                If objDataItem Is Nothing Then Exit Sub

                gSheetId = objDataItem.GetSubSheetID()
                If gSheetId.Equals(Guid.Empty) Then
                    sSheetName = My.Resources.frmCalculationZoom_MainPage
                Else
                    sSheetName = objDataItem.Process.GetSubSheetName(gSheetId)
                End If

                sTip = objDataItem.GetName
                sDescription = objDataItem.GetNarrative
                If sDescription <> "" Then
                    sTip &= vbCrLf & sDescription
                End If
                sTip &= vbCrLf & My.Resources.frmCalculationZoom_Page & sSheetName

                If sName.IndexOf(".") = -1 Then
                    'Data item
                    objValue = objDataItem.GetValue
                    sTip &= vbCrLf & My.Resources.frmCalculationZoom_DataType & clsProcessDataTypes.GetFriendlyName(objValue.DataType)
                    sTip &= vbCrLf & My.Resources.frmCalculationZoom_Value & objValue.FormattedValue
                Else
                    'Collection field
                    sField = sName.Mid(sName.IndexOf(".") + 2)
                    sTip &= vbCrLf & My.Resources.frmCalculationZoom_FieldName & sField


                End If
                mStageTooltip.SetToolTip(rtbExpression, sTip)
            End If
        Else
            'the mouse is too far away from the character, so remove any earlier tool tips
            mStageTooltip.RemoveAll()
            Cursor = Cursors.Default
        End If
    End Sub

#End Region

#Region "rtbExpression_MouseDown"

    ''' <summary>
    ''' Opens a property wizard if the user clicks on a data item name. 
    ''' </summary>
    ''' <param name="sender">The event source</param>
    ''' <param name="e">The event</param>
    Private Sub rtbExpression_MouseDown(ByVal sender As System.Object, ByVal e As MouseEventArgs) Handles rtbExpression.MouseDown

        If e.Clicks = 1 And e.Button = System.Windows.Forms.MouseButtons.Left Then
            Dim iIndex As Integer
            Dim sName As String
            Dim objDataItem As clsProcessStage

            'get any data item name that the user may have clicked on
            iIndex = rtbExpression.GetCharIndexFromPosition(New Point(e.X, e.Y))
            sName = GetExpressionDataItemNameFromCharIndex(iIndex)

            If sName.IndexOf(".") = -1 Then
                objDataItem = mStage.Process.GetStage(sName)
            ElseIf sName <> "" Then
                objDataItem = mStage.Process.GetStage(sName.Mid(1, sName.IndexOf(".")))
            Else
                objDataItem = Nothing
            End If

            If Not objDataItem Is Nothing Then
                mViewer.ParentForm.Activate()
                Me.mViewer.LaunchStageProperties(objDataItem)

                If sName <> objDataItem.GetName Then
                    mStage.Expression =
                     mStage.Expression.ReplaceDataItemName(sName, objDataItem.Name)
                End If
            End If

        End If
        rtbExpression.SelectionStart = 0
        rtbExpression.SelectionLength = 0
    End Sub

#End Region



#Region "frmCalculationZoom_MouseDown"

    ''' <summary>
    ''' Opens a property wizard if the user clicks in the calculation icon or result icon.
    ''' </summary>
    ''' <param name="sender">The event source</param>
    ''' <param name="e">The event</param>
    Private Sub PictureBox_MouseDown(ByVal sender As System.Object, ByVal e As MouseEventArgs) Handles pbCalcIllustration.MouseDown

        If e.Clicks = 1 AndAlso e.Button = System.Windows.Forms.MouseButtons.Left Then

            If Not mCalcIconRect.Equals(RectangleF.Empty) _
             AndAlso mCalcIconRect.Contains(e.X, e.Y) _
             AndAlso Not mStage Is Nothing Then

                'the user has clicked in the calculation area
                Me.mViewer.LaunchStageProperties(mStage)
                mViewer.ParentForm.Activate()
                DoRefresh()
            ElseIf Not mResultIconRect.Equals(RectangleF.Empty) _
             AndAlso mResultIconRect.Contains(e.X, e.Y) _
             AndAlso Not mResultStage Is Nothing Then

                'the user has clicked in the result area
                Me.mViewer.LaunchStageProperties(mResultStage)
                mViewer.ParentForm.Activate()
                DoRefresh()

            End If

        End If

    End Sub

#End Region

#Region "frmCalculationZoom_MouseMove"

    ''' <summary>
    ''' Displays tool tips according to where the mouse is on the form. 
    ''' </summary>
    ''' <param name="sender">The event source</param>
    ''' <param name="e">The event</param>
    Private Sub PictureBox_MouseMove(ByVal sender As System.Object, ByVal e As MouseEventArgs) Handles pbCalcIllustration.MouseMove
        If Not mCalcIconRect.Equals(RectangleF.Empty) AndAlso mCalcIconRect.Contains(e.X, e.Y) Then
            Me.Cursor = Cursors.Hand
            mStageTooltip.SetToolTip(Me, My.Resources.frmCalculationZoom_ClickToViewCalculationProperties)
        ElseIf Not mResultIconRect.Equals(RectangleF.Empty) AndAlso mResultIconRect.Contains(e.X, e.Y) Then
            Me.Cursor = Cursors.Hand
            mStageTooltip.SetToolTip(Me, My.Resources.frmCalculationZoom_ClickToViewDataItemProperties)
        Else
            mStageTooltip.RemoveAll()
            Me.Cursor = Cursors.Default
        End If
    End Sub

#End Region

#Region "frmCalculationZoom_MouseLeave"
    ''' <summary>
    ''' Changes the cursor back.
    ''' </summary>
    ''' <param name="sender">The event source</param>
    ''' <param name="e">The event</param>
    Private Sub PictureBox_MouseLeave(ByVal sender As System.Object, ByVal e As EventArgs) Handles pbCalcIllustration.MouseLeave
        Me.Cursor = Cursors.Default
    End Sub

#End Region



#Region "frmCalculationZoom_Closing"
    ''' <summary>
    ''' Closing handler for when the use has clicked the 'x' button on the 
    ''' top right of the form.
    ''' </summary>
    ''' <param name="sender">The event source</param>
    ''' <param name="e">The event</param>
    Private Sub frmCalculationZoom_Closing(ByVal sender As System.Object, ByVal e As System.ComponentModel.CancelEventArgs) Handles Me.Closing

        mbIsClosing = True
        Try
            mViewer.SetCalcZoomMenuOption(False)
            mViewer.mobjCalculationZoom = Nothing
            Disabled(mViewer) = True
        Catch ex As Exception
            Me.mViewer.ParentForm.Activate()
            UserMessage.ShowExceptionMessage(ex)
        End Try

    End Sub

#End Region

#Region "FadeIn"
    Private Sub FadeIn(ByVal sender As System.Object, ByVal e As System.EventArgs)

        If Math.Round(Me.Opacity, 1) < mMaxOpacity Then
            Me.Opacity += mOpacityStep
        Else
            FadeTimer.Stop()
        End If

    End Sub

#End Region

#Region "FadeOutAndClose"
    Private Sub FadeOutAndClose(ByVal sender As System.Object, ByVal e As System.EventArgs)

        mbIsClosing = True
        If Math.Round(Me.Opacity, 1) > 0 Then
            Me.Opacity -= mOpacityStep
        Else
            FadeTimer.Stop()
            Me.Close()
        End If

    End Sub

#End Region


End Class
