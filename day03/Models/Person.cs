namespace day03.Models;

public class Person
{
    // Encapsulation
    private string _name;
    public int Age { get; set; }

    public string Name
    {
        get => _name;
        set => _name = string.IsNullOrWhiteSpace(value) ? "Unknown" : value;
    }

    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }

    public virtual void DisplayInfo()
    {
        Console.WriteLine($"👤 {Name}, Age: {Age}");
    }
}
