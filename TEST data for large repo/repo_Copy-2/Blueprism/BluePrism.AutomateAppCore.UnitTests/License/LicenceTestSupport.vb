#If UNITTESTS Then
Imports BluePrism.BPCoreLib

Namespace License

    Public Module Constants
        ''' <summary>
        ''' Because Date.MaxValue would be too easy.
        ''' </summary>
        Public ReadOnly MaxExpiryDate As New Date(2099, 1, 1)

        ' A basic licence for 1 of everything throughout 2015
        Public Const Licence2015 As String = "<?xml version=""1.0"" encoding=""UTF-8""?><license><type>enterprise</type><licensee>Stuart Wood</licensee><starts>2015-01-01</starts><expires>2015-12-31</expires><maxprocesses>1</maxprocesses><maxresources>1</maxresources><maxconcurrentsessions>1</maxconcurrentsessions><maxprocessalerts>1</maxprocessalerts><Signature xmlns=""http://www.w3.org/2000/09/xmldsig#""><SignedInfo><CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315""/><SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1""/><Reference URI=""""><Transforms><Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature""/></Transforms><DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/><DigestValue>aqEgONr3JjKy8PCwhGrsWw0ZRF8=</DigestValue></Reference></SignedInfo><SignatureValue>QDVQBF9JjLGSMBAmWNJx6JPmTTd//WMwqDLRNE+yteR0iHRQ9JuC6YYzuNVs9XVx3LiJkIMsWxB9S4NhpHjRvUQXf0Jgk3ws1ifwT7LHHjo4Q7/uAzjJ7idMwgkDLi8IaPsk6USPvkqxDvf32+792PS2RHWFUNCJ6lli3EGuIgA=</SignatureValue></Signature></license>"

        ' A basic licence for 1 of everything throughout 2016
        Public Const Licence2016 As String = "<?xml version=""1.0"" encoding=""UTF-8""?><license><type>enterprise</type><licensee>Stuart Wood</licensee><starts>2016-01-01</starts><expires>2016-12-31</expires><maxprocesses>1</maxprocesses><maxresources>1</maxresources><maxconcurrentsessions>1</maxconcurrentsessions><maxprocessalerts>1</maxprocessalerts><Signature xmlns=""http://www.w3.org/2000/09/xmldsig#""><SignedInfo><CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315""/><SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1""/><Reference URI=""""><Transforms><Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature""/></Transforms><DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/><DigestValue>zHUCyXBIRU8+Qyv3fTvePR9Yunc=</DigestValue></Reference></SignedInfo><SignatureValue>I9X+UuTAvOAEasH6oRhyVyfl3j4Zfs9yALT3lbChfjKz9c6lgPB/TECNReG2UNtlUbJwaliAapTdnZ/ZVGrYNPktGIcT2otEr4/6wTNNuH+at5XNtGpoCpN+D9Paene9tpIwHlHGxJDHI7Zwezg01VtpJcG6xd/tqr7eIdn248w=</SignatureValue></Signature></license>"

        ' A basic licence for 1 of everything throughout 2017
        Public Const Licence2017 As String = "<?xml version=""1.0"" encoding=""UTF-8""?><license><type>enterprise</type><licensee>Stuart Wood</licensee><starts>2017-01-01</starts><expires>2017-12-31</expires><maxprocesses>1</maxprocesses><maxresources>1</maxresources><maxconcurrentsessions>1</maxconcurrentsessions><maxprocessalerts>1</maxprocessalerts><Signature xmlns=""http://www.w3.org/2000/09/xmldsig#""><SignedInfo><CanonicalizationMethod Algorithm=""http://www.w3.org/TR/2001/REC-xml-c14n-20010315""/><SignatureMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#rsa-sha1""/><Reference URI=""""><Transforms><Transform Algorithm=""http://www.w3.org/2000/09/xmldsig#enveloped-signature""/></Transforms><DigestMethod Algorithm=""http://www.w3.org/2000/09/xmldsig#sha1""/><DigestValue>LZfySqzvevU99M3AtewXt7r5lPw=</DigestValue></Reference></SignedInfo><SignatureValue>SM1crLfUkTrH5HFZPXfeLINIA1oYDXT4fz6L+mUDwr7Pa2GpwmBiaD+Uk8Kmhphw09rL0ofsYyvo8k9DE4RenBBWfvjphCW6nM9JYfxh3w/pRjUHXGldF6GGlY1Hn3Q+NILoOFBgZZxXspxa2Etxg6AJ02glTrpsO+YITP9yK70=</SignatureValue></Signature></license>"

        Public Const TestXml As String = "<?xml version='1.0' encoding='UTF-8'?><test></test>"

        'Valid Activation Response data
        Public Const RequestUser As String = "admin"
        Public Const ActivationReferenceGuid As String = "1a49450d-4408-4039-a430-b37c1b1cbcb1"
        Public Const EnvironmentId As String = "e9847e28-49d3-4883-aada-c8c34c2638ef"
        Public Const GoodBase64Response As String = "eyJkYXRhIjoie1wiQWN0SURcIjpcIjFcIixcIkFjdFJlZlwiOlwiMWE0OTQ1MGQtNDQwOC00MDM5LWE0MzAtYjM3YzFiMWNiY2IxXCIsXCJFbnZJRFwiOlwiZTk4NDdlMjgtNDlkMy00ODgzLWFhZGEtYzhjMzRjMjYzOGVmXCIsXCJUaW1lXCI6XCIyMDE5LTA0LTA5VDA5OjE2OjMwLjYxNTkyMjhaXCIsXCJVc2VyXCI6XCJhZG1pblwifSIsInNpZzY0IjoiRzAxM0VFMGdiaW5iSThDbHBQMW9lSFRjWlE2czh3VGt0MVhZeTRRNTdvMWV3RU9UY2tUYWlGVGs5b05LMEh4OFYzZ1hOT05YME9pMGUxY0hiSjlHdGVMMmhzbVlYYys4ODhVNmRvT3FoUnJmSFNJL2Y4dXNCYlNJRXkyMlRyekxVMjVNUGExb2ZsQWpHZzNza0FnVWJETkNDRjZiL1lXdzZmVTNQM1hpeGYwaUJ2TVdNQ3Uwdk9uRUcxSUkyK2cvR3J0VHpoU2NYSkFnR0JrL21oSndwVHdSdmxNeVJlWW1hbUxkNFlBQldmWWptRmszSVJlaFFjMDdYemxvQkNzd2hJN3dDYlR1NzRFOGVMZVN4V0l5R2pCVjg1alVSZzVIN3pHRXVhbXV4TjdLZGdGSWhCa1hSd3JNdytyY1ZTNlBlWHpjMzdnMnVHcFNpSGFiR3hZUGVTZHA2NnJiOCtNcTIxa2R2TEZ2Vm5tcU1HY3hKMDRMUFFTaVRzek9mL1duSXo1TC9Cb1pJMytKTkp6TUlKN3V2UVF5NjhvVmR2cDIvRnArQlNxSmlFWXB4UGlmSlh5LzV1anpzRUQrdTBTWVNDYkl2WjZNNDMzcmljVzR4UWJodi9vVnlSd3hFbEdzdkRSYlFzRWZHT3ErOXRKS1ZlQ0E1U0Rrdkd6M1R3L0dVcHV6Q0d4WjhEcnVrVk8wSkpkbGNjZUFMdDN2NTJjYzY0UmFQVXduU0dPRFJ0RVlLdHFKbzZhemNTc3ByVzM3TFhuQUhhb2NpdVQwT3Z5RkdrNGt2K0w0Mk8za1J4LzUxN0p0ZVVXdFBEa3ljK3crVWdHZm1LLzl6VEFiSERReWdBSUZvY3crOVNpSmQ4cUROYTAxNTdJVUprWlc3R1BENDROSkpvRkpSR2F4Yjg4bWtZQWpqVFhFOVhGR2JKeE51S1RkRjliTzI4OE9raDdwTkVZYzNTYTZmQUx6UDFEWllvMy83dnFUaElKdEN6VU4wTzdyVVBMWXcrS0JISCtIT3pORHduZHh3R2p2ZnVndndvMGJqZG1OM3VnZENNV1RGaFhzaWsxQmhzQldrbkxEcU5US0tHQWtoK2k1c013RlZrU2FGbk43NklDVU9CQ0xOWlNuTkg5VzZEYTc2Mi8zbFJ0a3VMVXBWbzkvN0tKcXJjM3NNWFB4L2d4dlY1aUhMdXZZekI3LzJkUXVtZGRXZ1hPS3BlS09DVU8vSHAyK1loTTh0ampIRXpzeFUraThtMENMazUvdXlIZVdYUnljbFlGQVBQKytQM01DaGpLeGtIQy9vdUhlSzVOTjRrbndBaS81czErQTdsVHlNMG5YampJdCtZb2xmQk5kdHRxTFYvTDRvZVhDQzdHS2RLU0lkbXBQa2FZZGZ2TlkyaTNEWStrekxDVkdVODdEa1g3TkRpM1ZEUW96V093NjRjWXBrQXJpWWIxWWhKMjhxOWR0TEQwNG45eXFvVVNNamRQd0tFakxDSWxoZ3AvYlFuSHloQ2tGZmVSelVLTENBTFI5OUY2M1hwODNIblZpMS9hakIydmlEMTJHaUtTay9kblJjemFCSGk5Rng0WWcrMXhGVFQvWVNQSXJlV1AzdWF3Wk5SUWVhOU1mMnROVkEzUEI4dHFSV1JtMXVHL28xeTBmRWJqRkNtdzdzSHVsZ2N3cWt5UGN6VG5NcVhNQ202UW1mTUtNZGtUNzhGcUk4c1hyanpsQjBlWG94Ung1QklOT1I4WmIyZkhhc3MwT2drVGlDN2owaTJ0UElsY1R1WGhsMk1ueitMZUpvWjE4MXZRNWhMdkR3Zz09In0="

        ' Invalid Activation Response 
        Public Const BadBase64Response As String = "xyJkYXRhIjoie1wiQWN0SURcIjpcIjFcIixcIkFjdFJlZlwiOlwiQzRBNDYyQTYtOEM1MC00RTAzLUI3MzUtQjhGQzlGMDE2QUE5XCIsXCJFbnZJRFwiOlwiN0RBNkVDMkYtOERERC00RDNCLTlEOTctODNCQjkxMTkxRjNEXCIsXCJUaW1lXCI6XCIyMDE5LTAxLTE2IDE1OjU1OjIzLjAwMFwiLFwiVXNlclwiOlwiaXhpc2l0XCJ9Iiwic2lnNjQiOiJTd0JmdVFpNzJkMUp4UTAyZWNHSzZ5S1lYY1FJWmxGV2Y2a3dpSURKZmtab0RTakkwWkFROHRwN1hWTG5sT3RDNFQzTDdWRm1GUzh0UVwvUlBXeVoweEFmMVNGdGJqQTMzRmhOSmRTZTczc0NISVA2eXNVUzZ6aEo3enFaQTNLdEhpemc5NkJnS3RVVnhVVU9KTFJvTlBjamRnTlBtUmc1Zjlhb0tNYWVMYTBoMHFuYmJHXC9KNnFjSG56KzhDNE9ZaDRWNUoyZmFocmdnVzg3MVNqZll2eEF6TjI0RytDYXRLVFJtY1NMcHFkc3FQRWJESjQ2NkZob2pjYVBsS2xwTjZGclB1R2t6WTJ0cGFieWlDNmxCZ1NWd0tzZE9sRktnQXkrb2QxMm1UY2NUWFNFZFhnMUN2VjdCWjR5VTdFMzVxVmc3MGhzb1NRN0JWRG8ydmpCdktBd2k0SGtZMDQrVXhNQ3Q2bHRnVkg0Z1lib2dUSGhjZnE3ZTJcLzA5VDBxZFVnT2NkenpUYkhvNTdrUXRIS3ZJQlZKU0lrMVdwTm1MaFwvczNtdWV2Mm5PZTVRMmJPQVlFcXA5ZHM5cGxMQXcxWVNxaVZtMXNXcU5aek0xRzV5RWxqc3lDUkZ1ckVWMCtybWdsSWF1Y3V4TTZBMlpxbkpUZUpoT0t6SW9oMHNROFZ5OER4XC9LSFV1SE50azJwR2FmcXY3ejlJZTJ2WWppa2V0XC9Cc2U4MWlzUTVTWXVmV2c3bngwUGF0bWZ2VUQ2MG9TSVVCcDVqNXBDTUh2VkpQWVd4bVljREZhN0hpQXJ0cnFrcjc5SVNDQzluWTNLblQ1ZDRcL0lLZ3pXc09IUGhOWDdRRFNYVVJ3NlwvTEtlY2hJOVVFMXZmQUZlTTNITVZSTjg3bnc2T0F4MktOaERUWmFYSGVpSmIwSmszbVJYc01JWjdvT2UwQzZGSkZMMW5IZXJTRzdUU0xQcWJST0Zuc0pHYWNRTHdZTDd4RTdKbG5JMWg5ZncxMm0wUG4xVzMzamRnWlRpRElpSWFPU1ZySkgzVEVOQjJMSFVlT0hOemhhMTFBVUJIQ2hCcHBsbFVmVXpmTTNtKzQ1dVBNZnNFbFdnV2FFaENBbWhXbU02dGg5WGZoZkZ3M09yVFl2ZUdBT0xqTjFkTEZQZHJtVnJcLzFETHlwT3N1MDk1XC9TR0RJZFd3azRBVzFTUlJtTHlxdlNMcVpHMHdJUGpvVUM5anB5ekxmK1RvNTNKZnRWUnJYb3NEaXRxV0ZaQklzM29HXC9ycWs2anczNFR4RXQ5OXNNQWFZdlQxMG5KdWozbmN5K2ZqQ2dOOENJcjdjZHNrUFNqd3Z5T1B3S2lENGY2cVp2VWFROEJuaDlYSk05czNqQ1d0cWhDWWk1RFl0VDZ1S1NPKzdYR1ZVczlBM2k3NTlDeFR6K1V3QU9adzFtMXB2V0FWMlwvbUxzbDZ5c1FhWW9GZGxVZzQzdlY0OVRKTWNySXpUVXdKSXFVa0pIWWdTc3BHOXZVd3JCTHJBYkQyVDhaT2o2NkpcL21TOFJiblFVdUQwK2xtUVRxWnNrZFgxUFwvS0JwcFhDU2tYcGhyZ29XSmVcL1JPS3k0bStqSVRDbW5OVWFsOU5BVStQOFwvRTlEU1lXcmRpU2VsV0dJbEFpQnZ3YkwxOE55XC9LQ0NveExvc05UVGJUanc2VU5uVVM3emIxRDZSdlZYTVdyZkYwTlwvclpzSVZDd285ZGhQMkRzTUJrOUZpSDJWcFBvVDJUamtsajlJMnc4MVlEVFd1ZWFCRWtTS1YxNVwvaTc5UHFGSkE5MUJ5dXFFYjBkUT09In0="

    End Module

    Public Class Key

        Public Property Type As LicenseTypes = LicenseTypes.Enterprise
        Public Property Name As String = "Unit Test"
        Public Property Processes As Integer = 0
        Public Property Resources As Integer = 0
        Public Property Sessions As Integer = 0
        Public Property Alerts As Integer = 0
        Public Property Starts As Date = Date.MinValue
        Public Property Expires As Date = MaxExpiryDate
        Public Property Icon As String
        Public Property Logo As String
        Public Property Title As String
        Public Property TxnModel As Nullable(Of Boolean) = Nothing
        Public Property KeyId As Integer = 100 'use different one from default
        Public Property GracePeriod As Integer
        Public Property Installed As Date = Date.UtcNow
        Public Property RequiresActivation As Boolean
        Public Property StandAlone As Boolean
        Public Property Id As Integer = 0
        Public Property Activated As Boolean

        ''' <summary>
        ''' Gets the branding element within the given 'licence' element, creating it
        ''' if it needs to
        ''' </summary>
        ''' <param name="lic">The "license" element from whence to get the branding
        ''' element</param>
        ''' <returns>The branding element inside the "license" element, either the
        ''' one that was already there or the one which has been created in this
        ''' method call.</returns>
        Private Shared Function GetBrandingElem(lic As XElement) As XElement
            Dim branding = lic.Element("branding")
            If branding Is Nothing Then
                lic.Add(<branding/>)
                branding = lic.Element("branding")
            End If
            Return branding
        End Function

        ''' <summary>
        ''' Converts a Key into a <see cref="KeyInfo"/> object with the same values
        ''' </summary>
        ''' <param name="kb">The key to convert</param>
        ''' <returns>The KeyInfo object with the same value as the given key
        ''' </returns>
        Public Overloads Shared Widening Operator CType(kb As Key) As KeyInfo
            Dim lic As XElement
            ' Include the transaction model value if it is set
            If kb.TxnModel.HasValue Then
                lic =
                    <license>
                        <id><%= kb.KeyId %></id>
                        <type><%= kb.Type %></type>
                        <licensee><%= kb.Name %></licensee>
                        <maxprocesses><%= kb.Processes %></maxprocesses>
                        <maxresources><%= kb.Resources %></maxresources>
                        <maxconcurrentsessions><%= kb.Sessions %></maxconcurrentsessions>
                        <maxprocessalerts><%= kb.Alerts %></maxprocessalerts>
                        <starts><%= kb.Starts.ToString("yyyy'-'MM'-'dd") %></starts>
                        <expires><%= kb.Expires.ToString("yyyy'-'MM'-'dd") %></expires>
                        <activation><%= kb.RequiresActivation %></activation>
                        <graceperiod><%= kb.GracePeriod %></graceperiod>
                        <standalone><%= kb.StandAlone %></standalone>
                        <transactionmodel><%= kb.TxnModel.Value %></transactionmodel>
                    </license>
            Else
                lic =
                    <license>
                        <id><%= kb.KeyId %></id>
                        <type><%= kb.Type %></type>
                        <licensee><%= kb.Name %></licensee>
                        <maxprocesses><%= kb.Processes %></maxprocesses>
                        <maxresources><%= kb.Resources %></maxresources>
                        <maxconcurrentsessions><%= kb.Sessions %></maxconcurrentsessions>
                        <maxprocessalerts><%= kb.Alerts %></maxprocessalerts>
                        <starts><%= kb.Starts.ToString("yyyy'-'MM'-'dd") %></starts>
                        <expires><%= kb.Expires.ToString("yyyy'-'MM'-'dd") %></expires>
                        <activation><%= kb.RequiresActivation %></activation>
                        <graceperiod><%= kb.GracePeriod %></graceperiod>
                        <standalone><%= kb.StandAlone %></standalone>
                    </license>

            End If
            If kb.TxnModel.HasValue Then lic.Add(
                <transactionmodel><%= kb.TxnModel.Value %></transactionmodel>)

            If kb.Icon IsNot Nothing Then GetBrandingElem(lic).Add(
                <icon><%= kb.Icon %></icon>)
            If kb.Logo IsNot Nothing Then GetBrandingElem(lic).Add(
                <largelogo><%= kb.Logo %></largelogo>)
            If kb.Title IsNot Nothing Then GetBrandingElem(lic).Add(
                <title><%= kb.Title %></title>)

            Dim actInfo As New List(Of ActivationInfo)

            Dim activationResponse As String = Nothing
            If kb.Activated Then
                Dim activationReference = New Guid(ActivationReferenceGuid)
                actInfo.Add(New ActivationInfo(1, activationReference))
                activationResponse = GoodBase64Response
            End If


            If kb.Installed = Date.MinValue Then Return New KeyInfo(lic.ToString())
            Return New KeyInfo(kb.Id, lic.ToString(), kb.Installed, Guid.NewGuid(), activationResponse, actInfo, EnvironmentId)

        End Operator

    End Class

End Namespace
#End If
