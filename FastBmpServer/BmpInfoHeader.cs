using System.Runtime.InteropServices;

namespace FastBmpServer
{
    [StructLayout(LayoutKind.Explicit, Pack = 1)]
    public struct BmpInfoHeader
    {
        [FieldOffset(0)]
        [MarshalAs(UnmanagedType.I4)]
        public uint Size;
        [FieldOffset(4)]
        [MarshalAs(UnmanagedType.I4)]
        public int Width;
        [FieldOffset(8)]
        [MarshalAs(UnmanagedType.I4)]
        public int Height;
        [FieldOffset(12)]
        [MarshalAs(UnmanagedType.I2)]
        public ushort Planes;
        [FieldOffset(14)]
        [MarshalAs(UnmanagedType.I2)]
        public ushort BitCount;
        [FieldOffset(16)]
        [MarshalAs(UnmanagedType.I4)]
        public uint Compression;
        [FieldOffset(20)]
        [MarshalAs(UnmanagedType.I4)]
        public uint SizeImage;
        [FieldOffset(24)]
        [MarshalAs(UnmanagedType.I4)]
        public uint XPixelsPerMeter;
        [FieldOffset(28)]
        [MarshalAs(UnmanagedType.I4)]
        public uint YPixelsPerMeter;
        [FieldOffset(32)]
        [MarshalAs(UnmanagedType.I4)]
        public uint ColorsUsed;
        [FieldOffset(36)]
        [MarshalAs(UnmanagedType.I4)]
        public uint ColorsImportant;
    }
}
