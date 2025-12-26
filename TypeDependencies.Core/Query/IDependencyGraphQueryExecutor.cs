namespace TypeDependencies.Core.Query
{
    public interface IDependencyGraphQueryExecutor
    {
        int GetDependentCount(string typeName);
        HashSet<string> GetTypesWithNoDependents();
    }
}

