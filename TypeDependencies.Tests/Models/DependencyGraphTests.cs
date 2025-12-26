using FluentAssertions;
using TypeDependencies.Core.Models;
using Xunit;

namespace TypeDependencies.Tests.Models
{
    public class DependencyGraphTests
    {
        [Fact]
        public void AddDependency_ShouldAddDependency()
        {
            DependencyGraph graph = new DependencyGraph();
            
            graph.AddDependency("TypeA", "TypeB");
            
            IReadOnlySet<string>? dependencies = graph.GetDependencies("TypeA");
            dependencies.Should().NotBeNull();
            dependencies.Should().Contain("TypeB");
        }

        [Fact]
        public void AddDependency_ShouldNotAddDuplicate()
        {
            DependencyGraph graph = new DependencyGraph();
            
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeB");
            
            IReadOnlySet<string>? dependencies = graph.GetDependencies("TypeA");
            dependencies.Should().NotBeNull();
            dependencies.Should().HaveCount(1);
            dependencies.Should().Contain("TypeB");
        }

        [Fact]
        public void AddDependencies_ShouldAddMultipleDependencies()
        {
            DependencyGraph graph = new DependencyGraph();
            
            graph.AddDependencies("TypeA", new[] { "TypeB", "TypeC", "TypeD" });
            
            IReadOnlySet<string>? dependencies = graph.GetDependencies("TypeA");
            dependencies.Should().NotBeNull();
            dependencies.Should().HaveCount(3);
            dependencies.Should().Contain("TypeB");
            dependencies.Should().Contain("TypeC");
            dependencies.Should().Contain("TypeD");
        }

        [Fact]
        public void ContainsType_ShouldReturnTrueForExistingType()
        {
            DependencyGraph graph = new DependencyGraph();
            
            graph.AddDependency("TypeA", "TypeB");
            
            graph.ContainsType("TypeA").Should().BeTrue();
        }

        [Fact]
        public void ContainsType_ShouldReturnFalseForNonExistentType()
        {
            DependencyGraph graph = new DependencyGraph();
            
            graph.ContainsType("TypeA").Should().BeFalse();
        }

        [Fact]
        public void GetDependencies_ShouldReturnNullForNonExistentType()
        {
            DependencyGraph graph = new DependencyGraph();
            
            IReadOnlySet<string>? dependencies = graph.GetDependencies("TypeA");
            dependencies.Should().BeNull();
        }

        [Fact]
        public void AddDependency_ShouldIgnoreNullOrWhitespace()
        {
            DependencyGraph graph = new DependencyGraph();
            
            graph.AddDependency("TypeA", null!);
            graph.AddDependency("TypeA", "");
            graph.AddDependency("TypeA", "   ");
            
            IReadOnlySet<string>? dependencies = graph.GetDependencies("TypeA");
            dependencies.Should().BeNull();
        }
    }
}

