using System.Runtime.InteropServices;

namespace FastBmpServer
{
    internal static class BmpProcess
    {
        public static void ProcessInPlace(byte[] data, int length)
        {
            DateTime startTime = DateTime.Now;
            // Ensure the data is large enough to contain the headers
            Span<byte> span = data.AsSpan(0, length);

            // Read the BMP headers
            BmpFileHeader fileHeader = MemoryMarshal.AsRef<BmpFileHeader>(span);
            // Validate the BMP signature
            BmpInfoHeader infoHeader = MemoryMarshal.AsRef<BmpInfoHeader>(span.Slice(14));

            ApplyGrayFilterParallel(data, fileHeader.Offset, infoHeader.Width, infoHeader.Height, infoHeader.BitCount);

            TimeSpan elapsedTime = DateTime.Now - startTime;
            Console.WriteLine($"BMP operation completed in {elapsedTime.TotalSeconds:F2} seconds.");
        }

        private unsafe static void ApplyGrayFilterParallel(byte[] data, int offset, int width, int height, int bitCount)
        {
            int bytesPerPixel = bitCount / 8;
            int stride = (width * bitCount + 31) / 32 * 4;

            fixed (byte* pBase = data)
            {
                byte* pPixelStart = pBase + offset;
                nint startAddress = (nint)pPixelStart;
                Parallel.For(0, height, y =>
                {
                    {
                        byte* pRow = (byte*)startAddress + (y * stride);
                        for (int x = 0; x < width; x++)
                        {
                            BmpPixel* pixel = (BmpPixel*)(pRow + (x * bytesPerPixel));

                            byte gray = (byte)(0.299f * pixel->Red + 0.587f * pixel->Green + 0.114f * pixel->Blue);

                            pixel->Blue = gray;
                            pixel->Green = gray;
                            pixel->Red = gray;
                        }
                    }
                });
            }
        }
    }
}
