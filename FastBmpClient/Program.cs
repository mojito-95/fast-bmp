using FastBmpClient;

while (true)
{
    Console.WriteLine("\nHow many requests do you want to send? (1, 5, 10, 50, 100) or 'q' to quit:");
    string input = Console.ReadLine() ?? "";

    if (input.ToLower() == "q") break;

    if (int.TryParse(input, out int count))
    {
        Console.WriteLine($"--- Sending {count} requests ---");

        List<Task> tasks = new List<Task>();
        for (int i = 0; i < count; i++)
        {
            int requestId = i;
            BmpClient bmpClient = new BmpClient(Path.Combine(Directory.GetCurrentDirectory(), $"output_{requestId}.bmp"));

            tasks.Add(bmpClient.StartClientAsync());
        }

        await Task.WhenAll(tasks);

        Console.WriteLine($"Finished sending {count} requests.");
    }
    else
    {
        Console.WriteLine("Invalid input. Please enter a number.");
    }
}

Console.WriteLine("Exiting...");