Imports System.Drawing
Imports System.Windows.Forms
Imports BluePrism.BPCoreLib
Imports BluePrism.CharMatching.UI

''' <summary>
''' Contains parameters used to find a region within the application window. This is 
''' mapped from the parameters dictionary sent with the query.
''' </summary>
Public Class RegionLocationParams
    Private ReadOnly mCoordinates As Rectangle
    Private ReadOnly mLocationMethod As RegionLocationMethod
    Private ReadOnly mPosition As RegionPosition
    Private ReadOnly mPadding As Padding
    Private ReadOnly mImage As clsPixRect
    Private ReadOnly mParent As RegionLocationParams
    Private ReadOnly mColourTolerance As Integer
    Private ReadOnly mGreyscale As Boolean

    Sub New(coordinates As Rectangle, locationMethod As RegionLocationMethod, position As RegionPosition,
            padding As Padding, image As clsPixRect, parent As RegionLocationParams, tolerance As Integer,
            greyScale As Boolean)
        mCoordinates = coordinates
        mLocationMethod = locationMethod
        mPosition = position
        mPadding = padding
        mImage = image
        mParent = parent
        mColourTolerance = tolerance
        mGreyScale = greyScale
    End Sub

    ''' <summary>
    ''' A Rectangle representing the original area of the region in the model
    ''' </summary>
    Public ReadOnly Property Coordinates As Rectangle
        Get
            Return mCoordinates
        End Get
    End Property

    ''' <summary>
    ''' The method used to locate the region
    ''' </summary>
    Public ReadOnly Property LocationMethod As RegionLocationMethod
        Get
            Return mLocationMethod
        End Get
    End Property

    ''' <summary>
    ''' At what position the image will be located
    ''' </summary>
    Public ReadOnly Property Position As RegionPosition
        Get
            Return mPosition
        End Get
    End Property

    ''' <summary>
    ''' The padding used to extend the area searched when locating region using an image
    ''' </summary>
    Public ReadOnly Property Padding As Padding
        Get
            Return mPadding
        End Get
    End Property

    ''' <summary>
    ''' The image recorded in the model for the region
    ''' </summary>
    Public ReadOnly Property Image As clsPixRect
        Get
            Return mImage
        End Get
    End Property

    ''' <summary>
    ''' The relative region via which this region is located
    ''' </summary>
    Public ReadOnly Property Parent As RegionLocationParams
        Get
            Return mParent
        End Get
    End Property

    ''' <summary>
    ''' The tolerance that is allowed when comparing r, g and b values of image pixels.
    ''' </summary>
    Public ReadOnly Property ColourTolerance As Integer
        Get
            Return mColourTolerance
        End Get
    End Property

    Public ReadOnly Property Greyscale As Boolean
        Get
            Return mGreyscale
        End Get
    End Property
End Class