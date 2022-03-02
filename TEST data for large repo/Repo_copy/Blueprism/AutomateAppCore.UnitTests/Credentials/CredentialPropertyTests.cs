#if UNITTESTS
using System.Collections.Generic;
using BluePrism.AutomateAppCore;
using BluePrism.BPCoreLib.Collections;
using BluePrism.Common.Security;
using BluePrism.UnitTesting.TestSupport;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.Credentials
{
    [TestFixture]
    public class CredentialPropertyTests
    {
        [Test]
        public void TestEmptyCredentialPropertyRoundTrip_Binary()
        {
            var prop = new CredentialProperty();
            Assert.That(ServiceUtil.DoBinarySerializationRoundTrip(prop), Is.EqualTo(prop));
        }

        [Test]
        public void TestEmptyCredentialPropertyRoundTrip_DataContract()
        {
            var prop = new CredentialProperty();
            Assert.That(ServiceUtil.DoDataContractRoundTrip(prop), Is.EqualTo(prop));
        }

        [Test]
        public void TestDeletedCredentialPropertyRoundTrip_Binary()
        {
            var prop = new CredentialProperty() { IsDeleted = true };
            Assert.That(ServiceUtil.DoBinarySerializationRoundTrip(prop), Is.EqualTo(prop));
        }

        [Test]
        public void TestDeletedCredentialPropertyRoundTrip_DataContract()
        {
            var prop = new CredentialProperty() { IsDeleted = true };
            Assert.That(ServiceUtil.DoDataContractRoundTrip(prop), Is.EqualTo(prop));
        }

        [Test]
        public void TestCompleteCredentialPropertyRoundTrip_Binary()
        {
            var prop = new CredentialProperty()
            {
                OldName = "Old Name",
                NewName = "New Name",
                Password = new SafeString("This is safe, right?")
            };
            Assert.That(ServiceUtil.DoBinarySerializationRoundTrip(prop), Is.EqualTo(prop));
        }

        [Test]
        public void TestCompleteCredentialPropertyRoundTrip_DataContract()
        {
            var prop = new CredentialProperty()
            {
                OldName = "Old Name",
                NewName = "New Name",
                Password = new SafeString("This is safe, right?")
            };
            Assert.That(ServiceUtil.DoDataContractRoundTrip(prop), Is.EqualTo(prop));
        }

        [Test]
        public void TestPropertyResolver_Empty()
        {
            var resolver = new CredentialPropertyResolver();
            var result = resolver.Resolve();
            Assert.That(result, Is.Not.Null.And.Empty);
        }

        [Test]
        public void TestPropertyResolver_NullChangesMeansNoChanges()
        {
            var resolver = new CredentialPropertyResolver()
            {
                ExistingProperties = new Dictionary<string, SafeString>()
                {
                    { "one",new SafeString("one-safe")},
                    { "two",new SafeString("two-safe")},
                    { "three",new SafeString("three-safe")}
                }
            };
            var result = resolver.Resolve();
            Assert.That(result, Is.EqualTo(new Dictionary<string, SafeString>()
            {
                {"one",new SafeString("one-safe")},
                {"two",new SafeString("two-safe")},
                {"three",new SafeString("three-safe")}
            }));
        }

        [Test]
        public void TestPropertyResolver_EmptyChangesMeansDeleteAllByOmission()
        {
            var resolver = new CredentialPropertyResolver()
            {
                ExistingProperties = new Dictionary<string, SafeString>()
                {
                    {"one",new SafeString("one-safe")},
                    {"two",new SafeString("two-safe")},
                    {"three",new SafeString("three-safe")}
                },
                PropertyChanges = GetEmpty.IEnumerable<CredentialProperty>()
            };
            var result = resolver.Resolve();
            Assert.That(result, Is.Not.Null.And.Empty);
        }

        [Test]
        public void TestPropertyResolver_NoChanges()
        {
            var resolver = new CredentialPropertyResolver()
            {
                ExistingProperties = new Dictionary<string, SafeString>()
                {
                    {"one",new SafeString("one-safe")},
                    {"two",new SafeString("two-safe")},
                    {"three",new SafeString("three-safe")}
                },
                PropertyChanges = new CredentialProperty[]
                {
                    new CredentialProperty() { OldName = "one", NewName = "one" },
                    new CredentialProperty() { OldName = "two", NewName = "two" },
                    new CredentialProperty() { OldName = "three", NewName = "three" }
                }
            };
            var result = resolver.Resolve();
            Assert.That(result, Is.EqualTo(new Dictionary<string, SafeString>()
            {
                {"one",new SafeString("one-safe")},
                {"two",new SafeString("two-safe")},
                {"three",new SafeString("three-safe")}
            }));
        }

        [Test]
        public void TestPropertyResolver_DeleteWithFlag()
        {
            var resolver = new CredentialPropertyResolver()
            {
                ExistingProperties = new Dictionary<string, SafeString>()
                {
                    {"one",new SafeString("one-safe")},
                    {"two",new SafeString("two-safe")},
                    {"three",new SafeString("three-safe")}
                },
                PropertyChanges = new CredentialProperty[]
                {
                    new CredentialProperty() { OldName = "one", NewName = "one" },
                    new CredentialProperty() { OldName = "two", NewName = "two", IsDeleted = true },
                    new CredentialProperty() { OldName = "three", NewName = "three" }
                }
            };
            var result = resolver.Resolve();
            Assert.That(result, Is.EqualTo(new Dictionary<string, SafeString>()
            {
                {"one",new SafeString("one-safe")},
                {"three",new SafeString("three-safe")}
            }));
        }

        [Test]
        public void TestPropertyResolver_DeleteByOmission()
        {
            var resolver = new CredentialPropertyResolver()
            {
                ExistingProperties = new Dictionary<string, SafeString>()
                {
                    {"one",new SafeString("one-safe")},
                    {"two",new SafeString("two-safe")},
                    {"three",new SafeString("three-safe")}
                },
                PropertyChanges = new CredentialProperty[]
                {
                    new CredentialProperty() { OldName = "one", NewName = "one" },
                    new CredentialProperty() { OldName = "three", NewName = "three" }
                }
            };
            var result = resolver.Resolve();
            Assert.That(result, Is.EqualTo(new Dictionary<string, SafeString>()
            {
                {"one",new SafeString("one-safe")},
                {"three",new SafeString("three-safe")}
            }));
        }

        [Test]
        public void TestPropertyResolver_ChangeAPassword()
        {
            var resolver = new CredentialPropertyResolver()
            {
                ExistingProperties = new Dictionary<string, SafeString>()
                {
                    {"one",new SafeString("one-safe")},
                    {"two",new SafeString("two-safe")},
                    {"three",new SafeString("three-safe")}
                },
                PropertyChanges = new CredentialProperty[]
                {
                    new CredentialProperty() { OldName = "one", NewName = "one" },
                    new CredentialProperty() { OldName = "two", NewName = "two", Password = new SafeString("two-changed") },
                    new CredentialProperty() { OldName = "three", NewName = "three" }
                }
            };
            var result = resolver.Resolve();
            Assert.That(result, Is.EqualTo(new Dictionary<string, SafeString>()
            {
                {"one",new SafeString("one-safe")},
                {"two",new SafeString("two-changed")},
                {"three",new SafeString("three-safe")}
            }));
        }

        [Test]
        public void TestPropertyResolver_SwapNames()
        {
            var resolver = new CredentialPropertyResolver()
            {
                ExistingProperties = new Dictionary<string, SafeString>()
                {
                    {"one",new SafeString("one-safe")},
                    {"two",new SafeString("two-safe")},
                    {"three",new SafeString("three-safe")}
                },
                PropertyChanges = new CredentialProperty[] {
                    new CredentialProperty() { OldName = "one", NewName = "two" },
                    new CredentialProperty() { OldName = "two", NewName = "one" },
                    new CredentialProperty() { OldName = "three", NewName = "three" }
                }
            };
            var result = resolver.Resolve();
            Assert.That(result, Is.EqualTo(new Dictionary<string, SafeString>()
            {
                {"two",new SafeString("one-safe")},
                {"one",new SafeString("two-safe")},
                {"three",new SafeString("three-safe")}
            }));
        }

        [Test]
        public void TestPropertyResolver_SwapNamesAndSetPasswords()
        {
            var resolver = new CredentialPropertyResolver()
            {
                ExistingProperties = new Dictionary<string, SafeString>()
                {
                    {"one",new SafeString("one-safe")},
                    {"two",new SafeString("two-safe")},
                    {"three",new SafeString("three-safe")}
                },
                PropertyChanges = new CredentialProperty[]
                {
                    new CredentialProperty() { OldName = "one", NewName = "two", Password = new SafeString("one-two-changed") },
                    new CredentialProperty() { OldName = "two", NewName = "one", Password = new SafeString("two-one-changed") },
                    new CredentialProperty() { OldName = "three", NewName = "three" }
                }
            };
            var result = resolver.Resolve();
            Assert.That(result, Is.EqualTo(new Dictionary<string, SafeString>()
            {
                {"two",new SafeString("one-two-changed")},
                {"one",new SafeString("two-one-changed")},
                {"three",new SafeString("three-safe")}
            }));
        }

        [Test]
        public void TestPropertyResolver_NewPropertyEmptyOldName()
        {
            var resolver = new CredentialPropertyResolver()
            {
                ExistingProperties = new Dictionary<string, SafeString>()
                {
                    {"one",new SafeString("one-safe")},
                    {"two",new SafeString("two-safe")},
                    {"three",new SafeString("three-safe")}
                },
                PropertyChanges = new CredentialProperty[]
                {
                    new CredentialProperty() { OldName = "one", NewName = "one" },
                    new CredentialProperty() { OldName = "two", NewName = "two" },
                    new CredentialProperty() { OldName = "three", NewName = "three" },
                    new CredentialProperty() { NewName = "four", Password = new SafeString("four-new") }
                }
            };
            var result = resolver.Resolve();
            Assert.That(result, Is.EqualTo(new Dictionary<string, SafeString>()
            {
                {"one",new SafeString("one-safe")},
                { "two",new SafeString("two-safe")},
                { "three",new SafeString("three-safe")},
                {"four",new SafeString("four-new")}
            }));
        }

        [Test]
        public void TestPropertyResolver_NewPropertyNullOldName()
        {
            var resolver = new CredentialPropertyResolver()
            {
                ExistingProperties = new Dictionary<string, SafeString>()
                {
                    {"one",new SafeString("one-safe")},
                    {"two",new SafeString("two-safe")},
                    {"three",new SafeString("three-safe")}
                },
                PropertyChanges = new CredentialProperty[]
                {
                    new CredentialProperty() { OldName = "one", NewName = "one" },
                    new CredentialProperty() { OldName = "two", NewName = "two" },
                    new CredentialProperty() { OldName = "three", NewName = "three" },
                    new CredentialProperty() { OldName = null, NewName = "four", Password = new SafeString("four-new") }
                }
            };
            var result = resolver.Resolve();
            Assert.That(result, Is.EqualTo(new Dictionary<string, SafeString>()
            {
                {"one",new SafeString("one-safe")},
                {"two",new SafeString("two-safe")},
                {"three",new SafeString("three-safe")},
                {"four",new SafeString("four-new")}
             }));
        }

        [Test]
        public void TestPropertyResolver_NewPropertyWithOldName()
        {
            var resolver = new CredentialPropertyResolver()
            {
                ExistingProperties = new Dictionary<string, SafeString>()
                {
                    {"one",new SafeString("one-safe")},
                    {"two",new SafeString("two-safe")},
                    {"three",new SafeString("three-safe")}
                },
                PropertyChanges = new CredentialProperty[]
                {
                    new CredentialProperty() { OldName = "one", NewName = "one", IsDeleted = true },
                    new CredentialProperty() { OldName = "two", NewName = "two" },
                    new CredentialProperty() { OldName = "three", NewName = "three" },
                    new CredentialProperty() { NewName = "one", Password = new SafeString("one-new") }
                }
            };
            var result = resolver.Resolve();
            Assert.That(result, Is.EqualTo(new Dictionary<string, SafeString>()
            {
                {"two",new SafeString("two-safe")},
                {"three",new SafeString("three-safe")},
                {"one",new SafeString("one-new")}
            }));
        }

        [Test]
        public void TestPropertyResolver_SwapsNewPasswordsNewPropertiesAndDeletions()
        {
            var resolver = new CredentialPropertyResolver()
            {
                ExistingProperties = new Dictionary<string, SafeString>()
                {
                    {"one",new SafeString("one-safe")},
                    {"two",new SafeString("two-safe")},
                    {"three",new SafeString("three-safe")
                }
            },
                PropertyChanges = new CredentialProperty[]
                {
                    new CredentialProperty() { OldName = "one", NewName = "two", Password = new SafeString("one-two-changed") },
                    new CredentialProperty() { OldName = "two", NewName = "one", Password = new SafeString("two-one-changed") },
                    new CredentialProperty() { OldName = "three", NewName = "three", IsDeleted = true },
                    new CredentialProperty() { OldName = null, NewName = "four", Password = new SafeString("four-new") },
                    new CredentialProperty() { OldName = "", NewName = "five", Password = new SafeString("five-new") }
                }
            };
            var result = resolver.Resolve();
            Assert.That(result, Is.EqualTo(new Dictionary<string, SafeString>()
            {
                {"two",new SafeString("one-two-changed")},
                {"one",new SafeString("two-one-changed")},
                {"four",new SafeString("four-new")},
                {"five",new SafeString("five-new")}
            }));
        }
    }
}
#endif