using System;

Console.WriteLine("===== Mini App: Calculator with Functions =====");

static double Calculate(double a, double b, char op)
{
    return op switch
    {
        '+' => a + b,
        '-' => a - b,
        '*' => a * b,
        '/' => b != 0 ? a / b : throw new DivideByZeroException("Cannot divide by zero!"),
        _ => throw new ArgumentException("Invalid operator!")
    };
}

try
{
    Console.Write("Enter a: ");
    double a = double.Parse(Console.ReadLine() ?? "0");

    Console.Write("Enter b: ");
    double b = double.Parse(Console.ReadLine() ?? "0");

    Console.Write("Operator (+ - * /): ");
    char op = char.Parse(Console.ReadLine() ?? "+");

    Console.WriteLine($"Result = {Calculate(a, b, op)}");
}
catch (FormatException)
{
    Console.WriteLine("Invalid number format!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
