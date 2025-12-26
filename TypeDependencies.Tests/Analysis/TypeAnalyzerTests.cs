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
                "..", "..", "..", "..", "..", "TypeDependencies.Core", "bin", "Debug", "net8.0", "TypeDependencies.Core.dll");
            coreDllPath = Path.GetFullPath(coreDllPath);

            if (File.Exists(coreDllPath))
            {
                DependencyGraph graph = analyzer.AnalyzeAssembly(coreDllPath);

                graph.Should().NotBeNull();
                // Should have at least some dependencies
                graph.Dependencies.Count.Should().BeGreaterThan(0);
            }
        }
    }
}

