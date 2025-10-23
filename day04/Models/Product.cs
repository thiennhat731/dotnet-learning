namespace day04.Models;

public class Product
{
    public string Name { get; set; }
    public double Price { get; set; }
    public string Category { get; set; }

    public Product(string name, double price, string category)
    {
        Name = name;
        Price = price;
        Category = category;
    }

    public override string ToString() => $"{Name} - ${Price} ({Category})";
}
