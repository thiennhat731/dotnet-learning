namespace day04.Examples;

// Delegate định nghĩa dạng hàm có tham số string
delegate void Notify(string message);

public class DelegateExample
{
    public static void Run()
    {
        Notify notify = ShowMessage;
        notify("Hello from delegate!");

        // Lambda Expression
        Notify notifyLambda = msg => Console.WriteLine($"[Lambda] {msg}");
        notifyLambda("Hello from lambda!");
    }

    static void ShowMessage(string message)
    {
        Console.WriteLine($"[Delegate] {message}");
    }
}
