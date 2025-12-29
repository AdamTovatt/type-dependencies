using FluentAssertions;
using Moq;
using TypeDependencies.Cli;
using TypeDependencies.Cli.Suggest;
using TypeDependencies.Core.Analysis;
using TypeDependencies.Core.Export;
using TypeDependencies.Core.Models;
using TypeDependencies.Core.State;

namespace TypeDependencies.Tests.Integration
{
    public class McpServerTests
    {
        [Fact]
        public async Task InitializeSessionAsync_ShouldReturnSessionId()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            ICurrentSessionFinder sessionFinder = new CurrentSessionFinder();
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinder, dllSuggesterMock.Object);

            string result = await mcpTools.InitializeSessionAsync(CancellationToken.None);

            result.Should().StartWith("Session initialized:");
        }

        [Fact]
        public async Task AddDllAsync_ShouldFailWhenNoSessionExists()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns((string?)null);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            // Use a path that exists so we can test the session check
            string tempFile = Path.GetTempFileName();
            try
            {
                string result = await mcpTools.AddDllAsync(tempFile, CancellationToken.None);

                result.Should().Contain("Error: No active session found");
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task AddDllAsync_ShouldFailWhenFileDoesNotExist()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.AddDllAsync(@"C:\NonExistent\File.dll", CancellationToken.None);

            result.Should().Contain("Error: DLL file not found");
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public async Task GenerateGraphAsync_ShouldFailWhenNoSessionExists()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns((string?)null);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.GenerateGraphAsync(CancellationToken.None);

            result.Should().Contain("Error: No active session found");
        }

        [Fact]
        public async Task GenerateGraphAsync_ShouldFailWhenNoDllsAdded()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.GenerateGraphAsync(CancellationToken.None);

            result.Should().Contain("Error: No DLLs added to the session");
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public async Task ExportGraphAsync_ShouldFailWhenNoSessionExists()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns((string?)null);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.ExportGraphAsync(null, null, CancellationToken.None);

            result.Should().Contain("Error: No active session found");
        }

        [Fact]
        public async Task ExportGraphAsync_ShouldFailWhenNoGraphGenerated()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.ExportGraphAsync(null, null, CancellationToken.None);

            result.Should().Contain("Error: No generated graph found");
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public async Task QueryDependentsOfAsync_ShouldFailWhenNoGraphGenerated()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns((string?)null);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.QueryDependentsOfAsync("SomeType", CancellationToken.None);

            result.Should().Contain("Error: No generated graph found");
        }

        [Fact]
        public async Task QueryDependentsOfAsync_ShouldReturnEmptyWhenNoDependents()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.QueryDependentsOfAsync("TypeA", CancellationToken.None);

            result.Should().Contain("No types depend on");
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public async Task QueryDependentsOfAsync_ShouldReturnCorrectResults()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.QueryDependentsOfAsync("TypeB", CancellationToken.None);

            result.Should().Contain("TypeA");
            result.Should().Contain("TypeC");
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public async Task QueryDependenciesOfAsync_ShouldReturnCorrectResults()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.QueryDependenciesOfAsync("TypeA", CancellationToken.None);

            result.Should().Contain("TypeB");
            result.Should().Contain("TypeC");
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public async Task QueryDependentsAsync_ShouldHandleExactCount()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.QueryDependentsAsync("2", detailed: false, CancellationToken.None);

            result.Should().Contain("TypeB");
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public async Task QueryDependenciesAsync_ShouldHandleExactCount()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.QueryDependenciesAsync("2", detailed: false, CancellationToken.None);

            result.Should().Contain("TypeA");
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public async Task QueryDependentsAsync_WithDetailed_ShouldShowDependencyCounts()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.QueryDependentsAsync("2", detailed: true, CancellationToken.None);

            result.Should().Contain("TypeB");
            result.Should().Contain("dependencies");
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public async Task QueryDependenciesAsync_WithDetailed_ShouldShowDependentCounts()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.QueryDependenciesAsync("2", detailed: true, CancellationToken.None);

            result.Should().Contain("TypeA");
            result.Should().Contain("dependents");
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public async Task QueryTransitiveDependenciesOfAsync_ShouldReturnCorrectResults()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeB", "TypeC");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.QueryTransitiveDependenciesOfAsync("TypeA", CancellationToken.None);

            result.Should().Contain("TypeB");
            result.Should().Contain("TypeC");
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public async Task QueryTransitiveDependentsOfAsync_ShouldReturnCorrectResults()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeB", "TypeA");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.QueryTransitiveDependentsOfAsync("TypeA", CancellationToken.None);

            result.Should().Contain("TypeB");
            result.Should().Contain("TypeC");
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public async Task QueryCircularDependenciesAsync_ShouldReturnEmptyWhenNoCycles()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeB", "TypeC");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.QueryCircularDependenciesAsync(CancellationToken.None);

            result.Should().Contain("No circular dependencies found");
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public async Task QueryCircularDependenciesAsync_ShouldDetectCycles()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeB", "TypeA");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            Mock<ICurrentSessionFinder> sessionFinderMock = new Mock<ICurrentSessionFinder>();
            sessionFinderMock.Setup(x => x.FindCurrentSessionId()).Returns(sessionId);
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinderMock.Object, dllSuggesterMock.Object);

            string result = await mcpTools.QueryCircularDependenciesAsync(CancellationToken.None);

            result.Should().Contain("TypeA");
            result.Should().Contain("TypeB");
            result.Should().Contain("->");
            stateManager.ClearSession(sessionId);
        }

        [Fact]
        public async Task GetHelpAsync_ShouldReturnHelpText()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            ITypeAnalyzer typeAnalyzer = new TypeAnalyzer();
            IExportStrategy exportStrategy = new DotExportStrategy();
            ICurrentSessionFinder sessionFinder = new CurrentSessionFinder();
            Mock<IDllSuggester> dllSuggesterMock = new Mock<IDllSuggester>();

            McpTools mcpTools = new McpTools(stateManager, typeAnalyzer, exportStrategy, sessionFinder, dllSuggesterMock.Object);

            string result = await mcpTools.GetHelpAsync(CancellationToken.None);

            result.Should().Contain("TypeDependencies");
            result.Should().Contain("WORKFLOW");
            result.Should().Contain("td_init");
            result.Should().Contain("td_add");
            result.Should().Contain("td_generate");
            result.Should().Contain("td_export");
            result.Should().Contain("td_query");
            result.Should().Contain("EXAMPLES");
            result.Should().Contain("ERROR HANDLING");
        }
    }
}

