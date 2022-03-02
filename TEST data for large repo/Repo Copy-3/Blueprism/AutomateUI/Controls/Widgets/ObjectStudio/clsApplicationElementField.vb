Imports System.Xml
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Core.Xml
Imports BluePrism.Images

''' <summary>
''' Represents a field in which an application element can be displayed,
''' or populated via drag drop.
''' </summary>
''' <remarks>The ApplicationDefinition property should be set immediately
''' after construction.</remarks>
Public Class clsApplicationElementField
    Inherits TextBox

    ' The context menu to show for this app element field
    Private WithEvents mContextMenu As ContextMenuStrip

    'The keys used in the dictionary of context menu items
    Private CutItem As New ToolStripMenuItem(
        My.Resources.clsApplicationElementField_CuT, ToolImages.Cut_16x16, AddressOf Me.HandleCut)
    Private PasteItem As New ToolStripMenuItem(
        My.Resources.clsApplicationElementField_Paste, ToolImages.Paste_16x16, AddressOf Me.HandlePaste)
    Private CopyItem As New ToolStripMenuItem(
        My.Resources.clsApplicationElementField_Copy, ToolImages.Copy_16x16, AddressOf Me.HandleCopy)
    Private SynchItem As New ToolStripMenuItem(
        My.Resources.clsApplicationElementField_ShowInApplicationExplorer, ComponentImages.Structure_16x16,
        AddressOf Me.HandleSynchronise)

    Public Sub New()
        MyBase.New()

        Me.ReadOnly = True
        BackColor = SystemColors.ControlLightLight
        AllowDrop = True
        BorderStyle = BorderStyle.None

        mContextMenu = New ContextMenuStrip()
        With mContextMenu.Items
            .Add(CutItem)
            .Add(CopyItem)
            .Add(PasteItem)
            .Add(New ToolStripSeparator)
            .Add(SynchItem)
        End With
        ContextMenuStrip = mContextMenu
    End Sub

    ''' <summary>
    ''' Handles the context menu opening, ensuring that the menu items are enabled
    ''' or disabled according to the current state of the system.
    ''' </summary>
    Private Sub HandleMenuStripOpening() Handles mContextMenu.Opening
        UpdateMenuItems()
    End Sub

    Private Sub HandleCut(ByVal sender As Object, ByVal e As EventArgs)
        Try
            Me.HandleCopy(sender, e)
            Me.Element = Nothing
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.clsApplicationElementField_InternalError0, ex.Message))
        End Try
    End Sub

    Private Sub HandleCopy(ByVal sender As Object, ByVal e As EventArgs)
        Try
            If Me.mElement IsNot Nothing Then
                Dim ElementSkeleton As New clsApplicationElement(mElement.Name)
                ElementSkeleton.ID = mElement.ID
                Dim xdoc As New XmlDocument
                Clipboard.SetData(DataFormats.StringFormat, ElementSkeleton.ToXML(xdoc).OuterXml)
            Else
                UserMessage.ShowFloating(Me, ToolTipIcon.Error, My.Resources.clsApplicationElementField_Error, My.Resources.clsApplicationElementField_NoDataToCopyThisCellIsEmpty, New Point(0, Me.Height), True)
            End If
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.clsApplicationElementField_InternalError0, ex.Message))
        End Try
    End Sub

    Private Sub HandlePaste(ByVal sender As Object, ByVal e As EventArgs)
        Dim ClipboardText As String = Clipboard.GetText()
        Try
            Dim xdoc As New ReadableXmlDocument(ClipboardText)
            Dim FirstChild As XmlElement = CType(xdoc.FirstChild, XmlElement)
            Select Case FirstChild.Name
                Case "element", "region", "region-container"
                    'Accept the element from the clipboard but then get the latest equivalent element
                    'from the application model - the xml may be out of date, or this may have been pasted from
                    'another business object (in which case it is not valid).
                    Dim Element As clsApplicationElement = DirectCast(
                     clsApplicationMember.CreateFromXML(FirstChild), clsApplicationElement)

                    Dim RealElement As clsApplicationElement = Me.mApplicationDefinition.FindElement(Element.ID)

                    If RealElement IsNot Nothing Then
                        Me.Element = RealElement
                    Else
                        UserMessage.Show(My.Resources.clsApplicationElementField_ErrorThePastedElementDoesNotAppearToBeAValidMemberOfTheCurrentApplicationDefini)
                    End If
                Case Else
                    GoTo ExitXMLError
            End Select
        Catch xmlEX As XmlException
            GoTo ExitXMLError
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.clsApplicationElementField_UnexpectedErrorDuringPasteOperation0, ex.Message))
        End Try

        Exit Sub
ExitXMLError:
        UserMessage.ShowFloating(Me, ToolTipIcon.Error, My.Resources.clsApplicationElementField_Error, My.Resources.clsApplicationElementField_TheDataPastedToThisCellDoesNotAppearToBeAnApplicationElement, New Point(0, Me.Height), True)
    End Sub

    ''' <summary>
    ''' Determines if the data on the clipboard is suitable for pasting.
    ''' </summary>
    ''' <returns>True if the data can be pasted, false otherwise.</returns>
    <DebuggerHidden()>
    Private Function CanPaste() As Boolean
        Dim clipTxt As String = Clipboard.GetText().Trim()
        ' Quick text check so we don't have to use an exception to determine
        ' whether this is XML we're looking for
        If Not clipTxt.StartsWith("<element") Then Return False

        Try
            Dim xdoc As New ReadableXmlDocument(clipTxt)
            Dim FirstChild = CType(xdoc.FirstChild, XmlElement)
            If FirstChild.Name = "element" Then
                Return True
            End If
        Catch ex As Exception
            Return False
        End Try
    End Function

    ''' <summary>
    ''' Event raised when the 'synchronise' option is selected from the
    ''' context menu.
    ''' </summary>
    Public Event SynchronisationRequsted(ByVal ElementID As Guid)

    Private Sub HandleSynchronise(ByVal sender As Object, ByVal e As EventArgs)
        If Me.mElement IsNot Nothing Then
            Try
                RaiseEvent SynchronisationRequsted(Me.Element.ID)
            Catch ex As Exception
                UserMessage.Show(String.Format(My.Resources.clsApplicationElementField_UnexpectedError0, ex.Message))
            End Try
        End If
    End Sub

    ''' <summary>
    ''' Private member to store public property Element()
    ''' </summary>
    Private mElement As clsApplicationElement
    ''' <summary>
    ''' Gets and sets the element displayed in this control, if any.
    ''' </summary>
    Public Property Element() As clsApplicationElement
        Get
            Return Me.mElement
        End Get
        Set(ByVal value As clsApplicationElement)
            If value IsNot mElement Then
                mElement = value

                If value IsNot Nothing Then
                    Me.Text = value.Name
                Else
                    Me.Clear()
                End If

                Try
                    RaiseEvent ElementChanged()
                Catch ex As Exception
                    UserMessage.Show(String.Format(My.Resources.clsApplicationElementField_InternalError0, ex.Message))
                End Try
            End If
        End Set
    End Property

    ''' <summary>
    ''' Event raised when the element is changed.
    ''' </summary>
    Public Event ElementChanged()

    ''' <summary>
    ''' Private member to store public property ApplicationDefinition()
    ''' </summary>
    Private mApplicationDefinition As clsApplicationDefinition
    ''' <summary>
    ''' The application definition to use. Must not be null - this property
    ''' should be set immediately after construction.
    ''' </summary>
    Public Property ApplicationDefinition() As clsApplicationDefinition
        Get
            Return mApplicationDefinition
        End Get
        Set(ByVal value As clsApplicationDefinition)
            mApplicationDefinition = value
        End Set
    End Property

    ''' <summary>
    ''' Updates the 'enabled' property on each menu item.
    ''' </summary>
    ''' <remarks></remarks>
    Private Sub UpdateMenuItems()
        Dim ElementIsNull As Boolean = (mElement Is Nothing)
        CopyItem.Enabled = Not ElementIsNull
        CutItem.Enabled = Not ElementIsNull
        PasteItem.Enabled = CanPaste()
        SynchItem.Enabled = Not ElementIsNull
    End Sub

    Private Sub clsApplicationElementField_MouseDown(ByVal sender As Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles Me.MouseDown
        Me.UpdateMenuItems()
    End Sub
End Class
