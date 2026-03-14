namespace GraphLib;

/// <summary>
/// The exception that is thrown when an operation would introduce a cycle
/// in a <see cref="DirectedAcyclicGraph{T}"/>.
/// </summary>
/// <remarks>
/// This exception inherits from <see cref="InvalidOperationException"/>,
/// allowing callers to catch either the specific type or the broader base type.
/// </remarks>
/// <seealso cref="DirectedAcyclicGraph{T}"/>
/// <seealso cref="DirectedAcyclicGraph{T}.AddEdge(T, T)"/>
public class CycleDetectedException : InvalidOperationException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CycleDetectedException"/> class.
    /// </summary>
    public CycleDetectedException() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CycleDetectedException"/> class
    /// with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public CycleDetectedException(string message)
        : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="CycleDetectedException"/> class
    /// with a specified error message and a reference to the inner exception that
    /// is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public CycleDetectedException(string message, Exception innerException)
        : base(message, innerException) { }
}
