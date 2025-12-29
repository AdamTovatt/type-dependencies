using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TypeDependencies.Cli.Commands;
using TypeDependencies.Cli.Suggest;
using TypeDependencies.Core.Analysis;
using TypeDependencies.Core.Export;
using TypeDependencies.Core.State;

namespace TypeDependencies.Cli
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Check if running in MCP server mode
            bool isMcpMode = args.Length > 0 &&
                (args[0].Equals("--mcp", StringComparison.OrdinalIgnoreCase) ||
                 args[0].Equals("mcp", StringComparison.OrdinalIgnoreCase));

            if (isMcpMode)
            {
                // Run as MCP server
                return await RunMcpServerAsync(args);
            }
            else
            {
                // Run as CLI tool
                return await RunCliAsync(args);
            }
        }

        private static async Task<int> RunCliAsync(string[] args)
        {
            // Set up dependency injection
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider serviceProvider = services.BuildServiceProvider();

            try
            {
                // Get services
                IAnalysisStateManager stateManager = serviceProvider.GetRequiredService<IAnalysisStateManager>();
                ITypeAnalyzer typeAnalyzer = serviceProvider.GetRequiredService<ITypeAnalyzer>();
                IExportStrategy defaultExportStrategy = serviceProvider.GetRequiredService<IExportStrategy>();
                ICurrentSessionFinder sessionFinder = serviceProvider.GetRequiredService<ICurrentSessionFinder>();
                IDllSuggester dllSuggester = serviceProvider.GetRequiredService<IDllSuggester>();

                // Create root command
                RootCommand rootCommand = new RootCommand("Type dependency analyzer for C# assemblies");

                // Add subcommands
                rootCommand.Subcommands.Add(InitCommand.Create(stateManager));
                rootCommand.Subcommands.Add(AddCommand.Create(stateManager, sessionFinder));
                rootCommand.Subcommands.Add(GenerateCommand.Create(stateManager, typeAnalyzer, sessionFinder));
                rootCommand.Subcommands.Add(ExportCommand.Create(stateManager, defaultExportStrategy, sessionFinder));
                rootCommand.Subcommands.Add(QueryCommand.Create(stateManager, sessionFinder));
                rootCommand.Subcommands.Add(SuggestCommand.Create(dllSuggester));

                return await rootCommand.Parse(args).InvokeAsync();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Unexpected error: {ex.Message}");
                return 1;
            }
        }

        private static async Task<int> RunMcpServerAsync(string[] args)
        {
            try
            {
                HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

                // Configure logging to stderr
                builder.Logging.ClearProviders();
                builder.Logging.AddConsole(options =>
                {
                    options.LogToStandardErrorThreshold = LogLevel.Trace;
                });

                // Register services
                ConfigureServices(builder.Services);

                // Configure MCP server
                builder.Services
                    .AddMcpServer()
                    .WithStdioServerTransport()
                    .WithToolsFromAssembly();

                // Register MCP tools class
                builder.Services.AddSingleton<McpTools>();

                IHost host = builder.Build();
                await host.RunAsync();

                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"MCP Server error: {ex.Message}");
                return 1;
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // Register core services
            services.AddSingleton<IAnalysisStateManager, AnalysisStateManager>();
            services.AddSingleton<ITypeAnalyzer, TypeAnalyzer>();
            services.AddSingleton<IExportStrategy, DotExportStrategy>();
            services.AddSingleton<ICurrentSessionFinder, CurrentSessionFinder>();
            services.AddSingleton<IDllSuggester, DllSuggester>();
        }
    }
}

