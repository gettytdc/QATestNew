#if UNITTESTS

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using BluePrism.AutomateProcessCore;
using BluePrism.Common.Security;
using BluePrism.Core;
using BluePrism.Core.Xml;
using NUnit.Framework;

namespace AutomateProcessCore.UnitTests
{
    [TestFixture]
    public class WebServiceDetailsTests
    {
        [TestCaseSource(typeof(TestUtil), "PasswordTests")]
        public void TestSettingsXML(string input)
        {
            var ws = new clsWebServiceDetails
            {
                ServiceToUse = "Test Service"
            };
            ws.Actions.Add("DoStuff", true);
            ws.Username = "admin";
            ws.Secret = new SafeString(input);

            var settingsXML = ws.GetSettings();
            ws.SetSettings(settingsXML);

            Assert.That(ws.Username, Is.EqualTo("admin"));
            Assert.That(ws.Secret.AsString(), Is.EqualTo(input));
        }

        [TestCaseSource(typeof(TestUtil), "PasswordTests")]
        public void TestSettingsXML_WithUnsafeSecret(string input)
        {
            var sampleXML = @"<?xml version=""1.0""?><settings><service name=""TestService"" username=""admin"" secret=""password1""><method name=""DoStuff"" enabled=""true"" /></service></settings>";
            var xd = new ReadableXmlDocument(sampleXML);
            var service = xd.SelectSingleNode("/settings/service") as XmlElement;
            service.SetAttribute("secret", input);

            var ws = new clsWebServiceDetails();
            ws.SetSettings(xd.OuterXml);

            Assert.That(ws.Username, Is.EqualTo("admin"));
            Assert.That(ws.Secret.AsString(), Is.EqualTo(input));
        }

        [Test]
        public void TestSettingsXML_WithNoCredentials()
        {
            var ws = new clsWebServiceDetails
            {
                ServiceToUse = "Test Service"
            };
            ws.Actions.Add("DoStuff", true);

            var settingsXML = ws.GetSettings();
            ws.SetSettings(settingsXML);

            Assert.That(ws.Username, Is.Null);
            Assert.That(ws.Secret, Is.Null);
        }

        [Test]
        public void TestSimpleWebService_SimpleSend()
        {
            var act = new clsWebServiceAction();
            act.SetName("blah");
            var soap = new clsSoap(clsSoap.SoapVersion.Soap12) { SoapAction = "POST", Action = act, Namespace = "mytestnamespace" };

            var outputs = new clsArgumentList
            {
                new clsArgument("arg1", new clsProcessValue(DataType.text, "testdata"))
            };

            var lst = new List<clsProcessParameter>() { new clsWebParameter() { Name = "name", ParamType = DataType.text } };
            using (var ms = new MemoryStream())
            {
                soap.SendMessage(ms, outputs, lst);
                ms.Position = 0L;
                var sr = new StreamReader(ms);
                var sentMessage = sr.ReadToEnd();
                Assert.AreEqual("<?xml version=\"1.0\" encoding=\"UTF-8\"?><soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:tns=\"mytestnamespace\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body><tns:blah /></soap:Body></soap:Envelope>", sentMessage, "Check the message sent was as expected.");
            }
        }

        [Test]
        public void TestSimpleWebService_SimpleReceive()
        {
            var act = new clsWebServiceAction();
            act.SetName("blah");

            var soap = new clsSoap(clsSoap.SoapVersion.Soap12) { SoapAction = "POST", Action = act, Namespace = "mytestnamespace" };
            var lst = new List<clsProcessParameter>() { new clsWebParameter() { Name = "addresslineone", ParamType = DataType.text, Direction = ParamDirection.Out } };
            var message = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                                            <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                                                xmlns:xs=""http://www.w3.org/2001/XMLSchema""
                                                xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/""
                                                xmlns:tns=""mytestnamespace""
                                                xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                                <soap:Body>
                                                    <standardizeAddress>
                                                        <addresslineone>This is my address</addresslineone>
                                                    </standardizeAddress>
                                                </soap:Body>
                                            </soap:Envelope>";
             
            var xSoap = new ReadableXmlDocument(message);
            var outputs = new clsArgumentList();
            outputs = soap.ReadMessage(xSoap, lst);

            Assert.AreEqual(1, outputs.Count, "Number of arguments received should be 1.");
            Assert.AreEqual("This is my address", outputs.First().Value.EncodedValue, "Check that the value was read out of the xml");
            Assert.AreEqual("addresslineone", outputs.First().Name, "check the param is the right name");
        }

        [Test]
        public void TestSimpleWebService_SimpleReceive_NoNamespace()
        {
            var act = new clsWebServiceAction();
            act.SetName("blah");

            var soap = new clsSoap(clsSoap.SoapVersion.Soap12) { SoapAction = "POST", Action = act, Namespace = string.Empty };
            var lst = new List<clsProcessParameter>() { new clsWebParameter() { Name = "addresslineone", ParamType = DataType.text, Direction = ParamDirection.Out } };
            var message = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body><standardizeAddress><addresslineone>This is my address</addresslineone></standardizeAddress></soap:Body></soap:Envelope>";
            var xSoap = new ReadableXmlDocument(message);
            var outputs = soap.ReadMessage(xSoap, lst);
            Assert.AreEqual(1, outputs.Count, "Number of arguments received should be 1.");

            Assert.AreEqual("This is my address", outputs.First().Value.EncodedValue, "Check that the value was read out of the xml");
            Assert.AreEqual("addresslineone", outputs.First().Name, "check the param is the right name");
        }

        [Test]
        public void TestSimpleWebService_SimpleSend_NoNamespace()
        {
            var act = new clsWebServiceAction() { InputMessage = "blah", InputMessageNamespace = string.Empty };
            var soap = new clsSoap(clsSoap.SoapVersion.Soap12) { SoapAction = "POST", Namespace = string.Empty, Action = act };
            var outputs = new clsArgumentList
            {
                new clsArgument("blah", new clsProcessValue(DataType.text, "blah"))
            };

            var lst = new List<clsProcessParameter>() { new clsWebParameter() { Name = "blah", Direction = ParamDirection.In } };
            using (var ms = new MemoryStream())
            {
                soap.SendMessage(ms, outputs, lst);
                ms.Position = 0L;
                var sr = new StreamReader(ms);
                var sentMessage = sr.ReadToEnd();

                Assert.AreEqual("<?xml version=\"1.0\" encoding=\"UTF-8\"?><soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xs=\"http://www.w3.org/2001/XMLSchema\" xmlns:soapenc=\"http://schemas.xmlsoap.org/soap/encoding/\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"><soap:Body><blah /></soap:Body></soap:Envelope>", sentMessage, "Check the message sent was as expected.");
            }
        }

        [Test]
        public void TestSimpleWebService_SimpleReceive_MultiFields()
        {
            var act = new clsWebServiceAction();
            act.SetName("blah");
            act.AddParameter(new clsWebParameter() { Direction = ParamDirection.Out, Name = "addresslineone", ParamType = DataType.text });
            act.AddParameter(new clsWebParameter() { Direction = ParamDirection.Out, Name = "addresslinetwo", ParamType = DataType.text });
            act.AddParameter(new clsWebParameter() { Direction = ParamDirection.Out, Name = "name", ParamType = DataType.text });
            act.AddParameter(new clsWebParameter() { Direction = ParamDirection.Out, Name = "postcode", ParamType = DataType.text });

            var soap = new clsSoap(clsSoap.SoapVersion.Soap12) { SoapAction = "POST", Action = act, Namespace = "mytestnamespace" };
            var message = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                                            <soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                                                xmlns:xs=""http://www.w3.org/2001/XMLSchema""
                                                xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/""
                                                xmlns:tns=""mytestnamespace"" xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                                <soap:Body>
                                                    <standardizeAddress>
                                                        <name>Adrian Vaughan</name>
                                                        <addresslineone>This is my address</addresslineone>
                                                        <addresslinetwo>This is my address line 2</addresslinetwo>
                                                        <postcode>M1 3DH</postcode>
                                                    </standardizeAddress>
                                                </soap:Body>
                                            </soap:Envelope>";
             
            var xSoap = new ReadableXmlDocument(message);
            var outputs = new clsArgumentList();
            outputs = soap.ReadMessage(xSoap, act.GetParameters());

            Assert.AreEqual(4, outputs.Count, "Number of arguments received should be 4.");
            Assert.True(outputs.Exists(f => f.Name == "addresslineone" && f.Value.EncodedValue == "This is my address"), "Check that the value was read out of the xml");
            Assert.True(outputs.Exists(f => f.Name == "addresslinetwo" && f.Value.EncodedValue == "This is my address line 2"), "Check that the value was read out of the xml");
            Assert.True(outputs.Exists(f => f.Name == "name" && f.Value.EncodedValue == "Adrian Vaughan"), "Check that the value was read out of the xml");
            Assert.True(outputs.Exists(f => f.Name == "postcode" && f.Value.EncodedValue == "M1 3DH"), "Check that the value was read out of the xml");
        }

        [Test]
        public void TestSimpleWebService_ComplexReceive()
        {
            var act = new clsWebServiceAction();
            act.SetName("blah");
            var soap = new clsSoap(clsSoap.SoapVersion.Soap12) { SoapAction = "POST", Action = act, Namespace = string.Empty };
            var lst = new List<clsProcessParameter>() { new clsWebParameter() { Name = "MtvnSvcVer", ParamType = DataType.text, Direction = ParamDirection.Out }, new clsWebParameter() { Name = "MsgUUID", ParamType = DataType.number, Direction = ParamDirection.Out }, new clsWebParameter() { Name = "ErrCde", ParamType = DataType.number, Direction = ParamDirection.Out }, new clsWebParameter() { Name = "ErrMsg", ParamType = DataType.text, Direction = ParamDirection.Out } };
            var x = new clsWebParameter() { Name = "Svc", ParamType = DataType.collection, Direction = ParamDirection.Out };
            x.CollectionInfo.AddField(new clsCollectionFieldInfo("ErrCde", DataType.number, string.Empty));
            x.CollectionInfo.AddField(new clsCollectionFieldInfo("ErrMsg", DataType.text, string.Empty));
            var y = new clsCollectionFieldInfo("SvcParms", DataType.collection, string.Empty);
            y.Children.AddField(new clsCollectionFieldInfo("SvcID", DataType.text, string.Empty));
            y.Children.AddField(new clsCollectionFieldInfo("ApplID", DataType.text, string.Empty));
            y.Children.AddField(new clsCollectionFieldInfo("SvcVer", DataType.text, string.Empty));
            y.Children.AddField(new clsCollectionFieldInfo("RqstUUID", DataType.text, string.Empty));
            x.CollectionInfo.AddField(y);
            lst.Add(x);

            var message = XElement.Parse("<soapenv:Envelope xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\"\r\n                                    xmlns:mtv=\"mtvnCWDPAcctBalInqSvcRes\"\r\n                                    xmlns:mtv1=\"mtvnCWDPAcctBalInqResData\"><soapenv:Header/><soapenv:Body>\r\n                                        <mtv:DPAcctBalInqMtvnSvcRes>\r\n                                            <mtv:MtvnSvcVer>1</mtv:MtvnSvcVer>\r\n                                            <mtv:MsgUUID>2</mtv:MsgUUID>\r\n                                            <mtv:Svc>\r\n                                                <mtv:SvcParms>\r\n                                                    <mtv:SvcID>4</mtv:SvcID>\r\n                                                    <mtv:ApplID>3</mtv:ApplID>\r\n                                                    <mtv:SvcVer>5</mtv:SvcVer>\r\n                                                    <mtv:RqstUUID>6</mtv:RqstUUID>\r\n                                                </mtv:SvcParms>\r\n                                                <mtv:ErrCde>7</mtv:ErrCde>\r\n                                                <mtv:ErrMsg>8</mtv:ErrMsg>\r\n                                            </mtv:Svc>\r\n                                            <mtv:ErrCde>9</mtv:ErrCde>\r\n                                            <mtv:ErrMsg>10</mtv:ErrMsg>\r\n                                        </mtv:DPAcctBalInqMtvnSvcRes>\r\n                                    </soapenv:Body></soapenv:Envelope>").ToString();
            var xSoap = new ReadableXmlDocument(message);
            var outputs = soap.ReadMessage(xSoap, lst);
            var target = outputs.FirstOrDefault(f => f.Name == "MtvnSvcVer");

            Assert.NotNull(target, "Check we found the argument");
            Assert.AreEqual("1", target.Value.EncodedValue, "Check the value of the MtvnSvcVer parameter is correct");
            target = outputs.FirstOrDefault(f => f.Name == "MsgUUID");
            Assert.NotNull(target, "Check we found the argument");
            Assert.AreEqual("2", target.Value.EncodedValue, "Check the value of the MsgUUID parameter is correct");
            target = outputs.FirstOrDefault(f => f.Name == "ErrCde");
            Assert.NotNull(target, "Check we found the argument");
            Assert.AreEqual("9", target.Value.EncodedValue, "Check the value of the ErrCde parameter is correct");
            target = outputs.FirstOrDefault(f => f.Name == "ErrMsg");
            Assert.NotNull(target, "Check we found the argument");
            Assert.AreEqual("10", target.Value.EncodedValue, "Check the value of the ErrMsg parameter is correct");
            target = outputs.FirstOrDefault(f => f.Name == "Svc");
            Assert.NotNull(target, "Check we found the argument");
            Assert.AreEqual(1, target.Value.Collection.Count, "Check the rows of this collection svc parameter are correct");
            Assert.AreEqual("8", target.Value.Collection.GetField("ErrMsg").EncodedValue, "Check the value of error message");
            Assert.AreEqual("7", target.Value.Collection.GetField("ErrCde").EncodedValue, "Check the value of error code");
            Assert.AreEqual("3", target.Value.Collection.GetField("SvcParms").Collection.GetField("ApplID").EncodedValue, "check the inner collection is populated");
            Assert.AreEqual("4", target.Value.Collection.GetField("SvcParms").Collection.GetField("SvcID").EncodedValue, "check the inner collection is populated");
            Assert.AreEqual("5", target.Value.Collection.GetField("SvcParms").Collection.GetField("SvcVer").EncodedValue, "check the inner collection is populated");
            Assert.AreEqual("6", target.Value.Collection.GetField("SvcParms").Collection.GetField("RqstUUID").EncodedValue, "check the inner collection is populated");
        }

        [Test]
        public void TestSimpleWebService_ComplexSend_InnerNamespace()
        {
            var act = new clsWebServiceAction();
            act.SetName("consumeComplexService");
            var soap = new clsSoap(clsSoap.SoapVersion.Soap12) { SoapAction = "POST", Action = act, Namespace = "mytestnamespace", BindingStyle = System.Web.Services.Description.SoapBindingStyle.Document };
            var svcParms = new clsCollection();
            svcParms.AddField("SvcID", DataType.text);
            svcParms.AddField("ApplID", DataType.text);
            svcParms.AddField("SvcVer", DataType.text);
            svcParms.AddField("RqstUUID", DataType.text);
            var myColRow = new clsCollectionRow();
            myColRow.SetField("SvcID", new clsProcessValue("Should be ApplID"));
            myColRow.SetField("ApplID", new clsProcessValue("Should be SvcID"));
            myColRow.SetField("SvcVer", new clsProcessValue("Should be SvcVer"));
            myColRow.SetField("RqstUUID", new clsProcessValue("Should be RqstUUID"));
            svcParms.Add(myColRow);
            var svcCol = new clsCollection();
            svcCol.AddField("SvcParms", DataType.collection);
            svcCol.AddField("ErrCde", DataType.text);
            svcCol.AddField("ErrMsg", DataType.text);
            var myOuterColRow = new clsCollectionRow();
            myOuterColRow.SetField("SvcParms", new clsProcessValue(svcParms));
            myOuterColRow.SetField("ErrCde", new clsProcessValue("Should be SvcParms"));
            myOuterColRow.SetField("ErrMsg", new clsProcessValue("Should be SvcParms"));
            svcCol.Add(myOuterColRow);
            var inputs = new clsArgumentList
            {
                new clsArgument("MtvnSvcVer", new clsProcessValue(DataType.text, "should be MtvnSvcVer")),
                new clsArgument("MsgUUID", new clsProcessValue(DataType.text, "should be MsgUUID")),
                new clsArgument("ErrCde", new clsProcessValue(DataType.text, "should be ErrCde1")),
                new clsArgument("ErrMsg", new clsProcessValue(DataType.text, "should be ErrMsg1")),
                new clsArgument("Svc", new clsProcessValue(svcCol))
            };

            var @params = new List<clsProcessParameter>() { new clsSoapParameter() { Name = "MtvnSvcVer", ParamType = DataType.text, Direction = ParamDirection.In }, new clsSoapParameter() { Name = "MsgUUID", ParamType = DataType.number, Direction = ParamDirection.In }, new clsSoapParameter() { Name = "ErrCde", ParamType = DataType.number, Direction = ParamDirection.In }, new clsSoapParameter() { Name = "ErrMsg", ParamType = DataType.text, Direction = ParamDirection.In } };
            var x = new clsSoapParameter() { Name = "Svc", ParamType = DataType.collection, Direction = ParamDirection.In };
            x.CollectionInfo.AddField(new clsCollectionFieldInfo("ErrCde", DataType.number, string.Empty));
            x.CollectionInfo.AddField(new clsCollectionFieldInfo("ErrMsg", DataType.text, string.Empty));
            var y = new clsCollectionFieldInfo("SvcParms", DataType.collection, "mycustomnamespace");
            y.Children.AddField(new clsCollectionFieldInfo("SvcID", DataType.text, string.Empty));
            y.Children.AddField(new clsCollectionFieldInfo("ApplID", DataType.text, string.Empty));
            y.Children.AddField(new clsCollectionFieldInfo("SvcVer", DataType.text, string.Empty));
            y.Children.AddField(new clsCollectionFieldInfo("RqstUUID", DataType.text, string.Empty));
            x.CollectionInfo.AddField(y);
            @params.Add(x);
            using (var ms = new MemoryStream())
            {
                soap.SendMessage(ms, inputs, @params);
                ms.Position = 0L;
                var sr = new StreamReader(ms);
                var sentMessage = sr.ReadToEnd();
                var expected = @"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                                                         xmlns:xs=""http://www.w3.org/2001/XMLSchema""
                                                         xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/""
                                                         xmlns:tns=""mytestnamespace""
                                                         xmlns:ns1=""mycustomnamespace""
                                                         xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                                         <soap:Body>
                                                             <tns:consumeComplexService>
                                                                 <MtvnSvcVer xsi:type=""xs:"">should be MtvnSvcVer</MtvnSvcVer>
                                                                 <MsgUUID xsi:type=""xs:"">should be MsgUUID</MsgUUID>
                                                                 <ErrCde xsi:type=""xs:"">should be ErrCde1</ErrCde>
                                                                 <ErrMsg xsi:type=""xs:"">should be ErrMsg1</ErrMsg>
                                                                 <Svc>
                                                                     <ErrCde>Should be SvcParms</ErrCde>
                                                                     <ErrMsg>Should be SvcParms</ErrMsg>
                                                                     <ns1:SvcParms>
                                                                         <SvcID>Should be ApplID</SvcID>
                                                                         <ApplID>Should be SvcID</ApplID>
                                                                         <SvcVer>Should be SvcVer</SvcVer>
                                                                         <RqstUUID>Should be RqstUUID</RqstUUID>
                                                                     </ns1:SvcParms>
                                                                 </Svc>
                                                             </tns:consumeComplexService>
                                                         </soap:Body>
                                                     </soap:Envelope>"; 

                expected = Regex.Replace(expected, @"\s+", " ").Replace("\r\n", " ").Replace("> ", ">").Replace(" <", "<");

                Assert.AreEqual("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + expected, sentMessage, "Check the message sent was as expected.");
            }
        }

        [Test]
        public void TestSimpleWebService_ComplexSend()
        {
            var act = new clsWebServiceAction();
            act.SetName("consumeComplexService");
            var soap = new clsSoap(clsSoap.SoapVersion.Soap12) { SoapAction = "POST", Action = act, Namespace = "mytestnamespace", BindingStyle = System.Web.Services.Description.SoapBindingStyle.Document };
            var svcParms = new clsCollection();
            svcParms.AddField("SvcID", DataType.text);
            svcParms.AddField("ApplID", DataType.text);
            svcParms.AddField("SvcVer", DataType.text);
            svcParms.AddField("RqstUUID", DataType.text);
            var myColRow = new clsCollectionRow();
            myColRow.SetField("SvcID", new clsProcessValue("Should be ApplID"));
            myColRow.SetField("ApplID", new clsProcessValue("Should be SvcID"));
            myColRow.SetField("SvcVer", new clsProcessValue("Should be SvcVer"));
            myColRow.SetField("RqstUUID", new clsProcessValue("Should be RqstUUID"));
            svcParms.Add(myColRow);
            var svcCol = new clsCollection();
            svcCol.AddField("SvcParms", DataType.collection);
            svcCol.AddField("ErrCde", DataType.text);
            svcCol.AddField("ErrMsg", DataType.text);
            var myOuterColRow = new clsCollectionRow();
            myOuterColRow.SetField("SvcParms", new clsProcessValue(svcParms));
            myOuterColRow.SetField("ErrCde", new clsProcessValue("Should be ErrCde"));
            myOuterColRow.SetField("ErrMsg", new clsProcessValue("Should be ErrMsg"));
            svcCol.Add(myOuterColRow);
            var inputs = new clsArgumentList
            {
                new clsArgument("MtvnSvcVer", new clsProcessValue(DataType.text, "should be MtvnSvcVer")),
                new clsArgument("MsgUUID", new clsProcessValue(DataType.text, "should be MsgUUID")),
                new clsArgument("ErrCde", new clsProcessValue(DataType.text, "should be ErrCde1")),
                new clsArgument("ErrMsg", new clsProcessValue(DataType.text, "should be ErrMsg1")),
                new clsArgument("Svc", new clsProcessValue(svcCol))
            };

            var @params = new List<clsProcessParameter>() { new clsSoapParameter() { Name = "MtvnSvcVer", ParamType = DataType.text, Direction = ParamDirection.In }, new clsSoapParameter() { Name = "MsgUUID", ParamType = DataType.number, Direction = ParamDirection.In }, new clsSoapParameter() { Name = "ErrCde", ParamType = DataType.number, Direction = ParamDirection.In }, new clsSoapParameter() { Name = "ErrMsg", ParamType = DataType.text, Direction = ParamDirection.In } };
            var x = new clsSoapParameter() { Name = "Svc", ParamType = DataType.collection, Direction = ParamDirection.In };
            x.CollectionInfo.AddField(new clsCollectionFieldInfo("ErrCde", DataType.number, string.Empty));
            x.CollectionInfo.AddField(new clsCollectionFieldInfo("ErrMsg", DataType.text, string.Empty));
            var y = new clsCollectionFieldInfo("SvcParms", DataType.collection, string.Empty);
            y.Children.AddField(new clsCollectionFieldInfo("SvcID", DataType.text, string.Empty));
            y.Children.AddField(new clsCollectionFieldInfo("ApplID", DataType.text, string.Empty));
            y.Children.AddField(new clsCollectionFieldInfo("SvcVer", DataType.text, string.Empty));
            y.Children.AddField(new clsCollectionFieldInfo("RqstUUID", DataType.text, string.Empty));
            x.CollectionInfo.AddField(y);
            @params.Add(x);
            using (var ms = new MemoryStream())
            {
                soap.SendMessage(ms, inputs, @params);
                ms.Position = 0L;
                var sr = new StreamReader(ms);
                var sentMessage = sr.ReadToEnd();
                var expected = @"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                                                         xmlns:xs=""http://www.w3.org/2001/XMLSchema""
                                                         xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/""
                                                         xmlns:tns=""mytestnamespace""
                                                         xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                                         <soap:Body>
                                                             <tns:consumeComplexService>
                                                                 <MtvnSvcVer xsi:type=""xs:"">should be MtvnSvcVer</MtvnSvcVer>
                                                                 <MsgUUID xsi:type=""xs:"">should be MsgUUID</MsgUUID>
                                                                 <ErrCde xsi:type=""xs:"">should be ErrCde1</ErrCde>
                                                                 <ErrMsg xsi:type=""xs:"">should be ErrMsg1</ErrMsg>
                                                                 <Svc>
                                                                     <ErrCde>Should be ErrCde</ErrCde>
                                                                     <ErrMsg>Should be ErrMsg</ErrMsg>
                                                                     <SvcParms>
                                                                         <SvcID>Should be ApplID</SvcID>
                                                                         <ApplID>Should be SvcID</ApplID>
                                                                         <SvcVer>Should be SvcVer</SvcVer>
                                                                         <RqstUUID>Should be RqstUUID</RqstUUID>
                                                                     </SvcParms>
                                                                 </Svc>
                                                             </tns:consumeComplexService>
                                                         </soap:Body>
                                                     </soap:Envelope>";

                expected = Regex.Replace(expected, @"\s+", " ").Replace("\r\n", " ").Replace("> ", ">").Replace(" <", "<");

                Assert.AreEqual("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + expected, sentMessage, "Check the message sent was as expected.");
            }
        }

        [Test]
        public void TestSimpleWebService_ComplexSend_NoNamespace()
        {
            var act = new clsWebServiceAction();
            act.SetName("consumeComplexService");
            var soap = new clsSoap(clsSoap.SoapVersion.Soap12) { SoapAction = "POST", Action = act, Namespace = string.Empty, BindingStyle = System.Web.Services.Description.SoapBindingStyle.Document };
            var svcParms = new clsCollection();
            svcParms.AddField("SvcID", DataType.text);
            svcParms.AddField("ApplID", DataType.text);
            svcParms.AddField("SvcVer", DataType.text);
            svcParms.AddField("RqstUUID", DataType.text);
            var myColRow = new clsCollectionRow();
            myColRow.SetField("SvcID", new clsProcessValue("Should be ApplID"));
            myColRow.SetField("ApplID", new clsProcessValue("Should be SvcID"));
            myColRow.SetField("SvcVer", new clsProcessValue("Should be SvcVer"));
            myColRow.SetField("RqstUUID", new clsProcessValue("Should be RqstUUID"));
            svcParms.Add(myColRow);
            var svcCol = new clsCollection();
            svcCol.AddField("SvcParms", DataType.collection);
            svcCol.AddField("ErrCde", DataType.text);
            svcCol.AddField("ErrMsg", DataType.text);
            var myOuterColRow = new clsCollectionRow();
            myOuterColRow.SetField("SvcParms", new clsProcessValue(svcParms));
            myOuterColRow.SetField("ErrCde", new clsProcessValue("Should be SvcParms"));
            myOuterColRow.SetField("ErrMsg", new clsProcessValue("Should be SvcParms"));
            svcCol.Add(myOuterColRow);
            var inputs = new clsArgumentList
            {
                new clsArgument("MtvnSvcVer", new clsProcessValue(DataType.text, "should be MtvnSvcVer")),
                new clsArgument("MsgUUID", new clsProcessValue(DataType.text, "should be MsgUUID")),
                new clsArgument("ErrCde", new clsProcessValue(DataType.text, "should be ErrCde1")),
                new clsArgument("ErrMsg", new clsProcessValue(DataType.text, "should be ErrMsg1")),
                new clsArgument("Svc", new clsProcessValue(svcCol))
            };

            var @params = new List<clsProcessParameter>
            {
                new clsSoapParameter() { Name = "MtvnSvcVer", ParamType = DataType.text, Direction = ParamDirection.In },
                new clsSoapParameter() { Name = "MsgUUID", ParamType = DataType.number, Direction = ParamDirection.In },
                new clsSoapParameter() { Name = "ErrCde", ParamType = DataType.number, Direction = ParamDirection.In },
                new clsSoapParameter() { Name = "ErrMsg", ParamType = DataType.text, Direction = ParamDirection.In }
            };
            var x = new clsSoapParameter() { Name = "Svc", ParamType = DataType.collection, Direction = ParamDirection.In };
            x.CollectionInfo.AddField(new clsCollectionFieldInfo("ErrCde", DataType.number, string.Empty));
            x.CollectionInfo.AddField(new clsCollectionFieldInfo("ErrMsg", DataType.text, string.Empty));
            var y = new clsCollectionFieldInfo("SvcParms", DataType.collection, string.Empty);
            y.Children.AddField(new clsCollectionFieldInfo("SvcID", DataType.text, string.Empty));
            y.Children.AddField(new clsCollectionFieldInfo("ApplID", DataType.text, string.Empty));
            y.Children.AddField(new clsCollectionFieldInfo("SvcVer", DataType.text, string.Empty));
            y.Children.AddField(new clsCollectionFieldInfo("RqstUUID", DataType.text, string.Empty));
            x.CollectionInfo.AddField(y);
            @params.Add(x);
            using (var ms = new MemoryStream())
            {
                soap.SendMessage(ms, inputs, @params);
                ms.Position = 0L;
                var sr = new StreamReader(ms);
                var sentMessage = sr.ReadToEnd();
                var expected = @"<soap:Envelope xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                                                         xmlns:xs=""http://www.w3.org/2001/XMLSchema""
                                                         xmlns:soapenc=""http://schemas.xmlsoap.org/soap/encoding/""
                                                         xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                                                         <soap:Body>
                                                             <consumeComplexService>
                                                                 <MtvnSvcVer xsi:type=""xs:"">should be MtvnSvcVer</MtvnSvcVer>
                                                                 <MsgUUID xsi:type=""xs:"">should be MsgUUID</MsgUUID>
                                                                 <ErrCde xsi:type=""xs:"">should be ErrCde1</ErrCde>
                                                                 <ErrMsg xsi:type=""xs:"">should be ErrMsg1</ErrMsg>
                                                                 <Svc>
                                                                     <ErrCde>Should be SvcParms</ErrCde>
                                                                     <ErrMsg>Should be SvcParms</ErrMsg>
                                                                     <SvcParms>
                                                                         <SvcID>Should be ApplID</SvcID>
                                                                         <ApplID>Should be SvcID</ApplID>
                                                                         <SvcVer>Should be SvcVer</SvcVer>
                                                                         <RqstUUID>Should be RqstUUID</RqstUUID>
                                                                     </SvcParms>
                                                                 </Svc>
                                                             </consumeComplexService>
                                                         </soap:Body>
                                                     </soap:Envelope>"; 

                expected = Regex.Replace(expected, @"\s+", " ").Replace("\r\n", " ").Replace("> ", ">").Replace(" <", "<");

                Assert.AreEqual("<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + expected, sentMessage, "Check the message sent was as expected.");
            }
        }
    }
}
#endif
