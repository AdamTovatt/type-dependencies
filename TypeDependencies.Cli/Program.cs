using System.CommandLine;
using TypeDependencies.Cli.Commands;
using TypeDependencies.Core.Analysis;
using TypeDependencies.Core.Export;
using TypeDependencies.Core.State;

namespace TypeDependencies.Cli
{
    internal class Program
    {
        static int Main(string[] args)
        {
            // Set up dependency injection
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy defaultExportStrategy = new DotExportStrategy();
            ICurrentSessionFinder sessionFinder = new CurrentSessionFinder();

            // Create root command
            RootCommand rootCommand = new RootCommand("Type dependency analyzer for C# assemblies");

            // Add subcommands
            rootCommand.Subcommands.Add(InitCommand.Create(stateManager));
            rootCommand.Subcommands.Add(AddCommand.Create(stateManager, sessionFinder));
            rootCommand.Subcommands.Add(GenerateCommand.Create(stateManager, typeAnalyzer, sessionFinder));
            rootCommand.Subcommands.Add(ExportCommand.Create(stateManager, defaultExportStrategy, sessionFinder));

            return rootCommand.Parse(args).Invoke();
        }
    }
}

