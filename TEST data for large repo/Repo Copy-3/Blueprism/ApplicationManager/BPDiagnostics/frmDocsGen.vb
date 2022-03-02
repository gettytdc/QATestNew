Imports System.Collections.Generic
Imports System.IO
Imports System.Text.RegularExpressions
Imports BluePrism.Core.Xml
Imports AppAMI = BluePrism.ApplicationManager.AMI

Public Class frmDocsGen
    Private Sub btnDocs_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnDocs.Click
        Dim Format As BluePrism.AMI.clsAMI.DocumentFormats
        Try
            Format = CType(System.Enum.Parse(GetType(BluePrism.AMI.clsAMI.DocumentFormats), Me.cmbDocFormat.Text), BluePrism.AMI.clsAMI.DocumentFormats)
        Catch ex As Exception
            Format = BluePrism.AMI.clsAMI.DocumentFormats.Wiki
        End Try

        Dim a As New BluePrism.AMI.clsAMI(New AppAMI.clsGlobalInfo)
        Dim f As New frmDocs(a.GetDocumentation(Format))
        f.ShowDialog()
    End Sub

    Private Sub frmDocsGen_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        'Populate document types combo
        Me.cmbDocFormat.Items.Clear()
        For Each sFormat As String In System.Enum.GetNames(GetType(BluePrism.AMI.clsAMI.DocumentFormats))
            Me.cmbDocFormat.Items.Add(sFormat)
        Next
        Me.cmbDocFormat.Text = BluePrism.AMI.clsAMI.DocumentFormats.Wiki.ToString
    End Sub

    Private Sub btnQDocs_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnQDocs.Click
        Dim f As New frmDocs(clsLocalTargetApp.GetWikiDocumentation())
        f.ShowDialog()
    End Sub


    Private Class Page
        Public Source As String
        Public URL As String
    End Class

    Private Class Resource
        Inherits Page
        Public ContentType As String
    End Class

    Private Sub btnSourceGen_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSourceGen.Click
        Try
            If Not IO.Directory.Exists(txtOutputDir.Text) Then
                MessageBox.Show(My.Resources.TheOutputDirectoryDoesNotExist, My.Resources.xError)
                Return
            End If
            If IO.Directory.GetFiles(txtOutputDir.Text).Length <> 0 Then
                MessageBox.Show(My.Resources.TheOutputDirectoryMustBeEmpty, My.Resources.xError)
                Return
            End If

            Dim documents As New List(Of Page)
            Dim resources As New List(Of Resource)
            ReadSourceCaptureDocument(documents, resources)
            WriteSourceCaptureToFiles(documents, resources)

            MessageBox.Show(My.Resources.SourceCaptureFilesExtracted, My.Resources.Success)

        Catch ex As Exception
            MessageBox.Show(String.Format(My.Resources.SomethingWentWrong0, ex.ToString), My.Resources.xError)
        End Try
    End Sub


    Private Sub ReadSourceCaptureDocument(ByVal documents As List(Of Page), ByVal resources As List(Of Resource))

        Dim x As ReadableXmlDocument
        Using str As Stream = File.OpenRead(txtSource.Text)
            x = New ReadableXmlDocument(str)
        End Using

        For Each node As Xml.XmlNode In x.ChildNodes
            If TypeOf (node) Is Xml.XmlElement Then
                Dim eCapture As Xml.XmlElement = CType(node, Xml.XmlElement)
                If eCapture.Name = "capture" Then
                    For Each xmlEl As Xml.XmlElement In eCapture.ChildNodes
                        Select Case xmlEl.Name
                            Case "document", "frame"

                                Dim document As New Page
                                document.Source = TryCast(xmlEl.FirstChild, Xml.XmlCDataSection).Value
                                document.URL = xmlEl.GetAttribute("url")
                                documents.Add(document)

                            Case "resource"

                                Dim resource As New Resource
                                resource.URL = xmlEl.GetAttribute("source")
                                resource.ContentType = xmlEl.GetAttribute("contenttype")
                                resource.Source = xmlEl.InnerText
                                resources.Add(resource)

                            Case Else
                                Throw New InvalidOperationException(My.Resources.InvalidCaptureFile)
                        End Select
                    Next
                End If
            End If
        Next
    End Sub

    Private Function SanitisePageName(ByVal name As String) As String
        If Not name.EndsWith(".html") Then
            name = name & ".html"
        End If
        name = name.Replace("?", "_q_")
        name = name.Replace("&", "_a_")
        name = name.Replace("=", "_e_")
        Return name
    End Function

    Private Sub WriteSourceCaptureToFiles(ByVal documents As List(Of Page), ByVal resources As List(Of Resource))

        Dim respaths As New List(Of String)

        For Each resource As Resource In resources

            If resource.URL.Length > 0 Then
                Dim resourceURL As Uri
                Try
                    resourceURL = New Uri(resource.URL)
                Catch ex As Exception
                    Throw New InvalidOperationException(My.Resources.InvalidResourceUrl + resource.URL)
                End Try

                Dim resourcePath As String = resourceURL.LocalPath()

                Dim directoryName As String = IO.Path.GetDirectoryName(resourcePath)

                Dim fileName As String = IO.Path.GetFileName(resourcePath)
                respaths.Add(IO.Path.Combine(directoryName, fileName))

                Dim directoryInfo As IO.DirectoryInfo = CreateDirectoryForDocument(directoryName)

                Dim filePath As String = IO.Path.Combine(directoryInfo.FullName, fileName)

                If resource.ContentType.StartsWith("text") Then
                    Using sw As New IO.StreamWriter(filePath)
                        sw.Write(resource.Source)
                    End Using
                Else
                    Dim output() As Byte = Convert.FromBase64String(resource.Source)
                    IO.File.WriteAllBytes(filePath, output)
                End If
            End If
        Next

        Dim root As String = Nothing

        For Each document As Page In documents

            Dim documentURL As New Uri(document.URL)
            If root Is Nothing Then
                root = documentURL.Scheme & ":" & Uri.SchemeDelimiter & documentURL.Authority
            End If

            Dim documentPath As String = documentURL.LocalPath()

            Dim directoryName As String
            Try
                directoryName = IO.Path.GetDirectoryName(documentPath)
            Catch e As Exception
                Continue For
            End Try

            Dim pageName As String = IO.Path.GetFileName(documentPath)
            pageName = SanitisePageName(pageName)

            Dim directoryInfo As IO.DirectoryInfo = CreateDirectoryForDocument(directoryName)

            Dim filePath As String = IO.Path.Combine(directoryInfo.FullName, pageName)

            If Not IO.File.Exists(filePath) Then

                Dim src As String = document.Source

                If chkFixLinks.Checked Then
                    'Try and replace absolute links back to the same site with relative
                    'ones...
                    src = src.Replace("""" & root & "/", """/")
                    'Resource paths in the document can have queries appended, e.g. ?h=abcd,
                    'but our resource paths don't, so attempt to remove those...
                    For Each respath As String In respaths
                        respath = respath.Replace("\", "/")
                        Dim pattern As String = """" & respath & "\?[^""]*"
                        Dim replacewith As String = """" & respath
                        src = Regex.Replace(src, pattern, replacewith)
                    Next
                End If

                Using sw As New IO.StreamWriter(filePath)
                    sw.Write(src)
                End Using
            End If

        Next

    End Sub

    Private Sub EscapeFilePath(ByRef Path As String)
        For Each c As Char In IO.Path.GetInvalidFileNameChars
            Path = Path.Replace(c, "_")
        Next
    End Sub

    Private Function CreateDirectoryForDocument(ByVal directoryName As String) As IO.DirectoryInfo

        Dim relativeDirectoryName As String = directoryName
        If IO.Path.IsPathRooted(directoryName) Then
            relativeDirectoryName = directoryName.TrimStart("\"c)
        End If

        Dim outputDirectory As String = IO.Path.Combine(Me.txtOutputDir.Text, relativeDirectoryName)

        Return IO.Directory.CreateDirectory(outputDirectory)
    End Function

    Private Sub btnBrowse_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowse.Click
        OpenFileDialog1.FileName = String.Empty
        OpenFileDialog1.Filter = My.Resources.XMLFileXmlXmlAllFiles
        If OpenFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            txtSource.Text = OpenFileDialog1.FileName
        End If
    End Sub

    Private Sub btnBrowseOutput_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnBrowseOutput.Click

        Dim fd As New FolderBrowserDialog()
        fd.ShowNewFolderButton = True
        If IO.File.Exists(txtSource.Text) Then
            fd.SelectedPath = IO.Path.GetDirectoryName(txtSource.Text)
        End If
        If fd.ShowDialog() = Windows.Forms.DialogResult.OK Then
            If IO.Directory.GetFiles(fd.SelectedPath).Length <> 0 Then
                MessageBox.Show(My.Resources.TheOutputDirectoryMustBeEmpty)
            Else
                txtOutputDir.Text = fd.SelectedPath
            End If
        End If

    End Sub

End Class
