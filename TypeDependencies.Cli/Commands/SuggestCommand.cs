using System.CommandLine;
using TypeDependencies.Cli.Models;
using TypeDependencies.Cli.Suggest;

namespace TypeDependencies.Cli.Commands
{
    public class SuggestCommand
    {
        private readonly IDllSuggester _dllSuggester;

        public SuggestCommand(IDllSuggester dllSuggester)
        {
            _dllSuggester = dllSuggester ?? throw new ArgumentNullException(nameof(dllSuggester));
        }

        public static Command Create(IDllSuggester dllSuggester)
        {
            Option<string> directoryOption = new Option<string>(
                name: "directory",
                aliases: new[] { "--directory", "-d" })
            {
                Description = "Directory to search for .csproj files (defaults to current directory)",
            };

            Command command = new Command("suggest", "Suggest DLL files based on .csproj files found in the directory tree");
            command.Options.Add(directoryOption);

            command.SetAction((parseResult, cancellationToken) =>
            {
                SuggestCommand handler = new SuggestCommand(dllSuggester);
                return handler.HandleAsync(parseResult, cancellationToken);
            });

            return command;
        }

        private Task<int> HandleAsync(ParseResult parseResult, CancellationToken cancellationToken)
        {
            Option<string>? directoryOption = parseResult.CommandResult.Command.Options.OfType<Option<string>>().FirstOrDefault(o => o.Name == "directory");

            if (directoryOption == null)
            {
                Console.Error.WriteLine("Error: Internal error - command option not properly configured.");
                return Task.FromResult(1);
            }

            string? directory = parseResult.GetValue(directoryOption);
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = Directory.GetCurrentDirectory();
            }

            if (!Directory.Exists(directory))
            {
                Console.Error.WriteLine($"Error: Directory not found: {directory}");
                return Task.FromResult(1);
            }

            try
            {
                IReadOnlyList<DllSuggestion> suggestions = _dllSuggester.SuggestDlls(directory);

                if (suggestions.Count == 0)
                {
                    Console.WriteLine("No DLL files found matching .csproj files in the specified directory.");
                    return Task.FromResult(0);
                }

                foreach (DllSuggestion suggestion in suggestions)
                {
                    Console.WriteLine($"{suggestion.ProjectName} -> {suggestion.DllPath}");
                }

                return Task.FromResult(0);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error suggesting DLL files: {ex.Message}");
                return Task.FromResult(1);
            }
        }
    }
}

