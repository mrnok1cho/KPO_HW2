using Microsoft.Extensions.DependencyInjection;
using Pos.Application;
using Pos.Domain;
using Pos.Infrastructure;
using Pos.ConsoleApp;

var services = new ServiceCollection();
services.AddSingleton<IGradebookRepository>(_ =>
    new JsonGradebookRepository(Path.Combine("data", "gradebook.json")));
services.AddSingleton<IResultExporter, CsvResultExporter>();
services.AddSingleton<IScoringStrategy, AllCriteriaStrategy>();
services.AddSingleton<IScoringStrategy, GradedOnlyStrategy>();
services.AddSingleton<ScoreCalculator>();
services.AddSingleton<PosFacade>();
services.AddSingleton<ConsoleMenu>();

using var provider = services.BuildServiceProvider();
provider.GetRequiredService<ConsoleMenu>().Run();
