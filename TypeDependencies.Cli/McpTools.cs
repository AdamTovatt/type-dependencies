using System.ComponentModel;
using ModelContextProtocol.Server;
using TypeDependencies.Core.Analysis;
using TypeDependencies.Core.Export;
using TypeDependencies.Core.Models;
using TypeDependencies.Core.Query;
using TypeDependencies.Core.State;

namespace TypeDependencies.Cli
{
    /// <summary>
    /// MCP tools for type dependency analysis operations.
    /// </summary>
    [McpServerToolType]
    public class McpTools
    {
        private readonly IAnalysisStateManager _stateManager;
        private readonly ITypeAnalyzer _typeAnalyzer;
        private readonly IExportStrategy _defaultExportStrategy;
        private readonly ICurrentSessionFinder _sessionFinder;

        /// <summary>
        /// Initializes a new instance of the <see cref="McpTools"/> class.
        /// </summary>
        public McpTools(
            IAnalysisStateManager stateManager,
            ITypeAnalyzer typeAnalyzer,
            IExportStrategy defaultExportStrategy,
            ICurrentSessionFinder sessionFinder)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _typeAnalyzer = typeAnalyzer ?? throw new ArgumentNullException(nameof(typeAnalyzer));
            _defaultExportStrategy = defaultExportStrategy ?? throw new ArgumentNullException(nameof(defaultExportStrategy));
            _sessionFinder = sessionFinder ?? throw new ArgumentNullException(nameof(sessionFinder));
        }

        /// <summary>
        /// Initialize a new analysis session.
        /// </summary>
        [McpServerTool(Name = "td_init")]
        [Description("Initialize a new analysis session")]
        public Task<string> InitializeSessionAsync(CancellationToken cancellationToken)
        {
            string sessionId = _stateManager.InitializeSession();
            return Task.FromResult($"Session initialized: {sessionId}");
        }

        /// <summary>
        /// Add a DLL to the current analysis session.
        /// </summary>
        [McpServerTool(Name = "td_add")]
        [Description("Add a DLL to the current analysis session")]
        public Task<string> AddDllAsync(
            [Description("Path to the DLL file to add")]
            string dllPath,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(dllPath))
            {
                return Task.FromResult("Error: DLL path cannot be empty.");
            }

            if (!File.Exists(dllPath))
            {
                return Task.FromResult($"Error: DLL file not found: {dllPath}");
            }

            string? sessionId = _sessionFinder.FindCurrentSessionId();
            if (sessionId == null)
            {
                return Task.FromResult("Error: No active session found. Please run td_init first.");
            }

            try
            {
                _stateManager.AddDllPath(sessionId, Path.GetFullPath(dllPath));
                return Task.FromResult($"Added DLL: {dllPath}");
            }
            catch (Exception ex)
            {
                return Task.FromResult($"Error adding DLL: {ex.Message}");
            }
        }

        /// <summary>
        /// Generate the dependency graph from added DLLs.
        /// </summary>
        [McpServerTool(Name = "td_generate")]
        [Description("Generate the dependency graph from added DLLs")]
        public Task<string> GenerateGraphAsync(CancellationToken cancellationToken)
        {
            string? sessionId = _sessionFinder.FindCurrentSessionId();
            if (sessionId == null)
            {
                return Task.FromResult("Error: No active session found. Please run td_init first.");
            }

            IReadOnlyList<string> dllPaths = _stateManager.GetDllPaths(sessionId);
            if (dllPaths.Count == 0)
            {
                return Task.FromResult("Error: No DLLs added to the session. Please run td_add first.");
            }

            // Analyze all DLLs
            DependencyGraph combinedGraph = new DependencyGraph();
            foreach (string dllPath in dllPaths)
            {
                try
                {
                    DependencyGraph graph = _typeAnalyzer.AnalyzeAssembly(dllPath);

                    // Merge graphs
                    foreach (KeyValuePair<string, HashSet<string>> entry in graph.Dependencies)
                    {
                        combinedGraph.AddDependencies(entry.Key, entry.Value);
                    }
                }
                catch (Exception ex)
                {
                    return Task.FromResult($"Error analyzing {dllPath}: {ex.Message}");
                }
            }

            try
            {
                _stateManager.SaveGeneratedGraph(sessionId, combinedGraph);
                return Task.FromResult($"Dependency graph generated successfully. Found {combinedGraph.Dependencies.Count} types.");
            }
            catch (Exception ex)
            {
                return Task.FromResult($"Error saving dependency graph: {ex.Message}");
            }
        }

        /// <summary>
        /// Export the generated dependency graph.
        /// </summary>
        [McpServerTool(Name = "td_export")]
        [Description("Export the generated dependency graph")]
        public Task<string> ExportGraphAsync(
            [Description("Output format (dot, json, or mermaid). Defaults to dot.")]
            string? format,
            [Description("Output file path. If not provided, defaults to type-dependencies.{ext} in current directory.")]
            string? outputPath,
            CancellationToken cancellationToken)
        {
            format ??= "dot";

            string? sessionId = _sessionFinder.FindCurrentSessionId();
            if (sessionId == null)
            {
                return Task.FromResult("Error: No active session found. Please run td_init first.");
            }

            DependencyGraph? graph = _stateManager.GetGeneratedGraph(sessionId);
            if (graph == null)
            {
                return Task.FromResult("Error: No generated graph found. Please run td_generate first.");
            }

            // Determine output path
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                string extension = format.ToLowerInvariant() switch
                {
                    "json" => "json",
                    "mermaid" => "mmd",
                    _ => "dot"
                };
                outputPath = Path.Combine(Directory.GetCurrentDirectory(), $"type-dependencies.{extension}");
            }

            // Select export strategy
            IExportStrategy exportStrategy = format.ToLowerInvariant() switch
            {
                "json" => new JsonExportStrategy(),
                "mermaid" => new MermaidExportStrategy(),
                _ => _defaultExportStrategy
            };

            try
            {
                exportStrategy.Export(graph, outputPath);
                return Task.FromResult($"Dependency graph exported to: {outputPath}");
            }
            catch (Exception ex)
            {
                return Task.FromResult($"Error exporting dependency graph: {ex.Message}");
            }
        }

        /// <summary>
        /// Find all types that depend on the specified type.
        /// </summary>
        [McpServerTool(Name = "td_query_dependents_of")]
        [Description("Find all types that depend on the specified type")]
        public Task<string> QueryDependentsOfAsync(
            [Description("Type name to find dependents for")]
            string typeName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return Task.FromResult("Error: Type name cannot be empty.");
            }

            DependencyGraph? graph = LoadGraph();
            if (graph == null)
            {
                return Task.FromResult("Error: No generated graph found. Please run td_generate first.");
            }

            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);
            HashSet<string> dependents = executor.GetDependentsOf(typeName);

            if (dependents.Count == 0)
            {
                return Task.FromResult($"No types depend on '{typeName}'.");
            }

            return Task.FromResult(FormatResults(dependents));
        }

        /// <summary>
        /// Find all types that the specified type depends on.
        /// </summary>
        [McpServerTool(Name = "td_query_dependencies_of")]
        [Description("Find all types that the specified type depends on")]
        public Task<string> QueryDependenciesOfAsync(
            [Description("Type name to find dependencies for")]
            string typeName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return Task.FromResult("Error: Type name cannot be empty.");
            }

            DependencyGraph? graph = LoadGraph();
            if (graph == null)
            {
                return Task.FromResult("Error: No generated graph found. Please run td_generate first.");
            }

            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);
            IReadOnlySet<string>? dependencies = executor.GetDependenciesOf(typeName);

            if (dependencies == null)
            {
                return Task.FromResult($"Error: Type '{typeName}' not found in the dependency graph.");
            }

            if (dependencies.Count == 0)
            {
                return Task.FromResult($"Type '{typeName}' has no dependencies.");
            }

            return Task.FromResult(FormatResults(new HashSet<string>(dependencies)));
        }

        /// <summary>
        /// Find types with a specific dependent count.
        /// </summary>
        [McpServerTool(Name = "td_query_dependents")]
        [Description("Find types with a specific dependent count. Supports: number, >number, >=number, <number, <=number, or min-max")]
        public Task<string> QueryDependentsAsync(
            [Description("Count expression (e.g., 5, >5, >=5, <5, <=5, 2-10)")]
            string countExpression,
            [Description("Show detailed output with additional count information")]
            bool detailed = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(countExpression))
            {
                return Task.FromResult("Error: Count expression cannot be empty.");
            }

            DependencyGraph? graph = LoadGraph();
            if (graph == null)
            {
                return Task.FromResult("Error: No generated graph found. Please run td_generate first.");
            }

            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);
            HashSet<string>? result = ParseAndExecuteDependentsQuery(executor, countExpression);

            if (result == null)
            {
                return Task.FromResult($"Error: Invalid count expression '{countExpression}'. Expected format: number, >number, >=number, <number, <=number, or min-max.");
            }

            if (result.Count == 0)
            {
                return Task.FromResult("No types match the specified criteria.");
            }

            return Task.FromResult(FormatResults(result, executor, detailed, isDependenciesQuery: false));
        }

        /// <summary>
        /// Find types with a specific dependency count.
        /// </summary>
        [McpServerTool(Name = "td_query_dependencies")]
        [Description("Find types with a specific dependency count. Supports: number, >number, >=number, <number, <=number, or min-max")]
        public Task<string> QueryDependenciesAsync(
            [Description("Count expression (e.g., 5, >5, >=5, <5, <=1, or 2-10)")]
            string countExpression,
            [Description("Show detailed output with additional count information")]
            bool detailed = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(countExpression))
            {
                return Task.FromResult("Error: Count expression cannot be empty.");
            }

            DependencyGraph? graph = LoadGraph();
            if (graph == null)
            {
                return Task.FromResult("Error: No generated graph found. Please run td_generate first.");
            }

            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);
            HashSet<string>? result = ParseAndExecuteDependenciesQuery(executor, countExpression);

            if (result == null)
            {
                return Task.FromResult($"Error: Invalid count expression '{countExpression}'. Expected format: number, >number, >=number, <number, <=number, or min-max.");
            }

            if (result.Count == 0)
            {
                return Task.FromResult("No types match the specified criteria.");
            }

            return Task.FromResult(FormatResults(result, executor, detailed, isDependenciesQuery: true));
        }

        /// <summary>
        /// Find all types that a type depends on (recursively).
        /// </summary>
        [McpServerTool(Name = "td_query_transitive_dependencies_of")]
        [Description("Find all types that a type depends on (recursively)")]
        public Task<string> QueryTransitiveDependenciesOfAsync(
            [Description("Type name to find transitive dependencies for")]
            string typeName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return Task.FromResult("Error: Type name cannot be empty.");
            }

            DependencyGraph? graph = LoadGraph();
            if (graph == null)
            {
                return Task.FromResult("Error: No generated graph found. Please run td_generate first.");
            }

            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);
            HashSet<string> transitiveDependencies = executor.GetTransitiveDependenciesOf(typeName);

            if (transitiveDependencies.Count == 0)
            {
                return Task.FromResult($"Type '{typeName}' has no transitive dependencies.");
            }

            return Task.FromResult(FormatResults(transitiveDependencies));
        }

        /// <summary>
        /// Find all types that depend on a type (recursively).
        /// </summary>
        [McpServerTool(Name = "td_query_transitive_dependents_of")]
        [Description("Find all types that depend on a type (recursively)")]
        public Task<string> QueryTransitiveDependentsOfAsync(
            [Description("Type name to find transitive dependents for")]
            string typeName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return Task.FromResult("Error: Type name cannot be empty.");
            }

            DependencyGraph? graph = LoadGraph();
            if (graph == null)
            {
                return Task.FromResult("Error: No generated graph found. Please run td_generate first.");
            }

            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);
            HashSet<string> transitiveDependents = executor.GetTransitiveDependentsOf(typeName);

            if (transitiveDependents.Count == 0)
            {
                return Task.FromResult($"No types transitively depend on '{typeName}'.");
            }

            return Task.FromResult(FormatResults(transitiveDependents));
        }

        /// <summary>
        /// Find all circular dependency cycles in the graph.
        /// </summary>
        [McpServerTool(Name = "td_query_circular_dependencies")]
        [Description("Find all circular dependency cycles in the graph")]
        public Task<string> QueryCircularDependenciesAsync(CancellationToken cancellationToken)
        {
            DependencyGraph? graph = LoadGraph();
            if (graph == null)
            {
                return Task.FromResult("Error: No generated graph found. Please run td_generate first.");
            }

            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);
            List<List<string>> cycles = executor.GetCircularDependencies();

            if (cycles.Count == 0)
            {
                return Task.FromResult("No circular dependencies found.");
            }

            return Task.FromResult(FormatCircularDependencies(cycles));
        }

        /// <summary>
        /// Get help information about the TypeDependencies tool.
        /// </summary>
        [McpServerTool(Name = "td_help")]
        [Description("Get help information about the TypeDependencies tool")]
        public Task<string> GetHelpAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(HelpText);
        }

        private static string HelpText => @"TypeDependencies - Analyze and visualize type dependencies in C# assemblies

WORKFLOW:
  1. td_init()                    - Initialize a new analysis session
  2. td_add(dllPath)              - Add DLL files to analyze
  3. td_generate()                 - Generate the dependency graph
  4. td_export(format?, output?)  - Export the graph (optional)
  5. td_query_*()                 - Query the generated graph

SESSION MANAGEMENT:
  td_init()
    Initialize a new analysis session.
    Returns: Session ID confirmation message.
    Each call creates a fresh session with a new unique ID.

DLL MANAGEMENT:
  td_add(dllPath: string)
    Add a DLL file to the current analysis session.
    Parameters:
      - dllPath: Full path to the DLL file to analyze
    Returns: Success message with DLL path or error message.
    Example: td_add(""C:\MyProject\bin\Debug\MyProject.dll"")

GRAPH GENERATION:
  td_generate()
    Generate the dependency graph from all added DLLs.
    Analyzes all DLLs in the current session and merges their dependencies.
    Returns: Success message with type count or error message.
    Must be called after adding DLLs and before querying/exporting.

EXPORT:
  td_export(format?: string, outputPath?: string)
    Export the generated dependency graph to a file.
    Parameters:
      - format: Output format - ""dot"", ""json"", or ""mermaid"" (defaults to ""dot"")
      - outputPath: Optional file path (defaults to type-dependencies.{ext} in current directory)
    Returns: Success message with file path or error message.
    Examples:
      td_export(""json"", ""dependencies.json"")
      td_export(""mermaid"")  // Uses default path: type-dependencies.mmd

QUERY TOOLS:
  td_query_dependents_of(typeName: string)
    Find all types that depend on the specified type.
    Returns: List of type names (one per line) or error message.

  td_query_dependencies_of(typeName: string)
    Find all types that the specified type depends on.
    Returns: List of type names (one per line) or error message.

  td_query_dependents(countExpression: string, detailed?: bool)
    Filter types by dependent count (how many types depend on them).
    Supports: number, >number, >=number, <number, <=number, or min-max
    Parameters:
      - countExpression: Count expression (e.g., ""0"", "">5"", ""2-10"")
      - detailed: Optional. If true, shows dependency counts and sorts by dependent count, then dependency count, then name
    Examples:
      td_query_dependents(""0"")           // Types with no dependents
      td_query_dependents("">5"")          // Types with more than 5 dependents
      td_query_dependents(""2-10"")        // Types with 2 to 10 dependents
      td_query_dependents(""0"", true)     // Types with no dependents, showing dependency counts

  td_query_dependencies(countExpression: string, detailed?: bool)
    Filter types by dependency count (how many types they depend on).
    Supports: number, >number, >=number, <number, <=number, or min-max
    Parameters:
      - countExpression: Count expression (e.g., ""0"", "">5"", ""2-10"")
      - detailed: Optional (but recommended). If true, shows dependent counts and sorts by dependency count, then dependent count, then name
    Examples:
      td_query_dependencies(""0"")        // Leaf nodes (no dependencies)
      td_query_dependencies("">10"")      // Highly coupled types
      td_query_dependencies(""0"", true)  // Leaf nodes, showing dependent counts

  td_query_transitive_dependencies_of(typeName: string)
    Find all types that a type depends on (recursively).
    Follows the entire dependency chain.
    Returns: List of all transitive dependencies.

  td_query_transitive_dependents_of(typeName: string)
    Find all types that depend on a type (recursively).
    Follows the entire dependency chain in reverse.
    Returns: List of all transitive dependents.

  td_query_circular_dependencies()
    Detect all circular dependency cycles in the graph.
    Returns: List of cycles, each shown as ""TypeA -> TypeB -> TypeC -> TypeA""
    If no cycles found, returns: ""No circular dependencies found.""

EXAMPLES:
  // Basic workflow
  td_init()
  td_add(""C:\MyProject\bin\Debug\MyProject.dll"")
  td_generate()
  td_export(""json"", ""dependencies.json"")

  // Query examples
  td_query_dependents_of(""MyNamespace.MyClass"")
  td_query_dependencies(""0"")  // Find leaf nodes
  td_query_circular_dependencies()

  // Multiple DLLs
  td_init()
  td_add(""C:\Project1\bin\Debug\Project1.dll"")
  td_add(""C:\Project2\bin\Debug\Project2.dll"")
  td_generate()

ERROR HANDLING:
  - If no session exists when running add/generate/export/query:
    ""Error: No active session found. Please run td_init first.""

  - If no DLLs added when running generate:
    ""Error: No DLLs added to the session. Please run td_add first.""

  - If no graph generated when running export/query:
    ""Error: No generated graph found. Please run td_generate first.""

  - If DLL file not found:
    ""Error: DLL file not found: [path]""

  - If type not found in query:
    ""Error: Type '[typeName]' not found in the dependency graph.""

  - If invalid count expression:
    ""Error: Invalid count expression. Expected format: number, >number, >=number, <number, <=number, or min-max.""

NOTES:
  - Sessions persist until manually cleared or a new session is created
  - After generating, you can export multiple times in different formats
  - Query results are sorted alphabetically by default
  - When using detailed mode, results are sorted by dependency count, then dependent count, then alphabetically
  - System and Microsoft namespaces are automatically filtered out
  - The tool analyzes: base types, interfaces, fields, properties, methods, attributes, and generic constraints

For more information, visit: https://github.com/AdamTovatt/type-dependencies
";

        private DependencyGraph? LoadGraph()
        {
            string? sessionId = _sessionFinder.FindCurrentSessionId();
            if (sessionId == null)
            {
                return null;
            }

            return _stateManager.GetGeneratedGraph(sessionId);
        }

        private static string FormatResults(HashSet<string> results, IDependencyGraphQueryExecutor? executor = null, bool detailed = false, bool isDependenciesQuery = false)
        {
            IEnumerable<string> filteredResult = results.Where(x => !IsAnonymousType(x));

            if (detailed && executor != null)
            {
                if (isDependenciesQuery)
                {
                    IEnumerable<string> sorted = filteredResult
                        .OrderBy(type => executor.GetDependencyCount(type))
                        .ThenBy(type => executor.GetDependentCount(type))
                        .ThenBy(type => type);
                    
                    return string.Join("\n", sorted.Select(typeName =>
                    {
                        int dependentCount = executor.GetDependentCount(typeName);
                        return $"{typeName} ({dependentCount} dependents)";
                    }));
                }
                else
                {
                    IEnumerable<string> sorted = filteredResult
                        .OrderBy(type => executor.GetDependentCount(type))
                        .ThenBy(type => executor.GetDependencyCount(type))
                        .ThenBy(type => type);
                    
                    return string.Join("\n", sorted.Select(typeName =>
                    {
                        int dependencyCount = executor.GetDependencyCount(typeName);
                        return $"{typeName} ({dependencyCount} dependencies)";
                    }));
                }
            }

            return string.Join("\n", filteredResult.OrderBy(x => x));
        }

        private static bool IsAnonymousType(string typeName)
        {
            return typeName.StartsWith("<", StringComparison.Ordinal);
        }

        private static string FormatCircularDependencies(List<List<string>> cycles)
        {
            return string.Join("\n", cycles
                .Select(cycle => cycle.Where(x => !IsAnonymousType(x)).ToList())
                .Where(filteredCycle => filteredCycle.Count > 0)
                .Select(filteredCycle => string.Join(" -> ", filteredCycle)));
        }

        private static HashSet<string>? ParseAndExecuteDependentsQuery(IDependencyGraphQueryExecutor executor, string expression)
        {
            expression = expression.Trim();

            // Range: min-max
            if (expression.Contains('-'))
            {
                string[] parts = expression.Split('-', 2);
                if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out int min) && int.TryParse(parts[1].Trim(), out int max))
                {
                    return executor.GetTypesWithDependentCountRange(min, max);
                }
                return null;
            }

            // Greater than or equal: >=number
            if (expression.StartsWith(">=", StringComparison.OrdinalIgnoreCase))
            {
                string numberPart = expression.Substring(2).Trim();
                if (int.TryParse(numberPart, out int min))
                {
                    return executor.GetTypesWithDependentCountGreaterThanOrEqual(min);
                }
                return null;
            }

            // Less than or equal: <=number
            if (expression.StartsWith("<=", StringComparison.OrdinalIgnoreCase))
            {
                string numberPart = expression.Substring(2).Trim();
                if (int.TryParse(numberPart, out int max))
                {
                    return executor.GetTypesWithDependentCountLessThanOrEqual(max);
                }
                return null;
            }

            // Greater than: >number
            if (expression.StartsWith(">", StringComparison.OrdinalIgnoreCase))
            {
                string numberPart = expression.Substring(1).Trim();
                if (int.TryParse(numberPart, out int min))
                {
                    return executor.GetTypesWithDependentCountGreaterThan(min);
                }
                return null;
            }

            // Less than: <number
            if (expression.StartsWith("<", StringComparison.OrdinalIgnoreCase))
            {
                string numberPart = expression.Substring(1).Trim();
                if (int.TryParse(numberPart, out int max))
                {
                    return executor.GetTypesWithDependentCountLessThan(max);
                }
                return null;
            }

            // Exact: number
            if (int.TryParse(expression, out int count))
            {
                return executor.GetTypesWithDependentCount(count);
            }

            return null;
        }

        private static HashSet<string>? ParseAndExecuteDependenciesQuery(IDependencyGraphQueryExecutor executor, string expression)
        {
            expression = expression.Trim();

            // Range: min-max
            if (expression.Contains('-'))
            {
                string[] parts = expression.Split('-', 2);
                if (parts.Length == 2 && int.TryParse(parts[0].Trim(), out int min) && int.TryParse(parts[1].Trim(), out int max))
                {
                    return executor.GetTypesWithDependencyCountRange(min, max);
                }
                return null;
            }

            // Greater than or equal: >=number
            if (expression.StartsWith(">=", StringComparison.OrdinalIgnoreCase))
            {
                string numberPart = expression.Substring(2).Trim();
                if (int.TryParse(numberPart, out int min))
                {
                    return executor.GetTypesWithDependencyCountGreaterThanOrEqual(min);
                }
                return null;
            }

            // Less than or equal: <=number
            if (expression.StartsWith("<=", StringComparison.OrdinalIgnoreCase))
            {
                string numberPart = expression.Substring(2).Trim();
                if (int.TryParse(numberPart, out int max))
                {
                    return executor.GetTypesWithDependencyCountLessThanOrEqual(max);
                }
                return null;
            }

            // Greater than: >number
            if (expression.StartsWith(">", StringComparison.OrdinalIgnoreCase))
            {
                string numberPart = expression.Substring(1).Trim();
                if (int.TryParse(numberPart, out int min))
                {
                    return executor.GetTypesWithDependencyCountGreaterThan(min);
                }
                return null;
            }

            // Less than: <number
            if (expression.StartsWith("<", StringComparison.OrdinalIgnoreCase))
            {
                string numberPart = expression.Substring(1).Trim();
                if (int.TryParse(numberPart, out int max))
                {
                    return executor.GetTypesWithDependencyCountLessThan(max);
                }
                return null;
            }

            // Exact: number
            if (int.TryParse(expression, out int count))
            {
                return executor.GetTypesWithDependencyCount(count);
            }

            return null;
        }
    }
}

