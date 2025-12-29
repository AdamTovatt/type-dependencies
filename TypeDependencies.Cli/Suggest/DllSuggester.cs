using TypeDependencies.Cli.Models;

namespace TypeDependencies.Cli.Suggest
{
    public class DllSuggester : IDllSuggester
    {
        public IReadOnlyList<DllSuggestion> SuggestDlls(string searchDirectory)
        {
            if (string.IsNullOrWhiteSpace(searchDirectory))
                throw new ArgumentException("Search directory cannot be null or empty.", nameof(searchDirectory));

            if (!Directory.Exists(searchDirectory))
                throw new DirectoryNotFoundException($"Directory not found: {searchDirectory}");

            List<DllSuggestion> suggestions = new List<DllSuggestion>();

            // Find all .csproj files recursively
            string[] csprojFiles = Directory.GetFiles(searchDirectory, "*.csproj", SearchOption.AllDirectories);

            foreach (string csprojFile in csprojFiles)
            {
                // Extract project name (filename without .csproj extension)
                string projectName = Path.GetFileNameWithoutExtension(csprojFile);

                // Find matching .dll files recursively
                string dllPattern = $"{projectName}.dll";
                string[] dllFiles = Directory.GetFiles(searchDirectory, dllPattern, SearchOption.AllDirectories);

                foreach (string dllFile in dllFiles)
                {
                    string fullDllPath = Path.GetFullPath(dllFile);
                    suggestions.Add(new DllSuggestion(projectName, fullDllPath));
                }
            }

            return suggestions;
        }
    }
}

