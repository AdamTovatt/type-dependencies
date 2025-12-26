using System.Net;
using System.Text.Json;
using TypeDependencies.Core.Models;
using EasyReasy;

namespace TypeDependencies.Core.Export
{
    public class HtmlExportStrategy : IExportStrategy
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private ResourceManager? _resourceManager;

        public HtmlExportStrategy()
        {
            _jsonSerializerOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
            };
        }

        public void Export(DependencyGraph dependencyGraph, string outputPath)
        {
            if (dependencyGraph == null)
                throw new ArgumentNullException(nameof(dependencyGraph));

            if (string.IsNullOrWhiteSpace(outputPath))
                throw new ArgumentException("Output path cannot be null or empty.", nameof(outputPath));

            // Initialize ResourceManager if not already done
            if (_resourceManager == null)
            {
                _resourceManager = ResourceManager.CreateInstanceAsync().GetAwaiter().GetResult();
            }

            // Read the HTML template
            string template = _resourceManager.ReadAsStringAsync(Resources.HtmlTemplate).GetAwaiter().GetResult();

            // Convert dependency graph to template data format
            HtmlTemplateDataConverter converter = new HtmlTemplateDataConverter();
            TemplateData templateData = converter.Convert(dependencyGraph);

            // Serialize to JSON with indentation (as shown in the working example)
            JsonSerializerOptions indentedOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
            string jsonData = JsonSerializer.Serialize(templateData, indentedOptions);

            // Replace tokens in template
            string html = template
                .Replace("{#SOURCE_TOKEN#}", jsonData)
                .Replace("{#TITLE_TOKEN#}", WebUtility.HtmlEncode("Type Dependencies"));

            // Write to file
            File.WriteAllText(outputPath, html);
        }
    }
}

