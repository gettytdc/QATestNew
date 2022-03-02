

Imports BluePrism.AutomateProcessCore

Imports BluePrism.AutomateAppCore
Imports BluePrism.Images
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.BPCoreLib

''' Project  : Automate
''' Class    : ctlProcessSearch
''' 
''' <summary>
''' A control to enable process searching.
''' 
''' In order to use this class you must call
''' SetProcessStudioParent, and SetProcessViewingControl after
''' creating an instance.
''' </summary>
Friend Class DiagramSearchToolstrip : Inherits ToolStrip

#Region " Events "

    Public Event NewSearch(ByVal SearchText As String)

    Public Event SearchFeedBack(ByVal Message As String)

#End Region

#Region "Member Variables"

    Private WithEvents cmbSearch As ToolStripComboBox
    Private WithEvents btnGo As ToolStripButton
    Private WithEvents btnSearch As ToolStripButton
    Private WithEvents btnDependencies As ToolStripButton

    ''' <summary>
    ''' Holds a value indicating whether the mode is object studio
    ''' </summary>
    ''' <remarks></remarks>
    Private mModeIsObjectStudio As Boolean

    ''' <summary>
    ''' The process being searched.
    ''' </summary>
    Private mobjprocess As clsProcess

    ''' <summary>
    ''' The form on which the process being searched is being viewed.
    ''' </summary>
    Private mobjProcessViewingForm As IProcessViewingForm

    ''' <summary>
    ''' The process viewing control on which the process being searched is 
    ''' viewed. It is necessary to have a reference to this in addititon to
    ''' the owning form because the owning form may own several such viewing
    ''' controls.
    ''' </summary>
    Private mobjProcessViewer As ctlProcessViewer

    ''' <summary>
    ''' Last stage found.
    ''' </summary>
    Private objPrevFound As clsProcessStage
    Private iPrevMode As Integer

    ''' <summary>
    ''' If set then unhighlight stage in lostfocus code
    ''' </summary>
    Private blnFocusCode As Boolean = True


    ''' <summary>
    ''' Flag used in cbosearch_textchanged
    ''' </summary>
    Private blnSkipStsClear As Boolean

    ''' <summary>
    ''' Tooltip object for helping users with this control.
    ''' </summary>
    Private mTooltip As ToolTip

    ''' <summary>
    ''' The advanced search form launched when the advanced search button 
    ''' is clicked.
    ''' </summary>
    Private mobjAdvSearch As frmAdvSearch

    ''' <summary>
    ''' The searching tool used to search the process.
    ''' </summary>
    ''' <remarks>This should not be exposed outside the
    ''' class, but the advanced search needs it. Hack for now.</remarks>
    Friend mProcessSearcher As clsProcessSearcher

#End Region

#Region " Constructors "

    Public Sub New()
        MyBase.New()

        btnSearch = New ToolStripButton() With {
            .Text = My.Resources.FindText,
            .Size = New Size(110, 17)
        }
        cmbSearch = New ToolStripComboBox() With {
            .ToolTipText = My.Resources.Search,
            .Size = New Size(184, 24)
        }
        btnGo = New ToolStripButton() With {
            .Text = My.Resources.FindNext,
            .DisplayStyle = ToolStripItemDisplayStyle.Image,
            .Size = New Size(43, 21),
            .Image = ToolImages.Search_Next_16x16
        }
        btnDependencies = New ToolStripButton() With {
            .Text = My.Resources.Dependencies,
            .DisplayStyle = ToolStripItemDisplayStyle.ImageAndText,
            .Size = New Size(110, 17),
            .Image = ToolImages.Site_Map2_16x16
        }

        SuspendLayout()
        Items.AddRange(
            New ToolStripItem() {btnSearch, cmbSearch, btnGo, btnDependencies})
        Size = New Size(344, 24)
        ResumeLayout(False)

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets or sets the search text for this search toolstrip
    ''' </summary>
    <Browsable(True), Category("Appearance"), DefaultValue(""),
     Description("The text within the search box")>
    Public Property SearchText As String
        Get
            Return cmbSearch.Text
        End Get
        Set(value As String)
            cmbSearch.Text = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the process being searched.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property Process() As clsProcess
        Get
            Return mobjprocess
        End Get
    End Property

    ''' <summary>
    ''' The advanced search form launched by this control.
    ''' </summary>
    ''' <value>.</value>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property SearchForm() As frmAdvSearch
        Get
            Return mobjAdvSearch
        End Get
    End Property

    ''' <summary>
    ''' Gets or sets whether the search should be in object or process studio mode.
    ''' </summary>
    Public Property ModeIsObjectStudio() As Boolean
        Get
            Return mModeIsObjectStudio
        End Get
        Set(ByVal value As Boolean)
            mModeIsObjectStudio = value
        End Set
    End Property

    ''' <summary>
    ''' Gets the search target for this search, either "object" or "process"
    ''' depending on whether the <see cref="ModeIsObjectStudio">mode is object studio
    ''' </see> or not.
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Private ReadOnly Property SearchTarget As String
        Get
            Return If(ModeIsObjectStudio, My.Resources.frmProcessComparison_Object, My.Resources.frmProcessComparison_Process)
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this control has text focus - ie. has the focus in a text
    ''' control operated within this control
    ''' </summary>
    <Browsable(False),
     DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)>
    Public ReadOnly Property HasTextFocus As Boolean
        Get
            Return cmbSearch.Focused
        End Get
    End Property

#End Region

#Region " Event Handlers "

    Private Sub cboSearch_KeyPress(ByVal sender As Object, ByVal e As KeyPressEventArgs) Handles cmbSearch.KeyPress
        'Suppress beep
        If e.KeyChar = vbCr Then
            e.Handled = True
        End If
    End Sub

    Private Sub cboSearch_KeyDown(ByVal sender As Object, ByVal e As KeyEventArgs) Handles cmbSearch.KeyDown
        'if the enterkey is pressed then do search
        mobjProcessViewingForm.SetStatusBarText("")
        If (e.KeyCode = Keys.Enter Or e.KeyCode = Keys.F3) And Len(cmbSearch.Text) > 0 Then
            DoSearch()
            e.Handled = True
        ElseIf e.KeyCode = Keys.Escape Then
            'if the escape key is pressed go back to pre search stage
            If Not objPrevFound Is Nothing Then
                objPrevFound.DisplayMode = CType(iPrevMode, StageShowMode)
                Me.mobjProcessViewer.ShowStage(objPrevFound)
            End If
        End If
    End Sub

    Private Sub ctlProcessSearch_LostFocus(ByVal sender As Object, ByVal e As EventArgs) Handles MyBase.LostFocus
        'reset last found object to previous colour
        If Not objPrevFound Is Nothing And blnFocusCode = True Then
            objPrevFound.DisplayMode = CType(iPrevMode, StageShowMode)
            objPrevFound = Nothing
            mobjProcessViewingForm.SetStatusBarText("")
        End If
    End Sub

    Private Sub cboSearch_GotFocus(ByVal sender As Object, ByVal e As EventArgs) Handles cmbSearch.GotFocus
        'reset last found object to previous colour
        'mobjProcessViewingForm.KeyPreview = False
        'mobjProcessViewer.Focus()
        If Not objPrevFound Is Nothing And blnFocusCode = True Then
            objPrevFound.DisplayMode = CType(iPrevMode, StageShowMode)
            objPrevFound = Nothing
        End If

    End Sub

    Private Sub cboSearch_LostFocus(ByVal sender As Object, ByVal e As EventArgs) Handles cmbSearch.LostFocus
        'reset last found object to previous colour
        If Not objPrevFound Is Nothing And blnFocusCode = True Then
            objPrevFound.DisplayMode = CType(iPrevMode, StageShowMode)
            objPrevFound = Nothing
        End If
    End Sub

    Private Sub btnDependencies_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnDependencies.Click
        StartAdvanced(True)
    End Sub

    Private Sub btnAdvanced_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnSearch.Click
        StartAdvanced()
    End Sub

    Private Sub pbGo_Click(ByVal sender As Object, ByVal e As EventArgs) Handles btnGo.Click
        If Len(cmbSearch.Text) > 0 Then
            DoSearch()
        End If
    End Sub

#End Region

#Region " Other Methods "

    Private Sub DoSearch()
        blnFocusCode = False
        Try
            RaiseEvent NewSearch(cmbSearch.Text)
            Find()
            cmbSearch.Focus()
        Finally
            blnFocusCode = True
        End Try
    End Sub

    ''' <summary>
    ''' Sets mobjProcessViewingForm to be the Form that started this form.
    ''' </summary>
    ''' <param name="objParent"></param>
    Public Sub SetProcessStudioParent(ByVal objParent As IProcessViewingForm)
        'this sets mobjProcessViewingForm to be the Form that started this form
        mobjProcessViewingForm = objParent
    End Sub

    ''' <summary>
    ''' Sets the process viewer to be the viewer to which this search is attached.
    ''' </summary>
    ''' <param name="objProcessViewer">The process viewer used. Must not be
    ''' null reference.</param>
    Public Sub SetProcessViewingControl(ByVal objProcessViewer As ctlProcessViewer)
        Me.mobjProcessViewer = objProcessViewer
        mobjprocess = mobjProcessViewer.Process
        Me.mProcessSearcher = New clsProcessSearcher(mobjprocess)
    End Sub

    Public Sub SelectDependency(dep As clsProcessDependency)
        StartAdvanced(True)
        Me.mobjAdvSearch.SelectDependency(dep)
    End Sub

#End Region

#Region "Methods"

    Public Sub Find()
        Dim params As clsProcessSearcher.SearchParameters
        If Me.mobjAdvSearch IsNot Nothing Then
            params = mobjAdvSearch.GetSearchParameters()
        Else
            Dim isNewSearch As Boolean = (mProcessSearcher.LastSearchString <> SearchText)
            params = New clsProcessSearcher.SearchParameters(
                SearchText, True, isNewSearch, clsProcessSearcher.SearchParameters.SearchTypes.Normal, False)
        End If
        Find(params)
    End Sub

    ''' <summary>
    ''' Searches the process using the supplied parameters.
    ''' </summary>
    ''' <param name="Params">The search parameters to be used.</param>
    ''' <remarks>
    ''' If the search text is not found then an error message is shown in the status bar.
    ''' If text is found the found stage is focused, selected, and highlighted.
    ''' </remarks>
    Public Sub Find(ByVal Params As clsProcessSearcher.SearchParameters)

        If Not cmbSearch.Items.Contains(Params.SearchText) Then
            Me.cmbSearch.Items.Add(Params.SearchText)
        End If
        Me.cmbSearch.Text = Params.SearchText
        Me.mProcessSearcher.LastSearchString = Params.SearchText

        Dim sErr As String = Nothing
        Dim SearchSuccess As Boolean
        Dim ResultStage As clsProcessStage = Nothing

        If Params.SearchElements Then
            Dim Result As clsProcessSearcher.SearchResult = Nothing
            If Not mProcessSearcher.FindElement(Params, Result, sErr) Then
                UserMessage.Show(String.Format(My.Resources.InternalError0, sErr))
                Exit Sub
            Else
                If (Result IsNot Nothing) Then
                    SearchSuccess = True
                    ResultStage = Result.Stage
                End If
            End If
        Else
            If Params.SearchType = clsProcessSearcher.SearchParameters.SearchTypes.Regex Then
                Select Case Params.SearchText.Mid(1, 1)
                    Case "?", "+", "*", "{"
                        UserMessage.Show(My.Resources.InvalidSearchYouCanNotUseOrAtTheStartOfARegularExpressionSearch)
                        Exit Sub
                End Select
            End If

            SearchSuccess = mProcessSearcher.StageSearch(Params, ResultStage)
        End If


        If SearchSuccess Then
            ShowResult(ResultStage)
        Else
            sErr = String.Format(My.Resources.x0HasFinishedSearchingThe12,
                ApplicationProperties.ApplicationName,
                SearchTarget.ToLower(),
                IIf(Params.NewSearch, My.Resources.TheSearchItemWasNotFound, ""))

            blnSkipStsClear = True
            GiveUserFeedBack(sErr)
            blnSkipStsClear = False

            'No (further) match was found - clear the search history
            Me.mProcessSearcher.LastSearchString = String.Empty
            If Not objPrevFound Is Nothing Then
                objPrevFound.DisplayMode = CType(iPrevMode, StageShowMode)
                objPrevFound = Nothing
            End If
        End If

        AddToCombo(cmbSearch.Text)
        mobjProcessViewer.InvalidateView()
    End Sub

    ''' <summary>
    ''' Highlights the supplied stage in the process
    ''' diagram.
    ''' </summary>
    ''' <param name="objFound">The stage to be highlighted.
    ''' Must not be null.</param>
    Public Sub ShowResult(ByVal objFound As clsProcessStage)
        Me.ClearLastFound()

        iPrevMode = objFound.DisplayMode
        objFound.DisplayMode = StageShowMode.Search_Highlight
        Me.mobjProcessViewer.ShowStage(objFound)

        objPrevFound = objFound
    End Sub

    ''' <summary>
    ''' Removes any highlighting from the last found
    ''' stage, if such exists.
    ''' </summary>
    Public Sub ClearLastFound()
        If Not objPrevFound Is Nothing Then
            objPrevFound.DisplayMode = CType(iPrevMode, StageShowMode)
            objPrevFound = Nothing
        End If
    End Sub

    ''' <summary>
    ''' Shows a message to the user.
    ''' </summary>
    ''' <param name="Message">The message to show</param>
    ''' <remarks></remarks>
    Private Sub GiveUserFeedBack(ByVal Message As String)
        mobjProcessViewingForm.SetStatusBarText(Message)
        UserMessage.OK(Message)
        RaiseEvent SearchFeedBack(Message)
    End Sub

    ''' <summary>
    ''' Creates an occurance of the advanced search form (frmAdvSearch) and sets it to
    ''' be a child form.
    ''' Copies details in this controls combobox to the combobox on the adv search form.
    ''' </summary>
    Private Sub StartAdvSearch()
        Dim index As Integer
        mobjAdvSearch = New frmAdvSearch(Me.mobjProcessViewingForm, Me)
        CType(mobjProcessViewingForm, Form).AddOwnedForm(mobjAdvSearch)
        mobjAdvSearch.cboAdvSearch.Items.Clear()
        For index = 0 To cmbSearch.Items.Count - 1
            mobjAdvSearch.cboAdvSearch.Items.Add(cmbSearch.Items(index))
        Next
        mobjAdvSearch.Show()
    End Sub

    ''' Adds the last search phrase to cboAdvSearch if it is not already there.
    ''' If the search phrase is already in cboSearch then it will move it to be 
    ''' first in the index.
    ''' If there are 20 items it will remove the oldest item before adding a new item.
    ''' It populates the search combobox of the child advanced search form/control 
    ''' with the same details if it exists.
    ''' <param name="sString">This is the string to be added</param>
    Friend Sub AddToCombo(ByVal sString As String)
        Dim index As Integer
        If cmbSearch.FindStringExact(sString) <> -1 Then          'found
            cmbSearch.Items.Remove(sString)
        End If
        If cmbSearch.Items.Count = 20 Then
            cmbSearch.Items.RemoveAt(19)
        End If
        cmbSearch.Items.Insert(0, sString)
        cmbSearch.Text = sString
        If Not mobjAdvSearch Is Nothing Then
            mobjAdvSearch.cboAdvSearch.Items.Clear()
            For index = 0 To cmbSearch.Items.Count - 1
                mobjAdvSearch.cboAdvSearch.Items.Add(cmbSearch.Items(index))
            Next
        End If
    End Sub


    ''' <summary>
    ''' Creates an occurance of the advanced search form (frmAdvSearch) and sets it to
    ''' be a child form.
    ''' Copies details in this controls combobox to the combobox on the adv search form.
    ''' </summary>
    Public Sub StartAdvanced(Optional showDependencyViewer As Boolean = False)
        'if adv search has been click before then just make adv search form visible and focused
        Dim index As Integer
        If mobjAdvSearch Is Nothing Then
            StartAdvSearch()
        ElseIf mobjAdvSearch.Visible = False Then
            mobjAdvSearch.Enabled = True
            mobjAdvSearch.Visible = True
        End If
        'fill advanced search combobox with data from ctl combobox
        mobjAdvSearch.cboAdvSearch.Items.Clear()
        For index = 0 To cmbSearch.Items.Count - 1
            mobjAdvSearch.cboAdvSearch.Items.Add(cmbSearch.Items(index))
        Next
        mobjAdvSearch.cboAdvSearch.Text = cmbSearch.Text
        If showDependencyViewer Then
            mobjAdvSearch.ShowDependencyTab()
            mobjAdvSearch.tvReferences.Focus()
        Else
            mobjAdvSearch.ShowFindTab()
            mobjAdvSearch.cboAdvSearch.Focus()
        End If
    End Sub

    Private Sub InitializeComponent()
        Me.SuspendLayout()
        Me.ResumeLayout(False)

    End Sub

#End Region


End Class
