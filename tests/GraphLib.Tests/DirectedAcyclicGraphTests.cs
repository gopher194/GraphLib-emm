using GraphLib;

namespace GraphLib.Tests;

public class DirectedAcyclicGraphTests
{
    // ========================================================================
    // Helper: assert that 'before' appears earlier than 'after' in the list
    // ========================================================================
    private static void AssertOrderedBefore<T>(IReadOnlyList<T> sorted, T before, T after)
        where T : notnull
    {
        int idxBefore = IndexOf(sorted, before);
        int idxAfter = IndexOf(sorted, after);

        Assert.True(idxBefore >= 0, $"Expected vertex '{before}' to be present in the sorted result.");
        Assert.True(idxAfter >= 0, $"Expected vertex '{after}' to be present in the sorted result.");
        Assert.True(idxBefore < idxAfter,
            $"Expected '{before}' (index {idxBefore}) to appear before '{after}' (index {idxAfter}).");
    }

    private static int IndexOf<T>(IReadOnlyList<T> list, T item) where T : notnull
    {
        for (int i = 0; i < list.Count; i++)
        {
            if (EqualityComparer<T>.Default.Equals(list[i], item))
                return i;
        }
        return -1;
    }

    private static void AssertMeaningfulCycleMessage(CycleDetectedException exception)
    {
        Assert.IsAssignableFrom<InvalidOperationException>(exception);
        Assert.False(string.IsNullOrWhiteSpace(exception.Message));
        Assert.Contains("cycle", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    // ========================================================================
    // Cycle Detection Tests
    // ========================================================================

    #region Cycle Detection

    [Fact]
    public void CycleDetectedException_InheritsInvalidOperationException()
    {
        var exception = new CycleDetectedException("Adding the edge would create a cycle.");

        Assert.IsAssignableFrom<InvalidOperationException>(exception);
    }

    [Fact]
    public void AddEdge_CreatingDirectCycle_ThrowsCycleDetectedException()
    {
        // Arrange
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");
        dag.AddEdge("A", "B");

        // Act & Assert — B→A completes a 2-node cycle
        var exception = Assert.Throws<CycleDetectedException>(() => dag.AddEdge("B", "A"));
        AssertMeaningfulCycleMessage(exception);
    }

    [Fact]
    public void AddEdge_CreatingIndirectCycle_ThrowsCycleDetectedException()
    {
        // Arrange
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");
        dag.AddVertex("C");
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        // Act & Assert — C→A completes a 3-node cycle
        var exception = Assert.Throws<CycleDetectedException>(() => dag.AddEdge("C", "A"));
        AssertMeaningfulCycleMessage(exception);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("Z")]
    [InlineData("self")]
    public void AddEdge_SelfLoop_ThrowsCycleDetectedException(string vertex)
    {
        // Arrange
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex(vertex);

        // Act & Assert — any vertex pointing to itself is a cycle
        var exception = Assert.Throws<CycleDetectedException>(() => dag.AddEdge(vertex, vertex));
        AssertMeaningfulCycleMessage(exception);
    }

    [Fact]
    public void AddEdge_CreatingLongChainCycle_ThrowsCycleDetectedException()
    {
        // Arrange — build chain A→B→C→D→E
        var dag = new DirectedAcyclicGraph<string>();
        foreach (var v in new[] { "A", "B", "C", "D", "E" })
            dag.AddVertex(v);

        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");
        dag.AddEdge("C", "D");
        dag.AddEdge("D", "E");

        // Act & Assert — E→A completes a 5-node cycle
        var exception = Assert.Throws<CycleDetectedException>(() => dag.AddEdge("E", "A"));
        AssertMeaningfulCycleMessage(exception);
    }

    [Fact]
    public void AddEdge_NotCreatingCycle_Succeeds()
    {
        // Arrange
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");
        dag.AddVertex("C");

        // Act — these form a valid DAG (A→B, A→C, B→C)
        dag.AddEdge("A", "B");
        dag.AddEdge("A", "C");
        dag.AddEdge("B", "C");

        // Assert
        Assert.True(dag.HasEdge("A", "B"));
        Assert.True(dag.HasEdge("A", "C"));
        Assert.True(dag.HasEdge("B", "C"));
    }

    [Fact]
    public void AddEdge_FailedCycleCreatingEdge_GraphStateUnchanged()
    {
        // Arrange
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");
        dag.AddEdge("A", "B");

        // Act — attempt to create cycle
        Assert.Throws<CycleDetectedException>(() => dag.AddEdge("B", "A"));

        // Assert — the failed edge was never persisted; original edge still present
        Assert.False(dag.HasEdge("B", "A"));
        Assert.True(dag.HasEdge("A", "B"));
        Assert.Contains("A", dag.GetPredecessors("B"));
        Assert.DoesNotContain("B", dag.GetPredecessors("A"));
    }

    [Fact]
    public void AddEdge_MultiplePaths_NoFalsePositiveCycleDetection()
    {
        // Arrange — diamond shape: A→B, A→C, B→D, C→D (multiple paths to D, no cycle)
        var dag = new DirectedAcyclicGraph<string>();
        foreach (var v in new[] { "A", "B", "C", "D" })
            dag.AddVertex(v);

        // Act — each edge should succeed without false cycle detection
        dag.AddEdge("A", "B");
        dag.AddEdge("A", "C");
        dag.AddEdge("B", "D");
        dag.AddEdge("C", "D");

        // Assert — all edges present, graph is valid
        Assert.True(dag.HasEdge("A", "B"));
        Assert.True(dag.HasEdge("A", "C"));
        Assert.True(dag.HasEdge("B", "D"));
        Assert.True(dag.HasEdge("C", "D"));
    }

    [Fact]
    public void AddEdge_ComplexGraphWithConvergingPaths_NoFalsePositive()
    {
        // A more complex graph: A→B, A→C, B→D, C→D, D→E, A→E
        // Multiple paths from A to E; none form a cycle.
        var dag = new DirectedAcyclicGraph<string>();
        foreach (var v in new[] { "A", "B", "C", "D", "E" })
            dag.AddVertex(v);

        dag.AddEdge("A", "B");
        dag.AddEdge("A", "C");
        dag.AddEdge("B", "D");
        dag.AddEdge("C", "D");
        dag.AddEdge("D", "E");
        dag.AddEdge("A", "E"); // second path to E

        Assert.True(dag.HasEdge("A", "E"));
        Assert.True(dag.HasEdge("D", "E"));
    }

    #endregion

    // ========================================================================
    // Topological Sort Tests
    // ========================================================================

    #region Topological Sort

    [Fact]
    public void TopologicalSort_LinearChain_ReturnsExactOrder()
    {
        // Arrange — A→B→C
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");
        dag.AddVertex("C");
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        // Act
        var result = dag.TopologicalSort();

        // Assert — only one valid topological order for a linear chain
        Assert.Equal(3, result.Count);
        Assert.Equal("A", result[0]);
        Assert.Equal("B", result[1]);
        Assert.Equal("C", result[2]);
    }

    [Fact]
    public void TopologicalSort_Diamond_RespectsAllOrderingConstraints()
    {
        // Arrange — A→B, A→C, B→D, C→D
        var dag = new DirectedAcyclicGraph<string>();
        foreach (var v in new[] { "A", "B", "C", "D" })
            dag.AddVertex(v);
        dag.AddEdge("A", "B");
        dag.AddEdge("A", "C");
        dag.AddEdge("B", "D");
        dag.AddEdge("C", "D");

        // Act
        var result = dag.TopologicalSort();

        // Assert — A before B and C; B and C before D; B/C order is flexible
        Assert.Equal(4, result.Count);
        AssertOrderedBefore(result, "A", "B");
        AssertOrderedBefore(result, "A", "C");
        AssertOrderedBefore(result, "B", "D");
        AssertOrderedBefore(result, "C", "D");
    }

    [Fact]
    public void TopologicalSort_SingleVertex_ReturnsSingleElement()
    {
        // Arrange
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("only");

        // Act
        var result = dag.TopologicalSort();

        // Assert
        Assert.Single(result);
        Assert.Equal("only", result[0]);
    }

    [Fact]
    public void TopologicalSort_DisconnectedComponents_AllVerticesPresent()
    {
        // Arrange — two disconnected components: A→B and C→D
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");
        dag.AddVertex("C");
        dag.AddVertex("D");
        dag.AddEdge("A", "B");
        dag.AddEdge("C", "D");

        // Act
        var result = dag.TopologicalSort();

        // Assert — all four vertices present; intra-component ordering preserved
        Assert.Equal(4, result.Count);
        Assert.Contains("A", result);
        Assert.Contains("B", result);
        Assert.Contains("C", result);
        Assert.Contains("D", result);
        AssertOrderedBefore(result, "A", "B");
        AssertOrderedBefore(result, "C", "D");
    }

    [Fact]
    public void TopologicalSort_ComplexDagWithMultipleValidOrderings_VerifiesConstraints()
    {
        // Arrange — A→B, A→C, B→D, C→D, B→E, D→E
        // Valid orderings include [A,B,C,D,E], [A,C,B,D,E], etc.
        var dag = new DirectedAcyclicGraph<string>();
        foreach (var v in new[] { "A", "B", "C", "D", "E" })
            dag.AddVertex(v);

        dag.AddEdge("A", "B");
        dag.AddEdge("A", "C");
        dag.AddEdge("B", "D");
        dag.AddEdge("C", "D");
        dag.AddEdge("B", "E");
        dag.AddEdge("D", "E");

        // Act
        var result = dag.TopologicalSort();

        // Assert — verify all edge constraints without mandating one specific order
        Assert.Equal(5, result.Count);
        AssertOrderedBefore(result, "A", "B");
        AssertOrderedBefore(result, "A", "C");
        AssertOrderedBefore(result, "B", "D");
        AssertOrderedBefore(result, "C", "D");
        AssertOrderedBefore(result, "B", "E");
        AssertOrderedBefore(result, "D", "E");
    }

    [Fact]
    public void TopologicalSort_EmptyGraph_ThrowsInvalidOperationException()
    {
        // Arrange
        var dag = new DirectedAcyclicGraph<string>();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => dag.TopologicalSort());
    }

    [Fact]
    public void TopologicalSort_ParallelEdgesFromSameSource_MaintainsOrderConstraints()
    {
        // Arrange — A fans out to B, C, D (no edges between B, C, D)
        var dag = new DirectedAcyclicGraph<string>();
        foreach (var v in new[] { "A", "B", "C", "D" })
            dag.AddVertex(v);

        dag.AddEdge("A", "B");
        dag.AddEdge("A", "C");
        dag.AddEdge("A", "D");

        // Act
        var result = dag.TopologicalSort();

        // Assert — A must appear before all its successors
        Assert.Equal(4, result.Count);
        AssertOrderedBefore(result, "A", "B");
        AssertOrderedBefore(result, "A", "C");
        AssertOrderedBefore(result, "A", "D");
    }

    [Theory]
    [InlineData(2)]
    [InlineData(5)]
    [InlineData(10)]
    public void TopologicalSort_LinearChainOfNVertices_ReturnsCorrectOrder(int chainLength)
    {
        // Arrange — build chain 0→1→2→...→(n-1)
        var dag = new DirectedAcyclicGraph<int>();
        for (int i = 0; i < chainLength; i++)
            dag.AddVertex(i);
        for (int i = 0; i < chainLength - 1; i++)
            dag.AddEdge(i, i + 1);

        // Act
        var result = dag.TopologicalSort();

        // Assert — exact order must be 0, 1, 2, ..., n-1
        Assert.Equal(chainLength, result.Count);
        for (int i = 0; i < chainLength; i++)
            Assert.Equal(i, result[i]);
    }

    #endregion

    // ========================================================================
    // Inherited Behavior Tests
    // ========================================================================

    #region Inherited Behavior

    [Fact]
    public void InheritedBehavior_DirectedEdges_SuccessorsAndPredecessorsCorrect()
    {
        // Arrange
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");
        dag.AddVertex("C");
        dag.AddEdge("A", "B");
        dag.AddEdge("A", "C");

        // Act & Assert — successors of A
        var successorsA = dag.GetSuccessors("A").ToList();
        Assert.Contains("B", successorsA);
        Assert.Contains("C", successorsA);
        Assert.Equal(2, successorsA.Count);

        // Predecessors of B and C
        Assert.Contains("A", dag.GetPredecessors("B"));
        Assert.Contains("A", dag.GetPredecessors("C"));

        // A has no predecessors; B and C have no successors
        Assert.Empty(dag.GetPredecessors("A"));
        Assert.Empty(dag.GetSuccessors("B"));
        Assert.Empty(dag.GetSuccessors("C"));
    }

    [Fact]
    public void InheritedBehavior_Vertices_ReturnsAllAddedVertices()
    {
        // Arrange
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("X");
        dag.AddVertex("Y");
        dag.AddVertex("Z");

        // Act
        var vertices = dag.Vertices.ToList();

        // Assert
        Assert.Equal(3, vertices.Count);
        Assert.Contains("X", vertices);
        Assert.Contains("Y", vertices);
        Assert.Contains("Z", vertices);
    }

    [Fact]
    public void InheritedBehavior_HasEdge_ReturnsFalseForNonexistentEdge()
    {
        // Arrange
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");
        dag.AddEdge("A", "B");

        // Act & Assert — directed: A→B exists, B→A does not
        Assert.True(dag.HasEdge("A", "B"));
        Assert.False(dag.HasEdge("B", "A"));
    }

    [Fact]
    public void RemoveEdge_AllowsReaddingPreviouslyCycleCreatingEdge()
    {
        // Arrange
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");
        dag.AddEdge("A", "B");

        // B→A would create a cycle
        Assert.Throws<CycleDetectedException>(() => dag.AddEdge("B", "A"));

        // Act — remove the blocking edge, then add the previously rejected one
        dag.RemoveEdge("A", "B");
        dag.AddEdge("B", "A");

        // Assert
        Assert.True(dag.HasEdge("B", "A"));
        Assert.False(dag.HasEdge("A", "B"));
    }

    [Fact]
    public void RemoveVertex_ThenReadd_CycleDetectionStillFunctions()
    {
        // Arrange — build chain A→B→C
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");
        dag.AddVertex("C");
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        // Act — remove middle vertex (and its edges), then re-add with reversed flow
        dag.RemoveVertex("B");

        dag.AddVertex("B");
        dag.AddEdge("C", "B"); // C→B
        dag.AddEdge("B", "A"); // B→A

        // Assert — now A→C would create cycle A→C→B→A
        Assert.Throws<CycleDetectedException>(() => dag.AddEdge("A", "C"));
    }

    [Fact]
    public void RemoveVertex_ThenReadd_ValidEdgesStillAllowed()
    {
        // Arrange
        var dag = new DirectedAcyclicGraph<string>();
        dag.AddVertex("A");
        dag.AddVertex("B");
        dag.AddVertex("C");
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        // Act — remove and re-add B, rebuild same direction
        dag.RemoveVertex("B");
        dag.AddVertex("B");
        dag.AddEdge("A", "B");
        dag.AddEdge("B", "C");

        // Assert — original structure restored, no issues
        Assert.True(dag.HasEdge("A", "B"));
        Assert.True(dag.HasEdge("B", "C"));
    }

    #endregion
}
