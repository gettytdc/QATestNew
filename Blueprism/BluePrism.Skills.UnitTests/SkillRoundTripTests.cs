#if UNITTESTS
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using BluePrism.UnitTesting.TestSupport;
using BluePrism.Utilities.Functional;
using FluentAssertions;

namespace BluePrism.Skills.UnitTests
{
    [TestFixture]
    public class SkillRoundTripTests
    {
        [Test]
        public void DoRoundTrip_IsCorrectlyReturned()
        {
                                   
            var version = new WebSkillVersion(
                Guid.NewGuid(), 
                 "name", 
                 true,
                 "skill1",
                SkillCategory.Collaboration,
                "1.1",
                "description",
                GetTestIcon(),
                "6.3", 
                "6.3", 
                DateTime.Now, 
                new List<string>() { "action1", "action2"});

            var skill = new Skill(
                Guid.NewGuid(), 
                "provider",
                true,
                new[] { version });

            var roundTripped = ServiceUtil.DoDataContractRoundTrip(skill);

            roundTripped.ShouldBeEquivalentTo(skill);
        }

        private byte[] GetTestIcon() => new UnitTesting.TestSupport.TestBitmapGenerator()
                                                  .WithColour('R', Color.Red)
                                                  .WithPixels("R")
                                                  .Create()
                                                  .Map(x =>
                                                  {
                                                      using (var stream = new MemoryStream())
                                                      {
                                                          x.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                                                          return stream.ToArray();
                                                      }
                                                  });
        
    }
}
#endif
