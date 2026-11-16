using Pos.Domain;
using Pos.Infrastructure;
using Pos.Application;
using Xunit;

namespace Pos.Tests;

public class FileTests
{
    [Fact]
    public void JsonCanSaveAndLoadGrades()
    {
        string path = Path.Combine(Path.GetTempPath(), "pos-" + Guid.NewGuid() + ".json");
        try
        {
            var repo = new JsonGradebookRepository(path);
            var book = new Gradebook();
            var student = book.AddStudent("Анна");
            var criterion = book.AddCriterion("ДЗ", 10, 2);
            book.SetEvaluation(student.Id, criterion.Id, 7);
            repo.Save(book);

            var loaded = repo.Load();
            Assert.Single(loaded.Students);
            Assert.Single(loaded.Criteria);
            Assert.Equal(7m, loaded.Evaluations[0].Score.Value);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    [Fact]
    public void CsvContainsStudentAndScore()
    {
        string path = Path.Combine(Path.GetTempPath(), "pos-" + Guid.NewGuid() + ".csv");
        try
        {
            var exporter = new CsvResultExporter();
            exporter.Export(path, new List<StudentResult> { new(1, "Иван", 7.25m) });
            string text = File.ReadAllText(path);
            Assert.Contains("Иван", text);
            Assert.Contains("7.25", text);
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
