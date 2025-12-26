using EasyReasy;

namespace TypeDependencies.Core
{
    [ResourceCollection(typeof(EmbeddedResourceProvider))]
    public static class Resources
    {
        public static readonly Resource HtmlTemplate = new Resource("Resources/HtmlTemplate.html");
    }
}

