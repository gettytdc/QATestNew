''' <summary>
''' Class which provides access to a lock and invokes delegates within a
''' locked code block, meaning that they have thread safe access to any
''' object which is using that lock to synchronize access.
''' 
''' eg. a dictionary returned from GetSynced.IDictionary() extends this
''' object allowing code like :-
''' <code>
''' IDictionary&lt;int, String&gt; dict = GetSynced.IDictionary(srcDict);
''' SyncedDelegator delegator = dict as SyncedDelegator;
''' delegator.PerformThreadSafeOperation(delegate(){
'''     if (!dict.ContainsKey(key))
'''         dict[key] = value;
''' });
''' </code>
''' ... thereby ensuring that any calls made within the method given are
''' done within the scope of the lock used by the dictionary. This ensures that locks
''' are controlled by the instance which maintains the lock and it doesn't have to
''' expose it to the wider world.
''' 
''' It also ensures that no monitor exits are missed.
''' </summary>
<Serializable>
Public Class SynchronizedDelegator

    ' The lock used when invoking delegate methods
    Protected ReadOnly mLock As Object

    ''' <summary>
    ''' Creates a new delegator using the given lock
    ''' </summary>
    ''' <param name="lock">The lock to use when invoking delegate methods.</param>
    ''' <exception cref="ArgumentNullException">If the given object was null.
    ''' </exception>
    Public Sub New(ByVal lock As Object)
        If lock Is Nothing Then Throw New ArgumentNullException(NameOf(lock))
        mLock = lock
    End Sub

    ''' <summary>
    ''' Performs the given action within the given lock
    ''' </summary>
    ''' <param name="a">The action to perform</param>
    Public Sub PerformThreadSafeOperation(ByVal a As Action)
        SyncLock mLock
            a()
        End SyncLock
    End Sub

    ''' <summary>
    ''' Performs the given action within the given lock
    ''' </summary>
    ''' <param name="a">The action to perform</param>
    ''' <param name="arg">The argument to pass to the action</param>
    ''' <typeparam name="T">The type of the argument to the action</typeparam>
    Public Sub PerformThreadSafeOperation(Of T)( _
     ByVal a As Action(Of T), ByVal arg As T)
        SyncLock mLock
            a(arg)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Performs the given action within the given lock
    ''' </summary>
    ''' <param name="a">The action to perform</param>
    ''' <param name="arg">The argument to pass to the action</param>
    ''' <param name="arg2">The second argument for the action</param>
    ''' <typeparam name="T">The type of the first argument to the action</typeparam>
    ''' <typeparam name="T2">The type of the second argument to the action
    ''' </typeparam>
    Public Sub PerformThreadSafeOperation(Of T, T2)( _
     ByVal a As Action(Of T, T2), ByVal arg As T, ByVal arg2 As T2)
        SyncLock mLock
            a(arg, arg2)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Performs the given function within the given lock
    ''' </summary>
    ''' <param name="f">The function to perform</param>
    ''' <returns>The value returned from the function call</returns>
    Public Function PerformThreadSafeOperation(Of TResult)( _
     ByVal f As Func(Of TResult)) As TResult
        SyncLock mLock
            Return f()
        End SyncLock
    End Function

    ''' <summary>
    ''' Performs the given function within the given lock
    ''' </summary>
    ''' <param name="f">The function to perform</param>
    ''' <param name="arg">The argument to pass to the function</param>
    ''' <returns>The value returned from the function call</returns>
    Public Function PerformThreadSafeOperation(Of T, TResult)( _
     ByVal f As Func(Of T, TResult), ByVal arg As T) As TResult
        SyncLock mLock
            Return f(arg)
        End SyncLock
    End Function

End Class
