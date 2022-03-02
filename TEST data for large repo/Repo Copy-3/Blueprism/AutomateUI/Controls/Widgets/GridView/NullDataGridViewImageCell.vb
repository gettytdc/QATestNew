''' <summary>
''' A DataGridView column designed to show nothing instead of showing
''' a missing image icon
''' </summary>
Public Class NullDataGridViewImageCell
    Inherits DataGridViewImageCell
    Public Overrides Function AdjustCellBorderStyle(dataGridViewAdvancedBorderStyleInput As DataGridViewAdvancedBorderStyle,
                                                    dataGridViewAdvancedBorderStylePlaceholder As DataGridViewAdvancedBorderStyle,
                                                    singleVerticalBorderAdded As Boolean,
                                                    singleHorizontalBorderAdded As Boolean,
                                                    isFirstDisplayedColumn As Boolean,
                                                    isFirstDisplayedRow As Boolean) As DataGridViewAdvancedBorderStyle

        dataGridViewAdvancedBorderStylePlaceholder.Right = DataGridViewAdvancedCellBorderStyle.None
        dataGridViewAdvancedBorderStyleInput.Top = DataGridViewAdvancedCellBorderStyle.Single
        dataGridViewAdvancedBorderStylePlaceholder.Bottom = DataGridViewAdvancedCellBorderStyle.Single
        dataGridViewAdvancedBorderStyleInput.Left = DataGridViewAdvancedCellBorderStyle.Single

        Return dataGridViewAdvancedBorderStylePlaceholder
    End Function

    Public Overrides ReadOnly Property DefaultNewRowValue As Object
        Get
            Return Nothing
        End Get
    End Property
End Class
