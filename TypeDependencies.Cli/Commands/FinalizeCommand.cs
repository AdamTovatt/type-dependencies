using System.CommandLine;
using TypeDependencies.Core.Analysis;
using TypeDependencies.Core.Export;
using TypeDependencies.Core.Models;
using TypeDependencies.Core.State;

namespace TypeDependencies.Cli.Commands
{
    public class FinalizeCommand
    {
        private readonly IAnalysisStateManager _stateManager;
        private readonly ITypeAnalyzer _typeAnalyzer;
        private readonly IExportStrategy _defaultExportStrategy;

        public FinalizeCommand(
            IAnalysisStateManager stateManager,
            ITypeAnalyzer typeAnalyzer,
            IExportStrategy defaultExportStrategy)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _typeAnalyzer = typeAnalyzer ?? throw new ArgumentNullException(nameof(typeAnalyzer));
            _defaultExportStrategy = defaultExportStrategy ?? throw new ArgumentNullException(nameof(defaultExportStrategy));
        }

        public static Command Create(
            IAnalysisStateManager stateManager,
            ITypeAnalyzer typeAnalyzer,
            IExportStrategy defaultExportStrategy)
        {
            Option<string> formatOption = new Option<string>(
                name: "format",
                aliases: new[] { "--format", "-f" })
            {
                Description = "Output format (dot, json, mermaid, or html)",
            };

            Option<string> outputOption = new Option<string>(
                name: "output",
                aliases: new[] { "--output", "-o" })
            {
                Description = "Output file path",
            };

            Command command = new Command("finalize", "Analyze all DLLs and export the dependency graph");
            command.Options.Add(formatOption);
            command.Options.Add(outputOption);

            command.SetAction((parseResult, cancellationToken) =>
            {
                FinalizeCommand handler = new FinalizeCommand(stateManager, typeAnalyzer, defaultExportStrategy);
                return handler.HandleAsync(parseResult, cancellationToken);
            });

            return command;
        }

        private Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            Option<string>? formatOption = parseResult.CommandResult.Command.Options.OfType<Option<string>>().FirstOrDefault(o => o.Name == "format");
            Option<string>? outputOption = parseResult.CommandResult.Command.Options.OfType<Option<string>>().FirstOrDefault(o => o.Name == "output");

            if (formatOption == null || outputOption == null)
            {
                Console.Error.WriteLine("Error: Internal error - command options not properly configured.");
                return Task.FromResult(1);
            }

            string? format = parseResult.GetValue(formatOption);
            string? outputPath = parseResult.GetValue(outputOption);

            format ??= "dot";

            // Find current session
            string? sessionId = FindCurrentSessionId();
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

            // Determine output path
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                string extension = format.ToLowerInvariant() switch
                {
                    "json" => "json",
                    "mermaid" => "mmd",
                    "html" => "html",
                    _ => "dot"
                };
                outputPath = Path.Combine(Directory.GetCurrentDirectory(), $"type-dependencies.{extension}");
            }

            // Select export strategy
            IExportStrategy exportStrategy = format.ToLowerInvariant() switch
            {
                "json" => new JsonExportStrategy(),
                "mermaid" => new MermaidExportStrategy(),
                "html" => new HtmlExportStrategy(),
                _ => _defaultExportStrategy
            };

            try
            {
                exportStrategy.Export(combinedGraph, outputPath);
                Console.WriteLine($"Dependency graph exported to: {outputPath}");

                // Clean up session
                _stateManager.ClearSession(sessionId);

                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error exporting dependency graph: {ex.Message}");
                return Task.FromResult(1);
            }
        }

        private string? FindCurrentSessionId()
        {
            string tempDirectory = Path.GetTempPath();
            string[] stateFiles = Directory.GetFiles(tempDirectory, "typedep-*.json");

            if (stateFiles.Length == 0)
                return null;

            // Get the most recently modified file
            FileInfo? mostRecent = stateFiles
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();

            if (mostRecent == null)
                return null;

            // Extract session ID from filename: typedep-{guid}.json
            string fileName = Path.GetFileNameWithoutExtension(mostRecent.Name);
            if (fileName.StartsWith("typedep-", StringComparison.OrdinalIgnoreCase))
            {
                return fileName.Substring("typedep-".Length);
            }

            return null;
        }
    }
}

