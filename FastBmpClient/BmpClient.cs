using System;
using System.Buffers;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace FastBmpClient
{
    internal class BmpClient
    {
        private const string SERVER_ADDRESS = "127.0.0.1";
        private const int SERVER_PORT = 8080;

        private const int BUFFER_SIZE = 64 * 1024; // 64 KB

        private readonly string outputFilePath;

        public BmpClient(string outputFilePath)
        {
            this.outputFilePath = outputFilePath;
        }

        public async Task StartClientAsync()
        {
            // Implement the client logic to connect to the server, send a BMP file, and receive the processed file
            // This is a placeholder for the actual client implementation
            Console.WriteLine("BMP Client is running...");
            DateTime startTime = DateTime.Now;

            byte[] buffer = ArrayPool<byte>.Shared.Rent(BUFFER_SIZE);
            try
            {
                using (MemoryStream bmpStream = GenerateRandomBmpStream(4096, 4096))
                {
                    int fileSize = (int)bmpStream.Length;

                    using (TcpClient tcpClient = new TcpClient())
                    {
                        Console.WriteLine($"Connecting to server at {SERVER_ADDRESS}:{SERVER_PORT}...");
                        await tcpClient.ConnectAsync(SERVER_ADDRESS, SERVER_PORT);
                        Console.WriteLine("Connected to server.");

                        using (NetworkStream networkStream = tcpClient.GetStream())
                        {
                            // Send the file size first
                            Console.WriteLine($"Sending file of size {fileSize / 1024} KB...");
                            byte[] sizeBuffer = BitConverter.GetBytes(fileSize);
                            await networkStream.WriteAsync(sizeBuffer, 0, sizeBuffer.Length);

                            await bmpStream.CopyToAsync(networkStream, BUFFER_SIZE);

                            Console.WriteLine("File data sent. Waiting for processed file...");
                            using (FileStream outputFileStream = new FileStream(outputFilePath, FileMode.Create, FileAccess.Write))
                            {
                                int totalReceived = 0;
                                while (totalReceived < fileSize)
                                {
                                    // Read the processed file data from the server in chunks
                                    int bytesRead = await networkStream.ReadAsync(buffer, 0, Math.Min(buffer.Length, fileSize - totalReceived));

                                    if (bytesRead == 0) break;

                                    // Write the received chunk to the output file
                                    await outputFileStream.WriteAsync(buffer, 0, bytesRead);

                                    totalReceived += bytesRead;
                                }

                                if (totalReceived == fileSize)
                                    Console.WriteLine($"Processed file saved to '{outputFilePath}'.");
                                else
                                    Console.WriteLine($"Warning: Received {totalReceived}/{fileSize} bytes. File may be corrupt.");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            finally
            {
                TimeSpan elapsedTime = DateTime.Now - startTime;
                Console.WriteLine($"Client operation completed in {elapsedTime.TotalSeconds:F2} seconds.");

                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        // Generate form with shapes and random colors to create a more complex image for testing
        private MemoryStream GenerateRandomBmpStream(int width, int height)
        {
            int stride = ((width * 24 + 31) / 32) * 4;
            int pixelDataSize = stride * height;
            int totalSize = 54 + pixelDataSize;

            byte[] bmpData = new byte[totalSize];
            Span<byte> span = bmpData.AsSpan();

            span[0] = (byte)'B'; span[1] = (byte)'M';
            MemoryMarshal.Write(span.Slice(2, 4), ref totalSize);
            int offset = 54;
            MemoryMarshal.Write(span.Slice(10, 4), ref offset);
            int headerSize = 40;
            short planes = 1, bitCount = 24;
            MemoryMarshal.Write(span.Slice(14, 4), ref headerSize);
            MemoryMarshal.Write(span.Slice(18, 4), ref width);
            MemoryMarshal.Write(span.Slice(22, 4), ref height);
            MemoryMarshal.Write(span.Slice(26, 2), ref planes);
            MemoryMarshal.Write(span.Slice(28, 2), ref bitCount);

            Random rnd = new Random();

            for (int i = 54; i < totalSize; i += 3)
            {
                bmpData[i] = 50;     // Blue
                bmpData[i + 1] = 20; // Green
                bmpData[i + 2] = 20; // Red
            }

            for (int f = 0; f < rnd.Next(32, 512); f++)
            {
                byte r = (byte)rnd.Next(100, 255);
                byte g = (byte)rnd.Next(100, 255);
                byte b = (byte)rnd.Next(100, 255);

                int shapeWidth = rnd.Next(100, 400);
                int shapeHeight = rnd.Next(100, 400);
                int startX = rnd.Next(0, width - shapeWidth);
                int startY = rnd.Next(0, height - shapeHeight);

                bool drawCircle = rnd.Next(0, 2) == 0;

                for (int y = startY; y < startY + shapeHeight; y++)
                {
                    for (int x = startX; x < startX + shapeWidth; x++)
                    {
                        bool shouldDraw = true;

                        if (drawCircle)
                        {
                            double centerX = startX + shapeWidth / 2.0;
                            double centerY = startY + shapeHeight / 2.0;
                            double radius = shapeWidth / 2.0;
                            double dist = Math.Pow(x - centerX, 2) + Math.Pow(y - centerY, 2);
                            shouldDraw = dist < Math.Pow(radius, 2);
                        }

                        if (shouldDraw)
                        {
                            int pIdx = 54 + (y * stride) + (x * 3);
                            bmpData[pIdx] = b;
                            bmpData[pIdx + 1] = g;
                            bmpData[pIdx + 2] = r;
                        }
                    }
                }
            }

            return new MemoryStream(bmpData);
        }
    }
}
