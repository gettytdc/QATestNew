Imports System.Xml
Imports BluePrism.Core.Xml

Public Class clsValidationInfoImportExport

    Public Sub Export(ByVal filename As String)
        Dim rules = gSv.GetValidationInfo()
        Dim validationInfo = rules.ToDictionary(Of Integer, clsValidationInfo)(Function(y) y.CheckID, Function(y) y)
        Dim types As IDictionary(Of Integer, String) = gSv.GetValidationTypes()
        Dim actions As IDictionary(Of Integer, String) = gSv.GetValidationActions()

        Dim categories As New SortedDictionary(Of Integer, clsValidationInfo)
        For Each checkId As Integer In validationInfo.Keys
            Dim info As clsValidationInfo = validationInfo(checkId)
            categories(info.CatID) = info
        Next

        Dim doc As New XmlDocument
        Dim xRoot As XmlElement = doc.CreateElement("designcontrol")
        For Each catid As Integer In categories.Keys
            Dim xCategory As XmlElement = doc.CreateElement("category")
            xCategory.SetAttribute("id", catid.ToString)
            xCategory.SetAttribute("name", CType(catid, clsValidationInfo.Categories).ToString)

            For Each typeid As Integer In types.Keys
                Dim actionSettings As IDictionary(Of Integer, Integer) = gSv.GetValidationActionSettings(catid)
                Dim xType As XmlElement = doc.CreateElement("type")
                xType.SetAttribute("id", typeid.ToString)
                xType.SetAttribute("name", CType(typeid, clsValidationInfo.Types).ToString)
                Dim xAction As XmlElement = doc.CreateElement("action")
                Dim actionid As Integer = actionSettings(typeid)
                xAction.SetAttribute("id", actionid.ToString)
                xAction.SetAttribute("name", actions(actionid))
                xType.AppendChild(xAction)
                xCategory.AppendChild(xType)
            Next

            For Each checkId As Integer In validationInfo.Keys
                Dim info As clsValidationInfo = validationInfo(checkId)
                If info.CatID = catid Then
                    info.ToXML(doc, xCategory)
                End If
            Next

            xRoot.AppendChild(xCategory)
        Next
        doc.AppendChild(xRoot)
        doc.Save(filename)
    End Sub

    Public Sub Import(ByVal filename As String)
        Dim doc As New ReadableXmlDocument()
        doc.Load(filename)
        Dim xRoot As XmlElement = doc.DocumentElement
        Dim allInfo As New Dictionary(Of Integer, clsValidationInfo)
        If xRoot.Name = "designcontrol" Then
            For Each xCategory As XmlElement In xRoot.ChildNodes
                If xCategory.Name = "category" Then
                    Dim catid As Integer = Integer.Parse(xCategory.GetAttribute("id"))
                    For Each xChild As XmlElement In xCategory.ChildNodes
                        If xChild.Name = "type" Then
                            Dim typeid As Integer = Integer.Parse(xChild.GetAttribute("id"))
                            For Each xAction As XmlElement In xChild.ChildNodes
                                If xAction.Name = "action" Then
                                    Dim actionid As Integer = Integer.Parse(xAction.GetAttribute("id"))
                                    gSv.SetValidationActionSetting(catid, typeid, actionid)
                                End If
                            Next
                        ElseIf xChild.Name = "check" Then
                            Dim info As clsValidationInfo
                            info = clsValidationInfo.FromXML(xChild)
                            allInfo(info.CheckID) = info
                        Else
                            Throw New InvalidOperationException(String.Format(My.Resources.clsValidationInfoImportExport_DesignControlXMLHadUnexpectedElement0, xChild.Name))
                        End If
                    Next
                Else
                    Throw New InvalidOperationException(String.Format(My.Resources.clsValidationInfoImportExport_DesignControlXMLHadUnexpectedElement0, xCategory.Name))
                End If
            Next
        Else
            Throw New InvalidOperationException(String.Format(My.Resources.clsValidationInfoImportExport_DesignControlXMLHadUnexpectedRootElement0, xRoot.Name))
        End If

        gSv.SetValidationInfo(allInfo.Values)
    End Sub
End Class
