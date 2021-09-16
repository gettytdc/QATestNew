'This file does not have Option Strict On, because it uses a lot
'of Late Binding to access the Business Object.
Option Strict Off

Imports System.IO
Imports System.Reflection
Imports System.Xml
Imports System.Xml.Schema
Imports BluePrism.AutomateProcessCore.Stages
Imports BluePrism.Core.Xml

''' Project  : AutomateProcessCore
''' Class    : AutomateProcessCore.clsBusinessObject
''' 
''' <summary>
''' A class representing a Business Object, with an encapsulated
''' instance of it.
''' </summary>
Public Class clsCOMBusinessObject
    Inherits clsBusinessObject

    ''' <summary>
    ''' A reference to the underlying Business Object instance.
    ''' </summary>
    Private mobjObject As Object

    ''' <summary>
    ''' The full capabilities of the Business Object, as returned by
    ''' GetCapabilitiesXML()
    ''' </summary>
    Private msCaps As String

    ''' <summary>
    ''' Logging information - NOT PROPERLY DOCUMENTED
    ''' </summary>
    Private mscolLog As Collection

    ''' <summary>
    ''' The ID this object has been given when registered with
    ''' the database.
    ''' </summary>
    Private mgRegisteredID As Guid

    Private mDisposedValue As Boolean = False

    ''' <summary>
    ''' Constructor for clsBusinessObject.
    ''' </summary>
    Public Sub New(ByVal sName As String, ByVal sConfigXML As String)
        MyBase.New()
        mobjObject = Nothing
        mName = ""
        mFriendlyName = ""
        msCaps = ""
        mValid = False

        'Assume not configurable unless set below.
        mConfigurable = False

        'And assume no lifecycle management.
        mLifecycle = False

        'If no run mode is set, we pessimistically assume the business
        'object requires exclusive access to the resource pc...
        mRunMode = BusinessObjectRunMode.Exclusive

        Dim sErr As String = Nothing
        mName = sName
        'Temporarily put something useful in the friendly name
        'field, in case the following code fails, as we will still
        'want something to display to the user.
        mFriendlyName = "<" & mName & ">"

        If Not DoCreateObject() Then
            Exit Sub
        End If

        Try
            msCaps = mobjObject.GetCapabilitiesXML()

            If msCaps Is Nothing Then
                Throw New InvalidOperationException(My.Resources.Resources.GetCapabilitiesXMLDidNotReturnAnything)
            End If
            'With a little modification we can enforce the schema validation
            'When we load the BusinessObjects.
            'If Not Validate(msCaps, x, sErr) Then
            '    mbValid = False
            '    msErrorMessage = sErr
            '    GoTo NextObject
            'End If

            Dim x As New ReadableXmlDocument(msCaps)

            Dim rootel As XmlElement, curel As XmlElement, curel2 As XmlElement, curel3 As XmlElement
            rootel = x.DocumentElement
            If rootel.Name <> "resourceunit" Then
                mValid = False
                mErrorMessage = My.Resources.Resources.MissingResourceunit
                Exit Sub
            End If
            If rootel.GetAttribute("name") <> mName Then
                mValid = False
                mErrorMessage = My.Resources.Resources.ResourceNameMismatch
                Exit Sub
            End If
            Dim sfn As String
            'Get friendly name, or just use object name if one
            'isn't defined...
            sfn = rootel.GetAttribute("friendlyname")
            If sfn = "" Then sfn = mName
            mFriendlyName = sfn
            Narrative = rootel.GetAttribute("narrative")
            For Each curel In rootel.ChildNodes
                Select Case curel.Name
                    Case "action"
                        Dim objNewAct As New clsCOMBusinessObjectAction
                        objNewAct.SetName(curel.GetAttribute("name"))
                        objNewAct.SetNarrative(curel.GetAttribute("narrative"))
                        For Each curel2 In curel.ChildNodes
                            Select Case curel2.Name
                                Case "preconditions"
                                    For Each curel3 In curel2.ChildNodes
                                        Select Case curel3.Name
                                            Case "condition"
                                                objNewAct.SetPrecondition(curel3.SelectSingleNode("@narrative").Value)
                                        End Select
                                    Next
                                Case "endpoint"
                                    objNewAct.SetEndpoint(curel2.SelectSingleNode("@narrative").Value)
                                Case "inputs"
                                    If Not objNewAct.SetParamsFromXML(curel2, False, sErr) Then
                                        mValid = False
                                        mErrorMessage = sErr
                                        Exit Sub
                                    End If
                                Case "outputs"
                                    If Not objNewAct.SetParamsFromXML(curel2, True, sErr) Then
                                        mValid = False
                                        mErrorMessage = sErr
                                        Exit Sub
                                    End If
                            End Select
                        Next
                        AddAction(objNewAct)

                    Case "runmode"
                        Dim TempString As String = curel.GetAttribute("mode")
                        Try
                            mRunMode = CType(System.Enum.Parse(GetType(BusinessObjectRunMode), TempString), BusinessObjectRunMode)
                        Catch ex As Exception
                            mErrorMessage = My.Resources.Resources.InvalidRunModeSpecifiedRunModeMustBeExclusiveForegroundOrBackground
                            mValid = False
                            Exit Sub
                        End Try

                    Case "lifecycle"
                        mLifecycle = True

                    Case "config"

                        Try
                            If sConfigXML <> "" Then
                                SetConfig(sConfigXML, sErr)
                            End If
                            mConfigurable = True
                        Catch ex As MissingMemberException
                            sErr = My.Resources.Resources.SetConfigMethodDoesNotExistInConfigurableObjectDefaultValuesWillBeShown
                            mConfigurable = False
                            mValid = False
                            mErrorMessage = sErr
                        Catch ex As Exception
                            sErr = String.Format(My.Resources.Resources.ErrorSettingConfigXML0, ex.Message)
                            mConfigurable = False
                            mValid = False
                            mErrorMessage = sErr
                        End Try

                End Select
            Next

            mValid = True

        Catch e As XmlException
            mValid = False
            mErrorMessage = String.Format(My.Resources.Resources.ErrorInCapabilitiesOf01, mName, e.Message)
        Catch e As Exception
            mValid = False
            mErrorMessage = String.Format(My.Resources.Resources.FailedToGetCapabilitiesOf01, mName, e.Message)
        End Try
    End Sub

    Protected Overloads Sub Dispose(ByVal disposing As Boolean)
        If Not mDisposedValue AndAlso disposing Then
            mobjObject = Nothing
            mscolLog = Nothing
        End If
        mDisposedValue = True
    End Sub

    Public Overloads Sub Dispose()
        ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        Dispose(True)
        GC.SuppressFinalize(Me)
    End Sub

    ''' <summary>
    ''' Handles anything that must be done to dispose the object.
    ''' </summary>
    ''' <remarks></remarks>
    Public Overrides Sub DisposeTasks()
        Dispose()
    End Sub

    ''' <summary>
    ''' Initialise the business object, as per API documentation.
    ''' </summary>
    ''' <returns>A clsProcessStage.Result</returns>
    Public Overrides Function DoInit() As StageResult
        Try
            If Not Valid Then
                Return New StageResult(False, "Internal", My.Resources.Resources.BusinessObjectIsNotValid)
            End If
            If Not mLifecycle Then Return New StageResult(True)
            Dim sErr As String = Nothing
            If Not mobjObject.Init(sErr) Then
                Return New StageResult(False, "Internal", sErr)
            End If
            Return New StageResult(True)
        Catch ex As Exception
            Return New StageResult(False, "Internal", "Failed Init: " & ex.Message)
        End Try
    End Function

    ''' <summary>
    ''' Clean up the business object, as per API documentation.
    ''' </summary>
    ''' <returns>A clsProcessStage.Result</returns>
    Public Overrides Function DoCleanUp() As StageResult
        Try
            If Not Valid Then
                Return New StageResult(False, "Internal", My.Resources.Resources.BusinessObjectIsNotValid)
            End If
            If Not mLifecycle Then Return New StageResult(True)
            Dim sErr As String = Nothing
            If Not mobjObject.CleanUp(sErr) Then
                Return New StageResult(False, "Internal", sErr)
            End If
            Return New StageResult(True)
        Catch e As Exception
            Return New StageResult(False, "Internal", My.Resources.Resources.FailedCleanUp & e.Message)
        End Try
    End Function


    ''' <summary>
    ''' Ask the Business Object to perform an action.
    ''' </summary>
    ''' <param name="sActionName">The name of the action to perform</param>
    ''' <param name="ScopeStage">The stage used to resolve scope within the business
    ''' object action. Not relevant for COM business objects. May be null.</param>
    ''' <param name="inputs">The inputs</param>
    ''' <param name="outputs">On return, contains the ouputs</param>
    ''' <returns>A clsProcessStage.Result</returns>
    Protected Overrides Function DoDoAction(ByVal sActionName As String, ByVal ScopeStage As clsProcessStage, ByVal inputs As clsArgumentList, ByRef outputs As clsArgumentList) As StageResult
        If mobjObject Is Nothing Then
            Return New StageResult(False, "Internal", My.Resources.Resources.MissingBusinessObject)
        End If
        Try
            Dim sInputsXML As String
            sInputsXML = inputs.ArgumentsToXML(False, GetAction(sActionName))


            Dim sOutputsXML As String = ""
            'Regarding the following code, please see bug #1846 and also read
            'the notes at http://devtalk.blueprism.co.uk/viewtopic.php?t=31
            Dim sErr As String = Nothing
            Try
                If Not mobjObject.DoAction(sActionName, sInputsXML, sOutputsXML, Nothing, sErr) Then
                    Return New StageResult(False, "Internal", sErr)
                End If
            Catch
                'Backwards compatability
                If Not mobjObject.DoAction(sActionName, sInputsXML, sOutputsXML, sErr) Then
                    Return New StageResult(False, "Internal", sErr)
                End If
            End Try

            outputs = clsArgumentList.XMLToArguments(sOutputsXML, True)

            Return New StageResult(True)

        Catch ex As Exception
            Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsCOMBusinessObject_Exception0, ex.Message))
        End Try

    End Function

    ''' <summary>
    ''' Set configuration on the Business Object - a wrapper for the
    ''' Business Object function itself
    ''' </summary>
    ''' <param name="sConfigXML">The config</param>
    ''' <param name="sErr">On failure, an error description</param>
    ''' <returns>True if successful, False otherwise</returns>
    Public Function SetConfig(ByVal sConfigXML As String, ByRef sErr As String) As Boolean
        If Not mobjObject Is Nothing Then
            Return mobjObject.SetConfig(sConfigXML, sErr)
        Else
            sErr = My.Resources.Resources.MissingObject
            Return False
        End If
    End Function

    ''' <summary>
    ''' Get configuration on the Business Object - a wrapper for the
    ''' Business Object function itself
    ''' </summary>
    ''' <param name="sErr">On failure, an error description, otherwise an
    ''' empty string</param>
    ''' <returns>The ConfigXML of the Businessobject</returns>
    Public Overrides Function GetConfig(ByRef sErr As String) As String
        If Not mobjObject Is Nothing Then
            Return mobjObject.GetConfig(sErr)
        Else
            sErr = My.Resources.Resources.MissingObject
            Return ""
        End If
    End Function

    ''' <summary>
    ''' Show Config UI on the Business Object - a wrapper for the
    ''' Business Object function itself
    ''' </summary>
    ''' <param name="sErr">On failure, an error description</param>
    ''' <returns>True if successful, False otherwise</returns>
    Public Overrides Function ShowConfigUI(ByRef sErr As String) As Boolean
        If Not mobjObject Is Nothing Then
            ' #4502 : ShowConfigUI not implemented on "BP Test - Runmode *"
            ' business objects.
            Try
                mobjObject.ShowConfigUI(sErr)
            Catch mme As MissingMemberException
                sErr = My.Resources.Resources.ThisObjectCannotBeConfiguredThroughTheUserInterface
            End Try

            If sErr = "" Then
                Return True
            Else
                Return False
            End If
        Else
            sErr = My.Resources.Resources.MissingObject
            Return False
        End If
    End Function


    ''' <summary>
    ''' Get the capabilities of the Business Object.
    ''' </summary>
    ''' <returns>The capabilities in XML format.</returns>
    Friend Function GetCapabilitiesXML() As String
        If Not mobjObject Is Nothing Then
            Return mobjObject.GetCapabilitiesXML()
        Else
            Return ""
        End If
    End Function

    ''' <summary>
    ''' Validates a businessobject's Capabilities XML against The CapabilitiesXML schema.
    ''' </summary>
    ''' <param name="sErr"></param>
    ''' <returns></returns>
    Public Function Validate(ByRef sErr As String) As Boolean

        If Not mValid Then
            sErr = mErrorMessage
            Return False
        End If

        If msCaps Is Nothing Then
            sErr = My.Resources.Resources.InternalError
            Return False
        End If

        Using schemaReader As New XmlTextReader(
         Assembly.GetExecutingAssembly().GetManifestResourceStream(
          "BluePrism.AutomateProcessCore.CapabilitiesXML.xsd"))

            Dim settings As New XmlReaderSettings() With {
                .ValidationType = ValidationType.Schema,
                .XmlResolver = Nothing
            }
            settings.Schemas.Add(Nothing, schemaReader)

            Using reader = XmlReader.Create(New StringReader(msCaps), settings)

                Dim x As New ReadableXmlDocument()
                Try
                    x.Load(reader)

                Catch exSchema As XmlSchemaException
                    ' because our getcapabilitiesXML is all on one line we get the xml
                    ' at posision ex.LinePosition and ignore ex.LineNumber
                    sErr = exSchema.Message
                    Return False

                Catch exXml As XmlException
                    sErr = String.Format(My.Resources.Resources.TheBusinessObject0CouldNotBeFound, mName)
                    Return False

                End Try

                Return True
            End Using
        End Using
    End Function

    Protected Overrides Sub GetHTMLPreamble(ByVal xr As System.Xml.XmlTextWriter)
        xr.WriteElementString("h1", My.Resources.Resources.BusinessObjectDefinition)
        xr.WriteElementString("div", My.Resources.Resources.TheInformationContainedInThisDocumentIsTheProprietaryInformationOfBluePrismLimi)
        xr.WriteElementString("div", My.Resources.Resources.CBluePrismLimited)
        xr.WriteElementString("h2", My.Resources.Resources.AboutThisDocument)
        xr.WriteElementString("div", My.Resources.Resources.TheBusinessObjectDefinitionDescribesTheAPIsAvailableWithinASingleBusinessObject)
        xr.WriteElementString("h2", My.Resources.Resources.AboutBusinessObjects)
        xr.WriteElementString("div", My.Resources.Resources.BusinessObjectsWithinTheEnvironmentIEObjectsWhichMayBeDrawnOntoAProcessToCaptur)
    End Sub

    <DebuggerStepThrough()>
    Private Function DoCreateObject() As Boolean
        Try
            mobjObject = CreateObject(mName)
        Catch ex As Exception
            mErrorMessage = String.Format(My.Resources.Resources.FailedToCreateInstanceOfBusinessObject01, mName, ex.Message)
            mValid = False
            Return False
        End Try

        Return True
    End Function

    Protected Overrides Sub Finalize()
        MyBase.Finalize()
    End Sub
End Class
