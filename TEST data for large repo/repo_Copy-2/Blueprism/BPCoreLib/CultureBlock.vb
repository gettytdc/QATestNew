Imports System.Globalization
Imports System.Threading

''' <summary>
''' Class to apply a culture to a thread over a using block, ensuring that the
''' thread's current culture is set back when an instance of this class is disposed
''' </summary>
Public Class CultureBlock : Implements IDisposable

    ' The culture saved at the point of creation
    Private mSavedCulture As CultureInfo

    ''' <summary>
    ''' Applies the given culture to the current thread, saving the current culture
    ''' to reset it back later.
    ''' </summary>
    ''' <param name="culture">The culture to apply to the current thread.</param>
    ''' <exception cref="ArgumentNullException">If the given culture object was null.
    ''' </exception>
    Public Sub New(ByVal culture As CultureInfo)
        If culture Is Nothing Then Throw New ArgumentNullException(NameOf(culture))
        mSavedCulture = Thread.CurrentThread.CurrentCulture
        Thread.CurrentThread.CurrentCulture = culture
    End Sub

    ''' <summary>
    ''' Applies the culture with the given name to the current thread, saving the
    ''' current culture to reset it back later
    ''' </summary>
    ''' <param name="cultureName">The name of the culture to apply.</param>
    ''' <exception cref="ArgumentException">If <paramref name="cultureName"/>
    ''' specifies a culture that is not supported.</exception>
    ''' <exception cref="ArgumentNullException">If <paramref name="cultureName"/> is
    ''' null.</exception>
    Public Sub New(ByVal cultureName As String)
        Me.New(CultureInfo.GetCultureInfo(cultureName))
    End Sub

    ''' <summary>
    ''' Disposes of this culture block, resetting the current thread's culture to
    ''' the saved culture. Any subsequent calls to this method will have no effect.
    ''' </summary>

    Public Sub Dispose() Implements IDisposable.Dispose
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    Private mDisposed As Boolean

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not mDisposed Then
            If disposing Then
                ' Localise and remove the saved culture
                Dim ci As CultureInfo = mSavedCulture
                mSavedCulture = Nothing
                ' If the local culture is null, Dispose() has already been called
                If ci Is Nothing Then Return
                ' Set the culture to the local value
                Thread.CurrentThread.CurrentCulture = ci
            End If
        End If
        mDisposed = True
    End Sub

    Protected Overrides Sub Finalize()
        Dispose(False)
        MyBase.Finalize()
    End Sub


End Class
