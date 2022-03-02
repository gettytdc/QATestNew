using System;
using System.Runtime.InteropServices;

namespace BluePrism.UIAutomation
{
    internal static class ComHelper
    {
        /// <summary>
        /// Tries the get a value from COM and returns a default value if this fails.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="getResult">The function to get the result.</param>
        /// <returns>The result of a call to getResult or <c>default(<typeparamref name="TResult"/>)</c> if a COM-based exception is thrown</returns>
        public static TResult TryGetComValue<TResult>(Func<TResult> getResult)
        {
            try
            {
                return getResult();
            }
            catch (COMException)
            {
                return default(TResult);
            }
            catch (ApplicationException)
            {
                return default(TResult);
            }
        }
    }
}
