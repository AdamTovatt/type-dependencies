namespace TypeDependencies.Core.Models
{
    public class TypeDependency
{
    public string TypeName { get; }
    public HashSet<string> Dependencies { get; }

    public TypeDependency(string typeName)
    {
        TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
        Dependencies = new HashSet<string>();
    }

    public void AddDependency(string dependencyTypeName)
    {
        if (!string.IsNullOrWhiteSpace(dependencyTypeName))
        {
            Dependencies.Add(dependencyTypeName);
        }
    }

    public void AddDependencies(IEnumerable<string> dependencyTypeNames)
    {
        foreach (string dependencyTypeName in dependencyTypeNames)
        {
            AddDependency(dependencyTypeName);
        }
    }
    }
}

