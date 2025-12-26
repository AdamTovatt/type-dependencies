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

        public HashSet<string> GetTypesWithDependencyCount(int count)
        {
            HashSet<string> allTypes = GetAllTypesInGraph();
            HashSet<string> result = new HashSet<string>();
            foreach (string typeName in allTypes)
            {
                int dependencyCount = GetDependencyCount(typeName);
                if (dependencyCount == count)
                {
                    result.Add(typeName);
                }
            }
            return result;
        }

        public HashSet<string> GetTypesWithDependencyCountGreaterThan(int min)
        {
            HashSet<string> allTypes = GetAllTypesInGraph();
            HashSet<string> result = new HashSet<string>();
            foreach (string typeName in allTypes)
            {
                int dependencyCount = GetDependencyCount(typeName);
                if (dependencyCount > min)
                {
                    result.Add(typeName);
                }
            }
            return result;
        }

        public HashSet<string> GetTypesWithDependencyCountGreaterThanOrEqual(int min)
        {
            HashSet<string> allTypes = GetAllTypesInGraph();
            HashSet<string> result = new HashSet<string>();
            foreach (string typeName in allTypes)
            {
                int dependencyCount = GetDependencyCount(typeName);
                if (dependencyCount >= min)
                {
                    result.Add(typeName);
                }
            }
            return result;
        }

        public HashSet<string> GetTypesWithDependencyCountLessThan(int max)
        {
            HashSet<string> allTypes = GetAllTypesInGraph();
            HashSet<string> result = new HashSet<string>();
            foreach (string typeName in allTypes)
            {
                int dependencyCount = GetDependencyCount(typeName);
                if (dependencyCount < max)
                {
                    result.Add(typeName);
                }
            }
            return result;
        }

        public HashSet<string> GetTypesWithDependencyCountLessThanOrEqual(int max)
        {
            HashSet<string> allTypes = GetAllTypesInGraph();
            HashSet<string> result = new HashSet<string>();
            foreach (string typeName in allTypes)
            {
                int dependencyCount = GetDependencyCount(typeName);
                if (dependencyCount <= max)
                {
                    result.Add(typeName);
                }
            }
            return result;
        }

        public HashSet<string> GetTypesWithDependencyCountRange(int min, int max)
        {
            HashSet<string> allTypes = GetAllTypesInGraph();
            HashSet<string> result = new HashSet<string>();
            foreach (string typeName in allTypes)
            {
                int dependencyCount = GetDependencyCount(typeName);
                if (dependencyCount >= min && dependencyCount <= max)
                {
                    result.Add(typeName);
                }
            }
            return result;
        }

        public HashSet<string> GetTransitiveDependenciesOf(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return new HashSet<string>();

            HashSet<string> transitiveDependencies = new HashSet<string>();
            HashSet<string> visited = new HashSet<string>();
            Stack<string> stack = new Stack<string>();

            IReadOnlySet<string>? directDependencies = _graph.GetDependencies(typeName);
            if (directDependencies != null)
            {
                foreach (string dependency in directDependencies)
                {
                    if (dependency != typeName)
                    {
                        stack.Push(dependency);
                    }
                }
            }

            while (stack.Count > 0)
            {
                string current = stack.Pop();
                if (visited.Contains(current) || current == typeName)
                    continue;

                visited.Add(current);
                transitiveDependencies.Add(current);

                IReadOnlySet<string>? dependencies = _graph.GetDependencies(current);
                if (dependencies != null)
                {
                    foreach (string dependency in dependencies)
                    {
                        if (!visited.Contains(dependency) && dependency != typeName)
                        {
                            stack.Push(dependency);
                        }
                    }
                }
            }

            return transitiveDependencies;
        }

        public HashSet<string> GetTransitiveDependentsOf(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return new HashSet<string>();

            HashSet<string> transitiveDependents = new HashSet<string>();
            HashSet<string> visited = new HashSet<string>();
            Stack<string> stack = new Stack<string>();

            HashSet<string> directDependents = GetDependentsOf(typeName);
            foreach (string dependent in directDependents)
            {
                if (dependent != typeName)
                {
                    stack.Push(dependent);
                }
            }

            while (stack.Count > 0)
            {
                string current = stack.Pop();
                if (visited.Contains(current) || current == typeName)
                    continue;

                visited.Add(current);
                transitiveDependents.Add(current);

                HashSet<string> dependents = GetDependentsOf(current);
                foreach (string dependent in dependents)
                {
                    if (!visited.Contains(dependent) && dependent != typeName)
                    {
                        stack.Push(dependent);
                    }
                }
            }

            return transitiveDependents;
        }

        public List<List<string>> GetCircularDependencies()
        {
            List<List<string>> cycles = new List<List<string>>();
            HashSet<string> allTypes = GetAllTypesInGraph();
            HashSet<string> visited = new HashSet<string>();

            foreach (string typeName in allTypes)
            {
                if (visited.Contains(typeName))
                    continue;

                HashSet<string> recursionStack = new HashSet<string>();
                List<string> currentPath = new List<string>();
                FindCycles(typeName, visited, recursionStack, currentPath, cycles);
            }

            return cycles;
        }

        private void FindCycles(
            string typeName,
            HashSet<string> visited,
            HashSet<string> recursionStack,
            List<string> currentPath,
            List<List<string>> cycles)
        {
            if (recursionStack.Contains(typeName))
            {
                int cycleStartIndex = currentPath.IndexOf(typeName);
                if (cycleStartIndex >= 0)
                {
                    List<string> cycle = new List<string>();
                    for (int i = cycleStartIndex; i < currentPath.Count; i++)
                    {
                        cycle.Add(currentPath[i]);
                    }
                    cycle.Add(typeName);
                    cycles.Add(cycle);
                }
                return;
            }

            if (visited.Contains(typeName))
                return;

            visited.Add(typeName);
            recursionStack.Add(typeName);
            currentPath.Add(typeName);

            IReadOnlySet<string>? dependencies = _graph.GetDependencies(typeName);
            if (dependencies != null)
            {
                foreach (string dependency in dependencies)
                {
                    FindCycles(dependency, visited, recursionStack, currentPath, cycles);
                }
            }

            recursionStack.Remove(typeName);
            currentPath.RemoveAt(currentPath.Count - 1);
        }

        private int GetDependencyCount(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                return 0;

            IReadOnlySet<string>? dependencies = _graph.GetDependencies(typeName);
            return dependencies?.Count ?? 0;
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

