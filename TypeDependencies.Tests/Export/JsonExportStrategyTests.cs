using FluentAssertions;
using System.Text.Json;
using TypeDependencies.Core.Export;
using TypeDependencies.Core.Models;
using Xunit;

namespace TypeDependencies.Tests.Export
{
    public class JsonExportStrategyTests
    {
        [Fact]
        public void Export_ShouldCreateValidJsonFile()
        {
            JsonExportStrategy strategy = new JsonExportStrategy();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");
            graph.AddDependency("TypeB", "TypeD");

            string tempFile = Path.GetTempFileName();
            try
            {
                strategy.Export(graph, tempFile);

                File.Exists(tempFile).Should().BeTrue();
                string content = File.ReadAllText(tempFile);
                Dictionary<string, List<string>>? data = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(content);
                
                data.Should().NotBeNull();
                data.Should().ContainKey("TypeA");
                data["TypeA"].Should().Contain("TypeB");
                data["TypeA"].Should().Contain("TypeC");
                data.Should().ContainKey("TypeB");
                data["TypeB"].Should().Contain("TypeD");
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void Export_ShouldSortDependencies()
        {
            JsonExportStrategy strategy = new JsonExportStrategy();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeZ");
            graph.AddDependency("TypeA", "TypeA");
            graph.AddDependency("TypeA", "TypeM");

            string tempFile = Path.GetTempFileName();
            try
            {
                strategy.Export(graph, tempFile);

                string content = File.ReadAllText(tempFile);
                Dictionary<string, List<string>>? data = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(content);
                
                data.Should().NotBeNull();
                List<string> dependencies = data!["TypeA"];
                dependencies.Should().BeInAscendingOrder();
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void Export_ShouldThrowOnNullGraph()
        {
            JsonExportStrategy strategy = new JsonExportStrategy();
            string tempFile = Path.GetTempFileName();
            try
            {
                Action act = () => strategy.Export(null!, tempFile);
                act.Should().Throw<ArgumentNullException>();
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void Export_ShouldThrowOnNullPath()
        {
            JsonExportStrategy strategy = new JsonExportStrategy();
            DependencyGraph graph = new DependencyGraph();

            Action act = () => strategy.Export(graph, null!);
            act.Should().Throw<ArgumentException>();
        }
    }
}

