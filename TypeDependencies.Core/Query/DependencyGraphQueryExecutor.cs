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
    }
}

