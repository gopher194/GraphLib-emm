using System.Diagnostics.CodeAnalysis;

namespace GraphLib;

/// <summary>
/// Represents a graph whose vertices are unique, non-null values of type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The type used to identify vertices in the graph.</typeparam>
/// <remarks>
/// The base implementation models an undirected graph by storing each edge in both connected
/// vertices' adjacency sets. Derived types can override members to provide different edge semantics.
/// </remarks>
public abstract class Graph<T> where T : notnull
{
    /// <summary>
    /// Maps each vertex to the set of vertices directly connected to it.
    /// </summary>
    /// <remarks>
    /// In the base implementation, each edge is stored in both connected vertices' adjacency sets.
    /// Derived types can reuse this dictionary to represent outgoing edges.
    /// </remarks>
    [SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "Derived graph types share the established protected adjacency storage.")]
    protected readonly Dictionary<T, HashSet<T>> _adjacency = new();

    /// <summary>
    /// Adds a vertex to the graph.
    /// </summary>
    /// <param name="vertex">The vertex to add.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="vertex"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="vertex"/> already exists in the graph.</exception>
    public virtual void AddVertex(T vertex)
    {
        ArgumentNullException.ThrowIfNull(vertex);

        if (_adjacency.ContainsKey(vertex))
        {
            throw new ArgumentException("The vertex already exists in the graph.", nameof(vertex));
        }

        _adjacency.Add(vertex, new HashSet<T>());
    }

    /// <summary>
    /// Removes a vertex and every edge connected to it.
    /// </summary>
    /// <param name="vertex">The vertex to remove.</param>
    /// <returns><see langword="true"/> if the vertex existed and was removed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="vertex"/> is <see langword="null"/>.</exception>
    /// <remarks>
    /// The current neighbor set is materialized before back-links are removed so the method does not
    /// modify a collection while it is being enumerated.
    /// </remarks>
    public virtual bool RemoveVertex(T vertex)
    {
        ArgumentNullException.ThrowIfNull(vertex);

        if (!_adjacency.TryGetValue(vertex, out var neighbors))
        {
            return false;
        }

        foreach (var neighbor in neighbors.ToList())
        {
            _adjacency[neighbor].Remove(vertex);
        }

        _adjacency.Remove(vertex);
        return true;
    }

    /// <summary>
    /// Adds an undirected edge between two existing vertices.
    /// </summary>
    /// <param name="from">One endpoint of the edge.</param>
    /// <param name="to">The other endpoint of the edge.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="from"/> or <paramref name="to"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="from"/> and <paramref name="to"/> identify the same vertex.</exception>
    /// <exception cref="InvalidOperationException">Thrown when either vertex does not exist or the edge already exists.</exception>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Public API is established and must remain unchanged.")]
    public virtual void AddEdge(T from, T to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        if (EqualityComparer<T>.Default.Equals(from, to))
        {
            throw new ArgumentException("Self-loops are not allowed.");
        }

        if (!_adjacency.TryGetValue(from, out var fromNeighbors) || !_adjacency.TryGetValue(to, out var toNeighbors))
        {
            throw new InvalidOperationException("Both vertices must exist before adding an edge.");
        }

        if (!fromNeighbors.Add(to))
        {
            throw new InvalidOperationException("The edge already exists in the graph.");
        }

        toNeighbors.Add(from);
    }

    /// <summary>
    /// Removes an undirected edge between two vertices.
    /// </summary>
    /// <param name="from">One endpoint of the edge to remove.</param>
    /// <param name="to">The other endpoint of the edge to remove.</param>
    /// <returns><see langword="true"/> if the edge existed and was removed; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="from"/> or <paramref name="to"/> is <see langword="null"/>.</exception>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Public API is established and must remain unchanged.")]
    public virtual bool RemoveEdge(T from, T to)
    {
        ArgumentNullException.ThrowIfNull(from);
        ArgumentNullException.ThrowIfNull(to);

        if (!_adjacency.TryGetValue(from, out var fromNeighbors) || !_adjacency.TryGetValue(to, out var toNeighbors))
        {
            return false;
        }

        if (!fromNeighbors.Remove(to))
        {
            return false;
        }

        toNeighbors.Remove(from);
        return true;
    }

    /// <summary>
    /// Determines whether the graph contains the specified vertex.
    /// </summary>
    /// <param name="vertex">The vertex to locate.</param>
    /// <returns><see langword="true"/> if <paramref name="vertex"/> exists in the graph; otherwise, <see langword="false"/>.</returns>
    public virtual bool HasVertex(T vertex) => vertex is not null && _adjacency.ContainsKey(vertex);

    /// <summary>
    /// Determines whether an edge exists between two vertices.
    /// </summary>
    /// <param name="from">One endpoint to evaluate.</param>
    /// <param name="to">The other endpoint to evaluate.</param>
    /// <returns><see langword="true"/> if an edge exists between the specified vertices; otherwise, <see langword="false"/>.</returns>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Public API is established and must remain unchanged.")]
    public virtual bool HasEdge(T from, T to) =>
        from is not null &&
        to is not null &&
        _adjacency.TryGetValue(from, out var neighbors) &&
        neighbors.Contains(to);

    /// <summary>
    /// Gets the number of vertices currently stored in the graph.
    /// </summary>
    /// <value>The total number of vertices in the graph.</value>
    public virtual int VertexCount => _adjacency.Count;

    /// <summary>
    /// Gets the number of undirected edges currently stored in the graph.
    /// </summary>
    /// <value>The total number of edges in the graph.</value>
    public virtual int EdgeCount => _adjacency.Values.Sum(neighbors => neighbors.Count) / 2;

    /// <summary>
    /// Gets a snapshot of the vertices currently stored in the graph.
    /// </summary>
    /// <value>A collection containing the current vertices.</value>
    public virtual IReadOnlyCollection<T> Vertices => GetVertices();

    /// <summary>
    /// Returns a snapshot of the vertices currently stored in the graph.
    /// </summary>
    /// <returns>A collection containing the current vertices.</returns>
    public virtual IReadOnlyCollection<T> GetVertices() => _adjacency.Keys.ToList();

    /// <summary>
    /// Gets the vertices adjacent to the specified vertex.
    /// </summary>
    /// <param name="vertex">The vertex whose adjacent vertices should be returned.</param>
    /// <returns>A collection containing the vertices adjacent to <paramref name="vertex"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="vertex"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when <paramref name="vertex"/> does not exist in the graph.</exception>
    /// <remarks>
    /// In the base undirected graph, the returned neighbors are all vertices that share an edge with
    /// <paramref name="vertex"/>.
    /// </remarks>
    public virtual IReadOnlyCollection<T> GetNeighbors(T vertex)
    {
        ArgumentNullException.ThrowIfNull(vertex);

        if (!_adjacency.TryGetValue(vertex, out var neighbors))
        {
            throw new InvalidOperationException("The vertex does not exist in the graph.");
        }

        return neighbors;
    }
}
