using FluentAssertions;
using TypeDependencies.Core.Export;
using TypeDependencies.Core.Models;

namespace TypeDependencies.Tests.Export
{
    public class HtmlTemplateDataConverterTests
    {
        [Fact]
        public void Convert_ShouldCreateProjectsForAllTypes()
        {
            HtmlTemplateDataConverter converter = new HtmlTemplateDataConverter();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");

            TemplateData result = converter.Convert(graph);

            result.Projects.Should().HaveCount(3);
            result.Projects.Should().Contain(p => p.Name == "TypeA" && p.Id == "TypeA");
            result.Projects.Should().Contain(p => p.Name == "TypeB" && p.Id == "TypeB");
            result.Projects.Should().Contain(p => p.Name == "TypeC" && p.Id == "TypeC");
        }

        [Fact]
        public void Convert_ShouldCreateReferencesFromDependencies()
        {
            HtmlTemplateDataConverter converter = new HtmlTemplateDataConverter();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");
            graph.AddDependency("TypeA", "TypeC");
            graph.AddDependency("TypeB", "TypeD");

            TemplateData result = converter.Convert(graph);

            result.References.Should().HaveCount(3);
            result.References.Should().Contain(r => r.From == "TypeA" && r.To == "TypeB");
            result.References.Should().Contain(r => r.From == "TypeA" && r.To == "TypeC");
            result.References.Should().Contain(r => r.From == "TypeB" && r.To == "TypeD");
        }

        [Fact]
        public void Convert_ShouldHaveEmptyPackages()
        {
            HtmlTemplateDataConverter converter = new HtmlTemplateDataConverter();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");

            TemplateData result = converter.Convert(graph);

            result.Packages.Should().BeEmpty();
        }

        [Fact]
        public void Convert_ShouldUseTypeNameAsId()
        {
            HtmlTemplateDataConverter converter = new HtmlTemplateDataConverter();
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("TypeA", "TypeB");

            TemplateData result = converter.Convert(graph);

            Project typeA = result.Projects.First(p => p.Name == "TypeA");
            typeA.Id.Should().Be("TypeA");
        }

        [Fact]
        public void Convert_ShouldThrowOnNullGraph()
        {
            HtmlTemplateDataConverter converter = new HtmlTemplateDataConverter();

            Action act = () => converter.Convert(null!);
            act.Should().Throw<ArgumentNullException>();
        }
    }
}

