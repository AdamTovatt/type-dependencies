using FluentAssertions;
using Moq;
using System.CommandLine;
using System.IO;
using TypeDependencies.Cli.Commands;
using TypeDependencies.Cli.Models;
using TypeDependencies.Cli.Suggest;

namespace TypeDependencies.Tests.Integration
{
    public class SuggestCommandTests
    {
        [Fact]
        public void SuggestCommand_ShouldCallSuggesterWithCurrentDirectoryWhenNoOptionProvided()
        {
            Mock<IDllSuggester> suggesterMock = new Mock<IDllSuggester>();
            suggesterMock.Setup(x => x.SuggestDlls(It.IsAny<string>())).Returns(new List<DllSuggestion>());

            Command command = SuggestCommand.Create(suggesterMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "suggest" }).Invoke();

            exitCode.Should().Be(0);
            suggesterMock.Verify(x => x.SuggestDlls(Directory.GetCurrentDirectory()), Times.Once);
        }

        [Fact]
        public void SuggestCommand_ShouldCallSuggesterWithProvidedDirectory()
        {
            string testDirectory = Path.GetTempPath();
            Mock<IDllSuggester> suggesterMock = new Mock<IDllSuggester>();
            suggesterMock.Setup(x => x.SuggestDlls(It.IsAny<string>())).Returns(new List<DllSuggestion>());

            Command command = SuggestCommand.Create(suggesterMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "suggest", "--directory", testDirectory }).Invoke();

            exitCode.Should().Be(0);
            suggesterMock.Verify(x => x.SuggestDlls(testDirectory), Times.Once);
        }

        [Fact]
        public void SuggestCommand_ShouldOutputSuggestionsInCorrectFormat()
        {
            Mock<IDllSuggester> suggesterMock = new Mock<IDllSuggester>();
            List<DllSuggestion> suggestions = new List<DllSuggestion>
            {
                new DllSuggestion("MyProject", @"C:\Test\bin\Debug\MyProject.dll"),
                new DllSuggestion("AnotherProject", @"C:\Test\bin\Release\AnotherProject.dll"),
            };
            suggesterMock.Setup(x => x.SuggestDlls(It.IsAny<string>())).Returns(suggestions);

            Command command = SuggestCommand.Create(suggesterMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            StringWriter stringWriter = new StringWriter();
            TextWriter originalOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                int exitCode = rootCommand.Parse(new[] { "suggest" }).Invoke();

                exitCode.Should().Be(0);
                string output = stringWriter.ToString();
                output.Should().Contain("MyProject -> C:\\Test\\bin\\Debug\\MyProject.dll");
                output.Should().Contain("AnotherProject -> C:\\Test\\bin\\Release\\AnotherProject.dll");
            }
            finally
            {
                Console.SetOut(originalOut);
                stringWriter.Dispose();
            }
        }

        [Fact]
        public void SuggestCommand_ShouldHandleEmptyResults()
        {
            Mock<IDllSuggester> suggesterMock = new Mock<IDllSuggester>();
            suggesterMock.Setup(x => x.SuggestDlls(It.IsAny<string>())).Returns(new List<DllSuggestion>());

            Command command = SuggestCommand.Create(suggesterMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            StringWriter stringWriter = new StringWriter();
            TextWriter originalOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                int exitCode = rootCommand.Parse(new[] { "suggest" }).Invoke();

                exitCode.Should().Be(0);
                string output = stringWriter.ToString();
                output.Should().Contain("No DLL files found");
            }
            finally
            {
                Console.SetOut(originalOut);
                stringWriter.Dispose();
            }
        }

        [Fact]
        public void SuggestCommand_ShouldFailWhenDirectoryDoesNotExist()
        {
            Mock<IDllSuggester> suggesterMock = new Mock<IDllSuggester>();

            Command command = SuggestCommand.Create(suggesterMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "suggest", "--directory", @"C:\NonExistent\Directory" }).Invoke();

            exitCode.Should().Be(1);
            suggesterMock.Verify(x => x.SuggestDlls(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public void SuggestCommand_ShouldHandleExceptionFromSuggester()
        {
            Mock<IDllSuggester> suggesterMock = new Mock<IDllSuggester>();
            suggesterMock.Setup(x => x.SuggestDlls(It.IsAny<string>())).Throws(new Exception("Test exception"));

            Command command = SuggestCommand.Create(suggesterMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "suggest" }).Invoke();

            exitCode.Should().Be(1);
        }
    }
}

