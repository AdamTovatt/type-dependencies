using EasyReasy;
using FluentAssertions;
using System.Reflection;

namespace TypeDependencies.Tests.Resources
{
    public class ResourceCollectionTests
    {
        [Fact]
        public async Task AllResourcesExist()
        {
            Assembly? assembly = Assembly.GetAssembly(typeof(Core.Resources));
            assembly.Should().NotBeNull();

            ResourceManager resourceManager = await ResourceManager.CreateInstanceAsync(assembly);

            resourceManager.Should().NotBeNull();

            // Verify the HTML template resource exists
            string templateContent = await resourceManager.ReadAsStringAsync(Core.Resources.HtmlTemplate);
            templateContent.Should().NotBeNullOrWhiteSpace();
            templateContent.Should().Contain("<!DOCTYPE html>", "HTML template should contain HTML content");
        }
    }
}

