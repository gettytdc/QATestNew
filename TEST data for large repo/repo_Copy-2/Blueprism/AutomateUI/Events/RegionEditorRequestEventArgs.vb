
Imports BluePrism.AMI.clsAMI
Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' The args detailing the event fired when a region editor request has been made -
''' ie. a request to open something in the region editor
''' </summary>
Public Class RegionEditorRequestEventArgs : Inherits ElementChosenEventArgs

    ''' <summary>
    ''' Creates a new RegionEditorRequestEventArgs for the given element.
    ''' </summary>
    ''' <param name="elem">The elements chosen</param>
    Public Sub New(ByVal elem As clsElement)
        MyBase.New(GetSingleton.ICollection(elem))
    End Sub

End Class


''' <summary>
''' Delegate used for handling RegionEditorRequest events
''' </summary>
Public Delegate Sub RegionEditorRequestEventHandler( _
 ByVal sender As Object, ByVal e As RegionEditorRequestEventArgs)
