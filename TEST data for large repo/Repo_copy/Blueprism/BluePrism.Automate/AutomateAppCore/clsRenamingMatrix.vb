Imports BluePrism.BPCoreLib.Collections

Public Class clsRenamingMatrix

    Public Class Clash

        ' The original name of the source of the clash
        Private mSource As String

        ' The original name of the object the source clashed with
        Private mOrig As String

        ' The new name of the object the source clashed with
        Private mNew As String

        Public Sub New(ByVal src As String, ByVal origName As String, ByVal newName As String)
            mSource = src
            mOrig = origName
            mNew = newName
        End Sub

        Public ReadOnly Property Source() As String
            Get
                Return mSource
            End Get
        End Property

        Public ReadOnly Property OriginalName() As String
            Get
                Return mOrig
            End Get
        End Property

        Public ReadOnly Property NewName() As String
            Get
                Return mNew
            End Get
        End Property

        Public ReadOnly Property WithOriginal() As Boolean
            Get
                Return (mNew Is Nothing)
            End Get
        End Property
    End Class

    Private mMap As IDictionary(Of String, String)

    Public Sub New()
        mMap = New Dictionary(Of String, String)
    End Sub

    Public Sub Add(ByVal origName As String)
        If Not mMap.ContainsKey(origName) Then mMap(origName) = Nothing
    End Sub

    Default Public Property Item(ByVal origName As String) As String
        Get
            Return mMap(origName)
        End Get
        Set(ByVal value As String)
            mMap(origName) = value
        End Set
    End Property

    Public Function FindCurrentNames(ByVal name As String) As IDictionary(Of String, String)
        Dim submap As New Dictionary(Of String, String)
        For Each pair As KeyValuePair(Of String, String) In mMap
            If pair.Value = name OrElse (pair.Value Is Nothing AndAlso pair.Key = name) Then
                submap.Add(pair.Key, pair.Value)
            End If
        Next
        Return submap
    End Function

    Public Function Validate() As ICollection(Of Clash)

        Dim clashes As New List(Of Clash)
        Dim dealtWith As New clsSet(Of String)

        For Each origName As String In mMap.Keys

            Dim newName As String = mMap(origName)
            If newName IsNot Nothing Then
                If dealtWith.Contains(newName) Then Continue For

                Dim submap As IDictionary(Of String, String) = FindCurrentNames(newName)

                ' Remove this one
                submap.Remove(origName)

                ' if we have any left, then we have a clash
                If submap.Count > 0 Then
                    For Each pair As KeyValuePair(Of String, String) In submap
                        clashes.Add(New Clash(origName, pair.Key, pair.Value))
                        ' we don't want double errors - eg.
                        ' a => c
                        ' b => c
                        ' should cause Clash(a, b, c)
                        ' but not then Clash(b, a, c)
                        If pair.Value IsNot Nothing Then dealtWith.Add(pair.Value)
                    Next
                End If
            End If
        Next
        Return clashes

    End Function

End Class
