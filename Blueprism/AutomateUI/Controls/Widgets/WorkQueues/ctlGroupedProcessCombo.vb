Imports BluePrism.AutomateAppCore
Imports System.Collections.Generic

Public Class clsGroupedProcessCombo
    Inherits AutomateControls.ComboBoxes.ComboBox

    Public Sub New()
        MyBase.New()

        Me.DropDownStyle = ComboBoxStyle.DropDownList
    End Sub

    ''' -----------------------------------------------------------------------------
    ''' <summary>
    ''' Populates the combobox with the supplied processes/groups.
    ''' </summary>
    ''' <param name="Groups">The groups to be populated. The processes should
    ''' be contained in the groups.</param>
    ''' -----------------------------------------------------------------------------
    Public Sub PopulateProcessGroups(ByVal Groups As Dictionary(Of Guid, clsBF.GroupInfo))
        Try
            Me.BeginUpdate()
            Me.Items.Clear()

            For Each G As clsBF.GroupInfo In Groups.Values
                Dim GI As New AutomateControls.ComboBoxes.ComboBoxItem(G.GroupName, G.GroupID)
                GI.ItemFont = New Font(Me.Font, FontStyle.Bold)
                GI.Enabled = False
                GI.DisabledColour = Color.Black
                Me.Items.Add(GI)

                If G.Members IsNot Nothing Then
                    For Each P As clsBF.ProcessInfo In G.Members.Values
                        'hack - we add some spaces to the process name to make it look indented.
                        Dim indentedname As String = "   " & P.ProcessName
                        Dim PI As New AutomateControls.ComboBoxes.ComboBoxItem(indentedname, P.ProcessID)
                        Me.Items.Add(PI)
                    Next
                End If
            Next
        Finally
            Me.EndUpdate()
        End Try
    End Sub
End Class
