namespace day03.Models;

public class Student : Person
{
    public string StudentId { get; set; }
    public double GPA { get; set; }

    public Student(string id, string name, int age, double gpa)
        : base(name, age)
    {
        StudentId = id;
        GPA = gpa;
    }

    // Polymorphism (Override)
    public override void DisplayInfo()
    {
        Console.WriteLine($"ID: {StudentId} | {Name} ({Age} yrs) | GPA: {GPA}");
    }
}
