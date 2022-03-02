
Imports BluePrism.AMI.clsAMI
Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' The args detailing the event fired when an application element has been chosen,
''' typically in the application navigator
''' </summary>
Public Class ElementChosenEventArgs : Inherits EventArgs

    ' The elements chosen
    Private mElems As ICollection(Of clsElement)

    ''' <summary>
    ''' Creates a new ElementChosenEventArgs object containing the given elements
    ''' </summary>
    ''' <param name="elems">The elements chosen</param>
    Public Sub New(ByVal elems As ICollection(Of clsElement))
        If elems Is Nothing Then Throw New ArgumentNullException(NameOf(elems))
        mElems = elems
    End Sub

    ''' <summary>
    ''' The elements chosen which triggered this event
    ''' </summary>
    Public ReadOnly Property Elements() As ICollection(Of clsElement)
        Get
            Return GetReadOnly.ICollection(mElems)
        End Get
    End Property

    ''' <summary>
    ''' The first element in the collection of elements detailed in these args.
    ''' Typically, there will just be one element returned by these args, so this
    ''' is here as a convenience, and so we can easily determine where something is
    ''' expecting only one element for the future.
    ''' </summary>
    Public ReadOnly Property FirstElement() As clsElement
        Get
            Return CollectionUtil.First(mElems)
        End Get
    End Property

End Class

''' <summary>
''' Delegate used for handling ElementChosen events
''' </summary>
Public Delegate Sub ElementChosenEventHandler( _
 ByVal sender As Object, ByVal e As ElementChosenEventArgs)

