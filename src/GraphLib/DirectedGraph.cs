namespace GraphLib;

/// <summary>
/// Represents a directed graph whose edges have distinct source and destination vertices.
/// </summary>
/// <typeparam name="T">The type used to identify vertices in the graph.</typeparam>
/// <remarks>
/// Outgoing edges are stored in the inherited adjacency map, while incoming edges are tracked in a
/// separate reverse adjacency map for predecessor and in-degree queries.
/// </remarks>
public class DirectedGraph<T> : Graph<T> where T : notnull
{
    private readonly Dictionary<T, HashSet<T>> _reverseAdjacency = new();

    /// <inheritdoc/>
    public override void AddVertex(T vertex)
    {
        base.AddVertex(vertex);
        _reverseAdjacency.Add(vertex, new HashSet<T>());
    }

    /// <inheritdoc cref="Graph{T}.RemoveVertex(T)"/>
    /// <remarks>
    /// Removing a vertex from a directed graph deletes both incoming and outgoing edges for that
    /// vertex.
    /// </remarks>
    public override bool RemoveVertex(T vertex)
    {
        ArgumentNullException.ThrowIfNull(vertex);

        if (!_adjacency.TryGetValue(vertex, out var successors) || !_reverseAdjacency.TryGetValue(vertex, out var predecessors))
        {
            return false;
        }

        foreach (var successor in successors.ToList())
        {
            _reverseAdjacency[successor].Remove(vertex);
        }

        foreach (var predecessor in predecessors.ToList())
        {
            _adjacency[predecessor].Remove(vertex);
        }

        _adjacency.Remove(vertex);
        _reverseAdjacency.Remove(vertex);
        return true;
    }

    /// <inheritdoc cref="Graph{T}.AddEdge(T, T)"/>
    /// <remarks>
    /// A directed graph stores only the edge from <paramref name="from"/> to <paramref name="to"/>.
    /// No reciprocal edge is added automatically.
    /// </remarks>
    public override void AddEdge(T from, T to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        if (EqualityComparer<T>.Default.Equals(from, to))
        {
            throw new ArgumentException("Self-loops are not allowed.");
        }

        if (!_adjacency.TryGetValue(from, out var successors) || !_reverseAdjacency.TryGetValue(to, out var predecessors))
        {
            throw new InvalidOperationException("Both vertices must exist before adding an edge.");
        }

        if (!successors.Add(to))
        {
            throw new InvalidOperationException("The edge already exists in the graph.");
        }

        predecessors.Add(from);
    }

    /// <inheritdoc cref="Graph{T}.RemoveEdge(T, T)"/>
    /// <remarks>
    /// Only the directed edge from <paramref name="from"/> to <paramref name="to"/> is removed.
    /// </remarks>
    public override bool RemoveEdge(T from, T to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        if (!_adjacency.TryGetValue(from, out var successors) || !_reverseAdjacency.TryGetValue(to, out var predecessors))
        {
            return false;
        }

        if (!successors.Remove(to))
        {
            return false;
        }

        predecessors.Remove(from);
        return true;
    }

    /// <summary>
    /// Gets the number of directed edges currently stored in the graph.
    /// </summary>
    /// <value>The total number of directed edges in the graph.</value>
    public override int EdgeCount => _adjacency.Values.Sum(successors => successors.Count);

    /// <inheritdoc cref="Graph{T}.GetNeighbors(T)"/>
    /// <remarks>
    /// In a directed graph, neighbors are the successors that can be reached through outgoing edges.
    /// </remarks>
    public override IReadOnlyCollection<T> GetNeighbors(T vertex) => GetSuccessors(vertex);

    /// <summary>
    /// Gets the vertices reachable from the specified vertex through outgoing edges.
    /// </summary>
    /// <param name="vertex">The vertex whose successors should be returned.</param>
    /// <returns>A collection containing the outgoing neighbors of <paramref name="vertex"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="vertex"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="vertex"/> does not exist in the graph.</exception>
    public IReadOnlyCollection<T> GetSuccessors(T vertex)
    {
        ArgumentNullException.ThrowIfNull(vertex);

        if (!_adjacency.TryGetValue(vertex, out var successors))
        {
            throw new InvalidOperationException("The vertex does not exist in the graph.");
        }

        return successors;
    }

    /// <summary>
    /// Gets the vertices that have outgoing edges to the specified vertex.
    /// </summary>
    /// <param name="vertex">The vertex whose predecessors should be returned.</param>
    /// <returns>A collection containing the incoming neighbors of <paramref name="vertex"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="vertex"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="vertex"/> does not exist in the graph.</exception>
    public IReadOnlyCollection<T> GetPredecessors(T vertex)
    {
        ArgumentNullException.ThrowIfNull(vertex);

        if (!_reverseAdjacency.TryGetValue(vertex, out var predecessors))
        {
            throw new InvalidOperationException("The vertex does not exist in the graph.");
        }

        return predecessors;
    }

    /// <summary>
    /// Gets the number of incoming edges for the specified vertex.
    /// </summary>
    /// <param name="vertex">The vertex whose in-degree should be returned.</param>
    /// <returns>The number of incoming edges for <paramref name="vertex"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="vertex"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="vertex"/> does not exist in the graph.</exception>
    public int InDegree(T vertex)
    {
        ArgumentNullException.ThrowIfNull(vertex);

        if (!_reverseAdjacency.TryGetValue(vertex, out var predecessors))
        {
            throw new InvalidOperationException("The vertex does not exist in the graph.");
        }

        return predecessors.Count;
    }

    /// <summary>
    /// Gets the number of outgoing edges for the specified vertex.
    /// </summary>
    /// <param name="vertex">The vertex whose out-degree should be returned.</param>
    /// <returns>The number of outgoing edges for <paramref name="vertex"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="vertex"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="vertex"/> does not exist in the graph.</exception>
    public int OutDegree(T vertex)
    {
        ArgumentNullException.ThrowIfNull(vertex);

        if (!_adjacency.TryGetValue(vertex, out var successors))
        {
            throw new InvalidOperationException("The vertex does not exist in the graph.");
        }

        return successors.Count;
    }
}
