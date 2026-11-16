namespace Pos.Domain;

public class Student
{
    public int Id { get; }
    public string Name { get; private set; } = string.Empty;

    public Student(int id, string name)
    {
        if (id <= 0)
            throw new ArgumentException("Неверный номер студента.");
        Id = id;
        Rename(name);
    }

    internal void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя не может быть пустым.");
        Name = name.Trim();
    }
}

public class Criterion
{
    public int Id { get; }
    public string Name { get; private set; } = string.Empty;
    public decimal MaxScore { get; private set; }
    public decimal Weight { get; private set; }

    public Criterion(int id, string name, decimal maxScore, decimal weight)
    {
        if (id <= 0)
            throw new ArgumentException("Неверный номер критерия.");
        Id = id;
        Change(name, maxScore, weight);
    }

    internal void Change(string name, decimal maxScore, decimal weight)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Название критерия не может быть пустым.");
        if (maxScore <= 0 || weight <= 0)
            throw new ArgumentException("Максимум и вес должны быть больше нуля.");
        Name = name.Trim();
        MaxScore = maxScore;
        Weight = weight;
    }
}

public readonly struct Score
{
    public decimal Value { get; }

    public Score(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Оценка не может быть отрицательной.");
        Value = value;
    }
}

public class Evaluation
{
    public int StudentId { get; }
    public int CriterionId { get; }
    public Score Score { get; }

    internal Evaluation(int studentId, int criterionId, Score score)
    {
        StudentId = studentId;
        CriterionId = criterionId;
        Score = score;
    }
}

public static class EvaluationFactory
{
    public static Evaluation Create(int studentId, Criterion criterion, decimal value)
    {
        if (studentId <= 0)
            throw new ArgumentException("Неверный номер студента.");
        Score score = new Score(value);
        if (score.Value > criterion.MaxScore)
            throw new ArgumentException("Оценка выше максимального балла.");
        return new Evaluation(studentId, criterion.Id, score);
    }
}
