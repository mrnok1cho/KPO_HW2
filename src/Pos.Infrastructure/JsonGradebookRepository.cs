using System.Text.Json;
using Pos.Application;
using Pos.Domain;

namespace Pos.Infrastructure;

public class JsonGradebookRepository : IGradebookRepository
{
    private readonly string _path;

    public JsonGradebookRepository(string path)
    {
        _path = path;
    }

    public Gradebook Load()
    {
        if (!File.Exists(_path))
            return new Gradebook();

        var json = File.ReadAllText(_path);
        var data = JsonSerializer.Deserialize<GradebookData>(json)
            ?? throw new InvalidOperationException("Файл с данными пуст.");

        var students = data.Students.Select(s => new Student(s.Id, s.Name)).ToList();
        var criteria = data.Criteria.Select(c => new Criterion(c.Id, c.Name, c.Max, c.Weight)).ToList();
        var evaluations = new List<Evaluation>();

        foreach (var item in data.Evaluations)
        {
            var criterion = criteria.FirstOrDefault(c => c.Id == item.CriterionId)
                ?? throw new InvalidOperationException("В файле неизвестный критерий.");
            evaluations.Add(EvaluationFactory.Create(item.StudentId, criterion, item.Score));
        }

        return Gradebook.Restore(students, criteria, evaluations);
    }

    public void Save(Gradebook book)
    {
        var data = new GradebookData();
        foreach (var student in book.Students)
            data.Students.Add(new StudentData { Id = student.Id, Name = student.Name });
        foreach (var criterion in book.Criteria)
            data.Criteria.Add(new CriterionData
            {
                Id = criterion.Id, Name = criterion.Name,
                Max = criterion.MaxScore, Weight = criterion.Weight
            });
        foreach (var evaluation in book.Evaluations)
            data.Evaluations.Add(new EvaluationData
            {
                StudentId = evaluation.StudentId,
                CriterionId = evaluation.CriterionId,
                Score = evaluation.Score.Value
            });

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_path))!);
        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_path, json);
    }
}

public class GradebookData
{
    public List<StudentData> Students { get; set; } = new();
    public List<CriterionData> Criteria { get; set; } = new();
    public List<EvaluationData> Evaluations { get; set; } = new();
}

public class StudentData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class CriterionData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Max { get; set; }
    public decimal Weight { get; set; }
}

public class EvaluationData
{
    public int StudentId { get; set; }
    public int CriterionId { get; set; }
    public decimal Score { get; set; }
}
