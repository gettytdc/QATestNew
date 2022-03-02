Imports System.Runtime.Serialization
Imports BluePrism.AutomateProcessCore.Processes

Namespace Stages
    ''' Project  : AutomateProcessCore
    ''' Class    : AutomateProcessCore.Stages.clsEndStage
    ''' 
    ''' <summary>
    ''' The end stage represents the endpoint of a process or the return point of a 
    ''' subsheet.
    ''' </summary>
    <Serializable, DataContract([Namespace]:="bp")>
    Public Class clsEndStage
        Inherits clsProcessStage

        ''' <summary>
        ''' Creates a new instance of the clsEndStage class and sets its parent.
        ''' </summary>
        ''' <param name="parent"></param>
        Public Sub New(ByVal parent As clsProcess)
            MyBase.New(parent)
        End Sub

        ''' <summary>
        ''' A factory method that creates the correct type of object for the clone
        ''' method
        ''' </summary>
        ''' <returns>A new instance of an end stage</returns>
        Public Overrides Function CloneCreate() As clsProcessStage
            Return New clsEndStage(mParent)
        End Function

        ''' <summary>
        ''' Get the type of this stage.
        ''' </summary>
        Public Overrides ReadOnly Property StageType() As StageTypes
            Get
                Return StageTypes.End
            End Get
        End Property

        Public Overrides Function Execute(ByRef gRunStageID As Guid, logger As CompoundLoggingEngine) As StageResult

            Dim sErr As String = Nothing

            'The end stage just needs to deal with storing the
            'output parameters in XML format, ready for collection
            'by the caller.
            Dim aParms As List(Of clsProcessParameter)
            aParms = GetParameters()
            Dim inputs As clsArgumentList = Nothing
            If Not mParent.ReadDataItemsFromParameters(aParms, Me, inputs, sErr, False, True) Then
                Return New StageResult(False, "Internal", sErr)
            End If
            Dim gSubSheetID As Guid = GetSubSheetID()
            If gSubSheetID.Equals(mParent.GetMainPage.ID) OrElse (mParent.ProcessType = DiagramType.Object AndAlso gSubSheetID.Equals(mParent.GetCleanupPage.ID)) Then
                'This is a real end stage...
                mParent.mOutputParameters = inputs

                ProcessEpilogue(logger, mParent.mOutputParameters)

                mParent.RunState = ProcessRunState.Completed
            Else
                'This is a subsheet end, so just deal with the
                'outputs and go back...

                'Check that we have a subsheet ref stage to return to
                If mParent.mSubSheetStack Is Nothing OrElse mParent.mSubSheetStack.Count = 0 Then
                    Select Case mParent.ProcessType
                        Case DiagramType.Process
                            Return New StageResult(False, "Internal", My.Resources.Resources.clsEndStage_EndOfSubsheetWithoutCorrespondingStart)
                        Case DiagramType.Object
                            'this is ok - we have simply finished the current action

                            'The onsuccess next stage will be undefined since we are now at the
                            'end of a business object action, with nothing left on the 
                            'subsheet call stack. It is the caller's job to move on;
                            'we just blank the current runstage
                            gRunStageID = Guid.Empty

                            mParent.mOutputParameters = inputs
                            mParent.RunState = ProcessRunState.Completed

                            Dim objLogProcessSubSheet As clsProcessSubSheet = mParent.GetSubSheetByID(Me.GetSubSheetID)
                            If objLogProcessSubSheet.Published Then
                                ObjectEpilogue(logger, mParent.Name, objLogProcessSubSheet.Name, mParent.mOutputParameters)
                            Else
                                SubSheetEpilogue(logger, mParent.mOutputParameters)
                            End If

                            Return New StageResult(True)
                    End Select
                End If


                gRunStageID = mParent.mSubSheetStack.Pop()

                'Store outputs in the appropriate places...
                Dim outputs As clsArgumentList = inputs
                If Not outputs Is Nothing Then
                    If Not mParent.StoreParametersInDataItems(mParent.GetStage(gRunStageID), outputs, sErr) Then
                        Return New StageResult(False, "Internal", sErr)
                    End If
                End If


                Dim objProcessSubSheet As clsProcessSubSheet = mParent.GetSubSheetByID(Me.GetSubSheetID)
                If mParent.ProcessType = DiagramType.Object AndAlso objProcessSubSheet.Published Then
                    ObjectEpilogue(logger, mParent.Name, objProcessSubSheet.Name, outputs)
                Else
                    SubSheetEpilogue(logger, outputs)
                End If
                Dim refStage = TryCast(mParent.GetStage(gRunStageID), clsSubSheetRefStage)
                If refStage IsNot Nothing Then
                    refStage.SubSheetRefEpilogue(logger, outputs)
                End If


                If Not mParent.UpdateNextStage(gRunStageID, LinkType.OnSuccess, sErr) Then
                    Return New StageResult(False, "Internal", sErr)
                End If
            End If

            Return New StageResult(True)

        End Function

        Private Sub ObjectEpilogue(logger As CompoundLoggingEngine, sResourceName As String, sActionName As String, outputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.ObjectEpiLogue(info, Me, sResourceName, sActionName, outputs)
        End Sub

        Private Sub ProcessEpilogue(logger As CompoundLoggingEngine, outputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.ProcessEpiLogue(info, Me, outputs)
        End Sub

        Private Sub SubSheetEpilogue(logger As CompoundLoggingEngine, outputs As clsArgumentList)
            Dim info = GetLogInfo()
            logger.SubSheetEpiLogue(info, Me, outputs)
        End Sub


        ''' <summary>
        ''' Get the first found end stage that is not this stage, and is on the same subsheet.
        ''' </summary>
        ''' <returns>the first found end stage or nothing if no end stages are found</returns>
        ''' <remarks></remarks>
        Public Function GetMainEnd() As clsProcessStage
            ' Check every stage in the process...
            For Each stg As clsProcessStage In Process.GetStages()
                If stg IsNot Me AndAlso stg.StageType = StageTypes.End _
                 AndAlso stg.SubSheet Is Me.SubSheet Then Return stg
            Next
            Return Nothing
        End Function

        ''' <summary>
        ''' When creating new stages update the new stages parameters to match already existing end stages.
        ''' </summary>
        Public Sub UpdateParametersToMatchMainEnd()
            Dim stg As clsProcessStage = GetMainEnd()
            If stg IsNot Nothing Then
                ClearParameters()
                AddParameters(stg.GetParameters())
            End If
        End Sub

        ''' <summary>
        ''' Duplicate the information in this stage to all other end stages in the parent process
        ''' </summary>
        Public Sub UpdateEndStages()
            ' Check every end stage in the the same subsheet as this one...
            For Each stg As clsEndStage In Process.GetStages(Of clsEndStage)(SubSheet.ID)
                If stg Is Me Then Continue For
                stg.ClearParameters()
                stg.AddParameters(GetParameters())
            Next
        End Sub

        Public Overrides Function GetShortText() As String
            Return "End"
        End Function
    End Class
End Namespace
