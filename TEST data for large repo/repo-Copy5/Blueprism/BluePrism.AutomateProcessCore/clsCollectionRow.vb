Imports System.Runtime.Serialization

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsCollectionRow
''' 
''' <summary>
''' This class is used to represent a single row in a clsCollection.
''' </summary>
<Serializable()> _
Public Class clsCollectionRow
    Inherits Dictionary(Of String, clsProcessValue)

    ''' <summary>
    ''' Default constructor needs to be explicitly defined now that there's a
    ''' serialization-based constructor
    ''' </summary>
    Public Sub New()
    End Sub

    ''' <summary>
    ''' Creates a collection row from the given data row, expected to be resident
    ''' within a data table.
    ''' </summary>
    ''' <param name="row">The row from which this collection row should draw its
    ''' data.</param>
    Friend Sub New(ByVal row As DataRow)
        For Each col As DataColumn In row.Table.Columns
            Dim d As DataType = clsProcessDataTypes.GetDataTypeFromFrameworkEquivalent(col.DataType)

            ' If the data table indicates what type of data it is, we should use that
            If col.ExtendedProperties.ContainsKey(clsCollection.BluePrismTypeExtendedPropertyName) Then
                Dim prop = col.ExtendedProperties(clsCollection.BluePrismTypeExtendedPropertyName)
                If TypeOf prop Is DataType Then d = DirectCast(prop, DataType)
            End If

            Add(col.ColumnName,
                clsCompilerRunner.ConvertNetTypeToValue(row(col), d))
        Next
    End Sub

    ''' <summary>
    ''' Creates a collection row, copied from the given row.
    ''' </summary>
    ''' <param name="row">The non-null collection row from which this object should
    ''' draw its data.</param>
    Friend Sub New(ByVal row As clsCollectionRow)
        For Each fld As String In row.FieldNames
            Add(fld, row(fld).Clone())
        Next
    End Sub

    ''' <summary>
    ''' Constructor with which instances of this class can be deserialized
    ''' </summary>
    ''' <param name="info">The serialization info with which this object can
    ''' be deserialized</param>
    ''' <param name="context">The context</param>
    Public Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        MyBase.New(info, context)
    End Sub

    ''' <summary>
    ''' Gets a collection of the values in the row
    ''' </summary>
    ''' <value></value>
    Public ReadOnly Property FieldValues() As System.Collections.ICollection
        Get
            Return Values
        End Get
    End Property


    ''' <summary>
    ''' Set the value of a field.
    ''' </summary>
    Public Sub SetField(ByVal name As String, ByVal value As clsProcessValue)
        'TODO: doesn't this need to handle nested fields?
        Me(name) = value
    End Sub

    ''' <summary>
    ''' Return the field from the collection - if it exists
    ''' </summary>
    ''' <param name="name">the name of the field</param>
    ''' <returns></returns>
    Public Function GetField(ByVal name As String) As clsProcessValue
        If Me.ContainsKey(name) Then
            Return Me(name)
        Else
            Return Nothing
        End If
    End Function

    ''' <summary>
    ''' Gets a collection of the fieldnames in the row
    ''' </summary>
    Public ReadOnly Property FieldNames() As Dictionary(Of String, clsProcessValue).KeyCollection
        Get
            Return Keys
        End Get
    End Property


    ''' <summary>
    ''' Determine if the row contains a field with the given name.
    ''' </summary>
    ''' <param name="sFieldName">The name of the field.</param>
    ''' <returns>True if the row contains a field with that name, False otherwise
    ''' </returns>
    Public Function Contains(ByVal sFieldName As String) As Boolean
        Return ContainsKey(sFieldName)
    End Function


    ''' <summary>
    ''' Clones this collection row and returns it.
    ''' </summary>
    ''' <returns>A deep clone of this collection row</returns>
    Public Function Clone() As clsCollectionRow
        Dim copy As New clsCollectionRow()
        For Each field As String In Keys
            copy(field) = Me(field).Clone()
        Next
        Return copy
    End Function

    ''' <summary>
    ''' Checks if this collection row is equal to the given object or not.
    ''' </summary>
    ''' <param name="obj">The object to test against</param>
    ''' <returns>True if the given object is a non-null collection row with the same
    ''' fields and values as this row</returns>
    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Dim row As clsCollectionRow = TryCast(obj, clsCollectionRow)
        If row Is Nothing OrElse Me.Count <> row.Count Then Return False

        For Each fld As String In FieldNames
            If Not row.Contains(fld) Then Return False
            If Not Me(fld).IsValueMatch(row(fld)) Then Return False
        Next

        ' If we get to the end of the fields with no differences, we must be the same
        Return True
    End Function

End Class
