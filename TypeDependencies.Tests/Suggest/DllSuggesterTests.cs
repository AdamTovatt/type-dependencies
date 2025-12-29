using FluentAssertions;
using TypeDependencies.Cli.Models;
using TypeDependencies.Cli.Suggest;

namespace TypeDependencies.Tests.Suggest
{
    public class DllSuggesterTests
    {
        [Fact]
        public void SuggestDlls_ShouldThrowOnNullDirectory()
        {
            IDllSuggester suggester = new DllSuggester();

            Action act = () => suggester.SuggestDlls(null!);
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SuggestDlls_ShouldThrowOnEmptyDirectory()
        {
            IDllSuggester suggester = new DllSuggester();

            Action act = () => suggester.SuggestDlls("");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SuggestDlls_ShouldThrowOnWhitespaceDirectory()
        {
            IDllSuggester suggester = new DllSuggester();

            Action act = () => suggester.SuggestDlls("   ");
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void SuggestDlls_ShouldThrowOnNonExistentDirectory()
        {
            IDllSuggester suggester = new DllSuggester();

            Action act = () => suggester.SuggestDlls(@"C:\NonExistent\Directory");
            act.Should().Throw<DirectoryNotFoundException>();
        }

        [Fact]
        public void SuggestDlls_ShouldReturnEmptyListWhenNoCsprojFilesFound()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                IDllSuggester suggester = new DllSuggester();
                IReadOnlyList<DllSuggestion> suggestions = suggester.SuggestDlls(tempDir);

                suggestions.Should().BeEmpty();
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void SuggestDlls_ShouldReturnEmptyListWhenNoMatchingDllFilesFound()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                string csprojPath = Path.Combine(tempDir, "MyProject.csproj");
                File.WriteAllText(csprojPath, "<Project></Project>");

                IDllSuggester suggester = new DllSuggester();
                IReadOnlyList<DllSuggestion> suggestions = suggester.SuggestDlls(tempDir);

                suggestions.Should().BeEmpty();
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void SuggestDlls_ShouldFindMatchingDllInSameDirectory()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                string csprojPath = Path.Combine(tempDir, "MyProject.csproj");
                File.WriteAllText(csprojPath, "<Project></Project>");

                string dllPath = Path.Combine(tempDir, "MyProject.dll");
                File.WriteAllText(dllPath, "dummy dll content");

                IDllSuggester suggester = new DllSuggester();
                IReadOnlyList<DllSuggestion> suggestions = suggester.SuggestDlls(tempDir);

                suggestions.Should().HaveCount(1);
                suggestions[0].ProjectName.Should().Be("MyProject");
                suggestions[0].DllPath.Should().Be(Path.GetFullPath(dllPath));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void SuggestDlls_ShouldFindMatchingDllInSubdirectory()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            string subDir = Path.Combine(tempDir, "bin", "Debug");
            Directory.CreateDirectory(subDir);

            try
            {
                string csprojPath = Path.Combine(tempDir, "MyProject.csproj");
                File.WriteAllText(csprojPath, "<Project></Project>");

                string dllPath = Path.Combine(subDir, "MyProject.dll");
                File.WriteAllText(dllPath, "dummy dll content");

                IDllSuggester suggester = new DllSuggester();
                IReadOnlyList<DllSuggestion> suggestions = suggester.SuggestDlls(tempDir);

                suggestions.Should().HaveCount(1);
                suggestions[0].ProjectName.Should().Be("MyProject");
                suggestions[0].DllPath.Should().Be(Path.GetFullPath(dllPath));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void SuggestDlls_ShouldFindMultipleDllsForSameProject()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            string debugDir = Path.Combine(tempDir, "bin", "Debug");
            Directory.CreateDirectory(debugDir);

            string releaseDir = Path.Combine(tempDir, "bin", "Release");
            Directory.CreateDirectory(releaseDir);

            try
            {
                string csprojPath = Path.Combine(tempDir, "MyProject.csproj");
                File.WriteAllText(csprojPath, "<Project></Project>");

                string debugDllPath = Path.Combine(debugDir, "MyProject.dll");
                File.WriteAllText(debugDllPath, "dummy dll content");

                string releaseDllPath = Path.Combine(releaseDir, "MyProject.dll");
                File.WriteAllText(releaseDllPath, "dummy dll content");

                IDllSuggester suggester = new DllSuggester();
                IReadOnlyList<DllSuggestion> suggestions = suggester.SuggestDlls(tempDir);

                suggestions.Should().HaveCount(2);
                suggestions.Should().Contain(s => s.ProjectName == "MyProject" && s.DllPath == Path.GetFullPath(debugDllPath));
                suggestions.Should().Contain(s => s.ProjectName == "MyProject" && s.DllPath == Path.GetFullPath(releaseDllPath));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void SuggestDlls_ShouldFindDllsForMultipleProjects()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                string project1Csproj = Path.Combine(tempDir, "Project1.csproj");
                File.WriteAllText(project1Csproj, "<Project></Project>");

                string project1Dll = Path.Combine(tempDir, "Project1.dll");
                File.WriteAllText(project1Dll, "dummy dll content");

                string project2Csproj = Path.Combine(tempDir, "Project2.csproj");
                File.WriteAllText(project2Csproj, "<Project></Project>");

                string project2Dll = Path.Combine(tempDir, "Project2.dll");
                File.WriteAllText(project2Dll, "dummy dll content");

                IDllSuggester suggester = new DllSuggester();
                IReadOnlyList<DllSuggestion> suggestions = suggester.SuggestDlls(tempDir);

                suggestions.Should().HaveCount(2);
                suggestions.Should().Contain(s => s.ProjectName == "Project1" && s.DllPath == Path.GetFullPath(project1Dll));
                suggestions.Should().Contain(s => s.ProjectName == "Project2" && s.DllPath == Path.GetFullPath(project2Dll));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void SuggestDlls_ShouldNotMatchDllsWithDifferentName()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                string csprojPath = Path.Combine(tempDir, "MyProject.csproj");
                File.WriteAllText(csprojPath, "<Project></Project>");

                string otherDllPath = Path.Combine(tempDir, "OtherProject.dll");
                File.WriteAllText(otherDllPath, "dummy dll content");

                IDllSuggester suggester = new DllSuggester();
                IReadOnlyList<DllSuggestion> suggestions = suggester.SuggestDlls(tempDir);

                suggestions.Should().BeEmpty();
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void SuggestDlls_ShouldReturnFullPaths()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                string csprojPath = Path.Combine(tempDir, "MyProject.csproj");
                File.WriteAllText(csprojPath, "<Project></Project>");

                string dllPath = Path.Combine(tempDir, "MyProject.dll");
                File.WriteAllText(dllPath, "dummy dll content");

                IDllSuggester suggester = new DllSuggester();
                IReadOnlyList<DllSuggestion> suggestions = suggester.SuggestDlls(tempDir);

                suggestions.Should().HaveCount(1);
                suggestions[0].DllPath.Should().Be(Path.GetFullPath(dllPath));
                Path.IsPathRooted(suggestions[0].DllPath).Should().BeTrue(); // Should be a full/rooted path
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}

