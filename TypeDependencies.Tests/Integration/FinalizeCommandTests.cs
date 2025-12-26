using FluentAssertions;
using System.CommandLine;
using TypeDependencies.Cli.Commands;
using TypeDependencies.Core.Analysis;
using TypeDependencies.Core.Export;
using TypeDependencies.Core.State;

namespace TypeDependencies.Tests.Integration
{
    public class FinalizeCommandTests
    {
        [Fact]
        public void FinalizeCommand_ShouldFailWhenNoSessionExists()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy defaultExportStrategy = new DotExportStrategy();
            Command command = FinalizeCommand.Create(stateManager, typeAnalyzer, defaultExportStrategy);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "finalize" }).Invoke();

            exitCode.Should().Be(1);
        }

        [Fact]
        public void FinalizeCommand_ShouldFailWhenNoDllsAdded()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy defaultExportStrategy = new DotExportStrategy();
            stateManager.InitializeSession();
            Command command = FinalizeCommand.Create(stateManager, typeAnalyzer, defaultExportStrategy);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "finalize" }).Invoke();

            exitCode.Should().Be(1);
        }
    }
}

