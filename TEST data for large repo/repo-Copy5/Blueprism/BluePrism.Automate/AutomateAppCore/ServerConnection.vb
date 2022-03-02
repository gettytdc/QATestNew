''' <summary>
''' Class that exposes the Mode enum, which describe the different types of
''' connection mode i.e. .NET Remoting or WCF. Also contains shared functions that 
''' get readable descriptions and help text for the different modes.
''' </summary>
Public Module ServerConnection

    ''' <summary>
    ''' The different connection modes that are available for the Blue Prism Server.
    ''' </summary>
    <TypeConverter(GetType(ModeEnumConverter))>
    Public Enum Mode
        ''' <summary>
        ''' Requires trust relationship between devices: Yes
        ''' Blue Prism Authentication Modes: Blue Prism Native / Single Sign-on
        ''' Requires server-side certificate: No
        ''' Transport: SOAP over HTTP
        ''' Only the message content is encrypted.  The SOAP and HTTP headers remain 
        ''' unencrypted which assists complex routing, load balancers, proxies etc. 
        ''' Client and server identity is validated via Windows / Active Directory.
        ''' </summary>
        WCFSOAPMessageWindows = 0
        ''' <summary>
        ''' Requires trust relationship between devices: Yes
        ''' Blue Prism Authentication Modes: Blue Prism Native / Single Sign-on
        ''' Requires server-side certificate: Yes
        ''' Transport: SOAP over HTTPS
        ''' The transport including SOAP headers are encrypted using certificate-based 
        ''' encryption. Client and server identity is validated via 
        ''' Windows / Active Directory.
        ''' </summary>
        WCFSOAPTransportWindows = 1
        ''' <summary>
        ''' Requires trust relationship between devices: No
        ''' Blue Prism Authentication Modes: Blue Prism Native
        ''' Requires server-side certificate: Yes
        ''' Transport: SOAP over HTTPS
        ''' The transport including SOAP headers are encrypted using certificate-based 
        ''' encryption. Server identity is validated using certificates.
        ''' </summary>
        WCFSOAPTransport = 2
        ''' <summary>
        ''' Requires trust relationship between devices: Yes
        ''' Blue Prism Authentication Modes: Blue Prism Native / Single Sign-on
        ''' Requires server-side certificate: No
        ''' Transport: TcpChannel over SChannel
        ''' Provided for backwards compatibility.
        ''' Encryption is negotiated between the client and server. 
        ''' Client and server identity is validated via Windows / Active Directory.
        ''' </summary>
        DotNetRemotingSecure = 3
        ''' <summary>
        ''' Requires trust relationship between devices: No
        ''' Blue Prism Authentication Modes: Blue Prism Native
        ''' Requires server-side certificate: No
        ''' Transport: TcpChannel
        ''' Not recommended - provided for backwards compatibility.
        ''' Connection security will need to be provided entirely by third-party 
        ''' solutions.
        ''' </summary>
        DotNetRemotingInsecure = 4
        ''' <summary>
        ''' Requires trust relationship between devices: No
        ''' Blue Prism Authentication Modes: Blue Prism Native
        ''' Requires server-side certificate: No
        ''' Transport: SOAP over HTTP
        ''' Not recommended.
        ''' Connection security will need to be provided entirely by third-party 
        ''' solutions.
        ''' </summary>
        WCFInsecure = 5

    End Enum

    ''' <summary>
    ''' Converts the <see cref="ServerConnection.Mode"/> enum to a readable string.
    ''' The convertor is used to retrieve descriptions for a combo box, if the combo
    ''' box data source is set to use enums.
    ''' </summary>
    Private Class ModeEnumConverter
        Inherits EnumConverter
        ''' <summary>
        ''' Create a new instance of the <see cref="ModeEnumConverter"/> class, 
        ''' although it is recommended that this constructor isn't used and the 
        ''' converter is accessed using <see cref="TypeDescriptor"/> instead.
        ''' </summary>
        Public Sub New(type As Type)
            MyBase.New(type)
        End Sub

        ''' <summary>
        ''' Converts the <see cref="ServerConnection.Mode"/> enum to a readable 
        ''' string.
        ''' </summary>
        ''' <remarks>
        ''' This function is called when setting enums to be the data source of a 
        ''' combo box. We override this so the enums in the drop down list have 
        ''' readable descriptions.
        ''' </remarks>
        Public Overrides Function ConvertTo(context As ITypeDescriptorContext,
                                            culture As Globalization.CultureInfo,
                                            value As Object, destinationType As Type) _
                                        As Object

            If destinationType Is GetType(String) Then
                Return ServerConnection.GetDescription(CType(value,
                                                       ServerConnection.Mode))
            Else
                Return MyBase.ConvertTo(context, culture, value, destinationType)
            End If

        End Function

    End Class

    ''' <summary>
    ''' Returns a more readable description for the connection mode enum.
    ''' </summary>
    ''' <param name="mode">The connection mode to get the readable description for.
    ''' </param>
    ''' <returns>A readable description for the connection mode</returns>
    ''' <exception cref="ArgumentException">If GetDescription has not yet been
    '''  implemented for <paramref name="mode"/> </exception>
    Public Function GetDescription(mode As Mode) As String
        Select Case mode
            Case Mode.WCFSOAPMessageWindows
                Return My.Resources.ServerConnection_WCFSOAPWithMessageEncryptionWindowsAuthentication
            Case Mode.WCFSOAPTransportWindows
                Return My.Resources.ServerConnection_WCFSOAPWithTransportEncryptionWindowsAuthentication
            Case Mode.WCFSOAPTransport
                Return My.Resources.ServerConnection_WCFSOAPWithTransportEncryption
            Case Mode.DotNetRemotingSecure
                Return My.Resources.ServerConnection_NETRemotingSecure
            Case Mode.DotNetRemotingInsecure
                Return My.Resources.ServerConnection_NETRemotingInsecure
            Case Mode.WCFInsecure
                Return My.Resources.ServerConnection_WCFInsecure
            Case Else
                Throw New ArgumentException(
                    My.Resources.ServerConnection_ThisConnectionModeCannotBeConvertedIntoAStringAsItHasNotBeenImplementedInThisFu,
                    mode.ToString())
        End Select
    End Function

    ''' <summary>
    ''' Returns a brief summary of the connection mode, and when you should and 
    ''' shouldn't use it.
    ''' </summary>
    ''' <param name="mode">The connection mode to get the help text.
    ''' </param>
    ''' <returns>The help text for the connection mode</returns>
    ''' <exception cref="ArgumentException">If GetHelpText has not yet been
    ''' implemented for <paramref name="mode"/> </exception>
    Public Function GetHelpText(mode As Mode) As String
        Select Case mode
            Case Mode.WCFSOAPMessageWindows
                Return My.Resources.ServerConnection_WCFSOAPMessageWindowsRequiresTrustRelationshipBetweenDevicesYesBluePrismAuthenticationModesBluePrism
            Case Mode.WCFSOAPTransportWindows
                Return My.Resources.ServerConnection_WCFSOAPTransportWindowsRequiresTrustRelationshipBetweenDevicesYesBluePrismAuthenticationModesBluePrism
            Case Mode.WCFSOAPTransport
                Return My.Resources.ServerConnection_WCFSOAPTransportRequiresTrustRelationshipBetweenDevicesNoBluePrismAuthenticationModesBluePrismN
            Case Mode.DotNetRemotingSecure
                Return My.Resources.ServerConnection_DotNetRemotingSecureRequiresTrustRelationshipBetweenDevicesYesBluePrismAuthenticationModesBluePrism
            Case Mode.DotNetRemotingInsecure
                Return My.Resources.ServerConnection_DotNetRemotingInsecureRequiresTrustRelationshipBetweenDevicesNoBluePrismAuthenticationModesBluePrismN
            Case Mode.WCFInsecure
                Return My.Resources.ServerConnection_WCFInsecureRequiresTrustRelationshipBetweenDevicesNoBluePrismAuthenticationModesBluePrismN
            Case Else
                Throw New ArgumentException(
                    My.Resources.ServerConnection_ThisConnectionModeCannotBeConvertedIntoAStringAsItHasNotBeenImplementedInThisFu,
                    mode.ToString())
        End Select
    End Function

End Module
