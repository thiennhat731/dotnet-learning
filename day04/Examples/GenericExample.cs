namespace day04.Examples;

public class Box<T>
{
    public T Value { get; set; }
    public Box(T value) => Value = value;
}

public class GenericExample
{
    public static void Run()
    {
        var intBox = new Box<int>(100);
        var strBox = new Box<string>("Hello Generics");

        Console.WriteLine($"IntBox: {intBox.Value}");
        Console.WriteLine($"StrBox: {strBox.Value}");

        PrintList(new List<string> { "A", "B", "C" });
    }

    static void PrintList<T>(List<T> list)
    {
        Console.WriteLine("\nList contents:");
        foreach (var item in list)
            Console.WriteLine($"- {item}");
    }
}
