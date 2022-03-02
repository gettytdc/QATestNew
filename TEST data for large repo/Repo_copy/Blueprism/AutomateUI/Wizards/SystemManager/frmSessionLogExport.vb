Imports System.IO
Imports System.Xml
Imports System.Xml.Xsl
Imports System.Text.RegularExpressions
Imports System.Globalization
Imports BluePrism.AutomateAppCore
Imports BluePrism.Core.Xml
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Server.Domain.Models
Imports LocaleTools


Friend Class frmSessionLogExport



    ''' <summary>
    ''' The session number.
    ''' </summary>
    ''' <remarks></remarks>
    Private miSessionNumber As Integer
    Private miRowsPerPage As Integer
    Private mbTextMode As Boolean
    ''' <summary>
    ''' Compile time flags to disable localisation of column headers and row content.
    ''' </summary>
    ''' <remarks></remarks>
    Public Shared bLocalise As Boolean = True

    Private mDataTablesRequiringDisposing As List(Of DataTable) = New List(Of DataTable)()

    ''' <summary>
    ''' Constructor.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub New()
        MyBase.New()

        InitializeComponent()
        DefaultFileName = "BPASessionLog"

        Me.Text = Me.Text.Replace(My.Resources.frmSessionLogExport_Log, My.Resources.frmSessionLogExport_SessionLog)
        Me.Title = Me.Title.Replace(My.Resources.frmSessionLogExport_Log, My.Resources.frmSessionLogExport_SessionLog)

    End Sub

    ''' <summary>
    ''' Populates the Log Export Wizard
    ''' </summary>
    ''' <param name="bTextMode"></param>
    ''' <param name="iRowsPerPage"></param>
    ''' <param name="aLogDataTables">The log data pages</param>
    ''' <param name="iSessionNumber">The session number</param>
    ''' <param name="sTitle">The form title</param>
    ''' <remarks></remarks>
    Public Overloads Sub Populate(ByRef aLogDataTables As DataTable(), ByVal bTextMode As Boolean, ByVal iRowsPerPage As Integer, ByVal iSessionNumber As Integer, ByVal sTitle As String)
        maLogDataTables = aLogDataTables
        msTitle = sTitle
        mbTextMode = bTextMode
        miRowsPerPage = iRowsPerPage
        miSessionNumber = iSessionNumber
    End Sub

    Private Function ContainsCollectionsInParams(ByVal tables As ICollection(Of DataTable)) As Boolean
        For Each dt As DataTable In tables
            If dt Is Nothing Then Continue For ' Really? We can have null tables? Why?
            Dim col As DataColumn = dt.Columns("ParameterXml")
            If col IsNot Nothing Then
                For Each row As DataRow In dt.Rows
                    Dim px As String = TryCast(row(col), String)
                    If px IsNot Nothing AndAlso px.Contains("type=""collection""") Then Return True
                Next
            End If
        Next
        Return False
    End Function

    ''' <summary>
    ''' Gets the localized friendly name for column according to the current culture.
    ''' </summary>
    ''' <param name="columnName">The column name string</param>
    ''' <returns>The localised column string for the current culture</returns>
    Public Shared Function GetLocalizedFriendlyNameHeader(columnName As String) As String
        Dim resxKey As String = "HeaderCell_FileExport_" & Regex.Replace(columnName, "\s*", "")
        Dim localizedHeader As String = My.Resources.ResourceManager.GetString(resxKey)
        Return If(localizedHeader Is Nothing, columnName, localizedHeader)
    End Function

    ''' <summary>
    ''' Writes log data out to a file.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Overrides Sub ExportLog()
        'Fetch any missing pages from the DB
        For p As Integer = 0 To maLogDataTables.Length - 1
            If maLogDataTables(p) Is Nothing Then
                GetMissingLogPages()
                Exit For
            End If
        Next

        ' Raise an audit event to track who did what here
        If gSv.AuditRecordSysConfigEvent(SysConfEventCode.ExportSessionLogs,
                                         msTitle,
                                         miSessionNumber.ToString) Then
            MyBase.ExportLog()
        Else
            Throw New BluePrismException(My.Resources.frmSessionLogExport_FailedToCreateAuditEventForExportOfSessionLogs)
        End If
    End Sub

    ''' <summary>
    ''' Looks at each element in the array of log page data. 
    ''' Any found to be empty are populated from the DB.
    ''' </summary>
    ''' <remarks></remarks>
    Protected Sub GetMissingLogPages()

        Me.lblExportMessages.Text = My.Resources.frmSessionLogExport_RetrievingDatabaseData
        Application.DoEvents()

        Dim iCurrentRow As Integer = 1
        Dim dtLogPage As DataTable = Nothing
        Dim sErr As String = ""

        ResetProgressBar(maLogDataTables.Length)

        'Get the first populated element to find the visible columns.
        Dim dtExistingPage As DataTable = Nothing
        Dim aVisibleColumns As New Generic.List(Of String)

        If Not mbTextMode Then
            For Each dt As DataTable In maLogDataTables
                If dt IsNot Nothing Then
                    For Each c As DataColumn In dt.Columns
                        aVisibleColumns.Add(c.ColumnName)
                    Next
                    Exit For
                End If
            Next
        End If

        Dim aColumnsToRemove As New Generic.List(Of DataColumn)
        For p As Integer = 0 To maLogDataTables.Length - 1

            Me.lblExportMessages.Text = String.Format(My.Resources.frmSessionLogExport_RetrievingPage0Of1, CStr(p + 1), CStr(maLogDataTables.Length))
            Application.DoEvents()

            If maLogDataTables(p) Is Nothing Then

                If mbTextMode Then
                    dtLogPage = gSv.GetLogsAsText(miSessionNumber, iCurrentRow, miRowsPerPage)
                Else
                    dtLogPage = gSv.GetLogs(miSessionNumber, iCurrentRow, miRowsPerPage)
                    'GetLogs may return more columns than is necessary if the 
                    'user has opted to hide any, so make note of ones to discard.
                    For Each c As DataColumn In dtLogPage.Columns
                        If Not aVisibleColumns.Contains(c.ColumnName) Then
                            aColumnsToRemove.Add(c)
                        End If
                    Next
                    'Discard any unwanted columns.
                    For Each c As DataColumn In aColumnsToRemove
                        dtLogPage.Columns.Remove(c)
                    Next
                End If

                maLogDataTables(p) = dtLogPage
                mDataTablesRequiringDisposing.Add(dtLogPage)

            End If
            aColumnsToRemove = New Generic.List(Of DataColumn)
            iCurrentRow += miRowsPerPage
            IncrementProgressBar()
        Next

        Application.DoEvents()

    End Sub

    ''' <summary>
    ''' Appends the given string into the buffer a specified number of times
    ''' </summary>
    ''' <param name="output">The streamwriter into which the string should be appended.
    ''' </param>
    ''' <param name="str">The string to append</param>
    ''' <param name="times">The number of times the string should be appended.
    ''' </param>
    Private Sub AppendRepeated(
     ByVal output As StreamWriter, ByVal str As String, ByVal times As Integer)
        While times > 0
            output.Write(str)
            times -= 1
        End While
    End Sub


    ''' <summary>
    ''' Gets the HTML value for the given object. If it is empty, this will return
    ''' a HTML non-break-space entity, otherwise, it will return the given value as
    ''' a string.
    ''' </summary>
    ''' <param name="val">The value for which the HTML value is required.</param>
    ''' <returns>The HTML value of the given object, or "&amp;nbsp;" if the object
    ''' was empty.</returns>
    Private Function GetHtmlValue(ByVal val As Object) As String
        If IsEmpty(val) Then Return "&nbsp;" Else Return val.ToString()
    End Function

    ''' <summary>
    ''' Initialises the Column Headers corresponding to ParameterXml.
    ''' </summary>
    ''' <param name="hasCollections">Indicates if parameter contains a collection.</param>
    ''' <returns>The localised Column Headers</returns>
    Private Function InitParamHeaders(ByVal hasCollections As Boolean) As ICollection(Of String)
        Dim paramColumns As ICollection(Of String)
        If hasCollections Then
            If bLocalise Then
                paramColumns = New String() {My.Resources.frmSessionLogExport_Direction, My.Resources.frmSessionLogExport_Name, My.Resources.frmSessionLogExport_Field, My.Resources.frmSessionLogExport_Row, My.Resources.frmSessionLogExport_Type, My.Resources.frmSessionLogExport_Value}
            Else
                paramColumns = New String() {"Direction", "Name", "Field", "Row", "Type", "Value"}
            End If
        Else
            If bLocalise Then
                paramColumns = New String() {My.Resources.frmSessionLogExport_Direction, My.Resources.frmSessionLogExport_Name, My.Resources.frmSessionLogExport_Type, My.Resources.frmSessionLogExport_Value}
            Else
                paramColumns = New String() {"Direction", "Name", "Type", "Value"}
            End If
        End If
        Return paramColumns
    End Function


    ''' <summary>
    ''' Writes the data into an HTML BODY statement.
    ''' </summary>
    ''' <param name="tables">The log data</param>
    ''' <param name="output">A streamwriter the data should be writtern into</param>
    Protected Overrides Sub WriteHTMLBody(ByVal tables As DataTable(), ByVal output As StreamWriter)

        If tables Is Nothing OrElse tables.Length = 0 Then
            Return
        End If

        Dim hasCollections As Boolean = ContainsCollectionsInParams(tables)
        Dim paramColumns As ICollection(Of String) = InitParamHeaders(hasCollections)

        ResetProgressBar(tables.Length)

        'Use a transform to reshape the parameter XML into HTML.
        Dim sbXSLT As New StringBuilder
        Dim sbInputXSLT As New StringBuilder

        sbXSLT.Append("<?xml version=""1.0"" encoding=""UTF-8"" ?>")
        sbXSLT.Append("<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" xmlns:ext=""urn:XsltFunctions"">")
        sbXSLT.Append("<xsl:param name=""objectname""></xsl:param>")
        sbXSLT.Append("<xsl:template match=""/"">")

        sbInputXSLT.Append("<xsl:for-each select=""parameters/inputs/input"">")
        sbInputXSLT.Append(" <TR>")
        sbInputXSLT.Append("  <TD>IN</TD>")
        sbInputXSLT.Append("  <TD><xsl:value-of select=""ext:ChooseLocalizedFriendlyName(@name, $objectname)""/></TD>")
        If hasCollections Then
            sbInputXSLT.Append("  <TD></TD>")
            sbInputXSLT.Append("  <TD></TD>")
        End If
        sbInputXSLT.Append("    <TD><xsl:value-of select=""ext:GetLocalizedFriendlyNameDataType(@type)""/></TD>")
        sbInputXSLT.Append("  <xsl:choose>")
        sbInputXSLT.Append("   <xsl:when test=""@type='collection'"">")
        sbInputXSLT.Append("    <TD></TD>")
        sbInputXSLT.Append("   </xsl:when>")
        sbInputXSLT.Append("   <xsl:otherwise>")
        sbInputXSLT.Append("    <TD><xsl:value-of select=""@value""/></TD>")
        sbInputXSLT.Append("   </xsl:otherwise>")
        sbInputXSLT.Append("  </xsl:choose>")
        sbInputXSLT.Append(" </TR>")
        sbInputXSLT.Append("</xsl:for-each>")

        sbInputXSLT.Append("<xsl:for-each select=""parameters/inputs/input/row"">")
        sbInputXSLT.Append("  <xsl:variable name=""rownumber"" select=""position()""/>")
        sbInputXSLT.Append("  <xsl:for-each select=""field"">")
        sbInputXSLT.Append("    <TR>")
        sbInputXSLT.Append("      <TD>IN</TD>")
        sbInputXSLT.Append("      <TD>")
        sbInputXSLT.Append("         <xsl:value-of select=""ext:ChooseLocalizedFriendlyName(../../@name, $objectname)""/>")
        sbInputXSLT.Append("      </TD>")
        sbInputXSLT.Append("      <TD>")
        sbInputXSLT.Append("        <xsl:value-of select=""@name""/>")
        sbInputXSLT.Append("      </TD>")
        sbInputXSLT.Append("      <TD>")
        sbInputXSLT.Append("        <xsl:value-of select=""$rownumber""/>")
        sbInputXSLT.Append("      </TD>")
        sbInputXSLT.Append("      <TD>")
        sbInputXSLT.Append("        <xsl:value-of select=""ext:GetLocalizedFriendlyNameDataType(@type)""/>")
        sbInputXSLT.Append("      </TD>")
        sbInputXSLT.Append("      <TD>")
        sbInputXSLT.Append("        <xsl:value-of select=""@value""/>")
        sbInputXSLT.Append("      </TD>")
        sbInputXSLT.Append("    </TR>")
        sbInputXSLT.Append("  </xsl:for-each>")
        sbInputXSLT.Append("</xsl:for-each>")

        'Add the input transfrom text and then modify 
        'and reuse as the output text.
        If bLocalise Then
            sbInputXSLT.Replace("<TD>IN</TD>", "<TD>" & My.Resources.frmLogParameterViewer_IN & "</TD>")
        End If
        sbXSLT.AppendLine(sbInputXSLT.ToString())
        sbInputXSLT.Replace("/input", "/output")
        If bLocalise Then
            sbInputXSLT.Replace("<TD>" & My.Resources.frmLogParameterViewer_IN & "</TD>", "<TD>" & My.Resources.frmLogParameterViewer_OUT & "</TD>")
        Else
            sbInputXSLT.Replace("<TD>IN</TD>", "<TD>OUT</TD>")
        End If
        sbXSLT.AppendLine(sbInputXSLT.ToString())

        sbXSLT.Append("</xsl:template>")
        sbXSLT.Append("</xsl:stylesheet>")

        Dim xsltDoc As New ReadableXmlDocument(sbXSLT.ToString())

        Dim xslTransformer As XslCompiledTransform = New XslCompiledTransform()
        xslTransformer.Load(xsltDoc)

        output.WriteLine("<BODY>")
        output.WriteLine("<TABLE class=""AutomateTable"">")

        Dim emptyNonParamCells As String = ""

        Dim writtenHeader As Boolean = False

        For Each dt As DataTable In tables

            ' Get the parameter column if one is there
            Dim colParam As DataColumn = dt.Columns("ParameterXml")

            ' Also the log number column - used when writing the parameters.
            Dim colLogNo As DataColumn = dt.Columns("LogNumber")

            If Not writtenHeader Then

                output.WriteLine("<TR>")
                For Each col As DataColumn In dt.Columns
                    ' Leave the parameter column til the end
                    If col IsNot colParam Then
                        ' Make up a string of empty TDs to use to make parameter rows.
                        If col IsNot colLogNo Then emptyNonParamCells &= "<TD>&nbsp;</TD>"
                        ' Write the column header
                        output.Write("<TH>")
                        output.Write(IIf(bLocalise, GetLocalizedFriendlyNameHeader(col.ColumnName), col.ColumnName))
                        output.Write("</TH>")
                    End If
                Next
                If colParam IsNot Nothing Then
                    For Each col As String In paramColumns
                        output.Write("<TH>")
                        output.Write(col)
                        output.Write("</TH>")
                    Next
                End If
                output.WriteLine("</TR>")

                writtenHeader = True

            End If

            'Add a TR for every data row.
            For Each r As DataRow In dt.Rows
                output.WriteLine("<TR>")
                For Each col As DataColumn In dt.Columns
                    If col Is colParam Then Continue For ' Leave the parameters until last

                    'For all othe columns, add a TD.
                    output.Write("<TD>")
                    Dim sHtmlValue = GetHtmlValue(r(col))
                    If Not sHtmlValue.Equals("&nbsp;") Then
                        If col.ColumnName = frmLogViewer.ColumnNames.StageType Then
                            sHtmlValue = CStr(IIf(bLocalise, clsStageTypeName.GetLocalizedFriendlyName(sHtmlValue), sHtmlValue))
                        ElseIf col.ColumnName = frmLogViewer.ColumnNames.ResultType Then
                            sHtmlValue = CStr(IIf(bLocalise, clsProcessDataTypes.GetFriendlyName(sHtmlValue), sHtmlValue))
                        ElseIf col.ColumnName = frmLogViewer.ColumnNames.StageName Then
                            sHtmlValue = CStr(IIf(bLocalise, LTools.GetC(sHtmlValue, "misc", "stage"), sHtmlValue))
                        ElseIf col.ColumnName = frmLogViewer.ColumnNames.Action Then
                            Dim sObjectValue = GetHtmlValue(r(frmLogViewer.ColumnNames.Object))
                            sHtmlValue = CStr(IIf(bLocalise, clsBusinessObjectAction.GetLocalizedFriendlyName(sHtmlValue, sObjectValue, "Action"), sHtmlValue))
                        ElseIf col.ColumnName = frmLogViewer.ColumnNames.Result Then
                            If sHtmlValue.StartsWith("ERROR: ") Then
                                sHtmlValue = sHtmlValue.Replace("ERROR: ", My.Resources.LogError)
                            End If
                        End If
                    End If
                    output.Write(sHtmlValue)
                    output.Write("</TD>")

                Next

                ' If there are no parameters, just end the row
                If colParam Is Nothing Then
                    output.Write("</TR>")
                Else
                    ' Regardless of whether we have any params in this row or not,
                    ' finish off the current line without them (ie. with blank cells)
                    AppendRepeated(output, "<TD>&nbsp;</TD>", paramColumns.Count)
                    output.Write("</TR>")

                    ' Now, if we have params, print them out in subsequent rows
                    If Not IsEmpty(r(colParam)) Then

                        output.WriteLine()

                        ' Load the XML and transform it into a HTML fragment in an 
                        ' unnecessarily complicated manner.
                        Dim paramXml As New ReadableXmlDocument(r(colParam).ToString())

                        Dim xslArg As XsltArgumentList = New XsltArgumentList
                        xslArg.AddExtensionObject("urn:XsltFunctions", New clsMyXsltExtensionFunctions(bLocalise))
                        If r.ItemArray.Contains(frmLogViewer.ColumnNames.Object) Then _
                            xslArg.AddParam("objectname", "", r(frmLogViewer.ColumnNames.Object).ToString())
                        Using sw As New StringWriter(CultureInfo.InvariantCulture)
                            Using tw As New XmlTextWriter(sw)
                                xslTransformer.Transform(paramXml, xslArg, tw)
                                tw.Flush()
                            End Using
                            Dim params As String = sw.ToString()

                            ' Prepend the parameter row with a log number cell (if that column is
                            ' visible) 'and additional blank cells to cover the data columns.
                            If colLogNo IsNot Nothing Then
                                params = params.Replace("<TR xmlns:ext=""urn:XsltFunctions"">",
                                 String.Format("<TR><TD>{0}</TD>{1}", r(colLogNo), emptyNonParamCells))
                            Else
                                params = params.Replace("<TR>", "<TR>" & emptyNonParamCells)
                            End If
                            output.WriteLine(params)

                        End Using
                    End If

                End If

            Next

            IncrementProgressBar()
        Next
        output.WriteLine("</TABLE>")
        output.WriteLine("</BODY>")

    End Sub

    ''' <summary>
    ''' Checks if the given value object is empty.
    ''' </summary>
    ''' <param name="val">The value to check</param>
    ''' <returns>True if the given value is null or empty, False otherwise.</returns>
    Private Function IsEmpty(ByVal val As Object) As Boolean
        Return (val Is Nothing OrElse val Is DBNull.Value OrElse val.ToString() = "")
    End Function

    ''' <summary>
    ''' Escapes the given string value for use in a CSV field.
    ''' </summary>
    ''' <param name="str">The string to escape</param>
    ''' <returns>The escaped string with all quote characters escaped as appropriate.
    ''' </returns>
    Private Function Escape(ByVal str As String) As String
        Return str.Replace("""", """""")
    End Function

    ''' <summary>
    ''' Appends the given value into the string buffer, prepended by the separator
    ''' string. After appending it, the separator is replaced with a comma.
    ''' </summary>
    ''' <param name="val">The value to append, if null or empty, no text is appended
    ''' except the separator string</param>
    ''' <param name="output">The writer the value should be appended.</param>
    ''' <param name="sep">The separator string. After appending the given value
    ''' prepended with this string, it is replaced with a single comma character.
    ''' </param>
    Private Sub AppendValueInto(ByVal val As Object, ByVal output As StreamWriter, ByRef sep As String)

        If IsEmpty(val) Then
            output.Write(sep)
        Else
            output.Write(sep)
            output.Write(""""c)
            output.Write(Escape(val.ToString()))
            output.Write(""""c)
        End If
        ' Update the separator so that the next value is prepended with a comma
        sep = ","
    End Sub

    ''' <summary>
    ''' Writes the data into a CSV file.
    ''' </summary>
    ''' <param name="tables">The log data</param>
    ''' <param name="output">A StreamWriter to write the data into</param>
    Protected Overrides Sub WriteCSV(ByVal tables As DataTable(), ByVal output As StreamWriter)

        If tables Is Nothing OrElse tables.Length = 0 Then Return

        ResetProgressBar(tables.Length)

        Dim hasCollections As Boolean = ContainsCollectionsInParams(tables)

        'Use a transform to reshape the parameter XML into CSV.
        Dim sbXSLT As New StringBuilder()
        Dim sbInputXSLT As New StringBuilder()

        sbXSLT.AppendLine("<?xml version=""1.0"" encoding=""UTF-8"" ?>")
        sbXSLT.Append("<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"" xmlns:ext=""urn:XsltFunctions"">")
        sbXSLT.Append("<xsl:param name=""objectname""></xsl:param>")
        sbXSLT.AppendLine("<xsl:template match=""/"">")

        sbInputXSLT.AppendLine("<xsl:for-each select=""parameters/inputs/input"">")
        If tables IsNot Nothing AndAlso tables.Length > 0 Then
            sbInputXSLT.Append(New String(","c, tables(0).Columns.Count - 1))
        Else
            sbInputXSLT.Append(New String(","c, 14))
        End If
        sbInputXSLT.Append("""IN"",")
        sbInputXSLT.Append("""<xsl:value-of select=""ext:ChooseLocalizedFriendlyName(@name, $objectname)""/>"",")
        sbInputXSLT.Append("<xsl:choose>")
        sbInputXSLT.Append("<xsl:when test=""@type='collection'"">,,,</xsl:when>")
        sbInputXSLT.Append("<xsl:otherwise>")
        If hasCollections Then sbInputXSLT.Append(",,")

        sbInputXSLT.Append("""<xsl:value-of select=""ext:GetLocalizedFriendlyNameDataType(@type)""/>"",")
        sbInputXSLT.Append("""<xsl:value-of select=""@value""/>""")
        sbInputXSLT.Append("</xsl:otherwise>")
        sbInputXSLT.AppendLine("</xsl:choose>")
        sbInputXSLT.AppendLine("</xsl:for-each>")

        sbInputXSLT.AppendLine("<xsl:for-each select=""parameters/inputs/input/row"">")
        sbInputXSLT.AppendLine("<xsl:variable name=""rownumber"" select=""position()""/>")
        sbInputXSLT.AppendLine("<xsl:for-each select=""field"">")
        If tables IsNot Nothing AndAlso tables.Length > 0 Then
            sbInputXSLT.Append(New String(","c, tables(0).Columns.Count - 1))
        Else
            sbInputXSLT.Append(New String(","c, 14))
        End If
        sbInputXSLT.Append("""IN"",")
        sbInputXSLT.Append("""<xsl:value-of select=""ext:ChooseLocalizedFriendlyName(../../@name, $objectname)""/>"",")
        sbInputXSLT.Append("""<xsl:value-of select=""@name""/>"",")
        sbInputXSLT.Append("""<xsl:value-of select=""$rownumber""/>"",")
        sbInputXSLT.Append("""<xsl:value-of select=""ext:GetLocalizedFriendlyNameDataType(@type)""/>"",")
        sbInputXSLT.Append("""<xsl:value-of select=""@value""/>""")
        sbInputXSLT.AppendLine("</xsl:for-each>")
        sbInputXSLT.AppendLine("</xsl:for-each>")

        'Add the input transfrom text and then modify 
        'and reuse as the output text.
        If bLocalise Then
            sbInputXSLT.Replace("""IN""", My.Resources.frmLogParameterViewer_IN)
        End If
        sbXSLT.AppendLine(sbInputXSLT.ToString())
        sbInputXSLT.Replace("/input", "/output")
        If bLocalise Then
            sbInputXSLT.Replace(My.Resources.frmLogParameterViewer_IN, My.Resources.frmLogParameterViewer_OUT)
        Else
            sbInputXSLT.Replace("""IN""", """OUT""")
        End If
        sbXSLT.AppendLine(sbInputXSLT.ToString())

        sbXSLT.AppendLine("</xsl:template>")
        sbXSLT.AppendLine("</xsl:stylesheet>")


        Dim xsltDoc As New ReadableXmlDocument(sbXSLT.ToString())

        Dim xslTransformer As New XslCompiledTransform()
        xslTransformer = New XslCompiledTransform()
        xslTransformer.Load(xsltDoc)

        ' Figure out the columns needed to hold the exploded parameters
        Dim paramColumns As ICollection(Of String) = InitParamHeaders(hasCollections)

        Dim writtenHeader As Boolean = False

        For Each dt As DataTable In tables

            ' Get the parameter column if one is there
            Dim colParam As DataColumn = dt.Columns("ParameterXml")

            ' Also the log number column - used when writing the parameters.
            Dim colLogNo As DataColumn = dt.Columns("LogNumber")

            ' Make a header row if we haven't already
            If Not writtenHeader Then
                Dim sep As String = ""
                For Each col As DataColumn In dt.Columns
                    If col Is colParam Then Continue For ' Skip the param column until the end
                    AppendValueInto(IIf(bLocalise, GetLocalizedFriendlyNameHeader(col.ColumnName), col.ColumnName), output, sep)
                Next
                ' If we have parameter columns, append them to the end of the column list
                If colParam IsNot Nothing Then
                    For Each paramColName As String In paramColumns
                        AppendValueInto(paramColName, output, sep)
                    Next
                End If

                output.WriteLine()
                writtenHeader = True
            End If

            ' Now output the actual data.
            For Each row As DataRow In dt.Rows
                ' Deal with all columns except the parameter XML column
                Dim sep As String = ""
                For Each col As DataColumn In dt.Columns
                    ' We deal with the parameters column last, so skip it at this point
                    If col Is colParam Then Continue For
                    Dim val = row(col)
                    If Not IsEmpty(val) Then
                        Dim valStr = Escape(val.ToString())
                        If col.ColumnName = frmLogViewer.ColumnNames.StageType Then
                            val = CStr(IIf(bLocalise, clsStageTypeName.GetLocalizedFriendlyName(valStr), valStr))
                        ElseIf col.ColumnName = frmLogViewer.ColumnNames.StageName Then
                            val = CStr(IIf(bLocalise, LTools.GetC(valStr, "misc", "stage"), valStr))
                        ElseIf col.ColumnName = frmLogViewer.ColumnNames.ResultType Then
                            val = CStr(IIf(bLocalise, clsProcessDataTypes.GetFriendlyName(valStr), valStr))
                        ElseIf col.ColumnName = frmLogViewer.ColumnNames.Action Then
                            Dim sObjectValue = GetHtmlValue(row(frmLogViewer.ColumnNames.Object))
                            val = CStr(IIf(bLocalise, clsBusinessObjectAction.GetLocalizedFriendlyName(valStr, sObjectValue, "Action"), valStr))
                        ElseIf col.ColumnName = frmLogViewer.ColumnNames.Result Then
                            If valStr.StartsWith("ERROR: ") Then
                                val = valStr.Replace("ERROR: ", My.Resources.LogError)
                            End If
                        End If
                    End If
                    AppendValueInto(val, output, sep)
                Next

                output.WriteLine()

                ' If this data table has a parameter column and this row has data in it...
                If colParam IsNot Nothing AndAlso Not IsEmpty(row(colParam)) Then
                    ' Transform the parameter XML into CSV
                    ' FIXME: There has to be an less convoluted way of doing this
                    Dim paramDoc As New ReadableXmlDocument(
                        row(colParam).ToString().Replace("&quot;", "&quot;&quot;"))
                    Dim xslArg As XsltArgumentList = New XsltArgumentList
                    xslArg.AddExtensionObject("urn:XsltFunctions", New clsMyXsltExtensionFunctions(bLocalise))
                    If row.ItemArray.Contains(frmLogViewer.ColumnNames.Object) Then _
                        xslArg.AddParam("objectname", "", row(frmLogViewer.ColumnNames.Object).ToString())
                    Using sw As New StringWriter(CultureInfo.InvariantCulture)
                        Using tw As New XmlTextWriter(sw)
                            xslTransformer.Transform(paramDoc, xslArg, tw)
                            tw.Flush()
                        End Using

                        'Add the log number at the start of each line of parameter CSV.
                        Dim params As String = sw.ToString()
                        If colLogNo IsNot Nothing Then
                            Static rxLogNo As New Regex("^,", RegexOptions.Multiline)
                            params = rxLogNo.Replace(params, """" & CStr(row(colLogNo)) & """,")
                        End If

                        output.WriteLine(params)

                    End Using

                End If
            Next

            'Next table
            IncrementProgressBar()
        Next

    End Sub

    Private Sub Me_Closing(sender As Object, e As FormClosingEventArgs) Handles MyBase.FormClosing
        mDataTablesRequiringDisposing.ForEach(Sub(x) x.Dispose())
    End Sub
End Class
