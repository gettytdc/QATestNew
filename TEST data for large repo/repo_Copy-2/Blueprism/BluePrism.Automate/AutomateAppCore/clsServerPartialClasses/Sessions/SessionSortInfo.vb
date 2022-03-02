Imports System.Runtime.Serialization
Imports System.Xml
Imports System.Xml.Linq

Namespace clsServerPartialClasses.Sessions
    <Serializable, DataContract(Name:="ssi", [Namespace]:="bp")>
    Public Class SessionSortInfo

        Private Const DefaultColumn As SessionManagementColumn = SessionManagementColumn.StartTime
        Private Const DefaultDirection As SortDirection = SortDirection.Descending

        <DataMember(Name:="c")>
        Private ReadOnly mColumn As SessionManagementColumn

        <DataMember(Name:="d")>
        Private ReadOnly mDirection As SortDirection

        Public ReadOnly Property Column As SessionManagementColumn
            Get
                Return mColumn
            End Get
        End Property

        Public ReadOnly Property Direction As SortDirection
            Get
                Return mDirection
            End Get
        End Property

        Public Enum SortDirection
            Ascending
            Descending
        End Enum

        Public Sub New(column As SessionManagementColumn, direction As SortDirection)
            mColumn = column
            mDirection = direction
        End Sub

        Public Shared Function GetDefaultSortInfo() As SessionSortInfo
            Return New SessionSortInfo(DefaultColumn, DefaultDirection)
        End Function

        Public Function ToXmlElement(doc As XmlDocument) As XmlElement
            Dim sortInfo = doc.CreateElement("sortinfo")
            sortInfo.SetAttribute("column", CInt(Column).ToString())
            sortInfo.SetAttribute("direction", Direction.ToString())
            Return sortInfo
        End Function

        Public Shared Function FromXmlElement(element As XmlElement) As SessionSortInfo
            Return FromXElement(XElement.Parse(element.OuterXml))
        End Function

        Private Shared Function FromXElement(element As XElement) As SessionSortInfo
            Dim columnValue = element.Attribute("column")?.Value
            Dim directionValue = element.Attribute("direction")?.Value

            Dim column As SessionManagementColumn
            If Not [Enum].TryParse(columnValue, column) Then column = DefaultColumn

            Dim direction As SortDirection
            If Not [Enum].TryParse(directionValue, direction) Then direction = DefaultDirection

            Return New SessionSortInfo(column, direction)

        End Function


    End Class
End Namespace
