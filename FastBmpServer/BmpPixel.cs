using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace FastBmpServer
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BmpPixel
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.I1)]
        public byte Blue;
        [FieldOffset(1)]
        [MarshalAs(UnmanagedType.I1)]
        public byte Green;
        [FieldOffset(2)]
        [MarshalAs(UnmanagedType.I1)]
        public byte Red;
    }
}
