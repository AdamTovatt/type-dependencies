using FluentAssertions;
using System.CommandLine;
using TypeDependencies.Cli.Commands;
using TypeDependencies.Core.State;
using Xunit;

namespace TypeDependencies.Tests.Integration
{
    public class AddCommandTests
    {
        [Fact]
        public void AddCommand_ShouldFailWhenNoSessionExists()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            Command command = AddCommand.Create(stateManager);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Invoke("add", @"C:\Test\Test.dll");

            exitCode.Should().Be(1);
        }

        [Fact]
        public void AddCommand_ShouldFailWhenFileDoesNotExist()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            Command command = AddCommand.Create(stateManager);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Invoke("add", @"C:\NonExistent\File.dll");

            exitCode.Should().Be(1);
        }
    }
}

