Option Strict Off

Imports System.IO
Imports System.Collections.Generic
Imports BluePrism.BPCoreLib

''' <summary>
''' Class to enumerate all products and patches installed on a machine and write 
''' them to an output file.
''' The code makes use of late binding to handle COM objects, so requires 
''' Option Strict to be turned Off.
''' </summary>
Public Class InstalledProductEnumerator

    Const quote As Char = Chr(34)

    Const MSIINSTALLCONTEXT_USERMANAGED As Integer = 1
    Const MSIINSTALLCONTEXT_USERUNMANAGED As Integer = 2
    Const MSIINSTALLCONTEXT_MACHINE As Integer = 4
    Const MSIPATCHSTATE_ALL As Integer = 15

    ''' <summary>
    ''' Enumerates all installed products and patches on the machine, and writes the 
    ''' output to the specified file.
    ''' </summary>
    ''' <returns>A list of error strings detailing which products could not be 
    ''' enumerated</returns>
    ''' <param name="path">The filepath to write the output to</param>
    Public Shared Function EnumerateInstalledProducts(ByVal path As String) _
        As ICollection(Of String)

        Dim errorList As New List(Of String)
        Dim headerRow As String =
            "Manufacturer, Product Name, Product Code, Install Date, " &
            "Install Location, Product Version, Patch State"

        Using fs As FileStream = IO.File.OpenWrite(path)
            Using sw As StreamWriter = New StreamWriter(fs)
                Dim installer As Object = CreateObject("WindowsInstaller.Installer")
                sw.WriteLine(headerRow)

                For Each product As Object In installer.Products
                    Dim prodName As String = ""
                    Try
                        prodName = installer.ProductInfo(product, "ProductName")
                        If prodName = "" Then Continue For

                        Dim productVersion As String =
                            (installer.ProductInfo(product, "Version"))

                        If productVersion <> "" Then
                            productVersion = productVersion \ 65536 \ 256 & "." &
                                (productVersion \ 65535 Mod 256) & "." &
                                (productVersion Mod 65536) & ".0"
                        End If

                        Dim publisher As String = quote & installer.ProductInfo(product, "Publisher")
                        Dim productDate As String = installer.ProductInfo(product, "InstallDate")
                        Dim installLocation As String = installer.ProductInfo(product, "InstallLocation")

                        sw.WriteLine(publisher & quote & "," & quote & prodName & quote & "," & product & "," &
                                     productDate.Right(2) & "/" & productDate.Mid(5, 2) & "/" &
                                     productDate.Left(4) & "," & quote &
                                         installLocation & quote & "," & productVersion)

                        ' Display any patches applied to this product
                        For Each patch As Object In installer.PatchesEx(
                                    product,
                                    "",
                                    MSIINSTALLCONTEXT_USERMANAGED Or MSIINSTALLCONTEXT_USERUNMANAGED Or MSIINSTALLCONTEXT_MACHINE,
                                    MSIPATCHSTATE_ALL)

                            ' Read patch properties
                            Dim patchState As String
                            Dim patchDate As String = patch.PatchProperty("InstallDate")
                            Select Case patch.State
                                Case 1 : patchState = "Applied"
                                Case 2 : patchState = "Superseded"
                                Case 4 : patchState = "Obsolete"
                                Case Else : patchState = patch.State
                            End Select

                            sw.WriteLine("," & quote & patch.PatchProperty("DisplayName") & quote & "," &
                                                 quote & patch.PatchCode & quote & "," &
                                                 patchDate.Right( 2) & "/" & patchDate.Mid( 5, 2) & "/" & patchDate.Left(4) & "," &
                                                 ",," & patchState)
                        Next

                    Catch ex As Exception
                        ' Add exception message to the list of errors to pass back to write to ui, 
                        ' then just ignore the exception and continue enumerating the other products
                        errorList.Add(
                            "Error writing details of product " & prodName & " to CSV: " & ex.Message)
                    End Try
                Next

                installer = Nothing

            End Using
        End Using

        Return errorList

    End Function
End Class
