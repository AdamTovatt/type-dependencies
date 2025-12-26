using System.CommandLine;
using TypeDependencies.Core.State;

namespace TypeDependencies.Cli.Commands
{
    public class InitCommand
    {
        private readonly IAnalysisStateManager _stateManager;

        public InitCommand(IAnalysisStateManager stateManager)
        {
            _stateManager = stateManager ?? throw new ArgumentNullException(nameof(stateManager));
        }

        public static Command Create(IAnalysisStateManager stateManager)
        {
            Command command = new Command("init", "Initialize a new analysis session");

            command.SetAction((parseResult, cancellationToken) =>
            {
                InitCommand handler = new InitCommand(stateManager);
                return handler.HandleAsync(parseResult, cancellationToken);
            });

            return command;
        }

        private Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            string sessionId = _stateManager.InitializeSession();
            Console.WriteLine($"Session initialized: {sessionId}");
            return Task.FromResult(0);
        }
    }
}

