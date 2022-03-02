Imports BluePrism.AutomateProcessCore

Public Class clsMyXsltExtensionFunctions

    Dim bLocalise As Boolean

    Public Sub New(localise As Boolean)
        bLocalise = localise
    End Sub

    ''' <summary>
    ''' Localise the xml data type
    ''' </summary>
    ''' <param name="type">The data type.</param>
    ''' <returns>The localised data type.</returns>
    Public Function GetLocalizedFriendlyNameDataType(type As String) As String
        If bLocalise Then
            Return BluePrism.AutomateProcessCore.clsProcessDataTypes.GetFriendlyName(type)
        End If
        Return type
    End Function

    ''' <summary>
    ''' Get the localised FriendlyName
    ''' </summary>
    ''' <param name="name">The non-localised Name.</param>
    ''' <param name="objectname">The non-localised Business Object Name.</param>
    ''' <returns>The localised name.</returns>
    Public Function ChooseLocalizedFriendlyName(name As String, objectname As String) As String
        If bLocalise Then
            Return clsBusinessObjectAction.GetLocalizedFriendlyName(name, objectname, "Params")
        End If
        Return name
    End Function
End Class
