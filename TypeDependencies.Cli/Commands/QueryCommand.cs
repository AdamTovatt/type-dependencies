using System.CommandLine;
using TypeDependencies.Core.Models;
using TypeDependencies.Core.Query;
using TypeDependencies.Core.State;

namespace TypeDependencies.Cli.Commands
{
    public class QueryCommand
    {
        private readonly IAnalysisStateManager _stateManager;
        private readonly ICurrentSessionFinder _sessionFinder;

        public QueryCommand(
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
            Command queryCommand = new Command("query", "Query the generated dependency graph");

            // Subcommand: dependents-of
            Argument<string> dependentsOfArgument = new Argument<string>("type-name")
            {
                Description = "Type name to find dependents for",
            };
            Command dependentsOfCommand = new Command("dependents-of", "Find all types that depend on the specified type");
            dependentsOfCommand.Arguments.Add(dependentsOfArgument);
            dependentsOfCommand.SetAction((parseResult, cancellationToken) =>
            {
                QueryCommand handler = new QueryCommand(stateManager, sessionFinder);
                return handler.HandleDependentsOfAsync(parseResult, cancellationToken);
            });

            // Subcommand: dependencies-of
            Argument<string> dependenciesOfArgument = new Argument<string>("type-name")
            {
                Description = "Type name to find dependencies for",
            };
            Command dependenciesOfCommand = new Command("dependencies-of", "Find all types that the specified type depends on");
            dependenciesOfCommand.Arguments.Add(dependenciesOfArgument);
            dependenciesOfCommand.SetAction((parseResult, cancellationToken) =>
            {
                QueryCommand handler = new QueryCommand(stateManager, sessionFinder);
                return handler.HandleDependenciesOfAsync(parseResult, cancellationToken);
            });

            // Subcommand: dependents
            Argument<string> dependentsArgument = new Argument<string>("count-expression")
            {
                Description = "Count expression (e.g., 5, >5, >=5, <5, <=5, 2-10)",
            };
            Command dependentsCommand = new Command("dependents", "Find types with a specific dependent count");
            dependentsCommand.Arguments.Add(dependentsArgument);
            dependentsCommand.SetAction((parseResult, cancellationToken) =>
            {
                QueryCommand handler = new QueryCommand(stateManager, sessionFinder);
                return handler.HandleDependentsAsync(parseResult, cancellationToken);
            });

            queryCommand.Subcommands.Add(dependentsOfCommand);
            queryCommand.Subcommands.Add(dependenciesOfCommand);
            queryCommand.Subcommands.Add(dependentsCommand);

            return queryCommand;
        }

        private Task<int> HandleDependentsOfAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            Argument<string>? argument = parseResult.CommandResult.Command.Arguments.OfType<Argument<string>>().FirstOrDefault();
            if (argument == null)
            {
                Console.Error.WriteLine("Error: Internal error - command argument not properly configured.");
                return Task.FromResult(1);
            }

            string? typeName = parseResult.GetValue(argument);
            if (string.IsNullOrWhiteSpace(typeName))
            {
                Console.Error.WriteLine("Error: Type name cannot be empty.");
                return Task.FromResult(1);
            }

            DependencyGraph? graph = LoadGraph();
            if (graph == null)
                return Task.FromResult(1);

            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);
            HashSet<string> dependents = executor.GetDependentsOf(typeName);

            if (dependents.Count == 0)
            {
                Console.WriteLine($"No types depend on '{typeName}'.");
                return Task.FromResult(0);
            }

            foreach (string dependent in dependents.OrderBy(x => x))
            {
                Console.WriteLine(dependent);
            }

            return Task.FromResult(0);
        }

        private Task<int> HandleDependenciesOfAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            Argument<string>? argument = parseResult.CommandResult.Command.Arguments.OfType<Argument<string>>().FirstOrDefault();
            if (argument == null)
            {
                Console.Error.WriteLine("Error: Internal error - command argument not properly configured.");
                return Task.FromResult(1);
            }

            string? typeName = parseResult.GetValue(argument);
            if (string.IsNullOrWhiteSpace(typeName))
            {
                Console.Error.WriteLine("Error: Type name cannot be empty.");
                return Task.FromResult(1);
            }

            DependencyGraph? graph = LoadGraph();
            if (graph == null)
                return Task.FromResult(1);

            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);
            IReadOnlySet<string>? dependencies = executor.GetDependenciesOf(typeName);

            if (dependencies == null)
            {
                Console.Error.WriteLine($"Error: Type '{typeName}' not found in the dependency graph.");
                return Task.FromResult(1);
            }

            if (dependencies.Count == 0)
            {
                Console.WriteLine($"Type '{typeName}' has no dependencies.");
                return Task.FromResult(0);
            }

            foreach (string dependency in dependencies.OrderBy(x => x))
            {
                Console.WriteLine(dependency);
            }

            return Task.FromResult(0);
        }

        private Task<int> HandleDependentsAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            Argument<string>? argument = parseResult.CommandResult.Command.Arguments.OfType<Argument<string>>().FirstOrDefault();
            if (argument == null)
            {
                Console.Error.WriteLine("Error: Internal error - command argument not properly configured.");
                return Task.FromResult(1);
            }

            string? countExpression = parseResult.GetValue(argument);
            if (string.IsNullOrWhiteSpace(countExpression))
            {
                Console.Error.WriteLine("Error: Count expression cannot be empty.");
                return Task.FromResult(1);
            }

            DependencyGraph? graph = LoadGraph();
            if (graph == null)
                return Task.FromResult(1);

            IDependencyGraphQueryExecutor executor = new DependencyGraphQueryExecutor(graph);
            HashSet<string> result = ParseAndExecuteDependentsQuery(executor, countExpression);

            if (result == null)
            {
                Console.Error.WriteLine($"Error: Invalid count expression '{countExpression}'. Expected format: number, >number, >=number, <number, <=number, or min-max.");
                return Task.FromResult(1);
            }

            if (result.Count == 0)
            {
                Console.WriteLine("No types match the specified criteria.");
                return Task.FromResult(0);
            }

            foreach (string typeName in result.OrderBy(x => x))
            {
                Console.WriteLine(typeName);
            }

            return Task.FromResult(0);
        }

        private HashSet<string>? ParseAndExecuteDependentsQuery(IDependencyGraphQueryExecutor executor, string expression)
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

        private DependencyGraph? LoadGraph()
        {
            string? sessionId = _sessionFinder.FindCurrentSessionId();
            if (sessionId == null)
            {
                Console.Error.WriteLine("Error: No active session found. Please run 'type-dep init' first.");
                return null;
            }

            DependencyGraph? graph = _stateManager.GetGeneratedGraph(sessionId);
            if (graph == null)
            {
                Console.Error.WriteLine("Error: No generated graph found. Please run 'type-dep generate' first.");
                return null;
            }

            return graph;
        }
    }
}

