#if UNITTESTS

using System;
using BluePrism.BPServer.UrlReservations;
using NUnit.Framework;

namespace BluePrism.BPServer.UnitTests.UrlReservations
{

    /// <summary>
    /// Tests the <see cref="UrlReservationMatchingDecisionTable"/> class
    /// </summary>
    public class UrlMatchingDecisionTableTests
    {
        private Func<BindingProperties, BindingProperties, bool> ReturnTrue = (x, y) => true;
        private Func<BindingProperties, BindingProperties, bool> ReturnFalse = (x, y) => false;

        
        [Test]
        public void ShouldReturnTheDecisionThatMeetsAllOfTheConditions()
        {
            var tbl = new UrlReservationMatchingDecisionTable();
            tbl.Add(UrlReservationMatchType.None, ReturnFalse);
            tbl.Add(UrlReservationMatchType.ExactMatch, ReturnTrue, ReturnTrue, ReturnTrue, ReturnFalse);
            tbl.Add(UrlReservationMatchType.Conflict, ReturnTrue, ReturnTrue);
            
            // Actual urls used don't matter as the condition functions either return false or true
            // and don't handle the url
            var url1 = BindingProperties.ParseReservationUrl("http://*:8199/");
            var url2 = BindingProperties.ParseReservationUrl("http://myserver:8199/");

            Assert.That(tbl.MakeDecision(url1, url2), Is.EqualTo(UrlReservationMatchType.Conflict));

        }

        [Test]
        public void ShouldReturnFirstDecisionIfMultipleRowsMeetAllConditions()
        {
            var tbl = new UrlReservationMatchingDecisionTable();
            tbl.Add(UrlReservationMatchType.None, ReturnFalse);
            tbl.Add(UrlReservationMatchType.ExactMatch, ReturnTrue, ReturnTrue);
            tbl.Add(UrlReservationMatchType.ExactMatch, ReturnTrue, ReturnFalse, ReturnTrue);
            tbl.Add(UrlReservationMatchType.Conflict, ReturnTrue, ReturnTrue);
            
            // Actual urls used don't matter as the condition functions either return false or true
            // and don't handle the url
            var url1 = BindingProperties.ParseReservationUrl("http://*:8199/");
            var url2 = BindingProperties.ParseReservationUrl("http://myserver:8199/");

            Assert.That(tbl.MakeDecision(url1, url2), Is.EqualTo(UrlReservationMatchType.ExactMatch));
        }

        [Test]
        public void ShouldReturnNoMatch_NoRulesAdded()
        {   // create a table with an empty set of rules
            var tbl = new UrlReservationMatchingDecisionTable();
            
            // Actual urls used don't matter as the condition functions either return false or true
            // and don't handle the url
            var url1 = BindingProperties.ParseReservationUrl("http://*:8199/");
            var url2 = BindingProperties.ParseReservationUrl("http://myserver:8199/");

            Assert.That(tbl.MakeDecision(url1, url2), Is.EqualTo(UrlReservationMatchType.None));
        }

        [Test] 
        public void ShouldReturnNoMatch_NoRulesMeetAllConditions()
        {
            var tbl = new UrlReservationMatchingDecisionTable();
            tbl.Add(UrlReservationMatchType.Conflict, ReturnFalse);
            tbl.Add(UrlReservationMatchType.ExactMatch, ReturnFalse, ReturnFalse);
            tbl.Add(UrlReservationMatchType.None, ReturnFalse, ReturnTrue);
            tbl.Add(UrlReservationMatchType.Conflict, ReturnTrue, ReturnFalse);

            // Actual urls used don't matter as the condition functions either return false or true
            // and don't handle the url
            var url1 = BindingProperties.ParseReservationUrl("http://*:8199/");
            var url2 = BindingProperties.ParseReservationUrl("http://myserver:8199/");

            Assert.That(tbl.MakeDecision(url1, url2), Is.EqualTo(UrlReservationMatchType.None));
        }

        [Test]
        public void ShouldMatchRulesWithNoConditions()
        {   
            // rows with no conditions will match (as long as they are the first match)
            var tbl = new UrlReservationMatchingDecisionTable();
            var emptyConditions = new Func<BindingProperties, BindingProperties, bool>[0]; 

            tbl.Add(UrlReservationMatchType.None, ReturnFalse);
            tbl.Add(UrlReservationMatchType.Conflict, emptyConditions);
            tbl.Add(UrlReservationMatchType.ExactMatch, ReturnTrue, ReturnTrue);

            // Actual urls used don't matter as the condition functions either return false or true
            // and don't handle the url
            var url1 = BindingProperties.ParseReservationUrl("http://*:8199/");
            var url2 = BindingProperties.ParseReservationUrl("http://myserver:8199/");

            Assert.That(tbl.MakeDecision(url1, url2), Is.EqualTo(UrlReservationMatchType.Conflict));
        }

    }
}

#endif