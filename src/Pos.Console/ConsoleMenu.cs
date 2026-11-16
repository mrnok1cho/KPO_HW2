using System.Globalization;
using System.Text;
using Pos.Application;

namespace Pos.ConsoleApp;

public class ConsoleMenu
{
    private readonly PosFacade _pos;

    public ConsoleMenu(PosFacade pos)
    {
        _pos = pos;
    }

    public void Run()
    {
        Console.OutputEncoding = Encoding.UTF8;

        while (true)
        {
            PrintMenu();
            string? command = Console.ReadLine();
            if (command == "0" || command == null)
                break;

            try
            {
                switch (command)
                {
                    case "1": AddStudent(); break;
                    case "2": ShowStudents(); break;
                    case "3": RenameStudent(); break;
                    case "4": RemoveStudent(); break;
                    case "5": AddCriterion(); break;
                    case "6": ShowCriteria(); break;
                    case "7": ChangeCriterion(); break;
                    case "8": RemoveCriterion(); break;
                    case "9": SetEvaluation(); break;
                    case "10": ShowEvaluations(); break;
                    case "11": RemoveEvaluation(); break;
                    case "12": ShowTotal(); break;
                    case "13": Export(); break;
                    default: Console.WriteLine("Нет такой команды."); break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
            }
        }
    }

    private static void PrintMenu()
    {
        Console.WriteLine();
        Console.WriteLine("=== ПОС ===");
        Console.WriteLine("1. Добавить студента         2. Список студентов");
        Console.WriteLine("3. Изменить имя студента     4. Удалить студента");
        Console.WriteLine("5. Добавить критерий         6. Список критериев");
        Console.WriteLine("7. Изменить критерий         8. Удалить критерий");
        Console.WriteLine("9. Поставить оценку         10. Посмотреть оценки");
        Console.WriteLine("11. Удалить оценку          12. Посчитать итог");
        Console.WriteLine("13. Экспорт в CSV            0. Выход");
        Console.Write("Команда: ");
    }

    private void AddStudent()
    {
        _pos.AddStudent(ReadText("Имя студента: "));
        Console.WriteLine("Студент добавлен.");
    }

    private void ShowStudents()
    {
        Console.WriteLine("Студенты:");
        foreach (var student in _pos.GetStudents())
            Console.WriteLine($"{student.Id}. {student.Name}");
    }

    private void RenameStudent()
    {
        ShowStudents();
        int id = ReadInt("Номер студента: ");
        _pos.RenameStudent(id, ReadText("Новое имя: "));
    }

    private void RemoveStudent()
    {
        ShowStudents();
        _pos.RemoveStudent(ReadInt("Номер студента: "));
    }

    private void AddCriterion()
    {
        string name = ReadText("Название: ");
        decimal max = ReadDecimal("Максимум: ");
        decimal weight = ReadDecimal("Вес: ");
        _pos.AddCriterion(name, max, weight);
        Console.WriteLine("Критерий добавлен.");
    }

    private void ShowCriteria()
    {
        Console.WriteLine("Критерии:");
        foreach (var criterion in _pos.GetCriteria())
            Console.WriteLine($"{criterion.Id}. {criterion.Name} (макс. {criterion.Max}, вес {criterion.Weight})");
    }

    private void ChangeCriterion()
    {
        ShowCriteria();
        int id = ReadInt("Номер критерия: ");
        string name = ReadText("Новое название: ");
        decimal max = ReadDecimal("Новый максимум: ");
        decimal weight = ReadDecimal("Новый вес: ");
        _pos.ChangeCriterion(id, name, max, weight);
    }

    private void RemoveCriterion()
    {
        ShowCriteria();
        _pos.RemoveCriterion(ReadInt("Номер критерия: "));
    }

    private void SetEvaluation()
    {
        ShowStudents();
        int studentId = ReadInt("Номер студента: ");
        ShowCriteria();
        int criterionId = ReadInt("Номер критерия: ");
        decimal score = ReadDecimal("Оценка: ");
        _pos.SetEvaluation(studentId, criterionId, score);
        Console.WriteLine("Оценка сохранена.");
    }

    private void ShowEvaluations()
    {
        ShowStudents();
        int studentId = ReadInt("Номер студента: ");
        Console.WriteLine("Оценки: " + _pos.GetStudentName(studentId));

        foreach (var evaluation in _pos.GetEvaluations(studentId))
        {
            string score = evaluation.Score?.ToString() ?? "нет";
            Console.WriteLine($"{evaluation.Name}: {score} / {evaluation.Max}");
        }
    }

    private void RemoveEvaluation()
    {
        ShowStudents();
        int studentId = ReadInt("Номер студента: ");
        ShowCriteria();
        int criterionId = ReadInt("Номер критерия: ");
        _pos.RemoveEvaluation(studentId, criterionId);
    }

    private void ShowTotal()
    {
        ShowStudents();
        int id = ReadInt("Номер студента: ");
        Console.WriteLine($"Итог: {_pos.GetTotal(id):0.##} / 10");
        Console.WriteLine($"По выставленным оценкам: {_pos.GetGradedTotal(id):0.##} / 10");
    }

    private void Export()
    {
        string path = ReadText("Имя файла (Enter = results.csv): ");
        if (path == "")
            path = "results.csv";
        Console.WriteLine("Сохранено: " + _pos.Export(path));
    }

    private static string ReadText(string message)
    {
        Console.Write(message);
        return Console.ReadLine() ?? "";
    }

    private static int ReadInt(string message)
    {
        if (int.TryParse(ReadText(message), out int value))
            return value;
        throw new ArgumentException("Введите целое число.");
    }

    private static decimal ReadDecimal(string message)
    {
        string value = ReadText(message).Replace(',', '.');
        if (decimal.TryParse(value, NumberStyles.Number,
            CultureInfo.InvariantCulture, out decimal number))
            return number;
        throw new ArgumentException("Введите число.");
    }
}
