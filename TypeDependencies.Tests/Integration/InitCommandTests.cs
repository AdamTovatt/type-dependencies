using FluentAssertions;
using System.CommandLine;
using TypeDependencies.Cli.Commands;
using TypeDependencies.Core.State;
using Xunit;

namespace TypeDependencies.Tests.Integration
{
    public class InitCommandTests
    {
        [Fact]
        public void InitCommand_ShouldCreateNewSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            Command command = InitCommand.Create(stateManager);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Invoke("init");

            exitCode.Should().Be(0);
        }
    }
}

