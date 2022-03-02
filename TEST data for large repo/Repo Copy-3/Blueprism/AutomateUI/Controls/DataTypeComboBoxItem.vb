Imports BluePrism.AutomateProcessCore

''' <summary>
''' Class used as a wrapper for combo box items and the DataType enum
''' </summary>
Public Class DataTypeComboBoxItem

    Public Sub New(dType As DataType)
        Type = dType
    End Sub

    ''' <summary>
    ''' The DataType of the underlying DataType value.
    ''' </summary>
    ''' <returns></returns>
    Property Type As DataType

    ''' <summary>
    ''' Returns the localised title of the DataType.
    ''' </summary>
    ReadOnly Property Title As String
        Get
            Return clsDataTypeInfo.GetLocalizedFriendlyName(Type)
        End Get
    End Property
End Class
