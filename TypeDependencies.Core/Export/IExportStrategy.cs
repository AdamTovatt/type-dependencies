using TypeDependencies.Core.Models;

namespace TypeDependencies.Core.Export
{
    public interface IExportStrategy
    {
        void Export(DependencyGraph dependencyGraph, string outputPath);
    }
}

