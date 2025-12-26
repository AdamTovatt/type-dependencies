using System.Collections.Generic;

namespace TypeDependencies.Core.Models
{
    public class DependencyGraph
{
    private readonly Dictionary<string, HashSet<string>> _dependencies;

    public DependencyGraph()
    {
        _dependencies = new Dictionary<string, HashSet<string>>();
    }

    public IReadOnlyDictionary<string, HashSet<string>> Dependencies => _dependencies;

    public void AddDependency(string typeName, string dependencyTypeName)
    {
        if (string.IsNullOrWhiteSpace(typeName) || string.IsNullOrWhiteSpace(dependencyTypeName))
            return;

        if (!_dependencies.ContainsKey(typeName))
        {
            _dependencies[typeName] = new HashSet<string>();
        }

        _dependencies[typeName].Add(dependencyTypeName);
    }

    public void AddDependencies(string typeName, IEnumerable<string> dependencyTypeNames)
    {
        if (string.IsNullOrWhiteSpace(typeName))
            return;

        foreach (string dependencyTypeName in dependencyTypeNames)
        {
            AddDependency(typeName, dependencyTypeName);
        }
    }

    public bool ContainsType(string typeName)
    {
        return _dependencies.ContainsKey(typeName);
    }

    public IReadOnlySet<string>? GetDependencies(string typeName)
    {
        return _dependencies.TryGetValue(typeName, out HashSet<string>? dependencies) ? dependencies : null;
    }
    }
}

