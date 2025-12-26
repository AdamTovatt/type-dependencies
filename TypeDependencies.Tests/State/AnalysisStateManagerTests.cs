using FluentAssertions;
using TypeDependencies.Core.State;

namespace TypeDependencies.Tests.State
{
    public class AnalysisStateManagerTests
    {
        [Fact]
        public void InitializeSession_ShouldCreateNewSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();

            string sessionId = stateManager.InitializeSession();

            sessionId.Should().NotBeNullOrWhiteSpace();
            stateManager.SessionExists(sessionId).Should().BeTrue();
        }

        [Fact]
        public void AddDllPath_ShouldAddPathToSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            string dllPath = @"C:\Test\Test.dll";

            stateManager.AddDllPath(sessionId, dllPath);

            IReadOnlyList<string> paths = stateManager.GetDllPaths(sessionId);
            paths.Should().Contain(dllPath);
        }

        [Fact]
        public void AddDllPath_ShouldNotAddDuplicate()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            string dllPath = @"C:\Test\Test.dll";

            stateManager.AddDllPath(sessionId, dllPath);
            stateManager.AddDllPath(sessionId, dllPath);

            IReadOnlyList<string> paths = stateManager.GetDllPaths(sessionId);
            paths.Should().HaveCount(1);
            paths.Should().Contain(dllPath);
        }

        [Fact]
        public void AddDllPath_ShouldBeCaseInsensitive()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();
            string dllPath1 = @"C:\Test\Test.dll";
            string dllPath2 = @"C:\TEST\TEST.DLL";

            stateManager.AddDllPath(sessionId, dllPath1);
            stateManager.AddDllPath(sessionId, dllPath2);

            IReadOnlyList<string> paths = stateManager.GetDllPaths(sessionId);
            paths.Should().HaveCount(1);
        }

        [Fact]
        public void GetDllPaths_ShouldReturnEmptyListForNewSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();

            IReadOnlyList<string> paths = stateManager.GetDllPaths(sessionId);

            paths.Should().BeEmpty();
        }

        [Fact]
        public void ClearSession_ShouldRemoveSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();
            string sessionId = stateManager.InitializeSession();

            stateManager.ClearSession(sessionId);

            stateManager.SessionExists(sessionId).Should().BeFalse();
        }

        [Fact]
        public void AddDllPath_ShouldThrowOnInvalidSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();

            Action act = () => stateManager.AddDllPath("invalid-session", @"C:\Test\Test.dll");
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void GetDllPaths_ShouldThrowOnInvalidSession()
        {
            IAnalysisStateManager stateManager = new AnalysisStateManager();

            Action act = () => stateManager.GetDllPaths("invalid-session");
            act.Should().Throw<FileNotFoundException>();
        }
    }
}

