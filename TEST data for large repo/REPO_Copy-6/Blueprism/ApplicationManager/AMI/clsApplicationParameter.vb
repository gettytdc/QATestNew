Imports System.Collections.Generic

Namespace BluePrism.ApplicationManager.AMI
    ''' <summary>
    ''' Class representing an application parameter. This is found as a member of
    ''' clsApplicationTypeInfo, where a list of these describes the parameters necessary
    ''' to start that type of application.
    ''' </summary>
    Public Class clsApplicationParameter

        ''' <summary>
        ''' The possible types of parameter that can be defined by this class.
        ''' </summary>
        Public Enum ParameterTypes
            [String]
            [List]
            [File]
            [Boolean]
            Number
            [Timespan]
        End Enum

        ''' <summary>The name of the parameter.</summary>
        Public Name As String



        ''' <summary>
        ''' The friendly name of the parameter shown to the user.
        ''' If no friendly name exists, falls back to name.
        ''' </summary>
        Public Property FriendlyName() As String
            Get
                Return If(Me.mFriendlyName <> Nothing, Me.mFriendlyName, Me.Name)
            End Get
            Set(ByVal value As String)
                Me.mFriendlyName = value
            End Set
        End Property
        Private mFriendlyName As String

        Public Property CheckFileExists As Boolean = True

        ''' <summary>Help text for parameter.</summary>
        Public HelpText As String

        ''' <summary>The type of the parameter.</summary>
        Public ParameterType As ParameterTypes

        ''' <summary>
        ''' The possible values, if the ParameterType is ParameterTypes.List. Note that
        ''' if FriendlyValues isn't Nothing, then for the purposes of display to the user
        ''' the corresponding entry there should be used instead.
        ''' </summary>
        Public Values As List(Of String) = Nothing

        ''' <summary>
        ''' Friendly names for the values, if the ParameterType is ParameterTypes.List,
        ''' or Nothing if the friendly names are the same as the values. If defined, it
        ''' must have the same number of entries as Values.
        ''' </summary>
        Public FriendlyValues As List(Of String) = Nothing

        ''' <summary>
        ''' The value, if this is being used to pass one. Also the default value. This is
        ''' always a String representation of the value, which for Boolean types is
        ''' either "True" or "False". For a List-type parameter, it must be one of the
        ''' values from Values.
        ''' </summary>
        Public Value As String

        ''' <summary>
        ''' The value that is used for applications created before this parameter 
        ''' existed. This is always a String representation of the value, which for 
        ''' Boolean types is either "True" or "False". For a List-type parameter, it must
        ''' be one of the values from Values. If this is not set, then Value will be used
        ''' instead.
        ''' </summary>
        Public LegacyValue As String

        ''' <summary>The file extension filter to use, when the type is File.
        ''' See System.Windows.Forms.OpenFileDialog.Filter documentation for
        ''' string format.</summary>
        Public FileExtensionFilter As String

        ''' <summary>
        ''' Determines whether a null value is acceptable. E.g. if type is file then it
        ''' is likely that the file is definitely required.
        ''' </summary>
        Public AcceptNullValue As Boolean

        ''' <summary>
        ''' Reference to a help file. This should be a file name only - not a full path.
        ''' </summary>
        Public HelpReference As String

        ''' <summary>
        ''' Indicates whether the parameter should be available for editing. Defaults to
        ''' True.
        ''' </summary>
        ''' <remarks>Used to make the parameter unavailable due to license restrictions,
        ''' etc.</remarks>
        Public Property Enabled() As Boolean
            Get
                If UpdateEnabled IsNot Nothing Then
                    mEnabled = UpdateEnabled()
                End If
                Return mEnabled
            End Get
            Set(value As Boolean)
                mEnabled = value
            End Set
        End Property

        Private mEnabled As Boolean = True

        ''' <summary>
        ''' If set, this function is used to update the value of the Enabled property 
        ''' every time the get accessor of the Enabled property is called.
        ''' Useful if you require the enabled value to be dependent on the value of 
        ''' something else.
        ''' </summary>
        ''' <remarks></remarks>
        Public UpdateEnabled As Func(Of Boolean) = Nothing


        ''' <summary>
        ''' Gets the datatype equivalent of the type of this parameter. For boolean
        ''' parameters, "flag"; for numeric parameter, "number"; for everything else,
        ''' "text".
        ''' </summary>
        Public ReadOnly Property DataTypeEquivalent() As String
            Get
                Select Case ParameterType
                    Case ParameterTypes.Boolean : Return "flag"
                    Case ParameterTypes.Number : Return "number"
                    Case Else : Return "text"
                End Select
            End Get
        End Property

        ''' <summary>
        ''' Returns a shallow (?) clone.
        ''' </summary>
        ''' <returns>As summary.</returns>
        Public Function Clone() As clsApplicationParameter
            Dim copy As New clsApplicationParameter()
            copy.Name = Name
            copy.FriendlyName = mFriendlyName
            copy.HelpText = HelpText
            copy.ParameterType = ParameterType
            If Values IsNot Nothing Then
                copy.Values = New List(Of String)
                copy.Values.AddRange(Values)
            End If
            If FriendlyValues IsNot Nothing Then
                copy.FriendlyValues = New List(Of String)
                copy.FriendlyValues.AddRange(FriendlyValues)
            End If
            copy.Value = Value
            copy.FileExtensionFilter = FileExtensionFilter
            copy.AcceptNullValue = AcceptNullValue
            Return copy
        End Function

    End Class
End Namespace
