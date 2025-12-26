using System.Text.Json;
using TypeDependencies.Core.Models;

namespace TypeDependencies.Core.Export
{
    public class JsonExportStrategy : IExportStrategy
    {
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonExportStrategy()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        public void Export(DependencyGraph dependencyGraph, string outputPath)
        {
            if (dependencyGraph == null)
                throw new ArgumentNullException(nameof(dependencyGraph));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));

            Dictionary<string, List<string>> exportData = new Dictionary<string, List<string>>();

            foreach (KeyValuePair<string, HashSet<string>> entry in dependencyGraph.Dependencies)
            {
                exportData[entry.Key] = entry.Value.OrderBy(x => x).ToList();
            }

            string json = JsonSerializer.Serialize(exportData, _jsonOptions);
            File.WriteAllText(outputPath, json);
        }
    }
}

