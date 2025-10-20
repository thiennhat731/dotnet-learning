Console.WriteLine("Enter a:");
double a = double.Parse(Console.ReadLine() ?? "0");
Console.WriteLine("Enter b:");
double b = double.Parse(Console.ReadLine() ?? "0");
Console.WriteLine("Enter operation (+, -, *, /):");
string op = Console.ReadLine() ?? "+";
double result = op switch
{
    "+" => a + b,
    "-" => a - b,
    "*" => a * b,
    "/" => b != 0 ? a / b : double.NaN,
    _ => double.NaN
};
Console.WriteLine($"Result: {result}");