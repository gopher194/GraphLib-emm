using GraphLib;
using System.Linq;

namespace GraphLib.Tests;

public class GraphTests
{
    [Fact]
    public void AddVertex_WithUniqueValue_AddsVertexAndUpdatesCounts()
    {
        // Arrange
        var graph = CreateGraph<string>();

        // Act
        graph.AddVertex("A");

        // Assert
        Assert.True(graph.HasVertex("A"));
        Assert.Equal(1, graph.VertexCount);
        Assert.Equal(new[] { "A" }, graph.GetVertices());
    }

    [Fact]
    public void AddVertex_WithDuplicate_ThrowsArgumentException()
    {
        // Arrange
        var graph = CreateGraph<int>();
        graph.AddVertex(1);

        // Act & Assert
        Assert.Throws<ArgumentException>(() => graph.AddVertex(1));
    }

    [Fact]
    public void AddVertex_WithNullReference_ThrowsArgumentNullException()
    {
        // Arrange
        var graph = CreateGraph<string>();
        string? vertex = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph.AddVertex(vertex!));
    }

    [Fact]
    public void RemoveVertex_WithExistingVertex_RemovesVertexAndIncidentEdges()
    {
        // Arrange
        var graph = CreateGraph<string>();
        graph.AddVertex("A");
        graph.AddVertex("B");
        graph.AddEdge("A", "B");

        // Act
        var removed = graph.RemoveVertex("A");

        // Assert
        Assert.True(removed);
        Assert.False(graph.HasVertex("A"));
        Assert.False(graph.HasEdge("A", "B"));
        Assert.False(graph.HasEdge("B", "A"));
        Assert.Equal(1, graph.VertexCount);
        Assert.Equal(0, graph.EdgeCount);
    }

    [Fact]
    public void RemoveVertex_WithMissingVertex_ReturnsFalse()
    {
        // Arrange
        var graph = CreateGraph<int>();

        // Act
        var removed = graph.RemoveVertex(42);

        // Assert
        Assert.False(removed);
        Assert.Equal(0, graph.VertexCount);
    }

    [Fact]
    public void RemoveVertex_WithNullReference_ThrowsArgumentNullException()
    {
        // Arrange
        var graph = CreateGraph<string>();
        string? vertex = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph.RemoveVertex(vertex!));
    }

    [Fact]
    public void AddEdge_WithExistingVertices_AddsUndirectedEdge()
    {
        // Arrange
        var graph = CreateGraph<string>();
        graph.AddVertex("A");
        graph.AddVertex("B");

        // Act
        graph.AddEdge("A", "B");

        // Assert
        Assert.True(graph.HasEdge("A", "B"));
        Assert.True(graph.HasEdge("B", "A"));
        Assert.Equal(1, graph.EdgeCount);

        var neighborsOfA = graph.GetNeighbors("A");
        var neighborsOfB = graph.GetNeighbors("B");

        Assert.Contains("B", neighborsOfA);
        Assert.Contains("A", neighborsOfB);
    }

    [Fact]
    public void AddEdge_WithMissingVertex_ThrowsInvalidOperationException()
    {
        // Arrange
        var graph = CreateGraph<int>();
        graph.AddVertex(1);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => graph.AddEdge(1, 2));
        Assert.Equal(1, graph.VertexCount);
        Assert.Equal(0, graph.EdgeCount);
    }

    [Fact]
    public void AddEdge_WithDuplicateEdge_ThrowsInvalidOperationException()
    {
        // Arrange
        var graph = CreateGraph<int>();
        graph.AddVertex(1);
        graph.AddVertex(2);
        graph.AddEdge(1, 2);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => graph.AddEdge(1, 2));
    }

    [Theory]
    [InlineData(true, false)] // null from
    [InlineData(false, true)] // null to
    public void AddEdge_WithNullEndpoints_ThrowsArgumentNullException(bool nullFrom, bool nullTo)
    {
        // Arrange
        var graph = CreateGraph<string>();
        graph.AddVertex("A");
        graph.AddVertex("B");
        string? from = nullFrom ? null : "A";
        string? to = nullTo ? null : "B";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph.AddEdge(from!, to!));
    }

    [Fact]
    public void AddEdge_WithSelfLoop_ThrowsArgumentException()
    {
        // Arrange
        var graph = CreateGraph<string>();
        graph.AddVertex("A");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => graph.AddEdge("A", "A"));
    }

    [Fact]
    public void RemoveEdge_WithExistingEdge_RemovesFromBothDirections()
    {
        // Arrange
        var graph = CreateGraph<int>();
        graph.AddVertex(1);
        graph.AddVertex(2);
        graph.AddEdge(1, 2);

        // Act
        var removed = graph.RemoveEdge(1, 2);

        // Assert
        Assert.True(removed);
        Assert.False(graph.HasEdge(1, 2));
        Assert.False(graph.HasEdge(2, 1));
        Assert.Equal(0, graph.EdgeCount);
    }

    [Fact]
    public void RemoveEdge_WithMissingEdge_ReturnsFalse()
    {
        // Arrange
        var graph = CreateGraph<string>();
        graph.AddVertex("A");
        graph.AddVertex("B");

        // Act
        var removed = graph.RemoveEdge("A", "B");

        // Assert
        Assert.False(removed);
        Assert.Equal(0, graph.EdgeCount);
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void RemoveEdge_WithNullEndpoints_ThrowsArgumentNullException(bool nullFrom, bool nullTo)
    {
        // Arrange
        var graph = CreateGraph<string>();
        graph.AddVertex("A");
        graph.AddVertex("B");
        graph.AddEdge("A", "B");
        string? from = nullFrom ? null : "A";
        string? to = nullTo ? null : "B";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph.RemoveEdge(from!, to!));
    }

    [Fact]
    public void HasVertex_WithNonexistentVertex_ReturnsFalse()
    {
        // Arrange
        var graph = CreateGraph<string>();

        // Act
        var result = graph.HasVertex("missing");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void HasEdge_WithNonexistentEdge_ReturnsFalse()
    {
        // Arrange
        var graph = CreateGraph<int>();
        graph.AddVertex(1);
        graph.AddVertex(2);

        // Act
        var result = graph.HasEdge(1, 2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetNeighbors_WithExistingVertex_ReturnsAllUndirectedNeighbors()
    {
        // Arrange
        var graph = CreateGraph<string>();
        graph.AddVertex("A");
        graph.AddVertex("B");
        graph.AddVertex("C");
        graph.AddEdge("A", "B");
        graph.AddEdge("A", "C");

        // Act
        var neighbors = graph.GetNeighbors("A").OrderBy(x => x).ToList();

        // Assert
        Assert.Equal(new[] { "B", "C" }, neighbors);
    }

    [Fact]
    public void GetNeighbors_WithMissingVertex_ThrowsInvalidOperationException()
    {
        // Arrange
        var graph = CreateGraph<int>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => graph.GetNeighbors(1));
    }

    [Fact]
    public void GetNeighbors_WithNullVertex_ThrowsArgumentNullException()
    {
        // Arrange
        var graph = CreateGraph<string>();
        string? vertex = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => graph.GetNeighbors(vertex!));
    }

    [Fact]
    public void VertexCount_OnEmptyGraph_ReturnsZero()
    {
        // Arrange
        var graph = CreateGraph<int>();

        // Act & Assert
        Assert.Equal(0, graph.VertexCount);
    }

    [Fact]
    public void EdgeCount_OnEmptyGraph_ReturnsZero()
    {
        // Arrange
        var graph = CreateGraph<int>();

        // Act & Assert
        Assert.Equal(0, graph.EdgeCount);
    }

    [Fact]
    public void GetVertices_OnEmptyGraph_ReturnsEmptySequence()
    {
        // Arrange
        var graph = CreateGraph<string>();

        // Act
        var vertices = graph.GetVertices();

        // Assert
        Assert.Empty(vertices);
    }

    [Fact]
    public void RemoveVertex_RemovesAssociatedEdgesAndUpdatesEdgeCount()
    {
        // Arrange
        var graph = CreateGraph<int>();
        graph.AddVertex(1);
        graph.AddVertex(2);
        graph.AddVertex(3);
        graph.AddEdge(1, 2);
        graph.AddEdge(2, 3);

        // Act
        graph.RemoveVertex(2);

        // Assert
        Assert.False(graph.HasEdge(1, 2));
        Assert.False(graph.HasEdge(2, 3));
        Assert.Equal(0, graph.EdgeCount);
    }

    [Fact]
    public void EdgeCount_UndirectedEdgesCountOnce()
    {
        // Arrange
        var graph = CreateGraph<string>();
        graph.AddVertex("A");
        graph.AddVertex("B");
        graph.AddVertex("C");

        // Act
        graph.AddEdge("A", "B");
        graph.AddEdge("B", "C");

        // Assert
        Assert.Equal(2, graph.EdgeCount);
        Assert.True(graph.HasEdge("B", "A"));
    }

    [Fact]
    public void GetVertices_ReturnsAllVerticesRegardlessOfType()
    {
        // Arrange
        var graph = CreateGraph<int>();
        graph.AddVertex(10);
        graph.AddVertex(20);
        graph.AddVertex(30);

        // Act
        var vertices = graph.GetVertices().OrderBy(x => x).ToList();

        // Assert
        Assert.Equal(new[] { 10, 20, 30 }, vertices);
    }

    [Fact]
    public void Graph_SupportsReferenceTypeVertices()
    {
        // Arrange
        var graph = CreateGraph<string>();

        // Act
        graph.AddVertex("node");

        // Assert
        Assert.True(graph.HasVertex("node"));
    }

    [Fact]
    public void Graph_SupportsValueTypeVertices()
    {
        // Arrange
        var graph = CreateGraph<int>();

        // Act
        graph.AddVertex(99);

        // Assert
        Assert.True(graph.HasVertex(99));
    }

    [Fact]
    public void RemoveEdge_WhenGraphIsEmpty_ReturnsFalse()
    {
        // Arrange
        var graph = CreateGraph<int>();

        // Act
        var result = graph.RemoveEdge(1, 2);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void RemoveVertex_WhenGraphIsEmpty_ReturnsFalse()
    {
        // Arrange
        var graph = CreateGraph<string>();

        // Act
        var result = graph.RemoveVertex("ghost");

        // Assert
        Assert.False(result);
    }

    private static Graph<T> CreateGraph<T>() where T : notnull => new TestGraph<T>();

    private sealed class TestGraph<T> : Graph<T> where T : notnull
    {
    }
}
