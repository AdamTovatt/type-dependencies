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

        [Fact]
        public void GetDependentsOf_ShouldReturnEmptyForTypeNotInGraph()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> dependents = executor.GetDependentsOf("NonExistentType");

            dependents.Should().BeEmpty();
        }

        [Fact]
        public void GetDependentsOf_ShouldReturnEmptyForTypeWithNoDependents()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            // TypeC is not depended upon by anyone
            HashSet<string> dependents = executor.GetDependentsOf("TypeC");

            dependents.Should().BeEmpty();
        }

        [Fact]
        public void GetDependentsOf_ShouldReturnCorrectDependents()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeB");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> dependents = executor.GetDependentsOf("TypeB");

            dependents.Should().HaveCount(3);
            dependents.Should().Contain("TypeA");
            dependents.Should().Contain("TypeC");
            dependents.Should().Contain("TypeD");
        }

        [Fact]
        public void GetDependentsOf_ShouldReturnEmptyForNullTypeName()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> dependents = executor.GetDependentsOf(null!);

            dependents.Should().BeEmpty();
        }

        [Fact]
        public void GetDependentsOf_ShouldReturnEmptyForEmptyTypeName()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> dependents = executor.GetDependentsOf("");

            dependents.Should().BeEmpty();
        }

        [Fact]
        public void GetDependenciesOf_ShouldReturnNullForTypeNotInGraph()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            IReadOnlySet<string>? dependencies = executor.GetDependenciesOf("NonExistentType");

            dependencies.Should().BeNull();
        }

        [Fact]
        public void GetDependenciesOf_ShouldReturnCorrectDependencies()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");
            graph.AddDependency("TypeA", "TypeD");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            IReadOnlySet<string>? dependencies = executor.GetDependenciesOf("TypeA");

            dependencies.Should().NotBeNull();
            dependencies.Should().HaveCount(3);
            dependencies.Should().Contain("TypeB");
            dependencies.Should().Contain("TypeC");
            dependencies.Should().Contain("TypeD");
        }

        [Fact]
        public void GetDependenciesOf_ShouldReturnEmptySetForTypeWithNoDependencies()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            IReadOnlySet<string>? dependencies = executor.GetDependenciesOf("TypeB");

            dependencies.Should().BeNull();
        }

        [Fact]
        public void GetDependenciesOf_ShouldReturnNullForNullTypeName()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            IReadOnlySet<string>? dependencies = executor.GetDependenciesOf(null!);

            dependencies.Should().BeNull();
        }

        [Fact]
        public void GetDependenciesOf_ShouldReturnNullForEmptyTypeName()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            IReadOnlySet<string>? dependencies = executor.GetDependenciesOf("");

            dependencies.Should().BeNull();
        }

        [Fact]
        public void GetTypesWithDependentCount_ShouldReturnExactMatches()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeE");
            graph.AddDependency("TypeF", "TypeE");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> result = executor.GetTypesWithDependentCount(2);

            result.Should().HaveCount(2);
            result.Should().Contain("TypeB");
            result.Should().Contain("TypeE");
        }

        [Fact]
        public void GetTypesWithDependentCount_ShouldReturnEmptyWhenNoMatches()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> result = executor.GetTypesWithDependentCount(5);

            result.Should().BeEmpty();
        }

        [Fact]
        public void GetTypesWithDependentCountGreaterThan_ShouldReturnCorrectTypes()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeB");
            graph.AddDependency("TypeE", "TypeF");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> result = executor.GetTypesWithDependentCountGreaterThan(1);

            result.Should().Contain("TypeB");
            result.Should().NotContain("TypeF");
        }

        [Fact]
        public void GetTypesWithDependentCountGreaterThanOrEqual_ShouldReturnCorrectTypes()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeE");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> result = executor.GetTypesWithDependentCountGreaterThanOrEqual(2);

            result.Should().Contain("TypeB"); // TypeB has 2 dependents (TypeA, TypeC)
            result.Should().NotContain("TypeE"); // TypeE has 1 dependent (TypeD), not >= 2
        }

        [Fact]
        public void GetTypesWithDependentCountLessThan_ShouldReturnCorrectTypes()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeE");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> result = executor.GetTypesWithDependentCountLessThan(2);

            result.Should().Contain("TypeE");
            result.Should().NotContain("TypeB");
        }

        [Fact]
        public void GetTypesWithDependentCountLessThanOrEqual_ShouldReturnCorrectTypes()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeE");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> result = executor.GetTypesWithDependentCountLessThanOrEqual(1);

            result.Should().Contain("TypeE");
            result.Should().NotContain("TypeB");
        }

        [Fact]
        public void GetTypesWithDependentCountRange_ShouldReturnCorrectTypes()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeE");
            graph.AddDependency("TypeF", "TypeG");
            graph.AddDependency("TypeH", "TypeG");
            graph.AddDependency("TypeI", "TypeG");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> result = executor.GetTypesWithDependentCountRange(1, 2);

            result.Should().Contain("TypeB");
            result.Should().Contain("TypeE");
            result.Should().NotContain("TypeG");
        }

        [Fact]
        public void GetTypesWithDependentCountRange_ShouldBeInclusive()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeC", "TypeB");
            graph.AddDependency("TypeD", "TypeE");
            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);

            HashSet<string> result = executor.GetTypesWithDependentCountRange(2, 2);

            result.Should().Contain("TypeB"); // TypeB has exactly 2 dependents
            result.Should().NotContain("TypeE"); // TypeE has 1 dependent, not 2
        }
    }
}

