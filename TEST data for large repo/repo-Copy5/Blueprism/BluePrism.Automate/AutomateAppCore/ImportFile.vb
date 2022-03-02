Imports System.IO

Public Class ImportFile

    Public Sub New(x As String)
        Me.FileName = x
    End Sub

    Public ReadOnly Property FileName As String = String.Empty

    Public ReadOnly Property File As FileInfo
        Get
            Return If(String.IsNullOrEmpty(FileName), Nothing, New FileInfo(FileName))
        End Get
    End Property
    Public Property BluePrismName As String
    Public Property BluePrismId As Guid
    Public Property CanImport As Boolean = True
    Public Property Errors As List(Of String) = New List(Of String)
    Public Property UserHasPermission As Boolean = True
    Public Property ProcessType As PackageComponentType
    Public Property Conflicts As List(Of Conflict) = New list(of Conflict)
End Class
