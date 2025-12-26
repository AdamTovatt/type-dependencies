# TypeDependencies

A command-line tool for analyzing and visualizing type dependencies in C# assemblies. Extract dependency graphs from compiled DLLs and export them in DOT (Graphviz) or JSON format.

## Installation

Install as a .NET tool:

```bash
dotnet tool install -g TypeDependencies
```

Or install from source:

```bash
git clone https://github.com/AdamTovatt/type-dependencies.git
cd type-dependencies
dotnet build
```

## Usage

### Basic Workflow

1. **Initialize** a new analysis session:
   ```bash
   type-dep init
   ```

2. **Add** DLL files to analyze:
   ```bash
   type-dep add path/to/your/assembly.dll
   type-dep add path/to/another/assembly.dll
   ```

3. **Finalize** and export the dependency graph:
   ```bash
   type-dep finalize
   ```

This creates a `type-dependencies.dot` file in the current working directory, which can be visualized using [Graphviz](https://graphviz.org/).

**Note:** The session is automatically cleared after a successful `finalize` command. To analyze more DLLs, run `init` again to start a new session.

### Export Formats

**DOT format (default):**
```bash
type-dep finalize
# or explicitly
type-dep finalize --format dot
```

**JSON format:**
```bash
type-dep finalize --format json
```

**Custom output path:**
```bash
type-dep finalize --output my-dependencies.json
```

By default, the output file is saved to the current working directory as `type-dependencies.dot` (or `type-dependencies.json` for JSON format).

### Example

```bash
type-dep init
type-dep add MyProject/bin/Debug/net8.0/MyProject.dll
type-dep finalize --format json --output dependencies.json
```

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

## License

GPL-3.0
