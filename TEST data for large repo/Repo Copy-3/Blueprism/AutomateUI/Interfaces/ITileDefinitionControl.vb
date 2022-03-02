Imports BluePrism.AutomateAppCore
''' <summary>
''' Interface describing tile definition controls.
''' </summary>
Friend Interface ITileDefinitionControl

    ''' <summary>
    ''' Method to pass the generic base tile to the control and trigger the loading 
    ''' any tiletype specific properties.
    ''' </summary>
    Sub LoadProperties(baseTile As Tile)

    ''' <summary>
    ''' Method to trigger the unloading of any tiletype specific properties from the
    ''' control back into the generic base tile.
    ''' </summary>
    Sub UnloadProperties()

    ''' <summary>
    ''' Method to request a preview of the tile.
    ''' </summary>
    ''' <returns>The tile UI Element</returns>
    Function BuildPreview(name As String, desc As String) As System.Windows.UIElement

    ''' <summary>
    ''' Method to return a formatted version of the tile's XML properties (for audit)
    ''' </summary>
    ''' <returns>Formatted properties</returns>
    Function FormatXMLProperties() As String
End Interface
