Imports BluePrism.AutomateAppCore.Auth

' This if debug block is here to allow the class to be used as concrete when in debug,
' so that the control can be rendered in the designer - without it you cannot view the designer
' at all for any form which inherits this control
#If DEBUG Then
<TypeDescriptionProvider(GetType(UserDetailsControlTypeDescriptionProvider))>
Friend Class UserDetailsControl : Inherits UserControl
    Public Overridable Function AllFieldsValid() As Boolean

    End Function
    Public Overridable Property User As User
End Class
#Else
<TypeDescriptionProvider(GetType(UserDetailsControlTypeDescriptionProvider))>
Friend MustInherit Class UserDetailsControl : Inherits UserControl
    Public MustOverride Function AllFieldsValid() As Boolean
    Public MustOverride Property User As User
End Class
#End If

