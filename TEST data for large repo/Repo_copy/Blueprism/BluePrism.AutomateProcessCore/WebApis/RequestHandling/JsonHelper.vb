Imports System.Linq
Imports Newtonsoft.Json.Linq

Namespace WebApis
    ''' <summary>
    ''' Provides assistance parsing JSON to objects.
    ''' </summary>
    Public Class JsonHelper

        Private Const SingleValueColumnName = "Value"

        ''' <summary>
        ''' Deserialize the element at the given path from the provided JSON Token
        ''' into the specified type. 
        ''' </summary>
        ''' <typeparam name="T">The type to deserialize into</typeparam>
        ''' <param name="json">The JSON Token to extract the object from</param>
        ''' <param name="path">The JSON path dictating the route by 
        ''' which the element should be found.</param>
        ''' <returns>An object of type T constructed from the JSON</returns>
        Public Shared Function Deserialize(Of T)(json As JToken, path As String) As T
            Dim serializedItem = json.SelectToken(path)
            Return If(serializedItem Is Nothing, Nothing, serializedItem.ToObject(Of T))
        End Function

        Public Shared Function CanDeserializeNonNullableType(json As JToken, path As String) As Boolean
            Return json.SelectToken(path) IsNot Nothing
        End Function

        ''' <summary>
        ''' Deserialize the element at the given path from the provided JSON Token
        ''' into a Data Table representing the collection.
        ''' </summary>
        ''' <param name="json">The JSON Token to extract the object from</param>
        ''' <param name="path">The JSON path dictating the route by 
        ''' which the element should be found.</param>
        ''' <returns>A Data Table constructed from the JSON</returns>
        Public Shared Function DeserializeCollection(json As JToken, path As String) As DataTable
            Dim serializedItem = json.SelectToken(path)
            If serializedItem Is Nothing Then Return Nothing
            Return DirectCast(DeserializeGeneric(serializedItem, True), DataTable)
        End Function

        Public Shared Function DeserializeGeneric(ByVal obj As Object, ByVal populate As Boolean) As Object
            Dim jsonArray As JArray = TryCast(obj, JArray)
            If jsonArray IsNot Nothing Then
                Return DeserializeArray(jsonArray, populate)
            End If

            Dim jsonObject As JObject = TryCast(obj, JObject)
            If jsonObject IsNot Nothing Then
                Return DeserializeObject(jsonObject, populate)
            End If

            Return TryCast(obj, JValue)?.Value
        End Function

        Private Shared Function DeserializeObject(ByVal obj As JObject, ByVal populate As Boolean) As DataTable
            Dim dataTable As New DataTable()

            dataTable.Columns.AddRange(obj.Properties.Select(Function(prop)
                                                                 Return New DataColumn(prop.Name, GetJPropertyValueType(prop))
                                                             End Function).ToArray())
            If populate Then
                Dim newRow As DataRow = dataTable.NewRow()
                For Each prop In obj.Properties
                    newRow(prop.Name) = DeserializeGeneric(prop.Value, True)
                Next
                dataTable.Rows.Add(newRow)
            End If

            Return dataTable
        End Function

        Public Shared Function DeserializeArray(ByVal jsonArray As JArray, ByVal populate As Boolean) As DataTable

            If jsonArray.Count < 1 Then Return New DataTable()
            Dim typeOfFirstItem = GetTypeOf(DeserializeGeneric(jsonArray.First(), False))

            'Check type of all items and throw error if not all matching
            For Each el In jsonArray.Skip(1)
                If (GetTypeOf(DeserializeGeneric(el, False)) <> typeOfFirstItem) Then _
                    Throw New InvalidOperationException(String.Format(WebApiResources.DataMismatchTemplate, el, typeOfFirstItem))
            Next

            Dim firstArrayElement = jsonArray.First()
            Dim fields As Dictionary(Of String, Type)

            If TryCast(firstArrayElement, JObject) IsNot Nothing Then
                fields = GetFields(jsonArray)
            Else
                fields = New Dictionary(Of String, Type)()
                fields.Add(SingleValueColumnName, typeOfFirstItem)
            End If

            'Build data table from fields
            Dim dataTable As New DataTable()
            dataTable.Columns.AddRange(fields.
                                          Select(Function(field) New DataColumn(field.Key, field.Value)).
                                          ToArray())
            If populate Then PopulateDataTable(dataTable, jsonArray)

            Return dataTable

        End Function

        Private Shared Function GetFields(ByVal jsonArray As JArray) As Dictionary(Of String, Type)
            Dim fields = New Dictionary(Of String, Type)

            For Each row As JObject In jsonArray
                For Each field In row.Properties
                    If fields.ContainsKey(field.Name) Then
                        Dim expectedType = fields.Item(field.Name)
                        If GetJPropertyValueType(field) <> expectedType Then
                            Throw New InvalidOperationException(String.Format(WebApiResources.DataMismatchTemplate, field.Value, expectedType))
                        End If
                    Else
                        fields.Add(field.Name, GetJPropertyValueType(field))
                    End If
                Next
            Next

            Return fields
        End Function

        Private Shared Sub PopulateDataTable(ByRef dt As DataTable, ByVal array As JArray)
            For Each item In array
                Dim newRow = dt.NewRow()

                If TypeOf item Is JObject Then
                    For Each prop In DirectCast(item, JObject).Properties
                        If IsJArrayOrJObject(prop.Value) Then
                            ' we have an array within an array so make a recursive 
                            ' call to populate this cell with the resulting datatable 
                            newRow(prop.Name) = DeserializeGeneric(prop.Value, True)
                        Else
                            newRow(prop.Name) = prop.Value
                        End If
                    Next
                ElseIf TypeOf item Is JArray Then
                    Dim table = DeserializeArray(DirectCast(item, JArray), True)
                    newRow(SingleValueColumnName) = table
                Else
                    newRow(SingleValueColumnName) = item.ToString()
                End If

                dt.Rows.Add(newRow)
            Next
        End Sub

        Private Shared Function IsJArrayOrJObject(token As JToken) As Boolean
            Return TypeOf token Is JObject OrElse TypeOf token Is JArray
        End Function

        Private Shared Function GetJPropertyValueType(prop As JProperty) As Type
            If IsJArrayOrJObject(prop.Value) Then
                Return GetType(DataTable)
            Else
                Return If(TryCast(prop.Value, JValue)?.Value, CObj("")).GetType()
            End If
        End Function

        Private Shared Function GetTypeOf(ByVal o As Object) As Type
            If o Is Nothing Then Return GetType(String)
            Return o.GetType
        End Function
    End Class
End Namespace