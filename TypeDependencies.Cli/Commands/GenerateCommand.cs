using System.CommandLine;
using TypeDependencies.Core.Analysis;
using TypeDependencies.Core.Models;
using TypeDependencies.Core.State;

namespace TypeDependencies.Cli.Commands
{
    public class GenerateCommand
    {
        private readonly IAnalysisStateManager _stateManager;
        private readonly ITypeAnalyzer _typeAnalyzer;
        private readonly ICurrentSessionFinder _sessionFinder;

        public GenerateCommand(
            IAnalysisStateManager stateManager,
            ITypeAnalyzer typeAnalyzer,
            ICurrentSessionFinder sessionFinder)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _typeAnalyzer = typeAnalyzer ?? throw new ArgumentNullException(nameof(typeAnalyzer));
            _sessionFinder = sessionFinder ?? throw new ArgumentNullException(nameof(sessionFinder));
        }

        public static Command Create(
            IAnalysisStateManager stateManager,
            ITypeAnalyzer typeAnalyzer,
            ICurrentSessionFinder sessionFinder)
        {
            Command command = new Command("generate", "Generate the dependency graph from added DLLs");

            command.SetAction((parseResult, cancellationToken) =>
            {
                GenerateCommand handler = new GenerateCommand(stateManager, typeAnalyzer, sessionFinder);
                return handler.HandleAsync(parseResult, cancellationToken);
            });

            return command;
        }

        private Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            // Find current session
            string? sessionId = _sessionFinder.FindCurrentSessionId();
            if (sessionId == null)
            {
                Console.Error.WriteLine("Error: No active session found. Please run 'type-dep init' first.");
                return Task.FromResult(1);
            }

            IReadOnlyList<string> dllPaths = _stateManager.GetDllPaths(sessionId);
            if (dllPaths.Count == 0)
            {
                Console.Error.WriteLine("Error: No DLLs added to the session. Please run 'type-dep add <dll-path>' first.");
                return Task.FromResult(1);
            }

            // Analyze all DLLs
            DependencyGraph combinedGraph = new DependencyGraph();
            foreach (string dllPath in dllPaths)
            {
                try
                {
                    Console.WriteLine($"Analyzing: {dllPath}");
                    DependencyGraph graph = _typeAnalyzer.AnalyzeAssembly(dllPath);

                    // Merge graphs
                    foreach (KeyValuePair<string, HashSet<string>> entry in graph.Dependencies)
                    {
                        combinedGraph.AddDependencies(entry.Key, entry.Value);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error analyzing {dllPath}: {ex.Message}");
                    return Task.FromResult(1);
                }
            }

            try
            {
                _stateManager.SaveGeneratedGraph(sessionId, combinedGraph);
                Console.WriteLine($"Dependency graph generated successfully. Found {combinedGraph.Dependencies.Count} types.");
                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error saving dependency graph: {ex.Message}");
                return Task.FromResult(1);
            }
        }
    }
}

