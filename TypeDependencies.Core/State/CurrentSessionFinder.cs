namespace TypeDependencies.Core.State
{
    public class CurrentSessionFinder : ICurrentSessionFinder
    {
        private readonly string _stateDirectory;

        public CurrentSessionFinder()
        {
            _stateDirectory = Path.GetTempPath();
        }

        public string? FindCurrentSessionId()
        {
            string[] stateFiles = Directory.GetFiles(_stateDirectory, "typedep-*.json");

            if (stateFiles.Length == 0)
                return null;

            // Get the most recently modified file
            FileInfo? mostRecent = stateFiles
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .FirstOrDefault();

            if (mostRecent == null)
                return null;

            // Extract session ID from filename: typedep-{guid}.json
            string fileName = Path.GetFileNameWithoutExtension(mostRecent.Name);
            if (fileName.StartsWith("typedep-", StringComparison.OrdinalIgnoreCase))
            {
                return fileName.Substring("typedep-".Length);
            }

            return null;
        }
    }
}

