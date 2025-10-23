using day04.Models;
namespace day04.Examples;

public class LinqExample
{
    public static void Run()
    {
        var products = new List<Product>
        {
            new("Laptop", 1500, "Electronics"),
            new("Keyboard", 40, "Electronics"),
            new("Book", 20, "Stationery"),
            new("Pen", 5, "Stationery"),
            new("Shoes", 120, "Fashion")
        };

        Console.Write("Enter keyword: ");
        string keyword = Console.ReadLine() ?? "";

        // LINQ Where + OrderBy
        var result = products
            .Where(p => p.Name.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(p => p.Price);

        Console.WriteLine("\n Search Result:");
        foreach (var p in result)
            Console.WriteLine(p);

        // LINQ GroupBy
        var groups = products.GroupBy(p => p.Category);
        Console.WriteLine("\n Products by Category:");
        foreach (var g in groups)
        {
            Console.WriteLine($"{g.Key}: {g.Count()} items");
        }
    }
}
