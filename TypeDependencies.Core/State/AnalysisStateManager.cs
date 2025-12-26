using System.Text.Json;
using TypeDependencies.Core.Models;

namespace TypeDependencies.Core.State
{
    public class AnalysisStateManager : IAnalysisStateManager
    {
        private readonly string _stateDirectory;
        private readonly JsonSerializerOptions _jsonOptions;

        public AnalysisStateManager()
        {
            _stateDirectory = Path.GetTempPath();
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
            };
        }

        public string InitializeSession()
        {
            string sessionId = Guid.NewGuid().ToString();
            string stateFilePath = GetStateFilePath(sessionId);
            AnalysisState state = new AnalysisState
            {
                SessionId = sessionId,
                DllPaths = new List<string>(),
            };

            string json = JsonSerializer.Serialize(state, _jsonOptions);
            File.WriteAllText(stateFilePath, json);

            return sessionId;
        }

        public void AddDllPath(string sessionId, string dllPath)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));

            if (string.IsNullOrWhiteSpace(dllPath))
                throw new ArgumentException("DLL path cannot be null or empty.", nameof(dllPath));

            string stateFilePath = GetStateFilePath(sessionId);
            if (!File.Exists(stateFilePath))
                throw new InvalidOperationException($"Session {sessionId} does not exist.");

            AnalysisState state = LoadState(sessionId);
            if (!state.DllPaths.Contains(dllPath, StringComparer.OrdinalIgnoreCase))
            {
                state.DllPaths.Add(dllPath);
                SaveState(state);
            }
        }

        public IReadOnlyList<string> GetDllPaths(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));

            AnalysisState state = LoadState(sessionId);
            return state.DllPaths.AsReadOnly();
        }

        public void ClearSession(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));

            string stateFilePath = GetStateFilePath(sessionId);
            if (File.Exists(stateFilePath))
            {
                File.Delete(stateFilePath);
            }
        }

        public bool SessionExists(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return false;

            string stateFilePath = GetStateFilePath(sessionId);
            return File.Exists(stateFilePath);
        }

        public void SaveGeneratedGraph(string sessionId, DependencyGraph graph)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));

            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            AnalysisState state = LoadState(sessionId);

            // Convert DependencyGraph to Dictionary<string, List<string>> for serialization
            Dictionary<string, List<string>> graphData = new Dictionary<string, List<string>>();
            foreach (KeyValuePair<string, HashSet<string>> entry in graph.Dependencies)
            {
                graphData[entry.Key] = entry.Value.OrderBy(x => x).ToList();
            }

            state.GeneratedGraph = graphData;
            SaveState(state);
        }

        public DependencyGraph? GetGeneratedGraph(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("Session ID cannot be null or empty.", nameof(sessionId));

            AnalysisState state = LoadState(sessionId);

            if (state.GeneratedGraph == null)
                return null;

            // Convert Dictionary<string, List<string>> back to DependencyGraph
            DependencyGraph graph = new DependencyGraph();
            foreach (KeyValuePair<string, List<string>> entry in state.GeneratedGraph)
            {
                graph.AddDependencies(entry.Key, entry.Value);
            }

            return graph;
        }

        public bool HasGeneratedGraph(string sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                return false;

            try
            {
                AnalysisState state = LoadState(sessionId);
                return state.GeneratedGraph != null && state.GeneratedGraph.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        private string GetStateFilePath(string sessionId)
        {
            return Path.Combine(_stateDirectory, $"typedep-{sessionId}.json");
        }

        private AnalysisState LoadState(string sessionId)
        {
            string stateFilePath = GetStateFilePath(sessionId);
            if (!File.Exists(stateFilePath))
                throw new FileNotFoundException($"State file not found for session {sessionId}", stateFilePath);

            string json = File.ReadAllText(stateFilePath);
            AnalysisState? state = JsonSerializer.Deserialize<AnalysisState>(json, _jsonOptions);

            if (state == null)
                throw new InvalidOperationException($"Failed to deserialize state for session {sessionId}");

            return state;
        }

        private void SaveState(AnalysisState state)
        {
            string stateFilePath = GetStateFilePath(state.SessionId);
            string json = JsonSerializer.Serialize(state, _jsonOptions);
            File.WriteAllText(stateFilePath, json);
        }
    }
}

