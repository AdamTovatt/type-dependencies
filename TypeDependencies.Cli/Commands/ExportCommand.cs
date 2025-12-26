using System.CommandLine;
using TypeDependencies.Core.Export;
using TypeDependencies.Core.Models;
using TypeDependencies.Core.State;

namespace TypeDependencies.Cli.Commands
{
    public class ExportCommand
    {
        private readonly IAnalysisStateManager _stateManager;
        private readonly IExportStrategy _defaultExportStrategy;
        private readonly ICurrentSessionFinder _sessionFinder;

        public ExportCommand(
            IAnalysisStateManager stateManager,
            IExportStrategy defaultExportStrategy,
            ICurrentSessionFinder sessionFinder)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _defaultExportStrategy = defaultExportStrategy ?? throw new ArgumentNullException(nameof(defaultExportStrategy));
            _sessionFinder = sessionFinder ?? throw new ArgumentNullException(nameof(sessionFinder));
        }

        public static Command Create(
            IAnalysisStateManager stateManager,
            IExportStrategy defaultExportStrategy,
            ICurrentSessionFinder sessionFinder)
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

            Command command = new Command("export", "Export the generated dependency graph");
            command.Options.Add(formatOption);
            command.Options.Add(outputOption);

            command.SetAction((parseResult, cancellationToken) =>
            {
                ExportCommand handler = new ExportCommand(stateManager, defaultExportStrategy, sessionFinder);
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
            string? sessionId = _sessionFinder.FindCurrentSessionId();
            if (sessionId == null)
            {
                Console.Error.WriteLine("Error: No active session found. Please run 'type-dep init' first.");
                return Task.FromResult(1);
            }

            // Load generated graph
            DependencyGraph? graph = _stateManager.GetGeneratedGraph(sessionId);
            if (graph == null)
            {
                Console.Error.WriteLine("Error: No generated graph found. Please run 'type-dep generate' first.");
                return Task.FromResult(1);
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
                exportStrategy.Export(graph, outputPath);
                Console.WriteLine($"Dependency graph exported to: {outputPath}");
                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error exporting dependency graph: {ex.Message}");
                return Task.FromResult(1);
            }
        }
    }
}

