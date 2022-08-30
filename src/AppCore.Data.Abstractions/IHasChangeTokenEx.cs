namespace AppCore.Data;

/// <summary>
/// Represents an entity which implements optimistic concurrency checks with a manually controlled change token.
/// </summary>
public interface IHasChangeTokenEx
{
    /// <summary>
    /// Gets the updated change token.
    /// </summary>
    /// <remarks>
    /// If the value is <c>null</c> or equal to the value of the <see cref="ExpectedChangeToken"/>
    /// property a new change token is generated automatically.
    /// </remarks>
    string? ChangeToken { get; }

    /// <summary>
    /// Gets the expected change token.
    /// </summary>
    /// <remarks>
    /// Contains the expected change token which is validated when the entity is saved to the
    /// data store. If the change token in the data store does not match the expected one
    /// a <see cref="EntityConcurrencyException"/> will be thrown.
    /// </remarks>
    string? ExpectedChangeToken { get; }
}