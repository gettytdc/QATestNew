Imports System.Collections.Generic
Imports Internationalisation
Imports clsElement = BluePrism.AMI.clsAMI.clsElement
Imports BluePrism.ApplicationManager.AMI

Public Class frmInspect

    Public mElements As ICollection(Of clsElement)

    Private Sub frmInspect_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        TreeView1.Nodes.Add(ResMan.GetString("frmInspect_tv_desktop"))
        PopulateTree(TreeView1.TopNode, mElements)

    End Sub


    Private Sub PopulateTree(ByVal t As TreeNode, ByVal children As ICollection(Of clsElement))

        For Each ce As clsElement In children

            'Try and come up with a more descriptive label than just the element
            'type, to make the tree more browsable. There is room for expansion
            'and improvement here...
            Dim name As String = ce.ElementType.Name
            Dim label As String = Nothing
            If name = "Window" Then
                If ce.Identifiers.ContainsKey("WindowText") Then
                    label = ce.Identifiers("WindowText").Value
                End If
                If String.IsNullOrEmpty(label) AndAlso ce.Identifiers.ContainsKey("ClassName") Then
                    label = ce.Identifiers("ClassName").Value
                End If
            End If
            If label IsNot Nothing AndAlso label.Length > 0 Then
                If label.Length > 17 Then
                    label = label.Substring(0, 17) & " ..."
                End If
                name &= " (" & label & ")"
            End If

            Dim ct As New TreeNode(name)
            ct.Tag = ce
            t.Nodes.Add(ct)
            PopulateTree(ct, ce.Children)
        Next

    End Sub

    Private Sub PopulateIdentifiers(ByVal el As clsElement)
        lstIdentifiers.Items.Clear()
        For Each id As clsIdentifierInfo In el.Identifiers.Values
            Dim li As New ListViewItem(id.FullyQualifiedName)
            li.SubItems.Add(New ListViewItem.ListViewSubItem(li, id.Value))
            lstIdentifiers.Items.Add(li)
        Next
    End Sub

    Private Sub TreeView1_AfterSelect(ByVal sender As System.Object, ByVal e As System.Windows.Forms.TreeViewEventArgs) Handles TreeView1.AfterSelect
        PopulateIdentifiers(CType(e.Node.Tag, clsElement))
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Close()
    End Sub

End Class
