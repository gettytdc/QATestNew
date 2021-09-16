#if UNITTESTS

using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace BluePrism.BPCoreLib.UnitTests.LicenceTestSupport
{
    public static class Constants
    {
        /// <summary>
        /// Because Date.MaxValue would be too easy.
        /// </summary>
        public static readonly DateTime MaxExpiryDate = new DateTime(2099, 1, 1);

        // A basic licence for 1 of everything throughout 2015
        public const string Licence2015 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><license created=\"2021-03-18 08:58:43.258363\"><type>enterprise</type><licensee>Stuart Wood</licensee><starts>2015-01-01</starts><expires>2015-12-31</expires><maxprocesses>1</maxprocesses><maxresources>1</maxresources><maxconcurrentsessions>1</maxconcurrentsessions><maxprocessalerts>1</maxprocessalerts><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"/><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"/><DigestValue>5SfIZAhAc/0Xcz+8TAjnjVTXOG0=</DigestValue></Reference></SignedInfo><SignatureValue>h2Nic5CRakLanV1y3PUHRfWzTogBT5XFurwvdJYRWtrVR2wGyXKfwdH4DfM0jgNqlC0y1nS9kl8QYwalHBKJ4SdlxYYi69L7wcgWznC3QUes+HH7X0oTdI/LYN3Hn7vMU1iIKZiNGFsX8DEuLmIbHTTZxiE6xX37YoTvm0sXF0Af/k1u/pFel5Q5AmtVXacG1r00X8tGMt12faMj/dMC1hahXAx+T/N6amHurXnN9RBeVVDCVlDJlW8afNyu4oX26x+87WqybIKjZYEUdBLAz/3g3z0v3k/L2WJxA2QeRnCQCBFmPyXMfvTUnzYpBowqjY6ZCP3RiLvTFDLYAiCouA==</SignatureValue></Signature></license>";

        // A basic licence for 1 of everything throughout 2016
        public const string Licence2016 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><license created=\"2021-03-18 09:02:09.321672\"><type>enterprise</type><licensee>Stuart Wood</licensee><starts>2016-01-01</starts><expires>2016-12-31</expires><maxprocesses>1</maxprocesses><maxresources>1</maxresources><maxconcurrentsessions>1</maxconcurrentsessions><maxprocessalerts>1</maxprocessalerts><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"/><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"/><DigestValue>yAY5ZWxJpVsyNT07Q+SvgBcvr9c=</DigestValue></Reference></SignedInfo><SignatureValue>R/X6QI9XxjLV7USX8rdDsJ1+A1t5J5qKsYxNU35JHffhfYC/m365ho3LvdpiqbLvWJdEzvDFlslEaOUBSQeVZO8iRLWe3gKozn/8VQfO+xHxWb2JC07GsSJn7FjeO+aWQmBe1Kwp718+39a+xKCWwXamlINpZ6vYTK0l/yfAvNqGcjA7XWuJ3MvTDvbvtTnS+gcpk/cAOhSxfTCiu7kv3s6AJLWk9kA/j/JFN9C5jwO7jJiSGZsqCe3Dcetm7W62hYiCQxakSUOLpoy/6EXaQK8WPWsmoPxhfxrmw+NzLyK1c0AucCfif679WRRj2yAkjiP2mqUyeZKz3co9yQv0Gg==</SignatureValue></Signature></license>";

        // A basic licence for 1 of everything throughout 2017
        public const string Licence2017 = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><license created=\"2021-03-18 09:03:48.998296\"><type>enterprise</type><licensee>Stuart Wood</licensee><starts>2017-01-01</starts><expires>2017-12-31</expires><maxprocesses>1</maxprocesses><maxresources>1</maxresources><maxconcurrentsessions>1</maxconcurrentsessions><maxprocessalerts>1</maxprocessalerts><Signature xmlns=\"http://www.w3.org/2000/09/xmldsig#\"><SignedInfo><CanonicalizationMethod Algorithm=\"http://www.w3.org/TR/2001/REC-xml-c14n-20010315\"/><SignatureMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#rsa-sha1\"/><Reference URI=\"\"><Transforms><Transform Algorithm=\"http://www.w3.org/2000/09/xmldsig#enveloped-signature\"/></Transforms><DigestMethod Algorithm=\"http://www.w3.org/2000/09/xmldsig#sha1\"/><DigestValue>8IHiyU6uzJia2TwPsMTJZDLBzKY=</DigestValue></Reference></SignedInfo><SignatureValue>FbdS4+jqvw2QSJTYAgJMiT3m4mqHhmGlBlt/pzZ1OsSq5OuvRG+NEh4HJnuDlBw2sFy9NdMrvJByVNML8FFJMTJYvXFIzGrmUBmvvoA+rxmmLu9QUgXChUsHM9jqz+axSD+2gIuysocoeYyP7DdEsPOx13a3S2OuEz3nemlJ66aGL1Nyz7+FQHShc0+5uKmjZqYSD13apz5GucZt+LixG7Th5EOVaJPhRm7+7nCqfjLGElNBx64MsxEOeidWSOJziFqpCurXOIF+Cat/eWPclt7PJtuy9lX1LbNF+6pBy65KBQ47D9Y9e5GWdj+zIs/FhI5A1SNfMIdjF7GZ2t3WHA==</SignatureValue></Signature></license>";

        public const string TestXml = "<?xml version='1.0' encoding='UTF-8'?><test></test>";

        // Valid Activation Response data
        public const string RequestUser = "admin";
        public const string ActivationReferenceGuid = "1a49450d-4408-4039-a430-b37c1b1cbcb1";
        public const string EnvironmentId = "e9847e28-49d3-4883-aada-c8c34c2638ef";

        public const string GoodBase64Response =
            "eyJkYXRhIjoie1wiQWN0SURcIjpcIjFcIixcIkFjdFJlZlwiOlwiMWE0OTQ1MGQtNDQwOC00MDM5LWE0MzAtYjM3YzFiMWNiY2IxXCIsXCJFbnZJRFwiOlwiZTk4NDdlMjgtNDlkMy00ODgzLWFhZGEtYzhjMzRjMjYzOGVmXCIsXCJUaW1lXCI6XCIyMDE5LTA0LTA5VDA5OjE2OjMwLjYxNTkyMjhaXCIsXCJVc2VyXCI6XCJhZG1pblwifSIsInNpZzY0IjoiRzAxM0VFMGdiaW5iSThDbHBQMW9lSFRjWlE2czh3VGt0MVhZeTRRNTdvMWV3RU9UY2tUYWlGVGs5b05LMEh4OFYzZ1hOT05YME9pMGUxY0hiSjlHdGVMMmhzbVlYYys4ODhVNmRvT3FoUnJmSFNJL2Y4dXNCYlNJRXkyMlRyekxVMjVNUGExb2ZsQWpHZzNza0FnVWJETkNDRjZiL1lXdzZmVTNQM1hpeGYwaUJ2TVdNQ3Uwdk9uRUcxSUkyK2cvR3J0VHpoU2NYSkFnR0JrL21oSndwVHdSdmxNeVJlWW1hbUxkNFlBQldmWWptRmszSVJlaFFjMDdYemxvQkNzd2hJN3dDYlR1NzRFOGVMZVN4V0l5R2pCVjg1alVSZzVIN3pHRXVhbXV4TjdLZGdGSWhCa1hSd3JNdytyY1ZTNlBlWHpjMzdnMnVHcFNpSGFiR3hZUGVTZHA2NnJiOCtNcTIxa2R2TEZ2Vm5tcU1HY3hKMDRMUFFTaVRzek9mL1duSXo1TC9Cb1pJMytKTkp6TUlKN3V2UVF5NjhvVmR2cDIvRnArQlNxSmlFWXB4UGlmSlh5LzV1anpzRUQrdTBTWVNDYkl2WjZNNDMzcmljVzR4UWJodi9vVnlSd3hFbEdzdkRSYlFzRWZHT3ErOXRKS1ZlQ0E1U0Rrdkd6M1R3L0dVcHV6Q0d4WjhEcnVrVk8wSkpkbGNjZUFMdDN2NTJjYzY0UmFQVXduU0dPRFJ0RVlLdHFKbzZhemNTc3ByVzM3TFhuQUhhb2NpdVQwT3Z5RkdrNGt2K0w0Mk8za1J4LzUxN0p0ZVVXdFBEa3ljK3crVWdHZm1LLzl6VEFiSERReWdBSUZvY3crOVNpSmQ4cUROYTAxNTdJVUprWlc3R1BENDROSkpvRkpSR2F4Yjg4bWtZQWpqVFhFOVhGR2JKeE51S1RkRjliTzI4OE9raDdwTkVZYzNTYTZmQUx6UDFEWllvMy83dnFUaElKdEN6VU4wTzdyVVBMWXcrS0JISCtIT3pORHduZHh3R2p2ZnVndndvMGJqZG1OM3VnZENNV1RGaFhzaWsxQmhzQldrbkxEcU5US0tHQWtoK2k1c013RlZrU2FGbk43NklDVU9CQ0xOWlNuTkg5VzZEYTc2Mi8zbFJ0a3VMVXBWbzkvN0tKcXJjM3NNWFB4L2d4dlY1aUhMdXZZekI3LzJkUXVtZGRXZ1hPS3BlS09DVU8vSHAyK1loTTh0ampIRXpzeFUraThtMENMazUvdXlIZVdYUnljbFlGQVBQKytQM01DaGpLeGtIQy9vdUhlSzVOTjRrbndBaS81czErQTdsVHlNMG5YampJdCtZb2xmQk5kdHRxTFYvTDRvZVhDQzdHS2RLU0lkbXBQa2FZZGZ2TlkyaTNEWStrekxDVkdVODdEa1g3TkRpM1ZEUW96V093NjRjWXBrQXJpWWIxWWhKMjhxOWR0TEQwNG45eXFvVVNNamRQd0tFakxDSWxoZ3AvYlFuSHloQ2tGZmVSelVLTENBTFI5OUY2M1hwODNIblZpMS9hakIydmlEMTJHaUtTay9kblJjemFCSGk5Rng0WWcrMXhGVFQvWVNQSXJlV1AzdWF3Wk5SUWVhOU1mMnROVkEzUEI4dHFSV1JtMXVHL28xeTBmRWJqRkNtdzdzSHVsZ2N3cWt5UGN6VG5NcVhNQ202UW1mTUtNZGtUNzhGcUk4c1hyanpsQjBlWG94Ung1QklOT1I4WmIyZkhhc3MwT2drVGlDN2owaTJ0UElsY1R1WGhsMk1ueitMZUpvWjE4MXZRNWhMdkR3Zz09In0=";

        // Invalid Activation Response 
        public const string BadBase64Response =
            "xyJkYXRhIjoie1wiQWN0SURcIjpcIjFcIixcIkFjdFJlZlwiOlwiQzRBNDYyQTYtOEM1MC00RTAzLUI3MzUtQjhGQzlGMDE2QUE5XCIsXCJFbnZJRFwiOlwiN0RBNkVDMkYtOERERC00RDNCLTlEOTctODNCQjkxMTkxRjNEXCIsXCJUaW1lXCI6XCIyMDE5LTAxLTE2IDE1OjU1OjIzLjAwMFwiLFwiVXNlclwiOlwiaXhpc2l0XCJ9Iiwic2lnNjQiOiJTd0JmdVFpNzJkMUp4UTAyZWNHSzZ5S1lYY1FJWmxGV2Y2a3dpSURKZmtab0RTakkwWkFROHRwN1hWTG5sT3RDNFQzTDdWRm1GUzh0UVwvUlBXeVoweEFmMVNGdGJqQTMzRmhOSmRTZTczc0NISVA2eXNVUzZ6aEo3enFaQTNLdEhpemc5NkJnS3RVVnhVVU9KTFJvTlBjamRnTlBtUmc1Zjlhb0tNYWVMYTBoMHFuYmJHXC9KNnFjSG56KzhDNE9ZaDRWNUoyZmFocmdnVzg3MVNqZll2eEF6TjI0RytDYXRLVFJtY1NMcHFkc3FQRWJESjQ2NkZob2pjYVBsS2xwTjZGclB1R2t6WTJ0cGFieWlDNmxCZ1NWd0tzZE9sRktnQXkrb2QxMm1UY2NUWFNFZFhnMUN2VjdCWjR5VTdFMzVxVmc3MGhzb1NRN0JWRG8ydmpCdktBd2k0SGtZMDQrVXhNQ3Q2bHRnVkg0Z1lib2dUSGhjZnE3ZTJcLzA5VDBxZFVnT2NkenpUYkhvNTdrUXRIS3ZJQlZKU0lrMVdwTm1MaFwvczNtdWV2Mm5PZTVRMmJPQVlFcXA5ZHM5cGxMQXcxWVNxaVZtMXNXcU5aek0xRzV5RWxqc3lDUkZ1ckVWMCtybWdsSWF1Y3V4TTZBMlpxbkpUZUpoT0t6SW9oMHNROFZ5OER4XC9LSFV1SE50azJwR2FmcXY3ejlJZTJ2WWppa2V0XC9Cc2U4MWlzUTVTWXVmV2c3bngwUGF0bWZ2VUQ2MG9TSVVCcDVqNXBDTUh2VkpQWVd4bVljREZhN0hpQXJ0cnFrcjc5SVNDQzluWTNLblQ1ZDRcL0lLZ3pXc09IUGhOWDdRRFNYVVJ3NlwvTEtlY2hJOVVFMXZmQUZlTTNITVZSTjg3bnc2T0F4MktOaERUWmFYSGVpSmIwSmszbVJYc01JWjdvT2UwQzZGSkZMMW5IZXJTRzdUU0xQcWJST0Zuc0pHYWNRTHdZTDd4RTdKbG5JMWg5ZncxMm0wUG4xVzMzamRnWlRpRElpSWFPU1ZySkgzVEVOQjJMSFVlT0hOemhhMTFBVUJIQ2hCcHBsbFVmVXpmTTNtKzQ1dVBNZnNFbFdnV2FFaENBbWhXbU02dGg5WGZoZkZ3M09yVFl2ZUdBT0xqTjFkTEZQZHJtVnJcLzFETHlwT3N1MDk1XC9TR0RJZFd3azRBVzFTUlJtTHlxdlNMcVpHMHdJUGpvVUM5anB5ekxmK1RvNTNKZnRWUnJYb3NEaXRxV0ZaQklzM29HXC9ycWs2anczNFR4RXQ5OXNNQWFZdlQxMG5KdWozbmN5K2ZqQ2dOOENJcjdjZHNrUFNqd3Z5T1B3S2lENGY2cVp2VWFROEJuaDlYSk05czNqQ1d0cWhDWWk1RFl0VDZ1S1NPKzdYR1ZVczlBM2k3NTlDeFR6K1V3QU9adzFtMXB2V0FWMlwvbUxzbDZ5c1FhWW9GZGxVZzQzdlY0OVRKTWNySXpUVXdKSXFVa0pIWWdTc3BHOXZVd3JCTHJBYkQyVDhaT2o2NkpcL21TOFJiblFVdUQwK2xtUVRxWnNrZFgxUFwvS0JwcFhDU2tYcGhyZ29XSmVcL1JPS3k0bStqSVRDbW5OVWFsOU5BVStQOFwvRTlEU1lXcmRpU2VsV0dJbEFpQnZ3YkwxOE55XC9LQ0NveExvc05UVGJUanc2VU5uVVM3emIxRDZSdlZYTVdyZkYwTlwvclpzSVZDd285ZGhQMkRzTUJrOUZpSDJWcFBvVDJUamtsajlJMnc4MVlEVFd1ZWFCRWtTS1YxNVwvaTc5UHFGSkE5MUJ5dXFFYjBkUT09In0=";
    }

    public class Key
    {
        public LicenseTypes Type { get; set; } = LicenseTypes.Enterprise;
        public string Name { get; set; } = "Unit Test";
        public int Processes { get; set; } = 0;
        public int Resources { get; set; } = 0;
        public int Sessions { get; set; } = 0;
        public int Alerts { get; set; } = 0;
        public DateTime Starts { get; set; } = DateTime.MinValue;
        public DateTime Expires { get; set; } = Constants.MaxExpiryDate;
        public string Icon { get; set; }
        public string Logo { get; set; }
        public string Title { get; set; }
        public bool? TxnModel { get; set; } = default;
        public int KeyId { get; set; } = 100; // use different one from default
        public int GracePeriod { get; set; }
        public DateTime Installed { get; set; } = DateTime.UtcNow;
        public bool RequiresActivation { get; set; }
        public bool StandAlone { get; set; }
        public int Id { get; set; } = 0;
        public bool Activated { get; set; }

        /// <summary>
        /// Gets the branding element within the given 'licence' element, creating it
        /// if it needs to
        /// </summary>
        /// <param name="lic">The "license" element from whence to get the branding
        /// element</param>
        /// <returns>The branding element inside the "license" element, either the
        /// one that was already there or the one which has been created in this
        /// method call.</returns>
        private static XElement GetBrandingElem(XElement lic)
        {
            if (lic.Element("branding") == null)
            {
                lic.Add(XElement.Parse("<branding/>"));
            }

            return lic.Element("branding");
        }

        /// <summary>
        /// Converts a Key into a <see cref="KeyInfo"/> object with the same values
        /// </summary>
        /// <param name="kb">The key to convert</param>
        /// <returns>The KeyInfo object with the same value as the given key
        /// </returns>
        public static implicit operator KeyInfo(Key kb)
        {
            // Include the transaction model value if it is set
            var lic = XElement.Parse(kb.TxnModel.HasValue
                ? $"<license><id>{kb.KeyId}</id><type>{kb.Type}</type><licensee>{kb.Name}</licensee><maxprocesses>{kb.Processes}</maxprocesses><maxresources>{kb.Resources}</maxresources><maxconcurrentsessions>{kb.Sessions}</maxconcurrentsessions><maxprocessalerts>{kb.Alerts}</maxprocessalerts><starts>{kb.Starts:yyyy-MM-dd}</starts><expires>{kb.Expires:yyyy-MM-dd}</expires><activation>{kb.RequiresActivation}</activation><graceperiod>{kb.GracePeriod}</graceperiod><standalone>{kb.StandAlone.ToString().ToLowerInvariant()}</standalone><transactionmodel>{kb.TxnModel.Value}</transactionmodel></license>"
                : $"<license><id>{kb.KeyId}</id><type>{kb.Type}</type><licensee>{kb.Name}</licensee><maxprocesses>{kb.Processes}</maxprocesses><maxresources>{kb.Resources}</maxresources><maxconcurrentsessions>{kb.Sessions}</maxconcurrentsessions><maxprocessalerts>{kb.Alerts}</maxprocessalerts><starts>{kb.Starts:yyyy-MM-dd}</starts><expires>{kb.Expires:yyyy-MM-dd}</expires><activation>{kb.RequiresActivation}</activation><graceperiod>{kb.GracePeriod}</graceperiod><standalone>{kb.StandAlone.ToString().ToLowerInvariant()}</standalone></license>");

            if (kb.TxnModel.HasValue)
            {
                lic.Add(XElement.Parse($"<transactionmodel>{kb.TxnModel.Value}</transactionmodel>"));
            }

            if (kb.Icon != null)
            {
                GetBrandingElem(lic).Add(XElement.Parse($"<icon>{kb.Icon}</icon>"));
            }

            if (kb.Logo != null)
            {
                GetBrandingElem(lic).Add(XElement.Parse($"<largelogo>{kb.Logo}</largelogo>"));
            }

            if (kb.Title != null)
            {
                GetBrandingElem(lic).Add(XElement.Parse($"<title>{kb.Title}</title>"));
            }

            var actInfo = new List<ActivationInfo>();
            string activationResponse = null;
            if (kb.Activated)
            {
                var activationReference = new Guid(Constants.ActivationReferenceGuid);
                actInfo.Add(new ActivationInfo(1, activationReference));
                activationResponse = Constants.GoodBase64Response;
            }

            if (kb.Installed == DateTime.MinValue)
            {
                return new KeyInfo(lic.ToString());
            }

            return new KeyInfo(kb.Id, lic.ToString(), kb.Installed, Guid.NewGuid(), activationResponse, actInfo,
                Constants.EnvironmentId);
        }
    }
}

#endif
