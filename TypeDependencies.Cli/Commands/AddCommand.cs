using System.CommandLine;
using TypeDependencies.Core.State;

namespace TypeDependencies.Cli.Commands
{
    public class AddCommand
    {
        private readonly IAnalysisStateManager _stateManager;
        private readonly ICurrentSessionFinder _sessionFinder;

        public AddCommand(
            IAnalysisStateManager stateManager,
            ICurrentSessionFinder sessionFinder)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
            _sessionFinder = sessionFinder ?? throw new ArgumentNullException(nameof(sessionFinder));
        }

        public static Command Create(
            IAnalysisStateManager stateManager,
            ICurrentSessionFinder sessionFinder)
        {
            Argument<string> dllPathArgument = new Argument<string>("dll-path")
            {
                Description = "Path to the DLL file to add",
            };

            Command command = new Command("add", "Add a DLL to the current analysis session");
            command.Arguments.Add(dllPathArgument);

            command.SetAction((parseResult, cancellationToken) =>
            {
                AddCommand handler = new AddCommand(stateManager, sessionFinder);
                return handler.HandleAsync(parseResult, cancellationToken);
            });

            return command;
        }

        private Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            Argument<string> dllPathArgument = parseResult.CommandResult.Command.Arguments.OfType<Argument<string>>().FirstOrDefault()
                ?? throw new InvalidOperationException("DLL path argument not found");

            string? dllPath = parseResult.GetValue(dllPathArgument);

            if (string.IsNullOrWhiteSpace(dllPath))
            {
                Console.Error.WriteLine("Error: DLL path cannot be empty.");
                return Task.FromResult(1);
            }

            if (!File.Exists(dllPath))
            {
                Console.Error.WriteLine($"Error: DLL file not found: {dllPath}");
                return Task.FromResult(1);
            }

            // Try to find the current session ID from environment or state files
            // For now, we'll use a simple approach: look for the most recent session file
            string? sessionId = _sessionFinder.FindCurrentSessionId();

            if (sessionId == null)
            {
                Console.Error.WriteLine("Error: No active session found. Please run 'type-dep init' first.");
                return Task.FromResult(1);
            }

            try
            {
                _stateManager.AddDllPath(sessionId, Path.GetFullPath(dllPath));
                Console.WriteLine($"Added DLL: {dllPath}");
                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error adding DLL: {ex.Message}");
                return Task.FromResult(1);
            }
        }
    }
}

