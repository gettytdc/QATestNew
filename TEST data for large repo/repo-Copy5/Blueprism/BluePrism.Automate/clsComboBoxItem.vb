''' -----------------------------------------------------------------------------
''' Project  : Automate
''' Class    : clsComboBoxItem
''' 
''' -----------------------------------------------------------------------------
''' <summary>
''' Designed for use with the custom class clsComboBoxWithTags - 
''' an enhancement of the existing combobox class in the framework.
''' </summary>
''' -----------------------------------------------------------------------------
Public Class clsComboBoxItem



    ''' <summary>
    ''' Private member holding the text to be displayed in the combo box.
    ''' </summary>
    Private msText As String

    ''' <summary>
    ''' Public property for managing the text to be displayed in a combobox
    ''' </summary>
    ''' <returns></returns>
    Public Property Text() As String
        Get
            Return msText
        End Get
        Set(ByVal Value As String)
            msText = Value
        End Set
    End Property


    ''' <summary>
    ''' Private member for holding the tag for this item.
    ''' </summary>
    Private mobjTag As Object


    ''' <summary>
    ''' Public property for managing the tag for this item.
    ''' </summary>
    ''' <returns></returns>
    Public Property ComboboxItemTag() As Object
        Get
            Return mobjTag
        End Get
        Set(ByVal Value As Object)
            mobjTag = Value
        End Set
    End Property

End Class
