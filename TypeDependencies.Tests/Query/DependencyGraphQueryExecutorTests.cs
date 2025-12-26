using FluentAssertions;
using TypeDependencies.Core.Models;
using TypeDependencies.Core.Query;

namespace TypeDependencies.Tests.Query
{
    public class DependencyGraphQueryExecutorTests
    {
        [Fact]
        public void GetDependentCount_ShouldReturnZeroForTypeNotInGraph()
        {
            DependencyGraph graph = new DependencyGraph();
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            int count = executor.GetDependentCount("NonExistentType");

            count.Should().Be(0);
        }

        [Fact]
        public void GetDependentCount_ShouldReturnZeroForTypeWithNoDependents()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            // TypeC is not depended upon by anyone
            int count = executor.GetDependentCount("TypeC");

            count.Should().Be(0);
        }

        [Fact]
        public void GetDependentCount_ShouldReturnCorrectCount()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeB");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            int count = executor.GetDependentCount("TypeB");

            count.Should().Be(3);
        }

        [Fact]
        public void GetDependentCount_ShouldReturnZeroForNullTypeName()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            int count = executor.GetDependentCount(null!);

            count.Should().Be(0);
        }

        [Fact]
        public void GetDependentCount_ShouldReturnZeroForEmptyTypeName()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            int count = executor.GetDependentCount("");

            count.Should().Be(0);
        }

        [Fact]
        public void GetDependentCount_ShouldReturnZeroForWhitespaceTypeName()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            int count = executor.GetDependentCount("   ");

            count.Should().Be(0);
        }

        [Fact]
        public void GetTypesWithNoDependents_ShouldReturnAllTypesWhenNothingDepends()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeD");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> types = executor.GetTypesWithNoDependents();

            types.Should().Contain("TypeA");
            types.Should().Contain("TypeC");
            types.Should().NotContain("TypeB");
            types.Should().NotContain("TypeD");
        }

        [Fact]
        public void GetTypesWithNoDependents_ShouldReturnEmptyWhenAllTypesHaveDependents()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeB", "TypeC");
            graph.AddDependency("TypeC", "TypeA");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> types = executor.GetTypesWithNoDependents();

            types.Should().BeEmpty();
        }

        [Fact]
        public void GetTypesWithNoDependents_ShouldReturnCorrectSubset()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeE");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> types = executor.GetTypesWithNoDependents();

            types.Should().Contain("TypeA");
            types.Should().Contain("TypeC");
            types.Should().Contain("TypeD");
            types.Should().NotContain("TypeB");
            types.Should().NotContain("TypeE");
        }

        [Fact]
        public void GetTypesWithNoDependents_ShouldHandleComplexDependencyChains()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeB", "TypeC");
            graph.AddDependency("TypeD", "TypeE");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> types = executor.GetTypesWithNoDependents();

            types.Should().Contain("TypeA");
            types.Should().Contain("TypeD");
            types.Should().NotContain("TypeB");
            types.Should().NotContain("TypeC");
            types.Should().NotContain("TypeE");
        }

        [Fact]
        public void GetTypesWithNoDependents_ShouldReturnAllTypesForEmptyGraph()
        {
            DependencyGraph graph = new DependencyGraph();
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> types = executor.GetTypesWithNoDependents();

            types.Should().BeEmpty();
        }

        [Fact]
        public void Constructor_ShouldThrowOnNullGraph()
        {
            Action act = () => new DependencyGraphQueryExecutor(null!);
            act.Should().Throw<ArgumentNullException>();
        }
    }
}

