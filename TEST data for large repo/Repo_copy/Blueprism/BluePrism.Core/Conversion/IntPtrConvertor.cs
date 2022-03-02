using System;

namespace BluePrism.Core.Conversion
{
    public static class IntPtrConvertor
    {
        public static unsafe IntPtr PtrConvert(long value)
        {
            if (IntPtr.Size == 4)
            {
                if (value < 0)
                {
                    return new IntPtr((void*)checked((int)value));
                }
                else
                {
                    return new IntPtr((void*)checked((uint)value));
                }
            }
            else
            {
                return new IntPtr((void*)value);
            }
        }
    }
}
