

Imports BluePrism.AutomateProcessCore
Imports BluePrism.AutomateProcessCore.My.Resources

''' <summary>
''' Business object to expose encryption functionality to processes.
''' </summary>
Public Class clsEncryptionBusinessObject : Inherits clsInternalBusinessObject

    ''' <summary>
    ''' Creates a new encryption business object.
    ''' </summary>
    ''' <param name="process">The process using the object.</param>
    ''' <param name="session">The session in which the object is being used.</param>
    Public Sub New(ByVal process As clsProcess, ByVal session As clsSession)
        MyBase.New(process, session,
         "EncryptionBusinessObject",
         IboResources.EncryptionBusinessObject_Encryption,
         IboResources.EncryptionBusinessObject_ThisInternalBusinessObjectProvidesTheAbilityForProcessesToEncryptOrDecryptDataU,
         New EncryptText(),
         New DecryptText(),
         New EncryptPassword(),
         New DecryptPassword(),
         New EncryptCollection(),
         New DecryptCollection(),
         New EncryptNumber(),
         New DecryptNumber(),
         New EncryptFlag(),
         New DecryptFlag(),
         New EncryptDate(),
         New DecryptDate(),
         New EncryptDateTime(),
         New DecryptDateTime(),
         New EncryptTime(),
         New DecryptTime(),
         New EncryptTimeSpan(),
         New DecryptTimeSpan(),
         New EncryptBinary(),
         New DecryptBinary()
        )

    End Sub

    Public Overrides Function CheckLicense() As Boolean
        Return True
    End Function

#Region " Action Base Classes "

    ''' <summary>
    ''' Internal action within the encryption business object.
    ''' </summary>
    Private MustInherit Class EncryptionAction
        Inherits clsInternalBusinessObjectAction

        ''' <summary>
        ''' A class encapsulating the parameter names used in encryption actions
        ''' </summary>
        Protected Class Params
            Public Shared Scheme As String = NameOf(IboResources.EncryptionBusinessObject_Params_EncryptionScheme)
            Public Shared PlainValue As String = NameOf(IboResources.EncryptionBusinessObject_Params_PlainValue)
            Public Shared EncryptedValue As String = NameOf(IboResources.EncryptionBusinessObject_Params_EncryptedValue)
        End Class
        Public Shared Function _T(ByVal param As String) As String
            Return IboResources.ResourceManager.GetString(param, New Globalization.CultureInfo("en"))
        End Function

        ''' <summary>
        ''' The encryption business object that this action belongs to.
        ''' </summary>
        Protected ReadOnly Property EncryptionObject() As clsEncryptionBusinessObject
            Get
                Return DirectCast(Me.Parent, clsEncryptionBusinessObject)
            End Get
        End Property

        ''' <summary>
        ''' Performs the task required of this action
        ''' </summary>
        ''' <param name="proc">The process calling this action</param>
        ''' <param name="sess">The session in which the action is called</param>
        ''' <param name="stg">The stage that calls this action - for scoping purposes
        ''' </param>
        ''' <param name="sErr">On return, a message indicating any errors.</param>
        ''' <returns>True on success; False on failure.</returns>
        Public Overrides Function [Do](ByVal proc As clsProcess,
         ByVal sess As clsSession, ByVal stg As clsProcessStage,
         ByRef sErr As String) As Boolean

            Dim scheme As String = Inputs.GetString(_T(Params.Scheme))
            If scheme = "" Then Return SendError(sErr,
             IboResources.EncryptionBusinessObject_EncryptionAction_NoEncryptionSchemeWasSpecifiedInThe0Action, FriendlyName)

            Try
                If gSv.HasEncryptionScheme(scheme) Then
                    If gSv.CheckSchemeForFIPSCompliance(scheme) Then
                        'Perform the action
                        Process(scheme)
                        Return True
                    Else
                        Return SendError(sErr,
                     IboResources.EncryptionBusinessObject_EncryptionAction_AnErrorOccurredInTheEncryptionScheme0isnotfipscompliantinaction1,
                     scheme, FriendlyName)
                    End If
                Else
                    Return SendError(sErr,
                     IboResources.EncryptionBusinessObject_EncryptionAction_AnErrorOccurredInThe0ActionNoEncrypterDecrypterWithTheName1WasFound,
                     FriendlyName, scheme)

                End If

            Catch ex As Exception
                Return SendError(sErr, IboResources.EncryptionBusinessObject_EncryptionAction_AnErrorOccurredInThe0Action1,
                 FriendlyName, ex.Message)

            End Try

        End Function

        ''' <summary>
        ''' Processes this encryption action
        ''' </summary>
        ''' <param name="scheme">The scheme to use for the encryption. This will have
        ''' been checked already to ensure it is recognised in this system.</param>
        Protected MustOverride Sub Process(ByVal scheme As String)

        ''' <summary>
        ''' Gets the endpoint for this action
        ''' </summary>
        ''' <returns>The endpoint reached after successful execution of this action.
        ''' </returns>
        Public MustOverride Overrides Function GetEndpoint() As String

        ''' <summary>
        ''' Gets the preconditions for this action
        ''' </summary>
        ''' <returns>A collection of preconditions for this action.</returns>
        Public Overrides Function GetPreConditions() As Collection
            Return SingletonPreCondition(
             IboResources.EncryptionBusinessObject_EncryptionAction_TheSpecifiedEncryptionSchemeMustBeAvailableToTheServerAndHaveAValidKeyEitherOnT)
        End Function

        ''' <summary>
        ''' The datatype to be used for the plain version of the data. By default,
        ''' this is text, but it should be overridden as appropriate by subclasses
        ''' </summary>
        Protected MustOverride ReadOnly Property ValueType() As DataType

    End Class

    ''' <summary>
    ''' Base action which encrypts data.
    ''' </summary>
    Private MustInherit Class EncryptValueAction : Inherits EncryptionAction

        ''' <summary>
        ''' Creates a new EncryptText action
        ''' </summary>
        Public Sub New()
            SetName(NameOf(IboResources.EncryptionBusinessObject_Action_Encrypt), ValueType)
            SetNarrative(IboResources.EncryptionBusinessObject_EncryptValueAction_EncryptsTheGivenValueUsingTheSpecifiedScheme)

            AddParameter(Params.Scheme, DataType.text, ParamDirection.In,
             IboResources.EncryptionBusinessObject_EncryptValueAction_TheNameOfTheSchemeWhichShouldEncryptTheData)

            AddParameter(Params.PlainValue, ValueType, ParamDirection.In,
             IboResources.EncryptionBusinessObject_EncryptValueAction_The0ValueToBeEncrypted, clsProcessDataTypes.GetFriendlyName(ValueType, False, True))

            AddParameter(Params.EncryptedValue, DataType.text, ParamDirection.Out,
             IboResources.EncryptionBusinessObject_EncryptValueAction_TheEncryptedValueInTextForm)
        End Sub

        ''' <summary>
        ''' Gets the endpoint for this action
        ''' </summary>
        ''' <returns>The endpoint indicating that an encrypted value is returned
        ''' </returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.EncryptionBusinessObject_EncryptValueAction_TheEncryptedValueIsReturned
        End Function

        ''' <summary>
        ''' Processes the arguments given in this action, encrypting the plain value
        ''' </summary>
        ''' <param name="scheme">The scheme to use to encrypt the data</param>
        ''' <remarks>This implementation gets the encoded value of the input argument
        ''' and encrypts that. If that is not sufficient for the encryption action,
        ''' this method needs to be overridden by subclasses.</remarks>
        Protected Overrides Sub Process(ByVal scheme As String)
            Dim plainValue As String = Inputs.GetString(_T(Params.PlainValue))
            Dim encValue As String = ""
            If plainValue <> "" Then encValue = gSv.Encrypt(scheme, plainValue)
            AddOutput(_T(Params.EncryptedValue), DataType.text, encValue)
        End Sub

    End Class

    ''' <summary>
    ''' Base class for decryption actions - this just acts a single place where a
    ''' common endpoint can be implemented.
    ''' </summary>
    Private MustInherit Class DecryptValueAction : Inherits EncryptionAction

        Public Sub New()
            Dim locValueType = clsProcessDataTypes.GetFriendlyName(ValueType, False, True)
            SetName(NameOf(IboResources.EncryptionBusinessObject_Action_Decrypt), ValueType)
            SetNarrative(
             IboResources.EncryptionBusinessObject_DecryptValueAction_DecryptsTheGivenValueIntoA0DataItemUsingTheSpecifiedScheme, locValueType)

            AddParameter(Params.Scheme, DataType.text, ParamDirection.In,
             IboResources.EncryptionBusinessObject_DecryptValueAction_TheNameOfTheSchemeWhichShouldDecryptTheData)

            AddParameter(Params.EncryptedValue, DataType.text, ParamDirection.In,
             IboResources.EncryptionBusinessObject_DecryptValueAction_TheEncryptedDataToBeDecryptedIntoA0DataItem, locValueType)

            AddParameter(Params.PlainValue, ValueType, ParamDirection.Out,
             IboResources.EncryptionBusinessObject_DecryptValueAction_TheDecrypted0Value, locValueType)

        End Sub

        ''' <summary>
        ''' Gets the endpoint for decrypting a value - ie. that the plain text value
        ''' is returned.
        ''' </summary>
        ''' <returns>The endpoint indicating that a plaintext value is returned.
        ''' </returns>
        Public Overrides Function GetEndpoint() As String
            Return IboResources.EncryptionBusinessObject_DecryptValueAction_TheDecryptedValueIsReturned
        End Function

        ''' <summary>
        ''' Process the specific decryption action
        ''' </summary>
        ''' <param name="scheme">The scheme to use to decrypt the data.</param>
        ''' <remarks>This implementation just decrypts the value given and then adds
        ''' the output as a process value of the specified <see cref="ValueType"/>
        ''' with decrypted text as its encoded value. If this implementation is not
        ''' sufficient, it should be overridden by subclasses. If this is enough,
        ''' then the <see cref="ValueType"/> should be overridden to indicate what
        ''' type of process value is required by the action.</remarks>
        Protected Overrides Sub Process(ByVal scheme As String)
            Dim encValue As String = Inputs.GetString(_T(Params.EncryptedValue))
            Dim plainValue As String = ""
            If encValue <> "" Then plainValue = gSv.Decrypt(scheme, encValue)
            AddOutput(_T(Params.PlainValue), ValueType, plainValue)
        End Sub

    End Class

#End Region

#Region " Encrypt / Decrypt Text "

    ''' <summary>
    ''' Action used to encrypt a value.
    ''' </summary>
    Private Class EncryptText : Inherits EncryptValueAction

        ''' <summary>
        ''' The value type being encrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.text
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Action to decrypt a text value.
    ''' </summary>
    Private Class DecryptText : Inherits DecryptValueAction

        ''' <summary>
        ''' The value type being decrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.text
            End Get
        End Property

    End Class

#End Region

#Region " Encrypt / Decrypt Password "

    ''' <summary>
    ''' Action used to encrypt a password value.
    ''' </summary>
    Private Class EncryptPassword : Inherits EncryptValueAction

        ''' <summary>
        ''' The value type being encrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.password
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Action to decrypt a password value.
    ''' </summary>
    Private Class DecryptPassword : Inherits DecryptValueAction

        ''' <summary>
        ''' The value type being decrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.password
            End Get
        End Property

    End Class

#End Region

#Region " Encrypt / Decrypt Collection "

    ''' <summary>
    ''' Action to encrypt a collection
    ''' </summary>
    Private Class EncryptCollection : Inherits EncryptValueAction

        ''' <summary>
        ''' The value type being encrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.collection
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Class to decrypt a collection using a specified encryption/decryption scheme
    ''' </summary>
    Private Class DecryptCollection : Inherits DecryptValueAction

        ''' <summary>
        ''' The value type being decrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.collection
            End Get
        End Property

    End Class

#End Region

#Region " Encrypt / Decrypt Number "

    ''' <summary>
    ''' Action used to encrypt a number.
    ''' </summary>
    Private Class EncryptNumber : Inherits EncryptValueAction

        ''' <summary>
        ''' The value type being encrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.number
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Action to decrypt a number value.
    ''' </summary>
    Private Class DecryptNumber : Inherits DecryptValueAction

        ''' <summary>
        ''' The value type being decrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.number
            End Get
        End Property

    End Class

#End Region

#Region " Encrypt / Decrypt Flag "

    ''' <summary>
    ''' Action used to encrypt a flag value.
    ''' </summary>
    Private Class EncryptFlag : Inherits EncryptValueAction

        ''' <summary>
        ''' The value type being encrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.flag
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Action to decrypt a flag value.
    ''' </summary>
    Private Class DecryptFlag : Inherits DecryptValueAction

        ''' <summary>
        ''' The value type being decrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.flag
            End Get
        End Property

    End Class

#End Region

#Region " Encrypt / Decrypt Date "

    ''' <summary>
    ''' Action used to encrypt a date.
    ''' </summary>
    Private Class EncryptDate : Inherits EncryptValueAction

        ''' <summary>
        ''' The value type being encrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.date
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Action to decrypt a date value.
    ''' </summary>
    Private Class DecryptDate : Inherits DecryptValueAction

        ''' <summary>
        ''' The value type being decrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.date
            End Get
        End Property

    End Class

#End Region

#Region " Encrypt / Decrypt DateTime "

    ''' <summary>
    ''' Action used to encrypt a datetime value.
    ''' </summary>
    Private Class EncryptDateTime : Inherits EncryptValueAction

        ''' <summary>
        ''' The value type being encrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.datetime
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Action to decrypt a datetime value.
    ''' </summary>
    Private Class DecryptDateTime : Inherits DecryptValueAction

        ''' <summary>
        ''' The value type being decrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.datetime
            End Get
        End Property

    End Class

#End Region

#Region " Encrypt / Decrypt TimeSpan "

    ''' <summary>
    ''' Action used to encrypt a timespan value.
    ''' </summary>
    Private Class EncryptTimeSpan : Inherits EncryptValueAction

        ''' <summary>
        ''' The value type being encrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.timespan
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Action to decrypt a timespan value.
    ''' </summary>
    Private Class DecryptTimeSpan : Inherits DecryptValueAction

        ''' <summary>
        ''' The value type being decrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.timespan
            End Get
        End Property

    End Class

#End Region

#Region " Encrypt / Decrypt Time "

    ''' <summary>
    ''' Action used to encrypt a time value.
    ''' </summary>
    Private Class EncryptTime : Inherits EncryptValueAction

        ''' <summary>
        ''' The value type being encrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.time
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Action to decrypt a time value.
    ''' </summary>
    Private Class DecryptTime : Inherits DecryptValueAction

        ''' <summary>
        ''' The value type being decrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.time
            End Get
        End Property

    End Class

#End Region

#Region " Encrypt / Decrypt Binary "

    ''' <summary>
    ''' Action used to encrypt a password value.
    ''' </summary>
    Private Class EncryptBinary : Inherits EncryptValueAction

        ''' <summary>
        ''' Processes the encryption of this binary data.
        ''' </summary>
        ''' <param name="scheme">The scheme that should be used to encrypt the data.
        ''' </param>
        Protected Overrides Sub Process(ByVal scheme As String)
            Dim binVal As clsProcessValue = Inputs.GetValue(_T(Params.PlainValue))
            Dim valStr As String = Convert.ToBase64String(CType(binVal, Byte()))
            AddOutput(_T(Params.EncryptedValue), DataType.text,
             gSv.Encrypt(scheme, valStr))
        End Sub

        ''' <summary>
        ''' The value type being encrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.binary
            End Get
        End Property

    End Class

    ''' <summary>
    ''' Action to decrypt a binary value.
    ''' </summary>
    Private Class DecryptBinary : Inherits DecryptValueAction

        ''' <summary>
        ''' Processes the encryption of this binary data.
        ''' </summary>
        ''' <param name="scheme">The scheme that should be used to encrypt the data.
        ''' </param>
        Protected Overrides Sub Process(ByVal scheme As String)
            Dim plainStr As String =
             gSv.Decrypt(scheme, Inputs.GetString(_T(Params.EncryptedValue)))
            AddOutput(_T(Params.PlainValue), Convert.FromBase64String(plainStr))
        End Sub

        ''' <summary>
        ''' The value type being decrypted by this action
        ''' </summary>
        Protected Overrides ReadOnly Property ValueType() As DataType
            Get
                Return DataType.binary
            End Get
        End Property

    End Class

#End Region

End Class
