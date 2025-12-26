using System.Text.Json.Serialization;

namespace TypeDependencies.Core.State
{
    public class AnalysisState
    {
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; } = string.Empty;

        [JsonPropertyName("dllPaths")]
        public List<string> DllPaths { get; set; } = new List<string>();

        [JsonPropertyName("generatedGraph")]
        public Dictionary<string, List<string>>? GeneratedGraph { get; set; }
    }
}

