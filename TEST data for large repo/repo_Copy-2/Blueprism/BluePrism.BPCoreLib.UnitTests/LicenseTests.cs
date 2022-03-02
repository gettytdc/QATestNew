#if UNITTESTS

using System;
using System.Collections.Generic;
using BluePrism.Server.Domain.Models;
using NUnit.Framework;

namespace BluePrism.BPCoreLib.UnitTests
{
    [TestFixture]
    public class LicenseTests
    {
        /// <summary>
        /// Tests that the start date aggregation properties do the right thing with a
        /// series of different start dates.
        /// </summary>
        [Test]
        public void TestEarliestStartDateAggregate()
        {
            // Empty AuthDetails means default licence - ie. Date.MinValue
            var info = LicenseInfo.LoadKeys(new KeyInfo[] { });
            Assert.That(info.StartDate, Is.EqualTo(DateTime.MinValue));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2016, 11, 14)}
            });
            Assert.That(info.StartDate, Is.EqualTo(new DateTime(2016, 11, 14)));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2016, 11, 14)},
                new LicenceTestSupport.Key {Starts = new DateTime(2016, 11, 12)}
            });
            Assert.That(info.StartDate, Is.EqualTo(new DateTime(2016, 11, 12)));

            // Not really sure about this one... should 'Date.MinValue' be returned here?
            // Should StartDate return the first non-MinValue date instead?
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = DateTime.MinValue},
                new LicenceTestSupport.Key {Starts = new DateTime(2016, 11, 12)}
            });
            Assert.That(info.StartDate, Is.EqualTo(DateTime.MinValue));
        }

        /// <summary>
        /// Tests that the expiry date aggregation properties do the right thing with a
        /// series of different start dates.
        /// </summary>
        [Test]
        public void TestLatestExpiryDateAggregate()
        {
            LicenseInfo info;

            // Empty AuthDetails means default licence - ie. Date.MinValue
            info = LicenseInfo.LoadKeys(new KeyInfo[] { });
            Assert.That(info.ExpiryDate, Is.EqualTo(LicenceTestSupport.Constants.MaxExpiryDate));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2016, 11, 14), Expires = new DateTime(2121, 12, 21)
                }
            });
            Assert.That(info.ExpiryDate, Is.EqualTo(new DateTime(2121, 12, 21)));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Expires = new DateTime(2116, 5, 17)},
                new LicenceTestSupport.Key {Expires = new DateTime(2091, 10, 31)},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2016, 11, 14), Expires = new DateTime(2091, 10, 31)
                }
            });
            Assert.That(info.ExpiryDate, Is.EqualTo(new DateTime(2116, 5, 17)));

            // Like the .StartDate = MinValue test; I'm not sure if this should be
            // ignoring the 'MaxExpiryDate' values when aggregating the expiry dates.
            // But at the moment, this is the behaviour
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key
                {
                    Expires = LicenceTestSupport.Constants.MaxExpiryDate
                },
                new LicenceTestSupport.Key {Expires = new DateTime(2018, 11, 12)}
            });
            Assert.That(info.ExpiryDate, Is.EqualTo(LicenceTestSupport.Constants.MaxExpiryDate));
        }

        /// <summary>
        /// Tests that the aggregation of processes works correctly
        /// </summary>
        [Test]
        public void TestProcessesAggregate()
        {
            LicenseInfo info;
            info = LicenseInfo.LoadKeys(new KeyInfo[] {new LicenceTestSupport.Key {Processes = 0}});
            Assert.That(info.AllowsUnlimitedPublishedProcesses, Is.False);
            Assert.That(info.NumPublishedProcesses, Is.EqualTo(0));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Processes = int.MaxValue}
            });
            Assert.That(info.AllowsUnlimitedPublishedProcesses, Is.True);
            Assert.That(info.NumPublishedProcesses, Is.EqualTo(int.MaxValue));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Processes = 0},
                new LicenceTestSupport.Key {Processes = 3},
                new LicenceTestSupport.Key {Processes = 2},
                new LicenceTestSupport.Key {Processes = 2}
            });
            Assert.That(info.AllowsUnlimitedPublishedProcesses, Is.False);
            Assert.That(info.NumPublishedProcesses, Is.EqualTo(7));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Processes = 0},
                new LicenceTestSupport.Key {Processes = 3},
                new LicenceTestSupport.Key {Processes = int.MaxValue},
                new LicenceTestSupport.Key {Processes = 2}
            });
            Assert.That(info.AllowsUnlimitedPublishedProcesses, Is.True);
            Assert.That(info.NumPublishedProcesses, Is.EqualTo(int.MaxValue));
        }

        /// <summary>
        /// Tests that the aggregation of resources works correctly
        /// </summary>
        [Test]
        public void TestResourcesAggregate()
        {
            LicenseInfo info;
            info = LicenseInfo.LoadKeys(new KeyInfo[] {new LicenceTestSupport.Key {Resources = 0}});
            Assert.That(info.AllowsUnlimitedResourcePCs, Is.False);
            Assert.That(info.NumResourcePCs, Is.EqualTo(0));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Resources = int.MaxValue}
            });
            Assert.That(info.AllowsUnlimitedResourcePCs, Is.True);
            Assert.That(info.NumResourcePCs, Is.EqualTo(int.MaxValue));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Resources = 0},
                new LicenceTestSupport.Key {Resources = 3},
                new LicenceTestSupport.Key {Resources = 2},
                new LicenceTestSupport.Key {Resources = 2}
            });
            Assert.That(info.AllowsUnlimitedResourcePCs, Is.False);
            Assert.That(info.NumResourcePCs, Is.EqualTo(7));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Resources = 0},
                new LicenceTestSupport.Key {Resources = 3},
                new LicenceTestSupport.Key {Resources = int.MaxValue},
                new LicenceTestSupport.Key {Resources = 2}
            });
            Assert.That(info.AllowsUnlimitedResourcePCs, Is.True);
            Assert.That(info.NumResourcePCs, Is.EqualTo(int.MaxValue));
        }

        /// <summary>
        /// Tests that the aggregation of sessions works correctly
        /// </summary>
        [Test]
        public void TestSessionsAggregate()
        {
            LicenseInfo info;
            info = LicenseInfo.LoadKeys(new KeyInfo[] {new LicenceTestSupport.Key {Sessions = 0}});
            Assert.That(info.AllowsUnlimitedSessions, Is.False);
            Assert.That(info.NumConcurrentSessions, Is.EqualTo(0));
            info = LicenseInfo.LoadKeys(
                new KeyInfo[] {new LicenceTestSupport.Key {Sessions = int.MaxValue}});
            Assert.That(info.AllowsUnlimitedSessions, Is.True);
            Assert.That(info.NumConcurrentSessions, Is.EqualTo(int.MaxValue));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Sessions = 0},
                new LicenceTestSupport.Key {Sessions = 3},
                new LicenceTestSupport.Key {Sessions = 2},
                new LicenceTestSupport.Key {Sessions = 2}
            });
            Assert.That(info.AllowsUnlimitedSessions, Is.False);
            Assert.That(info.NumConcurrentSessions, Is.EqualTo(7));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Sessions = 0},
                new LicenceTestSupport.Key {Sessions = 3},
                new LicenceTestSupport.Key {Sessions = int.MaxValue},
                new LicenceTestSupport.Key {Sessions = 2}
            });
            Assert.That(info.AllowsUnlimitedSessions, Is.True);
            Assert.That(info.NumConcurrentSessions, Is.EqualTo(int.MaxValue));
        }

        /// <summary>
        /// Tests that the aggregation of alerts PCs works correctly
        /// </summary>
        [Test]
        public void TestAlertsAggregate()
        {
            LicenseInfo info;
            info = LicenseInfo.LoadKeys(new KeyInfo[] {new LicenceTestSupport.Key {Alerts = 0}});
            Assert.That(info.AllowsUnlimitedProcessAlertsPCs, Is.False);
            Assert.That(info.NumProcessAlertsPCs, Is.EqualTo(0));
            info = LicenseInfo.LoadKeys(new KeyInfo[] {new LicenceTestSupport.Key {Alerts = int.MaxValue}});
            Assert.That(info.AllowsUnlimitedProcessAlertsPCs, Is.True);
            Assert.That(info.NumProcessAlertsPCs, Is.EqualTo(int.MaxValue));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Alerts = 0},
                new LicenceTestSupport.Key {Alerts = 3},
                new LicenceTestSupport.Key {Alerts = 2},
                new LicenceTestSupport.Key {Alerts = 2}
            });
            Assert.That(info.AllowsUnlimitedProcessAlertsPCs, Is.False);
            Assert.That(info.NumProcessAlertsPCs, Is.EqualTo(7));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Alerts = 0},
                new LicenceTestSupport.Key {Alerts = 3},
                new LicenceTestSupport.Key {Alerts = int.MaxValue},
                new LicenceTestSupport.Key {Alerts = 2}
            });
            Assert.That(info.AllowsUnlimitedProcessAlertsPCs, Is.True);
            Assert.That(info.NumProcessAlertsPCs, Is.EqualTo(int.MaxValue));
        }

        /// <summary>
        /// Tests that the aggregation of transaction model settings works correctly
        /// </summary>
        [Test]
        public void TestTransactionModelAggregate()
        {
            LicenseInfo info;

            // Default should always be false
            info = LicenseInfo.LoadKeys(new KeyInfo[] { });
            Assert.That(info.TransactionModel, Is.False);
            info = LicenseInfo.LoadKeys(new KeyInfo[] {new LicenceTestSupport.Key {Processes = 1}});
            Assert.That(info.TransactionModel, Is.False);
            info = LicenseInfo.LoadKeys(new KeyInfo[] {new LicenceTestSupport.Key {TxnModel = true}});
            Assert.That(info.TransactionModel, Is.True);

            // With lots of keys, TxnModel should still remain false
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3},
                new LicenceTestSupport.Key {Resources = 5},
                new LicenceTestSupport.Key {Alerts = 2},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2001, 5, 7), Expires = new DateTime(2004, 7, 12)
                }
            });
            Assert.That(info.TransactionModel, Is.False);

            // However, if even one of the keys has it set, it needs to aggregate to 'on'
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3},
                new LicenceTestSupport.Key {Resources = 5},
                new LicenceTestSupport.Key {Alerts = 2},
                new LicenceTestSupport.Key {TxnModel = true},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2001, 5, 7), Expires = new DateTime(2004, 7, 12)
                }
            });
            Assert.That(info.TransactionModel, Is.True);
        }

        /// <summary>
        /// Tests the aggregation of the icons in the licence keys
        /// </summary>
        [Test]
        public void TestIconAggregate()
        {
            LicenseInfo info;

            // No keys should mean no icon
            info = LicenseInfo.LoadKeys(new KeyInfo[] { });
            Assert.That(info.OverrideIcon, Is.Null);

            // Single key with no icon should mean no icon
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3}
            });
            Assert.That(info.OverrideIcon, Is.Null);

            // Multiple keys with no icon should mean no icon
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3},
                new LicenceTestSupport.Key {Resources = 5},
                new LicenceTestSupport.Key {Alerts = 2},
                new LicenceTestSupport.Key {TxnModel = true},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2001, 5, 7), Expires = new DateTime(2004, 7, 12)
                }
            });
            Assert.That(info.OverrideIcon, Is.Null);

            // Single key with icon should return it
            info = LicenseInfo.LoadKeys(new KeyInfo[] {new LicenceTestSupport.Key {Icon = "a"}});
            Assert.That(info.OverrideIcon, Is.EqualTo("a"));

            // Multiple keys with icons - we return the latest active one added
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key
                {
                    Icon = "a", Installed = new DateTime(2016, 10, 2, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Icon = "b", Installed = new DateTime(2016, 10, 3, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Icon = "c", Installed = new DateTime(2016, 10, 1, 12, 30, 10)
                }
            });
            Assert.That(info.OverrideIcon, Is.EqualTo("b"));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3},
                new LicenceTestSupport.Key {Resources = 5},
                new LicenceTestSupport.Key
                {
                    Icon = "a", Installed = new DateTime(2016, 10, 2, 12, 30, 10)
                },
                new LicenceTestSupport.Key {Alerts = 2},
                new LicenceTestSupport.Key
                {
                    Icon = "b", Installed = new DateTime(2016, 10, 3, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Icon = "c", Installed = new DateTime(2016, 10, 1, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2001, 5, 7), Expires = new DateTime(2004, 7, 12)
                }
            });
            Assert.That(info.OverrideIcon, Is.EqualTo("b"));

            // Neither start nor end date make a difference to the icon selected
            // (note end dates are in the 22nd century to ensure they are all in effect)
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3},
                new LicenceTestSupport.Key {Resources = 5},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2110, 12, 31),
                    Icon = "a",
                    Installed = new DateTime(2016, 10, 5, 12, 30, 10)
                },
                new LicenceTestSupport.Key {Alerts = 2},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2011, 1, 1),
                    Icon = "b",
                    Installed = new DateTime(2016, 10, 2, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2009, 1, 1),
                    Icon = "c",
                    Installed = new DateTime(2016, 10, 1, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2001, 5, 7), Expires = new DateTime(2104, 7, 12)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2110, 6, 30),
                    Icon = "d",
                    Installed = new DateTime(2016, 10, 4, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2111, 6, 30),
                    Icon = "e",
                    Installed = new DateTime(2016, 10, 3, 12, 30, 10)
                }
            });
            Assert.That(info.OverrideIcon, Is.EqualTo("a"));

            // ...unless they happen to cause the key to not be in effect
            // (note that end dates are no longer in the 22nd century)
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3},
                new LicenceTestSupport.Key {Resources = 5},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2010, 12, 31),
                    Icon = "a",
                    Installed = new DateTime(2016, 10, 5, 12, 30, 10)
                },
                new LicenceTestSupport.Key {Alerts = 2},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2011, 1, 1),
                    Icon = "b",
                    Installed = new DateTime(2016, 10, 2, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2009, 1, 1),
                    Icon = "c",
                    Installed = new DateTime(2016, 10, 1, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2001, 5, 7), Expires = new DateTime(2004, 7, 12)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2010, 6, 30),
                    Icon = "d",
                    Installed = new DateTime(2016, 10, 4, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2011, 6, 30),
                    Icon = "e",
                    Installed = new DateTime(2016, 10, 3, 12, 30, 10)
                }
            });
            // "a" has expired; "b" is the next most recent in the list
            Assert.That(info.OverrideIcon, Is.EqualTo("b"));

            // Neither start nor end date make a difference to the icon selected
            // (note end dates are in the 22nd century to ensure they are all in effect)
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2013, 1, 1),
                    Icon = "a",
                    Installed = new DateTime(2016, 11, 5, 7, 0, 0)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2011, 1, 1),
                    Icon = "b",
                    Installed = new DateTime(2016, 11, 5, 6, 0, 0)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2009, 1, 1),
                    Icon = "c",
                    Installed = new DateTime(2016, 11, 5, 8, 0, 0)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2011, 1, 1),
                    Icon = "d",
                    Installed = new DateTime(2016, 11, 4, 7, 0, 0)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2009, 1, 1),
                    Icon = "e",
                    Installed = new DateTime(2016, 11, 6, 7, 0, 0)
                }
            });
            Assert.That(info.OverrideIcon, Is.EqualTo("e"));
        }

        /// <summary>
        /// Tests the aggregation of the logo overrides in the licence keys
        /// </summary>
        [Test]
        public void TestLogoAggregate()
        {
            LicenseInfo info;

            // No keys should mean no logo
            info = LicenseInfo.LoadKeys(new KeyInfo[] { });
            Assert.That(info.OverrideLargeLogo, Is.Null);

            // Single key with no logo should mean no logo
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3}
            });
            Assert.That(info.OverrideLargeLogo, Is.Null);

            // Multiple keys with no logo should mean no logo
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3},
                new LicenceTestSupport.Key {Resources = 5},
                new LicenceTestSupport.Key {Alerts = 2},
                new LicenceTestSupport.Key {TxnModel = true},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2001, 5, 7), Expires = new DateTime(2004, 7, 12)
                }
            });
            Assert.That(info.OverrideLargeLogo, Is.Null);

            // Single key with logo should return it
            info = LicenseInfo.LoadKeys(new KeyInfo[] {new LicenceTestSupport.Key {Logo = "a"}});
            Assert.That(info.OverrideLargeLogo, Is.EqualTo("a"));

            // Multiple keys with logos - we return the latest active one added
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key
                {
                    Logo = "a", Installed = new DateTime(2016, 10, 2, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Logo = "b", Installed = new DateTime(2016, 10, 3, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Logo = "c", Installed = new DateTime(2016, 10, 1, 12, 30, 10)
                }
            });
            Assert.That(info.OverrideLargeLogo, Is.EqualTo("b"));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3},
                new LicenceTestSupport.Key {Resources = 5},
                new LicenceTestSupport.Key
                {
                    Logo = "a", Installed = new DateTime(2016, 10, 2, 12, 30, 10)
                },
                new LicenceTestSupport.Key {Alerts = 2},
                new LicenceTestSupport.Key
                {
                    Logo = "b", Installed = new DateTime(2016, 10, 3, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Logo = "c", Installed = new DateTime(2016, 10, 1, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2001, 5, 7), Expires = new DateTime(2004, 7, 12)
                }
            });
            Assert.That(info.OverrideLargeLogo, Is.EqualTo("b"));

            // Neither start nor end date make a difference to the logo selected
            // (note end dates are in the 22nd century to ensure they are all in effect)
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3},
                new LicenceTestSupport.Key {Resources = 5},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2110, 12, 31),
                    Logo = "a",
                    Installed = new DateTime(2016, 10, 5, 12, 30, 10)
                },
                new LicenceTestSupport.Key {Alerts = 2},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2011, 1, 1),
                    Logo = "b",
                    Installed = new DateTime(2016, 10, 2, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2009, 1, 1),
                    Logo = "c",
                    Installed = new DateTime(2016, 10, 1, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2001, 5, 7), Expires = new DateTime(2104, 7, 12)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2110, 6, 30),
                    Logo = "d",
                    Installed = new DateTime(2016, 10, 4, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2111, 6, 30),
                    Logo = "e",
                    Installed = new DateTime(2016, 10, 3, 12, 30, 10)
                }
            });
            Assert.That(info.OverrideLargeLogo, Is.EqualTo("a"));

            // ...unless they happen to cause the key to not be in effect
            // (note that end dates are no longer in the 22nd century)
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3},
                new LicenceTestSupport.Key {Resources = 5},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2010, 12, 31),
                    Logo = "a",
                    Installed = new DateTime(2016, 10, 5, 12, 30, 10)
                },
                new LicenceTestSupport.Key {Alerts = 2},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2011, 1, 1),
                    Logo = "b",
                    Installed = new DateTime(2016, 10, 2, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2009, 1, 1),
                    Logo = "c",
                    Installed = new DateTime(2016, 10, 1, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2001, 5, 7), Expires = new DateTime(2004, 7, 12)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2010, 6, 30),
                    Logo = "d",
                    Installed = new DateTime(2016, 10, 4, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2011, 6, 30),
                    Logo = "e",
                    Installed = new DateTime(2016, 10, 3, 12, 30, 10)
                }
            });
            // "a" has expired; "b" is the next most recent in the list
            Assert.That(info.OverrideLargeLogo, Is.EqualTo("b"));

            // Neither start nor end date make a difference to the icon selected
            // (note end dates are in the 22nd century to ensure they are all in effect)
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2013, 1, 1),
                    Logo = "a",
                    Installed = new DateTime(2016, 11, 5, 7, 0, 0)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2011, 1, 1),
                    Logo = "b",
                    Installed = new DateTime(2016, 11, 5, 6, 0, 0)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2009, 1, 1),
                    Logo = "c",
                    Installed = new DateTime(2016, 11, 5, 8, 0, 0)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2011, 1, 1),
                    Logo = "d",
                    Installed = new DateTime(2016, 11, 4, 7, 0, 0)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2009, 1, 1),
                    Logo = "e",
                    Installed = new DateTime(2016, 11, 6, 7, 0, 0)
                }
            });
            Assert.That(info.OverrideLargeLogo, Is.EqualTo("e"));
        }

        /// <summary>
        /// Tests the aggregation of the title overrides in the licence keys
        /// </summary>
        [Test]
        public void TestTitleAggregate()
        {
            // No keys should mean no title
            var info = LicenseInfo.LoadKeys(new KeyInfo[] { });
            Assert.That(info.OverrideTitle, Is.Null);

            // Single key with no title should mean no title
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3}
            });
            Assert.That(info.OverrideTitle, Is.Null);

            // Multiple keys with no title should mean no title
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3},
                new LicenceTestSupport.Key {Resources = 5},
                new LicenceTestSupport.Key {Alerts = 2},
                new LicenceTestSupport.Key {TxnModel = true},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2001, 5, 7), Expires = new DateTime(2004, 7, 12)
                }
            });
            Assert.That(info.OverrideTitle, Is.Null);

            // Single key with title should return it
            var iinfo = new LicenceTestSupport.Key {Title = "a"};
            info = LicenseInfo.LoadKeys(new KeyInfo[] { iinfo });
            Assert.That(info.OverrideTitle, Is.EqualTo("a"));

            // Multiple keys with titles - we return the latest active one added
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key
                {
                    Title = "a", Installed = new DateTime(2016, 10, 2, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Title = "b", Installed = new DateTime(2016, 10, 3, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Title = "c", Installed = new DateTime(2016, 10, 1, 12, 30, 10)
                }
            });
            Assert.That(info.OverrideTitle, Is.EqualTo("b"));
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3},
                new LicenceTestSupport.Key {Resources = 5},
                new LicenceTestSupport.Key
                {
                    Title = "a", Installed = new DateTime(2016, 10, 2, 12, 30, 10)
                },
                new LicenceTestSupport.Key {Alerts = 2},
                new LicenceTestSupport.Key
                {
                    Title = "b", Installed = new DateTime(2016, 10, 3, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Title = "c", Installed = new DateTime(2016, 10, 1, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2001, 5, 7), Expires = new DateTime(2004, 7, 12)
                }
            });
            Assert.That(info.OverrideTitle, Is.EqualTo("b"));

            // Neither start nor end date make a difference to the title selected
            // (note end dates are in the 22nd century to ensure they are all in effect)
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3},
                new LicenceTestSupport.Key {Resources = 5},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2110, 12, 31),
                    Title = "a",
                    Installed = new DateTime(2016, 10, 5, 12, 30, 10)
                },
                new LicenceTestSupport.Key {Alerts = 2},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2011, 1, 1),
                    Title = "b",
                    Installed = new DateTime(2016, 10, 2, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2009, 1, 1),
                    Title = "c",
                    Installed = new DateTime(2016, 10, 1, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2001, 5, 7), Expires = new DateTime(2104, 7, 12)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2110, 6, 30),
                    Title = "d",
                    Installed = new DateTime(2016, 10, 4, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2111, 6, 30),
                    Title = "e",
                    Installed = new DateTime(2016, 10, 3, 12, 30, 10)
                }
            });
            Assert.That(info.OverrideTitle, Is.EqualTo("a"));

            // ...unless they happen to cause the key to not be in effect
            // (note that end dates are no longer in the 22nd century)
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = new DateTime(2001, 5, 7), Processes = 3},
                new LicenceTestSupport.Key {Resources = 5},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2010, 12, 31),
                    Title = "a",
                    Installed = new DateTime(2016, 10, 5, 12, 30, 10)
                },
                new LicenceTestSupport.Key {Alerts = 2},
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2011, 1, 1),
                    Title = "b",
                    Installed = new DateTime(2016, 10, 2, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2009, 1, 1),
                    Title = "c",
                    Installed = new DateTime(2016, 10, 1, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2001, 5, 7), Expires = new DateTime(2004, 7, 12)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2010, 6, 30),
                    Title = "d",
                    Installed = new DateTime(2016, 10, 4, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2011, 6, 30),
                    Title = "e",
                    Installed = new DateTime(2016, 10, 3, 12, 30, 10)
                }
            });
            // "a" has expired; "b" is the next most recent in the list
            Assert.That(info.OverrideTitle, Is.EqualTo("b"));

            // Neither start nor end date make a difference to the icon selected
            // (note end dates are in the 22nd century to ensure they are all in effect)
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2013, 1, 1),
                    Title = "a",
                    Installed = new DateTime(2016, 11, 5, 7, 0, 0)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2011, 1, 1),
                    Title = "b",
                    Installed = new DateTime(2016, 11, 5, 6, 0, 0)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2009, 1, 1),
                    Title = "c",
                    Installed = new DateTime(2016, 11, 5, 8, 0, 0)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2011, 1, 1),
                    Title = "d",
                    Installed = new DateTime(2016, 11, 4, 7, 0, 0)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2009, 1, 1),
                    Title = "e",
                    Installed = new DateTime(2016, 11, 6, 7, 0, 0)
                }
            });
            Assert.That(info.OverrideTitle, Is.EqualTo("e"));
        }

        /// <summary>
        /// Tests the aggregation of the overall license owner in the licence keys
        /// </summary>
        [Test]
        public void TestOwnerAggregate()
        {
            LicenseInfo info;

            // No keys should mean no owner
            info = LicenseInfo.LoadKeys(new KeyInfo[] { });
            Assert.That(info.LicenseOwner, Is.Null);

            // Single key should return the owner
            info = LicenseInfo.LoadKeys(new KeyInfo[] {new LicenceTestSupport.Key {Name = "a"}});
            Assert.That(info.LicenseOwner, Is.EqualTo("a"));

            // Multiple keys - we return the owner from the latest active one added
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key
                {
                    Name = "a", Installed = new DateTime(2016, 10, 2, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Name = "b", Installed = new DateTime(2016, 10, 3, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Name = "c", Installed = new DateTime(2016, 10, 1, 12, 30, 10)
                }
            });
            Assert.That(info.LicenseOwner, Is.EqualTo("b"));

            // Neither start nor end date make a difference to the owner selected
            // (note end dates are in the 22nd century to ensure they are all in effect)
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2110, 12, 31),
                    Name = "a",
                    Installed = new DateTime(2016, 10, 5, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2011, 1, 1),
                    Name = "b",
                    Installed = new DateTime(2016, 10, 2, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2009, 1, 1),
                    Name = "c",
                    Installed = new DateTime(2016, 10, 1, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2110, 6, 30),
                    Name = "d",
                    Installed = new DateTime(2016, 10, 4, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2111, 6, 30),
                    Name = "e",
                    Installed = new DateTime(2016, 10, 3, 12, 30, 10)
                }
            });
            Assert.That(info.LicenseOwner, Is.EqualTo("a"));

            // ...unless they happen to cause the key to not be in effect
            // (note that end dates are no longer in the 22nd century)
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2010, 12, 31),
                    Name = "a",
                    Installed = new DateTime(2016, 10, 5, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2011, 1, 1),
                    Name = "b",
                    Installed = new DateTime(2016, 10, 2, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2009, 1, 1),
                    Name = "c",
                    Installed = new DateTime(2016, 10, 1, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2010, 6, 30),
                    Name = "d",
                    Installed = new DateTime(2016, 10, 4, 12, 30, 10)
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2010, 1, 1),
                    Expires = new DateTime(2011, 6, 30),
                    Name = "e",
                    Installed = new DateTime(2016, 10, 3, 12, 30, 10)
                }
            });
            // "a" has expired; "b" is the next most recent in the list
            Assert.That(info.LicenseOwner, Is.EqualTo("b"));
        }

        /// <summary>
        /// Tests that any licences which are not in effect don't count towards the
        /// aggregate values of the compound licence.
        /// </summary>
        [Test]
        public void TestLicencesNotInEffectDontCount()
        {
            LicenseInfo info;

            // I chose the 22nd century; I suspect these unit tests won't still be running
            // by then. Anyway, [0] has expired; [3] is yet to start, only [1] & [2] are
            // valid licences which are currently in effect.
            info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2015, 1, 1),
                    Expires = new DateTime(2015, 5, 4),
                    Processes = 2,
                    Resources = 5,
                    Sessions = int.MaxValue
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2016, 11, 10),
                    Expires = new DateTime(2121, 12, 21),
                    Processes = 1,
                    Resources = int.MaxValue,
                    Sessions = 5
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2016, 5, 7),
                    Expires = new DateTime(2094, 5, 6),
                    Processes = 1,
                    Resources = 2,
                    Sessions = 1
                },
                new LicenceTestSupport.Key
                {
                    Starts = new DateTime(2150, 11, 10),
                    Processes = int.MaxValue,
                    Resources = 0,
                    Sessions = 1,
                    Alerts = 21
                }
            });
            Assert.That(info.StartDate, Is.EqualTo(new DateTime(2016, 5, 7)));
            Assert.That(info.ExpiryDate, Is.EqualTo(new DateTime(2121, 12, 21)));
            Assert.That(info.AllowsUnlimitedPublishedProcesses, Is.False);
            Assert.That(info.NumPublishedProcesses, Is.EqualTo(2));
            Assert.That(info.AllowsUnlimitedResourcePCs, Is.True);
            Assert.That(info.NumResourcePCs, Is.EqualTo(int.MaxValue));
            Assert.That(info.AllowsUnlimitedSessions, Is.False);
            Assert.That(info.NumConcurrentSessions, Is.EqualTo(6));
            Assert.That(info.AllowsUnlimitedProcessAlertsPCs, Is.False);
            Assert.That(info.NumProcessAlertsPCs, Is.EqualTo(0));
        }

        private List<KeyInfo> GetLicenseKeys()
        {
            var lic = Licensing.License;

            return lic.LicenseType == LicenseTypes.None ? new List<KeyInfo>() : new List<KeyInfo>(lic.LicenseKeys);
        }


        /// <summary>
        /// Tests the adding of licenses to the currently configured licences in the
        /// runtime
        /// </summary>
        [Test]
        public void TestCanAddLicence()
        {
            // Test adding to an empty set
            Licensing.SetLicenseKeys(new List<KeyInfo>());
            Assert.That(GetLicenseKeys(), Is.Empty);
            var result = Licensing.DoesNotOverlap(new KeyInfo(LicenceTestSupport.Constants.Licence2016),
                GetLicenseKeys());
            Assert.That(result, Is.True);

            // Test adding to an existing set
            Licensing.SetLicenseKeys(new List<KeyInfo>
            {
                new KeyInfo(LicenceTestSupport.Constants.Licence2015)
            });
            Assert.That(GetLicenseKeys(),
                Is.EqualTo(new List<KeyInfo> {new KeyInfo(LicenceTestSupport.Constants.Licence2015)}));
            result = Licensing.DoesNotOverlap(new KeyInfo(LicenceTestSupport.Constants.Licence2016),
                GetLicenseKeys());
            Assert.That(result, Is.True);
        }

        [Test]
        public void TestLicenseChanges_NoLicensesInstalled()
        {
            var warning = default(bool);
            var info = LicenseInfo.LoadKeys(new KeyInfo[] { });
            Assert.That(info.GetLicenseChangeMessage(ref warning), Is.Empty);
            Assert.That(info.IsLicensed, Is.False);
        }

        [Test]
        public void TestLicenseChanges_SingleExpiredLicense()
        {
            var warning = default(bool);
            var info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Expires = DateTime.Today.AddDays(-1)}
            });
            Assert.That(info.GetLicenseChangeMessage(ref warning), Is.Empty);
            Assert.That(info.IsLicensed, Is.False);
        }

        [Test]
        public void TestLicenseChanges_SingleFutureLicense_WithinChangePeriod()
        {
            var info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Starts = DateTime.Today.AddDays(5d)}
            });
            var warning = true;
            Assert.AreEqual(info.GetLicenseChangeMessage(ref warning),
                "The license entitlement will be increased in 5 days. See the license screen for further details.");
            Assert.IsFalse(warning);
            Assert.That(info.IsLicensed, Is.False);
        }

        [Test]
        public void TestLicenseChanges_SingleFutureLicense_AfterChangePeriod()
        {
            var info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key
                {
                    Starts = DateTime.Today.AddDays(LicenseChanges.NumberOfDays)
                }
            });
            var warning = true;
            Assert.That(info.GetLicenseChangeMessage(ref warning), Is.Empty);
            Assert.IsTrue(warning);
            Assert.That(info.IsLicensed, Is.False);
        }

        [Test]
        public void TestLicenseChanges_SingleLicenseExpires_WithinChangePeriod()
        {
            var warning = default(bool);
            var info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Expires = DateTime.Today.AddDays(5d)}
            });
            Assert.That(info.GetLicenseChangeMessage(ref warning),
                Does.StartWith("All license keys will expire within 6 days"));
            Assert.That(warning, Is.True);
            Assert.That(info.IsLicensed, Is.True);
        }

        [Test]
        public void TestLicenseChanges_WithinChangePeriod_AfterChangePeriod()
        {
            var warning = default(bool);
            var info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Expires = DateTime.Today.AddDays(5d)},
                new LicenceTestSupport.Key {Starts = DateTime.Today.AddDays(7d)}
            });
            Assert.That(info.GetLicenseChangeMessage(ref warning),
                Does.StartWith("All license keys will expire within 6 days"));
            Assert.That(warning, Is.True);
            Assert.That(info.IsLicensed, Is.True);
        }

        [Test]
        public void TestLicenseChanges_WithinChangePeriod_NextDay()
        {
            var warning = default(bool);
            var info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Expires = DateTime.Today.AddDays(5d)},
                new LicenceTestSupport.Key {Starts = DateTime.Today.AddDays(6d)}
            });
            Assert.That(info.GetLicenseChangeMessage(ref warning),
                Does.StartWith(
                    "A number of license changes will occur within the next 6 days. Next change is in 6 days"));
            Assert.That(warning, Is.False);
            Assert.That(info.IsLicensed, Is.True);
        }

        [Test]
        public void TestLicenseChanges_WithinChangePeriod_PreviousDay()
        {
            var warning = default(bool);
            var info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key {Expires = DateTime.Today.AddDays(5d)},
                new LicenceTestSupport.Key {Starts = DateTime.Today.AddDays(4d)}
            });
            Assert.That(info.GetLicenseChangeMessage(ref warning),
                Does.StartWith(
                    "A number of license changes will occur within the next 6 days. Next change is in 4 days"));
            Assert.That(warning, Is.False);
            Assert.That(info.IsLicensed, Is.True);
        }

        [Test]
        public void TestLicenseChanges_TwoLicenses_WithinChangePeriod()
        {
            var warning = default(bool);
            var info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key(),
                new LicenceTestSupport.Key {Starts = DateTime.Today.AddDays(5d)}
            });
            Assert.That(info.GetLicenseChangeMessage(ref warning),
                Does.StartWith("The license entitlement will be increased in 5 days"));
            Assert.That(warning, Is.False);
            Assert.That(info.IsLicensed, Is.True);
        }

        [Test]
        public void TestLicenseChanges_TwoLicenses_AfterChangePeriod()
        {
            var warning = default(bool);
            var info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key(),
                new LicenceTestSupport.Key
                {
                    Starts = DateTime.Today.AddDays(LicenseChanges.NumberOfDays)
                }
            });
            Assert.That(info.GetLicenseChangeMessage(ref warning), Is.Empty);
            Assert.That(info.IsLicensed, Is.True);
        }

        [Test]
        public void TestLicenseChanges_TwoLicenses_OneExpiresWithinChangePeriod()
        {
            var warning = default(bool);
            var info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key(),
                new LicenceTestSupport.Key {Expires = DateTime.Today.AddDays(5d)}
            });
            Assert.That(info.GetLicenseChangeMessage(ref warning),
                Does.StartWith("The license entitlement will be decreased in 6 days"));
            Assert.That(warning, Is.True);
            Assert.That(info.IsLicensed, Is.True);
        }

        [Test]
        public void TestLicenseChanges_TwoLicenses_OneExpiresAfterChangePeriod()
        {
            var warning = default(bool);
            var info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key(),
                new LicenceTestSupport.Key
                {
                    Expires = DateTime.Today.AddDays(LicenseChanges.NumberOfDays)
                }
            });
            Assert.That(info.GetLicenseChangeMessage(ref warning), Is.Empty);
            Assert.That(info.IsLicensed, Is.True);
        }

        [Test]
        public void TestLicenseChanges_RequiresActivationBeforeGraceEnds()
        {
            var info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key
                {
                    Starts = DateTime.Today,
                    Expires = DateTime.Today.AddDays(100d),
                    GracePeriod = 1,
                    RequiresActivation = true
                }
            });
            var warning = false;
            Assert.AreEqual(
                "The license entitlement will be decreased in 1 day. See the license screen for further details.",
                info.GetLicenseChangeMessage(ref warning));
            Assert.IsTrue(warning);
            Assert.That(info.IsLicensed, Is.True);
        }

        [Test]
        public void TestLicenseChanges_StandaloneRequiresActivationBeforeGraceEnds()
        {
            var info = LicenseInfo.LoadKeys(new KeyInfo[]
            {
                new LicenceTestSupport.Key
                {
                    Starts = DateTime.Today,
                    Expires = DateTime.Today.AddDays(100d),
                    GracePeriod = 1,
                    RequiresActivation = true,
                    StandAlone = true
                }
            });
            var warning = false;
            Assert.AreEqual(
                "The standalone license must be activated - there are 1 day remaining until the end of the grace period",
                info.GetLicenseChangeMessage(ref warning));
            Assert.IsTrue(warning);
            Assert.That(info.IsLicensed, Is.True);
        }

        [Test]
        public void GetLicenseActivationRequest_DataLengthOKForKeysize() =>
            Assert.DoesNotThrow(() =>
                Licensing.CheckMaxDataLengthForKeysizeUsingOAEP(982, 8192, "")); // 8192 keysize, 982 max datalen

        [Test]
        public void GetLicenseActivationRequest_DataLengthExceedsKeysize() =>
            Assert.Throws<BluePrismException>(() =>
                Licensing.CheckMaxDataLengthForKeysizeUsingOAEP(983, 8192, "")); // 8192 keysize, 982 max datalen
    }
}

#endif
