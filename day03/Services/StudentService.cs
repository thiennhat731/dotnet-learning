using day03.Models;
namespace day03.Services;

public class StudentService
{
    private readonly List<Student> _students = new();

    public void AddStudent(Student s)
    {
        _students.Add(s);
        Console.WriteLine($"Added {s.Name}");
    }

    public void RemoveStudent(string id)
    {
        var s = _students.FirstOrDefault(x => x.StudentId == id);
        if (s != null)
        {
            _students.Remove(s);
            Console.WriteLine($"Removed {s.Name}");
        }
        else Console.WriteLine("Not found!");
    }

    public void ShowAll()
    {
        if (_students.Count == 0)
        {
            Console.WriteLine("No students found!");
            return;
        }
        Console.WriteLine("\n=== STUDENT LIST ===");
        _students.ForEach(s => s.DisplayInfo());
    }
}
