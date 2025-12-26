using TypeDependencies.Core.Models;

namespace TypeDependencies.Core.Export
{
    public class DotExportStrategy : IExportStrategy
    {
        public void Export(DependencyGraph dependencyGraph, string outputPath)
        {
            if (dependencyGraph == null)
                throw new ArgumentNullException(nameof(dependencyGraph));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));

            using StreamWriter writer = new StreamWriter(outputPath);

            writer.WriteLine("digraph TypeDependencies {");

            foreach (KeyValuePair<string, HashSet<string>> entry in dependencyGraph.Dependencies)
            {
                string typeName = EscapeDotString(entry.Key);

                foreach (string dependency in entry.Value)
                {
                    string dependencyName = EscapeDotString(dependency);
                    writer.WriteLine($"  \"{typeName}\" -> \"{dependencyName}\";");
                }
            }

            writer.WriteLine("}");
        }

        private string EscapeDotString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Replace("\\", "\\\\")
                       .Replace("\"", "\\\"")
                       .Replace("\n", "\\n")
                       .Replace("\r", "\\r");
        }
    }
}

