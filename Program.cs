using System.Globalization;
using System.Text.Json;

class Program
{
    private static List<Task> tasks = new List<Task>();
    private static string dataFile = "tasks.json";
    private static int nextId = 1;

    static void Main(string[] args)
    {
        InitializeSystem();
        MainMenu();
    }

    private static void InitializeSystem()
    {
        LoadTasks();
        nextId = tasks.Count > 0 ? tasks.Max(t => t.Id) + 1 : 1;
    }

    private static void MainMenu()
    {
        while (true)
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("🚀 Task Manager");
            Console.ResetColor();
            Console.WriteLine("1. 📋 List Tasks");
            Console.WriteLine("2. 📝 Add Task");
            Console.WriteLine("3. ✏️ Edit Task");
            Console.WriteLine("4. 🗑️ Delete Task");
            Console.WriteLine("5. 🔍 Search Tasks");
            Console.WriteLine("6. 💾 Save and Exit");
            Console.Write("\nEnter your choice: ");

            switch (Console.ReadLine())
            {
                case "1": ListTasks(); break;
                case "2": AddTask(); break;
                case "3": EditTask(); break;
                case "4": DeleteTask(); break;
                case "5": SearchTasks(); break;
                case "6": SaveAndExit(); break;
                default: ShowError("Invalid choice!"); break;
            }
        }
    }

    private static void ListTasks()
    {
        Console.Clear();
        Console.WriteLine("📋 Task List\n");

        if (!tasks.Any())
        {
            ShowWarning("No tasks found.");
            return;
        }

        foreach (var task in tasks.OrderBy(t => t.DueDate))
        {
            Console.ForegroundColor = task.IsComplete ? ConsoleColor.Green : ConsoleColor.Yellow;
            Console.WriteLine($"ID: {task.Id}");
            Console.WriteLine($"Title: {task.Title}");
            Console.WriteLine($"Description: {task.Description}");
            Console.WriteLine($"Due: {task.DueDate:dd/MM/yyyy}");
            Console.WriteLine($"Status: {(task.IsComplete ? "✅ Complete" : "⏳ Pending")}");
            Console.WriteLine($"Created: {task.CreatedAt:g}");

            if (task.History.Any())
            {
                Console.WriteLine("\nHistory:");
                foreach (var entry in task.History)
                {
                    Console.WriteLine($" ↳ {entry}");
                }
            }
            Console.WriteLine(new string('─', 40));
        }
        Console.ResetColor();
        WaitForInput();
    }

    private static void AddTask()
    {
        Console.Clear();
        Console.WriteLine("📝 New Task\n");

        var task = new Task { Id = nextId++ };

        task.Title = GetValidInput("Title: ", "Title cannot be empty!");
        task.Description = GetValidInput("Description: ", "Description cannot be empty!");
        task.DueDate = GetValidDate() ?? DateTime.Now;

        task.History.Add($"{DateTime.Now:g} - Task created");
        tasks.Add(task);
        ShowSuccess("Task added successfully!");
    }

    private static void EditTask()
    {
        Console.Clear();
        Console.WriteLine("✏️ Edit Task\n");

        var task = FindTaskById();
        if (task == null) return;

        Console.WriteLine("Leave field blank to keep current value\n");

        // Edit Title
        var newTitle = GetValidInput($"Current Title: {task.Title}\nNew Title: ", "Title cannot be empty!", true);
        if (!string.IsNullOrEmpty(newTitle))
        {
            task.History.Add($"{DateTime.Now:g} - Title changed from '{task.Title}' to '{newTitle}'");
            task.Title = newTitle;
        }

        // Edit Description
        var newDesc = GetValidInput($"Current Description: {task.Description}\nNew Description: ", "", true);
        if (!string.IsNullOrEmpty(newDesc))
        {
            task.History.Add($"{DateTime.Now:g} - Description updated");
            task.Description = newDesc;
        }

        // Edit Due Date
        var newDate = GetValidDate(true);
        if (newDate.HasValue)
        {
            task.History.Add($"{DateTime.Now:g} - Due date changed from {task.DueDate:dd/MM/yyyy} to {newDate:dd/MM/yyyy}");
            task.DueDate = newDate.Value;
        }

        // Toggle Complete Status
        Console.Write($"Current Status: {(task.IsComplete ? "Complete" : "Pending")}\nToggle status? (y/n): ");
        if (Console.ReadLine()?.ToLower() == "y")
        {
            task.IsComplete = !task.IsComplete;
            task.History.Add($"{DateTime.Now:g} - Status changed to {(task.IsComplete ? "Complete" : "Pending")}");
        }

        ShowSuccess("Task updated successfully!");
    }

    private static void DeleteTask()
    {
        Console.Clear();
        Console.WriteLine("🗑️ Delete Task\n");

        var task = FindTaskById();
        if (task == null) return;

        Console.Write($"Are you sure you want to delete '{task.Title}'? (y/n): ");
        if (Console.ReadLine()?.ToLower() != "y") return;

        tasks.Remove(task);
        ShowSuccess("Task deleted successfully!");
    }

    private static void SearchTasks()
    {
        Console.Clear();
        Console.WriteLine("🔍 Search Tasks\n");

        Console.Write("Search term: ");
        var term = Console.ReadLine()?.ToLower();

        var results = tasks.Where(t =>
            t.Title.ToLower().Contains(term) ||
            t.Description.ToLower().Contains(term))
            .ToList();

        Console.WriteLine($"\nFound {results.Count} matches:");
        foreach (var task in results)
        {
            Console.WriteLine($" [{task.Id}] {task.Title} - Due: {task.DueDate:dd/MM/yyyy}");
        }
        WaitForInput();
    }

    #region Helpers
    private static Task FindTaskById()
    {
        Console.Write("Enter Task ID: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            ShowError("Invalid ID format!");
            return null;
        }

        var task = tasks.FirstOrDefault(t => t.Id == id);
        if (task == null) ShowError("Task not found!");
        return task;
    }

    private static DateTime? GetValidDate(bool allowEmpty = false)
    {
        while (true)
        {
            Console.Write("Due Date (dd/MM/yyyy): ");
            var input = Console.ReadLine();

            if (allowEmpty && string.IsNullOrEmpty(input))
                return null;

            if (DateTime.TryParseExact(input, "dd/MM/yyyy",
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
            {
                return date;
            }
            ShowError("Invalid date format! Use dd/MM/yyyy");
        }
    }

    private static string GetValidInput(string prompt, string errorMessage, bool allowEmpty = false)
    {
        while (true)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (!allowEmpty && string.IsNullOrWhiteSpace(input))
                ShowError(errorMessage);
            else
                return input.Trim();
        }
    }

    private static void SaveAndExit()
    {
        SaveTasks();
        Environment.Exit(0);
    }

    private static void LoadTasks()
    {
        try
        {
            if (File.Exists(dataFile))
            {
                var json = File.ReadAllText(dataFile);
                tasks = JsonSerializer.Deserialize<List<Task>>(json) ?? new List<Task>();
            }
        }
        catch (Exception ex)
        {
            ShowError($"Load Error: {ex.Message}");
        }
    }

    private static void SaveTasks()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(dataFile, JsonSerializer.Serialize(tasks, options));
        }
        catch (Exception ex)
        {
            ShowError($"Save Error: {ex.Message}");
        }
    }

    private static void ShowSuccess(string message)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n✓ {message}");
        Console.ResetColor();
        WaitForInput();
    }

    private static void ShowError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"\n⚠️ {message}");
        Console.ResetColor();
        WaitForInput();
    }

    private static void ShowWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n⚠️ {message}");
        Console.ResetColor();
        WaitForInput();
    }

    private static void WaitForInput()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }
    #endregion
}

public class Task
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    public bool IsComplete { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<string> History { get; set; } = new List<string>();
}