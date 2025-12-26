using FluentAssertions;
using TypeDependencies.Core.Analysis;
using TypeDependencies.Core.Models;

namespace TypeDependencies.Tests.Analysis
{
    public class TypeAnalyzerTests
    {
        [Fact]
        public void AnalyzeAssembly_ShouldThrowOnNullPath()
        {
            ITypeAnalyzer analyzer = new TypeAnalyzer();

            Action act = () => analyzer.AnalyzeAssembly(null!);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AnalyzeAssembly_ShouldThrowOnEmptyPath()
        {
            ITypeAnalyzer analyzer = new TypeAnalyzer();

            Action act = () => analyzer.AnalyzeAssembly("");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void AnalyzeAssembly_ShouldThrowOnNonExistentFile()
        {
            ITypeAnalyzer analyzer = new TypeAnalyzer();

            Action act = () => analyzer.AnalyzeAssembly(@"C:\NonExistent\File.dll");
            act.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void AnalyzeAssembly_ShouldAnalyzeCoreLibrary()
        {
            ITypeAnalyzer analyzer = new TypeAnalyzer();

            // Analyze the Core library itself
            string coreDllPath = Path.Combine(
                AppContext.BaseDirectory,
                "..", "..", "..", "..", "TypeDependencies.Core", "bin", "Debug", "net8.0", "TypeDependencies.Core.dll");
            coreDllPath = Path.GetFullPath(coreDllPath);

            File.Exists(coreDllPath).Should().BeTrue($"Core library DLL should exist at: {coreDllPath}");

            DependencyGraph graph = analyzer.AnalyzeAssembly(coreDllPath);

            graph.Should().NotBeNull();
            graph.Dependencies.Count.Should().BeGreaterThan(0);

            // Find TypeAnalyzer in the graph (might be with or without namespace prefix)
            KeyValuePair<string, HashSet<string>>? typeAnalyzerEntry = graph.Dependencies
                .FirstOrDefault(kvp => kvp.Key.Contains("TypeAnalyzer") && !kvp.Key.Contains("ITypeAnalyzer"));
            typeAnalyzerEntry.Should().NotBe(default(KeyValuePair<string, HashSet<string>>), 
                "TypeAnalyzer should be found in the dependency graph");

            // TypeAnalyzer should depend on ITypeAnalyzer interface
            typeAnalyzerEntry.Value.Value.Should().Contain(dep => dep.Contains("ITypeAnalyzer"),
                "TypeAnalyzer should depend on ITypeAnalyzer interface");

            // TypeAnalyzer should depend on DependencyGraph
            typeAnalyzerEntry.Value.Value.Should().Contain(dep => dep.Contains("DependencyGraph"),
                "TypeAnalyzer should depend on DependencyGraph");

            // TypeAnalyzer uses Mono.Cecil types (AssemblyDefinition, TypeDefinition, etc.)
            typeAnalyzerEntry.Value.Value.Should().Contain(dep => dep.Contains("Mono.Cecil"),
                "TypeAnalyzer should depend on Mono.Cecil types");

            // Find JsonExportStrategy in the graph
            KeyValuePair<string, HashSet<string>>? jsonExportEntry = graph.Dependencies
                .FirstOrDefault(kvp => kvp.Key.Contains("JsonExportStrategy"));
            if (jsonExportEntry.Value.Key != null)
            {
                // JsonExportStrategy should depend on IExportStrategy interface
                jsonExportEntry.Value.Value.Should().Contain(dep => dep.Contains("IExportStrategy"),
                    "JsonExportStrategy should depend on IExportStrategy interface");
                // JsonExportStrategy should depend on DependencyGraph
                jsonExportEntry.Value.Value.Should().Contain(dep => dep.Contains("DependencyGraph"),
                    "JsonExportStrategy should depend on DependencyGraph");
            }

            // Find DotExportStrategy in the graph
            KeyValuePair<string, HashSet<string>>? dotExportEntry = graph.Dependencies
                .FirstOrDefault(kvp => kvp.Key.Contains("DotExportStrategy"));
            if (dotExportEntry.Value.Key != null)
            {
                // DotExportStrategy should depend on IExportStrategy interface
                dotExportEntry.Value.Value.Should().Contain(dep => dep.Contains("IExportStrategy"),
                    "DotExportStrategy should depend on IExportStrategy interface");
                // DotExportStrategy should depend on DependencyGraph
                dotExportEntry.Value.Value.Should().Contain(dep => dep.Contains("DependencyGraph"),
                    "DotExportStrategy should depend on DependencyGraph");
            }

            // Find AnalysisStateManager in the graph
            KeyValuePair<string, HashSet<string>>? stateManagerEntry = graph.Dependencies
                .FirstOrDefault(kvp => kvp.Key.Contains("AnalysisStateManager"));
            if (stateManagerEntry.Value.Key != null)
            {
                // AnalysisStateManager should depend on IAnalysisStateManager interface
                stateManagerEntry.Value.Value.Should().Contain(dep => dep.Contains("IAnalysisStateManager"),
                    "AnalysisStateManager should depend on IAnalysisStateManager interface");
                // AnalysisStateManager should depend on AnalysisState
                stateManagerEntry.Value.Value.Should().Contain(dep => dep.Contains("AnalysisState"),
                    "AnalysisStateManager should depend on AnalysisState");
            }
        }
    }
}

