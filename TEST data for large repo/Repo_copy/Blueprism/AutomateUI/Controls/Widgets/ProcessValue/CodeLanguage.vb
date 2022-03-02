Imports BluePrism.BPCoreLib

''' <summary>
''' Enumeration of the languages that are supported in various text controls in the
''' product (or not - this is a superset of those languages, so there may be some in
''' here which are not actually handled in the controls).
''' </summary>
''' <remarks>The <see cref="frmMultilineEdit"/> form uses the
''' <see cref="FriendlyNameAttribute"/> and <see cref="ScintillaNameAttribute"/> to
''' display these languages to the user, and to tell the scintilla component which
''' syntax highlighting method to use. If a value in this enum has no scintilla name,
''' it uses a basic textbox to display its value, otherwise it will use the scintilla
''' control initialised with the specified language name.</remarks>
Public Enum CodeLanguage
    <FriendlyName("Plain Text")>
    PlainText = 0

    <FriendlyName("Blue Prism Expression")> _
    Expression

    <FriendlyName("Javascript"), ScintillaName("js")> _
    Javascript

    <FriendlyName("SQL"), ScintillaName("mssql")> _
    Sql

    <FriendlyName("XML"), ScintillaName("xml")> _
    Xml

    <FriendlyName("C#"), ScintillaName("cs")> _
    CSharp

    <FriendlyName("Visual Basic"), ScintillaName("vb")> _
    VB

End Enum
