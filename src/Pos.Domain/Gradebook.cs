namespace Pos.Domain;

public class Gradebook
{
    private readonly List<Student> _students = new();
    private readonly List<Criterion> _criteria = new();
    private readonly List<Evaluation> _evaluations = new();

    public IReadOnlyList<Student> Students => _students.AsReadOnly();
    public IReadOnlyList<Criterion> Criteria => _criteria.AsReadOnly();
    public IReadOnlyList<Evaluation> Evaluations => _evaluations.AsReadOnly();

    public Student AddStudent(string name)
    {
        int id = _students.Count == 0 ? 1 : _students.Max(s => s.Id) + 1;
        var student = new Student(id, name);
        _students.Add(student);
        return student;
    }

    public void RenameStudent(int id, string name)
    {
        GetStudent(id).Rename(name);
    }

    public void RemoveStudent(int id)
    {
        var student = GetStudent(id);
        _students.Remove(student);
        _evaluations.RemoveAll(e => e.StudentId == id);
    }

    public Criterion AddCriterion(string name, decimal maxScore, decimal weight)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название не может быть пустым.");
        if (_criteria.Any(c => c.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Такой критерий уже есть.");

        int id = _criteria.Count == 0 ? 1 : _criteria.Max(c => c.Id) + 1;
        var criterion = new Criterion(id, name, maxScore, weight);
        _criteria.Add(criterion);
        return criterion;
    }

    public void ChangeCriterion(int id, string name, decimal maxScore, decimal weight)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название не может быть пустым.");
        var criterion = GetCriterion(id);
        if (_criteria.Any(c => c.Id != id &&
            c.Name.Equals(name.Trim(), StringComparison.OrdinalIgnoreCase)))
            throw new ArgumentException("Такой критерий уже есть.");
        if (_evaluations.Any(e => e.CriterionId == id && e.Score.Value > maxScore))
            throw new ArgumentException("Есть оценки выше нового максимума.");
        criterion.Change(name, maxScore, weight);
    }

    public void RemoveCriterion(int id)
    {
        var criterion = GetCriterion(id);
        _criteria.Remove(criterion);
        _evaluations.RemoveAll(e => e.CriterionId == id);
    }

    public void SetEvaluation(int studentId, int criterionId, decimal value)
    {
        GetStudent(studentId);
        var criterion = GetCriterion(criterionId);
        var evaluation = EvaluationFactory.Create(studentId, criterion, value);
        _evaluations.RemoveAll(e => e.StudentId == studentId && e.CriterionId == criterionId);
        _evaluations.Add(evaluation);
    }

    public void RemoveEvaluation(int studentId, int criterionId)
    {
        GetStudent(studentId);
        GetCriterion(criterionId);
        int count = _evaluations.RemoveAll(e => e.StudentId == studentId && e.CriterionId == criterionId);
        if (count == 0)
            throw new InvalidOperationException("Оценка не найдена.");
    }

    public Student GetStudent(int id)
    {
        return _students.FirstOrDefault(s => s.Id == id)
            ?? throw new InvalidOperationException("Студент не найден.");
    }

    public Criterion GetCriterion(int id)
    {
        return _criteria.FirstOrDefault(c => c.Id == id)
            ?? throw new InvalidOperationException("Критерий не найден.");
    }

    public Evaluation? FindEvaluation(int studentId, int criterionId)
    {
        return _evaluations.FirstOrDefault(e => e.StudentId == studentId && e.CriterionId == criterionId);
    }

    public static Gradebook Restore(List<Student> students, List<Criterion> criteria,
        List<Evaluation> evaluations)
    {
        var book = new Gradebook();

        foreach (var student in students)
        {
            if (book._students.Any(s => s.Id == student.Id))
                throw new InvalidOperationException("Повторяется номер студента в файле.");
            book._students.Add(student);
        }

        foreach (var criterion in criteria)
        {
            if (book._criteria.Any(c => c.Id == criterion.Id ||
                c.Name.Equals(criterion.Name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Повторяется критерий в файле.");
            book._criteria.Add(criterion);
        }

        foreach (var evaluation in evaluations)
        {
            book.GetStudent(evaluation.StudentId);
            var criterion = book.GetCriterion(evaluation.CriterionId);
            if (evaluation.Score.Value > criterion.MaxScore ||
                book.FindEvaluation(evaluation.StudentId, evaluation.CriterionId) != null)
                throw new InvalidOperationException("Некорректные оценки в файле.");
            book._evaluations.Add(evaluation);
        }

        return book;
    }
}
