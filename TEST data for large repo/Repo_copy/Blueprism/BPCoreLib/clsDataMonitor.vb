Imports System.ComponentModel
Imports System.Runtime.Serialization

''' Project  : BPCoreLib
''' Class    : clsDataMonitor
''' <summary>
''' Base class used to monitor changes in data. A call to ChangeData() will 
''' ensure that the 'dirty' flag is set (ie. that changes have been made to the
''' data represented by this class).
''' </summary>
<Serializable()>
<DataContract(Namespace:="bp", IsReference:=True)>
Public MustInherit Class clsDataMonitor
    Implements IMonitorable

    ''' <summary>
    ''' The handlers for the data changed event. This is explicitly done here
    ''' so that it can be non-serialized. You can't just set an event not to be
    ''' serialized in VB, and this is the best solution for this class.
    ''' </summary>
    <NonSerialized()> _
    Private mEventHandlers As EventHandlerList

    ''' <summary>
    ''' Gets the event handler list for this object, creating a new one if it
    ''' doesn't yet exist (which it might not if this object has been serialized
    ''' into being).
    ''' </summary>
    ''' <returns>The event handler list for this object.</returns>
    Private Function GetEventHandlers() As EventHandlerList
        If mEventHandlers Is Nothing Then mEventHandlers = New EventHandlerList()
        Return mEventHandlers
    End Function

    ''' <summary>
    ''' Event fired when the data in this class changes.
    ''' </summary>
    ''' <param name="sender">The source of the change</param>
    ''' <param name="args">The arguments defining the change</param>
    Public Custom Event DataChanged As DataChangeEventHandler _
     Implements IMonitorable.DataChanged

        AddHandler(ByVal value As DataChangeEventHandler)
            GetEventHandlers().AddHandler(Me, value)
        End AddHandler

        RemoveHandler(ByVal value As DataChangeEventHandler)
            GetEventHandlers().RemoveHandler(Me, value)
        End RemoveHandler

        RaiseEvent(ByVal sender As Object, ByVal args As DataChangeEventArgs)
            ' By definition, if there's no event handlers object, then
            ' there are no event handlers.
            If mEventHandlers Is Nothing Then Return

            Dim del As [Delegate] = mEventHandlers(Me)
            If del IsNot Nothing Then del.DynamicInvoke(sender, args)

        End RaiseEvent

    End Event

    ''' <summary>
    ''' The 'dirty' flag. ie. a flag which indicates that the data in this object
    ''' has changed or not.
    ''' </summary>
    Private mDirty As Boolean

    ''' <summary>
    ''' Checks to see if any data has been changed on this object since it was
    ''' retrieved from the database.
    ''' </summary>
    ''' <returns>True if the schedule has changed at all</returns>
    Public Overridable Function HasChanged() As Boolean _
     Implements IMonitorable.HasChanged
        Return mDirty
    End Function

    ''' <summary>
    ''' Resets the changed data flag within this object. Immediately after
    ''' calling this method, <see cref="HasChanged"/> will return False.
    ''' </summary>
    Public Overridable Sub ResetChanged() Implements IMonitorable.ResetChanged
        mDirty = False
    End Sub

    ''' <summary>
    ''' Marks this object as having been changed. This allows external objects
    ''' to alter the changed data state of this object.
    ''' </summary>
    ''' <exception cref="InvalidOperationException">If the descendant class does
    ''' not allow external manipulation of the data changed state.</exception>
    Public Overridable Sub Mark() Implements IMonitorable.Mark
        mDirty = True
        RaiseEvent DataChanged(Me, New DataChangeEventArgs())
    End Sub

    ''' <summary>
    ''' Indicates that the given data has changed within this object.
    ''' </summary>
    ''' <param name="dataName">The name of the data which has changed</param>
    ''' <param name="oldValue">The old value of the data</param>
    ''' <param name="newValue">The new value of the data</param>
    Protected Overridable Sub MarkDataChanged( _
     ByVal dataName As String, ByVal oldValue As Object, ByVal newValue As Object)
        mDirty = True
        OnDataChanged(New DataChangeEventArgs(dataName, oldValue, newValue))
    End Sub

    ''' <summary>
    ''' Changes the data in the given variable to the new value, then raises an event
    ''' indicating that the data has changed.
    ''' </summary>
    ''' <typeparam name="T">The type of data being changed</typeparam>
    ''' <param name="dataName">The name of the data to use in the
    ''' <see cref="DataChanged"/> event</param>
    ''' <param name="varToChange">The reference to the variable which should be
    ''' changed.</param>
    ''' <param name="newValue">The new value to change the variable to</param>
    ''' <remarks>If the values are the same according to
    ''' <see cref="[Object].Equals"/>, then this method will have no effect.
    ''' </remarks>
    Protected Overridable Sub ChangeData(Of T)( _
     ByVal dataName As String, ByRef varToChange As T, ByVal newValue As T)
        If Object.Equals(varToChange, newValue) Then Return
        Dim oldValue As T = varToChange
        varToChange = newValue
        MarkDataChanged(dataName, oldValue, newValue)
    End Sub

    ''' <summary>
    ''' Raises the <see cref="DataChanged"/> event with the given args.
    ''' </summary>
    ''' <param name="e">The arguments detailing the data change</param>
    Protected Overridable Sub OnDataChanged(ByVal e As DataChangeEventArgs)
        RaiseEvent DataChanged(Me, e)
    End Sub

    ''' <summary>
    ''' Gets a memberwise clone of this object with data monitor events
    ''' disassociated, ie. any listeners to this object's <see cref="DataChanged"/>
    ''' event will <em>not</em> be registered as listeners on the copied object.
    ''' </summary>
    ''' <returns>A shallow clone of this object with events described
    ''' in this object disassociated.</returns>
    ''' <remarks>This is primarily here to allow subclasses to copy their data
    ''' without copying events.</remarks>
    Protected Function Copy() As clsDataMonitor
        Dim dm As clsDataMonitor = DirectCast(MemberwiseClone(), clsDataMonitor)
        dm.mEventHandlers = Nothing
        Return dm
    End Function

End Class
