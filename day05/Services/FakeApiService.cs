namespace day05.Services;

public class FakeApiService
{
    private static readonly Random random = new();

    public async Task<string> GetDataAsync(string endpoint)
    {
        Console.WriteLine($"Fetching from {endpoint}...");
        await Task.Delay(random.Next(1000, 3000)); // mô phỏng độ trễ API
        return $"Response from {endpoint} at {DateTime.Now:T}";
    }

    public async Task FetchMultipleAsync(List<string> endpoints)
    {
        var tasks = endpoints.Select(GetDataAsync).ToList();
        var results = await Task.WhenAll(tasks);

        Console.WriteLine("\nAll responses received:");
        foreach (var res in results)
            Console.WriteLine(res);
    }
}
