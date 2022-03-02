Imports BluePrism.Skills
Imports System.IO
Imports System.Windows.Media.Imaging

Namespace Controls.Widgets.ProcessStudio.Skills.Model
    Public Class SkillViewModel : Implements INotifyPropertyChanged, IDisposable

        Private mImage As BitmapImage
        Private mName As String
        Private mDescription As String
        Private mActions As List(Of SkillActionViewModel)

        Public ReadOnly Property Category As SkillCategory
        Public ReadOnly Property ID As Guid
        Public Property Name As String
            Get
                Return mName
            End Get
            Set(value As String)
                If mName <> value Then
                    mName = value
                    OnPropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Name)))
                End If
            End Set
        End Property
        Public Property Description As String
            Get
                Return mDescription
            End Get
            Set(value As String)
                If mDescription <> value Then
                    mDescription = value
                    OnPropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Description)))
                End If
            End Set
        End Property
        Public Property Image As BitmapImage
            Get
                Return mImage
            End Get
            Set
                mImage = Value
                OnPropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Image)))
            End Set
        End Property

        Public ReadOnly Property ActionsHeader As String
            Get
                Return LocaleTools.LTools.GetC("Actions", "misc")
            End Get
        End Property

        Public Property Actions As List(Of SkillActionViewModel)
            Get
                Return mActions
            End Get
            Set
                mActions = Value
                OnPropertyChanged(Me, New PropertyChangedEventArgs(NameOf(Actions)))
            End Set
        End Property

        Public ReadOnly Property Provider As String

        Public Sub New(skill As Skill)
            ID = skill.Id
            Name = skill.LatestVersion.Name
            Description = skill.LatestVersion.Description
            Provider = skill.Provider
            Category = skill.LatestVersion.Category

            CreateActionViewModels(CType(skill.LatestVersion, WebSkillVersion).WebApiActionNames)
            LoadImageAsynchronously(skill.LatestVersion.Icon)
        End Sub

        Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged
        Private Sub OnPropertyChanged(sender As Object, e As PropertyChangedEventArgs)
            RaiseEvent PropertyChanged(sender, e)
        End Sub

        Private Async Sub LoadImageAsynchronously(imageToLoad As Byte())
            Await Task.Run(Sub() LoadImage(imageToLoad))
        End Sub

        Private Sub LoadImage(imageToLoad As Byte())
            If imageToLoad Is Nothing Then Return

            Using stream = New MemoryStream(imageToLoad)
                Dim bitmapImage = New BitmapImage()

                stream.Seek(0, SeekOrigin.Begin)
                bitmapImage.BeginInit()
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad
                bitmapImage.StreamSource = stream
                bitmapImage.EndInit()
                bitmapImage.Freeze()
                Image = bitmapImage
            End Using
        End Sub

        Private Sub CreateActionViewModels(actionNames As IEnumerable(Of String))
            Actions = actionNames.Select(Function(x) New SkillActionViewModel(x)).ToList()
        End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            Image.CacheOption = BitmapCacheOption.None
            Image = Nothing
        End Sub
    End Class

End Namespace