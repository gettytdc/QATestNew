Imports BluePrism.AutomateAppCore
Imports BluePrism.Images

''' <summary>
''' Control to display the difference components between two releases
''' </summary>
Public Class ctlReleaseDifference : Inherits ctlWizardStageControl

    ' The diffs currently being displayed
    Private mDiffs As IDictionary(Of PackageComponent, String)

    ''' <summary>
    ''' Creates a new release difference control
    ''' </summary>
    Public Sub New()

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        colType.ImageList = ImageLists.Components_16x16

    End Sub

    ''' <summary>
    ''' Sets the differences to display in this control
    ''' </summary>
    Public Property Differences() As IDictionary(Of PackageComponent, String)
        Get
            Return mDiffs
        End Get
        Set(ByVal value As IDictionary(Of PackageComponent, String))
            mDiffs = value
            With gridDiffs.Rows
                .Clear()
                If value Is Nothing Then Return
                For Each comp As PackageComponent In value.Keys
                    If TypeOf comp Is GroupComponent Then
                        .Add(PackageComponentType.Group.Key, comp.Name, value(comp))
                    Else
                        .Add(comp.TypeKey, comp.Name, value(comp))
                    End If
                Next
            End With
        End Set
    End Property

End Class
