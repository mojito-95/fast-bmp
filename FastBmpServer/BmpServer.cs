using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;

namespace FastBmpServer
{
    internal class BmpServer
    {
        private const int MAX_ALLOWED_SIZE = 100 * 1024 * 1024; // 100 MB
        
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(5);
        private static ArrayPool<byte> _serverPool = ArrayPool<byte>.Create(100 * 1024 * 1024, 5);

        public async Task StartServerAsync()
        {
            TcpListener? listener = new TcpListener(IPAddress.Any, 8080);
            if (listener is not null)
            {
                listener.Start();
                Console.WriteLine("BMP Server is running on port 8080...");
                while (true)
                {
                    Socket client = await listener.AcceptSocketAsync();
                    _ = Task.Run(async () => {
                        await _semaphore.WaitAsync();
                        try
                        {
                            await ProcessClientAsync(client);
                        }
                        finally
                        {
                            _semaphore.Release();
                        }
                    });
                }
            }
        }

        private async Task ProcessClientAsync(Socket socket)
        {
            using (NetworkStream stream = new NetworkStream(socket, true))
            {
                DateTime startTime = DateTime.Now;
                // Read the first 4 bytes to determine the size of the incoming BMP file
                byte[] sizeBuffer = new byte[4];
                await stream.ReadExactlyAsync(sizeBuffer);
                int fileSize = BitConverter.ToInt32(sizeBuffer);

                // Validate the file size to prevent potential DoS attacks
                if (fileSize > MAX_ALLOWED_SIZE)
                {
                    Console.WriteLine($"Connection attempt refused: Invalid size ({fileSize} bytes)");
                    socket.Close();
                    return;
                }

                // Rent a buffer from the pool to read the BMP file
                byte[] imageBuffer = _serverPool.Rent(fileSize);
                try
                {
                    // Read the BMP file data into the buffer
                    await stream.ReadExactlyAsync(imageBuffer.AsMemory(0, fileSize));

                    BmpProcess.ProcessInPlace(imageBuffer, fileSize);

                    // Send the processed BMP file back to the client
                    await stream.WriteAsync(imageBuffer.AsMemory(0, fileSize));
                }
                finally
                {
                    TimeSpan elapsedTime = DateTime.Now - startTime;
                    Console.WriteLine($"Server operation completed in {elapsedTime.TotalSeconds:F2} seconds.");

                    if (imageBuffer is not null)
                        _serverPool.Return(imageBuffer);
                    socket.Close();
                }
            }
        }
    }
}
