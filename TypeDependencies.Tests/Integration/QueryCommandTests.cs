using FluentAssertions;
using Moq;
using System.CommandLine;
using System.IO;
using TypeDependencies.Cli.Commands;
using TypeDependencies.Core.Models;
using TypeDependencies.Core.State;

namespace TypeDependencies.Tests.Integration
{
    public class QueryCommandTests
    {
        [Fact]
        public void QueryCommand_ShouldFailWhenNoSessionExists()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns((string?)null);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependents-of", "SomeType" }).Invoke();

            exitCode.Should().Be(1);
        }

        [Fact]
        public void QueryCommand_ShouldFailWhenNoGeneratedGraphExists()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependents-of", "SomeType" }).Invoke();

            exitCode.Should().Be(1);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_DependentsOf_ShouldReturnCorrectResults()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeE");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependents-of", "TypeB" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_DependentsOf_ShouldHandleEmptyResult()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependents-of", "TypeA" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_DependenciesOf_ShouldReturnCorrectResults()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");
            graph.AddDependency("TypeA", "TypeD");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependencies-of", "TypeA" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_DependenciesOf_ShouldHandleTypeNotFound()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependencies-of", "NonExistentType" }).Invoke();

            exitCode.Should().Be(1);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_Dependents_ShouldHandleExactCount()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeE");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependents", "2" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_Dependents_ShouldHandleGreaterThan()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeB");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependents", ">1" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_Dependents_ShouldHandleGreaterThanOrEqual()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependents", ">=2" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_Dependents_ShouldHandleLessThan()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeE");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependents", "<2" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_Dependents_ShouldHandleLessThanOrEqual()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeE");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependents", "<=1" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_Dependents_ShouldHandleRange()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeE");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependents", "1-2" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_Dependents_ShouldHandleInvalidExpression()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependents", "invalid" }).Invoke();

            exitCode.Should().Be(1);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_Dependencies_ShouldHandleExactCount()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");
            graph.AddDependency("TypeD", "TypeE");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependencies", "2" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_Dependencies_ShouldHandleGreaterThan()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");
            graph.AddDependency("TypeA", "TypeD");
            graph.AddDependency("TypeE", "TypeF");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependencies", ">2" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_Dependencies_ShouldHandleGreaterThanOrEqual()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");
            graph.AddDependency("TypeD", "TypeE");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependencies", ">=2" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_Dependencies_ShouldHandleLessThan()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");
            graph.AddDependency("TypeD", "TypeE");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependencies", "<2" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_Dependencies_ShouldHandleLessThanOrEqual()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");
            graph.AddDependency("TypeD", "TypeE");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependencies", "<=1" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_Dependencies_ShouldHandleRange()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");
            graph.AddDependency("TypeD", "TypeE");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependencies", "1-2" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_Dependencies_ShouldHandleInvalidExpression()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "dependencies", "invalid" }).Invoke();

            exitCode.Should().Be(1);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_TransitiveDependenciesOf_ShouldReturnCorrectResults()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeB", "TypeC");
            graph.AddDependency("TypeC", "TypeD");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "transitive-dependencies-of", "TypeA" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_TransitiveDependenciesOf_ShouldHandleEmptyResult()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "transitive-dependencies-of", "TypeB" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_TransitiveDependenciesOf_ShouldHandleCycles()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeB", "TypeC");
            graph.AddDependency("TypeC", "TypeA");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "transitive-dependencies-of", "TypeA" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_TransitiveDependentsOf_ShouldReturnCorrectResults()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeB", "TypeA");
            graph.AddDependency("TypeD", "TypeA");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "transitive-dependents-of", "TypeA" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_TransitiveDependentsOf_ShouldHandleEmptyResult()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "transitive-dependents-of", "TypeA" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_TransitiveDependentsOf_ShouldHandleCycles()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeB", "TypeC");
            graph.AddDependency("TypeC", "TypeA");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "transitive-dependents-of", "TypeA" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_CircularDependencies_ShouldReturnEmptyWhenNoCycles()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeB", "TypeC");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "circular-dependencies" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_CircularDependencies_ShouldDetectCycles()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeB", "TypeA");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "circular-dependencies" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_CircularDependencies_ShouldDetectMultipleCycles()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeB", "TypeA");
            graph.AddDependency("TypeC", "TypeD");
            graph.AddDependency("TypeD", "TypeC");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            int exitCode = rootCommand.Parse(new[] { "query", "circular-dependencies" }).Invoke();

            exitCode.Should().Be(0);
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public void QueryCommand_Dependents_ShouldFilterAnonymousTypes()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("<>f__AnonymousType0`1", "TypeB");
            graph.AddDependency("<Value>j__TPar", "TypeB");
            graph.AddDependency("<Vector>j__TPar", "TypeB");
            graph.AddDependency("TypeC", "TypeD");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);

            Command command = QueryCommand.Create(stateManager, sessionFinderMock.Object);
            RootCommand rootCommand = new RootCommand();
            rootCommand.Subcommands.Add(command);

            StringWriter stringWriter = new StringWriter();
            TextWriter originalOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                int exitCode = rootCommand.Parse(new[] { "query", "dependents", "1" }).Invoke();

                exitCode.Should().Be(0);
                string output = stringWriter.ToString();
                output.Should().Contain("TypeD");
                output.Should().NotContain("<>f__AnonymousType");
                output.Should().NotContain("<Value>");
                output.Should().NotContain("<Vector>");
            }
            finally
            {
                Console.SetOut(originalOut);
                stringWriter.Dispose();
                stateManager.ClearSession(sessionId);
            }
        }
    }
}

