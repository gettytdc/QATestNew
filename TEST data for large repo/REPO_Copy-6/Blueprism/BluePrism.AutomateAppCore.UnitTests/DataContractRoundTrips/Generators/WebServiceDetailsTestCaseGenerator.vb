#If UNITTESTS Then
Imports BluePrism.AutomateProcessCore
Imports BluePrism.Common.Security

Namespace DataContractRoundTrips.Generators

    Public Class WebServiceDetailsTestCaseGenerator
        Inherits TestCaseGenerator


        Public Overrides Iterator Function GetTestCases() As IEnumerable(Of IRoundTripTestCase)
            Dim webServiceDetails = New clsWebServiceDetails()
            webServiceDetails.Actions.Add("Action1", True)
            webServiceDetails.Actions.Add("Action2", False)
            webServiceDetails.Certificate = New Security.Cryptography.X509Certificates.X509Certificate2(GetTestCertBytes())
            webServiceDetails.Enabled = True
            webServiceDetails.ExtraWSDL.Add("extra.wsdl")
            webServiceDetails.FriendlyName = "Friendly webservice"
            webServiceDetails.Id = Guid.NewGuid()
            webServiceDetails.Loaded = True
            webServiceDetails.Schemas.Add("Schema1")
            webServiceDetails.Secret = New SafeString("password123")
            webServiceDetails.ServiceToUse = "Service1"
            webServiceDetails.SetSettings("<settings><service name=""Service1""></service></settings>")
            webServiceDetails.Timeout = 5000
            webServiceDetails.URL = "https://web.services.org/webservice1"
            webServiceDetails.Username = "User1"
            webServiceDetails.WSDL = "https://web.services.org/webservice1/wsdl"
            Yield Create("Standard", webServiceDetails, Function(o) o.Excluding(Function(m) m.Certificate))

        End Function


        Private Shared Function GetTestCertBytes() As Byte()
            Dim certDataString = "30820243308201F1A0030201020210AB2B32F7DD29DF8D4E26C7BA09275265300906052B0E03021D05003016311430120603550403130B526F6F74204167656E6379301E170D3137303432343037343434375A170D3339313233313233353935395A30223120301E060355040313174A6F6527732D536F6674776172652D456D706F7269756D30820122300D06092A864886F70D01010105000382010F003082010A0282010100B49BE9D882D1A9A488B94AFE6F10F1BF2B698989F7A89EE7C943AB430A3E0B7CE8FFE174F9023EE71F0895D99EFD1E5B4C784BAD7D75CE18CFA53BE188E21A69B0980DDFCDFF60E3A99FA3DCBB63CA59495BB4FE16ABB7CCEB87CE475CF8377E053F912F2E0383E0DA9FD957E64C7C96040F1907C21D0AC2F53043AFC9F538B16A48A4A614883992CB4CA1474FB1D5E23E54E108DA08217C2DF622263C121B0EC3517A12D8D585DBB8023F03AD04D714A5D8DB9375D27D75C7F923F8A48FBC070122DC60F0EC6294491BF1D2934419BA38C86603CB9B8D44427DE52EBF7AB8AA27EE699381ECF597E457C7C6C2A299F9B6982B6ECC76362285C37A1E08C92F150203010001A34B304930470603551D010440303E801012E4092D061D1D4F008D6121DC166463A1183016311430120603550403130B526F6F74204167656E6379821006376C00AA00648A11CFB8D4AA5C35F4300906052B0E03021D050003410009D3F171F740640910F71C7CA8F752FFB247BA3A80D94A20049C8B9EC967D7C53B761697C5005AB56586CB0E683AB49D3C9493EF6A825C3D58B55127B050A46A"
            Dim split = Enumerable.Range(0, CType(certDataString.Length / 2, Integer)).
                    Select(Function(i) Convert.ToByte(certDataString.Substring(i * 2, 2), 16))

            Return split.ToArray()
        End Function
    End Class

End Namespace
#End If
