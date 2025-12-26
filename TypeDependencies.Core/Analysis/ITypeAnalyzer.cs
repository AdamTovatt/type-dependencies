using TypeDependencies.Core.Models;

namespace TypeDependencies.Core.Analysis
{
    public interface ITypeAnalyzer
    {
        DependencyGraph AnalyzeAssembly(string assemblyPath);
    }
}

