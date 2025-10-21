// Day 02 - Functions, Try-Catch, CRUD List App

namespace Day02FunctionsCrud
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("===== Day 02: Functions & CRUD List =====");

            try
            {
                RunMenu();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }

        // Hàm hiển thị menu
        static void RunMenu()
        {
            List<string> students = new();

            while (true)
            {
                Console.WriteLine("\n1. Add Student");
                Console.WriteLine("2. Remove Student");
                Console.WriteLine("3. Show All");
                Console.WriteLine("0. Exit");
                Console.Write("👉 Choose: ");

                int choice = int.Parse(Console.ReadLine() ?? "0");

                switch (choice)
                {
                    case 0:
                        Console.WriteLine("Bye 👋");
                        return;
                    case 1:
                        AddStudent(students);
                        break;
                    case 2:
                        RemoveStudent(students);
                        break;
                    case 3:
                        ShowStudents(students);
                        break;
                    default:
                        Console.WriteLine("Invalid choice!");
                        break;
                }
            }
        }

        // Hàm thêm sinh viên
        static void AddStudent(List<string> list)
        {
            Console.Write("Enter student name: ");
            string name = Console.ReadLine() ?? "";
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty!");
            list.Add(name);
            Console.WriteLine($"Added: {name}");
        }

        // Hàm xóa sinh viên
        static void RemoveStudent(List<string> list)
        {
            Console.Write("Enter name to remove: ");
            string name = Console.ReadLine() ?? "";
            if (list.Remove(name))
                Console.WriteLine($"🗑️ Removed: {name}");
            else
                Console.WriteLine($"❌ Not found: {name}");
        }

        // Hàm hiển thị danh sách
        static void ShowStudents(List<string> list)
        {
            if (list.Count == 0)
            {
                Console.WriteLine("📭 No students yet!");
                return;
            }

            Console.WriteLine("\n📋 Student list:");
            foreach (var s in list)
            {
                Console.WriteLine($"- {s}");
            }
        }
    }
}
