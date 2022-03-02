Imports BluePrism.AutomateProcessCore

Friend Class ctlApplicationElementEditor
    Inherits ctlListView

    ' The application member being edited - not updated by this class.
    Private mElement As clsApplicationMember

    Public Sub New()
        MyBase.New()
    End Sub

    ''' <summary>
    ''' Raised when an attribute is edited.
    ''' </summary>
    ''' <remarks></remarks>
    Public Event AttributeChanged()

    ''' <summary>
    ''' Populates the list with the details of the supplied 
    ''' clsApplicationElementAttribute
    ''' </summary>
    ''' <param name="el">The element to be represented.
    ''' Must not be null.</param>
    ''' <remarks>A reference to the attribute will be held and the 
    ''' attribute will be updated live, as the user edits
    ''' the values on screen.
    ''' 
    ''' If this is not what you want then you should clone
    ''' it first.</remarks>
    Public Sub Populate(ByVal el As clsApplicationMember)

        Me.PrepareColumns()
        If el Is mElement Then
            For Each row As clsApplicationAttributeListRow In Me.Rows
                row.SetDisplayValues()
            Next
        Else
            Dim elem As clsApplicationElement = TryCast(el, clsApplicationElement)
            Dim gp As clsApplicationElementGroup = TryCast(el, clsApplicationElementGroup)

            If elem IsNot Nothing Then
                Dim attrRows As New List(Of clsListRow)
                For Each attr As clsApplicationAttribute In elem.Attributes
                    attrRows.Add(CreateAttributeRow(attr))
                Next
                Me.Enabled = True

                Me.Rows.Clear()
                Me.Rows.AddRange(attrRows)

                ' Re-enact the last scroll so we are consistent with how it looked previously.
                Me.Sorter.PerformSort()

            ElseIf gp IsNot Nothing Then
                Me.Rows.Clear()
                Me.Enabled = False

            End If

            If el IsNot mElement Then
                ClearScrollPosition()
                mElement = el
            End If
        End If
        UpdateView()
    End Sub

    ''' <summary>
    ''' Sets the scrollbar(s) to the top/left position.
    ''' </summary>
    Private Sub ClearScrollPosition()
        pnlScrollPanel.AutoScrollPosition = Point.Empty
    End Sub

    ''' <summary>
    ''' Restores the panels position window position
    ''' so that it matches its current scroll position
    ''' </summary>
    Public Sub RestoreScroll()
        pnlScrollPanel.RestoreScroll()
    End Sub

    ''' <summary>
    ''' Creates a listrow, representing the supplied
    ''' attribute.
    ''' </summary>
    ''' <param name="Attribute">The attribute which is to
    ''' be represented. Must not be null.</param>
    Private Function CreateAttributeRow(ByVal attribute As clsApplicationAttribute) As clsApplicationAttributeListRow
        Dim row As New clsApplicationAttributeListRow(Me)
        row.ApplicationAttribute = attribute
        AddHandler row.AttributeChanged, AddressOf HandleAttributeChanged
        Return row
    End Function



    Private Sub HandleAttributeChanged()
        RaiseEvent AttributeChanged()
    End Sub

    ''' <summary>
    ''' Indicates whether the listview columns have
    ''' already been prepared.
    ''' </summary>
    Private mbColumnsPrepared As Boolean
    ''' <summary>
    ''' Prepares the listview columns.
    ''' </summary>
    Private Sub PrepareColumns()
        If Not mbColumnsPrepared Then
            MinimumColumnWidth = 50

            With Me.Columns
                .Add(My.Resources.ctlApplicationElementEditor_Name, My.Resources.ctlApplicationElementEditor_TheNameOfTheAttributeReferToTheHelpForADescriptionOfEachOne)
                .Add(My.Resources.ctlApplicationElementEditor_Match, My.Resources.ctlApplicationElementEditor_DeterminesIfMatchIsRequiredYouShouldChooseAMatchAgainstEnoughAttributesInOrderT)
                .Add(My.Resources.ctlApplicationElementEditor_MatchType, My.Resources.ctlApplicationElementEditor_DeterminesWhichTypeOfExpressionToUseWithinTheMatch)
                .Add(My.Resources.ctlApplicationElementEditor_Value, My.Resources.ctlApplicationElementEditor_TheValueWhichIsToBeMatchedExactlyNotRelevantWhenTheAttributeIsDynamic)

                .Item(0).Width = 150
                .Item(1).Width = 50
                .Item(2).Width = 100
            End With
            Me.LastColumnAutoSize = True
            Me.Sortable = True
            With Me.Sorter

                .ColumnDataTypes = New DataType() {
                 DataType.text, DataType.flag, DataType.text, DataType.text}

                .ColumnDefaultSortOrder = New SortOrder() {
                 SortOrder.None, SortOrder.Descending, SortOrder.Descending, SortOrder.None}

                ' Default to ordering on ascending name
                .SortColumn = 0
                .Order = SortOrder.Ascending

                .PerformSort()
            End With
        End If

        mbColumnsPrepared = True
    End Sub

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlApplicationElementEditor))
        Me.pnlScrollPanel.SuspendLayout()
        CType(Me.Canvas, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'pnlScrollPanel
        '
        Me.pnlScrollPanel.DockPadding.All = 0
        Me.pnlScrollPanel.DockPadding.Bottom = 0
        Me.pnlScrollPanel.DockPadding.Left = 0
        Me.pnlScrollPanel.DockPadding.Right = 0
        Me.pnlScrollPanel.DockPadding.Top = 0
        resources.ApplyResources(Me.pnlScrollPanel, "pnlScrollPanel")
        Me.pnlScrollPanel.Controls.SetChildIndex(Me.Canvas, 0)
        '
        'ctlApplicationElementEditor
        '
        Me.Name = "ctlApplicationElementEditor"
        Me.Rows.Capacity = 0
        Me.pnlScrollPanel.ResumeLayout(False)
        CType(Me.Canvas, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)

    End Sub
End Class
