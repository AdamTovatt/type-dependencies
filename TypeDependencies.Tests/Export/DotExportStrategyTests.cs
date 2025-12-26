using FluentAssertions;
using TypeDependencies.Core.Export;
using TypeDependencies.Core.Models;

namespace TypeDependencies.Tests.Export
{
    public class DotExportStrategyTests
    {
        [Fact]
        public void Export_ShouldCreateValidDotFile()
        {
            DotExportStrategy strategy = new DotExportStrategy();
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
                content.Should().Contain("digraph TypeDependencies");
                content.Should().Contain("\"TypeA\" -> \"TypeB\"");
                content.Should().Contain("\"TypeA\" -> \"TypeC\"");
                content.Should().Contain("\"TypeB\" -> \"TypeD\"");
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        [Fact]
        public void Export_ShouldEscapeSpecialCharacters()
        {
            DotExportStrategy strategy = new DotExportStrategy();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("Type\"A", "Type\\B");

            string tempFile = Path.GetTempFileName();
            try
            {
                strategy.Export(graph, tempFile);

                string content = File.ReadAllText(tempFile);
                content.Should().Contain("\\\"");
                content.Should().Contain("\\\\");
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
            DotExportStrategy strategy = new DotExportStrategy();
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
            DotExportStrategy strategy = new DotExportStrategy();
            DependencyGraph graph = new DependencyGraph();

            Action act = () => strategy.Export(graph, null!);
            act.Should().Throw<ArgumentException>();
        }
    }
}

