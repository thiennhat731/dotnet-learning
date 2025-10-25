using day05.Services;
using System.Diagnostics;

Console.WriteLine("===== Day 07: Async & Task =====");

var api = new FakeApiService();
var endpoints = new List<string>
{
    "https://api.example.com/users",
    "https://api.example.com/products",
    "https://api.example.com/orders"
};

var sw = Stopwatch.StartNew();
await api.FetchMultipleAsync(endpoints);
sw.Stop();

Console.WriteLine($"\n⏱️ Total execution time: {sw.ElapsedMilliseconds} ms");

// Parallel.ForEach example
Console.WriteLine("\n===== Parallel.ForEach Demo =====");
Parallel.ForEach(endpoints, endpoint =>
{
    Console.WriteLine($"Thread {Thread.CurrentThread.ManagedThreadId} → {endpoint}");
    Thread.Sleep(1000);
});
Console.WriteLine("\n===== END OF DAY 07 =====");
