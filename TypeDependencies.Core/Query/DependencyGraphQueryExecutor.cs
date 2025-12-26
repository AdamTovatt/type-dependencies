using TypeDependencies.Core.Models;

namespace TypeDependencies.Core.Query
{
    public class DependencyGraphQueryExecutor : IDependencyGraphQueryExecutor
    {
        private readonly DependencyGraph _graph;

        public DependencyGraphQueryExecutor(DependencyGraph graph)
        {
            _graph = graph ?? throw new ArgumentNullException(nameof(graph));
        }

        public int GetDependentCount(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return 0;

            int count = 0;
            foreach (KeyValuePair<string, HashSet<string>> entry in _graph.Dependencies)
            {
                if (entry.Value.Contains(typeName))
                {
                    count++;
                }
            }
            return count;
        }

        public HashSet<string> GetTypesWithNoDependents()
        {
            HashSet<string> allTypes = new HashSet<string>(_graph.Dependencies.Keys);
            HashSet<string> typesWithDependents = new HashSet<string>();

            foreach (KeyValuePair<string, HashSet<string>> entry in _graph.Dependencies)
            {
                foreach (string dependency in entry.Value)
                {
                    if (allTypes.Contains(dependency))
                    {
                        typesWithDependents.Add(dependency);
                    }
                }
            }

            allTypes.ExceptWith(typesWithDependents);
            return allTypes;
        }

        public HashSet<string> GetDependentsOf(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return new HashSet<string>();

            HashSet<string> dependents = new HashSet<string>();
            foreach (KeyValuePair<string, HashSet<string>> entry in _graph.Dependencies)
            {
                if (entry.Value.Contains(typeName))
                {
                    dependents.Add(entry.Key);
                }
            }
            return dependents;
        }

        public IReadOnlySet<string>? GetDependenciesOf(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return null;

            return _graph.GetDependencies(typeName);
        }

        public HashSet<string> GetTypesWithDependentCount(int count)
        {
            HashSet<string> allTypes = GetAllTypesInGraph();
            HashSet<string> result = new HashSet<string>();
            foreach (string typeName in allTypes)
            {
                int dependentCount = GetDependentCount(typeName);
                if (dependentCount == count)
                {
                    result.Add(typeName);
                }
            }
            return result;
        }

        public HashSet<string> GetTypesWithDependentCountGreaterThan(int min)
        {
            HashSet<string> allTypes = GetAllTypesInGraph();
            HashSet<string> result = new HashSet<string>();
            foreach (string typeName in allTypes)
            {
                int dependentCount = GetDependentCount(typeName);
                if (dependentCount > min)
                {
                    result.Add(typeName);
                }
            }
            return result;
        }

        public HashSet<string> GetTypesWithDependentCountGreaterThanOrEqual(int min)
        {
            HashSet<string> allTypes = GetAllTypesInGraph();
            HashSet<string> result = new HashSet<string>();
            foreach (string typeName in allTypes)
            {
                int dependentCount = GetDependentCount(typeName);
                if (dependentCount >= min)
                {
                    result.Add(typeName);
                }
            }
            return result;
        }

        public HashSet<string> GetTypesWithDependentCountLessThan(int max)
        {
            HashSet<string> allTypes = GetAllTypesInGraph();
            HashSet<string> result = new HashSet<string>();
            foreach (string typeName in allTypes)
            {
                int dependentCount = GetDependentCount(typeName);
                if (dependentCount < max)
                {
                    result.Add(typeName);
                }
            }
            return result;
        }

        public HashSet<string> GetTypesWithDependentCountLessThanOrEqual(int max)
        {
            HashSet<string> allTypes = GetAllTypesInGraph();
            HashSet<string> result = new HashSet<string>();
            foreach (string typeName in allTypes)
            {
                int dependentCount = GetDependentCount(typeName);
                if (dependentCount <= max)
                {
                    result.Add(typeName);
                }
            }
            return result;
        }

        public HashSet<string> GetTypesWithDependentCountRange(int min, int max)
        {
            HashSet<string> allTypes = GetAllTypesInGraph();
            HashSet<string> result = new HashSet<string>();
            foreach (string typeName in allTypes)
            {
                int dependentCount = GetDependentCount(typeName);
                if (dependentCount >= min && dependentCount <= max)
                {
                    result.Add(typeName);
                }
            }
            return result;
        }

        private HashSet<string> GetAllTypesInGraph()
        {
            HashSet<string> allTypes = new HashSet<string>(_graph.Dependencies.Keys);
            foreach (HashSet<string> dependencies in _graph.Dependencies.Values)
            {
                foreach (string dependency in dependencies)
                {
                    allTypes.Add(dependency);
                }
            }
            return allTypes;
        }
    }
}

