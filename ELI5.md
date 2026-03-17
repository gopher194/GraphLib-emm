# 🧒 ELI5: What Is a Cyclical Graph in .NET?

## The Simple Version

Imagine you and your friends standing in a circle, each pointing to the next person. You point to Alice, Alice points to Bob, Bob points back to **you**. You can follow the arrows and end up right back where you started — that's a **cycle**. A graph that has one of these loops is a **cyclical graph**.

## In .NET Terms

A **graph** is a collection of dots (**nodes**) connected by lines (**edges**). .NET has no built-in `Graph<T>` class, so developers typically build graphs using `Dictionary<T, HashSet<T>>` as an adjacency list.

A graph is **cyclical** when you can follow edges and return to where you started:

```
A → B → C → A   (that's a cycle!)
```

A graph with **no** cycles is called a **Directed Acyclic Graph (DAG)** — think of a family tree where you can never be your own grandparent.

## Where Cycles Show Up in .NET

| Area | How Cycles Appear | How .NET Handles It |
|---|---|---|
| **Garbage Collection** | Object A references B, B references A | GC traces from roots — unreachable cycles get collected just fine ✅ |
| **JSON Serialization** | `Blog → Posts → Blog` | `System.Text.Json` throws by default; use `ReferenceHandler.Preserve` (adds `$id`/`$ref`) or `IgnoreCycles` (writes `null`) |
| **EF Core** | Navigation properties create `Blog ↔ Posts` cycles | Use `[JsonIgnore]` or `ReferenceHandler.IgnoreCycles` |
| **Dependency Injection** | Service A needs B, B needs A | DI container **detects the cycle and throws** — refuses to build the object graph |
| **NuGet** | Package dependency graphs | Resolved via topological ordering; cycles would break restore |

## Why Cycles Matter

- **Infinite loops** — walking a cycle forever without knowing you've been there before
- **Stack overflows** — recursive traversal without cycle detection blows up the call stack
- **Serialization crashes** — `JsonException: "A possible object cycle was detected"`

## Common Algorithms for Dealing with Cycles

### DFS Cycle Detection

Walk the graph depth-first. Keep track of nodes on your **current path**. If you visit a node that's already on the current path, you found a cycle.

```
visiting: A → B → C → A  ← already on the path! Cycle detected!
```

### Topological Sort

Line up all nodes so that every dependency comes **before** the thing that needs it. This is only possible if there are **no cycles** — which is why DAGs are so useful for task scheduling, build systems, and dependency resolution.

## How This GraphLib Implements It

This repository contains a three-tier class hierarchy that demonstrates the full spectrum of cycle handling:

### 1. `Graph<T>` — Undirected, Cycles Allowed

The base class. Edges go both ways (`A—B` means A connects to B *and* B connects to A). Cycles are naturally allowed — any triangle of connected nodes is a cycle.

### 2. `DirectedGraph<T>` — Directed, Cycles Allowed

Edges have direction (`A → B` does not imply `B → A`). Cycles like `A → B → A` are perfectly valid. Tracks both successors (outgoing) and predecessors (incoming) via separate adjacency maps.

### 3. `DirectedAcyclicGraph<T>` — Directed, Cycles **Forbidden**

The strictest form. Every call to `AddEdge()` runs a **DFS-based cycle check** — if the new edge would create a cycle, it throws a `CycleDetectedException` and the edge is rejected. Because cycles are impossible, this class can offer `TopologicalSort()` using Kahn's algorithm.

```
           Graph<T>              ← undirected, cycles OK
              │
      DirectedGraph<T>           ← directed, cycles OK
              │
  DirectedAcyclicGraph<T>        ← directed, cycles REJECTED
```

Nearly **half the test suite** (38 of 78 tests) is dedicated to validating cycle behavior — direct cycles, indirect cycles, self-loops, long chains, and ensuring diamond-shaped (non-cyclic) patterns are *not* falsely rejected.

## Further Reading (MS Learn)

- [.NET Garbage Collection Fundamentals](https://learn.microsoft.com/en-us/dotnet/standard/garbage-collection/fundamentals) — how GC handles cyclical object references
- [Preserve References in System.Text.Json](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/preserve-references) — handling cycles during JSON serialization
- [.NET Dependency Injection](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection) — how the DI container resolves (and rejects) dependency graphs
- [NuGet Dependency Resolution](https://learn.microsoft.com/en-us/nuget/concepts/dependency-resolution) — how NuGet resolves package dependency graphs
