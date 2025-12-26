using TypeDependencies.Core.Models;

namespace TypeDependencies.Core.State
{
    public interface IAnalysisStateManager
    {
        string InitializeSession();
        void AddDllPath(string sessionId, string dllPath);
        IReadOnlyList<string> GetDllPaths(string sessionId);
        void ClearSession(string sessionId);
        bool SessionExists(string sessionId);
        void SaveGeneratedGraph(string sessionId, DependencyGraph graph);
        DependencyGraph? GetGeneratedGraph(string sessionId);
        bool HasGeneratedGraph(string sessionId);
    }
}

