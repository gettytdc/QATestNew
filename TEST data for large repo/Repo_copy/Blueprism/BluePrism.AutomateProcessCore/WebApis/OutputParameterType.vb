Imports System.ComponentModel

Namespace WebApis
    Public Enum OutputMethodType
        None = 0

        <Description("Json Path")>
        JsonPath = 1

        <Description("Custom Code")>
        CustomCode = 2
    End Enum
End Namespace
