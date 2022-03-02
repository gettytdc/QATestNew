using System;
using System.Threading;

namespace BluePrism.Core.Utility
{
    public class SafeSemaphoreSlim : SemaphoreSlim
    {
        public SafeSemaphoreSlim(int initialCount, int maxCount) : base(initialCount, maxCount)
        {
        }

        public void TryRelease()
        {
            try
            {
                Release();
            }
            catch (ObjectDisposedException) 
            {    
                // Prevents exception being thrown if release is attempted 
                // when object has already been disposed
            }
        }
    }
}
