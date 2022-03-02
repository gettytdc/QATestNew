Imports System.Runtime.Serialization

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsSubSheetRefStage
    ''' 
    ''' <summary>
    ''' The sub sheet reference stage holds a reference to another sheet. The subsheet
    ''' reference is so similar to the process reference stage that all of the functionality
    ''' is inheritied from clsSubProcessRefStage
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsSubSheetRefStage
        Inherits clsSubProcessRefStage

        ''' <summary>
        ''' Creates a new instance of the clsActionStage class and sets its parent
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone method
        ''' </summary>
        ''' <returns>A new instance of an Process Info stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsSubSheetRefStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.SubSheet
            End Get
        End Property

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            Dim sErr As String = Nothing

            'Execute internal subsheet...
            Dim gSubID As Guid = ReferenceId
            If gSubID.Equals(Guid.Empty) OrElse mParent.GetSubSheetStartStage(gSubID).Equals(Guid.Empty) Then
                Return New StageResult(False, "Internal", My.Resources.Resources.ThePageReferencedIsEitherMissingOrInvalid)
            End If

            'Get the inputsXML for the subsheet
            Dim Inputs As clsArgumentList = Nothing
            If Not mParent.ReadDataItemsFromParameters(mParent.GetStage(mParent.GetSubSheetStartStage(gSubID)).GetParameters(), Me, Inputs, sErr, True, False) Then
                Return New StageResult(False, "Internal", sErr)
            End If

            SubSheetRefPrologue(logger, Inputs)


            'Set run stage to start of subsheet...
            mParent.mSubSheetStack.Push(GetStageID())
            gRunStageID = mParent.GetSubSheetStartStage(gSubID)
            mParent.mRunInputs = Inputs

            Return New StageResult(True)

        End Function

        Private Sub SubSheetRefPrologue(logger As CompoundLoggingEngine, ByVal inputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.SubSheetRefProLogue(info, Me, inputs)
        End Sub

        Friend Sub SubSheetRefEpilogue(logger As CompoundLoggingEngine, ByVal outputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.SubSheetRefEpiLogue(info, Me, outputs)
        End Sub

        Public Overrides Function CheckForErrors(ByVal bAttemptRepair As Boolean, ByVal SkipObjects As Boolean) As ValidationErrorList
            Dim errors As ValidationErrorList = MyBase.CheckForErrors(bAttemptRepair, SkipObjects)

            If StageType = StageTypes.SubSheet Then
                Dim objSubsheetRef As clsSubSheetRefStage = CType(Me, clsSubSheetRefStage)
                Dim SheetID As Guid = objSubsheetRef.ReferenceId

                'Check for page references to non-existent pages
                Dim validReference As Boolean = True
                If SheetID.Equals(Guid.Empty) Then
                    validReference = False
                    errors.Add(New ValidateProcessResult(Me, 109))
                Else
                    Dim sheet As clsProcessSubSheet = mParent.GetSubSheetByID(SheetID)
                    If sheet Is Nothing Then
                        validReference = False
                        errors.Add(New ValidateProcessResult(Me, 110))
                    End If
                End If

                'Check that the inputs/outputs in this ref stage match the signature
                'published by the target. If we define params which no longer exist then
                'that is not so bad, but when a param is not defined here it causes
                'an RTE. See bug 4552
                If validReference Then

                    'Assess the inputs locally and on the target page
                    Dim TargetStartStageID As Guid = mParent.GetSubSheetStartStage(Me.ReferenceId)
                    If Not TargetStartStageID.Equals(Guid.Empty) Then
                        Dim TargetStartStage As clsStartStage = CType(mParent.GetStage(TargetStartStageID), clsStartStage)

                        'Any input params not defined locally should result in an error
                        For Each TargetInputParam As clsProcessParameter In TargetStartStage.GetInputs
                            If Not IsParameterDefined(TargetInputParam, Me.GetInputs) Then
                                errors.Add(New ValidateProcessResult(Me, 111, TargetInputParam.Name))
                            End If
                        Next

                        'Any params defined locally which no longer exist on the target page should
                        'result in a warning. These are repairable.
                        For Each LocalInputParam As clsProcessParameter In Me.GetInputs
                            If Not IsParameterDefined(LocalInputParam, TargetStartStage.GetInputs) Then
                                If bAttemptRepair Then
                                    MyBase.RemoveParameter(LocalInputParam)
                                Else
                                    errors.Add(New RepairableValidateProcessResult(Me, 66, LocalInputParam.Name))
                                End If
                            End If
                        Next
                    End If


                    'Assess the inputs locally and on the target page
                    Dim TargetEndStageId As Guid = mParent.GetSubSheetEndStage(Me.ReferenceId)
                    If TargetEndStageId <> Guid.Empty Then
                        Dim TargetEndStage As clsEndStage = CType(mParent.GetStage(TargetEndStageId), clsEndStage)

                        'Any output params not defined locally should result in an error
                        For Each TargetOutputParam As clsProcessParameter In TargetEndStage.GetOutputs
                            If Not IsParameterDefined(TargetOutputParam, Me.GetOutputs) Then
                                errors.Add(New ValidateProcessResult(Me, 112, TargetOutputParam.Name))
                            End If
                        Next

                        'Any params defined locally which no longer exist on the target page should
                        'result in a warning. These are repairable.
                        For Each LocalOutputParam As clsProcessParameter In Me.GetOutputs
                            If Not IsParameterDefined(LocalOutputParam, TargetEndStage.GetOutputs) Then
                                If bAttemptRepair Then
                                    MyBase.RemoveParameter(LocalOutputParam)
                                Else
                                    errors.Add(New RepairableValidateProcessResult(Me, 67, LocalOutputParam.Name))
                                End If
                            End If
                        Next
                    End If
                End If

            End If

            Return errors
        End Function

        ''' <summary>
        ''' Determines whether the supplied parameter can be found in the List
        ''' provided.
        ''' </summary>
        ''' <param name="paramToLocate">The parameter of interest. The collection
        ''' will be searched for a parameter of matching name and datatype.</param>
        ''' <param name="searchList">The List to be searched for the
        ''' parameter of interest</param>
        ''' <returns>Returns True if the search collection contains the parameter
        ''' of interest; the comparison is performed by comparing the name and data
        ''' type of each parameter.</returns>
        Private Function IsParameterDefined(ByVal paramToLocate As clsProcessParameter, ByVal searchList As List(Of clsProcessParameter)) As Boolean

            For Each testParam As clsProcessParameter In searchList
                If testParam.GetDataType = paramToLocate.GetDataType AndAlso
                 testParam.Name = paramToLocate.Name Then
                    Return True
                End If
            Next

            Return False
        End Function


    End Class

End Namespace
