using day03.Models;
using day03.Services;
// 1️⃣ Object & Inheritance
var s1 = new Student("S01", "Nhat", 21, 3.7);
var s2 = new Student("S02", "Linh", 22, 3.5);

// 2️⃣ Service quản lý danh sách
var service = new StudentService();
service.AddStudent(s1);
service.AddStudent(s2);
service.ShowAll();

// 3️⃣ Array
string[] names = { "A", "B", "C" };
Console.WriteLine("\nArray:");
foreach (var n in names)
    Console.WriteLine(n);

// 4️⃣ List
List<int> scores = new() { 8, 9, 10 };
Console.WriteLine("\nList of scores:");
scores.ForEach(x => Console.WriteLine(x));

// 5️⃣ Dictionary
Dictionary<string, double> gpas = new()
{
    { "S01", 3.7 },
    { "S02", 3.5 }
};
Console.WriteLine("\nDictionary of GPA:");
foreach (var kv in gpas)
    Console.WriteLine($"{kv.Key}: {kv.Value}");
