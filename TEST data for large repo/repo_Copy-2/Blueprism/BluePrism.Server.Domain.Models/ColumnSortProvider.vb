Imports BluePrism.Server.Domain.Models.Attributes

Public Class ColumnSortProvider(Of TItem)

    Private Shared ReadOnly WorkQueueSortByPropertyDictionary As IDictionary(Of TItem, (String, String)) =
                                GetSortByDictionary(Of TItem)()

    Public Shared Function ColumnNameAndDirection(sortBy As TItem) As (ColumnName As String, SortDirection As String)

        Return WorkQueueSortByPropertyDictionary(sortBy)

    End Function

    Private Shared Function GetSortByDictionary(Of TEnum)() As IDictionary(Of TEnum, (String, String))

        Dim enumType = GetType(TEnum)

        Return [Enum].GetValues(enumType).Cast(Of TEnum).
            ToDictionary(Of TEnum, (String, String))(
                Function(x) x,
                Function(x) enumType.
                            GetMember(x.ToString).
                            First().
                            GetCustomAttributes(False).
                            OfType(Of ColumnNameSortByAttribute).
                            Select(Function(y) (y.ColumnName, y.SortDirection)).Single())
    End Function

End Class
