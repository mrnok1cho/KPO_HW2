using Pos.Domain;

namespace Pos.Application;

public interface IGradebookRepository
{
    Gradebook Load();
    void Save(Gradebook book);
}

public interface IResultExporter
{
    string Export(string path, List<StudentResult> results);
}

public record StudentResult(int Id, string Name, decimal Total);
