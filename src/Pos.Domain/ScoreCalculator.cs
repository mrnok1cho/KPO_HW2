namespace Pos.Domain;

public enum ScoringMode
{
    AllCriteria,
    GradedOnly
}

public interface IScoringStrategy
{
    ScoringMode Mode { get; }
    bool Include(bool hasEvaluation);
}

public class AllCriteriaStrategy : IScoringStrategy
{
    public ScoringMode Mode => ScoringMode.AllCriteria;

    public bool Include(bool hasEvaluation)
    {
        return true;
    }
}

public class GradedOnlyStrategy : IScoringStrategy
{
    public ScoringMode Mode => ScoringMode.GradedOnly;

    public bool Include(bool hasEvaluation)
    {
        return hasEvaluation;
    }
}

public class ScoreCalculator
{
    public decimal Calculate(Gradebook book, int studentId, IScoringStrategy strategy)
    {
        book.GetStudent(studentId);
        decimal total = 0;
        decimal weights = 0;

        foreach (var criterion in book.Criteria)
        {
            var evaluation = book.FindEvaluation(studentId, criterion.Id);
            if (!strategy.Include(evaluation != null))
                continue;

            weights += criterion.Weight;
            if (evaluation != null)
                total += evaluation.Score.Value / criterion.MaxScore * criterion.Weight;
        }

        if (weights == 0)
            return 0;

        return Math.Round(10 * total / weights, 2, MidpointRounding.AwayFromZero);
    }
}
