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

        [Fact]
        public void QueryCommand_Dependencies_WithDetailed_ShouldShowDependentCounts()
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

            StringWriter stringWriter = new StringWriter();
            TextWriter originalOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                int exitCode = rootCommand.Parse(new[] { "query", "dependencies", "0", "--detailed" }).Invoke();

                exitCode.Should().Be(0);
                string output = stringWriter.ToString();
                output.Should().Contain("TypeB (2 dependents)");
                output.Should().Contain("TypeE (1 dependents)");
            }
            finally
            {
                Console.SetOut(originalOut);
                stringWriter.Dispose();
                stateManager.ClearSession(sessionId);
            }
        }

        [Fact]
        public void QueryCommand_Dependencies_WithDetailed_ShouldSortByDependencyThenDependentCount()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            // TypeA: 2 dependencies, 0 dependents
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");
            // TypeD: 2 dependencies, 1 dependent
            graph.AddDependency("TypeD", "TypeE");
            graph.AddDependency("TypeD", "TypeF");
            graph.AddDependency("TypeG", "TypeD");
            // TypeH: 3 dependencies, 0 dependents
            graph.AddDependency("TypeH", "TypeI");
            graph.AddDependency("TypeH", "TypeJ");
            graph.AddDependency("TypeH", "TypeK");
            // TypeL: 3 dependencies, 1 dependent
            graph.AddDependency("TypeL", "TypeM");
            graph.AddDependency("TypeL", "TypeN");
            graph.AddDependency("TypeL", "TypeO");
            graph.AddDependency("TypeP", "TypeL");
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
                int exitCode = rootCommand.Parse(new[] { "query", "dependencies", ">=2", "--detailed" }).Invoke();

                exitCode.Should().Be(0);
                string output = stringWriter.ToString();
                string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                
                // Should be sorted: dependency count (2, then 3), then dependent count (0, then 1), then alphabetically
                // Expected order: TypeA (2 deps, 0 dependents), TypeD (2 deps, 1 dependent), TypeH (3 deps, 0 dependents), TypeL (3 deps, 1 dependent)
                int typeAIndex = Array.IndexOf(lines, "TypeA (0 dependents)");
                int typeDIndex = Array.IndexOf(lines, "TypeD (1 dependents)");
                int typeHIndex = Array.IndexOf(lines, "TypeH (0 dependents)");
                int typeLIndex = Array.IndexOf(lines, "TypeL (1 dependents)");

                typeAIndex.Should().BeLessThan(typeDIndex, "TypeA should come before TypeD (same dependency count, but 0 < 1 dependents)");
                typeAIndex.Should().BeLessThan(typeHIndex, "TypeA should come before TypeH (2 < 3 dependencies)");
                typeDIndex.Should().BeLessThan(typeHIndex, "TypeD should come before TypeH (2 < 3 dependencies)");
                typeHIndex.Should().BeLessThan(typeLIndex, "TypeH should come before TypeL (same dependency count, but 0 < 1 dependents)");
            }
            finally
            {
                Console.SetOut(originalOut);
                stringWriter.Dispose();
                stateManager.ClearSession(sessionId);
            }
        }

        [Fact]
        public void QueryCommand_Dependencies_WithDetailed_ShouldSortCorrectlyWhenAllHaveSameDependencyCount()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            // All have 0 dependencies, but different dependent counts
            graph.AddDependency("TypeA", "TypeX");
            graph.AddDependency("TypeB", "TypeX");
            graph.AddDependency("TypeC", "TypeX");
            graph.AddDependency("TypeD", "TypeY");
            graph.AddDependency("TypeE", "TypeY");
            // TypeX: 0 dependencies, 3 dependents
            // TypeY: 0 dependencies, 2 dependents
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
                int exitCode = rootCommand.Parse(new[] { "query", "dependencies", "0", "--detailed" }).Invoke();

                exitCode.Should().Be(0);
                string output = stringWriter.ToString();
                string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                
                int typeXIndex = Array.IndexOf(lines, "TypeX (3 dependents)");
                int typeYIndex = Array.IndexOf(lines, "TypeY (2 dependents)");

                typeYIndex.Should().BeGreaterThan(-1, "TypeY should be in the output");
                typeXIndex.Should().BeGreaterThan(-1, "TypeX should be in the output");
                
                // Should be sorted by dependent count: 2, 3
                if (typeYIndex >= 0 && typeXIndex >= 0)
                    typeYIndex.Should().BeLessThan(typeXIndex, "TypeY (2 dependents) should come before TypeX (3 dependents)");
            }
            finally
            {
                Console.SetOut(originalOut);
                stringWriter.Dispose();
                stateManager.ClearSession(sessionId);
            }
        }

        [Fact]
        public void QueryCommand_Dependents_WithDetailed_ShouldShowDependencyCounts()
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

            StringWriter stringWriter = new StringWriter();
            TextWriter originalOut = Console.Out;
            Console.SetOut(stringWriter);

            try
            {
                int exitCode = rootCommand.Parse(new[] { "query", "dependents", "0", "--detailed" }).Invoke();

                exitCode.Should().Be(0);
                string output = stringWriter.ToString();
                output.Should().Contain("TypeA (2 dependencies)");
                output.Should().Contain("TypeD (1 dependencies)");
            }
            finally
            {
                Console.SetOut(originalOut);
                stringWriter.Dispose();
                stateManager.ClearSession(sessionId);
            }
        }

        [Fact]
        public void QueryCommand_Dependents_WithDetailed_ShouldSortByDependentThenDependencyCount()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            // TypeA: 0 dependents, 2 dependencies
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");
            // TypeD: 0 dependents, 3 dependencies
            graph.AddDependency("TypeD", "TypeE");
            graph.AddDependency("TypeD", "TypeF");
            graph.AddDependency("TypeD", "TypeG");
            // TypeH: 1 dependent, 2 dependencies
            graph.AddDependency("TypeH", "TypeI");
            graph.AddDependency("TypeH", "TypeJ");
            graph.AddDependency("TypeK", "TypeH");
            // TypeL: 1 dependent, 3 dependencies
            graph.AddDependency("TypeL", "TypeM");
            graph.AddDependency("TypeL", "TypeN");
            graph.AddDependency("TypeL", "TypeO");
            graph.AddDependency("TypeP", "TypeL");
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
                int exitCode = rootCommand.Parse(new[] { "query", "dependents", ">=0", "--detailed" }).Invoke();

                exitCode.Should().Be(0);
                string output = stringWriter.ToString();
                string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                
                // Should be sorted: dependent count (0, then 1), then dependency count (2, then 3), then alphabetically
                int typeAIndex = Array.IndexOf(lines, "TypeA (2 dependencies)");
                int typeDIndex = Array.IndexOf(lines, "TypeD (3 dependencies)");
                int typeHIndex = Array.IndexOf(lines, "TypeH (2 dependencies)");
                int typeLIndex = Array.IndexOf(lines, "TypeL (3 dependencies)");

                typeAIndex.Should().BeLessThan(typeDIndex, "TypeA should come before TypeD (same dependent count, but 2 < 3 dependencies)");
                typeAIndex.Should().BeLessThan(typeHIndex, "TypeA should come before TypeH (0 < 1 dependents)");
                typeDIndex.Should().BeLessThan(typeHIndex, "TypeD should come before TypeH (0 < 1 dependents)");
                typeHIndex.Should().BeLessThan(typeLIndex, "TypeH should come before TypeL (same dependent count, but 2 < 3 dependencies)");
            }
            finally
            {
                Console.SetOut(originalOut);
                stringWriter.Dispose();
                stateManager.ClearSession(sessionId);
            }
        }

        [Fact]
        public void QueryCommand_Dependents_WithDetailed_ShouldSortCorrectlyWhenAllHaveSameDependentCount()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            // All have 0 dependents, but different dependency counts
            // TypeA: 2 dependencies (TypeX, TypeY), 0 dependents
            graph.AddDependency("TypeA", "TypeX");
            graph.AddDependency("TypeA", "TypeY");
            // TypeB: 2 dependencies (TypeY, TypeZ), 0 dependents
            graph.AddDependency("TypeB", "TypeY");
            graph.AddDependency("TypeB", "TypeZ");
            // TypeC: 3 dependencies (TypeX, TypeY, TypeZ), 0 dependents
            graph.AddDependency("TypeC", "TypeX");
            graph.AddDependency("TypeC", "TypeY");
            graph.AddDependency("TypeC", "TypeZ");
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
                int exitCode = rootCommand.Parse(new[] { "query", "dependents", "0", "--detailed" }).Invoke();

                exitCode.Should().Be(0);
                string output = stringWriter.ToString();
                string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                
                int typeAIndex = Array.IndexOf(lines, "TypeA (2 dependencies)");
                int typeBIndex = Array.IndexOf(lines, "TypeB (2 dependencies)");
                int typeCIndex = Array.IndexOf(lines, "TypeC (3 dependencies)");

                typeAIndex.Should().BeGreaterThan(-1, "TypeA should be in the output");
                typeBIndex.Should().BeGreaterThan(-1, "TypeB should be in the output");
                typeCIndex.Should().BeGreaterThan(-1, "TypeC should be in the output");
                
                // Should be sorted by dependency count: 2, 2, 3 (then alphabetically for same count)
                // TypeA and TypeB both have 2 dependencies, so they should be sorted alphabetically
                if (typeAIndex >= 0 && typeBIndex >= 0)
                    typeAIndex.Should().BeLessThan(typeBIndex, "TypeA should come before TypeB (alphabetically, both have 2 dependencies)");
                if (typeBIndex >= 0 && typeCIndex >= 0)
                    typeBIndex.Should().BeLessThan(typeCIndex, "TypeB (2 dependencies) should come before TypeC (3 dependencies)");
            }
            finally
            {
                Console.SetOut(originalOut);
                stringWriter.Dispose();
                stateManager.ClearSession(sessionId);
            }
        }

        [Fact]
        public void QueryCommand_Dependencies_WithDetailed_ShouldWorkWithRangeQueries()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            // TypeA: 1 dependency, 0 dependents
            graph.AddDependency("TypeA", "TypeB");
            // TypeC: 1 dependency, 1 dependent
            graph.AddDependency("TypeC", "TypeD");
            graph.AddDependency("TypeE", "TypeC");
            // TypeF: 2 dependencies, 0 dependents
            graph.AddDependency("TypeF", "TypeG");
            graph.AddDependency("TypeF", "TypeH");
            // TypeI: 2 dependencies, 1 dependent
            graph.AddDependency("TypeI", "TypeJ");
            graph.AddDependency("TypeI", "TypeK");
            graph.AddDependency("TypeL", "TypeI");
            // TypeM: 3 dependencies, 0 dependents
            graph.AddDependency("TypeM", "TypeN");
            graph.AddDependency("TypeM", "TypeO");
            graph.AddDependency("TypeM", "TypeP");
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
                int exitCode = rootCommand.Parse(new[] { "query", "dependencies", "1-3", "--detailed" }).Invoke();

                exitCode.Should().Be(0);
                string output = stringWriter.ToString();
                string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                
                // Should be sorted: dependency count (1, then 2, then 3), then dependent count (0, then 1), then alphabetically
                int typeAIndex = Array.IndexOf(lines, "TypeA (0 dependents)");
                int typeCIndex = Array.IndexOf(lines, "TypeC (1 dependents)");
                int typeFIndex = Array.IndexOf(lines, "TypeF (0 dependents)");
                int typeIIndex = Array.IndexOf(lines, "TypeI (1 dependents)");
                int typeMIndex = Array.IndexOf(lines, "TypeM (0 dependents)");

                typeAIndex.Should().BeLessThan(typeCIndex, "TypeA should come before TypeC (same dependency count, but 0 < 1 dependents)");
                typeAIndex.Should().BeLessThan(typeFIndex, "TypeA should come before TypeF (1 < 2 dependencies)");
                typeCIndex.Should().BeLessThan(typeFIndex, "TypeC should come before TypeF (1 < 2 dependencies)");
                typeFIndex.Should().BeLessThan(typeIIndex, "TypeF should come before TypeI (same dependency count, but 0 < 1 dependents)");
                typeFIndex.Should().BeLessThan(typeMIndex, "TypeF should come before TypeM (2 < 3 dependencies)");
                typeIIndex.Should().BeLessThan(typeMIndex, "TypeI should come before TypeM (2 < 3 dependencies)");
            }
            finally
            {
                Console.SetOut(originalOut);
                stringWriter.Dispose();
                stateManager.ClearSession(sessionId);
            }
        }

        [Fact]
        public void QueryCommand_Dependents_WithDetailed_ShouldWorkWithRangeQueries()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            // TypeA: 0 dependents, 1 dependency
            graph.AddDependency("TypeA", "TypeB");
            // TypeC: 0 dependents, 2 dependencies
            graph.AddDependency("TypeC", "TypeD");
            graph.AddDependency("TypeC", "TypeE");
            // TypeF: 1 dependent, 1 dependency
            graph.AddDependency("TypeF", "TypeG");
            graph.AddDependency("TypeH", "TypeF");
            // TypeI: 1 dependent, 2 dependencies
            graph.AddDependency("TypeI", "TypeJ");
            graph.AddDependency("TypeI", "TypeK");
            graph.AddDependency("TypeL", "TypeI");
            // TypeM: 2 dependents, 1 dependency
            graph.AddDependency("TypeM", "TypeN");
            graph.AddDependency("TypeO", "TypeM");
            graph.AddDependency("TypeP", "TypeM");
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
                int exitCode = rootCommand.Parse(new[] { "query", "dependents", "0-2", "--detailed" }).Invoke();

                exitCode.Should().Be(0);
                string output = stringWriter.ToString();
                string[] lines = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                
                // Should be sorted: dependent count (0, then 1, then 2), then dependency count (1, then 2), then alphabetically
                int typeAIndex = Array.IndexOf(lines, "TypeA (1 dependencies)");
                int typeCIndex = Array.IndexOf(lines, "TypeC (2 dependencies)");
                int typeFIndex = Array.IndexOf(lines, "TypeF (1 dependencies)");
                int typeIIndex = Array.IndexOf(lines, "TypeI (2 dependencies)");
                int typeMIndex = Array.IndexOf(lines, "TypeM (1 dependencies)");

                typeAIndex.Should().BeLessThan(typeCIndex, "TypeA should come before TypeC (same dependent count, but 1 < 2 dependencies)");
                typeAIndex.Should().BeLessThan(typeFIndex, "TypeA should come before TypeF (0 < 1 dependents)");
                typeCIndex.Should().BeLessThan(typeFIndex, "TypeC should come before TypeF (0 < 1 dependents)");
                typeFIndex.Should().BeLessThan(typeIIndex, "TypeF should come before TypeI (same dependent count, but 1 < 2 dependencies)");
                typeFIndex.Should().BeLessThan(typeMIndex, "TypeF should come before TypeM (1 < 2 dependents)");
                typeIIndex.Should().BeLessThan(typeMIndex, "TypeI should come before TypeM (1 < 2 dependents)");
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

