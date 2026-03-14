namespace GraphLib;

/// <summary>
/// Represents a directed acyclic graph (DAG), which is a directed graph whose edges
/// never allow a vertex to be reached again by following outgoing edges.
/// </summary>
/// <typeparam name="T">The type used to identify vertices stored in the graph.</typeparam>
/// <remarks>
/// This implementation preserves acyclicity by rejecting self-loops and any edge that
/// would create a path back to its source vertex. Because the graph always remains acyclic,
/// <see cref="TopologicalSort"/> can produce an ordering in which each vertex appears
/// before all of its successors.
/// </remarks>
public class DirectedAcyclicGraph<T> : DirectedGraph<T> where T : notnull
{
    /// <inheritdoc/>
    /// <remarks>
    /// In addition to the exceptions inherited from <see cref="DirectedGraph{T}.AddEdge(T, T)"/>,
    /// this override validates that the proposed edge would not introduce a cycle. Self-loops
    /// are treated as cycles and therefore also result in <see cref="CycleDetectedException"/>.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="from"/> or <paramref name="to"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when either vertex does not exist in the graph or when the edge already exists.
    /// </exception>
    /// <exception cref="CycleDetectedException">
    /// Thrown when adding the edge would create a cycle, including the special case where
    /// <paramref name="from"/> and <paramref name="to"/> represent the same vertex.
    /// </exception>
    public override void AddEdge(T from, T to)
    {
        if (from is not null && to is not null && EqualityComparer<T>.Default.Equals(from, to))
        {
            throw new CycleDetectedException($"Adding edge from '{from}' to '{to}' would create a cycle (self-loop).");
        }

        if (from is not null && to is not null
            && HasVertex(from) && HasVertex(to)
            && !HasEdge(from, to)
            && WouldCreateCycle(from, to))
        {
            throw new CycleDetectedException($"Adding edge from '{from}' to '{to}' would create a cycle.");
        }

        base.AddEdge(from!, to!);
    }

    /// <summary>
    /// Returns a topological ordering of all vertices in the graph.
    /// </summary>
    /// <returns>
    /// A read-only list that contains each vertex exactly once, ordered so every vertex
    /// appears before all of its successors.
    /// </returns>
    /// <remarks>
    /// This method uses Kahn's algorithm. It starts with every vertex whose in-degree is zero,
    /// repeatedly removes one such vertex from the working queue, and then decreases the in-degree
    /// of each successor. Because <see cref="AddEdge(T, T)"/> prevents cycles, the returned ordering
    /// always includes all vertices in the graph.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the graph contains no vertices.
    /// </exception>
    public IReadOnlyList<T> TopologicalSort()
    {
        if (VertexCount == 0)
        {
            throw new InvalidOperationException("Cannot perform topological sort on an empty graph.");
        }

        var inDegrees = new Dictionary<T, int>(VertexCount);
        var queue = new Queue<T>();

        foreach (var vertex in GetVertices())
        {
            var inDegree = InDegree(vertex);
            inDegrees[vertex] = inDegree;

            if (inDegree == 0)
            {
                queue.Enqueue(vertex);
            }
        }

        var result = new List<T>(VertexCount);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);

            foreach (var successor in GetSuccessors(current))
            {
                inDegrees[successor]--;
                if (inDegrees[successor] == 0)
                {
                    queue.Enqueue(successor);
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Determines whether adding an edge from one vertex to another would introduce a cycle.
    /// </summary>
    /// <param name="from">The proposed source vertex.</param>
    /// <param name="to">The proposed destination vertex.</param>
    /// <returns>
    /// <see langword="true"/> if a path already exists from <paramref name="to"/> to
    /// <paramref name="from"/>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The cycle check performs a depth-first search starting at <paramref name="to"/>.
    /// If <paramref name="from"/> is reachable, adding the proposed edge would close a cycle.
    /// </remarks>
    private bool WouldCreateCycle(T from, T to) => HasPath(to, from, new HashSet<T>());

    /// <summary>
    /// Determines whether a directed path exists from one vertex to another.
    /// </summary>
    /// <param name="current">The vertex currently being explored.</param>
    /// <param name="target">The destination vertex being searched for.</param>
    /// <param name="visited">The set of vertices that have already been visited during the search.</param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="target"/> is reachable from <paramref name="current"/>;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// This helper uses depth-first search and tracks visited vertices to avoid revisiting nodes
    /// while traversing the graph.
    /// </remarks>
    private bool HasPath(T current, T target, HashSet<T> visited)
    {
        if (EqualityComparer<T>.Default.Equals(current, target))
        {
            return true;
        }

        if (!visited.Add(current))
        {
            return false;
        }

        foreach (var successor in GetSuccessors(current))
        {
            if (HasPath(successor, target, visited))
            {
                return true;
            }
        }

        return false;
    }
}
