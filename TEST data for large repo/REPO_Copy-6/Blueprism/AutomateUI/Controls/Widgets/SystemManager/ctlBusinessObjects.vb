Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateAppCore
Imports ObjectEventCode = BluePrism.AutomateAppCore.ObjectEventCode
Imports BluePrism.Images

''' Project  : Automate
''' Class    : ctlBusinessObjectsView
''' 
''' <summary>
''' A control to display and manage Business Objects.
''' </summary>
Public Class ctlBusinessObjectsView
    Inherits System.Windows.Forms.UserControl

    Private mobjRefs As clsGroupBusinessObject
    Private objSorter As clsListViewSorter

#Region " Windows Form Designer generated code "

    Public Sub New()
        MyBase.New()

        'This call is required by the Windows Form Designer.
        InitializeComponent()

        'Add any initialization after the InitializeComponent() call
        lvBusinessObjects.SmallImageList = ImageLists.Components_16x16
        lvBusinessObjects.LargeImageList = ImageLists.Components_32x32

        If System.ComponentModel.LicenseManager.UsageMode = System.ComponentModel.LicenseUsageMode.Runtime Then
            PopulateUsingLatestObjects()
        End If
    End Sub

    'UserControl overrides dispose to clean up the component list.
    Protected Overloads Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing Then
            If Not (components Is Nothing) Then
                components.Dispose()
            End If
            If mobjRefs IsNot Nothing Then mobjRefs.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    Friend WithEvents lvBusinessObjects As AutomateControls.FlickerFreeListView
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(ctlBusinessObjectsView))
        Me.lvBusinessObjects = New AutomateControls.FlickerFreeListView()
        Me.SuspendLayout()
        '
        'lvBusinessObjects
        '
        resources.ApplyResources(Me.lvBusinessObjects, "lvBusinessObjects")
        Me.lvBusinessObjects.FullRowSelect = True
        Me.lvBusinessObjects.Name = "lvBusinessObjects"
        Me.lvBusinessObjects.UseCompatibleStateImageBehavior = False
        Me.lvBusinessObjects.View = System.Windows.Forms.View.Details
        '
        'ctlBusinessObjectsView
        '
        Me.BackColor = System.Drawing.Color.White
        Me.Controls.Add(Me.lvBusinessObjects)
        Me.Name = "ctlBusinessObjectsView"
        Me.ResumeLayout(False)

    End Sub

#End Region

    Private Sub lvBusinesObjects_MouseDown(ByVal sender As Object, ByVal e As MouseEventArgs) Handles lvBusinessObjects.MouseDown
        If e.Button = System.Windows.Forms.MouseButtons.Right Then
            Dim mnuContext As New ContextMenu

            mnuContext.MenuItems.Add(My.Resources.lvBusinesObjects_MouseDown_SmallIcons, New System.EventHandler(AddressOf ChangeView))
            mnuContext.MenuItems.Add(My.Resources.lvBusinesObjects_MouseDown_LargeIcons, New System.EventHandler(AddressOf ChangeView))
            mnuContext.MenuItems.Add(My.Resources.lvBusinesObjects_MouseDown_Details, New System.EventHandler(AddressOf ChangeView))

            Dim Item As ListViewItem = lvBusinessObjects.GetItemAt(e.X, e.Y)

            If Not Item Is Nothing Then
                Item.Selected = True
                mnuContext.MenuItems.Add(My.Resources.ctlBusinessObjectsView_MenuSeparator)
                Dim sobj As String
                sobj = Item.SubItems(1).Text

                Dim obj As clsBusinessObject = mobjRefs.FindObjectReference(sobj)

                Dim mi As MenuItem = mnuContext.MenuItems.Add(My.Resources.ConfigureObject, New System.EventHandler(AddressOf ConfigureObject))
                mi.Enabled = obj.Configurable

                mnuContext.MenuItems.Add(My.Resources.ViewObjectDocumentation, New System.EventHandler(AddressOf ViewDetails))

                mnuContext.MenuItems.Add(My.Resources.DeleteObject, New System.EventHandler(AddressOf DeleteSelectedObjects))
            End If

            Dim pPos As Point
            pPos.X = e.X
            pPos.Y = e.Y
            mnuContext.Show(lvBusinessObjects, pPos)
        End If

    End Sub

    ''' <summary>
    ''' The Configure Object menu item event handler.
    ''' </summary>
    ''' <param name="sender">The source object</param>
    ''' <param name="e">The event</param>
    Public Sub ConfigureObject(ByVal sender As Object, ByVal e As System.EventArgs)
        If lvBusinessObjects.SelectedItems.Count = 0 Then
            UserMessage.Show(My.Resources.NoBusinessObjectWasSelected)
            Exit Sub
        ElseIf Me.lvBusinessObjects.SelectedItems.Count > 1 Then
            UserMessage.Show(My.Resources.PleaseSelectOneObjectAtATimeToViewConfigure)
            Exit Sub
        End If
        Dim sObj As String
        Dim obj As clsBusinessObject
        Dim sConfigXML As String
        Dim sErr As String = Nothing

        sObj = lvBusinessObjects.SelectedItems(0).SubItems(1).Text

        obj = mobjRefs.FindObjectReference(sObj)
        If obj.Configurable AndAlso obj.ShowConfigUI(sErr) Then

            sConfigXML = obj.GetConfig(sErr)
            If sErr = "" Then
                Try 
                    gSv.SetResourceConfig(sObj, sConfigXML)
                Catch ex As Exception
                    sErr = String.Format(My.Resources.FailedToGetConfiguration0, ex.Message)
                End Try 
            End If
        Else
            If sErr Is Nothing Then sErr = My.Resources.ThisObjectCannotBeConfigured
        End If

        If sErr <> "" Then UserMessage.Show(sErr)
    End Sub

    ''' <summary>
    ''' Validates the selected Business Object.
    ''' </summary>
    Public Sub ValidateObject()
        If lvBusinessObjects.SelectedItems.Count = 0 Then
            UserMessage.Show(My.Resources.NoBusinessObjectWasSelected)
            Exit Sub
        ElseIf Me.lvBusinessObjects.SelectedItems.Count > 1 Then
            UserMessage.Show(My.Resources.PleaseSelectOneObjectAtATimeToViewValidate)
            Exit Sub
        End If
        Dim sObj As String
        Dim sErr As String = Nothing
        If lvBusinessObjects.SelectedItems.Count > 0 Then
            sObj = lvBusinessObjects.SelectedItems(0).SubItems(1).Text

            Dim ob As clsBusinessObject = mobjRefs.FindObjectReference(sObj)
            If TypeOf ob Is clsCOMBusinessObject Then
                Dim objCOM As clsCOMBusinessObject = CType(ob, clsCOMBusinessObject)

                If Not objCOM.Validate(sErr) Then
                    UserMessage.Show(sErr)
                Else
                    UserMessage.Show(My.Resources.ObjectHasValidCapabilities)
                End If
            End If
        End If
    End Sub

    ''' <summary>
    ''' The View Details menu item event handler.
    ''' </summary>
    ''' <param name="sender">The source object</param>
    ''' <param name="e">The event</param>
    Public Sub ViewDetails(ByVal sender As Object, ByVal e As System.EventArgs)
        If lvBusinessObjects.SelectedItems.Count = 0 Then
            UserMessage.Show(My.Resources.NoBusinessObjectWasSelected)
            Exit Sub
        ElseIf Me.lvBusinessObjects.SelectedItems.Count > 1 Then
            UserMessage.Show(My.Resources.PleaseSelectOneObjectAtATimeToViewDocumentation)
            Exit Sub
        End If
        Dim sClassName As String = lvBusinessObjects.SelectedItems(0).SubItems(1).Text
        Dim bo As clsBusinessObject = mobjRefs.FindObjectReference(sClassName)
        clsBOD.OpenAPIDocumentation(bo)
    End Sub

    Private Sub ChangeView(ByVal sender As Object, ByVal e As System.EventArgs)
        Select Case CType(sender, MenuItem).Text
            Case My.Resources.LargeIcons
                lvBusinessObjects.View = View.LargeIcon
            Case My.Resources.SmallIcons
                lvBusinessObjects.View = View.List
            Case My.Resources.ctlBusinessObjectsView_Details
                lvBusinessObjects.View = View.Details
        End Select
    End Sub

    Private Sub PopulateBusinessObjects()
        lvBusinessObjects.BeginUpdate()

        lvBusinessObjects.Items.Clear()
        lvBusinessObjects.Columns.Clear()

        lvBusinessObjects.Columns.Add(My.Resources.ctlBusinessObjectsView_mnuItemName, 200, HorizontalAlignment.Left)
        lvBusinessObjects.Columns.Add(My.Resources.ctlBusinessObjectsView_mnuitemClass, 200, HorizontalAlignment.Left)

        Dim item As ListViewItem
        For Each grp As clsGroupBusinessObject In mobjRefs.Children
            If grp.Name = "Legacy COM Objects" Then
                For Each objRef As clsCOMBusinessObject In grp.Children
                    item = lvBusinessObjects.Items.Add(objRef.FriendlyName, ImageLists.Keys.Component.Object)
                    item.SubItems.Add(objRef.Name)
                Next
            End If
        Next

        objSorter = New clsListViewSorter(lvBusinessObjects)
        objSorter.SortColumn = 0
        objSorter.Order = SortOrder.Ascending
        lvBusinessObjects.ListViewItemSorter = objSorter

        lvBusinessObjects.EndUpdate()
    End Sub

    ''' <summary>
    ''' Populates the control.
    ''' </summary>
    Public Sub PopulateUsingLatestObjects()
        If mobjRefs IsNot Nothing Then mobjRefs.Dispose()
        mobjRefs = New clsGroupBusinessObject(Options.Instance.GetExternalObjectsInfo(), Nothing, Nothing)
        PopulateBusinessObjects()
    End Sub

    ''' <summary>
    ''' The Delete Object menu item event handler.
    ''' </summary>
    ''' <param name="sender">The source object</param>
    ''' <param name="e">The event</param>
    Public Sub DeleteSelectedObjects(ByVal sender As Object, ByVal e As EventArgs)
        Dim configOptions = Options.Instance
        If Me.lvBusinessObjects.SelectedItems.Count = 0 Then
            UserMessage.Show(My.Resources.PleaseFirstSelectAnObjectToDelete)
            Return
        End If

        ' Construct dependencies
        Dim dependencyList As New clsProcessDependencyList()
        For Each obj As ListViewItem In lvBusinessObjects.SelectedItems
            dependencyList.Add(New clsProcessNameDependency(obj.SubItems(1).Text))
        Next

        ' Show dependency window
        Using application As New frmApplication
            If Not application.ConfirmDeletion(dependencyList) Then Return
        End Using

        ' Deselect items from the dependency window
        For Each obj As ListViewItem In lvBusinessObjects.SelectedItems
            obj.Selected = dependencyList.Has(New clsProcessNameDependency(obj.SubItems(1).Text))
        Next

        ' Delete items
        For Each obj As ListViewItem In lvBusinessObjects.SelectedItems
            Dim sName As String = obj.SubItems(1).Text
            configOptions.RemoveObject(sName)
            gSv.BusinessObjectDeleted(sName)
        Next

        ' Server Error Check
        Try
            configOptions.Save()
        Catch ex As Exception
            UserMessage.Show(String.Format(My.Resources.UnableToSaveTheSetting0, ex.Message))
        End Try

        Me.PopulateUsingLatestObjects()
    End Sub
End Class
