Imports System.Runtime.CompilerServices
Imports BluePrism.Core.Resources

Public Module ResourceAttributesExtensions
    <Extension>
    Public Function GetLocalizedName(attributes As ResourceAttribute) As String
        Dim attributeArray As String() = attributes.ToString().Split(","c)
        Dim res As String = String.Empty

        For Each attribute As String In attributeArray
            res += $", {My.Resources.ResourceManager.GetString($"ResourceAttributes_{attribute.Trim()}")}"
        Next

        Return CStr(IIf(String.IsNullOrWhiteSpace(res), attributes.ToString(), res.Trim(","c, " "c)))
    End Function
End Module
