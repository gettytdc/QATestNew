Option Strict On

''' <summary>
''' A class based on ToolStripProfessionalRenderer used to
''' highlight mnuEditUndo and mnuEditRedo in frmProcess.
''' </summary>
''' <remarks></remarks>
Friend Class clsProcessUndoMenuRenderer
    Inherits ToolStripProfessionalRenderer

    Private maItemsToHighlight As New List(Of ToolStripItem)
    Private moForm As frmProcess

#Region "New"

    ''' <summary>
    ''' Constructor with reference to the parent form.
    ''' </summary>
    ''' <param name="f"></param>
    ''' <remarks></remarks>
    Public Sub New(ByVal f As frmProcess)
        MyBase.New()
        moForm = f
    End Sub

#End Region

#Region "OnRenderMenuItemBackground"
    ''' <summary>
    ''' Event handler used to a render menu item background.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Protected Overrides Sub OnRenderMenuItemBackground(ByVal e As ToolStripItemRenderEventArgs)

        Dim oTopItem As ToolStripItem

        oTopItem = e.Item
        Do
            If oTopItem.OwnerItem Is Nothing Then
                Exit Do
            Else
                oTopItem = oTopItem.OwnerItem
            End If
        Loop

        If TypeOf oTopItem Is ToolStripSplitButton Then
            DoRenderButtonItemBackground(e)
        Else
            DoRenderMenuItemBackground(e)
        End If

    End Sub

#End Region

#Region "DoRenderMenuItemBackground"

    ''' <summary>
    ''' Method used to a render menu item background.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub DoRenderMenuItemBackground(ByVal e As ToolStripItemRenderEventArgs)

        Dim bHighLightThisItem As Boolean

        If e.Item.Selected _
            AndAlso (e.Item.OwnerItem Is moForm.mnuEditUndo Or e.Item.OwnerItem Is moForm.mnuEditRedo) Then

            'This item is a child of mnuEditUndo or mnuEditRedo and should be highlighted.
            bHighLightThisItem = HighLightSiblings(e.Item, CType(e.Item.OwnerItem, ToolStripMenuItem).DropDownItems)

        ElseIf e.Item.Selected _
            AndAlso (e.Item Is moForm.mnuEditUndo Or e.Item Is moForm.mnuEditRedo) Then

            'This item is mnuEditUndo or mnuEditRedo and should be highlighted.
            bHighLightThisItem = True

            'Any child items should be unhighlighted.
            Dim aItemsToHighlight As Object()
            aItemsToHighlight = maItemsToHighlight.ToArray
            maItemsToHighlight.Clear()
            For i As Integer = 0 To aItemsToHighlight.Length - 1
                CType(aItemsToHighlight(i), ToolStripItem).Invalidate()
            Next

        ElseIf maItemsToHighlight.Contains(e.Item) Then
            bHighLightThisItem = True

        Else
            'This item is unrelated to mnuEditUndo or mnuEditRedo and should be highlighted as normal.
            MyBase.OnRenderMenuItemBackground(e)

        End If

        If bHighLightThisItem Then
            HighLightItem(e)
        End If


    End Sub

#End Region

#Region "DoRenderButtonItemBackground"

    ''' <summary>
    ''' Method used to a render button menu item background.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub DoRenderButtonItemBackground(ByVal e As ToolStripItemRenderEventArgs)

        Dim bHighLightThisItem As Boolean

        If e.Item.Selected _
            AndAlso (e.Item.OwnerItem Is moForm.btnUndo Or e.Item.OwnerItem Is moForm.btnRedo) Then

            'This item is a child of btnUndo1 or btnRedo1 and should be highlighted.
            bHighLightThisItem = HighLightSiblings(e.Item, CType(e.Item.OwnerItem, ToolStripSplitButton).DropDownItems)

        ElseIf maItemsToHighlight.Contains(e.Item) Then
            bHighLightThisItem = True

        Else
            'This item is unrelated to btnUndo1 or btnRedo1 and should be highlighted as normal.
            MyBase.OnRenderMenuItemBackground(e)

        End If

        If bHighLightThisItem Then
            HighLightItem(e)
        End If

    End Sub

#End Region

#Region "HighLightSiblings"

    ''' <summary>
    ''' Invalidates menu items so that they are highlighted or rendered normally
    ''' according to whether they are above or below the current item.
    ''' </summary>
    ''' <param name="oThisItem"></param>
    ''' <param name="colSiblings"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Private Function HighLightSiblings(ByVal oThisItem As ToolStripItem, ByVal colSiblings As ToolStripItemCollection) As Boolean

        Dim oSiblingItem As ToolStripItem
        Dim bHighLightThisItem As Boolean
        Dim i, j As Integer

        bHighLightThisItem = False
        For i = 1 To colSiblings.Count

            oSiblingItem = colSiblings(i - 1)

            If j = 0 Then
                If oSiblingItem Is oThisItem Then
                    'The sibling is this item.
                    j = i
                    bHighLightThisItem = True
                    If Not maItemsToHighlight.Contains(oSiblingItem) Then
                        maItemsToHighlight.Add(oSiblingItem)
                    End If
                Else
                    'The sibling is above this item.
                    If Not maItemsToHighlight.Contains(oSiblingItem) Then
                        maItemsToHighlight.Add(oSiblingItem)
                        oSiblingItem.Invalidate()
                    End If
                End If
            Else
                'The sibling is below this item.
                maItemsToHighlight.Remove(oSiblingItem)
                oSiblingItem.Invalidate()
            End If
        Next

        Return bHighLightThisItem

    End Function

#End Region

#Region "HighLightItem"

    ''' <summary>
    ''' Highlights a menu item.
    ''' </summary>
    ''' <param name="e"></param>
    ''' <remarks></remarks>
    Private Sub HighLightItem(ByVal e As ToolStripItemRenderEventArgs)
        e.Graphics.FillRectangle(New SolidBrush(AutomateControls.ColourScheme.Default.UndoMenuHighLightBackground), _
   e.Item.ContentRectangle)
        e.Graphics.DrawRectangle(New Pen(AutomateControls.ColourScheme.Default.UndoMenuHighLightBorder), _
   e.Item.ContentRectangle)
    End Sub

#End Region

End Class
