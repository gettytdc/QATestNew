Imports BluePrism.AMI
Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports BluePrism.CharMatching.UI

Public Class clsRegionConvert

    Private Shared ReadOnly PaddingDescriptor As TypeConverter = TypeDescriptor.GetConverter(GetType(Padding))

    ''' <summary>
    ''' Converts the given application region to a spy region.
    ''' </summary>
    ''' <param name="cont">The container on which the application region resides.
    ''' </param>
    ''' <param name="appReg">The application region to convert</param>
    ''' <returns>The spy region which represents the given application region.
    ''' </returns>
    Public Shared Function ToSpyRegion(
     ByVal cont As ISpyRegionContainer, ByVal appReg As clsApplicationRegion) _
     As SpyRegion
        Dim r As Rectangle = appReg.Rectangle
        Dim reg As SpyRegion
        If appReg.Type Is clsAMI.GetElementTypeInfo("Win32ListRegion") Then
            Dim listReg As New ListSpyRegion(cont, r, appReg.Name)
            listReg.Padding = CInt(appReg.GetValue("Padding"))

            Dim dirn As ListDirection = Nothing
            clsEnum.TryParse(CStr(appReg.GetValue("ListDirection")), dirn)
            listReg.ListDirection = dirn

            reg = listReg

        ElseIf appReg.Type Is clsAMI.GetElementTypeInfo("Win32GridRegion") Then
            Dim gridReg As New GridSpyRegion(cont, r, appReg.Name)
            gridReg.Schema =
             New GridSpyRegionSchema(CStr(appReg.GetValue("GridSchema")))

            reg = gridReg

        Else
            reg = New SpyRegion(cont, r, appReg.Name)

        End If
        reg.Id = appReg.ID
        reg.RetainImage = CBool(appReg.GetValue("RetainImage"))
        reg.FontName = CStr(appReg.GetValue("FontName"))
        Dim locationMethod As RegionLocationMethod = Nothing
        clsEnum.TryParse(CStr(appReg.GetValue("LocationMethod")), locationMethod)
        reg.LocationMethod = locationMethod
        Dim position As RegionPosition = Nothing
        clsEnum.TryParse(CStr(appReg.GetValue("RegionPosition")), position)
        reg.RegionPosition = position
        Integer.TryParse(CStr(appReg.GetValue("ColourTolerance")), reg.ColourTolerance)

        Boolean.TryParse(CStr(appReg.GetValue("Greyscale")), reg.Greyscale)

        Dim paddingValue = appReg.GetValue("ImageSearchPadding")
        If Not paddingValue Is Nothing Then
            reg.ImageSearchPadding = CType(
                PaddingDescriptor.ConvertFromInvariantString(CStr(paddingValue)), Padding)
        End If

        ' Save the application region element in the tag. We use this to retrieve
        ' other attributes (and their comparison types) when converting the other
        ' way around
        reg.Tag = appReg

        Return reg

    End Function

    ''' <summary>
    ''' Converts the given collection of application regions to a collection of
    ''' SpyRegions.
    ''' </summary>
    ''' <param name="cont">The container on which the application region resides.
    ''' </param>
    ''' <param name="appRegs">The application regions to convert</param>
    ''' <returns>The spy regions which represent the given application regions.
    ''' </returns>
    Public Shared Function ToSpyRegions(ByVal cont As ISpyRegionContainer,
     ByVal appRegs As ICollection(Of clsApplicationRegion)) _
     As ICollection(Of SpyRegion)
        Dim spyRegions As New List(Of SpyRegion)
        For Each reg As clsApplicationRegion In appRegs
            spyRegions.Add(ToSpyRegion(cont, reg))
        Next

        'Only once the spy regions have been created can we loop back through the app 
        'regions and set relative parents of the new spy regions (and the relative parents
        'of those relative parents and so on...)
        For Each a In appRegs
            SetRelativeParentRegions(spyRegions, a, appRegs)
        Next

        Return spyRegions

    End Function

    ''' <summary>
    ''' Recursively set the Relative Parent properties for Spy Regions from its
    ''' app region value. This will set the spy region's relative parent property, and
    ''' the relative parent property of its relative parent, and the relative parent 
    ''' property of its relative parent's relative parent, .....etc. This method
    ''' can only be called once all of the spy regions have been converted from their
    ''' app region equivalents, as the relative parent property cannot be set otherwise.
    ''' </summary>
    ''' <param name="spyRegions">
    ''' The collection of Spy Regions to modify and set the relative parent property for.
    ''' </param>
    ''' <param name="appRegion">
    ''' The app region whose relative parent property is being converted into the
    ''' equivalent spy region property.
    ''' </param>
    ''' <param name="appRegions">
    ''' The full collection of app regions
    ''' </param>
    Private Shared Sub SetRelativeParentRegions(ByRef spyRegions As List(Of SpyRegion),
                                          ByVal appRegion As clsApplicationRegion,
                                          ByVal appRegions As ICollection(Of clsApplicationRegion))
        'Find whether the app region has a relative region set
        Dim relativeParentID = CType(appRegion.GetValue("RelativeParentID"), Guid)
        If relativeParentID <> Guid.Empty Then
            'Look through the spy regions, and find that relative parent
            Dim relativeParentSpyRegion = spyRegions.FirstOrDefault(Function(x) x.Id = relativeParentID)

            'This can then be used to set the relative parent property of the spy region
            'that is related to it
            spyRegions.FirstOrDefault(Function(x) x.Id = appRegion.ID).RelativeParent = relativeParentSpyRegion

            'Then go and retrieve the relative parent app region, of the original app region
            Dim relativeParentAppRegion = appRegions.FirstOrDefault(Function(x) x.ID = relativeParentID)

            'Set the relative parents for the relative parent
            SetRelativeParentRegions(spyRegions, relativeParentAppRegion, appRegions)
        End If
    End Sub

    ''' <summary>
    ''' Converts a spy region to an app region, first recursively checking for
    ''' and converting any relative parent regions so that the app region is
    ''' created before it is set as a relative parent.
    ''' </summary>
    ''' <param name="reg">the spy region to convert</param>
    ''' <param name="cont">the container in which the region exists</param>
    ''' <param name="screenshot">The screenshot from which the region was drawn</param>
    ''' <param name="children">A list of child elements</param>
    ''' <param name="processed">A list of elements already processed</param>
    ''' <returns>The app region created</returns>
    Public Shared Function ConvertToAppRegion(ByVal reg As SpyRegion,
                                        ByVal cont As clsRegionContainer,
                                        ByVal screenshot As Bitmap,
                                        ByVal children As ICollection(Of clsApplicationMember),
                                        ByVal processed As ICollection(Of SpyRegion)) _
                                    As clsApplicationRegion

        If reg.RelativeParent IsNot Nothing Then

            ' if we already have the parent's Id then use that
            Dim appRegionParent = children.FirstOrDefault(Function(x) x.Name = reg.RelativeParent.Name)

            ' ' otherwise we need to convert the region to an app region
            If (appRegionParent Is Nothing) Then
                appRegionParent = ConvertToAppRegion(reg.RelativeParent, cont, screenshot, children, processed)
            End If

            reg.RelativeParent.Id = appRegionParent.ID
        End If

        ' Convert it to an application region in the given container
        ' (Note it's added to the region container's collection of
        ' hosted regions as part of the conversion)
        Dim appReg As clsApplicationRegion = ToAppRegion(reg, cont, screenshot)

        ' It's not added as a child member, however, so do that now
        If Not cont.ChildMembers.Contains(appReg) Then
            cont.AddMember(appReg)
        End If

        ' And store it in our list to update the app explorer with
        children.Add(appReg)
        processed.Add(reg)

        Return appReg
    End Function

    ''' <summary>
    ''' Converts the given spy region to an application region residing on the given
    ''' container, using the specified screenshot.
    ''' </summary>
    ''' <param name="reg">The spy region to convert</param>
    ''' <param name="cont">The container on which the region will reside (may already
    ''' reside if the region is already represented there)</param>
    ''' <param name="screenshot">The screenshot from which the region was drawn
    ''' </param>
    ''' <returns>An ApplicationRegion object representing the spy region.</returns>
    Private Shared Function ToAppRegion(ByVal reg As SpyRegion,
     ByVal cont As clsRegionContainer, ByVal screenshot As Bitmap) As clsApplicationRegion

        Dim listReg As ListSpyRegion = TryCast(reg, ListSpyRegion)
        Dim gridReg As GridSpyRegion = TryCast(reg, GridSpyRegion)

        ' Use the old region if it's there - ie. maintain the attributes set within
        Dim el As clsApplicationRegion = TryCast(reg.Tag, clsApplicationRegion)

        ' Create the new region object to represent the region if it's not there;
        ' if it is, ensure that the name and container ID are accurate
        If el IsNot Nothing Then
            el.Name = reg.Name
            el.ContainerId = cont.ID

        Else
            el = New clsApplicationRegion(reg.Name, cont.ID)
            ' Give it the ID from the region, if there is one.
            If reg.Id <> Nothing Then el.ID = reg.Id
            ' Add the (Win32) identifiers from the parent element
            el.AllIdentifiers = cont.AllIdentifiers
            ' Remove the screenshot element - that's de trop
            el.Attributes.Remove(el.GetAttribute(clsAMI.ScreenshotIdentifierId))

        End If

        Dim typeName As String
        Select Case True
            Case listReg IsNot Nothing : typeName = "Win32ListRegion"
            Case gridReg IsNot Nothing : typeName = "Win32GridRegion"
            Case Else : typeName = "WindowRect"
        End Select

        el.BaseType = clsAMI.GetElementTypeInfo(typeName)
        el.Type = el.BaseType
        clsProcessDataTypes.TryParse(el.Type.DefaultDataType, el.DataType)

        ' Add the attributes into a temporary list - we'll later merge this with
        ' the existing attributes in the element.
        Dim attrsToAdd As New List(Of clsApplicationAttribute)
        Dim attrNamesToRemove As New List(Of String)
        With attrsToAdd
            ' Add the region attributes
            Dim r As Rectangle = reg.Rectangle
            .Add(New clsApplicationAttribute("StartX", r.X, True))
            .Add(New clsApplicationAttribute("StartY", r.Y, True))
            .Add(New clsApplicationAttribute("EndX", r.Right, True))
            .Add(New clsApplicationAttribute("EndY", r.Bottom, True))
            .Add(New clsApplicationAttribute(
             "RetainImage", reg.RetainImage, False, True))

            ' If the region is set to retain the image, save the region
            ' from the snapshotted image in the parent element.
            Dim bmp As Bitmap = Nothing
            ' Feather the region - the image capture will be treating the region
            ' as inclusive; if we want to be able to match against a retained image,
            ' the captured image must also be treated as inclusive.
            If reg.RetainImage Then bmp = DirectCast(reg.Feathered.Image, Bitmap)

            .Add(New clsApplicationAttribute("ElementSnapshot", bmp, True, True))
            .Add(New clsApplicationAttribute("FontName", reg.FontName, True, True))
            .Add(New clsApplicationAttribute("LocationMethod", reg.LocationMethod.ToString(), True, True))
            .Add(New clsApplicationAttribute("RegionPosition", reg.RegionPosition.ToString(), True, True))
            .Add(New clsApplicationAttribute("ImageSearchPadding",
                PaddingDescriptor.ConvertToInvariantString(reg.ImageSearchPadding), True, True))
            If reg.RegionPosition = RegionPosition.Relative AndAlso reg.RelativeParent IsNot Nothing Then
                .Add(New clsApplicationAttribute("RelativeParentID", reg.RelativeParent.Id, True, True))
            Else
                attrNamesToRemove.Add("RelativeParentID")
            End If

            .Add(New clsApplicationAttribute("ColourTolerance", reg.ColourTolerance, True, True))
            .Add(New clsApplicationAttribute("Greyscale", reg.Greyscale, True, True))

            ' I happen to know that clsProcessValue doesn't actually keep
            ' the bitmap after it initialises its own value, so we can
            ' dispose of it
            If bmp IsNot Nothing Then bmp.Dispose()

            If listReg IsNot Nothing Then
                .Add(New clsApplicationAttribute(
                 "Padding", listReg.Padding, True, True))

                .Add(New clsApplicationAttribute(
                 "ListDirection", listReg.ListDirection.ToString(), True, True))

            ElseIf gridReg IsNot Nothing Then
                .Add(New clsApplicationAttribute(
                 "GridSchema", gridReg.Schema.EncodedValue, True, True))

            End If

        End With

        ' Replace any attributes being added if they already exist on the element.
        ' Ensure we keep the comparison types and dynamic states of existing attrs
        For Each attr As clsApplicationAttribute In attrsToAdd
            Dim existing As clsApplicationAttribute = el.GetAttribute(attr.Name)
            If existing IsNot Nothing Then
                ' We copy the comparison type and its dynamic state, but the value
                ' and 'InUse' state we take from the region / defaults.
                attr.ComparisonType = existing.ComparisonType
                attr.Dynamic = existing.Dynamic
                ' Remove the old one from the element
                el.Attributes.Remove(existing)
            End If
            ' Add the new attribute
            el.Attributes.Add(attr)
        Next
        For Each attrName As String In attrNamesToRemove
            Dim attrToRemove = el.GetAttribute(attrName)
            If attrToRemove IsNot Nothing Then
                el.Attributes.Remove(attrToRemove)
            End If
        Next

        ' Add the element to the container's registered regions
        cont.Regions.Add(el)
        Return el

    End Function

End Class
