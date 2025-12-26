using System.Text.Json;

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

