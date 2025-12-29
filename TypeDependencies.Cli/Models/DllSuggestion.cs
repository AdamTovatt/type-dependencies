namespace TypeDependencies.Cli.Models
{
    public class DllSuggestion
    {
        public string ProjectName { get; }
        public string DllPath { get; }

        public DllSuggestion(string projectName, string dllPath)
        {
            ProjectName = projectName ?? throw new ArgumentNullException(nameof(projectName));
            DllPath = dllPath ?? throw new ArgumentNullException(nameof(dllPath));
        }
    }
}

