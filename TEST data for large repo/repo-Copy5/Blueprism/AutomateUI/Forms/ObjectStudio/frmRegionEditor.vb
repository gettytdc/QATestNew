Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib.Collections
Imports BluePrism.AutomateAppCore
Imports BluePrism.CharMatching.UI

Public Class frmRegionEditor : Inherits RegionEditorForm

#Region " Static Declarations "

    ''' <summary>
    ''' Displays region editor form, initialised with the given bitmap and captures
    ''' the regions created on it by the user.
    ''' </summary>
    ''' <param name="owner">The owner window for the region editor dialog</param>
    ''' <param name="img">The image on which the regions should be drawn</param>
    ''' <returns>A collection of the spy regions specified by the user, null if the
    ''' user cancelled or closed the dialog.</returns>
    Public Shared Function GetRegions(ByVal owner As IWin32Window, _
     ByVal img As Bitmap) As ICollection(Of SpyRegion)
        Return GetRegions( _
         owner, img, GetEmpty.ICollection(Of clsApplicationRegion), Nothing)
    End Function

    ''' <summary>
    ''' Displays region editor form, initialised with the given bitmap and the given
    ''' collection of regions, and captures the regions created on it by the user.
    ''' </summary>
    ''' <param name="owner">The owner window for the region editor dialog</param>
    ''' <param name="currRegions">The current regions to display on the form when it
    ''' is shown</param>
    ''' <param name="img">The image on which the regions should be drawn</param>
    ''' <returns>A collection of the spy regions specified by the user, null if the
    ''' user cancelled or closed the dialog.</returns>
    Public Shared Function GetRegions(ByVal owner As IWin32Window, _
     ByVal img As Bitmap, ByVal currRegions As ICollection(Of clsApplicationRegion)) _
     As ICollection(Of SpyRegion)
        Return GetRegions(owner, img, currRegions, Nothing)
    End Function

    ''' <summary>
    ''' Displays region editor form, initialised with the given bitmap and the given
    ''' collection of regions, and captures the regions created on it by the user.
    ''' </summary>
    ''' <param name="owner">The owner window for the region editor dialog</param>
    ''' <param name="currRegions">The current regions to display on the form when it
    ''' is shown</param>
    ''' <param name="img">The image on which the regions should be drawn</param>
    ''' <param name="apply">Event handler which is called when the Apply button is
    ''' pressed in this form.</param>
    ''' <returns>A collection of the spy regions specified by the user, null if the
    ''' user cancelled or closed the dialog.</returns>
    Public Shared Function GetRegions(ByVal owner As IWin32Window, _
     ByVal img As Bitmap, ByVal currRegions As ICollection(Of clsApplicationRegion), _
     ByVal apply As EventHandler) As ICollection(Of SpyRegion)

        Using f As New frmRegionEditor()
            f.ShowInTaskbar = False
            Dim mapper As RegionMapper = f.regMapper
            mapper.Image = img
            mapper.SpyRegions = clsRegionConvert.ToSpyRegions(mapper, currRegions)

            If apply IsNot Nothing Then AddHandler f.RegionsApplied, apply

            If f.ShowDialog() = DialogResult.OK Then Return mapper.SpyRegions

            Return Nothing

        End Using

    End Function

    ''' <summary>
    ''' Validates the names of a collection of SpyRegions
    ''' </summary>
    ''' <param name="regions">the collection of regions on the form</param>
    ''' <returns>True if all names pass validation</returns>
    ''' <remarks></remarks>
    Shared Function ValidateRegionNames(ByVal regions As ICollection(Of SpyRegion)) _
        As Boolean
        Dim emptyLabel As String = RelativeSpyRegionTypeConverter.EmptyLabel
        '' Cannot name a region the same as the empty string for the anchor region
        Return _
            Not regions.Any(Function(reg) String.Equals(reg.Name, emptyLabel, _
                                                        StringComparison.InvariantCultureIgnoreCase))
    End Function

#End Region

    ''' <summary>
    ''' Creates a new region editor form using the Automate DB for its fonts
    ''' </summary>
    Public Sub New()
        regMapper.Store = New clsAutomateFontStore()
    End Sub

    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmRegionEditor))
        Me.SuspendLayout()
        '
        'frmRegionEditor
        '
        resources.ApplyResources(Me, "$this")
        Me.Name = "frmRegionEditor"
        Me.ResumeLayout(False)

    End Sub

    ''' <summary>
    ''' Form Closing Event Handler allowing the region names to be validated before 
    ''' saving
    ''' </summary>
    Private Sub frmRegionEditor_FormClosing(sender As Object, e As FormClosingEventArgs) _
        Handles Me.FormClosing

        Dim mapper As RegionMapper = CType(sender, frmRegionEditor).regMapper

        e.Cancel = Not ValidateRegionNames(mapper.SpyRegions)

        If e.Cancel Then MessageBox.Show(String.Format(My.Resources.CannotNameRegion0,
                                      {RelativeSpyRegionTypeConverter.EmptyLabel}),
                        My.Resources.InvalidRegionName, MessageBoxButtons.OK, MessageBoxIcon.Error)
    End Sub
End Class
