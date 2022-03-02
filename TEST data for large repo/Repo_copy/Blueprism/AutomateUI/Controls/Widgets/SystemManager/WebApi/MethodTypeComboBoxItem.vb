Imports BluePrism.AutomateProcessCore.WebApis
Imports BluePrism.Core.Utility

''' <summary>
''' Class used as a wrapper for combo box items and the DataType enum
''' </summary>
Public Class MethodTypeComboBoxItem

    Public Sub New(parameterType As OutputMethodType)
        Type = parameterType
    End Sub

    ''' <summary>
    ''' The DataType of the underlying DataType value.
    ''' </summary>
    ''' <returns></returns>
    Property Type As OutputMethodType

    ''' <summary>
    ''' Returns the localised title of the DataType.
    ''' </summary>
    ReadOnly Property Title As String
        Get
            Return WebApiResources.ResourceManager.EnsureString("ResponseOutputParameterType_{0}_Title", Type)
        End Get
    End Property
End Class