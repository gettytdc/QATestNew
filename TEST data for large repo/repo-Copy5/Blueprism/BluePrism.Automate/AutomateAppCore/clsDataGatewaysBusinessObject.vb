Imports BluePrism.AutomateProcessCore
Imports BluePrism.BPCoreLib
Imports BluePrism.AutomateProcessCore.My.Resources

''' Project  : AutomateAppCore
''' Class    : clsDataGatewaysBusinessObject
''' 
''' <summary>
''' This class represents the Data Gateways Internal Business Object
''' </summary>
Public Class clsDataGatewaysBusinessObject
    Inherits clsInternalBusinessObject

    ''' <summary>
    ''' The new constructor just creates the Internal Business Object Actions
    ''' </summary>
    ''' <param name="process">A reference to the process calling the object</param>
    ''' <param name="session">The session the object is running under</param>
    Public Sub New(ByVal process As clsProcess, ByVal session As clsSession)
        MyBase.New(process, session,
        "Blueprism.Automate.clsDataGatewaysActions", IboResources.clsDataGatewaysActions_DataGateways)

        Narrative = IboResources.clsDataGatewaysActions_ThisInternalBusinessObjectProvidesTheAbilityForProcessesToInteractWithTheDataG
        AddAction(New clsDataGatewaysSendCustomData(Me))
    End Sub

    Public Overrides Function CheckLicense() As Boolean
        Return Licensing.License.CanUse(LicenseUse.Credentials)
    End Function

End Class
''' <summary>
''' Class to hold the parameter names as constants.
''' </summary>
Public Class DataGatewaysParams

    'input parameters
    Public Shared CustomData As String = NameOf(IboResources.clsDataGatewaysActions_Params_CustomData)

    Public Shared Function _T(ByVal param As String) As String
        Return IboResources.ResourceManager.GetString(param, New Globalization.CultureInfo("en"))
    End Function

End Class

''' Project  : AutomateAppCore
''' Class    : clsCredentialsGet
''' 
''' <summary>
''' Implements the "Get" action for the "Credentials" Internal Business Object.
''' </summary>
Public Class clsDataGatewaysSendCustomData
    Inherits clsInternalBusinessObjectAction

    ''' <summary>
    ''' Constructor - sets the details of the action.
    ''' </summary>
    Public Sub New(ByVal parent As clsInternalBusinessObject)
        MyBase.New(parent)
        SetName(NameOf(IboResources.clsDataGatewaysActions_Action_SendCustomData))
        SetNarrative(IboResources.clsDataGatewaysActions_clsDataGatewaysSendCustomData_CollectionOfDataToBeSentToTheDataGateway)

        AddParameter(DataGatewaysParams.CustomData, DataType.collection, ParamDirection.In,
         IboResources.clsDataGatewaysActions_clsDataGatewaysSendCustomData_CustomData)

    End Sub

    ''' <summary>
    ''' Get the endpoint text for this action.
    ''' </summary>
    ''' <returns>The endpoint as a String.</returns>
    Public Overrides Function GetEndpoint() As String
        Return IboResources.clsDataGatewaysActions_EndPoint
    End Function

    ''' <summary>
    ''' Get the preconditions for this action.
    ''' </summary>
    ''' <returns>The preconditions as a Collection containing a String for each
    ''' precondition.</returns>
    Public Overrides Function GetPreConditions() As Collection
        Return BuildCollection(
         IboResources.clsDataGatewaysActions_Preconditions)
    End Function

    ''' <summary>
    ''' Perform the action.
    ''' </summary>
    ''' <param name="process">A reference to the process making the call, or Nothing
    ''' if unknown.</param>
    ''' <param name="sess">The session under which the call is being made.</param>
    ''' <param name="stg">The stage used to resolve the scope.</param>
    ''' <param name="sErr">On return, an error message if unsuccessful</param>
    ''' <returns>True if successful</returns>
    Public Overrides Function [Do](
     ByVal process As clsProcess, ByVal sess As clsSession,
     ByVal stg As clsProcessStage, ByRef sErr As String) As Boolean

        Dim data As clsCollection = Nothing
        Dim collVal As clsProcessValue = Inputs.GetValue(DataGatewaysParams._T(DataGatewaysParams.CustomData))
        If collVal IsNot Nothing Then
            data = collVal.Collection
        Else
            Return False
        End If

        If sess.Identifier.SessionIdentifierType <> SessionIdentifierType.RuntimeResource Then _
            Return False

        Dim sessionNumber = CType(sess.Identifier, RuntimeResourceSessionIdentifier).SessionNumber
            gSv.SendCustomDataToGateway(data, sessionNumber, stg.Id, stg.Name, stg.StageType,
                                     DateTimeOffset.Now, process.Name, stg.GetSubSheetName,
                                     IboResources.clsDataGatewaysActions_DataGateways,
                                     IboResources.clsDataGatewaysActions_Action_SendCustomData)
        Return True
    End Function

End Class
