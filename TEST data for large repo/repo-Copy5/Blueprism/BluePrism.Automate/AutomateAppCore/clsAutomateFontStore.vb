Imports BluePrism.CharMatching
Imports BluePrism.BPCoreLib.Collections

''' <summary>
''' Font store for automate.
''' </summary>
Public Class clsAutomateFontStore : Implements IFontStore

    ' A cached version of the fonts loaded from the database.
    Private mFontCache As IDictionary(Of String, BPFont) = _
     GetSynced.IDictionary(New Dictionary(Of String, BPFont))

    ' The version number at which the cache was valid - the version is incremented
    ' whenever a font is updated; if the current version number is different, the
    ' cache must be invalidated and the fonts must be reloaded from the database
    Private mFontCacheVersion As Long

    ''' <summary>
    ''' Checks if the cache of fonts we currently have is up to date; clearing the
    ''' cache if it is not.
    ''' </summary>
    Private Sub CheckCacheVersion()
        If gSv.HasFontDataUpdated(mFontCacheVersion) Then mFontCache.Clear()
    End Sub

    ''' <summary>
    ''' Gets the names of the fonts available within this store
    ''' </summary>
    Public ReadOnly Property AvailableFontNames() As ICollection(Of String) _
     Implements IFontStore.AvailableFontNames
        Get
            Return gSv.GetFontNames()
        End Get
    End Property

    ''' <summary>
    ''' Gets the font with the given name from this store.
    ''' </summary>
    ''' <param name="name">The name of the required font</param>
    ''' <returns>The font corresponding to the given name</returns>
    ''' <exception cref="NoSuchFontException">If the font requested does not exist
    ''' in the database backing this store</exception>
    ''' <exception cref="Exception">If any errors occur while attempting to retrieve
    ''' the font.</exception>
    Public Function GetFont(ByVal name As String) As BPFont _
     Implements IFontStore.GetFont
        CheckCacheVersion()

        Dim f As BPFont = Nothing
        If mFontCache.TryGetValue(name, f) Then Return f

        Dim ver As String = Nothing
        Dim xml As String = gSv.GetFont(name, ver)
        If xml Is Nothing Then Throw New NoSuchFontException(name)
        Return New BPFont(name, name, ver, xml)
    End Function

    Public Function GetFontOcrPlus(ByVal name As String) As String _
     Implements IFontStore.GetFontOcrPlus

        Dim ver As String = Nothing
        Dim xml As String = gSv.GetFontOcrPlus(name, ver)
        If xml Is Nothing Then Throw New NoSuchFontException(name)
        Return xml
    End Function

    ''' <summary>
    ''' Saves the given font to this store
    ''' </summary>
    ''' <param name="font">The font to save</param>
    Public Sub SaveFont(ByVal font As BPFont) Implements IFontStore.SaveFont
        gSv.SaveFont(font.DbName, font.Name, font.Version, font.Data.GetXML(True))
        font.DbName = font.Name
    End Sub

    Public Sub SaveFontOcrPlus(ByVal fontName As String, ByVal fontData As String) Implements IFontStore.SaveFontOcrPlus
        gSv.SaveFontOcrPlus(fontName, fontData)
    End Sub

    ''' <summary>
    ''' Deletes the font with the given name
    ''' </summary>
    ''' <param name="name">The name of the font to be deleted.</param>
    Public Function DeleteFont(ByVal name As String) As Boolean _
     Implements IFontStore.DeleteFont
        Return gSv.DeleteFont(name)
    End Function

End Class
