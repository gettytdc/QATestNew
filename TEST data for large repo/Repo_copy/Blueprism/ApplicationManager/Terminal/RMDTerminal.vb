Imports System.Net
Imports System.IO
Imports System.Security
Imports BluePrism.Core.Xml

Public Class RMDTerminal
    Inherits AbstractTerminal

    Private mConnected As Boolean
    Private mPort As Integer

    Public Sub New()
        mConnected = False
    End Sub

    Public Overrides Function ConnectToHostOrSession(ByVal SessionProfile As String, ByVal SessionShortName As String, ByRef sErr As String) As Boolean
        Try
            If mConnected Then
                sErr = My.Resources.AlreadyConnected
                Return False
            End If
            If Not Integer.TryParse(SessionProfile, mPort) Then
                sErr = String.Format(My.Resources.SpecifyAValidPortNumber0IsNotValid, SessionProfile)
                Return False
            End If
            mConnected = True
            Dim response As String = Query("<macro></macro>")
            Debug.WriteLine("Connect responded: " & response)
            Return True
        Catch ex As Exception
            mConnected = False
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function DisconnectFromHost(ByRef sErr As String) As Boolean
        If Not mConnected Then
            sErr = My.Resources.NotConnected
            Return False
        End If
        mConnected = False
    End Function

    Public Overrides Function Launch(sessionProfile As String, ByRef sErr As String) As Boolean
        Throw New NotImplementedException
    End Function

    Public Overrides Function Attach(sessionShortName As String, ByRef sErr As String) As Boolean
        Throw New NotImplementedException
    End Function

    Public Overrides Function Detach(ByRef sErr As String) As Boolean
        Throw New NotImplementedException
    End Function

    Public Overrides Function Terminate(ByRef sErr As String) As Boolean
        Throw New NotImplementedException
    End Function

    Public Overrides Function GetText(ByVal StartRow As Integer, ByVal StartColumn As Integer, ByVal Length As Integer, ByRef Value As String, ByRef sErr As String) As Boolean
        Try
            Dim resp As String = Query("<macro><var><name>value</name></var><scrape><start>" & (80 * (StartRow - 1) + StartColumn - 1).ToString() & "</start><count>" & Length.ToString() & "</count><varname>value</varname></scrape></macro>")
            Dim x As New ReadableXmlDocument(resp)
            Value = x.SelectSingleNode("//var[name='value']/value").InnerText
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function SendKeystroke(ByVal Key As String, ByRef sErr As String) As Boolean
        Try
            Query("<macro><write><literal>" & SecurityElement.Escape(Key) & "</literal></write></macro>")
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function SendControlKeys(keys As String, ByRef sErr As String) As Boolean
        Throw New NotImplementedException()
    End Function

    Public Overrides Function SetText(ByVal StartRow As Integer, ByVal StartColumn As Integer, ByVal Value As String, ByRef sErr As String) As Boolean
        Query("<macro><write><x><literal>" & (StartColumn - 1).ToString() & "</literal></x><y><literal>" & (StartRow - 1).ToString() & "</literal></y><literal>" & SecurityElement.Escape(Value) & "</literal></write></macro>")
        Return True
    End Function

    Public Overrides Function GetWindowTitle(ByRef Value As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.FunctionNotSupportedByRMD
        Return False
    End Function

    Public Overrides Function SetWindowTitle(ByVal Value As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.FunctionNotSupportedByRMD
        Return False
    End Function

    Public Overrides Function GetCursorPosition(ByRef row As Integer, ByRef col As Integer, ByRef sErr As String) As Boolean
        Try
            Dim resp As String = Query("<macro><var><name>x</name></var><var><name>y</name></var><getX><varname>x</varname></getX><getY><varname>y</varname></getY></macro>")
            Dim x As New ReadableXmlDocument(resp)
            col = Integer.Parse(x.SelectSingleNode("//var[name='x']/value").InnerText) + 1
            row = Integer.Parse(x.SelectSingleNode("//var[name='y']/value").InnerText) + 1
            Return True
        Catch ex As Exception
            sErr = ex.Message
            Return False
        End Try
    End Function

    Public Overrides Function SetCursorPosition(ByVal row As Integer, ByVal col As Integer, ByRef sErr As String) As Boolean
        Query("<macro><setX><literal>" & (col - 1).ToString() & "</literal></setX><setY><literal>" & (row - 1).ToString() & "</literal></setY></macro>")
        Return True
    End Function

    Public Overrides Function RunEmulatorMacro(ByVal MacroName As String, ByRef sErr As String) As Boolean
        sErr = My.Resources.FunctionNotSupportedByRMD
        Return False
    End Function

    Public Overrides Function GetSessionSize(ByRef NumRows As Integer, ByRef NumColumns As Integer, ByRef sErr As String) As Boolean
        'It's a hard-coded fixed size!
        NumRows = 24
        NumColumns = 80
        Return True
    End Function

    Public Overrides Function IsConnected() As Boolean
        Return mConnected
    End Function

    Public Overrides Function SelectArea(ByVal StartRow As Integer, ByVal StartColumn As Integer, ByVal EndRow As Integer, ByVal EndColumn As Integer, ByVal Type As AbstractTerminal.SelectionType, ByRef sErr As String) As Boolean
        sErr = My.Resources.FunctionNotSupportedByRMD
        Return False
    End Function


    Public Overrides Property SleepTime() As Integer
        Get
            'do nothing - not relevant
        End Get
        Set(ByVal value As Integer)
            'do nothing - not relevant
        End Set
    End Property


    Public Overrides Property WaitTimeout() As Integer
        Get
            'do nothing - not relevant
        End Get
        Set(ByVal value As Integer)
            'do nothing - not relevant
        End Set
    End Property

    ''' <summary>
    ''' Send a query to the emulator and return the response. Throws an exception if
    ''' anything goes wrong.
    ''' </summary>
    ''' <param name="xml">The XML to send.</param>
    ''' <returns>The response.</returns>
    Private Function Query(ByVal xml As String) As String
        If Not mConnected Then Throw New InvalidOperationException("Not connected")
        Dim req As HttpWebRequest = CType(WebRequest.Create("http://localhost:" & mPort.ToString() & "/macro?xml=" & xml), HttpWebRequest)
        req.Method = "GET"
        req.KeepAlive = True
        Dim resp As HttpWebResponse
        Try
            resp = CType(req.GetResponse(), HttpWebResponse)
        Catch ex As WebException
            Throw New InvalidOperationException(String.Format(My.Resources.CommunicationWithRMDFailed0, ex.Message))
        End Try
        If resp.StatusCode <> HttpStatusCode.OK Then
            Throw New InvalidOperationException(String.Format(My.Resources.RequestToRMDFailedWithHTTPError0, resp.StatusCode))
        End If
        Using sr As New StreamReader(resp.GetResponseStream)
            Return sr.ReadToEnd()
        End Using
    End Function

End Class
