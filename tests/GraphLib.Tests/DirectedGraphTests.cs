using GraphLib;
using System.Linq;

namespace GraphLib.Tests;

public class DirectedGraphTests
{
    [Fact]
    public void AddEdge_WithExistingVertices_CreatesDirectedEdgeOnly()
    {
        // Arrange
        var graph = new DirectedGraph<string>();
        graph.AddVertex("A");
        graph.AddVertex("B");

        // Act
        graph.AddEdge("A", "B");

        // Assert
        Assert.True(graph.HasEdge("A", "B"));
        Assert.False(graph.HasEdge("B", "A"));
    }

    [Fact]
    public void GetSuccessors_WithOutgoingAndIncomingEdges_ReturnsOnlyOutgoingNeighbors()
    {
        // Arrange
        var graph = new DirectedGraph<int>();
        graph.AddVertex(1);
        graph.AddVertex(2);
        graph.AddVertex(3);
        graph.AddVertex(4);
        graph.AddEdge(1, 2);
        graph.AddEdge(1, 3);
        graph.AddEdge(4, 1);

        // Act
        var successors = graph.GetSuccessors(1).OrderBy(vertex => vertex).ToList();

        // Assert
        Assert.Equal(new[] { 2, 3 }, successors);
    }

    [Fact]
    public void GetSuccessors_WithMissingVertex_ThrowsInvalidOperationException()
    {
        // Arrange
        var graph = new DirectedGraph<int>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => graph.GetSuccessors(99));
    }

    [Fact]
    public void GetPredecessors_WithIncomingAndOutgoingEdges_ReturnsOnlyIncomingNeighbors()
    {
        // Arrange
        var graph = new DirectedGraph<int>();
        graph.AddVertex(1);
        graph.AddVertex(2);
        graph.AddVertex(3);
        graph.AddVertex(4);
        graph.AddEdge(2, 1);
        graph.AddEdge(3, 1);
        graph.AddEdge(1, 4);

        // Act
        var predecessors = graph.GetPredecessors(1).OrderBy(vertex => vertex).ToList();

        // Assert
        Assert.Equal(new[] { 2, 3 }, predecessors);
    }

    [Fact]
    public void GetPredecessors_WithMissingVertex_ThrowsInvalidOperationException()
    {
        // Arrange
        var graph = new DirectedGraph<string>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => graph.GetPredecessors("missing"));
    }

    [Theory]
    [InlineData(1, 0, 3)]
    [InlineData(2, 1, 1)]
    [InlineData(3, 2, 0)]
    [InlineData(4, 1, 0)]
    public void InDegreeAndOutDegree_WithDirectedEdges_ReturnExpectedCounts(int vertex, int expectedInDegree, int expectedOutDegree)
    {
        // Arrange
        var graph = new DirectedGraph<int>();
        graph.AddVertex(1);
        graph.AddVertex(2);
        graph.AddVertex(3);
        graph.AddVertex(4);
        graph.AddEdge(1, 2);
        graph.AddEdge(1, 3);
        graph.AddEdge(1, 4);
        graph.AddEdge(2, 3);

        // Act & Assert
        Assert.Equal(expectedInDegree, graph.InDegree(vertex));
        Assert.Equal(expectedOutDegree, graph.OutDegree(vertex));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void DegreeMethods_WithMissingVertex_ThrowInvalidOperationException(bool testInDegree)
    {
        // Arrange
        var graph = new DirectedGraph<int>();

        // Act & Assert
        if (testInDegree)
        {
            Assert.Throws<InvalidOperationException>(() => graph.InDegree(5));
            return;
        }

        Assert.Throws<InvalidOperationException>(() => graph.OutDegree(5));
    }

    [Fact]
    public void AddEdge_WithFanOutFromSingleVertex_TracksAllOutgoingEdges()
    {
        // Arrange
        var graph = new DirectedGraph<string>();
        graph.AddVertex("hub");
        graph.AddVertex("A");
        graph.AddVertex("B");
        graph.AddVertex("C");

        // Act
        graph.AddEdge("hub", "A");
        graph.AddEdge("hub", "B");
        graph.AddEdge("hub", "C");

        // Assert
        Assert.Equal(3, graph.OutDegree("hub"));
        Assert.Equal(new[] { "A", "B", "C" }, graph.GetSuccessors("hub").OrderBy(vertex => vertex));
    }

    [Fact]
    public void AddEdge_WithFanInToSingleVertex_TracksAllIncomingEdges()
    {
        // Arrange
        var graph = new DirectedGraph<string>();
        graph.AddVertex("sink");
        graph.AddVertex("A");
        graph.AddVertex("B");
        graph.AddVertex("C");

        // Act
        graph.AddEdge("A", "sink");
        graph.AddEdge("B", "sink");
        graph.AddEdge("C", "sink");

        // Assert
        Assert.Equal(3, graph.InDegree("sink"));
        Assert.Equal(new[] { "A", "B", "C" }, graph.GetPredecessors("sink").OrderBy(vertex => vertex));
    }

    [Fact]
    public void RemoveVertex_WithIncomingAndOutgoingEdges_UpdatesSuccessorsAndPredecessors()
    {
        // Arrange
        var graph = new DirectedGraph<int>();
        graph.AddVertex(1);
        graph.AddVertex(2);
        graph.AddVertex(3);
        graph.AddVertex(4);
        graph.AddEdge(1, 2);
        graph.AddEdge(3, 2);
        graph.AddEdge(2, 4);
        graph.AddEdge(1, 4);

        // Act
        var removed = graph.RemoveVertex(2);

        // Assert
        Assert.True(removed);
        Assert.False(graph.HasVertex(2));
        Assert.Equal(new[] { 4 }, graph.GetSuccessors(1));
        Assert.Empty(graph.GetSuccessors(3));
        Assert.Equal(new[] { 1 }, graph.GetPredecessors(4));
    }

    [Fact]
    public void AddEdge_WithSelfLoop_ThrowsArgumentException()
    {
        // Arrange
        var graph = new DirectedGraph<string>();
        graph.AddVertex("A");

        // Act & Assert
        Assert.Throws<ArgumentException>(() => graph.AddEdge("A", "A"));
    }

    [Fact]
    public void EdgeCount_WithTwoOppositeDirections_CountsBothDirectedEdges()
    {
        // Arrange
        var graph = new DirectedGraph<int>();
        graph.AddVertex(1);
        graph.AddVertex(2);

        // Act
        graph.AddEdge(1, 2);
        graph.AddEdge(2, 1);

        // Assert
        Assert.Equal(2, graph.EdgeCount);
    }

    [Fact]
    public void GetNeighbors_WithIncomingAndOutgoingEdges_ReturnsSuccessorsOnly()
    {
        // Arrange
        var graph = new DirectedGraph<int>();
        graph.AddVertex(1);
        graph.AddVertex(2);
        graph.AddVertex(3);
        graph.AddEdge(1, 2);
        graph.AddEdge(3, 1);

        // Act
        var neighbors = graph.GetNeighbors(1).ToList();

        // Assert
        Assert.Equal(new[] { 2 }, neighbors);
    }

    [Fact]
    public void AddVertex_WithUniqueVertex_UpdatesInheritedVertexTracking()
    {
        // Arrange
        var graph = new DirectedGraph<string>();

        // Act
        graph.AddVertex("vertex");

        // Assert
        Assert.True(graph.HasVertex("vertex"));
        Assert.Equal(1, graph.VertexCount);
        Assert.Equal(new[] { "vertex" }, graph.GetVertices());
    }

    [Fact]
    public void RemoveEdge_WithExistingDirectedEdge_RemovesOnlyThatDirection()
    {
        // Arrange
        var graph = new DirectedGraph<int>();
        graph.AddVertex(1);
        graph.AddVertex(2);
        graph.AddEdge(1, 2);
        graph.AddEdge(2, 1);

        // Act
        var removed = graph.RemoveEdge(1, 2);

        // Assert
        Assert.True(removed);
        Assert.False(graph.HasEdge(1, 2));
        Assert.True(graph.HasEdge(2, 1));
        Assert.Equal(1, graph.EdgeCount);
    }

    [Fact]
    public void RemoveVertex_WithExistingVertex_RemovesIncidentDirectedEdges()
    {
        // Arrange
        var graph = new DirectedGraph<int>();
        graph.AddVertex(1);
        graph.AddVertex(2);
        graph.AddVertex(3);
        graph.AddEdge(1, 2);
        graph.AddEdge(2, 3);

        // Act
        var removed = graph.RemoveVertex(2);

        // Assert
        Assert.True(removed);
        Assert.False(graph.HasEdge(1, 2));
        Assert.False(graph.HasEdge(2, 3));
        Assert.Equal(0, graph.EdgeCount);
    }
}
