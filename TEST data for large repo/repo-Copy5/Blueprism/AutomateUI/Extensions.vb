Imports AutomateControls
Imports System.Runtime.CompilerServices
Imports BluePrism.AutomateAppCore.Groups
Imports WizardType = AutomateUI.frmWizard.WizardType
Imports System.Io
Imports BluePrism.AutomateAppCore
Imports BluePrism.AutomateProcessCore

Module Extensions

    ''' <summary>
    ''' Tries to get the data from the given data object, cast as the specified type.
    ''' If the data does not contain anything, or it is not of the required type,
    ''' this will return the default value of the specified type.
    ''' </summary>
    ''' <typeparam name="T">The type of data required</typeparam>
    ''' <param name="data">The data object holding the data required.</param>
    ''' <returns>The data embedded in the data object, of the specified type, or the
    ''' default value for that type if the object has no data of the type required.
    ''' </returns>
    <Extension>
    Public Function GetData(Of T)(data As IDataObject) As T
        If Not data.GetDataPresent(GetType(T)) Then Return Nothing
        Return DirectCast(data.GetData(GetType(T)), T)
    End Function

    ''' <summary>
    ''' Sets data into the given data object, setting the specific type for it.
    ''' </summary>
    ''' <typeparam name="T">The type to set within the data object</typeparam>
    ''' <param name="dataObj">The data object to add the data to</param>
    ''' <param name="data">The data to add to the object</param>
    <Extension>
    Public Sub SetData(Of T)(dataObj As IDataObject, data As T)
        dataObj.SetData(GetType(T).FullName, data)
    End Sub

    ''' <summary>
    ''' Extended overload of Control's DoDragDrop method, which allows the class of
    ''' the data object to be set rather than inferred.
    ''' </summary>
    ''' <typeparam name="T">The type of data to be set in the data object used in the
    ''' drag/drop operation in the treeview</typeparam>
    ''' <param name="ctl">The control on which the drag/drop operation should be
    ''' invoked.</param>
    ''' <param name="data">The data to embed in the drag operation</param>
    ''' <param name="allowedEffects">The effects allowed by this operation.</param>
    <Extension>
    Public Function DoDragDrop(Of T)(
     ctl As Control, data As T, allowedEffects As DragDropEffects) As DragDropEffects
        Return ctl.DoDragDrop(New DataObject(GetType(T).FullName, data), allowedEffects)
    End Function

    ''' <summary>
    ''' Gets the wizard type which corresponds to the given process-backed group
    ''' member's type.
    ''' </summary>
    ''' <param name="gmt">The group member type for which the corresponding wizard
    ''' type is required</param>
    ''' <returns>The wizard type which corresponds to the given group member type, or
    ''' <see cref="WizardType.Selection"/> if a clear wizard type could not be
    ''' determined.</returns>
    <Extension>
    Public Function GetWizardType(gmt As GroupMemberType) As WizardType
        Select Case gmt
            Case GroupMemberType.Object : Return WizardType.BusinessObject
            Case GroupMemberType.Process : Return WizardType.Process
            Case Else : Return WizardType.Selection
        End Select
    End Function

    ''' <summary>
    ''' Applies the mode (eg. Create, Edit, Debug) of a process view mode to a value
    ''' with the required type (eg. Process, Object)
    ''' </summary>
    ''' <param name="targ">The target enum to which the mode should be applied. All
    ''' modes are stripped out of this value before the <paramref name="mode"/> is
    ''' applied.</param>
    ''' <param name="mode">The mode to apply to the target value; All types are
    ''' stripped out of this value before it is applied to <paramref name="targ"/>
    ''' </param>
    ''' <returns>The combination of the type of <paramref name="targ"/> and the
    ''' mode of <paramref name="mode"/></returns>
    <Extension>
    Public Function ApplyMode(targ As ProcessViewMode, mode As ProcessViewMode) _
     As ProcessViewMode
        ' Strip down to type in the target value
        targ = targ And ProcessViewMode.AllTypes

        ' Conversely strip type out of the mode value
        mode = mode And Not ProcessViewMode.AllTypes

        ' Apply the mode to the vm
        Return targ Or mode

    End Function

    ''' <summary>
    ''' Applies the type (eg. Process, Object) of a process view mode to a value
    ''' with the requiredmode (eg. Create, Edit, Debug)
    ''' </summary>
    ''' <param name="targ">The target enum to which the type should be applied. All
    ''' types are stripped out of this value before the <paramref name="tp">
    ''' type</paramref> is applied.</param>
    ''' <param name="tp">The type to apply to the target value; All modes are
    ''' stripped out of this value before it is applied to <paramref name="targ"/>
    ''' </param>
    ''' <returns>The combination of the mode of <paramref name="targ"/> and the
    ''' type of <paramref name="tp"/></returns>
    <Extension>
    Public Function ApplyType(targ As ProcessViewMode, tp As ProcessViewMode) _
     As ProcessViewMode
        ' This is basically the converse of ApplyMode, that strips any 'type' info
        ' out of its mode argument, and reduces its targ argument to just the type
        ' so we can just invert the args and apply the mode
        ' ie. get just the type from 'tp' and apply just the mode from 'targ' to it
        Return tp.ApplyMode(targ)
    End Function

    ''' <summary>
    ''' Gets the item at the screen co-ordinates given, or null if no item was found
    ''' at the given co-ordinates.
    ''' </summary>
    ''' <param name="lv">The ListView to act upon</param>
    ''' <param name="p">The screen co-ordinate point at which the item is required.
    ''' </param>
    ''' <returns>The ListViewItem at the given screen co-ordinate point, or null if
    ''' there was no such item there</returns>
    <Extension>
    Public Function GetItemAtScreenPoint(lv As ListView, p As Point) As ListViewItem
        p = lv.PointToClient(p)
        Return lv.GetItemAt(p.X, p.Y)
    End Function

    ''' <summary>
    ''' Gets the item at the screen co-ordinates given, or null if no item was found
    ''' at the given co-ordinates.
    ''' </summary>
    ''' <param name="lv">The ListView to act upon</param>
    ''' <param name="x">The screen x co-ordinate at which the item is required.
    ''' </param>
    ''' <param name="y">The screen y co-ordinate at which the item is required.
    ''' </param>
    ''' <returns>The ListViewItem at the given screen co-ordinate point, or null if
    ''' there was no such item there</returns>
    <Extension>
    Public Function GetItemAtScreenPoint(lv As ListView, x As Integer, y As Integer) _
     As ListViewItem
        Return GetItemAtScreenPoint(lv, New Point(x, y))
    End Function

    ''' <summary>
    ''' Clear method on controls collection that disposes of child controls.
    ''' Note: disposing a control automatically removes it from its parent.
    ''' </summary>
    <Extension>
    Public Sub Clear(controls As Control.ControlCollection, dispose As Boolean)
        For i As Integer = controls.Count - 1 To 0 Step -1
            If dispose Then
                controls(i).Dispose()
            Else
                controls.RemoveAt(i)
            End If
        Next
    End Sub

    ''' <summary>
    ''' Gets the group store associated with the <see cref="frmApplication"/> which
    ''' is hosting this control, or a <see cref="NullGroupStore"/> if no application
    ''' form instance was found, or it had no store associated with it.
    ''' </summary>
    ''' <param name="this">The control from which to retrieve the application
    ''' form-associated group store</param>
    ''' <returns>The group store associated with the application form which is
    ''' hosting this control, or a <see cref="NullGroupStore"/> if the group store
    ''' could not be found for any reason</returns>
    <Extension>
    Public Function GetGroupStore(this As Control) As IGroupStore
        ' Get the first child instance found in the hierarchy (including the control
        ' itself in the search).
        Dim child As IChild = TryCast(this, IChild)
        If child Is Nothing Then child = this.GetAncestor(Of IChild)()

        ' Get the App form from the child, if a child was found. If not, look for
        ' the app form itself directly
        Dim frm As frmApplication = If(child IsNot Nothing,
         child.ParentAppForm, this.GetAncestor(Of frmApplication)())

        ' If it's still not set, see if it can be found by the class itself (as it
        ' stands, there can be only one, but that may change)
        If frm Is Nothing Then frm = frmApplication.GetCurrent()

        ' Get the group store from the app form if one was found
        Dim store As IGroupStore = If(frm IsNot Nothing, frm.GroupStore, Nothing)

        ' And return it, providing a null store if no group was found because any
        ' part of the chain was missing without replacement
        Return If(store, NullGroupStore.Instance)

    End Function

    <Extension>
    Public Sub SetEnvironmentColours(this As IEnvironmentColourManager, mgr As IEnvironmentColourManager)
        If mgr IsNot Nothing Then
            this.EnvironmentForeColor = mgr.EnvironmentForeColor
            this.EnvironmentBackColor = mgr.EnvironmentBackColor
        End If
    End Sub

    <Extension>
    Public Sub SetEnvironmentColoursFromAncestor(this As IEnvironmentColourManager, fromAncestorIn As Control)
        Dim mgr = TryCast(fromAncestorIn, IEnvironmentColourManager)
        If mgr Is Nothing Then mgr = fromAncestorIn.GetAncestor(Of IEnvironmentColourManager)()
        SetEnvironmentColours(this, mgr)
    End Sub

    <Extension()>
    Public Function IsEmpty(this As DataTable) As Boolean
        Return If((this?.Rows?.Count = 0), False)
    End Function

    <Extension()>
    Public function GetBluePrismFileTypeName(this As string) As string
        Dim fileExtension = Path.GetExtension(this).Replace(".", "").Trim()
        Select Case True
            Case String.Compare(fileExtension, clsProcess.ObjectFileExtension, StringComparison.InvariantCultureIgnoreCase) = 0
                Return My.Resources.ctlProcessViewer_Object
            Case String.Compare(fileExtension, clsProcess.ProcessFileExtension, StringComparison.InvariantCultureIgnoreCase) = 0
                Return My.Resources.ProcessType_processL
            Case String.Compare(fileExtension, clsRelease.FileExtension, StringComparison.InvariantCultureIgnoreCase) = 0
                Return My.Resources.ReleaseL
            Case Else
                Return My.Resources.FileL
        End Select
    End function

End Module
