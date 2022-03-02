Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore

''' <summary>
''' Control to represent and resolve a conflict in a package component being
''' imported into the current environment.
''' </summary>
Public Class ctlConflict
    Inherits UserControl

#Region " Statics and constants "

    ''' <summary>
    ''' Gets the accessible name to represent the given option for the given
    ''' component.
    ''' </summary>
    ''' <param name="comp">The component that the option is for.</param>
    ''' <param name="opt">The option to represent in an accessible name.</param>
    ''' <returns>A name to use to identify the given option for the specified
    ''' component.</returns>
    Friend Shared Function GetAccessibleName( _
     ByVal comp As PackageComponent, ByVal opt As ConflictOption) As String
        Return String.Format("{0}: {1} ({2})", PackageComponentType.GetLocalizedFriendlyName(comp.Type.Plural, True), PackageComponentType.GetLocalizedFriendlyName(comp.Name), opt.Choice)
    End Function

    ''' <summary>
    ''' Gets the accessible name to represent the named field, which is required by
    ''' choosing the given option for the specified component.
    ''' </summary>
    ''' <param name="comp">The component that the option is for.</param>
    ''' <param name="opt">The option to represent in an accessible name.</param>
    ''' <param name="fieldName">The field name for which the accessible name is
    ''' required.</param>
    ''' <returns>A name to use to identify the field required by the given option for
    ''' the specified component.</returns>
    Friend Shared Function GetAccessibleName( _
     ByVal comp As PackageComponent, ByVal opt As ConflictOption, ByVal fieldName As String) _
     As String
        Return String.Format("{0}: {1} ({2}: {3})", PackageComponentType.GetLocalizedFriendlyName(comp.Type.Plural, True), PackageComponentType.GetLocalizedFriendlyName(comp.Name), opt.Choice, fieldName)
    End Function

#End Region

#Region " Member variables "

    ' The controls to keep in this conflict - not cleared when ClearOptionalControls() is called
    Private mConstantControls As New clsSet(Of Control)

    ' The conflict represented by this control
    Private mConflict As Conflict

    ' The currently selected option for this conflict
    Private mCurrentOption As ConflictOption

    ' The currently selected handler for this conflict
    Private mCurrentHandler As ConflictDataHandler

    ' The data handling controls which have been created for this conflict
    Private mDataHandlingControls As ICollection(Of Control)

#End Region

#Region " Constructors "

    ''' <summary>
    ''' Creates a new, empty conflict control
    ''' </summary>
    Public Sub New()
        Me.New(Nothing)
    End Sub

    ''' <summary>
    ''' Creates a new conflict control for the given package component, representing
    ''' the specified conflict.
    ''' </summary>
    ''' <param name="conf">The conflict to represent in this control</param>
    Public Sub New(ByVal conf As Conflict)

        ' This call is required by the Windows Form Designer.
        InitializeComponent()
        Me.DoubleBuffered = True

        ' Add any initialization after the InitializeComponent() call.
        mDataHandlingControls = New List(Of Control)
        mConstantControls.Add(lblConflictText)
        mConstantControls.Add(lblComponentName)

        Me.Conflict = conf

        Me.Text = String.Format("{0}:{1}({2})", _
         conf.Definition.Id, conf.Component.Name, conf.Component.TypeKey)

    End Sub

#End Region

#Region " Properties "

    ''' <summary>
    ''' Gets the conflict resolution for this conflict.
    ''' </summary>
    Public ReadOnly Property Resolution() As ConflictResolution
        Get
            ' Current Option is updated whenever the option is changed.
            ' Each of the process value handling controls should update their
            ' corresponding arguments when their values change, thus all of this
            ' should be up to date.
            Return New ConflictResolution(mConflict, mCurrentOption, mCurrentHandler)
        End Get
    End Property

    ''' <summary>
    ''' The conflict that this control is representing
    ''' </summary>
    Public Property Conflict() As Conflict
        Get
            Return mConflict
        End Get
        Set(ByVal value As Conflict)
            mConflict = value
            If value Is Nothing Then
                lblComponentName.Text = ""
                lblConflictText.Text = ""
            Else
                Dim defn As ConflictDefinition = value.Definition
                lblConflictText.Text = String.Format(My.Resources.ctlConflict_Text0Hint1, defn.Text, defn.Hint)
                lblComponentName.Text = value.Component.Name
                For Each opt As ConflictOption In defn.Options
                    Add(New ctlConflictOptionRadioButton(value.Component, opt) With {
                            .Checked = (opt.Choice = defn.DefaultInteractiveResolution)})
                Next
            End If
            mFlowPanel.PerformLayout()
        End Set
    End Property

    ''' <summary>
    ''' Flag indicating whether the component name should be visible in this control
    ''' or not.
    ''' </summary>
    Public Property ComponentNameVisible() As Boolean
        Get
            Return lblComponentName.Visible
        End Get
        Set(ByVal value As Boolean)
            lblComponentName.Visible = value
        End Set
    End Property

#End Region

#Region " Methods "

    ''' <summary>
    ''' Adds the given control to this conflict control - this just delegates to
    ''' the flow panel defined in this control
    ''' </summary>
    ''' <param name="ctl">The control to add to 'this' conflict control</param>
    Private Sub Add(ByVal ctl As Control)
        ctl.Margin = New Padding(3)
        mFlowPanel.Controls.Add(ctl)
    End Sub

    ''' <summary>
    ''' Adds the given conflict option radio button to this conflict control.
    ''' This ensures that a handler is assigned to the radio button to deal with
    ''' conflict options being chosen.
    ''' </summary>
    ''' <param name="rb">The radio button to add.</param>
    Private Sub Add(ByVal rb As ctlConflictOptionRadioButton)
        AddHandler rb.ConflictOptionChosen, AddressOf HandleConflictOptionChosen
        Add(DirectCast(rb, Control))
        ' If the radio button is already checked (due to being the recommended
        ' option), ensure that the handler is called to deal with associated text
        ' boxes and the like
        If rb.Checked Then _
         HandleConflictOptionChosen(Me, New ConflictEventArgs(rb, Nothing, rb.ConflictOption))

    End Sub

    ''' <summary>
    ''' Handles a conflict option for this conflict being chosen.
    ''' This stores the selected option and shows any ancillary controls to capture
    ''' additional data required by the option.
    ''' </summary>
    Private Sub HandleConflictOptionChosen(ByVal sender As Object, ByVal e As ConflictEventArgs)

        If mCurrentOption Is e.ConflictOption Then Return
        Try
            If Parent IsNot Nothing Then Parent.SuspendLayout()
            mFlowPanel.SuspendLayout()

            ' We need to ensure that no other handler is being shown
            If mCurrentOption IsNot Nothing Then
                For Each ctl As Control In mDataHandlingControls
                    If ctl.Enabled Then ctl.Enabled = False
                Next
            End If

            mCurrentOption = e.ConflictOption
            mCurrentHandler = mConflict.ChooseOption(mCurrentOption)

            Dim handler As ConflictDataHandler = mCurrentHandler
            If handler IsNot Nothing Then
                ' Find the controls in mDataHandlingControls if they are there
                Dim foundAlready As Boolean = False
                For Each ctl As Control In mDataHandlingControls
                    ' A control's tag either is the handler itself, or a clsArgument
                    ' which the handler contains
                    If handler Is ctl.Tag OrElse handler.Arguments.Contains(TryCast(ctl.Tag, ConflictArgument)) Then
                        ctl.Visible = True
                        ctl.Enabled = True
                        foundAlready = True
                    End If
                Next
                If foundAlready Then Return

                Dim controls As New List(Of Control)
                If handler.Text <> "" Then
                    Dim lbl As New Label()
                    lbl.Text = handler.Text
                    lbl.Tag = handler
                    controls.Add(lbl)
                End If
                For Each arg As ConflictArgument In handler.Arguments
                    Dim lbl As New Label()
                    lbl.Text = arg.CustomTitleOrDefault
                    controls.Add(lbl)
                    Dim ctl As Control = clsProcessValueControl.GetControl(arg.Value.DataType)
                    Dim ipv As IProcessValue = DirectCast(ctl, IProcessValue)
                    ipv.Value = arg.Value
                    AddHandler ipv.Changed, AddressOf HandleProcessValueChanged

                    ctl.AccessibleName = GetAccessibleName(mConflict.Component,
                                                           mCurrentOption,
                                                           arg.CustomTitleOrDefault)
                    controls.Add(ctl)

                    ' Set the arg into the controls' tags so we can identify them later
                    lbl.Tag = arg
                    ctl.Tag = arg
                Next
                For Each ctl As Control In controls
                    ctl.Visible = False
                    mDataHandlingControls.Add(ctl)
                Next
                ' Add them to the flow panel and insert them after the radio that was checked
                Dim flow As ControlCollection = mFlowPanel.Controls
                Dim lastControl As Control = DirectCast(e.Source, Control)
                For Each ctl As Control In controls
                    Add(ctl)
                    flow.SetChildIndex(ctl, flow.IndexOf(lastControl) + 1)
                    lastControl = ctl
                Next
                ' finally, make them all visible
                For Each ctl As Control In controls
                    ctl.Visible = True
                Next

            End If
        Finally
            If Parent IsNot Nothing Then Parent.ResumeLayout()
            mFlowPanel.ResumeLayout()
        End Try

    End Sub

    ''' <summary>
    ''' Handles a process value control's value being changed.
    ''' </summary>
    Private Sub HandleProcessValueChanged(ByVal sender As Object, ByVal e As EventArgs)
        Dim src As Control = DirectCast(sender, Control)
        For Each ctl As Control In mDataHandlingControls
            If src Is ctl Then
                DirectCast(ctl.Tag, ConflictArgument).Value = DirectCast(src, IProcessValue).Value
            End If
        Next
    End Sub

    ''' <summary>
    ''' Clears all controls which are not always present - the only ones which
    ''' are always present are defined in the <c>mConstantControls</c> set.
    ''' </summary>
    Private Sub ClearOptionalControls()
        ' Such a long winded way of removing controls...
        Dim removals As New List(Of Control)
        For Each ctl As Control In mFlowPanel.Controls
            If Not mConstantControls.Contains(ctl) Then removals.Add(ctl)
        Next
        For Each ctl As Control In removals
            mFlowPanel.Controls.Remove(ctl)
        Next
    End Sub

#End Region

End Class
