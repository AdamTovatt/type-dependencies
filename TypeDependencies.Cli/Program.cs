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

            // Create root command
            RootCommand rootCommand = new RootCommand("Type dependency analyzer for C# assemblies");

            // Add subcommands
            rootCommand.Subcommands.Add(InitCommand.Create(stateManager));
            rootCommand.Subcommands.Add(AddCommand.Create(stateManager));
            rootCommand.Subcommands.Add(FinalizeCommand.Create(stateManager, typeAnalyzer, defaultExportStrategy));

            return rootCommand.Parse(args).Invoke();
        }
    }
}

