Imports System.ComponentModel
Imports System.Drawing
Imports System.Globalization
Imports System.Linq
Imports System.Windows.Forms
Imports BluePrism.ApplicationManager.ApplicationManagerUtilities
Imports BluePrism.BPCoreLib
Imports BluePrism.CharMatching.UI
Imports BluePrism.Server.Domain.Models

Imports ParameterNames =
    BluePrism.ApplicationManager.ApplicationManagerUtilities.clsQuery.ParameterNames

Public Class RegionLocationParamsMapper


    ''' <summary>
    ''' Creates RegionLocationParams object based on parameters in the query
    ''' </summary>
    ''' <param name="query">The query instance</param>
    ''' <returns>A RegionLocationParams object initialised with values from the query</returns>
    Public Shared Function FromQuery(query As clsQuery) As RegionLocationParams
        Return CreateParams(query, -1)
    End Function

    ''' <summary>
    ''' Creates instance from query parameters
    ''' </summary>
    ''' <param name="query">The query instance</param>
    ''' <param name="level">Indicates level of relative parent region -1 for root</param>
    Private Shared Function CreateParams(query As clsQuery, level As Integer) As RegionLocationParams

        ' Get prefix for relativeparent - exit if no parameters exist for current level
        Dim namePrefix As String
        If level >= 0 Then
            namePrefix = My.Resources.Relativeparent_ & level.ToString(CultureInfo.InvariantCulture) & "_"
            Dim parametersExistForLevel = query.Parameters.Any(Function(p) p.Key.StartsWith(namePrefix))
            If Not parametersExistForLevel = True Then
                Return Nothing
            End If
        Else
            namePrefix = ""
        End If

        ' Ancestors initialised from top down by calling function recursively
        Dim parent = CreateParams(query, level + 1)

        Dim coordinates As RECT
        Try
            Dim startX = CInt(query.Parameters(namePrefix & ParameterNames.StartX))
            Dim endX = CInt(query.Parameters(namePrefix & ParameterNames.EndX))
            Dim startY = CInt(query.Parameters(namePrefix & ParameterNames.StartY))
            Dim endY = CInt(query.Parameters(namePrefix & ParameterNames.EndY))
            Dim width = endX - startX + 1
            Dim height = endY - startY + 1
            coordinates = New Rectangle(startX, startY, width, height)
        Catch ex As Exception
            Throw New InvalidFormatException(
                My.Resources.FailedToParseValuesForRegionCoordinates0, ex)
        End Try

        Dim locationMethod = clsEnum.Parse(
            query.Parameters(namePrefix & ParameterNames.LocationMethod),
            RegionLocationMethod.Coordinates)

        Dim position As RegionPosition = clsEnum.Parse(query.Parameters(namePrefix & ParameterNames.RegionPosition),
                                            RegionPosition.Fixed)
        Dim searchPadding As Padding
        Dim image As clsPixRect
        Dim colorTolerance As Integer = 0
        Dim greyScale As Boolean = False

        If locationMethod = RegionLocationMethod.Image Then

            searchPadding = DirectCast(TypeDescriptor.GetConverter(GetType(Padding)) _
                .ConvertFromInvariantString(query.Parameters(namePrefix & ParameterNames.ImageSearchPadding)), Padding)
            image = EnsureRegionImage(query, namePrefix)
            colorTolerance = query.GetIntParam(namePrefix & ParameterNames.ColourTolerance)
            greyScale = query.GetBoolParam(namePrefix & ParameterNames.Greyscale)
        Else
            image = Nothing
        End If

        Return New RegionLocationParams(coordinates, locationMethod, position, searchPadding, image, parent, colorTolerance, greyScale)
    End Function


    ''' <summary>
    ''' Gets ImageValue or ElementSnapshot parameter converted to an image, throwing an
    ''' exception if the parameter is not found
    ''' </summary>
    Private Shared Function EnsureRegionImage(query As clsQuery, namePrefix As String) As clsPixRect

        Dim img = If(query.GetImageParam(namePrefix & ParameterNames.ImageValue, True),
                     query.GetImageParam(namePrefix & ParameterNames.ElementSnapshot, True))
        If img Is Nothing Then Throw New BluePrismException(
            String.Format(My.Resources.AnElementsnapshotOrImagevalueMustBeProvidedToMatchAgainstButCouldNotBeFoundForP, namePrefix))
        Return img

    End Function
End Class
