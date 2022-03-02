Public Class UserDetailsControlTypeDescriptionProvider
    Inherits TypeDescriptionProvider

    Public Sub New()
        MyBase.New(TypeDescriptor.GetProvider(GetType(UserDetailsControl)))
    End Sub

    Public Overrides Function GetReflectionType(ByVal objectType As Type, ByVal instance As Object) As Type
        If objectType = GetType(UserDetailsControl) Then Return GetType(UserControl)
        Return MyBase.GetReflectionType(objectType, instance)
    End Function

    Public Overrides Function CreateInstance(ByVal provider As IServiceProvider, ByVal objectType As Type, ByVal argTypes As Type(), ByVal args As Object()) As Object
        If objectType = GetType(UserDetailsControl) Then objectType = GetType(UserControl)
        Return MyBase.CreateInstance(provider, objectType, argTypes, args)
    End Function
End Class

