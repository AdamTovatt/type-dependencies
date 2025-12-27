# TypeDependencies

[![Tests](https://github.com/AdamTovatt/type-dependencies/actions/workflows/dotnet.yml/badge.svg)](https://github.com/AdamTovatt/type-dependencies/actions/workflows/dotnet.yml)
[![NuGet Version](https://img.shields.io/nuget/v/TypeDependencies.svg)](https://www.nuget.org/packages/TypeDependencies)
[![NuGet Downloads](https://img.shields.io/nuget/dt/TypeDependencies.svg)](https://www.nuget.org/packages/TypeDependencies)
[![License: GPL-3.0](https://img.shields.io/badge/License-GPL--3.0-green.svg)](https://opensource.org/licenses/GPL-3.0)

A command-line tool for analyzing and visualizing type dependencies in C# assemblies. Extract dependency graphs from compiled DLLs and export them in DOT (Graphviz), JSON, or Mermaid format. Works as both a CLI tool and an MCP (Model Context Protocol) server for AI agents.

**Dependencies** are types that a given type depends on (outgoing), while **dependents** are types that depend on a given type (incoming).

### What You Can Do

The tool enables you to answer questions like:

- **Find all dependencies of a type**: See what types a specific type depends on
- **Find all dependents of a type**: See which types depend on a specific type
- **Find types with the fewest dependencies**: Identify leaf nodes or types with minimal coupling
- **Find types with the most dependents**: Identify highly-used types that many other types rely on
- **Detect circular dependencies**: Find dependency cycles that can indicate design issues
- **Analyze transitive relationships**: Follow dependency chains recursively to understand indirect relationships
- **Export dependency graphs**: Visualize the entire dependency structure in various formats

## Table of Contents

- [Installation](#installation)
- [MCP Server Mode](#mcp-server-mode)
- [Usage](#usage)
  - [CLI Tool](#cli-tool)
  - [Export Formats](#export-formats)
  - [Querying the Dependency Graph](#querying-the-dependency-graph)
  - [Available MCP Tools](#available-mcp-tools)
- [What Gets Analyzed](#what-gets-analyzed)
- [Development](#development)
- [License](#license)

## Installation

### As a .NET Global Tool

```bash
dotnet tool install --global TypeDependencies
```

After installation, the `type-dep` command will be available globally, the CLI is ready to use.

## MCP Server Mode

TypeDependencies can also run as an MCP server, making it available to AI agents through the Model Context Protocol.

**Run as MCP server:**
```bash
type-dep --mcp
```

If you want it as an MCP tool in for example Cursor, add this to your MCP configuration:

```json
{
  "mcpServers": {
    "type-dep": {
      "name": "TypeDependencies MCP Server",
      "stdio": true,
      "command": "type-dep",
      "args": ["--mcp"]
    }
  }
}
```

**Uninstall:**
```bash
dotnet tool uninstall -g TypeDependencies
```

## Usage

### CLI Tool

#### Workflow

1. Initialize a new analysis session
2. Add DLL files to analyze
3. Generate the dependency graph
4. Export the graph in your desired format (or query it)

#### Commands

**Initialize a session:**
```bash
type-dep init
```

**Add DLL files to analyze:**
```bash
type-dep add path/to/your/assembly.dll
type-dep add path/to/another/assembly.dll
```

**Generate the dependency graph:**
```bash
type-dep generate
```

**Export the dependency graph:**
```bash
type-dep export                           # Export as DOT (default)
type-dep export --format dot              # Export as DOT
type-dep export --format json             # Export as JSON
type-dep export --format mermaid          # Export as Mermaid
type-dep export --output my-dependencies.json  # Custom output path
```

**Query the dependency graph:**
```bash
type-dep query dependents-of MyNamespace.MyType
type-dep query dependencies-of MyNamespace.MyType
type-dep query transitive-dependencies-of MyNamespace.MyType
type-dep query transitive-dependents-of MyNamespace.MyType
type-dep query circular-dependencies
type-dep query dependents 0          # Types with exactly 0 dependents
type-dep query dependents >5         # Types with more than 5 dependents
type-dep query dependencies 0        # Types with no dependencies
```

**Get help:**
```bash
type-dep help
```

#### Examples

```bash
# Basic workflow
type-dep init
type-dep add MyProject/bin/Debug/net8.0/MyProject.dll
type-dep generate
type-dep export --format json --output dependencies.json

# Export in multiple formats
type-dep generate
type-dep export --format dot --output graph.dot
type-dep export --format json --output graph.json
type-dep export --format mermaid --output graph.mmd

# Query the graph
type-dep generate
type-dep query dependents-of MyNamespace.MyType
type-dep query dependencies 0          # Find leaf nodes
type-dep query circular-dependencies   # Detect cycles
```

### Export Formats

After generating the dependency graph, you can export it in multiple formats:

**DOT format (default):**
```bash
type-dep export
# or explicitly
type-dep export --format dot
```

**JSON format:**
```bash
type-dep export --format json
```

**Mermaid format:**
```bash
type-dep export --format mermaid
```

**Custom output path:**
```bash
type-dep export --output my-dependencies.json
```

By default, the output file is saved to the current working directory as `type-dependencies.dot` (or `type-dependencies.json` for JSON format, `type-dependencies.mmd` for Mermaid format).

**Multiple exports:** Since the graph is generated once and stored, you can export it multiple times in different formats without regenerating it. The session persists until you manually clear it or start a new one with `init`.

### Querying the Dependency Graph

After generating the dependency graph, you can query it to find specific information:

**Find types that depend on a specific type:**
```bash
type-dep query dependents-of MyNamespace.MyType
```

**Find types that a specific type depends on:**
```bash
type-dep query dependencies-of MyNamespace.MyType
```

**Filter types by dependent count:**
```bash
type-dep query dependents 0          # Types with exactly 0 dependents
type-dep query dependents >5         # Types with more than 5 dependents
type-dep query dependents >=3        # Types with 3 or more dependents
type-dep query dependents <2         # Types with less than 2 dependents
type-dep query dependents <=1        # Types with 1 or fewer dependents
type-dep query dependents 2-10       # Types with between 2 and 10 dependents (inclusive)
```

**Filter types by dependency count (outgoing dependencies):**
```bash
type-dep query dependencies 0          # Types with no dependencies (leaf nodes)
type-dep query dependencies >5         # Types that depend on more than 5 types
type-dep query dependencies >=3        # Types with 3 or more dependencies
type-dep query dependencies <2         # Types with less than 2 dependencies
type-dep query dependencies <=1        # Types with 1 or fewer dependencies
type-dep query dependencies 2-10       # Types with between 2 and 10 dependencies (inclusive)
```

**Find transitive dependencies (recursive):**
```bash
type-dep query transitive-dependencies-of MyNamespace.MyType
```
This returns all types that the specified type depends on, directly and indirectly, following the entire dependency chain.

**Find transitive dependents (recursive):**
```bash
type-dep query transitive-dependents-of MyNamespace.MyType
```
This returns all types that depend on the specified type, directly and indirectly, following the entire dependency chain in reverse.

**Detect circular dependencies:**
```bash
type-dep query circular-dependencies
```
This finds and lists all circular dependency cycles in the graph. Each cycle is displayed as a chain of type names connected by arrows (e.g., `TypeA -> TypeB -> TypeC -> TypeA`).

All query results are output one type per line, sorted alphabetically (except circular dependencies which show the cycle path).

### Available MCP Tools

When running as an MCP server, the following tools are available:

**Session Management:**
- `td_init()` - Initialize a new analysis session

**DLL Management:**
- `td_add(dllPath: string)` - Add a DLL to the current session

**Graph Generation:**
- `td_generate()` - Generate dependency graph from added DLLs

**Export:**
- `td_export(format?: string, outputPath?: string)` - Export the generated graph
  - `format`: "dot", "json", or "mermaid" (defaults to "dot")
  - `outputPath`: Optional file path (defaults to `type-dependencies.{ext}` in current directory)

**Query Tools:**
- `td_query_dependents_of(typeName: string)` - Find types that depend on a type
- `td_query_dependencies_of(typeName: string)` - Find types a type depends on
- `td_query_dependents(countExpression: string)` - Filter by dependent count (supports: number, >number, >=number, <number, <=number, min-max)
- `td_query_dependencies(countExpression: string)` - Filter by dependency count (supports: number, >number, >=number, <number, <=number, min-max)
- `td_query_transitive_dependencies_of(typeName: string)` - Find recursive dependencies
- `td_query_transitive_dependents_of(typeName: string)` - Find recursive dependents
- `td_query_circular_dependencies()` - Detect circular dependencies

All MCP tools return string responses with success messages or error descriptions.

## What Gets Analyzed

The tool extracts dependencies from:
- Base types
- Implemented interfaces
- Field types
- Property types
- Method return types and parameters
- Attribute types
- Generic type constraints

System and Microsoft namespaces are automatically filtered out to focus on your application code.

## Development

### Getting the repository

```bash
git clone https://github.com/AdamTovatt/type-dependencies.git
cd type-dependencies
```

### Building

```bash
dotnet build
```

### Installing from source

```bash
dotnet build
dotnet tool install -g --add-source "./TypeDependencies.Cli/bin/Debug" TypeDependencies
```

### Running Tests

```bash
dotnet test
```

## License

GPL-3.0
