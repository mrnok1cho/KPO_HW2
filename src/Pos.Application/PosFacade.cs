using Pos.Domain;

namespace Pos.Application;

public class PosFacade
{
    private readonly IGradebookRepository _repository;
    private readonly IResultExporter _exporter;
    private readonly ScoreCalculator _calculator;
    private readonly List<IScoringStrategy> _strategies;
    private readonly Gradebook _book;

    public PosFacade(IGradebookRepository repository, IResultExporter exporter,
        ScoreCalculator calculator, IEnumerable<IScoringStrategy> strategies)
    {
        _repository = repository;
        _exporter = exporter;
        _calculator = calculator;
        _strategies = strategies.ToList();
        _book = repository.Load();
    }

    public List<(int Id, string Name)> GetStudents()
    {
        return _book.Students.Select(s => (s.Id, s.Name)).ToList();
    }

    public List<(int Id, string Name, decimal Max, decimal Weight)> GetCriteria()
    {
        return _book.Criteria
            .Select(c => (c.Id, c.Name, c.MaxScore, c.Weight)).ToList();
    }

    public string GetStudentName(int studentId)
    {
        return _book.GetStudent(studentId).Name;
    }

    public List<(string Name, decimal Max, decimal? Score)> GetEvaluations(int studentId)
    {
        _book.GetStudent(studentId);
        var result = new List<(string Name, decimal Max, decimal? Score)>();

        foreach (var criterion in _book.Criteria)
        {
            var evaluation = _book.FindEvaluation(studentId, criterion.Id);
            result.Add((criterion.Name, criterion.MaxScore, evaluation?.Score.Value));
        }

        return result;
    }

    public void AddStudent(string name)
    {
        _book.AddStudent(name);
        _repository.Save(_book);
    }

    public void RenameStudent(int id, string name)
    {
        _book.RenameStudent(id, name);
        _repository.Save(_book);
    }

    public void RemoveStudent(int id)
    {
        _book.RemoveStudent(id);
        _repository.Save(_book);
    }

    public void AddCriterion(string name, decimal max, decimal weight)
    {
        _book.AddCriterion(name, max, weight);
        _repository.Save(_book);
    }

    public void ChangeCriterion(int id, string name, decimal max, decimal weight)
    {
        _book.ChangeCriterion(id, name, max, weight);
        _repository.Save(_book);
    }

    public void RemoveCriterion(int id)
    {
        _book.RemoveCriterion(id);
        _repository.Save(_book);
    }

    public void SetEvaluation(int studentId, int criterionId, decimal score)
    {
        _book.SetEvaluation(studentId, criterionId, score);
        _repository.Save(_book);
    }

    public void RemoveEvaluation(int studentId, int criterionId)
    {
        _book.RemoveEvaluation(studentId, criterionId);
        _repository.Save(_book);
    }

    public decimal GetTotal(int studentId)
    {
        return CalculateTotal(studentId, ScoringMode.AllCriteria);
    }

    public decimal GetGradedTotal(int studentId)
    {
        return CalculateTotal(studentId, ScoringMode.GradedOnly);
    }

    private decimal CalculateTotal(int studentId, ScoringMode mode)
    {
        var strategy = _strategies.First(s => s.Mode == mode);
        return _calculator.Calculate(_book, studentId, strategy);
    }

    public string Export(string path)
    {
        var results = new List<StudentResult>();
        foreach (var student in _book.Students)
            results.Add(new StudentResult(student.Id, student.Name, GetTotal(student.Id)));
        return _exporter.Export(path, results);
    }
}
