using Pos.Domain;
using Xunit;

namespace Pos.Tests;

public class GradebookTests
{
    [Fact]
    public void CannotGiveScoreAboveMaximum()
    {
        var book = new Gradebook();
        var student = book.AddStudent("Иван");
        var criterion = book.AddCriterion("Семинар", 10, 1);

        Assert.Throws<ArgumentException>(() => book.SetEvaluation(student.Id, criterion.Id, 11));
        Assert.Empty(book.Evaluations);
    }

    [Fact]
    public void CannotGiveNegativeScore()
    {
        var book = new Gradebook();
        var student = book.AddStudent("Иван");
        var criterion = book.AddCriterion("Семинар", 10, 1);

        Assert.Throws<ArgumentException>(() => book.SetEvaluation(student.Id, criterion.Id, -1));
    }

    [Fact]
    public void CannotEvaluateUnknownCriterion()
    {
        var book = new Gradebook();
        var student = book.AddStudent("Иван");

        Assert.Throws<InvalidOperationException>(() => book.SetEvaluation(student.Id, 999, 5));
    }

    [Fact]
    public void CannotEvaluateUnknownStudent()
    {
        var book = new Gradebook();
        var criterion = book.AddCriterion("Семинар", 10, 1);

        Assert.Throws<InvalidOperationException>(() => book.SetEvaluation(999, criterion.Id, 5));
    }

    [Fact]
    public void TotalUsesWeights()
    {
        var book = new Gradebook();
        var student = book.AddStudent("Иван");
        var seminar = book.AddCriterion("Семинар", 10, 3);
        var exam = book.AddCriterion("Экзамен", 20, 1);
        book.SetEvaluation(student.Id, seminar.Id, 8);
        book.SetEvaluation(student.Id, exam.Id, 10);

        decimal result = new ScoreCalculator().Calculate(book, student.Id, new AllCriteriaStrategy());
        Assert.Equal(7.25m, result);
    }

    [Fact]
    public void StrategiesHandleMissingGradesDifferently()
    {
        var book = new Gradebook();
        var student = book.AddStudent("Иван");
        var seminar = book.AddCriterion("Семинар", 10, 3);
        book.AddCriterion("Экзамен", 20, 1);
        book.SetEvaluation(student.Id, seminar.Id, 8);
        var calculator = new ScoreCalculator();

        Assert.Equal(6m, calculator.Calculate(book, student.Id, new AllCriteriaStrategy()));
        Assert.Equal(8m, calculator.Calculate(book, student.Id, new GradedOnlyStrategy()));
    }

    [Fact]
    public void ReplacingGradeDoesNotCreateDuplicate()
    {
        var book = new Gradebook();
        var student = book.AddStudent("Иван");
        var criterion = book.AddCriterion("Семинар", 10, 1);
        book.SetEvaluation(student.Id, criterion.Id, 4);
        book.SetEvaluation(student.Id, criterion.Id, 7);

        Assert.Single(book.Evaluations);
        Assert.Equal(7m, book.Evaluations[0].Score.Value);
    }

    [Fact]
    public void CannotReduceMaximumBelowExistingGrade()
    {
        var book = new Gradebook();
        var student = book.AddStudent("Иван");
        var criterion = book.AddCriterion("Семинар", 10, 1);
        book.SetEvaluation(student.Id, criterion.Id, 9);

        Assert.Throws<ArgumentException>(() => book.ChangeCriterion(criterion.Id, "Семинар", 8, 1));
        Assert.Equal(10m, criterion.MaxScore);
    }

    [Fact]
    public void RemovingCriterionRemovesGrades()
    {
        var book = new Gradebook();
        var student = book.AddStudent("Иван");
        var criterion = book.AddCriterion("Семинар", 10, 1);
        book.SetEvaluation(student.Id, criterion.Id, 8);
        book.RemoveCriterion(criterion.Id);

        Assert.Empty(book.Evaluations);
    }

    [Fact]
    public void EmptyNameIsRejected()
    {
        var book = new Gradebook();
        Assert.Throws<ArgumentException>(() => book.AddStudent("  "));
        Assert.Throws<ArgumentException>(() => book.AddCriterion("", 10, 1));
    }
}
