using System.Globalization;
using System.Text;
using Pos.Application;

namespace Pos.Infrastructure;

public class CsvResultExporter : IResultExporter
{
    public string Export(string path, List<StudentResult> results)
    {
        string fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var lines = new List<string> { "Номер;Студент;Итог из 10" };

        foreach (var result in results)
        {
            string name = "\"" + result.Name.Replace("\"", "\"\"") + "\"";
            string total = result.Total.ToString(CultureInfo.InvariantCulture);
            lines.Add($"{result.Id};{name};{total}");
        }

        File.WriteAllLines(fullPath, lines, new UTF8Encoding(true));
        return fullPath;
    }
}
