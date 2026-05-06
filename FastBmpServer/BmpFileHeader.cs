using System.Runtime.InteropServices;

namespace FastBmpServer
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BmpFileHeader
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.I2)]
        public ushort Type;
        [FieldOffset(2)]
        [MarshalAs(UnmanagedType.I4)]
        public int Size;
        [FieldOffset(6)]
        [MarshalAs(UnmanagedType.I2)]
        public ushort Reserved1;
        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.I2)]
        public ushort Reserved2;
        [FieldOffset(10)]
        [MarshalAs(UnmanagedType.I4)]
        public int Offset;

        public string RealType => Type == 0x4D42 ? "BM" : "Unknown";
    }
}
