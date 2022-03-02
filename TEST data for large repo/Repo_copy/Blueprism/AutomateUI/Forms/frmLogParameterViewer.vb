Imports System.Xml
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Core.Xml
Imports LocaleTools

''' Project  : Automate
''' Class    : AutomateUI.frmLogParameterViewer
''' 
''' <summary>
''' Used from frmLogViewer to view parameters. An instance of this form is owned
''' by the parameter viewer.
''' </summary>
Friend Class frmLogParameterViewer

    ''' <summary>
    ''' True if the parameters being viewed contain one or more collections. Used to
    ''' determine certain behaviours, such as the presence of additional columns.
    ''' </summary>
    Private mCollectionsInParameters As Boolean

    ''' <summary>
    ''' A list of searches to be performed on this form's content.
    ''' </summary>
    Private mSearches As Dictionary(Of String, frmLogViewer.SearchItem)

    ''' <summary>
    ''' Constructor. Once the form has been constructed, use SetData to populate it.
    ''' </summary>
    Friend Sub New()
        MyBase.New()
        InitializeComponent()
    End Sub

    ''' <summary>
    ''' Sets up the data grid once the data has been attached.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub DataGridView_DataBindingComplete(ByVal sender As Object, ByVal e As DataGridViewBindingCompleteEventArgs) Handles DataGridView.DataBindingComplete

        DataGridView.Columns("Direction").Width = 50
        DataGridView.Columns("Type").Width = 50
        If mCollectionsInParameters Then
            DataGridView.Columns("Row").Width = 20
            DataGridView.Columns("Row").HeaderCell.Value = My.Resources.frmLogParameterViewer_Row
            DataGridView.Columns("Field").HeaderCell.Value = My.Resources.DataGridView_DataBindingComplete_Field
        End If

        DataGridView.Columns("Direction").HeaderCell.Value = My.Resources.frmLogParameterViewer_Direction
        DataGridView.Columns("Name").HeaderCell.Value = My.Resources.frmLogParameterViewer_Name
        DataGridView.Columns("Type").HeaderCell.Value = My.Resources.frmLogParameterViewer_Type
        DataGridView.Columns("Value").HeaderCell.Value = My.Resources.frmLogParameterViewer_Value

        DataGridView.Columns("Direction").FillWeight = 2
        DataGridView.Columns("Name").FillWeight = 3
        If mCollectionsInParameters Then
            DataGridView.Columns("Field").FillWeight = 3
            DataGridView.Columns("Row").FillWeight = 1
        End If
        DataGridView.Columns("Type").FillWeight = 2
        DataGridView.Columns("Value").FillWeight = 3

        AddHandler DataGridView.CellFormatting, AddressOf DataGridView_CellFormatting
        RemoveHandler DataGridView.DataBindingComplete, AddressOf DataGridView_DataBindingComplete

    End Sub

    ''' <summary>
    ''' Call once the form has been constructed, either after the first time to set
    ''' it up initially, or later to load a new set of data.
    ''' </summary>
    ''' <param name="sParameterXML">The parameters to be displayed, in XML form as
    ''' stored in the session table of the database.</param>
    ''' <param name="Searches">A dictionary of search objects, indicating search
    ''' terms to be found.</param>
    ''' <param name="colHighLight">The highlight colour</param>
    Public Sub SetData(ByVal sParameterXML As String, ByVal Searches As Dictionary(Of String, frmLogViewer.SearchItem), ByVal colHighLight As Color, Optional ByVal objectName As String = "")

        mSearches = Searches
        Me.Cursor = Cursors.WaitCursor
        mCollectionsInParameters = False

        Try
            Dim xmlDoc As New ReadableXmlDocument(sParameterXML)

            Dim dtParameters As New DataTable

            Dim drParameter As DataRow
            dtParameters.Columns.Add("Direction", GetType(String))
            dtParameters.Columns.Add("Name", GetType(String))
            dtParameters.Columns.Add("Field", GetType(String))
            dtParameters.Columns.Add("Row", GetType(Integer))
            dtParameters.Columns.Add("Type", GetType(String))
            dtParameters.Columns.Add("Value", GetType(String))

            Dim sDirection As String

            For Each parameters As XmlElement In xmlDoc.ChildNodes
                For Each inputs As XmlElement In parameters.ChildNodes

                    If inputs.ChildNodes.Count > 0 Then

                        For Each input As XmlElement In inputs.ChildNodes

                            If input.Name = "input" Then
                                sDirection = My.Resources.frmLogParameterViewer_IN
                            Else
                                sDirection = My.Resources.frmLogParameterViewer_OUT
                            End If

                            Dim ParamDataType As DataType
                            Try
                                ParamDataType = clsProcessDataTypes.DataTypeId(input.GetAttribute("type"))
                            Catch ex As Exception
                                ParamDataType = DataType.unknown
                            End Try

                            drParameter = dtParameters.NewRow
                            dtParameters.Rows.Add(drParameter)
                            drParameter("Direction") = sDirection
                            drParameter("Name") = clsBusinessObjectAction.GetLocalizedFriendlyName(input.GetAttribute("name"), objectName, "Params")
                            drParameter("Type") = clsProcessDataTypes.GetFriendlyName(ParamDataType)


                            If ParamDataType <> DataType.collection Then
                                drParameter("Value") = input.GetAttribute("value")
                            Else
                                mCollectionsInParameters = True

                                drParameter("Value") = LTools.Format(My.Resources.frmLogParameterViewer_plural_rows, "COUNT", input.ChildNodes.Count)

                                If input.ChildNodes.Count > 0 Then
                                    For r As Integer = 0 To input.ChildNodes.Count - 1
                                        For c As Integer = 0 To input.ChildNodes(0).ChildNodes.Count - 1
                                            Dim field As XmlElement = Nothing
                                            If input.ChildNodes(r) IsNot Nothing Then
                                                field = CType(input.ChildNodes(r).ChildNodes(c), XmlElement)
                                            End If
                                            If field IsNot Nothing Then
                                                drParameter = dtParameters.NewRow
                                                dtParameters.Rows.Add(drParameter)
                                                drParameter("Direction") = sDirection
                                                drParameter("Name") = clsBusinessObjectAction.GetLocalizedFriendlyName(input.GetAttribute("name"), objectName, "Params")
                                                drParameter("Field") = field.GetAttribute("name")
                                                drParameter("Row") = r + 1
                                                drParameter("Type") = clsProcessDataTypes.GetFriendlyName(field.GetAttribute("type"))
                                                drParameter("Value") = field.GetAttribute("value")
                                            End If
                                        Next
                                    Next
                                End If

                            End If
                        Next
                    End If
                Next
            Next

            If Not mCollectionsInParameters Then
                dtParameters.Columns.Remove("Row")
                dtParameters.Columns.Remove("Field")
            End If

            DataGridView.Columns.Clear()
            DataGridView.DataSource = dtParameters

        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.AnErrorHasOccurredAndTheParameterDetailsCannotBeDisplayed0, ex.Message))
            Me.Close()
        Finally
            Cursor = Cursors.Default
        End Try

    End Sub

    ''' <summary>
    ''' Formats the cells displayed on screen.
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub DataGridView_CellFormatting(ByVal sender As Object, ByVal e As DataGridViewCellFormattingEventArgs)

        'Colour the 'find all' cells.
        If (e.Value IsNot Nothing) AndAlso (Not (TypeOf e.Value Is DBNull)) Then
            For Each S As frmLogViewer.SearchItem In Me.mSearches.Values
                If S.SearchPattern.IsMatch(CStr(e.Value)) Then
                    e.CellStyle.BackColor = S.HighlightColour
                End If
            Next

        End If

        Dim col = DataGridView.Columns(e.ColumnIndex)
        Dim row = DataGridView.Rows(e.RowIndex)
        Dim cell = row.Cells(e.ColumnIndex)

        If col.Name = "Value" Then
            Dim typeCell As Object = row.Cells("Type").Value
            If Not IsDBNull(typeCell) Then
                Try
                    'Parse dates and times
                    Dim dtype = clsProcessDataTypes.DataTypeId(typeCell.ToString.ToLower)
                    If dtype = DataType.password Then Return
                    Dim val = clsProcessValue.Decode(dtype, CStr(e.Value))

                    Select Case dtype
                        Case DataType.date : e.Value = CDate(val).ToShortDateString()
                        Case DataType.time : e.Value = CDate(val).ToShortTimeString()
                        Case DataType.number : e.Value = CDec(val).ToString()
                        Case DataType.datetime
                            Dim dateValue = New DateTimeOffset(CDate(val))
                            frmLogViewer.SetDateTimeTooltipText(cell, dateValue)
                            e.Value = dateValue.DateTime
                    End Select
                Catch
                    'If any of this fails just display the value as it appears in the database
                End Try
            End If
        End If

    End Sub

End Class
