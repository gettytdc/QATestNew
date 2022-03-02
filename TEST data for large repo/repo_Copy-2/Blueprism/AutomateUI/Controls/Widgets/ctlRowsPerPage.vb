Public Class ctlRowsPerPage


    ''' <summary>
    ''' Event raised any time the configuration is changed: eg
    ''' total number of rows available, the current page, the
    ''' rows per page, etc.
    ''' </summary>
    Public Event ConfigChanged()


    Public Event PageChanged()


    Protected Overrides Sub OnLoad(ByVal e As EventArgs)
        MyBase.OnLoad(e)
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlRowsPerPage))
        Dim items = New String() {resources.GetString("cmbRowsPerPage.Items"), resources.GetString("cmbRowsPerPage.Items1"), resources.GetString("cmbRowsPerPage.Items2"), resources.GetString("cmbRowsPerPage.Items3"), resources.GetString("cmbRowsPerPage.Items4"), resources.GetString("cmbRowsPerPage.Items5"), resources.GetString("cmbRowsPerPage.Items6"), resources.GetString("cmbRowsPerPage.Items7"), resources.GetString("cmbRowsPerPage.Items8"), resources.GetString("cmbRowsPerPage.Items9"), resources.GetString("cmbRowsPerPage.Items10"), resources.GetString("cmbRowsPerPage.Items11")}

        If miMaxRows = 0 OrElse miMaxRows = Nothing Then
            Me.cmbRowsPerPage.Items.AddRange(items)
        Else
            For Each element As String In items
                If CInt(element) <= miMaxRows Then
                    Me.cmbRowsPerPage.Items.Add(element)
                End If
            Next
        End If
    End Sub

    ''' <summary>
    ''' The current page, indexed from 1.
    ''' </summary>
    Public Property CurrentPage() As Integer
        Get
            Return miCurrentPage
        End Get
        Set(ByVal value As Integer)
            If value <> miCurrentPage Then
                If value >= 1 AndAlso value <= Me.mitotalpages Then
                    miCurrentPage = value
                    Me.OnPageChange()
                Else
                    Throw New ArgumentException(My.Resources.ctlRowsPerPage_ValueMustBeAWholeNumberBetween1AndTheTotalNumberOfPages)
                End If
            End If
        End Set
    End Property
    Private miCurrentPage As Integer = 1

    ''' <summary>
    ''' The total number of pages available.
    ''' </summary>
    Public ReadOnly Property TotalPages() As Integer
        Get
            Return mitotalpages
        End Get
    End Property
    Private mitotalpages As Integer

    ''' <summary>
    ''' The number of rows per page, as currently configured from the
    ''' user's point of view
    ''' </summary>
    Public Property RowsPerPage() As Integer
        Get
            Return miRowsPerPage
        End Get
        Set(ByVal value As Integer)
            If value <> miRowsPerPage Then
                If value > 0 Then
                    If value < Me.MaxRows Then
                        Me.SetRowsPerPage(value)
                    Else
                        Throw New ArgumentException(String.Format(My.Resources.ctlRowsPerPage_ValueForRowsPerPageExceedsTheMaximumAllowableValue0, Me.MaxRows))
                    End If
                Else
                    Throw New ArgumentException(My.Resources.ctlRowsPerPage_ValueForRowsPerPageMustBeAPositiveWholeNumber)
                End If
            End If
        End Set
    End Property
    Private miRowsPerPage As Integer

    ''' <summary>
    ''' The number of rows available to the user
    ''' </summary>
    Public Property TotalRows() As Integer
        Get
            Return miTotalRows
        End Get
        Set(ByVal value As Integer)
            If value <> miTotalRows Then
                If value >= 0 Then
                    miTotalRows = value
                    Me.llTotalRows.Text = String.Format(My.Resources.ctlRowsPerPage_Total0Rows, miTotalRows.ToString)
                    Me.UpdateTotalPages()
                Else
                    Throw New ArgumentException(My.Resources.ctlRowsPerPage_TheNumberOfRowsMustNotBeNegative)
                End If
            End If
        End Set
    End Property
    Private miTotalRows As Integer = -1

    ''' <summary>
    ''' The maximum number of rows per page to be made available to the user
    ''' </summary>
    Public Property MaxRows() As Integer
        Get
            Return miMaxRows
        End Get
        Set(ByVal value As Integer)
            miMaxRows = value
        End Set
    End Property
    Private miMaxRows As Integer

    ''' <summary>
    ''' Updates the appearance and function of the buttons
    ''' </summary>
    Private Sub UpdateButtons()
        btnPrevPage.Enabled = miCurrentPage > 1
        btnFirstPage.Enabled = btnPrevPage.Enabled
        btnNextPage.Enabled = miCurrentPage < mitotalpages
        btnLastPage.Enabled = btnNextPage.Enabled
    End Sub

    ''' <summary>
    ''' Updates the appearance and function of the textboxes
    ''' </summary>
    Private Sub UpdateTextBoxes()
        txtCurrentPage.Text = miCurrentPage.ToString
        txtTotalPages.Text = mitotalpages.ToString
    End Sub

    Private Sub btnPrevPage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnPrevPage.Click
        If miCurrentPage > 1 Then
            miCurrentPage -= 1
            UpdateButtons()
            UpdateTextBoxes()
            OnPageChange()
        End If
    End Sub

    Private Sub btnNextPage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNextPage.Click
        If miCurrentPage < mitotalpages Then
            miCurrentPage += 1
            UpdateButtons()
            UpdateTextBoxes()
            OnPageChange()
        End If
    End Sub

    Private Sub btnFirstPage_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnFirstPage.Click
        GotoFirstPage()
    End Sub

    ''' <summary>
    ''' Changes the current view to the first page
    ''' </summary>
    Private Sub GotoFirstPage()
        If miCurrentPage > 1 Then
            miCurrentPage = 1
            UpdateButtons()
            UpdateTextBoxes()
            OnPageChange()
        End If
    End Sub

    Private Sub btnLastPage_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btnLastPage.Click
        If miCurrentPage < mitotalpages Then
            miCurrentPage = mitotalpages
            UpdateButtons()
            UpdateTextBoxes()
            OnPageChange()
        End If
    End Sub

    ''' <summary>
    ''' Updates the number of pages, based on the current number of rows
    ''' per page, and the number of rows available.
    ''' </summary>
    Private Sub UpdateTotalPages()
        If miTotalRows = 0 Then
            mitotalpages = 1
        Else
            mitotalpages = CInt(Math.Ceiling(miTotalRows / miRowsPerPage))
        End If
        Me.UpdateTextBoxes()
        Me.UpdateButtons()
    End Sub

    ''' <summary>
    ''' Applies a new rows per page value
    ''' </summary>
    ''' <param name="Value">The new value</param>
    Private Sub SetRowsPerPage(ByVal Value As Integer)
        miRowsPerPage = Value
        GotoFirstPage()
        UpdateTotalPages()

        UpdateTextBoxes()
        Me.cmbRowsPerPage.Text = Value.ToString
        OnConfigChange()
    End Sub

    Private Sub DisableAll()
        Me.Enabled = False
    End Sub

    Private Sub ReEnable()
        Me.Enabled = True
        Me.UpdateButtons()
        Me.UpdateTextBoxes()
    End Sub

    Private Sub OnConfigChange()
        DisableAll()
        Try
            RaiseEvent ConfigChanged()
        Catch Ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlRowsPerPage_UnexpectedError0, Ex.Message), Ex)
        Finally
            Me.ReEnable()
        End Try
    End Sub

    Private Sub OnPageChange()
        DisableAll()
        Try
            RaiseEvent PageChanged()
        Catch Ex As Exception
            UserMessage.Show(String.Format(My.Resources.ctlRowsPerPage_UnexpectedError0, Ex.Message), Ex)
        Finally
            Me.ReEnable()
        End Try
    End Sub


    ''' <summary>
    ''' Updates the current number of rows per page, based on the value in
    ''' the user interface.
    ''' </summary>
    Private Sub ReAdjustRowsPerPage()
        Dim Result As Integer
        If Integer.TryParse(Me.cmbRowsPerPage.Text, Result) Then
            If Result > 0 Then
                If Result <= Me.MaxRows Then
                    SetRowsPerPage(Result)
                Else
                    UserMessage.ShowFloating(Me.cmbRowsPerPage, ToolTipIcon.Info, My.Resources.ctlRowsPerPage_InvalidValue, String.Format(My.Resources.ctlRowsPerPage_TheRequestedValueIsTooLargePleaseChooseAnotherValueNotExceeding0, Me.MaxRows.ToString()), Point.Empty, TooltipDuration, True)
                End If
            Else
                UserMessage.ShowFloating(Me.cmbRowsPerPage, ToolTipIcon.Info, My.Resources.ctlRowsPerPage_InvalidValue, My.Resources.ctlRowsPerPage_TheRequestedValueMustBeAPositiveNumber, Point.Empty, TooltipDuration, True)
            End If
        Else
            UserMessage.ShowFloating(Me.cmbRowsPerPage, ToolTipIcon.Info, My.Resources.ctlRowsPerPage_InvalidValue, My.Resources.ctlRowsPerPage_TheRequestedValueIsNotAWholeNumber, Point.Empty, TooltipDuration, True)
        End If
    End Sub

    ''' <summary>
    ''' The time, in milliseconds, during which tooltip messages are visible.
    ''' </summary>
    Const TooltipDuration As Integer = 3000

    Private Sub cmbRowsPerPage_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles cmbRowsPerPage.KeyDown
        If (e.KeyCode = Keys.Enter OrElse e.KeyCode = Keys.F3) Then
            Try
                mbSuspendSelectedIndexChange = True
                ReAdjustRowsPerPage()
                Me.cmbRowsPerPage.Select()
            Finally
                mbSuspendSelectedIndexChange = False
            End Try
        End If
    End Sub

    Private mbSuspendSelectedIndexChange As Boolean
    Private Sub cmbRowsPerPage_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmbRowsPerPage.SelectedIndexChanged
        If Not mbSuspendSelectedIndexChange Then
            ReAdjustRowsPerPage()
        End If
    End Sub


    Private Sub txtCurrentPage_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtCurrentPage.KeyDown
        If (e.KeyCode = Keys.Enter OrElse e.KeyCode = Keys.F3) Then
            Dim CorrectInput As Boolean

            Dim Value As Integer
            If Integer.TryParse(txtCurrentPage.Text, Value) Then
                If Value >= 1 AndAlso Value <= Me.TotalPages Then
                    CorrectInput = True
                    Me.miCurrentPage = Value
                    Me.OnPageChange()
                End If
            End If

            If Not CorrectInput Then
                UserMessage.ShowFloating(txtCurrentPage, ToolTipIcon.Info, My.Resources.ctlRowsPerPage_InvalidValue, My.Resources.ctlRowsPerPage_TheValueYouEnterMustBeAPositiveWholeNumberNotExceedingTheMaximumNumberOfPages, New Point(Me.txtCurrentPage.Width, 0), TooltipDuration, InBalloonStyle:=True)
            End If
        End If
    End Sub
End Class
