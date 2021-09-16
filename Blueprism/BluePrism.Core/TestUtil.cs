using System.Collections.Generic;

namespace BluePrism.Core
{
    /// <summary>
    /// Utility class to aid in unit tests. Primarily intended for use for setting up
    /// test cases which might be used in various tests in various projects.
    /// </summary>
    public static class TestUtil
    {
        /// <summary>
        /// A set of tests for use when testing passwords.
        /// </summary>
        public static IEnumerable<string> PasswordTests
        {
            get
            {
                return new string[]{
                    "Password1",
                    "#^&_[]()=@*^%$'¬`;:><,+-",
                    "",
                    " ",
                    "ÄäÖöÜüß",
                    "\0\r\n\t\r\n"
                };
            }
        }
    }
}

