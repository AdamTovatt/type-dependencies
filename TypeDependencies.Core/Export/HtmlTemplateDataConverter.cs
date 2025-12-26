using TypeDependencies.Core.Models;

namespace TypeDependencies.Core.Export
{
    public class HtmlTemplateDataConverter
    {
        public TemplateData Convert(DependencyGraph dependencyGraph)
        {
            if (dependencyGraph == null)
                throw new ArgumentNullException(nameof(dependencyGraph));

            // Collect all unique types (nodes)
            HashSet<string> allTypes = new HashSet<string>();
            foreach (KeyValuePair<string, HashSet<string>> entry in dependencyGraph.Dependencies)
            {
                allTypes.Add(entry.Key);
                foreach (string dependency in entry.Value)
                {
                    allTypes.Add(dependency);
                }
            }

            // Create projects (treating all types as "projects")
            List<Project> projects = new List<Project>();
            Dictionary<string, string> typeToIdMap = new Dictionary<string, string>();

            foreach (string typeName in allTypes.OrderBy(x => x))
            {
                string id = typeName; // Use type name as ID (string, not numeric)
                typeToIdMap[typeName] = id;
                projects.Add(new Project
                {
                    Id = id,
                    Name = typeName
                });
            }

            // Create references (edges) from dependencies
            List<Reference> references = new List<Reference>();
            foreach (KeyValuePair<string, HashSet<string>> entry in dependencyGraph.Dependencies)
            {
                string fromId = typeToIdMap[entry.Key];
                foreach (string dependency in entry.Value)
                {
                    if (typeToIdMap.TryGetValue(dependency, out string? toId))
                    {
                        references.Add(new Reference
                        {
                            From = fromId,
                            To = toId
                        });
                    }
                }
            }

            return new TemplateData
            {
                Projects = projects,
                Packages = new List<Package>(), // Empty packages array
                References = references
            };
        }
    }

    public class TemplateData
    {
        public List<Project> Projects { get; set; } = new List<Project>();
        public List<Package> Packages { get; set; } = new List<Package>();
        public List<Reference> References { get; set; } = new List<Reference>();
    }

    public class Project
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class Package
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    public class Reference
    {
        public string From { get; set; } = string.Empty;
        public string To { get; set; } = string.Empty;
    }
}

