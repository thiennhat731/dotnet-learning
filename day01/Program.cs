// Day 01 - Basic Syntax, Variables, Conditions, Loops

// Variables
string name = "Nhat";
int age = 21;
double height = 1.75;
bool isStudent = true;

Console.WriteLine($"Hello, {name}!");
Console.WriteLine($"Age: {age}, Height: {height}m, Student: {isStudent}");

// Condition
int score = 80;
if (score >= 90) Console.WriteLine("Grade: A");
else if (score >= 75) Console.WriteLine("Grade: B");
else Console.WriteLine("Grade: C");

// Loop
Console.WriteLine("\nCounting 1 → 5:");
for (int i = 1; i <= 5; i++)
{
    Console.WriteLine(i);
}

// While loop
int j = 0;
while (j < 3)
{
    Console.WriteLine($"While loop {j}");
    j++;
}