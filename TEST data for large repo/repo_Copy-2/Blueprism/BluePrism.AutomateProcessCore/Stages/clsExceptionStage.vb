Imports BluePrism.Core.Expressions
Imports BluePrism.BPCoreLib
Imports System.Runtime.Serialization

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsExceptionStage
    ''' 
    ''' <summary>
    ''' A class representing a Exception stage
    ''' </summary>
    ''' <remarks>
    ''' </remarks>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsExceptionStage
        Inherits clsProcessStage

        Private Const YES As String = "yes"
        Private Const NO As String = "no"

        Public Property ExceptionLocalized() As String
            Get
                Return mExceptionLocalized
            End Get
            Set(ByVal value As String)
                mExceptionLocalized = value
            End Set
        End Property
        <DataMember>
        Private mExceptionLocalized As String

        ''' <summary>
        ''' The type of the exception
        ''' </summary>
        Public Property ExceptionType() As String
            Get
                Return mExceptionType
            End Get
            Set(ByVal value As String)
                mExceptionType = value
            End Set
        End Property
        <DataMember>
        Private mExceptionType As String

        ''' <summary>
        ''' The expression to evaluate as the exception detail
        ''' </summary>
        Public Property ExceptionDetail() As String
            Get
                Return mExceptionDetail
            End Get
            Set(ByVal value As String)
                mExceptionDetail = value
            End Set
        End Property
        Public Property ExceptionDetailForLocalizationSetting() As String
            Get
                If Not String.IsNullOrWhiteSpace(ExceptionLocalized) AndAlso ExceptionLocalized = YES Then
                    Return clsExpression.NormalToLocal(mExceptionDetail)
                Else
                    Return mExceptionDetail
                End If
            End Get
            Set(ByVal value As String)
                If Not String.IsNullOrWhiteSpace(ExceptionLocalized) AndAlso ExceptionLocalized = YES Then
                    mExceptionDetail = clsExpression.LocalToNormal(value)
                Else
                    mExceptionDetail = value
                End If
            End Set
        End Property
        <DataMember>
        Private mExceptionDetail As String

        ''' <summary>
        ''' A Boolean indicating whether to use the current exception details
        ''' </summary>
        Public Property UseCurrentException() As Boolean
            Get
                Return mUseCurrentException
            End Get
            Set(ByVal value As Boolean)
                mUseCurrentException = value
            End Set
        End Property
        <DataMember>
        Private mUseCurrentException As Boolean

        ''' <summary>
        ''' A Boolean indicating whether to save a screen capture when the exception
        ''' occours.
        ''' </summary>
        Public Property SaveScreenCapture As Boolean
            Get
                Return mSaveScreenCapture
            End Get
            Set(value As Boolean)
                mSaveScreenCapture = value
            End Set
        End Property
        <DataMember>
        Private mSaveScreenCapture As Boolean = False


        ''' <summary>
        ''' Constructor for the clsExceptionStage class
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' Creates a new instance of this stage for the purposes of cloning
        ''' </summary>
        ''' <returns></returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsExceptionStage(mParent)
        End Function

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult
            Dim sErr As String = Nothing
            ExceptionPrologue(logger)

            If SaveScreenCapture Then
                Dim timestamp = DateTimeOffset.Now
                Dim image = clsPixRect.CaptureScreen()
                LogExceptionScreenshot(logger, mParent.Name, image, timestamp)
            End If

            Dim res As clsProcessValue = Nothing
            If Not clsExpression.EvaluateExpression(mExceptionDetail, res, Me, False, Nothing, sErr) Then
                Return New StageResult(False, "Internal", String.Format(My.Resources.Resources.clsExceptionStage_ThereWasAnErrorInTheExpressionOfTheExceptionStage0, sErr))
            End If
            If mUseCurrentException Then
                Return New StageResult(False, mParent.mRecoveryType, mParent.mRecoveryDetail)
            End If

            ExceptionEpilogue(logger)
            Return New StageResult(False, mExceptionType, res.FormattedValue)
        End Function

        Private Sub ExceptionPrologue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.ExceptionPrologue(info, Me)
        End Sub

        Private Sub ExceptionEpilogue(logger As CompoundLoggingEngine)
            Dim info = GetLogInfo()
            logger.ExceptionEpilogue(info, Me)
        End Sub

        Private Sub LogExceptionScreenshot(logger As CompoundLoggingEngine, processName As String, ByVal image As clsPixRect, timestamp As DateTimeOffset)
            Dim info = GetLogInfo()
            logger.LogExceptionScreenshot(info, Me, processName, image, timestamp)
        End Sub

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.Exception
            End Get
        End Property

        ''' <summary>
        ''' Returns items referred to by this stage, currently only things defined
        ''' within the process (e.g. data items).
        ''' </summary>
        ''' <param name="inclInternal">Indicates internal references required</param>
        ''' <returns>List of dependencies</returns>
        Public Overrides Function GetDependencies(inclInternal As Boolean) As List(Of clsProcessDependency)
            Dim deps As List(Of clsProcessDependency) = MyBase.GetDependencies(inclInternal)

            If inclInternal Then
                For Each dataItem As String In BPExpression.FromNormalised(ExceptionDetail).GetDataItems()
                    Dim outOfScope As Boolean
                    Dim stage = mParent.GetDataStage(dataItem, Me, outOfScope)
                    If Not outOfScope AndAlso stage IsNot Nothing Then _
                        deps.Add(New clsProcessDataItemDependency(stage))
                Next
            End If

            Return deps
        End Function

        ''' <summary>
        ''' Clones the stage.
        ''' </summary>
        ''' <returns>A deep clone of the stage object</returns>
        Public Overrides Function Clone() As clsProcessStage
            Dim copy As clsExceptionStage = TryCast(MyBase.Clone(), clsExceptionStage)
            copy.ExceptionLocalized = mExceptionLocalized
            copy.ExceptionType = mExceptionType
            copy.ExceptionDetail = mExceptionDetail
            copy.UseCurrentException = mUseCurrentException
            copy.SaveScreenCapture = SaveScreenCapture
            Return copy
        End Function

        Public Overrides Sub FromXML(ByVal e2 As System.Xml.XmlElement)
            MyBase.FromXML(e2)
            For Each e3 As Xml.XmlElement In e2
                Select Case e3.Name
                    Case "exception"
                        mExceptionLocalized = e3.GetAttribute("localized")
                        mExceptionDetail = e3.GetAttribute("detail")
                        If String.IsNullOrWhiteSpace(mExceptionLocalized) AndAlso Not String.IsNullOrWhiteSpace(mExceptionDetail) Then
                            mExceptionLocalized = NO
                        End If
                        mExceptionType = e3.GetAttribute("type")
                        mUseCurrentException = e3.HasAttribute("usecurrent")
                        SaveScreenCapture = e3.HasAttribute("savescreencapture")
                End Select
            Next
        End Sub

        Public Overrides Sub ToXml(ByVal ParentDocument As System.Xml.XmlDocument, ByVal StageElement As System.Xml.XmlElement, ByVal bSelectionOnly As Boolean)
            MyBase.ToXml(ParentDocument, StageElement, bSelectionOnly)

            Dim e2 As Xml.XmlElement
            e2 = ParentDocument.CreateElement("exception")
            If String.IsNullOrWhiteSpace(mExceptionLocalized) Or mExceptionLocalized = YES Then
                mExceptionLocalized = YES
                e2.SetAttribute("localized", mExceptionLocalized)
            End If
            e2.SetAttribute("type", mExceptionType)
            e2.SetAttribute("detail", mExceptionDetail)
            If mUseCurrentException Then e2.SetAttribute("usecurrent", "yes")
            If SaveScreenCapture Then e2.SetAttribute("savescreencapture", "yes")
            StageElement.AppendChild(e2)
        End Sub

        ''' <summary>
        ''' Validation logic for the exception stage.
        ''' </summary>
        ''' <param name="bAttemptRepair">Whether to try and attempt to repair the 
        ''' error or not</param>
        ''' <param name="existingExTypes">A case insensitive collection of exception 
        ''' types already present in the database.</param>
        ''' <returns>A list of validation errors</returns>
        Friend Overrides Function CheckForErrors(ByVal bAttemptRepair As Boolean,
                                                 ByVal SkipObjects As Boolean,
                                                 ByVal existingExTypes As ICollection(Of String)) _
            As ValidationErrorList

            Dim valList As New ValidationErrorList

            ' Validate exception detail expression
            ' If we're not rethrowing, check that the expression is valid
            If Not UseCurrentException Then valList =
                clsExpression.CheckExpressionForErrors(
                    ExceptionDetail, Me, DataType.text, "", Nothing, Nothing)

            ' Now validate exception type if the existing types have been passed in
            ' The comparison is case insensitive
            If ExceptionType IsNot Nothing Then
                Dim trimmed = ExceptionType.Trim()
                If Not existingExTypes.Contains(trimmed) Then
                    valList.Add(Me, 142, trimmed)
                End If
            End If

            Return valList

        End Function
    End Class
End Namespace
