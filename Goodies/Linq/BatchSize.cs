using System;
using System.Runtime.InteropServices;

namespace BusterWood.Linq
{
    static class BatchSize<T>
    {
        public static readonly int Suggested;

        static BatchSize()
        {
            int pageSize = Environment.SystemPageSize; // usually 4K
            if (!Environment.Is64BitProcess)
                pageSize /= 2; // use less memory on x86

            pageSize -= IntPtr.Size * 3; // for array
            if (typeof(T).IsClass)
            {
                Suggested = (int)Math.Floor(pageSize / (double)IntPtr.Size);
            }
            else
            {

                int structSize = Marshal.SizeOf(typeof(T));
                Suggested = (int)Math.Floor(pageSize / (double)structSize);
            }
        }
    }


}
