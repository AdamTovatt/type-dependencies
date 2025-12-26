using FluentAssertions;
using TypeDependencies.Core.Models;
using TypeDependencies.Core.State;

namespace TypeDependencies.Tests.State
{
    public class AnalysisStateManagerTests
    {
        [Fact]
        public void InitializeSession_ShouldCreateNewSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();

            string sessionId = stateManager.InitializeSession();

            sessionId.Should().NotBeNullOrWhiteSpace();
            stateManager.SessionExists(sessionId).Should().BeTrue();
        }

        [Fact]
        public void AddDllPath_ShouldAddPathToSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            string dllPath = @"C:\Test\Test.dll";

            stateManager.AddDllPath(sessionId, dllPath);

            IReadOnlyList<string> paths = stateManager.GetDllPaths(sessionId);
            paths.Should().Contain(dllPath);
        }

        [Fact]
        public void AddDllPath_ShouldNotAddDuplicate()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            string dllPath = @"C:\Test\Test.dll";

            stateManager.AddDllPath(sessionId, dllPath);
            stateManager.AddDllPath(sessionId, dllPath);

            IReadOnlyList<string> paths = stateManager.GetDllPaths(sessionId);
            paths.Should().HaveCount(1);
            paths.Should().Contain(dllPath);
        }

        [Fact]
        public void AddDllPath_ShouldBeCaseInsensitive()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            string dllPath1 = @"C:\Test\Test.dll";
            string dllPath2 = @"C:\TEST\TEST.DLL";

            stateManager.AddDllPath(sessionId, dllPath1);
            stateManager.AddDllPath(sessionId, dllPath2);

            IReadOnlyList<string> paths = stateManager.GetDllPaths(sessionId);
            paths.Should().HaveCount(1);
        }

        [Fact]
        public void GetDllPaths_ShouldReturnEmptyListForNewSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();

            IReadOnlyList<string> paths = stateManager.GetDllPaths(sessionId);

            paths.Should().BeEmpty();
        }

        [Fact]
        public void ClearSession_ShouldRemoveSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();

            stateManager.ClearSession(sessionId);

            stateManager.SessionExists(sessionId).Should().BeFalse();
        }

        [Fact]
        public void AddDllPath_ShouldThrowOnInvalidSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();

            Action act = () => stateManager.AddDllPath("invalid-session", @"C:\Test\Test.dll");
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetDllPaths_ShouldThrowOnInvalidSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();

            Action act = () => stateManager.GetDllPaths("invalid-session");
            act.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void SaveGeneratedGraph_ShouldSaveGraphToSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");

            stateManager.SaveGeneratedGraph(sessionId, graph);

            DependencyGraph? loadedGraph = stateManager.GetGeneratedGraph(sessionId);
            loadedGraph.Should().NotBeNull();
            loadedGraph!.Dependencies.Should().ContainKey("TypeA");
            loadedGraph.Dependencies["TypeA"].Should().Contain("TypeB");
            loadedGraph.Dependencies["TypeA"].Should().Contain("TypeC");
        }

        [Fact]
        public void SaveGeneratedGraph_ShouldThrowOnInvalidSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            DependencyGraph graph = new DependencyGraph();

            Action act = () => stateManager.SaveGeneratedGraph("invalid-session", graph);
            act.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void SaveGeneratedGraph_ShouldThrowOnNullGraph()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();

            Action act = () => stateManager.SaveGeneratedGraph(sessionId, null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SaveGeneratedGraph_ShouldOverwriteExistingGraph()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            
            DependencyGraph graph1 = new DependencyGraph();
            graph1.AddDependency("TypeA", "TypeB");
            stateManager.SaveGeneratedGraph(sessionId, graph1);

            DependencyGraph graph2 = new DependencyGraph();
            graph2.AddDependency("TypeC", "TypeD");
            stateManager.SaveGeneratedGraph(sessionId, graph2);

            DependencyGraph? loadedGraph = stateManager.GetGeneratedGraph(sessionId);
            loadedGraph.Should().NotBeNull();
            loadedGraph!.Dependencies.Should().ContainKey("TypeC");
            loadedGraph.Dependencies.Should().NotContainKey("TypeA");
        }

        [Fact]
        public void SaveGeneratedGraph_ShouldHandleEmptyGraph()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();

            stateManager.SaveGeneratedGraph(sessionId, graph);

            DependencyGraph? loadedGraph = stateManager.GetGeneratedGraph(sessionId);
            loadedGraph.Should().NotBeNull();
            loadedGraph!.Dependencies.Should().BeEmpty();
        }

        [Fact]
        public void GetGeneratedGraph_ShouldReturnNullWhenNoGraphExists()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();

            DependencyGraph? graph = stateManager.GetGeneratedGraph(sessionId);

            graph.Should().BeNull();
        }

        [Fact]
        public void GetGeneratedGraph_ShouldReturnCorrectGraph()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph originalGraph = new DependencyGraph();
            originalGraph.AddDependency("TypeA", "TypeB");
            originalGraph.AddDependency("TypeA", "TypeC");
            originalGraph.AddDependency("TypeB", "TypeD");
            stateManager.SaveGeneratedGraph(sessionId, originalGraph);

            DependencyGraph? loadedGraph = stateManager.GetGeneratedGraph(sessionId);

            loadedGraph.Should().NotBeNull();
            loadedGraph!.Dependencies.Should().HaveCount(2);
            loadedGraph.Dependencies["TypeA"].Should().HaveCount(2);
            loadedGraph.Dependencies["TypeB"].Should().HaveCount(1);
        }

        [Fact]
        public void GetGeneratedGraph_ShouldThrowOnInvalidSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();

            Action act = () => stateManager.GetGeneratedGraph("invalid-session");
            act.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void HasGeneratedGraph_ShouldReturnFalseForNewSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();

            bool hasGraph = stateManager.HasGeneratedGraph(sessionId);

            hasGraph.Should().BeFalse();
        }

        [Fact]
        public void HasGeneratedGraph_ShouldReturnTrueAfterSave()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            stateManager.SaveGeneratedGraph(sessionId, graph);

            bool hasGraph = stateManager.HasGeneratedGraph(sessionId);

            hasGraph.Should().BeTrue();
        }

        [Fact]
        public void HasGeneratedGraph_ShouldReturnFalseAfterClear()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            stateManager.SaveGeneratedGraph(sessionId, graph);
            stateManager.ClearSession(sessionId);

            bool hasGraph = stateManager.HasGeneratedGraph(sessionId);

            hasGraph.Should().BeFalse();
        }

        [Fact]
        public void HasGeneratedGraph_ShouldReturnFalseForInvalidSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();

            bool hasGraph = stateManager.HasGeneratedGraph("invalid-session");

            hasGraph.Should().BeFalse();
        }

        [Fact]
        public void HasGeneratedGraph_ShouldReturnFalseForEmptySessionId()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();

            bool hasGraph = stateManager.HasGeneratedGraph("");

            hasGraph.Should().BeFalse();
        }
    }
}

