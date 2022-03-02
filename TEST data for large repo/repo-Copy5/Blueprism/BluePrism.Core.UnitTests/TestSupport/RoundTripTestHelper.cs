#if UNITTESTS

using BluePrism.UnitTesting.TestSupport;
using FluentAssertions;

namespace BluePrism.Core.TestSupport
{
    /// <summary>
    /// Performs serialisation - deserialisation on an object, and then checks that the 
    /// deserialised object is the same as the original object.
    /// </summary>
    public class RoundTripTestHelper
    {
        public static void DoRoundTripAndTest<T>(T obj)
        {

            var returned = ServiceUtil.DoDataContractRoundTrip(obj);
            obj.ShouldBeEquivalentTo(returned);

        }

    }
}

#endif