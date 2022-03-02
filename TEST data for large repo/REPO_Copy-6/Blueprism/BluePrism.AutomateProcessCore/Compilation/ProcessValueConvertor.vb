Imports System.Drawing
Imports BluePrism.BPCoreLib
Imports BluePrism.Server.Domain.Models

Namespace Compilation

    ''' <summary>
    ''' Provides conversion between Blue Prism and .NET types that is used to map 
    ''' between parameters and Blue Prism values when executing compiled code
    ''' </summary>
    Public Class ProcessValueConvertor

        ''' <summary>
        ''' Converts a .Net value into a clsProcessValue
        ''' </summary>
        ''' <param name="val">The .Net value to convert</param>
        Public Shared Function ConvertNetTypeToValue(ByVal val As Object) As clsProcessValue
            Return ConvertNetTypeToValue(val, val.GetType())
        End Function

        ''' <summary>
        ''' Converts a .Net value into a clsProcessValue
        ''' </summary>
        ''' <param name="val">The .Net value to convert</param>
        ''' <param name="tp">The system type to use for the .NET value - this may differ
        ''' from the concrete type of the value (eg. for DataTable values - you may want
        ''' the type defined in the column rather than the absolute type of the value).
        ''' </param>
        ''' <returns>A process value corresponding to the provided .net value</returns>
        Public Shared Function ConvertNetTypeToValue(val As Object, tp As Type) As clsProcessValue
            Return ConvertNetTypeToValue(val, clsProcessDataTypes.GetDataTypeFromFrameworkEquivalent(tp))
        End Function

        ''' <summary>
        ''' Converts a .Net value into a clsProcessValue
        ''' </summary>
        ''' <param name="val">The .Net value to convert</param>
        ''' <param name="dtype">The destination data type</param>
        ''' <returns>A process value corresponding to the provided .net value</returns>
        Public Shared Function ConvertNetTypeToValue(val As Object, dtype As DataType) As clsProcessValue

            If val Is Nothing OrElse IsDBNull(val) Then Return New clsProcessValue(dtype, "")

            Select Case dtype
                Case DataType.collection : Return CType(val, DataTable)
                Case DataType.timespan : Return CType(val, TimeSpan)
                Case DataType.image : Return CType(val, Bitmap)
                Case DataType.binary : Return CType(val, Byte())
                Case DataType.number : Return CDec(val)
                Case DataType.flag : Return CBool(val)
                Case DataType.datetime, DataType.date, DataType.time
                    Dim dt As Date = CType(val, Date)
                    If dtype = DataType.datetime Then dt = dt.ToUniversalTime()
                    Return New clsProcessValue(dtype, dt)
                Case Else
                    Return New clsProcessValue(dtype, BPUtil.IfNull(val, ""))
            End Select
        End Function

        ''' <summary>
        ''' Converts a clsProcessValue into the appropriate .Net value
        ''' </summary>
        ''' <param name="val">The clsProcessValue to convert</param>
        ''' <param name="typeOnly">This is used just to get an object whose type is
        ''' appropriate for the clsProcessValue, but which actual value left as the
        ''' default for that type, e.g decimal would be 0and date would be
        ''' 0000-00-00 00:00:00</param>
        ''' <param name="name">A name for the value. This is optional, and only used for
        ''' assigning names to DataTable outputs, when the type is a Collection.</param>
        ''' <returns></returns>
        ''' <exception cref="BadCastException">If the value could not be converted from
        ''' a process value to a .net value</exception>
        Friend Shared Function ConvertValueToNetType(ByVal val As clsProcessValue, ByVal typeOnly As Boolean, Optional ByVal name As String = Nothing) As Object
            Try
                Select Case val.DataType
                    Case DataType.number
                        If typeOnly Then Return 0D
                        Return CDec(val)
                    Case DataType.text, DataType.password
                        If typeOnly Then Return String.Empty
                        Return CStr(val)
                    Case DataType.flag
                        If typeOnly Then Return False
                        Return CBool(val)
                    Case DataType.date, DataType.datetime, DataType.time
                        If typeOnly Then Return Date.MinValue
                        Return CDate(val)
                    Case DataType.timespan
                        If typeOnly Then Return TimeSpan.Zero
                        Return CType(val, TimeSpan)
                    Case DataType.image
                        If typeOnly Then Return New Bitmap(1, 1)
                        Return CType(val, Bitmap)
                    Case DataType.binary
                        Return CType(val, Byte())
                    Case DataType.collection
                        Dim tab As New DataTable()
                        If name IsNot Nothing Then tab.TableName = name

                        'Currently, a null collection is just passed as a completely
                        'empty DataTable - this is for backward compatibility.
                        If val.Collection Is Nothing Then Return tab

                        tab.ExtendedProperties.Add("SingleRow", val.Collection.SingleRow)

                        For Each fi As clsCollectionFieldInfo In val.Collection.Definition.FieldDefinitions
                            Dim dataCol As New DataColumn(fi.Name)

                            Dim dt As Type = clsProcessDataTypes.GetFrameworkEquivalentFromDataType(fi.DataType)
                            If dt Is Nothing Then Throw New BadCastException(My.Resources.Resources.ProcessValueConvertor_CannotConvertCollectionFieldOfType0, fi.DataType)


                            dataCol.DataType = dt
                            If dt = GetType(DateTime) Then
                                dataCol.DateTimeMode = DataSetDateTime.Utc
                            End If
                            ' Mark column with Blue Prism data type
                            dataCol.ExtendedProperties(clsCollection.BluePrismTypeExtendedPropertyName) = fi.DataType
                            tab.Columns.Add(dataCol)
                        Next
                        For i As Integer = 0 To val.Collection.Count - 1
                            Dim colRow As clsCollectionRow = val.Collection.Row(i)
                            Dim newRow As DataRow = tab.NewRow()
                            Dim j As Integer = 0
                            For Each fieldValue As clsProcessValue In colRow.FieldValues
                                Dim o As Object = ConvertValueToNetType(fieldValue, typeOnly, tab.Columns(j).ColumnName)
                                newRow.Item(j) = If(o Is Nothing, DBNull.Value, o)
                                j += 1
                            Next

                            tab.Rows.Add(newRow)
                        Next

                        Return tab

                    Case Else
                        If typeOnly Then Return New Object()
                        Throw New BadCastException(My.Resources.Resources.ProcessValueConvertor_CannotConvertValueOfDatatype0ToADatatypeCompatibleWithNETCode, clsProcessDataTypes.GetFriendlyName(val.DataType))
                End Select

            Catch bce As BadCastException
                ' BadCastExceptions are the only ones that we throw intentionally, so
                ' rethrow them as before
                Throw

            Catch ex As Exception
                ' I'm not a big fan of masking errors, but it's a breaking change if we
                ' introduce a new failure point to existing processes, and everything
                ' before was not reporting errors, so return a null value for any failed
                ' conversions
                Debug.Fail(String.Format(My.Resources.Resources.ProcessValueConvertor_UnexpectedExceptionConvertingValue0ToANetValue1, val, ex))

            End Try

            Return Nothing
        End Function
    End Class
End NameSpace
