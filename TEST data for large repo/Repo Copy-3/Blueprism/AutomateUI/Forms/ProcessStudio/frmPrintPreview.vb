Imports System.Drawing.Printing
Imports System.Drawing.Drawing2D
Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore
Imports AutomateControls
Imports AutomateUI.My.Resources
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateAppCore.Utility

Friend Class frmPrintPreview
    Inherits frmForm

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        InitializeComponent()


    End Sub

    'Form overrides dispose to clean up the component list.
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
    Friend WithEvents moPrintPreviewControl As System.Windows.Forms.PrintPreviewControl
    Friend WithEvents PageSetupDialog1 As System.Windows.Forms.PageSetupDialog
    Friend WithEvents PrintDialog1 As System.Windows.Forms.PrintDialog
    Friend WithEvents cmbZoom As System.Windows.Forms.ComboBox
    Friend WithEvents cmbPageLayout As System.Windows.Forms.ComboBox
    Friend WithEvents HiddenBluePrismLogo As System.Windows.Forms.PictureBox
    Friend WithEvents btnScaleUpToFit As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnScaleDown As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnPrint As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnSetup As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnClockwise As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnAntiClockwise As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnLeft As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnRight As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnUp As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnDown As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnHelp As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnViewOnePage As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnViewAllPages As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnScaleUp As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnFirstPage As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnPreviousPage As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnNextPage As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents btnLastPage As AutomateControls.Buttons.StandardStyledButton
    Friend WithEvents Panel1 As System.Windows.Forms.Panel
    Friend WithEvents Panel2 As System.Windows.Forms.Panel
    '   <System.Diagnostics.DebuggerStepThrough()> 
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents txtPageNumber As AutomateControls.Textboxes.StyledTextBox
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmPrintPreview))
        Me.moPrintPreviewControl = New System.Windows.Forms.PrintPreviewControl()
        Me.PageSetupDialog1 = New System.Windows.Forms.PageSetupDialog()
        Me.PrintDialog1 = New System.Windows.Forms.PrintDialog()
        Me.cmbZoom = New System.Windows.Forms.ComboBox()
        Me.cmbPageLayout = New System.Windows.Forms.ComboBox()
        Me.Panel1 = New System.Windows.Forms.Panel()
        Me.txtPageNumber = New AutomateControls.Textboxes.StyledTextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.btnFirstPage = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnPreviousPage = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnNextPage = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnLastPage = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnViewOnePage = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnViewAllPages = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.Panel2 = New System.Windows.Forms.Panel()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.btnScaleUp = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnScaleDown = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnDown = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnUp = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnRight = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnLeft = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnAntiClockwise = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnClockwise = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnScaleUpToFit = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.HiddenBluePrismLogo = New System.Windows.Forms.PictureBox()
        Me.btnHelp = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnSetup = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.btnPrint = New AutomateControls.Buttons.StandardStyledButton(Me.components)
        Me.Panel1.SuspendLayout
        Me.Panel2.SuspendLayout
        CType(Me.HiddenBluePrismLogo,System.ComponentModel.ISupportInitialize).BeginInit
        Me.SuspendLayout
        '
        'moPrintPreviewControl
        '
        resources.ApplyResources(Me.moPrintPreviewControl, "moPrintPreviewControl")
        Me.moPrintPreviewControl.AutoZoom = false
        Me.moPrintPreviewControl.Name = "moPrintPreviewControl"
        Me.moPrintPreviewControl.Zoom = 1R
        '
        'PrintDialog1
        '
        Me.PrintDialog1.AllowSelection = true
        Me.PrintDialog1.AllowSomePages = true
        '
        'cmbZoom
        '
        resources.ApplyResources(Me.cmbZoom, "cmbZoom")
        Me.cmbZoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbZoom.Items.AddRange(New Object() {resources.GetString("cmbZoom.Items"), resources.GetString("cmbZoom.Items1"), resources.GetString("cmbZoom.Items2"), resources.GetString("cmbZoom.Items3"), resources.GetString("cmbZoom.Items4"), resources.GetString("cmbZoom.Items5"), resources.GetString("cmbZoom.Items6"), resources.GetString("cmbZoom.Items7"), resources.GetString("cmbZoom.Items8")})
        Me.cmbZoom.Name = "cmbZoom"
        '
        'cmbPageLayout
        '
        resources.ApplyResources(Me.cmbPageLayout, "cmbPageLayout")
        Me.cmbPageLayout.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cmbPageLayout.Name = "cmbPageLayout"
        '
        'Panel1
        '
        resources.ApplyResources(Me.Panel1, "Panel1")
        Me.Panel1.Controls.Add(Me.txtPageNumber)
        Me.Panel1.Controls.Add(Me.Label7)
        Me.Panel1.Controls.Add(Me.Label5)
        Me.Panel1.Controls.Add(Me.btnFirstPage)
        Me.Panel1.Controls.Add(Me.btnPreviousPage)
        Me.Panel1.Controls.Add(Me.btnNextPage)
        Me.Panel1.Controls.Add(Me.btnLastPage)
        Me.Panel1.Controls.Add(Me.btnViewOnePage)
        Me.Panel1.Controls.Add(Me.btnViewAllPages)
        Me.Panel1.Controls.Add(Me.cmbZoom)
        Me.Panel1.Controls.Add(Me.Label2)
        Me.Panel1.Name = "Panel1"
        '
        'txtPageNumber
        '
        resources.ApplyResources(Me.txtPageNumber, "txtPageNumber")
        Me.txtPageNumber.BorderColor = System.Drawing.Color.Empty
        Me.txtPageNumber.Name = "txtPageNumber"
        '
        'Label7
        '
        resources.ApplyResources(Me.Label7, "Label7")
        Me.Label7.Name = "Label7"
        '
        'Label5
        '
        resources.ApplyResources(Me.Label5, "Label5")
        Me.Label5.Name = "Label5"
        '
        'btnFirstPage
        '
        resources.ApplyResources(Me.btnFirstPage, "btnFirstPage")
        Me.btnFirstPage.Name = "btnFirstPage"
        Me.btnFirstPage.UseVisualStyleBackColor = false
        '
        'btnPreviousPage
        '
        resources.ApplyResources(Me.btnPreviousPage, "btnPreviousPage")
        Me.btnPreviousPage.Name = "btnPreviousPage"
        Me.btnPreviousPage.UseVisualStyleBackColor = false
        '
        'btnNextPage
        '
        resources.ApplyResources(Me.btnNextPage, "btnNextPage")
        Me.btnNextPage.Name = "btnNextPage"
        Me.btnNextPage.UseVisualStyleBackColor = false
        '
        'btnLastPage
        '
        resources.ApplyResources(Me.btnLastPage, "btnLastPage")
        Me.btnLastPage.Name = "btnLastPage"
        Me.btnLastPage.UseVisualStyleBackColor = false
        '
        'btnViewOnePage
        '
        resources.ApplyResources(Me.btnViewOnePage, "btnViewOnePage")
        Me.btnViewOnePage.Image = Global.AutomateUI.My.Resources.ToolImages.Window_16x16
        Me.btnViewOnePage.Name = "btnViewOnePage"
        Me.btnViewOnePage.UseVisualStyleBackColor = false
        '
        'btnViewAllPages
        '
        resources.ApplyResources(Me.btnViewAllPages, "btnViewAllPages")
        Me.btnViewAllPages.Image = Global.AutomateUI.My.Resources.ToolImages.Window_New_Window_16x16
        Me.btnViewAllPages.Name = "btnViewAllPages"
        Me.btnViewAllPages.UseVisualStyleBackColor = false
        '
        'Label2
        '
        resources.ApplyResources(Me.Label2, "Label2")
        Me.Label2.Name = "Label2"
        '
        'Label6
        '
        resources.ApplyResources(Me.Label6, "Label6")
        Me.Label6.Name = "Label6"
        '
        'Panel2
        '
        resources.ApplyResources(Me.Panel2, "Panel2")
        Me.Panel2.Controls.Add(Me.Label3)
        Me.Panel2.Controls.Add(Me.btnScaleUp)
        Me.Panel2.Controls.Add(Me.btnScaleDown)
        Me.Panel2.Controls.Add(Me.btnDown)
        Me.Panel2.Controls.Add(Me.btnUp)
        Me.Panel2.Controls.Add(Me.btnRight)
        Me.Panel2.Controls.Add(Me.btnLeft)
        Me.Panel2.Controls.Add(Me.btnAntiClockwise)
        Me.Panel2.Controls.Add(Me.btnClockwise)
        Me.Panel2.Controls.Add(Me.btnScaleUpToFit)
        Me.Panel2.Controls.Add(Me.Label1)
        Me.Panel2.Controls.Add(Me.Label4)
        Me.Panel2.Controls.Add(Me.Label6)
        Me.Panel2.Controls.Add(Me.cmbPageLayout)
        Me.Panel2.Name = "Panel2"
        '
        'Label3
        '
        resources.ApplyResources(Me.Label3, "Label3")
        Me.Label3.Name = "Label3"
        '
        'btnScaleUp
        '
        resources.ApplyResources(Me.btnScaleUp, "btnScaleUp")
        Me.btnScaleUp.Image = Global.AutomateUI.My.Resources.ToolImages.Zoom_In_2_16x16
        Me.btnScaleUp.Name = "btnScaleUp"
        Me.btnScaleUp.UseVisualStyleBackColor = false
        '
        'btnScaleDown
        '
        resources.ApplyResources(Me.btnScaleDown, "btnScaleDown")
        Me.btnScaleDown.Image = Global.AutomateUI.My.Resources.ToolImages.Zoom_Out_2_16x16
        Me.btnScaleDown.Name = "btnScaleDown"
        Me.btnScaleDown.UseVisualStyleBackColor = false
        '
        'btnDown
        '
        resources.ApplyResources(Me.btnDown, "btnDown")
        Me.btnDown.Image = Global.AutomateUI.My.Resources.ToolImages.Nudge_Down_16x16
        Me.btnDown.Name = "btnDown"
        Me.btnDown.UseVisualStyleBackColor = false
        '
        'btnUp
        '
        resources.ApplyResources(Me.btnUp, "btnUp")
        Me.btnUp.Image = Global.AutomateUI.My.Resources.ToolImages.Nudge_Up_16x16
        Me.btnUp.Name = "btnUp"
        Me.btnUp.UseVisualStyleBackColor = false
        '
        'btnRight
        '
        resources.ApplyResources(Me.btnRight, "btnRight")
        Me.btnRight.Image = Global.AutomateUI.My.Resources.ToolImages.Nudge_Right_16x16
        Me.btnRight.Name = "btnRight"
        Me.btnRight.UseVisualStyleBackColor = false
        '
        'btnLeft
        '
        resources.ApplyResources(Me.btnLeft, "btnLeft")
        Me.btnLeft.Image = Global.AutomateUI.My.Resources.ToolImages.Nudge_Left_16x16
        Me.btnLeft.Name = "btnLeft"
        Me.btnLeft.UseVisualStyleBackColor = false
        '
        'btnAntiClockwise
        '
        resources.ApplyResources(Me.btnAntiClockwise, "btnAntiClockwise")
        Me.btnAntiClockwise.Image = Global.AutomateUI.My.Resources.ToolImages.Rotate_Left_16x16
        Me.btnAntiClockwise.Name = "btnAntiClockwise"
        Me.btnAntiClockwise.UseVisualStyleBackColor = false
        '
        'btnClockwise
        '
        resources.ApplyResources(Me.btnClockwise, "btnClockwise")
        Me.btnClockwise.Image = Global.AutomateUI.My.Resources.ToolImages.Rotate_Right_16x16
        Me.btnClockwise.Name = "btnClockwise"
        Me.btnClockwise.UseVisualStyleBackColor = false
        '
        'btnScaleUpToFit
        '
        resources.ApplyResources(Me.btnScaleUpToFit, "btnScaleUpToFit")
        Me.btnScaleUpToFit.Image = Global.AutomateUI.My.Resources.ToolImages.Same_Size_Both_16x16
        Me.btnScaleUpToFit.Name = "btnScaleUpToFit"
        Me.btnScaleUpToFit.UseVisualStyleBackColor = false
        '
        'Label1
        '
        resources.ApplyResources(Me.Label1, "Label1")
        Me.Label1.Name = "Label1"
        '
        'Label4
        '
        resources.ApplyResources(Me.Label4, "Label4")
        Me.Label4.Name = "Label4"
        '
        'HiddenBluePrismLogo
        '
        resources.ApplyResources(Me.HiddenBluePrismLogo, "HiddenBluePrismLogo")
        Me.HiddenBluePrismLogo.Name = "HiddenBluePrismLogo"
        Me.HiddenBluePrismLogo.TabStop = false
        '
        'btnHelp
        '
        resources.ApplyResources(Me.btnHelp, "btnHelp")
        Me.btnHelp.Image = Global.AutomateUI.My.Resources.ToolImages.Help_16x16
        Me.btnHelp.Name = "btnHelp"
        Me.btnHelp.UseVisualStyleBackColor = false
        '
        'btnSetup
        '
        resources.ApplyResources(Me.btnSetup, "btnSetup")
        Me.btnSetup.Image = Global.AutomateUI.My.Resources.ToolImages.Settings_16x16
        Me.btnSetup.Name = "btnSetup"
        Me.btnSetup.UseVisualStyleBackColor = false
        '
        'btnPrint
        '
        resources.ApplyResources(Me.btnPrint, "btnPrint")
        Me.btnPrint.Image = Global.AutomateUI.My.Resources.ToolImages.Print_16x16
        Me.btnPrint.Name = "btnPrint"
        Me.btnPrint.UseVisualStyleBackColor = false
        '
        'frmPrintPreview
        '
        Me.BackColor = System.Drawing.SystemColors.Control
        resources.ApplyResources(Me, "$this")
        Me.Controls.Add(Me.Panel2)
        Me.Controls.Add(Me.Panel1)
        Me.Controls.Add(Me.HiddenBluePrismLogo)
        Me.Controls.Add(Me.btnHelp)
        Me.Controls.Add(Me.btnSetup)
        Me.Controls.Add(Me.btnPrint)
        Me.Controls.Add(Me.moPrintPreviewControl)
        Me.Name = "frmPrintPreview"
        Me.Panel1.ResumeLayout(false)
        Me.Panel1.PerformLayout
        Me.Panel2.ResumeLayout(false)
        CType(Me.HiddenBluePrismLogo,System.ComponentModel.ISupportInitialize).EndInit
        Me.ResumeLayout(false)

End Sub

#End Region

#Region "Members"

    ''' <summary>
    ''' The percentage step used to change the scale of the image.
    ''' </summary>
    Private Const csngScaleStep As Single = 0.2
    ''' <summary>
    ''' The distance to offset the image when user clicks a nudge button.
    ''' </summary>
    Private Const miNudgeStep As Integer = 30
    ''' <summary>
    ''' The scale of the logo image.
    ''' </summary>
    Private Const ciBluePrismLogoScale As Integer = 10
    ''' <summary>
    ''' The gap between the bottom edge of the border and the logo.
    ''' </summary>
    Private Const ciBluePrismLogoGap As Integer = 4


    ''' <summary>
    ''' The linear translation used to nudge the image.
    ''' </summary>
    Private miNudgeTranslationX As Integer
    ''' <summary>
    ''' The linear translation used to nudge the image.
    ''' </summary>
    Private miNudgeTranslationY As Integer
    ''' <summary>
    ''' The image rotation.
    ''' </summary>
    Private miImageRotation As Integer
    ''' <summary>
    ''' The image scale.
    ''' </summary>
    Private msngImageScale As Single = 1
    ''' <summary>
    ''' The number of page columns.
    ''' </summary>
    Private miColumnCount As Integer
    ''' <summary>
    ''' The number of page rows.
    ''' </summary>
    Private miRowCount As Integer
    ''' <summary>
    ''' The current column.
    ''' </summary>
    Private miCurrentColumn As Integer
    ''' <summary>
    ''' The current row.
    ''' </summary>
    Private miCurrentRow As Integer
    ''' <summary>
    ''' The current page number.
    ''' </summary>
    Private miCurrentPage As Integer
    ''' <summary>
    ''' A counter to use on the 'Page x of y' section of the border.
    ''' </summary>
    Private miPrintedPageNumber As Integer
    ''' <summary>
    ''' The mouse drag start position.
    ''' </summary>
    Private moMouseStart As Point
    ''' <summary>
    ''' Indicates that the set up has been done.
    ''' </summary>
    Private mbSetupDocumentDone As Boolean
    ''' <summary>
    ''' Indicates previewing rather than printing to a printer.
    ''' </summary>
    Private mbPreviewing As Boolean
    ''' <summary>
    ''' Indicates that the process should be scaled up to fill the current page layout.
    ''' </summary>
    Private mbScaleUpToFit As Boolean
    ''' <summary>
    ''' Indicates that the current number of rows and columns will dictate the 
    ''' process scale. By default the number of rows and columns is dictated by the 
    ''' size of the process.
    ''' </summary>
    Private mbFixedNumberOfPages As Boolean
    ''' <summary>
    ''' A flag to block concurrent printing.
    ''' </summary>
    Private Shared mbPrintingInProgress As Boolean

    ''' <summary>
    ''' The print document object.
    ''' </summary>
    Private moPrintDocument As PrintDocument
    ''' <summary>
    ''' The process object.
    ''' </summary>
    Private moProcess As clsProcess
    ''' <summary>
    ''' The renderer object.
    ''' </summary>
    Private moRenderer As clsRenderer
    ''' <summary>
    ''' The rectangular area covered by the process.
    ''' </summary>
    Private mrProcessArea As Rectangle
    ''' <summary>
    ''' The name of the current process page.
    ''' </summary>
    Private msProcessPageName As String
    ''' <summary>
    ''' The process name.
    ''' </summary>
    Private msProcessName As String
    ''' <summary>
    ''' A list of pages that cover blank areas of the process.
    ''' </summary>
    Private maBlankPages As List(Of Integer)

#End Region

#Region "PrintProcess"
    ''' <summary>
    ''' Prints every page of a process scaled to fit on individual sheets. Paper 
    ''' orientation is determined by the proportions of the process diagram.
    ''' </summary>
    ''' <param name="d">The print document</param>
    ''' <param name="p">The process</param>
    ''' <param name="r">The renderer</param>
    Public Sub PrintProcess(ByVal d As PrintDocument, ByVal p As clsProcess, ByVal r As clsRenderer)

        SetupPrintDocument(d)

        If Not mbSetupDocumentDone Then
            Exit Sub
        End If

        moProcess = p
        moProcess.SelectNone()
        msProcessName = moProcess.Name

        moRenderer = r

        mbPrintingInProgress = True
        btnPrint.Enabled = False

        Dim gCurrentSubSheetID As Guid = moProcess.GetActiveSubSheet()

        'Get the pages from the process 
        For Each objSubSheet As clsProcessSubSheet In moProcess.SubSheets
            msProcessPageName = objSubSheet.Name

            moProcess.SetActiveSubSheet(objSubSheet.ID)
            mrProcessArea = GetProcessArea(moProcess)
            moPrintDocument.DefaultPageSettings.Landscape = mrProcessArea.Width >= mrProcessArea.Height

            mbPreviewing = False
            mbScaleUpToFit = True
            msngImageScale = 1
            Reset()

            Try
                moPrintDocument.Print()
            Catch ex As Exception
                UserMessage.OK(String.Format(My.Resources.frmPrintPreview_AnErrorHasOccurredTryingToPrintOn0, moPrintDocument.PrinterSettings.PrinterName) _
                 & vbCrLf & vbCrLf & ex.Message)
                Exit For
            End Try

        Next

        moProcess.SetActiveSubSheet(gCurrentSubSheetID)

        mbPrintingInProgress = False
        btnPrint.Enabled = True

    End Sub

#End Region
#Region "PrintOnOnePage"
    ''' <summary>
    ''' Prints the current page of a process scaled to fit on one sheet. Paper 
    ''' orientation is determined by the proportions of the process diagram.
    ''' </summary>
    ''' <param name="d">The print document</param>
    ''' <param name="p">The process</param>
    ''' <param name="r">The renderer</param>
    Public Sub PrintOnOnePage(ByVal d As PrintDocument, ByVal p As clsProcess, ByVal r As clsRenderer)

        If Not mbPrintingInProgress Then

            SetupPrintDocument(d)

            If Not mbSetupDocumentDone Then
                Exit Sub
            End If

            moProcess = p
            moProcess.SelectNone()

            moRenderer = r

            mrProcessArea = GetProcessArea(moProcess)
            moPrintDocument.DefaultPageSettings.Landscape = mrProcessArea.Width >= mrProcessArea.Height

            Dim bSubsheet As Guid
            bSubsheet = moProcess.GetActiveSubSheet()
            If bSubsheet.Equals(Guid.Empty) Then
                msProcessPageName = My.Resources.frmPrintPreview_MainPage
            Else
                msProcessPageName = moProcess.GetSubSheetName(bSubsheet)
            End If
            msProcessName = moProcess.Name

            mbScaleUpToFit = True
            SendToPrinter()

        End If

    End Sub

#End Region
#Region "PrintPreview"

    ''' <summary>
    ''' Previews the current page of a process. No scaling or rotation is used initially 
    ''' and paper orientation is determined by the proportions of the process diagram.
    ''' </summary>
    ''' <param name="d">The print document</param>
    ''' <param name="p">The process</param>
    ''' <param name="r">The renderer</param>
    Public Sub PrintPreview(ByVal d As PrintDocument, ByVal p As clsProcess, ByVal r As clsRenderer)

        SetupPrintDocument(d)

        If Not mbSetupDocumentDone Then
            Exit Sub
        End If

        moProcess = p
        moProcess.SelectNone()

        moRenderer = r

        mrProcessArea = GetProcessArea(moProcess)
        moPrintDocument.DefaultPageSettings.Landscape = mrProcessArea.Width >= mrProcessArea.Height

        Dim bSubsheet As Guid
        bSubsheet = moProcess.GetActiveSubSheet()
        If bSubsheet.Equals(Guid.Empty) Then
            msProcessPageName = My.Resources.frmPrintPreview_MainPage
        Else
            msProcessPageName = moProcess.GetSubSheetName(bSubsheet)
        End If
        msProcessName = moProcess.Name
        Me.Text = String.Format(My.Resources.frmPrintPreview_0PrintPreview12, ApplicationProperties.ApplicationName, msProcessName, msProcessPageName)

        ReDrawDefaultPreview()

    End Sub

#End Region

#Region "SetupPrintDocument"
    ''' <summary>
    ''' Applies initial settings to the print document.
    ''' </summary>
    ''' <param name="p">The print document</param>
    Private Sub SetupPrintDocument(ByRef p As PrintDocument)

        If Not mbSetupDocumentDone Then

            moPrintDocument = p
            If moPrintDocument.PrinterSettings.IsValid Then
                moPrintDocument.DefaultPageSettings.Landscape = True

                Dim iMargin, iBluePrismHeight As Integer

                iBluePrismHeight = CInt(Me.HiddenBluePrismLogo.Image.Height / ciBluePrismLogoScale)

                'Fix top, left and right margins at 7mm, with bottom margin allowing for logo and page title details
                iMargin = CInt(7 / 0.254)
                moPrintDocument.DefaultPageSettings.Margins = New System.Drawing.Printing.Margins(iMargin, iMargin, iMargin, iMargin + ciBluePrismLogoGap + iBluePrismHeight)
                PageSetupDialog1.AllowMargins = True

                'Set print document.
                moPrintPreviewControl.Document = moPrintDocument
                PageSetupDialog1.Document = moPrintDocument
                PrintDialog1.Document = moPrintDocument
                PrintDialog1.PrinterSettings.PrintRange = PrintRange.AllPages

                AddHandler moPrintDocument.BeginPrint, AddressOf Me.moPrintDocument_BeginPrint
                AddHandler moPrintDocument.PrintPage, AddressOf Me.moPrintDocument_PrintPage
                AddHandler moPrintDocument.EndPrint, AddressOf Me.moPrintDocument_EndPrint

                mbSetupDocumentDone = True
            Else
                'Note that when the printer is invalid, that the printername will be
                ' "Default printer is not set." See Bug# 3380
                UserMessage.Show(moPrintDocument.PrinterSettings.PrinterName)
            End If
        End If

    End Sub

    ''' <summary>
    ''' Provides access to the value indicating that the setup for the document has
    ''' been done.
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    Public ReadOnly Property SetupDocumentDone() As Boolean
        Get
            Return mbSetupDocumentDone
        End Get
    End Property

#End Region
#Region "Reset"
    ''' <summary>
    ''' Re-initialises page counters.
    ''' </summary>
    Private Sub Reset()

        miCurrentPage = 0
        miPrintedPageNumber = 0

        If Not mbFixedNumberOfPages Then
            miRowCount = 0
            miColumnCount = 0
        End If

    End Sub

#End Region

#Region "GetBlankPages"

    ''' <summary>
    ''' Identifies pages covering empty areas of the process.
    ''' </summary>
    ''' <param name="rProcess">The process area</param>
    ''' <param name="iPageWidth">The width of the page</param>
    ''' <param name="iPageHeight">The height of the page</param>
    ''' <param name="iColumns">The page columns</param>
    ''' <param name="iRows">The page rows</param>
    ''' <param name="iAngle">The page rotation</param>
    ''' <param name="iNudgeX">The x-axis nudge or nudge applied by the user</param>
    ''' <param name="iNudgeY">The y-axis nudge or nudge applied by the user</param>
    ''' <returns>The blank page numbers</returns>
    Private Function GetBlankPages(ByVal rProcess As Rectangle, ByVal iPageWidth As Integer, ByVal iPageHeight As Integer, ByVal iColumns As Integer, ByVal iRows As Integer, ByVal iAngle As Integer, ByVal iNudgeX As Integer, ByVal iNudgeY As Integer) As List(Of Integer)

        Dim rPageProcessArea As Rectangle
        Dim iNumberOfPages As Integer
        Dim aBlankPages As New List(Of Integer)


        iNumberOfPages = CInt(iColumns * iRows)
        For iPageNumber As Integer = 1 To iNumberOfPages
            rPageProcessArea = GetPageProcessArea(rProcess, iPageWidth, iPageHeight, iPageNumber, iColumns, iRows, iAngle)
            rPageProcessArea.X -= iNudgeX
            rPageProcessArea.Y -= iNudgeY
            If Not ProcessAreaContainsStages(moProcess, rPageProcessArea) Then
                aBlankPages.Add(iPageNumber)
            End If
        Next

        Return aBlankPages

    End Function

#End Region
#Region "GetPageProcessArea"

    ''' <summary>
    ''' Gets the area covered by a page in world (process) coords.
    ''' </summary>
    ''' <param name="rProcess">The process area</param>
    ''' <param name="iPageWidth">The width of the page</param>
    ''' <param name="iPageHeight">The height of the page</param>
    ''' <param name="iPageNumber"></param>
    ''' <param name="iColumns">The page columns</param>
    ''' <param name="iRows">The page rows</param>
    ''' <param name="iAngle">The page rotation</param>
    ''' <returns></returns>
    Private Function GetPageProcessArea(ByVal rProcess As Rectangle, ByVal iPageWidth As Integer, ByVal iPageHeight As Integer, ByVal iPageNumber As Integer, ByVal iColumns As Integer, ByVal iRows As Integer, ByVal iAngle As Integer) As Rectangle

        Dim mxRotation, mxPage As Matrix
        Dim pPageOneTL, pPageOneBL, pPageOneTR, pPageOneBR As Point
        Dim aPageOnePoints As Point()
        Dim rPageOne, rAllPages, rPageProcessArea As Rectangle
        Dim pProcessCentre As PointF

        'Get the area of the process covered by all the pages at the 
        'default angle of zero degrees.
        rAllPages = New Rectangle
        rAllPages.Width = CInt(iPageWidth * iColumns)
        rAllPages.Height = CInt(iPageHeight * iRows)
        rAllPages.X = rProcess.X + CInt((rProcess.Width - rAllPages.Width) / 2)
        rAllPages.Y = rProcess.Y + CInt((rProcess.Height - rAllPages.Height) / 2)

        'Get the area of the default first page in the top left corner
        rPageOne = New Rectangle
        rPageOne.Width = iPageWidth
        rPageOne.Height = iPageHeight
        rPageOne.X = rAllPages.X
        rPageOne.Y = rAllPages.Y

        'Get the four corner points of page one
        pPageOneTL = New Point(rPageOne.X, rPageOne.Y)
        pPageOneBL = New Point(rPageOne.X, rPageOne.Y + rPageOne.Height)
        pPageOneTR = New Point(rPageOne.X + rPageOne.Width, rPageOne.Y)
        pPageOneBR = New Point(rPageOne.X + rPageOne.Width, rPageOne.Y + rPageOne.Height)
        aPageOnePoints = New Point() {pPageOneTL, pPageOneTR, pPageOneBR, pPageOneBL}

        'Rotate the page one points around the centre of the process.
        'NB The page rotation in the opposite direction to the process, 
        'hence the negative angle.
        pProcessCentre = New PointF(CSng(rProcess.X + rProcess.Width / 2), CSng(rProcess.Y + rProcess.Height / 2))
        mxRotation = New Matrix
        mxRotation.RotateAt(CSng(-iAngle), pProcessCentre, MatrixOrder.Append)
        mxRotation.TransformPoints(aPageOnePoints)

        'Transform the points again by shifting from the rotated page 
        'one position to the nth page position
        mxPage = GetPageMatrix(iPageWidth, iPageHeight, iPageNumber, iColumns, iRows, iAngle)
        mxPage.TransformPoints(aPageOnePoints)

        'Get the top left corner in world (ie process) coords
        Select Case iAngle

            Case 0
                'No rotation and the original TL point is still at TL.
                rPageProcessArea = New Rectangle(aPageOnePoints(0), New Size(iPageWidth, iPageHeight))
            Case 90
                'The page width and height are reversed and the original TR point is now at TL.
                rPageProcessArea = New Rectangle(aPageOnePoints(1), New Size(iPageHeight, iPageWidth))
            Case 180
                'The original BR point is now at TL.
                rPageProcessArea = New Rectangle(aPageOnePoints(2), New Size(iPageWidth, iPageHeight))
            Case 270
                'The page width and height are reversed the original BL point is now at TL.
                rPageProcessArea = New Rectangle(aPageOnePoints(3), New Size(iPageHeight, iPageWidth))

        End Select

        'Return the page as a rectangle world coords
        Return rPageProcessArea

    End Function

#End Region
#Region "GetPageMatrix"

    ''' <summary>
    ''' Gets the matrix to transform the position of page 1 to page n.
    ''' </summary>
    ''' <param name="iWidth">The page width</param>
    ''' <param name="iHeight">The page height</param>
    ''' <param name="iPageNumber">The page number</param>
    ''' <param name="iColumns">The page columns</param>
    ''' <param name="iRows">The page rows</param>
    ''' <param name="iAngle">The rotation</param>
    ''' <returns></returns>
    Private Function GetPageMatrix(ByVal iWidth As Integer, ByVal iHeight As Integer, ByVal iPageNumber As Integer, ByVal iColumns As Integer, ByVal iRows As Integer, ByVal iAngle As Integer) As Matrix

        Dim iColumnTranslateX, iColumnTranslateY, iRowTranslateX, iRowTranslateY As Integer
        Dim mxPage As Matrix
        Dim iRow, iColumn As Integer

        iRow = CInt(Math.Floor((iPageNumber - 1) / iColumns) + 1)
        If iPageNumber Mod iColumns = 0 Then
            iColumn = iColumns
        Else
            iColumn = iPageNumber Mod iColumns
        End If

        iRow -= 1
        iColumn -= 1

        Select Case iAngle

            Case 0
                ' 123
                ' 456
                ' 789
                iColumnTranslateX = CInt(iColumn * iWidth)
                iColumnTranslateY = 0
                iRowTranslateX = 0
                iRowTranslateY = CInt(iRow * iHeight)

            Case 90
                ' 369
                ' 258
                ' 147

                iColumnTranslateX = 0
                iColumnTranslateY = CInt(-iColumn * iWidth)
                iRowTranslateX = CInt(iRow * iHeight)
                iRowTranslateY = 0

            Case 180
                ' 987
                ' 654
                ' 321
                iColumnTranslateX = CInt(-iColumn * iWidth)
                iColumnTranslateY = 0
                iRowTranslateX = 0
                iRowTranslateY = CInt(-iRow * iHeight)


            Case 270
                ' 741
                ' 852
                ' 963
                iColumnTranslateX = 0
                iColumnTranslateY = CInt(iColumn * iWidth)
                iRowTranslateX = CInt(-iRow * iHeight)
                iRowTranslateY = 0
        End Select

        mxPage = New Matrix
        mxPage.Translate(iColumnTranslateX, iColumnTranslateY)
        mxPage.Translate(iRowTranslateX, iRowTranslateY)
        Return mxPage

    End Function

#End Region
#Region "ProcessAreaContainsStages"

    ''' <summary>
    ''' Analyses an area of the process to see if it is empty.
    ''' </summary>
    ''' <param name="oProcess">The process</param>
    ''' <param name="rArea">The area</param>
    ''' <returns></returns>
    Private Function ProcessAreaContainsStages(ByVal oProcess As clsProcess, ByVal rArea As Rectangle) As Boolean

        Dim gSubSheetID As Guid
        Dim iX, iY, iW, iH As Integer
        Dim bAreaContainsStages As Boolean
        Dim rProcessStage As Rectangle

        gSubSheetID = oProcess.GetActiveSubSheet()

        For Each stage As clsProcessStage In oProcess.GetStages()
            If stage.GetSubSheetID().Equals(gSubSheetID) Then

                iX = CInt(stage.GetDisplayX())
                iY = CInt(stage.GetDisplayY())
                iW = CInt(stage.GetDisplayWidth())
                iH = CInt(stage.GetDisplayHeight())
                rProcessStage = New Rectangle(iX - CInt(iW / 2), iY - CInt(iH / 2), iW, iH)
                If rArea.IntersectsWith(rProcessStage) Then
                    bAreaContainsStages = True
                    Exit For
                End If

            End If
        Next

        Return bAreaContainsStages

    End Function

#End Region

#Region "ReDrawPreview"
    ''' <summary>
    ''' Causes the preview to be redrawn.
    ''' </summary>
    Private Sub ReDrawPreview()

        Reset()
        mbPreviewing = True
        moPrintPreviewControl.InvalidatePreview()

    End Sub

#End Region
#Region "ReDrawDefaultPreview"
    ''' <summary>
    ''' Causes the preview to be redrawn at the default scale, with the size of the
    ''' process determining the number of pages.
    ''' </summary>
    Private Sub ReDrawDefaultPreview()

        miNudgeTranslationX = 0
        miNudgeTranslationY = 0
        msngImageScale = 1

        mbFixedNumberOfPages = False

        ReDrawPreview()

    End Sub

#End Region

#Region "SendToPrinter"

    ''' <summary>
    ''' Prints the document to the printer.
    ''' </summary>
    Private Sub SendToPrinter()

        mbPrintingInProgress = True
        btnPrint.Enabled = False

        Reset()
        mbPreviewing = False

        If moPrintDocument.PrinterSettings.IsValid Then
            Try
                moPrintDocument.Print()
            Catch ex As Exception
                UserMessage.OK(String.Format(My.Resources.frmPrintPreview_AnErrorHasOccurredTryingToPrintOn0, moPrintDocument.PrinterSettings.PrinterName) _
                 & vbCrLf & vbCrLf & ex.Message)
            End Try
        Else
            UserMessage.OK(String.Format(My.Resources.frmPrintPreview_PrinterSettingsFor0AreInvalid, moPrintDocument.PrinterSettings.PrinterName))
        End If

        mbPrintingInProgress = False
        btnPrint.Enabled = True

    End Sub

#End Region
#Region "DrawBorder"
    ''' <summary>
    ''' Draws a border rectangle and applies the BP logo and other details to the foot 
    ''' of each page.
    ''' </summary>
    ''' <param name="e"></param>
    Private Sub DrawBorder(ByRef e As System.Drawing.Printing.PrintPageEventArgs)

        Dim iX, iY As Integer
        Dim oFont As Font
        Dim sText, sSpacer As String
        Dim oSizeF As SizeF
        Dim iNumberOfPages, iRow, iColumn As Integer
        Dim iBluePrismHeight, iBluePrismWidth As Integer

        'Draw the border
        Dim oPen As New Pen(ColourScheme.Default.PrintBorder, 2)          'blue prism logo light blue
        e.Graphics.DrawRectangle(oPen, e.MarginBounds.X, e.MarginBounds.Y, e.MarginBounds.Width, e.MarginBounds.Height)

        'Draw the logo
        iY = e.MarginBounds.Y + e.MarginBounds.Height + ciBluePrismLogoGap
        iBluePrismHeight = CInt(Me.HiddenBluePrismLogo.Image.Height / ciBluePrismLogoScale)
        iBluePrismWidth = CInt(Me.HiddenBluePrismLogo.Image.Width / ciBluePrismLogoScale)
        e.Graphics.DrawImage(Me.HiddenBluePrismLogo.Image, e.MarginBounds.X, iY, iBluePrismWidth, iBluePrismHeight)

        'Get the process title
        sSpacer = "  |  "
        oFont = New Font("Segoe UI", 8)

        'Get the page number. NB Blank pages will not be included.
        If moPrintDocument.PrinterSettings.PrintRange = PrintRange.AllPages Then
            iNumberOfPages = CInt(miColumnCount * miRowCount)
            If Not maBlankPages Is Nothing Then
                iNumberOfPages -= maBlankPages.Count
            End If
        Else
            iNumberOfPages = PrintDialog1.PrinterSettings.FromPage - PrintDialog1.PrinterSettings.ToPage + 1
            If Not maBlankPages Is Nothing Then
                For i As Integer = PrintDialog1.PrinterSettings.FromPage To PrintDialog1.PrinterSettings.ToPage
                    If maBlankPages.Contains(i) Then
                        iNumberOfPages -= 1
                    End If
                Next
            End If
        End If
        miPrintedPageNumber += 1
        sText = String.Format(My.Resources.frmPrintPreview_01Page2Of3, msProcessName, msProcessPageName, CStr(miPrintedPageNumber), CStr(iNumberOfPages))

        'Get the position of this page in relation to the other pages when 
        'the process spans multiple pages. NB Blank pages are included.
        If miColumnCount > 1 Or miRowCount > 1 Then
            iRow = CInt((miCurrentPage - 1) / miColumnCount) + 1
            If miCurrentPage Mod miColumnCount = 0 Then
                iColumn = miColumnCount
            Else
                iColumn = miCurrentPage Mod miColumnCount
            End If
            sText = String.Format(My.Resources.frmPrintPreview_01Page2Of3Row4Column5, msProcessName, msProcessPageName, CStr(miPrintedPageNumber), CStr(iNumberOfPages), CStr(iRow), CStr(iColumn))
        End If

        'Get the date
        sText &= sSpacer & Format(Now, Resources.DateTimeFormat_frmPrintPreview_DdMMMYy)

        'Draw the text
        oSizeF = e.Graphics.MeasureString(sText, oFont)
        iX = CInt(e.MarginBounds.X + e.MarginBounds.Width - oSizeF.Width)
        e.Graphics.DrawString(sText, oFont, Brushes.Black, iX, iY)

        'Draw the company details on a new line
        oFont = New Font("Segoe UI", 6)
        sText = Chr(169)
        sText &= My.Resources.frmPrintPreview_BluePrismLimitedCentrixHouseCrowLaneEastNewtonLeWillowsWA129UYT08708793000F0870
        oSizeF = e.Graphics.MeasureString(sText, oFont)
        iX = CInt(e.MarginBounds.X + e.MarginBounds.Width - oSizeF.Width)
        iY += iBluePrismHeight - oFont.Height
        e.Graphics.DrawString(sText, oFont, Brushes.Black, iX, iY)

        'Clip just inside the margin. This will keep the process image within the border rectangle.
        e.Graphics.SetClip(New Rectangle(e.MarginBounds.X + 1, e.MarginBounds.Y + 1, e.MarginBounds.Width - 2, e.MarginBounds.Height - 2))

    End Sub

#End Region

#Region "GetProcessArea"
    ''' <summary>
    ''' Determines the area covered by the process.
    ''' </summary>
    ''' <param name="p">The process</param>
    ''' <returns>A rectangle of the process area</returns>
    Private Function GetProcessArea(ByVal p As clsProcess) As Rectangle

        Dim gSubSheetID As Guid
        Dim iX, iY, iXmin, iXmax, iYmin, iYmax, iW, iH As Integer
        Dim bFirst As Boolean = True

        gSubSheetID = p.GetActiveSubSheet()

        For Each stage As clsProcessStage In p.GetStages()
            If stage.GetSubSheetID().Equals(gSubSheetID) Then

                iX = CInt(stage.GetDisplayX())
                iY = CInt(stage.GetDisplayY())
                iW = CInt(stage.GetDisplayWidth() / 2)
                iH = CInt(stage.GetDisplayHeight() / 2)

                If bFirst OrElse iX - iW < iXmin Then iXmin = iX - iW
                If bFirst OrElse iX + iW > iXmax Then iXmax = iX + iW
                If bFirst OrElse iY - iH < iYmin Then iYmin = iY - iH
                If bFirst OrElse iY + iH > iYmax Then iYmax = iY + iH
                bFirst = False

            End If
        Next

        iW = Math.Abs(iXmax - iXmin)
        iH = Math.Abs(iYmax - iYmin)

        Return New Rectangle(iXmin, iYmin, iW, iH)

    End Function

#End Region

#Region "moPrintDocument_BeginPrint"

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub moPrintDocument_BeginPrint(ByVal sender As System.Object, ByVal e As System.Drawing.Printing.PrintEventArgs)


    End Sub

#End Region
#Region "moPrintDocument_PrintPage"
    ''' <summary>
    ''' This event handler is called when the print document prints or previews a 
    ''' page. It will be called repeatedly until e.HasMorePages becomes false. In 
    ''' brief, this method sizes the process to see how many pages to use and sets 
    ''' e.HasMorePages to false when it has done the last page. Coordinate translations 
    ''' are applied for each page and to cater for any rotation, scale or nudge set by 
    ''' the user.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub moPrintDocument_PrintPage(ByVal sender As System.Object, ByVal e As System.Drawing.Printing.PrintPageEventArgs)

        Dim iScaledImageWidth, iScaledImageHeight As Integer
        Dim iRotatedImageWidth, iRotatedImageHeight As Integer
        Dim iTotalPagesWidth, iTotalPagesHeight As Integer

        Dim iPageRotationTranslationX, iPageRotationTranslationY As Integer
        Dim iCentreImageTranslationX, iCentreImageTranslationY As Integer
        Dim iNudgeTranslationX, iNudgeTranslationY As Integer
        Dim iPageTranslateX, iPageTranslateY As Integer
        Dim iProcessTranslateX, iProcessTranslateY As Integer

        Dim iTemp As Integer
        Dim bPreviewing As Boolean

        If miImageRotation = 0 Or miImageRotation = 180 Then
            iRotatedImageWidth = mrProcessArea.Width
            iRotatedImageHeight = mrProcessArea.Height
        Else
            'The width and height are reversed
            iRotatedImageWidth = mrProcessArea.Height
            iRotatedImageHeight = mrProcessArea.Width
        End If


        'Calculate the number of pages required
        If miCurrentPage = 0 And Not mbFixedNumberOfPages Then
            'This is the first print page event and the size of the rotated and 
            'scaled image will determine the number of pages to span.
            While iRotatedImageWidth * msngImageScale > miColumnCount * e.MarginBounds.Width
                miColumnCount += 1
            End While

            'Find how many pages the rotated and scaled image height will span.
            While iRotatedImageHeight * msngImageScale > miRowCount * e.MarginBounds.Height
                miRowCount += 1
            End While

        End If
        iTotalPagesWidth = miColumnCount * e.MarginBounds.Width
        iTotalPagesHeight = miRowCount * e.MarginBounds.Height


        'Calculate the scaled size of the process
        If mbScaleUpToFit Then
            'Scale to fit just inside the available area.
            msngImageScale = CSng((iTotalPagesWidth - 10) / iRotatedImageWidth)

            If iRotatedImageHeight * msngImageScale > iTotalPagesHeight Then
                msngImageScale = CSng((iTotalPagesHeight - 10) / iRotatedImageHeight)
            End If
        End If
        iScaledImageWidth = CInt(mrProcessArea.Width * msngImageScale)
        iScaledImageHeight = CInt(mrProcessArea.Height * msngImageScale)


        bPreviewing = mbPreviewing
        e.HasMorePages = True

        'Decide whether to start printing at page obe or further along.
        If bPreviewing Or moPrintDocument.PrinterSettings.PrintRange = PrintRange.AllPages Then
            'We are either previewing or printing all pages, so just increment the page number.
            miCurrentPage += 1
        Else
            'We are printing some pages, so check if we are starting at page 1.
            If moPrintDocument.PrinterSettings.FromPage = 1 Then
                miCurrentPage += 1
            Else
                If miCurrentPage = 0 Then
                    'Printing is not starting at page 1, so work out what the 
                    'starting row and column numbers are.
                    miCurrentPage = moPrintDocument.PrinterSettings.FromPage
                    miCurrentRow = CInt(Math.Floor((miCurrentPage - 1) / miColumnCount) + 1)
                    If miCurrentPage Mod miColumnCount = 0 Then
                        miCurrentColumn = miColumnCount
                    Else
                        miCurrentColumn = miCurrentPage Mod miColumnCount
                    End If
                Else
                    miCurrentPage += 1
                End If
            End If

            If miCurrentPage = moPrintDocument.PrinterSettings.ToPage Then
                'This is the last page to print.
                e.HasMorePages = False
            End If

        End If


        'Find the current row and column and set the event flag 
        'to stop printing if this is the last page.
        If miCurrentPage = 1 Then

            iPageTranslateX = 0
            iPageTranslateY = 0

            If miColumnCount = 1 Then
                If miRowCount = 1 Then
                    e.HasMorePages = False
                Else
                    miCurrentRow = 2
                    miCurrentColumn = 1
                End If
            Else
                miCurrentRow = 1
                miCurrentColumn = 2
            End If

        Else

            If miCurrentRow > 1 Then
                'This is not the first row, so need to translate up.
                iPageTranslateY = (1 - miCurrentRow) * e.MarginBounds.Height
            End If

            If miCurrentColumn > 1 Then
                'This is not the first column, so need to translate left.
                iPageTranslateX = (1 - miCurrentColumn) * e.MarginBounds.Width
            End If
            miCurrentColumn += 1

            If miCurrentColumn > miColumnCount Then
                'This is the end of the row.
                If miCurrentRow = miRowCount Then
                    'This is also the last row.
                    e.HasMorePages = False
                Else
                    'Move to the first column of the next row.
                    miCurrentColumn = 1
                    miCurrentRow += 1
                End If
            End If

        End If

        'Work out the translations required to draw the right part of 
        'the process on the page.
        Select Case miImageRotation
            Case 0

                'No rotation, so no need to translate.
                iPageRotationTranslationX = 0
                iPageRotationTranslationY = 0

                'Translation to move the image to centre of page(s).
                iCentreImageTranslationX = CInt((iTotalPagesWidth - iScaledImageWidth) / 2)
                iCentreImageTranslationY = CInt((iTotalPagesHeight - iScaledImageHeight) / 2)

                'Translation from user nudging or dragging the image.
                iNudgeTranslationX = miNudgeTranslationX
                iNudgeTranslationY = miNudgeTranslationY

                'Translation to move image according to page number.
                iPageTranslateX = iPageTranslateX + e.MarginBounds.Top
                iPageTranslateY = iPageTranslateY + e.MarginBounds.Left

            Case 90

                'Rotating 90 clockwise, so translate the image to 
                'the right to return the top left to the origin.
                iPageRotationTranslationX = 0
                iPageRotationTranslationY = -iScaledImageHeight

                'Translation to move image to centre of page(s).
                iCentreImageTranslationX = CInt((iTotalPagesHeight - iScaledImageWidth) / 2)
                iCentreImageTranslationY = CInt((iTotalPagesWidth - iScaledImageHeight) / -2)

                'Translation from user nudging or dragging the image.
                iNudgeTranslationX = miNudgeTranslationY
                iNudgeTranslationY = -miNudgeTranslationX

                'Translation to move image according to page number.
                iTemp = iPageTranslateX
                iPageTranslateX = iPageTranslateY + e.MarginBounds.Left
                iPageTranslateY = -iTemp - e.MarginBounds.Top

            Case 180

                'Rotating 180 clockwise, so translate the image to 
                'the right and down to return the top left to the origin.
                iPageRotationTranslationX = -iScaledImageWidth
                iPageRotationTranslationY = -iScaledImageHeight

                'Translation to move image to centre of page(s).
                iCentreImageTranslationX = CInt((iTotalPagesWidth - iScaledImageWidth) / -2)
                iCentreImageTranslationY = CInt((iTotalPagesHeight - iScaledImageHeight) / -2)

                'Translation from user nudging or dragging the image.
                iNudgeTranslationX = -miNudgeTranslationX
                iNudgeTranslationY = -miNudgeTranslationY

                'Translation to move image according to page number.
                iPageTranslateX = -iPageTranslateX - e.MarginBounds.Top
                iPageTranslateY = -iPageTranslateY - e.MarginBounds.Left

            Case 270

                'Rotating 270 clockwise, so translate the image to 
                'the down to return the top left to the origin.
                iPageRotationTranslationX = -iScaledImageWidth
                iPageRotationTranslationY = 0

                'Translation to move image to centre of page(s).
                iCentreImageTranslationX = CInt((iTotalPagesHeight - iScaledImageWidth) / -2)
                iCentreImageTranslationY = CInt((iTotalPagesWidth - iScaledImageHeight) / 2)

                'Translation from user nudging or dragging the image.
                iNudgeTranslationX = -miNudgeTranslationY
                iNudgeTranslationY = miNudgeTranslationX

                'Translation to move image according to page number.
                iTemp = iPageTranslateX
                iPageTranslateX = -iPageTranslateY - e.MarginBounds.Left
                iPageTranslateY = iTemp + e.MarginBounds.Top

        End Select


        'Check the area of the process covered by each page
        If maBlankPages Is Nothing Then
            'The check has not been done for this sequence of print page events
            maBlankPages = GetBlankPages(mrProcessArea, CInt(e.MarginBounds.Width / msngImageScale), CInt(e.MarginBounds.Height / msngImageScale), miColumnCount, miRowCount, miImageRotation, iNudgeTranslationX, iNudgeTranslationY)
        End If

        If maBlankPages.Contains(miCurrentPage) Then

            'No part of the process falls on this page, so print nothing.

        Else

            'Part of the process falls on this page, so print as normal.

            If Not bPreviewing Then
                'Translate to put margin origin just inside print area origin
                e.Graphics.TranslateTransform(-e.MarginBounds.Left + 1, -e.MarginBounds.Top + 1)
            End If

            DrawBorder(e)

            If miImageRotation > 0 Then
                e.Graphics.RotateTransform(miImageRotation)
                e.Graphics.TranslateTransform(iPageRotationTranslationX, iPageRotationTranslationY)
            End If

            If Not iCentreImageTranslationX = 0 Or Not iCentreImageTranslationY = 0 Then
                e.Graphics.TranslateTransform(iCentreImageTranslationX, iCentreImageTranslationY)
            End If

            If miNudgeTranslationX <> 0 Or miNudgeTranslationY <> 0 Then
                e.Graphics.TranslateTransform(iNudgeTranslationX, iNudgeTranslationY)
            End If

            If iPageTranslateX <> 0 Or iPageTranslateY <> 0 Then
                e.Graphics.TranslateTransform(iPageTranslateX, iPageTranslateY)
            End If

            e.Graphics.ScaleTransform(msngImageScale, msngImageScale)

            'Translation to move process origin
            iProcessTranslateX = -mrProcessArea.X - CInt(mrProcessArea.Width / 2)
            iProcessTranslateY = -mrProcessArea.Y - CInt(mrProcessArea.Height / 2)
            If iProcessTranslateX <> 0 Or iProcessTranslateY <> 0 Then
                e.Graphics.TranslateTransform(iProcessTranslateX, iProcessTranslateY)
            End If

            Dim sngOldCamX, sngOldCamY, sngOldZoom As Single
            'Preserve old camera and zoom information...
            sngOldCamX = moProcess.GetCameraX()
            sngOldCamY = moProcess.GetCameraY()
            sngOldZoom = moProcess.Zoom

            'Calculate camera and zoom for this page...
            moProcess.Zoom = 1
            moProcess.SetCameraX(0)
            moProcess.SetCameraY(0)

            'Draw the relevant area...
            moRenderer.UpdateView(e.Graphics, mrProcessArea, moProcess, False, False)

            'Restore old camera and zoom information...
            moProcess.SetCameraX(sngOldCamX)
            moProcess.SetCameraY(sngOldCamY)
            moProcess.Zoom = sngOldZoom

        End If

    End Sub

#End Region
#Region "moPrintDocument_EndPrint"

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub moPrintDocument_EndPrint(ByVal sender As System.Object, ByVal e As System.Drawing.Printing.PrintEventArgs)

        If mbPreviewing Then
            moPrintPreviewControl.Rows = miRowCount
            moPrintPreviewControl.Columns = miColumnCount
            PopulatePageLayoutList(miRowCount, miColumnCount)
        End If

        mbScaleUpToFit = False
        mbPreviewing = False
        maBlankPages = Nothing

    End Sub

#End Region

#Region "frmPrintPreview_Load"

    Private Sub frmPrintPreview_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        miCurrentPage = 0
        miNudgeTranslationX = 0
        miNudgeTranslationY = 0
        msngImageScale = 1
        miImageRotation = 0
        miColumnCount = 0
        miRowCount = 0

        Dim iX, iY As Integer

        Me.Width = 900
        Me.Height = 700
        For Each oScreen As Screen In Screen.AllScreens
            iX = CInt((oScreen.Bounds.Width - Me.Width) / 2)
            iY = CInt((oScreen.Bounds.Height - Me.Height) / 2)
        Next
        Me.Location = New Point(iX, iY)

        moPrintPreviewControl.AutoZoom = True
        cmbZoom.SelectedIndex = 0
        AddHandler cmbZoom.SelectedIndexChanged, AddressOf cmbZoom_SelectedIndexChanged

        AddHandler moPrintPreviewControl.MouseDown, AddressOf Me.PrintPreviewControl_MouseDown
        AddHandler moPrintPreviewControl.MouseUp, AddressOf Me.PrintPreviewControl_MouseUp
        AddHandler moPrintPreviewControl.MouseMove, AddressOf Me.PrintPreviewControl_Mousemove

        Dim oToolTip As New ToolTip
        oToolTip.AutoPopDelay = 5000
        oToolTip.InitialDelay = 1000
        oToolTip.ReshowDelay = 500
        oToolTip.ShowAlways = True

        oToolTip.SetToolTip(Me.btnAntiClockwise, My.Resources.frmPrintPreview_RotateTheImageAnticlockwise)
        oToolTip.SetToolTip(Me.btnClockwise, My.Resources.frmPrintPreview_RotateTheImageClockwise)
        oToolTip.SetToolTip(Me.btnUp, My.Resources.frmPrintPreview_NudgeTheImageUp)
        oToolTip.SetToolTip(Me.btnDown, My.Resources.frmPrintPreview_NudgeTheImageDown)
        oToolTip.SetToolTip(Me.btnLeft, My.Resources.frmPrintPreview_NudgeTheImageLeft)
        oToolTip.SetToolTip(Me.btnRight, My.Resources.frmPrintPreview_NudgeTheImageRight)
        oToolTip.SetToolTip(Me.btnFirstPage, My.Resources.frmPrintPreview_GoToFirstPage)
        oToolTip.SetToolTip(Me.btnLastPage, My.Resources.frmPrintPreview_GoToTheLastPage)
        oToolTip.SetToolTip(Me.btnNextPage, My.Resources.frmPrintPreview_GoToTheNextPage)
        oToolTip.SetToolTip(Me.btnPreviousPage, My.Resources.frmPrintPreview_GoToThePreviousPage)
        oToolTip.SetToolTip(Me.btnHelp, My.Resources.frmPrintPreview_Help)
        oToolTip.SetToolTip(Me.btnPrint, My.Resources.frmPrintPreview_Print)
        oToolTip.SetToolTip(Me.btnScaleDown, My.Resources.frmPrintPreview_ScaleTheImageDown)
        oToolTip.SetToolTip(Me.btnScaleUp, My.Resources.frmPrintPreview_ScaleTheImageUp)
        oToolTip.SetToolTip(Me.btnScaleUpToFit, My.Resources.frmPrintPreview_ScaleTheImageUpToFillTheCurrentPageLayout)
        oToolTip.SetToolTip(Me.btnSetup, My.Resources.frmPrintPreview_PageSetup)
        oToolTip.SetToolTip(Me.btnViewAllPages, My.Resources.frmPrintPreview_DisplayAllPagesSideBySide)
        oToolTip.SetToolTip(Me.btnViewOnePage, My.Resources.frmPrintPreview_DisplayOnePageAtATime)
        oToolTip.SetToolTip(Me.cmbPageLayout, My.Resources.frmPrintPreview_SelectTheNumberOfPagesToDisplayTheImageOn)
        oToolTip.SetToolTip(Me.cmbZoom, My.Resources.frmPrintPreview_ZoomInOrOutOnTheImage)

    End Sub

#End Region

#Region "cmbZoom_SelectedIndexChanged"

    Private Sub cmbZoom_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)

        Dim sZoom As String

        If cmbZoom.SelectedIndex > 0 Then
            'Trim the % character
            sZoom = cmbZoom.Text.Substring(0, cmbZoom.Text.Length - 1)
            moPrintPreviewControl.Zoom = CInt(sZoom.Trim) / 100
        Else
            moPrintPreviewControl.AutoZoom = True
        End If

    End Sub

#End Region
#Region "cmbPageLayout_SelectedIndexChanged"

    ''' <summary>
    ''' Redraws the preview with the process scaled to fit the selected page layout.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub cmbPageLayout_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles cmbPageLayout.SelectedIndexChanged

        Dim sText As String
        Dim aText As String()
        Dim iR, iC As Integer

        If cmbPageLayout.SelectedIndex = 0 Then
            'Auto
            ReDrawDefaultPreview()
        Else
            sText = cmbPageLayout.Text
            aText = sText.Split("x"c)
            iC = CInt(aText(0))
            iR = CInt(aText(1))
            If iC = miColumnCount And iR = miRowCount Then
                'No change, so do nothing
            Else
                'Redraw the preview scaled to fit the new page layout
                miColumnCount = iC
                miRowCount = iR
                miNudgeTranslationX = 0
                miNudgeTranslationY = 0
                msngImageScale = 1
                miCurrentPage = 0
                mbPreviewing = True
                mbScaleUpToFit = True
                mbFixedNumberOfPages = True
                moPrintPreviewControl.InvalidatePreview()
            End If
        End If

    End Sub

#End Region
#Region "PopulatePageLayoutList"

    ''' <summary>
    ''' Refills the page layout drop down list, from 1x1 up to five steps beyond the
    ''' current layout. For example, when the current display is 4x2, the list will
    ''' be 1x1, 2x1, 4x2, 5x3, 6x4, 7x5, 8x6, 9x7.
    ''' </summary>
    ''' <param name="r">The current number of rows</param>
    ''' <param name="c">The current number of columns</param>
    Private Sub PopulatePageLayoutList(ByVal r As Integer, ByVal c As Integer)

        Dim sItem As String
        Dim iR, iC, iSelectedIndex As Integer

        cmbPageLayout.Items.Clear()
        RemoveHandler cmbPageLayout.SelectedIndexChanged, AddressOf Me.cmbPageLayout_SelectedIndexChanged

        iR = r
        iC = c
        For i As Integer = 1 To 5
            iC += 1
            iR += 1
            sItem = CStr(iC) & " x " & CStr(iR)
            cmbPageLayout.Items.Add(sItem)
        Next

        iR = r
        iC = c
        While iR > 0 And iC > 0
            sItem = CStr(iC) & " x " & CStr(iR)
            cmbPageLayout.Items.Insert(0, sItem)
            iSelectedIndex += 1
            iC -= 1
            iR -= 1
        End While

        If iR > 0 Or iC > 0 Then
            cmbPageLayout.Items.Insert(0, "1 x 1")
            iSelectedIndex += 1
        End If

        cmbPageLayout.Items.Insert(0, My.Resources.frmPrintPreview_Auto)
        iSelectedIndex += 1

        If mbFixedNumberOfPages Then
            cmbPageLayout.SelectedIndex = iSelectedIndex - 1
        Else
            cmbPageLayout.SelectedIndex = 0
        End If

        AddHandler cmbPageLayout.SelectedIndexChanged, AddressOf Me.cmbPageLayout_SelectedIndexChanged

    End Sub

#End Region

#Region "btnClockwise_Click"
    ''' <summary>
    ''' Initiates a redraw with a 90 degree rotation.
    ''' </summary>
    Private Sub btnClockwise_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClockwise.Click

        If miImageRotation = 270 Then
            miImageRotation = 0
        Else
            miImageRotation += 90
        End If
        miNudgeTranslationX = 0
        miNudgeTranslationY = 0
        If mbFixedNumberOfPages Then
            mbScaleUpToFit = True
        End If
        ReDrawPreview()


    End Sub

#End Region
#Region "btnAnticlockwise_Click"
    ''' <summary>
    ''' Initiates a redraw with a 90 degree rotation.
    ''' </summary>
    Private Sub btnAnticlockwise_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAntiClockwise.Click

        If miImageRotation = 0 Then
            miImageRotation = 270
        ElseIf miImageRotation = 90 Then
            miImageRotation = 0
        Else
            miImageRotation -= 90
        End If
        miNudgeTranslationX = 0
        miNudgeTranslationY = 0
        If mbFixedNumberOfPages Then
            mbScaleUpToFit = True
        End If
        ReDrawPreview()

    End Sub

#End Region

#Region "btnLeft_Click"
    ''' <summary>
    ''' Initiates a redraw with a lateral nudge.
    ''' </summary>
    Private Sub btnLeft_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnLeft.Click
        Nudge(-1, 0)
    End Sub

#End Region
#Region "btnRight_Click"
    ''' <summary>
    ''' Initiates a redraw with a lateral nudge.
    ''' </summary>
    Private Sub btnRight_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRight.Click
        Nudge(1, 0)
    End Sub

#End Region
#Region "btnup_Click"
    ''' <summary>
    ''' Initiates a redraw with a vertical nudge.
    ''' </summary>
    Private Sub btnup_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnUp.Click
        Nudge(0, -1)
    End Sub

#End Region
#Region "btnDown_Click"
    ''' <summary>
    ''' Initiates a redraw with a vertical nudge.
    ''' </summary>
    Private Sub btnDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDown.Click
        Nudge(0, 1)
    End Sub

#End Region
#Region "Nudge"

    ''' <summary>
    ''' Redraws the preview with the image nudged left, right, up or down.
    ''' </summary>
    ''' <param name="iXNudges">Nudges left or right</param>
    ''' <param name="iYNudges">Nudges up or down</param>
    Private Sub Nudge(ByVal iXNudges As Integer, ByVal iYNudges As Integer)

        miNudgeTranslationX += CInt(iXNudges * miNudgeStep)
        miNudgeTranslationY += CInt(iYNudges * miNudgeStep)
        ReDrawPreview()

    End Sub

#End Region

#Region "btnScaleUp_Click"
    ''' <summary>
    ''' Initiates a redraw with an increase in scale.
    ''' </summary>
    Private Sub btnScaleUp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnScaleUp.Click

        msngImageScale = CSng(msngImageScale * (1 + csngScaleStep))
        ReDrawPreview()
    End Sub

#End Region
#Region "btnScaleDown_Click"
    ''' <summary>
    ''' Initiates a redraw with an decrease in scale.
    ''' </summary>
    Private Sub btnScaleDown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnScaleDown.Click

        msngImageScale = CSng(msngImageScale * (1 - csngScaleStep))
        ReDrawPreview()
    End Sub

#End Region
#Region "btnScaleUpToFit_Click"
    ''' <summary>
    ''' Initiates a redraw with the scale to be increased so that the diagram fills 
    ''' the page(s) currently on display.
    ''' </summary>
    Private Sub btnScaleUpToFit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnScaleUpToFit.Click

        mbScaleUpToFit = True
        miNudgeTranslationX = 0
        miNudgeTranslationY = 0
        ReDrawPreview()
    End Sub

#End Region

#Region "btnPrint_Click"

    Private Sub btnPrint_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPrint.Click

        PrintDialog1.PrinterSettings.FromPage = 1
        PrintDialog1.PrinterSettings.ToPage = CInt(miColumnCount * miRowCount)
        If PrintDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then
            SendToPrinter()
        End If

    End Sub

#End Region
#Region "btnSetup_Click"

    Private Sub btnSetup_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSetup.Click

        Dim dInch As Double = 2.54
        Dim iTitleBox, iTop, iBottom, ileft, iRight As Integer

        iTop = PageSetupDialog1.PageSettings.Margins.Top
        iBottom = PageSetupDialog1.PageSettings.Margins.Bottom
        ileft = PageSetupDialog1.PageSettings.Margins.Left
        iRight = PageSetupDialog1.PageSettings.Margins.Right

        'PageSetupDialog has a known bug, where it cannot convert margin 
        'units from the default imperial units (100ths of an inch) to the 
        'local units (mm). This is a work around to convert to whole mm.
        'NB This may need looking at again for internationalisation.
        PageSetupDialog1.PageSettings.Margins.Top = CInt(Math.Round(iTop * dInch / 10, 0) * 10)
        PageSetupDialog1.PageSettings.Margins.Bottom = CInt(Math.Round(iBottom * dInch / 10, 0) * 10)
        PageSetupDialog1.PageSettings.Margins.Left = CInt(Math.Round(ileft * dInch / 10, 0) * 10)
        PageSetupDialog1.PageSettings.Margins.Right = CInt(Math.Round(iRight * dInch / 10, 0) * 10)

        'Calculate the extra margin used for the title box
        iTitleBox = CInt(Me.HiddenBluePrismLogo.Image.Height / ciBluePrismLogoScale) + ciBluePrismLogoGap
        iTitleBox = CInt(Math.Round(iTitleBox * dInch / 10, 0) * 10)

        'Temporarily remove the extra title box margin space so that 
        'all margins appear equal in the dialogue box. The reason for 
        'this is PageSetupDialog swaps the margin values around as you
        'change from landscape to portrait, so that the larger bottom
        'margin moves to the left instead of staying put. The results 
        'are counter-intuitive to my mind and this problem will reoccur
        'when the user starts to change individual margins. Displaying
        'all margins as equal should minimise this problem.
        PageSetupDialog1.PageSettings.Margins.Bottom -= iTitleBox

        If PageSetupDialog1.ShowDialog() = System.Windows.Forms.DialogResult.OK Then

            'Re-add the title box margin
            PageSetupDialog1.PageSettings.Margins.Bottom += CInt(iTitleBox / dInch)
            ReDrawPreview()

        Else

            'The user did not OK the dialogue box, so change the 
            'margins back to their original values.
            PageSetupDialog1.PageSettings.Margins.Top = iTop
            PageSetupDialog1.PageSettings.Margins.Bottom = iBottom
            PageSetupDialog1.PageSettings.Margins.Left = ileft
            PageSetupDialog1.PageSettings.Margins.Right = iRight

        End If

    End Sub

#End Region

#Region "btnViewOnePage_Click"
    Private Sub btnViewOnePage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnViewOnePage.Click

        moPrintPreviewControl.Rows = 1
        moPrintPreviewControl.Columns = 1
        moPrintPreviewControl.StartPage = 0
        txtPageNumber.Text = "1"

    End Sub

#End Region
#Region "btnViewAllPages_Click"
    Private Sub btnViewAllPages_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnViewAllPages.Click

        moPrintPreviewControl.Rows = miRowCount
        moPrintPreviewControl.Columns = miColumnCount
        txtPageNumber.Text = ""

    End Sub

#End Region

#Region "btnFirstPage_Click"
    Private Sub btnFirstPage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnFirstPage.Click

        moPrintPreviewControl.Rows = 1
        moPrintPreviewControl.Columns = 1
        moPrintPreviewControl.StartPage = 0
        txtPageNumber.Text = "1"
    End Sub

#End Region
#Region "btnPreviousPage_Click"
    Private Sub btnPreviousPage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPreviousPage.Click

        If moPrintPreviewControl.StartPage > 0 Then
            moPrintPreviewControl.Rows = 1
            moPrintPreviewControl.Columns = 1
            moPrintPreviewControl.StartPage -= 1
            txtPageNumber.Text = CStr(moPrintPreviewControl.StartPage + 1)
        End If

    End Sub

#End Region
#Region "btnNextPage_Click"
    Private Sub btnNextPage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNextPage.Click

        If moPrintPreviewControl.StartPage + 1 < miColumnCount * miRowCount Then
            moPrintPreviewControl.Rows = 1
            moPrintPreviewControl.Columns = 1
            moPrintPreviewControl.StartPage += 1
            txtPageNumber.Text = CStr(moPrintPreviewControl.StartPage + 1)
        End If

    End Sub

#End Region
#Region "btnLastPage_Click"
    Private Sub btnLastPage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnLastPage.Click

        moPrintPreviewControl.Rows = 1
        moPrintPreviewControl.Columns = 1
        moPrintPreviewControl.StartPage = CInt(miColumnCount * miRowCount) - 1
        txtPageNumber.Text = CStr(moPrintPreviewControl.StartPage + 1)

    End Sub

#End Region

#Region "txtPageNumber_KeyPress"

    Private Sub txtPageNumber_KeyPress(ByVal sender As System.Object, ByVal e As System.Windows.Forms.KeyPressEventArgs) Handles txtPageNumber.KeyPress

        Dim iPageNumber As Integer

        If e.KeyChar = ChrW(13) Then

            If IsNumeric(txtPageNumber.Text) Then
                iPageNumber = CInt(txtPageNumber.Text)
                If iPageNumber >= 1 And iPageNumber <= miColumnCount * miRowCount Then
                    moPrintPreviewControl.Rows = 1
                    moPrintPreviewControl.Columns = 1
                    moPrintPreviewControl.StartPage = iPageNumber - 1
                End If
            End If

        End If

    End Sub

#End Region

#Region "btnHelp_Click"
    Private Sub btnHelp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnHelp.Click
        Try
            OpenHelpFile(Me, "frmPrintPreview.htm")
        Catch
            UserMessage.Err(CannotOpenOfflineHelp)
        End Try
    End Sub

#End Region

#Region "Mouse Events"
    ''' <summary>
    ''' Changes the cursor when the user drags the diagram.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub PrintPreviewControl_Mousemove(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)

        If Not moMouseStart.Equals(Point.Empty) Then

            Dim oMousePosition As Point
            Dim iTolerance As Integer

            iTolerance = 10
            oMousePosition = Control.MousePosition

            If Math.Abs(oMousePosition.X - moMouseStart.X) <= iTolerance Then
                If Math.Abs(oMousePosition.Y - moMouseStart.Y) <= iTolerance Then
                    moPrintPreviewControl.Cursor = Cursors.Default
                ElseIf oMousePosition.Y > moMouseStart.Y Then
                    moPrintPreviewControl.Cursor = Cursors.PanSouth
                Else
                    moPrintPreviewControl.Cursor = Cursors.PanNorth
                End If
            ElseIf oMousePosition.X > moMouseStart.X Then
                If Math.Abs(oMousePosition.Y - moMouseStart.Y) <= iTolerance Then
                    moPrintPreviewControl.Cursor = Cursors.PanEast
                ElseIf oMousePosition.Y > moMouseStart.Y Then
                    moPrintPreviewControl.Cursor = Cursors.PanSE
                Else
                    moPrintPreviewControl.Cursor = Cursors.PanNE
                End If
            Else
                If Math.Abs(oMousePosition.Y - moMouseStart.Y) <= iTolerance Then
                    moPrintPreviewControl.Cursor = Cursors.PanWest
                ElseIf oMousePosition.Y > moMouseStart.Y Then
                    moPrintPreviewControl.Cursor = Cursors.PanSW
                Else
                    moPrintPreviewControl.Cursor = Cursors.PanNW
                End If
            End If

        End If

    End Sub

    ''' <summary>
    ''' Makes a reference to the mouse position.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub PrintPreviewControl_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)

        If e.Button = System.Windows.Forms.MouseButtons.Left Then
            moMouseStart = Control.MousePosition
        Else
            moMouseStart = Point.Empty
        End If

    End Sub

    ''' <summary>
    ''' Initaites a redraw if the user has dragged more than 10 units.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Sub PrintPreviewControl_MouseUp(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs)

        moPrintPreviewControl.Cursor = Cursors.Hand

        Dim iMouseX, iMouseY As Integer
        Dim bReDraw As Boolean
        Dim iTolerance As Integer

        iTolerance = 10

        If moMouseStart <> Point.Empty Then

            iMouseX = CInt((Control.MousePosition.X - moMouseStart.X) / moPrintPreviewControl.Zoom)
            iMouseY = CInt((Control.MousePosition.Y - moMouseStart.Y) / moPrintPreviewControl.Zoom)

            If iMouseX < -iTolerance Or iMouseX > iTolerance Then
                miNudgeTranslationX += iMouseX
                bReDraw = True
            End If

            If iMouseY < -iTolerance Or iMouseY > iTolerance Then
                miNudgeTranslationY += iMouseY
                bReDraw = True
            End If

            If bReDraw Then
                ReDrawPreview()
            End If

            moMouseStart = Point.Empty
        End If

    End Sub

#End Region


End Class
