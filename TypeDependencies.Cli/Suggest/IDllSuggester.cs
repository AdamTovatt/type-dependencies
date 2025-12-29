using TypeDependencies.Cli.Models;

namespace TypeDependencies.Cli.Suggest
{
    public interface IDllSuggester
    {
        IReadOnlyList<DllSuggestion> SuggestDlls(string searchDirectory);
    }
}

