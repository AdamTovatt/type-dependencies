namespace TypeDependencies.Core.Query
{
    public interface IDependencyGraphQueryExecutor
    {
        int GetDependentCount(string typeName);
        HashSet<string> GetTypesWithNoDependents();
        HashSet<string> GetDependentsOf(string typeName);
        IReadOnlySet<string>? GetDependenciesOf(string typeName);
        HashSet<string> GetTypesWithDependentCount(int count);
        HashSet<string> GetTypesWithDependentCountGreaterThan(int min);
        HashSet<string> GetTypesWithDependentCountGreaterThanOrEqual(int min);
        HashSet<string> GetTypesWithDependentCountLessThan(int max);
        HashSet<string> GetTypesWithDependentCountLessThanOrEqual(int max);
        HashSet<string> GetTypesWithDependentCountRange(int min, int max);
        HashSet<string> GetTypesWithDependencyCount(int count);
        HashSet<string> GetTypesWithDependencyCountGreaterThan(int min);
        HashSet<string> GetTypesWithDependencyCountGreaterThanOrEqual(int min);
        HashSet<string> GetTypesWithDependencyCountLessThan(int max);
        HashSet<string> GetTypesWithDependencyCountLessThanOrEqual(int max);
        HashSet<string> GetTypesWithDependencyCountRange(int min, int max);
        HashSet<string> GetTransitiveDependenciesOf(string typeName);
        HashSet<string> GetTransitiveDependentsOf(string typeName);
        List<List<string>> GetCircularDependencies();
    }
}

