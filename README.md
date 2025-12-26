# TypeDependencies

A command-line tool for analyzing and visualizing type dependencies in C# assemblies. Extract dependency graphs from compiled DLLs and export them in DOT (Graphviz), JSON, Mermaid, or HTML format.

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

3. **Generate** the dependency graph:
   ```bash
   type-dep generate
   ```

4. **Export** the dependency graph in your desired format:
   ```bash
   type-dep export
   ```

This creates a `type-dependencies.dot` file in the current working directory, which can be visualized using [Graphviz](https://graphviz.org/).

**Note:** After running `generate`, you can export the graph multiple times in different formats without regenerating it. The session persists until you manually clear it or start a new one with `init`.

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

**HTML format:**
```bash
type-dep export --format html
```

This creates an interactive HTML visualization that can be opened in any web browser.

**Custom output path:**
```bash
type-dep export --output my-dependencies.json
```

By default, the output file is saved to the current working directory as `type-dependencies.dot` (or `type-dependencies.json` for JSON format, `type-dependencies.mmd` for Mermaid format, or `type-dependencies.html` for HTML format).

**Multiple exports:** Since the graph is generated once and stored, you can export it multiple times in different formats:
```bash
type-dep generate
type-dep export --format dot --output graph.dot
type-dep export --format json --output graph.json
type-dep export --format mermaid --output graph.mmd
type-dep export --format html --output graph.html
```

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

### Example

```bash
type-dep init
type-dep add MyProject/bin/Debug/net8.0/MyProject.dll
type-dep generate
type-dep export --format json --output dependencies.json
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
