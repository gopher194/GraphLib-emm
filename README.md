# GraphLib

Generic graph data structures for .NET 10.0 / C# 14 — simple to learn, strongly typed, and ready for real-world relationship modeling.

![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)
![Tests](https://img.shields.io/badge/tests-78%20passing-brightgreen)
![xUnit](https://img.shields.io/badge/tested%20with-xUnit-5C2D91)

GraphLib is a small C# library for working with generic graphs, directed graphs, and directed acyclic graphs (DAGs). It targets **.NET 10.0**, uses **C# 14**, and is backed by **xUnit** tests.

## Overview

GraphLib gives you a clean set of graph data structures for modeling connections between things:

- people in a social network
- tasks in a build pipeline
- courses and prerequisites
- cities and routes
- any other relationship graph where your vertex type can be used as a dictionary key

All graph types are generic and constrained with `where T : notnull`, so your vertices are strongly typed and non-null by design.

### Class hierarchy

```text
Graph<T> (abstract, undirected base behavior)
└── DirectedGraph<T>
    └── DirectedAcyclicGraph<T>
```

### When to use each class

- **`Graph<T>`** — Use when you want an **undirected** graph and are happy to create a tiny concrete subclass.
- **`DirectedGraph<T>`** — Use when edges have a direction, like `A -> B`.
- **`DirectedAcyclicGraph<T>`** — Use when edges are directed **and** cycles must be rejected, such as dependency graphs.

## Getting Started

### Prerequisites

- **.NET 10.0 SDK**
- **C# 14** language support (included with the .NET 10 SDK/toolchain used by this project)

### Build

From the repository root:

```powershell
dotnet build
```

### Run tests

```powershell
dotnet test
```

### Reference the project

At the moment, the easiest way to consume GraphLib is with a project reference.

From your consuming project directory (adjust the relative path as needed for your folder layout):

```powershell
dotnet add reference ..\GraphLib\src\GraphLib\GraphLib.csproj
```

Or add it manually in your `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="..\GraphLib\src\GraphLib\GraphLib.csproj" />
</ItemGroup>
```

## Modern .NET Practices

- **.NET 10 LTS** — Targets the Long Term Support release for a current, supported runtime and SDK baseline.
- **C# 14** — Uses the latest language version, including features such as the `field` keyword and extension members.
- **Directory.Build.props** — Centralizes shared build configuration such as the target framework, nullable reference types, and analysis level.
- **.editorconfig** — Enforces coding standards such as Allman braces, naming conventions, and consistent formatting.
- **TreatWarningsAsErrors** — Keeps compilation strict by failing builds on warnings.
- **Code analysis** — Uses `AnalysisLevel=latest-recommended` to enable built-in Roslyn analyzers with current recommendations.
- **XML documentation** — Generates XML documentation for all public APIs.

## API Reference

### `Graph<T>`

**Description**

`Graph<T>` is the abstract base class for GraphLib. Even though it is abstract, it already contains the full implementation for an **undirected** graph. In practice, you create a very small derived type and use it directly.

### Public API

| Name | Returns | Description |
| --- | --- | --- |
| `AddVertex(T vertex)` | `void` | Adds a new vertex. Throws if the vertex is `null` or already exists. |
| `RemoveVertex(T vertex)` | `bool` | Removes a vertex and all incident edges. Returns `false` if the vertex does not exist. |
| `AddEdge(T from, T to)` | `void` | Adds an undirected edge between two existing, distinct vertices. |
| `RemoveEdge(T from, T to)` | `bool` | Removes an undirected edge. Returns `false` if the edge does not exist. |
| `HasVertex(T vertex)` | `bool` | Returns `true` if the vertex exists. |
| `HasEdge(T from, T to)` | `bool` | Returns `true` if the undirected edge exists. |
| `VertexCount` | `int` | Gets the number of vertices in the graph. |
| `EdgeCount` | `int` | Gets the number of undirected edges. `A-B` counts as `1`. |
| `Vertices` | `IReadOnlyCollection<T>` | Gets the current set of vertices. |
| `GetVertices()` | `IReadOnlyCollection<T>` | Returns the current set of vertices. |
| `GetNeighbors(T vertex)` | `IReadOnlyCollection<T>` | Returns all vertices adjacent to `vertex`. |

### Example

```csharp
using System;
using System.Linq;
using GraphLib;

public sealed class UndirectedGraph<T> : Graph<T> where T : notnull
{
}

public static class Program
{
    public static void Main()
    {
        var cities = new UndirectedGraph<string>();

        cities.AddVertex("Seattle");
        cities.AddVertex("Portland");
        cities.AddVertex("Boise");

        cities.AddEdge("Seattle", "Portland");
        cities.AddEdge("Seattle", "Boise");

        Console.WriteLine($"Vertices: {cities.VertexCount}");
        Console.WriteLine($"Edges: {cities.EdgeCount}");
        Console.WriteLine($"Seattle neighbors: {string.Join(", ", cities.GetNeighbors("Seattle").OrderBy(x => x))}");
    }
}
```

### Exception behavior

- `AddVertex` throws:
  - `ArgumentNullException` if `vertex` is `null`
  - `ArgumentException` if the vertex already exists
- `RemoveVertex` throws:
  - `ArgumentNullException` if `vertex` is `null`
- `AddEdge` throws:
  - `ArgumentNullException` if either endpoint is `null`
  - `ArgumentException` for self-loops
  - `InvalidOperationException` if either vertex does not exist
  - `InvalidOperationException` if the edge already exists
- `RemoveEdge` throws:
  - `ArgumentNullException` if either endpoint is `null`
- `GetNeighbors` throws:
  - `ArgumentNullException` if `vertex` is `null`
  - `InvalidOperationException` if the vertex does not exist
- `HasVertex` and `HasEdge` return `false` for `null` input rather than throwing

---

### `DirectedGraph<T>`

**Description**

`DirectedGraph<T>` extends `Graph<T>` with **one-way edges**. `A -> B` does **not** imply `B -> A`. It also keeps track of incoming and outgoing relationships, which makes predecessor/successor queries and in/out degree calculations straightforward.

### Public API

| Name | Returns | Description |
| --- | --- | --- |
| `AddVertex(T vertex)` | `void` | Adds a new vertex. |
| `RemoveVertex(T vertex)` | `bool` | Removes a vertex and all incoming/outgoing edges. |
| `AddEdge(T from, T to)` | `void` | Adds a directed edge from `from` to `to`. |
| `RemoveEdge(T from, T to)` | `bool` | Removes a directed edge from `from` to `to`. |
| `HasVertex(T vertex)` | `bool` | Returns `true` if the vertex exists. |
| `HasEdge(T from, T to)` | `bool` | Returns `true` if the directed edge exists. |
| `VertexCount` | `int` | Gets the number of vertices. |
| `EdgeCount` | `int` | Gets the number of directed edges. `A -> B` and `B -> A` count as `2`. |
| `Vertices` | `IReadOnlyCollection<T>` | Gets the current set of vertices. |
| `GetVertices()` | `IReadOnlyCollection<T>` | Returns the current set of vertices. |
| `GetNeighbors(T vertex)` | `IReadOnlyCollection<T>` | Returns successors only. This is an alias for `GetSuccessors`. |
| `GetSuccessors(T vertex)` | `IReadOnlyCollection<T>` | Returns outgoing neighbors. |
| `GetPredecessors(T vertex)` | `IReadOnlyCollection<T>` | Returns incoming neighbors. |
| `InDegree(T vertex)` | `int` | Returns the number of incoming edges. |
| `OutDegree(T vertex)` | `int` | Returns the number of outgoing edges. |

### Example

```csharp
using System;
using System.Linq;
using GraphLib;

public static class Program
{
    public static void Main()
    {
        var workflow = new DirectedGraph<string>();

        foreach (var step in new[] { "Parse", "Validate", "Publish" })
        {
            workflow.AddVertex(step);
        }

        workflow.AddEdge("Parse", "Validate");
        workflow.AddEdge("Validate", "Publish");

        Console.WriteLine(workflow.HasEdge("Validate", "Parse")); // False
        Console.WriteLine($"Validate successors: {string.Join(", ", workflow.GetSuccessors("Validate").OrderBy(x => x))}");
        Console.WriteLine($"Publish in-degree: {workflow.InDegree("Publish")}");
        Console.WriteLine($"Parse out-degree: {workflow.OutDegree("Parse")}");
    }
}
```

### Exception behavior

- `AddVertex`, `RemoveVertex`, `RemoveEdge`, `HasVertex`, `HasEdge`, `VertexCount`, `Vertices`, and `GetVertices()` behave the same as in `Graph<T>`
- `AddEdge` throws:
  - `ArgumentNullException` if either endpoint is `null`
  - `ArgumentException` for self-loops
  - `InvalidOperationException` if either vertex does not exist
  - `InvalidOperationException` if the directed edge already exists
- `GetNeighbors`, `GetSuccessors`, `GetPredecessors`, `InDegree`, and `OutDegree` throw:
  - `ArgumentNullException` if `vertex` is `null`
  - `InvalidOperationException` if the vertex does not exist
- `HasVertex` and `HasEdge` still return `false` for `null` input rather than throwing

---

### `DirectedAcyclicGraph<T>`

**Description**

`DirectedAcyclicGraph<T>` (often shortened to **DAG**) extends `DirectedGraph<T>` by preventing cycles. This is the right choice for dependency ordering, scheduling, and any situation where something must happen **before** something else.

### Public API

| Name | Returns | Description |
| --- | --- | --- |
| `AddVertex(T vertex)` | `void` | Adds a new vertex. |
| `RemoveVertex(T vertex)` | `bool` | Removes a vertex and all incoming/outgoing edges. |
| `AddEdge(T from, T to)` | `void` | Adds a directed edge, unless it would create a cycle. |
| `RemoveEdge(T from, T to)` | `bool` | Removes a directed edge. |
| `HasVertex(T vertex)` | `bool` | Returns `true` if the vertex exists. |
| `HasEdge(T from, T to)` | `bool` | Returns `true` if the directed edge exists. |
| `VertexCount` | `int` | Gets the number of vertices. |
| `EdgeCount` | `int` | Gets the number of directed edges. |
| `Vertices` | `IReadOnlyCollection<T>` | Gets the current set of vertices. |
| `GetVertices()` | `IReadOnlyCollection<T>` | Returns the current set of vertices. |
| `GetNeighbors(T vertex)` | `IReadOnlyCollection<T>` | Returns successors only. |
| `GetSuccessors(T vertex)` | `IReadOnlyCollection<T>` | Returns outgoing neighbors. |
| `GetPredecessors(T vertex)` | `IReadOnlyCollection<T>` | Returns incoming neighbors. |
| `InDegree(T vertex)` | `int` | Returns the number of incoming edges. |
| `OutDegree(T vertex)` | `int` | Returns the number of outgoing edges. |
| `TopologicalSort()` | `IReadOnlyList<T>` | Returns a valid topological ordering using Kahn's algorithm. |

### Example

```csharp
using System;
using GraphLib;

public static class Program
{
    public static void Main()
    {
        var pipeline = new DirectedAcyclicGraph<string>();

        foreach (var step in new[] { "Design", "Build", "Test", "Deploy" })
        {
            pipeline.AddVertex(step);
        }

        pipeline.AddEdge("Design", "Build");
        pipeline.AddEdge("Build", "Test");
        pipeline.AddEdge("Test", "Deploy");

        var order = pipeline.TopologicalSort();
        Console.WriteLine(string.Join(" -> ", order));
    }
}
```

### Exception behavior

- All inherited members behave as in `DirectedGraph<T>` unless noted otherwise
- `AddEdge` throws:
  - `CycleDetectedException` if the edge would create a cycle
  - `CycleDetectedException` for self-loops
  - `ArgumentNullException` if either endpoint is `null`
  - `InvalidOperationException` if either vertex does not exist
  - `InvalidOperationException` if the edge already exists
- `TopologicalSort` throws:
  - `InvalidOperationException` if the graph is empty

Because cycles are rejected at insertion time, `TopologicalSort()` gives you a valid ordering for any non-empty graph built through the public API.

---

### `CycleDetectedException`

**Description**

A specialized exception type used by `DirectedAcyclicGraph<T>` when an edge would introduce a cycle. It inherits from `InvalidOperationException`, so you can catch either the specific type or the broader base type.

### Public API

| Name | Returns | Description |
| --- | --- | --- |
| `CycleDetectedException()` | `CycleDetectedException` | Creates the exception with the default message. |
| `CycleDetectedException(string message)` | `CycleDetectedException` | Creates the exception with a custom message. |
| `CycleDetectedException(string message, Exception innerException)` | `CycleDetectedException` | Creates the exception with a custom message and inner exception. |

### Example

```csharp
using System;
using GraphLib;

public static class Program
{
    public static void Main()
    {
        var courses = new DirectedAcyclicGraph<string>();

        foreach (var course in new[] { "Math 101", "Physics 101", "Engineering 101" })
        {
            courses.AddVertex(course);
        }

        courses.AddEdge("Math 101", "Physics 101");
        courses.AddEdge("Physics 101", "Engineering 101");

        try
        {
            courses.AddEdge("Engineering 101", "Math 101");
        }
        catch (CycleDetectedException ex)
        {
            Console.WriteLine($"Cycle blocked: {ex.Message}");
        }
    }
}
```

### Exception behavior

- This type is thrown by `DirectedAcyclicGraph<T>.AddEdge`
- It derives from `InvalidOperationException`
- Catch it when you want to handle cycle errors separately from other invalid operations

## Usage Examples

### 1. Social network (undirected graph)

Friendships usually go both ways, so an undirected graph is a natural fit.

```csharp
using System;
using System.Linq;
using GraphLib;

public sealed class SocialGraph<T> : Graph<T> where T : notnull
{
}

public static class Program
{
    public static void Main()
    {
        var friends = new SocialGraph<string>();

        foreach (var person in new[] { "Alice", "Bob", "Cara", "Diego" })
        {
            friends.AddVertex(person);
        }

        friends.AddEdge("Alice", "Bob");
        friends.AddEdge("Alice", "Cara");
        friends.AddEdge("Bob", "Diego");

        Console.WriteLine($"Alice knows: {string.Join(", ", friends.GetNeighbors("Alice").OrderBy(x => x))}");
        Console.WriteLine($"Total friendships: {friends.EdgeCount}");
    }
}
```

### 2. Task dependency resolution (DAG + topological sort)

A DAG is perfect when tasks must happen in a safe order.

```csharp
using System;
using GraphLib;

public static class Program
{
    public static void Main()
    {
        var tasks = new DirectedAcyclicGraph<string>();

        foreach (var task in new[]
        {
            "Restore packages",
            "Build",
            "Run tests",
            "Publish"
        })
        {
            tasks.AddVertex(task);
        }

        tasks.AddEdge("Restore packages", "Build");
        tasks.AddEdge("Build", "Run tests");
        tasks.AddEdge("Run tests", "Publish");

        var executionOrder = tasks.TopologicalSort();

        Console.WriteLine("Recommended order:");
        foreach (var item in executionOrder)
        {
            Console.WriteLine($"- {item}");
        }
    }
}
```

### 3. Course prerequisites with cycle detection

Cycles in prerequisite graphs are a problem: if Course A requires Course B, and Course B eventually requires Course A, nobody can start.

```csharp
using System;
using GraphLib;

public static class Program
{
    public static void Main()
    {
        var prereqs = new DirectedAcyclicGraph<string>();

        foreach (var course in new[]
        {
            "Intro to Programming",
            "Data Structures",
            "Algorithms"
        })
        {
            prereqs.AddVertex(course);
        }

        prereqs.AddEdge("Intro to Programming", "Data Structures");
        prereqs.AddEdge("Data Structures", "Algorithms");

        try
        {
            prereqs.AddEdge("Algorithms", "Intro to Programming");
        }
        catch (CycleDetectedException ex)
        {
            Console.WriteLine("Invalid prerequisite chain detected.");
            Console.WriteLine(ex.Message);
        }
    }
}
```

## 🎓 ELI5 — Graph Theory for Beginners

If you're brand new to graph theory, you're in good company. The word **graph** can sound intimidating at first, but the idea is actually very natural.

A graph is just a way to describe **things** and the **connections between them**.

### What's a graph?

Think of a graph like a map of cities connected by roads.

- **Cities** are the **vertices** (also called **nodes**)
- **Roads** are the **edges**

```text
[Seattle] ---- [Portland] ---- [Boise]
     \ 
      \---- [Spokane]
```

In code, the cities might be strings like `"Seattle"` or `"Portland"`, and the roads are the relationships you add with `AddEdge`.

The beautiful part is that the "cities" do not have to be real places. They could be:

- people
- tasks
- courses
- servers
- documents
- anything else that can be connected to something else

So when someone says, "Use a graph," they usually mean:

> "We need a good way to model relationships."

That is all. No scary math required to get started.

### Undirected vs directed

Now imagine two kinds of roads.

#### Undirected graph: a two-way street

If Alice and Bob are friends, that relationship usually goes both ways.

```text
[Alice] <----> [Bob]
```

That is what an **undirected** graph represents.

In GraphLib's base `Graph<T>` behavior, adding an edge between `Alice` and `Bob` means each one becomes the other's neighbor.

#### Directed graph: a one-way street

Now imagine a Twitter-style follow, or a task dependency.

```text
[Alice] ----> [Bob]
```

This means Alice points to Bob, but Bob does not automatically point back.

That is a **directed** graph.

Use `DirectedGraph<T>` when direction matters.

A nice way to remember it:

- **Undirected** = two-way street
- **Directed** = one-way street

### What's a DAG?

A **DAG** is a **Directed Acyclic Graph**.

That sounds fancy, so let's unpack it:

- **Directed** = arrows have direction
- **Acyclic** = you can never follow arrows and come back to where you started

Imagine a city of one-way streets where driving in circles is impossible.

```text
[Plan] ----> [Code] ----> [Test] ----> [Deploy]
```

This is a DAG because the arrows keep moving forward.

Now compare it to this:

```text
[A] ----> [B] ----> [C]
 ^                     |
 |_____________________|
```

That is **not** a DAG, because if you start at `A`, you can follow arrows and eventually end up back at `A`.

DAGs are wonderful for:

- task scheduling
- build pipelines
- course prerequisites
- dependency graphs

Anywhere you need a sensible "before/after" structure, a DAG is often the right tool.

### What's topological sort?

A **topological sort** is just a valid order to do things.

Think about getting dressed:

```text
[Underwear] -> [Pants] -> [Shoes]
[Shirt] ----------------> [Sweater]
```

You do not have to put your shirt on before your underwear, because those are unrelated. But you probably want to put on:

- underwear before pants
- pants before shoes
- shirt before sweater

A topological sort finds an order that respects all those rules.

One valid order might be:

```text
Underwear, Shirt, Pants, Sweater, Shoes
```

Another valid order might be:

```text
Shirt, Underwear, Pants, Shoes, Sweater
```

Both can be correct if they obey the dependency arrows.

That is why topological sort is so useful: it does not invent extra rules, it simply finds an order that satisfies the rules you already gave it.

In GraphLib, `DirectedAcyclicGraph<T>.TopologicalSort()` does exactly that.

### What's cycle detection?

Cycle detection is how the library catches impossible dependency loops.

Here is a bad dependency chain:

```text
[A] -> [B] -> [C] -> [A]
```

Read it in plain English:

- A needs B first
- B needs C first
- C needs A first

Now we have a problem.

Who goes first?

- A cannot start until B is done
- B cannot start until C is done
- C cannot start until A is done

Nobody can begin. The rules contradict each other.

That is a **cycle**.

GraphLib helps by stopping you at the moment you try to create that impossible loop. `DirectedAcyclicGraph<T>.AddEdge(...)` throws a `CycleDetectedException` instead of letting the graph become invalid.

That is helpful because the bug appears **early**, right when the bad relationship is added, instead of much later when you try to compute an order.

### A quick mental model

If you're ever unsure which graph type you need, ask yourself these three questions:

1. **Do connections go both ways?**  
   Use an undirected graph (`Graph<T>` behavior in a small derived type).
2. **Do connections go one way?**  
   Use `DirectedGraph<T>`.
3. **Do connections go one way and must never form a loop?**  
   Use `DirectedAcyclicGraph<T>`.

### You've got this ✅

If graph theory feels new, that is completely normal. Most people already understand graph ideas before they know the vocabulary:

- friendships
- roads
- dependencies
- followers
- family trees
- workflows

GraphLib simply gives those ideas a clean .NET API.

If you want to keep learning, these are good next steps:

- 🔗 [Graph theory overview](https://en.wikipedia.org/wiki/Graph_theory)
- 🔗 [Directed acyclic graph](https://en.wikipedia.org/wiki/Directed_acyclic_graph)
- 🔗 [Topological sorting](https://en.wikipedia.org/wiki/Topological_sorting)
- 🔗 [C# and .NET documentation](https://learn.microsoft.com/dotnet/)

Take it one concept at a time. Start with vertices and edges, then direction, then DAGs, and topological sort will feel much more natural.

## Project Structure

Shared repository configuration is captured in `Directory.Build.props`, and code style enforcement lives in `.editorconfig`.

```text
GraphLib/
├─ README.md
├─ GraphLib.slnx
├─ Directory.Build.props
├─ .editorconfig
├─ src/
│  └─ GraphLib/
│     ├─ CycleDetectedException.cs
│     ├─ DirectedAcyclicGraph.cs
│     ├─ DirectedGraph.cs
│     ├─ Graph.cs
│     └─ GraphLib.csproj
└─ tests/
   └─ GraphLib.Tests/
      ├─ DirectedAcyclicGraphTests.cs
      ├─ DirectedGraphTests.cs
      ├─ GraphTests.cs
      └─ GraphLib.Tests.csproj
```

## Contributing

Contributions are welcome. If you would like to improve the library, add more algorithms, or strengthen the documentation, feel free to open an issue or submit a pull request.

For now, a good contribution checklist is:

- discuss the change clearly
- keep the API intuitive
- add or update tests
- update documentation when behavior changes

## License

License information has not been added yet. Treat this as a placeholder section until the project owner chooses and publishes a license.
