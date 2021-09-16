Imports BluePrism.Images

Public NotInheritable Class clsImageLists

    ''' <summary>
    ''' There's no concept of a static class in VB.net - this has much the same effect.
    ''' </summary>
    Private Sub New()
    End Sub

    Private Shared sSmallExportTargets As ImageList
    Public Shared ReadOnly Property SmallExportTargets() As ImageList
        Get
            If sSmallExportTargets Is Nothing Then
                Dim imgList As New ImageList()
                imgList.ColorDepth = ColorDepth.Depth32Bit
                With imgList.Images
                    .Add("file", ToolImages.New_16x16)
                    .Add("environment", ToolImages.Database_Save_16x16)
                End With
                sSmallExportTargets = imgList
            End If
            Return sSmallExportTargets
        End Get
    End Property

End Class
