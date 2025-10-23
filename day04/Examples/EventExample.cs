namespace day04.Examples;

public class Button
{
    public event Action<string>? OnClick;

    public void Click()
    {
        OnClick?.Invoke("🖱️ Button clicked!");
    }
}

public class EventExample
{
    public static void Run()
    {
        var btn = new Button();
        btn.OnClick += message => Console.WriteLine(message);
        btn.Click();
    }
}
