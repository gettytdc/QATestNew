Imports System.Globalization
Imports System.Runtime.CompilerServices
Imports BluePrism.BPCoreLib
Imports BluePrism.BPCoreLib.Data

Namespace Groups

    Public Module Extensions

        ''' <summary>
        ''' Gets the group member attribute for a group member type.
        ''' </summary>
        ''' <param name="tp">The type for which the attribute is required</param>
        ''' <returns>The GroupMemberAttribute associated with the given member type,
        ''' or null if there is no such attribute.</returns>
        <Extension>
        Public Function GetMemberAttribute(tp As GroupMemberType) _
         As GroupMemberAttribute
            Return If(
                BPUtil.GetAttributeValue(Of GroupMemberAttribute)(tp),
                GroupMemberAttribute.Empty)
        End Function

        ''' <summary>
        ''' Gets the name of the given group member type
        ''' </summary>
        ''' <param name="tp">The type for which the name is required</param>
        ''' <returns>The name of the type, or an empty string if no group member
        ''' attribute is associated with the given type, or if it has no name.
        ''' </returns>
        <Extension>
        Public Function GetName(tp As GroupMemberType) As String
            Return GetMemberAttribute(tp).Name
        End Function

        ''' <summary>
        ''' Gets the localized friendly name for attribute according To the current culture.
        ''' The resources are created from GroupMemberType.vb
        ''' </summary>
        ''' <param name="tp">The type for which the name is required</param>
        ''' <returns>The localised attribute string for the current culture</returns>
        <Extension>
        Public Function GetLocalizedFriendlyName(tp As GroupMemberType, Optional toLowerCase As Boolean = False) As String
            Dim Attribute As String = tp.GetName()
            Dim resxKey = "GroupMemberTypeAttribute_" & Attribute
            Dim res As String = My.Resources.ResourceManager.GetString($"{resxKey}")
            Dim translatedString = CStr(IIf(res Is Nothing, Attribute, res))

            If toLowerCase Then
                Dim currentCulture = CultureInfo.CurrentCulture.Name.ToUpper
                If currentCulture <> "DE-DE" Then Return translatedString.ToLower
            End If

            Return translatedString

        End Function


        ''' <summary>
        ''' Gets the name of the view which holds the group linking data for the
        ''' given group member type
        ''' </summary>
        ''' <param name="tp">The type for which the view name is required</param>
        ''' <returns>The name of the view which holds the data linking the groups and
        ''' the member types, or an empty string if no group member attribute is
        ''' associated with the given type, or if it has no name.</returns>
        <Extension>
        Public Function GetViewName(tp As GroupMemberType) As String
            Return GetMemberAttribute(tp).ViewName
        End Function

        ''' <summary>
        ''' Creates a new group member of the given type if available.
        ''' </summary>
        ''' <param name="memberType">The member type for which a new member is required.
        ''' </param>
        ''' <param name="dataProvider">The data provider used to initialise the new member.
        ''' </param>
        ''' <returns>A new member object, initialised with the given ID and name, or
        ''' null if the member type has no class associated with it, or if any errors
        ''' occurred while attempting to create the new object.</returns>
        <Extension>
        Public Function CreateNew(memberType As GroupMemberType, dataProvider As IDataProvider) _
         As GroupMember
            Try
                Dim memberAttribute As GroupMemberAttribute = memberType.GetMemberAttribute()
                If memberAttribute?.MemberType Is Nothing
                    Return Nothing
                Else
                    Return memberAttribute.CreateMember(dataProvider)
                End If

            Catch ex As Exception
                Debug.Fail(
                    $"Failed to create new member for type: {memberType}: {ex.GetType().Name} - {ex.Message}",
                    ex.ToString())

                Return Nothing

            End Try
        End Function

        ''' <summary>
        ''' Gets the group member attribute for a group member type.
        ''' </summary>
        ''' <param name="tp">The type for which the attribute is required</param>
        ''' <returns>The GroupMemberAttribute associated with the given member type,
        ''' or an empty attribute if there is no such attribute associated with the
        ''' type</returns>
        <Extension>
        Public Function GetTreeDefinition(tp As GroupTreeType) _
         As TreeDefinitionAttribute
            Return If(
                BPUtil.GetAttributeValue(Of TreeDefinitionAttribute)(tp),
                TreeDefinitionAttribute.Empty)
        End Function


    End Module

End Namespace
