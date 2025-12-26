using TypeDependencies.Core.Models;

namespace TypeDependencies.Core.Export
{
    public class MermaidExportStrategy : IExportStrategy
    {
        public void Export(DependencyGraph dependencyGraph, string outputPath)
        {
            if (dependencyGraph == null)
                throw new ArgumentNullException(nameof(dependencyGraph));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));

            using StreamWriter writer = new StreamWriter(outputPath);

            writer.WriteLine("```mermaid");
            writer.WriteLine("graph TD");

            foreach (KeyValuePair<string, HashSet<string>> entry in dependencyGraph.Dependencies)
            {
                string typeName = EscapeMermaidString(entry.Key);
                string typeId = GetMermaidId(typeName);

                foreach (string dependency in entry.Value)
                {
                    string dependencyName = EscapeMermaidString(dependency);
                    string dependencyId = GetMermaidId(dependencyName);
                    writer.WriteLine($"    {typeId}[\"{typeName}\"] --> {dependencyId}[\"{dependencyName}\"]");
                }
            }

            writer.WriteLine("```");
        }

        private string EscapeMermaidString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return string.Empty;

            return input.Replace("\"", "&quot;")
                       .Replace("\n", " ")
                       .Replace("\r", " ");
        }

        private string GetMermaidId(string typeName)
        {
            // Mermaid IDs must be alphanumeric or underscore, and can't start with a number
            // Replace invalid characters with underscores
            string id = typeName;
            
            // Replace spaces and special characters with underscores
            char[] invalidChars = { ' ', '-', '.', '+', '[', ']', '<', '>', '(', ')', '{', '}', ',', ':', ';', '/', '\\', '&', '*', '%', '$', '#', '@', '!', '?', '=', '|', '~', '`', '^' };
            foreach (char c in invalidChars)
            {
                id = id.Replace(c, '_');
            }

            // Ensure it doesn't start with a number
            if (id.Length > 0 && char.IsDigit(id[0]))
            {
                id = "_" + id;
            }

            // Remove consecutive underscores
            while (id.Contains("__"))
            {
                id = id.Replace("__", "_");
            }

            // Remove leading/trailing underscores
            id = id.Trim('_');

            // Ensure it's not empty
            if (string.IsNullOrEmpty(id))
            {
                id = "Type";
            }

            return id;
        }
    }
}

