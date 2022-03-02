Imports System.Collections.Concurrent
Imports System.Timers
Imports System.Threading

Imports Timer = System.Timers.Timer
Imports BluePrism.AutomateProcessCore

Namespace Groups

    ''' <summary>
    ''' Database group store which retains the data that it retrieves and updates it
    ''' in a single model
    ''' </summary>
    Public Class RetentiveGroupStore : Inherits DatabaseGroupStore

        ''' <summary>
        ''' Event fired when a tree is reloaded within this group store.
        ''' </summary>
        Public Event TreeUpdated As GroupTreeEventHandler

        ' Flag indicating if an update is currently in progress
        Private mUpdating As Integer

        ' A timer which kicks off an update from the database every 10 seconds
        Private WithEvents mUpdateTimer As Timer

        ' The (raw) trees mapped against their tree type held in this store
        Private ReadOnly mTrees As ConcurrentDictionary(Of (treeType As GroupTreeType, comboAtt As TreeAttributes), GroupTree)

        ''' <summary>
        ''' Creates a new retentive group store
        ''' </summary>
        Public Sub New(server As IServer)
            MyBase.New(server)
            mTrees = New ConcurrentDictionary(Of (GroupTreeType, TreeAttributes), GroupTree)
            mUpdateTimer = New Timer() With {.Interval = 10000}

            ' TODO: at the moment, there is nothing in place which indicates that
            ' the database has been updated; when that changes, this timer is worth
            ' revisiting to ensure that the tree data is kept up to date.
            mUpdateTimer.Enabled = False
        End Sub

        ''' <summary>
        ''' Gets the tree of the given type from the backing store
        ''' </summary>
        ''' <param name="tp">The type denoting which tree to retrieve</param>
        ''' <returns>The group tree of the given type from the database.</returns>
        Protected Overrides Function GetRawTree(
         tp As GroupTreeType, forceReload As Boolean, combinedAttributes As TreeAttributes) As GroupTree
            ' Default for key is second int attribute parameter, no retired
            If forceReload Then
                Dim t As GroupTree = MyBase.GetRawTree(tp, forceReload, combinedAttributes)
                mTrees((tp, combinedAttributes)) = t
                Return t
            Else
                Return mTrees.GetOrAdd((tp, combinedAttributes), Function(t) MyBase.GetRawTree(t.treeType, True, combinedAttributes))
            End If
        End Function

        ''' <summary>
        ''' Checks the database for updates to the trees currently held in this
        ''' group store.
        ''' </summary>
        Public Sub CheckForUpdates()
            ' Set the updating 'flag' - if it's already set, skip this check because
            ' it's already being done
            If Interlocked.CompareExchange(mUpdating, 1, 0) <> 0 Then Return

            Dim versions As IDictionary(Of String, Long) =
             New Dictionary(Of String, Long)
            For Each tree As GroupTree In mTrees.Values
                versions(tree.DataName) = tree.DataVersion
            Next
            versions = Server.GetUpdatedDataVersions(versions)
            For Each tree As GroupTree In mTrees.Values
                If versions.ContainsKey(tree.DataName) Then
                    tree.Reload()
                    OnTreeUpdated(New GroupTreeEventArgs(tree))
                End If
            Next

            mUpdating = 0
        End Sub

        ''' <summary>
        ''' Handles the timer elapsing, prompting a check for updated trees in the
        ''' database.
        ''' </summary>
        Private Sub HandleUpdateTimerElapsed(
         sender As Object, e As ElapsedEventArgs) Handles mUpdateTimer.Elapsed
            CheckForUpdates()
        End Sub

        ''' <summary>
        ''' Raises the <see cref="TreeUpdated"/> event
        ''' </summary>
        ''' <param name="e">The args detailing the event</param>
        Protected Overridable Sub OnTreeUpdated(e As GroupTreeEventArgs)
            RaiseEvent TreeUpdated(Me, e)
        End Sub

        ''' <summary>
        ''' Disposes of this store. This disposes and removes the update timer and
        ''' clears the map of trees from this object.
        ''' </summary>
        Public Overrides Sub Dispose()
            MyBase.Dispose()

            Dim tim = mUpdateTimer
            If tim IsNot Nothing Then tim.Dispose()
            mUpdateTimer = Nothing

            mTrees.Clear()
        End Sub

    End Class

End Namespace
